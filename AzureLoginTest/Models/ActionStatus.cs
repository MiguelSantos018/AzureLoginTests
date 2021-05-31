using Newtonsoft.Json;

namespace AzureLoginTest.Models
{
    public class ActionStatus
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        [JsonProperty("Requisition Status")]
        public string RequisitionStatus { get; set; }
        [JsonProperty("Worker name")]
        public string WorkerName { get; set; }
        [JsonProperty("Personal Number")]
        public string PersonalNumber { get; set; }
        [JsonProperty("Purchase Requisition Id")]
        public string PurchaseRequisitionId { get; set; }

        [JsonProperty("Total budget")]
        public string TotalBudget { get; set; }

        [JsonProperty("Purchase Id")]
        public string PurchId { get; set; }
    }
}
