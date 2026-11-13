using Infin8.Coapp.Dto;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>Income tax reports (group 14): 109 loans, 110 share capital, 111 members, 112 fixed deposits.</summary>
    public interface IReportsITHandler
    {
        /// <summary>Rows, DataSet name and report parameters for one request; Message is set for an unknown report.</summary>
        Task<ITReportData> BuildAsync(rptReportITObject request);
    }

    public class ITReportData
    {
        public string DataSetName { get; set; } = "";
        public System.Collections.IEnumerable? Rows { get; set; }
        /// <summary>Report parameters other than paramSocietyName (added by the controller).</summary>
        public Dictionary<string, string> Parameters { get; } = new();
        public string? Message { get; set; }
    }
}
