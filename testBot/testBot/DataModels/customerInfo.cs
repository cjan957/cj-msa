using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace testBot.DataModels
{
    public class customerInfo
    {
        [JsonProperty(PropertyName = "Id")]
        public string id { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string username { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string password { get; set; }

        [JsonProperty(PropertyName = "phonenum")]
        public string phoneNum { get; set; }

        [JsonProperty(PropertyName = "firstname")]
        public string firstName { get; set; }

        [JsonProperty(PropertyName = "lastname")]
        public string lastName { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string address { get; set; }

        [JsonProperty(PropertyName = "acc1Name")]
        public string acc1Name { get; set; }

        [JsonProperty(PropertyName = "acc1No")]
        public string acc1No { get; set; }

        [JsonProperty(PropertyName = "acc1Bal")]
        public double acc1Bal { get; set; }

        [JsonProperty(PropertyName = "acc2Name")]
        public string acc2Name { get; set; }

        [JsonProperty(PropertyName = "acc2No")]
        public string acc2No { get; set; }

        [JsonProperty(PropertyName = "acc2Bal")]
        public double acc2Bal { get; set; }

        [JsonProperty(PropertyName = "acc3Name")]
        public string acc3Name { get; set; }

        [JsonProperty(PropertyName = "acc3no")]
        public string acc3No { get; set; }

        [JsonProperty(PropertyName = "acc3Bal")]
        public double acc3Bal { get; set; }

        [JsonProperty(PropertyName = "acc3Ava")]
        public double acc3Ava { get; set; }
    }
}