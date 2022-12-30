using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class AddInvoiceQB
    {
        public static List<string> existingId = new List<string>();
        public static void BuildInvoiceAddRq(IMsgSetRequest createInvoiceRequest,
            string customer, Dictionary<string, double> metalItems,
            string externalId,
            string dateCleared, 
            string billingAddress, 
            double totalAmount)
        {
            if (existingId.Contains(externalId))
            {
                return;
            }
            existingId.Add(externalId);
            IInvoiceAdd InvoiceAddRq = createInvoiceRequest
                .AppendInvoiceAddRq();
            DateTime currentDate = System.DateTime.Now;
            DateTime dateSet = new DateTime();
            if(dateCleared == "null")
            {
                dateSet = currentDate;
            }
            else if(currentDate
                .Subtract(System
                .Convert
                .ToDateTime(dateCleared)).Days > 90)
            {
                dateSet = currentDate
                    .AddDays(-90);
            }
            else
            {
                dateSet = System.Convert.ToDateTime(dateCleared);
            }
            InvoiceAddRq.CustomerRef.FullName.SetValue(customer);
            InvoiceAddRq.RefNumber.SetValue(externalId);
            if (billingAddress != null)
            {
                InvoiceAddRq.BillAddress.Addr1.SetValue(billingAddress);
            }
            else
            {
                InvoiceAddRq.BillAddress.Addr1.SetValue(customer);
            }
            InvoiceAddRq.defMacro.SetValue(externalId + "upper");
            InvoiceAddRq.TxnDate.SetValue(dateSet);
            InvoiceAddRq.TemplateRef.FullName
                .SetValue("Intuit Service Invoice");
            int count = 0;
            HashSet<string> uniqueName = new HashSet<string>();
            if(metalItems == null)
            {
                IORInvoiceLineAdd line = InvoiceAddRq.ORInvoiceLineAddList.Append();
                line.InvoiceLineAdd.ItemRef.FullName.SetValue("Buyback Metals");
                line.InvoiceLineAdd.defMacro.SetValue(externalId);
                line.InvoiceLineAdd.ORRatePriceLevel.Rate.SetValue(totalAmount);
                line.InvoiceLineAdd.Desc.SetValue(billingAddress);
                return;
            }
            foreach(KeyValuePair<string, double> item in metalItems)
            {
                if(item.Value == 0 && item.Key == "Fee")
                {
                    continue;
                }
                IORInvoiceLineAdd lineAdd = InvoiceAddRq
                    .ORInvoiceLineAddList.Append();
                lineAdd
                    .InvoiceLineAdd
                    .ItemRef
                    .FullName
                    .SetValue(item.Key);
                lineAdd
                    .InvoiceLineAdd
                    .defMacro
                    .SetValue(externalId + count.ToString());
                double rate = Math.Round(item.Value, 2);
                lineAdd.InvoiceLineAdd.ORRatePriceLevel.Rate.SetValue(rate);
                count++;
            }
        }
    }
}
