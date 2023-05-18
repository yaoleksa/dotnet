@RestResource(urlMapping='/pojson')
global class PurchaseOrderJSONAPI {
    public static boolean disabled = false;
    @HttpGet
    global static void getPurchaseOrder(){
        restContext.response.addHeader('Content-Type', 'application/json;charset=UTF-8');
        date td = date.today().addDays(-10);
        if(restContext.request.headers.get('X-API-Key') != 'YOUR API KEY'){
            restContext.response.responseBody = Blob.valueOf('API key is missed');
        }
        restContext.response.responseBody = Blob.valueOf(JSON.serialize([select name,
                      deal_type__c, 
                      client__r.name,
                      client__r.firstName,
                      client__r.lastName,
                      client__r.birch_lm_velocify_id__c,
                      Manager_Review_Started_On__c, 
                	  Promo_Coin_Shipping_Loction__c,
                      (select coin_metal_type__c,
                       Total_Price_Formula__c,
                       BG_Total_Cost__c,
                       vendor__r.name,
                       Vendor_Order_Number__c, 
                       Promo_coin__c
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
                       Total_Cost__c, 
                       Imported_to_QB__c
                       from purchase_order__c
                     where (recordType.name = 'Birch PO' or 
                            recordType.name = 'Buy Back') and 
                      deal_type__c != null and
                     client__r.name != null and
                      All_payments_inserted__c = true
                      and Manager_Review_Started_On__c >= :td and
                		date_cleared__c >= :td and
                		date_cleared__c != null and
                		manager_review_started_on__c != null and
                     (deal_type__c like '%IRA%' or 
                      deal_type__c like '%Cash%')]));
    }
    @httpPost
    global static string getQuickBookRecords(retrievedRecords er){
        if(er == null || er.retrievedList == null){
            return 'no records retrieved';
        }
        list<id> importedIds = new list<id>();
        string res = '';
        for(purchase_order__c rec: er.retrievedList){
            res += rec.id;
            importedIds.add(rec.id);
        }
        list<purchase_order__c> comeBackLst = [select id from purchase_order__c 
                                               where id in: importedIds for update];
        for(purchase_order__c po: comeBackLst){
            po.Imported_to_QB__c = true;
        }
        disabled = true;
        try{
        	update comeBackLst;
        } catch(System.Exception e){
            return 'Error occurred while trying to update records';
        }
        disabled = true;
        return res;
    }
    global class retrievedRecords{
        public list<purchase_order__c> retrievedList { get; set; }
    }
}