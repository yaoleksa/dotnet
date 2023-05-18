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
            DateTime date) 
        {
            ISalesReceiptQuery salesRecepitQuery = msgRequest
                .AppendSalesReceiptQueryRq();
            salesRecepitQuery.IncludeLineItems.SetValue(true);
            salesRecepitQuery.ORTxnQuery.TxnFilter.ORDateRangeFilter
                .TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter
                .FromTxnDate.SetValue(date);
        }
        public static void parseResponse(IMsgSetResponse queryResponse,
            HashSet<string> qbSR)
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
                        if (resLst.GetAt(j).RefNumber == null)
                        {
                            qbSR.Add("null");
                        }
                        else
                        {
                            qbSR.Add(resLst.GetAt(j)
                                .RefNumber
                                .GetValue());
                        }
                    }
                }
            }
        }
    }
}
