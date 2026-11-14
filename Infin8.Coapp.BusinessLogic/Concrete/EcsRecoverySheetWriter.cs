using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>One member on a pay office's specimen recovery sheet (spec 13a §3).</summary>
    public sealed record EcsSpecimenRow(string? EmployeeNo, string? MemberNo, string? Name, double Demand);

    /// <summary>
    /// Writes the specimen recovery sheet a society sends to a pay office (spec 13a §3): the office's members with their
    /// demand, and a blank "Amount recovered from salary" column for the pay office to fill. The headers are the ones
    /// <see cref="EcsRecoverySheetReader"/> looks for, so the filled sheet can be uploaded as it comes back. No package:
    /// the .xlsx is a minimal one-sheet OOXML zip; ids are written as text so leading zeros survive.
    /// </summary>
    public static class EcsRecoverySheetWriter
    {
        public static readonly string[] Headers =
            { "Employee No", "Member No", "Employee Name", "Demand", "Amount recovered from salary" };

        public const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string CsvContentType = "text/csv";

        public static byte[] Xlsx(string title, IReadOnlyList<EcsSpecimenRow> rows)
        {
            var sheet = new StringBuilder();
            sheet.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>")
                 .Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">")
                 .Append("<sheetViews><sheetView workbookViewId=\"0\"><pane ySplit=\"3\" topLeftCell=\"A4\" activePane=\"bottomLeft\" state=\"frozen\"/></sheetView></sheetViews>")
                 .Append("<cols>")
                 .Append("<col min=\"1\" max=\"2\" width=\"14\" customWidth=\"1\"/>")
                 .Append("<col min=\"3\" max=\"3\" width=\"34\" customWidth=\"1\"/>")
                 .Append("<col min=\"4\" max=\"4\" width=\"14\" customWidth=\"1\"/>")
                 .Append("<col min=\"5\" max=\"5\" width=\"30\" customWidth=\"1\"/>")
                 .Append("</cols><sheetData>");

            sheet.Append("<row r=\"1\">").Append(Text("A1", title, Style.Bold)).Append("</row>");
            sheet.Append("<row r=\"3\">");
            for (int c = 0; c < Headers.Length; c++) sheet.Append(Text($"{Col(c)}3", Headers[c], Style.Bold));
            sheet.Append("</row>");

            int r = 4;
            foreach (var row in rows)
            {
                sheet.Append($"<row r=\"{r}\">")
                     .Append(Text($"A{r}", row.EmployeeNo))
                     .Append(Text($"B{r}", row.MemberNo))
                     .Append(Text($"C{r}", row.Name))
                     .Append($"<c r=\"D{r}\" s=\"{(int)Style.Amount}\"><v>{row.Demand.ToString("0.##", CultureInfo.InvariantCulture)}</v></c>")
                     .Append($"<c r=\"E{r}\" s=\"{(int)Style.Amount}\"/>")
                     .Append("</row>");
                r++;
            }
            sheet.Append("</sheetData></worksheet>");

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                Add(zip, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                    "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                    "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                    "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                    "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                    "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
                    "</Types>");
                Add(zip, "_rels/.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                    "</Relationships>");
                Add(zip, "xl/workbook.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                    "<sheets><sheet name=\"Recovery\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
                Add(zip, "xl/_rels/workbook.xml.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                    "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
                    "</Relationships>");
                // Styles: 0 default, 1 bold, 2 text (@) so typed ids keep leading zeros, 3 amount 0.00.
                Add(zip, "xl/styles.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                    "<fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Calibri\"/></font><font><b/><sz val=\"11\"/><name val=\"Calibri\"/></font></fonts>" +
                    "<fills count=\"2\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill></fills>" +
                    "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>" +
                    "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
                    "<cellXfs count=\"4\">" +
                    "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>" +
                    "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\"/>" +
                    "<xf numFmtId=\"49\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>" +
                    "<xf numFmtId=\"2\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>" +
                    "</cellXfs></styleSheet>");
                Add(zip, "xl/worksheets/sheet1.xml", sheet.ToString());
            }
            return ms.ToArray();
        }

        /// <summary>UTF-8 with a BOM (so Excel shows Tamil / accented names), header row, RFC 4180 quoting.</summary>
        public static byte[] Csv(IReadOnlyList<EcsSpecimenRow> rows)
        {
            var sb = new StringBuilder();
            sb.Append(string.Join(",", Headers.Select(Quote))).Append("\r\n");
            foreach (var r in rows)
            {
                sb.Append(Quote(r.EmployeeNo)).Append(',')
                  .Append(Quote(r.MemberNo)).Append(',')
                  .Append(Quote(r.Name)).Append(',')
                  .Append(r.Demand.ToString("0.00", CultureInfo.InvariantCulture)).Append(',')
                  .Append("\r\n");
            }
            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        }

        private enum Style { Default = 0, Bold = 1, Text = 2, Amount = 3 }

        private static string Text(string cellRef, string? value, Style style = Style.Text) =>
            string.IsNullOrEmpty(value)
                ? $"<c r=\"{cellRef}\" s=\"{(int)style}\"/>"
                : $"<c r=\"{cellRef}\" s=\"{(int)style}\" t=\"inlineStr\"><is><t xml:space=\"preserve\">{SecurityElement.Escape(value)}</t></is></c>";

        private static string Col(int i) => ((char)('A' + i)).ToString();

        private static string Quote(string? s)
        {
            s ??= "";
            return s.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        }

        private static void Add(ZipArchive zip, string name, string content)
        {
            var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
            using var w = new StreamWriter(entry.Open(), new UTF8Encoding(false));
            w.Write(content);
        }
    }
}
