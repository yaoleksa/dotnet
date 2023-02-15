using System.Text.Json;
using System;

namespace APIGetsSFData.Controllers
{
    public class outboundWire
    {
        public string Payment_Type__c { get; set; }
        public double Amount__c { get; set; }
        public JsonDocument Client__r { get; set; }
    }
}
