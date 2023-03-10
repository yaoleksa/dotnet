using System.Collections.Generic;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class UpdateInvoice
    {
        public static void BuildUpdateReq(IMsgSetRequest updateReq,
            string customerName,
            Dictionary<string, double> metalPrices,
            Dictionary<string, invoiceLineItems> items,
            string sfId,
            string date,
            string qbId,
            string editSequence)
        { 
            IInvoiceMod invoiceModReq = updateReq
                .AppendInvoiceModRq();
            invoiceModReq.TxnID.SetValue(qbId);
            invoiceModReq.EditSequence.SetValue(editSequence);
            invoiceModReq
                .CustomerRef
                .FullName
                .SetValue(customerName);
            invoiceModReq
                .RefNumber
                .SetValue(sfId);
            DateTime currentDate = System.DateTime.Now;
            DateTime dateSet = new DateTime();
            if (date == null)
            {
                dateSet = currentDate;
            }
            else if (currentDate
                .Subtract(System
                .Convert
                .ToDateTime(date)).Days > 90)
            {
                dateSet = currentDate
                    .AddDays(-90);
            }
            else
            {
                dateSet = System.Convert.ToDateTime(date);
            }
            invoiceModReq.TxnDate.SetValue(dateSet);
            foreach(KeyValuePair<string, double> item in metalPrices)
            {
                IORInvoiceLineMod lineMod = invoiceModReq
                .ORInvoiceLineModList.Append();
                if (items.Count < 1 || lineMod == null)
                {
                    return;
                }
                invoiceLineItems value = new invoiceLineItems();
                items.TryGetValue(item.Key, out value);
                if(value == null)
                {
                    continue;
                }
                lineMod.InvoiceLineMod.TxnLineID.SetValue(value.id);
                double rate = Math.Round(item.Value, 2);
                lineMod.InvoiceLineMod.Amount.SetValue(rate);
                lineMod.InvoiceLineMod.ORRatePriceLevel.Rate.SetValue(rate);
            }
        }
    }
}
