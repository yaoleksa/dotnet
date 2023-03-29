using System;
using Interop.QBFC15;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace APIGetsSFData.Controllers
{
    public class AddPurchaseOrderQB
    {
        public static void BuildPurchaseOrderAddRq(IMsgSetRequest requestMsgSet,
            string poName,
            string vendorNumber,
            string vendorName,
            string customerName,
            string dateCleared,
            HashSet<purchaseOrderItem> mtlLst)
        {
            IPurchaseOrderAdd poAddRq = requestMsgSet.AppendPurchaseOrderAddRq();
            poAddRq.RefNumber.SetValue(vendorNumber);
            if (Regex.IsMatch(vendorName, "[a-z]", RegexOptions.IgnoreCase))
            {
                poAddRq.VendorRef.FullName.SetValue(vendorName);
            }
            poAddRq
                .ORInventorySiteORShipToEntity
                .ShipToEntityRef
                .FullName
                .SetValue(customerName);
            poAddRq.Memo.SetValue(poName.Split('-')[1].Trim());
            poAddRq.TxnDate.SetValue(System.Convert.ToDateTime(dateCleared));
            List<string> duplicateAvoider = new List<string>();
            Dictionary<string, double> sum = new Dictionary<string, double>();
            foreach(purchaseOrderItem item in mtlLst)
            {
                if (!sum.ContainsKey(item.Coin_Metal_Type__c))
                {
                    sum.Add(item.Coin_Metal_Type__c, 0.0);
                }
                sum[item.Coin_Metal_Type__c] += item.BG_Total_Cost__c;
            }
            foreach(purchaseOrderItem mtl in mtlLst)
            {
                if (duplicateAvoider.Contains(mtl.Coin_Metal_Type__c))
                {
                    continue;
                }
                IORPurchaseOrderLineAdd line = poAddRq
                    .ORPurchaseOrderLineAddList.Append();
                if (!mtl.Coin_Metal_Type__c.Contains("Fee"))
                {
                    line
                        .PurchaseOrderLineAdd
                        .ItemRef
                        .FullName
                        .SetValue(mtl.Coin_Metal_Type__c + " Purchase");
                } else
                {
                    line
                        .PurchaseOrderLineAdd
                        .ItemRef
                        .FullName.SetValue(mtl.Coin_Metal_Type__c);
                }
                line.PurchaseOrderLineAdd.Amount
                    .SetValue(Math.Round(sum[mtl.Coin_Metal_Type__c], 2));
                duplicateAvoider.Add(mtl.Coin_Metal_Type__c);
            }
        }
    }
}
