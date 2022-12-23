using System;

namespace APIGetsSFData.Controllers
{
    public class bill
    {
        public string RefNumber { get; set; }
        public string TxnId { get; set; }
        public string EditSequence { get; set; }
        public List<billLineItems> billItems { get; set; }
    }
}
