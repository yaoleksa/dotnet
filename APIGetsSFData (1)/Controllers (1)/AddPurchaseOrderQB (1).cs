using System;
using Interop.QBFC15;
using System.Collections.Generic;

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
            poAddRq.VendorRef.FullName.SetValue(vendorName);
            poAddRq
                .ORInventorySiteORShipToEntity
                .ShipToEntityRef
                .FullName
                .SetValue(customerName);
            poAddRq.Memo.SetValue(poName.Split('-')[1].Trim());
            poAddRq.TxnDate.SetValue(System.Convert.ToDateTime(dateCleared));
            foreach(string mtl in mtlLst)
            {
                if (mtl == "Fee")
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
            }
        }
    }
}
