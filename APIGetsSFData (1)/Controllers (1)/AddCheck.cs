using System.Collections.Generic;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class AddCheck
    {
        public static void BuildAddCheckQuery(IMsgSetRequest req, 
            string customerJob, 
            string paymentType, 
            string extendedCustomer, 
            double amount)
        {
            ICheckAdd addCheck = req.AppendCheckAddRq();
            addCheck.PayeeEntityRef.FullName.SetValue(customerJob);
            addCheck.AccountRef.FullName.SetValue("1023 - BofA - Main 2975");
            if (paymentType == "Check")
            {
                addCheck.IsToBePrinted.SetValue(true);
            } else
            {
                addCheck.IsToBePrinted.SetValue(false);
                addCheck.RefNumber.SetValue("WIRE");
            }
            addCheck.Memo.SetValue(extendedCustomer);
            IExpenseLineAdd line = addCheck.ExpenseLineAddList.Append();
            line.AccountRef.FullName.SetValue("5000 Cost of Goods Sold:5100 Buybacks");
            line.Amount.SetValue(amount);
            line.Memo.SetValue(extendedCustomer);
            line.CustomerRef.FullName.SetValue(customerJob);
            line.BillableStatus.SetValue(ENBillableStatus.bsNotBillable);
        }
    }
}
