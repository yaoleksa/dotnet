using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class PostPOtoSF
    {
        public static async Task postData(Dictionary<string, salesReceipt> rqRec,
            Dictionary<string, invoice> rqInv)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            string baseURL = "YOUR URL ENTITY";
            string extention = "EXTENTION";
            HttpResponseMessage res = await client.PostAsync(
                baseURL + extention,
                JsonContent.Create(
                    new qbRequest
                    {
                        er = new requestContent
                        {
                            retrievedDictionary = rqRec,
                            retrievedInvoice = rqInv
                        }
                    })); 
            string response = await res.Content.ReadAsStringAsync();
        }
        public class qbRequest 
        {
            public requestContent er { get; set; }
        }
        public class requestContent
        {
            public Dictionary<string, salesReceipt> 
                retrievedDictionary { get; set; }
            public Dictionary<string, invoice> retrievedInvoice { get; set; }
        }
    }
}
