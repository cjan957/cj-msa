using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.MobileServices;
using testBot.DataModels;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace testBot
{
    public class AzureManager
    {
        private static AzureManager instance;
        private MobileServiceClient client;
        public IMobileServiceTable<customerInfo> timelineTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://cj-msa2.azurewebsites.net");
            this.timelineTable = this.client.GetTable<customerInfo>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task AddTimeline(customerInfo timeline)
        {
            await this.timelineTable.InsertAsync(timeline);
        }

        public async Task<List<customerInfo>> GetTimelines(string passedID)
        {
            //return await this.timelineTable.ToListAsync();
            return await this.timelineTable.Where(customerInfo => customerInfo.id == passedID).ToListAsync();
        }

        public async Task UpdateTimeline(string id, double amount, string accountName)
        {
            var account = timelineTable
                .Where(customerInfo => customerInfo.id == id).Take(1).ToListAsync();
            List<customerInfo> list = await account;

            if (accountName == "cheque")
            {
                list[0].acc1Bal = amount;
            }
            else if (accountName == "saving")
            {
                list[0].acc2Bal = amount;
            }
            else if (accountName == "credit")
            {
                list[0].acc3Bal = amount;
            }

            await timelineTable.UpdateAsync(list[0]);
        }
    }
}