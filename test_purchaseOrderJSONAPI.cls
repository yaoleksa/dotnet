@isTest
public class Test_PurchaseOrderJSONAPI {
    @testSetup
    public static void createTestRecords(){
        id propperAccId = [select id, name from recordType where name = 'Person Account'].id;
        insert new account(firstName='Michel', 
                           lastName='Carleone', 
                           recordTypeId = propperAccId);
        insert new purchase_order__c(deal_type__c = 'IRA', 
                                     Shipping_Start_Date__c = system.today(),
                                     client__c = [select id, name from account limit 1].id, 
                                     IRA_Specialist__c = 'NVU', 
                                     IRA_Fee_s_Covered__c = '1 year');
    }
	@isTest
    public static void testGetPurchaseOrder(){
        RestRequest req = new RestRequest();
        RestResponse res = new RestResponse();
        req.requestURI = 'https://ccdevs-birchgoldgroup.cs66.force.com' + 
            '/webconnector/services/apexrest/pojson';
        req.httpMethod = 'GET';
        RestContext.request = req;
        RestContext.response = res;
        Test.startTest();
        	PurchaseOrderJSONAPI.getPurchaseOrder();
        Test.stopTest();
    }
    @isTest
    public static void testPostRequest(){
        RestRequest req = new restRequest();
        RestResponse res = new RestResponse();
        req.requestURI = 'https://ccdevs-birchgoldgroup.cs66.force.com' + 
            '/webconnector/services/apexrest/pojson';
        req.httpMethod = 'POST';
        RestContext.request = req;
        RestContext.response = res;
        Test.startTest();
        	PurchaseOrderJSONAPI.retrievedRecords request = 
                new PurchaseOrderJSONAPI.retrievedRecords();
        	PurchaseOrderJSONAPI.quickBookRecord record = 
                new PurchaseOrderJSONAPI.quickBookRecord();
        	PurchaseOrderJSONAPI.lineItems lis = 
                new PurchaseOrderJSONAPI.lineItems();
        	lis.id = 'this_is_id';
        	lis.item = 'this_is_item';
        	map<string, PurchaseOrderJSONAPI.lineItems> lisMap = 
                new map<string, PurchaseOrderJSONAPI.lineItems>();
        lisMap.put('id', lis);
        	record.salesReceiptLineItems = lisMap;
        	record.qbId = 'qbId';
        	record.editSequence = 'seq';
        	record.amount = 25.13;
        	record.num = 'that';
        	map<string, PurchaseOrderJSONAPI.quickBookRecord> retrievedRecords = 
                new map<string, PurchaseOrderJSONAPI.quickBookRecord>();
        	retrievedRecords.put('id', record);
        	request.retrievedDictionary = retrievedRecords;
        	PurchaseOrderJSONAPI.getQuickBookRecords(request);
        	PurchaseOrderJSONAPI.getQuickBookRecords(null);
        	PurchaseOrderJSONAPI.invoice inv = new PurchaseOrderJSONAPI.invoice();
        	inv.num = '?';
        	inv.txnCustomer = '?';
        	inv.txnId = '?';
        	inv.txnDate = system.now();
        	inv.editSequence = '?';
        	inv.invoiceLineItems = lisMap;
        	request.retrievedInvoice = new map<string, PurchaseOrderJSONAPI.invoice>();
        	request.retrievedInvoice.put('?', inv);
        Test.stopTest();
    }
}