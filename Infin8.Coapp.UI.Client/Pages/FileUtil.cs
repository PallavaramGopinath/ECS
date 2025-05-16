using Microsoft.JSInterop;
namespace Infin8.Coapp.UI.Client.Pages
{
    public static class FileUtil
    {
        public static ValueTask<object> SaveAs(this IJSRuntime js, string filename, byte[] data)
        {
            //string base64Data = Convert.ToBase64String(data, 0, data.Length);
            return js.InvokeAsync<object>("saveAsFile", filename, data);
        }
    }
}
