using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class GetQBCustomers
    {
        public static void buildCustomerQuery(IMsgSetRequest queryCustomers) 
        {
            ICustomerQuery customerQuery = queryCustomers
                .AppendCustomerQueryRq();
            customerQuery.ORCustomerListQuery.CustomerListFilter.ORNameFilter.NameFilter.MatchCriterion.SetValue(ENMatchCriterion.mcContains);
            customerQuery.ORCustomerListQuery.CustomerListFilter.ORNameFilter.NameFilter.Name.SetValue("BUY BACK");
        }
        public static void handleResponse(IMsgSetResponse response, 
            List<string> names)
        {
            string custName = "";
            customerQB thatCustomer = new customerQB();
            if(response.ResponseList != null && 
                response.ResponseList.Count > 0)
            {
                for(int i = 0; i < response.ResponseList.Count; i++)
                {
                    ICustomerRetList retLst = (ICustomerRetList)
                        response.ResponseList.GetAt(i).Detail;
                    if(retLst == null)
                    {
                        return;
                    }
                    for(int j = 0; j < retLst.Count; j++)
                    {
                       if(retLst.GetAt(j).Name != null &&
                            retLst.GetAt(j).Name.GetValue().Length > 0) 
                        {
                            custName = retLst.GetAt(j).Name.GetValue();
                            if (!names.Contains(custName))
                            {
                                names.Add(custName);
                            }
                        }
                    }
                }
            }
        }
    }
}
