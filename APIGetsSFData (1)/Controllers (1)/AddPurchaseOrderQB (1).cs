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
            Dictionary<string, double> poItems,
            List<string> mtlLst)
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
            foreach(string mtl in mtlLst)
            {
                if (mtl == "Fee" || duplicateAvoider.Contains(mtl))
                {
                    continue;
                }
                IORPurchaseOrderLineAdd line = poAddRq
                    .ORPurchaseOrderLineAddList.Append();
                line
                    .PurchaseOrderLineAdd
                    .ItemRef
                    .FullName
                    .SetValue(mtl + " Purchase");
                line.PurchaseOrderLineAdd.Amount
                    .SetValue(Math.Round(poItems[mtl], 2));
                duplicateAvoider.Add(mtl);
            }
        }
    }
}
