using System.Text.Json;

namespace APIGetsSFData.Controllers
{
    public class purchaseOrderRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Deal_Type__c { get; set; }
        public string Date_Cleared__c { get; set; }
        public string IRA_Account__c { get; set; }
        public bool All_outbound_wires_inserted__c { get; set; }
        public double Total_Amount_Paid__c { get; set; }
        public double Total_Cost__c { get; set; }
        public string Vendor_Order__c { get; set; }
        public string Promo_Coin_Shipping_Loction__c { get; set; }
        public JsonDocument Outbound_Wire__r { get; set; }
        public JsonDocument Purchase_Order_Line_Items__r { get; set; }
        public JsonDocument Client__r { get; set; }
        public JsonDocument RecordType { get; set; }
        public JsonDocument Custodian_lookup__r { get; set; }
    }
}
