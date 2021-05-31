using Newtonsoft.Json;
using RestSharp;
using System;


namespace AzureLoginTest.Models
{
    public class Action
    {
        [JsonIgnore]
        public string ApiAction { get; set; }
        [JsonIgnore]
        public Method ApiMethod { get; set; }

        public string _company { get; set; }
        public string _purchReqId { get; set; }
        public string _workflowComment { get; set; }
        public string _submittedBy { get; set; }
        public DateTime _datetime { get; set; }
        public string _networkAlias { get; set; }
        public string _userID { get; set; }
        public string _extPurchReqId { get; set; }
        public string _personnelNumberId { get; set; }
        public string _financialDimensionValue { get; set; }

        public Action()
        {
            ApiMethod = Method.POST;
        }
    }
}
