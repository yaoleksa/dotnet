using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class GetSalesRecepietQB
    {
        public static string ceil;
        public static void BuildSalesRecepietRq(IMsgSetRequest msgRequest,
            List<string> sfIds) 
        {
            ISalesReceiptQuery salesRecepitQuery = msgRequest
                .AppendSalesReceiptQueryRq();
            salesRecepitQuery.IncludeLineItems.SetValue(true);
            foreach(string id in sfIds)
            {
                salesRecepitQuery
                    .ORTxnQuery
                    .RefNumberList
                    .Add(id);
            }
        }
        public static void parseResponse(IMsgSetResponse queryResponse,
            Dictionary<string, salesReceipt> exSrQb,
            List<string> esrr, List<string> qbSR)
        {
            if (queryResponse.ResponseList != null &&
                queryResponse.ResponseList.Count > 0) 
            {
                for (int i = 0; i < queryResponse.ResponseList.Count; i++) 
                {
                    ISalesReceiptRetList resLst = (ISalesReceiptRetList)
                        queryResponse.ResponseList.GetAt(i).Detail;
                    if(resLst == null)
                    {
                        //Console.WriteLine("empty list");
                        return;
                    }
                    for (int j = 0; j < resLst.Count; j++)
                    {
                        salesReceipt currentSR = new salesReceipt();
                        if (resLst.GetAt(j).RefNumber == null)
                        {
                            qbSR.Add("null");
                            currentSR.num = "null";
                        }
                        else
                        {
                            qbSR.Add(resLst.GetAt(j)
                                .RefNumber
                                .GetValue());
                            currentSR.num = resLst
                                .GetAt(j)
                                .RefNumber
                                .GetValue();
                        }
                        if (!esrr.Contains(currentSR.num)) 
                        {
                            continue;
                        }
                        currentSR.editSequence = resLst
                            .GetAt(j)
                            .EditSequence
                            .GetValue();
                        currentSR.qbId = resLst
                            .GetAt(j)
                            .TxnID
                            .GetValue();
                        currentSR.amount = resLst
                            .GetAt(j)
                            .TotalAmount
                            .GetValue();
                        IORSalesReceiptLineRetList lineList = resLst
                            .GetAt(j)
                            .ORSalesReceiptLineRetList;
                        if (lineList == null)
                        {
                            continue;
                        }
                        ceil = "";
                        for (int a = 0; a < lineList.Count; a++)
                        {
                            salesReceiptLineItems lineItem = new
                                salesReceiptLineItems();
                            if(lineList.GetAt(a)
                                .SalesReceiptLineRet
                                .ItemRef == null) 
                            {
                                continue;
                            }
                            if(lineList
                                .GetAt(a)
                                .SalesReceiptLineRet
                                .ItemRef == null)
                            {
                                continue;
                            }
                            lineItem.item = lineList
                                .GetAt(a)
                                .SalesReceiptLineRet
                                .ItemRef
                                .FullName
                                .GetValue();
                                
                            lineItem.id = lineList
                                .GetAt(a)
                                .SalesReceiptLineRet
                                .TxnLineID
                                .GetValue();
                            try
                            {
                                currentSR.lineItems
                                    .Add(lineItem.item, lineItem);
                            } catch(Exception e)
                            {
                                int h = 100;
                            }
                        }
                        if (!esrr.Contains(currentSR.num)) 
                        {
                            break;
                        }
                        try
                        {
                            exSrQb.Add(currentSR.num, currentSR);
                        } catch(Exception e) { }
                    }
                }
            }
        }
    }
}
