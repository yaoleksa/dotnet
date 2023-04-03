using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class AddNonInventoryItems
    {
        public static void BuildItemNonInventoryAddRq(IMsgSetRequest requestMsgSet,
            string itemName) 
        {
            IItemNonInventoryAdd ItemNonInventoryAddRq = requestMsgSet
                .AppendItemNonInventoryAddRq();
            ItemNonInventoryAddRq.Name.SetValue(itemName);
            ItemNonInventoryAddRq
                .ORSalesPurchase
                .SalesOrPurchase.AccountRef.FullName
                .SetValue("5675 Professional Fees");
        }
    }
}
