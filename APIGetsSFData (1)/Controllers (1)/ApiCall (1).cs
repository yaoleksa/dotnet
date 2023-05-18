using APIGetsSFData.Controllers;
using System.Net.Http.Headers;
using System.Text.Json;
using System;

public class ApiCall
{
    public static HashSet<purchaseOrderRecord> porLst = new
        HashSet<purchaseOrderRecord>();
    public static async Task getData()
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        string baseUrl = "https://birchgoldgroup.secure.force.com/";
        string extention = "qb/services/apexrest/pojson";
        client.DefaultRequestHeaders.Add("X-API-Key", 
            "YOUR API KEY");
        System.Threading.Tasks.Task<System.IO.Stream> tsk = client
            .GetStreamAsync(baseUrl + extention);
        HashSet<purchaseOrderRecord> response = await JsonSerializer
            .DeserializeAsync<HashSet<purchaseOrderRecord>>(await tsk);
        porLst = response;
    }
}