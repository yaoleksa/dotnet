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
        public static async Task postData(HashSet<purchaseOrderRecord> recs)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            string baseURL = "YOUR ORG URL INSTANCE";
            string extention = "PATH TO API";
            HttpResponseMessage res = await client.PostAsync(
                baseURL + extention,
                JsonContent.Create(
                    new qbRequest
                    {
                        er = new requestContent
                        {
                            retrievedList = recs
                        }
                    })); 
            string response = await res.Content.ReadAsStringAsync();
            Console.WriteLine("<<<<================>>>>>");
            Console.WriteLine(response);
        }
        public class qbRequest 
        {
            public requestContent er { get; set; }
        }
        public class requestContent
        {
            public HashSet<purchaseOrderRecord> retrievedList { get; set; }
        }
    }
}
