using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAdvantage.Classes;
using VAdvantage.Common;
using VAdvantage.Process;
using VAdvantage.Model;
using VAdvantage.DataBase;
using VAdvantage.SqlExec;
using VAdvantage.Utility;
using System.Data;
using System.Data.SqlClient;
using VAdvantage.Logging;
using VAdvantage.ProcessEngine;
using ViennaAdvantage.Model;

namespace ViennaAdvantage.Process
{
    public class VA009_Generate_Payment : SvrProcess
    {
        #region Variables
        string IsBankresponse = "N";
        //string msg = "VA009_PymentSaved";
        string msg = String.Empty;
        String checkMsg = String.Empty;
        StringBuilder docNos = new StringBuilder();
        StringBuilder sql = new StringBuilder();
        int countresponse = 0;
        string invDocNo = "";
        MVA009BatchLineDetails batchLineDetails = null;
        MVA009BatchLines batchLines = null;

        private List<int> payment = new List<int>();
        private List<int> viewAllocationId = new List<int>();
        private string tenderType = string.Empty;
        private string createdDocType = string.Empty;

        private string viewAllocationRecords = string.Empty;
        private string PaymentCreated = string.Empty;
        private string errorMsg = string.Empty;

        private string paymentDocumentNo = string.Empty;
        private string allocationDocumentNo = string.Empty;

        X_C_BankAccountDoc BAcctDoc = null;
        //bool CurrtNxtUpdated = false;
        Decimal discountAmt, DueAmount = 0, _ConvertedAmt = 0;
        MPayment _pay = null;
        Int32 currencyTo_ID = 0, BlineDetailCur_ID = 0, _ConversionType_ID;

        private int allocationId = 0;

        #endregion

        protected override void Prepare()
        {
            ProcessInfoParameter[] para = GetParameter();
            for (int i = 0; i < para.Length; i++)
            {
                String name = para[i].GetParameterName();
                if (para[i].GetParameter() == null && para[i].GetParameter_To() == null)
                {
                    ;
                }
                else if (name.Equals("VA009_IsBankresponse"))
                {
                    IsBankresponse = Util.GetValueOfString(para[i].GetParameter());
                }
                //else if (name.Equals("C_Currency_ID"))
                //{
                //    currency_ID = Util.GetValueOfInt(para[i].GetParameter());
                //}

            }
        }

        #region Commented Code
        //        protected override string DoIt()
        //        {
        //            StringBuilder _sql = new StringBuilder();
        //            string _msg = string.Empty;
        //            MVA009Batch batch = new MVA009Batch(GetCtx(), GetRecord_ID(), null);
        //            MPayment _pay = null;
        //            _Batch_ID = GetRecord_ID();
        //            _sql.Clear();
        //            _sql.Append(@"SELECT b.c_bankaccount_id, BL.VA009_BatchLines_ID,   bl.c_bpartner_id,bl.va009_consolidate,  CASE    WHEN bl.va009_consolidate='Y'    THEN 0    ELSE bd.c_invoice_id
        //                          END AS c_invoice_id,      CASE     WHEN bl.va009_consolidate='Y'    THEN 0    Else BD.c_invoicepayschedule_id    END AS c_invoicepayschedule_id,  i.c_currency_id,bd.c_currency_id,
        //                          bl.va009_consolidate,  DBT.DocBaseType,  I.ad_org_id,  SUM(bd.dueamt) AS dueamt FROM VA009_Batch B INNER JOIN VA009_Batchlines BL ON B.VA009_Batch_id= BL.VA009_Batch_ID
        //                          INNER JOIN VA009_BatchLineDetails BD ON BL.VA009_BatchLines_ID= BD.VA009_BatchLines_ID INNER JOIN C_Invoice I ON BD.C_Invoice_ID = I.C_Invoice_ID LEFT JOIN C_DocType DT
        //                          ON I.C_DocType_ID = DT.C_DocType_ID LEFT JOIN C_DocBaseType DBT ON DBT.docbasetype    = DT.docbasetype WHERE B.VA009_batch_ID=" + _Batch_ID + " AND dbt.docbasetype IN ('ARI', 'API') And B.IsActive = 'Y' AND B.AD_Client_ID = " + GetAD_Client_ID());

        //            DataSet ds = new DataSet();
        //            MVA009PaymentMethod _PayMethd = new MVA009PaymentMethod(GetCtx(), batch.GetVA009_PaymentMethod_ID(), Get_TrxName());
        //            if (_PayMethd.GetVA009_PaymentRule() == "E")
        //            {
        //                if (_PayMethd.IsVA009_InitiatePay() == false)
        //                {
        //                    int countresponse = Util.GetValueOfInt(DB.ExecuteScalar("SELECT bd.VA009_BankResponse FROM va009_batchlinedetails bd INNER JOIN va009_batchlines bl ON bl.va009_batchlines_id=bd.va009_batchlines_id WHERE bl.va009_batch_id=" + batch.GetVA009_Batch_ID() + " AND bd.VA009_BankResponse NOT IN  ('RE','RJ') AND bd.AD_Client_ID = " + GetAD_Client_ID() + " Group by bd.VA009_BankResponse ", null, Get_TrxName()));
        //                    if (countresponse > 0)
        //                        _sql.Append(@"AND BD.VA009_BankResponse IN ('RE','RJ') GROUP BY ( b.c_bankaccount_id, BL.VA009_BatchLines_ID,bl.c_bpartner_id,bl.va009_consolidate,  CASE    WHEN bl.va009_consolidate='Y'    THEN 0    ELSE bd.c_invoice_id  END, CASE     WHEN bl.va009_consolidate='Y'
        //                          THEN 0    Else BD.c_invoicepayschedule_id    END, i.c_currency_id,bd.c_currency_id, bl.va009_consolidate, DBT.DocBaseType, I.ad_org_id) ORDER BY bl.c_bpartner_id ASC");
        //                    else
        //                        _sql.Append(@"GROUP BY ( b.c_bankaccount_id,BL.VA009_BatchLines_ID, bl.c_bpartner_id,bl.va009_consolidate,  CASE    WHEN bl.va009_consolidate='Y'    THEN 0    ELSE bd.c_invoice_id  END, CASE     WHEN bl.va009_consolidate='Y'
        //                          THEN 0    Else BD.c_invoicepayschedule_id    END, i.c_currency_id,bd.c_currency_id, bl.va009_consolidate, DBT.DocBaseType, I.ad_org_id) ORDER BY bl.c_bpartner_id ASC");
        //                }
        //            }
        //            ds = DB.ExecuteDataset(_sql.ToString());
        //            if (ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        //            {
        //                _DoctypeARR = Util.GetValueOfInt(DB.ExecuteScalar("Select  dt.c_doctype_id  From C_doctype DT inner join c_docbasetype DBT On dt.docbasetype=dbt.docbasetype where dbt.docbasetype='ARR' AND dt.IsActive = 'Y' AND DT.AD_Client_ID = " + GetAD_Client_ID()));
        //                _DoctypeAPP = Util.GetValueOfInt(DB.ExecuteScalar("Select  dt.c_doctype_id  From C_doctype DT inner join c_docbasetype DBT On dt.docbasetype=dbt.docbasetype where dbt.docbasetype='APP' AND  dt.IsActive = 'Y' AND DT.AD_Client_ID = " + GetAD_Client_ID()));
        //                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //                {
        //                    _pay = new MPayment(GetCtx(), 0, null);
        //                    _invoice = Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoice_id"]);
        //                    _DocBaseType = Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]);
        //                    if (_DocBaseType == "ARI")
        //                    {
        //                        _pay.SetC_DocType_ID(_DoctypeARR);
        //                    }
        //                    else
        //                    {
        //                        _pay.SetC_DocType_ID(_DoctypeAPP);
        //                    }
        //                    if (_invoice > 0)
        //                    {

        //                        _pay.SetC_Invoice_ID(_invoice);
        //                        _pay.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoicepayschedule_id"]));
        //                    }
        //                    _pay.SetAD_Client_ID(GetCtx().GetAD_Client_ID());
        //                    _pay.SetAD_Org_ID(GetCtx().GetAD_Org_ID());
        //                    _pay.SetDateAcct(System.DateTime.Now);
        //                    _pay.SetDateTrx(System.DateTime.Now);
        //                    _pay.SetC_BankAccount_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bankaccount_id"]));
        //                    _pay.SetC_BPartner_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]));
        //                    _pay.SetPayAmt(Util.GetValueOfInt(ds.Tables[0].Rows[i]["dueamt"]));
        //                    _pay.SetC_Currency_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]));
        //                    if (!_pay.Save())
        //                    {
        //                        _msg = Msg.GetMsg(GetCtx(), "VA009_PaymtNtCrtd");
        //                    }
        //                    else
        //                        _msg = Msg.GetMsg(GetCtx(), "VA009_PaymntCrted");
        //                }
        //                batch.SetVA009_GeneratePayment("Y");
        //                batch.SetProcessed(true);
        //                batch.Save();
        //                return _msg;
        //            }
        //            else
        //                return Msg.GetMsg(GetCtx(), "VA009_PaymtNtCrtd");
        //        }
        #endregion

        protected override string DoIt()
        {
            //Check Bank Response
            //            sql.Append(@"SELECT count(bd.VA009_BankResponse) FROM va009_batchlinedetails bd INNER JOIN va009_batchlines bl ON bl.va009_batchlines_id=bd.va009_batchlines_id
            //                          WHERE bl.va009_batch_id=" + GetRecord_ID() + " AND bd.VA009_BankResponse='IP' AND bd.AD_Client_ID = " + GetAD_Client_ID() + " Group by bd.VA009_BankResponse ");
            //            countresponse = Util.GetValueOfInt(DB.ExecuteScalar(sql.ToString(), null, Get_TrxName()));
            bool isPaymentAlreadyGenerated = CheckPaymentStatus();
            if (!isPaymentAlreadyGenerated)
            {
                sql.Clear();
                try
                {
                    //Rakesh(VA228):Fetched discount amount from Batch detail line instead of invoice and order pay schedule on 17/Sep/2021 as discussed with Ranvir
                    sql.Append(@"SELECT T.C_ConversionType_ID,
                               T.c_bankaccount_id, 
                               T.c_bpartner_id,
                               T.C_BPartner_Location_ID, 
                               T.c_currency_id, 
                               T.c_invoice_id,
                               T.C_order_ID,  
                               T.dueamt, 
                               T.VA009_ConvertedAmt, 
                               T.discountamt,
                               T.va009_batchlinedetails_ID ,
                               T.va009_batchlines_id , 
                               T.discountdate,
                               T.issotrx,
                               T.isreturntrx,
                               T.DocumentNo,
                               T.c_invoicepayschedule_id,
                               T.VA009_OrderPaySchedule_ID,
                               T.ad_org_id, 
                               T.ad_client_id , 
                               T.DocBaseType ,
                               T.va009_paymentmethod_id ,
                               T.VA009_PAYMENTBASETYPE,
                               T.VA009_DueAmount,
                               T.Currency_ID,
                               T.C_BP_BankAccount_ID,
                               T.swiftcode, 
                               T.Acctnumber,
                               T.AcctName
                                FROM (");
                    sql.Append(@"SELECT bld.C_ConversionType_ID,
                               b.c_bankaccount_id, 
                               bl.c_bpartner_id, 
                               inv.C_BPartner_Location_ID, 
                               bld.c_currency_id, 
                               bld.c_invoice_id,
                               NULL AS C_order_ID,  
                               IPS.DueAmt, 
                               bld.VA009_ConvertedAmt, 
                               bld.discountamt,
                               bld.va009_batchlinedetails_ID ,
                               bl.va009_batchlines_id , 
                               bld.discountdate,
                               inv.issotrx,
                               inv.isreturntrx,
                               inv.DocumentNo,
                               bld.c_invoicepayschedule_id,
                               NULL AS VA009_OrderPaySchedule_ID,
                               bld.ad_org_id, 
                               bld.ad_client_id , 
                               doc.DocBaseType ,
                               b.va009_paymentmethod_id ,
                               PM.VA009_PAYMENTBASETYPE,
                               bl.VA009_DueAmount,
                               bl.C_BP_BankAccount_ID,
                               bl.RoutingNo as swiftcode, bl.AccountNo as Acctnumber, bl.A_Name as AcctName,
                               IPS.C_Currency_ID AS Currency_ID
                               FROM va009_batchlinedetails bld 
                               INNER JOIN va009_batchlines bl ON bl.va009_batchlines_id=bld.va009_batchlines_id 
                               INNER JOIN va009_batch b ON b.va009_batch_id =bl.va009_batch_id 
                               INNER JOIN c_invoice inv ON inv.c_invoice_id = bld.c_invoice_id
                               INNER JOIN C_DocType doc ON doc.C_Doctype_ID = inv.C_Doctype_ID
                               INNER JOIN C_InvoicePaySchedule IPS ON IPS.c_invoicepayschedule_id = bld.c_invoicepayschedule_id
                               INNER JOIN VA009_PaymentMethod PM ON PM.VA009_PaymentMethod_ID = b.VA009_PaymentMethod_ID
                               WHERE  NVL(bl.c_payment_id , 0) = 0 
                                      AND NVL(bld.c_payment_id , 0) = 0 
                                      AND NVL(bld.C_AllocationHdr_ID , 0) = 0 
                                      AND  b.va009_batch_id    =" + GetRecord_ID());
                    // if (IsBankresponse == "Y")
                    //    sql.Append(" AND bld.va009_bankresponse='RE' ORDER BY bl.c_bpartner_id ASC ");
                    // else if (IsBankresponse == "N")
                    // sql.Append(" ORDER BY bld.va009_batchlines_id ,  bl.c_bpartner_id ASC ");   
                    //Added by Arpit TO Create Payment against Order as well as Invoice on 14the Dec,2016
                    sql.Append(@" UNION SELECT bld.C_ConversionType_ID,b.c_bankaccount_id, 
                               bl.c_bpartner_id, 
                               odr.C_BPartner_Location_ID, 
                               bld.c_currency_id, 
                               NULL AS C_Invoice_ID,
                               bld.C_order_ID,  
                               OPS.DueAmt, 
                               bld.VA009_ConvertedAmt, 
                               bld.discountamt,
                               bld.va009_batchlinedetails_ID ,
                               bl.va009_batchlines_id , 
                               bld.discountdate,
                               odr.issotrx,
                               odr.isreturntrx,
                               odr.DocumentNo,
                               NULL AS C_InvoicePaySchedule_ID,
                               bld.VA009_OrderPaySchedule_ID,
                               bld.ad_org_id, 
                               bld.ad_client_id , 
                               doc.DocBaseType ,
                               b.va009_paymentmethod_id ,
                               PM.VA009_PAYMENTBASETYPE,
                               bl.VA009_DueAmount,
                               bl.C_BP_BankAccount_ID,
                               bl.RoutingNo as swiftcode, bl.AccountNo as Acctnumber, bl.A_Name as AcctName,
                                OPS.C_Currency_ID AS Currency_ID
                               FROM va009_batchlinedetails bld 
                               INNER JOIN va009_batchlines bl ON bl.va009_batchlines_id=bld.va009_batchlines_id 
                               INNER JOIN va009_batch b ON b.va009_batch_id =bl.va009_batch_id 
                               INNER JOIN C_Order odr ON odr.C_Order_ID = bld.C_Order_ID
                               INNER JOIN C_DocType doc ON doc.C_Doctype_ID = odr.C_Doctype_ID
                               INNER JOIN VA009_OrderPaySchedule OPS on OPS.VA009_OrderPaySchedule_ID=bld.VA009_OrderPaySchedule_ID
                               INNER JOIN VA009_PaymentMethod PM ON PM.VA009_PaymentMethod_ID = b.VA009_PaymentMethod_ID
                               WHERE  NVL(bl.c_payment_id , 0) = 0 
                                      AND NVL(bld.c_payment_id , 0) = 0 
                                      AND NVL(bld.C_AllocationHdr_ID , 0) = 0 
                                      AND  b.va009_batch_id    =" + GetRecord_ID());
                    sql.Append(")T");
                    if (IsBankresponse == "Y")
                        sql.Append(" AND T.va009_bankresponse='RE' ORDER BY T.c_bpartner_id, T.VA009_paymentmethod_ID  ASC ");
                    else if (IsBankresponse == "N")
                        sql.Append(" ORDER BY T.va009_batchlines_id ,  T.c_bpartner_id , T.VA009_paymentmethod_ID ASC ");
                    //Changes --Changed above queries to get Invoice Amount from Invoice Schedule/Order Schedule to convert the amount on same day when payment is generating
                    //End here (Arpit)
                    //else if (IsBankresponse == "Y" && countresponse > 0)
                    //    return Msg.GetMsg(GetCtx(), "VA009_AllResponseNotAvailable");

                    DataSet ds = DB.ExecuteDataset(sql.ToString(), null, Get_TrxName());

                    MVA009Batch _batch = new MVA009Batch(GetCtx(), GetRecord_ID(), Get_TrxName());

                    currencyTo_ID = Util.GetValueOfInt(_batch.Get_Value("C_Currency_ID"));
                    //Rakesh(VA228):Set ConversionTypeId
                    _ConversionType_ID = _batch.GetC_ConversionType_ID();

                    if (ds != null && ds.Tables[0].Rows.Count > 0)
                    {
                        #region Update Payment Execution true on Invoice Payment Schedule
                        sql.Clear();
                        try
                        {
                            sql.Append(@" SELECT BL.VA009_BatchLines_ID,BDL.VA009_BatchLineDetails_ID,NVL(BDL.VA009_OrderPaySchedule_ID,0) VA009_OrderPaySchedule_ID,
                                  NVL(BDL.C_InvoicePaySchedule_ID,0) C_InvoicePaySchedule_ID FROM VA009_BatchLines BL 
                                  INNER JOIN VA009_BatchLineDetails BDL ON BL.VA009_BatchLines_ID=BDL.VA009_BatchLines_ID
                                  WHERE BL.IsActive='Y' AND  BDL.IsActive='Y' AND BL.VA009_Batch_ID=" + GetRecord_ID());

                            using (DataSet dSet = DB.ExecuteDataset(sql.ToString(), null, Get_TrxName()))
                            {
                                if (dSet != null && dSet.Tables[0].Rows.Count > 0)
                                {
                                    int oldBline_ID = 0;
                                    MVA009BatchLines bLines = null;
                                    for (Int32 i = 0; i < dSet.Tables[0].Rows.Count; i++)
                                    {
                                        if (bLines == null)
                                        {
                                            bLines = new MVA009BatchLines(GetCtx(), Util.GetValueOfInt(dSet.Tables[0].Rows[i]["VA009_BatchLines_ID"]), Get_TrxName());
                                            oldBline_ID = Util.GetValueOfInt(dSet.Tables[0].Rows[i]["VA009_BatchLines_ID"]);
                                        }
                                        else if (oldBline_ID != Util.GetValueOfInt(dSet.Tables[0].Rows[i]["VA009_BatchLines_ID"]))
                                        {
                                            oldBline_ID = Util.GetValueOfInt(dSet.Tables[0].Rows[i]["VA009_BatchLines_ID"]);
                                            if (bLines != null)
                                            {
                                                bLines.SetProcessed(true);
                                                if (!bLines.Save(Get_TrxName()))
                                                {
                                                    return ErrorMessage();
                                                }
                                            }
                                            bLines = new MVA009BatchLines(GetCtx(), Util.GetValueOfInt(dSet.Tables[0].Rows[i]["VA009_BatchLines_ID"]), Get_TrxName());
                                        }
                                        MVA009BatchLineDetails bLineDetails = new MVA009BatchLineDetails(GetCtx(), Util.GetValueOfInt(dSet.Tables[0].Rows[i]["VA009_BatchLineDetails_ID"]), Get_TrxName());
                                        bLineDetails.SetProcessed(true);
                                        if (!bLineDetails.Save(Get_TrxName()))
                                        {
                                            return ErrorMessage();
                                        }
                                        else
                                        {
                                            if (Util.GetValueOfInt(dSet.Tables[0].Rows[i]["C_InvoicePaySchedule_ID"]) > 0)
                                            {
                                                MInvoicePaySchedule IPSchedule = new MInvoicePaySchedule(GetCtx(), Util.GetValueOfInt(dSet.Tables[0].Rows[i]["C_InvoicePaySchedule_ID"]), Get_TrxName());
                                                IPSchedule.SetVA009_ExecutionStatus("Y");
                                                if (!IPSchedule.Save(Get_TrxName()))
                                                {
                                                    return ErrorMessage();
                                                }
                                            }
                                            #region When Order Pay Shcedule will handled then this method will be called
                                            /*
                                        else if(Util.GetValueOfInt(dSet.Tables[0].Rows[i]["VA009_OrderPaySchedule_ID"]) > 0)
                                        {
                                           String eMsg= SetOrderScheduleExecutionStatus(dSet, i);
                                           if (!String.IsNullOrEmpty(eMsg))
                                           {
                                               return eMsg;
                                           }
                                        }
                                             */
                                            #endregion
                                        }
                                        if (i == dSet.Tables[0].Rows.Count - 1)
                                        {
                                            bLines.SetProcessed(true);
                                            if (!bLines.Save(Get_TrxName()))
                                            {
                                                return ErrorMessage();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    msg = Msg.GetMsg(GetCtx(), "VA009_LinesNotAvailable");
                                    return msg;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            return e.Message;
                        }
                        #endregion

                        #region Consolidate  = true
                        if (_batch.IsVA009_Consolidate() == true)
                        {
                            decimal totalPayAmt = 0;
                            //Issue ID In Google Sheet: SI_0673 --> While generating payment from Payment Schedule Batch, "Consolidate Payment" checkbox is true & there are multiple Payment method on payment batch lines, System creates single payment record for all schedules.
                            int c_currency_id = 0; int Bpartner_ID = 0; int C_Payment_ID = 0, batchline_id = 0, allocationHeader = 0, VA009_PaymentMethod_ID = 0;



                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]).Equals(MDocBaseType.DOCBASETYPE_APCREDITMEMO) &&
                                   Util.GetValueOfString(ds.Tables[0].Rows[i]["VA009_PAYMENTBASETYPE"]).Equals(MVA009PaymentMethod.VA009_PAYMENTBASETYPE_Check))
                                {
                                    //Payment should not be crteated if Payment Method is check and APC Invoice
                                    invDocNo += ", " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["DocumentNo"]);
                                    continue;
                                }

                                BlineDetailCur_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Currency_ID"]);
                                if (currencyTo_ID > 0)
                                {
                                    BlineDetailCur_ID = currencyTo_ID;
                                    //Getting the Currency From Window
                                }
                                discountAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]);
                                DueAmount = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]);
                                //Rakesh(VA228):Get converted amount
                                _ConvertedAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]);

                                #region Commented Not Required to Convert
                                //Rakesh(VA228)::Not requird to convert as already converted in respective currency
                                // Convert Amounts if currency is not same...conversion must be at the same day on which payment is generating
                                //if (Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Currency_ID"]) != Util.GetValueOfInt(ds.Tables[0].Rows[i]["Currency_ID"]))
                                //if (BlineDetailCur_ID != Util.GetValueOfInt(ds.Tables[0].Rows[i]["Currency_ID"]))
                                //{
                                //    if (discountAmt > 0)
                                //    {
                                //        discountAmt = MConversionRate.Convert(GetCtx(), discountAmt, Util.GetValueOfInt(ds.Tables[0].Rows[i]["Currency_ID"]),
                                //               BlineDetailCur_ID,//Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Currency_ID"]),
                                //               _batch.GetDateAcct(),//Changed to account date
                                //               Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_ConversionType_ID"]),
                                //               GetAD_Client_ID(), GetAD_Org_ID());
                                //        if (discountAmt == 0)
                                //        {
                                //            Get_TrxName().Rollback();
                                //            msg = Msg.GetMsg(GetCtx(), "NoCurrencyConversion");
                                //            MCurrency from = new MCurrency(GetCtx(),
                                //               BlineDetailCur_ID,// Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]),
                                //                Get_TrxName());
                                //            MCurrency to = new MCurrency(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["Currency_ID"]), Get_TrxName());
                                //            return msg + " " + to.GetISO_Code() + "," + from.GetISO_Code();
                                //        }
                                //    }
                                //    DueAmount = MConversionRate.Convert(GetCtx(), DueAmount, Util.GetValueOfInt(ds.Tables[0].Rows[i]["Currency_ID"]),
                                //      BlineDetailCur_ID,//  Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Currency_ID"]),
                                //        _batch.GetDateAcct(),//Changed to account date
                                //        Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_ConversionType_ID"]),
                                //        GetAD_Client_ID(), GetAD_Org_ID());
                                //    if (DueAmount == 0)
                                //    {
                                //        Get_TrxName().Rollback();
                                //        msg = Msg.GetMsg(GetCtx(), "NoCurrencyConversion");
                                //        MCurrency from = new MCurrency(GetCtx(),
                                //            BlineDetailCur_ID,//Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]),
                                //            Get_TrxName());
                                //        MCurrency to = new MCurrency(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["Currency_ID"]), Get_TrxName());
                                //        return msg + " " + to.GetISO_Code() + "," + from.GetISO_Code();
                                //    }
                                //}
                                #endregion

                                #region Create View Allocation Header and line when the Due Amount on Batch line = 0
                                if (c_currency_id ==
                                    BlineDetailCur_ID
                                    //Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"])
                                    &&
                                   Bpartner_ID == Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]) &&
                                   batchline_id == Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlines_id"]) &&
                                    Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_DueAmount"]) == 0
                                    && VA009_PaymentMethod_ID == Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_PaymentMethod_ID"]))
                                {
                                    MAllocationLine alloclne = new MAllocationLine(GetCtx(), 0, Get_TrxName());
                                    alloclne.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                                    alloclne.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                                    alloclne.SetC_AllocationHdr_ID(allocationHeader);
                                    alloclne.SetC_BPartner_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]));
                                    alloclne.SetC_Invoice_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoice_id"]));
                                    alloclne.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoicepayschedule_id"]));
                                    //to set document date of batch header on all payments and allocations
                                    alloclne.SetDateTrx(_batch.GetVA009_DocumentDate());

                                    #region Commented Code
                                    //if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC")
                                    //{
                                    //    alloclne.SetAmount(-1 * (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"])));
                                    //}
                                    //else
                                    //{
                                    //alloclne.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                                    //}
                                    // alloclne.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                                    //  alloclne.SetDiscountAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                                    #endregion

                                    alloclne.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                                    alloclne.SetDiscountAmt(discountAmt);
                                    if (!alloclne.Save(Get_TrxName()))
                                    {
                                        msg = Msg.GetMsg(GetCtx(), "VA009_PymentNotSaved");
                                        ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                        SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                            GetRecord_ID(), ppE.ToString());
                                        Get_TrxName().Rollback();
                                        payment.Clear();
                                        viewAllocationId.Clear();
                                        allocationDocumentNo = string.Empty;
                                        paymentDocumentNo = string.Empty;
                                        break;
                                    }
                                    else
                                    {
                                        // set Allocation ID on Batch Line Details
                                        batchLineDetails = new MVA009BatchLineDetails(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]), Get_Trx());
                                        batchLineDetails.SetC_AllocationHdr_ID(allocationHeader);
                                        if (!batchLineDetails.Save(Get_TrxName()))
                                        {
                                            Get_TrxName().Rollback();
                                        }
                                    }
                                }
                                else if (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_DueAmount"]) == 0)
                                {
                                    MAllocationHdr allocHdr = new MAllocationHdr(GetCtx(), 0, Get_TrxName());
                                    allocHdr.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                                    allocHdr.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                                    //Rakesh(VA228):to set account date of batch header on all payments and allocations
                                    allocHdr.SetDateAcct(_batch.GetDateAcct());
                                    allocHdr.SetDateTrx(_batch.GetVA009_DocumentDate());
                                    //allocHdr.SetC_Currency_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]));
                                    allocHdr.SetC_Currency_ID(BlineDetailCur_ID);
                                    //Rakesh(VA228):Set ConversionType_ID
                                    allocHdr.SetC_ConversionType_ID(_ConversionType_ID);
                                    allocHdr.SetDocStatus("DR");
                                    allocHdr.SetDocAction("CO");
                                    if (!allocHdr.Save(Get_TrxName()))
                                    {
                                        msg = Msg.GetMsg(GetCtx(), "VA009_PymentNotSaved");
                                        ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                        SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                            GetRecord_ID(), ppE.ToString());
                                        Get_TrxName().Rollback();
                                        payment.Clear();
                                        viewAllocationId.Clear();
                                        allocationDocumentNo = string.Empty;
                                        paymentDocumentNo = string.Empty;
                                        break;
                                    }
                                    else
                                    {
                                        if (!viewAllocationId.Contains(allocHdr.GetC_AllocationHdr_ID()))
                                        {
                                            viewAllocationId.Add(allocHdr.GetC_AllocationHdr_ID());
                                        }
                                        MAllocationLine alloclne = new MAllocationLine(GetCtx(), 0, Get_TrxName());
                                        alloclne.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                                        alloclne.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                                        alloclne.SetC_AllocationHdr_ID(allocHdr.GetC_AllocationHdr_ID());
                                        alloclne.SetC_BPartner_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]));
                                        alloclne.SetC_Invoice_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoice_id"]));
                                        alloclne.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoicepayschedule_id"]));
                                        //to set document date of batch header on all payments and allocations
                                        alloclne.SetDateTrx(_batch.GetVA009_DocumentDate());

                                        #region Commented Code
                                        //if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC")
                                        //{
                                        //    alloclne.SetAmount(-1 * (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"])));
                                        //}
                                        //else
                                        //{

                                        //}
                                        //alloclne.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                                        //alloclne.SetDiscountAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                                        #endregion

                                        alloclne.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                                        alloclne.SetDiscountAmt(discountAmt);
                                        if (!alloclne.Save(Get_TrxName()))
                                        {
                                            msg = Msg.GetMsg(GetCtx(), "VA009_PymentNotSaved");
                                            ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                            SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                                GetRecord_ID(), ppE.ToString());
                                            Get_TrxName().Rollback();
                                            payment.Clear();
                                            viewAllocationId.Clear();
                                            allocationDocumentNo = string.Empty;
                                            paymentDocumentNo = string.Empty;
                                            break;
                                        }
                                        else
                                        {
                                            c_currency_id = BlineDetailCur_ID;//Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]);
                                            Bpartner_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]);
                                            VA009_PaymentMethod_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_PaymentMethod_ID"]);
                                            batchline_id = Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlines_id"]);
                                            allocationHeader = allocHdr.GetC_AllocationHdr_ID();
                                            allocationDocumentNo += allocHdr.GetDocumentNo() + " , ";

                                            #region Commented Code
                                            // set Allocation ID on Batch Line 
                                            //batchLines = new MVA009BatchLines(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlines_id"]), Get_Trx());
                                            //batchLineDetails.SetC_AllocationHdr_ID(allocationHeader);
                                            //batchLines.Save();
                                            #endregion

                                            // set Allocation ID on Batch Line Details
                                            batchLineDetails = new MVA009BatchLineDetails(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]), Get_Trx());
                                            batchLineDetails.SetC_AllocationHdr_ID(allocationHeader);
                                            if (!batchLineDetails.Save(Get_TrxName()))
                                            {
                                                Get_TrxName().Rollback();
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region Create a new entry of payment Allocate against same payment and the condition
                                //else if (c_currency_id == Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]) &&
                                else if ((c_currency_id == BlineDetailCur_ID) &&
                                Bpartner_ID == Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]) &&
                                batchline_id == Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlines_id"]) &&
                                    VA009_PaymentMethod_ID == Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_PaymentMethod_ID"]))
                                {
                                    MPaymentAllocate PayAlocate = new MPaymentAllocate(GetCtx(), 0, Get_TrxName());
                                    PayAlocate.SetC_Payment_ID(C_Payment_ID);
                                    PayAlocate.SetC_Invoice_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoice_id"]));
                                    PayAlocate.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoicepayschedule_id"]));

                                    #region Commented Code
                                    //if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC")
                                    //{
                                    //    PayAlocate.SetDiscountAmt(-1 * Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                                    //    PayAlocate.SetAmount(-1 * Util.GetValueOfInt(ds.Tables[0].Rows[i]["dueamt"]));
                                    //    PayAlocate.SetInvoiceAmt(-1 * Util.GetValueOfInt(ds.Tables[0].Rows[i]["dueamt"]));
                                    //}
                                    //else
                                    //{
                                    //PayAlocate.SetDiscountAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));

                                    //if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC")
                                    //{
                                    //    PayAlocate.SetAmount(-1 * (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"])));
                                    //}
                                    //else
                                    //{

                                    //}
                                    // PayAlocate.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                                    //PayAlocate.SetInvoiceAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["dueamt"]));
                                    //}
                                    #endregion

                                    PayAlocate.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                                    PayAlocate.SetDiscountAmt(discountAmt);
                                    PayAlocate.SetInvoiceAmt(DueAmount);

                                    PayAlocate.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                                    PayAlocate.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                                    PayAlocate.SetWriteOffAmt(0);
                                    PayAlocate.SetOverUnderAmt(0);
                                    if (!PayAlocate.Save(Get_TrxName()))
                                    {
                                        msg = Msg.GetMsg(GetCtx(), "VA009_PymentAllocateNotSaved");
                                        ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                        SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                            GetRecord_ID(), ppE.ToString());
                                        Get_TrxName().Rollback();
                                        payment.Clear();
                                        viewAllocationId.Clear();
                                        allocationDocumentNo = string.Empty;
                                        paymentDocumentNo = string.Empty;
                                        break;
                                    }
                                    else
                                    {
                                        batchLineDetails = new MVA009BatchLineDetails(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]), Get_Trx());
                                        batchLineDetails.SetC_Payment_ID(C_Payment_ID);
                                        if (!batchLineDetails.Save(Get_TrxName()))
                                        {
                                            Get_TrxName().Rollback();
                                        }
                                    }
                                }
                                #endregion

                                #region Create a new payment
                                else
                                {
                                    _pay = new MPayment(GetCtx(), 0, Get_TrxName());
                                    if (Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Order_ID"]) != 0)
                                    {
                                        //to set document date of batch header on all payments and allocations
                                        //Rakesh(VA228):Replaced due amount with converted amount
                                        checkMsg = CreatePaymentAgainstOrders(ds, i, _pay, discountAmt, _ConvertedAmt, BlineDetailCur_ID, _batch.GetVA009_DocumentDate(), _batch.GetDateAcct());
                                        if (checkMsg != "")
                                        {
                                            Get_TrxName().Rollback();
                                            return checkMsg;
                                        }
                                    }
                                    else
                                    {
                                        int C_Doctype_ID = GetDocumnetType(Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]));
                                        _pay.SetC_DocType_ID(C_Doctype_ID);
                                        _pay.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                                        _pay.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                                        //Rakesh(VA228):to set account date of batch header on all payments and allocations
                                        _pay.SetDateAcct(_batch.GetDateAcct());
                                        _pay.SetDateTrx(_batch.GetVA009_DocumentDate());
                                        _pay.SetC_BankAccount_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bankaccount_id"]));
                                        _pay.SetC_BPartner_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]));
                                        #region to set bank account of business partner and name on batch line
                                        //to set value of routing number and account number of batch lines 
                                        if (Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BP_BankAccount_ID"]) > 0)
                                        {
                                            DataSet ds1 = new DataSet();
                                            ds1 = DB.ExecuteDataset(@" SELECT a_name, RoutingNo, AccountNo FROM 
                                                  C_BP_BankAccount WHERE C_BP_BankAccount_ID = " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BP_BankAccount_ID"]));
                                            if (ds1.Tables != null && ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                                            {
                                                _pay.Set_ValueNoCheck("C_BP_BankAccount_ID", Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BP_BankAccount_ID"]));
                                                _pay.Set_ValueNoCheck("A_Name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                                                _pay.Set_ValueNoCheck("RoutingNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["RoutingNo"]));
                                                _pay.Set_ValueNoCheck("AccountNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["AccountNo"]));
                                            }
                                        }
                                        else
                                        {
                                            //T.C_BP_BankAccount_ID,//T.swiftcode,//T.Acctnumber,//T.AcctName
                                            _pay.Set_ValueNoCheck("C_BP_BankAccount_ID", Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BP_BankAccount_ID"]));
                                            //if partner bank account is not present then set null because constraint null is on ther payment table and it will not allow to save zero.
                                            if (Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BP_BankAccount_ID"]) == 0)
                                                _pay.Set_ValueNoCheck("C_BP_BankAccount_ID", null);
                                            _pay.Set_ValueNoCheck("A_Name", Util.GetValueOfString(ds.Tables[0].Rows[i]["AcctName"]));
                                            _pay.Set_ValueNoCheck("RoutingNo", Util.GetValueOfString(ds.Tables[0].Rows[i]["swiftcode"]));
                                            _pay.Set_ValueNoCheck("AccountNo", Util.GetValueOfString(ds.Tables[0].Rows[i]["Acctnumber"]));
                                        }
                                        #endregion
                                        _pay.SetC_BPartner_Location_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_Location_ID"]));
                                        //_pay.SetC_Currency_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]));
                                        _pay.SetC_Currency_ID(BlineDetailCur_ID);
                                        //Rakesh(VA228):Set ConversionType_ID
                                        _pay.SetC_ConversionType_ID(_ConversionType_ID);
                                        _pay.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_paymentmethod_id"]));
                                        tenderType = Util.GetValueOfString(DB.ExecuteScalar(@"select VA009_PAYMENTBASETYPE from VA009_PAYMENTMETHOD where VA009_PAYMENTMETHOD_ID=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_paymentmethod_id"])));
                                        if (tenderType == "K")          // Credit Card
                                        {
                                            _pay.SetTenderType("C");
                                        }
                                        else if (tenderType == "D")   // Direct Debit
                                        {
                                            _pay.SetTenderType("D");
                                        }
                                        else if (tenderType == "S")    // Check
                                        {
                                            _pay.SetTenderType("K");
                                            //Arpit In Case of Payment is of check type then we insert Check Date + Check Number
                                            _pay.SetCheckDate(_batch.GetDateAcct());//Changed to account date from system date
                                            checkMsg = UpdateCheckNoOnPayment(ds, i, _pay);
                                            if (checkMsg != "")
                                            {
                                                Get_TrxName().Rollback();
                                                msg = Msg.GetMsg(GetCtx(), checkMsg);
                                                MBankAccount ba = new MBankAccount(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bankaccount_id"]), Get_TrxName());
                                                //Want space between the Message and AccountNo
                                                return msg + " : " + ba.GetAccountNo();
                                            }
                                        }
                                        else if (tenderType == "T")    // Direct Deposit
                                        {
                                            _pay.SetTenderType("A");
                                        }
                                        else
                                        {
                                            _pay.SetTenderType("A");
                                        }
                                    }
                                    if (!_pay.Save(Get_TrxName()))
                                    {
                                        msg = Msg.GetMsg(GetCtx(), "VA009_PymentNotSaved");
                                        ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                        SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                            GetRecord_ID(), ppE.ToString());
                                        Get_TrxName().Rollback();
                                        payment.Clear();
                                        viewAllocationId.Clear();
                                        allocationDocumentNo = string.Empty;
                                        paymentDocumentNo = string.Empty;
                                        break;
                                    }
                                    else
                                    {                         
                                        //check number to generated on Payment Completion
                                        //if (CurrtNxtUpdated)
                                        //{
                                        //    BAcctDoc.SetCurrentNext(BAcctDoc.GetCurrentNext() + 1);
                                        //    if (!BAcctDoc.Save(Get_TrxName()))
                                        //    {
                                        //        Get_TrxName().Rollback();
                                        //    }
                                        //    CurrtNxtUpdated = false;
                                        //}
                                        if (!payment.Contains(_pay.GetC_Payment_ID()))
                                        {
                                            payment.Add(_pay.GetC_Payment_ID());
                                        }
                                        //c_currency_id = Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]);
                                        c_currency_id = BlineDetailCur_ID;
                                        Bpartner_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]);
                                        VA009_PaymentMethod_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_PaymentMethod_ID"]);
                                        C_Payment_ID = _pay.GetC_Payment_ID();
                                        batchline_id = Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlines_id"]);
                                        paymentDocumentNo += _pay.GetDocumentNo() + " , ";

                                        MPaymentAllocate PayAlocate = new MPaymentAllocate(GetCtx(), 0, Get_TrxName());
                                        PayAlocate.SetC_Payment_ID(C_Payment_ID);
                                        if (Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoice_id"]) > 0)
                                        {
                                            PayAlocate.SetC_Invoice_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoice_id"]));
                                            PayAlocate.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoicepayschedule_id"]));
                                        }

                                        #region Commented Code
                                        //if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC")
                                        //{
                                        //    PayAlocate.SetDiscountAmt(-1 * Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                                        //    PayAlocate.SetAmount(-1 * Util.GetValueOfInt(ds.Tables[0].Rows[i]["dueamt"]));
                                        //    PayAlocate.SetInvoiceAmt(-1 * Util.GetValueOfInt(ds.Tables[0].Rows[i]["dueamt"]));
                                        //}
                                        //else
                                        //{

                                        //if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC")
                                        //{
                                        //    PayAlocate.SetAmount(-1 * (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"])));
                                        //}
                                        //else
                                        //{

                                        //}
                                        //PayAlocate.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));

                                        //}
                                        #endregion

                                        PayAlocate.SetDiscountAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                                        PayAlocate.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                                        PayAlocate.SetInvoiceAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["dueamt"]));
                                        PayAlocate.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                                        PayAlocate.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                                        PayAlocate.SetWriteOffAmt(0);
                                        PayAlocate.SetOverUnderAmt(0);
                                        if (!PayAlocate.Save(Get_TrxName()))
                                        {
                                            msg = Msg.GetMsg(GetCtx(), "VA009_PymentAllocateNotSaved");
                                            ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                            SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                                GetRecord_ID(), ppE.ToString());
                                            Get_TrxName().Rollback();
                                            payment.Clear();
                                            viewAllocationId.Clear();
                                            allocationDocumentNo = string.Empty;
                                            paymentDocumentNo = string.Empty;
                                            break;
                                        }
                                        else
                                        {
                                            batchLineDetails = new MVA009BatchLineDetails(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]), Get_Trx());
                                            batchLineDetails.SetC_Payment_ID(_pay.GetC_Payment_ID());
                                            batchLineDetails.Save(Get_TrxName());

                                            batchLines = new MVA009BatchLines(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlines_id"]), Get_Trx());
                                            batchLines.SetC_Payment_ID(_pay.GetC_Payment_ID());
                                            if (!batchLines.Save(Get_TrxName()))
                                            {
                                                Get_TrxName().Rollback();
                                            }
                                        }
                                    }
                                }
                                #endregion

                            }

                            // Complete the Consolidate Records of payment
                            for (int i = 0; i < payment.Count(); i++)
                            {
                                Get_TrxName().Commit();
                                MPayment completePayment = new MPayment(GetCtx(), payment[i], Get_TrxName());
                                string result = CompleteOrReverse(GetCtx(), completePayment.GetC_Payment_ID(), 149, "CO");
                                if (!String.IsNullOrEmpty(result))
                                {
                                    Get_TrxName().Rollback();
                                    //Remove Payment reference if payment is not completed
                                    if (DB.ExecuteQuery("UPDATE VA009_BatchLineDetails SET C_Payment_ID = NULL WHERE VA009_BatchLines_ID= " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_BatchLines_ID"])) > 0)
                                    {
                                        DB.ExecuteQuery("UPDATE VA009_BatchLines SET C_Payment_ID = NULL WHERE VA009_BatchLines_ID= " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_BatchLines_ID"]));
                                    }

                                    log.Log(Level.SEVERE, "Payment Not Completed "+completePayment.GetDocumentNo()+" "+ result);

                                    DB.ExecuteQuery("DELETE FROM C_Payment WHERE C_Payment_ID = " + completePayment.GetC_Payment_ID());
                                 
                                    msg = result ;

                                }
                                else 
                                {
                                    if (docNos.Length > 1)
                                    {
                                        docNos.Append(",").Append(completePayment.GetDocumentNo());
                                    }
                                    else
                                    {
                                        docNos.Append(completePayment.GetDocumentNo());
                                    }                           
                                   
                                }
                               
                            }

                            // Complete the Consolidate Records of View allocation 
                            for (int i = 0; i < viewAllocationId.Count(); i++)
                            {
                                MAllocationHdr completeAllocation = new MAllocationHdr(GetCtx(), viewAllocationId[i], Get_Trx());
                                if (completeAllocation.CompleteIt() == "CO")
                                {
                                    completeAllocation.SetDocStatus("CO");
                                    completeAllocation.SetDocAction("CL");
                                    completeAllocation.Save(Get_TrxName());
                                }
                            }
                        }
                        #endregion

                        #region Consolidate = false
                        else if (_batch.IsVA009_Consolidate() == false)
                        {                      
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]).Equals(MDocBaseType.DOCBASETYPE_APCREDITMEMO) &&
                                  Util.GetValueOfString(ds.Tables[0].Rows[i]["VA009_PAYMENTBASETYPE"]).Equals(MVA009PaymentMethod.VA009_PAYMENTBASETYPE_Check))
                                {
                                    //Payment should not be crteated if Payment Method is check and APC Invoice
                                    invDocNo += "," + Util.GetValueOfInt(ds.Tables[0].Rows[i]["DocumentNo"]);
                                    continue;
                                }
                                BlineDetailCur_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Currency_ID"]);
                                if (currencyTo_ID > 0)
                                {
                                    BlineDetailCur_ID = currencyTo_ID;
                                    //Getting the Currency From Window
                                }
                                discountAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]);
                                DueAmount = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]);
                                //Rakesh(VA228):Get converted amount
                                _ConvertedAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]);
                                // Convert Amounts if currency is not same...conversion must be at the same day on which payment is generating
                                // if (Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Currency_ID"]) != Util.GetValueOfInt(ds.Tables[0].Rows[i]["Currency_ID"]))

                                #region Commented Not Required to Convert
                                //Rakesh(VA228):Not requird to convert as already converted in respective currency
                                //if (BlineDetailCur_ID != Util.GetValueOfInt(ds.Tables[0].Rows[i]["Currency_ID"]))
                                //{
                                //    if (discountAmt > 0)
                                //    {
                                //        discountAmt = MConversionRate.Convert(GetCtx(), discountAmt, Util.GetValueOfInt(ds.Tables[0].Rows[i]["Currency_ID"]),
                                //              //Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Currency_ID"]),
                                //              BlineDetailCur_ID,
                                //               _batch.GetDateAcct(),//Changed to account date
                                //               Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_ConversionType_ID"]),
                                //               GetAD_Client_ID(), GetAD_Org_ID());
                                //        if (discountAmt == 0)
                                //        {
                                //            Get_TrxName().Rollback();
                                //            msg = Msg.GetMsg(GetCtx(), "NoCurrencyConversion");
                                //            MCurrency from = new MCurrency(GetCtx(),
                                //              BlineDetailCur_ID,  //Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]), 
                                //                Get_TrxName());
                                //            MCurrency to = new MCurrency(GetCtx(),
                                //              BlineDetailCur_ID,//  Util.GetValueOfInt(ds.Tables[0].Rows[i]["Currency_ID"]),
                                //                Get_TrxName());
                                //            return msg + " " + to.GetISO_Code() + "," + from.GetISO_Code();
                                //        }
                                //    }
                                //    DueAmount = MConversionRate.Convert(GetCtx(), DueAmount, Util.GetValueOfInt(ds.Tables[0].Rows[i]["Currency_ID"]),
                                //      BlineDetailCur_ID,
                                //        //Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Currency_ID"]),
                                //        _batch.GetDateAcct(),//Changed to account date
                                //        Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_ConversionType_ID"]),
                                //        GetAD_Client_ID(), GetAD_Org_ID());
                                //    if (DueAmount == 0)
                                //    {
                                //        Get_TrxName().Rollback();
                                //        msg = Msg.GetMsg(GetCtx(), "NoCurrencyConversion");
                                //        MCurrency from = new MCurrency(GetCtx(),
                                //            BlineDetailCur_ID,
                                //            //Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]), 
                                //            Get_TrxName());
                                //        MCurrency to = new MCurrency(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["Currency_ID"]),
                                //            Get_TrxName());
                                //        return msg + " " + to.GetISO_Code() + "," + from.GetISO_Code();
                                //    }
                                //    DueAmount = DueAmount - discountAmt;
                                //}
                                #endregion

                                _pay = new MPayment(GetCtx(), 0, Get_TrxName());
                                if (Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Order_ID"]) != 0)
                                {
                                    //to set document date of batch header on all payments and allocations
                                    //Rakesh(VA228):Replaced due amount with converted amount
                                    checkMsg = CreatePaymentAgainstOrders(ds, i, _pay, discountAmt, _ConvertedAmt, BlineDetailCur_ID, _batch.GetVA009_DocumentDate(), _batch.GetDateAcct());
                                    if (checkMsg != "")
                                    {
                                        return checkMsg;
                                    }
                                }
                                else
                                {
                                    int C_Doctype_ID = GetDocumnetType(Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]));
                                    _pay.SetC_DocType_ID(C_Doctype_ID);
                                    _pay.SetC_Invoice_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoice_id"]));
                                    _pay.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoicepayschedule_id"]));
                                    _pay.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                                    _pay.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                                    //Rakesh(VA228):to set account date of batch header on all payments and allocations
                                    _pay.SetDateAcct(_batch.GetDateAcct());
                                    _pay.SetDateTrx(_batch.GetVA009_DocumentDate());
                                    _pay.SetC_BankAccount_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bankaccount_id"]));
                                    _pay.SetC_BPartner_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]));
                                    _pay.SetC_BPartner_Location_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_Location_ID"]));

                                    #region Commented Code
                                    //if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC")
                                    //{
                                    //    _pay.SetDiscountAmt(-1 * Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                                    //    _pay.SetPayAmt(-1 * Util.GetValueOfInt(ds.Tables[0].Rows[i]["dueamt"]));
                                    //}
                                    //else
                                    //{
                                    //_pay.SetDiscountAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));

                                    //if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC")
                                    //{
                                    //    decimal payamt = (-1 * (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"])));
                                    //    _pay.SetPayAmt(payamt);
                                    //}
                                    //else
                                    //{
                                    //_pay.SetPayAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                                    //}
                                    //}
                                    #endregion

                                    _pay.SetDiscountAmt(discountAmt);
                                    _pay.SetPayAmt(_ConvertedAmt);

                                    //_pay.SetC_Currency_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]));
                                    _pay.SetC_Currency_ID(BlineDetailCur_ID);
                                    //Rakesh(VA228):Set ConversionType_ID
                                    _pay.SetC_ConversionType_ID(_ConversionType_ID);
                                    _pay.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_paymentmethod_id"]));
                                    tenderType = Util.GetValueOfString(DB.ExecuteScalar(@"select VA009_PAYMENTBASETYPE from VA009_PAYMENTMETHOD where VA009_PAYMENTMETHOD_ID=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_paymentmethod_id"])));
                                    if (tenderType == "K")          // Credit Card
                                    {
                                        _pay.SetTenderType("C");
                                    }
                                    else if (tenderType == "D")   // Direct Debit
                                    {
                                        _pay.SetTenderType("D");
                                    }
                                    else if (tenderType == "S")    // Check
                                    {
                                        _pay.SetTenderType("K");
                                        //Arpit In Case of Payment is of check type then we insert Check Date + Check Number
                                        _pay.SetCheckDate(_batch.GetDateAcct());
                                        String checkMsg = UpdateCheckNoOnPayment(ds, i, _pay);
                                        if (checkMsg != "")
                                        {
                                            Get_TrxName().Rollback();
                                            msg = Msg.GetMsg(GetCtx(), checkMsg);
                                            MBankAccount ba = new MBankAccount(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bankaccount_id"]), Get_TrxName());
                                            //Want space between the Message and AccountNo
                                            return msg + " : " + ba.GetAccountNo();
                                        }
                                    }
                                    else if (tenderType == "T")    // Direct Deposit
                                    {
                                        _pay.SetTenderType("A");
                                    }
                                    else
                                    {
                                        _pay.SetTenderType("A");
                                    }
                                }
                                if (!_pay.Save(Get_TrxName()))
                                {
                                    ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                    SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                        GetRecord_ID(), ppE.ToString());
                                    Rollback();
                                    msg = Msg.GetMsg(GetCtx(), "VA009_PymentNotSaved") + ": " + ppE.ToString();

                                    allocationDocumentNo = string.Empty;
                                    paymentDocumentNo = string.Empty;
                                    break;
                                }
                                else
                                {
                                   
                                    //check number to generated on Payment Completion
                                    //if (CurrtNxtUpdated)
                                    //{
                                    //    BAcctDoc.SetCurrentNext(BAcctDoc.GetCurrentNext() + 1);
                                    //    BAcctDoc.Save(Get_TrxName());
                                    //    CurrtNxtUpdated = false;
                                    //}
                                    paymentDocumentNo += _pay.GetDocumentNo() + " , ";
                                    batchLineDetails = new MVA009BatchLineDetails(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]), Get_TrxName());
                                    batchLineDetails.SetC_Payment_ID(_pay.GetC_Payment_ID());
                                    batchLineDetails.Save(Get_TrxName());
                                    #region Commented
                                    //if (_pay.GetDocStatus() == "CO" || _pay.GetDocStatus() == "CL" || _pay.GetDocStatus() == "VO" || _pay.GetDocStatus() == "RE")
                                    //{
                                    //    string docNo = _pay.GetDocumentNo();
                                    //    Get_TrxName().Rollback();
                                    //    msg = Msg.GetMsg(GetCtx(), "VA009_AlreadyCreated");
                                    //    msg += ":" + docNo;
                                    //}
                                    #endregion
                                    Get_TrxName().Commit();
                                    MPayment completePayment = new MPayment(GetCtx(), batchLineDetails.GetC_Payment_ID(), Get_Trx());
                                    string result = CompleteOrReverse(GetCtx(), completePayment.GetC_Payment_ID(), 149, "CO");
                                    if (string.IsNullOrEmpty(result))
                                    {
                                        if (docNos.Length > 1)
                                        {
                                            docNos.Append(",").Append(_pay.GetDocumentNo());
                                        }
                                        else
                                        {
                                            docNos.Append(_pay.GetDocumentNo());
                                        }

                                        //to Set Allocation ID on Batch Line Details
                                        sql.Clear();
                                        sql.Append(@"Select AL.C_AllocationHdr_ID FROM C_AllocationLine AL  
                                                    INNER JOIN C_AllocationHdr AH ON
                                                    AH.C_AllocationHdr_ID=AL.C_AllocationHdr_ID
                                                    WHERE AH.Processed='Y'
                                                    AND AH.DocStatus   IN ('CO','CL')
                                                    AND AL.C_Payment_ID =" + _pay.GetC_Payment_ID());
                                        try
                                        {
                                            //using (IDataReader idr = DB.ExecuteReader(sql.ToString(), null, Get_TrxName()))
                                            //{
                                            //    while (idr.Read())
                                            //    {
                                            allocationId = Util.GetValueOfInt(DB.ExecuteScalar(sql.ToString(), null, Get_TrxName()));
                                            batchLineDetails.SetC_AllocationHdr_ID(allocationId);
                                            if (!batchLineDetails.Save(Get_TrxName()))
                                            {
                                                return ErrorMessage();
                                            }
                                            //    }
                                            //}
                                        }
                                        catch (Exception ex)
                                        {
                                            Get_TrxName().Rollback();
                                            return ex.Message;
                                        }
                                        //}
                                    }
                                    else
                                    {
                                        Get_TrxName().Rollback();

                                        //Remove Paymenmt reference if Payment is not completed
                                        DB.ExecuteQuery("UPDATE VA009_BatchLineDetails SET C_Payment_ID = null WHERE VA009_BatchLineDetails_ID= " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_BatchLineDetails_ID"]));

                                        log.Log(Level.SEVERE, "Payment Not Completed " + completePayment.GetDocumentNo() + " " + result);
                                       
                                        //delete Payment Document in Drafetd Stage 
                                        DB.ExecuteQuery("DELETE FROM C_Payment WHERE C_Payment_ID = " + completePayment.GetC_Payment_ID());
                                        
                                        msg = result;

                                    }
                                }
                            }
                        }

                        #endregion
                    }
                    else
                        return msg = Msg.GetMsg(GetCtx(), "VA009_LinesNotAvailable");

                    if (paymentDocumentNo != "" || allocationDocumentNo != "")
                    {
                        SaveRecordPaymentBachLog(_batch.GetAD_Client_ID(), _batch.GetAD_Org_ID(), GetRecord_ID(), paymentDocumentNo, allocationDocumentNo);
                    }
                    if (!string.IsNullOrEmpty(invDocNo))
                    {
                        //payment not generated -- APC case 
                        msg += " " + Msg.GetMsg(GetCtx(), "VA009_CantGenPaymentForCheck") + " " + invDocNo.TrimStart(',');
                    }
                    if (String.IsNullOrEmpty(msg) && !String.IsNullOrEmpty(docNos.ToString()))
                    {
                        //Set batch Processed True when payment generation and complete payment Done
                        MVA009Batch batch = new MVA009Batch(GetCtx(), GetRecord_ID(), Get_TrxName());
                        batch.SetProcessed(true);
                        if (batch.Save(Get_TrxName()))
                        {
                            msg = Msg.GetMsg(GetCtx(), "VA009_PymentSaved");
                            msg = msg + ":" + docNos.ToString();
                        }
                        else
                        {
                            return ErrorMessage();
                        }
                    }
                }

                catch (Exception e)
                {
                    Get_TrxName().Rollback();
                    e = VLogger.RetrieveException();
                    return (e.Message);
                }
            }
            else
            {
                msg = Msg.GetMsg(GetCtx(), "VA009_PaymentAlreadyGenerated");
            }
         
            return msg;
        }

        /// <summary> Obsolete method because plenty of cases for order payment Schedule
        /// Set Execution Status In Case of Order Payment Schedule
        /// </summary> Set Order Schedule Execution Status
        /// <param name="dSet"></param>
        /// <param name="i"></param>
        private String SetOrderScheduleExecutionStatus(DataSet dSet, Int32 i)
        {
            MVA009OrderPaySchedule OPSchedule = new MVA009OrderPaySchedule(GetCtx(), Util.GetValueOfInt(dSet.Tables[0].Rows[i]["VA009_OrderPaySchedule_ID"]), Get_TrxName());
            OPSchedule.SetVA009_ExecutionStatus("Y");
            if (!OPSchedule.Save(Get_TrxName()))
            {
                return ErrorMessage();
            }
            return "";
        }

        private string ErrorMessage()
        {
            Get_TrxName().Rollback();
            msg = Msg.GetMsg(GetCtx(), "VA009_PymentNotSaved");
            ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
            return msg;
        }
        /// <summary>
        /// Update Check NO for the Payment in Case of Check Type Method
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="i"></param>
        /// <param name="_pay"></param>
        private String UpdateCheckNoOnPayment(DataSet ds, int i, MPayment _pay)
        {
            sql.Clear();
            //get Bank Account document record on respective condition
            sql.Append(@"SELECT C_BankAccountDoc.C_BankAccountDoc_ID FROM 
                            C_BankAccountDoc C_BankAccountDoc INNER JOIN C_BankAccount C_BankAccount ON (C_BankAccount.C_BankAccount_ID = C_BankAccountDoc.C_BankAccount_ID)
                        Where C_BankAccountDoc.IsActive='Y' 
                        AND C_BankAccountDoc.PaymentRule='S' 
                        AND C_BankAccount.ChkNoAutoControl = 'Y' 
                        AND EndChkNumber != (CurrentNext-1)
                        AND C_BankAccountDoc.VA009_PaymentMethod_ID = " + _pay.GetVA009_PaymentMethod_ID() + @"
                        AND C_BankAccountDoc.C_BankAccount_ID=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bankaccount_id"]));
            Int32 bankAcctDoc_ID = Util.GetValueOfInt(DB.ExecuteScalar(sql.ToString(), null, Get_TrxName()));
            if (bankAcctDoc_ID > 0)
            {
                BAcctDoc = new X_C_BankAccountDoc(GetCtx(), bankAcctDoc_ID, Get_TrxName());
                if (BAcctDoc.GetCurrentNext() > 0)
                {
                    //not required to set checkno from here
                    //_pay.SetCheckNo(Util.GetValueOfString(BAcctDoc.GetCurrentNext()));
                   // CurrtNxtUpdated = true;
                }
                else
                {
                    return "VA009_NoCurNxtForAcctNo";
                }
            }
            else
            {
                //Auto check control not defined for selected Payment Method on Bank Account Document
                return "VA009_PayMthodOrBkAcctDocNotFund";
            }
            return "";
        }
        /// <summary>
        /// Get Document Base Type based on Sales Transaction and Return Transction
        /// </summary>
        /// <param name="issotrx"></param>
        /// <param name="isreturntrx"></param>
        /// <returns></returns>
        public int GetDocbaseType(string issotrx, string isreturntrx)
        {
            string Docbasetype = string.Empty;

            if (issotrx == "Y" && isreturntrx == "N")
                Docbasetype = "ARR";
            else if (issotrx == "N" && isreturntrx == "N")
                Docbasetype = "APP";
            else if (issotrx == "N" && isreturntrx == "Y")
                Docbasetype = "ARR";
            else if (issotrx == "Y" && isreturntrx == "Y")
                Docbasetype = "APP";

            int DocType_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select  dt.c_doctype_id  From C_doctype DT inner join c_docbasetype DBT On dt.docbasetype=dbt.docbasetype where dbt.docbasetype='" + Docbasetype + "' AND dt.IsActive = 'Y' AND (DT.ad_org_id = " + GetAD_Org_ID() + " or  DT.ad_org_id = 0) AND DT.AD_Client_ID = " + GetAD_Client_ID()));
            return DocType_ID;
        }

        /// <summary>
        ///Get Document Type Based on Document Base Type 
        /// </summary>
        /// <param name="docbasetype"></param>
        /// <returns></returns>
        public int GetDocumnetType(string docbasetype)
        {
            if (docbasetype == "ARI")
                docbasetype = "ARR";
            else if (docbasetype == "API")
                docbasetype = "APP";
            else if (docbasetype == "ARC" || docbasetype == "SOO") // with reverse entry //When AR Credit & sales order done by rakesh assigned by ranvir
                docbasetype = "ARR";
            else if (docbasetype == "APC" || docbasetype == "POO") // with reverse entry //When AP Credit & purchase order done by rakesh assigned by ranvir
                docbasetype = "APP";
            int DocType_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select  min(dt.c_doctype_id)  From C_doctype DT inner join c_docbasetype DBT On dt.docbasetype=dbt.docbasetype where dbt.docbasetype='" + docbasetype + "' AND dt.IsActive = 'Y' AND (DT.ad_org_id = " + GetAD_Org_ID() + " or  DT.ad_org_id = 0) AND DT.AD_Client_ID = " + GetAD_Client_ID()));
            return DocType_ID;
        }

        /// <summary>
        /// Saving data against Payment Batch in Log window if we get error while processing  
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="OrgId"></param>
        /// <param name="BatchId"></param>
        /// <param name="ErrorMsg"></param>
        public void SavePaymentBachLog(int ClientId, int OrgId, int BatchId, string ErrorMsg)
        {
            MVA009PaymentBatchLog paymentBatchLog = new MVA009PaymentBatchLog(GetCtx(), 0, Get_Trx());
            paymentBatchLog.SetAD_Client_ID(ClientId);
            paymentBatchLog.SetAD_Org_ID(OrgId);
            paymentBatchLog.SetVA009_Batch_ID(BatchId);
            paymentBatchLog.SetIsError(true);
            paymentBatchLog.SetTextMsg(errorMsg);
            paymentBatchLog.Save(Get_TrxName());
        }

        /// <summary>
        /// Saving data against Payment Batch in Log window if we get error while processing  
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="OrgId"></param>
        /// <param name="BatchId"></param>
        /// <param name="paymentDocumentNo"></param>
        /// <param name="allocationDocumentNo"></param>
        public void SaveRecordPaymentBachLog(int ClientId, int OrgId, int BatchId, string paymentDocumentNo, string allocationDocumentNo)
        {
            MVA009PaymentBatchLog paymentBatchLog = new MVA009PaymentBatchLog(GetCtx(), 0, Get_Trx());
            paymentBatchLog.SetAD_Client_ID(ClientId);
            paymentBatchLog.SetAD_Org_ID(OrgId);
            paymentBatchLog.SetVA009_Batch_ID(BatchId);
            paymentBatchLog.SetIsError(false);
            if (paymentDocumentNo != "" && allocationDocumentNo != "")
            {
                paymentBatchLog.SetSummary("Payment Generated Document No are : " + paymentDocumentNo.Substring(0, paymentDocumentNo.Length - 2) + " And View Allocation Generated Document no are : " + allocationDocumentNo.Substring(0, allocationDocumentNo.Length - 2));
            }
            else if (paymentDocumentNo != "")
            {
                paymentBatchLog.SetSummary("Payment Generated Document No are : " + paymentDocumentNo.Substring(0, paymentDocumentNo.Length - 2));
            }
            else if (allocationDocumentNo != "")
            {
                paymentBatchLog.SetSummary("View Allocation Generated Document no are : " + allocationDocumentNo.Substring(0, allocationDocumentNo.Length - 2));
            }
            paymentBatchLog.Save(Get_TrxName());
        }

        ///<Summary>
        ///check weather payment already generated against this batch or not
        ///<Summary>
        public bool CheckPaymentStatus()
        {
            sql.Clear();
            MVA009Batch _batch = new MVA009Batch(GetCtx(), GetRecord_ID(), Get_TrxName());
            if (_batch.IsVA009_Consolidate())
            {

                sql.Append("SELECT COUNT(C_payment_ID) AS C_payment_ID FROM VA009_BatchLines WHERE VA009_BATCH_ID=" + GetRecord_ID() + " AND C_payment_ID IS NOT NULL");
            }
            else
            {
                sql.Append(@"SELECT COUNT(bld.C_PAYMENT_ID) AS C_PAYMENT_ID FROM VA009_BATCHLINEDETAILS BLD INNER JOIN VA009_BATCHLINES BL ON BL.VA009_BATCHLINES_ID=BLD.VA009_BATCHLINES_ID 
                          WHERE BL.VA009_BATCH_ID=" + GetRecord_ID() + " AND bld.C_payment_ID IS NOT NULL");
            }
            int result = Util.GetValueOfInt(DB.ExecuteScalar(sql.ToString(), null, null));
            if (result == 0)
                return false;
            else
                return true;
        }

        //Following method added by Arpit to create Payment For diffrent orders
        /// <summary>
        /// Save Payment record in Case when Data Is having Order Payment Schedule
        /// </summary>
        /// <param name="ds">dataset</param>
        /// <param name="i">recordnumber</param>
        /// <param name="_pay">payment object</param>
        /// <param name="discAmt">dicsount amount</param>
        /// <param name="dueAmt">Due amount</param>
        /// <param name="dateAcct">Account Date</param>
        /// <returns></returns>
        public String CreatePaymentAgainstOrders(DataSet ds, int i, MPayment _pay, decimal discAmt, decimal dueAmt, int currencyTo_ID, DateTime? docdate, DateTime? dateAcct)
        {
            //MPayment _pay = new MPayment(GetCtx(), 0, Get_TrxName());
            int C_Doctype_ID = GetDocumnetType(Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]));
            _pay.SetC_DocType_ID(C_Doctype_ID);
            _pay.SetC_Order_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Order_ID"]));
            _pay.SetVA009_OrderPaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_OrderPaySchedule_ID"]));
            _pay.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
            _pay.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
            //Rakesh(VA228):to set account date of batch header on all payments and allocations
            _pay.SetDateAcct(dateAcct);
            _pay.SetDateTrx(docdate);
            _pay.SetC_BankAccount_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bankaccount_id"]));
            _pay.SetC_BPartner_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]));
            #region to set bank account of business partner and name on batch line
            if (Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]) > 0)
            {
                DataSet ds1 = new DataSet();
                ds1 = DB.ExecuteDataset(@" SELECT MAX(C_BP_BankAccount_ID) as C_BP_BankAccount_ID,
                                  a_name FROM C_BP_BankAccount WHERE C_BPartner_ID = " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]) + " AND "
                       + " AD_Org_ID =" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]) + " GROUP BY C_BP_BankAccount_ID, a_name ");
                if (ds1.Tables != null && ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                {
                    _pay.Set_Value("C_BP_BankAccount_ID", Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                    //if partner bank account is not present then set null because constraint null is on ther payment table and it will not allow to save zero.
                    if (Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]) == 0)
                        _pay.Set_Value("C_BP_BankAccount_ID", null);
                    _pay.Set_Value("a_name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                }
            }
            #endregion
            _pay.SetC_BPartner_Location_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_Location_ID"]));
            //_pay.SetDiscountAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
            _pay.SetDiscountAmt(discAmt);
            //_pay.SetPayAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
            _pay.SetPayAmt(dueAmt);
            //_pay.SetC_Currency_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]));
            _pay.SetC_Currency_ID(BlineDetailCur_ID);
            //Rakesh(VA228):Set ConversionType_ID
            _pay.SetC_ConversionType_ID(_ConversionType_ID);
            _pay.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_paymentmethod_id"]));
            tenderType = Util.GetValueOfString(DB.ExecuteScalar(@"select VA009_PAYMENTBASETYPE from VA009_PAYMENTMETHOD where VA009_PAYMENTMETHOD_ID=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_paymentmethod_id"])));
            if (tenderType == "K")          // Credit Card
            {
                _pay.SetTenderType("C");
            }
            else if (tenderType == "D")   // Direct Debit
            {
                _pay.SetTenderType("D");
            }
            else if (tenderType == "S")    // Check
            {
                _pay.SetTenderType("K");
                _pay.SetCheckDate(dateAcct);
                checkMsg = UpdateCheckNoOnPayment(ds, i, _pay);
                if (checkMsg != "")
                {
                    Get_TrxName().Rollback();
                    msg = Msg.GetMsg(GetCtx(), checkMsg);
                    MBankAccount ba = new MBankAccount(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bankaccount_id"]), Get_TrxName());
                    //Want space between the Message and AccountNo
                    return msg + " : " + ba.GetAccountNo();
                }
            }
            else if (tenderType == "T")    // Direct Deposit
            {
                _pay.SetTenderType("A");
            }
            else
            {
                _pay.SetTenderType("A");
            }
            //_pay.SetDocStatus("CO");
            //_pay.SetDocAction("CL");
            //if (!_pay.Save(Get_TrxName()))
            //{
            //    msg = Msg.GetMsg(GetCtx(), "VA009_PymentNotSaved");
            //    ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
            //    SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
            //                        GetRecord_ID(), ppE.ToString());
            //    Rollback();
            //}
            //else
            //{
            //    //To set Payment's ID on batch Line Details
            //    MVA009BatchLineDetails BatchDetLines = new MVA009BatchLineDetails(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]), Get_TrxName());
            //    BatchDetLines.SetC_Payment_ID(_pay.Get_ID());
            //    BatchDetLines.Save(Get_TrxName());
            //}
            return "";

        }


        /// <summary>
        /// Mehtod added to complete and reverse the document and execute the workflow as well
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="Record_ID">C_Payment_ID</param>
        /// <param name="Process_ID">Process</param>
        /// <param name="DocAction">Document Action</param>
        /// <returns>result of completion or reversal in a string array</returns>
        public string CompleteOrReverse(Ctx ctx, int Record_ID, int Process_ID, string DocAction)
        {
            string result = "";
            MRole role = MRole.Get(ctx, ctx.GetAD_Role_ID());
            if (Util.GetValueOfBool(role.GetProcessAccess(Process_ID)))
            {
                DB.ExecuteQuery("UPDATE C_Payment SET DocAction = '" + DocAction + "' WHERE C_Payment_ID = " + Record_ID);

                MProcess proc = new MProcess(ctx, Process_ID, null);
                MPInstance pin = new MPInstance(proc, Record_ID);
                if (!pin.Save())
                {
                    ValueNamePair vnp = VLogger.RetrieveError();
                    string errorMsg = "";
                    if (vnp != null)
                    {
                        errorMsg = vnp.GetName();
                        if (errorMsg == "")
                            errorMsg = vnp.GetValue();
                    }
                    if (errorMsg == "")
                        result = errorMsg = Msg.GetMsg(ctx, "DocNotCompleted");

                    return result;
                }

                MPInstancePara para = new MPInstancePara(pin, 20);
                para.setParameter("DocAction", DocAction);
                if (!para.Save())
                {
                    //String msg = "No DocAction Parameter added";  //  not translated
                }
                VAdvantage.ProcessEngine.ProcessInfo pi = new VAdvantage.ProcessEngine.ProcessInfo("WF", Process_ID);
                pi.SetAD_User_ID(ctx.GetAD_User_ID());
                pi.SetAD_Client_ID(ctx.GetAD_Client_ID());
                pi.SetAD_PInstance_ID(pin.GetAD_PInstance_ID());
                pi.SetRecord_ID(Record_ID);
                if (Process_ID == 149)
                {
                    pi.SetTable_ID(335);
                }

                ProcessCtl worker = new ProcessCtl(ctx, null, pi, Get_TrxName());
                worker.Run();

                if (pi.IsError())
                {
                    ValueNamePair vnp = VLogger.RetrieveError();
                    string errorMsg = "";
                    if (vnp != null)
                    {
                        errorMsg = vnp.GetName();
                        if (errorMsg == "")
                            errorMsg = vnp.GetValue();
                    }

                    if (errorMsg == "")
                        errorMsg = pi.GetSummary();

                    if (errorMsg == "")
                        errorMsg = Msg.GetMsg(ctx, "DocNotCompleted");
                    result = errorMsg;
                    return result;
                }
                else
                    result = "";
            }
            else
            {
                result = Msg.GetMsg(ctx, "NoAccess");
                return result;
            }
            return result;
        }



    }
}









