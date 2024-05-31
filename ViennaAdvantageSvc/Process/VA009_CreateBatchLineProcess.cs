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
using ViennaAdvantage.Common;

namespace ViennaAdvantage.Process
{
    public class VA009_CreateBatchLineProcess : SvrProcess
    {
        int _docType = 0;
        int _C_invoice_ID = 0;
        int _C_BPartner_ID = 0;
        int _BPartner = 0;
        private string docBaseType = string.Empty;
        int _paySchedule_ID = 0;
        int _paymentMethod = 0;
        bool _trigger = false;
        DateTime? _DateDoc_From = null;
        DateTime? _DateDoc_To = null;
        int _VA009_BatchLine_ID = 0;
        bool VA009_IsSameCurrency = false;
        int C_ConversionType_ID = 0;
        //int _VA009_BatchDetail_ID = 0;
        String msg = String.Empty;
        bool deleteBatchLine = false;
        //VIS_427 DevOps TaskId: 2156 parameters added for Gl journalline case 
        bool includeGl = false;
        int C_ElementValue_ID = 0;
        //varriable to save lines count
        int Line_MaxCount = 0, Total_Lines_Count = 0;

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
                else if (name.Equals("C_DocType_ID"))
                {
                    _docType = para[i].GetParameterAsInt();
                }
                else if (name.Equals("C_Invoice_ID"))
                {
                    _C_invoice_ID = para[i].GetParameterAsInt();
                }
                else if (name.Equals("C_BPartner_ID"))
                {
                    _C_BPartner_ID = para[i].GetParameterAsInt();
                }
                else if (name.Equals("C_InvoicePaySchedule_ID"))
                {
                    _paySchedule_ID = para[i].GetParameterAsInt();
                }
                else if (name.Equals("VA009_PaymentMethod_ID"))
                {
                    _paymentMethod = para[i].GetParameterAsInt();
                }
                else if (name.Equals("DateInvoiced"))
                {
                    _DateDoc_From = (DateTime?)(para[i].GetParameter());
                    _DateDoc_To = (DateTime?)(para[i].GetParameter_To());
                }
                else if (name.Equals("VA009_IsSameCurrency"))
                {
                    VA009_IsSameCurrency = "Y".Equals(para[i].GetParameter());
                }
                else if (name.Equals("C_ConversionType_ID"))
                {
                    C_ConversionType_ID = para[i].GetParameterAsInt();
                }
                else if (name.Equals("VA009_IsDeleteBatchLines"))
                {
                    deleteBatchLine = "Y".Equals(para[i].GetParameter());
                }
                else if (name.Equals("IsIncludeGL"))
                {
                    includeGl = "Y".Equals(para[i].GetParameter());                  //VIS_427 DevOps TaskId: 2156 parameters added for Gl journalline case
                }
                else if (name.Equals("C_ElementValue_ID"))
                {
                    C_ElementValue_ID = para[i].GetParameterAsInt();
                }
                else
                {
                    log.Log(Level.SEVERE, "Unknown Parameter: " + name);
                }
            }
        }
        protected override string DoIt()
        {
            StringBuilder _sql = new StringBuilder();
            MVA009Batch batch = new MVA009Batch(GetCtx(), GetRecord_ID(), Get_TrxName());
            //VIS_427 10/10/2023 created object of currency to get stdprecision value
            MCurrency currency = MCurrency.Get(GetCtx(), batch.GetC_Currency_ID());
            MVA009BatchLineDetails lineDetail = null;
            MVA009BatchLines line = null;
            bool isCount = false; //VIS_427 Bug id 2323 defined variable 

            // Delete Lines if selected as true
            if (deleteBatchLine)
            {
                msg = DeleteBatchLines(_sql, batch.GetVA009_Batch_ID(), GetCtx(), Get_TrxName());
                if (!String.IsNullOrEmpty(msg))
                {
                    return msg;
                }
            }
            bool isAPI_APC = false;
            MBankAccount _bankacc = new MBankAccount(GetCtx(), batch.GetC_BankAccount_ID(), Get_TrxName());
            //to get cheque details based on payment method and bank account
            List<CheckDetails> _ChkDtlsDT = DBFuncCollection.GetDetailsofChequeForBatch(batch.GetC_BankAccount_ID(), batch.GetVA009_PaymentMethod_ID(), Get_Trx());

            if (_ChkDtlsDT != null && _ChkDtlsDT.Count > 0)
            {
                if (Util.GetValueOfString(_ChkDtlsDT[0].chknoautocontrol).ToUpper().Equals("Y"))
                {
                    Line_MaxCount = Util.GetValueOfInt(_ChkDtlsDT[0].va009_batchlinedetailcount);
                }
            }
            decimal dueamt = 0;

            // docbasetype
            string _baseType = null;

            _sql.Clear();
            _sql.Append(@"SELECT * FROM (SELECT 'INVOICE' as Type,cp.ad_client_id, cp.ad_org_id,ci.c_invoice_id,CI.C_Bpartner_ID, cp.c_invoicepayschedule_id, 
                          cp.duedate, C_BP_BankAccount_ID,null as AccountType, cp.dueamt, cp.discountdate, cp.discountamt,cp.DiscountDays2 , cp.Discount2,cp.va009_paymentmethod_id,
                          ci.c_currency_id , doc.DocBaseType, CI.C_ConversionType_ID, 
                          CASE WHEN (bpLoc.IsPayFrom = 'Y' AND doc.DocBaseType IN ('ARI' , 'ARC')) THEN  CI.C_BPartner_Location_ID
                               WHEN (bpLoc.IsRemitTo = 'Y' AND doc.DocBaseType IN ('API' , 'APC')) THEN  CI.C_BPartner_Location_ID
                               WHEN (bpLoc.IsPayFrom = 'N' AND doc.DocBaseType IN ('ARI' , 'ARC')) THEN  bpLoc.VA009_ReceiptLocation_ID
                               WHEN (bpLoc.IsRemitTo = 'N' AND doc.DocBaseType IN ('API' , 'APC')) THEN  bpLoc.VA009_PaymentLocation_ID 
                          END AS C_BPartner_Location_ID 
                          From C_Invoice CI 
                          INNER JOIN C_InvoicePaySchedule CP ON (CI.c_invoice_id= CP.C_Invoice_ID) 
                          INNER JOIN C_BPartner_Location bpLoc ON (bpLoc.C_BPartner_Location_ID = CI.C_BPartner_Location_ID)
                          INNER JOIN C_DocType doc ON (doc.C_DocType_ID = CI.C_DocType_ID) 
                          WHERE ci.ispaid='N' AND cp.va009_ispaid='N' AND cp.C_Payment_ID IS NULL AND
                          CI.IsActive = 'Y' and ci.docstatus in ('CO','CL') AND cp.VA009_ExecutionStatus NOT IN ('Y','J')
                          AND CP.C_InvoicePaySchedule_ID NOT IN (
                          SELECT CASE WHEN C_Payment.C_Payment_ID != COALESCE(C_PaymentAllocate.C_Payment_ID,0) 
                          THEN COALESCE(C_Payment.C_InvoicePaySchedule_ID,0)  ELSE COALESCE(C_PaymentAllocate.C_InvoicePaySchedule_ID,0) END 
                          FROM C_Payment LEFT JOIN C_PaymentAllocate ON (C_PaymentAllocate.C_Payment_ID = C_Payment.C_Payment_ID) 
                          WHERE C_Payment.DocStatus NOT IN ('CO', 'CL' ,'RE','VO')) 
                          AND cp.IsHoldPayment!='Y'  AND CI.AD_Client_ID = " + batch.GetAD_Client_ID()
              + " AND CI.AD_Org_ID = " + batch.GetAD_Org_ID());

            if (_C_BPartner_ID > 0)
            {
                _sql.Append(" and CI.C_Bpartner_ID=" + _C_BPartner_ID);
            }
            if (_C_invoice_ID > 0)
            {
                _sql.Append("  and CI.C_invoice_ID=" + _C_invoice_ID);
            }
            if (_paySchedule_ID > 0)
            {
                _sql.Append(" AND CP.C_InvoicePaySchedule_ID=" + _paySchedule_ID);
            }
            if (_docType > 0)
            {
                _sql.Append(" ANd CI.C_DocType_ID=" + _docType);
            }
            else
            {
                _sql.Append(" ANd doc.DocBaseType IN ('API' , 'ARI' , 'APC' , 'ARC') ");
            }
            if (_DateDoc_From != null && _DateDoc_To != null)
            {
                _sql.Append(" and cp.duedate BETWEEN  ");
                _sql.Append(GlobalVariable.TO_DATE(_DateDoc_From, true) + " AND ");
                _sql.Append(GlobalVariable.TO_DATE(_DateDoc_To, true));
            }
            else if (_DateDoc_From != null && _DateDoc_To == null)
            {
                _sql.Append(" and cp.duedate >=" + GlobalVariable.TO_DATE(_DateDoc_From, true));
            }
            else if (_DateDoc_From == null && _DateDoc_To != null)
            {
                _sql.Append(" and cp.duedate <=" + GlobalVariable.TO_DATE(_DateDoc_From, true));
            }
            if (VA009_IsSameCurrency == true)
            {
                _sql.Append(" AND CI.C_Currency_ID =" + _bankacc.GetC_Currency_ID());
            }
            if (!includeGl)
            {
                _sql.Append(" Order by CI.C_Bpartner_ID asc , doc.docbasetype ");
            }

            //VIS_427 DevOps TaskId: 2156 added query for Gl journalline
            if (includeGl)
            {
                _sql.Append(" UNION ");
                _sql.Append(@"SELECT 'GL' AS Type,gl.AD_Client_ID, gl.AD_Org_ID,gl.GL_JournalLine_ID AS C_Invoice_ID,gl.C_BPartner_ID,ev.C_ElementValue_ID AS C_InvoicePaySchedule_ID,
                              NULL AS DueDate,0 AS C_BP_BankAccount_ID,ev.AccountType,
                              CASE 
                              WHEN (ev.AccountType = 'A' AND AmtSourceDr > 0) THEN AmtSourceDr
                              WHEN (ev.AccountType = 'A' AND AmtSourceCr > 0) THEN AmtSourceCr
                              WHEN (ev.AccountType = 'L' AND AmtSourceCr > 0) THEN AmtSourceCr
                              WHEN (ev.AccountType = 'L' AND AmtSourceDr > 0) THEN AmtSourceDr
                              END AS DueAmt,NULL AS DiscountDate,0 AS DiscountAmt,NULL AS DiscountDays2 , 0 AS Discount2," + batch.GetVA009_PaymentMethod_ID() + @" AS VA009_PaymentMethod_ID,gl.C_Currency_ID,
                              CASE 
                              WHEN (ev.AccountType = 'A' AND AmtSourceDr >0) THEN 'ARI'
                              WHEN (ev.AccountType = 'A' AND AmtSourceCr >0) THEN 'ARC'
                              WHEN (ev.AccountType = 'L' AND AmtSourceCr >0) THEN 'API'
                              WHEN (ev.AccountType = 'L' AND AmtSourceDr >0) THEN 'APC'
                              END AS DocBaseType,
                              " + batch.GetC_ConversionType_ID() + @" AS C_ConversionType_ID ,
                              (First_VALUE(loc.C_BPartner_Location_ID) OVER (PARTITION BY cb.C_BPartner_ID
                              ORDER BY CASE WHEN ev.AccountType = 'A' THEN loc.IsPayFrom ELSE loc.IsRemitTo END DESC, loc.C_BPartner_Location_ID DESC)) AS C_BPartner_Location_ID
                              FROM GL_Journal g
                              INNER JOIN GL_JournalLine gl ON (gl.GL_Journal_ID=g.GL_Journal_ID)
                              INNER JOIN C_BPartner cb ON (cb.C_BPartner_ID =gl.C_BPartner_ID )
                              INNER JOIN C_BPartner_Location loc ON (loc.C_BPartner_ID =gl.C_BPartner_ID )
                              INNER JOIN C_ElementValue ev on (ev.C_ElementValue_ID=gl.Account_ID)
                              WHERE gl.IsAllocated='N' AND ev.IsAllocationRelated='Y' AND gl.VA009_IsAssignedtoBatch='N' AND g.DocStatus IN ('CO','CL')
                              AND gl.GL_JournalLine_ID NOT IN (SELECT NVL(al.GL_JournalLine_ID,0) FROM C_AllocationHdr ah 
                                        INNER JOIN C_AllocationLine al ON (al.C_AllocationHdr_ID=ah.C_AllocationHdr_ID)
                                        WHERE ah.DocStatus NOT IN ('CO', 'CL' ,'RE','VO'))
                              AND gl.GL_JournalLine_ID NOT IN (
                                  SELECT CASE WHEN C_Payment.C_Payment_ID != COALESCE(C_PaymentAllocate.C_Payment_ID,0) 
                                  THEN COALESCE(C_Payment.GL_JournalLine_ID,0) ELSE COALESCE(C_PaymentAllocate.GL_JournalLine_ID,0) END 
                                  FROM C_Payment LEFT JOIN C_PaymentAllocate ON (C_PaymentAllocate.C_Payment_ID = C_Payment.C_Payment_ID) 
                                  WHERE C_Payment.DocStatus NOT IN ('CO', 'CL' ,'RE','VO'))
                              AND gl.AD_Client_ID =" + batch.GetAD_Client_ID() + " AND gl.AD_Org_ID=" + batch.GetAD_Org_ID());

                if (_C_BPartner_ID > 0)
                {
                    _sql.Append(" AND gl.C_BPartner_ID=" + _C_BPartner_ID);
                }
                if (VA009_IsSameCurrency)
                {
                    _sql.Append(" AND gl.C_Currency_ID =" + _bankacc.GetC_Currency_ID());
                }
                if (C_ElementValue_ID > 0)
                {
                    _sql.Append(" AND gl.Account_ID =" + C_ElementValue_ID);
                }
                _sql.Append(" ORDER BY C_BPartner_ID ASC , DocbaseType ");
            }

            _sql.Append(")t ORDER BY t.C_BPartner_ID ASC ,t.C_BPartner_Location_ID,t.DocbaseType");

            DataSet ds = DB.ExecuteDataset(_sql.ToString());
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                // Get Base Type
                _baseType = Util.GetValueOfString(DB.ExecuteScalar(@"SELECT VA009_PaymentBaseType FROM VA009_PaymentMethod WHERE 
                                VA009_PaymentMethod_ID=" + batch.GetVA009_PaymentMethod_ID(), null, Get_TrxName()));

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    // when due Amount is ZERO, than continue
                    if (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]) == 0)
                    {
                        continue;
                    }
                   // Bug id 2323 set the boolean value false when business partener is not same and location is different
                    if (i > 0 && (_BPartner != Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_ID"]) ||
                        Util.GetValueOfInt(ds.Tables[0].Rows[i - 1]["C_BPartner_Location_ID"]) != Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_Location_ID"])))
                    {
                        isCount = false;
                    }

                    // to set value of routing number and account number of batch lines 
                    DataSet _ds = new DataSet();
                    _BPartner = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_ID"]);
                    if (Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BP_BankAccount_ID"]) > 0)
                    {
                        //to set value of routing number and account number of batch lines 
                        _ds = DB.ExecuteDataset(@" SELECT C_BP_BankAccount_ID, a_name,RoutingNo,AccountNo FROM C_BP_BankAccount WHERE IsActive='Y' AND 
                                            C_BP_BankAccount_ID=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BP_BankAccount_ID"]), null, Get_TrxName());
                    }
                    else
                    {
                        if (_BPartner > 0)
                        {
                            //to set value of routing number and account number of batch lines 
                            _ds = DB.ExecuteDataset(@" SELECT MAX(C_BP_BankAccount_ID) as C_BP_BankAccount_ID,
                                  a_name,RoutingNo,AccountNo,AD_Org_ID  FROM C_BP_BankAccount WHERE C_BPartner_ID = " + _BPartner + " AND IsActive='Y' AND "
                               + " AD_Org_ID IN (0, " + batch.GetAD_Org_ID() + ") GROUP BY C_BP_BankAccount_ID, A_Name, RoutingNo, AccountNo, AD_Org_ID ORDER BY AD_Org_ID DESC", null, Get_TrxName());
                        }
                        else
                        {
                            //Type Error fixed
                            //to set value of routing number and account number of batch lines 
                            _ds = DB.ExecuteDataset(@"SELECT MAX(BPBA.C_BP_BankAccount_ID) as C_BP_BankAccount_ID,
                                  BPBA.a_name,BPBA.RoutingNo,BPBA.AccountNo,BP.AD_Org_ID  FROM C_BP_BankAccount BPBA
                                  INNER JOIN C_BPartner BP ON BPBA.C_BPartner_ID=BP.C_BPartner_ID
                                  WHERE BPBA.AD_Org_ID IN (0, " + batch.GetAD_Org_ID() + @") AND BPBA.IsActive='Y' 
                                  GROUP BY BPBA.C_BP_BankAccount_ID, BPBA.A_Name, RoutingNo, BPBA.AccountNo,BP.AD_Org_ID ORDER BY BP.AD_Org_ID DESC", null, Get_TrxName());
                        }
                    }
                    //in case of AP side we need to run new setting btch line count 
                    if ("API" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) ||
                              "APC" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]))
                    {
                        isAPI_APC = true;
                    }
                    else { isAPI_APC = false; Total_Lines_Count = 0; }
                    // if invoice is of AP Invoice, AP Credit Memo, AR Invoice and AR Credit Memo then make a single Batch line
                    if (_BPartner == Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_ID"]) &&
                        ("API" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) ||
                         "APC" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) ||
                         "ARI" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) ||
                         "ARC" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"])))
                    {
                        _sql.Clear();
                        _sql.Append(@"SELECT * FROM VA009_BatchLines WHERE C_BPartner_ID = " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_ID"]) +
                             @" AND VA009_Batch_ID = " + batch.GetVA009_Batch_ID());

                        if (_ds != null && _ds.Tables[0].Rows.Count > 0 &&
                            !_baseType.Equals(X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Check) &&
                            !_baseType.Equals(X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Cash))
                        {
                            _sql.Append(" AND NVL(C_BP_BankAccount_ID, 0) = " + Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                        }

                        // BP Location
                        if ("API" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) ||
                           "APC" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]))
                        {
                            _sql.Append(" AND NVL(VA009_PaymentLocation_ID, 0) = " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_Location_ID"]));
                        }
                        else if ("ARI" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) ||
                                 "ARC" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]))
                        {
                            _sql.Append(" AND NVL(VA009_ReceiptLocation_ID, 0) = " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_Location_ID"]));
                        }
                        _sql.Append(" ORDER BY VA009_BatchLines_ID DESC ");
                        DataSet dsBatchLine = DB.ExecuteDataset(_sql.ToString(), null, Get_Trx());
                        if (dsBatchLine != null && dsBatchLine.Tables[0].Rows.Count > 0)
                        {
                            /*VIS_427 Bug id 2323 Handled the batch line count issue which is defined on bank account window
                            in order to get batch line detalis according to that count*/
                            if (!isCount)
                            {
                                string sql = @"SELECT COUNT(VA009_BatchLineDetails_ID) FROM VA009_BatchLineDetails WHERE VA009_BatchLines_ID="
                                                + Util.GetValueOfInt(dsBatchLine.Tables[0].Rows[0]["VA009_BatchLines_ID"]);
                                int BatchDetailCount = Util.GetValueOfInt(DB.ExecuteScalar(sql, null, Get_Trx()));
                                if (BatchDetailCount > 0)
                                {
                                    isCount = true;
                                    Total_Lines_Count = BatchDetailCount;
                                }
                            }
                            DataRow dr = dsBatchLine.Tables[0].Rows[0];
                            line = new MVA009BatchLines(GetCtx(), dr, Get_TrxName());
                            //if line found then add batchlinedetail agaisnt same line otherwise create new line
                            if (Line_MaxCount > 0 && isAPI_APC)
                            {
                                if (Total_Lines_Count == Line_MaxCount)
                                {
                                    line = null;
                                    Total_Lines_Count = 0;
                                }
                            }
                        }
                        else
                        {
                            line = null;
                            //if line null then reset total lines to 0
                            if (Line_MaxCount > 0 && isAPI_APC)
                                Total_Lines_Count = 0;
                        }
                    }
                    else
                    {
                        line = null;
                        //if line null then reset total lines to 0
                        if (Line_MaxCount > 0 && isAPI_APC)
                            Total_Lines_Count = 0;
                    }

                    if (line == null)
                    {
                        line = new MVA009BatchLines(GetCtx(), 0, Get_TrxName());
                        line.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["Ad_Client_ID"]));
                        line.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["Ad_Org_ID"]));
                        line.SetVA009_Batch_ID(batch.GetVA009_Batch_ID());
                        docBaseType = Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]);
                        line.SetC_BPartner_ID(_BPartner);
                        // Set BP Location 
                        if ("API" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) ||
                            "APC" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]))
                        {
                            line.SetVA009_PaymentLocation_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_Location_ID"]));
                        }
                        else if ("ARI" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) ||
                                 "ARC" == Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]))
                        {
                            line.SetVA009_ReceiptLocation_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_Location_ID"]));
                        }

                        #region to set bank account of business partner and name on batch line
                        if (_BPartner > 0)
                        {
                            //to check if payment method is CHECK/Cash then skip otherwise set these values
                            if (_baseType != X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Check && _baseType != X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Cash)
                            {
                                if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                                {
                                    //if partner bank account is not present then set null because constraint null is on ther payment table and it will not allow to save zero.
                                    if (Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BP_BankAccount_ID"]) == 0)
                                    {
                                        line.Set_Value("C_BP_BankAccount_ID", null);
                                    }
                                    else
                                    {
                                        line.Set_Value("C_BP_BankAccount_ID", Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                                    }
                                    line.Set_ValueNoCheck("A_Name", Util.GetValueOfString(_ds.Tables[0].Rows[0]["a_name"]));
                                    line.Set_ValueNoCheck("RoutingNo", Util.GetValueOfString(_ds.Tables[0].Rows[0]["RoutingNo"]));
                                    line.Set_ValueNoCheck("AccountNo", Util.GetValueOfString(_ds.Tables[0].Rows[0]["AccountNo"]));
                                }
                            }
                        }
                        #endregion

                        if (_trigger == true)
                        {
                            _sql.Clear();
                            _sql.Append("Select VA009_BPMandate_id from C_BPartner Where C_BPartner_ID=" + _BPartner + " AND IsActive = 'Y' AND AD_Client_ID = " + GetAD_Client_ID());
                            DataSet ds1 = new DataSet();
                            ds1 = DB.ExecuteDataset(_sql.ToString());
                            if (ds1.Tables != null && ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                            {
                                line.SetVA009_BPMandate_ID(Util.GetValueOfInt(ds1.Tables[0].Rows[0]["VA009_BPMandate_id"]));
                            }
                        }

                        if (line.Save(Get_TrxName()))
                        {
                            _VA009_BatchLine_ID = line.GetVA009_BatchLines_ID();
                        }
                        else
                        {
                            Get_TrxName().Rollback();
                            _BPartner = 0;
                            _VA009_BatchLine_ID = 0;
                            //if line null then reset total lines to 0
                            if (Line_MaxCount > 0 && isAPI_APC)
                                Total_Lines_Count = 0;
                        }
                    }

                    //Rakesh(VA228)://to Set Invoice conversion Type
                    C_ConversionType_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_ConversionType_ID"]);
                    lineDetail = new MVA009BatchLineDetails(GetCtx(), 0, Get_TrxName());
                    lineDetail.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["Ad_Client_ID"]));
                    lineDetail.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["Ad_Org_ID"]));
                    lineDetail.SetVA009_BatchLines_ID(line.GetVA009_BatchLines_ID());
                    lineDetail.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_paymentmethod_id"]));
                    lineDetail.SetC_ConversionType_ID(C_ConversionType_ID);
                    //VIS_427 DevOps TaskId: 2156 Setting value based on Gl and Invoice 
                    if (Util.GetValueOfString(ds.Tables[0].Rows[i]["Type"]) != "GL")
                    {
                        lineDetail.SetC_Invoice_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Invoice_ID"]));
                        lineDetail.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_InvoicePaySchedule_id"]));
                    }
                    else if (Util.GetValueOfString(ds.Tables[0].Rows[i]["Type"]) == "GL")
                    {
                        lineDetail.Set_Value("GL_JournalLine_ID", Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Invoice_ID"]));
                    }
                    lineDetail.SetDueDate(Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["DueDate"]));
                    dueamt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]);
                    lineDetail.SetDueAmt(dueamt);
                    Decimal DiscountAmt = 0;
                    if (Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["DiscountDate"]) >= batch.GetDateAcct()) 
                    {
                        DiscountAmt=Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DiscountAmt"]);
                    }
                    if (Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["DiscountDays2"]) >= batch.GetDateAcct())
                    {
                        DiscountAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["Discount2"]);
                    }

                    bool issamme = true; decimal comvertedamt = 0;
                    //if batch currency not equal to invoice currency
                    if (Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]) == batch.GetC_Currency_ID())
                    {
                        issamme = true;
                    }
                    else
                    {
                        issamme = false;
                    }

                    if (!issamme)
                    {
                        //Rakesh(VA228):Changed system date to DateAcct
                        //Convert amount if batch currency not equal to invoice currency
                        dueamt = MConversionRate.Convert(GetCtx(), dueamt, Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]), batch.GetC_Currency_ID(), batch.GetDateAcct(), batch.GetC_ConversionType_ID(), batch.GetAD_Client_ID(), batch.GetAD_Org_ID());
                        if (DiscountAmt > 0)
                        {
                            //Convert discount amount if batch currency not equal to invoice currency
                            DiscountAmt = MConversionRate.Convert(GetCtx(), DiscountAmt, Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]), batch.GetC_Currency_ID(), batch.GetDateAcct(), batch.GetC_ConversionType_ID(), batch.GetAD_Client_ID(), batch.GetAD_Org_ID());
                            if (DiscountAmt == 0)
                            {
                                Get_TrxName().Rollback();
                                msg = Msg.GetMsg(GetCtx(), "NoCurrencyConversion");
                                MCurrency from = MCurrency.Get(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]));
                                MCurrency to = MCurrency.Get(GetCtx(), batch.GetC_Currency_ID()); //Replaced bank currency with batch currency
                                return msg + from.GetISO_Code() + "," + to.GetISO_Code();
                            }
                        }
                        if (dueamt == 0)
                        {
                            Get_TrxName().Rollback();
                            msg = Msg.GetMsg(GetCtx(), "NoCurrencyConversion");
                            MCurrency from = MCurrency.Get(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]));
                            MCurrency to = MCurrency.Get(GetCtx(), batch.GetC_Currency_ID());//Replaced bank currency with batch currency
                            return msg + from.GetISO_Code() + "," + to.GetISO_Code();
                        }
                    }

                    if (Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["DiscountDate"]) >= Util.GetValueOfDateTime(batch.GetDateAcct()))
                    {
                        dueamt = dueamt - DiscountAmt;
                    }
                    if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC" ||
                       Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC")
                    {
                        //VIS_427 10/10/2023  restricted the value according to precision 
                        lineDetail.SetDueAmt(Math.Round(-1 * lineDetail.GetDueAmt(), currency.GetStdPrecision(),MidpointRounding.AwayFromZero));
                        comvertedamt = (-1 * dueamt);
                        DiscountAmt = Decimal.Negate(DiscountAmt);

                    }
                    else
                    {
                        //VIS_427 10/10/2023  restricted the value according to precision 
                        lineDetail.SetDueAmt(Math.Round(lineDetail.GetDueAmt(), currency.GetStdPrecision(), MidpointRounding.AwayFromZero));
                        comvertedamt = (dueamt);
                    }

                    if (issamme == false)
                    {
                        comvertedamt = dueamt;
                        if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC" ||
                            Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC")
                        {
                            comvertedamt = (-1 * comvertedamt);
                            DiscountAmt = Decimal.Negate(DiscountAmt);
                        }
                    }

                    //Replaced bank currency with invoice currency                    
                    lineDetail.SetC_Currency_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]));
                    //VIS_427 10/10/2023  restricted the value according to precision 
                    lineDetail.SetVA009_ConvertedAmt(Math.Round(comvertedamt, currency.GetStdPrecision(), MidpointRounding.AwayFromZero));
                    if (Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["DiscountDate"]) < Util.GetValueOfDateTime(batch.GetDateAcct()))
                    {
                        lineDetail.SetDiscountDate(null);
                        lineDetail.SetDiscountAmt(0);
                    }
                    else if (Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["DiscountDate"]) >= Util.GetValueOfDateTime(batch.GetDateAcct()))
                    {
                        lineDetail.SetDiscountDate(Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["DiscountDate"]));
                        //VIS_427 10/10/2023  restricted the value according to precision 
                        lineDetail.SetDiscountAmt(Math.Round(DiscountAmt, currency.GetStdPrecision(), MidpointRounding.AwayFromZero));
                    }
                    //set the C_BP_BankAccount_ID
                    if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                    {
                        lineDetail.Set_Value("C_BP_BankAccount_ID", Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                    }
                    if (!lineDetail.Save(Get_TrxName()))
                    {
                        Get_TrxName().Rollback();
                        //return message
                        ValueNamePair pp = VLogger.RetrieveError();
                        //some times getting the error pp also
                        //Check first GetName() then GetValue() to get proper Error Message
                        string error = pp != null ? pp.ToString() ?? pp.GetName() : "";
                        if (string.IsNullOrEmpty(error))
                        {
                            error = pp != null ? pp.GetValue() : "";
                        }
                        Get_TrxName().Close();
                        return !string.IsNullOrEmpty(error) ? error : Msg.GetMsg(GetCtx(), "VA009_BatchLineNotCrtd");
                    }
                    else
                    {
                        //increase total line count after save the linedetails
                        if (Line_MaxCount > 0 && isAPI_APC)
                            Total_Lines_Count = Total_Lines_Count + 1;
                        // Update Invoice Schedule with Status as "Assigned To Batch"
                        if (Util.GetValueOfString(ds.Tables[0].Rows[i]["TYPE"]) != "GL")
                        {
                            DB.ExecuteQuery(@"UPDATE C_InvoicePaySchedule SET VA009_ExecutionStatus = 'Y' 
                         WHERE C_InvoicePaySchedule_ID = " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_InvoicePaySchedule_id"]), null, Get_Trx());
                        }
                        //VIS_427 DevOps TaskId: 2156 check added for Gl journalline case to change status "Assigned To Batch"
                        else if (Util.GetValueOfString(ds.Tables[0].Rows[i]["TYPE"]) == "GL")
                        {
                            DB.ExecuteQuery(@"UPDATE GL_JournalLine SET VA009_IsAssignedtoBatch = 'Y' 
                         WHERE GL_JournalLine_ID = " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Invoice_id"]), null, Get_Trx());
                        }
                    }
                }

                //Updating the C_BP_BankAccount_ID on Batch Lines Tab
                if (!X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Check.Equals(_baseType) && !X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Cash.Equals(_baseType))
                {
                    ds = DB.ExecuteDataset(@"SELECT MAX(BLD.C_BP_BankAccount_ID) AS C_BP_BankAccount_ID,BL.VA009_BatchLines_ID, 
                                        BPBA.A_Name,BPBA.RoutingNo,BPBA.AccountNo FROM VA009_BatchLineDetails BLD
                                        INNER JOIN VA009_BatchLines BL ON BLD.VA009_BatchLines_ID = BL.VA009_BatchLines_ID
                                        INNER JOIN VA009_Batch B ON BL.VA009_Batch_ID=B.VA009_Batch_ID
                                        INNER JOIN C_BP_BankAccount BPBA ON BLD.C_BP_BankAccount_ID=BPBA.C_BP_BankAccount_ID
                                        WHERE B.VA009_Batch_ID = " + batch.GetVA009_Batch_ID() + @" AND BPBA.IsActive='Y'
                                        GROUP BY BL.VA009_BatchLines_ID, BLD.C_BP_BankAccount_ID, BPBA.A_Name, 
                                        BPBA.RoutingNo, BPBA.AccountNo", null, Get_TrxName());
                    if (ds != null && ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            line = new MVA009BatchLines(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_BatchLines_ID"]), Get_TrxName());
                            line.Set_Value("C_BP_BankAccount_ID", Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BP_BankAccount_ID"]));
                            line.Set_ValueNoCheck("A_Name", Util.GetValueOfString(ds.Tables[0].Rows[i]["a_name"]));
                            line.Set_ValueNoCheck("RoutingNo", Util.GetValueOfString(ds.Tables[0].Rows[i]["RoutingNo"]));
                            line.Set_ValueNoCheck("AccountNo", Util.GetValueOfString(ds.Tables[0].Rows[i]["AccountNo"]));
                            if (!line.Save(Get_TrxName()))
                            {
                                Get_TrxName().Rollback();
                                //return message
                                ValueNamePair pp = VLogger.RetrieveError();
                                //some times getting the error pp also
                                //Check first GetName() then GetValue() to get proper Error Message
                                string error = pp != null ? pp.ToString() ?? pp.GetName() : "";
                                if (string.IsNullOrEmpty(error))
                                {
                                    error = pp != null ? pp.GetValue() : "";
                                }
                                Get_TrxName().Close();
                                return !string.IsNullOrEmpty(error) ? error : Msg.GetMsg(GetCtx(), "VA009_BatchLineNotCrtd");
                            }
                        }
                    }
                }

                batch.SetVA009_GenerateLines("Y");
                //batch.SetProcessed(true); //Commeted by Arpit asked by Ashish Gandhi to set processed only if the Payment completion is done
                if (!batch.Save(Get_TrxName()))
                {
                    Get_TrxName().Rollback();
                    //return message
                    ValueNamePair pp = VLogger.RetrieveError();
                    //some times getting the error pp also
                    //Check first GetName() then GetValue() to get proper Error Message
                    string error = pp != null ? pp.ToString() ?? pp.GetName() : "";
                    if (string.IsNullOrEmpty(error))
                    {
                        error = pp != null ? pp.GetValue() : "";
                    }
                    Get_TrxName().Close();
                    return !string.IsNullOrEmpty(error) ? error : Msg.GetMsg(GetCtx(), "VA009_BatchLineNotCrtd");
                }

                return Msg.GetMsg(GetCtx(), "VA009_BatchLineCrtd");
            }
            else
            {
                return Msg.GetMsg(GetCtx(), "NoRecords");
            }
        }

        /// <summary>
        /// Get Default Conversion Type ID From the system
        /// </summary>
        /// <param name="_sql"></param>
        /// <returns></returns>
        private int GetDefaultConversionType(StringBuilder _sql)
        {
            _sql.Clear();
            _sql.Append("SELECT C_ConversionType_ID FROM C_ConversionType WHERE IsActive='Y' AND IsDefault='Y'");
            return Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, Get_TrxName()));
        }

        /// <summary> Arpit
        /// Delete All Batch Lines and Batch Details Lines of selected Batch
        /// </summary>
        /// <param name="_sql"></param>
        /// <param name="batch_ID"></param>
        /// <param name="ctx_"></param>
        /// <param name="trx_"></param>
        /// <returns> String msg if got error in deleting the records</returns>
        private static String DeleteBatchLines(StringBuilder _sql, int batch_ID, Ctx ctx_, Trx trx_)
        {
            MVA009BatchLines bLines = null;
            _sql.Clear();
            _sql.Append("SELECT VA009_BatchLines_ID FROM VA009_BatchLines WHERE VA009_Batch_ID=" + batch_ID);
            using (DataSet ds = DB.ExecuteDataset(_sql.ToString(), null, trx_))
            {
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (Int32 i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        bLines = new MVA009BatchLines(ctx_, Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_BatchLines_ID"]), trx_);
                        if (!bLines.Delete(false, trx_))
                        {
                            trx_.Rollback();
                            return Msg.GetMsg(ctx_, "VA009_BatchLineNotCrtd");
                        }
                    }
                }
            }
            return "";
        }

    }
}