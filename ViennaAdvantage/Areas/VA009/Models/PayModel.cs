﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VAdvantage.Utility;
using VAdvantage.Model;
using System.Data;
using VAdvantage.DataBase;
using System.Data.SqlClient;
using System.Text;

namespace VA009.Models
{
    public class PayModel
    {
        /// <summary>
        /// Get Invoice Payment Method
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns>Dictionary</returns>
        public Dictionary<String, object> GetInvPaymentMethod(Ctx ctx, string fields)
        {
            if (fields != null)
            {
                Dictionary<String, object> retDic = null;
                string[] paramValue = fields.ToString().Split(',');
                //Assign parameter value
                int C_PaySchedule_ID = Util.GetValueOfInt(paramValue[0].ToString());
                int AD_Client_ID = Util.GetValueOfInt(paramValue[1].ToString());
                //End Assign parameter
                string _sql = "select IP.VA009_PAYMENTMETHOD_ID, IP.VA009_EXECUTIONSTATUS,IP.DISCOUNTAMT,IP.DUEDATE,IP.VA009_PLANNEDDUEDATE,IP.DUEAMT,PM.VA009_PAYMENTBASETYPE from C_INVOICEPAYSCHEDULE IP"
                              + " inner join VA009_PAYMENTMETHOD PM on PM.VA009_PAYMENTMETHOD_ID = IP.VA009_PAYMENTMETHOD_ID  where IP.C_INVOICEPAYSCHEDULE_ID=" + C_PaySchedule_ID + " AND  IP.IsActive = 'Y' AND IP.AD_Client_ID = " + AD_Client_ID;
                DataSet ds = DB.ExecuteDataset(_sql);

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    retDic = new Dictionary<string, object>();
                    retDic["VA009_PaymentMethod_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["VA009_PaymentMethod_ID"]);
                    retDic["VA009_ExecutionStatus"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_ExecutionStatus"]);
                    retDic["DiscountAmt"] = Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["DiscountAmt"]);
                    retDic["DueDate"] = Util.GetValueOfDateTime(ds.Tables[0].Rows[0]["DueDate"]);
                    retDic["VA009_PlannedDueDate"] = Util.GetValueOfDateTime(ds.Tables[0].Rows[0]["VA009_PlannedDueDate"]);
                    retDic["DueAmt"] = Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["DueAmt"]);
                    retDic["VA009_PaymentBaseType"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PaymentBaseType"]);
                }
                return retDic;
            }
            else
            {
                return null;
            }
        }

        /// Get Order Payment Method
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns>Dictionary</returns>
        public Dictionary<String, object> GetOrdPaymentMethod(Ctx ctx, string fields)
        {
            if (fields != null)
            {
                Dictionary<String, object> retDic = null;
                string[] paramValue = fields.ToString().Split(',');
                //Assign parameter value
                int C_PaySchedule_ID = Util.GetValueOfInt(paramValue[0].ToString());
                int AD_Client_ID = Util.GetValueOfInt(paramValue[1].ToString());
                //End Assign parameter
                string _sql = "select IP.VA009_PAYMENTMETHOD_ID, IP.VA009_EXECUTIONSTATUS,IP.DISCOUNTAMT,IP.DUEDATE,IP.VA009_PLANNEDDUEDATE,IP.DUEAMT,PM.VA009_PAYMENTBASETYPE from "
                            + "VA009_OrderPaySchedule IP inner join VA009_PAYMENTMETHOD PM on PM.VA009_PAYMENTMETHOD_ID = IP.VA009_PAYMENTMETHOD_ID  where IP.VA009_OrderPaySchedule_ID=" + C_PaySchedule_ID + " AND  IP.IsActive = 'Y' AND IP.AD_Client_ID = " + AD_Client_ID;
                DataSet ds = DB.ExecuteDataset(_sql);

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    retDic = new Dictionary<string, object>();
                    retDic["VA009_PaymentMethod_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["VA009_PaymentMethod_ID"]);
                    retDic["VA009_ExecutionStatus"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_ExecutionStatus"]);
                    retDic["VA009_PaymentBaseType"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PaymentBaseType"]);
                }
                return retDic;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Get Journal Detail
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="fields">This Field is used to display the Parameter</param>
        /// <returns>Dictionary</returns>
        public Dictionary<String, object> GetJournalDetail(Ctx ctx, string fields)
        {
            if (fields != null)
            {
                Dictionary<String, object> retDic = null;
                string[] paramValue = fields.ToString().Split(',');
                //Assign parameter value
                int GL_JournalLine_ID = Util.GetValueOfInt(paramValue[0].ToString());
                int C_DocType_ID = Util.GetValueOfInt(paramValue[1].ToString());
                int C_Currency_ID = Util.GetValueOfInt(paramValue[2].ToString());
                DateTime? AcountDate = Util.GetValueOfDateTime(paramValue[3].ToString());
                int AD_Client_ID = Util.GetValueOfInt(paramValue[4].ToString());
                int AD_Org_ID = Util.GetValueOfInt(paramValue[5].ToString());
                int C_ConversionType_ID = Util.GetValueOfInt(paramValue[6].ToString());

                string _sql= @"SELECT GL.AmtSourceDr,GL.AmtSourceCr,GL.C_Currency_ID AS C_Currency_ID,El.AccountType," +
                            "(SELECT DocBaseType FROM c_doctype WHERE C_doctype_Id=" + C_DocType_ID+ ") AS docbaseType FROM " +
                            "GL_Journalline GL INNER JOIN C_ELEMENTVALUE El ON GL.Account_ID=El.C_ELEMENTVALUE_ID WHERE" +
                            " GL.GL_JournalLine_ID=" + GL_JournalLine_ID;
                DataSet ds = DB.ExecuteDataset(_sql,null,null);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    retDic = new Dictionary<string, object>();
                    decimal rate;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if (Util.GetValueOfDecimal(dr["AmtSourceDr"]) != 0)
                        {
                            rate = MConversionRate.Convert(ctx, Util.GetValueOfDecimal(dr["AmtSourceDr"]),
                                Util.GetValueOfInt(dr["C_Currency_ID"]), C_Currency_ID, AcountDate, C_ConversionType_ID,
                                AD_Client_ID, AD_Org_ID);
                            retDic["AmtSourceDr"] = rate;
                            retDic["AmtSourceCr"] = 0;
                        }
                        else
                        {
                            rate = MConversionRate.Convert(ctx, Util.GetValueOfDecimal(dr["AmtSourceCr"]),
                                Util.GetValueOfInt(dr["C_Currency_ID"]), C_Currency_ID, AcountDate, C_ConversionType_ID,
                                AD_Client_ID, AD_Org_ID);
                            retDic["AmtSourceCr"] = rate;
                            retDic["AmtSourceDr"] = 0;
                        }

                        retDic["AccountType"] = Util.GetValueOfString(dr["AccountType"]);
                        retDic["docbaseType"] = Util.GetValueOfString(dr["docbaseType"]);
                    }
                }
                return retDic;
            }
            else
            {
                return null;
            }
        }

        /// Get Due Amount
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns>Dictionary</returns>
        public Dictionary<String, object> GetDueAmt(Ctx ctx, string fields)
        {
            if (fields != null)
            {
                Dictionary<String, object> retDic = null;
                string[] paramValue = fields.ToString().Split(',');
                int VA009_OrderPaySchedule_ID = 0;
                //Assign parameter value
                int C_Order_ID = Util.GetValueOfInt(paramValue[0].ToString());
                //VIS_427 Handled issue to get the amount of only particular selected order schedule
                if (paramValue.Length > 1)
                {
                    VA009_OrderPaySchedule_ID = Util.GetValueOfInt(paramValue[1].ToString());
                }
                //End Assign parameter
                StringBuilder _sql = new StringBuilder();
                _sql.Append(@"SELECT * FROM   (SELECT ips.VA009_OrderPaySchedule_ID, 
                            ips.DueAmt  FROM C_Order i  INNER JOIN VA009_OrderPaySchedule  ips 
                            ON (i.C_Order_ID        =ips.C_Order_ID)  WHERE ips.isactive          ='Y' 
                            AND i.C_Order_ID    = " + C_Order_ID);
                if (VA009_OrderPaySchedule_ID > 0)
                {
                    _sql.Append(" AND VA009_OrderPaySchedule_ID = " + VA009_OrderPaySchedule_ID);
                }
                _sql.Append(@" AND ips.VA009_OrderPaySchedule_ID NOT IN
                            (SELECT NVL(VA009_OrderPaySchedule_ID,0) FROM VA009_OrderPaySchedule  WHERE C_Payment_Id !=0 
                             UNION (SELECT NVL(VA009_OrderPaySchedule_ID,0) FROM C_Payment WHERE DocStatus NOT IN ('CO', 'CL' ,'RE','VO')))
                             AND ips.VA009_ExecutionStatus NOT IN ('Y','J','R') ORDER BY ips.duedate ASC) t WHERE rownum=1");
                DataSet ds = DB.ExecuteDataset(_sql.ToString());

                //VA230:Check if no OrderPaySchedule data found
                if (ds != null && ds.Tables[0].Rows.Count == 0)
                {
                    _sql.Clear();
                    //Get Due amount (GrandTotal-DueAmt) based on orderid when no VA009_OrderPaySchedule_ID found
                    //GrandTotal-Get sum of DueAmt of OrderPaySchedule which are paid
                    _sql.Append(@"SELECT 0 as VA009_OrderPaySchedule_ID, O.GrandTotal - NVL(SUM(S.DueAmt),0) AS DueAmt FROM C_Order O
                                LEFT JOIN VA009_OrderPaySchedule S ON O.C_Order_ID = S.C_Order_ID AND S.IsActive = 'Y' AND S.VA009_IsPaid='Y'
                                WHERE O.C_Order_ID=" + C_Order_ID + " GROUP BY O.GrandTotal");
                    ds = DB.ExecuteDataset(_sql.ToString());
                }

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    retDic = new Dictionary<string, object>();
                    retDic["VA009_OrderPaySchedule_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["VA009_OrderPaySchedule_ID"]);
                    retDic["DueAmt"] = Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["DueAmt"]);
                }
                return retDic;
            }
            else
            {
                return null;
            }
        }

        /// Get Open Amount
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns>Dictionary</returns>
        public Dictionary<String, object> GetOpenAmt(Ctx ctx, string fields)
        {
            if (fields != null)
            {
                Dictionary<String, object> retDic = null;
                string[] paramValue = fields.ToString().Split(',');
                //Assign parameter value
                int C_Order_ID = Util.GetValueOfInt(paramValue[0].ToString());
                DateTime? tsDate = Util.GetValueOfDateTime(paramValue[1].ToString());
                int VA009_OrderPaySchedule_ID = Util.GetValueOfInt(paramValue[2].ToString());
                //End Assign parameter
                string _sql = "SELECT C_BPartner_ID,C_Currency_ID,"
                            + " Orderopen(C_Order_ID, " + VA009_OrderPaySchedule_ID + ") as orderopen,"
                            + " Orderdiscount(C_Order_ID," + GlobalVariable.TO_DATE(tsDate, true) + "," + VA009_OrderPaySchedule_ID + ") as OrderDiscount, IsSOTrx ,IsReturnTrx, C_ConversionType_ID "
                            + "FROM C_Order WHERE C_Order_ID=" + C_Order_ID;
                DataSet ds = DB.ExecuteDataset(_sql);

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    retDic = new Dictionary<string, object>();
                    retDic["C_BPartner_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["C_BPartner_ID"]);
                    retDic["C_Currency_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["C_Currency_ID"]);
                    retDic["orderopen"] = Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["orderopen"]);
                    retDic["OrderDiscount"] = Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["OrderDiscount"]);
                    retDic["IsSOTrx"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["IsSOTrx"]);
                    retDic["IsReturnTrx"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["IsReturnTrx"]);
                    retDic["C_ConversionType_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["C_ConversionType_ID"]);
                }
                return retDic;
            }
            else
            {
                return null;
            }
        }

        /// Get Payment Schedule
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns>Dictionary</returns>
        public Dictionary<string, object> GetSchedule(Ctx ctx, string fields)
        {
            Dictionary<string, object> result = null;
            string[] paramValue = fields.Split(',');
            //Assign parameter value
            int C_INVOICE_ID = Util.GetValueOfInt(paramValue[0].ToString());
            //End Assign parameter
            result = new Dictionary<string, object>();
            //VIS_427 BugId 3082 Handled query to restrict records to not visible if they are drafted against payment and cash journal
            string _sql = @"SELECT C_InvoicePaySchedule_ID FROM C_InvoicePaySchedule WHERE C_INVOICE_ID = " + C_INVOICE_ID+
                           @" AND C_InvoicePaySchedule_ID NOT IN (SELECT CASE WHEN C_Payment.C_Payment_ID != COALESCE(C_PaymentAllocate.C_Payment_ID, 0)
                           THEN COALESCE(C_Payment.C_InvoicePaySchedule_ID,0) ELSE COALESCE(C_PaymentAllocate.C_InvoicePaySchedule_ID,0) END
                           FROM C_Payment LEFT JOIN C_PaymentAllocate ON(C_PaymentAllocate.C_Payment_ID = C_Payment.C_Payment_ID)
                           WHERE C_Payment.DocStatus NOT IN ('CO', 'CL', 'RE', 'VO')) AND VA009_ExecutionStatus NOT IN ('Y','J','R') ORDER BY DueDate";

            result["C_InvoicePaySchedule_ID"] = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
            return result;
        }

        /// <summary>
        /// Get VA009_OrderPaySchedule_ID to check it is avanced payment or not
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="c_Order_ID">c_Order_ID</param>
        /// <returns> bool true or false </returns>
        public bool GetIsAdvanceOrder(Ctx ctx, string c_Order_ID)
        {
            int count = Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT COUNT(VA009_OrderPaySchedule_ID) FROM VA009_OrderPaySchedule pay INNER JOIN C_Order o ON pay.C_Order_ID=o.C_Order_ID WHERE o.IsActive='Y' AND  o.C_Order_ID=" + c_Order_ID, null, null));
            return count > 0 ? true : false;
        }

        /// Get Mandate Value from selected Payment Method
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns>Dictionary</returns>
        public Dictionary<string, object> GetMandate(Ctx ctx, string fields)
        {
            Dictionary<string, object> result = null;
            string[] paramValue = fields.Split(',');
            //Assign parameter value
            int _paymentMethod = Util.GetValueOfInt(paramValue[0].ToString());
            int _Client = Util.GetValueOfInt(paramValue[1].ToString());
            //End Assign parameter
            result = new Dictionary<string, object>();
            string _sql = "Select VA009_IsMandate From VA009_PaymentMethod Where VA009_PaymentMethod_ID=" + _paymentMethod + " And IsActive ='Y' AND AD_Client_ID=" + _Client;

            result["VA009_IsMandate"] = Util.GetValueOfString(DB.ExecuteScalar(_sql));
            return result;
        }

        /// Get Payment Rule
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns>Dictionary</returns>
        public Dictionary<string, object> GetPaymentRule(Ctx ctx, string fields)
        {
            Dictionary<string, object> result = null;
            string[] paramValue = fields.Split(',');
            //Assign parameter value
            int _paymentMethod_ID = Util.GetValueOfInt(paramValue[0].ToString());
            //End Assign parameter
            result = new Dictionary<string, object>();
            string _sql = "select VA009_PAYMENTTRIGGER,VA009_PAYMENTRULE,VA009_PAYMENTBASETYPE,C_Currency_ID from VA009_PAYMENTMETHOD where VA009_PAYMENTMETHOD_ID=" + _paymentMethod_ID;
            DataSet ds = DB.ExecuteDataset(_sql);

            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                result["VA009_PAYMENTTRIGGER"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PAYMENTTRIGGER"]);
                result["VA009_PAYMENTRULE"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PAYMENTRULE"]);
                result["VA009_PAYMENTBASETYPE"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PAYMENTBASETYPE"]);
                result["C_Currency_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["C_Currency_ID"]);
            }
            return result;
        }

        /// Get Business Partner Payment Rule
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns>Dictionary</returns>
        public Dictionary<string, object> GetBPPaymentRule(Ctx ctx, string fields)
        {
            Dictionary<string, object> result = null;
            string[] paramValue = fields.Split(',');
            //Assign parameter value
            int C_BPartner_ID = Util.GetValueOfInt(paramValue[0].ToString());
            //End Assign parameter
            result = new Dictionary<string, object>();
            //VA009_PO_PaymentMethod_ID added new column for enhancement.. Google Sheet ID-- SI_0036
            string _sql = "SELECT pm.va009_paymentbasetype, PMM.VA009_PAYMENTBASETYPE AS VA009_PAYMENTBASETYPEPO, cb.va009_paymentmethod_id,cb.VA009_PO_PaymentMethod_ID,cb.ISVENDOR,cb.IsCustomer FROM c_bpartner cb left join VA009_PAYMENTMETHOD PM ON cb.VA009_PAYMENTMETHOD_ID=PM.VA009_PAYMENTMETHOD_ID left join VA009_PAYMENTMETHOD PMM ON cb.VA009_PO_PAYMENTMETHOD_ID = PMM.VA009_PAYMENTMETHOD_ID  WHERE c_bpartner_id=" + C_BPartner_ID;
            DataSet ds = DB.ExecuteDataset(_sql);

            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                result["va009_paymentbasetype"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["va009_paymentbasetype"]);
                result["va009_paymentmethod_id"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["va009_paymentmethod_id"]);
                result["ISVENDOR"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["ISVENDOR"]);
                result["IsCustomer"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["IsCustomer"]);
                result["va009_paymentbasetypePO"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PAYMENTBASETYPEPO"]);
                result["VA009_PO_PaymentMethod_ID"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PO_PaymentMethod_ID"]);
            }
            return result;
        }

        /// Get Invoice Payment Rule
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns>Dictionary</returns>
        public Dictionary<string, object> GetInvoicePaymentRule(Ctx ctx, string fields)
        {
            Dictionary<string, object> result = null;
            string[] paramValue = fields.Split(',');
            //Assign parameter value
            int C_Invoice_ID = Util.GetValueOfInt(paramValue[0].ToString());
            //End Assign parameter
            result = new Dictionary<string, object>();
            string _sql = "SELECT pm.va009_paymentbasetype,cb.va009_paymentmethod_id FROM c_invoice cb INNER JOIN va009_paymentmethod pm ON cb.va009_paymentmethod_id=pm.va009_paymentmethod_id WHERE cb.c_invoice_id=" + C_Invoice_ID;
            DataSet ds = DB.ExecuteDataset(_sql);

            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                result["va009_paymentbasetype"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["va009_paymentbasetype"]);
                result["va009_paymentmethod_id"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["va009_paymentmethod_id"]);
            }
            return result;
        }

        /// Get Payment Allocate Amount
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns>Dictionary</returns>
        public Dictionary<String, object> GetPayAllocateAmt(Ctx ctx, string fields)
        {
            if (fields != null)
            {
                Dictionary<String, object> retDic = null;
                string[] paramValue = fields.ToString().Split(',');
                //Assign parameter value
                int C_Payment_ID = Util.GetValueOfInt(paramValue[0].ToString());
                int C_Invoice_ID = Util.GetValueOfInt(paramValue[1].ToString());
                int c_invoicepayschedule_id = Util.GetValueOfInt(paramValue[2].ToString());
                //DateTime? date = Util.GetValueOfDateTime(paramValue[1]);
                DateTime? date = Util.GetValueOfDateTime(DB.ExecuteScalar("SELECT DateTrx FROM C_Payment WHERE C_Payment_ID=" + C_Payment_ID));
                //End Assign parameter
                //string _sql = "SELECT dueamt,  CASE    WHEN (TRUNC(discountdate) >= TRUNC(" + GlobalVariable.TO_DATE(date, true) + "))    THEN DiscountAmt    WHEN (TRUNC(discountdays2) >= TRUNC(" + GlobalVariable.TO_DATE(date, true) + ")"
                //            + " AND TRUNC(" + GlobalVariable.TO_DATE(date, true) + ")          > TRUNC(discountdate))    THEN Discount2    ELSE 0  END AS discount FROM c_invoicepayschedule WHERE c_invoicepayschedule_id=" + c_invoicepayschedule_id;

                string _sql = "SELECT NVL(p.DueAmt , 0) - NVL(p.VA009_PaidAmntInvce , 0) as invoiceOpen,"
                     + " CASE WHEN (TRUNC(p.discountdate) >= TRUNC(" + GlobalVariable.TO_DATE(date, true) + ")) THEN p.DiscountAmt WHEN (TRUNC(p.discountdays2) >= TRUNC(" + GlobalVariable.TO_DATE(date, true) + ")"
                     + " AND TRUNC(" + GlobalVariable.TO_DATE(date, true) + ") > TRUNC(p.discountdate)) THEN p.Discount2 ELSE 0  END AS invoiceDiscount,"
                     + " i.IsSOTrx, i.IsReturnTrx"
                     + " FROM C_Invoice i INNER JOIN C_InvoicePaySchedule p ON p.C_Invoice_ID = i.C_Invoice_ID"
                     + " WHERE i.C_Invoice_ID=" + C_Invoice_ID + " AND p.C_InvoicePaySchedule_ID=" + c_invoicepayschedule_id;

                DataSet ds = DB.ExecuteDataset(_sql);

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    retDic = new Dictionary<string, object>();
                    retDic["Dueamt"] = Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["invoiceOpen"]);
                    retDic["discount"] = Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["invoiceDiscount"]);
                    retDic["IsReturnTrx"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["IsReturnTrx"]);
                }
                return retDic;
            }
            else
            {
                return null;
            }
        }


        /// Get Bank Details
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns>Dictionary</returns>
        public Dictionary<String, object> GetBankDetails(Ctx ctx, string fields)
        {
            if (fields != null)
            {
                Dictionary<String, object> retDic = null;
                string[] paramValue = fields.ToString().Split(',');
                //Assign parameter value
                int C_Order_ID = Util.GetValueOfInt(paramValue[0].ToString());
                //End Assign parameter

                MBank _bank = new MBank(ctx, C_Order_ID, null);
                if (_bank != null)
                {
                    retDic = new Dictionary<string, object>();
                    retDic["C_Location_ID"] = _bank.GetC_Location_ID();
                    retDic["RoutingNo"] = _bank.GetRoutingNo();
                    return retDic;
                }
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        /// Get Payment Base Type
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns>Dictionary</returns>
        public Dictionary<String, object> GetPayBaseType(Ctx ctx, string fields)
        {
            if (fields != null)
            {
                Dictionary<String, object> retDic = null;
                string[] paramValue = fields.ToString().Split(',');
                //Assign parameter value
                int C_DocType_ID = Util.GetValueOfInt(paramValue[0].ToString());
                //End Assign parameter

                MDocType _doctype = new MDocType(ctx, C_DocType_ID, null);
                if (_doctype != null)
                {
                    retDic = new Dictionary<string, object>();
                    retDic["DocBaseType"] = _doctype.GetDocBaseType();
                    return retDic;
                }
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get Bank Currency
        /// </summary>  Arpit to Get Bank Currency
        /// <param name="ctx"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public Dictionary<String, object> GetBankCurrency(Ctx ctx, string fields)
        {
            if (fields != null)
            {
                try
                {
                    Dictionary<String, object> retDic = null;
                    string[] paramValue = fields.ToString().Split(',');
                    //Assign parameter value
                    int BankAccount_ID = Util.GetValueOfInt(paramValue[0].ToString());
                    //End Assign parameter

                    MBankAccount BankAcct = new MBankAccount(ctx, BankAccount_ID, null);
                    if (BankAcct != null)
                    {
                        retDic = new Dictionary<string, object>();
                        retDic["C_Currency_ID"] = BankAcct.GetC_Currency_ID();
                        return retDic;
                    }
                    else
                        return null;
                }
                catch (Exception ex)
                {
                    String msg_ = ex.Message;
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        //this method is used to veify the payment base type of payment method
        // payment base type is Cash then return 0 else 1
        public int VerifyPayMethod(Ctx ctx, string fields)
        {
            if (fields != null)
            {
                var docBaseType = Util.GetValueOfString(DB.ExecuteScalar(@"SELECT VA009_PaymentBaseType FROM VA009_PaymentMethod WHERE 
                          VA009_PaymentMethod_ID=" + Convert.ToInt32(fields)));
                if (docBaseType == "B" || docBaseType == "C" || docBaseType == "P")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            return 1;
        }

        public Dictionary<string, object> GetScheduleData(Ctx ctx, string fields)
        {
            Dictionary<string, object> result = null;
            string[] paramValue = fields.Split(',');
            //Assign parameter value
            int C_Schedule_ID = Util.GetValueOfInt(paramValue[0].ToString());
            int C_PaySelection_ID = Util.GetValueOfInt(paramValue[2].ToString());
            MPaySelection paysel = new MPaySelection(ctx, C_PaySelection_ID, null);
            //End Assign parameter
            result = new Dictionary<string, object>();
            string _sql = @"SELECT   i.C_Invoice_ID,  currencyConvert(ips.DUEAMT,ips.C_Currency_ID, " + paysel.GetC_Currency_ID() + @",TO_DATE('2018-12-21','YYYY-MM-DD'), i.C_ConversionType_ID,ips.AD_Client_ID,ips.AD_Org_ID) AS OpenAmt,
                          I.PAYMENTRULE,  ips.VA009_PAYMENTMETHOD_ID,  i.IsSOTrx,  CASE    WHEN (TO_DATE('2018-12-21','YYYY-MM-DD') <= IPS.DISCOUNTDATE)    THEN IPS.DISCOUNTAMT    ELSE 0  END AS DISCOUNT1,
                          CASE    WHEN (TO_DATE('2018-12-21','YYYY-MM-DD') > IPS.DISCOUNTDATE    AND TO_DATE('2018-12-21','YYYY-MM-DD')  <= IPS.DISCOUNTDAYS2)    THEN IPS.DISCOUNT2    ELSE 0  END AS DISCOUNT2
                          FROM C_INVOICEPAYSCHEDULE ips INNER JOIN C_INVOICE i on IPS.C_INVOICE_ID   =I.C_INVOICE_ID WHERE  ips.C_INVOICEPAYSCHEDULE_ID=" + C_Schedule_ID;
            DataSet ds = DB.ExecuteDataset(_sql);

            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                result["C_Invoice_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["C_Invoice_ID"]);
                result["OpenAmt"] = Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["OpenAmt"]);
                result["PAYMENTRULE"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["PAYMENTRULE"]);
                result["VA009_PAYMENTMETHOD_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["VA009_PAYMENTMETHOD_ID"]);
                result["IsSOTrx"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["IsSOTrx"]);
                result["DISCOUNT1"] = Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["DISCOUNT1"]);
                result["DISCOUNT2"] = Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["DISCOUNT2"]);
            }
            return result;

        }

        /// Get Bank Details
        /// </summary>
        /// <param name="ctx">context </param>
        /// <param name="fields">Account ID</param>
        /// <returns>Dictionary</returns>
        public Dictionary<String, object> getaccountdetails(Ctx ctx, string fields)
        {
            if (fields != null)
            {
                Dictionary<String, object> retDic = null;
                string[] paramValue = fields.ToString().Split(',');
                //Assign parameter value
                int C_BP_BankAccount_ID = Util.GetValueOfInt(paramValue[0].ToString());
                //End Assign parameter
                retDic = new Dictionary<string, object>();
                //changes done for adding routing number and account number for batch 
                DataSet ds = DB.ExecuteDataset(@" SELECT a_name , RoutingNo , AccountNo 
                             FROM C_BP_BankAccount WHERE C_BP_BankAccount_ID = " + C_BP_BankAccount_ID);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    retDic["a_name"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["a_name"]);
                    retDic["RoutingNo"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["RoutingNo"]);
                    retDic["AccountNo"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["AccountNo"]);
                }
                return retDic;
            }
            else
            {
                return null;
            }
        }
    }
}