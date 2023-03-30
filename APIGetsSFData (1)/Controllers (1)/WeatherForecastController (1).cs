using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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
        private string thisVendorNumber;
        [HttpGet(Name = "SFData")]
        public async Task Get()
        {
            System.GC.Collect();
            // Call SF API *~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*~*
            string mode = "prod";
            if (mode == "prod")
            {
                await ApiCall.getData();
            } else
            {
                await ApiCallSb.getData();
            }
            List<purchaseOrderRecord> displayResp = mode == "prod" ? 
                ApiCall.porLst : ApiCallSb.porLst;
            // Check if there are new records
            if(displayResp.Count == 0)
            {
                Response.WriteAsync("No new records");
                return;
            }
            // end check
            // Make list of po numbers
            List<string> poNumLst = new List<string>();
            poLineItems itms = null;
            purchaseOrderItem poitem = null;
            foreach (purchaseOrderRecord record in displayResp)
            {
                poNumLst.Add(Regex.Replace(Regex
                    .Match(record.Name, @"[\s\0-9]+").Value, @"[^0-9^\s]", "")
                    .Trim());
                if (record.Purchase_Order_Line_Items__r != null)
                {
                    itms = JsonSerializer.Deserialize<poLineItems>(
                        record.Purchase_Order_Line_Items__r);
                    if(itms.records != null)
                    {
                        foreach(JsonDocument i in itms.records)
                        {
                            poitem = JsonSerializer
                                .Deserialize<purchaseOrderItem>(i);
                            if(poitem.Vendor_Order_Number__c == null)
                            {
                                continue;
                            }
                            this.thisVendorNumber = Regex.Replace(Regex
                                .Match(poitem.Vendor_Order_Number__c, 
                                @"[\s\0-9]+").Value, @"[^0-9^\s]", "")
                                .Trim();
                            poNumLst.Add(this.thisVendorNumber);
                        }
                    }
                }
            }
            // List is ready ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
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
            IMsgSetRequest updateSalesReceipt = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            updateSalesReceipt.Attributes.OnError = ENRqOnError.roeContinue;
            IMsgSetRequest updateInvoiceQB = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            updateInvoiceQB.Attributes.OnError = ENRqOnError.roeContinue;
            // Purchase Order QB Request
            IMsgSetRequest addPurchaseOrder = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            addPurchaseOrder.Attributes.OnError = ENRqOnError.roeContinue;
            // End Purchase Order QB Request
            ///////////////////////////////////////////////Begin session
            sessionManager.OpenConnection("", "Production json");
            // Specify file to run API even file is closed
            sessionManager.BeginSession("", ENOpenMode.omDontCare);
            IMsgSetRequest getCustomers = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            getCustomers.Attributes.OnError = ENRqOnError.roeContinue;
            GetQBCustomers.buildCustomerQuery(getCustomers);
            List<string> existingCustomers = new List<string>();
            IMsgSetResponse getCustomersResponse = sessionManager
                .DoRequests(getCustomers);
            GetQBCustomers.handleResponse(getCustomersResponse, existingCustomers);
            // Customers were retrieved ------@@@@@@@@@@@
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
            List<string> qbVenodors = new List<string>();
            GetQBVendors.handleResult(responseVendors, qbVenodors);
            List<string> cNames = new List<string>();
            /// List was created =***=*=*=**=**=**=*=*=***=**=***=**=*=*=***===
            /// Add check or not *****
            IMsgSetRequest addCheckRq = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            addCheckRq.Attributes.OnError = ENRqOnError.roeContinue;
            /// Request was created (.)(.)(.)(.)(.)(.)
            /// QueryCheck
            // Checks were retrieved -.......-.....-...-...-.-......
            string res = "";
            // Insert all necessary records ***=============================***
            //Variables which will be used in two loops
            foreach (purchaseOrderRecord po in displayResp)
            {
                customer c = null;
                customer wireCustomer = null;
                poLineItems li = null;
                purchaseOrderItem poItem = null;
                string thisCustomerName = "";
                string wireCustomerName = "";
                string thisVendorNumber = "";
                string promoVendorNumber = "";
                string thisVendorName = "";
                string customerJob = "";
                string custodian = "";
                string extendedCustomerName = "";
                string extendedWireName = "";
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
                    
                    if (rt.Name == "Birch PO" && !po.Imported_to_QB__c)
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
                    else if(rt.Name == "Birch PO" && po.Imported_to_QB__c)
                    {
                        res += "Existing sales receipt: " + withZeros + '\n';
                    }
                    else if(rt.Name == "Buy Back" && !po.Imported_to_QB__c)
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
                    else
                    {
                        res += "Existing sales receipt: " + 
                            thisVendorNumber + '\n';
                    }
                }
                else if (po.Deal_Type__c.Contains("Cash") && 
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
                    if (!po.Imported_to_QB__c)
                    {
                        res += "New invoice: " + withZeros + '\n';

                        AddInvoiceQB.BuildInvoiceAddRq(invRq,
                        thisCustomerName,
                        poItems,
                        withZeros,
                        po.Date_Cleared__c,
                        null,
                        0.7);
                    } else
                    {
                        res += "Existing invoice: " + withZeros + '\n';
                    }
                }
                else if (po.Deal_Type__c.Contains("Cash") &&
                    rt.Name == "Buy Back")
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
                        if (!po.Imported_to_QB__c)
                        {
                            AddCheck.BuildAddCheckQuery(addCheckRq,
                                wireCustomerName,
                                currentWire.Payment_Type__c,
                                extendedWireName,
                                currentWire.Amount__c);
                            res += "New Check: " + extendedWireName + '\n';
                        } else
                        {
                            res += "Existing Check: " + extendedWireName + '\n';
                        }
                    }
                }
                else if (po.Deal_Type__c.Contains("IRA") && 
                    !po.Deal_Type__c.Contains("Cash"))
                {
                    foreach (purchaseOrderItem metal in metalItems)
                    {
                        // Extend all collections for IRA deal type records
                        poItems[metal.Coin_Metal_Type__c] +=
                            metal.Total_Price_Formula__c;
                        bgAllCost[metal.Coin_Metal_Type__c] +=
                            metal.BG_Total_Cost__c;
                        if (metal.Promo_coin__c == false)
                        {
                            bgCost[metal.Coin_Metal_Type__c] +=
                                metal.BG_Total_Cost__c;
                        } else if(metal.Promo_coin__c == true)
                        {
                            bgCostPromo[metal.Coin_Metal_Type__c] +=
                                metal.BG_Total_Cost__c;
                        }
                    }
                }
                else if (po.Deal_Type__c.Contains("Cash"))
                {
                    foreach (purchaseOrderItem metal in metalItems)
                    {
                        // Extand collections for cash deal type records
                        poItems[metal.Coin_Metal_Type__c] +=
                            metal.Total_Price_Formula__c;
                        bgAllCost[metal.Coin_Metal_Type__c] +=
                            metal.BG_Total_Cost__c;
                        if (!metal.Promo_coin__c)
                        {
                            bgCost[metal.Coin_Metal_Type__c] +=
                                metal.BG_Total_Cost__c;
                        }
                        if (metal.Promo_coin__c)
                        {
                            bgCostPromo[metal.Coin_Metal_Type__c] +=
                                metal.BG_Total_Cost__c;
                        }
                    }
                }
                
                if (po.All_outbound_wires_inserted__c)
                {
                    Dictionary<string, HashSet<purchaseOrderItem>> allNames = 
                        new Dictionary<string, HashSet<purchaseOrderItem>>();
                    Dictionary<string, HashSet<purchaseOrderItem>> vendorNames = 
                        new Dictionary<string, HashSet<purchaseOrderItem>>();
                    Dictionary<string, HashSet<purchaseOrderItem>> notPromoNames = 
                        new Dictionary<string, HashSet<purchaseOrderItem>>();
                    string lastVendor = "";
                    int indexName = 0;
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
                            lastVendor = poi.Vendor__r.Name;
                        } else if(poi != null &&
                            poi.Vendor__r == null &&
                            lastVendor != "" &&
                            !allNames[lastVendor]
                            .Contains(poi)) 
                        {
                            allNames[lastVendor].Add(poi);
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
                            lastVendor = poi.Vendor__r.Name;
                        }
                        else if(poi != null &&
                            poi.Vendor__r == null &&
                            lastVendor != "" &&
                            poi.Promo_coin__c &&
                            vendorNames.ContainsKey(lastVendor) &&
                            !vendorNames[lastVendor]
                            .Contains(poi))
                        {
                            vendorNames[lastVendor]
                                .Add(poi);
                        }
                        if (poi != null && poi.Vendor__r == null &&
                            lastVendor == "" &&
                            !allNames.ContainsKey(indexName.ToString())) 
                        {
                            allNames.Add(indexName.ToString(), 
                                new HashSet<purchaseOrderItem>());
                        }
                        if (poi != null && poi.Vendor__r == null && 
                            lastVendor == "" && 
                            poi.Promo_coin__c && 
                            !vendorNames.ContainsKey(indexName.ToString()))
                        {
                            vendorNames.Add(indexName.ToString(), 
                                new HashSet<purchaseOrderItem>());
                        }
                        if (poi != null && poi.Vendor__r == null &&
                            allNames.ContainsKey(indexName.ToString()) &&
                            !allNames[indexName.ToString()]
                            .Contains(poi))
                        {
                            allNames[indexName.ToString()]
                                .Add(poi);
                        }
                        if (poi != null && poi.Vendor__r == null && 
                            poi.Promo_coin__c &&
                            vendorNames.ContainsKey(indexName.ToString()) &&
                            !vendorNames[indexName.ToString()]
                            .Contains(poi))
                        {
                            vendorNames[indexName.ToString()]
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
                            lastVendor = poi.Vendor__r.Name;
                        }
                        else if (poi != null &&
                            poi.Vendor__r == null &&
                            lastVendor != "" &&
                            !poi.Promo_coin__c &&
                            notPromoNames.ContainsKey(lastVendor) &&
                            !notPromoNames[lastVendor]
                            .Contains(poi))
                        {
                            notPromoNames[lastVendor]
                                .Add(poi);
                        }
                        if (poi != null && poi.Vendor__r == null &&
                            lastVendor == "" &&
                            !poi.Promo_coin__c &&
                            !notPromoNames.ContainsKey(indexName.ToString()))
                        {
                            notPromoNames.Add(indexName.ToString(),
                                new HashSet<purchaseOrderItem>());
                        }
                        if (poi != null && poi.Vendor__r == null &&
                            !poi.Promo_coin__c &&
                            notPromoNames.ContainsKey(indexName.ToString()) &&
                            !notPromoNames[indexName.ToString()]
                            .Contains(poi))
                        {
                            notPromoNames[indexName.ToString()]
                                .Add(poi);
                        }
                        indexName++;
                    }
                    foreach(KeyValuePair<string, HashSet<purchaseOrderItem>> kvp 
                        in allNames)
                    {
                        if (!po.Imported_to_QB__c && 
                            thisVendorNumber.Length > 0 &&
                            (thisVendorNumber == promoVendorNumber ||
                            promoVendorNumber.Length == 0) &&
                            kvp.Value.Count > 0)
                        {
                            int l = thisVendorNumber.Length > 10 ? 
                                10 : thisVendorNumber.Length;
                            thisVendorNumber = thisVendorNumber.Substring(0, l);
                            AddPurchaseOrderQB.BuildPurchaseOrderAddRq(
                                addPurchaseOrder,
                                po.Name,
                                thisVendorNumber,
                                kvp.Key,
                                thisCustomerName,
                                po.Date_Cleared__c,
                                kvp.Value);
                            res += "New PurchaseOrder: " + thisVendorNumber + '\n';
                        }
                        if(po.Imported_to_QB__c && 
                            thisVendorNumber.Length > 0 && 
                            (thisVendorNumber == promoVendorNumber ||
                            promoVendorNumber.Length == 0) && 
                            kvp.Value.Count > 0)
                        {
                            res += "Existing PurchaseOrder: " +
                                thisVendorNumber + '\n';
                        }
                    }
                    foreach (
                        KeyValuePair<string, 
                        HashSet<purchaseOrderItem>> kvp in vendorNames)
                    {
                        if (!po.Imported_to_QB__c && 
                            promoVendorNumber.Length > 0 &&
                            promoVendorNumber != thisVendorNumber &&
                            kvp.Value.Count > 0)
                        {
                            int l = promoVendorNumber.Length > 10 ?
                                10 : thisVendorNumber.Length;
                            promoVendorNumber = promoVendorNumber.Substring(0, l);
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
                        if(po.Imported_to_QB__c && 
                            promoVendorNumber.Length > 0 &&
                            promoVendorNumber != thisVendorNumber &&
                            kvp.Value.Count > 0)
                        {
                            res += "Existing PurchaseOrder: " +
                                promoVendorNumber + '\n';
                        }
                    }
                    foreach(KeyValuePair<string, HashSet<purchaseOrderItem>> 
                        kvp in notPromoNames)
                    {
                        if (!po.Imported_to_QB__c && 
                            thisVendorNumber.Length > 0 &&
                            thisVendorNumber != promoVendorNumber &&
                            promoVendorNumber.Length > 0 &&
                            kvp.Value.Count > 0)
                        {
                            int l = thisVendorNumber.Length > 10 ?
                                10 : thisVendorNumber.Length;
                            thisVendorNumber = thisVendorNumber.Substring(0, l);
                            AddPurchaseOrderQB.BuildPurchaseOrderAddRq(
                                addPurchaseOrder,
                                po.Name,
                                thisVendorNumber,
                                kvp.Key,
                                thisCustomerName,
                                po.Date_Cleared__c,
                                kvp.Value);
                            res += "New PurchaseOrder: " + thisVendorNumber + '\n';
                        }
                        if (po.Imported_to_QB__c && 
                            thisVendorNumber.Length > 0 && 
                            thisVendorNumber != promoVendorNumber && 
                            promoVendorNumber.Length > 0 && 
                            kvp.Value.Count > 0)
                        {
                            res += "Existing PurchaseOrder: " +
                                thisVendorNumber + '\n';
                        }
                    }
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
                            sw.WriteLineAsync(rspk.StatusMessage + '\n');
                        }
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed create customer: " + e.Message + '\n');
                if (sw != null)
                {
                    sw.WriteLineAsync(e.Message + '\n');
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
                    sw.WriteLineAsync(ex.Message + '\n');
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
                            sw.WriteLineAsync(rsp.StatusMessage + '\n');
                        }
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed create items: " + 
                    e.Message + '\n');
                if (sw != null)
                {
                    sw.WriteLineAsync(e.Message + '\n');
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
                            sw.WriteLineAsync(respons.StatusMessage + '\n');
                        }
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed create sales receipt: " + 
                    e.Message + '\n');
                if (sw != null)
                {
                    sw.WriteLineAsync(e.Message + '\n');
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
                            sw.WriteLineAsync(rsp.StatusMessage + '\n');
                        }
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed create invoice: " + e.Message + '\n');
                if (sw != null)
                {
                    sw.WriteLineAsync(e.Message + '\n');
                }
            }
            try
            {
              IMsgSetResponse response =  sessionManager
                    .DoRequests(updateSalesReceipt);
                IResponseList respLst = response.ResponseList;
                if (respLst != null)
                {
                    for (int i = 0; i < respLst.Count; i++)
                    {
                        IResponse resp = respLst.GetAt(i);
                        Console.WriteLine("update sr " + resp.StatusMessage + '\n');
                        if (sw != null)
                        {
                            sw.WriteLineAsync(resp.StatusMessage + '\n');
                        }
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed update sales receipt: " + 
                    e.Message + '\n');
                if (sw != null)
                {
                    sw.WriteLineAsync(e.Message);
                }
            }
            try
            {
                sessionManager.DoRequests(updateInvoiceQB);
            } catch(Exception e)
            {
                Console.WriteLine("Failed update invoice: " + e.Message + '\n');
                if (sw != null)
                {
                   sw.WriteLineAsync(e.Message + '\n');
                }
            }
            Dictionary<string, List<string>> eqbpo =
                new Dictionary<string, List<string>>();
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
                            eqbpo.Add(refNum, new List<string>());
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
                            sw.WriteLineAsync(resp.StatusMessage + '\n');
                        }
                    }
                }
            } catch(Exception ex) 
            { 
                Console.WriteLine("PO Error ===> " + 
                    ex.Message + " STATUS CODE ===> " + ex.Data + '\n');
                if (sw != null)
                {
                    sw.WriteLineAsync(ex.Message + '\n');
                }
            }
            /*TO CREATE BILLS AT THE SAME TIME WITH PURCHASE ORDERS WE NEED
             QUERY PURCHASE ORDERS AND GO THROUGH RETRIEVED FROM SF RECORDS*/
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
                        throw new Exception("Recort Type not set" + 
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
                            if (!por.Imported_to_QB__c && 
                                promoVendorNumber.Length > 0 &&
                                promoVendorNumber == thisVendorNumber &&
                                eqbpo.ContainsKey(promoVendorNumber))
                            {
                                int l = promoVendorNumber.Length > 10 ?
                                    10 : promoVendorNumber.Length;
                                promoVendorNumber = promoVendorNumber
                                    .Substring(0, l);
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
                            if(por.Imported_to_QB__c && 
                                promoVendorNumber.Length > 0 &&
                                promoVendorNumber == thisVendorNumber &&
                                eqbpo.ContainsKey(promoVendorNumber))
                            {
                                res += "Existing bill: " + 
                                    promoVendorNumber + '\n';
                            }
                            if (!por.Imported_to_QB__c && 
                                promoVendorNumber.Length > 0 &&
                                promoVendorNumber != thisVendorNumber &&
                                eqbpo.ContainsKey(promoVendorNumber))
                            {
                                int l = promoVendorNumber.Length > 10 ?
                                    10 : promoVendorNumber.Length;
                                promoVendorNumber = promoVendorNumber
                                    .Substring(0, l);
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
                            if(por.Imported_to_QB__c && 
                                promoVendorNumber.Length > 0 && 
                                promoVendorNumber != thisVendorNumber &&
                                eqbpo.ContainsKey(promoVendorNumber))
                            {
                                res += "Existing bill: " +
                                    promoVendorNumber + '\n';
                            }
                            if (!por.Imported_to_QB__c && 
                                thisVendorNumber.Length > 0 &&
                                thisVendorNumber != promoVendorNumber &&
                                eqbpo.ContainsKey(thisVendorNumber))
                            {
                                int l = thisVendorNumber.Length > 10 ?
                                    10 : thisVendorNumber.Length;
                                thisVendorNumber = thisVendorNumber.Substring(0, l);
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
                            if(por.Imported_to_QB__c && 
                                thisVendorNumber.Length > 0 &&
                                thisVendorNumber != promoVendorNumber &&
                                eqbpo.ContainsKey(thisVendorNumber))
                            {
                                res += "Existing bill: " + 
                                    thisVendorNumber + '\n';
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
                                if (!por.Imported_to_QB__c)
                                {
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
                                else
                                {
                                    res += "Existing bill: " + billNo + '\n';
                                }
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
                            sw.WriteLineAsync(r.StatusMessage + '\n');
                        }
                    }
                }
            } catch(Exception exce)
            {
                Console.WriteLine(exce.Message);
                if (sw != null)
                {
                    sw.WriteLineAsync(exce.Message + '\n');
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
                            sw.WriteLineAsync(rsp.ResponseList
                                .GetAt(i).StatusMessage);
                        }
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
                if (sw != null)
                {
                    sw.WriteLineAsync(e.Message + '\n');
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
                Response.WriteAsync("No records during the last day");
            }
            // POST data to SF
            PostPOtoSF.postData(displayResp);
            //nullify all stuff
            ApiCall.porLst = null;
            res = null;
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