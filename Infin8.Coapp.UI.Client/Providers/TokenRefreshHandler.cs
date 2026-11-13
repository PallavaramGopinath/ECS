namespace Infin8.Coapp.UI.Client.Providers
{
    using System.Net;

    /// <summary>
    /// Renews the short-lived auth-token cookie when an API call returns 401, then retries the call once.
    /// The tokens are HttpOnly cookies, so the client never sees them: it only POSTs api/Auth/refresh and
    /// the browser stores the new cookies from the response.
    /// </summary>
    public class TokenRefreshHandler : DelegatingHandler
    {
        private const string RefreshPath = "api/Auth/refresh";

        // One refresh at a time: the refresh token is rotated, so parallel refreshes would invalidate each other.
        private static readonly SemaphoreSlim RefreshLock = new(1, 1);
        private static DateTime lastRefreshUtc = DateTime.MinValue;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (IsAuthEndpoint(request.RequestUri))
                return await base.SendAsync(request, cancellationToken);

            // Buffer the body so the request can be replayed after a refresh.
            if (request.Content != null)
                await request.Content.LoadIntoBufferAsync();

            var sentAtUtc = DateTime.UtcNow;
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            await RefreshAsync(request.RequestUri, sentAtUtc, cancellationToken);

            // Retry once even if this tab's refresh failed: another tab may have just renewed the cookies.
            var retry = await CloneAsync(request);
            var retryResponse = await base.SendAsync(retry, cancellationToken);
            if (retryResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                retryResponse.Dispose();
                return response;
            }
            response.Dispose();
            return retryResponse;
        }

        private async Task RefreshAsync(Uri? requestUri, DateTime sentAtUtc, CancellationToken cancellationToken)
        {
            await RefreshLock.WaitAsync(cancellationToken);
            try
            {
                // Another request already refreshed after ours was sent — just retry with the new cookie.
                if (lastRefreshUtc > sentAtUtc) return;

                // HttpClient has already made the request URI absolute by the time a handler sees it.
                if (requestUri is not { IsAbsoluteUri: true }) return;
                var refreshUri = new Uri(new Uri(requestUri.GetLeftPart(UriPartial.Authority) + "/"), RefreshPath);
                using var refresh = new HttpRequestMessage(HttpMethod.Post, refreshUri);
                using var result = await base.SendAsync(refresh, cancellationToken);
                if (result.IsSuccessStatusCode) lastRefreshUtc = DateTime.UtcNow;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine("Token refresh failed: " + ex.Message);
            }
            finally
            {
                RefreshLock.Release();
            }
        }

        private static bool IsAuthEndpoint(Uri? uri)
        {
            var path = uri?.AbsolutePath ?? "";
            return path.EndsWith("/api/Auth/login", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith("/api/Auth/logout", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith("/" + RefreshPath, StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };
            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            foreach (var option in request.Options)
                ((IDictionary<string, object?>)clone.Options)[option.Key] = option.Value;

            if (request.Content != null)
            {
                var body = new ByteArrayContent(await request.Content.ReadAsByteArrayAsync());
                foreach (var header in request.Content.Headers)
                    body.Headers.TryAddWithoutValidation(header.Key, header.Value);
                clone.Content = body;
            }
            return clone;
        }
    }
}
