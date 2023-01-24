using System.Collections.Generic;
using Interop.QBFC15;
using System;

namespace APIGetsSFData.Controllers
{
    public class GetQBVendors
    {
        public static void buildRequest(IMsgSetRequest rq)
        {
            IVendorQuery vendorQuery = rq.AppendVendorQueryRq();
            vendorQuery
                .ORVendorListQuery
                .VendorListFilter
                .ORNameFilter
                .NameFilter
                .MatchCriterion
                .SetValue(ENMatchCriterion.mcContains);
            vendorQuery
                .ORVendorListQuery
                .VendorListFilter
                .ORNameFilter
                .NameFilter
                .Name
                .SetValue("Trust");
        }
        public static void handleResult(IMsgSetResponse res, List<string> names)
        {
            if(res == null)
            {
                return;
            }
            IResponseList resLst = res.ResponseList;
            if(resLst == null || resLst.Count == 0)
            {
                return;
            }
            for(int i = 0; i < resLst.Count; i++)
            {
                IResponse r = resLst.GetAt(i);
                if(r == null || r.Detail == null)
                {
                    continue;
                }
                IVendorRetList venRetLst = (IVendorRetList)r.Detail;
                if(venRetLst.Count == 0)
                {
                    continue;
                }
                for(int j = 0; j < venRetLst.Count; j++)
                {
                    if(venRetLst.GetAt(j) == null || venRetLst.GetAt(j).Name == null)
                    {
                        continue;
                    }
                    names.Add(venRetLst.GetAt(j).Name.GetValue());
                }
            }
        }
    }
}
