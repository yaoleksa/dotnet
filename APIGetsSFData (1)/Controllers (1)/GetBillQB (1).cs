using System.Collections.Generic;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class GetBillQB
    {
        public static void BuildBillQuery(IMsgSetRequest billQueryRequest, DateTime date)
        {
            IBillQuery queryBill = billQueryRequest.AppendBillQueryRq();
            queryBill.IncludeLineItems.SetValue(true);
            queryBill
                .ORBillQuery
                .BillFilter
                .ORDateRangeFilter
                .TxnDateRangeFilter
                .ORTxnDateRangeFilter
                .TxnDateFilter
                .FromTxnDate
                .SetValue(date);
        }
        public static void handleResponse(IMsgSetResponse bills, Dictionary<string, bill> existingBills) 
        {
            if(bills != null)
            {
                IResponseList rLst = bills.ResponseList;
                if(rLst != null)
                {
                    for(int i = 0; i < rLst.Count; i++)
                    {
                        IResponse r = rLst.GetAt(i);
                        if(r.StatusCode >= 0 && r.Detail != null)
                        {
                            IBillRetList billRetLst = (IBillRetList)r.Detail;
                            if(billRetLst != null)
                            {
                                for(int j = 0; j < billRetLst.Count; j++)
                                {
                                    if (billRetLst.GetAt(j) != null &&
                                        billRetLst.GetAt(j).RefNumber != null)
                                    {
                                        bill record = new bill();
                                        record.TxnId = billRetLst.GetAt(j).TxnID.GetValue();
                                        record.RefNumber = billRetLst.GetAt(j).RefNumber.GetValue();
                                        record.EditSequence = billRetLst.GetAt(j).EditSequence.GetValue();
                                        record.billItems = new List<billLineItems>();
                                        if(billRetLst.GetAt(j).ORItemLineRetList != null)
                                        {
                                            for(int k = 0; k < billRetLst.GetAt(j).ORItemLineRetList.Count; k++)
                                            {
                                                billLineItems bli = new billLineItems();
                                                IORItemLineRet line = billRetLst.GetAt(j).ORItemLineRetList.GetAt(k);
                                                bli.TxnLineId = line.ItemLineRet.TxnLineID.GetValue();
                                                bli.ItemRef = line.ItemLineRet.ItemRef.FullName.GetValue();
                                                bli.Quantity = line.ItemLineRet.Quantity.GetValue();
                                                bli.Cost = line.ItemLineRet.Cost.GetValue();
                                                bli.Amount = line.ItemLineRet.Amount.GetValue();
                                                record.billItems.Add(bli);
                                            }
                                        }
                                        if (!existingBills.ContainsKey(billRetLst.GetAt(j).RefNumber.GetValue()))
                                        {
                                            existingBills
                                                .Add(billRetLst
                                                .GetAt(j)
                                                .RefNumber
                                                .GetValue(), record);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
