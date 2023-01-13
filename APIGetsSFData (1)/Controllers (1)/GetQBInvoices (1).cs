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
        public static string QBInvoices = "";
        public static void queryQBInvoices(IMsgSetRequest query, 
            List<string> sfIds) 
        {
            IInvoiceQuery InvoiceQueryRq = query.AppendInvoiceQueryRq();
            InvoiceQueryRq.IncludeLineItems.SetValue(true);
            foreach(string id in sfIds) 
            {
                InvoiceQueryRq
                    .ORInvoiceQuery
                    .RefNumberList
                    .Add(id);
            }
        }
        public static Dictionary<string, invoice> 
            handleResponse(IMsgSetResponse res, List<string> ei) 
        {
            Dictionary<string, invoice> response = new 
                Dictionary<string, invoice>();
            if(res.ResponseList != null && res.ResponseList.Count > 0) 
            {
                for(int i = 0; i < res.ResponseList.Count; i++) 
                {
                    IInvoiceRetList retLst =
                        (IInvoiceRetList)res.ResponseList.GetAt(i).Detail;
                    if(retLst == null) 
                    {
                        return null;
                    }
                    for(int j = 0; j < retLst.Count; j++) 
                    {
                        invoice record = new invoice();
                        if (retLst.GetAt(j).RefNumber == null)
                        {
                            continue;
                        }
                        QBInvoices += retLst
                            .GetAt(j).RefNumber
                            .GetValue() + '\n';
                        record.num = retLst
                            .GetAt(j)
                            .RefNumber
                            .GetValue();
                        if (!ei.Contains(record.num))
                        {
                            continue;
                        }
                        record.txnDate = retLst
                            .GetAt(j)
                            .TxnDate
                            .GetValue();
                        record.txnCustomer = retLst
                            .GetAt(j)
                            .CustomerRef
                            .FullName
                            .GetValue();
                        record.editSequence = retLst
                            .GetAt(j)
                            .EditSequence
                            .GetValue();
                        record.txnId = retLst
                            .GetAt(j)
                            .TxnID
                            .GetValue();
                        IORInvoiceLineRetList lineRet = retLst
                            .GetAt(j)
                            .ORInvoiceLineRetList;
                        if (lineRet == null)
                        {
                            continue;
                        }
                        for(int a = 0; a < lineRet.Count; a++)
                        {
                            invoiceLineItems li = new invoiceLineItems();
                            li.id = lineRet
                                .GetAt(a)
                                .InvoiceLineRet
                                .TxnLineID
                                .GetValue();
                            li.item = lineRet
                                .GetAt(a)
                                .InvoiceLineRet
                                .ItemRef
                                .FullName
                                .GetValue();
                            try
                            {
                                record.lineItems.Add(li.item, li);
                            } catch(Exception e)
                            {
                                int s = 7;
                            }
                        }
                        if (!ei.Contains(record.num))
                        {
                            break;
                        }
                        try
                        {
                            response.Add(
                                record.num, record);
                        } catch(Exception e)
                        {
                            int o = 3030;
                        }
                    }
                }
            }
            return response;
        }
    }
}
