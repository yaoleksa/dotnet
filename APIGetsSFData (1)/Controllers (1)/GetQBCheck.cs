using System.Collections.Generic;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class GetQBCheck
    {
        public static void BuildCheckGet(IMsgSetRequest req) 
        {
            ICheckQuery qr = req.AppendCheckQueryRq();
            qr
                .ORTxnQuery
                .TxnFilter
                .ORDateRangeFilter
                .TxnDateRangeFilter
                .ORTxnDateRangeFilter
                .TxnDateFilter
                .FromTxnDate
                .SetValue(DateTime.Today.AddDays(-7));
        }
        public static void handleResponse(IMsgSetResponse resp, HashSet<string> exMemos) 
        {
            IResponseList resLst = resp.ResponseList;
            if(resLst == null || resLst.Count == 0)
            {
                return;
            }
            for(int i = 0; i < resLst.Count; i++)
            {
                IResponse res = resLst.GetAt(i);
                if(res.Detail == null)
                {
                    return;
                }
                ICheckRetList retLst = (ICheckRetList)res.Detail;
                if(retLst == null || retLst.Count == 0)
                {
                    return;
                }
                for(int j = 0; j < retLst.Count; j++)
                {
                    if(retLst.GetAt(j) == null)
                    {
                        continue;
                    }
                    if (retLst != null &&
                        exMemos != null &&
                        retLst.GetAt(j) != null &&
                        retLst.GetAt(j).Memo != null)
                    {
                        exMemos.Add(retLst.GetAt(j).Memo.GetValue());
                    }
                }
            }
        }
    }
}
