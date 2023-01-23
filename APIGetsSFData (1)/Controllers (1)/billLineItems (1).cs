using System;

namespace APIGetsSFData.Controllers
{
    public class billLineItems
    {
        public string TxnLineId { get; set; }
        public string ItemRef { get; set; }
        public double Quantity { get; set; }
        public double Cost { get; set; }
        public double Amount { get; set; }
    }
}
