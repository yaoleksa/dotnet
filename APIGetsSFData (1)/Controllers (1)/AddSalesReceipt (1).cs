using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class AddSalesReceipt
    {
        public static List<string> existingId = new List<string>();
        public static void BuildSalesRecepietAddRq(
            IMsgSetRequest salesRecepietRq,
            string customerName,
            Dictionary<string, double> metalPrices,
            string sfId,
            string date, 
            string billingAddress, 
            double totalAmount) 
        {
            if (existingId.Contains(sfId)) 
            {
                return;
            }
            existingId.Add(sfId);
            ISalesReceiptAdd addRq = salesRecepietRq.AppendSalesReceiptAddRq();
            if (sfId.Contains('-'))
            {
                addRq.RefNumber.SetValue(sfId.Split('-')[1].Trim());
            } else
            {
                addRq.RefNumber.SetValue(sfId);
            }
            addRq.defMacro.SetValue(sfId);
            DateTime dateSet = new DateTime();
            DateTime currentDate = System.DateTime.Now;
            if(date == "null")
            {
                dateSet = currentDate;
            }
            else if(currentDate
                .Subtract(System
                .Convert.ToDateTime(date))
                .Days > 90)
            {
                dateSet = currentDate.AddDays(-90);
            } else
            {
                dateSet = System.Convert.ToDateTime(date);
            }
            addRq.TxnDate.SetValue(dateSet);
            addRq.CustomerRef.FullName.SetValue(customerName);
            if(billingAddress != null)
            {
                addRq.BillAddress.Addr1.SetValue(billingAddress);
            }
            else
            {
                addRq.BillAddress.Addr1.SetValue(customerName);
            }
            HashSet<string> uniqueName = new HashSet<string>();
            int count = 0;
            if(metalPrices == null)
            {
                IORSalesReceiptLineAdd addLine = addRq
                    .ORSalesReceiptLineAddList.Append();
                addLine.SalesReceiptLineAdd.ItemRef
                    .FullName.SetValue("Buyback Metals");
                addLine.SalesReceiptLineAdd.ORRatePriceLevel
                    .Rate.SetValue(totalAmount);
                addLine.SalesReceiptLineAdd.Desc.SetValue(billingAddress);
                return;
            }
            foreach(KeyValuePair<string, double> poi in metalPrices)
            {
                if(poi.Value == 0 && poi.Key == "Fee")
                {
                    continue;
                }
                IORSalesReceiptLineAdd addLine = addRq
                    .ORSalesReceiptLineAddList.Append();
                addLine
                    .SalesReceiptLineAdd
                    .ItemRef
                    .FullName
                    .SetValue(poi.Key);
                addLine
                    .SalesReceiptLineAdd
                    .defMacro
                    .SetValue(sfId + count.ToString());
                count++;
                double rate = Math.Round(poi.Value, 2);
                addLine.SalesReceiptLineAdd.ORRatePriceLevel.Rate.SetValue(rate);
            }
        }
    }
}
