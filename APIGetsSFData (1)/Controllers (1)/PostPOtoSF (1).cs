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
        public static async Task postData(List<purchaseOrderRecord> recs)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            string baseURL = "https://birchgoldgroup.secure.force.com/qb/";
            string extention = "services/apexrest/pojson";
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
            public List<purchaseOrderRecord> retrievedList { get; set; }
        }
    }
}
