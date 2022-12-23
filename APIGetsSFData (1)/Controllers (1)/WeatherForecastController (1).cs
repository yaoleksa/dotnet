using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using Interop.QBFC15;
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
            string mode = "test";
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
                poNumLst.Add(record.Name.Split('-')[1].Trim());
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
                            this.thisVendorNumber = poitem
                                .Vendor_Order_Number__c
                                .Split(',')[0].Trim()
                                .Split('&')[0].Trim()
                                .Split('-')[1].Trim()
                                .Split(' ')[0].Trim();
                            this.thisVendorNumber = Regex
                                .Replace(this.thisVendorNumber,
                                "[^0-9]", String.Empty);
                            poNumLst.Add(this.thisVendorNumber);
                        }
                    }
                }
            }
            // List is ready
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
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
            sessionManager.BeginSession("", ENOpenMode.omDontCare);
            ///////////////////////////////////////////
            // Query Sales Receipt from QB DB ##############################
            IMsgSetRequest querySRQB = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            querySRQB.Attributes.OnError = ENRqOnError.roeContinue;
            GetSalesRecepietQB.BuildSalesRecepietRq(querySRQB, poNumLst);
            IMsgSetResponse querySRrecords = sessionManager
                .DoRequests(querySRQB);
            Dictionary<string, salesReceipt> exSrQb = 
                new Dictionary<string, salesReceipt>();
            List<string> existingSRArr = new List<string>();
                GetSalesRecepietQB
                .parseResponse(querySRrecords, exSrQb, poNumLst, 
                existingSRArr);
            // Query Invoices QB !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            IMsgSetRequest queryInvc = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            queryInvc.Attributes.OnError = ENRqOnError.roeContinue;
            // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            // Query Purchase Order from QB
            IMsgSetRequest queryPurchaseOrder = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            queryPurchaseOrder.Attributes.OnError = ENRqOnError.roeContinue;
            PurchaseOrderQBQuery.BuildPurchaseOrderQuery(queryPurchaseOrder, 
                DateTime.Today.AddDays(-2));
            IMsgSetResponse doneQuery = sessionManager
                .DoRequests(queryPurchaseOrder);
            Dictionary<string, List<string>> eqbpo = 
                new Dictionary<string, List<string>>();
            PurchaseOrderQBQuery.handleRespons(doneQuery, eqbpo);
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // Query Bill from QB
            IMsgSetRequest queryBill = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            queryBill.Attributes.OnError = ENRqOnError.roeContinue;
            GetBillQB.BuildBillQuery(queryBill, DateTime.Today.AddDays(-2));
            IMsgSetResponse recievedBills = sessionManager.DoRequests(queryBill);
            Dictionary<string, bill> exBills = new Dictionary<string, bill>();
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
            List<string> existingCustomers = new List<string>();
            IMsgSetResponse getCustomersResponse = sessionManager
                .DoRequests(getCustomers);
            GetQBCustomers.handleResponse(getCustomersResponse, existingCustomers);
            // %%%%%%%%%%%%------------$$$$$$$$$$$$$$$$$$------@@@@@@@@@@@
            //131311313131313131313131313131313131311313131313131311313131
            GetQBInvoices.queryQBInvoices(queryInvc, poNumLst);
            IMsgSetResponse invResp = sessionManager
                .DoRequests(queryInvc);
            Dictionary<string, invoice> exRec = 
                new Dictionary<string, invoice>();
            exRec = GetQBInvoices
                .handleResponse(invResp, poNumLst);
            string qbInv = GetQBInvoices.QBInvoices;
            string[] existingInv = qbInv.Split("\n",
                StringSplitOptions.RemoveEmptyEntries);
            ////// Create bill insert query
            IMsgSetRequest queryBillCreate = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            queryBillCreate.Attributes.OnError = ENRqOnError.roeContinue;
            ///*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*--*-*--*-*-*-*-*-*-*-*-*-*-*-*-*
            /// Get vendors from QB
            IMsgSetRequest queryVendors = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            GetQBVendors.buildRequest(queryVendors);
            IMsgSetResponse responseVendors = sessionManager.DoRequests(queryVendors);
            List<string> qbVenodors = new List<string>();
            GetQBVendors.handleResult(responseVendors, qbVenodors);
            /// |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
            /// Update customer
            List<string> cNames = new List<string>();
            /// =*=**=*=*=*=**=**=*=***=*=*=**=**=**=*=*=***=**=***=**=*=*=***===
            /// Add check or not *****
            IMsgSetRequest addCheckRq = sessionManager
                .CreateMsgSetRequest("US", 15, 0);
            addCheckRq.Attributes.OnError = ENRqOnError.roeContinue;
            /// (.)(.)(.)(.)(.)(.)(.)(.)(.)(.)(.)(.)(.)
            string res = "";
            // Insert all necessary records ***=============================***
            res += " Existing sales receipt " + existingSRArr
                .Count.ToString() + '\n';
            res += "Existing invoice " + existingInv
                .Length.ToString() + '\n';
            foreach (purchaseOrderRecord po in displayResp)
            {
                customer c = null;
                customer wireCustomer = null;
                poLineItems li = null;
                purchaseOrderItem poItem = null;
                string thisCustomerName = "";
                string wireCustomerName = "";
                string thisVendorNumber = "";
                string thisVendorName = "";
                string customerJob = "";
                string custodian = "";
                string extendedCustomerName = "";
                string extendedWireName = "";
                Dictionary<string, double> poItems = new 
                    Dictionary<string, double>();
                Dictionary<string, double> bgCost = new 
                    Dictionary<string, double>();
                List<purchaseOrderItem> metalItems = new
                    List<purchaseOrderItem>();
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
                    sfCustodian = JsonSerializer.Deserialize<recordType>(po.Custodian_lookup__r);
                }
                if (po.Purchase_Order_Line_Items__r != null &&
                     po.Deal_Type__c.Contains("IRA") && 
                     !po.Deal_Type__c.Contains("Cash"))
                {
                    li = JsonSerializer
                        .Deserialize<poLineItems>(
                        po.Purchase_Order_Line_Items__r);
                    foreach (JsonDocument item in li.records)
                    {
                        poItem = JsonSerializer
                            .Deserialize<purchaseOrderItem>(item);
                        if (poItem.Coin_Metal_Type__c != null)
                        {
                            poItems.TryAdd(poItem
                                .Coin_Metal_Type__c, 0.0);
                            bgCost.TryAdd(poItem.Coin_Metal_Type__c, 0.0);
                            metalItems.Add(poItem);
                            AddNonInventoryItems
                                .BuildItemNonInventoryAddRq(
                                requestMetalAdd,
                                poItem.Coin_Metal_Type__c);
                        }
                    }
                }
                
                else if (po.Purchase_Order_Line_Items__r != null &&
                    po.Deal_Type__c.Contains("Cash"))
                {
                    li = JsonSerializer
                        .Deserialize<poLineItems>(
                        po.Purchase_Order_Line_Items__r);
                    foreach(JsonDocument item in li.records)
                    {
                        poItem = JsonSerializer
                            .Deserialize<purchaseOrderItem>(item);
                        if(poItem.Coin_Metal_Type__c != null)
                        {
                            poItems.TryAdd(
                                poItem.Coin_Metal_Type__c, 0.0);
                            bgCost.TryAdd(poItem.Coin_Metal_Type__c, 0.0);
                            metalItems.Add(poItem);
                            AddNonInventoryItems
                                .BuildItemNonInventoryAddRq(
                                requestMetalAdd,
                                poItem.Coin_Metal_Type__c);
                        }
                    }
                }

                thisVendorNumber = po.Vendor_Order__c
                            .Split(',')[0].Trim()
                            .Split('&')[0].Trim()
                            .Split('-')[1].Trim()
                            .Split(' ')[0].Trim();
                thisVendorNumber = Regex
                    .Replace(thisVendorNumber,
                    "[^0-9]",
                    String.Empty);
                if (metalItems.Count > 0 && metalItems[0].Vendor__r != null && metalItems[0].Vendor__r.Name != null)
                {
                    thisVendorName = metalItems[0].Vendor__r.Name.Split(' ')[0].ToLower();
                }
                foreach (string cn in existingCustomers)
                {
                    if (cn.ToLower().Contains(thisVendorName))
                    {
                        customerJob = cn;
                        break;
                    }
                }
                extendedCustomerName = thisCustomerName + " B-" +
                            thisVendorNumber;
                
                if (po.Deal_Type__c.Contains("IRA") &&
                    !po.Deal_Type__c.Contains("Cash") &&
                    !existingSRArr
                    .Contains(po.Name.Split('-')[1].Trim()) && 
                    !existingSRArr.Contains(thisVendorNumber))
                {
                    //TODO/////////////////////////////////////?????????????
                    foreach (purchaseOrderItem metal in metalItems)
                    {
                        poItems[metal.Coin_Metal_Type__c] +=
                            metal.Total_Price_Formula__c;
                        bgCost[metal.Coin_Metal_Type__c] +=
                            metal.BG_Total_Cost__c;
                    }
                    
                    if (rt.Name == "Birch PO")
                    {
                        res += "New sales receipt: " + po.Name.Split('-')[1] + '\n';
                        AddSalesReceipt
                            .BuildSalesRecepietAddRq(srReq,
                            thisCustomerName,
                            poItems,
                            po.Name,
                            po.Date_Cleared__c, 
                            null, 
                            0.8);
                    }
                    else
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
                    existingInv != null &&
                    !existingInv
                    .Contains(po.Name.Split('-')[1].Trim()) &&
                    !existingInv.Contains(thisVendorNumber) && 
                    rt.Name == "Birch PO")
                {
                    foreach (purchaseOrderItem metal in metalItems)
                    {
                        poItems[metal.Coin_Metal_Type__c] +=
                            metal.Total_Price_Formula__c;
                        bgCost[metal.Coin_Metal_Type__c] += 
                            metal.BG_Total_Cost__c;
                    }
                    res += "New invoice: " + po.Name.Split('-')[1] + '\n';

                    AddInvoiceQB.BuildInvoiceAddRq(invRq,
                    thisCustomerName,
                    poItems,
                    po.Name,
                    po.Date_Cleared__c, 
                    null, 
                    0.7);
                }
                else if (po.Deal_Type__c.Contains("Cash") &&
                    rt.Name == "Buy Back" &&
                    existingInv != null &&
                    !existingInv.Contains(thisVendorNumber) &&
                    !existingInv
                    .Contains(po.Name.Split('-')[1].Trim()))
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
                        extendedWireName = wireCustomerName + System
                            .Convert.ToString(wireCustomer
                            .Birch_LM_Velocify_ID__c);
                        AddCheck.BuildAddCheckQuery(addCheckRq, 
                            wireCustomerName, 
                            currentWire.Payment_Type__c, 
                            extendedWireName,
                            currentWire.Amount__c);
                    }
                }
                else if (po.Deal_Type__c.Contains("IRA") && 
                    !po.Deal_Type__c.Contains("Cash") &&
                    (existingSRArr
                    .Contains(po.Name.Split('-')[1].Trim()) || 
                    existingSRArr.Contains(thisVendorNumber)))
                {
                    foreach (purchaseOrderItem metal in metalItems)
                    {
                        poItems[metal.Coin_Metal_Type__c] +=
                            metal.Total_Price_Formula__c;
                        bgCost[metal.Coin_Metal_Type__c] += 
                            metal.BG_Total_Cost__c;
                    }
                    salesReceipt value = new salesReceipt();
                    exSrQb
                        .TryGetValue(po.Name.Split('-')[1].Trim(), 
                        out value);
                    if (rt.Name == "Birch PO")
                    {
                        if (value == null)
                        {
                            continue;
                        }
                        res += "Existing sales receipt: " +
                            po.Name.Split('-')[1] + '\n';

                        UpdateSalesReceipt.SalesReceiptMod(updateSalesReceipt,
                            thisCustomerName,
                            poItems,
                            value,
                            po.Name,
                            po.Date_Cleared__c,
                            value.qbId,
                            value.editSequence);
                    } else
                    {
                        res += "Existing sales receipt: " + thisVendorNumber + '\n';
                    }
                }
                else if (po.Deal_Type__c.Contains("Cash") &&
                    existingInv != null &&
                    (existingInv
                    .Contains(po.Name.Split('-')[1].Trim()) || 
                    existingInv.Contains(thisVendorNumber)))
                {
                    foreach (purchaseOrderItem metal in metalItems)
                    {
                        poItems[metal.Coin_Metal_Type__c] +=
                            metal.Total_Price_Formula__c;
                        bgCost[metal.Coin_Metal_Type__c] += 
                            metal.BG_Total_Cost__c;
                    }
                    invoice value = new invoice();
                    exRec.TryGetValue(po.Name.Split('-')[1].Trim(), 
                        out value);
                    if (rt.Name == "Birch PO")
                    {
                        if (value == null)
                        {
                            continue;
                        }
                    
                        res += "Existing invoice: " + po.Name.Split('-')[1] + '\n';
                        UpdateInvoice.BuildUpdateReq(
                            updateInvoiceQB,
                            thisCustomerName,
                            poItems,
                            value.lineItems,
                            po.Name,
                            po.Date_Cleared__c,
                            value.txnId,
                            value.editSequence);
                    } else
                    {
                        res += "Existing invoice: " + thisVendorNumber + '\n';
                    }
                }
                
                if (po.All_outbound_wires_inserted__c &&
                    !eqbpo
                    .ContainsKey(thisVendorNumber))
                {
                    Dictionary<string, List<string>> vendorNames = 
                        new Dictionary<string, List<string>>();
                    string lastVendor = "";
                    foreach(purchaseOrderItem poi in metalItems)
                    {
                        if(poi.Vendor__r != null && 
                            !vendorNames.ContainsKey(poi.Vendor__r.Name))
                        {
                            AddVendor.BuildAddVendorRequest(
                                requestVendorAdd,
                                poi.Vendor__r.Name);
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
                        else if(poi != null &&
                            poi.Vendor__r == null &&
                            lastVendor != "" &&
                            !vendorNames[lastVendor]
                            .Contains(poi.Coin_Metal_Type__c))
                        {
                            vendorNames[lastVendor]
                                .Add(poi.Coin_Metal_Type__c);
                        }
                    }
                    foreach(
                        KeyValuePair<string, 
                        List<string>> kvp in vendorNames)
                    {
                        AddPurchaseOrderQB.BuildPurchaseOrderAddRq(
                            addPurchaseOrder,
                            po.Name,
                            thisVendorNumber,
                            kvp.Key,
                            thisCustomerName,
                            po.Date_Cleared__c,
                            bgCost, 
                            kvp.Value);
                    }
                }
                if(po.All_outbound_wires_inserted__c &&
                    po.IRA_Account__c != null &&
                    eqbpo.ContainsKey(thisVendorNumber) &&
                    !exBills.ContainsKey(thisVendorNumber) && 
                    !exBills.ContainsKey(po.IRA_Account__c))
                {
                    Dictionary<string, List<string>> vendorNames =
                        new Dictionary<string, List<string>>();
                    string lastVendor = "";
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
                            AddBill.BuildBillAddRq(queryBillCreate,
                                kvp.Key,
                                po.Date_Cleared__c,
                                po.Name,
                                thisVendorNumber,
                                eqbpo, 
                                0.0, 
                                "", 
                                "");
                        } else
                        {
                            foreach(string cust in qbVenodors)
                            {
                                if (sfCustodian != null && 
                                    cust.Contains(sfCustodian.Name.Split(' ')[0]))
                                {
                                    custodian = cust;
                                }
                            }
                            
                            AddBill.BuildBillAddRq(queryBillCreate,
                                custodian,
                                po.Date_Cleared__c,
                                po.Name,
                                po.IRA_Account__c,
                                null, 
                                po.Total_Cost__c, 
                                thisCustomerName, 
                                extendedCustomerName);
                        }
                    }
                }
            }
            //***************************************************************
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
                        //Console.WriteLine(rspk.StatusCode);
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed create customer: " + e.Message);
            }
            try
            {
                sessionManager.DoRequests(requestVendorAdd);
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                        //Console.WriteLine(rsp.StatusMessage);
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed create items: " + e.Message);
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
                        break;
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed create sales receipt: " + e.Message);
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
                        Console.WriteLine("query inv " + rsp.StatusMessage);
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed create invoice: " + e.Message);
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
                        Console.WriteLine("update sr " + resp.StatusMessage);
                        break;
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Failed update sales receipt: " + e.Message);
            }
            try
            {
                sessionManager.DoRequests(updateInvoiceQB);
            } catch(Exception e)
            {
                //Console.WriteLine("Failed update invoice: " + e.Message);
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
                        /*Console.WriteLine("purchase order");
                        Console.Write(resp.StatusMessage);*/
                    }
                }
            } catch(Exception ex) 
            { 
                Console.WriteLine(ex.Message);
            }
            try
            {
                IMsgSetResponse rsp = sessionManager.DoRequests(queryBillCreate);
                IResponseList rlst = rsp.ResponseList;
                if(rlst != null)
                {
                    for(int i = 0; i < rlst.Count; i++)
                    {
                        IResponse r = rlst.GetAt(i);
                        Console.WriteLine("billcreate " + r.StatusMessage);
                    }
                }
            } catch(Exception exce)
            {
                Console.WriteLine(exce.Message);
            }
            try
            {
                IMsgSetResponse rsp = sessionManager.DoRequests(addCheckRq);
                if(rsp.ResponseList != null)
                {
                    for(int i = 0; i < rsp.ResponseList.Count; i++)
                    {
                        Console.WriteLine("Check situation" + rsp.ResponseList.GetAt(i).StatusMessage);
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //Start work with Purchase Order
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
            PostPOtoSF.postData(exSrQb, exRec);
            //nullify all stuff
            existingSRArr = null;
            ApiCall.porLst = null;
            exSrQb = null;
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
            string display = System.IO.File
                .ReadAllText(@"C:\access\simple.txt");
            using(
                StreamWriter sw = System
                .IO
                .File
                .AppendText(@"C:\access\simple.txt"))
            {
                sw.WriteLine("success");
            }
            Response.WriteAsync("Created by Oleksii Yaremchuk" + 
                lst[0] +
                display);
        }
    }
}