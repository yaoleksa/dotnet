@RestResource(urlMapping='/pojson')
global class PurchaseOrderJSONAPI {
    @HttpGet
    global static list<purchase_order__c> getPurchaseOrder(){
        date td = date.today().addDays(-5);
        if(restContext.request.headers.get('X-API-Key') != 'YOUR API KEY'){
            return null;
        }
        return [select name,
                      deal_type__c, 
                      client__r.name,
                      client__r.firstName,
                      client__r.lastName,
                      client__r.birch_lm_velocify_id__c,
                      Manager_Review_Started_On__c,
                      (select coin_metal_type__c,
                       Total_Price_Formula__c,
                       BG_Total_Cost__c,
                       vendor__r.name,
                       Vendor_Order_Number__c
                       from Purchase_Order_Line_Items__r),
                       (select id, 
                        client__r.Name,
                        client__r.FirstName,
                        client__r.LastName,
                        client__r.Birch_LM_Velocify_ID__c,
                        Clear_Date__c, 
                        Payment_Type__c, 
                        Amount__c 
                        from outbound_wire__r),
                       vendor_order__c,
                       Total_Amount_Paid__c,
                       total_price__c,
                       date_cleared__c,
                       shipping_start_date__c,
                       All_payments_inserted__c,
                       All_outbound_wires_inserted__c,
                       recordType.name,
                       Custodian_lookup__r.Name,
                       IRA_Account__c,
                       Total_Cost__c
                       from purchase_order__c
                     where (recordType.name = 'Birch PO' or 
                            recordType.name = 'Buy Back') and 
                      deal_type__c != null and
                     client__r.name != null and
                      All_payments_inserted__c = true
                      and Manager_Review_Started_On__c >= :td and
                		manager_review_started_on__c != null and
                     (deal_type__c like '%IRA%' or 
                      deal_type__c like '%Cash%')];
    }
    @httpPost
    global static string getQuickBookRecords(retrievedRecords er){
        if(er == null || er.retrievedDictionary == null|| er.retrievedDictionary.size() == 0){
            map<string, quickBookRecord> mapRecords = new map<string, quickBookRecord>();
            quickBookRecord qbRecord = new quickBookRecord();
            qbRecord.qbId = 'no records retrieved';
            mapRecords.put('key', qbRecord);
            er = new retrievedRecords();
            er.retrievedDictionary = mapRecords;
        }
        return 'Records were retrieved by Salesforce org' + 
            ' ' + 
            er.retrievedDictionary.values()[0].qbid;
    }
    global class retrievedRecords{
        public map<string, quickBookRecord> retrievedDictionary { get; set;}
        public map<string, invoice> retrievedInvoice { get; set;}
    }
    global class quickBookRecord{
        public string qbId { get; set;}
        public string editSequence { get; set;}
        public double amount { get; set;}
        public string num { get; set;}
        public map<string, lineItems> salesReceiptLineItems { get; set;}
    }
    global class lineItems{
        public string id { get; set;}
        public string item { get; set;}
    }
    global class invoice{
        public string num { get; set; }
        public string txnCustomer { get; set; }
        public string txnId { get; set; }
        public DateTime txnDate { get; set; }
        public string editSequence { get; set; }
        public map<string, lineItems> invoiceLineItems { get; set; }
    }
}