using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAdvantage.Classes;
using VAdvantage.Common;
using VAdvantage.Process;
using VAdvantage.Model;
//using VAdvantage.DataBase;
using VAdvantage.SqlExec;
using VAdvantage.Utility;
using System.Data;
using System.Data.SqlClient;
using VAdvantage.Logging;
using VAdvantage.ProcessEngine;
using ViennaAdvantage.Model;
using System.Reflection;
using ViennaAdvantage.Process;
using VAdvantage.DataBase;
using System.Dynamic;
using System.Globalization;
using ViennaAdvantage.Common;

namespace VA009.Models
{
    public class PaymentModel
    {
        private static VLogger _log = VLogger.GetVLogger(typeof(PaymentModel).FullName);

        public LoadData GetloadData(int pageNo, int pageSize, Ctx ctx, string whereQry, string OrgWhr, string SearchText, string WhrDueDate, string TransType, string FromDate, string ToDate)
        {
            LoadData obj = new LoadData();
            obj.paymentdata = GetPaymentData(pageNo, pageSize, ctx, whereQry, SearchText, WhrDueDate, TransType, FromDate, ToDate);
            obj.bankdetails = GetBankDetails(ctx, OrgWhr);
            obj.Cbk = Getcashbooks(ctx, OrgWhr);
            return obj;
        }

        /// <summary>
        /// Get Currency of Bank Account
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="BankAccount_ID">Bank Account</param>
        /// <returns>C_Currency_ID</returns>
        public int GetBankAccountCurrency(Ctx ctx, int BankAccount_ID)
        {
            string sql = "SELECT C_Currency_ID FROM C_BankAccount WHERE C_BankACcount_ID=" + BankAccount_ID;
            return Util.GetValueOfInt(DB.ExecuteScalar(sql));
        }
        public List<BPDetails> GetBPnames(string searchText, Ctx ct)
        {
            List<BPDetails> Bp = new List<BPDetails>();
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT C_BPartner.C_BPartner_ID,C_BPartner.Name FROM C_BPartner C_BPartner WHERE C_BPartner.ISACTIVE='Y' AND UPPER(C_BPartner.Name) like UPPER('%" + searchText + "%')");
            string finalQuery = MRole.GetDefault(ct).AddAccessSQL(sql.ToString(), "C_BPartner", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(finalQuery);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    BPDetails _bpData = new BPDetails();
                    _bpData.C_BPartner_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_ID"]);
                    _bpData.Name = Util.GetValueOfString(ds.Tables[0].Rows[i]["Name"]);
                    Bp.Add(_bpData);
                }
            }
            sql = null;
            return Bp;
        }

        /// <summary>
        /// Get Bank detail like currency reconciled and un-reconciled balance
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="OrgWhr">basd on organization - can pass multiple organization</param>
        /// <returns>list of bank detail class</returns>
        public List<BankDetails> GetBankDetails(Ctx ctx, string OrgWhr)
        {
            List<BankDetails> bd = new List<BankDetails>();
            StringBuilder sql = new StringBuilder();
            //Table name must Camel format
            //Log Issue handled
            sql.Append(@"SELECT cs.Name,  bc.C_Bank_ID,  bc.C_BankAccount_ID,  bc.AccountNo,  cc.Iso_Code, bc.CurrentBalance, bc.UnMatchedBalance, cs.AD_Org_ID, cs.AD_Client_ID,
                         SUM(p.PayAmt) AS TotalAmt FROM C_BankAccount bc INNER JOIN C_Bank cs ON (cs.C_Bank_ID =bc.C_Bank_ID) LEFT JOIN C_Payment p ON 
                         (p.C_BankAccount_ID=bc.C_BankAccount_ID) INNER JOIN C_Currency cc ON (cc.C_Currency_ID =bc.C_Currency_ID) WHERE cs.ISACTIVE='Y' AND bc.ISACTIVE='Y' AND cs.IsOwnBank ='Y' ");

            // check access of Organization on Bank Account not on Bank
            string finalQuery = MRole.GetDefault(ctx).AddAccessSQL(sql.ToString(), "bc", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);

            // sorting curency code wise
            if (OrgWhr != string.Empty)
                finalQuery += (OrgWhr + @" GROUP BY cs.Name, bc.C_Bank_ID, bc.C_BankAccount_ID, bc.CurrentBalance, bc.UnMatchedBalance, bc.AccountNo, cc.Iso_Code, cs.AD_Org_ID,
                                       cs.AD_Client_ID ORDER BY cc.Iso_Code");
            else
                finalQuery += (@" GROUP BY cs.name, bc.C_Bank_ID, bc.C_BankAccount_ID, bc.CurrentBalance, bc.UnMatchedBalance, bc.AccountNo, cc.Iso_Code, cs.AD_Org_ID, 
                              cs.AD_Client_ID ORDER BY cc.Iso_Code ");


            DataSet ds = DB.ExecuteDataset(finalQuery);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    BankDetails _bdData = new BankDetails();
                    _bdData.C_Bank_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Bank_ID"]);
                    _bdData.BankName = Util.GetValueOfString(ds.Tables[0].Rows[i]["Name"]);
                    _bdData.C_BankAccount_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BankAccount_ID"]);
                    _bdData.BankAccountNumber = Util.GetValueOfString(ds.Tables[0].Rows[i]["AccountNo"]);
                    _bdData.CurrencyCode1 = Util.GetValueOfString(ds.Tables[0].Rows[i]["Iso_Code"]);
                    _bdData.TotalAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["TotalAmt"]);
                    _bdData.CurrentBalance = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["CurrentBalance"]);

                    // Reconciled and Unreconciled amount should pick from Bank Account's Current Balanace and Unmatched Balance
                    _bdData.UnreconsiledAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["UnMatchedBalance"]);


                    //string sql1 = @"SELECT (t.bb-t.aa) FROM  (SELECT    (SELECT SUM(payamt)    FROM c_payment    WHERE ad_client_id  =" + ctx.GetAD_Client_ID() + "    AND c_bankaccount_id=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BankAccount_ID"]) + "    AND isreconciled    ='N'    AND c_doctype_id    =      (SELECT MAX(c_doctype_id)      FROM C_DocType      WHERE docbasetype='APP'      AND ad_client_id =" + ctx.GetAD_Client_ID() + "      )    )AS aa,    (SELECT SUM(payamt)    FROM c_payment    WHERE ad_client_id  =" + ctx.GetAD_Client_ID() + "    AND c_bankaccount_id=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BankAccount_ID"]) + "    AND isreconciled    ='N'    AND c_doctype_id    =      (SELECT MAX(c_doctype_id)      FROM C_DocType      WHERE docbasetype='ARR'      AND ad_client_id =" + ctx.GetAD_Client_ID() + "      )    ) AS bb  FROM dual  )t ";

                    //commented because Unreconciled amount was not converted in bank currency.

                    //string sql1 = @"SELECT (NVL(t.Rec,0)-NVL(t.RecMemo,0))-(NVL(t.Payble,0)-NVL(t.PayMemo,0)) as unreconsiled FROM (SELECT (SELECT SUM(payamt) " +
                    //        "FROM c_payment WHERE ad_client_id  =" + ctx.GetAD_Client_ID() + " AND c_bankaccount_id=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BankAccount_ID"]) + " AND isreconciled ='N' AND Docstatus='CO' AND payamt > 0 AND " +
                    //        "c_doctype_id IN (SELECT (c_doctype_id) FROM C_DocType WHERE docbasetype='APP' AND IsActive = 'Y' AND ad_client_id =" + ctx.GetAD_Client_ID() + " ) " +
                    //        ")AS Payble, (SELECT SUM(payamt) FROM c_payment WHERE ad_client_id  =" + ctx.GetAD_Client_ID() + " AND c_bankaccount_id=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BankAccount_ID"]) + " AND isreconciled " +
                    //        "='N' AND Docstatus='CO' AND payamt < 0 AND c_doctype_id IN (SELECT (c_doctype_id) FROM C_DocType WHERE docbasetype='APP' AND IsActive = 'Y' " +
                    //        "AND ad_client_id =" + ctx.GetAD_Client_ID() + "))AS RecMemo, (SELECT SUM(payamt) FROM c_payment WHERE ad_client_id  =" + ctx.GetAD_Client_ID() + " AND " +
                    //        "c_bankaccount_id=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BankAccount_ID"]) + " AND Docstatus='CO' AND isreconciled ='N' AND payamt > 0 AND c_doctype_id IN (SELECT (c_doctype_id) " +
                    //        "FROM C_DocType WHERE docbasetype='ARR' AND IsActive = 'Y' AND ad_client_id =" + ctx.GetAD_Client_ID() + " ) ) AS Rec, (SELECT SUM(payamt) FROM c_payment " +
                    //        "WHERE ad_client_id =" + ctx.GetAD_Client_ID() + " AND c_bankaccount_id=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BankAccount_ID"]) + " AND Docstatus='CO' AND isreconciled ='N' AND payamt < 0 AND c_doctype_id IN " +
                    //        "(SELECT (c_doctype_id) FROM C_DocType WHERE docbasetype='ARR' AND IsActive = 'Y' AND ad_client_id =" + ctx.GetAD_Client_ID() + " ) ) AS PayMemo  FROM  dual)t";
                    //decimal Amt = Util.GetValueOfDecimal(DB.ExecuteScalar(sql1, null, null));
                    //_bdData.UnreconsiledAmt = Amt;
                    bd.Add(_bdData);
                }
            }
            sql = null;
            return bd;
        }

        /// <summary>
        /// Get Payment Schedules Data
        /// </summary>
        /// <param name="pageNo">Current Page No</param>
        /// <param name="pageSize">No of records per Page</param>
        /// <param name="ctx">Context</param>
        /// <param name="whereQry">Query Filter</param>
        /// <param name="SearchText">Search Text</param>
        /// <param name="WhrDueDate">Due Date</param>
        /// <param name="TransType">Transaction Type</param>
        /// <param name="FromDate">Date From</param>
        /// <param name="ToDate">Date To</param>
        /// <returns>List of Payment Schedules Data</returns>
        public List<PaymentData> GetPaymentData(int pageNo, int pageSize, Ctx ctx, string whereQry, string SearchText, string WhrDueDate, string TransType, string FromDate, string ToDate)
        {
            string query;
            List<PaymentData> _payList = new List<PaymentData>();
            int countRecords = 0;

            #region Commented Query
            //if (TransTypes.Count() == 0 || TransTypes.Count() == 2 || TransTypes[0] == 1)
            //{
            //    sql.Append(@"SELECT t.VA009_PaymentMode,  t.c_Bpartner_id,  t.C_invoice_ID,  t.DocumentNo,  t.C_Bpartner,  t.c_bp_group_id,  t.c_bp_group,  
            //             t.C_InvoicePaySchedule_ID,  t.VA009_PaymentMethod_ID,  t.VA009_PaymentMethod,  t.va009_paymentbasetype,  t.VA009_PaymentRule,  t.VA009_PaymentType,  t.VA009_PaymentTrigger,
            //             t.va009_plannedduedate, t.VA009_FollowupDate,  t.VA009_RecivedAmt,  t.DueAmt, t.VA009_OpenAmnt, t.VA009_ExecutionStatus,  t.ad_org_id,  t.ad_client_id ,  t.C_Currency_ID,  
            //             t.ISO_CODE, t.basecurrency, t.multiplyrate, t.Due_Date_Diff, t.basecurrencycode,t.GrandTotal, t.va009_transactiontype, t.IsHoldPayment FROM (");

            //    string query = @"SELECT pm.VA009_PaymentMode,cb.c_Bpartner_id, cs.C_invoice_ID,inv.DocumentNo, cb.name as C_Bpartner, cb.c_bp_group_id, cbg.name as c_bp_group, cs.C_InvoicePaySchedule_ID,
            //             pm.VA009_PaymentMethod_ID, pm.VA009_name as VA009_PaymentMethod,pm.va009_paymentbasetype,pm.VA009_PaymentRule, pm.VA009_PaymentType, pm.VA009_PaymentTrigger,
            //             cs.duedate as va009_plannedduedate,
            //             cs.VA009_PlannedDueDate as VA009_FollowupDate,inv.VA009_PaidAmount AS VA009_RecivedAmt,
            //             CASE WHEN (cd.DOCBASETYPE IN ('ARI','APC')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) WHEN (cd.DOCBASETYPE IN ('API','ARC'))     
            //             THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) * 1  END AS DueAmt,
            //             cs.VA009_OpenAmnt, rsf.name as VA009_ExecutionStatus,  cs.ad_org_id,  cs.ad_client_id ,
            //             inv.C_Currency_ID,  cc.ISO_CODE, ac.c_currency_id as basecurrency,  CURRENCYRATE(cc.C_CURRENCY_ID,cy.C_CURRENCY_ID,TRUNC(sysdate)," + conversionType_ID
            //             + @",inv.AD_Client_ID,inv.AD_ORG_ID) as multiplyrate, cy.ISO_CODE as basecurrencycode,inv.GrandTotal, (to_date(TO_CHAR(TRUNC(cs.VA009_PlannedDueDate)),'dd/mm/yyyy')
            //            -to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy')) as Due_Date_Diff,cs.duedate, 'Invoice' AS VA009_TransactionType, cs.IsHoldPayment FROM 
            //             C_InvoicePaySchedule cs INNER JOIN VA009_PaymentMethod pm ON pm.VA009_PaymentMethod_ID=cs.VA009_PaymentMethod_ID INNER JOIN C_Doctype 
            //             cd ON cs.C_Doctype_ID=cd.C_Doctype_ID INNER JOIN ad_ref_list rsf ON rsf.value= cs.VA009_ExecutionStatus INNER JOIN ad_reference re ON 
            //             rsf.ad_reference_id=re.ad_reference_id LEFT JOIN C_invoice inv ON inv.C_Invoice_ID=cs.C_invoice_ID LEFT JOIN C_BPartner cb ON 
            //             cb.c_bpartner_id=inv.c_bpartner_id INNER JOIN c_bp_group cbg ON cb.c_bp_group_id=cbg.c_bp_group_id INNER JOIN C_Currency cc ON 
            //             inv.C_Currency_ID=cc.C_Currency_ID INNER JOIN AD_ClientInfo aclnt ON aclnt.AD_Client_ID =cs.AD_Client_ID INNER JOIN C_acctschema ac ON 
            //             ac.C_AcctSchema_ID =aclnt.C_AcctSchema1_ID INNER JOIN C_CURRENCY CY ON AC.C_CURRENCY_ID=CY.C_CURRENCY_ID  " +
            //             whereQry + @"AND re.name= 'VA009_ExecutionStatus' AND re.Export_ID='VA009_20000279' AND rsf.value NOT IN ( 'Y','J')
            //             AND cs.AD_Client_ID = " + ctx.GetAD_Client_ID() + " AND NVL(cs.C_Payment_ID , 0) = 0 AND NVL(cs.C_CashLine_ID , 0) = 0 AND cs.VA009_IsPaid = 'N' ";

            //    query = MRole.GetDefault(ctx).AddAccessSQL(query, "cs", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            //    sql.Append(query);

            //    sql.Append(") t WHERE t.DueAmt !=0 ");
            //    string whrduedte = DueDateSearch(WhrDueDate);
            //    sql.Append(whrduedte);

            //    if (SearchText != string.Empty)
            //    {
            //        //JID_1793 -- when search text contain "=" then serach with documnet no only
            //        if (SearchText.Contains("="))
            //        {
            //            String[] myStringArray = SearchText.TrimStart(new Char[] { ' ', '=' }).Split(',');
            //            if (myStringArray.Length > 0)
            //            {
            //                sql.Append(" AND UPPER(t.DocumentNo) IN ( ");
            //                for (int z = 0; z < myStringArray.Length; z++)
            //                {
            //                    if (z != 0)
            //                    { sql.Append(","); }
            //                    sql.Append(" UPPER('" + myStringArray[z].Trim(new Char[] { ' ' }) + "')");
            //                }
            //                sql.Append(")");
            //            }
            //        }
            //        else
            //        {
            //            sql.Append(" AND ( UPPER(t.C_Bpartner) LIKE UPPER('%" + SearchText + "%') OR (UPPER(t.c_bp_group) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_PaymentMethod) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_ExecutionStatus) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.DocumentNo) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.DueAmt) LIKE UPPER('%" + SearchText + "%'))  OR (UPPER(to_date(TO_CHAR(TRUNC(t.VA009_FollowupDate)),'dd/mm/yyyy')) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(to_date(TO_CHAR(TRUNC(t.va009_plannedduedate)),'dd/mm/yyyy')) LIKE UPPER('%" + SearchText + "%')) ) ");
            //        }
            //    }

            //    if (FromDate != string.Empty && ToDate != string.Empty)
            //    {
            //        sql.Append(" and t.VA009_FollowupDate BETWEEN  ");
            //        sql.Append(GlobalVariable.TO_DATE(dateFrom, true) + " AND ");
            //        sql.Append(GlobalVariable.TO_DATE(dateTo, true));
            //    }
            //    else if (FromDate != string.Empty && ToDate == string.Empty)
            //    {
            //        sql.Append(" and t.VA009_FollowupDate >=" + GlobalVariable.TO_DATE(dateFrom, true));
            //    }
            //    else if (FromDate == string.Empty && ToDate != string.Empty)
            //    {
            //        sql.Append(" and t.VA009_FollowupDate <=" + GlobalVariable.TO_DATE(dateTo, true));
            //    }
            //}
            //if (TransTypes.Count() == 0 || TransTypes.Count() == 2)
            //{
            //    sql.Append(" UNION ");
            //}
            //if (TransTypes.Count() == 0 || TransTypes.Count() == 2 || TransTypes[0] == 0)
            //{
            //    sql.Append(@"SELECT t.VA009_PaymentMode,  t.c_Bpartner_id,  t.C_invoice_ID,  t.DocumentNo,  t.C_Bpartner,  t.c_bp_group_id,  t.c_bp_group,  t.C_InvoicePaySchedule_ID,
            //            t.VA009_PaymentMethod_ID,  t.VA009_PaymentMethod,  t.va009_paymentbasetype, t.VA009_PaymentRule,  t.VA009_PaymentType,  t.VA009_PaymentTrigger,  t.va009_plannedduedate, 
            //            t.VA009_FollowupDate,  t.VA009_RecivedAmt, t.DueAmt, t.VA009_OpenAmnt,  t.VA009_ExecutionStatus,  t.ad_org_id,  t.ad_client_id ,  t.C_Currency_ID,  t.ISO_CODE,  t.basecurrency, 
            //            t.multiplyrate, t.Due_Date_Diff, t.basecurrencycode, t.GrandTotal, t.va009_transactiontype, t.IsHoldPayment FROM ( ");

            //    string query = @" SELECT pm.VA009_PaymentMode, cb.c_Bpartner_id, cs.C_Order_ID AS C_invoice_ID, inv.DocumentNo, cb.name AS C_Bpartner, cb.c_bp_group_id,
            //            cbg.name AS c_bp_group, cs.VA009_OrderPaySchedule_ID AS C_InvoicePaySchedule_ID, pm.VA009_PaymentMethod_ID, pm.VA009_name AS VA009_PaymentMethod, pm.va009_paymentbasetype,
            //            pm.VA009_PaymentRule, pm.VA009_PaymentType, pm.VA009_PaymentTrigger, cs.duedate AS va009_plannedduedate, cs.VA009_PlannedDueDate  AS VA009_FollowupDate,    
            //            0 AS VA009_RecivedAmt, 
            //            CASE  WHEN (cd.DOCBASETYPE IN ('SOO','APC')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) WHEN (cd.DOCBASETYPE IN ('POO','ARC')) 
            //            THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) * 1 END AS DueAmt,
            //            cs.VA009_OpenAmnt, rsf.name AS VA009_ExecutionStatus, cs.ad_org_id, cs.ad_client_id, inv.C_Currency_ID, cc.ISO_CODE, ac.c_currency_id  AS basecurrency,
            //            CURRENCYRATE(cc.C_CURRENCY_ID,cy.C_CURRENCY_ID,TRUNC(sysdate)," + conversionType_ID + @",inv.AD_Client_ID,inv.AD_ORG_ID) AS multiplyrate,  cy.ISO_CODE AS basecurrencycode,
            //            inv.GrandTotal, (to_date(TO_CHAR(TRUNC(cs.VA009_PlannedDueDate)),'dd/mm/yyyy') -to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy')) AS Due_Date_Diff,
            //            cs.duedate, 'Order' AS VA009_TransactionType, 'N' AS IsHoldPayment
            //            FROM VA009_OrderPaySchedule cs INNER JOIN VA009_PaymentMethod pm   ON pm.VA009_PaymentMethod_ID=cs.VA009_PaymentMethod_ID
            //            INNER JOIN ad_ref_list rsf  ON rsf.value= cs.VA009_ExecutionStatus  INNER JOIN ad_reference re  ON (rsf.ad_reference_id=re.ad_reference_id
            //            AND re.name = 'VA009_ExecutionStatus')  INNER JOIN C_Order inv  ON inv.C_Order_ID=cs.C_Order_ID  INNER JOIN C_Doctype cd
            //            ON inv.C_Doctype_ID=cd.C_Doctype_ID  INNER JOIN C_BPartner cb  ON cb.c_bpartner_id=inv.c_bpartner_id  INNER JOIN c_bp_group cbg  ON cb.c_bp_group_id=cbg.c_bp_group_id
            //            INNER JOIN C_Currency cc  ON inv.C_Currency_ID=cc.C_Currency_ID  INNER JOIN AD_ClientInfo aclnt  ON aclnt.AD_Client_ID =cs.AD_Client_ID
            //            INNER JOIN C_acctschema ac  ON ac.C_AcctSchema_ID =aclnt.C_AcctSchema1_ID  INNER JOIN C_CURRENCY CY  ON AC.C_CURRENCY_ID=CY.C_CURRENCY_ID " +
            //            whereQry.Replace("c_invoice_id", "C_Order_ID") + @" AND re.name= 'VA009_ExecutionStatus' AND re.Export_ID='VA009_20000279' AND rsf.value NOT IN ( 'Y','J')
            //            AND cs.AD_Client_ID = " + ctx.GetAD_Client_ID() + " AND NVL(cs.C_Payment_ID , 0) = 0 AND NVL(cs.C_CashLine_ID , 0) = 0 AND cs.VA009_IsPaid = 'N' ";

            //    query = MRole.GetDefault(ctx).AddAccessSQL(query, "cs", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            //    sql.Append(query);

            //    sql.Append(") t WHERE t.DueAmt !=0 ");
            //    string whrduedte = DueDateSearch(WhrDueDate);
            //    sql.Append(whrduedte);
            //    if (SearchText != string.Empty)
            //    {
            //        // JID_1793 -- when search text contain "=" then serach with documnet no 
            //        if (SearchText.Contains("="))
            //        {
            //            String[] myStringArray = SearchText.TrimStart(new Char[] { ' ', '=' }).Split(',');
            //            if (myStringArray.Length > 0)
            //            {
            //                sql.Append(" AND UPPER(t.DocumentNo) IN ( ");
            //                for (int z = 0; z < myStringArray.Length; z++)
            //                {
            //                    if (z != 0)
            //                    { sql.Append(","); }
            //                    sql.Append(" UPPER('" + myStringArray[z].Trim(new Char[] { ' ' }) + "')");
            //                }
            //                sql.Append(")");
            //            }
            //        }
            //        else
            //        {
            //            sql.Append(" AND ( UPPER(t.C_Bpartner) LIKE UPPER('%" + SearchText + "%') OR (UPPER(t.c_bp_group) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_PaymentMethod) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_ExecutionStatus) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.DocumentNo) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.DueAmt) LIKE UPPER('%" + SearchText + "%'))  OR (UPPER(to_date(TO_CHAR(TRUNC(t.VA009_FollowupDate)),'dd/mm/yyyy')) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(to_date(TO_CHAR(TRUNC(t.va009_plannedduedate)),'dd/mm/yyyy')) LIKE UPPER('%" + SearchText + "%')) ) ");
            //        }
            //    }

            //    if (FromDate != string.Empty && ToDate != string.Empty)
            //    {
            //        sql.Append(" and t.VA009_FollowupDate BETWEEN  ");
            //        sql.Append(GlobalVariable.TO_DATE(dateFrom, true) + " AND ");
            //        sql.Append(GlobalVariable.TO_DATE(dateTo, true));
            //    }
            //    else if (FromDate != string.Empty && ToDate == string.Empty)
            //    {
            //        sql.Append(" and t.VA009_FollowupDate >=" + GlobalVariable.TO_DATE(dateFrom, true));
            //    }
            //    else if (FromDate == string.Empty && ToDate != string.Empty)
            //    {
            //        sql.Append(" and t.VA009_FollowupDate <=" + GlobalVariable.TO_DATE(dateTo, true));
            //    }
            //}
            #endregion

            query = DBFuncCollection.GetPaymentDataSql(ctx, whereQry, SearchText, WhrDueDate, TransType, FromDate, ToDate);
            DataSet ds = VIS.DBase.DB.ExecuteDatasetPaging(query, pageNo, pageSize);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                //for paging 
                if (pageNo == 1)
                    countRecords = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) FROM ( " + query + " ) t"));

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    PaymentData _payData = new PaymentData();
                    _payData.C_BPartner_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_ID"]);
                    _payData.C_BP_Group_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BP_Group_ID"]);
                    _payData.VA009_PaymentMode = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA009_PaymentMode"]);
                    _payData.C_InvoicePaySchedule_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_InvoicePaySchedule_ID"]);
                    _payData.C_Invoice_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Invoice_ID"]);
                    _payData.VA009_PaymentMethod_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_PaymentMethod_ID"]);
                    _payData.VA009_plannedduedate = Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["VA009_plannedduedate"]);
                    _payData.VA009_FollowupDate = Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["VA009_FollowupDate"]);
                    _payData.VA009_RecivedAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_RecivedAmt"]);
                    _payData.DueAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]);
                    //_payData.BaseAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_OpenAmnt"]);
                    _payData.VA009_ExecutionStatus = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA009_ExecutionStatus"]);
                    _payData.AD_Org_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["AD_Org_ID"]);
                    _payData.AD_Client_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["AD_Client_ID"]);
                    _payData.C_Bpartner = Util.GetValueOfString(ds.Tables[0].Rows[i]["C_Bpartner"]);
                    _payData.C_BP_Group = Util.GetValueOfString(ds.Tables[0].Rows[i]["C_BP_Group"]);
                    _payData.LastChat = GetLastChat(ctx, Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_InvoicePaySchedule_ID"]));
                    _payData.VA009_PaymentMethod = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA009_PaymentMethod"]);
                    _payData.PaymwentBaseType = Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_paymentbasetype"]);
                    _payData.CurrencyCode = Util.GetValueOfString(ds.Tables[0].Rows[i]["ISO_CODE"]);
                    _payData.BaseCurrencyCode = Util.GetValueOfString(ds.Tables[0].Rows[i]["basecurrencycode"]);
                    _payData.DocumentNo = Util.GetValueOfString(ds.Tables[0].Rows[i]["DocumentNo"]);
                    _payData.TotalInvAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["GrandTotal"]);
                    _payData.TransactionType = Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_transactiontype"]);
                    _payData.IsHoldPayment = Util.GetValueOfString(ds.Tables[0].Rows[i]["IsHoldPayment"]);
                    //Added By Manjot For Getting All the information about Payment Method
                    _payData.PaymentRule = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA009_PaymentRule"]);
                    _payData.PaymentType = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA009_PaymentType"]);
                    _payData.PaymentTriggerBy = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA009_PaymentTrigger"]);
                    _payData.DocBaseType = Util.GetValueOfString(DB.ExecuteScalar(@"SELECT dt.DocBaseType FROM C_Invoice inv INNER JOIN C_DocType dt ON inv.C_DocTypeTarget_ID=dt.C_DocType_ID 
                            WHERE inv.C_Invoice_ID=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Invoice_ID"])));
                    _payData.Systemdate = string.Empty;
                    if (Util.GetValueOfString(ds.Tables[0].Rows[i]["basecurrencycode"]) != Util.GetValueOfString(ds.Tables[0].Rows[i]["ISO_CODE"]))
                    {
                        _payData.convertedAmt = (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]) * Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["multiplyrate"]));
                        _payData.BaseAmt = (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]) * Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["multiplyrate"]));
                    }
                    else
                    {
                        _payData.BaseAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]);
                    }
                    //_payData.paymentCount = ds.Tables[0].Rows.Count;
                    _payData.paymentCount = countRecords;
                    _payList.Add(_payData);
                }
            }
            //sql = null;
            return _payList;
        }

        /// <summary>
        /// Get process_ID from Bank window
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="bankAct_Id">C_BankAccount_ID</param>
        /// <returns>returns Process_ID</returns>
        public int GetProcessId(Ctx ctx, string bankAct_Id)
        {
            int _process_Id = Util.GetValueOfInt(DB.ExecuteScalar("SELECT AD_Process_ID from C_BankAccountDoc WHERE IsActive='Y' AND C_BankAccount_ID=" + Util.GetValueOfInt(bankAct_Id), null, null));
            return _process_Id;
        }

        public List<CashBook> Getcashbooks(Ctx ctx, string OrgWhr)
        {
            List<CashBook> Cbk = new List<CashBook>();
            StringBuilder sql = new StringBuilder();
            //Table name must Camel format
            //Log Issue handled
            sql.Append(@"SELECT cs.name,  cs.completedbalance,  c.iso_code,cs.C_Cashbook_ID FROM C_CashBook cs INNER JOIN C_Currency c ON 
                         (c.c_currency_id=cs.c_currency_id) WHERE cs.ISACTIVE='Y' ");
            sql.Append(OrgWhr);
            string finalQuery = MRole.GetDefault(ctx).AddAccessSQL(sql.ToString(), "cs", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);

            finalQuery += (" order by cs.c_cashbook_id");

            DataSet ds = DB.ExecuteDataset(finalQuery);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CashBook bk = new CashBook();
                    bk.CBCurrencyCode = Util.GetValueOfString(ds.Tables[0].Rows[i]["iso_code"]);
                    bk.CashBookName = Util.GetValueOfString(ds.Tables[0].Rows[i]["name"]);
                    bk.Csb_Amt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["completedbalance"]);
                    bk.C_Cashbook_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Cashbook_ID"]);
                    Cbk.Add(bk);
                }
            }
            sql = null;
            return Cbk;
        }

        /// <summary>
        /// Get Data for Check Payment
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="InvPayids">List of Invoice Schedules</param>
        /// <param name="bank_id">Bank</param>
        /// <param name="acctno">Bank Account</param>
        /// <param name="chkno">Check NO</param>
        /// <param name="OrderPayids">List Of Order Schedules</param>
        /// <returns>List of Check Payment Data</returns>
        public List<PaymentData> GetChquePopUpdata(Ctx ctx, string InvPayids, int bank_id, int acctno, string chkno, string OrderPayids)
        {
            if (string.IsNullOrEmpty(OrderPayids))
            {
                OrderPayids = "0";
            }
            if (string.IsNullOrEmpty(InvPayids))
            {
                InvPayids = "0";
            }
            //Rakesh(VA228):Get convertiontype,discount amount done on date 17/Sep/2021
            List<PaymentData> _lstChqPay = new List<PaymentData>();
            StringBuilder sql = new StringBuilder();
            sql.Append(@"SELECT inv.C_DocType_ID, pm.VA009_PaymentMode,pm.VA009_PaymentMethod_ID,cb.c_Bpartner_id, 
                                inv.DocumentNo, cb.name AS C_Bpartner,cs.C_Invoice_ID,
                                cs.C_InvoicePaySchedule_ID,inv.C_Currency_ID,cc.ISO_CODE, ");
            sql.Append(@" CASE WHEN (cd.DOCBASETYPE IN ('ARI','APC')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2))    
                               WHEN (cd.DOCBASETYPE IN ('API','ARC')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) * 1  END AS DueAmt, "); // -1 Because during payble dont show negative amount on UI
            /* VIS0045 : Date - 15-Feb-2022*/
            /* This Enhancement will create Batch Line with Business Partner Location*/
            /* System will BP location where "Pay From Address" and "Remit To Address" is True*/
            sql.Append(@" CASE WHEN (bpLoc.IsPayFrom = 'Y' AND cd.DocBaseType IN ('ARI' , 'ARC')) THEN  inv.C_BPartner_Location_ID
                               WHEN (bpLoc.IsRemitTo = 'Y' AND cd.DocBaseType IN ('API' , 'APC')) THEN  inv.C_BPartner_Location_ID
                               WHEN (bpLoc.IsPayFrom = 'N' AND cd.DocBaseType IN ('ARI' , 'ARC')) THEN  bpLoc.VA009_ReceiptLocation_ID
                               WHEN (bpLoc.IsRemitTo = 'N' AND cd.DocBaseType IN ('API' , 'APC')) THEN  bpLoc.VA009_PaymentLocation_ID 
                          END AS C_BPartner_Location_ID, ");
            sql.Append(@" cs.DueDate , 0 AS VA009_RecivedAmt,  cs.ad_org_id,  cs.AD_Client_ID , 'Invoice' AS va009_transactiontype, 
                          inv.DateAcct,inv.c_conversiontype_id,cs.DiscountAmt,cs.DiscountDate
                        FROM C_InvoicePaySchedule cs 
                        INNER JOIN VA009_PaymentMethod pm ON pm.VA009_PaymentMethod_ID = cs.VA009_PaymentMethod_ID 
                        LEFT JOIN C_invoice inv ON inv.C_Invoice_ID = cs.C_invoice_ID 
                        LEFT JOIN C_BPartner cb ON cb.c_bpartner_id = inv.c_bpartner_id 
                        LEFT JOIN C_BPartner_Location bpLoc ON (bpLoc.C_BPartner_Location_ID = inv.C_BPartner_Location_ID)
                        INNER JOIN C_Currency cc ON inv.C_Currency_ID = cc.C_Currency_ID 
                        INNER JOIN AD_ClientInfo aclnt  ON aclnt.AD_Client_ID = cs.AD_Client_ID 
                        INNER JOIN C_acctschema ac ON ac.C_AcctSchema_ID = aclnt.C_AcctSchema1_ID 
                        LEFT JOIN c_conversion_rate ccr ON ccr.C_Currency_ID = ac.C_Currency_ID
                        INNER JOIN C_Currency cy ON ac.C_Currency_ID=cy.C_Currency_ID 
                        INNER JOIN C_Doctype cd ON cs.C_Doctype_ID = cd.C_Doctype_ID 
                        WHERE cs.AD_Client_ID= " + ctx.GetAD_Client_ID() + " AND cs.C_InvoicePaySchedule_ID IN (" + InvPayids + ")");

            //string finalQuery = MRole.GetDefault(ctx).AddAccessSQL(sql.ToString(), "cs", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            //sql.Clear();

            sql.Append(@" GROUP BY inv.C_DocType_ID, pm.VA009_PaymentMode, pm.VA009_PaymentMethod_ID, cb.c_Bpartner_id,  cb.name, inv.DocumentNo, cs.C_invoice_ID,
                          cs.DueDate,  cs.C_InvoicePaySchedule_ID, CY.StdPrecision,cd.DOCBASETYPE ,  inv.C_Currency_ID,  cs.DueAmt,  cs.ad_org_id,
                          cs.AD_Client_ID, cc.ISO_CODE, inv.DateAcct,inv.c_conversiontype_id,cs.DiscountAmt,cs.DiscountDate");
            sql.Append(" ,bpLoc.IsPayFrom, inv.C_BPartner_Location_ID, bpLoc.IsRemitTo, bpLoc.VA009_ReceiptLocation_ID, bpLoc.VA009_PaymentLocation_ID");

            sql.Append(" UNION ");

            sql.Append(@"SELECT DISTINCT inv.C_DocType_ID, pm.VA009_PaymentMode,  pm.VA009_PaymentMethod_ID,  cb.c_Bpartner_id,  
                                inv.DocumentNo,  cb.name AS C_Bpartner,  cs.C_Order_ID AS C_Invoice_ID,
                                cs.VA009_OrderPaySchedule_ID As C_InvoicePaySchedule_ID,  inv.C_Currency_ID,  cc.ISO_CODE, ");
            sql.Append(@" CASE WHEN (cd.DOCBASETYPE IN ('SOO')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) 
                              WHEN (cd.DOCBASETYPE IN ('POO')) THEN ROUND(cs.DUEAMT, NVL(CY.StdPrecision,2)) * 1   END AS DueAmt, "); // -1 Because during payble dont show negative amount on UI
            /* VIS0045 : Date - 15-Feb-2022*/
            /* This Enhancement will create Batch Line with Business Partner Location*/
            /* System will BP location where "Pay From Address" and "Remit To Address" is True*/
            sql.Append(@" CASE WHEN (bpLoc.IsPayFrom = 'Y' AND cd.DocBaseType IN ('SOO')) THEN  inv.C_BPartner_Location_ID
                               WHEN (bpLoc.IsRemitTo = 'Y' AND cd.DocBaseType IN ('POO')) THEN  inv.C_BPartner_Location_ID
                               WHEN (bpLoc.IsPayFrom = 'N' AND cd.DocBaseType IN ('SOO')) THEN  bpLoc.VA009_ReceiptLocation_ID
                               WHEN (bpLoc.IsRemitTo = 'N' AND cd.DocBaseType IN ('POO')) THEN  bpLoc.VA009_PaymentLocation_ID 
                          END AS C_BPartner_Location_ID, ");
            sql.Append(@" cs.DueDate ,  0 AS VA009_RecivedAmt,  cs.ad_org_id,  cs.AD_Client_ID,  'Order' AS VA009_TransactionType, 
                          inv.DateAcct,inv.c_conversiontype_id,cs.DiscountAmt,cs.DiscountDate
                        FROM VA009_OrderPaySchedule cs 
                        INNER JOIN VA009_PaymentMethod pm ON pm.VA009_PaymentMethod_ID=cs.VA009_PaymentMethod_ID
                        INNER JOIN C_Order inv ON inv.C_Order_ID=cs.C_Order_ID 
                        INNER JOIN C_Doctype cd ON inv.C_Doctype_ID= cd.C_Doctype_ID 
                        INNER JOIN C_BPartner cb ON cb.c_bpartner_id=inv.c_bpartner_id 
                        INNER JOIN C_BPartner_Location bpLoc ON (bpLoc.C_BPartner_Location_ID = inv.C_BPartner_Location_ID)
                        INNER JOIN C_Currency cc ON inv.C_Currency_ID=cc.C_Currency_ID
                        INNER JOIN AD_ClientInfo aclnt ON aclnt.AD_Client_ID =cs.AD_Client_ID 
                        INNER JOIN C_acctschema ac ON ac.C_AcctSchema_ID =aclnt.C_AcctSchema1_ID
                        LEFT JOIN c_conversion_rate ccr ON ccr.C_Currency_ID= ac.C_Currency_ID 
                        INNER JOIN C_Currency cy ON ac.C_Currency_ID=cy.C_Currency_ID
                        WHERE cs.AD_Client_ID= " + ctx.GetAD_Client_ID() + " AND cs.VA009_OrderPaySchedule_ID IN (" + OrderPayids + ")");

            //finalQuery = MRole.GetDefault(ctx).AddAccessSQL(sql.ToString(), "cs", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            //sql.Clear();
            //sql.Append(finalQuery);

            sql.Append(@" GROUP BY inv.C_DocType_ID, pm.VA009_PaymentMode, pm.VA009_PaymentMethod_ID, cb.c_Bpartner_id,  cb.name, inv.DocumentNo, cs.C_Order_ID,
                          cs.DueDate,  cs.VA009_OrderPaySchedule_ID, CY.StdPrecision,cd.DOCBASETYPE ,  inv.C_Currency_ID,  cs.DueAmt,  cs.ad_org_id,
                          cs.AD_Client_ID, cc.ISO_CODE, inv.DateAcct,inv.c_conversiontype_id,cs.DiscountAmt,cs.DiscountDate ");           // Order By Business Partner name not on ID
            sql.Append(@" ,bpLoc.IsPayFrom, inv.C_BPartner_Location_ID, bpLoc.IsRemitTo, bpLoc.VA009_ReceiptLocation_ID,
                           bpLoc.VA009_PaymentLocation_ID ORDER BY C_Bpartner");

            DataSet ds = DB.ExecuteDataset(sql.ToString());
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    PaymentData _payData = new PaymentData();
                    _payData.C_BPartner_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_ID"]);
                    _payData.C_BPartner_Location_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_Location_ID"]);
                    _payData.C_Bpartner = Util.GetValueOfString(ds.Tables[0].Rows[i]["C_Bpartner"]);
                    _payData.C_Invoice_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Invoice_ID"]);
                    _payData.C_InvoicePaySchedule_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_InvoicePaySchedule_ID"]);
                    _payData.CurrencyCode = Util.GetValueOfString(ds.Tables[0].Rows[i]["ISO_CODE"]);
                    _payData.C_Currency_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Currency_ID"]);
                    _payData.DueAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]);
                    _payData.VA009_RecivedAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_RecivedAmt"]);
                    _payData.AD_Org_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["AD_Org_ID"]);
                    _payData.AD_Client_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["AD_Client_ID"]);
                    _payData.recid = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_InvoicePaySchedule_ID"]);
                    _payData.DueDate = Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["DueDate"]);
                    //Get Account Date
                    _payData.DateAcct = Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["DateAcct"]);
                    _payData.VA009_PaymentMode = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA009_PaymentMode"]);
                    _payData.VA009_PaymentMethod_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_PaymentMethod_ID"]);
                    _payData.DocumentNo = Util.GetValueOfString(ds.Tables[0].Rows[i]["DocumentNo"]);
                    //change by amit
                    _payData.TransactionType = Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_transactiontype"]);
                    //int doctypeId = 0;
                    //if (Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_transactiontype"]) == "Invoice")
                    //{
                    //    MInvoice _inv = new MInvoice(ctx, Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Invoice_ID"]), null);
                    //    doctypeId = _inv.GetC_DocType_ID();
                    //}
                    //else
                    //{
                    //    MOrder _order = new MOrder(ctx, Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Invoice_ID"]), null);
                    //    doctypeId = _order.GetC_DocType_ID();
                    //}
                    MDocType docbasdetype = MDocType.Get(ctx, Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_DocType_ID"]));
                    _payData.DocBaseType = docbasdetype.GetDocBaseType();
                    if (docbasdetype.GetDocBaseType() == "API" || docbasdetype.GetDocBaseType() == "APC" || docbasdetype.GetDocBaseType() == "POO")
                    {
                        _payData.PaymwentBaseType = "APP";
                    }
                    else if (docbasdetype.GetDocBaseType() == "ARC" || docbasdetype.GetDocBaseType() == "ARI" || docbasdetype.GetDocBaseType() == "SOO")
                    {
                        _payData.PaymwentBaseType = "ARR";
                    }
                    //change by amit
                    if (docbasdetype.GetDocBaseType() == "ARC" || docbasdetype.GetDocBaseType() == "APC")
                    {
                        if (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]) < 0)
                        {
                            _payData.convertedAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]);
                        }
                        else
                        {
                            _payData.convertedAmt = 1 * Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]); // -1 Because during payble dont show negative amount on UI
                        }

                        if (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DiscountAmt"]) < 0)
                        {
                            _payData.DiscountAmount = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DiscountAmt"]);
                        }
                        else
                        {
                            _payData.DiscountAmount = 1 * Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DiscountAmt"]);
                        }
                    }
                    else
                    {
                        _payData.convertedAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]);
                        _payData.DiscountAmount = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DiscountAmt"]);
                    }
                    //Rakesh(VA228):Set invoice/order conversion type/discount amount on date 17/Sep/2021
                    _payData.ConversionTypeId = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_ConversionType_ID"]);
                    _payData.DiscountDate = Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["DiscountDate"]);
                    //end
                    _lstChqPay.Add(_payData);
                }
            }
            sql = null;
            return _lstChqPay;
        }

        /// <summary>
        /// Create Payment Records
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="PaymentData">List of invoice/Order schedules data</param>
        /// <returns>String, Message</returns>
        public string CreatePayments(Ctx ct, GeneratePaymt[] PaymentData)
        {
            Trx trx = Trx.GetTrx("Payment_" + DateTime.Now.ToString("yyMMddHHmmssff"));
            StringBuilder ex = new StringBuilder();
            string msg = Msg.GetMsg(ct, "VA009_PaymentCompletedWith");
            StringBuilder docno = new StringBuilder();
            string docno1;
            string processMsg = "";
            string[] _result = null;//to get result from CompleteOrReverse function and store into this variable to execute the condition based on this result
            try
            {
                if (PaymentData.Length > 0)
                {
                    List<int> PartnerID = new List<int>();
                    List<string> Checkno = new List<string>();
                    int payid = 0; List<int> count = new List<int>();
                    bool Found = false, Allocate = false; MPayment _pay = null;
                    //Rakesh(VA228):Set Document Type id (13/Sep/2021)
                    int C_doctype_ID = PaymentData[0].TargetDocType;
                    int currencyTypeID = PaymentData[0].CurrencyType;
                    if (PaymentData.Length > 0)
                    {
                        #region If Single Schedule
                        if (PaymentData.Length == 1)
                        {
                            MInvoicePaySchedule _payschedule = null;
                            MDocType _doctype = null;
                            MInvoice _invoice = null;
                            ViennaAdvantage.Model.MVA009OrderPaySchedule orderPaySchedule = null;
                            if (PaymentData[0].TransactionType == "Invoice")
                            {
                                _payschedule = new MInvoicePaySchedule(ct, PaymentData[0].C_InvoicePaySchedule_ID, trx);
                                //_doctype = new MDocType(ct, _payschedule.GetC_DocType_ID(), trx);
                                _doctype = MDocType.Get(ct, _payschedule.GetC_DocType_ID());
                                _invoice = new MInvoice(ct, _payschedule.GetC_Invoice_ID(), trx);
                                _pay = new MPayment(ct, 0, trx);
                                _pay.SetAD_Client_ID(Util.GetValueOfInt(PaymentData[0].AD_Client_ID));
                                _pay.SetAD_Org_ID(Util.GetValueOfInt(PaymentData[0].AD_Org_ID));
                                _pay.SetC_DocType_ID(C_doctype_ID);
                                _pay.SetDateAcct(PaymentData[0].DateAcct);
                                // to set Transaction date 
                                _pay.SetDateTrx(PaymentData[0].DateTrx);
                                //_pay.SetDateTrx(System.DateTime.Now);
                                _pay.SetDescription(Util.GetValueOfString(PaymentData[0].Description));
                                _pay.SetC_BankAccount_ID(Util.GetValueOfInt(PaymentData[0].C_BankAccount_ID));
                                _pay.SetC_ConversionType_ID(PaymentData[0].CurrencyType);
                                _pay.SetC_BPartner_ID(Util.GetValueOfInt(PaymentData[0].C_BPartner_ID));
                                _pay.SetC_BPartner_Location_ID(_invoice.GetC_BPartner_Location_ID());
                                if (PaymentData[0].PaymwentBaseType == "APP")
                                {
                                    if (PaymentData[0].VA009_RecivedAmt < 0)
                                        _pay.SetPayAmt(-1 * (PaymentData[0].VA009_RecivedAmt));
                                    else
                                        _pay.SetPayAmt(PaymentData[0].VA009_RecivedAmt);

                                    if (_doctype.GetDocBaseType() == "APC")
                                    {
                                        if (PaymentData[0].VA009_RecivedAmt > 0)
                                            _pay.SetPayAmt(-1 * (PaymentData[0].VA009_RecivedAmt));
                                        else
                                            _pay.SetPayAmt(PaymentData[0].VA009_RecivedAmt);
                                    }

                                    if (PaymentData[0].Discount < 0)
                                        _pay.SetDiscountAmt(-1 * (PaymentData[0].Discount));
                                    else
                                        _pay.SetDiscountAmt(PaymentData[0].Discount);

                                    if (PaymentData[0].Writeoff < 0)
                                        _pay.SetWriteOffAmt(-1 * (PaymentData[0].Writeoff));
                                    else
                                        _pay.SetWriteOffAmt(PaymentData[0].Writeoff);

                                    if (_doctype.GetDocBaseType() == "API")
                                    {
                                        if (PaymentData[0].OverUnder < 0)
                                            _pay.SetOverUnderAmt(-1 * (PaymentData[0].OverUnder));
                                        else
                                            _pay.SetOverUnderAmt((PaymentData[0].OverUnder));
                                    }

                                    else if (_doctype.GetDocBaseType() == "ARC")
                                    {
                                        if (PaymentData[0].OverUnder < 0)
                                            _pay.SetOverUnderAmt(-1 * (PaymentData[0].OverUnder));
                                        else
                                            _pay.SetOverUnderAmt((PaymentData[0].OverUnder));
                                    }
                                    else if (_doctype.GetDocBaseType() == "APC")
                                    {
                                        if (PaymentData[0].OverUnder > 0)
                                            _pay.SetOverUnderAmt(-1 * (PaymentData[0].OverUnder));
                                        else
                                            _pay.SetOverUnderAmt((PaymentData[0].OverUnder));

                                        if (PaymentData[0].Discount > 0)
                                            _pay.SetDiscountAmt(-1 * (PaymentData[0].Discount));
                                        else
                                            _pay.SetDiscountAmt(PaymentData[0].Discount);

                                        if (PaymentData[0].Writeoff > 0)
                                            _pay.SetWriteOffAmt(-1 * (PaymentData[0].Writeoff));
                                        else
                                            _pay.SetWriteOffAmt((PaymentData[0].Writeoff));
                                    }
                                }
                                else
                                {
                                    if (_doctype.GetDocBaseType() == "ARC")
                                    {
                                        if (PaymentData[0].VA009_RecivedAmt > 0)
                                            _pay.SetPayAmt(-1 * (PaymentData[0].VA009_RecivedAmt));

                                        if (PaymentData[0].OverUnder > 0)
                                            _pay.SetOverUnderAmt(-1 * (PaymentData[0].OverUnder));
                                        else
                                            _pay.SetOverUnderAmt(PaymentData[0].OverUnder);

                                        if (PaymentData[0].Discount > 0)
                                            _pay.SetDiscountAmt(Decimal.Negate(PaymentData[0].Discount));
                                        else
                                            _pay.SetDiscountAmt(PaymentData[0].Discount);

                                        if (PaymentData[0].Writeoff > 0)
                                            _pay.SetWriteOffAmt(Decimal.Negate(PaymentData[0].Writeoff));
                                        else
                                            _pay.SetWriteOffAmt(PaymentData[0].Writeoff);
                                    }
                                    else
                                    {
                                        _pay.SetPayAmt(PaymentData[0].VA009_RecivedAmt);
                                        _pay.SetOverUnderAmt(PaymentData[0].OverUnder);
                                        _pay.SetDiscountAmt(PaymentData[0].Discount);
                                        _pay.SetWriteOffAmt(PaymentData[0].Writeoff);
                                    }
                                }
                                //change by amit

                                //end
                                _pay.SetC_Currency_ID(GetPaymentCurrency(ct, Util.GetValueOfInt(PaymentData[0].C_BankAccount_ID)));

                                int bankcurrency = GetPaymentCurrency(ct, Util.GetValueOfInt(PaymentData[0].C_BankAccount_ID));
                                if (Util.GetValueOfInt(PaymentData[0].C_Currency_ID) != bankcurrency)
                                {
                                }
                                else
                                {
                                }
                                _pay.SetC_ConversionType_ID(PaymentData[0].CurrencyType);

                                if (Util.GetValueOfInt(PaymentData[0].VA009_PaymentMethod_ID) > 0)
                                {
                                    _pay.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(PaymentData[0].VA009_PaymentMethod_ID));
                                }
                                else
                                {
                                    _pay.SetVA009_PaymentMethod_ID(_payschedule.GetVA009_PaymentMethod_ID());
                                }
                                if (PaymentData[0].CheckDate != null)
                                {
                                    _pay.SetCheckDate(PaymentData[0].CheckDate);
                                }
                                _pay.SetCheckNo(PaymentData[0].CheckNumber);
                                if ((PaymentData[0].VA009_RecivedAmt > 0 && PaymentData[0].OverUnder < 0) || (PaymentData[0].VA009_RecivedAmt < 0 && PaymentData[0].OverUnder > 0))
                                {
                                    _pay.SetC_Invoice_ID(Util.GetValueOfInt(PaymentData[0].C_Invoice_ID));
                                    _pay.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(PaymentData[0].C_InvoicePaySchedule_ID));
                                }
                                else
                                {
                                    _pay.SetC_Invoice_ID(Util.GetValueOfInt(PaymentData[0].C_Invoice_ID));
                                    _pay.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(PaymentData[0].C_InvoicePaySchedule_ID));
                                }
                                if (!_pay.Save())
                                {
                                    ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    if (pp != null)
                                    {
                                        ex.Append(", " + pp.GetName());
                                    }
                                    //ex.Append(": " + PaymentData[0].CheckNumber);
                                    _log.Info(ex.ToString());
                                }
                                else
                                {
                                    //based on result get from Complete function should execute the condition
                                    _result = CompleteOrReverse(ct, _pay.GetC_Payment_ID(), _pay.Get_Table_ID(), _pay.Get_TableName().ToLower(), DocActionVariables.ACTION_COMPLETE, trx);
                                    //if (_pay.CompleteIt() == "CO")
                                    //{
                                    //    _pay.SetProcessed(true);
                                    //    _pay.SetDocAction("CL");
                                    //    _pay.SetDocStatus("CO");
                                    //    _pay.Save();
                                    //    docno.Append(_pay.GetDocumentNo());
                                    //}
                                    //if (_pay.Save())
                                    //'Y' Indicates the record is Completed Successfully
                                    if (_result != null && _result[1].Equals("Y"))
                                    {
                                        docno.Append(_pay.GetDocumentNo());
                                    }
                                    else
                                    {
                                        //trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotCompelted") + ": " + _pay.GetDocumentNo());
                                        if (_pay.GetProcessMsg() != null && _pay.GetProcessMsg().IndexOf("@") != -1)
                                        {
                                            processMsg = Msg.ParseTranslation(ct, _pay.GetProcessMsg());
                                        }
                                        else
                                        {
                                            processMsg = Msg.GetMsg(ct, _pay.GetProcessMsg());
                                        }
                                        ex.Append(", " + processMsg);
                                        _log.Info(ex.ToString());
                                    }
                                }
                            }
                            // added by amit - 21-10-2016
                            else if (PaymentData[0].TransactionType == "Order")
                            {
                                orderPaySchedule = new MVA009OrderPaySchedule(ct, PaymentData[0].C_InvoicePaySchedule_ID, trx);
                                MOrder order = new MOrder(ct, orderPaySchedule.GetC_Order_ID(), trx);
                                // _doctype = new MDocType(ct, order.GetC_DocType_ID(), trx);
                                _doctype = MDocType.Get(ct, order.GetC_DocType_ID());
                                ex.Append(GeneratePaymentAgainstSOSchedule(ct, PaymentData[0], C_doctype_ID, ex.ToString(), out docno1, trx));
                                if (docno1 != "")
                                {
                                    if (docno.Length > 0)
                                    {
                                        docno.Append(", ");
                                    }
                                    docno.Append(docno1);
                                }
                            }
                            //end
                        }
                        #endregion

                        #region Multiple Schedule
                        else if (PaymentData.Length > 1)
                        {
                            for (int i = 0; i < PaymentData.Length; i++)
                            {
                                MInvoicePaySchedule _payschedule = null;
                                MDocType _doctype = null;
                                MInvoice _invoice = null;
                                ViennaAdvantage.Model.MVA009OrderPaySchedule orderPaySchedule = null;
                                if (PaymentData[i].TransactionType == "Invoice")
                                {
                                    _payschedule = new MInvoicePaySchedule(ct, PaymentData[i].C_InvoicePaySchedule_ID, trx);
                                    //_doctype = new MDocType(ct, _payschedule.GetC_DocType_ID(), trx);
                                    _doctype = MDocType.Get(ct, _payschedule.GetC_DocType_ID());
                                    _invoice = new MInvoice(ct, _payschedule.GetC_Invoice_ID(), trx);

                                    if ((PartnerID.Contains(PaymentData[i].C_BPartner_ID)) && (Checkno.Contains(PaymentData[i].CheckNumber)))
                                    {
                                        int indx = Checkno.IndexOf(PaymentData[i].CheckNumber);
                                        _pay = new MPayment(ct, count[indx], trx);
                                        _pay.SetC_InvoicePaySchedule_ID(0);
                                        _pay.SetC_ConversionType_ID(currencyTypeID);
                                        _pay.SetC_ConversionType_ID(PaymentData[i].CurrencyType);
                                        if (!_pay.Save())
                                        {
                                            ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved") + ": " + PaymentData[i].C_BPartner_ID);
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                            {
                                                ex.Append(", " + pp.GetName() + ": " + PaymentData[i].CheckNumber);
                                            }
                                            _log.Info(ex.ToString());
                                        }
                                        else
                                        {
                                            MPaymentAllocate M_Allocate = new MPaymentAllocate(ct, 0, trx);
                                            M_Allocate.SetAD_Org_ID(PaymentData[i].AD_Org_ID);
                                            M_Allocate.SetAD_Client_ID(PaymentData[i].AD_Client_ID);
                                            M_Allocate.SetC_Payment_ID(_pay.GetC_Payment_ID());
                                            M_Allocate.SetC_Invoice_ID(PaymentData[i].C_Invoice_ID);
                                            M_Allocate.SetC_InvoicePaySchedule_ID(PaymentData[i].C_InvoicePaySchedule_ID);
                                            if (PaymentData[i].PaymwentBaseType == "APP")
                                            {
                                                if (PaymentData[i].VA009_RecivedAmt < 0)
                                                    M_Allocate.SetAmount(-1 * (PaymentData[i].VA009_RecivedAmt));
                                                else
                                                    M_Allocate.SetAmount(PaymentData[i].VA009_RecivedAmt);

                                                if (PaymentData[i].Discount < 0)
                                                    M_Allocate.SetDiscountAmt(-1 * (PaymentData[i].Discount));
                                                else
                                                    M_Allocate.SetDiscountAmt(PaymentData[i].Discount);

                                                if (PaymentData[i].Writeoff < 0)
                                                    M_Allocate.SetWriteOffAmt(-1 * (PaymentData[i].Writeoff));
                                                else
                                                    M_Allocate.SetWriteOffAmt(PaymentData[i].Writeoff);

                                                if (PaymentData[i].OverUnder < 0)
                                                    M_Allocate.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                                else
                                                    M_Allocate.SetOverUnderAmt((PaymentData[i].OverUnder));

                                                if (_doctype.GetDocBaseType() == "APC")
                                                {
                                                    // if (PaymentData[i].OverUnder < 0) commented by manjot suggested by puneet and ashish this works same as on window 16/4/19
                                                    M_Allocate.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                                    M_Allocate.SetAmount(-1 * (PaymentData[i].VA009_RecivedAmt));
                                                    M_Allocate.SetDiscountAmt(-1 * (PaymentData[i].Discount));
                                                    M_Allocate.SetWriteOffAmt(-1 * (PaymentData[i].Writeoff));
                                                }

                                                // set invoice amount
                                                M_Allocate.SetInvoiceAmt(M_Allocate.GetAmount() + M_Allocate.GetDiscountAmt() +
                                                                         M_Allocate.GetWriteOffAmt() + M_Allocate.GetOverUnderAmt());
                                            }
                                            else
                                            {

                                                M_Allocate.SetAmount(PaymentData[i].VA009_RecivedAmt);
                                                M_Allocate.SetDiscountAmt(PaymentData[i].Discount);
                                                M_Allocate.SetOverUnderAmt(PaymentData[i].OverUnder);
                                                M_Allocate.SetWriteOffAmt(PaymentData[i].Writeoff);
                                                if (_doctype.GetDocBaseType() == "ARC")
                                                {
                                                    //if (PaymentData[i].OverUnder < 0) commented by manjot suggested by puneet and ashish this works same as on window 16/4/19
                                                    M_Allocate.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                                    M_Allocate.SetAmount(-1 * (PaymentData[i].VA009_RecivedAmt));
                                                    M_Allocate.SetDiscountAmt(-1 * (PaymentData[i].Discount));
                                                    M_Allocate.SetWriteOffAmt(-1 * (PaymentData[i].Writeoff));
                                                }

                                                // set invoice amount
                                                M_Allocate.SetInvoiceAmt(M_Allocate.GetAmount() + M_Allocate.GetDiscountAmt() +
                                                                         M_Allocate.GetWriteOffAmt() + M_Allocate.GetOverUnderAmt());
                                            }
                                            if (!M_Allocate.Save())
                                            {
                                                ex.Append(Msg.GetMsg(ct, "VA009_PALNotSaved"));
                                                ValueNamePair pp = VLogger.RetrieveError();
                                                if (pp != null)
                                                {
                                                    ex.Append(", " + pp.GetName());
                                                }
                                                _log.Info(ex.ToString());
                                            }
                                        }
                                    }
                                    else
                                    { // to check weather the check number exist on next records or Not... if check number found than FOUND will be TRUE
                                        for (int j = 0; j < PaymentData.Length; j++)
                                        {
                                            if (j == i) { continue; }
                                            else
                                            {
                                                if ((PaymentData[j].C_BPartner_ID == PaymentData[i].C_BPartner_ID) && (PaymentData[j].CheckNumber == PaymentData[i].CheckNumber))
                                                {
                                                    Found = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (Found == true)
                                        {
                                            _pay = new MPayment(ct, 0, trx);
                                            _pay.SetAD_Client_ID(Util.GetValueOfInt(PaymentData[i].AD_Client_ID));
                                            _pay.SetAD_Org_ID(Util.GetValueOfInt(PaymentData[i].AD_Org_ID));
                                            _pay.SetC_DocType_ID(C_doctype_ID);
                                            _pay.SetDateAcct(PaymentData[i].DateAcct);
                                            // to set Transaction date 
                                            _pay.SetDateTrx(PaymentData[i].DateTrx);
                                            _pay.SetDescription(Util.GetValueOfString(PaymentData[i].Description));
                                            _pay.SetC_ConversionType_ID(PaymentData[i].CurrencyType);
                                            //_pay.SetDateTrx(System.DateTime.Now.ToLocalTime());
                                            _pay.SetC_ConversionType_ID(currencyTypeID);
                                            _pay.SetC_BankAccount_ID(Util.GetValueOfInt(PaymentData[i].C_BankAccount_ID));
                                            _pay.SetC_BPartner_ID(Util.GetValueOfInt(PaymentData[i].C_BPartner_ID));
                                            _pay.SetC_BPartner_Location_ID(_invoice.GetC_BPartner_Location_ID());
                                            //change by amit
                                            _pay.SetC_Currency_ID(GetPaymentCurrency(ct, Util.GetValueOfInt(PaymentData[i].C_BankAccount_ID)));

                                            if (Util.GetValueOfInt(PaymentData[i].VA009_PaymentMethod_ID) > 0)
                                            {
                                                _pay.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(PaymentData[i].VA009_PaymentMethod_ID));
                                            }
                                            else
                                            {
                                                _pay.SetVA009_PaymentMethod_ID(_payschedule.GetVA009_PaymentMethod_ID());
                                            }
                                            //end
                                            if (PaymentData[i].PaymwentBaseType == "APP")
                                            {
                                                if (PaymentData[i].VA009_RecivedAmt < 0)
                                                    _pay.SetPayAmt(-1 * (PaymentData[i].VA009_RecivedAmt));
                                                else
                                                    _pay.SetPayAmt(PaymentData[i].VA009_RecivedAmt);

                                                if (_doctype.GetDocBaseType() == "APC")
                                                {
                                                    if (PaymentData[i].VA009_RecivedAmt > 0)
                                                        _pay.SetPayAmt(-1 * (PaymentData[i].VA009_RecivedAmt));// -1 Received amount can't be nagative
                                                    else
                                                        _pay.SetPayAmt(PaymentData[i].VA009_RecivedAmt);
                                                }

                                                if (PaymentData[i].Discount < 0)
                                                    _pay.SetDiscountAmt(-1 * (PaymentData[i].Discount));
                                                else
                                                    _pay.SetDiscountAmt(PaymentData[i].Discount);

                                                if (PaymentData[i].Writeoff < 0)
                                                    _pay.SetWriteOffAmt(-1 * (PaymentData[i].Writeoff));
                                                else
                                                    _pay.SetWriteOffAmt(PaymentData[i].Writeoff);

                                                if (_doctype.GetDocBaseType() == "API")
                                                {
                                                    if (PaymentData[i].OverUnder < 0)
                                                        _pay.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                                    else
                                                        _pay.SetOverUnderAmt((PaymentData[i].OverUnder));
                                                }
                                                else if (_doctype.GetDocBaseType() == "APC")
                                                {
                                                    if (PaymentData[i].OverUnder > 0)
                                                        _pay.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                                    else
                                                        _pay.SetOverUnderAmt((PaymentData[i].OverUnder));

                                                    if (PaymentData[i].Discount > 0)
                                                        _pay.SetDiscountAmt(-1 * (PaymentData[i].Discount));
                                                    else
                                                        _pay.SetDiscountAmt(PaymentData[i].Discount);

                                                    if (PaymentData[i].Writeoff > 0)
                                                        _pay.SetWriteOffAmt(-1 * (PaymentData[i].Writeoff));
                                                    else
                                                        _pay.SetWriteOffAmt(PaymentData[i].Writeoff);
                                                }
                                                else
                                                {
                                                    _pay.SetOverUnderAmt(PaymentData[i].OverUnder);
                                                }
                                            }
                                            else
                                            {
                                                if (_doctype.GetDocBaseType() == "ARC")
                                                {
                                                    // payment amount
                                                    if (PaymentData[i].VA009_RecivedAmt > 0)
                                                        _pay.SetPayAmt(-1 * (PaymentData[i].VA009_RecivedAmt));
                                                    else
                                                        _pay.SetPayAmt(PaymentData[i].VA009_RecivedAmt);
                                                    //under amount
                                                    if (PaymentData[i].OverUnder > 0)
                                                        _pay.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                                    else
                                                        _pay.SetOverUnderAmt(PaymentData[i].OverUnder);
                                                    // discount amount
                                                    if (PaymentData[0].Discount > 0)
                                                        _pay.SetDiscountAmt(Decimal.Negate(PaymentData[0].Discount));
                                                    else
                                                        _pay.SetDiscountAmt(PaymentData[0].Discount);
                                                    // write-off amount
                                                    if (PaymentData[0].Writeoff > 0)
                                                        _pay.SetWriteOffAmt(Decimal.Negate(PaymentData[0].Writeoff));
                                                    else
                                                        _pay.SetWriteOffAmt(PaymentData[0].Writeoff);
                                                }
                                                else
                                                {
                                                    _pay.SetPayAmt(PaymentData[i].VA009_RecivedAmt);
                                                    _pay.SetOverUnderAmt(PaymentData[i].OverUnder);
                                                    _pay.SetDiscountAmt(PaymentData[i].Discount);
                                                    _pay.SetWriteOffAmt(PaymentData[i].Writeoff);
                                                }
                                            }
                                            if (PaymentData[i].CheckDate != null)
                                            {
                                                _pay.SetCheckDate(PaymentData[i].CheckDate);
                                            }
                                            _pay.SetCheckNo(PaymentData[i].CheckNumber);
                                            if (!_pay.Save())
                                            {
                                                ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved") + ": " + PaymentData[i].C_BPartner_ID);
                                                ValueNamePair pp = VLogger.RetrieveError();
                                                if (pp != null)
                                                {
                                                    ex.Append(", " + pp.GetName() + ": " + PaymentData[i].CheckNumber);
                                                }
                                                _log.Info(ex.ToString());
                                            }
                                            else
                                            {
                                                Allocate = true;
                                                payid = _pay.GetC_Payment_ID();
                                                count.Add(payid);
                                                PartnerID.Add(Util.GetValueOfInt(PaymentData[i].C_BPartner_ID));
                                                Checkno.Add(PaymentData[i].CheckNumber);
                                            }
                                            Found = false;
                                        }
                                        else
                                        {
                                            _pay = new MPayment(ct, 0, trx);
                                            _pay.SetAD_Client_ID(Util.GetValueOfInt(PaymentData[i].AD_Client_ID));
                                            _pay.SetAD_Org_ID(Util.GetValueOfInt(PaymentData[i].AD_Org_ID));
                                            _pay.SetC_DocType_ID(C_doctype_ID);
                                            _pay.SetDateAcct(PaymentData[i].DateAcct);
                                            // to set Transaction date 
                                            _pay.SetDateTrx(PaymentData[i].DateTrx);
                                            //_pay.SetDateTrx(System.DateTime.Now.ToLocalTime());
                                            _pay.SetDescription(Util.GetValueOfString(PaymentData[i].Description));
                                            _pay.SetC_ConversionType_ID(currencyTypeID);
                                            _pay.SetC_ConversionType_ID(PaymentData[i].CurrencyType);
                                            _pay.SetC_BankAccount_ID(Util.GetValueOfInt(PaymentData[i].C_BankAccount_ID));
                                            _pay.SetC_BPartner_ID(Util.GetValueOfInt(PaymentData[i].C_BPartner_ID));
                                            _pay.SetC_BPartner_Location_ID(_invoice.GetC_BPartner_Location_ID());
                                            if (PaymentData[i].PaymwentBaseType == "APP")
                                            {
                                                if (PaymentData[i].VA009_RecivedAmt < 0)
                                                    _pay.SetPayAmt(-1 * (PaymentData[i].VA009_RecivedAmt));
                                                else
                                                    _pay.SetPayAmt(PaymentData[i].VA009_RecivedAmt);
                                                if (_doctype.GetDocBaseType() == "APC")
                                                {
                                                    if (PaymentData[i].VA009_RecivedAmt > 0)
                                                        _pay.SetPayAmt(-1 * (PaymentData[i].VA009_RecivedAmt));// -1 Received amount can't be nagative
                                                    else
                                                        _pay.SetPayAmt(PaymentData[i].VA009_RecivedAmt);
                                                }

                                                if (PaymentData[i].Discount < 0)
                                                    _pay.SetDiscountAmt(-1 * (PaymentData[i].Discount));
                                                else
                                                    _pay.SetDiscountAmt(PaymentData[i].Discount);

                                                if (PaymentData[i].Writeoff < 0)
                                                    _pay.SetWriteOffAmt(-1 * (PaymentData[i].Writeoff));
                                                else
                                                    _pay.SetWriteOffAmt(PaymentData[i].Writeoff);

                                                if (_doctype.GetDocBaseType() == "API")
                                                {
                                                    if (PaymentData[i].OverUnder < 0)
                                                        _pay.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                                    else
                                                        _pay.SetOverUnderAmt((PaymentData[i].OverUnder));
                                                }
                                                else if (_doctype.GetDocBaseType() == "APC")
                                                {
                                                    if (PaymentData[i].OverUnder > 0)
                                                        _pay.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                                    else
                                                        _pay.SetOverUnderAmt((PaymentData[i].OverUnder));

                                                    if (PaymentData[i].Discount > 0)
                                                        _pay.SetDiscountAmt(-1 * (PaymentData[i].Discount));
                                                    else
                                                        _pay.SetDiscountAmt(PaymentData[i].Discount);

                                                    if (PaymentData[i].Writeoff > 0)
                                                        _pay.SetWriteOffAmt(-1 * (PaymentData[i].Writeoff));
                                                    else
                                                        _pay.SetWriteOffAmt(PaymentData[i].Writeoff);
                                                }
                                            }
                                            else
                                            {
                                                if (_doctype.GetDocBaseType() == "ARC")
                                                {
                                                    if (PaymentData[i].VA009_RecivedAmt > 0)
                                                        _pay.SetPayAmt(-1 * (PaymentData[i].VA009_RecivedAmt));
                                                    else
                                                        _pay.SetPayAmt(PaymentData[i].VA009_RecivedAmt);

                                                    if (PaymentData[i].OverUnder > 0)
                                                        _pay.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                                    else
                                                        _pay.SetOverUnderAmt(PaymentData[i].OverUnder);

                                                    // discount amount
                                                    if (PaymentData[0].Discount > 0)
                                                        _pay.SetDiscountAmt(Decimal.Negate(PaymentData[0].Discount));
                                                    else
                                                        _pay.SetDiscountAmt(PaymentData[0].Discount);

                                                    // write-off amount
                                                    if (PaymentData[0].Writeoff > 0)
                                                        _pay.SetWriteOffAmt(Decimal.Negate(PaymentData[0].Writeoff));
                                                    else
                                                        _pay.SetWriteOffAmt(PaymentData[0].Writeoff);
                                                }
                                                else
                                                {
                                                    _pay.SetPayAmt(PaymentData[i].VA009_RecivedAmt);
                                                    _pay.SetDiscountAmt(PaymentData[i].Discount);
                                                    _pay.SetOverUnderAmt(PaymentData[i].OverUnder);
                                                    _pay.SetWriteOffAmt(PaymentData[i].Writeoff);
                                                }
                                            }
                                            //change by amit
                                            _pay.SetC_Currency_ID(GetPaymentCurrency(ct, Util.GetValueOfInt(PaymentData[i].C_BankAccount_ID)));

                                            if (Util.GetValueOfInt(PaymentData[i].VA009_PaymentMethod_ID) > 0)
                                            {
                                                _pay.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(PaymentData[i].VA009_PaymentMethod_ID));
                                            }
                                            else
                                            {
                                                _pay.SetVA009_PaymentMethod_ID(_payschedule.GetVA009_PaymentMethod_ID());
                                            }
                                            //end
                                            if (PaymentData[i].CheckDate != null)
                                            {
                                                _pay.SetCheckDate(PaymentData[i].CheckDate);
                                            }
                                            _pay.SetCheckNo(PaymentData[i].CheckNumber);
                                            if ((PaymentData[i].VA009_RecivedAmt > 0 && PaymentData[i].OverUnder < 0) || (PaymentData[i].VA009_RecivedAmt < 0 && PaymentData[i].OverUnder > 0))
                                            {
                                                _pay.SetC_Invoice_ID(Util.GetValueOfInt(PaymentData[i].C_Invoice_ID));
                                                _pay.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(PaymentData[i].C_InvoicePaySchedule_ID));
                                            }
                                            else
                                            {
                                                _pay.SetC_Invoice_ID(Util.GetValueOfInt(PaymentData[i].C_Invoice_ID));
                                                _pay.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(PaymentData[i].C_InvoicePaySchedule_ID));
                                            }
                                            if (!_pay.Save())
                                            {
                                                ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved") + ": " + PaymentData[i].C_BPartner_ID);
                                                ValueNamePair pp = VLogger.RetrieveError();
                                                if (pp != null)
                                                {
                                                    ex.Append(", " + pp.GetName() + ": " + PaymentData[i].CheckNumber);
                                                }
                                                _log.Info(ex.ToString());
                                            }
                                            else
                                            {
                                                //based on result get from Complete function should execute the condition
                                                _result = CompleteOrReverse(ct, _pay.GetC_Payment_ID(), _pay.Get_Table_ID(), _pay.Get_TableName().ToLower(), DocActionVariables.ACTION_COMPLETE, trx);
                                                //if (_pay.CompleteIt() == "CO")
                                                //'Y' indicates  the record is Completed Successfully
                                                if (_result != null && _result[1].Equals("Y"))
                                                {
                                                    //_pay.SetProcessed(true);
                                                    //_pay.SetDocAction("CL");
                                                    //_pay.SetDocStatus("CO");
                                                    //_pay.Save();

                                                    if (docno.Length > 0)
                                                    {
                                                        docno.Append(", ");
                                                    }
                                                    docno.Append(_pay.GetDocumentNo());
                                                }
                                                else
                                                {
                                                    ex.Append(Msg.GetMsg(ct, "VA009_PNotCompelted") + ": " + _pay.GetDocumentNo());
                                                    if (_pay.GetProcessMsg() != null && _pay.GetProcessMsg().IndexOf("@") != -1)
                                                    {
                                                        processMsg = Msg.ParseTranslation(ct, _pay.GetProcessMsg());
                                                    }
                                                    else
                                                    {
                                                        processMsg = Msg.GetMsg(ct, _pay.GetProcessMsg());
                                                    }
                                                    ex.Append(", " + processMsg);
                                                    _log.Info(ex.ToString());
                                                }
                                            }
                                        }
                                        if (Allocate == true)
                                        {
                                            MPaymentAllocate M_Allocate = new MPaymentAllocate(ct, 0, trx);
                                            M_Allocate.SetAD_Org_ID(PaymentData[i].AD_Org_ID);
                                            M_Allocate.SetAD_Client_ID(PaymentData[i].AD_Client_ID);
                                            M_Allocate.SetC_Payment_ID(payid);
                                            if (PaymentData[i].TransactionType == "Invoice")
                                            {
                                                M_Allocate.SetC_Invoice_ID(PaymentData[i].C_Invoice_ID);
                                                M_Allocate.SetC_InvoicePaySchedule_ID(PaymentData[i].C_InvoicePaySchedule_ID);
                                            }

                                            if (PaymentData[i].PaymwentBaseType == "APP")
                                            {
                                                if (PaymentData[i].VA009_RecivedAmt < 0)
                                                    M_Allocate.SetAmount(-1 * (PaymentData[i].VA009_RecivedAmt));
                                                else
                                                    M_Allocate.SetAmount(PaymentData[i].VA009_RecivedAmt);

                                                if (PaymentData[i].Discount < 0)
                                                    M_Allocate.SetDiscountAmt(-1 * (PaymentData[i].Discount));
                                                else
                                                    M_Allocate.SetDiscountAmt(PaymentData[i].Discount);

                                                if (PaymentData[i].Writeoff < 0)
                                                    M_Allocate.SetWriteOffAmt(-1 * (PaymentData[i].Writeoff));
                                                else
                                                    M_Allocate.SetWriteOffAmt(PaymentData[i].Writeoff);

                                                if (_doctype.GetDocBaseType() == "API")
                                                {
                                                    if (PaymentData[i].OverUnder < 0)
                                                        M_Allocate.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                                    else
                                                        M_Allocate.SetOverUnderAmt(PaymentData[i].OverUnder);
                                                }
                                                else if (_doctype.GetDocBaseType() == "APC")
                                                {
                                                   
                                                    if (PaymentData[i].OverUnder > 0)
                                                        _pay.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                                    if (M_Allocate.GetAmount() > 0)
                                                        M_Allocate.SetAmount(-1 * (PaymentData[i].VA009_RecivedAmt));
                                                    if (M_Allocate.GetDiscountAmt() > 0)
                                                        M_Allocate.SetDiscountAmt(-1 * (PaymentData[i].Discount));
                                                    if (M_Allocate.GetWriteOffAmt() > 0)
                                                        M_Allocate.SetWriteOffAmt(-1 * (PaymentData[i].Writeoff));
                                                }

                                                // set invoice amount
                                                M_Allocate.SetInvoiceAmt(M_Allocate.GetAmount() + M_Allocate.GetDiscountAmt() +
                                                                        M_Allocate.GetWriteOffAmt() + M_Allocate.GetOverUnderAmt());
                                            }
                                            else
                                            {
                                                M_Allocate.SetAmount(PaymentData[i].VA009_RecivedAmt);
                                                M_Allocate.SetDiscountAmt(PaymentData[i].Discount);
                                                M_Allocate.SetOverUnderAmt(PaymentData[i].OverUnder);
                                                M_Allocate.SetWriteOffAmt(PaymentData[i].Writeoff);
                                                if (_doctype.GetDocBaseType() == "ARC")
                                                {
                                                    //if (PaymentData[i].OverUnder < 0) commented by manjot suggested by puneet and ashish this works same as on window 16/4/19
                                                    M_Allocate.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                                    M_Allocate.SetAmount(-1 * (PaymentData[i].VA009_RecivedAmt));
                                                    M_Allocate.SetDiscountAmt(-1 * (PaymentData[i].Discount));
                                                    M_Allocate.SetWriteOffAmt(-1 * (PaymentData[i].Writeoff));
                                                }
                                                // set invoice amount
                                                M_Allocate.SetInvoiceAmt(M_Allocate.GetAmount() + M_Allocate.GetDiscountAmt() +
                                                                        M_Allocate.GetWriteOffAmt() + M_Allocate.GetOverUnderAmt());
                                            }
                                            if (!M_Allocate.Save())
                                            {
                                                ex.Append(Msg.GetMsg(ct, "VA009_PALNotSaved"));
                                                ValueNamePair pp = VLogger.RetrieveError();
                                                if (pp != null)
                                                {
                                                    ex.Append(", " + pp.GetName());
                                                }
                                                _log.Info(ex.ToString());
                                            }
                                            Allocate = false;
                                        }
                                    }
                                }
                                else if (PaymentData[i].TransactionType == "Order")
                                {
                                    orderPaySchedule = new MVA009OrderPaySchedule(ct, PaymentData[i].C_InvoicePaySchedule_ID, trx);
                                    MOrder order = new MOrder(ct, orderPaySchedule.GetC_Order_ID(), trx);
                                    //_doctype = new MDocType(ct, order.GetC_DocType_ID(), trx);
                                    _doctype = MDocType.Get(ct, order.GetC_DocType_ID());
                                    ex.Append("\n" + GeneratePaymentAgainstSOSchedule(ct, PaymentData[i], C_doctype_ID, ex.ToString(), out docno1, trx));
                                    if (docno1 != "")
                                    {
                                        if (docno.Length > 0)
                                        {
                                            docno.Append(", ");
                                        }
                                        docno.Append(docno1);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Complete Payments
                        if (count.Count > 0)
                        {
                            for (int j = 0; j < count.Count; j++)
                            {
                                MPayment _PayComp = new MPayment(ct, count[j], trx);
                                //based on result get from CompleteOrReverse function should execute the condition
                                _result = CompleteOrReverse(ct, _PayComp.GetC_Payment_ID(), _PayComp.Get_Table_ID(), _PayComp.Get_TableName().ToLower(), DocActionVariables.ACTION_COMPLETE, trx);
                                //string docstatus = _PayComp.CompleteIt();
                                //if (docstatus == "CO")
                                //{
                                //    _PayComp.SetDocStatus("CO");
                                //    _PayComp.SetDocAction("CL");
                                //    _PayComp.SetProcessed(true);

                                //if (!_PayComp.Save())
                                //'N' indicates  the record is not Completed Successfully
                                if (_result != null && _result[1].Equals("N"))
                                {
                                    ex.Append(Msg.GetMsg(ct, "VA009_PNotCompelted") + ": " + _PayComp.GetDocumentNo());
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    if (pp != null)
                                    {
                                        ex.Append(", " + pp.GetName());
                                    }
                                    _log.Info(ex.ToString());
                                }
                                else
                                {
                                    if (docno.Length > 0)
                                    {
                                        docno.Append(", ");
                                    }
                                    docno.Append(_PayComp.GetDocumentNo());
                                }
                                //}
                                //else
                                //{
                                //    ex.Append("\n" + Msg.GetMsg(ct, "VA009_PNotCompelted") + ": " + _PayComp.GetDocumentNo());
                                //    if (_PayComp.GetProcessMsg() != null && _PayComp.GetProcessMsg().IndexOf("@") != -1)
                                //    {
                                //        processMsg = Msg.ParseTranslation(ct, _PayComp.GetProcessMsg());
                                //    }
                                //    else
                                //    {
                                //        processMsg = Msg.GetMsg(ct, _PayComp.GetProcessMsg());
                                //    }
                                //    ex.Append(", " + processMsg);
                                //    _log.Info(ex.ToString());
                                //}
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                ex.Append(e.Message);
                _log.Info(ex.ToString());
            }
            finally
            {
                trx.Commit();
                trx.Close();
            }

            if (docno.Length > 0)
            {
                msg += docno.ToString();
                ex.Append("\n" + msg);
            }
            return ex.ToString();
        }

        /// <summary>
        /// Generate Payment against Order Schedule
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="PaymentData">Order Schedule Data</param>
        /// <param name="C_doctype_ID">Document Type</param>
        /// <param name="ex">Exception Message</param>
        /// <param name="docno">Document No as Out Parameter</param>
        /// <param name="trx">Transaction Object</param>
        /// <returns>String, Message</returns>
        public string GeneratePaymentAgainstSOSchedule(Ctx ct, GeneratePaymt PaymentData, int C_doctype_ID, string ex, out string docno, Trx trx)
        {
            docno = "";
            string processMsg = "";
            //to get result from CompleteOrReverse function and store into this variable to execute the condition based on this result
            string[] _result = null;
            try
            {
                MPayment _pay = null;
                ViennaAdvantage.Model.MVA009OrderPaySchedule orderPaySchedule = new MVA009OrderPaySchedule(ct, PaymentData.C_InvoicePaySchedule_ID, trx);
                MOrder order = new MOrder(ct, orderPaySchedule.GetC_Order_ID(), trx);
                MDocType _doctype = new MDocType(ct, order.GetC_DocType_ID(), trx);

                _pay = new MPayment(ct, 0, trx);
                _pay.SetAD_Client_ID(Util.GetValueOfInt(PaymentData.AD_Client_ID));
                _pay.SetAD_Org_ID(Util.GetValueOfInt(PaymentData.AD_Org_ID));
                _pay.SetC_DocType_ID(C_doctype_ID);
                _pay.SetDateAcct(PaymentData.DateAcct);
                _pay.SetDateTrx(System.DateTime.Now.ToLocalTime());
                _pay.SetC_BankAccount_ID(Util.GetValueOfInt(PaymentData.C_BankAccount_ID));
                _pay.SetC_ConversionType_ID(PaymentData.CurrencyType);
                _pay.SetC_BPartner_ID(Util.GetValueOfInt(PaymentData.C_BPartner_ID));
                _pay.SetC_BPartner_Location_ID(order.GetC_BPartner_Location_ID());
                if (PaymentData.PaymwentBaseType == "APP")
                {
                    if (PaymentData.VA009_RecivedAmt < 0)
                        _pay.SetPayAmt(-1 * (PaymentData.VA009_RecivedAmt));
                    else
                        _pay.SetPayAmt(PaymentData.VA009_RecivedAmt);

                    if (_doctype.GetDocBaseType() == "APC")
                    {
                        if (PaymentData.VA009_RecivedAmt > 0)
                            _pay.SetPayAmt(-1 * (PaymentData.VA009_RecivedAmt));
                    }

                    if (PaymentData.Discount < 0)
                        _pay.SetDiscountAmt(-1 * (PaymentData.Discount));
                    else
                        _pay.SetDiscountAmt(PaymentData.Discount);

                    if (_doctype.GetDocBaseType() == "API")
                    {
                        _pay.SetOverUnderAmt((PaymentData.OverUnder));
                    }
                    else if (_doctype.GetDocBaseType() == "ARC")
                    {
                        if (PaymentData.OverUnder < 0)
                            _pay.SetOverUnderAmt(-1 * (PaymentData.OverUnder));
                        else
                            _pay.SetOverUnderAmt((PaymentData.OverUnder));
                    }
                    else if (_doctype.GetDocBaseType() == "APC")
                    {
                        if (PaymentData.OverUnder < 0)
                            _pay.SetOverUnderAmt(-1 * (PaymentData.OverUnder));
                        else
                            _pay.SetOverUnderAmt((PaymentData.OverUnder));
                    }
                    else
                        _pay.SetOverUnderAmt(PaymentData.OverUnder);

                    if (PaymentData.Writeoff < 0)
                        _pay.SetWriteOffAmt(-1 * (PaymentData.Writeoff));
                    else
                        _pay.SetWriteOffAmt(PaymentData.Writeoff);
                }
                else
                {
                    _pay.SetPayAmt(PaymentData.VA009_RecivedAmt);
                    _pay.SetDiscountAmt(PaymentData.Discount);
                    _pay.SetOverUnderAmt(PaymentData.OverUnder);
                    _pay.SetWriteOffAmt(PaymentData.Writeoff);
                }

                _pay.SetC_Currency_ID(GetPaymentCurrency(ct, Util.GetValueOfInt(PaymentData.C_BankAccount_ID)));

                int bankcurrency = GetPaymentCurrency(ct, Util.GetValueOfInt(PaymentData.C_BankAccount_ID));

                _pay.SetC_ConversionType_ID(PaymentData.CurrencyType);
                if (Util.GetValueOfInt(PaymentData.VA009_PaymentMethod_ID) > 0)
                {
                    _pay.SetVA009_PaymentMethod_ID(PaymentData.VA009_PaymentMethod_ID);
                }
                else
                {
                    _pay.SetVA009_PaymentMethod_ID(orderPaySchedule.GetVA009_PaymentMethod_ID());
                }

                if (PaymentData.CheckDate != null)
                {
                    _pay.SetCheckDate(PaymentData.CheckDate);
                }
                _pay.SetCheckNo(PaymentData.CheckNumber);
                if ((PaymentData.VA009_RecivedAmt > 0 && PaymentData.OverUnder < 0) || (PaymentData.VA009_RecivedAmt < 0 && PaymentData.OverUnder > 0))
                {
                    _pay.SetC_Order_ID(Util.GetValueOfInt(PaymentData.C_Invoice_ID));
                    _pay.SetVA009_OrderPaySchedule_ID(Util.GetValueOfInt(PaymentData.C_InvoicePaySchedule_ID));
                }
                else
                {
                    _pay.SetC_Order_ID(Util.GetValueOfInt(PaymentData.C_Invoice_ID));
                    _pay.SetVA009_OrderPaySchedule_ID(Util.GetValueOfInt(PaymentData.C_InvoicePaySchedule_ID));
                }
                if (!_pay.Save())
                {
                    ex = Msg.GetMsg(ct, "VA009_PNotSaved");
                    ValueNamePair pp = VLogger.RetrieveError();
                    if (pp != null)
                        ex += ", " + pp.GetName();
                    _log.Info(ex);
                }
                else
                {
                    //based on result get from CompleteOrReverse function should execute the condition
                    _result = CompleteOrReverse(ct, _pay.GetC_Payment_ID(), _pay.Get_Table_ID(), _pay.Get_TableName().ToLower(), DocActionVariables.ACTION_COMPLETE, trx);
                    //if (_pay.CompleteIt() == "CO")
                    //{
                    //    _pay.SetProcessed(true);
                    //    _pay.SetDocAction("CL");
                    //    _pay.SetDocStatus("CO");
                    //    _pay.Save();
                    //    docno = _pay.GetDocumentNo();
                    //}
                    //if (_pay.Save())
                    //'Y' Indicates the record is Completed Successfully
                    if (_result != null && _result[1].Equals("Y"))
                    {
                        docno = _pay.GetDocumentNo();
                    }
                    else
                    {
                        ex = Msg.GetMsg(ct, "VA009_PNotCompelted") + ": " + _pay.GetDocumentNo();
                        if (_pay.GetProcessMsg() != null && _pay.GetProcessMsg().IndexOf("@") != -1)
                        {
                            processMsg = Msg.ParseTranslation(ct, _pay.GetProcessMsg());
                        }
                        else
                        {
                            processMsg = Msg.GetMsg(ct, _pay.GetProcessMsg());
                        }
                        ex += ", " + processMsg;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Info(e.Message);
            }
            return ex;
        }
        /// <summary>
        /// Get Query to fetch DocBaseType of selected InvoicePaySchedule or Order
        /// Author:VA230
        /// </summary>
        /// <param name="PaymentData">payment data list</param>
        /// <param name="whereCondition">where condition</param>
        /// <returns>Order Pay Schedule Dataset Query</returns>
        public static string GetOrderOrInvoiceDocBaseTypeQuery(GeneratePaymt[] PaymentData, out List<int> ids, out string whereCondition)
        {
            StringBuilder query = new StringBuilder();
            //VA230:Get DocType dataset based on Order or InvoicePaySchedule ids
            int orderCount = PaymentData.Where(x => x.TransactionType == "Order").Count();
            if (orderCount > 0)
            {
                //Get distinct Order ids
                ids = PaymentData.Select(y => y.C_Invoice_ID).Distinct().ToList();
                query.Append(@"SELECT ORD.C_DocTypeTarget_ID AS C_DocType_ID,DT.DocBaseType,ORD.C_BPartner_Location_ID,ORD.C_Order_ID,0 AS C_InvoicePaySchedule_ID FROM C_Order ORD
                            INNER JOIN C_DocType DT ON DT.C_DocType_ID=ORD.C_DocType_ID");
                whereCondition = "ORD.C_Order_ID";
            }
            else
            {
                //Get distinct InvoicePaySchedule ids
                ids = PaymentData.Select(y => y.C_InvoicePaySchedule_ID).Distinct().ToList();
                //Get invoice doctype and BP LocationID query
                query.Append(@"SELECT PS.C_DocType_ID,DT.DocBaseType,INV.C_BPartner_Location_ID,0 AS C_Order_ID,PS.C_InvoicePaySchedule_ID FROM C_InvoicePaySchedule PS
                    INNER JOIN C_Invoice INV ON INV.C_Invoice_ID=PS.C_Invoice_ID
                    INNER JOIN C_DocType DT ON DT.C_DocType_ID=PS.C_DocType_ID");
                whereCondition = "PS.C_InvoicePaySchedule_ID";
            }
            return query.ToString();
        }
        /// <summary>
        /// Get DocBaseType of selected InvoicePaySchedule or Order
        /// Author:VA230
        /// </summary>
        /// <param name="ids">InvoicePaySchedule or Order ids list</param>
        /// <returns>Order Pay Schedule Dataset</returns>
        public static DataSet GetOrderOrInvoiceDocBaseTypeData(List<int> ids, string query, string whereId)
        {
            decimal totalPages = ids.Count();
            //to fixed 999 ids per page
            totalPages = Math.Ceiling(totalPages / 999);

            StringBuilder sql = new StringBuilder();
            sql.Append(query);
            List<string> schedule_Ids = new List<string>();
            //loop through each page
            for (int i = 0; i <= totalPages - 1; i++)
            {
                //get comma seperated product ids max 999
                schedule_Ids.Add(string.Join(",", ids.Select(r => r.ToString()).Skip(i * 999).Take(999)));
            }
            if (schedule_Ids.Count > 0)
            {
                //append product in sql statement use OR keyword when records are more than 999
                for (int i = 0; i < schedule_Ids.Count; i++)
                {
                    if (i == 0)
                    {
                        sql.Append(@" AND (");
                    }
                    else
                    {
                        sql.Append(" OR ");
                    }
                    sql.Append(" " + whereId + @" IN (" + schedule_Ids[i] + @")");
                    if (i == schedule_Ids.Count - 1)
                    {
                        sql.Append(" ) ");
                    }
                }
            }
            DataSet ds = DB.ExecuteDataset(sql.ToString());
            return ds;
        }
        /// <summary>
        /// Create Cash Journal Payment
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="PaymentData">Cash Data</param>
        /// <param name="C_CashBook_ID">Cash Book</param>
        /// <param name="BeginningBalance">Begining Balance of Cash Book</param>
        /// <returns>String, Message</returns>
        public string CreatePaymentsCash(Ctx ct, GeneratePaymt[] PaymentData, int C_CashBook_ID, decimal BeginningBalance)
        {
            Trx trx = Trx.GetTrx("Cash_" + DateTime.Now.ToString("yyMMddHHmmssff"));
            string status = "I";
            string msg = string.Empty;
            string docno = "";
            try
            {
                MCashBook cashbookid = new MCashBook(ct, C_CashBook_ID, null);
                int C_Cash_ID = 0;

                if (PaymentData.Length > 0)
                {
                    //Rakesh(VA228):Set Document Type id (13/Sep/2021)
                    int C_doctype_ID = PaymentData[0].TargetDocType;

                    //fetch the Cash_ID with the StatementDate not the AccountDate
                    int no = Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT COUNT(C_Cash_ID) FROM C_Cash WHERE IsActive = 'Y' AND DocStatus NOT IN ('CO' , 'CL', 'VO')  
                                 AND StatementDate != " + GlobalVariable.TO_DATE(PaymentData[0].DateTrx, true) + " AND C_CashBook_ID = " + C_CashBook_ID, null, null));
                    if (no > 0)
                    {
                        msg = Msg.GetMsg(ct, "VIS_CantOpenNewCashBook");
                        return msg;
                    }
                    else
                    {
                        //fetch the Cash_ID with the StatementDate not the AccountDate
                        C_Cash_ID = Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT C_Cash_ID FROM C_Cash WHERE IsActive='Y' AND DocStatus='DR' 
                                    AND StatementDate = " + GlobalVariable.TO_DATE(PaymentData[0].DateTrx, true) + " AND C_CashBook_ID = " + C_CashBook_ID, null, null));
                    }

                    MCash _Cash = null;
                    if (C_Cash_ID != 0)
                    {
                        _Cash = new MCash(ct, C_Cash_ID, trx);
                    }
                    else
                    {
                        _Cash = new MCash(ct, 0, trx);
                        _Cash.SetAD_Client_ID(ct.GetAD_Client_ID());
                        _Cash.SetAD_Org_ID(PaymentData[0].AD_Org_ID);
                        _Cash.SetC_CashBook_ID(C_CashBook_ID);
                        _Cash.SetC_DocType_ID(C_doctype_ID);
                        //_Cash.SetName(DateTime.Now.ToShortDateString());
                        //set Name as TransactionDate 
                        _Cash.SetName(PaymentData[0].DateTrx.Value.ToShortDateString());
                        // to set Transaction date 
                        _Cash.SetStatementDate(PaymentData[0].DateTrx);
                        _Cash.SetDateAcct(PaymentData[0].DateAcct);
                        //_Cash.SetStatementDate(DateTime.Now.ToLocalTime());
                        _Cash.SetBeginningBalance(BeginningBalance);
                        if (!_Cash.Save())
                        {
                            trx.Rollback();
                            ValueNamePair pp = VLogger.RetrieveError();
                            if (pp != null)
                                msg = pp.GetName();
                            //if GetName is Empty then it will check GetValue
                            if (string.IsNullOrEmpty(msg))
                                msg = Msg.GetMsg("", pp.GetValue());
                            _log.Info(msg);
                            return msg;
                        }
                    }

                    #region GetDocTypes
                    //VA230:Get DocType dataset based on Order or InvoicePaySchedule ids
                    DataSet _Ds = null;
                    DataRow[] dr = null;
                    //Get query to fetch DocBase type
                    string query = GetOrderOrInvoiceDocBaseTypeQuery(PaymentData, out List<int> ids, out string where);
                    //Get docbasetype data
                    _Ds = GetOrderOrInvoiceDocBaseTypeData(ids, query, where);
                    if (_Ds == null && _Ds.Tables.Count == 0 && _Ds.Tables[0].Rows.Count == 0)
                    {
                        msg = Msg.GetMsg(ct, "VA009_DocTypeNotFound");
                        return msg;
                    }

                    #endregion

                    for (int i = 0; i < PaymentData.Length; i++)
                    {
                        //VA230:If transaction type is Order then get order doctype and BP LocationID based on OrderId
                        if (!string.IsNullOrEmpty(PaymentData[i].TransactionType) && PaymentData[i].TransactionType.ToUpper() == "ORDER")
                        {
                            dr = _Ds.Tables[0].Select("C_Order_ID=" + Util.GetValueOfInt(PaymentData[i].C_Invoice_ID));
                        }
                        else
                        {
                            //Get invoice doctype and BP LocationID based on InvoicePayScheduleId
                            dr = _Ds.Tables[0].Select("C_InvoicePaySchedule_ID=" + Util.GetValueOfInt(PaymentData[i].C_InvoicePaySchedule_ID));
                        }
                        if (dr == null && dr.Length == 0)
                        {
                            continue;
                        }
                        #region CashLineSave

                        MCashLine _cline = new MCashLine(ct, 0, trx);
                        if (PaymentData[i].PaymwentBaseType == "APP")
                        {
                            //In case of AP Invoice and Purchase Order
                            if (Util.GetValueOfString(dr[0]["DocBaseType"]) == "API" || Util.GetValueOfString(dr[0]["DocBaseType"]) == "POO")
                            {
                                _cline.SetVSS_PAYMENTTYPE("P");
                                _cline.SetAmount(-1 * (PaymentData[i].VA009_RecivedAmt));
                                //both Amount and ConvertedAmt must be equal in CashLine
                                _cline.SetConvertedAmt(Util.GetValueOfString(-1 * (PaymentData[i].VA009_RecivedAmt)));
                                _cline.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                _cline.SetDiscountAmt(-1 * (PaymentData[i].Discount));
                                _cline.SetWriteOffAmt(-1 * (PaymentData[i].Writeoff));
                            }
                            else
                            {
                                _cline.SetVSS_PAYMENTTYPE("R");
                                _cline.SetAmount(PaymentData[i].VA009_RecivedAmt);
                                //both Amount and ConvertedAmt must be equal in CashLine
                                _cline.SetConvertedAmt(Util.GetValueOfString(PaymentData[i].VA009_RecivedAmt));
                                _cline.SetOverUnderAmt(PaymentData[i].OverUnder);
                                _cline.SetDiscountAmt(PaymentData[i].Discount);
                                _cline.SetWriteOffAmt(PaymentData[i].Writeoff);
                            }
                        }
                        else
                        {
                            if (Util.GetValueOfString(dr[0]["DocBaseType"]) == "ARC")
                            {
                                _cline.SetVSS_PAYMENTTYPE("P");
                                _cline.SetAmount(-1 * (PaymentData[i].VA009_RecivedAmt));
                                //both Amount and ConvertedAmt must be equal in CashLine
                                _cline.SetConvertedAmt(Util.GetValueOfString(-1 * (PaymentData[i].VA009_RecivedAmt)));
                                _cline.SetOverUnderAmt(-1 * (PaymentData[i].OverUnder));
                                _cline.SetDiscountAmt(-1 * (PaymentData[i].Discount));
                                _cline.SetWriteOffAmt(-1 * (PaymentData[i].Writeoff));
                            }
                            else
                            {
                                //Also Excute in case doctbasetype is SOO
                                _cline.SetVSS_PAYMENTTYPE("R");
                                _cline.SetAmount(PaymentData[i].VA009_RecivedAmt);
                                //both Amount and ConvertedAmt must be equal in CashLine
                                _cline.SetConvertedAmt(Util.GetValueOfString(PaymentData[i].VA009_RecivedAmt));
                                _cline.SetOverUnderAmt(PaymentData[i].OverUnder);
                                _cline.SetDiscountAmt(PaymentData[i].Discount);
                                _cline.SetWriteOffAmt(PaymentData[i].Writeoff);
                            }
                        }
                        _cline.SetAD_Client_ID(PaymentData[i].AD_Client_ID);
                        _cline.SetAD_Org_ID(PaymentData[i].AD_Org_ID);
                        _cline.SetC_Cash_ID(_Cash.GetC_Cash_ID());
                        _cline.SetC_BPartner_ID(PaymentData[i].C_BPartner_ID);
                        _cline.SetDescription(Util.GetValueOfString(PaymentData[i].Description));
                        _cline.SetC_BPartner_Location_ID(Util.GetValueOfInt(dr[0]["C_BPartner_Location_ID"]));
                        _cline.SetC_Currency_ID(cashbookid.GetC_Currency_ID());
                        //VA230:If transaction type is Order then set order and order payschedule reference else set invoice reference 
                        if (!String.IsNullOrEmpty(PaymentData[i].TransactionType) && PaymentData[i].TransactionType.ToUpper() == "ORDER")
                        {
                            if (_cline.Get_ColumnIndex("VA009_OrderPaySchedule_ID") >= 0)
                            {
                                _cline.SetVA009_OrderPaySchedule_ID(PaymentData[i].C_InvoicePaySchedule_ID);
                                _cline.SetC_Order_ID(PaymentData[i].C_Invoice_ID);
                                _cline.SetIsPrepayment(true);
                            }
                            //Set cashtype Order
                            status = "O";
                        }
                        else
                        {
                            _cline.SetC_InvoicePaySchedule_ID(PaymentData[i].C_InvoicePaySchedule_ID);
                            _cline.SetC_Invoice_ID(PaymentData[i].C_Invoice_ID);
                            status = "I";
                        }
                        _cline.SetCashType(status);
                        //to set ConversionType in Cash Journal Line
                        _cline.SetC_ConversionType_ID(PaymentData[i].CurrencyType);
                        //amit
                        if (!_cline.Save())
                        {
                            trx.Rollback();
                            ValueNamePair pp = VLogger.RetrieveError();
                            if (pp != null)
                                msg = pp.GetName();
                            _log.Info(msg);
                            return msg;
                        }
                        else
                        {
                            DB.ExecuteQuery("UPDATE C_InvoicePaySchedule SET VA009_ExecutionStatus= 'J' WHERE C_InvoicePaySchedule_ID = " + PaymentData[i].C_InvoicePaySchedule_ID, null, trx);
                        }
                        #endregion
                    }
                    docno = _Cash.GetDocumentNo();
                }
            }
            catch (Exception e)
            {
                trx.Rollback();
                msg = e.Message;
                _log.Info(msg);
            }
            finally
            {
                trx.Commit();
                trx.Close();
            }

            return Msg.GetMsg(ct, "VA009_SavedSuccessfully") + " = " + docno;
        }

        public string CrateBatchPayment(Ctx ct, GeneratePaymt[] PaymentData)
        {
            Trx trx = Trx.GetTrx("Batch_" + DateTime.Now.ToString("yyMMddHHmmssff"));
            List<int> abc = new List<int>();
            List<int> BtachId = new List<int>();
            StringBuilder docno = new StringBuilder();
            StringBuilder ex = new StringBuilder();
            string msg = "";
            MBankAccount _BankAcct = new MBankAccount(ct, PaymentData[0].C_BankAccount_ID, trx);
            List<int> PaymentMethodIDS = new List<int>();
            string isconsolidate = PaymentData[0].isconsolidate;
            string isOverwrite = PaymentData[0].isOverwrite;
            int batchid = 0; MVA009Batch _Bt = null;
            decimal convertedAmount = 0;
            String _TransactionType = String.Empty; //Arpit
            try
            {
                if (PaymentData.Length > 0)
                {
                    for (int i = 0; i < PaymentData.Length; i++)
                    {
                        int paymentmethdoID = PaymentData[i].VA009_PaymentMethod_ID; convertedAmount = PaymentData[i].convertedAmt;
                        MVA009PaymentMethod _paymthd = new MVA009PaymentMethod(ct, paymentmethdoID, trx);
                        _TransactionType = String.Empty;
                        _TransactionType = PaymentData[i].TransactionType;
                        if (_TransactionType == "Invoice")
                        {
                            MInvoicePaySchedule _invpaySchdule = new MInvoicePaySchedule(ct, PaymentData[i].C_InvoicePaySchedule_ID, trx);
                            MDocType _doctype = new MDocType(ct, _invpaySchdule.GetC_DocType_ID(), trx);
                            #region Is Consolidate
                            //if (isconsolidate == "Y")
                            //{

                            if (abc.Contains(PaymentData[i].C_BPartner_ID) && (PaymentMethodIDS.Contains(paymentmethdoID)) && (_paymthd.GetVA009_PaymentType() != "S"))
                            {
                                _Bt = new MVA009Batch(ct, batchid, trx);
                                _Bt.SetVA009_PaymentMethod_ID(paymentmethdoID);
                                _Bt.SetVA009_PaymentRule(_paymthd.GetVA009_PaymentRule());
                                _Bt.SetVA009_PaymentTrigger(_paymthd.GetVA009_PaymentTrigger());
                                if (isconsolidate == "Y")
                                    _Bt.SetVA009_Consolidate(true);

                                _Bt.SetProcessed(true);
                                if (!_Bt.Save())
                                {
                                    trx.Rollback();
                                    ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    if (pp != null)
                                    {
                                        ex.Append(", " + pp.GetName());
                                    }
                                    _log.Info(ex.ToString());
                                }
                                else
                                {
                                    #region BatchLine

                                    int Batchline_ID = Util.GetValueOfInt(DB.ExecuteScalar(@"select VA009_BatchLines_ID from VA009_BatchLines where 
                                    VA009_Batch_ID=" + _Bt.GetVA009_Batch_ID() + " and C_BPartner_ID=" + PaymentData[i].C_BPartner_ID, null, trx));

                                    if (Batchline_ID == 0)
                                    {
                                        Batchline_ID = GenerateBatchLine(ct, PaymentData[i], _Bt, trx);
                                        if (Batchline_ID == 0)
                                        {
                                            trx.Rollback();
                                            ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                            {
                                                ex.Append(", " + pp.GetName());
                                            }
                                            _log.Info(ex.ToString());
                                        }
                                    }
                                    else
                                    {
                                        if (GenerateBatchLineDetails(ct, PaymentData[i], _Bt, _BankAcct, _invpaySchdule, _doctype, convertedAmount, paymentmethdoID, Batchline_ID, isOverwrite, trx) == 0)
                                        {
                                            trx.Rollback();
                                            ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                            {
                                                ex.Append(", " + pp.GetName());
                                            }
                                            _log.Info(ex.ToString());
                                        }
                                    }
                                    #endregion
                                }
                                continue;
                            }
                            else
                            {
                                _Bt = new MVA009Batch(ct, 0, trx);

                                _Bt.SetC_Bank_ID(PaymentData[0].C_Bank_ID);
                                _Bt.SetC_BankAccount_ID(PaymentData[0].C_BankAccount_ID);
                                _Bt.SetAD_Client_ID(PaymentData[0].AD_Client_ID);
                                _Bt.SetAD_Org_ID(PaymentData[0].AD_Org_ID);
                                //to set document type against batch payment
                                _Bt.Set_ValueNoCheck("C_DocType_ID", getDocumentTypeID(ct, PaymentData[0].AD_Org_ID, trx));
                                //end
                                _Bt.SetVA009_PaymentMethod_ID(paymentmethdoID);
                                _Bt.SetVA009_PaymentRule(_paymthd.GetVA009_PaymentRule());
                                _Bt.SetVA009_PaymentTrigger(_paymthd.GetVA009_PaymentTrigger());
                                //to set bank currency on Payment Batch given by Rajni and Ashish
                                _Bt.Set_Value("C_Currency_ID", _BankAcct.GetC_Currency_ID());
                                //_Bt.SetProcessed(true);
                                _Bt.SetVA009_DocumentDate(DateTime.Now);
                                //Rakesh(VA228):Set account date
                                _Bt.SetDateAcct(DateTime.Now);
                                if (isconsolidate == "Y")
                                    _Bt.SetVA009_Consolidate(true);

                                if (!_Bt.Save())
                                {
                                    trx.Rollback();
                                    ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    if (pp != null)
                                    {
                                        ex.Append(", " + pp.GetName());
                                    }
                                    _log.Info(ex.ToString());
                                }
                                else
                                {
                                    batchid = _Bt.GetVA009_Batch_ID();
                                    BtachId.Add(batchid);
                                    PaymentMethodIDS.Add(paymentmethdoID);
                                }

                                abc.Add(PaymentData[i].C_BPartner_ID);

                                int Batchline_ID = GenerateBatchLine(ct, PaymentData[i], _Bt, trx);
                                if (Batchline_ID == 0)
                                {
                                    trx.Rollback();
                                    ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    if (pp != null)
                                    {
                                        ex.Append(", " + pp.GetName());
                                    }
                                    _log.Info(ex.ToString());
                                }
                                else
                                {
                                    if (GenerateBatchLineDetails(ct, PaymentData[i], _Bt, _BankAcct, _invpaySchdule, _doctype, convertedAmount, paymentmethdoID, Batchline_ID, isOverwrite, trx) == 0)
                                    {
                                        trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }
                                }

                                #region Commented Code
                                //MVA009BatchLines _BtLines = new MVA009BatchLines(ct, 0, trx);
                                // _BtLines.SetVA009_Batch_ID(_Bt.GetVA009_Batch_ID());
                                // _BtLines.SetC_BPartner_ID(PaymentData[i].C_BPartner_ID);
                                // _BtLines.SetAD_Client_ID(PaymentData[i].AD_Client_ID);
                                // _BtLines.SetAD_Org_ID(PaymentData[i].AD_Org_ID);
                                // _BtLines.SetProcessed(true);
                                // if (!_BtLines.Save())
                                // {
                                //     //trx.Rollback();
                                //     ValueNamePair pp = VLogger.RetrieveError();
                                //     _log.Info(pp.GetName());
                                // }
                                // else
                                // {
                                //     MVA009BatchLineDetails _btDetal = new MVA009BatchLineDetails(ct, 0, trx);
                                //     _btDetal.SetAD_Client_ID(PaymentData[i].AD_Client_ID);
                                //     _btDetal.SetAD_Org_ID(PaymentData[i].AD_Org_ID);
                                //     _btDetal.SetC_InvoicePaySchedule_ID(PaymentData[i].C_InvoicePaySchedule_ID);
                                //     _btDetal.SetC_Invoice_ID(PaymentData[i].C_Invoice_ID);
                                //     _btDetal.SetC_Currency_ID(_BankAcct.GetC_Currency_ID());
                                //     _btDetal.SetDueDate(_invpaySchdule.GetDueDate());
                                //     _btDetal.SetC_ConversionType_ID(PaymentData[i].CurrencyType);
                                //     _btDetal.SetDueAmt(PaymentData[i].DueAmt);
                                //     if (_doctype.GetDocBaseType() == "ARC" || _doctype.GetDocBaseType() == "APC")
                                //     {
                                //         if (convertedAmount > 0)
                                //             convertedAmount = -1 * convertedAmount;
                                //     }
                                //     else
                                //     {
                                //         if (_doctype.GetDocBaseType() == "API")
                                //         {
                                //             if (convertedAmount < 0)
                                //                 convertedAmount = -1 * convertedAmount;
                                //         }

                                //     }
                                //     _btDetal.SetVA009_ConvertedAmt(convertedAmount);
                                //     if (paymentmethdoID > 0)
                                //         _btDetal.SetVA009_PaymentMethod_ID(paymentmethdoID);

                                //     _btDetal.SetProcessed(true);
                                //     _btDetal.SetVA009_BatchLines_ID(_BtLines.GetVA009_BatchLines_ID());
                                //     if (!_btDetal.Save())
                                //     {
                                //         //trx.Rollback();
                                //         ValueNamePair pp = VLogger.RetrieveError();
                                //         _log.Info(pp.GetName());
                                //     }
                                //     else
                                //     {
                                //         _invpaySchdule = new MInvoicePaySchedule(ct, PaymentData[i].C_InvoicePaySchedule_ID, trx);
                                //         _invpaySchdule.SetVA009_ExecutionStatus("Y");
                                //         if (isOverwrite == "Y")
                                //             _invpaySchdule.SetVA009_PaymentMethod_ID(PaymentData[i].VA009_PaymentMethod_ID);

                                //         if (!_invpaySchdule.Save())
                                //         {
                                //             //trx.Rollback();
                                //             ValueNamePair pp = VLogger.RetrieveError();
                                //             _log.Info(pp.GetName());
                                //         }
                                //     }
                                // }
                                #endregion
                            }
                            //}
                            #endregion
                        }
                        if (_TransactionType == "Order") //Added by Arpit on 7th Dec,2016 
                        //Purpose- To Create Batch Line Details against Order & OrderPay Schedule 
                        {
                            MVA009OrderPaySchedule _OrdPaySchdule = new MVA009OrderPaySchedule(ct, PaymentData[i].C_InvoicePaySchedule_ID, trx);
                            MDocType _doctype = new MDocType(ct, _OrdPaySchdule.GetC_DocType_ID(), trx);
                            #region Is Consolidate
                            //if (isconsolidate == "Y")
                            //{
                            if (abc.Contains(PaymentData[i].C_BPartner_ID) && (PaymentMethodIDS.Contains(paymentmethdoID)) && (_paymthd.GetVA009_PaymentType() != "S"))
                            {
                                _Bt = new MVA009Batch(ct, batchid, trx);
                                _Bt.SetVA009_PaymentMethod_ID(paymentmethdoID);
                                _Bt.SetVA009_PaymentRule(_paymthd.GetVA009_PaymentRule());
                                _Bt.SetVA009_PaymentTrigger(_paymthd.GetVA009_PaymentTrigger());
                                if (isconsolidate == "Y")
                                {
                                    _Bt.SetVA009_Consolidate(true);
                                }
                                _Bt.SetProcessed(true);
                                if (!_Bt.Save())
                                {
                                    // trx.Rollback();
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    if (pp != null)
                                        _log.Info(pp.GetName());
                                }
                                else
                                {
                                    #region BatchLine
                                    int Batchline_ID = Util.GetValueOfInt(DB.ExecuteScalar(@"select VA009_BatchLines_ID from VA009_BatchLines where 
                                     VA009_Batch_ID=" + _Bt.GetVA009_Batch_ID() + " and C_BPartner_ID=" + PaymentData[i].C_BPartner_ID, null, trx));
                                    if (Batchline_ID == 0)
                                    {
                                        Batchline_ID = GenerateBatchLine(ct, PaymentData[i], _Bt, trx);
                                        if (Batchline_ID == 0)
                                        {
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                                _log.Info(pp.GetName());
                                        }

                                        #region Commented Code
                                        //MVA009BatchLines _BtLines = new MVA009BatchLines(ct, 0, trx);
                                        //_BtLines.SetVA009_Batch_ID(_Bt.GetVA009_Batch_ID());
                                        //_BtLines.SetC_BPartner_ID(PaymentData[i].C_BPartner_ID);
                                        //_BtLines.SetAD_Client_ID(PaymentData[i].AD_Client_ID);
                                        //_BtLines.SetAD_Org_ID(PaymentData[i].AD_Org_ID);
                                        //_BtLines.SetProcessed(true);
                                        //if (!_BtLines.Save())
                                        //{
                                        //    //trx.Rollback();
                                        //    ValueNamePair pp = VLogger.RetrieveError();
                                        //    _log.Info(pp.GetName());
                                        //}
                                        //else
                                        //{
                                        //    Batchline_ID = _BtLines.GetVA009_BatchLines_ID();

                                        //    #region BatchDetail
                                        //    MVA009BatchLineDetails _btDetal = new MVA009BatchLineDetails(ct, 0, trx);
                                        //    _btDetal.SetAD_Client_ID(PaymentData[i].AD_Client_ID);
                                        //    _btDetal.SetAD_Org_ID(PaymentData[i].AD_Org_ID);
                                        //    _btDetal.Set_Value("VA009_OrderPaySchedule_ID", PaymentData[i].C_InvoicePaySchedule_ID); // Here OrderPaySchedule_ID is AS InvoicePaySchedule_ID
                                        //    _btDetal.Set_Value("C_Order_ID", PaymentData[i].C_Invoice_ID); //Here C_Order_ID is as C_Invoice_ID
                                        //    _btDetal.SetC_Currency_ID(_BankAcct.GetC_Currency_ID());
                                        //    _btDetal.SetVA009_BatchLines_ID(Batchline_ID);
                                        //    _btDetal.SetDueAmt(PaymentData[i].DueAmt);
                                        //    _btDetal.SetC_ConversionType_ID(PaymentData[i].CurrencyType);
                                        //    _btDetal.SetDueDate(_OrdPaySchdule.GetDueDate());
                                        //    if (_doctype.GetDocBaseType() == "ARC" || _doctype.GetDocBaseType() == "APC")
                                        //    {
                                        //        if (convertedAmount > 0)
                                        //            convertedAmount = -1 * convertedAmount;
                                        //    }
                                        //    else
                                        //    {
                                        //        if (_doctype.GetDocBaseType() == "API")
                                        //        {
                                        //            if (convertedAmount < 0)
                                        //                convertedAmount = -1 * convertedAmount;
                                        //        }

                                        //    }
                                        //    _btDetal.SetVA009_ConvertedAmt(convertedAmount);
                                        //    if (paymentmethdoID > 0)
                                        //    {
                                        //        _btDetal.SetVA009_PaymentMethod_ID(paymentmethdoID);
                                        //    }
                                        //    _btDetal.SetProcessed(true);
                                        //    if (!_btDetal.Save())
                                        //    {
                                        //        //  trx.Rollback();
                                        //        ValueNamePair pp = VLogger.RetrieveError();
                                        //        _log.Info(pp.GetName());
                                        //    }
                                        //    else
                                        //    {
                                        //        _OrdPaySchdule = new MVA009OrderPaySchedule(ct, PaymentData[i].C_InvoicePaySchedule_ID, trx);
                                        //        _OrdPaySchdule.SetVA009_ExecutionStatus("Y");
                                        //        if (isOverwrite == "Y")
                                        //        {
                                        //            _OrdPaySchdule.SetVA009_PaymentMethod_ID(PaymentData[i].VA009_PaymentMethod_ID);
                                        //        }

                                        //        if (!_OrdPaySchdule.Save())
                                        //        {
                                        //            //  trx.Rollback();
                                        //            ValueNamePair pp = VLogger.RetrieveError();
                                        //            _log.Info(pp.GetName());
                                        //        }
                                        //    }
                                        //    #endregion
                                        //}
                                        #endregion
                                    }
                                    else
                                    {
                                        if (GenerateBatchOrdLineDetails(ct, PaymentData[i], _Bt, _BankAcct, _OrdPaySchdule, _doctype, convertedAmount, paymentmethdoID, Batchline_ID, isOverwrite, trx) == 0)
                                        {
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                                _log.Info(pp.GetName());
                                        }
                                    }
                                    #endregion
                                }
                                continue;
                            }
                            else
                            {
                                _Bt = new MVA009Batch(ct, 0, trx);
                                _Bt.SetC_Bank_ID(PaymentData[0].C_Bank_ID);
                                _Bt.SetC_BankAccount_ID(PaymentData[0].C_BankAccount_ID);
                                _Bt.SetAD_Client_ID(PaymentData[0].AD_Client_ID);
                                _Bt.SetAD_Org_ID(PaymentData[0].AD_Org_ID);
                                _Bt.SetVA009_PaymentMethod_ID(paymentmethdoID);
                                _Bt.SetVA009_PaymentRule(_paymthd.GetVA009_PaymentRule());
                                _Bt.SetVA009_PaymentTrigger(_paymthd.GetVA009_PaymentTrigger());
                                //to set bank currency on Payment Batch given by Rajni and Ashish
                                _Bt.Set_Value("C_Currency_ID", _BankAcct.GetC_Currency_ID());
                                //_Bt.SetProcessed(true);
                                _Bt.SetVA009_DocumentDate(DateTime.Now);
                                //Rakesh(VA228):Set account date
                                _Bt.SetDateAcct(DateTime.Now);
                                if (isconsolidate == "Y")
                                {
                                    _Bt.SetVA009_Consolidate(true);
                                }
                                if (!_Bt.Save())
                                {
                                    trx.Rollback();
                                    ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    if (pp != null)
                                    {
                                        ex.Append(", " + pp.GetName());
                                    }
                                    _log.Info(ex.ToString());
                                }
                                else
                                {
                                    batchid = _Bt.GetVA009_Batch_ID();
                                    BtachId.Add(batchid);
                                    PaymentMethodIDS.Add(paymentmethdoID);

                                    #region BatchLine
                                    abc.Add(PaymentData[i].C_BPartner_ID);
                                    int Batchline_ID = GenerateBatchLine(ct, PaymentData[i], _Bt, trx);
                                    if (Batchline_ID == 0)
                                    {
                                        trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }
                                    else
                                    {
                                        if (GenerateBatchOrdLineDetails(ct, PaymentData[i], _Bt, _BankAcct, _OrdPaySchdule, _doctype, convertedAmount, paymentmethdoID, Batchline_ID, isOverwrite, trx) == 0)
                                        {
                                            trx.Rollback();
                                            ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                            {
                                                ex.Append(", " + pp.GetName());
                                            }
                                            _log.Info(ex.ToString());
                                        }
                                    }

                                    #region commented code
                                    //MVA009BatchLines _BtLines = new MVA009BatchLines(ct, 0, trx);
                                    //_BtLines.SetVA009_Batch_ID(_Bt.GetVA009_Batch_ID());
                                    //_BtLines.SetC_BPartner_ID(PaymentData[i].C_BPartner_ID);
                                    //_BtLines.SetAD_Client_ID(PaymentData[i].AD_Client_ID);
                                    //_BtLines.SetAD_Org_ID(PaymentData[i].AD_Org_ID);
                                    //_BtLines.SetProcessed(true);
                                    //if (!_BtLines.Save())
                                    //{
                                    //    //trx.Rollback();
                                    //    ValueNamePair pp = VLogger.RetrieveError();
                                    //    _log.Info(pp.GetName());
                                    //}
                                    //else
                                    //{
                                    //    MVA009BatchLineDetails _btDetal = new MVA009BatchLineDetails(ct, 0, trx);
                                    //    _btDetal.SetAD_Client_ID(PaymentData[i].AD_Client_ID);
                                    //    _btDetal.SetAD_Org_ID(PaymentData[i].AD_Org_ID);
                                    //    _btDetal.Set_Value("VA009_OrderPaySchedule_ID", PaymentData[i].C_InvoicePaySchedule_ID); // Here OrderPaySchedule_ID is AS InvoicePaySchedule_ID
                                    //    _btDetal.Set_Value("C_Order_ID", PaymentData[i].C_Invoice_ID); //Here C_Order_ID is as C_Invoice_ID
                                    //    _btDetal.SetC_Currency_ID(_BankAcct.GetC_Currency_ID());
                                    //    _btDetal.SetDueDate(_OrdPaySchdule.GetDueDate());
                                    //    _btDetal.SetC_ConversionType_ID(PaymentData[i].CurrencyType);
                                    //    _btDetal.SetDueAmt(PaymentData[i].DueAmt);
                                    //    if (_doctype.GetDocBaseType() == "ARC" || _doctype.GetDocBaseType() == "APC")
                                    //    {
                                    //        if (convertedAmount > 0)
                                    //            convertedAmount = -1 * convertedAmount;
                                    //    }
                                    //    else
                                    //    {
                                    //        if (_doctype.GetDocBaseType() == "API")
                                    //        {
                                    //            if (convertedAmount < 0)
                                    //                convertedAmount = -1 * convertedAmount;
                                    //        }

                                    //    }
                                    //    _btDetal.SetVA009_ConvertedAmt(convertedAmount);
                                    //    if (paymentmethdoID > 0)
                                    //    {
                                    //        _btDetal.SetVA009_PaymentMethod_ID(paymentmethdoID);
                                    //    }
                                    //    _btDetal.SetProcessed(true);
                                    //    _btDetal.SetVA009_BatchLines_ID(_BtLines.GetVA009_BatchLines_ID());
                                    //    if (!_btDetal.Save())
                                    //    {
                                    //        // trx.Rollback();
                                    //        ValueNamePair pp = VLogger.RetrieveError();
                                    //        _log.Info(pp.GetName());
                                    //    }
                                    //    else
                                    //    {
                                    //        _OrdPaySchdule = new MVA009OrderPaySchedule(ct, PaymentData[i].C_InvoicePaySchedule_ID, trx);
                                    //        _OrdPaySchdule.SetVA009_ExecutionStatus("Y");
                                    //        if (isOverwrite == "Y")
                                    //        {
                                    //            _OrdPaySchdule.SetVA009_PaymentMethod_ID(PaymentData[i].VA009_PaymentMethod_ID);
                                    //        }
                                    //        if (!_OrdPaySchdule.Save())
                                    //        {
                                    //            // trx.Rollback();
                                    //            ValueNamePair pp = VLogger.RetrieveError();
                                    //            _log.Info(pp.GetName());
                                    //        }
                                    //    }
                                    //}
                                    #endregion

                                    #endregion
                                }
                            }
                            //}
                            #endregion
                        } //End Here Arpit
                    }
                    if (BtachId.Count > 0)
                    {
                        for (int j = 0; j < BtachId.Count; j++)
                        {
                            MVA009Batch _batchComp = new MVA009Batch(ct, BtachId[j], trx);
                            MVA009PaymentMethod _payMthd = new MVA009PaymentMethod(ct, _batchComp.GetVA009_PaymentMethod_ID(), trx);
                            if (docno.Length > 0)
                                docno.Append(".");

                            docno.Append(_batchComp.GetDocumentNo());
                            if (_payMthd.GetVA009_PaymentRule() == "M")
                            {
                                if (_payMthd.IsVA009_InitiatePay())
                                {

                                    VA009_CreatePayments payment = new VA009_CreatePayments();
                                    return payment.DoIt(BtachId[j], ct, trx, PaymentData[0].CurrencyType);
                                }
                            }
                            else if (_payMthd.GetVA009_PaymentRule() == "E")
                            {
                                if (_payMthd.IsVA009_InitiatePay())
                                {
                                    VA009_CreatePayments payment = new VA009_CreatePayments();
                                    return payment.DoIt(BtachId[j], ct, trx, PaymentData[0].CurrencyType);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                trx.Rollback();
                ex.Append(e.Message);
                _log.Info(e.Message);
            }
            finally
            {
                trx.Commit();
                trx.Close();
            }

            if (docno.Length > 0)
            {
                msg = Msg.GetMsg(ct, "VA009_PaymentCompletedWith");
                msg += docno.ToString();
                ex.Append(msg);
            }

            return ex.ToString();
        }

        public string PrintChque(Ctx ct, GeneratePaymt[] PaymentData)
        {
            string ex = "", saved = "";
            if (PaymentData.Length > 0)
            {

            }
            return ex;
        }

        /// <summary>
        /// Split Schedule
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="PaymentData">List of Schedules Data</param>
        /// <param name="SplitAmmount">Split Amount</param>
        /// <returns>String, Message</returns>
        public string SplitSchedule(Ctx ct, GeneratePaymt[] PaymentData, string SplitAmmount)
        {
            StringBuilder _sql = new StringBuilder();
            String Msg = "VA009_SavedSuccessfullySplit";
            if (PaymentData[0].TransactionType == "Invoice")
            {
                Trx trx = Trx.GetTrx("Split_" + DateTime.Now.ToString("yyMMddHHmmssff"));
                try
                {
                    int BaseCurrency = 0;
                    decimal convertedAmt = 0;
                    int _invoicePaySchedule = PaymentData[0].C_InvoicePaySchedule_ID;
                    int tableid = MTable.Get_Table_ID("C_InvoicePaySchedule");
                    int chatid = Util.GetValueOfInt(DB.ExecuteScalar("SELECT CM_Chat_ID FROM CM_Chat WHERE AD_Table_ID=" + tableid + " AND Record_ID=" + _invoicePaySchedule, null, null));
                    MChat chat = new MChat(ct, chatid, trx);
                    MInvoicePaySchedule _Oldschedule = new MInvoicePaySchedule(ct, _invoicePaySchedule, trx);
                    MInvoice invoice = new MInvoice(ct, _Oldschedule.GetC_Invoice_ID(), trx);
                    for (int i = 0; i < PaymentData.Length; i++)
                    {
                        MInvoicePaySchedule schedule = new MInvoicePaySchedule(ct, 0, trx);
                        schedule.SetAD_Client_ID(_Oldschedule.GetAD_Client_ID());
                        schedule.SetAD_Org_ID(_Oldschedule.GetAD_Org_ID());
                        schedule.SetC_Invoice_ID(_Oldschedule.GetC_Invoice_ID());
                        schedule.SetC_DocType_ID(_Oldschedule.GetC_DocType_ID());
                        schedule.SetC_Currency_ID(invoice.GetC_Currency_ID());
                        schedule.SetC_BPartner_ID(_Oldschedule.GetC_BPartner_ID());

                        //                        _sql.Clear();
                        //                        _sql.Append(@"SELECT UNIQUE asch.C_Currency_ID FROM c_acctschema asch INNER JOIN ad_clientinfo ci ON ci.c_acctschema1_id = asch.c_acctschema_id
                        //                         INNER JOIN ad_client c ON c.ad_client_id = ci.ad_client_id INNER JOIN c_invoice i ON c.ad_client_id    = i.ad_client_id
                        //                         WHERE i.ad_client_id = " + _Oldschedule.GetAD_Client_ID());
                        //                        int BaseCurrency = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, null));

                        // Get default currency from Context
                        BaseCurrency = ct.GetContextAsInt("$C_Currency_ID");
                        if (BaseCurrency != invoice.GetC_Currency_ID())
                        {
                            //_sql.Clear();
                            //_sql.Append(@"SELECT multiplyrate FROM c_conversion_rate WHERE c_currency_id  = " + invoice.GetC_Currency_ID() +
                            //              " AND c_currency_to_id = " + BaseCurrency + " AND " + GlobalVariable.TO_DATE(invoice.GetDateAcct(), true) + " BETWEEN ValidFrom AND ValidTo");
                            //decimal multiplyRate = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql.ToString(), null, null));
                            //if (multiplyRate == 0)
                            //{
                            //    _sql.Clear();
                            //    _sql.Append(@"SELECT multiplyrate FROM c_conversion_rate WHERE c_currency_id  = " + BaseCurrency +
                            //                  " AND c_currency_to_id = " + invoice.GetC_Currency_ID() + " AND " + GlobalVariable.TO_DATE(invoice.GetDateAcct(), true) + " BETWEEN ValidFrom AND ValidTo");
                            //    multiplyRate = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql.ToString(), null, null));
                            //}
                            //schedule.SetVA009_OpenAmnt(Util.GetValueOfDecimal(PaymentData[i].DueAmt) * multiplyRate);

                            // Get convered Amount from Standard Conversion Method
                            convertedAmt = MConversionRate.Convert(ct, PaymentData[i].DueAmt, invoice.GetC_Currency_ID(), BaseCurrency, invoice.GetDateAcct(), invoice.GetC_ConversionType_ID(),
                                invoice.GetAD_Client_ID(), invoice.GetAD_Org_ID());
                            schedule.SetVA009_OpenAmnt(convertedAmt);
                        }
                        else
                        {
                            schedule.SetVA009_OpenAmnt(Util.GetValueOfDecimal(PaymentData[i].DueAmt));
                        }
                        schedule.SetVA009_BseCurrncy(BaseCurrency);
                        schedule.SetVA009_OpnAmntInvce(Util.GetValueOfDecimal(PaymentData[i].DueAmt));
                        schedule.SetC_PaymentTerm_ID(_Oldschedule.GetC_PaymentTerm_ID());
                        schedule.SetVA009_GrandTotal(_Oldschedule.GetVA009_GrandTotal());
                        schedule.SetVA009_PaymentMethod_ID(_Oldschedule.GetVA009_PaymentMethod_ID());
                        schedule.SetDueDate(PaymentData[i].DueDate);
                        schedule.SetDueAmt(Util.GetValueOfDecimal(PaymentData[i].DueAmt));
                        schedule.SetDiscountDate(PaymentData[i].DueDate);
                        schedule.SetDiscountAmt(0);
                        schedule.SetVA009_PlannedDueDate(PaymentData[i].DueDate);
                        schedule.SetVA009_FollowupDate(PaymentData[i].DueDate);
                        schedule.SetVA009_PaymentMode(_Oldschedule.GetVA009_PaymentMode());
                        schedule.SetVA009_PaymentType(_Oldschedule.GetVA009_PaymentType());
                        schedule.SetVA009_PaymentTrigger(_Oldschedule.GetVA009_PaymentTrigger());
                        schedule.SetVA009_ExecutionStatus(_Oldschedule.GetVA009_ExecutionStatus());
                        //JID_1932_1 payment schedule read only
                        schedule.SetProcessed(true);
                        if (!schedule.Save())
                        {
                            trx.Rollback();
                            Msg = "VA009_SchdNotSaved";
                            ValueNamePair pp = VLogger.RetrieveError();
                            if (pp != null)
                                Msg += pp.GetName();
                            _log.Info(Msg);
                            //Msg = "VA009_SchdNotSaved";
                        }
                        else
                        {
                            if (chatid > 0)
                            {
                                MChat cht = new MChat(ct, 0, trx);
                                chat.CopyTo(cht);
                                cht.SetAD_Client_ID(chat.GetAD_Client_ID());
                                cht.SetAD_Org_ID(chat.GetAD_Org_ID());
                                cht.SetAD_Table_ID(tableid);
                                chat.SetDescription(tableid + "#" + _invoicePaySchedule);
                                cht.SetRecord_ID(schedule.Get_ID());
                                if (!cht.Save())
                                {
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    if (pp != null)
                                        _log.Info(pp.GetName());
                                    //trx.Rollback();
                                }
                                else
                                {
                                    DataSet ds = DB.ExecuteDataset("SELECT CM_ChatEntry_ID FROM  CM_ChatEntry WHERE CM_Chat_ID = " + chatid, null, null);
                                    if (ds != null && ds.Tables[0].Rows.Count > 0)
                                    {
                                        for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                                        {
                                            MChatEntry chEnt = new MChatEntry(ct, Util.GetValueOfInt(ds.Tables[0].Rows[j][0]), trx);
                                            MChatEntry chEntry = new MChatEntry(ct, 0, trx);
                                            chEnt.CopyTo(chEntry);
                                            chEntry.SetAD_Client_ID(chEnt.GetAD_Client_ID());
                                            chEntry.SetAD_Org_ID(chEnt.GetAD_Org_ID());
                                            chEntry.SetCM_Chat_ID(cht.Get_ID());
                                            chEntry.SetCharacterData(chEnt.GetCharacterData());
                                            if (!chEntry.Save())
                                            {
                                                ValueNamePair pp = VLogger.RetrieveError();
                                                if (pp != null)
                                                    _log.Info(pp.GetName());
                                                //trx.Rollback();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (PaymentData.Length > 0)
                    {
                        //MInvoicePaySchedule oldschedule = new MInvoicePaySchedule(ct, _invoicePaySchedule, trx);
                        //oldschedule.Delete(true);
                        int no = DB.ExecuteQuery("DELETE FROM C_InvoicePaySchedule WHERE C_InvoicePaySchedule_ID=" + _invoicePaySchedule, null, trx);
                        invoice.ValidatePaySchedule();
                        if (!invoice.Save())
                        {
                            ValueNamePair pp = VLogger.RetrieveError();
                            if (pp != null)
                                _log.Info(pp.GetName());
                            //trx.Rollback();
                        }
                    }
                }
                catch (Exception e)
                {
                    trx.Rollback();
                    _log.Info(e.Message);
                    ValueNamePair pp = VLogger.RetrieveError();
                    if (pp != null)
                        _log.Info(pp.GetName());
                }
                finally
                {
                    trx.Commit();
                    trx.Close();
                }
            }
            else
            {
                Msg = CreateOrderSplitSchedule(ct, PaymentData, SplitAmmount);
            }
            return Msg;
        }

        /// <summary>
        /// Split Order Schedule
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="PaymentData">List of Schedules Data</param>
        /// <param name="SplitAmmount">Split Amount</param>
        /// <returns>String, Message</returns>
        public string CreateOrderSplitSchedule(Ctx ct, GeneratePaymt[] PaymentData, string SplitAmmount)
        {
            Trx trx = Trx.GetTrx("SplitSchedule_" + DateTime.Now.ToString("yyMMddHHmmssff"));
            string msg = "VA009_SavedSuccessfully";
            try
            {
                StringBuilder _sql = new StringBuilder();
                int BaseCurrency = 0;
                decimal convertedAmt = 0;
                int _orderPaySchedule = PaymentData[0].C_InvoicePaySchedule_ID;
                int tableid = MTable.Get_Table_ID("VA009_OrderPaySchedule");
                int chatid = Util.GetValueOfInt(DB.ExecuteScalar("SELECT CM_Chat_ID FROM CM_Chat WHERE AD_Table_ID=" + tableid + " AND Record_ID=" + _orderPaySchedule, null, null));
                MChat chat = new MChat(ct, chatid, trx);
                MVA009OrderPaySchedule _Oldschedule = new MVA009OrderPaySchedule(ct, _orderPaySchedule, trx);
                MOrder invoice = new MOrder(ct, _Oldschedule.GetC_Order_ID(), trx);
                for (int i = 0; i < PaymentData.Length; i++)
                {
                    MVA009OrderPaySchedule schedule = new MVA009OrderPaySchedule(ct, 0, trx);
                    schedule.SetAD_Client_ID(_Oldschedule.GetAD_Client_ID());
                    schedule.SetAD_Org_ID(_Oldschedule.GetAD_Org_ID());
                    schedule.SetC_BPartner_ID(_Oldschedule.GetC_BPartner_ID());
                    schedule.SetC_Order_ID(_Oldschedule.GetC_Order_ID());
                    schedule.SetC_DocType_ID(_Oldschedule.GetC_DocType_ID());
                    schedule.SetC_PaymentTerm_ID(_Oldschedule.GetC_PaymentTerm_ID());
                    schedule.SetVA009_PaymentMethod_ID(_Oldschedule.GetVA009_PaymentMethod_ID());
                    schedule.SetDueDate(PaymentData[i].DueDate);
                    schedule.SetDueAmt(Util.GetValueOfDecimal(PaymentData[i].DueAmt));
                    schedule.SetDiscountDate(PaymentData[i].DueDate);
                    schedule.SetDiscountAmt(0);
                    schedule.SetVA009_PlannedDueDate(PaymentData[i].DueDate);
                    schedule.SetVA009_FollowupDate(PaymentData[i].DueDate);
                    schedule.SetVA009_PaymentMode(_Oldschedule.GetVA009_PaymentMode());
                    schedule.SetVA009_PaymentType(_Oldschedule.GetVA009_PaymentType());
                    schedule.SetVA009_PaymentTrigger(_Oldschedule.GetVA009_PaymentTrigger());
                    schedule.SetVA009_ExecutionStatus(_Oldschedule.GetVA009_ExecutionStatus());
                    //                    _sql.Clear();
                    //                    _sql.Append(@"SELECT UNIQUE asch.C_Currency_ID FROM c_acctschema asch INNER JOIN ad_clientinfo ci ON ci.c_acctschema1_id = asch.c_acctschema_id
                    //                                 INNER JOIN ad_client c ON c.ad_client_id = ci.ad_client_id INNER JOIN c_invoice i ON c.ad_client_id    = i.ad_client_id
                    //                                 WHERE i.ad_client_id = " + _Oldschedule.GetAD_Client_ID());
                    //                    int BaseCurrency = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, null));

                    // Get default currency from Context
                    BaseCurrency = ct.GetContextAsInt("$C_Currency_ID");
                    if (BaseCurrency != invoice.GetC_Currency_ID())
                    {
                        //_sql.Clear();
                        //_sql.Append(@"SELECT multiplyrate FROM c_conversion_rate WHERE c_currency_id  = " + invoice.GetC_Currency_ID() +
                        //              " AND c_currency_to_id = " + BaseCurrency + " AND " + GlobalVariable.TO_DATE(invoice.GetDateAcct(), true) + " BETWEEN ValidFrom AND ValidTo");
                        //decimal multiplyRate = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql.ToString(), null, null));
                        //if (multiplyRate == 0)
                        //{
                        //    _sql.Clear();
                        //    _sql.Append(@"SELECT multiplyrate FROM c_conversion_rate WHERE c_currency_id  = " + BaseCurrency +
                        //                  " AND c_currency_to_id = " + invoice.GetC_Currency_ID() + " AND " + GlobalVariable.TO_DATE(invoice.GetDateAcct(), true) + " BETWEEN ValidFrom AND ValidTo");
                        //    multiplyRate = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql.ToString(), null, null));
                        //}
                        //schedule.SetVA009_OpenAmnt(Util.GetValueOfDecimal(PaymentData[i].DueAmt) * multiplyRate);

                        // Get convered Amount from Standard Conversion Method
                        convertedAmt = MConversionRate.Convert(ct, PaymentData[i].DueAmt, invoice.GetC_Currency_ID(), BaseCurrency, invoice.GetDateAcct(), invoice.GetC_ConversionType_ID(),
                            invoice.GetAD_Client_ID(), invoice.GetAD_Org_ID());
                        schedule.SetVA009_OpenAmnt(convertedAmt);
                    }
                    else
                    {
                        schedule.SetVA009_OpenAmnt(Util.GetValueOfDecimal(PaymentData[i].DueAmt));
                    }
                    schedule.SetVA009_BseCurrncy(BaseCurrency);
                    schedule.SetC_Currency_ID(invoice.GetC_Currency_ID());
                    schedule.SetVA009_OpnAmntInvce(Util.GetValueOfDecimal(PaymentData[i].DueAmt));
                    schedule.SetVA009_GrandTotal(_Oldschedule.GetVA009_GrandTotal());
                    //JID_1932_1 payment schedule read only
                    schedule.SetProcessed(true);
                    if (!schedule.Save())
                    {
                        trx.Rollback();
                        msg = "VA009_SchdNotSaved";
                        ValueNamePair pp = VLogger.RetrieveError();
                        if (pp != null)
                            msg += pp.GetName();
                        _log.Info(msg);
                    }
                    else
                    {
                        if (chatid > 0)
                        {
                            MChat cht = new MChat(ct, 0, trx);
                            chat.CopyTo(cht);
                            cht.SetAD_Client_ID(chat.GetAD_Client_ID());
                            cht.SetAD_Org_ID(chat.GetAD_Org_ID());
                            cht.SetAD_Table_ID(tableid);
                            chat.SetDescription(tableid + "#" + _orderPaySchedule);
                            cht.SetRecord_ID(schedule.Get_ID());
                            if (!cht.Save())
                            {
                                //trx.Rollback();
                                ValueNamePair pp = VLogger.RetrieveError();
                                if (pp != null)
                                    _log.Info(pp.GetName());
                            }
                            else
                            {
                                DataSet ds = DB.ExecuteDataset("SELECT CM_ChatEntry_ID FROM  CM_ChatEntry WHERE CM_Chat_ID = " + chatid, null, null);
                                if (ds != null && ds.Tables[0].Rows.Count > 0)
                                {
                                    for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                                    {
                                        MChatEntry chEnt = new MChatEntry(ct, Util.GetValueOfInt(ds.Tables[0].Rows[j][0]), trx);
                                        MChatEntry chEntry = new MChatEntry(ct, 0, trx);
                                        chEnt.CopyTo(chEntry);
                                        chEntry.SetAD_Client_ID(chEnt.GetAD_Client_ID());
                                        chEntry.SetAD_Org_ID(chEnt.GetAD_Org_ID());
                                        chEntry.SetCM_Chat_ID(cht.Get_ID());
                                        chEntry.SetCharacterData(chEnt.GetCharacterData());
                                        if (!chEntry.Save())
                                        {
                                            //trx.Rollback();
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                                _log.Info(pp.GetName());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (PaymentData.Length > 0)
                {
                    //MVA009OrderPaySchedule oldschedule = new MVA009OrderPaySchedule(ct, _orderPaySchedule, trx);
                    //oldschedule.Delete(true);
                    int no = DB.ExecuteQuery("DELETE FROM VA009_OrderPaySchedule  WHERE VA009_OrderPaySchedule_ID =" + _orderPaySchedule, null, trx);
                }

            }
            catch (Exception ex)
            {
                trx.Rollback();
                _log.Info(ex.Message);
                ValueNamePair pp = VLogger.RetrieveError();
                if (pp != null)
                    msg = pp.GetName();
                _log.Info(msg);
            }
            finally
            {
                trx.Commit();
                trx.Close();
            }
            return msg;
        }

        public string DueDateSearch(String WhrDueDate)
        {
            #region Commented
            //if (WhrDueDate == "0")
            //    WhrDueDate = " AND to_date(TO_CHAR(TRUNC(cs.duedate)),'dd/mm/yyyy')  =to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy') ";
            //else if (WhrDueDate == "7")
            //    WhrDueDate = "AND to_date(TO_CHAR(TRUNC(cs.duedate)),'dd/mm/yyyy') between to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy') AND to_date(TO_CHAR(TRUNC(sysdate + 7)),'dd/mm/yyyy')";
            //else if (WhrDueDate == "14")
            //    WhrDueDate = "AND to_date(TO_CHAR(TRUNC(cs.duedate)),'dd/mm/yyyy') between to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy') AND to_date(TO_CHAR(TRUNC(sysdate + 14)),'dd/mm/yyyy')";
            //else if (WhrDueDate == "30")
            //    WhrDueDate = "AND to_date(TO_CHAR(TRUNC(cs.duedate)),'dd/mm/yyyy') between to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy') AND to_date(TO_CHAR(TRUNC(sysdate + 30)),'dd/mm/yyyy')";
            //else if (WhrDueDate == "60")
            //    WhrDueDate = "AND to_date(TO_CHAR(TRUNC(cs.duedate)),'dd/mm/yyyy') between to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy') AND to_date(TO_CHAR(TRUNC(sysdate + 60)),'dd/mm/yyyy')";
            //else if (WhrDueDate == "90")
            //    WhrDueDate = "AND to_date(TO_CHAR(TRUNC(cs.duedate)),'dd/mm/yyyy') between to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy') AND to_date(TO_CHAR(TRUNC(sysdate + 90)),'dd/mm/yyyy')";
            //else
            //    WhrDueDate = "";
            #endregion

            if (WhrDueDate != string.Empty)
                WhrDueDate = " AND T.Due_Date_Diff <= " + WhrDueDate;
            else if (WhrDueDate == "99")
                WhrDueDate = string.Empty;
            else
                WhrDueDate = string.Empty;
            return WhrDueDate;
        }

        public string GetLastChat(Ctx ctx, int recordid)
        {
            int tableid = Util.GetValueOfInt(DB.ExecuteScalar("select ad_table_id from ad_table where tablename like ('%C_InvoicePaySchedule%') and export_id='VIS_551'", null, null));
            int chatid = Util.GetValueOfInt(DB.ExecuteScalar("select cm_chat_id from cm_chat  WHERE ad_table_id=" + tableid + " and record_id=" + recordid, null, null));
            string str = @"SELECT CH.characterdata FROM (SELECT * FROM (SELECT CH.cm_chat_id AS ChatID,MAX(CE.cm_chatentry_id)AS EntryID FROM cm_chatentry CE 
                            JOIN cm_chat CH ON (CE.cm_chat_id= CH.cm_chat_id) GROUP BY CH.cm_chat_id ORDER BY entryID)inn1) inn INNER JOIN cm_chatentry CH 
                            ON inn.ChatID= ch.cm_chat_id JOIN cm_chat CMH ON (cmh.cm_chat_id= inn.chatid) JOIN ad_user Au ON au.ad_user_id=CH.createdBy 
                            WHERE CH.createdby=" + ctx.GetAD_User_ID() + " AND ch.cm_chatentry_ID = (SELECT MAX(cm_chatentry_ID)  FROM cm_chatentry   WHERE CM_Chat_ID= " + chatid + ")";
            String LastChat = Util.GetValueOfString(DB.ExecuteScalar(str, null, null));
            return LastChat;
        }

        /// <summary>
        /// Generate Lines on Payment Selection
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="PaymentData">List of Payment Data</param>
        /// <returns>String, Message</returns>
        public string GeneratelinesOnRule(Ctx ctx, GeneratePaymt[] PaymentData)
        {
            bool NextRecord = false;
            int _PaySelectionCheck = 0;
            MPaySelection _PaySelection = null;
            MPaySelectionCheck _PaySelCheck = null;
            decimal PayAmt = 0, DueAmtt = 0;
            string msg = string.Empty;
            Trx trx = Trx.GetTrx("PaymentSelection_" + DateTime.Now.ToString("yyMMddHHmmssff"));
            try
            {
                #region get unique check no
                List<string> lstCheckNo = new List<string>();
                for (int k = 0; k < PaymentData.Length; k++)
                {
                    if (k == 0)
                    {
                        lstCheckNo.Add(PaymentData[k].CheckNumber);
                    }
                    else if (!lstCheckNo.Contains(PaymentData[k].CheckNumber))
                    {
                        lstCheckNo.Add(PaymentData[k].CheckNumber);
                    }
                }
                #endregion

                #region Set Sequence based on customer and check no
                List<GeneratePaymt> recordSequence = new List<GeneratePaymt>();
                for (int m = 0; m < lstCheckNo.Count; m++)
                {
                    for (int n = 0; n < PaymentData.Length; n++)
                    {
                        if (PaymentData[n].CheckNumber == lstCheckNo[m])
                        {
                            recordSequence.Add(new GeneratePaymt
                            {
                                AD_Client_ID = PaymentData[n].AD_Client_ID,
                                AD_Org_ID = PaymentData[n].AD_Org_ID,
                                C_BPartner_ID = PaymentData[n].C_BPartner_ID,
                                C_BankAccount_ID = PaymentData[n].C_BankAccount_ID,
                                C_Currency_ID = PaymentData[n].C_Currency_ID,
                                C_InvoicePaySchedule_ID = PaymentData[n].C_InvoicePaySchedule_ID,
                                C_Invoice_ID = PaymentData[n].C_Invoice_ID,
                                CheckDate = PaymentData[n].CheckDate,
                                CheckNumber = PaymentData[n].CheckNumber,
                                CurrencyType = PaymentData[n].CurrencyType,
                                Discount = PaymentData[n].Discount,
                                DueAmt = PaymentData[n].DueAmt,
                                From = PaymentData[n].From,
                                OverUnder = PaymentData[n].OverUnder,
                                PaymwentBaseType = PaymentData[n].PaymwentBaseType,
                                VA009_RecivedAmt = PaymentData[n].VA009_RecivedAmt,
                                Writeoff = PaymentData[n].Writeoff,
                                convertedAmt = PaymentData[n].convertedAmt,
                                VA009_PaymentMethod_ID = PaymentData[n].VA009_PaymentMethod_ID,
                                DateAcct = PaymentData[n].DateAcct
                            });
                        }
                    }
                }
                #endregion

                for (int i = 0; i < recordSequence.Count; i++)
                {
                    NextRecord = false;
                    if (i == 0)
                    //|| (recordSequence[i].C_BPartner_ID != recordSequence[i - 1].C_BPartner_ID && recordSequence[i].CheckNumber != recordSequence[i - 1].CheckNumber)
                    //|| (recordSequence[i].C_BPartner_ID == recordSequence[i - 1].C_BPartner_ID && recordSequence[i].CheckNumber != recordSequence[i - 1].CheckNumber)
                    //|| (recordSequence[i].C_BPartner_ID != recordSequence[i - 1].C_BPartner_ID && recordSequence[i].CheckNumber == recordSequence[i - 1].CheckNumber))
                    {
                        // Create Header of Pay selection
                        _PaySelection = new MPaySelection(ctx, 0, trx);
                        _PaySelection.SetAD_Client_ID(recordSequence[i].AD_Client_ID);
                        _PaySelection.SetAD_Org_ID(recordSequence[i].AD_Org_ID);
                        _PaySelection.SetC_BankAccount_ID(recordSequence[i].C_BankAccount_ID);
                        _PaySelection.SetPayDate(recordSequence[i].DateAcct);
                        _PaySelection.SetName((recordSequence[i].DateAcct.Value.ToShortDateString()));
                        _PaySelection.SetIsApproved(true);
                        _PaySelection.SetProcessed(true);
                        if (!_PaySelection.Save())
                        {
                            msg = Msg.GetMsg(ctx, "VA009_PaySelectNotSaved");
                            ValueNamePair pp = VLogger.RetrieveError();
                            if (pp != null)
                            {
                                msg += ", " + pp.GetName();
                            }
                            _log.Info(msg);
                            trx.Rollback();
                            break;
                        }
                        else
                        {
                            //payselectionID += _PaySelection.GetC_PaySelection_ID() + " , ";
                            NextRecord = true;

                        }
                    }
                    else if (recordSequence[i].C_BPartner_ID != recordSequence[i - 1].C_BPartner_ID)
                    {
                        NextRecord = true;
                    }

                    // Create Pay selection check based on Pay selection
                    if (NextRecord == true)
                    {
                        _PaySelCheck = new MPaySelectionCheck(ctx, 0, trx);
                        _PaySelCheck.SetAD_Client_ID(recordSequence[i].AD_Client_ID);
                        _PaySelCheck.SetAD_Org_ID(recordSequence[i].AD_Org_ID);
                        _PaySelCheck.SetC_PaySelection_ID(_PaySelection.GetC_PaySelection_ID());
                        _PaySelCheck.SetPaymentRule("S");
                        _PaySelCheck.SetDocumentNo((recordSequence[i].C_Invoice_ID).ToString());
                        _PaySelCheck.SetC_BPartner_ID(recordSequence[i].C_BPartner_ID);

                        _PaySelCheck.Set_Value("VA009_PaymentMethod_ID", recordSequence[i].VA009_PaymentMethod_ID);

                        if (recordSequence[i].VA009_RecivedAmt < 0)
                        {
                            PayAmt = -1 * (recordSequence[i].VA009_RecivedAmt);
                            DueAmtt = decimal.Negate(recordSequence[i].convertedAmt);
                        }
                        else
                        {
                            PayAmt = recordSequence[i].VA009_RecivedAmt;
                            DueAmtt = recordSequence[i].convertedAmt;
                        }
                        _PaySelCheck.SetPayAmt(PayAmt);
                        _PaySelCheck.SetProcessed(true);
                        if (!_PaySelCheck.Save())
                        {
                            msg = Msg.GetMsg(ctx, "VA009_PaySelChkNotSaved");
                            ValueNamePair pp = VLogger.RetrieveError();
                            if (pp != null)
                            {
                                msg += ", " + pp.GetName();
                            }
                            _log.Info(msg);
                            trx.Rollback();
                            break;
                        }
                        else
                        {
                            _PaySelectionCheck = _PaySelCheck.GetC_PaySelectionCheck_ID();
                        }
                    }
                    else
                    {
                        _PaySelCheck = new MPaySelectionCheck(ctx, _PaySelectionCheck, trx);
                        if (recordSequence[i].VA009_RecivedAmt < 0)
                        {
                            PayAmt = -1 * (recordSequence[i].VA009_RecivedAmt);
                            DueAmtt = decimal.Negate(recordSequence[i].convertedAmt);
                        }
                        else
                        {
                            PayAmt = recordSequence[i].VA009_RecivedAmt;
                            DueAmtt = recordSequence[i].convertedAmt;
                        }
                        _PaySelCheck.SetPayAmt((_PaySelCheck.GetPayAmt() + PayAmt));
                        if (!_PaySelCheck.Save())
                        {
                            msg = Msg.GetMsg(ctx, "VA009_PaySelChkNotSaved");
                            ValueNamePair pp = VLogger.RetrieveError();
                            if (pp != null)
                            {
                                msg += ", " + pp.GetName();
                            }
                            _log.Info(msg);
                            trx.Rollback();
                            break;
                        }
                    }

                    // create pay selectionline
                    MPaySelectionLine _SelectionLine = new MPaySelectionLine(ctx, 0, trx);
                    _SelectionLine.SetC_PaySelection_ID(_PaySelection.GetC_PaySelection_ID());
                    _SelectionLine.SetAD_Client_ID(recordSequence[i].AD_Client_ID);
                    _SelectionLine.SetAD_Org_ID(recordSequence[i].AD_Org_ID);
                    _SelectionLine.SetPaymentRule("S");
                    if (_PaySelectionCheck > 0)
                    {
                        _SelectionLine.SetC_PaySelectionCheck_ID(_PaySelectionCheck);
                    }
                    _SelectionLine.SetC_Invoice_ID(recordSequence[i].C_Invoice_ID);
                    _SelectionLine.SetC_InvoicePaySchedule_ID(recordSequence[i].C_InvoicePaySchedule_ID);
                    _SelectionLine.Set_Value("VA009_PaymentMethod_ID", recordSequence[i].VA009_PaymentMethod_ID);
                    if (recordSequence[i].VA009_RecivedAmt < 0)
                    {
                        PayAmt = -1 * (recordSequence[i].VA009_RecivedAmt);
                        DueAmtt = decimal.Negate(recordSequence[i].convertedAmt);
                    }
                    else
                    {
                        PayAmt = recordSequence[i].VA009_RecivedAmt;
                        DueAmtt = recordSequence[i].convertedAmt;
                    }
                    _SelectionLine.SetPayAmt(PayAmt);
                    _SelectionLine.SetOpenAmt(DueAmtt);
                    decimal discount = recordSequence[i].Discount;
                    if (discount < 0)
                    {
                        discount = decimal.Negate(recordSequence[i].Discount);
                    }
                    _SelectionLine.SetDiscountAmt(discount);
                    if (recordSequence[i].OverUnder > 0)
                    {
                        _SelectionLine.SetDifferenceAmt(decimal.Negate(recordSequence[i].OverUnder));
                    }
                    else if (recordSequence[i].Writeoff < 0)
                    {
                        _SelectionLine.SetDifferenceAmt(decimal.Negate(recordSequence[i].Writeoff));
                    }
                    _SelectionLine.SetProcessed(true);
                    _SelectionLine.SetLine(Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT NVL(MAX(Line),0)+10 AS DefaultValue FROM C_PaySelectionLine WHERE C_PaySelection_ID="
                          + _PaySelection.GetC_PaySelection_ID(), null, null)));
                    if (!_SelectionLine.Save())
                    {
                        msg = Msg.GetMsg(ctx, "VA009_PaySelLineNotSaved");
                        ValueNamePair pp = VLogger.RetrieveError();
                        if (pp != null)
                        {
                            msg += ", " + pp.GetName();
                        }
                        _log.Info(msg);
                        trx.Rollback();
                        break;
                    }

                }
            }
            catch (Exception e)
            {
                trx.Rollback();
            }
            finally
            {
                trx.Commit();
                trx.Close();
            }
            #region
            //if (PaymentData.Length > 0)
            //{
            //    for (int i = 0; i < PaymentData.Length; i++)
            //    {
            //        _PaySelection = new MPaySelection(ctx, 0, null);
            //        _PaySelection.SetAD_Client_ID(PaymentData[i].AD_Client_ID);
            //        _PaySelection.SetAD_Org_ID(PaymentData[i].AD_Org_ID);
            //        _PaySelection.SetC_BankAccount_ID(PaymentData[i].C_BankAccount_ID);
            //        _PaySelection.SetPayDate(DateTime.Now);
            //        _PaySelection.SetName((DateTime.Now.ToShortDateString()));
            //        _PaySelection.SetIsApproved(true);
            //        _PaySelection.SetProcessed(true);
            //        if (!_PaySelection.Save())
            //        {
            //        }
            //        else
            //        {
            //            if (_PaySelectionCheck == 0)
            //            {
            //                payselectionID = _PaySelection.GetC_PaySelection_ID().ToString();
            //                MVA009PaymentMethod _paymethod = new MVA009PaymentMethod(ctx, PaymentData[i].VA009_PaymentMethod_ID, null);
            //                 _PaySelCheck = new MPaySelectionCheck(ctx, 0, null);
            //                _PaySelCheck.SetAD_Client_ID(PaymentData[i].AD_Client_ID);
            //                _PaySelCheck.SetAD_Org_ID(PaymentData[i].AD_Org_ID);
            //                _PaySelCheck.SetC_PaySelection_ID(_PaySelection.GetC_PaySelection_ID());
            //                _PaySelCheck.SetPaymentRule("S");
            //                _PaySelCheck.SetDocumentNo((PaymentData[i].C_Invoice_ID).ToString());
            //                _PaySelCheck.SetC_BPartner_ID(PaymentData[i].C_BPartner_ID);
            //                _PaySelCheck.SetPayAmt(PaymentData[i].VA009_RecivedAmt);
            //                _PaySelCheck.SetProcessed(true);
            //                if (!_PaySelCheck.Save())
            //                {
            //                }
            //                else
            //                {
            //                    _PaySelectionCheck = _PaySelCheck.GetC_PaySelectionCheck_ID();
            //                }
            //            }
            //            else
            //            {
            //                 _PaySelCheck = new MPaySelectionCheck(ctx, _PaySelectionCheck, null);
            //                _PaySelCheck.SetPayAmt((_PaySelCheck.GetPayAmt() + PaymentData[i].VA009_RecivedAmt));
            //                if (!_PaySelCheck.Save())
            //                {
            //                }
            //            }
            //            MPaySelectionLine _SelectionLine = new MPaySelectionLine(ctx, 0, null);
            //            _SelectionLine.SetC_PaySelection_ID(_PaySelection.GetC_PaySelection_ID());
            //            _SelectionLine.SetAD_Client_ID(PaymentData[i].AD_Client_ID);
            //            _SelectionLine.SetAD_Org_ID(PaymentData[i].AD_Org_ID);
            //            _SelectionLine.SetPaymentRule("S");
            //            _SelectionLine.SetC_Invoice_ID(PaymentData[i].C_Invoice_ID);
            //            _SelectionLine.SetPayAmt(PaymentData[i].VA009_RecivedAmt);
            //            _SelectionLine.SetC_PaySelectionCheck_ID(_PaySelectionCheck);
            //            _SelectionLine.SetOpenAmt(PaymentData[i].VA009_RecivedAmt);
            //            _SelectionLine.SetProcessed(true);
            //            _SelectionLine.SetLine(Util.GetValueOfInt(DB.ExecuteScalar("SELECT NVL(MAX(Line),0)+10 AS DefaultValue FROM C_PaySelectionLine WHERE C_PaySelection_ID=" + _PaySelection.GetC_PaySelection_ID(), null, null)));
            //            //_SelectionLine.SetDiscountAmt(PaymentData[i].DiscountAmt);
            //            if (!_SelectionLine.Save())
            //            {
            //            }
            //        }
            //    }
            //}
            #endregion
            return msg;
        }

        /// <summary>
        /// To get next check number against Bank and Org.
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="C_BankAccount_ID">Bank Account</param>
        /// <param name="VA009_PaymentMethod_ID">Payment Method</param>
        /// <returns>Check number</returns>
        public string getCheckNo(int C_BankAccount_ID, int VA009_PaymentMethod_ID)
        {
            string checkNo = string.Empty;
            checkNo = Util.GetValueOfString(DB.ExecuteScalar(" SELECT CurrentNext FROM C_BankAccountDoc WHERE C_BankAccount_ID = " + C_BankAccount_ID + " AND IsActive='Y' AND VA009_PaymentMethod_ID = " + VA009_PaymentMethod_ID));
            return checkNo;
        }

        /// <summary>
        /// Get Payment Data
        /// </summary>
        /// <param name="ctx">Current Context</param>
        /// <param name="PaymentData">Payment Data</param>
        /// <param name="BankAccount">C_BankAccount_ID</param>
        /// <param name="CurrencyType">C_ConversionType_ID</param>
        /// <param name="dateAcct">Account Date</param>
        /// <param name="_org_Id">AD_Org_ID</param>
        /// <returns>returns Payment Data which is used to bind on grid</returns>
        public List<PaymentData> ConvertedAmt(Ctx ctx, GeneratePaymt[] PaymentData, int BankAccount, int CurrencyType, int Tocurrency, DateTime? dateAcct, int _org_Id)
        {
            List<PaymentData> _lstChqPay = new List<PaymentData>();
            if (PaymentData.Length > 0)
            {
                decimal convertdamt = 0, discountAmt = 0;
                for (int i = 0; i < PaymentData.Length; i++)
                {
                    PaymentData _payData = new PaymentData();
                    _payData.C_BPartner_ID = PaymentData[i].C_BPartner_ID;
                    _payData.C_BPartner_Location_ID = PaymentData[i].C_BPartner_Location_ID;
                    _payData.C_Bpartner = PaymentData[i].C_Bpartner;
                    _payData.C_Invoice_ID = PaymentData[i].C_Invoice_ID;
                    _payData.C_InvoicePaySchedule_ID = PaymentData[i].C_InvoicePaySchedule_ID;
                    _payData.CurrencyCode = PaymentData[i].CurrencyCode;
                    _payData.C_Currency_ID = PaymentData[i].C_Currency_ID;
                    //Rakesh(VA228):Set conversion type
                    _payData.ConversionTypeId = PaymentData[i].ConversionTypeId;
                    _payData.DiscountDate = PaymentData[i].DiscountDate;
                    _payData.DueAmt = PaymentData[i].DueAmt;
                    _payData.VA009_RecivedAmt = PaymentData[i].VA009_RecivedAmt;
                    _payData.AD_Org_ID = PaymentData[i].AD_Org_ID;
                    _payData.AD_Client_ID = PaymentData[i].AD_Client_ID;
                    _payData.recid = PaymentData[i].C_InvoicePaySchedule_ID;
                    _payData.VA009_PaymentMethod_ID = PaymentData[i].VA009_PaymentMethod_ID;
                    //not required GetConvertedAmt() here bacause below calling this ConvertedAmt
                    //convertdamt = GetConvertedAmt(ctx, PaymentData[i].DueAmt, PaymentData[i].C_Currency_ID, BankAccount, PaymentData[i].AD_Client_ID, PaymentData[i].AD_Org_ID, CurrencyType, dateAcct);
                    //if (convertdamt == 0)
                    //{
                    //    _payData.ERROR = "ConversionNotFound";
                    //}
                    //not required here
                    //_payData.convertedAmt = convertdamt;

                    _payData.TransactionType = PaymentData[i].TransactionType;
                    int documentId = 0;
                    if (PaymentData[i].TransactionType == "Invoice")
                    {
                        MInvoice _inv = new MInvoice(ctx, PaymentData[i].C_Invoice_ID, null);
                        documentId = _inv.GetC_DocType_ID();
                    }
                    else if (PaymentData[i].TransactionType == "Order")
                    {
                        MOrder _order = new MOrder(ctx, PaymentData[i].C_Invoice_ID, null);
                        documentId = _order.GetC_DocType_ID();
                    }

                    MDocType docbasdetype = new MDocType(ctx, documentId, null);
                    _payData.DocBaseType = docbasdetype.GetDocBaseType();
                    if (docbasdetype.GetDocBaseType() == "API" || docbasdetype.GetDocBaseType() == "APC" || docbasdetype.GetDocBaseType() == "POO")
                    {
                        _payData.PaymwentBaseType = "APP";
                    }
                    else if (docbasdetype.GetDocBaseType() == "ARC" || docbasdetype.GetDocBaseType() == "ARI" || docbasdetype.GetDocBaseType() == "SOO")
                    {
                        _payData.PaymwentBaseType = "ARR";
                    }
                    _payData.DiscountAmount = PaymentData[i].DiscountAmount;

                    //change by amit
                    if (BankAccount > 0)
                    {
                        if (docbasdetype.GetDocBaseType() == "ARC" || docbasdetype.GetDocBaseType() == "API")
                        {
                            //get converted amount  as per the  selected currency
                            convertdamt = GetConvertedAmt(ctx, PaymentData[i].DueAmt, PaymentData[i].C_Currency_ID, BankAccount, PaymentData[i].AD_Client_ID, _org_Id, CurrencyType, Tocurrency, dateAcct);
                            convertdamt = convertdamt >= 0 ? convertdamt : -1 * convertdamt;
                            _payData.convertedAmt = convertdamt;

                            //Rakesh(VA228):Check if any discount given
                            if (_payData.DiscountAmount > 0)
                            {
                                //Get Converted discount amount as per the  selected currency
                                discountAmt = GetConvertedAmt(ctx, PaymentData[i].DiscountAmount, PaymentData[i].C_Currency_ID, BankAccount, PaymentData[i].AD_Client_ID, _org_Id, CurrencyType, Tocurrency, dateAcct);
                                _payData.ConvertedDiscountAmount = discountAmt >= 0 ? discountAmt : -1 * discountAmt;
                            }
                        }
                        else
                        {
                            //get converted amount  as per the  selected currency
                            convertdamt = GetConvertedAmt(ctx, PaymentData[i].DueAmt, PaymentData[i].C_Currency_ID, BankAccount, PaymentData[i].AD_Client_ID, _org_Id, CurrencyType, Tocurrency, dateAcct);
                            _payData.convertedAmt = convertdamt;

                            //if any discount given
                            if (_payData.DiscountAmount > 0)
                            {
                                //Get Converted discount amount as per the  selected currency
                                discountAmt = GetConvertedAmt(ctx, PaymentData[i].DiscountAmount, PaymentData[i].C_Currency_ID, BankAccount, PaymentData[i].AD_Client_ID, _org_Id, CurrencyType, Tocurrency, dateAcct);
                                _payData.ConvertedDiscountAmount = discountAmt;
                            }
                        }

                        //If converted Amt is zero then Conversion Not found
                        if (_payData.convertedAmt == 0)
                        {
                            _payData.ERROR = "ConversionNotFound";
                        }
                        //If any discount is applied and converted discount is zero then Conversion Not found
                        if (PaymentData[i].DiscountAmount > 0 && discountAmt == 0)
                        {
                            _payData.ERROR = "ConversionNotFound";
                        }
                    }
                    else
                    {
                        if (docbasdetype.GetDocBaseType() == "ARC" || docbasdetype.GetDocBaseType() == "API")
                        {
                            if (PaymentData[i].DueAmt < 0)
                            {
                                _payData.convertedAmt = -1 * PaymentData[i].DueAmt;//On UI should get amount in positive sign
                            }
                            else
                            {
                                _payData.convertedAmt = PaymentData[i].DueAmt; // -1 Because during payble dont show negative amount on UI
                            }

                            if (PaymentData[i].ConvertedDiscountAmount < 0)
                            {
                                _payData.ConvertedDiscountAmount = -1 * PaymentData[i].ConvertedDiscountAmount;
                            }
                            else
                            {
                                _payData.ConvertedDiscountAmount = PaymentData[i].ConvertedDiscountAmount;
                            }
                        }
                        else
                        {
                            _payData.convertedAmt = PaymentData[i].DueAmt;
                        }
                    }
                    if (docbasdetype.GetDocBaseType() == "ARC" || docbasdetype.GetDocBaseType() == "API")
                    {
                        if (PaymentData[i].DueAmt < 0)
                        {
                            _payData.DueAmt = PaymentData[i].DueAmt;
                        }
                        else
                        {
                            _payData.DueAmt = 1 * PaymentData[i].DueAmt; // -1 Because during payble dont show negative amount on UI
                        }

                        if (PaymentData[i].DiscountAmount < 0)
                        {
                            _payData.DiscountAmount = PaymentData[i].DiscountAmount;
                        }
                        else
                        {
                            _payData.DiscountAmount = 1 * PaymentData[i].DiscountAmount;
                        }
                    }
                    else
                    {
                        _payData.DueAmt = PaymentData[i].DueAmt;
                    }
                    //end
                    _lstChqPay.Add(_payData);
                }
            }
            return _lstChqPay;
        }

        public void GetDocTypeID(Ctx ctx, string docBaseType)
        {
            if (docBaseType == "API" || docBaseType == "APC")
            {
                docBaseType = "APP";
            }
            else if (docBaseType == "ARI" || docBaseType == "ARC")
            {
                docBaseType = "ARR";
            }
        }

        /// <summary>
        /// Get the Converted Amount
        /// </summary>
        /// <param name="ctx">Current Context</param>
        /// <param name="amt">Actual Amount</param>
        /// <param name="fromCurr">C_Currency_ID</param>
        /// <param name="bankaccount">C_BankAccount_ID</param>
        /// <param name="client_id">AD_Client_ID</param>
        /// <param name="org_id">AD_Org_ID</param>
        /// <param name="CurrencyType_ID">C_ConversionType_ID</param>
        /// <param name="dateAcct">DateAcct</param>
        /// <returns>returns Converted Amount</returns>
        public decimal GetConvertedAmt(Ctx ctx, decimal amt, int fromCurr, int bankaccount, int client_id, int org_id, int CurrencyType_ID, int ToCurrency, DateTime? dateAcct)
        {
            if (ToCurrency == 0)
            {
                //MBankAccount _bnkAcct = new MBankAccount(ctx, bankaccount, null);//to get single Value not required to create Object fetch directly from query
                ToCurrency = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_Currency_ID FROM C_BankAccount WHERE IsActive='Y' AND C_BankAccount_ID=" + bankaccount, null, null));
                //decimal comvertedamt = 0;
            }

            //Use User selected Date of Account from the grid not the System Date
            decimal comvertedamt = MConversionRate.Convert(ctx, amt, fromCurr, ToCurrency, dateAcct, CurrencyType_ID, client_id, org_id);
            return comvertedamt;
        }
        //added by amit
        public int GetPaymentCurrency(Ctx ctx, int bankaccount)
        {
            //MBankAccount _bnkAcct = new MBankAccount(ctx, bankaccount, null);
            //to improve the Performance Used DB Query to get C_Currency_ID
            int _bnkAcct = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_Currency_ID FROM C_BankAccount WHERE IsActive='Y' AND C_BankAccount_ID=" + bankaccount));
            return _bnkAcct;
        }
        //end
        public int CreateViewAllocation(Ctx ctx, GeneratePaymt _formData, int _paymentID)
        {
            MPayment pay = new MPayment(ctx, _paymentID, null);
            decimal _amount = 0;
            decimal overUnderAmt = 0;
            MInvoice _inv = new MInvoice(ctx, _formData.C_Invoice_ID, null);
            MAllocationHdr alloc = new MAllocationHdr(ctx, true, pay.GetDateTrx(), pay.GetC_Currency_ID(), ctx.GetContext("#AD_User_Name"), null);
            alloc.SetAD_Org_ID(ctx.GetAD_Org_ID());
            if (!alloc.Save())
            {
            }
            else
            {
                decimal toAllocateAmount = pay.GetPayAmt();
                if (_inv.GetDocBaseType() == "API" || _inv.GetDocBaseType() == "ARI")
                {
                    _amount = Util.GetValueOfDecimal(_formData.convertedAmt);
                }
                else
                {
                    _amount = -1 * Util.GetValueOfDecimal(_formData.convertedAmt);
                }

                if (Math.Abs(toAllocateAmount) - Math.Abs(_amount) > Math.Abs(_amount))
                {
                    toAllocateAmount = toAllocateAmount - _amount;
                    overUnderAmt = 0;
                    MAllocationLine aLine = new MAllocationLine(ctx, 0, null);
                    aLine.SetAmount(_amount);
                    aLine.SetC_AllocationHdr_ID(alloc.GetC_AllocationHdr_ID());
                    aLine.SetDiscountAmt(Env.ZERO);
                    aLine.SetWriteOffAmt(Env.ZERO);
                    aLine.SetOverUnderAmt(Env.ZERO);
                    aLine.SetDateTrx(DateTime.Now);
                    aLine.SetC_BPartner_ID(Util.GetValueOfInt(_formData.C_BPartner_ID));

                    aLine.SetC_Invoice_ID(Util.GetValueOfInt(_formData.C_Invoice_ID));

                    MInvoice inv = new MInvoice(ctx, aLine.GetC_Invoice_ID(), null);
                    if (Util.GetValueOfInt(inv.GetC_Order_ID()) > 0)
                    {
                        aLine.SetC_Order_ID(Util.GetValueOfInt(inv.GetC_Order_ID()));
                    }
                    aLine.SetC_Payment_ID(_paymentID);
                    aLine.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(_formData.C_InvoicePaySchedule_ID));
                    if (!aLine.Save())
                    {
                    }
                }
                else
                {

                    overUnderAmt = Math.Abs(_amount) - Math.Abs(toAllocateAmount);
                    MAllocationLine aLine = new MAllocationLine(ctx, 0, null);
                    aLine.SetAmount(_amount);
                    aLine.SetC_AllocationHdr_ID(alloc.GetC_AllocationHdr_ID());
                    aLine.SetDiscountAmt(Env.ZERO);
                    aLine.SetWriteOffAmt(Env.ZERO);
                    aLine.SetOverUnderAmt(overUnderAmt);
                    aLine.SetDateTrx(DateTime.Now);
                    aLine.SetC_BPartner_ID(Util.GetValueOfInt(_formData.C_BPartner_ID));

                    aLine.SetC_Invoice_ID(Util.GetValueOfInt(_formData.C_Invoice_ID));

                    MInvoice inv = new MInvoice(ctx, aLine.GetC_Invoice_ID(), null);
                    if (Util.GetValueOfInt(inv.GetC_Order_ID()) > 0)
                    {
                        aLine.SetC_Order_ID(Util.GetValueOfInt(inv.GetC_Order_ID()));
                    }
                    aLine.SetC_Payment_ID(_paymentID);
                    aLine.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(_formData.C_InvoicePaySchedule_ID));
                    if (!aLine.Save())
                    {
                    }
                }
                CompleteOrReverse(ctx, alloc.Get_ID(), alloc.Get_Table_ID(), alloc.Get_TableName().ToLower(), DocActionVariables.ACTION_COMPLETE, null);
                if (alloc.Save())
                {
                    return alloc.GetC_AllocationHdr_ID();
                }
                //if (alloc.CompleteIt() == "CO")
                //{
                //    alloc.SetProcessed(true);
                //    alloc.SetDocAction("CL");
                //    alloc.SetDocStatus("CO");
                //    alloc.Save();
                //    return alloc.GetC_AllocationHdr_ID();
                //}
            }
            return 0;
        }

        /// <summary>
        /// Get List of Data with Converted Amonunt
        /// </summary>
        /// <param name="ctx">Current Context</param>
        /// <param name="PaymentData">Payment Data</param>
        /// <param name="CurrencyCashBook">CashBook C_Currency_ID</param>
        /// <param name="CurrencyType">C_ConversionType_ID</param>
        /// <param name="dateAcct">DateAcct</param>
        /// <param name="org_ID">AD_Org_ID</param>
        /// <returns>returns List of Data to do Payment</returns>
        public List<PaymentData> CashBookConvertedAmt(Ctx ctx, GeneratePaymt[] PaymentData, int CurrencyCashBook, int CurrencyType, DateTime? dateAcct, int org_ID)
        {
            List<PaymentData> _lstChqPay = new List<PaymentData>();
            if (PaymentData.Length > 0)
            {
                decimal convrtedamt = 0;

                #region GetDocTypes
                //VA230:Get DocType dataset based on Order or InvoicePaySchedule ids
                DataSet _Ds = null;
                DataRow[] dr = null;
                //Get query to fetch DocBase type
                string query = GetOrderOrInvoiceDocBaseTypeQuery(PaymentData, out List<int> ids, out string where);
                //Get docbasetype data
                _Ds = GetOrderOrInvoiceDocBaseTypeData(ids, query, where);
                if (_Ds == null && _Ds.Tables.Count == 0 && _Ds.Tables[0].Rows.Count == 0)
                {
                    PaymentData _payData = new PaymentData();
                    _payData.ERROR = "VA009_DocTypeNotFound";
                    _lstChqPay.Add(_payData);
                    return _lstChqPay;
                }
                #endregion

                for (int i = 0; i < PaymentData.Length; i++)
                {
                    PaymentData _payData = new PaymentData();
                    _payData.C_BPartner_ID = PaymentData[i].C_BPartner_ID;
                    _payData.C_Bpartner = PaymentData[i].C_Bpartner;
                    _payData.C_Invoice_ID = PaymentData[i].C_Invoice_ID;
                    _payData.C_InvoicePaySchedule_ID = PaymentData[i].C_InvoicePaySchedule_ID;
                    _payData.CurrencyCode = PaymentData[i].CurrencyCode;
                    _payData.C_Currency_ID = PaymentData[i].C_Currency_ID;
                    _payData.VA009_RecivedAmt = PaymentData[i].VA009_RecivedAmt;
                    _payData.AD_Org_ID = PaymentData[i].AD_Org_ID;
                    _payData.AD_Client_ID = PaymentData[i].AD_Client_ID;
                    _payData.recid = PaymentData[i].C_InvoicePaySchedule_ID;
                    _payData.VA009_PaymentMethod_ID = PaymentData[i].VA009_PaymentMethod_ID;
                    //VA230:If transaction type is Order then get order doctype and BP LocationID based on OrderId
                    if (!string.IsNullOrEmpty(PaymentData[i].TransactionType) && PaymentData[i].TransactionType.ToUpper() == "ORDER")
                    {
                        dr = _Ds.Tables[0].Select("C_Order_ID=" + Util.GetValueOfInt(PaymentData[i].C_Invoice_ID));
                    }
                    else
                    {
                        //Get invoice doctype and BP LocationID based on InvoicePayScheduleId
                        dr = _Ds.Tables[0].Select("C_InvoicePaySchedule_ID=" + Util.GetValueOfInt(PaymentData[i].C_InvoicePaySchedule_ID));
                    }
                    if (dr == null && dr.Length == 0)
                    {
                        continue;
                    }
                    if (Util.GetValueOfString(dr[0]["DocBaseType"]) == "API" || Util.GetValueOfString(dr[0]["DocBaseType"]) == "APC" || Util.GetValueOfString(dr[0]["DocBaseType"]) == "POO")
                    {
                        _payData.PaymwentBaseType = "APP";
                    }
                    else if (Util.GetValueOfString(dr[0]["DocBaseType"]) == "ARC" || Util.GetValueOfString(dr[0]["DocBaseType"]) == "ARI" || Util.GetValueOfString(dr[0]["DocBaseType"]) == "SOO")
                    {
                        _payData.PaymwentBaseType = "ARR";
                    }
                    if (CurrencyCashBook > 0)
                    {
                        if (Util.GetValueOfString(dr[0]["DocBaseType"]) == "ARC" || Util.GetValueOfString(dr[0]["DocBaseType"]) == "API")
                        {
                            //modified according to user selected AcctDate and BankAccount Org_ID
                            //convrtedamt = -1 * MConversionRate.Convert(ctx, -1 * PaymentData[i].DueAmt, PaymentData[i].C_Currency_ID, CurrencyCashBook, DateTime.Now, CurrencyType, PaymentData[i].AD_Client_ID, PaymentData[i].AD_Org_ID);
                            convrtedamt = MConversionRate.Convert(ctx, PaymentData[i].DueAmt, PaymentData[i].C_Currency_ID, CurrencyCashBook, dateAcct, CurrencyType, PaymentData[i].AD_Client_ID, org_ID);
                            convrtedamt = convrtedamt >= 0 ? convrtedamt : -1 * convrtedamt; //on UI will get amount in positive sign
                        }
                        else
                        {
                            //modified according to user selected AcctDate and BankAccount Org_ID
                            //convrtedamt = MConversionRate.Convert(ctx, PaymentData[i].DueAmt, PaymentData[i].C_Currency_ID, CurrencyCashBook, DateTime.Now, CurrencyType, PaymentData[i].AD_Client_ID, PaymentData[i].AD_Org_ID);
                            convrtedamt = MConversionRate.Convert(ctx, PaymentData[i].DueAmt, PaymentData[i].C_Currency_ID, CurrencyCashBook, dateAcct, CurrencyType, PaymentData[i].AD_Client_ID, org_ID);
                        }
                        _payData.convertedAmt = convrtedamt;
                        if (_payData.convertedAmt == 0)
                        {
                            _payData.ERROR = "ConversionNotFound";
                        }
                    }
                    if (Util.GetValueOfString(dr[0]["DocBaseType"]) == "ARC" || Util.GetValueOfString(dr[0]["DocBaseType"]) == "API")
                    {
                        if (PaymentData[i].DueAmt < 0)
                        {
                            _payData.DueAmt = -1 * PaymentData[i].DueAmt; //on UI will get amount in positive sign
                        }
                        else
                        {
                            //on UI will get amount in positive sign
                            _payData.DueAmt = PaymentData[i].DueAmt; //-1 Because during payble dont show negative amount on UI
                        }
                    }
                    else
                    {
                        _payData.DueAmt = PaymentData[i].DueAmt;
                    }
                    _payData.TransactionType = PaymentData[i].TransactionType;
                    _lstChqPay.Add(_payData);
                }
            }
            return _lstChqPay;
        }

        public int GetCurrencyType()
        {
            return Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_CONVERSIONTYPE_ID FROM C_CONVERSIONTYPE WHERE VALUE='S'"));
        }

        //Added by Bharat on 01/May/2017
        public string GetCashPayments(string payment_Ids, string order_Ids, Ctx ct)
        {
            string checkData = "";
            if (payment_Ids == "")
            {
                payment_Ids = "0";
            }
            if (order_Ids == "")
            {
                order_Ids = "0";
            }
            string sql = "SELECT pm.va009_paymentbasetype  FROM C_InvoicePaySchedule cs " +
                         " INNER JOIN VA009_PaymentMethod pm ON pm.VA009_PaymentMethod_ID=cs.VA009_PaymentMethod_ID " +
                         " WHERE cs.AD_Client_ID= " + ct.GetAD_Client_ID() + " AND cs.C_InvoicePaySchedule_ID IN (" + payment_Ids +
                         ") AND pm.va009_paymentbasetype != 'S' GROUP BY pm.va009_paymentbasetype" +
                         " UNION " +
                         " SELECT pm.va009_paymentbasetype FROM VA009_OrderPaySchedule cs " +
                         " INNER JOIN VA009_PaymentMethod pm ON pm.VA009_PaymentMethod_ID =cs.VA009_PaymentMethod_ID " +
                         " WHERE cs.AD_Client_ID= " + ct.GetAD_Client_ID() + " AND cs.VA009_OrderPaySchedule_ID IN (" + order_Ids +
                         ") AND pm.va009_paymentbasetype != 'S' GROUP BY pm.va009_paymentbasetype";
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                checkData = "Cash";
            }
            return checkData;
        }

        /// <summary>
        /// Load Bank
        /// </summary>
        /// <param name="orgs">Organization</param>
        /// <param name="ct">Context</param>
        /// <returns>List of Banks</returns>
        public List<Dictionary<string, object>> LoadBank(string orgs, Ctx ct)
        {
            List<Dictionary<string, object>> retDic = null;
            StringBuilder qry = new StringBuilder();
            //REMOVED ORGNIZATION NAME FROM BANK BECAUSE NOW WE ADDED ORG PARAMETER ON FORM
            string sql = @"SELECT DISTINCT bk.C_Bank_ID, bk.Name AS Bank FROM C_BankAccount bc INNER JOIN C_Bank bk 
            ON (bc.C_Bank_ID = bk.C_Bank_ID) WHERE bc.IsActive='Y' AND bk.IsActive='Y' AND bk.IsOwnBank ='Y' ";

            // Check Access of Organization on Bank Account not to Bank
            qry.Append(MRole.GetDefault(ct).AddAccessSQL(sql, "bc", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO));
            if (orgs.Length > 0)
                qry.Append(" AND bc.AD_Org_ID IN (0," + orgs + ")");
            qry.Append(" ORDER BY Bank");
            DataSet ds = DB.ExecuteDataset(qry.ToString());
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<Dictionary<string, object>>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    obj["C_Bank_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[i][0]);
                    obj["Name"] = Util.GetValueOfString(ds.Tables[0].Rows[i][1]);
                    retDic.Add(obj);
                }
            }
            return retDic;
        }

        /// <summary>
        /// Fetch Bank Account Data
        /// </summary>
        /// <param name="bankAccount_ID">Bank Account</param>
        /// <param name="ct">Context</param>
        /// <returns>Bank Account Data </returns>
        public Dictionary<string, object> GetBankAccountData(int bankAccount_ID, Ctx ct)
        {
            Dictionary<string, object> retBank = null;
            //handled the logs
            string sql = @"SELECT ba.CurrentBalance,  bd.CurrentNext FROM C_BankAccount ba LEFT JOIN C_BankAccountDoc bd ON (bd.C_BankAccount_ID = ba.C_BankAccount_ID)
                         WHERE ba.ISACTIVE='Y' AND  ba.C_BankAccount_ID=" + bankAccount_ID + " AND ba.AD_Client_ID =" + ct.GetAD_Client_ID();
            sql = MRole.GetDefault(ct).AddAccessSQL(sql, "C_BankAccount", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retBank = new Dictionary<string, object>();
                retBank["CurrentBalance"] = Util.GetValueOfDecimal(ds.Tables[0].Rows[0][0]);
                retBank["CurrentNext"] = Util.GetValueOfString(ds.Tables[0].Rows[0][1]);
            }
            return retBank;
        }

        /// <summary>
        /// Get CurrentNextCheckNo against selected Payment Method and Bank Account
        /// </summary>
        /// <param name="bankAccount_ID">Bank Account</param>
        /// <param name="payMethod_ID">Payment Method</param>
        /// <param name="ct">Context</param>
        /// <writer>1052</writer>
        /// <returns>CurrentNextCheckNo</returns>
        public int GetBankAccountCheckNo(int bankAccount_ID, int payMethod_ID, Ctx ct)
        {
            //handled the logs
            string sql = @"SELECT bd.CurrentNext FROM C_BankAccount ba INNER JOIN C_BankAccountDoc bd ON (bd.C_BankAccount_ID = ba.C_BankAccount_ID)
             WHERE bd.VA009_PaymentMethod_ID = " + payMethod_ID + "AND ba.ChkNoAutoControl='Y' AND bd.CurrentNext <= bd.EndChkNumber AND bd.IsActive = 'Y'" +
             " AND  bd.C_BankAccount_ID=" + bankAccount_ID + " AND ba.AD_Client_ID =" + ct.GetAD_Client_ID();

            sql = MRole.GetDefault(ct).AddAccessSQL(sql, "C_BankAccount", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            return Util.GetValueOfInt(DB.ExecuteScalar(sql));

        }


        //Added by Bharat on 01/June/2017
        public List<Dictionary<string, object>> LoadOrganization(Ctx ct)
        {
            List<Dictionary<string, object>> retDic = null;
            //added IsCostCenter and IsProfitCenter check  suggested by mukesh sir and Ashish
            string sql = "SELECT AD_Org.AD_Org_ID, AD_Org.Name FROM AD_Org AD_Org WHERE AD_Org.IsActive='Y' AND AD_Org.AD_Org_ID != 0 AND AD_Org.IsSummary='N' AND AD_Org.IsCostCenter='N' AND AD_Org.IsProfitCenter='N' ";
            sql = MRole.GetDefault(ct).AddAccessSQL(sql, "AD_Org", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            sql += " ORDER BY AD_Org.AD_Org_ID";
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<Dictionary<string, object>>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    obj["AD_Org_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[i][0]);
                    obj["Name"] = Util.GetValueOfString(ds.Tables[0].Rows[i][1]);
                    retDic.Add(obj);
                }
            }
            return retDic;
        }

        //Added by Bharat on 01/June/2017
        public List<Dictionary<string, object>> LoadPaymentMethod(Ctx ct)
        {
            List<Dictionary<string, object>> retDic = null;
            string sql = "SELECT VA009_PaymentMethod.VA009_PaymentMethod_ID, VA009_PaymentMethod.VA009_Name, VA009_PaymentMethod.VA009_PaymentBaseType FROM VA009_PaymentMethod VA009_PaymentMethod WHERE VA009_PaymentMethod.IsActive='Y' ";
            sql = MRole.GetDefault(ct).AddAccessSQL(sql, "VA009_PaymentMethod", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            sql += " ORDER BY VA009_PaymentMethod.VA009_PaymentMethod_ID";
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<Dictionary<string, object>>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    obj["VA009_PaymentMethod_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[i][0]);
                    obj["VA009_Name"] = Util.GetValueOfString(ds.Tables[0].Rows[i][1]);
                    obj["VA009_PaymentBaseType"] = Util.GetValueOfString(ds.Tables[0].Rows[i][2]);
                    retDic.Add(obj);
                }
            }
            return retDic;
        }

        /// <summary>
        /// Fetch Payment Methods
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="Org_ID">Organization</param>
        /// <returns>List of Payment Methods</returns>
        public List<Dictionary<string, object>> LoadChequePaymentMethod(Ctx ct, int? Org_ID)
        {
            List<Dictionary<string, object>> retDic = null;

            string sql = "SELECT VA009_PaymentMethod.VA009_PaymentMethod_ID,VA009_PaymentMethod.VA009_Name FROM VA009_PaymentMethod VA009_PaymentMethod WHERE VA009_PaymentMethod.IsActive='Y' AND VA009_PaymentMethod.VA009_PaymentBaseType IN  ('S') ";
            if (Org_ID > 0)
            {
                //Payable case -- get Paymenthod of selected Organization
                sql += " AND VA009_PaymentMethod.AD_Org_ID IN (0," + Org_ID + ") ";
            }
            sql = MRole.GetDefault(ct).AddAccessSQL(sql, "VA009_PaymentMethod", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            sql += " ORDER BY VA009_PaymentMethod_ID";
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<Dictionary<string, object>>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    obj["VA009_PaymentMethod_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[i][0]);
                    obj["VA009_Name"] = Util.GetValueOfString(ds.Tables[0].Rows[i][1]);
                    retDic.Add(obj);
                }
            }
            return retDic;
        }

        //Added by Bharat on 01/June/2017
        public List<Dictionary<string, object>> LoadStatus(Ctx ct)
        {
            List<Dictionary<string, object>> retDic = null;
            string sql = @"SELECT rl.Value,rl.Name FROM AD_Reference re INNER JOIN AD_Ref_List rl ON rl.AD_Reference_ID = re.AD_Reference_ID
                        WHERE re.Name= 'VA009_ExecutionStatus' AND re.Export_ID='VA009_20000279' AND rl.value NOT IN ( 'Y','J') ";
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<Dictionary<string, object>>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    obj["Value"] = Util.GetValueOfString(ds.Tables[0].Rows[i][0]);
                    obj["Name"] = Util.GetValueOfString(ds.Tables[0].Rows[i][1]);
                    retDic.Add(obj);
                }
            }
            return retDic;
        }

        //Added by Bharat on 01/June/2017
        public List<Dictionary<string, object>> loadCurrencyType(Ctx ct)
        {
            List<Dictionary<string, object>> retDic = null;
            string sql = "SELECT C_ConversionType_ID, Name, IsDefault FROM C_ConversionType WHERE ISACTIVE='Y' AND AD_Client_ID IN(0, " + ct.GetAD_Client_ID() + ") ORDER BY AD_Client_ID";
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<Dictionary<string, object>>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    obj["C_ConversionType_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[i][0]);
                    obj["Name"] = Util.GetValueOfString(ds.Tables[0].Rows[i][1]);
                    obj["IsDefault"] = Util.GetValueOfString(ds.Tables[0].Rows[i][2]);
                    retDic.Add(obj);
                }
            }
            return retDic;
        }

        /// <summary>
        /// Load Bank Accounts
        /// </summary>
        /// <param name="c_Bank_ID">Bank</param>
        /// <param name="orgs">Organizations</param>
        /// <param name="ct">Context</param>
        /// <returns>List of Bank Accounts</returns>
        public List<Dictionary<string, object>> LoadBankAccount(int c_Bank_ID, string orgs, Ctx ct)
        {
            if (orgs == null)
                orgs = string.Empty;
            StringBuilder qry = new StringBuilder();
            List<Dictionary<string, object>> retDic = null;
            string sql = "";

            // Show currency Code with Bank Account
            //handled logs
            qry.Append("SELECT acct.C_BankAccount_ID, acct.AccountNo || '_' || cu.Iso_Code AS AccountNo FROM C_BankAccount acct INNER JOIN C_Currency cu ON (acct.C_Currency_ID = cu.C_Currency_ID)");
            if (c_Bank_ID == 0)
            {
                qry.Clear();
                qry.Append(@"SELECT ba.C_BankAccount_ID, b.name  || '_'  || ba.AccountNo || '_' || cu.Iso_Code AS AccountNo FROM C_BankAccount ba INNER JOIN C_Bank B ON (b.C_Bank_ID=ba.C_Bank_ID)
                            INNER JOIN C_Currency cu ON (ba.C_Currency_ID = cu.C_Currency_ID)");
            }

            if (orgs.Length > 0)
            {
                if (c_Bank_ID == 0)
                    qry.Append(" WHERE b.isActive='Y' AND ba.IsActive='Y' AND ba.AD_Client_ID =" + ct.GetAD_Client_ID() + " AND ba.AD_Org_ID IN (0," + orgs + ")");
                else
                    qry.Append(" WHERE acct.IsActive='Y' AND acct.AD_Client_ID =" + ct.GetAD_Client_ID() + "  AND acct.C_Bank_ID =" + c_Bank_ID + " AND acct.AD_Org_ID IN (0," + orgs + ")");
            }
            else if (c_Bank_ID == 0)
            {
                qry.Append(" WHERE b.IsActive='Y' AND ba.IsActive='Y' AND ba.AD_Client_ID =" + ct.GetAD_Client_ID());
            }
            else
            {
                qry.Append(" WHERE acct.IsActive='Y' AND acct.AD_Client_ID =" + ct.GetAD_Client_ID());
                if (c_Bank_ID > 0)
                {
                    qry.Append(" AND acct.C_Bank_ID =" + c_Bank_ID);
                }
            }

            if (c_Bank_ID == 0)
            {
                sql = MRole.GetDefault(ct).AddAccessSQL(qry.ToString(), "ba", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO) + "ORDER BY b.Name";
            }
            else
            {
                sql = MRole.GetDefault(ct).AddAccessSQL(qry.ToString(), "acct", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO) + "ORDER BY acct.AccountNo";
            }

            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<Dictionary<string, object>>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    obj["C_BankAccount_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[i][0]);
                    obj["AccountNo"] = Util.GetValueOfString(ds.Tables[0].Rows[i][1]);
                    retDic.Add(obj);
                }
            }
            return retDic;
        }

        //Added by Bharat on 01/June/2017
        public int GetCurrencyPrecision(int c_BankAccount_ID, string from, Ctx ct)
        {
            string sql = "";
            if (from == "B")
            {
                sql = "SELECT StdPrecision FROM C_Currency WHERE C_Currency_ID = (SELECT C_Currency_ID FROM C_BankAccount WHERE C_BankAccount_ID = " + c_BankAccount_ID + ")";
            }
            else
            {
                sql = "SELECT StdPrecision FROM C_Currency WHERE C_Currency_ID = (SELECT C_Currency_ID FROM C_Cashbook WHERE C_Cashbook_ID = " + c_BankAccount_ID + ")";
            }
            int Precision = Util.GetValueOfInt(DB.ExecuteScalar(sql, null, null));
            return Precision;
        }

        //Added by Bharat on 01/June/2017
        public List<string> GetDocBaseType(string payments, Ctx ct)
        {
            List<string> retDic = null;
            string sql = @"SELECT pm.VA009_PaymentBaseType FROM C_InvoicePaySchedule cs INNER JOIN VA009_PaymentMethod pm ON pm.VA009_PaymentMethod_ID = cs.VA009_PaymentMethod_ID 
                        WHERE cs.AD_Client_ID = " + ct.GetAD_Client_ID() + " AND cs.C_InvoicePaySchedule_ID IN (" + payments
                        + ") AND pm.VA009_PaymentBaseType != 'B' GROUP BY pm.VA009_PaymentBaseType";
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<string>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    string docType = Util.GetValueOfString(ds.Tables[0].Rows[i][0]);
                    retDic.Add(docType);
                }
            }
            return retDic;
        }

        /// <summary>
        /// Load CashBook Data
        /// </summary>
        /// <param name="orgs">Organization</param>
        /// <param name="ct">Context</param>
        /// <returns>List of Cashbook Data</returns>
        public List<Dictionary<string, object>> LoadCashBook(string orgs, Ctx ct)
        {
            List<Dictionary<string, object>> retDic = null;
            StringBuilder qry = new StringBuilder();
            //handled logs
            string sql = @"SELECT cb.C_Cashbook_ID, cb.Name || '_' || cu.ISO_Code AS Name FROM C_CashBook cb INNER JOIN C_Currency cu ON (cb.C_Currency_ID=cu.C_Currency_ID)  
                    WHERE cb.ISACTIVE='Y' AND cb.AD_Client_ID=" + ct.GetAD_Client_ID() + " AND cb.AD_Org_ID = " + Util.GetValueOfInt(orgs) + " ORDER BY cb.Name";
            qry.Append(MRole.GetDefault(ct).AddAccessSQL(sql, "cb", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO));
            //commented code, used directly in above query!
            //if (orgs.Length > 0)
            //{
            //    qry.Append(" AND cb.AD_Org_ID IN (" + orgs + ")");
            //}
            //qry.Append(" ORDER BY cb.Name");
            DataSet ds = DB.ExecuteDataset(qry.ToString());
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<Dictionary<string, object>>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    obj["C_CashBook_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[i][0]);
                    obj["Name"] = Util.GetValueOfString(ds.Tables[0].Rows[i][1]);
                    retDic.Add(obj);
                }
            }
            return retDic;
        }

        //Added by Bharat on 01/June/2017
        public Dictionary<string, object> GetCashBookData(int C_CashBook_ID, Ctx ct)
        {
            Dictionary<string, object> retBank = null;
            //string sql = "SELECT (CompletedBalance || '.00') AS CompletedBalance, C_Currency_ID FROM C_CashBook WHERE ISACTIVE='Y' AND  C_CashBook_ID = "
            //    + C_CashBook_ID + " AND AD_Client_ID =" + ct.GetAD_Client_ID();
            string sql = "SELECT CompletedBalance AS CompletedBalance, C_Currency_ID FROM C_CashBook WHERE ISACTIVE='Y' AND  C_CashBook_ID = "
                + C_CashBook_ID + " AND AD_Client_ID =" + ct.GetAD_Client_ID(); //removed '.00'

            sql = MRole.GetDefault(ct).AddAccessSQL(sql, "C_CashBook", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);

            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retBank = new Dictionary<string, object>();
                retBank["CompletedBalance"] = Util.GetValueOfString(ds.Tables[0].Rows[0][0]);
                retBank["C_Currency_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[0][1]);
            }
            return retBank;
        }

        //Added by Bharat on 01/June/2017
        /// <summary>
        /// To get Payment Menthods
        /// </summary>
        /// <param name="ct">Context Object</param>
        /// <returns>List Of Payment Methods</returns>
        public List<Dictionary<string, object>> LoadBatchPaymentMethod(Ctx ct)
        {
            List<Dictionary<string, object>> retDic = null;
            string sql = @" SELECT VA009_PaymentMethod_ID,VA009_Name,VA009_PaymentBaseType FROM VA009_PaymentMethod WHERE IsActive='Y' AND 
            VA009_PAYMENTMETHOD_ID  IN (SELECT VA009_PAYMENTMETHOD_ID FROM VA009_PAYMENTMETHOD WHERE 
            VA009_PAYMENTBASETYPE !='B' AND COALESCE(VA009_PaymentType , ' ' ) != 'S') AND 
            AD_Client_ID= " + ct.GetAD_Client_ID();
            sql = MRole.GetDefault(ct).AddAccessSQL(sql, "VA009_PaymentMethod", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            sql += " ORDER BY VA009_PaymentMethod_ID";
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<Dictionary<string, object>>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    obj["VA009_PaymentMethod_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[i][0]);
                    obj["VA009_Name"] = Util.GetValueOfString(ds.Tables[0].Rows[i][1]);
                    obj["VA009_PaymentBaseType"] = Util.GetValueOfString(ds.Tables[0].Rows[i][2]);
                    retDic.Add(obj);
                }
            }
            return retDic;
        }

        //Added by Bharat on 01/June/2017
        public string GetPaymentRule(int PaymentMethod_ID, Ctx ct)
        {
            string sql = "SELECT VA009_PaymentRule FROM VA009_PaymentMethod WHERE IsActive='Y' AND VA009_PaymentMethod_ID=" + PaymentMethod_ID;
            sql = MRole.GetDefault(ct).AddAccessSQL(sql, "VA009_PaymentMethod", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            string rule = Util.GetValueOfString(DB.ExecuteScalar(sql, null, null));
            return rule;
        }

        //Added by Bharat on 05/June/2017
        public List<string> GetPaymentBaseType(string sql, Ctx ct)
        {
            List<string> retDic = null;
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<string>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    string paybaseType = Util.GetValueOfString(ds.Tables[0].Rows[i][0]);
                    retDic.Add(paybaseType);
                }
            }
            return retDic;
        }

        //Added by Bharat on 05/June/2017
        public Dictionary<string, object> GetChatID(int recordid, Ctx ct)
        {
            Dictionary<string, object> retChat = null;
            int tableid = Util.GetValueOfInt(DB.ExecuteScalar("SELECT AD_Table_ID FROM AD_Table WHERE TableName LIKE ('%C_InvoicePaySchedule%') AND Export_ID = 'VIS_551'", null, null));
            int chatid = Util.GetValueOfInt(DB.ExecuteScalar("SELECT CM_Chat_ID FROM CM_Chat  WHERE AD_Table_ID=" + tableid + " AND Record_ID= " + recordid, null, null));
            retChat = new Dictionary<string, object>();
            retChat["AD_Table_ID"] = tableid;
            retChat["CM_Chat_ID"] = chatid;
            return retChat;
        }

        //Added by Bharat on 05/June/2017
        public Dictionary<string, object> GetBatchProcess(Ctx ct)
        {
            Dictionary<string, object> retPro = null;
            string sql = "SELECT AD_Process_ID, Name, ClassName, EntityType FROM AD_Process WHERE Value='Create Batch And Lines' AND IsActive='Y' AND Export_ID='VA009_20000253'";
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retPro = new Dictionary<string, object>();
                retPro["AD_Process_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["AD_Process_ID"]);
                retPro["Name"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["Name"]);
                retPro["ClassName"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["ClassName"]);
                retPro["EntityType"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["EntityType"]);
            }
            return retPro;
        }

        //Added by Bharat on 05/June/2017
        public int GetWindowID(string WindowName, Ctx ct)
        {
            int windowid = Util.GetValueOfInt(DB.ExecuteScalar("SELECT AD_Window_ID FROM AD_Window WHERE Name = '" + WindowName + "'"));
            return windowid;
        }

        /// <summary>
        /// Create Payment Manually
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="InvoiceSchdIDS">List of Selected invoice schedules</param>
        /// <param name="OrderSchdIDS">List of Selected order schedules</param>
        /// <param name="BankID">Bank ID</param>
        /// <param name="BankAccountID">Bank Account ID</param>
        /// <param name="PaymentMethodID">Payment Method ID</param>
        /// <param name="DateAcct">Account Date</param>
        /// <param name="CurrencyType">Currency Type</param>
        /// <param name="DateTrx">Date Trx</param>
        /// <param name="AD_Org_ID">Org ID</param>
        /// <returns>String, Message</returns>
        public string CreatePaymentsMannualy(Ctx ct, string InvoiceSchdIDS, string OrderSchdIDS, int BankID, int BankAccountID, int PaymentMethodID, string DateAcct1, string CurrencyType, string DateTrx1, int AD_Org_ID, int docTypeId)
        {
            Trx trx = Trx.GetTrx("Manually_" + DateTime.Now.ToString("yyMMddHHmmssff"));
            string[] invoiceIds = { };
            string[] OrderIds = { };
            StringBuilder ex = new StringBuilder();
            string msg = Msg.GetMsg(ct, "VA009_PaymentCompletedWith");
            StringBuilder docno = new StringBuilder();
            string processMsg = "";
            StringBuilder _conv = new StringBuilder();
            DateTime? DateAcct = (DateAcct1 != null ? Convert.ToDateTime(DateAcct1) : System.DateTime.Now);
            DateTime? DateTrx = (DateTrx1 != null ? Convert.ToDateTime(DateTrx1) : System.DateTime.Now);
            try
            {
                MInvoicePaySchedule _payschedule = null, _payschedule1 = null;
                ViennaAdvantage.Model.MVA009OrderPaySchedule orderPaySchedule = null;
                MDocType _doctype = null;
                MInvoice _invoice = null, _invoice1 = null;
                MOrder _ord = null;
                MCurrency _curr = null;
                MPayment _pay = null;

                int payid = 0;
                List<int> count = new List<int>();
                bool Found = false, Allocate = false;

                int _doctype_ID = 0;
                int c_currencytype = 0;
                decimal _dueAmt = 0;
                string[] _result = null;
                //to set currency type 
                if (CurrencyType != string.Empty)
                {
                    c_currencytype = Util.GetValueOfInt(CurrencyType);
                }
                else
                {
                    c_currencytype = ct.GetContextAsInt("#C_ConversionType_ID");       //GetCurrencyType();
                }

                if (InvoiceSchdIDS != string.Empty && InvoiceSchdIDS != null)
                    invoiceIds = InvoiceSchdIDS.Split(',');
                if (OrderSchdIDS != string.Empty && OrderSchdIDS != null)
                    OrderIds = OrderSchdIDS.Split(',');

                //                StringBuilder sql = new StringBuilder();
                //                sql.Append(@"SELECT bc.C_Bankaccount_id FROM C_Bank cs INNER JOIN C_Bankaccount bc ON cs.C_Bank_ID=bc.C_Bank_ID WHERE cs.IsOwnBank  ='Y' AND
                //                         cs.AD_Client_ID =" + ct.GetAD_Client_ID());
                //                sql.Append(@" ORDER BY bc.C_Bank_ID");
                //                DataSet ds = DB.ExecuteDataset(sql.ToString());
                //                if (ds != null && ds.Tables[0].Rows.Count > 0)
                //                {
                //                    bankacctid = Util.GetValueOfInt(ds.Tables[0].Rows[0]["C_BankAccount_ID"]);
                //                }

                if (invoiceIds.Length > 0)
                {
                    List<int> PartnerID = new List<int>();

                    _payschedule1 = new MInvoicePaySchedule(ct, Util.GetValueOfInt(invoiceIds[0]), trx);
                    _invoice1 = new MInvoice(ct, _payschedule1.GetC_Invoice_ID(), trx);

                    //_doctype = new MDocType(ct, _invoice1.GetC_DocType_ID(), trx);
                    _doctype = MDocType.Get(ct, _invoice1.GetC_DocType_ID());
                    _doctype_ID = docTypeId;
                    //if (_doctype.GetDocBaseType() == "API" || _doctype.GetDocBaseType() == "APC" || _doctype.GetDocBaseType() == "POO")
                    //{
                    //    _doctype_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_DocType_ID FROM C_DocType WHERE DocBaseType='APP' AND IsActive = 'Y' AND AD_Client_ID="
                    //        + _invoice1.GetAD_Client_ID() + " AND AD_Org_ID IN (0, " + _invoice1.GetAD_Org_ID() + ") ORDER BY AD_Org_ID DESC, C_DocType_ID DESC"));
                    //}
                    //else if (_doctype.GetDocBaseType() == "ARC" || _doctype.GetDocBaseType() == "ARI" || _doctype.GetDocBaseType() == "SOO")
                    //{
                    //    _doctype_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_DocType_ID FROM C_DocType WHERE DocBaseType='ARR' AND IsActive = 'Y' AND AD_Client_ID="
                    //        + _invoice1.GetAD_Client_ID() + " AND AD_Org_ID IN (0, " + _invoice1.GetAD_Org_ID() + ") ORDER BY AD_Org_ID DESC, C_DocType_ID DESC"));
                    //}

                    #region Single Invoice Schedule
                    if (invoiceIds.Length == 1)
                    {
                        _payschedule = new MInvoicePaySchedule(ct, Util.GetValueOfInt(invoiceIds[0]), trx);
                        _invoice = new MInvoice(ct, _payschedule.GetC_Invoice_ID(), trx);
                        //_curr = new MCurrency(ct, _payschedule.GetC_Currency_ID(), trx);

                        _pay = new MPayment(ct, 0, trx);
                        _pay.SetAD_Client_ID(Util.GetValueOfInt(_payschedule.GetAD_Client_ID()));
                        _pay.SetAD_Org_ID(Util.GetValueOfInt(AD_Org_ID));
                        _pay.SetC_BankAccount_ID(BankAccountID);
                        //Currency get from the Bank not from the Invoice
                        _pay.SetC_Currency_ID(GetPaymentCurrency(ct, Util.GetValueOfInt(BankAccountID)));
                        //Handled multi-Currency Case
                        if (_pay.GetC_Currency_ID() != _payschedule.GetC_Currency_ID())
                        {
                            _dueAmt = MConversionRate.Convert(ct, _payschedule.GetDueAmt(), _payschedule.GetC_Currency_ID(), _pay.GetC_Currency_ID(), DateAcct, c_currencytype, ct.GetAD_Client_ID(), AD_Org_ID);
                            if (_dueAmt == 0 && _payschedule.GetDueAmt() != 0)
                            {
                                ex.Append(Msg.GetMsg(ct, "NoCurrencyConversion") + ": " + _invoice.GetDocumentNo());
                                _log.Info(ex.ToString());
                            }
                        }
                        else
                        {
                            _dueAmt = _payschedule.GetDueAmt();
                        }
                        if (_doctype.GetDocBaseType().Equals("APC") || _doctype.GetDocBaseType().Equals("ARC"))
                        {
                            _pay.SetPayAmt(-1 * _dueAmt);
                        }
                        else
                        {
                            _pay.SetPayAmt(_dueAmt);
                        }
                        //if (_doctype.GetDocBaseType() == "APC" || _doctype.GetDocBaseType() == "ARC")
                        //{
                        //    _pay.SetPayAmt(-1 * _payschedule.GetDueAmt());
                        //}
                        //else
                        //{
                        //    _pay.SetPayAmt(_payschedule.GetDueAmt());
                        //}

                        _pay.SetC_DocType_ID(_doctype_ID);
                        _pay.SetDateAcct(Util.GetValueOfDateTime(DateAcct));
                        //to set trx date
                        _pay.SetDateTrx(Util.GetValueOfDateTime(DateTrx));
                        //_pay.SetDateTrx(System.DateTime.Now.ToLocalTime());
                        //_pay.SetC_Currency_ID(_payschedule.GetC_Currency_ID());
                        _pay.SetVA009_PaymentMethod_ID(PaymentMethodID);
                        _pay.SetC_Invoice_ID(Util.GetValueOfInt(_payschedule.GetC_Invoice_ID()));
                        _pay.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(_payschedule.GetC_InvoicePaySchedule_ID()));
                        if (c_currencytype != 0)
                            _pay.SetC_ConversionType_ID(c_currencytype);
                        _pay.SetC_BPartner_ID(_payschedule.GetC_BPartner_ID());
                        _pay.SetC_BPartner_Location_ID(_invoice.GetC_BPartner_Location_ID());
                        #region to set bank account of business partner and name on batch line
                        if (_payschedule.GetC_BPartner_ID() > 0)
                        {
                            DataSet ds1 = new DataSet();
                            ds1 = DB.ExecuteDataset(@" SELECT MAX(C_BP_BankAccount_ID) as C_BP_BankAccount_ID,
                                  a_name,RoutingNo,AccountNo FROM C_BP_BankAccount WHERE C_BPartner_ID = " + _payschedule.GetC_BPartner_ID() + " AND "
                                   + " AD_Org_ID =" + Util.GetValueOfInt(AD_Org_ID) + " GROUP BY C_BP_BankAccount_ID, a_name, RoutingNo, AccountNo ");
                            if (ds1.Tables != null && ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                            {
                                //_pay.Set_Value("C_BP_BankAccount_ID", Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                                //_pay.Set_Value("a_name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                                _pay.Set_ValueNoCheck("C_BP_BankAccount_ID", Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                                //if partner bank account is not present then set null because constraint null is on ther payment table and it will not allow to save zero.
                                if (Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]) == 0)
                                    _pay.Set_Value("C_BP_BankAccount_ID", null);
                                _pay.Set_ValueNoCheck("A_Name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                                _pay.Set_ValueNoCheck("RoutingNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["RoutingNo"]));
                                _pay.Set_ValueNoCheck("AccountNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["AccountNo"]));
                            }
                        }
                        #endregion
                        //_dueAmt should not be zero
                        if (_dueAmt != 0)
                        {
                            if (!_pay.Save())
                            {
                                ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                ValueNamePair pp = VLogger.RetrieveError();
                                if (pp != null)
                                {
                                    ex.Append(", " + pp.GetName());
                                }
                                _log.Info(ex.ToString());
                            }
                            else
                            {
                                _result = CompleteOrReverse(ct, _pay.GetC_Payment_ID(), _pay.Get_Table_ID(), _pay.Get_TableName().ToLower(), DocActionVariables.ACTION_COMPLETE, trx);
                                //avoid null exception used _result !=null
                                if (_result != null && _result[1].Equals("Y"))
                                {
                                    docno.Append(_pay.GetDocumentNo());
                                }
                                //if (_pay.CompleteIt() == "CO")
                                //{
                                //    _pay.SetProcessed(true);
                                //    _pay.SetDocAction("CL");
                                //    _pay.SetDocStatus("CO");
                                //    _pay.Save();
                                //    docno.Append(_pay.GetDocumentNo());
                                //}
                                else
                                {
                                    ex.Append(Msg.GetMsg(ct, "VA009_PNotCompelted") + ": " + _pay.GetDocumentNo());
                                    if (_pay.GetProcessMsg() != null && _pay.GetProcessMsg().IndexOf("@") != -1)
                                    {
                                        processMsg = Msg.ParseTranslation(ct, _pay.GetProcessMsg());
                                    }
                                    else
                                    {
                                        processMsg = Msg.GetMsg(ct, _pay.GetProcessMsg());
                                    }
                                    ex.Append(", " + processMsg);
                                    _log.Info(ex.ToString());
                                }
                            }
                        }
                    }
                    #endregion

                    #region Multiple Invoice Schedule
                    else
                    {
                        decimal _discAmt = 0;
                        for (int i = 0; i < invoiceIds.Length; i++)
                        {
                            _payschedule = new MInvoicePaySchedule(ct, Util.GetValueOfInt(invoiceIds[i]), trx);
                            _invoice = new MInvoice(ct, _payschedule.GetC_Invoice_ID(), trx);
                            //_curr = new MCurrency(ct, _payschedule.GetC_Currency_ID(), trx);
                            _doctype = MDocType.Get(ct, _invoice.GetC_DocType_ID());

                            if (PartnerID.Contains(_invoice.GetC_BPartner_ID()))
                            {
                                int indx = PartnerID.IndexOf(_invoice.GetC_BPartner_ID());
                                payid = count[indx];
                                //_pay = new MPayment(ct, count[indx], trx);
                                //_pay.SetC_InvoicePaySchedule_ID(0);
                                //if (c_currencytype != 0)
                                //{
                                //    _pay.SetC_ConversionType_ID(c_currencytype);
                                //}
                                //if (!_pay.Save())
                                //{
                                //    ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                //    ValueNamePair pp = VLogger.RetrieveError();
                                //    if (pp != null)
                                //    {
                                //        ex.Append(", " + pp.GetName());
                                //    }
                                //    _log.Info(ex.ToString());
                                //}

                                MPaymentAllocate M_Allocate = new MPaymentAllocate(ct, 0, trx);
                                M_Allocate.SetAD_Org_ID(AD_Org_ID);
                                M_Allocate.SetAD_Client_ID(_invoice.GetAD_Client_ID());
                                M_Allocate.SetC_Payment_ID(payid);
                                M_Allocate.SetC_Invoice_ID(_invoice.Get_ID());
                                M_Allocate.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(invoiceIds[i]));
                                //Handled multi-Currency Case
                                if (_pay.GetC_Currency_ID() != _payschedule.GetC_Currency_ID())
                                {
                                    _dueAmt = MConversionRate.Convert(ct, _payschedule.GetDueAmt(), _payschedule.GetC_Currency_ID(), _pay.GetC_Currency_ID(), DateAcct, c_currencytype, ct.GetAD_Client_ID(), AD_Org_ID);
                                    _discAmt = MConversionRate.Convert(ct, _payschedule.GetDiscountAmt(), _payschedule.GetC_Currency_ID(), _pay.GetC_Currency_ID(), DateAcct, c_currencytype, ct.GetAD_Client_ID(), AD_Org_ID);
                                    if (_dueAmt == 0 && _payschedule.GetDueAmt() != 0)
                                    {
                                        //trx.Rollback();
                                        if (!string.IsNullOrEmpty(_conv.ToString()))
                                        {
                                            _conv.Append(", " + _invoice.GetDocumentNo() + "_" + _payschedule.GetDueAmt() + "_" + _payschedule.GetDueAmt());
                                        }
                                        else
                                        {
                                            _conv.Append(Msg.GetMsg(ct, "NoCurrencyConversion") + ": " + _invoice.GetDocumentNo() + "_" + _payschedule.GetDueAmt());
                                        }
                                        _log.Info(_conv.ToString());
                                        //Skip the current iteration to avoid to Save Allocate Line
                                        continue;
                                    }
                                }
                                else
                                {
                                    _dueAmt = _payschedule.GetDueAmt();
                                    _discAmt = _payschedule.GetDiscountAmt();
                                }
                                if (_doctype.GetDocBaseType() == "APC" || _doctype.GetDocBaseType() == "API")
                                {
                                    //if (_payschedule.GetDueAmt() < 0)
                                    if (_dueAmt < 0)
                                        M_Allocate.SetAmount(-1 * _dueAmt);
                                    else
                                        M_Allocate.SetAmount(_dueAmt);

                                    //if (_payschedule.GetDiscountAmt() < 0)
                                    if (_discAmt < 0)
                                        M_Allocate.SetDiscountAmt(-1 * _discAmt);
                                    else
                                        M_Allocate.SetDiscountAmt(_discAmt);

                                    if (_doctype.GetDocBaseType() == "APC")
                                    {
                                        // if (PaymentData[i].OverUnder < 0) commented by manjot suggested by puneet and ashish this works same as on window 16/4/19

                                        M_Allocate.SetAmount(-1 * _dueAmt);
                                        M_Allocate.SetDiscountAmt(-1 * _discAmt);
                                    }
                                    else
                                    {
                                        M_Allocate.SetOverUnderAmt(0);
                                    }
                                    M_Allocate.SetWriteOffAmt(0);
                                }
                                else
                                {

                                    M_Allocate.SetAmount(_dueAmt);
                                    M_Allocate.SetDiscountAmt(_discAmt);
                                    M_Allocate.SetOverUnderAmt(0);
                                    M_Allocate.SetWriteOffAmt(0);

                                    if (_doctype.GetDocBaseType() == "ARC")
                                    {
                                        M_Allocate.SetAmount(-1 * _dueAmt);
                                        M_Allocate.SetDiscountAmt(-1 * _discAmt);
                                    }
                                }
                                if (!M_Allocate.Save())
                                {
                                    ex.Append(Msg.GetMsg(ct, "VA009_PALNotSaved"));
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    if (pp != null)
                                    {
                                        ex.Append(", " + pp.GetName());
                                    }
                                    _log.Info(ex.ToString());
                                }
                            }
                            else
                            {
                                for (int j = 0; j < invoiceIds.Length; j++)
                                {
                                    // check if more than one schedule have same business partner then create consolidate payment
                                    if (j == i) { continue; }
                                    else
                                    {
                                        _payschedule1 = new MInvoicePaySchedule(ct, Util.GetValueOfInt(invoiceIds[j]), trx);
                                        _invoice1 = new MInvoice(ct, _payschedule1.GetC_Invoice_ID(), trx);
                                        if (_invoice1.GetC_BPartner_ID() == _invoice.GetC_BPartner_ID())
                                        {
                                            Found = true;
                                            break;
                                        }
                                    }
                                }

                                if (Found == true)
                                {
                                    _pay = new MPayment(ct, 0, trx);
                                    _pay.SetAD_Client_ID(_invoice.GetAD_Client_ID());
                                    _pay.SetAD_Org_ID(AD_Org_ID);
                                    _pay.SetC_DocType_ID(_doctype_ID);
                                    _pay.SetDateAcct(Util.GetValueOfDateTime(DateAcct));
                                    // to set date trx
                                    _pay.SetDateTrx(Util.GetValueOfDateTime(DateTrx));
                                    //_pay.SetDateTrx(System.DateTime.Now.ToLocalTime());
                                    _pay.SetC_ConversionType_ID(c_currencytype);
                                    _pay.SetC_BankAccount_ID(BankAccountID);
                                    _pay.SetC_BPartner_ID(_payschedule.GetC_BPartner_ID());
                                    #region to set bank account of business partner and name on batch line
                                    if (_payschedule.GetC_BPartner_ID() > 0)
                                    {
                                        DataSet ds1 = new DataSet();
                                        ds1 = DB.ExecuteDataset(@" SELECT MAX(C_BP_BankAccount_ID) as C_BP_BankAccount_ID,
                                        a_name,RoutingNo,AccountNo FROM C_BP_BankAccount WHERE C_BPartner_ID = " + _payschedule.GetC_BPartner_ID() + " AND "
                                               + " AD_Org_ID =" + Util.GetValueOfInt(AD_Org_ID) + " GROUP BY C_BP_BankAccount_ID, a_name,RoutingNo, AccountNo  ");
                                        if (ds1.Tables != null && ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                                        {
                                            //_pay.Set_Value("C_BP_BankAccount_ID", Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                                            //_pay.Set_Value("a_name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                                            _pay.Set_ValueNoCheck("C_BP_BankAccount_ID", Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                                            //if partner bank account is not present then set null because constraint null is on ther payment table and it will not allow to save zero.
                                            if (Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]) == 0)
                                                _pay.Set_Value("C_BP_BankAccount_ID", null);
                                            _pay.Set_ValueNoCheck("A_Name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                                            _pay.Set_ValueNoCheck("RoutingNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["RoutingNo"]));
                                            _pay.Set_ValueNoCheck("AccountNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["AccountNo"]));
                                        }
                                    }
                                    #endregion
                                    _pay.SetC_BPartner_Location_ID(_invoice.GetC_BPartner_Location_ID());
                                    _pay.SetC_Currency_ID(GetPaymentCurrency(ct, BankAccountID));
                                    _pay.SetVA009_PaymentMethod_ID(PaymentMethodID);

                                    if (!_pay.Save())
                                    {
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }
                                    else
                                    {
                                        Allocate = true;
                                        payid = _pay.GetC_Payment_ID();
                                        count.Add(payid);
                                        PartnerID.Add(_payschedule.GetC_BPartner_ID());
                                    }
                                    Found = false;
                                }
                                else
                                {
                                    _pay = new MPayment(ct, 0, trx);
                                    _pay.SetAD_Client_ID(_invoice.GetAD_Client_ID());
                                    _pay.SetAD_Org_ID(AD_Org_ID);
                                    _pay.SetC_DocType_ID(_doctype_ID);
                                    _pay.SetDateAcct(Util.GetValueOfDateTime(DateAcct));
                                    //to set trx date
                                    _pay.SetDateTrx(Util.GetValueOfDateTime(DateTrx));
                                    //_pay.SetDateTrx(System.DateTime.Now.ToLocalTime());
                                    //Get the Currency from the BankAccount
                                    _pay.SetC_Currency_ID(GetPaymentCurrency(ct, BankAccountID));
                                    _pay.SetC_ConversionType_ID(c_currencytype);
                                    _pay.SetC_BankAccount_ID(BankAccountID);
                                    _pay.SetC_BPartner_ID(_payschedule.GetC_BPartner_ID());
                                    _pay.SetC_BPartner_Location_ID(_invoice.GetC_BPartner_Location_ID());
                                    #region to set bank account of business partner and name on batch line
                                    if (_payschedule.GetC_BPartner_ID() > 0)
                                    {
                                        DataSet ds1 = new DataSet();
                                        ds1 = DB.ExecuteDataset(@" SELECT MAX(C_BP_BankAccount_ID) as C_BP_BankAccount_ID,
                                  a_name, RoutingNo, AccountNo FROM C_BP_BankAccount WHERE C_BPartner_ID = " + _payschedule.GetC_BPartner_ID() + " AND "
                                               + " AD_Org_ID =" + Util.GetValueOfInt(AD_Org_ID) + " GROUP BY C_BP_BankAccount_ID, a_name, RoutingNo, AccountNo ");
                                        if (ds1.Tables != null && ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                                        {
                                            _pay.Set_ValueNoCheck("C_BP_BankAccount_ID", Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                                            //if partner bank account is not present then set null because constraint null is on ther payment table and it will not allow to save zero.
                                            if (Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]) == 0)
                                                _pay.Set_Value("C_BP_BankAccount_ID", null);
                                            _pay.Set_ValueNoCheck("A_Name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                                            _pay.Set_ValueNoCheck("RoutingNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["RoutingNo"]));
                                            _pay.Set_ValueNoCheck("AccountNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["AccountNo"]));
                                        }
                                    }
                                    #endregion
                                    //Handled multi-Currency Case
                                    if (_pay.GetC_Currency_ID() != _payschedule.GetC_Currency_ID())
                                    {
                                        _dueAmt = MConversionRate.Convert(ct, _payschedule.GetDueAmt(), _payschedule.GetC_Currency_ID(), _pay.GetC_Currency_ID(), DateAcct, c_currencytype, ct.GetAD_Client_ID(), AD_Org_ID);
                                        _discAmt = MConversionRate.Convert(ct, _payschedule.GetDiscountAmt(), _payschedule.GetC_Currency_ID(), _pay.GetC_Currency_ID(), DateAcct, c_currencytype, ct.GetAD_Client_ID(), AD_Org_ID);
                                        if (_dueAmt == 0 && _payschedule.GetDueAmt() != 0)
                                        {
                                            //trx.Rollback();
                                            //ex.Append(", " + Msg.GetMsg(ct, "NoCurrencyConversion"));
                                            if (!string.IsNullOrEmpty(_conv.ToString()))
                                            {
                                                _conv.Append(", " + _invoice.GetDocumentNo() + "_" + _payschedule.GetDueAmt());
                                            }
                                            else
                                            {
                                                _conv.Append(Msg.GetMsg(ct, "NoCurrencyConversion") + ": " + _invoice.GetDocumentNo() + "_" + _payschedule.GetDueAmt());
                                            }
                                            _log.Info(_conv.ToString());
                                        }
                                    }
                                    else
                                    {
                                        _dueAmt = _payschedule.GetDueAmt();
                                        _discAmt = _payschedule.GetDiscountAmt();
                                    }

                                    if (_doctype.GetDocBaseType() == "APC" || _doctype.GetDocBaseType() == "API")
                                    {
                                        if (_dueAmt < 0)
                                            _pay.SetPayAmt(-1 * _dueAmt);
                                        else
                                            _pay.SetPayAmt(_dueAmt);

                                        if (_doctype.GetDocBaseType() == "APC")
                                        {
                                            if (_dueAmt > 0)
                                                _pay.SetPayAmt(-1 * _dueAmt);// -1 Received amount can't be nagative
                                            else
                                                _pay.SetPayAmt(_dueAmt);
                                        }

                                        if (_discAmt < 0)
                                            _pay.SetDiscountAmt(-1 * _discAmt);
                                        else
                                        {
                                            _pay.SetDiscountAmt(_discAmt);
                                        }
                                        _pay.SetOverUnderAmt(0);
                                        _pay.SetWriteOffAmt(0);
                                    }
                                    else
                                    {
                                        if (_doctype.GetDocBaseType() == "ARC")
                                        {
                                            if (_payschedule.GetDueAmt() > 0)
                                                _pay.SetPayAmt(-1 * _dueAmt);
                                            else
                                                _pay.SetPayAmt(_dueAmt);
                                        }
                                        else
                                        {
                                            _pay.SetPayAmt(_dueAmt);
                                            _pay.SetOverUnderAmt(0);
                                        }
                                        _pay.SetDiscountAmt(_discAmt);
                                        _pay.SetWriteOffAmt(0);
                                    }
                                    //change by amit
                                    //_pay.SetC_Currency_ID(GetPaymentCurrency(ct, BankAccountID));
                                    _pay.SetVA009_PaymentMethod_ID(PaymentMethodID);

                                    _pay.SetC_Invoice_ID(_invoice.Get_ID());
                                    _pay.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(invoiceIds[i]));
                                    //dueAmt is Zero mean not found Conversion
                                    if (_dueAmt != 0)
                                    {
                                        if (!_pay.Save())
                                        {
                                            ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                            {
                                                ex.Append(", " + pp.GetName());
                                            }
                                            _log.Info(ex.ToString());
                                        }
                                        else
                                        {
                                            _result = CompleteOrReverse(ct, _pay.GetC_Payment_ID(), _pay.Get_Table_ID(), _pay.Get_TableName().ToLower(), DocActionVariables.ACTION_COMPLETE, trx);
                                            //Used condition with null to avoid null exception
                                            if (_result != null && _result[1].Equals("Y"))
                                            {
                                                if (docno.Length > 0)
                                                {
                                                    docno.Append(", ");
                                                }
                                                docno.Append(_pay.GetDocumentNo());
                                            }
                                            //if (_pay.CompleteIt() == "CO")
                                            //{
                                            //    _pay.SetProcessed(true);
                                            //    _pay.SetDocAction("CL");
                                            //    _pay.SetDocStatus("CO");
                                            //    _pay.Save();

                                            //    if (docno.Length > 0)
                                            //    {
                                            //        docno.Append(", ");
                                            //    }
                                            //    docno.Append(_pay.GetDocumentNo());
                                            //}
                                            else
                                            {
                                                ex.Append(Msg.GetMsg(ct, "VA009_PNotCompelted") + ": " + _pay.GetDocumentNo());
                                                if (_pay.GetProcessMsg() != null && _pay.GetProcessMsg().IndexOf("@") != -1)
                                                {
                                                    processMsg = Msg.ParseTranslation(ct, _pay.GetProcessMsg());
                                                }
                                                else
                                                {
                                                    processMsg = Msg.GetMsg(ct, _pay.GetProcessMsg());
                                                }
                                                ex.Append(", " + processMsg);
                                                _log.Info(ex.ToString());
                                            }
                                        }
                                    }
                                }
                                if (Allocate == true)
                                {
                                    MPaymentAllocate M_Allocate = new MPaymentAllocate(ct, 0, trx);
                                    M_Allocate.SetAD_Org_ID(AD_Org_ID);
                                    M_Allocate.SetAD_Client_ID(_invoice.GetAD_Client_ID());
                                    M_Allocate.SetC_Payment_ID(payid);

                                    M_Allocate.SetC_Invoice_ID(_invoice.GetC_Invoice_ID());
                                    M_Allocate.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(invoiceIds[i]));
                                    //Handled multi-Currency Case
                                    if (_pay.GetC_Currency_ID() != _payschedule.GetC_Currency_ID())
                                    {
                                        _dueAmt = MConversionRate.Convert(ct, _payschedule.GetDueAmt(), _payschedule.GetC_Currency_ID(), _pay.GetC_Currency_ID(), DateAcct, c_currencytype, ct.GetAD_Client_ID(), AD_Org_ID);
                                        _discAmt = MConversionRate.Convert(ct, _payschedule.GetDiscountAmt(), _payschedule.GetC_Currency_ID(), _pay.GetC_Currency_ID(), DateAcct, c_currencytype, ct.GetAD_Client_ID(), AD_Org_ID);
                                        if (_dueAmt == 0 && _payschedule.GetDueAmt() != 0)
                                        {
                                            //ex.Append(", " + Msg.GetMsg(ct, "NoCurrencyConversion"));
                                            if (!string.IsNullOrEmpty(_conv.ToString()))
                                            {
                                                _conv.Append(", " + _invoice.GetDocumentNo() + "_" + _payschedule.GetDueAmt());
                                            }
                                            else
                                            {
                                                _conv.Append(Msg.GetMsg(ct, "NoCurrencyConversion") + ": " + _invoice.GetDocumentNo() + "_" + _payschedule.GetDueAmt());
                                            }
                                            _log.Info(_conv.ToString());
                                        }
                                    }
                                    else
                                    {
                                        _dueAmt = _payschedule.GetDueAmt();
                                        _discAmt = _payschedule.GetDiscountAmt();
                                    }
                                    if (_doctype.GetDocBaseType() == "APC" || _doctype.GetDocBaseType() == "API")
                                    {
                                        if (_dueAmt < 0)
                                            M_Allocate.SetAmount(-1 * _dueAmt);
                                        else
                                            M_Allocate.SetAmount(_dueAmt);

                                        if (_payschedule.GetDiscountAmt() < 0)
                                            M_Allocate.SetDiscountAmt(-1 * _discAmt);
                                        else
                                            M_Allocate.SetDiscountAmt(_discAmt);

                                        if (_doctype.GetDocBaseType() == "APC")
                                        {
                                            M_Allocate.SetAmount(-1 * _dueAmt);
                                            M_Allocate.SetDiscountAmt(-1 * _discAmt);
                                        }
                                        else
                                        {
                                            M_Allocate.SetOverUnderAmt(0);
                                        }
                                        M_Allocate.SetWriteOffAmt(0);
                                    }
                                    else
                                    {
                                        M_Allocate.SetAmount(_dueAmt);
                                        M_Allocate.SetDiscountAmt(_discAmt);
                                        M_Allocate.SetOverUnderAmt(0);
                                        M_Allocate.SetWriteOffAmt(0);

                                        if (_doctype.GetDocBaseType() == "ARC")
                                        {
                                            M_Allocate.SetAmount(-1 * _dueAmt);
                                            M_Allocate.SetDiscountAmt(-1 * _discAmt);
                                        }
                                    }
                                    if (_dueAmt != 0 && !M_Allocate.Save())
                                    {
                                        ex.Append(Msg.GetMsg(ct, "VA009_PALNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }
                                    Allocate = false;
                                }
                            }
                        }
                    }

                    #endregion

                    #region Complete Payments
                    if (count.Count > 0)
                    {
                        for (int j = 0; j < count.Count; j++)
                        {
                            MPayment _PayComp = new MPayment(ct, count[j], trx);
                            if (_PayComp.GetPayAmt() != 0 && Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(C_PaymentAllocate_ID) FROM C_PaymentAllocate WHERE C_Payment_ID=" + _PayComp.GetC_Payment_ID(), null, trx)) > 0)
                            {
                                _result = CompleteOrReverse(ct, _PayComp.GetC_Payment_ID(), _PayComp.Get_Table_ID(), _PayComp.Get_TableName().ToLower(), DocActionVariables.ACTION_COMPLETE, trx);
                                //if (_PayComp.Save())
                                if (_result != null && _result[1].Equals("Y"))
                                {
                                    //string docstatus = _PayComp.CompleteIt();
                                    //if (docstatus == "CO")
                                    //{
                                    //    _PayComp.SetDocStatus("CO");
                                    //    _PayComp.SetDocAction("CL");
                                    //    _PayComp.SetProcessed(true);

                                    //if (!_PayComp.Save())
                                    //if (_result[1].Equals('N'))
                                    //{
                                    //    ex.Append(Msg.GetMsg(ct, "VA009_PNotCompelted") + ": " + _PayComp.GetDocumentNo());
                                    //    ValueNamePair pp = VLogger.RetrieveError();
                                    //    if (pp != null)
                                    //    {
                                    //        ex.Append(", " + pp.GetName());
                                    //    }
                                    //    _log.Info(ex.ToString());
                                    //}
                                    //else
                                    //{
                                    if (docno.Length > 0)
                                    {
                                        docno.Append(", ");
                                    }
                                    docno.Append(_PayComp.GetDocumentNo());
                                    //}
                                }
                                else
                                {
                                    ex.Append(Msg.GetMsg(ct, "VA009_PNotCompelted") + ": " + _PayComp.GetDocumentNo());
                                    if (_PayComp.GetProcessMsg() != null && _PayComp.GetProcessMsg().IndexOf("@") != -1)
                                    {
                                        processMsg = Msg.ParseTranslation(ct, _PayComp.GetProcessMsg());
                                    }
                                    else
                                    {
                                        processMsg = Msg.GetMsg(ct, _PayComp.GetProcessMsg());
                                    }
                                    ex.Append(", " + processMsg);
                                    _log.Info(ex.ToString());
                                }
                            }
                        }
                    }
                    #endregion

                    #region commented
                    //for (int i = 0; i < invoiceIds.Length; i++)
                    //{
                    //    _payschedule = new MInvoicePaySchedule(ct, Util.GetValueOfInt(invoiceIds[i]), trx);
                    //    _invoice = new MInvoice(ct, _payschedule.GetC_Invoice_ID(), trx);
                    //    _curr = new MCurrency(ct, _payschedule.GetC_Currency_ID(), trx);
                    //    _doctype = new MDocType(ct, _invoice.GetC_DocType_ID(), trx);
                    //    _pay = new MPayment(ct, 0, trx);
                    //    _pay.SetAD_Client_ID(Util.GetValueOfInt(_payschedule.GetAD_Client_ID()));
                    //    _pay.SetAD_Org_ID(Util.GetValueOfInt(_payschedule.GetAD_Org_ID()));
                    //    _pay.SetC_BankAccount_ID(BankAccountID);

                    //    if (_doctype.GetDocBaseType() == "API" || _doctype.GetDocBaseType() == "APC" || _doctype.GetDocBaseType() == "POO")
                    //    {
                    //        _doctype_ID = Util.GetValueOfInt(DB.ExecuteScalar(" (SELECT MAX(c_doctype_id) FROM C_DocType WHERE docbasetype='APP' AND IsActive = 'Y' AND ad_client_id =" 
                    //            + ct.GetAD_Client_ID() + " ) "));
                    //        if (_doctype.GetDocBaseType() == "APC")
                    //            _pay.SetPayAmt(-1 * _payschedule.GetDueAmt());
                    //        else
                    //            _pay.SetPayAmt(_payschedule.GetDueAmt());
                    //    }
                    //    else if (_doctype.GetDocBaseType() == "ARC" || _doctype.GetDocBaseType() == "ARI" || _doctype.GetDocBaseType() == "SOO")
                    //    {
                    //        _doctype_ID = Util.GetValueOfInt(DB.ExecuteScalar(" (SELECT MAX(c_doctype_id) FROM C_DocType WHERE docbasetype='ARR' AND IsActive = 'Y' AND ad_client_id =" 
                    //            + ct.GetAD_Client_ID() + " ) "));
                    //        if (_doctype.GetDocBaseType() == "ARC")
                    //            _pay.SetPayAmt(-1 * _payschedule.GetDueAmt());
                    //        else
                    //            _pay.SetPayAmt(_payschedule.GetDueAmt());
                    //    }

                    //    _pay.SetC_DocType_ID(_doctype_ID);
                    //    _pay.SetDateAcct(System.DateTime.Now);
                    //    _pay.SetDateTrx(System.DateTime.Now);
                    //    _pay.SetC_Currency_ID(_curr.GetC_Currency_ID());
                    //    _pay.SetVA009_PaymentMethod_ID(PaymentMethodID);
                    //    _pay.SetC_Invoice_ID(Util.GetValueOfInt(_payschedule.GetC_Invoice_ID()));
                    //    _pay.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(_payschedule.GetC_InvoicePaySchedule_ID()));
                    //    if (c_currencytype != 0)
                    //        _pay.SetC_ConversionType_ID(c_currencytype);
                    //    _pay.SetC_BPartner_ID(Util.GetValueOfInt(_payschedule.GetC_BPartner_ID()));
                    //    _pay.SetC_BPartner_Location_ID(_invoice.GetC_BPartner_Location_ID());

                    //    if (!_pay.Save())
                    //    {
                    //        //trx.Rollback();
                    //        //ex = Msg.GetMsg(ct, "VA009_PNotSaved");
                    //        ValueNamePair pp = VLogger.RetrieveError();
                    //        if (ex.Length == 0)
                    //        {
                    //            ex.Append(Msg.GetMsg(ct, "VA009_NotSaved"));
                    //        }
                    //        if (pp != null)
                    //            ex.Append(" ," + pp.GetName());
                    //        _log.Info(ex.ToString());
                    //    }
                    //    else
                    //    {
                    //        if (_pay.CompleteIt() == "CO")
                    //        {
                    //            _pay.SetProcessed(true);
                    //            _pay.SetDocAction("CL");
                    //            _pay.SetDocStatus("CO");
                    //            _pay.Save();
                    //            if (msg.Length == 0)
                    //            {
                    //                msg = Msg.GetMsg(ct, "VA009_SavedSuccessfully");
                    //            }
                    //            msg += " ," + _pay.GetDocumentNo();
                    //        }
                    //        else
                    //        {
                    //            //trx.Rollback();
                    //            //ex = Msg.GetMsg(ct, "VA009_PaymentNotProcessed");
                    //            ValueNamePair pp = VLogger.RetrieveError();
                    //            if (ex.Length == 0)
                    //            {
                    //                ex.Append(Msg.GetMsg(ct, "VA009_PNotCompelted"));
                    //            }
                    //            if (pp != null)
                    //                ex.Append(" ," + pp.GetName());
                    //            _log.Info(ex.ToString());
                    //        }
                    //    }
                    //}
                    #endregion
                }

                #region Order Schedules
                if (OrderIds.Length > 0)
                {
                    for (int i = 0; i < OrderIds.Length; i++)
                    {
                        orderPaySchedule = new ViennaAdvantage.Model.MVA009OrderPaySchedule(ct, Util.GetValueOfInt(OrderIds[i]), trx);
                        _ord = new MOrder(ct, orderPaySchedule.GetC_Order_ID(), trx);
                        _curr = new MCurrency(ct, orderPaySchedule.GetC_Currency_ID(), trx);
                        //_doctype = new MDocType(ct, _ord.GetC_DocType_ID(), trx);
                        // _doctype = MDocType.Get(ct, _ord.GetC_DocType_ID());

                        //Bug177 Get Doctype 
                        if (!_ord.IsSOTrx())
                        {
                            //Ap Payment
                            _doctype_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_DocType_ID FROM C_DocType WHERE DocBaseType='APP' AND IsActive = 'Y' AND AD_Client_ID="
                                + _ord.GetAD_Client_ID() + " AND AD_Org_ID IN (0, " + AD_Org_ID + ") ORDER BY AD_Org_ID DESC, C_DocType_ID DESC"));
                        }
                        else
                        {
                            //Ar Receipt
                            _doctype_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_DocType_ID FROM C_DocType WHERE DocBaseType='ARR' AND IsActive = 'Y' AND AD_Client_ID="
                                + _ord.GetAD_Client_ID() + " AND AD_Org_ID IN (0, " + AD_Org_ID + ") ORDER BY AD_Org_ID DESC, C_DocType_ID DESC"));
                        }
                        _pay = new MPayment(ct, 0, trx);
                        _pay.SetAD_Client_ID(Util.GetValueOfInt(orderPaySchedule.GetAD_Client_ID()));
                        _pay.SetAD_Org_ID(Util.GetValueOfInt(AD_Org_ID));
                        _pay.SetC_DocType_ID(_doctype_ID);
                        _pay.SetDateAcct(Util.GetValueOfDateTime(DateAcct));
                        //to set trx date 
                        _pay.SetDateTrx(Util.GetValueOfDateTime(DateTrx));
                        //_pay.SetDateTrx(System.DateTime.Now.ToLocalTime());
                        //if (bankacctid != 0)
                        _pay.SetC_BankAccount_ID(BankAccountID);
                        //_pay.SetC_Currency_ID(_curr.GetC_Currency_ID());
                        //Get the Currency from BankAccount
                        _pay.SetC_Currency_ID(GetPaymentCurrency(ct, BankAccountID));
                        _pay.SetVA009_PaymentMethod_ID(PaymentMethodID);
                        _pay.SetC_Order_ID(Util.GetValueOfInt(orderPaySchedule.GetC_Order_ID()));
                        _pay.SetVA009_OrderPaySchedule_ID(Util.GetValueOfInt(orderPaySchedule.GetVA009_OrderPaySchedule_ID()));
                        //handled the Currency Conversion
                        if (_pay.GetC_Currency_ID() != orderPaySchedule.GetC_Currency_ID())
                        {
                            _dueAmt = MConversionRate.Convert(ct, orderPaySchedule.GetDueAmt(), orderPaySchedule.GetC_Currency_ID(), _pay.GetC_Currency_ID(), DateAcct, c_currencytype, ct.GetAD_Client_ID(), AD_Org_ID);
                            if (_dueAmt == 0 && orderPaySchedule.GetDueAmt() != 0)
                            {
                                //trx.Rollback();
                                //ex.Append(Msg.GetMsg(ct, "NoCurrencyConversion"));
                                if (!string.IsNullOrEmpty(_conv.ToString()))
                                {
                                    _conv.Append(", " + _invoice.GetDocumentNo() + "_" + _payschedule.GetDueAmt());
                                }
                                else
                                {
                                    _conv.Append(Msg.GetMsg(ct, "NoCurrencyConversion") + ": " + _invoice.GetDocumentNo() + "_" + _payschedule.GetDueAmt());
                                }
                                _log.Info(_conv.ToString());
                            }
                        }
                        else
                        {
                            _dueAmt = orderPaySchedule.GetDueAmt();
                        }
                        //_pay.SetPayAmt(orderPaySchedule.GetDueAmt());
                        _pay.SetPayAmt(_dueAmt);
                        if (c_currencytype != 0)
                            _pay.SetC_ConversionType_ID(c_currencytype);
                        _pay.SetC_BPartner_ID(Util.GetValueOfInt(orderPaySchedule.GetC_BPartner_ID()));
                        #region to set bank account of business partner and name on batch line
                        if (orderPaySchedule.GetC_BPartner_ID() > 0)
                        {
                            DataSet ds1 = new DataSet();
                            ds1 = DB.ExecuteDataset(@" SELECT MAX(C_BP_BankAccount_ID) as C_BP_BankAccount_ID,
                                  a_name,RoutingNo, AccountNo FROM C_BP_BankAccount WHERE C_BPartner_ID = " + orderPaySchedule.GetC_BPartner_ID() + " AND "
                                   + " AD_Org_ID =" + Util.GetValueOfInt(AD_Org_ID) + " GROUP BY C_BP_BankAccount_ID, a_name,RoutingNo,AccountNo ");
                            if (ds1.Tables != null && ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                            {
                                _pay.Set_ValueNoCheck("C_BP_BankAccount_ID", Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                                //if partner bank account is not present then set null because constraint null is on ther payment table and it will not allow to save zero.
                                if (Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]) == 0)
                                    _pay.Set_Value("C_BP_BankAccount_ID", null);
                                _pay.Set_ValueNoCheck("A_Name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                                _pay.Set_ValueNoCheck("RoutingNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["RoutingNo"]));
                                _pay.Set_ValueNoCheck("AccountNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["AccountNo"]));
                            }
                        }
                        #endregion
                        _pay.SetC_BPartner_Location_ID(_ord.GetC_BPartner_Location_ID());
                        if (_dueAmt != 0)
                        {
                            if (!_pay.Save())
                            {
                                //trx.Rollback();
                                //ex = Msg.GetMsg(ct, "VA009_PNotSaved");
                                ValueNamePair pp = VLogger.RetrieveError();
                                if (ex.Length == 0)
                                {
                                    ex.Append(Msg.GetMsg(ct, "VA009_NotSaved"));
                                }
                                if (pp != null)
                                    ex.Append(", " + pp.GetName());
                            }
                            else
                            {
                                _result = CompleteOrReverse(ct, _pay.GetC_Payment_ID(), _pay.Get_Table_ID(), _pay.Get_TableName().ToLower(), DocActionVariables.ACTION_COMPLETE, trx);
                                //if (_pay.Save())
                                //used not null condtion to avoid null exception
                                if (_result != null && _result[1].Equals("Y"))
                                {
                                    if (docno.Length > 0)
                                    {
                                        docno.Append(" ,");
                                    }
                                    docno.Append(_pay.GetDocumentNo());
                                }
                                //if (_pay.CompleteIt() == "CO")
                                //{
                                //    _pay.SetProcessed(true);
                                //    _pay.SetDocAction("CL");
                                //    _pay.SetDocStatus("CO");
                                //    _pay.Save();
                                //    if (docno.Length > 0)
                                //    {
                                //        docno.Append(" ,");
                                //    }
                                //    docno.Append(_pay.GetDocumentNo());
                                //}
                                else
                                {
                                    //ex.Append("\n" + Msg.GetMsg(ct, "VA009_PNotCompelted") + ": " + _pay.GetDocumentNo());
                                    ex.Append("\n" + _result[0] + ": " + _pay.GetDocumentNo());
                                    if (_pay.GetProcessMsg() != null && _pay.GetProcessMsg().IndexOf("@") != -1)
                                    {
                                        processMsg = Msg.ParseTranslation(ct, _pay.GetProcessMsg());
                                    }
                                    else
                                    {
                                        processMsg = Msg.GetMsg(ct, _pay.GetProcessMsg());
                                    }
                                    ex.Append(", " + processMsg);
                                    _log.Info(ex.ToString());
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                _log.Info(e.Message);
                ValueNamePair pp = VLogger.RetrieveError();
                if (pp != null)
                    _log.Info(pp.GetName());
            }
            finally
            {
                trx.Commit();
                trx.Close();
            }
            if (docno.Length > 0)
            {
                msg += docno.ToString();
                ex.Append("\n" + msg);
            }
            //If Conversion not found then it will return message those records
            if (!string.IsNullOrEmpty(_conv.ToString()))
            {
                if (!string.IsNullOrEmpty(ex.ToString()))
                {
                    ex.Append(" AND " + _conv.ToString());
                }
                else
                {
                    ex.Append("\n" + _conv.ToString());
                }
            }
            return ex.ToString();
        }

        public List<PayBatchDetails> GetPayScheduleBatch(Ctx ctx)
        {
            List<PayBatchDetails> lst = new List<PayBatchDetails>();
            //Table name must Camel format
            string sql = "SELECT b.DocumentNo,  b.VA009_DocumentDate,  bn.name AS bankName,  bna.ACCOUNTNO AS bankaccount,  b.c_bank_id,  b.c_bankaccount_id,  pm.VA009_NAME AS PaymentMethod,  b.va009_paymentmethod_id,"
                        + "c.c_currency_id,  c.ISO_CODE,  bd.VA009_ConvertedAmt FROM VA009_Batch b INNER JOIN C_Bank bn ON bn.c_bank_id=b.c_bank_id INNER JOIN C_BankAccount bna ON bna.c_bankaccount_id=b.c_bankaccount_id"
                        + " INNER JOIN VA009_BatchLines bl ON bl.VA009_batch_id=b.va009_batch_id INNER JOIN VA009_BatchLineDetails bd ON bd.va009_batchlines_id=bl.va009_batchlines_id INNER JOIN C_Currency c "
                        + "ON c.c_Currency_id=bd.C_Currency_ID INNER JOIN VA009_PaymentMethod pm ON pm.va009_paymentmethod_id= b.va009_paymentmethod_id";

            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "b", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);

            DataSet ds = DB.ExecuteDataset(sql.ToString());
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    PayBatchDetails obj = new PayBatchDetails();
                    obj.DocumentNo = Util.GetValueOfString(ds.Tables[0].Rows[i]["DocumentNo"]);
                    obj.VA009_DocumentDate = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA009_DocumentDate"]);
                    obj.bankName = Util.GetValueOfString(ds.Tables[0].Rows[i]["bankName"]);
                    obj.bankaccount = Util.GetValueOfString(ds.Tables[0].Rows[i]["bankaccount"]);
                    obj.c_bank_id = Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bank_id"]);
                    obj.c_bankaccount_id = Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bankaccount_id"]);
                    obj.va009_paymentmethod_id = Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_paymentmethod_id"]);
                    obj.PaymentMethod = Util.GetValueOfString(ds.Tables[0].Rows[i]["PaymentMethod"]);
                    obj.c_currency_id = Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]);
                    obj.ISO_CODE = Util.GetValueOfString(ds.Tables[0].Rows[i]["ISO_CODE"]);
                    obj.VA009_ConvertedAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]);
                    obj.BaseAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]);
                    lst.Add(obj);
                }
            }
            return lst
                ;
        }

        public List<Dictionary<string, object>> LoadCurrencies(Ctx ct)
        {
            List<Dictionary<string, object>> retDic = null;
            string sql = "SELECT C_Currency_ID, ISO_Code FROM C_Currency WHERE IsActive='Y' AND IsMyCurrency='Y' ";
            sql = MRole.GetDefault(ct).AddAccessSQL(sql, "C_Currency", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            sql += " ORDER BY C_Currency_ID";
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<Dictionary<string, object>>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    obj["C_Currency_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[i][0]);
                    obj["ISO_Code"] = Util.GetValueOfString(ds.Tables[0].Rows[i][1]);
                    retDic.Add(obj);
                }
            }
            return retDic;
        }

        /// <summary>
        /// Create Payment against Bank to Bank Transfer
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="PaymentData">Payment Data</param>
        /// <returns>String, Message</returns>
        public Dictionary<string, object> CreatePaymentsBtoB(Ctx ct, dynamic PaymentData)
        {
            Trx trx = Trx.GetTrx("PaymentB2B_" + DateTime.Now.ToString("yyMMddHHmmssff"));
            //1052 - to store custom success and error message  
            Dictionary<string, object> retDic = new Dictionary<string, object>();
            StringBuilder ex = new StringBuilder();
            bool isReceipt = Util.GetValueOfBool(PaymentData.isReceipt);
            bool isPayment = Util.GetValueOfBool(PaymentData.isPayment);
            int C_doctype_ID = 0;
            string[] _result = null;
            //bool isAllocationSaved = false;
            try
            {
                if (PaymentData != null)
                {
                    DataSet ds = new DataSet();
                    // string documentNo = ""; not used 
                    //MAllocationHdr alloc = null;
                    for (int i = 0; i <= 1; i++)
                    {
                        MPayment _pay = new MPayment(ct, 0, trx);
                        _pay.SetAD_Client_ID(Util.GetValueOfInt(PaymentData.clientID));
                        _pay.SetAD_Org_ID(Util.GetValueOfInt(PaymentData.OrgID));

                        //Rakesh(VA228):Set value from properties on date 13/Sept/2021
                        if (isReceipt && i == 0) // For Receipt and making first Payment AR Receipt
                        {
                            C_doctype_ID = Util.GetValueOfInt(PaymentData.ARDocumentTypeId);
                            _pay.SetC_BankAccount_ID(Util.GetValueOfInt(PaymentData.toBank));
                            //Commented code because we don't need to set Business Partner and Business Partner Location while creating Payment and they both are UI mandatory on window
                        }
                        else if (isReceipt && i == 1)// For Payment and making Second Payment AR Receipt
                        {
                            C_doctype_ID = Util.GetValueOfInt(PaymentData.APDocumentTypeId);
                            _pay.SetC_BankAccount_ID(Util.GetValueOfInt(PaymentData.fromBank));
                        }
                        if (isPayment && i == 0) // For Payment and making first Payment AP Payment
                        {
                            C_doctype_ID = Util.GetValueOfInt(PaymentData.APDocumentTypeId);
                            _pay.SetC_BankAccount_ID(Util.GetValueOfInt(PaymentData.fromBank));
                        }
                        else if (isPayment && i == 1)// For Receipt and making Second Payment AP Payment
                        {
                            C_doctype_ID = Util.GetValueOfInt(PaymentData.ARDocumentTypeId);
                            _pay.SetC_BankAccount_ID(Util.GetValueOfInt(PaymentData.toBank));
                        }

                        _pay.SetC_DocType_ID(C_doctype_ID);
                        _pay.SetDateAcct(Util.GetValueOfDateTime(PaymentData.acctDate));
                        _pay.SetDateTrx(Util.GetValueOfDateTime(PaymentData.transDate));
                        _pay.SetC_ConversionType_ID(Util.GetValueOfInt(PaymentData.currencyType));
                        _pay.SetC_Currency_ID(Util.GetValueOfInt(PaymentData.currencyID));
                        _pay.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(PaymentData.paymentMethod));
                        if (PaymentData.PayBase == MVA009PaymentMethod.VA009_PAYMENTBASETYPE_Check)
                        {
                            _pay.SetCheckNo(PaymentData.CheckNo);
                            _pay.SetCheckDate(Util.GetValueOfDateTime(PaymentData.CheckDate));
                            //VA230:Set overrideautocheck
                            _pay.SetIsOverrideAutoCheck(Util.GetValueOfBool(PaymentData.IsOverrideAutoCheck));
                        }
                        _pay.SetPayAmt(Util.GetValueOfDecimal(PaymentData.amount));
                        _pay.SetDocStatus("DR");
                        //Rakesh(VA228):Check contra column exists and set value true
                        if (_pay.Get_ColumnIndex("VA009_IsContra") >= 0)
                        {
                            _pay.Set_Value("VA009_IsContra", true);
                        }
                        if (!_pay.Save())
                        {
                            trx.Rollback();
                            ValueNamePair pp = VLogger.RetrieveError();
                            ex.Clear();
                            if (pp != null)
                                ex.Append(Msg.GetMsg(ct, "VA009_PymentNotSaved") + ", " + pp.GetName());
                            _log.Info(ex.ToString());
                            retDic["error"] = ex.ToString();
                            break;
                        }
                        else
                        {
                            //based on result get from CompleteOrReverse function should execute the condition
                            _result = CompleteOrReverse(ct, _pay.GetC_Payment_ID(), _pay.Get_Table_ID(), _pay.Get_TableName().ToLower(), DocActionVariables.ACTION_COMPLETE, trx);
                            //if (_pay.Save())
                            //'Y' Indicates the record is Completed Successfully
                            if (_result != null && _result[1].Equals("Y"))
                            {
                                //if (_pay.CompleteIt() == "CO")
                                //{
                                //_pay.SetProcessed(true);
                                //_pay.SetDocAction("CL");
                                //_pay.SetDocStatus("CO");
                                //_pay.Save();
                                // no need to create allocation in case of bank to bank transfer as discussed with ashish and gagan
                                if (i == 0)
                                {
                                    //VA230:Commented not required as CurrentNext get updated while completing payment in MClass MPayment
                                    // Updated checkno as discussed with ashish and gagan we need to update the series from Bank to bank transfer
                                    //if (Util.GetValueOfInt(PaymentData.paymentMethod) > 0)
                                    //{
                                    //    //string checkno = getCheckNo(Util.GetValueOfInt(PaymentData.fromBank), Util.GetValueOfInt(PaymentData.paymentMethod));
                                    //    if (DB.ExecuteScalar("SELECT va009_paymentbasetype FROM VA009_PaymentMethod WHERE VA009_PaymentMethod_ID = " + Util.GetValueOfInt(PaymentData.paymentMethod)) == "S")
                                    //    {
                                    //        if (DB.ExecuteScalar(" UPDATE C_BankAccountDoc SET CurrentNext = " + (Util.GetValueOfInt(PaymentData.CheckNo) + 1) + " WHERE C_BankAccount_ID = " + Util.GetValueOfInt(PaymentData.fromBank) + " AND IsActive='Y' AND VA009_PaymentMethod_ID = " + Util.GetValueOfInt(PaymentData.paymentMethod), null, trx) <= 0)
                                    //        {
                                    //            _log.Info("Checkno Not Updated.");
                                    //        }
                                    //    }
                                    //}

                                    //alloc = new MAllocationHdr(ct, true, _pay.GetDateTrx(), _pay.GetC_Currency_ID(), ct.GetContext("#AD_User_Name"), trx);
                                    //alloc.SetAD_Org_ID(ct.GetAD_Org_ID());
                                    //if (alloc.Save())
                                    //{
                                    //    MAllocationLine aLine = new MAllocationLine(ct, 0, trx);
                                    //    aLine.SetAmount(_pay.GetPayAmt());
                                    //    aLine.SetC_AllocationHdr_ID(alloc.GetC_AllocationHdr_ID());
                                    //    aLine.SetDiscountAmt(Env.ZERO);
                                    //    aLine.SetWriteOffAmt(Env.ZERO);
                                    //    aLine.SetOverUnderAmt(Env.ZERO);
                                    //    aLine.SetDateTrx(DateTime.Now);
                                    //    aLine.SetC_BPartner_ID(Util.GetValueOfInt(_pay.GetC_BPartner_ID()));
                                    //    aLine.SetC_Payment_ID(_pay.GetC_Payment_ID());
                                    //    if (!aLine.Save())
                                    //    {
                                    //        isAllocationSaved = false;
                                    //        ex.Append(Msg.GetMsg(ct, "VA009_PALNotSaved"));
                                    //        ValueNamePair pp = VLogger.RetrieveError();
                                    //        if (pp != null)
                                    //        {
                                    //            ex.Append(", " + pp.GetName());
                                    //        }
                                    //        _log.Info(ex.ToString());
                                    //    }
                                    //    else
                                    //        isAllocationSaved = true;
                                    //}

                                    //1052--mention doctype before document no 
                                    ex.Append(Msg.GetMsg(ct, "VA009_SavedSuccessfully") + ":-" +
                                        (isPayment ? Msg.GetMsg(ct, "VA009_APPayment") : Msg.GetMsg(ct, "VA009_ARPayment")) + _pay.GetDocumentNo());
                                    retDic["success"] = ex.ToString();

                                }
                                else
                                {
                                    //if (alloc.GetC_AllocationHdr_ID() > 0)
                                    //{
                                    //    MAllocationLine aLine = new MAllocationLine(ct, 0, trx);
                                    //    aLine.SetAmount(_pay.GetPayAmt());
                                    //    aLine.SetC_AllocationHdr_ID(alloc.GetC_AllocationHdr_ID());
                                    //    aLine.SetDiscountAmt(Env.ZERO);
                                    //    aLine.SetWriteOffAmt(Env.ZERO);
                                    //    aLine.SetOverUnderAmt(Env.ZERO);
                                    //    aLine.SetDateTrx(DateTime.Now);
                                    //    aLine.SetC_BPartner_ID(Util.GetValueOfInt(_pay.GetC_BPartner_ID()));
                                    //    aLine.SetC_Payment_ID(_pay.GetC_Payment_ID());
                                    //    if (!aLine.Save())
                                    //    {
                                    //        isAllocationSaved = false;
                                    //        ex.Append(Msg.GetMsg(ct, "VA009_PALNotSaved"));
                                    //        ValueNamePair pp = VLogger.RetrieveError();
                                    //        if (pp != null)
                                    //        {
                                    //            ex.Append(", " + pp.GetName());
                                    //        }
                                    //        _log.Info(ex.ToString());
                                    //    }
                                    //    else
                                    //        isAllocationSaved = true;
                                    //}

                                    //1052--mention doctype before document no 
                                    ex.Append(", " + (isPayment ? Msg.GetMsg(ct, "VA009_ARPayment") : Msg.GetMsg(ct, "VA009_APPayment")) + _pay.GetDocumentNo());
                                    retDic["success"] = ex.ToString();
                                }

                                // JID_1340: Set Allocated True of AP Payment and AR receipt in case of Bank to Bank transfer
                                //if (isAllocationSaved)
                                //{
                                if (DB.ExecuteQuery(" UPDATE C_Payment SET IsAllocated='Y' WHERE C_payment_ID = " + _pay.GetC_Payment_ID(), null, trx) <= 0)
                                {
                                    trx.Rollback();
                                    DB.ExecuteQuery("DELETE FROM C_Payment WHERE C_Payment_ID = " + _pay.GetC_Payment_ID());
                                    ex.Clear();
                                    ex.Append(Msg.GetMsg(ct, "VA009_PNotCompelted") + ":-" + _pay.GetProcessMsg());
                                    _log.Info(ex.ToString());
                                    retDic["error"] = ex.ToString();
                                    break;
                                }
                                //}
                                //else
                                //{
                                //    trx.Rollback();
                                //    ex.Append(Msg.GetMsg(ct, "VA009_PNotCompelted") + ":-" + _pay.GetProcessMsg());
                                //    _log.Info(ex.ToString());
                                //    break;
                                //}
                                //End
                            }
                            else
                            {
                                trx.Rollback();
                                //1052-- delete Payment Document in Drafetd Stage 
                                DB.ExecuteQuery("DELETE FROM C_Payment WHERE C_Payment_ID = " + _pay.GetC_Payment_ID());
                                ex.Clear();
                                ex.Append(Msg.GetMsg(ct, "VA009_PNotCompelted") + ":-" + _pay.GetProcessMsg());
                                _log.Info(ex.ToString());
                                retDic["error"] = ex.ToString();
                                break;
                            }
                        }
                    }
                    //to complete allocation
                    //if (alloc.CompleteIt() == "CO")
                    //{
                    //    alloc.SetProcessed(true);
                    //    alloc.SetDocAction("CL");
                    //    alloc.SetDocStatus("CO");
                    //    alloc.Save();
                    //}
                }
            }
            catch (Exception e)
            {
                trx.Rollback();
                _log.Info(e.Message);
                ValueNamePair pp = VLogger.RetrieveError();
                if (pp != null)
                    _log.Info(pp.GetName());
            }
            finally
            {
                trx.Commit();
                trx.Close();
            }
            return retDic;
        }

        /// <summary>
        /// Generate Batch Lines
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="PaymentData">Payment Data</param>
        /// <param name="_Bt">MVA009Batch Object</param>
        /// <param name="trx">Transaction</param>
        /// <writer>VIS_0045</writer>
        /// <returns>Batch Line ID</returns>
        public int GenerateBatchLine(Ctx ct, GeneratePaymt PaymentData, MVA009Batch _Bt, Trx trx)
        {
            return GenerateBatchLine(ct, PaymentData, _Bt, trx, null);
        }

        /// <summary>
        /// added by Manjot 19/Feb/2019
        /// Generate Batch Lines
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="PaymentData">Payment Data</param>
        /// <param name="_Bt">MVA009Batch Object</param>
        /// <param name="trx">Transaction</param>
        /// <returns>VA009_BatchLine_ID</returns>
        public int GenerateBatchLine(Ctx ct, GeneratePaymt PaymentData, MVA009Batch _Bt, Trx trx, String DocumentBaseType)
        {
            MVA009BatchLines _BtLines = new MVA009BatchLines(ct, 0, trx);
            _BtLines.SetVA009_Batch_ID(_Bt.GetVA009_Batch_ID());
            _BtLines.SetC_BPartner_ID(PaymentData.C_BPartner_ID);
            // Set BP Location 
            if (!String.IsNullOrEmpty(DocumentBaseType))
            {
                if ("API".Equals(DocumentBaseType) || "APC".Equals(DocumentBaseType) || "POO".Equals(DocumentBaseType))
                {
                    _BtLines.SetVA009_PaymentLocation_ID(PaymentData.C_BPartner_Location_ID);
                }
                else if ("ARI".Equals(DocumentBaseType) || "ARC".Equals(DocumentBaseType) || "SOO".Equals(DocumentBaseType))
                {
                    _BtLines.SetVA009_ReceiptLocation_ID(PaymentData.C_BPartner_Location_ID);
                }
            }
            #region to set bank account of business partner and name on batch line
            if (PaymentData.C_BPartner_ID > 0)
            {
                //to check if payment method is CHECK then skip otherwise set these values
                string _baseType = Util.GetValueOfString(DB.ExecuteScalar(@"SELECT VA009_PaymentBaseType FROM VA009_PaymentMethod WHERE 
                                VA009_PaymentMethod_ID=" + _Bt.GetVA009_PaymentMethod_ID(), null,
                 trx));
                if (_baseType != X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Check && _baseType != X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Cash)
                {
                    // 
                    DataSet ds1 = new DataSet();
                    //updated query as per requirement
                    ds1 = DB.ExecuteDataset(@"SELECT MAX(C_BP_BankAccount_ID) as C_BP_BankAccount_ID,
                                  A_Name, RoutingNo, AccountNo FROM C_BP_BankAccount WHERE IsActive='Y' AND C_BPartner_ID = " + PaymentData.C_BPartner_ID + " AND "
                           + " AD_Org_ID IN(0, " + _Bt.GetAD_Org_ID() + ") GROUP BY C_BP_BankAccount_ID, A_Name,RoutingNo,AccountNo, AD_Org_ID ORDER BY AD_Org_ID DESC");
                    if (ds1 != null && ds1.Tables[0].Rows.Count > 0)
                    {
                        // _BtLines.Set_Value("C_BP_BankAccount_ID", Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                        // _BtLines.Set_Value("a_name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                        _BtLines.Set_ValueNoCheck("C_BP_BankAccount_ID", Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                        //if partner bank account is not present then set null because constraint null is on ther payment table and it will not allow to save zero.
                        if (Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]) == 0)
                            _BtLines.Set_Value("C_BP_BankAccount_ID", null);
                        _BtLines.Set_ValueNoCheck("A_Name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                        _BtLines.Set_ValueNoCheck("RoutingNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["RoutingNo"]));
                        _BtLines.Set_ValueNoCheck("AccountNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["AccountNo"]));
                    }
                }
            }
            #endregion
            _BtLines.SetAD_Client_ID(PaymentData.AD_Client_ID);
            _BtLines.SetAD_Org_ID(PaymentData.AD_Org_ID);
            //_BtLines.SetProcessed(true);
            if (!_BtLines.Save())
            {
                return 0;
            }
            else
                return _BtLines.GetVA009_BatchLines_ID();
        }


        /// <summary>
        /// added by Manjot 19/Feb/2019
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="PaymentData">Payment Data</param>
        /// <param name="_Bt">MVA009Batch Object</param>
        /// <param name="_BankAcct">C_BankAccount_ID</param>
        /// <param name="_invpaySchdule">C_InvoicePaySchedule_ID</param>
        /// <param name="_doctype">C_DocType_ID</param>
        /// <param name="convertedAmount">Converted Amount</param>
        /// <param name="paymentmethdoID">VA009_PaymentMethod_ID</param>
        /// <param name="Batchline_ID">VA009_BatchLines_ID</param>
        /// <param name="isOverwrite">Is Overwrite Payment Method</param>
        /// <param name="trx">Transaction</param>
        /// <returns>VA009_BatchLineDetails_ID</returns>
        public int GenerateBatchLineDetails(Ctx ct, GeneratePaymt PaymentData, MVA009Batch _Bt, MBankAccount _BankAcct, MInvoicePaySchedule _invpaySchdule, MDocType _doctype, decimal convertedAmount, int paymentmethdoID, int Batchline_ID, string isOverwrite, Trx trx)
        {
            //to get the BP_BankAccount_ID
            int _BP_BankAccount_ID = 0;
            MVA009BatchLineDetails _btDetal = new MVA009BatchLineDetails(ct, 0, trx);
            _btDetal.SetAD_Client_ID(PaymentData.AD_Client_ID);
            _btDetal.SetAD_Org_ID(PaymentData.AD_Org_ID);
            _btDetal.SetC_InvoicePaySchedule_ID(PaymentData.C_InvoicePaySchedule_ID);
            _btDetal.SetC_Invoice_ID(PaymentData.C_Invoice_ID);
            _btDetal.SetVA009_BatchLines_ID(Batchline_ID);
            //Rakesh(VA228):Set currency and conversion type id from invoice
            _btDetal.SetC_Currency_ID(PaymentData.C_Currency_ID);
            _btDetal.SetC_ConversionType_ID(PaymentData.ConversionTypeId);
            //Adjust discount amount from due amount if discount date greater than account date
            if (Util.GetValueOfDateTime(_invpaySchdule.GetDiscountDate()) >= Util.GetValueOfDateTime(_Bt.GetDateAcct()))
            {
                convertedAmount = convertedAmount - PaymentData.ConvertedDiscountAmount;
                if (_doctype.GetDocBaseType() == "ARC" || _doctype.GetDocBaseType() == "APC")
                {
                    if (PaymentData.ConvertedDiscountAmount > 0)
                    {
                        PaymentData.ConvertedDiscountAmount = -1 * PaymentData.ConvertedDiscountAmount;
                    }
                }
                else
                {
                    if (_doctype.GetDocBaseType() == "API" && PaymentData.ConvertedDiscountAmount < 0)
                    {
                        PaymentData.ConvertedDiscountAmount = -1 * PaymentData.ConvertedDiscountAmount;
                    }
                }
                _btDetal.SetDiscountAmt(PaymentData.ConvertedDiscountAmount);
                _btDetal.SetDiscountDate(_invpaySchdule.GetDiscountDate());
            }
            else
            {
                PaymentData.ConvertedDiscountAmount = 0;
                PaymentData.DiscountDate = null;
            }
            _btDetal.SetDueAmt(PaymentData.DueAmt);
            _btDetal.SetDueDate(_invpaySchdule.GetDueDate());

            if (_doctype.GetDocBaseType() == "ARC" || _doctype.GetDocBaseType() == "APC")
            {
                if (convertedAmount > 0)
                    convertedAmount = -1 * convertedAmount;
            }
            else
            {
                if (_doctype.GetDocBaseType() == "API")
                {
                    if (convertedAmount < 0)
                        convertedAmount = -1 * convertedAmount;
                }
            }
            _btDetal.SetVA009_ConvertedAmt(convertedAmount);
            if (paymentmethdoID > 0)
            {
                _btDetal.SetVA009_PaymentMethod_ID(paymentmethdoID);
            }
            //to set the C_BP_BankAccount_ID get the C_BP_BankAccount_ID from Invoice or C_BP_BankAccount table
            if (PaymentData.C_Invoice_ID > 0)
            {
                _BP_BankAccount_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_BP_BankAccount_ID FROM C_Invoice WHERE IsActive='Y' AND C_Invoice_ID=" + PaymentData.C_Invoice_ID, null, trx));
                if (_BP_BankAccount_ID == 0)
                {
                    //PostGre SQL for Aggragate funcation should use Group by clause whenever used Order By Clause otherwise through error
                    _BP_BankAccount_ID = Util.GetValueOfInt(DB.ExecuteScalar(@" SELECT MAX(C_BP_BankAccount_ID) as C_BP_BankAccount_ID
                                  FROM C_BP_BankAccount WHERE C_BPartner_ID = " + PaymentData.C_BPartner_ID + " AND IsActive='Y' AND "
                               + " AD_Org_ID IN (0, " + PaymentData.AD_Org_ID + ") GROUP BY AD_Org_ID ORDER BY AD_Org_ID DESC", null, trx));
                }
            }
            //Set the C_BP_BankAccount_ID Value
            _btDetal.Set_Value("C_BP_BankAccount_ID", _BP_BankAccount_ID);
            //_btDetal.SetProcessed(true);
            if (!_btDetal.Save())
            {
                return 0;
            }
            else
            {
                _invpaySchdule = new MInvoicePaySchedule(ct, PaymentData.C_InvoicePaySchedule_ID, trx);
                _invpaySchdule.SetVA009_ExecutionStatus("Y");
                if (isOverwrite == "Y")
                    _invpaySchdule.SetVA009_PaymentMethod_ID(PaymentData.VA009_PaymentMethod_ID);

                if (!_invpaySchdule.Save())
                {
                    ValueNamePair pp = VLogger.RetrieveError();
                    if (pp != null)
                        _log.Info(pp.GetName());
                }
                return _btDetal.GetVA009_BatchLineDetails_ID();
            }
        }

        //added by Manjot 19/Feb/2019
        public int GenerateBatchOrdLineDetails(Ctx ct, GeneratePaymt PaymentData, MVA009Batch _Bt, MBankAccount _BankAcct, MVA009OrderPaySchedule _OrdPaySchdule, MDocType _doctype, decimal convertedAmount, int paymentmethdoID, int Batchline_ID, string isOverwrite, Trx trx)
        {
            MVA009BatchLineDetails _btDetal = new MVA009BatchLineDetails(ct, 0, trx);
            _btDetal.SetAD_Client_ID(PaymentData.AD_Client_ID);
            _btDetal.SetAD_Org_ID(PaymentData.AD_Org_ID);
            _btDetal.Set_Value("VA009_OrderPaySchedule_ID", PaymentData.C_InvoicePaySchedule_ID); // Here OrderPaySchedule_ID is AS InvoicePaySchedule_ID
            _btDetal.Set_Value("C_Order_ID", PaymentData.C_Invoice_ID); //Here C_Order_ID is as C_Invoice_ID
                                                                        //set Order Currency 
            _btDetal.SetC_Currency_ID(PaymentData.C_Currency_ID);
            //Set Order Currency Type
            _btDetal.SetC_ConversionType_ID(PaymentData.ConversionTypeId);
            _btDetal.SetVA009_BatchLines_ID(Batchline_ID);

            //Adjust discount amount from due amount if discount date greater than account date
            if (Util.GetValueOfDateTime(_OrdPaySchdule.GetDiscountDate()) >= Util.GetValueOfDateTime(_Bt.GetDateAcct()))
            {
                convertedAmount = convertedAmount - PaymentData.ConvertedDiscountAmount;
                if (_doctype.GetDocBaseType() == "ARC" || _doctype.GetDocBaseType() == "APC")
                {
                    if (PaymentData.ConvertedDiscountAmount > 0)
                    {
                        PaymentData.ConvertedDiscountAmount = -1 * PaymentData.ConvertedDiscountAmount;
                    }
                }
                else
                {
                    if (_doctype.GetDocBaseType() == "API" && PaymentData.ConvertedDiscountAmount < 0)
                    {
                        PaymentData.ConvertedDiscountAmount = -1 * PaymentData.ConvertedDiscountAmount;
                    }
                }
                _btDetal.SetDiscountAmt(PaymentData.ConvertedDiscountAmount);
                _btDetal.SetDiscountDate(_OrdPaySchdule.GetDiscountDate());
            }
            else
            {
                PaymentData.ConvertedDiscountAmount = 0;
                PaymentData.DiscountDate = null;
            }

            _btDetal.SetDueAmt(PaymentData.DueAmt);
            _btDetal.SetDueDate(_OrdPaySchdule.GetDueDate());

            if (_doctype.GetDocBaseType() == "ARC" || _doctype.GetDocBaseType() == "APC")
            {
                if (convertedAmount > 0)
                    convertedAmount = -1 * convertedAmount;
            }
            else
            {
                if (_doctype.GetDocBaseType() == "API")
                {
                    if (convertedAmount < 0)
                        convertedAmount = -1 * convertedAmount;
                }

            }
            _btDetal.SetVA009_ConvertedAmt(convertedAmount);
            if (paymentmethdoID > 0)
            {
                _btDetal.SetVA009_PaymentMethod_ID(paymentmethdoID);
            }
            //_btDetal.SetProcessed(true);
            if (!_btDetal.Save())
            {
                return 0;
            }
            else
            {
                _OrdPaySchdule = new MVA009OrderPaySchedule(ct, PaymentData.C_InvoicePaySchedule_ID, trx);
                _OrdPaySchdule.SetVA009_ExecutionStatus("Y");
                if (isOverwrite == "Y")
                {
                    _OrdPaySchdule.SetVA009_PaymentMethod_ID(PaymentData.VA009_PaymentMethod_ID);
                }
                if (!_OrdPaySchdule.Save())
                {
                    ValueNamePair pp = VLogger.RetrieveError();
                    if (pp != null)
                        _log.Info(pp.GetName());
                }
                return _btDetal.GetVA009_BatchLineDetails_ID();
            }
        }

        public static bool HasProperty(ExpandoObject expandoObj, string name)
        {
            return ((IDictionary<string, object>)expandoObj).ContainsKey(name);
        }

        /// <summary>
        /// Get Document Type according to selected organization
        /// </summary>
        /// <param name="orgs">organization id</param>
        /// <param name="ctx">session</param>
        /// <returns>List of Document types</returns>
        public List<DocTypeDetails> GetDocumentType(string orgs, Ctx ctx)
        {
            List<DocTypeDetails> retDic = null;
            //Table name must Camel format because Table name is case sensitive
            //VA230:Get DocBaseType to check document type
            string sql = @"SELECT Name,C_DocType_ID,DocBaseType FROM C_DocType WHERE C_DocType.DOCBASETYPE IN ('ARR', 'APP') AND C_DocType.AD_ORG_ID IN (0, " + orgs + ")";
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql.ToString(), "C_DocType", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                retDic = new List<DocTypeDetails>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DocTypeDetails obj = new DocTypeDetails();//
                    obj.C_DocType_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_DocType_ID"]);
                    obj.Name = Util.GetValueOfString(ds.Tables[0].Rows[i]["Name"]);
                    obj.DocBaseType = Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]);
                    retDic.Add(obj);
                }
            }
            return retDic;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="BP"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public List<LocationDetails> GetLocation(string BP, Ctx ct)
        {
            List<LocationDetails> Locations = new List<LocationDetails>();
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT C_BPartner_Location.C_BPartner_Location_ID , C_BPartner_Location.Name FROM C_BPartner_Location C_BPartner_Location WHERE C_BPartner_Location.C_BPartner_ID =" + BP);
            string finalQuery = MRole.GetDefault(ct).AddAccessSQL(sql.ToString(), "C_BPartner_Location", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(finalQuery);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    LocationDetails _locData = new LocationDetails();
                    _locData.C_Location_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_Location_ID"]);
                    _locData.Name = Util.GetValueOfString(ds.Tables[0].Rows[i]["Name"]);
                    Locations.Add(_locData);
                }
            }
            sql = null;
            return Locations;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public List<ChargeDetails> GetCharge(string orgs, Ctx ct)
        {
            List<ChargeDetails> Charge = new List<ChargeDetails>();
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT C_Charge.C_Charge_ID , C_Charge.NAME FROM C_Charge C_Charge WHERE C_Charge.AD_Org_ID=" + orgs);
            string finalQuery = MRole.GetDefault(ct).AddAccessSQL(sql.ToString(), "C_Charge", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(finalQuery);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ChargeDetails _chargeData = new ChargeDetails();
                    _chargeData.C_Charge_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Charge_ID"]);
                    _chargeData.Name = Util.GetValueOfString(ds.Tables[0].Rows[i]["Name"]);
                    Charge.Add(_chargeData);
                }
            }
            sql = null;
            return Charge;
        }
        public List<BPDetails> GetBPartnerName(string orgs, Ctx ct)
        {
            List<BPDetails> Bp = new List<BPDetails>();
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT C_BPartner.C_BPartner_ID,C_BPartner.Name FROM C_BPartner C_BPartner WHERE C_BPartner.AD_Org_ID=" + orgs);
            string finalQuery = MRole.GetDefault(ct).AddAccessSQL(sql.ToString(), "C_BPartner", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(finalQuery);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    BPDetails _bpData = new BPDetails();
                    _bpData.C_BPartner_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BPartner_ID"]);
                    _bpData.Name = Util.GetValueOfString(ds.Tables[0].Rows[i]["Name"]);
                    Bp.Add(_bpData);
                }
            }
            sql = null;
            return Bp;
        }

        /// <summary>
        /// to generate payment Manually 
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="paymentData">Data of Payment</param>
        /// <returns></returns>
        public string GeneratePayMannual(Ctx ct, List<Dictionary<string, string>> paymentData)
        {
            Trx trx = Trx.GetTrx("GeneratePayMannual_" + DateTime.Now.ToString("yyMMddHHmmssff"));
            StringBuilder ex = new StringBuilder();
            string msg = Msg.GetMsg(ct, "VA009_PaymentCompletedWith");
            StringBuilder docno = new StringBuilder();
            MPayment _pay = null;
            try
            {
                _pay = new MPayment(ct, 0, trx);
                _pay.SetAD_Client_ID(Util.GetValueOfInt(ct.GetAD_Client_ID()));
                _pay.SetAD_Org_ID(Util.GetValueOfInt(paymentData[0]["Org"]));
                _pay.SetC_BankAccount_ID(Util.GetValueOfInt(paymentData[0]["BankAccountID"]));
                //_pay.SetC_DocType_ID(Util.GetValueOfInt(paymentData[0]["DocType"]));
                _pay.SetDateAcct(Convert.ToDateTime(paymentData[0]["DateAcct"]));
                //to set trx date
                _pay.SetDateTrx(Convert.ToDateTime(paymentData[0]["DateTrx"]));
                _pay.SetC_Currency_ID(Util.GetValueOfInt(paymentData[0]["CurrencyID"]));
                _pay.SetC_DocType_ID(Util.GetValueOfInt(paymentData[0]["DocType"]));
                _pay.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(paymentData[0]["PaymentMethod"]));
                _pay.SetC_ConversionType_ID(Util.GetValueOfInt(paymentData[0]["CurrencyType"]));
                _pay.SetC_BPartner_ID(Util.GetValueOfInt(paymentData[0]["BPID"]));
                #region to set bank account of business partner and name on batch line
                if (Util.GetValueOfInt(paymentData[0]["BPID"]) > 0)
                {
                    DataSet ds1 = new DataSet();
                    ds1 = DB.ExecuteDataset(@" SELECT MAX(C_BP_BankAccount_ID) as C_BP_BankAccount_ID,
                                  a_name,RoutingNo,AccountNo FROM C_BP_BankAccount WHERE C_BPartner_ID = " + Util.GetValueOfInt(paymentData[0]["BPID"]) + " AND "
                           + " AD_Org_ID  IN (0 , " + paymentData[0]["Org"] + ") GROUP BY C_BP_BankAccount_ID, a_name,RoutingNo,AccountNo ");
                    if (ds1.Tables != null && ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                    {
                        _pay.Set_ValueNoCheck("C_BP_BankAccount_ID", Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                        //if partner bank account is not present then set null because constraint null is on ther payment table and it will not allow to save zero.
                        if (Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]) == 0)
                            _pay.Set_Value("C_BP_BankAccount_ID", null);
                        _pay.Set_ValueNoCheck("A_Name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                        _pay.Set_ValueNoCheck("RoutingNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["RoutingNo"]));
                        _pay.Set_ValueNoCheck("AccountNo", Util.GetValueOfString(ds1.Tables[0].Rows[0]["AccountNo"]));
                    }
                }
                #endregion
                _pay.SetC_BPartner_Location_ID(Util.GetValueOfInt(paymentData[0]["BPLocation"]));
                _pay.SetPayAmt(Util.GetValueOfDecimal(paymentData[0]["PaymentAmount"]));
                if (Util.GetValueOfInt(paymentData[0]["charge"]) > 0)
                {
                    _pay.SetC_Charge_ID(Util.GetValueOfInt(paymentData[0]["charge"]));
                }
                _pay.SetCheckNo(Util.GetValueOfString(paymentData[0]["CheckNo"]));
                if (paymentData[0]["CheckDate"] != null)
                    _pay.SetCheckDate(Util.GetValueOfDateTime(paymentData[0]["CheckDate"]));
                //VA230:Set overrideautocheck
                _pay.SetIsOverrideAutoCheck(Util.GetValueOfBool(paymentData[0]["IsOverrideAutoCheck"]));
                if (!_pay.Save())
                {
                    ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                    ValueNamePair pp = VLogger.RetrieveError();
                    if (pp != null)
                    {
                        ex.Append(", " + pp.GetName());
                    }
                    _log.Info(ex.ToString());
                }
                else
                {
                    docno.Append(_pay.GetDocumentNo());
                    CompleteOrReverse(ct, _pay.GetC_Payment_ID(), _pay.Get_Table_ID(), _pay.Get_TableName().ToLower(), DocActionVariables.ACTION_COMPLETE, trx);

                    //if (_pay.CompleteIt() == "CO")
                    //{
                    //    _pay.SetProcessed(true);
                    //    _pay.SetDocAction("CL");
                    //    _pay.SetDocStatus("CO");
                    //    _pay.Save();
                    //}
                    if (_pay.Save())
                    {
                    }
                    else
                    {
                        msg = _pay.GetProcessMsg() + " -";
                    }
                }
            }
            catch (Exception e)
            {
                _log.Info(e.Message);
                trx.Rollback();
                ValueNamePair pp = VLogger.RetrieveError();
                if (pp != null)
                    _log.Info(pp.GetName());
            }
            trx.Commit();
            trx.Close();
            if (docno.Length > 0)
            {
                msg += docno.ToString();
                ex.Append("\n" + msg);
            }
            return ex.ToString();
        }

        /// <summary>
        /// Prepare Data for Payment File 
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="DocNumber">Document Number</param>
        /// <param name="isBatch">Is Batch Check</param>
        /// <returns>List Of Files Created</returns>
        public List<PaymentResponse> prepareDataForPaymentFile(Ctx ct, string DocNumber, bool isBatch, string AD_Org_ID)
        {
            int payment_ID = 0;
            List<PaymentResponse> batchResponse = new List<PaymentResponse>();
            bool ispaymentGenerated = false;
            //add sql access to generate batch file for those who have access
            StringBuilder sql = new StringBuilder();
            if (isBatch)
            {
                //handled logs
                //add sql access to generate batch file for those who have access
                sql.Clear();
                sql.Append(@"SELECT VA009_Batch_ID FROM VA009_Batch
                            WHERE AD_Org_ID =" + Util.GetValueOfInt(AD_Org_ID) + " AND UPPER(documentno) = UPPER('" + DocNumber + "')");
                payment_ID = Util.GetValueOfInt(DB.ExecuteScalar(MRole.GetDefault(ct).AddAccessSQL(sql.ToString(), "VA009_Batch", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO)));
                sql.Clear();
                sql.Append(@"SELECT Count(bld.C_Payment_ID) FROM VA009_BatchLineDetails bld INNER JOIN VA009_BatchLines bl ON (bld.VA009_BatchLines_ID=bl.VA009_BatchLines_ID)
                    INNER JOIN VA009_Batch b ON (bl.VA009_Batch_ID = b.VA009_Batch_ID) LEFT JOIN C_Payment p ON (p.C_Payment_ID = bld.C_Payment_ID)
                    WHERE b.VA009_Batch_ID = " + payment_ID);
                ispaymentGenerated = (Util.GetValueOfInt(DB.ExecuteScalar(sql.ToString())) > 0 ? true : false);
            }
            else
            {
                //add sql access to generate batch file for those who have access
                sql.Clear();
                //removed brackets from this query because it was creating problem in case of document number was having special characters
                sql.Append(@"SELECT c_payment_id FROM C_Payment 
                            WHERE AD_Org_ID =" + Util.GetValueOfInt(AD_Org_ID) + " AND UPPER(documentno)=UPPER('" + DocNumber + "') ");
                payment_ID = Util.GetValueOfInt(DB.ExecuteScalar(MRole.GetDefault(ct).AddAccessSQL(sql.ToString(), "C_Payment", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO)));
            }
            PaymentResponse obj = null;
            sql.Clear();
            sql.Append(@"SELECT bpc.value FROM C_Payment p INNER JOIN VA009_BankPaymentClass pc
                                  ON (pc.C_bankAccount_ID= p.c_bankaccount_id AND pc.VA009_PaymentMethod_ID=p.VA009_PaymentMethod_ID AND pc.IsActive = 'Y')
                                  INNER JOIN VA009_PaymentClass  bpc ON (pc.VA009_PaymentClass_ID=bpc.VA009_PaymentClass_ID) ");
            if (isBatch)
            {
                if (!ispaymentGenerated)
                {
                    obj = new PaymentResponse();
                    obj._error = Msg.GetMsg(ct, "VA009_PleaseGenerateBatchPaymentFirst");
                    batchResponse.Add(obj);
                    return batchResponse;
                }
                sql.Append(@" WHERE p.c_payment_id IN (SELECT bld.C_Payment_ID FROM VA009_BatchLineDetails bld INNER JOIN VA009_BatchLines bl ON (bld.VA009_BatchLines_ID=bl.VA009_BatchLines_ID)
                    INNER JOIN VA009_Batch b ON (bl.VA009_Batch_ID = b.VA009_Batch_ID) LEFT JOIN C_Payment p ON (p.C_Payment_ID = bld.C_Payment_ID)
                    WHERE b.VA009_Batch_ID = " + payment_ID + ")");
            }
            else
            {
                sql.Append(" WHERE p.c_payment_id = " + payment_ID);
            }
            string PaymentClass = Util.GetValueOfString(DB.ExecuteScalar(sql.ToString(), null, null));
            if (!string.IsNullOrEmpty(PaymentClass))
            {
                batchResponse = ExecuteClass(PaymentClass, ct, payment_ID, isBatch);
            }
            else
            {
                obj = new PaymentResponse();
                obj._error = Msg.GetMsg(ct, "VA009_PleaseSelectClassFirst");
                batchResponse.Add(obj);
            }

            return batchResponse;
        }

        /// <summary>
        /// To Execute Specific Class Which Is Binded on Bank Account
        /// </summary>
        /// <param name="_className">Class Name</param>
        /// <param name="ctx">Context</param>
        /// <param name="Payment_ID">Payment Or Batch ID</param>
        /// <param name="isBatch">Batch Check</param>
        /// <returns>List Of FIles Created</returns>
        private static List<PaymentResponse> ExecuteClass(string _className, Ctx ctx, int Payment_ID, bool isBatch)
        {
            List<PaymentResponse> batchResponse = new List<PaymentResponse>();
            PaymentResponse _obj = null; ;
            Type type = null;
            string Prefix = "";
            string[] dotSplit = _className.Split('.');
            string methodName = dotSplit[dotSplit.Length - 1];
            int startindex = _className.LastIndexOf('.');
            string Class = dotSplit[dotSplit.Length - 2];
            int CharcterIndex = Class.IndexOf("_");
            if (CharcterIndex > 0)
            {
                Prefix = Class.Substring(0, CharcterIndex) + "_";
            }

            _className = _className.Remove(startindex, methodName.Length + 1);

            if (Class.Contains("VA009_"))
                type = Type.GetType(_className);
            else
                type = ClassTypeContainer.GetClassType(_className, Class.Substring(0, CharcterIndex));

            if (type != null)
            {
                if (type.IsClass)
                {
                    if (type.GetMethod(methodName) != null)
                    {
                        object _classobj = Activator.CreateInstance(type);
                        MethodInfo method = type.GetMethod(methodName);
                        object[] obj = new object[] { ctx, Payment_ID, isBatch };
                        return (List<PaymentResponse>)method.Invoke(_classobj, obj);
                    }
                    else
                    {
                        _obj = new PaymentResponse();
                        _obj._error = Msg.GetMsg(ctx, "VA009_MethdNotFound");
                        batchResponse.Add(_obj);
                        return batchResponse;
                    }
                }
            }
            _obj = new PaymentResponse();
            _obj._error = Msg.GetMsg(ctx, "VA009_Error");
            batchResponse.Add(_obj);
            return batchResponse;
        }

        /// <summary>
        /// To Call the Workflow Against Selected Record
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="Record_ID">Record ID</param>
        /// <param name="Table_ID">Table ID</param>
        /// <param name="TableName">Table Name</param>
        /// <param name="DocAction">Document Action</param>
        /// <param name="trx">Transaction Object</param>
        /// <returns>Msg From Workflow</returns>
        private string[] CompleteOrReverse(Ctx ctx, int Record_ID, int Table_ID, string TableName, string DocAction, Trx trx)
        {
            int AD_Process_ID = 0;
            AD_Process_ID = Util.GetValueOfInt(DB.ExecuteScalar("select ad_process_ID from ad_column where ad_table_id = " + Table_ID + " and lower(columnname)= 'docaction'", null, null));
            string[] result = new string[2];
            MRole role = MRole.Get(ctx, ctx.GetAD_Role_ID());
            int ad_window_id = 0;
            if (Util.GetValueOfBool(role.GetProcessAccess(AD_Process_ID)))
            {
                string Sql = "";
                if (TableName == "c_cash")
                { //Cash Journal
                    Sql = "UPDATE C_Cash SET DocAction = '" + DocAction + "' WHERE C_Cash_ID = " + Record_ID;
                }
                else if (TableName == "c_payment")
                { //Payment
                    Sql = "UPDATE C_Payment SET DocAction = '" + DocAction + "' WHERE C_Payment_ID = " + Record_ID;
                    ad_window_id = Util.GetValueOfInt(DB.ExecuteScalar("SELECT AD_Window_ID FROM AD_Window WHERE Export_ID = 'VIS_195'"));
                }
                else if (TableName == "c_allocationhdr")
                {
                    Sql = "UPDATE C_AllocationHdr SET DocAction = '" + DocAction + "' WHERE C_AllocationHdr_ID = " + Record_ID;
                }
                if (DB.ExecuteQuery(Sql, null, trx) < 0)
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
                        errorMsg = Msg.GetMsg(ctx, "VA009_PNotCompelted");
                    result[0] = errorMsg;
                    result[1] = "N";
                    trx.Rollback();
                    return result;
                }

                trx.Commit();
                MProcess proc = new MProcess(ctx, AD_Process_ID, null);
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
                        errorMsg = Msg.GetMsg(ctx, "VA009_PNotCompelted");
                    result[0] = errorMsg;
                    result[1] = "N";
                    return result;
                }

                //MPInstancePara para = new MPInstancePara(pin, 20);
                //para.setParameter("DocAction", DocAction);
                //if (!para.Save())
                //{
                //    //String msg = "No DocAction Parameter added";  //  not translated
                //}

                VAdvantage.ProcessEngine.ProcessInfo pi = new VAdvantage.ProcessEngine.ProcessInfo("WF", AD_Process_ID);
                pi.SetAD_User_ID(ctx.GetAD_User_ID());
                pi.SetAD_Client_ID(ctx.GetAD_Client_ID());
                pi.SetAD_PInstance_ID(pin.GetAD_PInstance_ID());
                pi.SetRecord_ID(Record_ID);
                pi.SetTable_ID(Table_ID);
                if (ad_window_id > 0)
                {
                    pi.SetAD_Window_ID(ad_window_id);
                }
                ProcessCtl worker = new ProcessCtl(ctx, null, pi, null);
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
                        errorMsg = Msg.GetMsg(ctx, "VA009_PNotCompelted");
                    result[0] = errorMsg;
                    result[1] = "N";
                    return result;
                }
                else
                    Msg.GetMsg(ctx, "VA009_SavedSuccessfully");

                result[0] = "";
                result[1] = "Y";
            }
            else
            {
                result[0] = Msg.GetMsg(ctx, "ViewAccess");
                return result;
            }
            return result;
        }

        /// <summary>
        /// To create batch and Batch lines
        /// </summary>
        /// <param name="ct">Context Object</param>
        /// <param name="PaymentData">Payment Data (Object no of Records)</param>
        /// <returns>Batch Document Number</returns>
        public string CrateBatchPayments(Ctx ct, GeneratePaymt[] PaymentData)
        {
            Trx trx = Trx.GetTrx("Batch_" + DateTime.Now.ToString("yyMMddHHmmssff"));
            List<int> BpList = new List<int>();
            List<int> BtachId = new List<int>();
            StringBuilder docno = new StringBuilder();
            StringBuilder ex = new StringBuilder();
            string msg = "";
            MBankAccount _BankAcct = new MBankAccount(ct, PaymentData[0].C_BankAccount_ID, trx);
            List<int> PaymentMethodIDS = new List<int>();
            string isconsolidate = PaymentData[0].isconsolidate;
            string isOverwrite = PaymentData[0].isOverwrite;
            int batchid = 0; MVA009Batch _Bt = null;
            decimal convertedAmount = 0;
            String _TransactionType = String.Empty; //Arpit
            StringBuilder _sql = new StringBuilder();
            int C_Doctype_ID = 0;
            try
            {
                if (PaymentData.Length > 0)
                {
                    int paymentmethdoID = 0;
                    Dictionary<string, object> paymethodDetails = null;
                    for (int i = 0; i < PaymentData.Length; i++)
                    {
                        paymentmethdoID = PaymentData[i].VA009_PaymentMethod_ID; convertedAmount = PaymentData[i].convertedAmt;
                        paymethodDetails = GetPaymentMethodDetails(ct, paymentmethdoID, trx);
                        // MVA009PaymentMethod _paymthd = new MVA009PaymentMethod(ct, paymentmethdoID, trx);
                        _TransactionType = String.Empty;
                        _TransactionType = PaymentData[i].TransactionType;

                        #region Create Batch
                        if (i == 0)
                        {
                            _Bt = new MVA009Batch(ct, 0, trx);
                            _Bt.SetC_Bank_ID(PaymentData[0].C_Bank_ID);
                            _Bt.SetC_BankAccount_ID(PaymentData[0].C_BankAccount_ID);
                            _Bt.SetAD_Client_ID(PaymentData[0].AD_Client_ID);
                            _Bt.SetAD_Org_ID(PaymentData[0].AD_Org_ID);
                            _Bt.SetVA009_PaymentMethod_ID(paymentmethdoID);
                            //to set document type against batch payment
                            //_Bt.Set_ValueNoCheck("C_DocType_ID", getDocumentTypeID(ct, PaymentData[0].AD_Org_ID, trx));
                            //Target Document Type selected by the User
                            _Bt.Set_ValueNoCheck("C_DocType_ID", PaymentData[0].TargetDocType);
                            //end
                            _Bt.SetVA009_PaymentRule(paymethodDetails["VA009_PaymentRule"].ToString());
                            _Bt.SetVA009_PaymentTrigger(paymethodDetails["VA009_PaymentTrigger"].ToString());
                            //to set bank currency on Payment Batch given by Rajni and Ashish
                            _Bt.Set_Value("C_Currency_ID", PaymentData[0].HeaderCurrency);
                            _Bt.Set_Value("C_ConversionType_ID", PaymentData[0].CurrencyType);
                            // _Bt.SetProcessed(true);
                            _Bt.SetVA009_DocumentDate(DateTime.Now);
                            //VA230:Set account date
                            _Bt.SetDateAcct(PaymentData[0].DateAcct);
                            if (isconsolidate == "Y")
                            {
                                _Bt.SetVA009_Consolidate(true);
                            }
                            else
                            {
                                //Bug175 set consolidated check box false if on form not checked.
                                _Bt.SetVA009_Consolidate(false);
                            }
                            // if overwrite payment method is true then set payment method on Batch
                            if (isOverwrite == "Y")
                            {
                                Dictionary<string, object> paymethod = new Dictionary<string, object>();
                                paymethod = GetPaymentMethodDetails(ct, PaymentData[0].VA009_PaymentMethod_ID, trx);
                                _Bt.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(paymethod["VA009_PaymentMethod_ID"].ToString()));
                                _Bt.SetVA009_PaymentRule(paymethod["VA009_PaymentRule"].ToString());
                                _Bt.SetVA009_PaymentTrigger(paymethod["VA009_PaymentTrigger"].ToString());
                            }

                            if (!_Bt.Save())
                            {
                                trx.Rollback();
                                ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                ValueNamePair pp = VLogger.RetrieveError();
                                if (pp != null)
                                {
                                    ex.Append(", " + pp.GetName());
                                }
                                _log.Info(ex.ToString());
                            }
                            else
                            {
                                batchid = _Bt.GetVA009_Batch_ID();
                                BtachId.Add(batchid);
                                PaymentMethodIDS.Add(paymentmethdoID);
                            }

                        }
                        #endregion

                        #region Create Batch Lines and Details
                        if (batchid > 0)
                        {
                            if (_TransactionType.Equals("Invoice"))
                            {
                                MInvoicePaySchedule _invpaySchdule = new MInvoicePaySchedule(ct, PaymentData[i].C_InvoicePaySchedule_ID, trx);
                                MDocType _doctype = new MDocType(ct, _invpaySchdule.GetC_DocType_ID(), trx);
                                //removed condition of Cheque Payment method Suggested by Ashish and Rajni
                                // && (paymethodDetails["VA009_PaymentType"].ToString() != "S")
                                if (BpList.Contains(PaymentData[i].C_BPartner_ID) && (PaymentMethodIDS.Contains(paymentmethdoID)))
                                {
                                    #region BatchLine and Batch Line Details
                                    _sql.Clear();
                                    _sql.Append(@"SELECT VA009_BatchLines_ID FROM VA009_BatchLines WHERE 
                                                  VA009_Batch_ID=" + _Bt.GetVA009_Batch_ID() +
                                                  " AND C_BPartner_ID=" + PaymentData[i].C_BPartner_ID);
                                    //VIS0045 : check Batch line created with selected BP Location
                                    if ("API".Equals(_doctype.GetDocBaseType()) || "APC".Equals(_doctype.GetDocBaseType()))
                                    {
                                        _sql.Append(" AND NVL(VA009_PaymentLocation_ID, 0) = " + PaymentData[i].C_BPartner_Location_ID);
                                    }
                                    else if ("ARI".Equals(_doctype.GetDocBaseType()) || "ARC".Equals(_doctype.GetDocBaseType()))
                                    {
                                        _sql.Append(" AND NVL(VA009_ReceiptLocation_ID, 0) = " + PaymentData[i].C_BPartner_Location_ID);
                                    }
                                    int Batchline_ID = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, trx));

                                    if (Batchline_ID == 0)
                                    {
                                        Batchline_ID = GenerateBatchLine(ct, PaymentData[i], _Bt, trx, _doctype.GetDocBaseType());
                                        if (Batchline_ID == 0)
                                        {
                                            trx.Rollback();
                                            ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                            {
                                                ex.Append(", " + pp.GetName());
                                            }
                                            _log.Info(ex.ToString());
                                        }
                                    }

                                    if (GenerateBatchLineDetails(ct, PaymentData[i], _Bt, _BankAcct, _invpaySchdule, _doctype, convertedAmount, paymentmethdoID, Batchline_ID, isOverwrite, trx) == 0)
                                    {
                                        trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }
                                    #endregion
                                    continue;
                                }
                                else
                                {
                                    BpList.Add(PaymentData[i].C_BPartner_ID);
                                    int Batchline_ID = GenerateBatchLine(ct, PaymentData[i], _Bt, trx, _doctype.GetDocBaseType());
                                    if (Batchline_ID == 0)
                                    {
                                        trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }
                                    else
                                    {
                                        //Rakesh(VA228):Set batch line detail
                                        if (GenerateBatchLineDetails(ct, PaymentData[i], _Bt, _BankAcct, _invpaySchdule, _doctype, convertedAmount, paymentmethdoID, Batchline_ID, isOverwrite, trx) == 0)
                                        {
                                            trx.Rollback();
                                            ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                            {
                                                ex.Append(", " + pp.GetName());
                                            }
                                            _log.Info(ex.ToString());
                                        }
                                    }
                                }
                            }

                            if (_TransactionType.Equals("Order"))
                            {
                                MVA009OrderPaySchedule _OrdPaySchdule = new MVA009OrderPaySchedule(ct, PaymentData[i].C_InvoicePaySchedule_ID, trx);
                                MDocType _doctype = MDocType.Get(ct, _OrdPaySchdule.GetC_DocType_ID());
                                if (_doctype.Get_ID() <= 0)
                                {
                                    C_Doctype_ID = Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT C_DocTypeTarget_ID FROM C_Order
                                    WHERE C_Order_ID = " + _OrdPaySchdule.GetC_Order_ID()));
                                    _doctype = MDocType.Get(ct, C_Doctype_ID);
                                }

                                if (BpList.Contains(PaymentData[i].C_BPartner_ID) && (PaymentMethodIDS.Contains(paymentmethdoID)))
                                {

                                    #region BatchLine and Batch Line Details
                                    _sql.Clear();
                                    _sql.Append(@"SELECT VA009_BatchLines_ID FROM VA009_BatchLines WHERE 
                                                  VA009_Batch_ID=" + _Bt.GetVA009_Batch_ID() +
                                                  " AND C_BPartner_ID=" + PaymentData[i].C_BPartner_ID);
                                    //VIS0045 : check Batch line created with selected BP Location
                                    if ("POO".Equals(_doctype.GetDocBaseType()))
                                    {
                                        _sql.Append(" AND NVL(VA009_PaymentLocation_ID, 0) = " + PaymentData[i].C_BPartner_Location_ID);
                                    }
                                    else if ("SOO".Equals(_doctype.GetDocBaseType()))
                                    {
                                        _sql.Append(" AND NVL(VA009_ReceiptLocation_ID, 0) = " + PaymentData[i].C_BPartner_Location_ID);
                                    }
                                    int Batchline_ID = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, trx));

                                    if (Batchline_ID == 0)
                                    {
                                        Batchline_ID = GenerateBatchLine(ct, PaymentData[i], _Bt, trx, _doctype.GetDocBaseType());
                                        if (Batchline_ID == 0)
                                        {
                                            trx.Rollback();
                                            ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                            {
                                                ex.Append(", " + pp.GetName());
                                            }
                                            _log.Info(ex.ToString());
                                        }
                                    }

                                    if (GenerateBatchOrdLineDetails(ct, PaymentData[i], _Bt, _BankAcct, _OrdPaySchdule, _doctype, convertedAmount, paymentmethdoID, Batchline_ID, isOverwrite, trx) == 0)
                                    {
                                        trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }
                                    #endregion
                                    continue;
                                }
                                else
                                {
                                    #region BatchLine AND Batch Line Details
                                    BpList.Add(PaymentData[i].C_BPartner_ID);
                                    int Batchline_ID = GenerateBatchLine(ct, PaymentData[i], _Bt, trx, _doctype.GetDocBaseType());
                                    if (Batchline_ID == 0)
                                    {
                                        trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }

                                    if (GenerateBatchOrdLineDetails(ct, PaymentData[i], _Bt, _BankAcct, _OrdPaySchdule, _doctype, convertedAmount, paymentmethdoID, Batchline_ID, isOverwrite, trx) == 0)
                                    {
                                        trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }
                                    #endregion
                                }
                            }

                        }
                        #endregion
                    }

                    //to check if payment method is CHECK then skip otherwise set these values
                    string _baseType = Util.GetValueOfString(DB.ExecuteScalar(@"SELECT VA009_PaymentBaseType FROM VA009_PaymentMethod WHERE 
                                VA009_PaymentMethod_ID=" + _Bt.GetVA009_PaymentMethod_ID(), null, trx));
                    //Updating the C_BP_BankAccount_ID on Batch Lines Tab
                    if (_Bt != null && !X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Check.Equals(_baseType) && !X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Cash.Equals(_baseType))
                    {
                        DataSet ds = DB.ExecuteDataset(@"SELECT MAX(BLD.C_BP_BankAccount_ID) AS C_BP_BankAccount_ID,BL.VA009_BatchLines_ID, 
                                        BPBA.A_Name,BPBA.RoutingNo,BPBA.AccountNo FROM VA009_BatchLineDetails BLD
                                        INNER JOIN VA009_BatchLines BL ON BLD.VA009_BatchLines_ID = BL.VA009_BatchLines_ID
                                        INNER JOIN VA009_Batch B ON BL.VA009_Batch_ID=B.VA009_Batch_ID
                                        INNER JOIN C_BP_BankAccount BPBA ON BLD.C_BP_BankAccount_ID=BPBA.C_BP_BankAccount_ID
                                        WHERE B.VA009_Batch_ID = " + _Bt.GetVA009_Batch_ID() + @" AND BPBA.IsActive='Y'
                                        GROUP BY BL.VA009_BatchLines_ID, BLD.C_BP_BankAccount_ID, BPBA.A_Name, 
                                        BPBA.RoutingNo, BPBA.AccountNo", null, trx);
                        if (ds != null && ds.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                MVA009BatchLines line = new MVA009BatchLines(ct, Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_BatchLines_ID"]), trx);
                                line.Set_Value("C_BP_BankAccount_ID", Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BP_BankAccount_ID"]));
                                line.Set_ValueNoCheck("A_Name", Util.GetValueOfString(ds.Tables[0].Rows[i]["a_name"]));
                                line.Set_ValueNoCheck("RoutingNo", Util.GetValueOfString(ds.Tables[0].Rows[i]["RoutingNo"]));
                                line.Set_ValueNoCheck("AccountNo", Util.GetValueOfString(ds.Tables[0].Rows[i]["AccountNo"]));
                                if (!line.Save(trx))
                                {
                                    trx.Rollback();
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    //some times getting the error pp also
                                    //Check first GetName() then GetValue() to get proper Error Message
                                    string error = pp != null ? pp.ToString() ?? pp.GetName() : "";
                                    if (string.IsNullOrEmpty(error))
                                    {
                                        error = pp != null ? pp.GetValue() : "";
                                    }
                                    error = !string.IsNullOrEmpty(error) ? error : Msg.GetMsg(ct, "VA009_BatchNotCrtd");
                                    if (string.IsNullOrEmpty(ex.ToString()))
                                    {
                                        ex.Append(error);
                                    }
                                    else
                                    {
                                        ex.Append(", " + error);
                                    }
                                    _log.Info(ex.ToString());
                                    trx.Close();
                                    trx = null;
                                    return ex.ToString();
                                }
                            }
                        }
                    }

                    if (BtachId.Count > 0)
                    {
                        for (int j = 0; j < BtachId.Count; j++)
                        {
                            MVA009Batch _batchComp = new MVA009Batch(ct, BtachId[j], trx);
                            // MVA009PaymentMethod _payMthd = new MVA009PaymentMethod(ct, _batchComp.GetVA009_PaymentMethod_ID(), trx);
                            paymethodDetails = GetPaymentMethodDetails(ct, _batchComp.GetVA009_PaymentMethod_ID(), trx);
                            if (docno.Length > 0)
                                docno.Append(".");

                            docno.Append(_batchComp.GetDocumentNo());
                            if (paymethodDetails["VA009_PaymentRule"].ToString().Equals("M"))
                            {
                                if (Util.GetValueOfBool(paymethodDetails["VA009_InitiatePay"].ToString()))
                                {

                                    VA009_CreatePayments payment = new VA009_CreatePayments();
                                    return payment.DoIt(BtachId[j], ct, trx, PaymentData[0].CurrencyType);
                                }
                            }
                            else if (paymethodDetails["VA009_PaymentRule"].ToString().Equals("E"))
                            {
                                if (Util.GetValueOfBool(paymethodDetails["VA009_InitiatePay"].ToString()))
                                {
                                    VA009_CreatePayments payment = new VA009_CreatePayments();
                                    return payment.DoIt(BtachId[j], ct, trx, PaymentData[0].CurrencyType);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                trx.Rollback();
                ex.Append(e.Message);
                _log.Info(e.Message);
            }
            finally
            {
                trx.Commit();
                trx.Close();
            }

            if (docno.Length > 0)
            {//changed msg 
                msg = Msg.GetMsg(ct, "VA009_BatchCompletedWith");
                msg += docno.ToString();
                ex.Append(msg);
            }

            return ex.ToString();
        }

        /// <summary>
        /// To get Details of payment method
        /// </summary>
        /// <param name="ct">Context object</param>
        /// <param name="VA009_PaymentMethod_ID">Payment method id</param>
        /// <returns>details of payment method</returns>
        public Dictionary<string, object> GetPaymentMethodDetails(Ctx ct, int VA009_PaymentMethod_ID, Trx trx)
        {
            StringBuilder sql = new StringBuilder(@"SELECT VA009_PaymentMethod.VA009_PaymentMethod_ID,
            VA009_PaymentMethod.VA009_PaymentType,VA009_PaymentMethod.VA009_PaymentRule, 
            VA009_PaymentMethod.VA009_PaymentTrigger, VA009_PaymentMethod.VA009_InitiatePay 
            FROM VA009_PaymentMethod VA009_PaymentMethod WHERE VA009_PaymentMethod.IsActive='Y' 
            AND VA009_PaymentMethod.VA009_PaymentMethod_ID = " + VA009_PaymentMethod_ID + " ");
            DataSet ds = DB.ExecuteDataset(MRole.GetDefault(ct).AddAccessSQL(sql.ToString(), "VA009_PaymentMethod", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO), null, trx);
            Dictionary<string, object> obj = new Dictionary<string, object>();
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                obj["VA009_PaymentMethod_ID"] = Util.GetValueOfInt(ds.Tables[0].Rows[0]["VA009_PaymentMethod_ID"]);
                obj["VA009_PaymentType"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PaymentType"]);
                obj["VA009_PaymentRule"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PaymentRule"]);
                obj["VA009_PaymentTrigger"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PaymentTrigger"]);
                obj["VA009_InitiatePay"] = Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_InitiatePay"]).Equals("Y") ? true : false;
            }
            return obj;
        }

        /// <summary>
        /// Get C_DocType_ID against Batch Payment
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="org_id">Org ID</param>
        /// <param name="trx">Trx</param>
        /// <returns>C_DocType_ID</returns>
        public int getDocumentTypeID(Ctx ct, int org_id, Trx trx)
        {
            int ID = Util.GetValueOfInt(DB.ExecuteScalar(" SELECT NVL(Max(C_DocType_ID),0) FROM C_DocType WHERE DocBaseType IN ('BAP') AND AD_Org_ID = " + org_id, null, trx));
            if (ID == 0)
            {
                ID = Util.GetValueOfInt(DB.ExecuteScalar(" SELECT NVL(Max(C_DocType_ID),0) FROM C_DocType WHERE DocBaseType IN ('BAP') AND AD_Org_ID = 0", null, trx));
            }
            return ID;
        }

        /// <summary>
        /// This function is used to check the conversion rate availabe or not
        /// </summary>
        /// <param name="ctx">context</param>
        /// <param name="fields">bank, currencyTo, ConversionType, Date, client , org</param>
        /// <returns>Conversion rate</returns>
        public Decimal CheckConversionRate(Ctx ctx, string fields)
        {
            string[] paramValue = fields.Split(',');
            int CurFrom_ID;
            int CurTo_ID;
            DateTime? convDate;
            int ConversionType_ID;
            int AD_Client_ID;
            int AD_Org_ID;
            int bankAccountId = 0;

            bankAccountId = Util.GetValueOfInt(paramValue[0].ToString());
            CurFrom_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_Currency_ID FROM C_BankAccount WHERE C_BankAccount_ID = " + bankAccountId));
            CurTo_ID = Util.GetValueOfInt(paramValue[1].ToString());

            try
            {
                convDate = System.Convert.ToDateTime(paramValue[2].ToString());
            }
            catch
            {
                convDate = DateTime.Now;
            }

            ConversionType_ID = Util.GetValueOfInt(paramValue[3].ToString());
            AD_Client_ID = Util.GetValueOfInt(paramValue[4].ToString());
            AD_Org_ID = Util.GetValueOfInt(paramValue[5].ToString());

            Decimal rate = MConversionRate.GetRate(CurFrom_ID, CurTo_ID, convDate, ConversionType_ID, AD_Client_ID, AD_Org_ID);
            return rate;
        }

        /// <summary>
        /// Get C_DocType_ID against Batch Payment
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="org_id">Org ID</param>
        /// <param name="baseType">1->AP Receipt 2->AP Payment 3->Cash Journal 4->Batch Payment</param>
        /// <returns>List of Document Types</returns>
        public List<DocTypeDetails> GetTargetType(Ctx ct, int org_id, int baseType)
        {
            List<DocTypeDetails> _list = new List<DocTypeDetails>();
            DocTypeDetails docTypes = null;
            //applied filter with Client ID
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT C_DocType_ID, Name FROM C_DocType C_DocType WHERE IsActive='Y' AND AD_Org_ID IN(" + org_id + ", 0) AND AD_Client_ID IN(" + ct.GetAD_Client_ID() + ", 0)");
            //AP Receipt 2->AP Payment 3->Cash Journal 4->Batch Payment
            if (baseType == 1)
            {
                sql.Append(" AND DocBaseType='ARR' ");
            }
            else if (baseType == 2)
            {
                sql.Append(" AND DocBaseType='APP' ");
            }
            else if (baseType == 3)
            {
                sql.Append(" AND DocBaseType='CMC' ");
            }
            else if (baseType == 4)
            {
                sql.Append(" AND DocBaseType='BAP' ");
            }
            //Check Role
            string query = MRole.GetDefault(ct).AddAccessSQL(sql.ToString(), "C_DocType", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet _ds = DB.ExecuteDataset(query, null, null);
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    docTypes = new DocTypeDetails();
                    docTypes.C_DocType_ID = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_DocType_ID"]);
                    docTypes.Name = Util.GetValueOfString(_ds.Tables[0].Rows[i]["Name"]);
                    _list.Add(docTypes);
                }
            }
            return _list;
        }

        /// <summary>
        /// Get C_DocType_ID based on Bank Account Organization
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="BankAcct_ID">Bank Account</param>
        /// <param name="BaseType">1->AP Receipt 2->AP Payment 3->Cash Journal 4->Batch Payment</param>
        /// <writer>1052</writer>
        /// <returns>List of Document Types</returns>
        public List<DocTypeDetails> GetBankTargetType(Ctx ct, int BankAcct_ID, int BaseType)
        {
            List<DocTypeDetails> _list = new List<DocTypeDetails>();
            DocTypeDetails docTypes = null;
            //applied filter with Client ID
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT C_DocType_ID, Name FROM C_DocType C_DocType WHERE IsActive='Y' AND AD_Org_ID IN" +
                "((SELECT AD_Org_ID FROM C_BankAccount WHERE C_BankAccount_ID= " + BankAcct_ID + "), 0) " +
                "AND AD_Client_ID IN(" + ct.GetAD_Client_ID() + ", 0)");

            //AP Receipt 2->AP Payment 3->Cash Journal 4->Batch Payment
            if (BaseType == 1)
            {
                sql.Append(" AND DocBaseType='ARR' ");
            }
            else if (BaseType == 2)
            {
                sql.Append(" AND DocBaseType='APP' ");
            }
            else if (BaseType == 3)
            {
                sql.Append(" AND DocBaseType='CMC' ");
            }
            else if (BaseType == 4)
            {
                sql.Append(" AND DocBaseType='BAP' ");
            }
            //Check Role
            string query = MRole.GetDefault(ct).AddAccessSQL(sql.ToString(), "C_DocType", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet _ds = DB.ExecuteDataset(query, null, null);
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    docTypes = new DocTypeDetails();
                    docTypes.C_DocType_ID = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_DocType_ID"]);
                    docTypes.Name = Util.GetValueOfString(_ds.Tables[0].Rows[i]["Name"]);
                    _list.Add(docTypes);
                }
            }
            return _list;
        }

        /// <summary>
        /// To create batch and Batch lines
        /// </summary>
        /// <param name="ct">Context Object</param>
        /// <param name="PaymentData">Payment Data (Object no of Records)</param>
        /// <returns>Batch Document Number</returns>
        public string CratePaymentsForBatchWithMaxCount(Ctx ct, GeneratePaymt[] PaymentData)
        {
            #region varriables 
            Trx trx = Trx.GetTrx("Batch_" + DateTime.Now.ToString("yyMMddHHmmssff"));
            List<int> BpList = new List<int>();
            List<int> BtachId = new List<int>();
            BPLocDetails loc = null;
            List<BPLocDetails> BpLoc = new List<BPLocDetails>();
            StringBuilder docno = new StringBuilder();
            StringBuilder ex = new StringBuilder();
            MBankAccount _BankAcct = null;
            List<int> PaymentMethodIDS = new List<int>();
            string isconsolidate = string.Empty;
            string isOverwrite = string.Empty;
            int batchid = 0; MVA009Batch _Bt = null;
            decimal convertedAmount = 0;
            String _TransactionType = String.Empty;
            StringBuilder _sql = new StringBuilder();
            int C_Doctype_ID = 0, paymentmethdoID = 0, Line_MaxCount = 0, total_LineCount = 0;
            Dictionary<string, object> paymethodDetails = null;
            List<CheckDetails> _bankDoc_chequeDT = null;
            #endregion

            try
            {
                if (PaymentData.Length > 0)
                {
                    _BankAcct = new MBankAccount(ct, PaymentData[0].C_BankAccount_ID, trx);
                    isconsolidate = PaymentData[0].isconsolidate;
                    isOverwrite = PaymentData[0].isOverwrite;

                    //if consolidate is true then get the details from Bank Dcoument against payment method
                    if (isconsolidate.ToUpper().Equals("Y"))
                    {
                        //it will contain current check number, CHeck number auto control, Max Batch Line count 
                        //BatchLineDetailCount is used to create maximum lines on Batch Line Details Tab.
                        _bankDoc_chequeDT = DBFuncCollection.GetDetailsofChequeForBatch(PaymentData[0].C_BankAccount_ID, PaymentData[0].VA009_PaymentMethod_ID, null);
                        if (_bankDoc_chequeDT != null && _bankDoc_chequeDT.Count > 0)
                        {
                            if (Util.GetValueOfString(_bankDoc_chequeDT[0].chknoautocontrol).ToUpper().Equals("Y"))
                            {
                                Line_MaxCount = Util.GetValueOfInt(_bankDoc_chequeDT[0].va009_batchlinedetailcount);
                            }
                        }
                    }

                    for (int i = 0; i < PaymentData.Length; i++)
                    {
                        paymentmethdoID = PaymentData[i].VA009_PaymentMethod_ID; convertedAmount = PaymentData[i].convertedAmt;
                        paymethodDetails = GetPaymentMethodDetails(ct, paymentmethdoID, trx);
                        _TransactionType = String.Empty;
                        _TransactionType = PaymentData[i].TransactionType;
                        loc = new BPLocDetails();
                        loc.BP_ID = PaymentData[i].C_BPartner_ID;
                        loc.BP_Loc_ID = PaymentData[i].C_BPartner_Location_ID;
                        loc.Total_Lines_Count = 0;
                        #region Create Batch
                        if (i == 0)
                        {
                            _Bt = createBatchHeader(ct, _Bt, PaymentData[0], paymethodDetails, isconsolidate, isOverwrite, ex, trx);
                            if (_Bt != null)
                            {
                                total_LineCount = 0;
                                batchid = _Bt.GetVA009_Batch_ID();
                                BtachId.Add(batchid);
                                PaymentMethodIDS.Add(paymentmethdoID);
                            }
                        }
                        #endregion

                        #region Create Batch Lines and Details
                        if (batchid > 0)
                        {
                            if (_TransactionType.Equals("Invoice"))
                            {
                                MInvoicePaySchedule _invpaySchdule = new MInvoicePaySchedule(ct, PaymentData[i].C_InvoicePaySchedule_ID, trx);
                                MDocType _doctype = MDocType.Get(ct, _invpaySchdule.GetC_DocType_ID());
                                //removed condition of Cheque Payment method Suggested by Ashish and Rajni
                                // && (paymethodDetails["VA009_PaymentType"].ToString() != "S")
                                if (Line_MaxCount == 0 ?
                                        (BpList.Contains(PaymentData[i].C_BPartner_ID) &&
                                        (PaymentMethodIDS.Contains(paymentmethdoID))) :
                                    (BpList.Contains(PaymentData[i].C_BPartner_ID) &&
                                    (PaymentMethodIDS.Contains(paymentmethdoID)) &&
                                    (BpLoc.Find(x => x.BP_ID == loc.BP_ID &&
                                    x.BP_Loc_ID == loc.BP_Loc_ID &&
                                    x.Total_Lines_Count < Line_MaxCount) != null)))
                                {
                                    #region BatchLine and Batch Line Details
                                    _sql.Clear();
                                    _sql.Append(@"SELECT MAX(VA009_BatchLines_ID) FROM VA009_BatchLines WHERE 
                                                  VA009_Batch_ID=" + _Bt.GetVA009_Batch_ID() +
                                                  " AND C_BPartner_ID=" + PaymentData[i].C_BPartner_ID);
                                    //VIS0045 : check Batch line created with selected BP Location
                                    if ("API".Equals(_doctype.GetDocBaseType()) || "APC".Equals(_doctype.GetDocBaseType()))
                                    {
                                        _sql.Append(" AND NVL(VA009_PaymentLocation_ID, 0) = " + PaymentData[i].C_BPartner_Location_ID);
                                    }
                                    else if ("ARI".Equals(_doctype.GetDocBaseType()) || "ARC".Equals(_doctype.GetDocBaseType()))
                                    {
                                        _sql.Append(" AND NVL(VA009_ReceiptLocation_ID, 0) = " + PaymentData[i].C_BPartner_Location_ID);
                                    }
                                    int Batchline_ID = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, trx));

                                    if (Batchline_ID == 0)
                                    {
                                        Batchline_ID = GenerateBatchLine(ct, PaymentData[i], _Bt, trx, _doctype.GetDocBaseType());
                                        if (Batchline_ID == 0)
                                        {
                                            trx.Rollback();
                                            ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                            {
                                                ex.Append(", " + pp.GetName());
                                            }
                                            _log.Info(ex.ToString());
                                        }
                                    }

                                    if (GenerateBatchLineDetails(ct, PaymentData[i], _Bt, _BankAcct, _invpaySchdule, _doctype, convertedAmount, paymentmethdoID, Batchline_ID, isOverwrite, trx) == 0)
                                    {
                                        trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }
                                    else
                                    {
                                        if (Line_MaxCount > 0)
                                        {
                                            total_LineCount = total_LineCount + 1;
                                            loc = BpLoc.Find(x => x.BP_ID == loc.BP_ID &&
                                      x.BP_Loc_ID == loc.BP_Loc_ID &&
                                      x.Total_Lines_Count < Line_MaxCount);
                                            loc.Total_Lines_Count = total_LineCount;
                                        }
                                        continue;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    BpList.Add(PaymentData[i].C_BPartner_ID);
                                    BpLoc.Add(loc);
                                    int Batchline_ID = GenerateBatchLine(ct, PaymentData[i], _Bt, trx, _doctype.GetDocBaseType());
                                    if (Batchline_ID == 0)
                                    {
                                        trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }
                                    else
                                    {
                                        //Rakesh(VA228):Set batch line detail
                                        if (GenerateBatchLineDetails(ct, PaymentData[i], _Bt, _BankAcct, _invpaySchdule, _doctype, convertedAmount, paymentmethdoID, Batchline_ID, isOverwrite, trx) == 0)
                                        {
                                            trx.Rollback();
                                            ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                            {
                                                ex.Append(", " + pp.GetName());
                                            }
                                            _log.Info(ex.ToString());
                                        }
                                        else
                                        {
                                            if (Line_MaxCount > 0)
                                            {
                                                total_LineCount = 1;
                                                loc = BpLoc.Find(x => x.BP_ID == loc.BP_ID &&
                                          x.BP_Loc_ID == loc.BP_Loc_ID &&
                                          x.Total_Lines_Count < Line_MaxCount);
                                                loc.Total_Lines_Count = total_LineCount;
                                            }
                                        }
                                    }
                                }
                            }

                            if (_TransactionType.Equals("Order"))
                            {
                                MVA009OrderPaySchedule _OrdPaySchdule = new MVA009OrderPaySchedule(ct, PaymentData[i].C_InvoicePaySchedule_ID, trx);
                                MDocType _doctype = MDocType.Get(ct, _OrdPaySchdule.GetC_DocType_ID());
                                if (_doctype.Get_ID() <= 0)
                                {
                                    C_Doctype_ID = Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT C_DocTypeTarget_ID FROM C_Order
                                    WHERE C_Order_ID = " + _OrdPaySchdule.GetC_Order_ID()));
                                    _doctype = MDocType.Get(ct, C_Doctype_ID);
                                }

                                if (Line_MaxCount == 0 ?
                                        (BpList.Contains(PaymentData[i].C_BPartner_ID) &&
                                        (PaymentMethodIDS.Contains(paymentmethdoID))) :
                                        (BpList.Contains(PaymentData[i].C_BPartner_ID) &&
                                    (PaymentMethodIDS.Contains(paymentmethdoID)) &&
                                    (BpLoc.Find(x => x.BP_ID == loc.BP_ID &&
                                    x.BP_Loc_ID == loc.BP_Loc_ID &&
                                    x.Total_Lines_Count < Line_MaxCount) != null)))
                                {
                                    #region BatchLine and Batch Line Details
                                    _sql.Clear();
                                    _sql.Append(@"SELECT MAX(VA009_BatchLines_ID) FROM VA009_BatchLines WHERE 
                                                  VA009_Batch_ID=" + _Bt.GetVA009_Batch_ID() +
                                                  " AND C_BPartner_ID=" + PaymentData[i].C_BPartner_ID);
                                    //VIS0045 : check Batch line created with selected BP Location
                                    if ("POO".Equals(_doctype.GetDocBaseType()))
                                    {
                                        _sql.Append(" AND NVL(VA009_PaymentLocation_ID, 0) = " + PaymentData[i].C_BPartner_Location_ID);
                                    }
                                    else if ("SOO".Equals(_doctype.GetDocBaseType()))
                                    {
                                        _sql.Append(" AND NVL(VA009_ReceiptLocation_ID, 0) = " + PaymentData[i].C_BPartner_Location_ID);
                                    }
                                    int Batchline_ID = Util.GetValueOfInt(DB.ExecuteScalar(_sql.ToString(), null, trx));

                                    if (Batchline_ID == 0)
                                    {
                                        Batchline_ID = GenerateBatchLine(ct, PaymentData[i], _Bt, trx, _doctype.GetDocBaseType());
                                        if (Batchline_ID == 0)
                                        {
                                            trx.Rollback();
                                            ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            if (pp != null)
                                            {
                                                ex.Append(", " + pp.GetName());
                                            }
                                            _log.Info(ex.ToString());
                                        }
                                    }

                                    if (GenerateBatchOrdLineDetails(ct, PaymentData[i], _Bt, _BankAcct, _OrdPaySchdule, _doctype, convertedAmount, paymentmethdoID, Batchline_ID, isOverwrite, trx) == 0)
                                    {
                                        trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }
                                    #endregion
                                    else
                                    {
                                        if (Line_MaxCount > 0)
                                        {
                                            total_LineCount = total_LineCount + 1;
                                            loc = BpLoc.Find(x => x.BP_ID == loc.BP_ID &&
                                      x.BP_Loc_ID == loc.BP_Loc_ID &&
                                      x.Total_Lines_Count < Line_MaxCount);
                                            loc.Total_Lines_Count = total_LineCount;
                                        }
                                        continue;
                                    }
                                }
                                else
                                {
                                    #region BatchLine AND Batch Line Details
                                    BpList.Add(PaymentData[i].C_BPartner_ID);
                                    BpLoc.Add(loc);
                                    int Batchline_ID = GenerateBatchLine(ct, PaymentData[i], _Bt, trx, _doctype.GetDocBaseType());
                                    if (Batchline_ID == 0)
                                    {
                                        trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }

                                    if (GenerateBatchOrdLineDetails(ct, PaymentData[i], _Bt, _BankAcct, _OrdPaySchdule, _doctype, convertedAmount, paymentmethdoID, Batchline_ID, isOverwrite, trx) == 0)
                                    {
                                        trx.Rollback();
                                        ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        if (pp != null)
                                        {
                                            ex.Append(", " + pp.GetName());
                                        }
                                        _log.Info(ex.ToString());
                                    }
                                    else
                                    {
                                        if (Line_MaxCount > 0)
                                        {
                                            total_LineCount = 1;
                                            loc = BpLoc.Find(x => x.BP_ID == loc.BP_ID &&
                                      x.BP_Loc_ID == loc.BP_Loc_ID &&
                                      x.Total_Lines_Count < Line_MaxCount);
                                            loc.Total_Lines_Count = total_LineCount;
                                        }
                                    }
                                    #endregion
                                }
                            }

                        }
                        #endregion
                    }

                    //to check if payment method is CHECK then skip otherwise set these values
                    string _baseType = Util.GetValueOfString(DB.ExecuteScalar(@"SELECT VA009_PaymentBaseType FROM VA009_PaymentMethod WHERE 
                                VA009_PaymentMethod_ID=" + _Bt.GetVA009_PaymentMethod_ID(), null, trx));
                    //Updating the C_BP_BankAccount_ID on Batch Lines Tab
                    if (_Bt != null && !X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Check.Equals(_baseType) && !X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Cash.Equals(_baseType))
                    {
                        DataSet ds = DB.ExecuteDataset(@"SELECT MAX(BLD.C_BP_BankAccount_ID) AS C_BP_BankAccount_ID,BL.VA009_BatchLines_ID, 
                                        BPBA.A_Name,BPBA.RoutingNo,BPBA.AccountNo FROM VA009_BatchLineDetails BLD
                                        INNER JOIN VA009_BatchLines BL ON BLD.VA009_BatchLines_ID = BL.VA009_BatchLines_ID
                                        INNER JOIN VA009_Batch B ON BL.VA009_Batch_ID=B.VA009_Batch_ID
                                        INNER JOIN C_BP_BankAccount BPBA ON BLD.C_BP_BankAccount_ID=BPBA.C_BP_BankAccount_ID
                                        WHERE B.VA009_Batch_ID = " + _Bt.GetVA009_Batch_ID() + @" AND BPBA.IsActive='Y'
                                        GROUP BY BL.VA009_BatchLines_ID, BLD.C_BP_BankAccount_ID, BPBA.A_Name, 
                                        BPBA.RoutingNo, BPBA.AccountNo", null, trx);
                        if (ds != null && ds.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                MVA009BatchLines line = new MVA009BatchLines(ct, Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_BatchLines_ID"]), trx);
                                line.Set_Value("C_BP_BankAccount_ID", Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_BP_BankAccount_ID"]));
                                line.Set_ValueNoCheck("A_Name", Util.GetValueOfString(ds.Tables[0].Rows[i]["a_name"]));
                                line.Set_ValueNoCheck("RoutingNo", Util.GetValueOfString(ds.Tables[0].Rows[i]["RoutingNo"]));
                                line.Set_ValueNoCheck("AccountNo", Util.GetValueOfString(ds.Tables[0].Rows[i]["AccountNo"]));
                                if (!line.Save(trx))
                                {
                                    trx.Rollback();
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    //some times getting the error pp also
                                    //Check first GetName() then GetValue() to get proper Error Message
                                    string error = pp != null ? pp.ToString() ?? pp.GetName() : "";
                                    if (string.IsNullOrEmpty(error))
                                    {
                                        error = pp != null ? pp.GetValue() : "";
                                    }
                                    error = !string.IsNullOrEmpty(error) ? error : Msg.GetMsg(ct, "VA009_BatchNotCrtd");
                                    if (string.IsNullOrEmpty(ex.ToString()))
                                    {
                                        ex.Append(error);
                                    }
                                    else
                                    {
                                        ex.Append(", " + error);
                                    }
                                    _log.Info(ex.ToString());
                                    trx.Close();
                                    trx = null;
                                    return ex.ToString();
                                }
                            }
                        }
                    }

                    if (BtachId.Count > 0)
                    {
                        for (int j = 0; j < BtachId.Count; j++)
                        {
                            MVA009Batch _batchComp = new MVA009Batch(ct, BtachId[j], trx);
                            // MVA009PaymentMethod _payMthd = new MVA009PaymentMethod(ct, _batchComp.GetVA009_PaymentMethod_ID(), trx);
                            paymethodDetails = GetPaymentMethodDetails(ct, _batchComp.GetVA009_PaymentMethod_ID(), trx);
                            if (docno.Length > 0)
                                docno.Append(".");

                            docno.Append(_batchComp.GetDocumentNo());
                            if (paymethodDetails["VA009_PaymentRule"].ToString().Equals("M"))
                            {
                                if (Util.GetValueOfBool(paymethodDetails["VA009_InitiatePay"].ToString()))
                                {

                                    VA009_CreatePayments payment = new VA009_CreatePayments();
                                    return payment.DoIt(BtachId[j], ct, trx, PaymentData[0].CurrencyType);
                                }
                            }
                            else if (paymethodDetails["VA009_PaymentRule"].ToString().Equals("E"))
                            {
                                if (Util.GetValueOfBool(paymethodDetails["VA009_InitiatePay"].ToString()))
                                {
                                    VA009_CreatePayments payment = new VA009_CreatePayments();
                                    return payment.DoIt(BtachId[j], ct, trx, PaymentData[0].CurrencyType);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                trx.Rollback();
                ex.Append(e.Message);
                _log.Info(e.Message);
            }
            finally
            {
                trx.Commit();
                trx.Close();
            }

            if (docno.Length > 0)
            {
                ex.Append(Msg.GetMsg(ct, "VA009_BatchCompletedWith") + docno.ToString());
            }

            return ex.ToString();
        }

        /// <summary>
        /// to create new batch header and return the object of batch header
        /// </summary>
        /// <param name="ct">context</param>
        /// <param name="_Bt">batch class object</param>
        /// <param name="PaymentData">array</param>
        /// <param name="paymethodDetails">payment details array</param>
        /// <param name="isconsolidate">is Consolidated</param>
        /// <param name="isOverwrite">Y if want to overwrite payment method</param>
        /// <param name="ex">exception string</param>
        /// <param name="trx">transaction object</param>
        /// <returns>batch MClass object</returns>
        public MVA009Batch createBatchHeader(Ctx ct, MVA009Batch _Bt, GeneratePaymt PaymentData, Dictionary<string, object> paymethodDetails,
            string isconsolidate, string isOverwrite, StringBuilder ex, Trx trx)
        {
            _Bt = new MVA009Batch(ct, 0, trx);
            _Bt.SetC_Bank_ID(PaymentData.C_Bank_ID);
            _Bt.SetC_BankAccount_ID(PaymentData.C_BankAccount_ID);
            _Bt.SetAD_Client_ID(PaymentData.AD_Client_ID);
            _Bt.SetAD_Org_ID(PaymentData.AD_Org_ID);
            _Bt.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(paymethodDetails["VA009_PaymentMethod_ID"]));
            //to set document type against batch payment
            //_Bt.Set_ValueNoCheck("C_DocType_ID", getDocumentTypeID(ct, PaymentData[0].AD_Org_ID, trx));
            //Target Document Type selected by the User
            _Bt.Set_ValueNoCheck("C_DocType_ID", PaymentData.TargetDocType);
            //end
            _Bt.SetVA009_PaymentRule(paymethodDetails["VA009_PaymentRule"].ToString());
            _Bt.SetVA009_PaymentTrigger(paymethodDetails["VA009_PaymentTrigger"].ToString());
            //to set bank currency on Payment Batch given by Rajni and Ashish
            _Bt.Set_Value("C_Currency_ID", PaymentData.HeaderCurrency);
            _Bt.Set_Value("C_ConversionType_ID", PaymentData.CurrencyType);
            // _Bt.SetProcessed(true);
            _Bt.SetVA009_DocumentDate(DateTime.Now);
            //VA230:Set account date
            _Bt.SetDateAcct(PaymentData.DateAcct);
            if (isconsolidate == "Y")
            {
                _Bt.SetVA009_Consolidate(true);
            }
            else
            {
                //Bug175 set consolidated check box false if on form not checked.
                _Bt.SetVA009_Consolidate(false);
            }
            // if overwrite payment method is true then set payment method on Batch
            if (isOverwrite == "Y")
            {
                Dictionary<string, object> paymethod = new Dictionary<string, object>();
                paymethod = GetPaymentMethodDetails(ct, PaymentData.VA009_PaymentMethod_ID, trx);
                _Bt.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(paymethod["VA009_PaymentMethod_ID"].ToString()));
                _Bt.SetVA009_PaymentRule(paymethod["VA009_PaymentRule"].ToString());
                _Bt.SetVA009_PaymentTrigger(paymethod["VA009_PaymentTrigger"].ToString());
            }

            if (!_Bt.Save())
            {
                trx.Rollback();
                ex.Append(Msg.GetMsg(ct, "VA009_PNotSaved"));
                ValueNamePair pp = VLogger.RetrieveError();
                if (pp != null)
                {
                    ex.Append(", " + pp.GetName());
                }
                _log.Info(ex.ToString());
                return null;
            }
            return _Bt;
        }

    }
}
//**************************************
//Properties Classes for Payment Form
//**************************************

public class PaymentResponse
{
    public string _path { get; set; }
    public string _filename { get; set; }
    public string _error { get; set; }
}
public class PaymentData
{
    public int C_BPartner_ID { get; set; }
    public int C_BPartner_Location_ID { get; set; }
    public int C_BP_Group_ID { get; set; }
    public int C_InvoicePaySchedule_ID { get; set; }
    public int C_Currency_ID { get; set; }
    public int VA009_PaymentMethod_ID { get; set; }
    public DateTime? VA009_plannedduedate { get; set; }
    public DateTime? VA009_FollowupDate { get; set; }
    public decimal VA009_RecivedAmt { get; set; }
    public decimal DueAmt { get; set; }
    public decimal BaseAmt { get; set; }
    public string VA009_ExecutionStatus { get; set; }
    public int AD_Org_ID { get; set; }
    public int AD_Client_ID { get; set; }
    public string VA009_PaymentMode { get; set; }
    public int paymentCount { get; set; }
    public string C_Bpartner { get; set; }
    public string C_BP_Group { get; set; }
    public string VA009_PaymentMethod { get; set; }
    public string CurrencyCode { get; set; }
    public string BaseCurrencyCode { get; set; }
    public decimal convertedAmt { get; set; }
    public decimal TotalInvAmt { get; set; }
    public int C_Invoice_ID { get; set; }
    public int recid { get; set; }
    public DateTime? DueDate { get; set; }
    public string ERROR { get; set; }
    public String LastChat { get; set; }
    public string Systemdate { get; set; }
    public string PaymwentBaseType { get; set; }
    public string DocumentNo { get; set; }
    public string TransactionType { get; set; }
    public string PaymentRule { get; set; }
    public string PaymentType { get; set; }
    public string PaymentTriggerBy { get; set; }
    public string DocBaseType { get; set; }
    public string IsHoldPayment { get; set; }
    public DateTime? DateAcct { get; set; }
    public int ConversionTypeId { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ConvertedDiscountAmount { get; set; }
    public DateTime? DiscountDate { get; set; }
}
public class BPDetails
{
    public int C_BPartner_ID { get; set; }
    public string Name { get; set; }
}
public class BankDetails
{
    public string BankName { get; set; }
    public int C_Bank_ID { get; set; }
    public int C_BankAccount_ID { get; set; }
    public String BankAccountNumber { get; set; }
    public string CurrencyCode1 { get; set; }
    public decimal TotalAmt { get; set; }
    public decimal UnreconsiledAmt { get; set; }
    public decimal CurrentBalance { get; set; }
    //public Dictionary<string, decimal> TotalAmtBank { get; set; }
}
public class LoadData
{
    public List<PaymentData> paymentdata { get; set; }
    public List<BankDetails> bankdetails { get; set; }
    public List<CashBook> Cbk { get; set; }
}
public class CashBook
{
    public string CBCurrencyCode { get; set; }
    public int C_Cashbook_ID { get; set; }
    public string CashBookName { get; set; }
    public decimal Csb_Amt { get; set; }

}
public class GeneratePaymt
{
    public int C_BPartner_ID { get; set; }
    public int C_BPartner_Location_ID { get; set; }
    public string Description { get; set; }
    public int C_Invoice_ID { get; set; }
    public string ValidMonths { get; set; }
    public int C_Currency_ID { get; set; }
    public int AD_Org_ID { get; set; }
    public int AD_Client_ID { get; set; }
    public decimal VA009_RecivedAmt { get; set; }
    public int VA009_PaymentMethod_ID { get; set; }
    public decimal DueAmt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CheckDate { get; set; }
    public DateTime? DateAcct { get; set; }
    //Date Trx
    public DateTime? DateTrx { get; set; }
    public string CheckNumber { get; set; }
    public int C_BankAccount_ID { get; set; }
    public int C_Bank_ID { get; set; }
    public int C_CashBook_ID { get; set; }
    public int C_InvoicePaySchedule_ID { get; set; }
    public string C_Bpartner { get; set; }
    public string ISO_CODE { get; set; }
    public string isconsolidate { get; set; }
    public string isOverwrite { get; set; }
    public string From { get; set; }
    public string CurrencyCode { get; set; }
    public int CurrencyType { get; set; }
    //change by Amit
    public Decimal Discount { get; set; }
    public Decimal Writeoff { get; set; }
    public Decimal OverUnder { get; set; }
    public string PaymwentBaseType { get; set; }
    public decimal convertedAmt { get; set; }
    public string TransactionType { get; set; }
    public int TargetDocType { get; set; }
    public int HeaderCurrency { get; set; }
    public int ConversionTypeId { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ConvertedDiscountAmount { get; set; }
    public DateTime? DiscountDate { get; set; }
    public bool IsOverrideAutoCheck { get; set; }

    //end
}
public class PayBatchDetails
{
    public string DocumentNo { get; set; }
    public string VA009_DocumentDate { get; set; }
    public string bankName { get; set; }
    public string bankaccount { get; set; }
    public int c_bank_id { get; set; }
    public decimal BaseAmt { get; set; }
    public int c_bankaccount_id { get; set; }
    public int va009_paymentmethod_id { get; set; }
    public string PaymentMethod { get; set; }
    public int c_currency_id { get; set; }
    public string ISO_CODE { get; set; }
    public decimal VA009_ConvertedAmt { get; set; }
}
public class LocationDetails
{
    public int C_Location_ID { get; set; }
    public string Name { get; set; }
}
public class ChargeDetails
{
    public int C_Charge_ID { get; set; }
    public string Name { get; set; }
}
public class DocTypeDetails
{
    public int C_DocType_ID { get; set; }
    public string Name { get; set; }
    public string DocBaseType { get; set; }
}
public class BPLocDetails
{
    public int BP_ID { get; set; }
    public int BP_Loc_ID { get; set; }
    public int Total_Lines_Count { get; set; }
}
