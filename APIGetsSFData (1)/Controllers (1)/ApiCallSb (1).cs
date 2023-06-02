using APIGetsSFData.Controllers;
using System.Net.Http.Headers;
using System.Text.Json;
using System;

public class ApiCallSb
{
    public static HashSet<purchaseOrderRecord> porLst = new
        HashSet<purchaseOrderRecord>();
    public static async Task getData()
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        string baseUrl =
            "https://birchgoldgroup--olx.sandbox.my.salesforce-sites.com/";
        string extention = "qb/services/apexrest/pojson";
        client.DefaultRequestHeaders.Add("X-API-Key", 
            "1e71595a-e8a3-46a5-b8ce-777968dc56b9");
        System.Threading.Tasks.Task<System.IO.Stream> tsk = client
            .GetStreamAsync(baseUrl + extention);
        HashSet<purchaseOrderRecord> response = await JsonSerializer
            .DeserializeAsync<HashSet<purchaseOrderRecord>>(await tsk);
        porLst = response;
    }
}