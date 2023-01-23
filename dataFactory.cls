public class dataFactory {
    public static void createData(){
        account customer = new account(lastName = 'client', type = 'Customer', 
                                       firstName = 'Me', birch_lm_velocify_id__c = 1217173);
        insert customer;
        account vendor = new account(lastName='Exchange', 
                                     firstName='Bayside', 
                                     middleName='Metal', 
                                     vendor__c = true);
        insert vendor;
        account custodian = new account(lastName = 'Company', 
                                        firstName ='Equity', 
                                        middleName = 'Trust', 
                                        Custodian__c = true);
        insert custodian;
        product2 palladium = new product2(metal_type__c = 'Palladium', name='palladium');
        insert palladium;
    }
    public static void createPO(){
        id birchPO = [select id, name from recordType where name = 'Birch PO' limit 1].id;
        id buyBack = [select id, name from recordType where name = 'Buy Back' limit 1].id;
        account customer = [select id, lastName from account where lastName = 'client' limit 1];
        account vendor = [select id, lastName from account where firstName = 'Bayside' limit 1];
        account custodian = [select id, lastName from account where lastName = 'Company' limit 1];
        product2 palladium = [select id, name from product2 where name = 'palladium' limit 1];
        list<purchase_order__c> poLst = new list<purchase_order__c>();
        purchase_order__c p = null;
        date td = date.today();
        for(integer i = 0; i < 10; i++){
            if(math.mod(i, 2) == 0 && i < 10){
                p = new purchase_order__c(custodian_lookup__c = custodian.id, 
                                          client__c = customer.id, 
                                          Manager_Review_Started_On__c = td,
                                          date_cleared__c = td, 
                                          shipping_start_date__c =td, 
                                          All_payments_inserted__c = true, 
                                          All_outbound_wires_inserted__c = true, 
                                          recordTypeId = buyBack, 
                                          deal_type__c = 'Cash Buyback', 
                                          Vendor_Order__c = 'P-443' + i + '& motive395',
                                          IRA_account__c = 'R-000' + i);
            } else if(math.mod(i, 2) != 0 && i < 10) {
                p = new purchase_order__c(custodian_lookup__c = custodian.id, 
                                          client__c = customer.id, 
                                          Manager_Review_Started_On__c = td,
                                          date_cleared__c = td, 
                                          shipping_start_date__c = td, 
                                          All_payments_inserted__c = true, 
                                          All_outbound_wires_inserted__c = true, 
                                          recordTypeId = buyBack, 
                                          deal_type__c = 'IRA Buyback', 
                                          Vendor_Order__c = 'P - 7715' + i + '# $307', IRA_account__c = 'R-9685' + i);
            }
            poLst.add(p);
        }
        insert poLst;
        list<purchase_order_line_item__c> poliLst = new list<purchase_order_line_item__c>();
        purchase_order_line_item__c poli = null;
        for(purchase_order__c p_o: poLst){
            poli = new purchase_order_line_item__c(quantity__c = 1, 
                                                   Unit_Price__c = 5, 
                                                   coin__c = palladium.id, 
                                                   purchase_order__c = p_o.id, 
                                                   bg_unit_cost__c = 1, 
                                                   Vendor__c = vendor.id);
            poliLst.add(poli);
        }
        insert poliLst;
        list<outbound_wire__c> owLst = new list<outbound_wire__c>();
        for(purchase_order__c po: poLst){
            for(integer i = 0; i < 3; i++){
                outbound_wire__c ow = new outbound_wire__c();
                ow.Client__c = customer.id;
                ow.Clear_Date__c = td;
                ow.Payment_Type__c = math.mod(i, 2) == 0 ? 'Bank Wire' : 'Check';
                ow.Amount__c = 7.35;
                ow.Purchase_Order__c = po.id;
                owLst.add(ow);
            }
        }
        insert owLst;
    }
}