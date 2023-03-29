using System;
using Interop.QBFC15;
using System.Collections.Generic;

namespace APIGetsSFData.Controllers
{
    public class AddBill
    {
        public static void BuildBillAddRq(IMsgSetRequest addBill,
            string vendorFullName,
            string dateCleared,
            string poName,
            string vendorNumber,
            Dictionary<string, List<string>> poTxnIds, 
            double totalAmount, 
            string customerJob, 
            string customerName, 
            string sfPo) 
        {
            IBillAdd billAdd = addBill.AppendBillAddRq();
            billAdd.VendorRef.FullName.SetValue(vendorFullName);
            billAdd.TxnDate.SetValue(System.Convert.ToDateTime(dateCleared));
            billAdd.Memo.SetValue(sfPo);
            billAdd.RefNumber.SetValue(vendorNumber);
            if(poTxnIds == null)
            {
                IExpenseLineAdd line = billAdd.ExpenseLineAddList.Append();
                line.AccountRef.FullName
                    .SetValue("5000 Cost of Goods Sold:5100 Buybacks");
                line.Amount.SetValue(totalAmount);
                line.CustomerRef.FullName.SetValue(customerJob);
                line.Memo.SetValue(customerName);
                line.BillableStatus.SetValue(ENBillableStatus.bsNotBillable);
                return;
            }
            foreach(string item in poTxnIds[vendorNumber])
            {
                IORItemLineAdd line = billAdd.ORItemLineAddList.Append();
                line.ItemLineAdd.LinkToTxn.TxnID.SetValue(item.Split("$%&", 
                    StringSplitOptions.RemoveEmptyEntries)[1]);
                line.ItemLineAdd.LinkToTxn.TxnLineID.SetValue(item.Split("$%&",
                    StringSplitOptions.RemoveEmptyEntries)[0]);
                line.ItemLineAdd.BillableStatus.SetValue(ENBillableStatus
                    .bsNotBillable);
            }
        }
    }
}
