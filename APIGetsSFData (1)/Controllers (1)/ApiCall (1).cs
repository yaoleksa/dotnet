using APIGetsSFData.Controllers;
using System.Net.Http.Headers;
using System.Text.Json;
using System;

public class ApiCall
{
    public static List<purchaseOrderRecord> porLst = new
        List<purchaseOrderRecord>();
    public static async Task getData()
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        string baseUrl = "YOUR ORG URL INSTANCE";
        string extention = "PATH TO API";
        client.DefaultRequestHeaders.Add("X-API-Key", 
            "YOUR API KEY");
        System.Threading.Tasks.Task<System.IO.Stream> tsk = client
            .GetStreamAsync(baseUrl + extention);
        List<purchaseOrderRecord> response = await JsonSerializer
            .DeserializeAsync<List<purchaseOrderRecord>>(await tsk);
        porLst = response;
    }
}