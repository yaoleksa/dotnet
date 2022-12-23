using Interop.QBFC15;
using System.Collections.Generic;

namespace APIGetsSFData.Controllers
{
    public class PurchaseOrderQBQuery
    {
        public static void BuildPurchaseOrderQuery(
            IMsgSetRequest queryRequest, DateTime date) 
        { 
            IPurchaseOrderQuery q = queryRequest.AppendPurchaseOrderQueryRq();
            q.IncludeLineItems.SetValue(true);
            q.ORTxnQuery
                .TxnFilter
                .ORDateRangeFilter
                .TxnDateRangeFilter
                .ORTxnDateRangeFilter
                .TxnDateFilter
                .FromTxnDate
                .SetValue(date);
        }
        public static void handleRespons(IMsgSetResponse response,
            Dictionary<string, List<string>> existingNumbers)
        {
            if(response != null && 
                response.ResponseList != null && 
                response.ResponseList.Count > 0) 
            { 
                for(int i = 0; i < response.ResponseList.Count; i++)
                {
                    IPurchaseOrderRetList retLst = (IPurchaseOrderRetList)
                        response.ResponseList.GetAt(i).Detail;
                    if(retLst == null)
                    {
                        return;
                    }
                    for(int j = 0; j < retLst.Count; j++)
                    {
                        if(retLst.GetAt(j) == null)
                        {
                            continue;
                        }
                        if(retLst.GetAt(j).RefNumber != null &&
                            retLst.GetAt(j).RefNumber.GetValue() != null)
                        {
                            if (!existingNumbers
                                .ContainsKey(retLst.GetAt(j)
                                .RefNumber
                                .GetValue()))
                            {
                                List<string> lineTxnIds = new List<string>();
                                if(retLst.GetAt(j).ORPurchaseOrderLineRetList != null)
                                {
                                    for(int s = 0; s < retLst.GetAt(j).ORPurchaseOrderLineRetList.Count; s++)
                                    {
                                        IORPurchaseOrderLineRet retLine = retLst.GetAt(j)
                                            .ORPurchaseOrderLineRetList.GetAt(s);
                                        if (!lineTxnIds.Contains(retLine.PurchaseOrderLineRet.TxnLineID.GetValue()))
                                        {
                                            lineTxnIds.Add(retLine.PurchaseOrderLineRet.TxnLineID.GetValue() + 
                                                "$%&" + retLst.GetAt(j).TxnID.GetValue());
                                        }
                                    }
                                }
                                existingNumbers
                                    .Add(retLst.GetAt(j).RefNumber.GetValue(),
                                    lineTxnIds);
                            }
                        }
                    }
                }
            }
        }
    }
}
