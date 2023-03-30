using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class AddVendor
    {
        public static void BuildAddVendorRequest(IMsgSetRequest requestMsgSet,
            string name) 
        {
            IVendorAdd VendorAddRq = requestMsgSet.AppendVendorAddRq();
            VendorAddRq.Name.SetValue(name);
            VendorAddRq.IsActive.SetValue(true);
        }
    }
}
