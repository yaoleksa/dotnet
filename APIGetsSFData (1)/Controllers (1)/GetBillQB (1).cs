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
        public static void handleResponse(IMsgSetResponse bills, 
            List<string> existingBills) 
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
                                        if (!existingBills
                                            .Contains(billRetLst
                                            .GetAt(j)
                                            .RefNumber.GetValue()))
                                        {
                                            existingBills
                                                .Add(billRetLst
                                                .GetAt(j)
                                                .RefNumber
                                                .GetValue());
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
