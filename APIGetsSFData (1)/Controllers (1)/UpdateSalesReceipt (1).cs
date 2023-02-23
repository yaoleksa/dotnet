using System.Collections.Generic;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class UpdateSalesReceipt
    {
        public static void SalesReceiptMod(IMsgSetRequest modRq,
            string customerName,
            Dictionary<string, double> metalPrices,
            salesReceipt value,
            string sfId,
            string date,
            string qbId,
            string editSequence
            )
        {
            ISalesReceiptMod srmodRq = modRq.AppendSalesReceiptModRq();
            srmodRq.TxnID.SetValue(qbId);
            srmodRq.EditSequence.SetValue(editSequence);
            srmodRq.CustomerRef.FullName.SetValue(customerName);
            srmodRq.RefNumber.SetValue(sfId.Split('-')[1]);
            DateTime dateSet = new DateTime();
            DateTime currentDate = System.DateTime.Now;
            if (date == "null")
            {
                dateSet = currentDate;
            }
            else if (currentDate
                .Subtract(System
                .Convert.ToDateTime(date))
                .Days > 90)
            {
                dateSet = currentDate.AddDays(-90);
            }
            else
            {
                dateSet = System.Convert.ToDateTime(date);
            }
            srmodRq.TxnDate.SetValue(dateSet);
            foreach(KeyValuePair<string, double> kvp in metalPrices)
            {
                IORSalesReceiptLineMod lineMod = srmodRq
                    .ORSalesReceiptLineModList.Append();
                salesReceiptLineItems val = new salesReceiptLineItems();
                if (value.lineItems.ContainsKey(kvp.Key))
                {
                    value.lineItems.TryGetValue(kvp.Key, out val);
                }
                lineMod.SalesReceiptLineMod.TxnLineID.SetValue(val.id);
                double rate = Math.Round(kvp.Value, 2);
                lineMod.SalesReceiptLineMod.Amount.SetValue(rate);
                lineMod.SalesReceiptLineMod.ORRatePriceLevel.Rate.SetValue(rate);
            }
        }
    }
}
