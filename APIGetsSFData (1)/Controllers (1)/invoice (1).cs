using System.Collections.Generic;

namespace APIGetsSFData.Controllers
{
    public class invoice
    {
        public string num { get; set; }
        public string txnCustomer { get; set; }
        public string txnId { get; set; }
        public DateTime txnDate { get; set; }
        public string editSequence { get; set; }

        public Dictionary<string, invoiceLineItems> lineItems = new
            Dictionary<string, invoiceLineItems>();
    }
}
