using Interop.QBFC15;

namespace APIGetsSFData.Controllers
{
    public class CustomerUpdate
    {
        public static void buildCustomerMod(IMsgSetRequest custModRq, 
            customerQB customer,
            string newName) 
        {
            ICustomerMod rq = custModRq.AppendCustomerModRq();
            rq.Name.SetValue(newName);
            rq.BillAddress.Addr1.SetValue(newName);
            rq.EditSequence.SetValue(customer.editSequence);
            rq.ListID.SetValue(customer.listId);
            IShipToAddress addr = rq.ShipToAddressList.Append();
            addr.Name.SetValue(newName);
        }
    }
}
