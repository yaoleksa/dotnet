using System.Collections.Generic;

namespace APIGetsSFData.Controllers
{
    public class salesReceipt
    {
        public string qbId { get; set; }
        public string editSequence { get; set; }
        public double amount { get; set; }
        public string num { get; set; }
        public Dictionary<string, salesReceiptLineItems> lineItems = 
            new Dictionary<string, salesReceiptLineItems>();
    }
}
