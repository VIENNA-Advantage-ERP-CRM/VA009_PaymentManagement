﻿/** 
  *    Sample Class for Callout
       MPC--> refers to Module Prefix Code
  */
/*Module Name space initialization*/
; VA009 = window.VA009 || {};

; (function (VA009, $) {

    var Level = VIS.Logging.Level;
    var Util = VIS.Utility.Util;

    //1
    /* Sample Start */


    /**
    *  Callout Class
      -   must call this function
             VIS.CalloutEngine.call(this, [className]);
    */
    function VA009_CalloutPayment() {
        VIS.CalloutEngine.call(this, "VA009.VA009_CalloutPayment"); // must call base class (CalloutEngine)
    };
    /**
     * Inherit CalloutEngine Class 
     *VIS.Utility.inheritPrototype([Callout class Name], VIS.CalloutEngine)
     */
    VIS.Utility.inheritPrototype(VA009_CalloutPayment, VIS.CalloutEngine);//must inheirt Base class CalloutEngine


    /**
     *  Callout function
     */
    VA009_CalloutPayment.prototype.setPMethod = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null) {
            return "";
        }
        this.setCalloutActive(true);
        var _paySchedule = mTab.getValue("C_InvoicePaySchedule_ID");
        var _Client = mTab.getValue("AD_Client_ID");
        var paramString = _paySchedule.toString() + "," + _Client.toString();
        var dr = VIS.dataContext.getJSONRecord("Pay/GetInvPaymentMethod", paramString);
        //var _sql = "select IP.VA009_PAYMENTMETHOD_ID, IP.VA009_EXECUTIONSTATUS,IP.DISCOUNTAMT,IP.DUEDATE,IP.VA009_PLANNEDDUEDATE,IP.DUEAMT,PM.VA009_PAYMENTBASETYPE from C_INVOICEPAYSCHEDULE IP inner join VA009_PAYMENTMETHOD PM on PM.VA009_PAYMENTMETHOD_ID = IP.VA009_PAYMENTMETHOD_ID  where IP.C_INVOICEPAYSCHEDULE_ID=" + _paySchedule + " AND  IP.IsActive = 'Y' AND IP.AD_Client_ID = " + _Client;
        //var ds = VIS.DB.executeDataSet(_sql);
        if (dr != null) {

            mTab.setValue("VA009_PaymentMethod_id", dr["VA009_PaymentMethod_ID"]);

            if (dr["VA009_PaymentBaseType"] == "D") {
                mTab.setValue("TenderType", "D");
            }
            else if (dr["VA009_PaymentBaseType"] == "K") {
                mTab.setValue("TenderType", "C");
            }
            else if (dr["VA009_PaymentBaseType"] == "S") {
                mTab.setValue("TenderType", "K");
            }
            else if (dr["VA009_PaymentBaseType"] == "T") {
                mTab.setValue("TenderType", "A");
            }
            else if (dr["VA009_PaymentBaseType"] == "L") {
                mTab.setValue("TenderType", "L");
            }
            else {
                mTab.setValue("TenderType", "A");
            }
            mTab.setValue("VA009_ExecutionStatus", dr["VA009_ExecutionStatus"]);
            //mTab.setValue("DISCOUNTAMT", dr["DiscountAmt"]);
            //mTab.setValue("PayAmt", dr["DueAmt"]);
        }

        //mTab.setValue("Description", value);
        this.setCalloutActive(false);

        return "";

    };

    /**
   * Callout function for Payment window and Allocate tab to set PaymentAmount,PayAmt and InvoiceAmt,Amount
   * @param {any} ctx
   * @param {any} windowNo
   * @param {any} mTab
   * @param {any} mField
   * @param {any} value
   * @param {any} oldValue
   */
    VA009_CalloutPayment.prototype.setPayAmt = function (ctx, windowNo, mTab, mField, value, oldValue) {

        if (this.isCalloutActive() || value == null || value.toString() == "") {
            return "";
        }
        try {
            var AcountDate = ctx.getContext(windowNo, "DateAcct");//get AcountDate from Payment window
            var C_Currency_ID = ctx.getContextAsInt(windowNo, "C_Currency_ID");//get C_Currency_ID from Payment window
            var AD_Client_ID = mTab.getValue("AD_Client_ID");//get AD_Client_ID
            var AD_Org_ID = mTab.getValue("AD_Org_ID");//get AD_Org_ID
            var C_ConversionType_ID = ctx.getContextAsInt(windowNo, "C_ConversionType_ID");//get C_Currency_ID from Payment window
            var docTypeId = ctx.getContextAsInt(windowNo, "C_DocType_ID");
            this.setCalloutActive(true);
            var paramString = value.toString() + "," + docTypeId + "," + C_Currency_ID + "," + AcountDate + "," + AD_Client_ID
                + "," + AD_Org_ID + "," + C_ConversionType_ID;

            var dr = VIS.dataContext.getJSONRecord("Pay/GetjournalDetail", paramString);
            if (dr != null) {

                var docbaseType = Util.getValueOfString(dr["docbaseType"]);
                var AccountType = Util.getValueOfString(dr["AccountType"]);

                // In case of APP Selected Accounttype not Liability then show the message 
                //In case of ARR Selected Accounttype not Asset then show the message 
                if ("APP" == docbaseType && AccountType != "L" || "ARR" == docbaseType && AccountType != "A") {
                    VIS.ADialog.info("VIS_SlctedAccountType");
                    this.setCalloutActive(false);
                    return "";
                }
                var credit = 0;
                if (Util.getValueOfDecimal(dr["AmtSourceCr"]) == 0) {
                    credit = Util.getValueOfDecimal(dr["AmtSourceDr"]);
                }
                else {
                    credit = Util.getValueOfDecimal(dr["AmtSourceCr"])
                }

                // On GL Journal line selected accounttype is Liability /Asset and Amount source is Debited / Credited  then Amount sign will be negative
                if ((AccountType == "L" && credit == Util.getValueOfDecimal(dr["AmtSourceDr"])) ||
                    (AccountType == "A" && credit == Util.getValueOfDecimal(dr["AmtSourceCr"]))) {
                    credit = -1 * credit;
                }
                if (mTab.getField("C_BankAccount_ID") != null) {
                    mTab.setValue("PayAmt", credit); // Set PayAmt and PaymentAmount
                    mTab.setValue("PaymentAmount", credit);
                }
                else if (mTab.getField("C_BankAccount_ID") == null) {                  
                    mTab.setValue("Amount", credit);// Set Amount

                }
            }
        }
        catch (err) {
            this.setCalloutActive(false);
            return err;
        }
        this.setCalloutActive(false);
        return "";
    };

    //Callout Added By Vivek on 20/06/2016 for Advance payment on order Schedules

    VA009_CalloutPayment.prototype.setScheduleAmount = function (ctx, windowNo, mTab, mField, value, oldValue) {
        //if (this.isCalloutActive() || value == null) {
        //    return "";
        //}

        // olny for Advance payment on order Schedules otherwise not execute this functio
        var isAdvOrder = VIS.dataContext.getJSONRecord("Pay/GetIsAdvanceOrder", ctx.getContextAsInt(windowNo, "C_Order_ID"));
        if (isAdvOrder != null && !isAdvOrder) {
            return "";
        }

        if (value == null || value.toString() == "") {
            // mTab.setValue("TenderType", null);
            mTab.setValue("VA009_ExecutionStatus", null);
            mTab.setValue("DiscountAmt", VIS.Env.ZERO);
            mTab.setValue("PayAmt", VIS.Env.ZERO);
            mTab.setValue("WriteOffAmt", VIS.Env.ZERO);
            mTab.setValue("IsOverUnderPayment", false);//Boolean.FALSE);
            mTab.setValue("OverUnderAmt", VIS.Env.ZERO);
            //VA230:Set due amount when VA009_OrderPaySchedule_ID not found
            var dr = VIS.dataContext.getJSONRecord("Pay/GetDueAmt", ctx.getContextAsInt(windowNo, "C_Order_ID"));
            if (dr != null) {
                dueAmount = dr["DueAmt"];
                mTab.setValue("PayAmt", dueAmount);
                mTab.setValue("PaymentAmount", dueAmount);
            }
            return "";
        }
        this.setCalloutActive(true);
        //var _paySchedule = mTab.getValue("VA009_OrderPaySchedule_ID");
        var _Client = mTab.getValue("AD_Client_ID");
        var paramString = value.toString() + "," + _Client.toString();
        var dr = VIS.dataContext.getJSONRecord("Pay/GetOrdPaymentMethod", paramString);
        //var _sql = "select IP.VA009_PAYMENTMETHOD_ID, IP.VA009_EXECUTIONSTATUS,IP.DISCOUNTAMT,IP.DUEDATE,IP.VA009_PLANNEDDUEDATE,IP.DUEAMT,PM.VA009_PAYMENTBASETYPE from VA009_OrderPaySchedule IP inner join VA009_PAYMENTMETHOD PM on PM.VA009_PAYMENTMETHOD_ID = IP.VA009_PAYMENTMETHOD_ID  where IP.VA009_OrderPaySchedule_ID=" + _paySchedule + " AND  IP.IsActive = 'Y' AND IP.AD_Client_ID = " + _Client;
        //var ds = VIS.DB.executeDataSet(_sql);
        if (dr != null) {

            mTab.setValue("VA009_PaymentMethod_id", dr["VA009_PaymentMethod_ID"]);

            if (dr["VA009_PaymentBaseType"] == "D") {
                mTab.setValue("TenderType", "D");
            }
            else if (dr["VA009_PaymentBaseType"] == "K") {
                mTab.setValue("TenderType", "C");
            }
            else if (dr["VA009_PaymentBaseType"] == "S") {
                mTab.setValue("TenderType", "K");
            }
            else if (dr["VA009_PaymentBaseType"] == "T") {
                mTab.setValue("TenderType", "A");
            }
            else if (dr["VA009_PaymentBaseType"] == "L") {
                mTab.setValue("TenderType", "L");
            }
            else {
                mTab.setValue("TenderType", "A");
            }
            mTab.setValue("VA009_ExecutionStatus", dr["VA009_ExecutionStatus"]);
            mTab.setValue("DiscountAmt", VIS.Env.ZERO);
            mTab.setValue("WriteOffAmt", VIS.Env.ZERO);
            mTab.setValue("IsOverUnderPayment", false);//Boolean.FALSE);
            mTab.setValue("OverUnderAmt", VIS.Env.ZERO);
        }


        var VA009_OrderPaySchedule_ID = 0; 
        var C_Order_ID = ctx.getContextAsInt(windowNo, "C_Order_ID")
        //VIS_427 Getting order Schedule id to get accurate amount of that schedule
        VA009_OrderPaySchedule_ID = Util.getValueOfInt(mTab.getValue("VA009_OrderPaySchedule_ID"));
        paramString = C_Order_ID.toString() + "," + VA009_OrderPaySchedule_ID.toString();

        var dueAmount = 0;
        var _chk = 0;
        var dr = VIS.dataContext.getJSONRecord("Pay/GetDueAmt", paramString);
        //var _sqlAmt = "SELECT * FROM   (SELECT ips.VA009_OrderPaySchedule_ID, "
        //+ " ips.DueAmt  FROM C_Order i  INNER JOIN VA009_OrderPaySchedule  ips "
        //+ " ON (i.C_Order_ID        =ips.C_Order_ID)  WHERE ips.isactive          ='Y' "
        //+ " AND i.C_Order_ID    = " + C_Order_ID
        //+ "  AND ips.VA009_OrderPaySchedule_ID NOT IN"
        //+ "(SELECT NVL(VA009_OrderPaySchedule_ID,0) FROM VA009_OrderPaySchedule  WHERE c_payment_id IN"
        //+ "(SELECT NVL(c_payment_id,0) FROM VA009_OrderPaySchedule ) union "
        //+ " SELECT NVL(VA009_OrderPaySchedule_ID,0) FROM VA009_OrderPaySchedule   WHERE c_cashline_id IN"
        //+ "(SELECT NVL(c_cashline_id,0) FROM VA009_OrderPaySchedule  )) "
        //+ " ORDER BY ips.duedate ASC  ) WHERE rownum=1";
        //var drAmt = null;
        //try {
        //    drAmt = VIS.DB.executeReader(_sqlAmt, null, null);
        //    if (drAmt.read()) {
        if (dr != null) {
            VA009_OrderPaySchedule_ID = dr["VA009_OrderPaySchedule_ID"];
            dueAmount = dr["DueAmt"];
            mTab.setValue("PayAmt", dueAmount);
            mTab.setValue("PaymentAmount", dueAmount);
        }

        //  Payment Date
        //var ts = new Date(mTab.getValue("DateTrx"));
        //var tsDate;
        //if (ts == null) {
        //ts = DateTime.Now.Date; //new DateTime(CommonFunctions.CurrentTimeMillis());
        //    ts = new Date();
        //}
        //tsDate = VIS.DB.to_date(ts, true);
        //tsDate = "TO_DATE( '" + ts.getMonth() + "-" + ts.getDate() + "-" + ts.getFullYear() + "', 'MM-DD-YYYY')";
        //
        var IsSoTrx = 'N';
        var IsReturnTrx = 'N';
        var paramstring = C_Order_ID.toString() + "," + mTab.getValue("DateTrx").toString() + "," + VA009_OrderPaySchedule_ID.toString();
        var dr = VIS.dataContext.getJSONRecord("Pay/GetOpenAmt", paramstring);
        //var sql = "SELECT C_BPartner_ID,C_Currency_ID,"
        //    + " Orderopen(C_Order_ID, " + VA009_OrderPaySchedule_ID + ") as orderopen,"
        //    + " Orderdiscount(C_Order_ID," + tsDate + "," + VA009_OrderPaySchedule_ID + ") as OrderDiscount, IsSOTrx "
        //    + "FROM C_Order WHERE C_Order_ID=" + C_Order_ID;
        //var dr = null;

        //try {
        //    dr = VIS.DB.executeReader(sql, null);

        //    if (dr.read()) {
        if (dr != null) {
            mTab.setValue("C_BPartner_ID", dr["C_BPartner_ID"]);//.getInt(1)));
            var C_Currency_ID = dr["C_Currency_ID"];//dr.getInt(2);					//	Set Order Currency
            IsSoTrx = dr["IsSOTrx"];
            IsReturnTrx = dr["IsReturnTrx"];
            mTab.setValue("C_Currency_ID", C_Currency_ID);
            //
            var orderopen = dr["orderopen"];//.getBigDecimal(3);		//	Set Order OPen Amount
            if (orderopen == null) {
                orderopen = VIS.Env.ZERO;
            }
            var discountAmt = dr["OrderDiscount"];//.getBigDecimal(4);		//	Set Discount Amt
            if (discountAmt == null) {
                discountAmt = VIS.Env.ZERO;
            }
            //mTab.setValue("PayAmt", Decimal.Subtract(ORDEROPEN, discountAmt));                
            if (discountAmt != 0) {
                mTab.setValue("PayAmt", (orderopen - discountAmt));
                mTab.setValue("PaymentAmount", (orderopen - discountAmt));
            }
            //mTab.setValue("VA009_OrderPaySchedule_ID", VA009_OrderPaySchedule_ID);//Pratap
            mTab.setValue("DiscountAmt", discountAmt);
            //  reset as dependent fields get reset
            ctx.setContext(windowNo, "C_Order_ID", C_Order_ID.toString());
        }
        var payAmt = Util.getValueOfDecimal(mTab.getValue("PayAmt") == null ? VIS.Env.ZERO : mTab.getValue("PayAmt"));
        var writeOffAmt = Util.getValueOfDecimal(mTab.getValue("WriteOffAmt") == null ? VIS.Env.ZERO : mTab.getValue("WriteOffAmt"));
        var overUnderAmt = Util.getValueOfDecimal((mTab.getValue("OverUnderAmt") == null ? VIS.Env.ZERO : mTab.getValue("OverUnderAmt")));
        var enteredDiscountAmt = Util.getValueOfDecimal((mTab.getValue("DiscountAmt") == null ? VIS.Env.ZERO : mTab.getValue("DiscountAmt")));
        if (IsReturnTrx == "Y") {
            if (payAmt > 0) {
                payAmt = payAmt * -1;
                mTab.setValue("PayAmt", payAmt);
                mTab.setValue("PaymentAmount", payAmt);
            }
            if (enteredDiscountAmt > 0) {
                enteredDiscountAmt = enteredDiscountAmt * -1;
            }
            if (discountAmt > 0) {
                discountAmt = discountAmt * -1;
            }
            if (writeOffAmt > 0) {
                writeOffAmt = writeOffAmt * -1;
                mTab.setValue("WriteOffAmt", writeOffAmt);
            }
            if (overUnderAmt > 0) {
                overUnderAmt = overUnderAmt * -1;
                mTab.setValue("OverUnderAmt", overUnderAmt);
            }
        }
        else {
            if (payAmt < 0) {
                payAmt = payAmt * -1;
                mTab.setValue("PayAmt", payAmt);
                mTab.setValue("PaymentAmount", payAmt);
            }
            if (enteredDiscountAmt < 0) {
                enteredDiscountAmt = enteredDiscountAmt * -1;
            }
            if (discountAmt < 0) {
                discountAmt = discountAmt * -1;
            }
            if (writeOffAmt < 0) {
                writeOffAmt = writeOffAmt * -1;
                mTab.setValue("WriteOffAmt", writeOffAmt);
            }
            if (overUnderAmt < 0) {
                overUnderAmt = overUnderAmt * -1;
                mTab.setValue("OverUnderAmt", overUnderAmt);
            }
        }
        // mTab.setValue("C_Order_ID", C_Order_ID);
        //    }
        //    dr.close();
        //}
        //catch (err) {
        //    if (dr != null) {
        //        dr.close();
        //        dr = null;
        //    }
        //    this.log.log(Level.SEVERE, sql, err);
        //    this.setCalloutActive(false);
        //    //return e.getLocalizedMessage();
        //    return err.message;
        //}

        //mTab.setValue("Description", value);
        this.setCalloutActive(false);

        return "";

    };

    VA009_CalloutPayment.prototype.SetPaySelectionScheduleAmt = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null) {
            return "";
        }
        this.setCalloutActive(true);
        var _Client = mTab.getValue("AD_Client_ID");
        var _PaySelectionID = mTab.getValue("C_PaySelection_ID");
        var paramString = value.toString() + "," + _Client.toString() + "," + _PaySelectionID;
        var dr = VIS.dataContext.getJSONRecord("Pay/GetScheduleData", paramString);
        if (dr != null) {
            mTab.setValue("C_Invoice_ID", dr["C_Invoice_ID"]);
            mTab.setValue("PayAmt", (Util.getValueOfDecimal(dr["OpenAmt"]) - Util.getValueOfDecimal(dr["DISCOUNT1"]) - Util.getValueOfDecimal(dr["DISCOUNT2"])));
            mTab.setValue("OpenAmt", dr["OpenAmt"]);
            mTab.setValue("DiscountAmt", Util.getValueOfDecimal(dr["DISCOUNT1"]) + Util.getValueOfDecimal(dr["DISCOUNT2"]));
            mTab.setValue("WriteOffAmt", VIS.Env.ZERO);
            mTab.setValue("IsOverUnderPayment", false);//Boolean.FALSE);
            mTab.setValue("PaymentRule", dr["PAYMENTRULE"]);
            mTab.setValue("VA009_PAYMENTMETHOD_ID", dr["VA009_PAYMENTMETHOD_ID"]);
            mTab.setValue("IsSOTrx", dr["IsSOTrx"]);
            mTab.setValue("C_InvoicePaySchedule_ID", value);


        }
        this.setCalloutActive(false);
        return "";
    };

    VA009_CalloutPayment.prototype.Amounts = function (ctx, windowNo, mTab, mField, value, oldValue) {

        if (this.isCalloutActive())		//	assuming it is resetting value
        {
            return "";
        }
        //VIS_427 Bug ID:2488 :- Defined Variables to get precision of currency present on bank
        var stdPrecision = 2;
        var colName = mField.getColumnName();
        var dr = VIS.dataContext.getJSONRecord("MCurrency/GetCurrency", Util.getValueOfDecimal(mTab.getValue("C_Currency_ID")));
        if (dr != null) {
            stdPrecision = Util.getValueOfInt(dr["StdPrecision"]);
        }

        var C_Order_ID = ctx.getContextAsInt(windowNo, "C_Order_ID");
        var VA009_OrderPaySchedule_ID = ctx.getContextAsInt(windowNo, "VA009_OrderPaySchedule_ID");
        if (C_Order_ID > 0 || VA009_OrderPaySchedule_ID > 0) {
            //	New Payment
            if (ctx.getContextAsInt(windowNo, "C_Payment_ID") == 0
                && ctx.getContextAsInt(windowNo, "C_BPartner_ID") == 0
                && C_Order_ID == 0) {
                return "";
            }
            var cur = mTab.getValue("C_Currency_ID");
            if (cur == null) {
                return "";
            }
            this.setCalloutActive(true);

            //var VA009_OrderPaySchedule_ID = 0;
            //if (ctx.getContextAsInt(windowNo, "C_Order_ID") == C_Order_ID
            //    && ctx.getContextAsInt(windowNo, "VA009_OrderPaySchedule_ID") != 0) {
            //    VA009_OrderPaySchedule_ID = ctx.getContextAsInt(windowNo, "VA009_OrderPaySchedule_ID");
            //}
            //	Get Open Amount & Order Currency
            var OrderopenAmt = VIS.Env.ZERO;
            var discountAmt = VIS.Env.ZERO;
            var C_Currency_Order_ID = 0, conversionType_Order_ID = 0;
            var IsSoTrx = 'N';
            var IsReturnTrx = 'N';
            if (C_Order_ID != 0) {
                //var ts = mTab.getValue("DateTrx");
                //if (ts == null) {
                //ts = DateTime.Now.Date; //new DateTime(CommonFunctions.CurrentTimeMillis());
                //    ts = new Date();
                //}
                //tsDate = VIS.DB.to_date(ts, true);
                //
                var paramstring = C_Order_ID.toString() + "," + mTab.getValue("DateTrx").toString() + "," + VA009_OrderPaySchedule_ID.toString();
                var dr = VIS.dataContext.getJSONRecord("Pay/GetOpenAmt", paramstring);
                //var sql = "SELECT C_BPartner_ID,C_Currency_ID,"
                //    + " ORDEROPEN(C_Order_ID, " + VA009_OrderPaySchedule_ID + ") as ORDEROPEN,"
                //    + " Orderdiscount(C_Order_ID," + tsDate + "," + VA009_OrderPaySchedule_ID + ") as orderdiscount, IsSOTrx "
                //    + "FROM C_Order WHERE C_Order_ID=" + C_Order_ID;
                //var dr = null;

                //try {
                //    dr = VIS.DB.executeReader(sql, null);

                //    if (dr.read()) {
                if (dr != null) {
                    C_Currency_Order_ID = dr["C_Currency_ID"];//.getInt(2);
                    OrderopenAmt = dr["orderopen"];//.getBigDecimal(3);		//	Set Order Open Amount
                    IsSoTrx = dr["IsSOTrx"];
                    IsReturnTrx = dr["IsReturnTrx"];
                    if (OrderopenAmt == null) {
                        OrderopenAmt = VIS.Env.ZERO;
                    }
                    discountAmt = dr["OrderDiscount"];//.getBigDecimal(4);
                    conversionType_Order_ID = dr["C_ConversionType_ID"];
                }
                //    dr.close();
                //}
                //catch (err) {
                //    if (dr != null) {
                //        dr.close();
                //    }
                //    this.log.log(Level.SEVERE, sql, err);
                //    this.setCalloutActive(false);
                //    //return e.getLocalizedMessage();

                //    return err.message;
                //}
            }	//	get Order Info

            this.log.fine(" Discount= " + discountAmt + ", C_Order_ID=" + C_Order_ID + ", C_Currency_ID=" + C_Currency_Order_ID);

            //	Get Info from Tab
            if (colName == "PaymentAmount") {
                mTab.setValue("PayAmt", mTab.getValue("PaymentAmount"));
            }
            var payAmt = Util.getValueOfDecimal(mTab.getValue("PayAmt") == null ? VIS.Env.ZERO : mTab.getValue("PayAmt"));
            var writeOffAmt = Util.getValueOfDecimal(mTab.getValue("WriteOffAmt") == null ? VIS.Env.ZERO : mTab.getValue("WriteOffAmt"));
            var overUnderAmt = Util.getValueOfDecimal((mTab.getValue("OverUnderAmt") == null ? VIS.Env.ZERO : mTab.getValue("OverUnderAmt")));
            var enteredDiscountAmt = Util.getValueOfDecimal((mTab.getValue("DiscountAmt") == null ? VIS.Env.ZERO : mTab.getValue("DiscountAmt")));

            if (IsReturnTrx == "Y") {
                if (payAmt > 0) {
                    payAmt = payAmt * -1;
                    mTab.setValue("PayAmt", payAmt);
                    mTab.setValue("PaymentAmount", payAmt);
                }
                if (enteredDiscountAmt > 0) {
                    enteredDiscountAmt = enteredDiscountAmt * -1;
                }
                if (discountAmt > 0) {
                    discountAmt = discountAmt * -1;
                }
                if (writeOffAmt > 0) {
                    writeOffAmt = writeOffAmt * -1;
                    mTab.setValue("WriteOffAmt", writeOffAmt);
                }
                if (overUnderAmt > 0) {
                    overUnderAmt = overUnderAmt * -1;
                    mTab.setValue("OverUnderAmt", overUnderAmt);
                }
            }
            else {
                if (payAmt < 0) {
                    payAmt = payAmt * -1;
                    mTab.setValue("PayAmt", payAmt);
                    mTab.setValue("PaymentAmount", payAmt);
                }
                if (enteredDiscountAmt < 0) {
                    enteredDiscountAmt = enteredDiscountAmt * -1;
                }
                if (discountAmt < 0) {
                    discountAmt = discountAmt * -1;
                }
                if (writeOffAmt < 0) {
                    writeOffAmt = writeOffAmt * -1;
                    mTab.setValue("WriteOffAmt", writeOffAmt);
                }
                if (overUnderAmt < 0) {
                    overUnderAmt = overUnderAmt * -1;
                    mTab.setValue("OverUnderAmt", overUnderAmt);
                }
            }


            this.log.fine("Pay=" + payAmt + ", Discount=" + enteredDiscountAmt
                + ", WriteOff=" + writeOffAmt + ", OverUnderAmt=" + overUnderAmt);
            //	Get Currency Info
            var C_Currency_ID = Util.getValueOfInt(cur);
            //var paramString = C_Currency_ID.toString();
            //var currency = VIS.dataContext.getJSONRecord("MCurrency/GetCurrency", paramString);
            //MCurrency currency = MCurrency.Get(ctx, C_Currency_ID);
            var ConvDate = mTab.getValue("DateTrx");
            var C_ConversionType_ID = 0;
            var ii = Util.getValueOfInt(mTab.getValue("C_ConversionType_ID"));
            if (ii != null) {
                C_ConversionType_ID = ii;
            }
            var AD_Client_ID = ctx.getContextAsInt(windowNo, "AD_Client_ID");
            var AD_Org_ID = ctx.getContextAsInt(windowNo, "AD_Org_ID");
            //	Get Currency Rate
            var currencyRate = VIS.Env.ONE;
            if ((C_Currency_ID > 0 && C_Currency_Order_ID > 0 &&
                C_Currency_ID != C_Currency_Order_ID)
                || colName == "C_Currency_ID" || colName == "C_ConversionType_ID") {
                this.log.fine("InvCurrency=" + C_Currency_Order_ID
                    + ", PayCurrency=" + C_Currency_ID
                    + ", Date=" + ConvDate + ", Type=" + C_ConversionType_ID);


                var paramStr = C_Currency_Order_ID + "," + C_Currency_ID + "," + ConvDate + "," + C_ConversionType_ID + "," + AD_Client_ID + "," + AD_Org_ID;
                currencyRate = VIS.dataContext.getJSONRecord("MConversionRate/GetRate", paramStr);

                //currencyRate = MConversionRate.GetRate(C_Currency_Order_ID, C_Currency_ID,
                //    ConvDate, C_ConversionType_ID, AD_Client_ID, AD_Org_ID);
                if (currencyRate == null || currencyRate.toString() == 0) {
                    //	 mTab.setValue("C_Currency_ID", new int(C_Currency_Order_ID));	//	does not work
                    this.setCalloutActive(false);
                    if (C_Currency_Order_ID == 0) {
                        return "";		//	no error message when no Order is selected
                    }

                    // Set Order Currency and Currency Type if Currency Conversion not available
                    mTab.setValue("C_Currency_ID", C_Currency_Order_ID);
                    if (colName == "C_ConversionType_ID") {
                        if (conversionType_Order_ID == 0) {
                            return "";
                        }
                        mTab.setValue("C_ConversionType_ID", conversionType_Order_ID);
                    }
                    return "NoCurrencyConversion";
                }
                //
                OrderopenAmt = (OrderopenAmt * currencyRate);//, MidpointRounding.AwayFromZero);
                discountAmt = (discountAmt * currencyRate);
                //currency.GetStdPrecision());//, MidpointRounding.AwayFromZero);
                this.log.fine("Rate=" + currencyRate + ", OrderopenAmt=" + OrderopenAmt + ", DiscountAmt=" + discountAmt);
            }

            //	Currency Changed - convert all
            if (colName == "C_Currency_ID" || colName == "C_ConversionType_ID") {

                writeOffAmt = (writeOffAmt * currencyRate);
                //  currency.GetStdPrecision());//, MidpointRounding.AwayFromZero);
                mTab.setValue("WriteOffAmt", writeOffAmt.toFixed(stdPrecision));
                overUnderAmt = (overUnderAmt * currencyRate);
                //currency.GetStdPrecision());//, MidpointRounding.AwayFromZero);
                mTab.setValue("OverUnderAmt", overUnderAmt.toFixed(stdPrecision));

                // nnayak - Entered Discount amount should be converted to entered currency 
                enteredDiscountAmt = (enteredDiscountAmt * currencyRate).toFixed(stdPrecision);
                //currency.GetStdPrecision());//, MidpointRounding.AwayFromZero);
                mTab.setValue("DiscountAmt", enteredDiscountAmt);

                payAmt = (((OrderopenAmt - discountAmt) - writeOffAmt) - overUnderAmt);
                mTab.setValue("PayAmt", payAmt.toFixed(stdPrecision));
                mTab.setValue("PaymentAmount", payAmt.toFixed(stdPrecision));
                if (payAmt == (((OrderopenAmt - discountAmt) - writeOffAmt) - overUnderAmt)) {
                    mTab.setValue("DiscountAmt", VIS.Env.ZERO);
                    mTab.setValue("OverUnderAmt", VIS.Env.ZERO);
                    mTab.setValue("WriteOffAmt", VIS.Env.ZERO);
                }
            }

            //	No Order - Set Discount, Witeoff, Under/Over to 0
            else if (C_Order_ID == 0) {
                if (VIS.Env.ZERO != discountAmt) {
                    mTab.setValue("DiscountAmt", VIS.Env.ZERO);
                }
                if (VIS.Env.ZERO != writeOffAmt) {
                    mTab.setValue("WriteOffAmt", VIS.Env.ZERO);
                }
                if (VIS.Env.ZERO != overUnderAmt) {
                    mTab.setValue("OverUnderAmt", VIS.Env.ZERO);
                }
            }
            //  PayAmt - calculate write off
            else if (colName == "PayAmt" || colName == "PaymentAmount") {
                if (mTab.getValue("PayAmt") > OrderopenAmt) {
                    mTab.setValue("PayAmt", OrderopenAmt.toFixed(stdPrecision));
                    mTab.setValue("PaymentAmount", OrderopenAmt.toFixed(stdPrecision));
                    mTab.setValue("DiscountAmt", VIS.Env.ZERO);
                    mTab.setValue("OverUnderAmt", VIS.Env.ZERO);
                    mTab.setValue("WriteOffAmt", VIS.Env.ZERO);
                    VIS.ADialog.info("VA009_CannottIncreaseAmount");
                    this.setCalloutActive(false);
                    return "";
                }
                else {
                    //overUnderAmt = (((OrderopenAmt - payAmt) - discountAmt) - writeOffAmt);
                    //if (VIS.Env.ZERO.compareTo(overUnderAmt) > 0) {
                    //    if (Math.abs(writeOffAmt).compareTo(discountAmt) <= 0) {
                    //        discountAmt = (discountAmt + overUnderAmt);
                    //    }
                    //    else {
                    //        discountAmt = VIS.Env.ZERO;
                    //    }
                    overUnderAmt = (((OrderopenAmt - payAmt) - discountAmt) - writeOffAmt);
                    //}
                    // now, we have to split order schedule also in case of under payment
                    //mTab.setValue("DiscountAmt", overUnderAmt);
                    mTab.setValue("OverUnderAmt", overUnderAmt.toFixed(stdPrecision));
                    mTab.setValue("PayAmt", payAmt.toFixed(stdPrecision));
                    mTab.setValue("PaymentAmount", payAmt.toFixed(stdPrecision));
                }
            }
            else    //  calculate PayAmt
            {
                /* nnayak - Allow reduction in discount, but not an increase. To give a discount that is higher
                   than the calculated discount, users have to enter a write off */
                //Source code modified by Suganthi for Allowing positive and negative discount in order
                //if(EnteredDiscountAmt.compareTo(DiscountAmt)<0)
                discountAmt = enteredDiscountAmt;
                payAmt = (((OrderopenAmt - discountAmt) - writeOffAmt) - overUnderAmt);
                mTab.setValue("PayAmt", payAmt.toFixed(stdPrecision));
                mTab.setValue("PaymentAmount", payAmt.toFixed(stdPrecision));
                mTab.setValue("DiscountAmt", discountAmt.toFixed(stdPrecision));
            }
        }
        //VIS_427 Bug ID:2488 :- Handled value according to precision
        else if (Util.getValueOfInt(mTab.getValue("C_Invoice_ID")) == 0 ||
            Util.getValueOfInt(mTab.getValue("GL_JournalLine_ID")) == 0 || Util.getValueOfInt(mTab.getValue("C_Order_ID")) == 0)
        {            
            if (colName == "PaymentAmount") {
                this.setCalloutActive(true);
                mTab.setValue("PayAmt", mTab.getValue("PaymentAmount").toFixed(stdPrecision));
                mTab.setValue("PaymentAmount", mTab.getValue("PayAmt"))
            }
        }
        ctx.setContext(windowNo, "PayAmt", payAmt);
        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };


    VA009_CalloutPayment.prototype.Invoice = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null) {
            return "";
        }
        this.setCalloutActive(true);
        var dr = VIS.dataContext.getJSONRecord("Pay/GetSchedule", Util.getValueOfInt(value));
        //var _sql = "SELECT C_InvoicePaySchedule_ID FROM C_InvoicePaySchedule WHERE C_INVOICE_ID = " + Util.getValueOfInt(value);
        //var ds = VIS.DB.executeDataSet(_sql);
        //if (ds != null && ds.tables[0].rows.length > 0 && ds.tables[0].rows.length == 1) {
        if (dr != null) {
            mTab.setValue("C_InvoicePaySchedule_ID", dr["C_InvoicePaySchedule_ID"]);
        }
        this.setCalloutActive(false);
        return "";
    };

    //Callout For VA009- Payment Management Module....Manjot.
    VA009_CalloutPayment.prototype.PaymentBaseType = function (ctx, windowNo, mTab, mField, value, oldValue) {

        if (this.isCalloutActive())		//	assuming it is resetting value
        {
            return "";
        }
        if (value.toString() == "B") {
            mTab.setValue("VA009_PaymentMode", "C");
            mTab.getField("VA009_PaymentMode").setReadOnly(true);
            mTab.setValue("VA009_PaymentType", "S");
            mTab.getField("VA009_PaymentType").setReadOnly(true);
            mTab.setValue("VA009_PaymentTrigger", "S");
            mTab.getField("VA009_PaymentTrigger").setReadOnly(true);
            mTab.setValue("VA009_PaymentRule", "M");
            mTab.getField("VA009_PaymentRule").setReadOnly(true);

        }
        else {
            mTab.setValue("VA009_PaymentMode", "B");
            mTab.getField("VA009_PaymentMode").setReadOnly(true);
            mTab.setValue("VA009_PaymentType", null);
            mTab.getField("VA009_PaymentType").setReadOnly(false);
            mTab.setValue("VA009_PaymentTrigger", null);
            mTab.getField("VA009_PaymentTrigger").setReadOnly(false);
            mTab.setValue("VA009_PaymentRule", null);
            mTab.getField("VA009_PaymentRule").setReadOnly(false);
        }

        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    VA009_CalloutPayment.prototype.PaymentTrigger = function (ctx, windowNo, mTab, mField, value, oldValue) {

        if (this.isCalloutActive())		//	assuming it is resetting value
        {
            return "";
        }
        if (value != null) {
            if (value.toString() == "R") {
                mTab.setValue("VA009_IsMandate", "Y");
            }
            else {
                mTab.setValue("VA009_IsMandate", "N");
            }
        }

        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    VA009_CalloutPayment.prototype.DisplayBPmandate = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null) {
            return "";
        }
        this.setCalloutActive(true);
        var _paymentMethod = mTab.getValue("VA009_PaymentMethod_ID");
        var _Client = mTab.getValue("AD_Client_ID");
        var paramString = _paymentMethod.toString() + "," + _Client.toString();
        var dr = VIS.dataContext.getJSONRecord("Pay/GetMandate", paramString);
        //var sql = "Select VA009_IsMandate From VA009_PaymentMethod Where VA009_PaymentMethod_ID=" + _paymentMethod + "And IsActive ='Y' AND AD_Client_ID=" + _Client;
        //var ds = VIS.DB.executeDataSet(sql);
        //if (ds != null && ds.tables[0].rows.length > 0) {
        if (dr != null) {
            var _bpmandate = dr["VA009_IsMandate"];
            if (_bpmandate == 'Y') {
                mTab.setValue("VA009_IsBPMandate", 'Y');
            }
            else
                mTab.setValue("VA009_IsBPMandate", 'N');
        }
        this.setCalloutActive(false);
    };

    VA009_CalloutPayment.prototype.DisplayBPmandatePO = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null) {
            return "";
        }
        this.setCalloutActive(true);
        var _paymentMethod = mTab.getValue("VA009_PO_PaymentMethod_ID");
        var _Client = mTab.getValue("AD_Client_ID");
        var paramString = _paymentMethod.toString() + "," + _Client.toString();
        var dr = VIS.dataContext.getJSONRecord("Pay/GetMandate", paramString);
        if (dr != null) {
            var _bpmandate = dr["VA009_IsMandate"];
            if (_bpmandate == 'Y') {
                mTab.setValue("VA009_IsBPMandate", 'Y');
            }
            else
                mTab.setValue("VA009_IsBPMandate", 'N');
        }
        this.setCalloutActive(false);
    };

    VA009_CalloutPayment.prototype.PaymentMethod = function (ctx, windowNo, mTab, mField, value, oldValue) {

        if (this.isCalloutActive() || value == null || value.toString() == "")		//	assuming it is resetting value
        {
            return "";
        }
        var dr = VIS.dataContext.getJSONRecord("Pay/GetPaymentRule", Util.getValueOfInt(value));
        //var _sql = "select VA009_PAYMENTTRIGGER,VA009_PAYMENTRULE from VA009_PAYMENTMETHOD where VA009_PAYMENTMETHOD_ID=" + Util.getValueOfInt(value);
        //var ds = VIS.DB.executeDataSet(_sql);

        //if (ds != null && ds.tables[0].rows.length > 0) {
        if (dr != null) {
            mTab.setValue("VA009_PaymentRule", dr["VA009_PAYMENTRULE"]);
            mTab.setValue("VA009_PAYMENTTRIGGER", dr["VA009_PAYMENTTRIGGER"]);
        }

        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    VA009_CalloutPayment.prototype.SetPaymentType = function (ctx, windowNo, mTab, mField, value, oldValue) {

        ////Issue ID : SI_0637 In google sheet standard issues.. Error msg was coming can't read value of null
        if (this.isCalloutActive() || value == null || value.toString() == "")  // assuming it is resetting value
        {
            return "";
        }
        this.setCalloutActive(true);
        var dr = VIS.dataContext.getJSONRecord("Pay/GetPaymentRule", Util.getValueOfInt(value));
        //var _sql = "select VA009_PAYMENTBASETYPE from VA009_PAYMENTMETHOD where VA009_PAYMENTMETHOD_ID=" + Util.getValueOfInt(value);
        //var ds = VIS.DB.executeDataSet(_sql);
        //if (ds != null && ds.tables[0].rows.length > 0) {
        //if (ds.tables[0].rows[0].cells["va009_paymentbasetype"] != "W") {
        var C_Currency_ID = mTab.getValue("C_Currency_ID");
        if (dr != null) {
            /*VIS_427 if the currency id column is their in tab then execute below code else not*/
            if (Util.getValueOfInt(dr["C_Currency_ID"]) > 0 && mTab.findColumn("C_Currency_ID") >=0) {
                if (C_Currency_ID != Util.getValueOfInt(dr["C_Currency_ID"])) {
                    mTab.setValue("VA009_PaymentMethod_ID", null);
                    this.setCalloutActive(false);
                    VIS.ADialog.info("VA009_CurrencyWithPaymentMethod");
                    return "";
                }
            }
            var isVendor = "N";
            var isCustomer = "N";
            var C_BPartner_ID = Util.getValueOfInt(mTab.getValue("C_BPartner_ID"));
            var BPdtl = VIS.dataContext.getJSONRecord("Pay/GetBPPaymentRule", C_BPartner_ID);
            if (BPdtl != null) {
                isVendor = Util.getValueOfString(BPdtl["ISVENDOR"]);
                isCustomer = Util.getValueOfString(BPdtl["IsCustomer"]);
            }
            if (isVendor == "Y") {
                mTab.setValue("PaymentRulePO", BPdtl["va009_paymentbasetypePO"]);
                if (isCustomer == "N")
                    mTab.setValue("PaymentRule", null);
                else
                    mTab.setValue("PaymentRule", dr["VA009_PAYMENTBASETYPE"]);
            }
            else {
                mTab.setValue("PaymentRule", dr["VA009_PAYMENTBASETYPE"]);
                mTab.setValue("PaymentRulePO", null);
            }
            mTab.setValue("PaymentMethod", dr["VA009_PAYMENTBASETYPE"]);


        }
        else {
            mTab.setValue("PaymentRule", "T");
            mTab.setValue("PaymentMethod", "T");
            mTab.setValue("PaymentRulePO", "T");
            //mTab.setValue("PaymentRule", Util.getValueOfString(ds.tables[0].rows[0].cells["va009_paymentbasetype"]));
        }
        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    VA009_CalloutPayment.prototype.SetPaymentTypeInv = function (ctx, windowNo, mTab, mField, value, oldValue) {

        ////Issue ID : SI_0637 In google sheet standard issues.. Error msg was coming can't read value of null
        if (this.isCalloutActive() || value == null || value.toString() == "")  // assuming it is resetting value
        {
            return "";
        }
        this.setCalloutActive(true);
        var dr = VIS.dataContext.getJSONRecord("Pay/GetPaymentRule", Util.getValueOfInt(value));
        var C_Currency_ID = mTab.getValue("C_Currency_ID");
        var IsSoTrx = false;
        IsSoTrx = mTab.getValue("IsSOTrx");
        if (dr != null) {

            if (Util.getValueOfInt(dr["C_Currency_ID"]) > 0) {
                if (C_Currency_ID != Util.getValueOfInt(dr["C_Currency_ID"])) {
                    mTab.setValue("VA009_PaymentMethod_ID", null);
                    this.setCalloutActive(false);
                    VIS.ADialog.info("VA009_CurrencyWithPaymentMethod");
                    return "";
                }
            }
            //var isVendor = "N";
            //var isCustomer = "N";
            //var C_BPartner_ID = Util.getValueOfInt(mTab.getValue("C_BPartner_ID"));
            //var BPdtl = VIS.dataContext.getJSONRecord("Pay/GetBPPaymentRule", C_BPartner_ID);
            //if (BPdtl != null) {
            //    isVendor = Util.getValueOfString(BPdtl["ISVENDOR"]);
            //    isCustomer = Util.getValueOfString(BPdtl["IsCustomer"]);
            //}
            //if (isVendor == "Y") {
            //    if (isCustomer == "N") {
            //        if (IsSoTrx) {
            //            mTab.setValue("PaymentRule", dr["VA009_PAYMENTBASETYPE"]);//Vendor only
            //            mTab.setValue("PaymentMethod", dr["VA009_PAYMENTBASETYPE"]);
            //        }
            //    }
            //    else {
            //        if (IsSoTrx) {
            //            mTab.setValue("PaymentRule", dr["VA009_PAYMENTBASETYPE"]);//Vendor only
            //            mTab.setValue("PaymentMethod", dr["VA009_PAYMENTBASETYPE"]);
            //        }
            //        else {
            //            mTab.setValue("PaymentRule", BPdtl["va009_paymentbasetypePO"]); //vendor + Customer
            //            mTab.setValue("PaymentMethod", BPdtl["va009_paymentbasetypePO"]);
            //        }
            //    }
            //}
            //else {
            mTab.setValue("PaymentRule", dr["VA009_PAYMENTBASETYPE"]); //customer
            mTab.setValue("PaymentMethod", dr["VA009_PAYMENTBASETYPE"]);
            //}

        }
        else {
            mTab.setValue("PaymentRule", "T");
            mTab.setValue("PaymentMethod", "T");
            mTab.setValue("PaymentRulePO", "T");
        }
        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    VA009_CalloutPayment.prototype.SetPaymentTypeOnVendor = function (ctx, windowNo, mTab, mField, value, oldValue) {

        ////Issue ID : SI_0637 In google sheet standard issues.. Error msg was coming can't read value of null
        if (this.isCalloutActive() || value == null || value.toString() == "")  // assuming it is resetting value
        {
            return "";
        }
        this.setCalloutActive(true);
        var isVendor = false;
        var isCustomer = false;
        isVendor = mTab.getValue("IsVendor");
        isCustomer = mTab.getValue("IsCustomer");
        var dr = VIS.dataContext.getJSONRecord("Pay/GetPaymentRule", Util.getValueOfInt(value));
        if (dr != null) {
            if (isVendor) {
                mTab.setValue("PaymentRulePO", dr["VA009_PAYMENTBASETYPE"]);
                if (!isCustomer)
                    mTab.setValue("PaymentRule", null);
            }
            else {
                mTab.setValue("PaymentRule", dr["VA009_PAYMENTBASETYPE"]);
                mTab.setValue("PaymentRulePO", null);
            }
            mTab.setValue("PaymentMethod", dr["VA009_PAYMENTBASETYPE"]);
        }
        else {
            mTab.setValue("PaymentRule", "T");
            mTab.setValue("PaymentMethod", "T");
            mTab.setValue("PaymentRulePO", "T");
            //mTab.setValue("PaymentRule", Util.getValueOfString(ds.tables[0].rows[0].cells["va009_paymentbasetype"]));
        }
        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    VA009_CalloutPayment.prototype.SetBPPaymentRuleOnPayment = function (ctx, windowNo, mTab, mField, value, oldValue) {

        if (this.isCalloutActive() || value == null || value.toString() == "") 	//	assuming it is resetting value
        {
            return "";
        }
        this.setCalloutActive(true);
        var dr = VIS.dataContext.getJSONRecord("Pay/GetBPPaymentRule", Util.getValueOfInt(value));
        var PaymentBaseType = "A";
        //var _sql = "SELECT pm.va009_paymentbasetype,cb.va009_paymentmethod_id FROM c_bpartner cb INNER JOIN va009_paymentmethod pm ON cb.va009_paymentmethod_id=pm.va009_paymentmethod_id WHERE c_bpartner_id=" + Util.getValueOfInt(value);
        //var ds = VIS.DB.executeDataSet(_sql);
        //if (ds != null && ds.tables[0].rows.length > 0) {
        if (dr != null) {
            //VA009_PO_PaymentMethod_ID added new column for enhancement.. Google Sheet ID-- SI_0036
            var C_DocType_ID = Util.getValueOfInt(mTab.getValue("C_DocType_ID"));
            var drr = VIS.dataContext.getJSONRecord("Pay/GetPayBaseType", C_DocType_ID);

            if (Util.getValueOfString(dr["ISVENDOR"]) == 'N') {
                if (drr != null) {
                    if (drr["DocBaseType"] == "ARR") {
                        mTab.setValue("va009_paymentmethod_id", dr["va009_paymentmethod_id"]);
                        PaymentBaseType = Util.getValueOfString(dr["va009_paymentbasetype"]);
                    }
                    if (drr["DocBaseType"] == "APP") {
                        mTab.setValue("va009_paymentmethod_id", null);
                        PaymentBaseType = "A";
                    }
                }
            }
            else {
                if (drr != null) {
                    if (drr["DocBaseType"] == "ARR") {
                        mTab.setValue("va009_paymentmethod_id", null);
                        PaymentBaseType = "A";
                    }
                    if (drr["DocBaseType"] == "APP") {
                        mTab.setValue("va009_paymentmethod_id", dr["VA009_PO_PaymentMethod_ID"]);
                        PaymentBaseType = Util.getValueOfString(dr["va009_paymentbasetypePO"]);
                    }
                }
                if ((Util.getValueOfString(dr["IsCustomer"]) == 'Y')) {
                    if (drr != null) {
                        if (drr["DocBaseType"] == "ARR") {
                            mTab.setValue("va009_paymentmethod_id", dr["va009_paymentmethod_id"]);
                            PaymentBaseType = Util.getValueOfString(dr["va009_paymentbasetype"]);
                        }
                        if (drr["DocBaseType"] == "APP") {
                            mTab.setValue("va009_paymentmethod_id", dr["VA009_PO_PaymentMethod_ID"]);
                            PaymentBaseType = Util.getValueOfString(dr["va009_paymentbasetypePO"]);
                        }
                    }
                }
            }

            if ((PaymentBaseType != "B") && (PaymentBaseType != "C") && (PaymentBaseType != "P")) {
                if (PaymentBaseType == "D") {
                    mTab.setValue("TenderType", "D");
                }
                else if (PaymentBaseType == "K") {
                    mTab.setValue("TenderType", "C");
                }
                else if (PaymentBaseType == "S") {
                    mTab.setValue("TenderType", "K");
                }
                else if (PaymentBaseType == "T") {
                    mTab.setValue("TenderType", "A");
                }
                else if (PaymentBaseType == "L") {
                    mTab.setValue("TenderType", "L");
                }
                else {
                    mTab.setValue("TenderType", "A");
                }
            }
            else
                mTab.setValue("va009_paymentmethod_id", null);
        }
        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    VA009_CalloutPayment.prototype.SetBPPaymentRule = function (ctx, windowNo, mTab, mField, value, oldValue) {

        if (this.isCalloutActive() || value == null || value.toString() == "") 	//	assuming it is resetting value
        {
            return "";
        }
        this.setCalloutActive(true);
        var dr = VIS.dataContext.getJSONRecord("Pay/GetBPPaymentRule", Util.getValueOfInt(value));
        //var _sql = "SELECT pm.va009_paymentbasetype,cb.va009_paymentmethod_id FROM c_bpartner cb INNER JOIN va009_paymentmethod pm ON cb.va009_paymentmethod_id=pm.va009_paymentmethod_id WHERE c_bpartner_id=" + Util.getValueOfInt(value);
        //var ds = VIS.DB.executeDataSet(_sql);
        //if (ds != null && ds.tables[0].rows.length > 0) {
        if (dr != null) {
            //VA009_PO_PaymentMethod_ID added new column for enhancement.. Google Sheet ID-- SI_0036
            if (Util.getValueOfString(dr["ISVENDOR"]) == 'N') {
                mTab.setValue("va009_paymentmethod_id", dr["va009_paymentmethod_id"]);
            }
            else
                mTab.setValue("va009_paymentmethod_id", dr["VA009_PO_PaymentMethod_ID"]);

            if ((Util.getValueOfString(dr["va009_paymentbasetype"]) != "B") && (Util.getValueOfString(dr["va009_paymentbasetype"]) != "C") && (Util.getValueOfString(dr["va009_paymentbasetype"]) != "P")) {
                if (Util.getValueOfString(dr["va009_paymentbasetype"] == "D")) {
                    mTab.setValue("TenderType", "D");
                }
                else if (Util.getValueOfString(dr["va009_paymentbasetype"] == "K")) {
                    mTab.setValue("TenderType", "C");
                }
                else if (Util.getValueOfString(dr["va009_paymentbasetype"] == "S")) {
                    mTab.setValue("TenderType", "K");
                }
                else if (Util.getValueOfString(dr["va009_paymentbasetype"] == "T")) {
                    mTab.setValue("TenderType", "A");
                }
                else if (dr["va009_paymentbasetype"] == "L") {
                    mTab.setValue("TenderType", "L");
                }
                else {
                    mTab.setValue("TenderType", "A");
                }
            }
            else
                mTab.setValue("va009_paymentmethod_id", null);
        }
        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    VA009_CalloutPayment.prototype.SetInvoicePaymentRule = function (ctx, windowNo, mTab, mField, value, oldValue) {

        if (this.isCalloutActive() || value == null || value.toString() == "") 		//	assuming it is resetting value
        {
            return "";
        }
        this.setCalloutActive(true);
        var dr = VIS.dataContext.getJSONRecord("Pay/GetInvoicePaymentRule", Util.getValueOfInt(value));
        //var _sql = "SELECT pm.va009_paymentbasetype,cb.va009_paymentmethod_id FROM c_invoice cb INNER JOIN va009_paymentmethod pm ON cb.va009_paymentmethod_id=pm.va009_paymentmethod_id WHERE cb.c_invoice_id=" + Util.getValueOfInt(value);
        //var ds = VIS.DB.executeDataSet(_sql);
        //if (ds != null && ds.tables[0].rows.length > 0) {
        if (dr != null) {
            mTab.setValue("va009_paymentmethod_id", dr["va009_paymentmethod_id"]);
            if (dr["va009_paymentbasetype"] == "D") {
                mTab.setValue("TenderType", "D");
            }
            else if (dr["va009_paymentbasetype"] == "K") {
                mTab.setValue("TenderType", "C");
            }
            else if (dr["va009_paymentbasetype"] == "S") {
                mTab.setValue("TenderType", "K");
            }
            else if (dr["va009_paymentbasetype"] == "T") {
                mTab.setValue("TenderType", "A");
            }
            else if (dr["va009_paymentbasetype"] == "L") {
                mTab.setValue("TenderType", "L");
            }
            else {
                mTab.setValue("TenderType", "A");
            }
        }
        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    VA009_CalloutPayment.prototype.SetTenderType = function (ctx, windowNo, mTab, mField, value, oldValue) {

        if (this.isCalloutActive() || value == null || value.toString() == "")  // assuming it is resetting value
        {
            return "";
        }
        this.setCalloutActive(true);
        var dr = VIS.dataContext.getJSONRecord("Pay/GetPaymentRule", Util.getValueOfInt(value));
        //var _sql = "select VA009_PAYMENTBASETYPE from VA009_PAYMENTMETHOD where VA009_PAYMENTMETHOD_ID=" + Util.getValueOfInt(value);
        //var ds = VIS.DB.executeDataSet(_sql);

        //if (ds != null && ds.tables[0].rows.length > 0) {
        if (dr != null) {
            if (dr["VA009_PAYMENTBASETYPE"] == "D") {
                mTab.setValue("TenderType", "D");
            }
            else if (dr["VA009_PAYMENTBASETYPE"] == "K") {
                mTab.setValue("TenderType", "C");
            }
            else if (dr["VA009_PAYMENTBASETYPE"] == "S") {
                mTab.setValue("TenderType", "K");
            }
            else if (dr["VA009_PAYMENTBASETYPE"] == "T") {
                mTab.setValue("TenderType", "A");
            }
            else if (dr["VA009_PAYMENTBASETYPE"] == "L") {
                mTab.setValue("TenderType", "L");
            }
            else {
                mTab.setValue("TenderType", "A");
            }
        }
        //Reset checkno/checkdate if tendertype other than check
        if (Util.getValueOfString(mTab.getValue("TenderType")) != "K") {
            mTab.setValue("CheckNo", "");
            mTab.setValue("CheckDate", "");
        }
        //VA230:Get autocheckcontrol and set override autocheck funcationlity based on below condition
        var bankaccountId = Util.getValueOfInt(mTab.getValue("C_BankAccount_ID"));
        var paymentMethodId = Util.getValueOfInt(mTab.getValue("VA009_PaymentMethod_ID"));
        var checkNo = Util.getValueOfString(mTab.getValue("CheckNo"));
        var autoCheck = false;
        if (bankaccountId > 0 && paymentMethodId > 0 && checkNo != "" && Util.getValueOfString(mTab.getValue("TenderType")) == "K") {
            var paramString = bankaccountId.toString() + "," + paymentMethodId.toString();
            autoCheck = Util.getValueOfBoolean(VIS.dataContext.getJSONRecord("VIS/MPayment/GetAutoCheckControl", paramString.toString()));
        }
        mTab.setValue("IsOverrideAutoCheck", autoCheck);
        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    VA009_CalloutPayment.prototype.SetPayAllocateAmt = function (ctx, windowNo, mTab, mField, value, oldValue) {

        if (this.isCalloutActive() || value == null || value.toString() == "")   // assuming it is resetting value
        {
            return "";
        }
        this.setCalloutActive(true);
        var c_payment_id = mTab.getValue("C_Payment_ID");
        var c_invoice_id = mTab.getValue("C_Invoice_ID");

        // when we clear invoice, then update amount as 0
        if (c_invoice_id == null) {
            mTab.setValue("Amount", VIS.Env.ZERO);
            mTab.setValue("InvoiceAmt", VIS.Env.ZERO);
            mTab.setValue("DiscountAmt", VIS.Env.ZERO);
            mTab.setValue("WriteOffAmt", VIS.Env.ZERO);
            mTab.setValue("OverUnderAmt", VIS.Env.ZERO);
        }
        else {
            //var date = VIS.DB.executeScalar("SELECT DateTrx FROM C_Payment WHERE C_Payment_ID=" + c_payment_id);
            var dr = VIS.dataContext.getJSONRecord("Pay/GetPayAllocateAmt", c_payment_id.toString() + "," + c_invoice_id + "," + value.toString());
            //var _sql = "SELECT dueamt,  CASE    WHEN (TRUNC(discountdate) >= TRUNC(" + date + "))    THEN DiscountAmt    WHEN (TRUNC(discountdays2) >= TRUNC(" + date + ")    AND TRUNC(" + date + ")          > TRUNC(discountdate))    THEN Discount2    ELSE 0  END AS discount FROM c_invoicepayschedule WHERE c_invoicepayschedule_id=" + Util.getValueOfInt(value);
            //var ds = VIS.DB.executeDataSet(_sql);

            //if (ds != null && ds.tables[0].rows.length > 0) {
            if (dr != null) {
                var invoiceOpenAmt = Util.getValueOfDecimal(dr["Dueamt"]);
                var discount = Util.getValueOfDecimal(dr["discount"]);
                var IsReturnTrx = dr["IsReturnTrx"];
                if (IsReturnTrx == "Y") {
                    if (invoiceOpenAmt > 0) {
                        invoiceOpenAmt = invoiceOpenAmt * -1;
                    }
                    if (discount > 0) {
                        discount = discount * -1;
                    }
                }
                //VIS_427 BugID 5620 Handled amount by subtarcting discount with due amount
                mTab.setValue("Amount", invoiceOpenAmt - discount);
                mTab.setValue("InvoiceAmt", invoiceOpenAmt);
                mTab.setValue("DiscountAmt", discount);
                mTab.setValue("WriteOffAmt", VIS.Env.ZERO);
                mTab.setValue("OverUnderAmt", VIS.Env.ZERO);
            }
        }
        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    VA009_CalloutPayment.prototype.OrderSchedule = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null) {
            return "";
        }
        this.setCalloutActive(true);
        var dr = VIS.dataContext.getJSONRecord("Pay/GetDueAmt", Util.getValueOfInt(value));
        if (dr != null) {
            mTab.setValue("VA009_OrderPaySchedule_ID", dr["VA009_OrderPaySchedule_ID"]);
        }
        else {
            mTab.setValue("VA009_OrderPaySchedule_ID", null);
            var drs = VIS.dataContext.getJSONRecord("Pay/GetPaymentRule", Util.getValueOfInt(mTab.getValue("VA009_PaymentMethod_ID")));
            if (drs != null) {
                if (drs["VA009_PAYMENTBASETYPE"] == "D") {
                    mTab.setValue("TenderType", "D");
                }
                else if (drs["VA009_PAYMENTBASETYPE"] == "K") {
                    mTab.setValue("TenderType", "C");
                }
                else if (drs["VA009_PAYMENTBASETYPE"] == "S") {
                    mTab.setValue("TenderType", "K");
                }
                else if (drs["VA009_PAYMENTBASETYPE"] == "T") {
                    mTab.setValue("TenderType", "A");
                }
                else if (drs["VA009_PAYMENTBASETYPE"] == "L") {
                    mTab.setValue("TenderType", "L");
                }
                else {
                    mTab.setValue("TenderType", "A");
                }
            }
        }
        this.setCalloutActive(false);
        return "";
    };

    VA009_CalloutPayment.prototype.BankDetails = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null) {
            return "";
        }
        this.setCalloutActive(true);
        var dr = VIS.dataContext.getJSONRecord("Pay/GetBankDetails", Util.getValueOfInt(value));
        if (dr != null) {
            mTab.setValue("C_Location_ID", dr["C_Location_ID"]);
            mTab.setValue("RoutingNo", dr["RoutingNo"]);
        }
        this.setCalloutActive(false);
        return "";
    };

    VA009_CalloutPayment.prototype.SetPayBaseType = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null) {
            return "";
        }
        this.setCalloutActive(true);
        var dr = VIS.dataContext.getJSONRecord("Pay/GetPayBaseType", Util.getValueOfInt(value));
        if (dr != null) {
            mTab.setValue("VA009_PayBaseType", dr["DocBaseType"]);
        }
        this.setCalloutActive(false);
        return "";
    };

    // this callout is call from payment window - from VA009_PaymentMethod_ID column
    // if payment base type of this method is "Cash", then set null with this column
    VA009_CalloutPayment.prototype.ComparePayMethod = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (value == null) {
            return "";
        }
        var dr = VIS.dataContext.getJSONRecord("Pay/VerifyPayMethod", Util.getValueOfInt(value));
        if (dr != null && dr == 0) {
            mTab.setValue("VA009_PaymentMethod_ID", null);
        }
        return "";
    };

    // this callout is call from payment window - from VA009_PaymentMethod_ID column
    // if payment base type of this method is "card", then blank all the card columns
    VA009_CalloutPayment.prototype.ClearCardFields = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (value == null) {
            return "";
        }
        var TenderType = Util.getValueOfString(mTab.getValue("TenderType"));
        if (TenderType != "C") {

            mTab.setValue("CreditCardType", null);
            mTab.setValue("CreditCardNumber", null);
            mTab.setValue("CreditCardExpMM", 1);
            mTab.setValue("CreditCardExpYY", 3);
        }
        return "";
    };

    //Arpit To Set Currency From Bank Account to Generate Payment against the currency
    VA009_CalloutPayment.prototype.SetBankCurrency = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null) {
            return "";
        }
        this.setCalloutActive(true);
        var dr = VIS.dataContext.getJSONRecord("Pay/GetBankCurrency", Util.getValueOfInt(value));
        if (dr != null) {
            mTab.setValue("C_Currency_ID", dr["C_Currency_ID"]);
        }
        this.setCalloutActive(false);
        return "";
    };

    //To Set Account Name From Partner Bank Account
    VA009_CalloutPayment.prototype.SetAccountName = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null) {
            return "";
        }
        this.setCalloutActive(true);
        var dr = VIS.dataContext.getJSONRecord("Pay/getaccountName", Util.getValueOfInt(value));
        if (dr != null) {
            //new columns needs to set on payment schedule batch lines window
            mTab.setValue("a_name", dr["a_name"]);
            mTab.setValue("RoutingNo", dr["RoutingNo"]);
            mTab.setValue("AccountNo", dr["AccountNo"]);
            //if (mTab.getValue("IsReceipt") == "N") {
            //    mTab.setValue("a_name", "");
            //    mTab.setValue("RoutingNo", "");
            //    mTab.setValue("AccountNo", "");
            //}
        }
        this.setCalloutActive(false);
        return "";
    };

    //Rakesh(VA228):To Set AccountDate same as DocumentDate
    VA009_CalloutPayment.prototype.SetDateAcct = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null) {
            return "";
        }
        this.setCalloutActive(true);

        mTab.setValue("DateAcct", value);

        this.setCalloutActive(false);
        return "";
    };

    VA009.Model = VA009.Model || {};
    VA009.Model.VA009_CalloutPayment = VA009_CalloutPayment; //assign object in Model NameSpace


    function VA009_CalloutPaymentTerm() {
        VIS.CalloutEngine.call(this, "VA009.VA009_CalloutPaymentTerm"); // must call base class (CalloutEngine)
    };
    VIS.Utility.inheritPrototype(VA009_CalloutPaymentTerm, VIS.CalloutEngine);//must inheirt Base class CalloutEngine

    // Is used to update VA009_IsPayScheduleTabDisabled as True / False based on VA009_Advance. 
    // If this is TRUE then schedule tab should be Read Only (Disabled)
    VA009_CalloutPaymentTerm.prototype.SetPayScheduleTabDisabled = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null || value.toString() == "") {
            return "";
        }
        this.setCalloutActive(true);
        if (value == true) {
            mTab.setValue("VA009_IsPayScheduleTabDisabled", true);
            //** clearing WeekDay value when Advance checkbox is true ** Dt: 02/04/2021 ** Modified By: Kumar ** //
            mTab.setValue("NetDay", "");
            mTab.setValue("WeekOffset", "0");
        }
        else {
            mTab.setValue("VA009_IsPayScheduleTabDisabled", false);
        }
        this.setCalloutActive(false);
        return "";
    };

    //** Disabling PaySchedule tab when WeekDay value is selected ** Dt: 02/04/2021 ** Modified By: Kumar ** //
    // Is used to update VA009_IsPayScheduleTabDisabled as True / False based on WeekDay. 
    // If this is TRUE then schedule tab should be Read Only (Disabled)
    VA009_CalloutPaymentTerm.prototype.SetPayScheduleTabDisabledForWeekDay = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive() || value == null || value.toString() == "") {
            mTab.setValue("VA009_IsPayScheduleTabDisabled", false);
            return "";
        }
        this.setCalloutActive(true);

        if (Util.getValueOfInt(value) > 0) {
            mTab.setValue("VA009_IsPayScheduleTabDisabled", true);
        }
        else {
            mTab.setValue("VA009_IsPayScheduleTabDisabled", false);
        }
        this.setCalloutActive(false);
        return "";
    };

    VA009.Model = VA009.Model || {};
    VA009.Model.VA009_CalloutPaymentTerm = VA009_CalloutPaymentTerm;

    function VA009_CalloutCashJournal() {
        VIS.CalloutEngine.call(this, "VA009.VA009_CalloutCashJournal"); // must call base class (CalloutEngine)
    };
    VIS.Utility.inheritPrototype(VA009_CalloutCashJournal, VIS.CalloutEngine);//must inheirt Base class CalloutEngine

    /// <summary>
    ///  Cash Journal Line Order. when Order selected - set C_Currency,Amount
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="windowNo"></param>
    /// <param name="mTab"></param>
    /// <param name="mField"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    VA009_CalloutCashJournal.prototype.GetOrderPaySchedule = function (ctx, windowNo, mTab, mField, value, oldValue) {
        //  
        if (this.isCalloutActive()) // assuming it is resetting value
        {
            return "";
        }
        if (value == null || value.toString() == "") {
            /*Clear references and amounts when we clear invoice*/
            if (mField.getColumnName() == "C_Order_ID") {
                this.setCalloutActive(true);
                mTab.setValue("C_BPartner_ID", null);
                mTab.setValue("C_BPartner_Location_ID", null);
                mTab.setValue("VA009_OrderPaySchedule_ID", null);

                mTab.setValue("ConvertedAmt", VIS.Env.ZERO);
                mTab.setValue("Amount", VIS.Env.ZERO);
                mTab.setValue("DiscountAmt", VIS.Env.ZERO);
                mTab.setValue("WriteOffAmt", VIS.Env.ZERO);
                mTab.setValue("OverUnderAmt", VIS.Env.ZERO);
                this.setCalloutActive(false);
            }
            return "";
        }
        this.setCalloutActive(true);

        var C_Order_ID = Util.getValueOfInt(value);

        var dueAmount = 0;
        var OrderPaySchedule_ID = 0;
        var data = null;
        try {
            data = VIS.dataContext.getJSONRecord("VA009/CashJournal/GetOrderPaySchedDetail", C_Order_ID.toString());
            if (data != null) {
                OrderPaySchedule_ID = Util.getValueOfInt(data["VA009_OrderPaySchedule_ID"]);
                mTab.setValue("VA009_OrderPaySchedule_ID", OrderPaySchedule_ID);
                mTab.setValue("C_BPartner_ID", Util.getValueOfInt(data["C_BPartner_ID"]));
                mTab.setValue("C_Currency_ID", Util.getValueOfInt(data["C_Currency_ID"]));
                mTab.setValue("C_ConversionType_ID", Util.getValueOfInt(data["C_ConversionType_ID"]));
                mTab.setValue("C_BPartner_Location_ID", Util.getValueOfInt(data["C_BPartner_Location_ID"]));
                dueAmount = Util.getValueOfDecimal(data["DueAmount"]);

                //Set IsSOTrx type in context
                ctx.setContext(windowNo, "VA009_IsSOTrx", data["IsSOTrx"]);
                var isSOTrx = "Y" == data["IsSOTrx"];
                if (!isSOTrx) {
                    dueAmount = (dueAmount) * (-1);
                }
                var docbaseType = Util.getValueOfString(data["DocBaseType"]);
                if (docbaseType == "SOO") {
                    mTab.setValue("VSS_PAYMENTTYPE", "R");
                }
                else {
                    mTab.setValue("VSS_PAYMENTTYPE", "P");
                }
                mTab.setValue("Amount", dueAmount);
                mTab.setValue("DiscountAmt", VIS.Env.ZERO);
                mTab.setValue("WriteOffAmt", VIS.Env.ZERO);
            }
        }
        catch (err) {
            if (data != null) {
                data = null;
            }
            this.log.log(Level.SEVERE, "VA009_CalloutCashJournal.GetOrderPaySchedule -" + C_Order_ID, err.message);
            this.setCalloutActive(false);
            return err.toString();
        }
        if (C_Order_ID == null || C_Order_ID == 0) {
            mTab.setValue("C_Currency_ID", null);
            this.setCalloutActive(false);
            return "";
        }
        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    /// <summary>
    /// Order Pay Schedule
    /// When Order Pay Schedule Selected
    /// The Amount Corresponding to that pay Schedule
    /// filled in Amount
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="WindowNo"></param>
    /// <param name="mTab"></param>
    /// <param name="mField"></param>
    /// <param name="value"></param>
    /// <returns>Amount</returns>
    VA009_CalloutCashJournal.prototype.SetAmount = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive()) {
            return "";
        }
        if (value == null || value.toString() == "") {
            return "";
        }
        this.setCalloutActive(true);

        if (Util.getValueOfInt(mTab.getValue("VA009_OrderPaySchedule_ID")) > 0) {
            var Amount = Util.getValueOfDecimal(VIS.dataContext.getJSONRecord("VA009/CashJournal/GetPaySheduleAmt", mTab.getValue("VA009_OrderPaySchedule_ID").toString()));
            ctx.setContext(windowNo, "InvTotalAmt", Amount.toString());
            //Get IsSOTrx type from context
            var isSOTrx = "Y" == ctx.getContext(windowNo, "VA009_IsSOTrx");
            if (!isSOTrx) {
                Amount = (Amount) * (-1);
            }
            mTab.setValue("Amount", Amount);
            this.setCalloutActive(false);
            return "";
        }
        else {
            this.setCalloutActive(false);
            return "";
        }
        this.setCalloutActive(false);
        return "";
    }

    VA009.Model = VA009.Model || {};
    VA009.Model.VA009_CalloutCashJournal = VA009_CalloutCashJournal;

    /* Sample END */
})(VA009, jQuery);