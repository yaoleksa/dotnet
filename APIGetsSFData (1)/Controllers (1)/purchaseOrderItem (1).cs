namespace APIGetsSFData.Controllers
{
    public class purchaseOrderItem
    {
        public string Coin_Metal_Type__c { get; set; }
        public double Total_Price_Formula__c { get; set; }
        public double BG_Total_Cost__c { get; set; }
        public string Vendor_Order_Number__c { get; set; }
        public string Vendor__c { get; set; }
        public bool Promo_coin__c { get; set; }
        public customer Vendor__r { get; set; }
    }
}
