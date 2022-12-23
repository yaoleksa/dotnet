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
        string baseUrl = "YOUR URL ENTITY";
        string extention = "URL EXTENTION";
        System.Threading.Tasks.Task<System.IO.Stream> tsk = client
            .GetStreamAsync(baseUrl + extention);
        List<purchaseOrderRecord> response = await JsonSerializer
            .DeserializeAsync<List<purchaseOrderRecord>>(await tsk);
        porLst = response;
    }
}
