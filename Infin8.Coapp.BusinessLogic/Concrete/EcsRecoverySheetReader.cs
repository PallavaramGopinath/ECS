using System.Globalization;
using System.Text;
using ExcelDataReader;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>One data row of a pay office's recovery sheet (spec 13a §3). <see cref="RowNo"/> is the Excel row number.</summary>
    public sealed record EcsSheetRow(int RowNo, string? EmployeeNo, string? MemberNo, string? SheetName, double Amount);

    /// <summary>
    /// Reads the recovery sheet a pay office sends (spec 13a §3): <c>.xlsx</c>, <c>.xls</c> or <c>.csv</c>, first worksheet,
    /// a header row whose columns are found by their text (Employee No, Member No, Employee Name, Amount …), blank rows
    /// ignored. A row with a blank or zero amount is skipped — the member recovers nothing — so the specimen sheet
    /// (<see cref="EcsRecoverySheetWriter"/>) can come back with only the deducted members filled in.
    /// No database: matching the rows to members is the handler's job.
    /// </summary>
    public static class EcsRecoverySheetReader
    {
        static EcsRecoverySheetReader()
        {
            // .xls (BIFF) stores text in legacy code pages that .NET does not load by default.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>Header rows are looked for in the first rows only (a title or office name may sit above it).</summary>
        private const int HeaderSearchRows = 10;

        public static (List<EcsSheetRow> rows, List<string> errors) Read(Stream stream, string fileName)
        {
            var rows = new List<EcsSheetRow>();
            var errors = new List<string>();
            var ext = Path.GetExtension(fileName ?? "").ToLowerInvariant();
            if (ext is not (".xlsx" or ".xls" or ".csv"))
            {
                errors.Add("Upload an Excel or CSV file (.xlsx, .xls or .csv).");
                return (rows, errors);
            }

            List<object?[]> raw;
            try
            {
                using var reader = ext switch
                {
                    ".xls" => ExcelReaderFactory.CreateBinaryReader(stream),
                    ".csv" => ExcelReaderFactory.CreateCsvReader(stream),
                    _ => ExcelReaderFactory.CreateOpenXmlReader(stream)
                };
                raw = new List<object?[]>();
                while (reader.Read())
                {
                    var values = new object?[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++) values[i] = reader.GetValue(i);
                    raw.Add(values);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"The file could not be read as {(ext == ".csv" ? "a CSV file" : "an Excel workbook")} ({ex.GetBaseException().Message}).");
                return (rows, errors);
            }

            // --- header ---
            int headerRow = -1, empCol = -1, memCol = -1, nameCol = -1, amtCol = -1;
            for (int r = 0; r < Math.Min(HeaderSearchRows, raw.Count) && headerRow < 0; r++)
            {
                int e = -1, m = -1, n = -1, a = -1;
                for (int c = 0; c < raw[r].Length; c++)
                {
                    var h = Key(Text(raw[r][c]));
                    if (h.Length == 0) continue;
                    if (e < 0 && (h.StartsWith("EMPLOYEENO") || h.StartsWith("EMPNO") || h.StartsWith("PERNO") || h == "EMPLOYEECODE")) e = c;
                    else if (m < 0 && (h.StartsWith("MEMBERNO") || h.StartsWith("MEMNO"))) m = c;
                    else if (n < 0 && (h.Contains("NAME"))) n = c;
                    else if (a < 0 && (h.StartsWith("AMOUNT") || h.Contains("RECOVER"))) a = c;
                }
                if (a >= 0 && (e >= 0 || m >= 0)) (headerRow, empCol, memCol, nameCol, amtCol) = (r, e, m, n, a);
            }
            if (headerRow < 0)
            {
                errors.Add("No header row found. The sheet needs columns headed \"Employee No\" and/or \"Member No\", and \"Amount recovered from salary\".");
                return (rows, errors);
            }

            // --- data ---
            for (int r = headerRow + 1; r < raw.Count; r++)
            {
                var v = raw[r];
                string? emp = Cell(v, empCol), mem = Cell(v, memCol), name = Cell(v, nameCol);
                object? amtCell = amtCol < v.Length ? v[amtCol] : null;
                string amtText = Text(amtCell);
                if (string.IsNullOrEmpty(emp) && string.IsNullOrEmpty(mem) && string.IsNullOrEmpty(name) && amtText.Length == 0)
                    continue; // blank row

                int rowNo = r + 1;
                if (string.IsNullOrEmpty(emp) && string.IsNullOrEmpty(mem))
                {
                    // A total line at the foot of the sheet usually has only a label and an amount.
                    if (!string.IsNullOrEmpty(name) && name.Contains("total", StringComparison.OrdinalIgnoreCase)) continue;
                    errors.Add($"Row {rowNo}: neither an employee no nor a member no.");
                    continue;
                }
                if (amtText.Length == 0) continue;   // not deducted: the member recovers nothing
                if (!TryAmount(amtCell, out var amount))
                {
                    errors.Add($"Row {rowNo}: '{amtText}' is not an amount.");
                    continue;
                }
                amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
                if (amount == 0) continue;
                if (amount < 0)
                {
                    errors.Add($"Row {rowNo}: the amount cannot be negative.");
                    continue;
                }
                rows.Add(new EcsSheetRow(rowNo, emp, mem, name, amount));
            }
            if (rows.Count == 0 && errors.Count == 0) errors.Add("The sheet has no recovery amounts.");
            return (rows, errors);
        }

        private static string? Cell(object?[] v, int col)
        {
            if (col < 0 || col >= v.Length) return null;
            var s = Text(v[col]);
            return s.Length == 0 ? null : s;
        }

        /// <summary>A cell as text; whole numbers (Excel stores ids as doubles) without a decimal part.</summary>
        private static string Text(object? o) => o switch
        {
            null => "",
            double d when d == Math.Floor(d) && Math.Abs(d) < 1e15 => ((long)d).ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            _ => (Convert.ToString(o, CultureInfo.InvariantCulture) ?? "").Trim()
        };

        private static bool TryAmount(object? o, out double amount)
        {
            switch (o)
            {
                case double d: amount = d; return !double.IsNaN(d);
                case int i: amount = i; return true;
                case decimal m: amount = (double)m; return true;
            }
            var s = Text(o).Replace(",", "").Replace("₹", "").Replace("Rs.", "", StringComparison.OrdinalIgnoreCase).Trim();
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out amount);
        }

        /// <summary>Header text reduced to letters and digits, upper case ("Employee No." → "EMPLOYEENO").</summary>
        private static string Key(string s) => new string(s.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}
