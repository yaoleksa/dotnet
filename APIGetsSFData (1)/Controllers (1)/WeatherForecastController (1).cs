using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.Json;
using Interop.QBFC15;
using System.Text;
using System.IO;
using System;

namespace APIGetsSFData.Controllers
{
    [ApiController]
    [Route("SFData")]
    public class WeatherForecastController : ControllerBase
    {
        [HttpGet(Name = "SFData")]
        public async Task Get()
        {
            System.GC.Collect();
            // Call SF API *~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*
            string mode = "prod";
            int timeLimit = -11;
            if (mode == "prod")
            {
                await ApiCall.getData();
            } else
            {
                await ApiCallSb.getData();
            }
            HashSet<purchaseOrderRecord> displayResp = mode == "prod" ? 
                ApiCall.porLst : ApiCallSb.porLst;
            // Check if there are new records
            if(displayResp.Count == 0)
            {
                Response.WriteAsync("No new records");
                return;
            }
            // end check
            QBSessionManager sessionManager = new QBSessionManager();
            IMsgSetRequest requestMetalAdd = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            requestMetalAdd.Attributes.OnError = ENRqOnError.roeContinue;
            IMsgSetRequest requestCustomerAdd = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            requestCustomerAdd.Attributes.OnError = ENRqOnError.roeContinue;
            IMsgSetRequest requestVendorAdd = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            requestVendorAdd.Attributes.OnError = ENRqOnError.roeContinue;
            IMsgSetRequest srReq = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            srReq.Attributes.OnError = ENRqOnError.roeContinue;
            IMsgSetRequest invRq = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            invRq.Attributes.OnError = ENRqOnError.roeContinue;
            // Purchase Order QB Request
            IMsgSetRequest addPurchaseOrder = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            addPurchaseOrder.Attributes.OnError = ENRqOnError.roeContinue;
            // End Purchase Order QB Request
            ///////////////////////////////////////////////Begin session
            sessionManager.OpenConnection("", "Production json");
            // Specify file to run API even file is closed
            string path = "C:\\Quickbooks FIles\\" +
                "1white horse capital, llc permanent 2012.qbw";
            sessionManager.BeginSession("", ENOpenMode.omDontCare);
            // Query Sales Receipt from QB DB ##############################
            IMsgSetRequest querySRQB = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            querySRQB.Attributes.OnError = ENRqOnError.roeContinue;
            GetSalesRecepietQB.BuildSalesRecepietRq(querySRQB, 
                DateTime.Now.AddDays(timeLimit));
            IMsgSetResponse querySRrecords = sessionManager
                .DoRequests(querySRQB);
            HashSet<string> existingSRArr = new HashSet<string>();
                GetSalesRecepietQB
                .parseResponse(querySRrecords, 
                existingSRArr);
            // Create Query Invoices QB request !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            IMsgSetRequest queryInvc = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            queryInvc.Attributes.OnError = ENRqOnError.roeContinue;
            // Request was created $$$$$$$$$$$$$$$$$$$$$$$$$
            // Query Purchase Order from QB
            IMsgSetRequest queryPurchaseOrder = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            queryPurchaseOrder.Attributes.OnError = ENRqOnError.roeContinue;
            GetPurchaseOrderQB.BuildPurchaseOrderQuery(queryPurchaseOrder, 
                DateTime.Today.AddDays(timeLimit));
            IMsgSetResponse doneQuery = sessionManager
                .DoRequests(queryPurchaseOrder);
            Dictionary<string, List<string>> eqbpo = 
                new Dictionary<string, List<string>>();
            GetPurchaseOrderQB.handleRespons(doneQuery, eqbpo);
            // Purchase orders were retrieved ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // Query Bill from QB
            IMsgSetRequest queryBill = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            queryBill.Attributes.OnError = ENRqOnError.roeContinue;
            GetBillQB.BuildBillQuery(queryBill, 
                DateTime.Today.AddDays(timeLimit));
            IMsgSetResponse recievedBills = sessionManager.DoRequests(queryBill);
            HashSet<string> exBills = new HashSet<string>();
            GetBillQB.handleResponse(recievedBills, exBills);
            // Update Bill
            IMsgSetRequest updateBillRq = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            updateBillRq.Attributes.OnError = ENRqOnError.roeContinue;
            // Query QB customers ****
            IMsgSetRequest getCustomers = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            getCustomers.Attributes.OnError = ENRqOnError.roeContinue;
            GetQBCustomers.buildCustomerQuery(getCustomers);
            HashSet<string> existingCustomers = new HashSet<string>();
            IMsgSetResponse getCustomersResponse = sessionManager
                .DoRequests(getCustomers);
            GetQBCustomers.handleResponse(getCustomersResponse, existingCustomers);
            // Customers were retrieved ------@@@@@@@@@@@
            // Query Invoices from QB ``````````````````````````````````(:;|
            GetQBInvoices.queryQBInvoices(queryInvc, 
                DateTime.Now.AddDays(timeLimit));
            IMsgSetResponse invResp = sessionManager
                .DoRequests(queryInvc);
            HashSet<string> exRec = 
                new HashSet<string>();
            GetQBInvoices
                .handleResponse(invResp, exRec);
            ////// Create bill insert query
            IMsgSetRequest queryBillCreate = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            queryBillCreate.Attributes.OnError = ENRqOnError.roeContinue;
            /// Query was created --*-*-*-*-*-*-*-*-*-*-*-*-*
            /// Get vendors from QB
            IMsgSetRequest queryVendors = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            GetQBVendors.buildRequest(queryVendors);
            IMsgSetResponse responseVendors = sessionManager.DoRequests(queryVendors);
            HashSet<string> qbVenodors = new HashSet<string>();
            GetQBVendors.handleResult(responseVendors, qbVenodors);
            /// Vendors were retrieved |||||||||||||||||||||||||||||||||||||||
            /// Update customer list
            List<string> cNames = new List<string>();
            /// List was created =***=*=*=**=**=**=*=*=***=**=***=**=*=*=***===
            /// Add check or not *****
            IMsgSetRequest addCheckRq = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            addCheckRq.Attributes.OnError = ENRqOnError.roeContinue;
            /// Request was created (.)(.)(.)(.)(.)(.)
            /// QueryCheck
            IMsgSetRequest queryCheck = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            GetQBCheck.BuildCheckGet(queryCheck);
            IMsgSetResponse exCheck = sessionManager.DoRequests(queryCheck);
            HashSet<string> exMems = new HashSet<string>();
            GetQBCheck.handleResponse(exCheck, exMems);
            // Checks were retrieved -.......-.....-...-...-.-......
            string res = "";
            // Insert all necessary records ***=============================***
            res += "Last insert date: " + System.DateTime.Now.ToString() + '\n';
            res += "Existing sales receipt " + existingSRArr
                .Count.ToString() + '\n';
            res += "Existing invoice " + exRec
                .Count.ToString() + '\n';
            //Variables which will be used in two loops
            foreach (purchaseOrderRecord po in displayResp)
            {
                customer c = null;
                customer wireCustomer = null;
                poLineItems li = null;
                purchaseOrderItem poItem = null;
                int avoidDup = 0;
                string thisCustomerName = "";
                string wireCustomerName = "";
                string thisVendorNumber = "";
                string promoVendorNumber = "";
                string thisVendorName = "";
                string customerJob = "";
                string custodian = "";
                string extendedCustomerName = "";
                string extendedWireName = "";
                string anyVendor = "";
                string withZeros = Regex.Replace(Regex.Match(po.Name, 
                    @"[\s\0-9]+", 
                    RegexOptions.IgnoreCase).Value, @"[^0-9^\s]", "").Trim();
                string poNum = Regex.Replace(withZeros, "^[0]+", "").Trim();
                // Create four collections for promo, regular and all records
                List<purchaseOrderItem> metalItems =
                    new List<purchaseOrderItem>();
                Dictionary<string, double> poItems = new 
                    Dictionary<string, double>();
                Dictionary<string, double> bgAllCost = 
                    new Dictionary<string, double>();
                Dictionary<string, double> bgCost = new 
                    Dictionary<string, double>();
                Dictionary<string, double> bgCostPromo =
                    new Dictionary<string, double>();
                List<outboundWire> owLst = new List<outboundWire>();
                outboundWire currentWire = null;
                recordType rt = null;
                recordType sfCustodian = null;
                if (po.Client__r != null)
                {
                    c = JsonSerializer
                        .Deserialize<customer>(po.Client__r);
                    thisCustomerName = c.LastName + 
                        ", " + c.FirstName + 
                        " " + c.Birch_LM_Velocify_ID__c;
                    AddClientQB
                            .BuildCustomerAddRq(requestCustomerAdd,
                            thisCustomerName);
                   cNames.Add(thisCustomerName);
                }
                // Create metals and sumarize their prices for Sales Receipt
                rt = JsonSerializer.Deserialize<recordType>(po.RecordType);
                if (po.Custodian_lookup__r != null)
                {
                    sfCustodian = JsonSerializer
                        .Deserialize<recordType>(po.Custodian_lookup__r);
                }
                if (po.Purchase_Order_Line_Items__r != null)
                {
                    li = JsonSerializer
                        .Deserialize<poLineItems>(
                        po.Purchase_Order_Line_Items__r);
                    foreach (JsonDocument item in li.records)
                    {
                        poItem = JsonSerializer
                            .Deserialize<purchaseOrderItem>(item);
                        // Extend all collections and create items if need
                        if(poItem.Coin_Metal_Type__c != null)
                        {
                            if(poItem.Vendor__r != null && 
                                Regex.IsMatch(poItem.Vendor__r.Name, 
                                "[a-z]", RegexOptions.IgnoreCase)) 
                            {
                                anyVendor = poItem.Vendor__r.Name;
                            }
                            bgAllCost.TryAdd(poItem.Coin_Metal_Type__c.Trim(),
                                0.0);
                            poItems.TryAdd(poItem
                                .Coin_Metal_Type__c.Trim(), 0.0);
                            metalItems.Add(poItem);
                            AddNonInventoryItems
                                .BuildItemNonInventoryAddRq(
                                requestMetalAdd,
                                poItem.Coin_Metal_Type__c);
                        }
                        if (poItem.Coin_Metal_Type__c != null && 
                            poItem.Promo_coin__c == false)
                        {
                            bgCost.TryAdd(poItem.Coin_Metal_Type__c.Trim(), 
                                0.0);
                        }
                        if(poItem.Coin_Metal_Type__c != null && 
                            poItem.Promo_coin__c == true)
                        {
                            bgCostPromo.TryAdd(poItem.Coin_Metal_Type__c.Trim(),
                                0.0);
                        }
                        if (po.Promo_Coin_Shipping_Loction__c != null &&
                            po.Promo_Coin_Shipping_Loction__c
                            .Trim() == "Home Address" &&
                            poItem.Promo_coin__c &&
                            poItem.Vendor_Order_Number__c != null)
                        {
                            promoVendorNumber = Regex.Replace(Regex
                                .Match(poItem.Vendor_Order_Number__c,
                                @"[\s\0-9]+", 
                                RegexOptions.IgnoreCase).Value, @"[^0-9^\s]", 
                                "").Trim();
                        }
                    }
                }
                if (po.Vendor_Order__c != null && thisVendorNumber.Length == 0)
                {
                    thisVendorNumber = Regex.Replace(Regex.Match(po
                        .Vendor_Order__c, 
                        @"[\s\0-9]+").Value, @"[^0-9^\s]", "").Trim();
                }
                if(existingSRArr.Contains(withZeros) ||
                    exRec.Contains(withZeros))
                {
                    if (po.Deal_Type__c.Contains("Cash")) 
                    {
                        res += ". Existing invoice: " + withZeros + '\n';
                    } else if (po.Deal_Type__c.Contains("IRA"))
                    {
                        res += ". Existing sales receipt: " + withZeros + '\n';
                    }
                    avoidDup++;
                }
                if (eqbpo.ContainsKey(thisVendorNumber))
                {
                    res += ". Existing purchase order: " + thisVendorNumber + '\n';
                    avoidDup++;
                }
                if (exBills.Contains(thisVendorNumber))
                {
                    res += ". Existing bill: " + thisVendorNumber + '\n';
                    avoidDup++;
                }
                if (eqbpo.ContainsKey(promoVendorNumber))
                {
                    res += ". Existing purchase order: " + promoVendorNumber + '\n';
                    avoidDup++;
                }
                if (exBills.Contains(promoVendorNumber))
                {
                    res += ". Existing bill: " + promoVendorNumber + '\n';
                    avoidDup++;
                }
                if(avoidDup >= 3)
                {
                    continue;
                }
                if (metalItems.Count > 0 && metalItems[0].Vendor__r != null && 
                    metalItems[0].Vendor__r.Name != null)
                {
                    thisVendorName = metalItems[0].Vendor__r.Name.Split(' ')[0]
                        .ToLower();
                }
                foreach (string cn in existingCustomers)
                {
                    if (cn.ToLower().Contains(thisVendorName))
                    {
                        customerJob = cn;
                        break;
                    }
                }
                extendedCustomerName = thisCustomerName + " - BB" +
                            poNum;
                if (po.Deal_Type__c.Contains("IRA") &&
                    !po.Deal_Type__c.Contains("Cash") &&
                    !existingSRArr
                    .Contains(withZeros) &&
                    thisVendorNumber.Length > 0)
                {
                    //TODO: Extand collections for IRA deal type records
                    foreach (purchaseOrderItem metal in metalItems)
                    {
                        poItems[metal.Coin_Metal_Type__c.Trim()] +=
                            metal.Total_Price_Formula__c;
                        bgAllCost[metal.Coin_Metal_Type__c.Trim()] +=
                            metal.BG_Total_Cost__c;
                        if (metal.Promo_coin__c == false)
                        {
                            bgCost[metal.Coin_Metal_Type__c.Trim()] +=
                                metal.BG_Total_Cost__c;
                        } else if(metal.Promo_coin__c == true)
                        {
                            bgCostPromo[metal.Coin_Metal_Type__c.Trim()] += 
                                metal.BG_Total_Cost__c;
                        }
                    }
                    
                    if (rt.Name == "Birch PO")
                    {
                        res += "New sales receipt: " + 
                            withZeros + '\n';
                        AddSalesReceipt
                            .BuildSalesRecepietAddRq(srReq,
                            thisCustomerName,
                            poItems,
                            withZeros,
                            po.Date_Cleared__c, 
                            null, 
                            0.8);
                    }
                    else if(!existingSRArr.Contains(thisVendorNumber))
                    { 
                        res += "New sales receipt: " + thisVendorNumber + '\n';
                      
                        
                        AddSalesReceipt.BuildSalesRecepietAddRq(srReq, 
                            customerJob, 
                            null, 
                            thisVendorNumber, 
                            po.Date_Cleared__c, 
                            extendedCustomerName,
                            po.Total_Amount_Paid__c);
                    }
                }
                else if (po.Deal_Type__c.Contains("Cash") &&
                    exRec != null &&
                    !exRec
                    .Contains(withZeros) && 
                    rt.Name == "Birch PO")
                {
                    foreach (purchaseOrderItem metal in metalItems)
                    {
                        // Extand collections for cash deal type records
                        poItems[metal.Coin_Metal_Type__c] +=
                            metal.Total_Price_Formula__c;
                        bgAllCost[metal.Coin_Metal_Type__c] +=
                            metal.BG_Total_Cost__c;
                        if (metal.Promo_coin__c)
                        {
                            bgCostPromo[metal.Coin_Metal_Type__c] +=
                                metal.BG_Total_Cost__c;
                        }
                        if (!metal.Promo_coin__c)
                        {
                            bgCost[metal.Coin_Metal_Type__c] +=
                                metal.BG_Total_Cost__c;
                        }
                    }
                    res += "New invoice: " + withZeros + '\n';

                    AddInvoiceQB.BuildInvoiceAddRq(invRq,
                    thisCustomerName,
                    poItems,
                    withZeros,
                    po.Date_Cleared__c, 
                    null, 
                    0.7);
                }
                else if (po.Deal_Type__c.Contains("Cash") &&
                    rt.Name == "Buy Back" &&
                    exRec != null &&
                    !exRec
                    .Contains(poNum))
                {
                    poLineItems wireItm = JsonSerializer
                        .Deserialize<poLineItems>(po
                        .Outbound_Wire__r);
                    foreach(JsonDocument jd in wireItm.records)
                    {
                        currentWire = JsonSerializer
                            .Deserialize<outboundWire>(jd);
                        wireCustomer = JsonSerializer
                            .Deserialize<customer>(currentWire.Client__r);
                        wireCustomerName = wireCustomer
                            .LastName + ", " + wireCustomer
                            .FirstName + " " + wireCustomer
                            .Birch_LM_Velocify_ID__c;
                        extendedWireName = wireCustomerName + " - BB" +
                           poNum;
                        if (!exMems.Contains(extendedWireName))
                        {
                            AddCheck.BuildAddCheckQuery(addCheckRq,
                                wireCustomerName,
                                currentWire.Payment_Type__c,
                                extendedWireName,
                                currentWire.Amount__c);
                        }
                    }
                    if (!exMems.Contains(extendedWireName))
                    {
                        res += "New Check: " + poNum + '\n';
                    } else
                    {
                        res += "Existing check: " + poNum + '\n';
                    }
                }
                if (po.All_outbound_wires_inserted__c)
                {
                    Dictionary<string, HashSet<purchaseOrderItem>> allNames = 
                        new Dictionary<string, HashSet<purchaseOrderItem>>();
                    allNames.Add(anyVendor, new HashSet<purchaseOrderItem>());
                    Dictionary<string, HashSet<purchaseOrderItem>> vendorNames = 
                        new Dictionary<string, HashSet<purchaseOrderItem>>();
                    Dictionary<string, HashSet<purchaseOrderItem>> notPromoNames = 
                        new Dictionary<string, HashSet<purchaseOrderItem>>();
                    foreach (purchaseOrderItem poi in metalItems)
                    {
                        if(poi.Vendor__r != null &&
                            !allNames.ContainsKey(poi.Vendor__r.Name))
                        {
                            AddVendor.BuildAddVendorRequest(
                                requestVendorAdd,
                                poi.Vendor__r.Name);
                            allNames.Add(poi.Vendor__r.Name,
                                new HashSet<purchaseOrderItem>());
                        }
                        if(poi.Vendor__r != null && 
                            !vendorNames.ContainsKey(poi.Vendor__r.Name) && 
                            poi.Promo_coin__c)
                        {
                            vendorNames.Add(poi.Vendor__r.Name, 
                                new HashSet<purchaseOrderItem>());
                        }
                        if(poi != null && 
                            poi.Vendor__r != null &&
                            allNames[poi.Vendor__r.Name] != null &&
                            !allNames[poi.Vendor__r.Name]
                            .Contains(poi))
                        {
                            allNames[poi.Vendor__r.Name]
                                .Add(poi);
                        } else if(poi != null &&
                            poi.Vendor__r == null &&
                            anyVendor != "" &&
                            !allNames[anyVendor]
                            .Contains(poi)) 
                        {
                            allNames[anyVendor].Add(poi);
                        }
                        if (poi != null &&
                            poi.Vendor__r != null && 
                            poi.Promo_coin__c &&
                            vendorNames[poi.Vendor__r.Name] != null &&
                            !vendorNames[poi.Vendor__r.Name]
                            .Contains(poi))
                        {
                            vendorNames[poi.Vendor__r.Name]
                                .Add(poi);
                        }
                        else if(poi != null &&
                            poi.Vendor__r == null &&
                            anyVendor != "" &&
                            poi.Promo_coin__c &&
                            vendorNames.ContainsKey(anyVendor) &&
                            !vendorNames[anyVendor]
                            .Contains(poi))
                        {
                            vendorNames[anyVendor]
                                .Add(poi);
                        }
                        if (poi.Vendor__r != null &&
                            !notPromoNames.ContainsKey(poi.Vendor__r.Name) &&
                            !poi.Promo_coin__c)
                        {
                            AddVendor.BuildAddVendorRequest(
                                requestVendorAdd,
                                poi.Vendor__r.Name);
                            notPromoNames.Add(poi.Vendor__r.Name,
                                new HashSet<purchaseOrderItem>());
                        }
                        if (poi != null &&
                            poi.Vendor__r != null &&
                            !poi.Promo_coin__c &&
                            notPromoNames.ContainsKey(poi.Vendor__r.Name) &&
                            notPromoNames[poi.Vendor__r.Name] != null &&
                            !notPromoNames[poi.Vendor__r.Name]
                            .Contains(poi))
                        {
                            notPromoNames[poi.Vendor__r.Name]
                                .Add(poi);
                        }
                        else if (poi != null &&
                            poi.Vendor__r == null &&
                            anyVendor != "" &&
                            !poi.Promo_coin__c &&
                            notPromoNames.ContainsKey(anyVendor) &&
                            !notPromoNames[anyVendor]
                            .Contains(poi))
                        {
                            notPromoNames[anyVendor]
                                .Add(poi);
                        }
                    }
                    foreach(KeyValuePair<string, HashSet<purchaseOrderItem>> kvp 
                        in allNames)
                    {
                        int l = thisVendorNumber.Length > 10 ?
                            10 : thisVendorNumber.Length;
                        thisVendorNumber = thisVendorNumber.Substring(0, l);
                        if (thisVendorNumber.Length > 0 &&
                            (thisVendorNumber == promoVendorNumber ||
                            promoVendorNumber.Length == 0) &&
                            !eqbpo.ContainsKey(thisVendorNumber) &&
                            kvp.Value.Count > 0)
                        {
                            AddPurchaseOrderQB.BuildPurchaseOrderAddRq(
                                addPurchaseOrder,
                                po.Name,
                                thisVendorNumber,
                                kvp.Key,
                                thisCustomerName,
                                po.Date_Cleared__c,
                                kvp.Value);
                            res += "New PurchaseOrder: " + 
                                thisVendorNumber + '\n';
                        }
                    }
                    foreach (
                        KeyValuePair<string, 
                        HashSet<purchaseOrderItem>> kvp in vendorNames)
                    {
                        int l = promoVendorNumber.Length > 10 ?
                            10 : promoVendorNumber.Length;
                        promoVendorNumber = promoVendorNumber.Substring(0, l);
                        if (promoVendorNumber.Length > 0 &&
                            promoVendorNumber != thisVendorNumber &&
                            !eqbpo.ContainsKey(promoVendorNumber) &&
                            kvp.Value.Count > 0)
                        {
                            AddPurchaseOrderQB.BuildPurchaseOrderAddRq(
                                addPurchaseOrder,
                                po.Name,
                                promoVendorNumber,
                                kvp.Key,
                                thisCustomerName,
                                po.Date_Cleared__c,
                                kvp.Value);
                            res += "New PurchaseOrder: " +
                                promoVendorNumber + '\n';
                        }
                    }
                    foreach(KeyValuePair<string, HashSet<purchaseOrderItem>> 
                        kvp in notPromoNames)
                    {
                        int l = thisVendorNumber.Length > 10 ?
                            10 : thisVendorNumber.Length;
                        thisVendorNumber = thisVendorNumber.Substring(0, l);
                        if (thisVendorNumber.Length > 0 &&
                            !eqbpo.ContainsKey(thisVendorNumber) &&
                            thisVendorNumber != promoVendorNumber &&
                            promoVendorNumber.Length > 0 &&
                            kvp.Value.Count > 0)
                        {
                            AddPurchaseOrderQB.BuildPurchaseOrderAddRq(
                                addPurchaseOrder,
                                po.Name,
                                thisVendorNumber,
                                kvp.Key,
                                thisCustomerName,
                                po.Date_Cleared__c,
                                kvp.Value);
                            res += "New PurchaseOrder: " + 
                                thisVendorNumber + '\n';
                        }
                    }
                }
                if (eqbpo.ContainsKey(promoVendorNumber))
                {
                    res += "Existing PurchaseOrder: " + promoVendorNumber + '\n';
                }
                if (eqbpo.ContainsKey(thisVendorNumber))
                {
                    res += "Existing PurchaseOrder: " + thisVendorNumber + '\n';
                }
                if (exBills.Contains(thisVendorNumber))
                {
                    res += "Existing bill: " + thisVendorNumber + '\n';
                }
                if (exBills.Contains(promoVendorNumber))
                {
                    res += "Existing bill: " + promoVendorNumber + '\n';
                }
            }
            // Start execute requests ****************************************
            StreamWriter sw = null;
            try 
            {
                if (!System.IO.File.Exists("traceLog.txt")) 
                {
                    using (FileStream fs = System.IO.File
                        .Create("traceLog.txt")) ;
                }
            }catch(Exception excp) 
            {
                Console.WriteLine("Failed to create a file => " + 
                    excp.Message + '\n');
                sw = null;
            }
            try
            {
                sw = new("traceLog.txt", append: true);
            } catch(Exception ex)
            {
                Console.WriteLine("Can't open a file\n");
            }
            try
            {
                IMsgSetResponse rspn = sessionManager
                    .DoRequests(requestCustomerAdd);
                IResponseList rspnLst = rspn.ResponseList;
                if(rspnLst != null)
                {
                    for(int i = 0; i < rspnLst.Count; i++)
                    {
                        IResponse rspk = rspnLst.GetAt(i);
                        Console.WriteLine(rspk.StatusCode + '\n');
                        if (sw != null)
                        {
                            try
                            {
                                sw.WriteLineAsync(rspk.StatusMessage + '\n');
                            } catch(Exception ei) 
                            {
                                res += ei.Message + '\n';
                            }
                        }
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed create customer: " + e.Message + '\n');
                if (sw != null)
                {
                    try
                    {
                        sw.WriteLineAsync(e.Message + '\n');
                    } catch(Exception ei) 
                    {
                        res += ei.Message + '\n';
                    }
                }
            }
            try
            {
                sessionManager.DoRequests(requestVendorAdd);
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message + '\n');
                if (sw != null)
                {
                    try
                    {
                        sw.WriteLineAsync(ex.Message + '\n');
                    } catch(Exception ei) 
                    {
                        res += ei.Message + '\n';
                    }
                }
            }
            try
            {
                IMsgSetResponse r =  sessionManager
                    .DoRequests(requestMetalAdd);
                IResponseList rlst = r.ResponseList;
                if(rlst != null)
                {
                    for(int i = 0; i < rlst.Count; i++)
                    {
                        IResponse rsp = r.ResponseList.GetAt(i);
                        Console.WriteLine(rsp.StatusMessage + '\n');
                        if (sw != null)
                        {
                            try
                            {
                                sw.WriteLineAsync(rsp.StatusMessage + '\n');
                            } catch(Exception ei) 
                            {
                                res += ei.Message + '\n';
                            }
                        }
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed create items: " + 
                    e.Message + '\n');
                if (sw != null)
                {
                    try
                    {
                        sw.WriteLineAsync(e.Message + '\n');
                    } catch(Exception ei) 
                    {
                        res += ei.Message + '\n';
                    }
                }
            }
            try
            {
                IMsgSetResponse r = sessionManager.DoRequests(srReq);
                IResponseList rlst = r.ResponseList;
                if(rlst != null)
                {
                    for(int i = 0; i < rlst.Count; i++)
                    {
                        IResponse respons = rlst.GetAt(i);
                        Console.WriteLine("SR st" + respons.StatusMessage);
                        if (sw != null)
                        {
                            try
                            {
                                sw.WriteLineAsync(respons.StatusMessage + '\n');
                            } catch(Exception ei) 
                            {
                                res += ei.Message + '\n';
                            }
                        }
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed create sales receipt: " + 
                    e.Message + '\n');
                if (sw != null)
                {
                    try
                    {
                        sw.WriteLineAsync(e.Message + '\n');
                    } catch (Exception ei) 
                    {
                        res += ei.Message + '\n';
                    }
                }
            }
            try
            {
                IMsgSetResponse createdInv = sessionManager.DoRequests(invRq);
                IResponseList createdInvRspLst = createdInv.ResponseList;
                if(createdInvRspLst != null)
                {
                    for(int i = 0; i < createdInvRspLst.Count; i++)
                    {
                        IResponse rsp = createdInvRspLst.GetAt(i);
                        Console.WriteLine("query inv " + 
                            rsp.StatusMessage + '\n');
                        if (sw != null)
                        {
                            try
                            {
                                sw.Dispose();
                                sw.WriteLineAsync(rsp.StatusMessage + '\n');
                            }catch(Exception ei) 
                            {
                                res += ei.Message + '\n';
                            }
                        }
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed create invoice: " + e.Message + '\n');
                if (sw != null)
                {
                    try
                    {
                        sw.WriteLineAsync(e.Message + '\n');
                    } catch(Exception ei) 
                    {
                        res += ei.Message + '\n';
                    }
                }
            }
            try
            {
                IMsgSetResponse response = sessionManager
                    .DoRequests(addPurchaseOrder);
                IResponseList responseLst = response.ResponseList;
                if(responseLst != null)
                {
                    for(int i = 0; i < responseLst.Count; i++)
                    {
                        IResponse resp = responseLst.GetAt(i);
                        if(resp.StatusCode >= 0 && resp.Detail != null)
                        {
                            IPurchaseOrderRet ret = 
                                (IPurchaseOrderRet)resp.Detail;
                            string refNum = ret.RefNumber.GetValue();
                            string itemIds;
                            eqbpo.Add(refNum, 
                                new List<string>());
                            if(ret.ORPurchaseOrderLineRetList != null &&
                                ret.ORPurchaseOrderLineRetList.Count > 0)
                            {
                                for(int j = 0; 
                                    j < ret.ORPurchaseOrderLineRetList.Count;
                                    j++)
                                {
                                    itemIds = "";
                                    itemIds += ret
                                        .ORPurchaseOrderLineRetList
                                        .GetAt(j)
                                        .PurchaseOrderLineRet
                                        .TxnLineID
                                        .GetValue() + "$%&";
                                    itemIds += ret.TxnID.GetValue();
                                    eqbpo[refNum].Add(itemIds);
                                }
                            }
                        }
                        Console.Write("PO--->" + resp.StatusMessage + '\n');
                        if (sw != null)
                        {
                            try
                            {
                                sw.WriteLineAsync(resp.StatusMessage + '\n');
                            }catch(Exception ei)
                            {
                                res += ei.Message + '\n';
                            }
                        }
                    }
                }
            } catch(Exception ex) 
            { 
                Console.WriteLine("PO Error ===> " + 
                    ex.Message + " STATUS CODE ===> " + ex.Data + '\n');
                if (sw != null)
                {
                    try
                    {
                        sw.WriteLineAsync(ex.Message + '\n');
                    }catch(Exception ei) 
                    {
                        res += ei.Message + '\n';
                    }
                }
            }
            /*TO CREATE BILLS AT THE SAME TIME WITH PURCHASE ORDERS WE NEED USE
             INSERTED RECORDS IDs*/
            /*GO THROUGH RETRIEVED RECORDS AGAIN*/
            foreach(purchaseOrderRecord por in displayResp)
            {
                if (por.All_outbound_wires_inserted__c)
                {
                    string sfPoN = Regex.Match(por.Name, "[0-9]+").Value;
                    Dictionary<string, List<string>> vendorNames = 
                        new Dictionary<string, List<string>>();
                    string lastVendor = "";
                    List<purchaseOrderItem> metalItems =
                        new List<purchaseOrderItem>();
                    poLineItems li = null;
                    purchaseOrderItem poItem = null;
                    string promoVendorNumber = "";
                    string thisVendorNumber = "";
                    string custodian = "";
                    if(por.RecordType == null) 
                    {
                        throw new Exception("Record Type not set" + 
                            "on Salesforce side");
                    }
                    recordType rt = JsonSerializer
                        .Deserialize<recordType>(por.RecordType);
                    recordType sfCustodian = null;
                    if(por.Custodian_lookup__r != null)
                    {
                        sfCustodian = JsonSerializer
                            .Deserialize<recordType>(por.Custodian_lookup__r);
                    }
                    string thisCustomerName = "";
                    string extendedCustomerName = "";
                    if (por.Purchase_Order_Line_Items__r != null)
                    {
                        li = JsonSerializer
                            .Deserialize<poLineItems>(
                            por.Purchase_Order_Line_Items__r);
                        foreach (JsonDocument item in li.records)
                        {
                            poItem = JsonSerializer
                                .Deserialize<purchaseOrderItem>(item);
                            if (poItem.Coin_Metal_Type__c != null)
                            {
                                metalItems.Add(poItem);
                                AddNonInventoryItems
                                    .BuildItemNonInventoryAddRq(
                                    requestMetalAdd,
                                    poItem.Coin_Metal_Type__c);
                            }
                            // Extend all collections and create items if need
                            if (por.Promo_Coin_Shipping_Loction__c != null &&
                                por.Promo_Coin_Shipping_Loction__c
                                .Trim() == "Home Address" &&
                                poItem.Promo_coin__c &&
                                poItem.Vendor_Order_Number__c != null)
                            {
                                promoVendorNumber = Regex.Replace(Regex
                                    .Match(poItem.Vendor_Order_Number__c,
                                    @"[\s\0-9]+",
                                    RegexOptions.IgnoreCase).Value, @"[^0-9^\s]",
                                    "").Trim();
                            }
                        }
                        if (por.Vendor_Order__c != null &&
                            thisVendorNumber.Length == 0)
                        {
                            thisVendorNumber = Regex.Replace(Regex.Match(por
                                .Vendor_Order__c,
                                @"[\s\0-9]+").Value, @"[^0-9^\s]", "").Trim();
                        }
                        if (exBills.Contains(promoVendorNumber))
                        {
                            res += ". Existing bill: " + promoVendorNumber + '\n';
                            continue;
                        }
                        if (exBills.Contains(thisVendorNumber))
                        {
                            res += ". Existing bill: " + thisVendorNumber + '\n';
                            continue;
                        }
                    }
                    foreach (purchaseOrderItem poi in metalItems)
                    {
                        if (poi.Vendor__r != null &&
                            !vendorNames.ContainsKey(poi.Vendor__r.Name))
                        {
                            vendorNames.Add(poi.Vendor__r.Name,
                                new List<string>());
                        }
                        if (poi != null &&
                            poi.Vendor__r != null &&
                            vendorNames[poi.Vendor__r.Name] != null &&
                            !vendorNames[poi.Vendor__r.Name]
                            .Contains(poi.Coin_Metal_Type__c))
                        {
                            vendorNames[poi.Vendor__r.Name]
                                .Add(poi.Coin_Metal_Type__c);
                            lastVendor = poi.Vendor__r.Name;
                        }
                        else if (poi != null &&
                            poi.Vendor__r == null &&
                            lastVendor != "" &&
                            !vendorNames[lastVendor]
                            .Contains(poi.Coin_Metal_Type__c))
                        {
                            vendorNames[lastVendor]
                                .Add(poi.Coin_Metal_Type__c);
                        }
                    }
                    foreach (
                        KeyValuePair<string,
                        List<string>> kvp in vendorNames)
                    {
                        if (rt.Name == "Birch PO")
                        {
                            int l = promoVendorNumber.Length > 10 ?
                                10 : promoVendorNumber.Length;
                            promoVendorNumber = promoVendorNumber.Substring(0, l);
                            l = thisVendorNumber.Length > 10 ?
                                10 : thisVendorNumber.Length;
                            thisVendorNumber = thisVendorNumber.Substring(0, l);
                            if (promoVendorNumber.Length > 0 &&
                                promoVendorNumber == thisVendorNumber &&
                                !exBills.Contains(promoVendorNumber) &&
                                eqbpo.ContainsKey(promoVendorNumber))
                            {
                                AddBill.BuildBillAddRq(queryBillCreate,
                                    kvp.Key,
                                    por.Date_Cleared__c,
                                    por.Name,
                                    promoVendorNumber,
                                    eqbpo,
                                    0.0,
                                    "",
                                    "",
                                    sfPoN);
                                res += "New bill: " + promoVendorNumber + '\n';
                            }
                            if (promoVendorNumber.Length > 0 &&
                                promoVendorNumber != thisVendorNumber &&
                                !exBills.Contains(promoVendorNumber) &&
                                eqbpo.ContainsKey(promoVendorNumber))
                            {
                                AddBill.BuildBillAddRq(queryBillCreate,
                                    kvp.Key,
                                    por.Date_Cleared__c,
                                    por.Name,
                                    promoVendorNumber,
                                    eqbpo,
                                    0.0,
                                    "",
                                    "",
                                    sfPoN);
                                res += "New bill: " + promoVendorNumber + '\n';
                            }
                            if (thisVendorNumber.Length > 0 &&
                                thisVendorNumber != promoVendorNumber &&
                                !exBills.Contains(thisVendorNumber) &&
                                eqbpo.ContainsKey(thisVendorNumber))
                            {
                                AddBill.BuildBillAddRq(queryBillCreate,
                                    kvp.Key,
                                    por.Date_Cleared__c,
                                    por.Name,
                                    thisVendorNumber,
                                    eqbpo,
                                    0.0,
                                    "",
                                    "",
                                    sfPoN);
                                res += "New bill: " + thisVendorNumber + '\n';
                            }
                        }
                        else
                        {
                            foreach (string cust in qbVenodors)
                            {
                                if (sfCustodian != null &&
                                    cust.Contains(sfCustodian.Name.Split(' ')[0]))
                                {
                                    custodian = cust;
                                }
                            }
                            if (por.IRA_Account__c != null)
                            {
                                string billNo = Regex.Replace(Regex
                                    .Match(por.IRA_Account__c,
                                    @"[\s\0-9]+", RegexOptions.IgnoreCase).Value,
                                    @"[^0-9^\s]", "").Trim();
                                if (exBills.Contains(billNo))
                                {
                                    res += "Existing bill: " + billNo + '\n';
                                    continue;
                                }
                                res += "New bill: " + billNo + '\n';
                                AddBill.BuildBillAddRq(queryBillCreate,
                                    custodian,
                                    por.Date_Cleared__c,
                                    por.Name,
                                    billNo,
                                    null,
                                    por.Total_Cost__c,
                                    thisCustomerName,
                                    extendedCustomerName,
                                    sfPoN);
                            }
                        }
                    }
                }
            }
            /*BILLS SHOULD BE INSERTED PROPERLY*/
            try
            {
                IMsgSetResponse rsp = sessionManager.DoRequests(queryBillCreate);
                IResponseList rlst = rsp.ResponseList;
                if(rlst != null)
                {
                    for(int i = 0; i < rlst.Count; i++)
                    {
                        IResponse r = rlst.GetAt(i);
                        Console.WriteLine("billcreate " + r.StatusMessage + '\n');
                        if (sw != null)
                        {
                            try
                            {
                                sw.WriteLineAsync(r.StatusMessage + '\n');
                            }catch(Exception ei) 
                            {
                                res += ei.Message + '\n';
                            }
                        }
                    }
                }
            } catch(Exception exce)
            {
                Console.WriteLine(exce.Message);
                if (sw != null)
                {
                    try
                    {
                        sw.WriteLineAsync(exce.Message + '\n');
                    }catch(Exception ei) 
                    {
                        res += ei.Message + '\n';
                    }
                }
            }
            try
            {
                IMsgSetResponse rsp = sessionManager.DoRequests(addCheckRq);
                if(rsp.ResponseList != null)
                {
                    for(int i = 0; i < rsp.ResponseList.Count; i++)
                    {
                        Console.WriteLine("Check situation" + rsp
                            .ResponseList
                            .GetAt(i)
                            .StatusMessage + '\n');
                        if (sw != null)
                        {
                            try
                            {
                                sw.WriteLineAsync(rsp.ResponseList
                                    .GetAt(i).StatusMessage);
                            }catch(Exception ei) 
                            {
                                res += ei.Message + '\n';
                            }
                        }
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
                if (sw != null)
                {
                    try
                    {
                        sw.WriteLineAsync(e.Message + '\n');
                    }catch (Exception ei) 
                    {
                        res += ei.Message + '\n';
                    }
                }
            }
            // End session
            sessionManager.EndSession();
            sessionManager.CloseConnection();
            if (res.Length > 0)
            {
                Response.WriteAsync(res);
            } else if(res.Length == 0)
            {
                Response.WriteAsync("No records during the last 20 days");
            }
            // POST data to SF
            if (false)
            {
                PostPOtoSF.postData(displayResp);
            }
            //nullify all stuff
            existingSRArr = null;
            ApiCall.porLst = null;
            res = null;
            await Task.Delay(3000);
            Process.Start("restart.cmd");
        }
    }
    [ApiController]
    [Route("author")]
    public class AuthorController : ControllerBase
    {
        [HttpGet(Name = "GetAuthor")]
        public async Task Get() 
        {
            List<int> lst = new List<int>();
            lst.Add(1);
            
            Response.WriteAsync("Created by Oleksii Yaremchuk" + 
                lst[0]);
        }
    }
}