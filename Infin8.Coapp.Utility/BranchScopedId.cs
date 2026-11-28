using System.Globalization;

namespace Infin8.Coapp.Utility
{
    /// <summary>Numeric transaction IDs: branch code and seven-digit running number.</summary>
    public static class BranchScopedId
    {
        public const int RunningNumberWidth = 7;
        public const decimal MaxRunningNumber = 9_999_999m;

        public static decimal Floor(string brCode)
        {
            // Existing 11005 test data retains its five-digit operational code.
            if (brCode is null || (brCode.Length != 4 && brCode.Length != 5) ||
                !brCode.All(c => c >= '0' && c <= '9'))
                throw new ArgumentException("ECS BrCode must be four digits (or the existing five-digit legacy code).", nameof(brCode));

            return decimal.Parse(brCode, CultureInfo.InvariantCulture) * 10_000_000m;
        }

        public static decimal Next(string brCode, decimal? currentMax)
        {
            decimal floor = Floor(brCode);
            decimal max = currentMax ?? floor;
            if (max < floor) max = floor;
            if (max >= floor + MaxRunningNumber)
                throw new InvalidOperationException($"The seven-digit ID sequence for society {brCode} is exhausted.");
            if (max != decimal.Truncate(max))
                throw new InvalidOperationException("The current ID is not a whole number.");
            return max + 1;
        }
    }
}
