using Infin8.Coapp.Dto;
using Infin8.Coapp.ReportServices.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Microsoft.Reporting.NETCore;
namespace Infin8.Coapp.ReportServices.Concrete
{
    public class Instant_Reports : IInstant_Reports
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        public Instant_Reports(HttpClient httpClient, IJSRuntime jsRuntime)  ///  IJSRuntime jsRuntime
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }
        public async Task Print_Member_Receipt(rptReceiptObject rptObject)
        {
            byte[] fileBytes;
            //LocalReport localReport = new LocalReport();
            var response = await  _httpClient.PostAsJsonAsync<rptReceiptObject>($"https://localhost:7073/api/Print/print-receipt", rptObject);
            if (response.IsSuccessStatusCode)
            {
                fileBytes = await response.Content.ReadAsByteArrayAsync();
                // 1. Convert byte array to a Base64 string
                var base64String = Convert.ToBase64String(fileBytes);

                /// 2. Render in iframe commented
                // Set the data URL to a property bound to the iframe's src
                // pdfDataUrl = $"data:application/pdf;base64,{base64String}";

                // If fileBytes is null or empty, the API returned nothing.
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    Console.WriteLine("Error: API returned a success status code but with no content.");
                    return; // Stop here
                }
                // 3. Call the new JavaScript function to open the PDF in a new tab
                await _jsRuntime.InvokeVoidAsync("openPdfInNewTab", base64String);

                // 4. Down load pdf commented
                // await JSRuntime.SaveAs("Receipt.pdf", fileBytes!);
            }
            else
            {
                // Log or show error
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
    }
}
