using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class GetQBInvoices
    {
        public static void queryQBInvoices(IMsgSetRequest query, 
            DateTime date) 
        {
            IInvoiceQuery InvoiceQueryRq = query.AppendInvoiceQueryRq();
            InvoiceQueryRq.IncludeLineItems.SetValue(true);
            InvoiceQueryRq.ORInvoiceQuery.InvoiceFilter.ORDateRangeFilter
                .TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter
                .FromTxnDate.SetValue(date);
        }
        public static void 
            handleResponse(IMsgSetResponse res, HashSet<string> exRec) 
        {
            if(res.ResponseList != null && res.ResponseList.Count > 0) 
            {
                for(int i = 0; i < res.ResponseList.Count; i++) 
                {
                    IInvoiceRetList retLst =
                        (IInvoiceRetList)res.ResponseList.GetAt(i).Detail;
                    if(retLst == null) 
                    {
                        return;
                    }
                    for(int j = 0; j < retLst.Count; j++) 
                    {
                        if (retLst.GetAt(j).RefNumber == null)
                        {
                            continue;
                        }
                        exRec.Add(retLst.GetAt(j).RefNumber.GetValue());
                    }
                }
            }
        }
    }
}
