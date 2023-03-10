@isTest
private class Test_PurchaseOrderJSONAPI {
    @testSetup
    private static void createTestRecords(){
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
    private static testMethod void testGetPurchaseOrderWithKey(){
        RestRequest req = new RestRequest();
        RestResponse res = new RestResponse();
        req.requestURI = 'YOUR ORG URL INSTANCE' + 
            'PATH TO API';
        req.httpMethod = 'GET';
        req.addHeader('X-API-Key', 'YOUR API KEY');
        RestContext.request = req;
        RestContext.response = res;
        Test.startTest();
        	PurchaseOrderJSONAPI.getPurchaseOrder();
        Test.stopTest();
    }
    private static testMethod void testGetPurchaseOrderWithoutKey(){
        RestRequest req = new RestRequest();
        RestResponse res = new RestResponse();
        req.requestURI = 'YOUR ORG URL INSTANCE' + 
            'PATH TO API';
        req.httpMethod = 'GET';
        RestContext.request = req;
        RestContext.response = res;
        Test.startTest();
        	PurchaseOrderJSONAPI.getPurchaseOrder();
        Test.stopTest();
    }
    private static testMethod void testPostRequest(){
        RestRequest req = new restRequest();
        RestResponse res = new RestResponse();
        req.requestURI = 'YOUR ORG URL INSTANCE' + 
            'PATH TO API';
        req.httpMethod = 'POST';
        RestContext.request = req;
        RestContext.response = res;
        Test.startTest();
        	PurchaseOrderJSONAPI.retrievedRecords request = 
                new PurchaseOrderJSONAPI.retrievedRecords();
        	PurchaseOrderJSONAPI.retrievedRecords rr = new PurchaseOrderJSONAPI.retrievedRecords();
        	rr.retrievedList = [select id from purchase_order__c];
        	PurchaseOrderJSONAPI.getQuickBookRecords(rr);
        Test.stopTest();
    }
}