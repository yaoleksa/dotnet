using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class AddClientQB
    {
        public static void BuildCustomerAddRq(IMsgSetRequest requestmsgSet,
            string clientName) 
        {
            ICustomerAdd CustomerAddRq = requestmsgSet.AppendCustomerAddRq();
            CustomerAddRq.Name.SetValue(clientName);
            CustomerAddRq.IsActive.SetValue(true);
        }
    }
}
