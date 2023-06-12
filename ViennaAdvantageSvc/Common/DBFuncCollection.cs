using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAdvantage.DataBase;
using VAdvantage.Model;
using VAdvantage.Utility;

namespace ViennaAdvantage.Common
{
    public static class DBFuncCollection
    {
        /// <summary>
        /// Get the Query in string format to get Invoice and Order Schedules
        /// </summary>
        /// <param name="ctx">Currenct Context</param>
        /// <param name="whereQry">WHERE CLAUSE query</param>
        /// <param name="SearchText">Search Text</param>
        /// <param name="WhrDueDate">Due Date</param>
        /// <param name="TransType">Transaction Type</param>
        /// <param name="FromDate">From Date</param>
        /// <param name="ToDate">To Date</param>
        /// <returns>returns string value query to get Invoice and Order Schedules</returns>
        public static string GetPaymentDataSql(Ctx ctx, string whereQry, string SearchText, string WhrDueDate, string TransType, string FromDate, string ToDate)
        {
            StringBuilder sql = new StringBuilder();
            DateTime? dateFrom = new DateTime();
            if (FromDate != string.Empty)
                dateFrom = Convert.ToDateTime(FromDate);
            DateTime? dateTo = new DateTime();
            if (ToDate != string.Empty)
                dateTo = Convert.ToDateTime(ToDate);
            int[] TransTypes;
            if (!string.IsNullOrEmpty(TransType))
            {
                TransTypes = Array.ConvertAll(TransType.Split(','), int.Parse);
            }
            else
            {
                TransTypes = new int[0];
            }
            //when load the Schedules should get Converted Amount based on Schedule ConversionType not the default ConversionType
            //int conversionType_ID = ctx.GetContextAsInt("#C_ConversionType_ID");

            if (DB.IsOracle())
            {
                if (TransTypes.Count() == 0 || TransTypes.Count() == 3 || TransType.Contains("1"))
                {
                    //Table Name is case sensitive must follow Camel format
                    sql.Append(@"SELECT t.VA009_PaymentMode,  t.c_Bpartner_id,  t.c_invoice_id,  t.DocumentNo,  t.C_Bpartner,  t.c_bp_group_id,  t.c_bp_group,  
                         t.C_InvoicePaySchedule_ID,  t.VA009_PaymentMethod_ID,  t.VA009_PaymentMethod,  t.va009_paymentbasetype,  t.VA009_PaymentRule,  t.VA009_PaymentType,  t.VA009_PaymentTrigger,
                         t.va009_plannedduedate, t.VA009_FollowupDate,  t.VA009_RecivedAmt,  t.DueAmt, t.VA009_OpenAmnt, t.VA009_ExecutionStatus,  t.ad_org_id,  t.ad_client_id ,  t.C_Currency_ID,  
                         t.ISO_CODE, t.basecurrency, t.multiplyrate, t.Due_Date_Diff, t.basecurrencycode,t.GrandTotal, t.va009_transactiontype, t.IsHoldPayment FROM (");
                    //Log Warnings handled
                    string query = @"SELECT pm.VA009_PaymentMode,cb.c_Bpartner_id, cs.c_invoice_id,inv.DocumentNo, cb.name as C_Bpartner, cb.c_bp_group_id, cbg.name as c_bp_group, cs.C_InvoicePaySchedule_ID,
                         pm.VA009_PaymentMethod_ID, pm.VA009_name as VA009_PaymentMethod,pm.va009_paymentbasetype,pm.VA009_PaymentRule, pm.VA009_PaymentType, pm.VA009_PaymentTrigger,
                         cs.duedate as va009_plannedduedate,
                         cs.VA009_PlannedDueDate as VA009_FollowupDate,inv.VA009_PaidAmount AS VA009_RecivedAmt,
                         CASE WHEN (cd.DOCBASETYPE IN ('ARI','APC')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) WHEN (cd.DOCBASETYPE IN ('API','ARC'))     
                         THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) * 1  END AS DueAmt,
                         cs.VA009_OpenAmnt, rsf.name as VA009_ExecutionStatus,  cs.ad_org_id,  cs.ad_client_id ,
                         inv.C_Currency_ID,  cc.ISO_CODE, ac.c_currency_id as basecurrency,  CURRENCYRATE(cc.C_CURRENCY_ID,cy.C_CURRENCY_ID, cs.DueDate, inv.C_ConversionType_ID,
                         inv.AD_Client_ID,inv.AD_ORG_ID) as multiplyrate, cy.ISO_CODE as basecurrencycode,inv.GrandTotal, (to_date(TO_CHAR(TRUNC(cs.VA009_PlannedDueDate)),'dd/mm/yyyy')
                        -to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy')) as Due_Date_Diff,cs.duedate, 'Invoice' AS VA009_TransactionType, cs.IsHoldPayment FROM 
                         C_InvoicePaySchedule cs INNER JOIN VA009_PaymentMethod pm ON (pm.VA009_PaymentMethod_ID=cs.VA009_PaymentMethod_ID) INNER JOIN C_DocType cd
                         ON (cs.C_DocType_ID=cd.C_DocType_ID) INNER JOIN AD_Ref_List rsf ON (rsf.value= cs.VA009_ExecutionStatus) INNER JOIN AD_Reference re ON 
                         (rsf.AD_Reference_ID=re.AD_Reference_ID) LEFT JOIN C_Invoice inv ON (inv.c_invoice_id=cs.c_invoice_id) LEFT JOIN C_BPartner cb ON 
                         (cb.C_BPartner_ID=inv.C_BPartner_ID) INNER JOIN C_BP_Group cbg ON (cb.C_BP_Group_ID=cbg.C_BP_Group_ID) INNER JOIN C_Currency cc ON 
                         (inv.C_Currency_ID=cc.C_Currency_ID) INNER JOIN AD_ClientInfo aclnt ON (aclnt.AD_Client_ID =cs.AD_Client_ID) INNER JOIN C_AcctSchema ac ON 
                         (ac.C_AcctSchema_ID =aclnt.C_AcctSchema1_ID) INNER JOIN C_Currency CY ON (AC.C_Currency_ID=CY.C_Currency_ID)  " +
                             whereQry + @" AND re.name= 'VA009_ExecutionStatus' AND re.Export_ID='VA009_20000279' AND rsf.value NOT IN ('Y','J')"
                         //AND cs.AD_Client_ID = " + ctx.GetAD_Client_ID() 
                         + " AND NVL(cs.C_Payment_ID , 0) = 0 AND NVL(cs.C_CashLine_ID , 0) = 0 AND cs.VA009_IsPaid = 'N' ";

                    query = MRole.GetDefault(ctx).AddAccessSQL(query, "cs", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
                    sql.Append(query);

                    sql.Append(") t WHERE t.DueAmt !=0 ");
                    string whrduedte = DueDateSearch(WhrDueDate);
                    sql.Append(whrduedte);

                    if (SearchText != string.Empty)
                    {
                        //JID_1793 -- when search text contain "=" then serach with documnet no only
                        if (SearchText.Contains("="))
                        {
                            String[] myStringArray = SearchText.TrimStart(new Char[] { ' ', '=' }).Split(',');
                            if (myStringArray.Length > 0)
                            {
                                sql.Append(" AND UPPER(t.DocumentNo) IN ( ");
                                for (int z = 0; z < myStringArray.Length; z++)
                                {
                                    if (z != 0)
                                    { sql.Append(","); }
                                    sql.Append(" UPPER('" + myStringArray[z].Trim(new Char[] { ' ' }) + "')");
                                }
                                sql.Append(")");
                            }
                        }
                        else
                        {
                            sql.Append(" AND ( UPPER(t.C_Bpartner) LIKE UPPER('%" + SearchText + "%') OR (UPPER(t.c_bp_group) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_PaymentMethod) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_ExecutionStatus) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.DocumentNo) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.DueAmt) LIKE UPPER('%" + SearchText + "%'))  OR (UPPER(to_date(TO_CHAR(TRUNC(t.VA009_FollowupDate)),'dd/mm/yyyy')) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(to_date(TO_CHAR(TRUNC(t.va009_plannedduedate)),'dd/mm/yyyy')) LIKE UPPER('%" + SearchText + "%')) ) ");
                        }
                    }

                    if (FromDate != string.Empty && ToDate != string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate BETWEEN  ");
                        sql.Append(GlobalVariable.TO_DATE(dateFrom, true) + " AND ");
                        sql.Append(GlobalVariable.TO_DATE(dateTo, true));
                    }
                    else if (FromDate != string.Empty && ToDate == string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate >=" + GlobalVariable.TO_DATE(dateFrom, true));
                    }
                    else if (FromDate == string.Empty && ToDate != string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate <=" + GlobalVariable.TO_DATE(dateTo, true));
                    }
                }
                if (TransTypes.Count() == 0 || TransTypes.Count() >= 2)
                {
                    sql.Append(" UNION ");
                }
                if (TransTypes.Count() == 0 || TransTypes.Count() == 3 || TransType.Contains("0"))
                {
                    //Table Name is case sensitive must follow Camel format
                    sql.Append(@"SELECT t.VA009_PaymentMode,  t.c_Bpartner_id,  t.c_invoice_id,  t.DocumentNo,  t.C_Bpartner,  t.c_bp_group_id,  t.c_bp_group,  t.C_InvoicePaySchedule_ID,
                        t.VA009_PaymentMethod_ID,  t.VA009_PaymentMethod,  t.va009_paymentbasetype, t.VA009_PaymentRule,  t.VA009_PaymentType,  t.VA009_PaymentTrigger,  t.va009_plannedduedate, 
                        t.VA009_FollowupDate,  t.VA009_RecivedAmt, t.DueAmt, t.VA009_OpenAmnt,  t.VA009_ExecutionStatus,  t.ad_org_id,  t.ad_client_id ,  t.C_Currency_ID,  t.ISO_CODE,  t.basecurrency, 
                        t.multiplyrate, t.Due_Date_Diff, t.basecurrencycode, t.GrandTotal, t.va009_transactiontype, t.IsHoldPayment FROM ( ");
                    //Log Warnings handled
                    string query = @" SELECT pm.VA009_PaymentMode, cb.c_Bpartner_id, cs.C_Order_ID AS c_invoice_id, inv.DocumentNo, cb.name AS C_Bpartner, cb.c_bp_group_id,
                        cbg.name AS c_bp_group, cs.VA009_OrderPaySchedule_ID AS C_InvoicePaySchedule_ID, pm.VA009_PaymentMethod_ID, pm.VA009_name AS VA009_PaymentMethod, pm.va009_paymentbasetype,
                        pm.VA009_PaymentRule, pm.VA009_PaymentType, pm.VA009_PaymentTrigger, cs.duedate AS va009_plannedduedate, cs.VA009_PlannedDueDate  AS VA009_FollowupDate,    
                        0 AS VA009_RecivedAmt, 
                        CASE  WHEN (cd.DOCBASETYPE IN ('SOO','APC')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) WHEN (cd.DOCBASETYPE IN ('POO','ARC')) 
                        THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) * 1 END AS DueAmt,
                        cs.VA009_OpenAmnt, rsf.name AS VA009_ExecutionStatus, cs.ad_org_id, cs.ad_client_id, inv.C_Currency_ID, cc.ISO_CODE, ac.c_currency_id  AS basecurrency,
                        CURRENCYRATE(cc.C_CURRENCY_ID,cy.C_CURRENCY_ID,cs.DueDate, inv.C_ConversionType_ID,inv.AD_Client_ID,inv.AD_ORG_ID) AS multiplyrate,  cy.ISO_CODE AS basecurrencycode,
                        inv.GrandTotal, (to_date(TO_CHAR(TRUNC(cs.VA009_PlannedDueDate)),'dd/mm/yyyy') -to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy')) AS Due_Date_Diff,
                        cs.duedate, 'Order' AS VA009_TransactionType, 'N' AS IsHoldPayment
                        FROM VA009_OrderPaySchedule cs INNER JOIN VA009_PaymentMethod pm   ON (pm.VA009_PaymentMethod_ID=cs.VA009_PaymentMethod_ID)
                        INNER JOIN AD_Ref_List rsf  ON (rsf.value= cs.VA009_ExecutionStatus)  INNER JOIN AD_Reference re  ON (rsf.AD_Reference_ID=re.AD_Reference_ID
                        AND re.name = 'VA009_ExecutionStatus')  INNER JOIN C_Order inv  ON (inv.C_Order_ID=cs.C_Order_ID)  INNER JOIN C_DocType cd
                        ON (inv.C_DocType_ID=cd.C_DocType_ID)  INNER JOIN C_BPartner cb  ON (cb.C_Bpartner_ID=inv.C_Bpartner_ID)  INNER JOIN C_BP_Group cbg  ON (cb.C_BP_Group_ID=cbg.C_BP_Group_ID)
                        INNER JOIN C_Currency cc  ON (inv.C_Currency_ID=cc.C_Currency_ID)  INNER JOIN AD_ClientInfo aclnt  ON (aclnt.AD_Client_ID =cs.AD_Client_ID)
                        INNER JOIN C_AcctSchema ac  ON (ac.C_AcctSchema_ID =aclnt.C_AcctSchema1_ID)  INNER JOIN C_Currency CY  ON (AC.C_Currency_ID=CY.C_Currency_ID) " +
                            whereQry.Replace("c_invoice_id", "C_Order_ID") + @" AND re.name= 'VA009_ExecutionStatus' AND re.Export_ID='VA009_20000279' AND rsf.value NOT IN ( 'Y','J')"
                        //AND cs.AD_Client_ID = " + ctx.GetAD_Client_ID() + 
                        + " AND NVL(cs.C_Payment_ID , 0) = 0 AND NVL(cs.C_CashLine_ID , 0) = 0 AND cs.VA009_IsPaid = 'N' ";

                    query = MRole.GetDefault(ctx).AddAccessSQL(query, "cs", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
                    sql.Append(query);

                    sql.Append(") t WHERE t.DueAmt !=0 ");
                    string whrduedte = DueDateSearch(WhrDueDate);
                    sql.Append(whrduedte);
                    if (SearchText != string.Empty)
                    {
                        // JID_1793 -- when search text contain "=" then serach with documnet no 
                        if (SearchText.Contains("="))
                        {
                            String[] myStringArray = SearchText.TrimStart(new Char[] { ' ', '=' }).Split(',');
                            if (myStringArray.Length > 0)
                            {
                                sql.Append(" AND UPPER(t.DocumentNo) IN ( ");
                                for (int z = 0; z < myStringArray.Length; z++)
                                {
                                    if (z != 0)
                                    { sql.Append(","); }
                                    sql.Append(" UPPER('" + myStringArray[z].Trim(new Char[] { ' ' }) + "')");
                                }
                                sql.Append(")");
                            }
                        }
                        else
                        {
                            sql.Append(" AND ( UPPER(t.C_Bpartner) LIKE UPPER('%" + SearchText + "%') OR (UPPER(t.c_bp_group) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_PaymentMethod) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_ExecutionStatus) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.DocumentNo) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.DueAmt) LIKE UPPER('%" + SearchText + "%'))  OR (UPPER(to_date(TO_CHAR(TRUNC(t.VA009_FollowupDate)),'dd/mm/yyyy')) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(to_date(TO_CHAR(TRUNC(t.va009_plannedduedate)),'dd/mm/yyyy')) LIKE UPPER('%" + SearchText + "%')) ) ");
                        }
                    }

                    if (FromDate != string.Empty && ToDate != string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate BETWEEN  ");
                        sql.Append(GlobalVariable.TO_DATE(dateFrom, true) + " AND ");
                        sql.Append(GlobalVariable.TO_DATE(dateTo, true));
                    }
                    else if (FromDate != string.Empty && ToDate == string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate >=" + GlobalVariable.TO_DATE(dateFrom, true));
                    }
                    else if (FromDate == string.Empty && ToDate != string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate <=" + GlobalVariable.TO_DATE(dateTo, true));
                    }
                }

                // GET GL Journal Data
                if (TransTypes.Count() == 0 || TransTypes.Count() > 2)
                {
                    sql.Append(" UNION ");
                }
                if (TransTypes.Count() == 0 || TransTypes.Count() == 3 || TransType.Contains("2"))
                {
                    //Table Name is case sensitive must follow Camel format
                    sql.Append(@"SELECT t.VA009_PaymentMode,  t.c_Bpartner_id,  t.c_invoice_id,  t.DocumentNo,  t.C_Bpartner,  t.c_bp_group_id,  
                        t.c_bp_group,  t.C_InvoicePaySchedule_ID, t.VA009_PaymentMethod_ID,  t.VA009_PaymentMethod,  t.va009_paymentbasetype,
                        t.VA009_PaymentRule,  t.VA009_PaymentType,  t.VA009_PaymentTrigger,  t.va009_plannedduedate, 
                        t.VA009_FollowupDate,  t.VA009_RecivedAmt, t.DueAmt, t.DueAmt AS VA009_OpenAmnt, t.VA009_ExecutionStatus,  t.ad_org_id,
                        t.ad_client_id ,  t.C_Currency_ID,  t.ISO_CODE,  t.basecurrency, t.multiplyrate, t.Due_Date_Diff, t.basecurrencycode,
                        t.DueAmt AS GrandTotal, t.va009_transactiontype, t.IsHoldPayment FROM (");
                    //Log Warnings handled
                    string query = $@" SELECT '' AS VA009_PaymentMode, gl.C_BPartner_ID, g.GL_Journal_ID AS c_invoice_id, g.DocumentNo, 
                                        cb.name AS C_Bpartner, cb.c_bp_group_id, cbg.name AS c_bp_group, gl.GL_JournalLine_ID AS C_InvoicePaySchedule_ID,
                                        0 AS VA009_PaymentMethod_ID, null AS VA009_PaymentMethod, '' AS va009_paymentbasetype, 
                                       '' AS VA009_PaymentRule, '' AS VA009_PaymentType, '' AS VA009_PaymentTrigger, g.DateAcct AS va009_plannedduedate,
                                        g.DateAcct AS VA009_FollowupDate, 0 AS VA009_RecivedAmt, 
                                       CASE WHEN (ev.AccountType = 'A' AND AmtSourceDr > 0) THEN AmtSourceDr
                                            WHEN (ev.AccountType = 'A' AND AmtSourceDr <= 0) THEN  -1 * AmtSourceCr
                                            WHEN (ev.AccountType = 'L' AND AmtSourceCr > 0) THEN AmtSourceCr
                                            WHEN (ev.AccountType = 'L' AND AmtSourceCr <= 0) THEN  -1 * AmtSourceDr
                                        END AS DueAmt, 
                                        0 AS VA009_OpenAmnt, null AS VA009_ExecutionStatus, gl.ad_org_id, gl.ad_client_id, gl.C_Currency_ID, cc.ISO_CODE, 
                                        ac.c_currency_id AS basecurrency,
                                         NVL(CURRENCYRATE(gl.C_Currency_ID,cy.C_Currency_ID,g.DateAcct, {ctx.GetContextAsInt("#C_ConversionType_ID")}, gl.AD_Client_ID,gl.AD_ORG_ID), 0) AS multiplyrate,  
                                        cy.ISO_CODE AS basecurrencycode, 0 AS GrandTotal, 
                                        (to_date(TO_CHAR(TRUNC(g.DateAcct)),'dd/mm/yyyy') - to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy')) AS Due_Date_Diff,
                                        g.DateAcct, 'GL Journal' AS VA009_TransactionType, 'N' AS IsHoldPayment
                                  FROM GL_JournalLine gl
                                  INNER JOIN C_ElementValue ev ON (ev.C_ElementValue_ID = gl.Account_ID AND ev.IsAllocationRelated = 'Y')
                                  INNER JOIN GL_Journal g ON (g.GL_Journal_ID = gl.GL_Journal_ID)
                                  INNER JOIN C_BPartner cb  ON (cb.C_Bpartner_ID=gl.C_Bpartner_ID)  
                                  INNER JOIN C_BP_Group cbg  ON (cb.C_BP_Group_ID=cbg.C_BP_Group_ID)
                                  INNER JOIN C_Currency cc  ON (gl.C_Currency_ID=cc.C_Currency_ID)
                                  INNER JOIN AD_ClientInfo aclnt  ON (aclnt.AD_Client_ID =gl.AD_Client_ID)
                                  INNER JOIN C_AcctSchema ac  ON (ac.C_AcctSchema_ID =aclnt.C_AcctSchema1_ID)  
                                  INNER JOIN C_Currency cy  ON (ac.C_Currency_ID=cy.C_Currency_ID) 
                                  WHERE gl.IsAllocated='N' AND ev.IsAllocationRelated = 'Y' AND gl.VA009_IsAssignedtoBatch = 'N' 
                                        AND ev.AccountType IN ({(whereQry.Contains("'ARI'") ? "'A'" : "'L'")} ) 
                                        AND g.docstatus in ('CO','CL')  {(whereQry.IndexOf("cb.") >= 0 ? ("AND " + whereQry.Substring(whereQry.IndexOf("cb.")).Replace("cs" , "gl"))
                                        : (whereQry.IndexOf("cs.AD_Org") >= 0 ? ("AND " + whereQry.Substring(whereQry.IndexOf("cs.AD_Org")).Replace("cs" , "gl")) : ""))} ";

                    query = MRole.GetDefault(ctx).AddAccessSQL(query, "gl", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
                    sql.Append(query);

                    sql.Append(") t WHERE t.DueAmt !=0 ");
                    string whrduedte = DueDateSearch(WhrDueDate);
                    sql.Append(whrduedte);
                    if (SearchText != string.Empty)
                    {
                        if (SearchText.Contains("="))
                        {
                            String[] myStringArray = SearchText.TrimStart(new Char[] { ' ', '=' }).Split(',');
                            if (myStringArray.Length > 0)
                            {
                                sql.Append(" AND UPPER(t.DocumentNo) IN ( ");
                                for (int z = 0; z < myStringArray.Length; z++)
                                {
                                    if (z != 0)
                                    { sql.Append(","); }
                                    sql.Append(" UPPER('" + myStringArray[z].Trim(new Char[] { ' ' }) + "')");
                                }
                                sql.Append(")");
                            }
                        }
                        else
                        {
                            sql.Append(" AND ( UPPER(t.C_Bpartner) LIKE UPPER('%" + SearchText + "%') OR (UPPER(t.c_bp_group) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_PaymentMethod) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_ExecutionStatus) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.DocumentNo) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.DueAmt) LIKE UPPER('%" + SearchText + "%'))  OR (UPPER(to_date(TO_CHAR(TRUNC(t.VA009_FollowupDate)),'dd/mm/yyyy')) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(to_date(TO_CHAR(TRUNC(t.va009_plannedduedate)),'dd/mm/yyyy')) LIKE UPPER('%" + SearchText + "%')) ) ");
                        }
                    }

                    if (FromDate != string.Empty && ToDate != string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate BETWEEN  ");
                        sql.Append(GlobalVariable.TO_DATE(dateFrom, true) + " AND ");
                        sql.Append(GlobalVariable.TO_DATE(dateTo, true));
                    }
                    else if (FromDate != string.Empty && ToDate == string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate >=" + GlobalVariable.TO_DATE(dateFrom, true));
                    }
                    else if (FromDate == string.Empty && ToDate != string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate <=" + GlobalVariable.TO_DATE(dateTo, true));
                    }
                }

            }
            else if (DB.IsPostgreSQL())
            {
                if (TransTypes.Count() == 0 || TransTypes.Count() == 2 || TransTypes[0] == 1)
                {
                    //Table Name is case sensitive must follow Camel format
                    sql.Append(@"SELECT t.VA009_PaymentMode,  t.c_Bpartner_id,  t.c_invoice_id,  t.DocumentNo,  t.C_Bpartner,  t.c_bp_group_id,  t.c_bp_group,  
                         t.C_InvoicePaySchedule_ID,  t.VA009_PaymentMethod_ID,  t.VA009_PaymentMethod,  t.va009_paymentbasetype,  t.VA009_PaymentRule,  t.VA009_PaymentType,  t.VA009_PaymentTrigger,
                         t.va009_plannedduedate, t.VA009_FollowupDate,  t.VA009_RecivedAmt,  t.DueAmt, t.VA009_OpenAmnt, t.VA009_ExecutionStatus,  t.ad_org_id,  t.ad_client_id ,  t.C_Currency_ID,  
                         t.ISO_CODE, t.basecurrency, t.multiplyrate, t.Due_Date_Diff, t.basecurrencycode,t.GrandTotal, t.va009_transactiontype, t.IsHoldPayment FROM (");
                    //Log Warnings handled
                    string query = @"SELECT pm.VA009_PaymentMode,cb.c_Bpartner_id, cs.c_invoice_id,inv.DocumentNo, cb.name as C_Bpartner, cb.c_bp_group_id, cbg.name as c_bp_group, cs.C_InvoicePaySchedule_ID,
                         pm.VA009_PaymentMethod_ID, pm.VA009_name as VA009_PaymentMethod,pm.va009_paymentbasetype,pm.VA009_PaymentRule, pm.VA009_PaymentType, pm.VA009_PaymentTrigger,
                         cs.duedate as va009_plannedduedate,
                         cs.VA009_PlannedDueDate as VA009_FollowupDate,inv.VA009_PaidAmount AS VA009_RecivedAmt,
                         CASE WHEN (cd.DOCBASETYPE IN ('ARI','APC')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) WHEN (cd.DOCBASETYPE IN ('API','ARC'))     
                         THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) * 1  END AS DueAmt,
                         cs.VA009_OpenAmnt, rsf.name as VA009_ExecutionStatus,  cs.ad_org_id,  cs.ad_client_id ,
                         inv.C_Currency_ID,  cc.ISO_CODE, ac.c_currency_id as basecurrency,  CURRENCYRATE(cc.C_CURRENCY_ID,cy.C_CURRENCY_ID, cs.DueDate, inv.C_ConversionType_ID,
                         inv.AD_Client_ID,inv.AD_ORG_ID) as multiplyrate, cy.ISO_CODE as basecurrencycode,inv.GrandTotal, 
                         DATE_PART('day', (to_date(TO_CHAR(TRUNC(cs.VA009_PlannedDueDate),'dd/mm/yyyy'),'dd/mm/yyyy')-to_date(TO_CHAR(TRUNC(sysdate),'dd/mm/yyyy'),'dd/mm/yyyy'))) 
                         as Due_Date_Diff,cs.duedate, 'Invoice' AS VA009_TransactionType, cs.IsHoldPayment FROM 
                         C_InvoicePaySchedule cs INNER JOIN VA009_PaymentMethod pm ON (pm.VA009_PaymentMethod_ID=cs.VA009_PaymentMethod_ID) INNER JOIN C_DocType cd 
                         ON (cs.C_DocType_ID=cd.C_DocType_ID) INNER JOIN AD_Ref_List rsf ON (rsf.value= cs.VA009_ExecutionStatus) INNER JOIN AD_Reference re ON 
                         (rsf.AD_Reference_ID=re.AD_Reference_ID) LEFT JOIN C_Invoice inv ON (inv.c_invoice_id=cs.c_invoice_id) LEFT JOIN C_BPartner cb ON 
                         (cb.C_Bpartner_ID=inv.C_Bpartner_ID) INNER JOIN C_BP_Group cbg ON (cb.C_BP_Group_ID=cbg.C_BP_Group_ID) INNER JOIN C_Currency cc ON 
                         (inv.C_Currency_ID=cc.C_Currency_ID) INNER JOIN AD_ClientInfo aclnt ON (aclnt.AD_Client_ID =cs.AD_Client_ID) INNER JOIN C_AcctSchema ac ON 
                         (ac.C_AcctSchema_ID =aclnt.C_AcctSchema1_ID) INNER JOIN C_Currency CY ON (AC.C_Currency_ID=CY.C_Currency_ID)  " +
                             whereQry + @"AND re.name= 'VA009_ExecutionStatus' AND re.Export_ID='VA009_20000279' AND rsf.value NOT IN ( 'Y','J')"
                         //AND cs.AD_Client_ID = " + ctx.GetAD_Client_ID() 
                         + " AND NVL(cs.C_Payment_ID , 0) = 0 AND NVL(cs.C_CashLine_ID , 0) = 0 AND cs.VA009_IsPaid = 'N' ";

                    query = MRole.GetDefault(ctx).AddAccessSQL(query, "cs", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
                    sql.Append(query);

                    sql.Append(") t WHERE t.DueAmt !=0 ");
                    string whrduedte = DueDateSearch(WhrDueDate);
                    sql.Append(whrduedte);

                    if (SearchText != string.Empty)
                    {
                        //JID_1793 -- when search text contain "=" then serach with documnet no only
                        if (SearchText.Contains("="))
                        {
                            String[] myStringArray = SearchText.TrimStart(new Char[] { ' ', '=' }).Split(',');
                            if (myStringArray.Length > 0)
                            {
                                sql.Append(" AND UPPER(t.DocumentNo) IN ( ");
                                for (int z = 0; z < myStringArray.Length; z++)
                                {
                                    if (z != 0)
                                    { sql.Append(","); }
                                    sql.Append(" UPPER('" + myStringArray[z].Trim(new Char[] { ' ' }) + "')");
                                }
                                sql.Append(")");
                            }
                        }
                        else
                        {
                            sql.Append(" AND ( UPPER(t.C_Bpartner) LIKE UPPER('%" + SearchText + "%') OR (UPPER(t.c_bp_group) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_PaymentMethod) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_ExecutionStatus) LIKE UPPER('%" + SearchText + "%'))OR (UPPER(t.DocumentNo) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(CAST(t.DueAmt AS VARCHAR(100))) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(CAST(t.VA009_FollowupDate AS VARCHAR(100))) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(CAST(t.va009_plannedduedate AS VARCHAR(100))) LIKE UPPER('%" + SearchText + "%')) ) ");
                        }
                    }

                    if (FromDate != string.Empty && ToDate != string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate BETWEEN  ");
                        sql.Append(GlobalVariable.TO_DATE(dateFrom, true) + " AND ");
                        sql.Append(GlobalVariable.TO_DATE(dateTo, true));
                    }
                    else if (FromDate != string.Empty && ToDate == string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate >=" + GlobalVariable.TO_DATE(dateFrom, true));
                    }
                    else if (FromDate == string.Empty && ToDate != string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate <=" + GlobalVariable.TO_DATE(dateTo, true));
                    }
                }
                if (TransTypes.Count() == 0 || TransTypes.Count() == 2)
                {
                    sql.Append(" UNION ");
                }
                if (TransTypes.Count() == 0 || TransTypes.Count() == 2 || TransTypes[0] == 0)
                {
                    //Table Name is case sensitive must follow Camel format
                    sql.Append(@"SELECT t.VA009_PaymentMode,  t.c_Bpartner_id,  t.c_invoice_id,  t.DocumentNo,  t.C_Bpartner,  t.c_bp_group_id,  t.c_bp_group,  t.C_InvoicePaySchedule_ID,
                        t.VA009_PaymentMethod_ID,  t.VA009_PaymentMethod,  t.va009_paymentbasetype, t.VA009_PaymentRule,  t.VA009_PaymentType,  t.VA009_PaymentTrigger,  t.va009_plannedduedate, 
                        t.VA009_FollowupDate,  t.VA009_RecivedAmt, t.DueAmt, t.VA009_OpenAmnt,  t.VA009_ExecutionStatus,  t.ad_org_id,  t.ad_client_id ,  t.C_Currency_ID,  t.ISO_CODE,  t.basecurrency, 
                        t.multiplyrate, t.Due_Date_Diff, t.basecurrencycode, t.GrandTotal, t.va009_transactiontype, t.IsHoldPayment FROM ( ");
                    //Log Warnings handled
                    string query = @" SELECT pm.VA009_PaymentMode, cb.c_Bpartner_id, cs.C_Order_ID AS c_invoice_id, inv.DocumentNo, cb.name AS C_Bpartner, cb.c_bp_group_id,
                        cbg.name AS c_bp_group, cs.VA009_OrderPaySchedule_ID AS C_InvoicePaySchedule_ID, pm.VA009_PaymentMethod_ID, pm.VA009_name AS VA009_PaymentMethod, pm.va009_paymentbasetype,
                        pm.VA009_PaymentRule, pm.VA009_PaymentType, pm.VA009_PaymentTrigger, cs.duedate AS va009_plannedduedate, cs.VA009_PlannedDueDate  AS VA009_FollowupDate,    
                        0 AS VA009_RecivedAmt, 
                        CASE  WHEN (cd.DOCBASETYPE IN ('SOO','APC')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) WHEN (cd.DOCBASETYPE IN ('POO','ARC')) 
                        THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) * 1 END AS DueAmt,
                        cs.VA009_OpenAmnt, rsf.name AS VA009_ExecutionStatus, cs.ad_org_id, cs.ad_client_id, inv.C_Currency_ID, cc.ISO_CODE, ac.c_currency_id  AS basecurrency,
                        CURRENCYRATE(cc.C_CURRENCY_ID,cy.C_CURRENCY_ID, cs.DueDate, inv.C_ConversionType_ID,inv.AD_Client_ID,inv.AD_ORG_ID) AS multiplyrate,  cy.ISO_CODE AS basecurrencycode,
                        inv.GrandTotal, DATE_PART('day', (to_date(TO_CHAR(TRUNC(cs.VA009_PlannedDueDate),'dd/mm/yyyy'),'dd/mm/yyyy') -to_date(TO_CHAR(TRUNC(sysdate),'dd/mm/yyyy'),'dd/mm/yyyy'))) AS Due_Date_Diff,
                        cs.duedate, 'Order' AS VA009_TransactionType, 'N' AS IsHoldPayment
                        FROM VA009_OrderPaySchedule cs INNER JOIN VA009_PaymentMethod pm   ON (pm.VA009_PaymentMethod_ID=cs.VA009_PaymentMethod_ID)
                        INNER JOIN AD_Ref_List rsf  ON (rsf.value= cs.VA009_ExecutionStatus)  INNER JOIN AD_Reference re  ON (rsf.AD_Reference_ID=re.AD_Reference_ID
                        AND re.name = 'VA009_ExecutionStatus')  INNER JOIN C_Order inv  ON (inv.C_Order_ID=cs.C_Order_ID)  INNER JOIN C_DocType cd
                        ON (inv.C_DocType_ID=cd.C_DocType_ID)  INNER JOIN C_BPartner cb  ON (cb.C_BPartner_ID=inv.C_BPartner_ID)  INNER JOIN C_BP_Group cbg  ON (cb.C_BP_Group_ID=cbg.C_BP_Group_ID)
                        INNER JOIN C_Currency cc  ON (inv.C_Currency_ID=cc.C_Currency_ID)  INNER JOIN AD_ClientInfo aclnt  ON (aclnt.AD_Client_ID =cs.AD_Client_ID)
                        INNER JOIN C_AcctSchema ac  ON (ac.C_AcctSchema_ID =aclnt.C_AcctSchema1_ID)  INNER JOIN C_Currency CY  ON (AC.C_Currency_ID=CY.C_Currency_ID) " +
                            whereQry.Replace("c_invoice_id", "C_Order_ID") + @" AND re.name= 'VA009_ExecutionStatus' AND re.Export_ID='VA009_20000279' AND rsf.value NOT IN ( 'Y','J')"
                        //AND cs.AD_Client_ID = " + ctx.GetAD_Client_ID() + 
                        + " AND NVL(cs.C_Payment_ID , 0) = 0 AND NVL(cs.C_CashLine_ID , 0) = 0 AND cs.VA009_IsPaid = 'N' ";

                    query = MRole.GetDefault(ctx).AddAccessSQL(query, "cs", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
                    sql.Append(query);

                    sql.Append(") t WHERE t.DueAmt !=0 ");
                    string whrduedte = DueDateSearch(WhrDueDate);
                    sql.Append(whrduedte);
                    if (SearchText != string.Empty)
                    {
                        // JID_1793 -- when search text contain "=" then serach with documnet no 
                        if (SearchText.Contains("="))
                        {
                            String[] myStringArray = SearchText.TrimStart(new Char[] { ' ', '=' }).Split(',');
                            if (myStringArray.Length > 0)
                            {
                                sql.Append(" AND UPPER(t.DocumentNo) IN ( ");
                                for (int z = 0; z < myStringArray.Length; z++)
                                {
                                    if (z != 0)
                                    { sql.Append(","); }
                                    sql.Append(" UPPER('" + myStringArray[z].Trim(new Char[] { ' ' }) + "')");
                                }
                                sql.Append(")");
                            }
                        }
                        else
                        {
                            sql.Append(" AND ( UPPER(t.C_Bpartner) LIKE UPPER('%" + SearchText + "%') OR (UPPER(t.c_bp_group) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_PaymentMethod) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(t.VA009_ExecutionStatus) LIKE UPPER('%" + SearchText + "%'))OR (UPPER(t.DocumentNo) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(CAST(t.DueAmt AS VARCHAR(100))) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(CAST(t.VA009_FollowupDate AS VARCHAR(100))) LIKE UPPER('%" + SearchText + "%')) OR (UPPER(CAST(t.va009_plannedduedate AS VARCHAR(100))) LIKE UPPER('%" + SearchText + "%')) ) ");
                        }
                    }

                    if (FromDate != string.Empty && ToDate != string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate BETWEEN  ");
                        sql.Append(GlobalVariable.TO_DATE(dateFrom, true) + " AND ");
                        sql.Append(GlobalVariable.TO_DATE(dateTo, true));
                    }
                    else if (FromDate != string.Empty && ToDate == string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate >=" + GlobalVariable.TO_DATE(dateFrom, true));
                    }
                    else if (FromDate == string.Empty && ToDate != string.Empty)
                    {
                        sql.Append(" and t.VA009_FollowupDate <=" + GlobalVariable.TO_DATE(dateTo, true));
                    }
                }
            }
            //Get Payment Data Order By DueDate
            sql.Replace(sql.ToString(), "SELECT * FROM ( " + sql.ToString() + " ) t ORDER BY t.va009_plannedduedate");

            return sql.ToString();
        }

        public static string DueDateSearch(String WhrDueDate)
        {
            //we need to check if due date is 99 then we need to get all the schedules else we have to add due date condition.
            if (Util.GetValueOfInt(WhrDueDate) == 99)
                WhrDueDate = string.Empty;
            else if (WhrDueDate != string.Empty)
                WhrDueDate = " AND T.Due_Date_Diff <= " + WhrDueDate;
            else
                WhrDueDate = string.Empty;
            return WhrDueDate;
        }

        /// <summary>
        /// update execution status 
        /// </summary>
        /// <param name="VA009_Batch_ID">Batch ID</param>
        /// <param name="ExecutionStatus">Execution Status</param>
        /// <param name="trx">Transaction Object</param>
        /// <returns>query for update execution status</returns>
        public static string UpdateExecutionStatus(int VA009_Batch_ID, string ExecutionStatus, Trx trx)
        {

            StringBuilder updateSql = new StringBuilder();
            StringBuilder sql = new StringBuilder();
            sql.Append(@" SELECT VA009_OrderPaySchedule_ID, C_InvoicePaySchedule_ID , GL_JournalLine_ID FROM VA009_BatchLineDetails  WHERE VA009_BatchLines_ID IN
                        (SELECT VA009_BatchLines_ID  FROM VA009_BatchLines  WHERE VA009_Batch_ID = " + VA009_Batch_ID + " ) GROUP BY " +
                        " VA009_OrderPaySchedule_ID, C_InvoicePaySchedule_ID ");
            DataSet ds = DB.ExecuteDataset(sql.ToString(), null, trx);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                if (DB.IsOracle())
                {
                    updateSql.Append("BEGIN ");
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        sql.Clear();
                        sql.Append(GetExecutionStatusQry(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_InvoicePaySchedule_ID"]),
                            Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_OrderPaySchedule_ID"]),
                            Util.GetValueOfInt(ds.Tables[0].Rows[i]["GL_JournalLine_ID"]),
                            ExecutionStatus));
                        updateSql.Append(" BEGIN execute immediate('" + sql.Replace("'", "''") + "'); exception when others then null; END;");
                    }
                    updateSql.Append(" END;");
                }
                else if (DB.IsPostgreSQL())
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        sql.Clear();
                        sql.Append(GetExecutionStatusQry(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_InvoicePaySchedule_ID"]),
                            Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA009_OrderPaySchedule_ID"]),
                             Util.GetValueOfInt(ds.Tables[0].Rows[i]["GL_JournalLine_ID"]),
                            ExecutionStatus));
                        updateSql.Append(" SELECT ExecuteImmediate('" + sql.Replace("'", "''") + "') FROM DUAL;");
                    }
                }
            }
            return updateSql.ToString();
        }

        /// <summary>
        /// get the query for update execution status
        /// </summary>
        /// <param name="C_InvoicePaySchedule_ID">Invoice Pay Schedule ID</param>
        /// <param name="VA009_OrderPaySchedule_ID">Order Pay Schedule ID</param>
        /// <param name="GL_JournalLine_ID">Journal Line ID</param>
        /// <param name="ExecutionStatus">ExecutionStatus</param>
        /// <returns>Update Query</returns>
        public static string GetExecutionStatusQry(int C_InvoicePaySchedule_ID, int VA009_OrderPaySchedule_ID, int GL_JournalLine_ID, string ExecutionStatus)
        {
            if (C_InvoicePaySchedule_ID > 0)
            {
                return @"UPDATE C_InvoicePaySchedule SET VA009_ExecutionStatus = '" + ExecutionStatus + "' WHERE C_InvoicePaySchedule_ID = " + C_InvoicePaySchedule_ID;
            }
            else if (VA009_OrderPaySchedule_ID > 0)
            {
                return @"UPDATE VA009_OrderPaySchedule SET VA009_ExecutionStatus = '" + ExecutionStatus + "' WHERE VA009_OrderPaySchedule_ID = " + VA009_OrderPaySchedule_ID;
            }
            else if(GL_JournalLine_ID > 0)
            {
                return $@"UPDATE GL_JournalLine SET VA009_IsAssignedtoBatch = 'N' WHERE GL_JournalLine_ID = " + GL_JournalLine_ID;
            }
            return "";
        }

        /// <summary>
        /// This function is used to convert ListAgg to String_Agg
        /// </summary>
        /// <param name="listAggregation">aggregated to</param>
        /// <returns>aggregation syntax</returns>
        public static string ListAggregationName(string listAggregation)
        {
            if (DB.IsOracle())
            {
                return listAggregation;
            }
            else if (DB.IsPostgreSQL())
            {
                return " STRING_AGG(Name, ' ,' ORDER BY Name)";
            }
            return listAggregation;
        }


        /// <summary>
        /// To get the details of cheque 
        /// </summary>
        /// <param name="C_BankAccount_ID">Bank Account</param>
        /// <param name="VA009_PaymentMethod_ID">Payment Method</param>
        ///  <param name="tr">Transaction object</param>
        /// <returns>DataTable or null with cheque details</returns>
        public static List<CheckDetails> GetDetailsofChequeForBatch(int C_BankAccount_ID, int VA009_PaymentMethod_ID, Trx tr)
        {
            // added condition to get only those cheques which current cheque number are less than or equal to end cheque number.
            DataSet ds = DB.ExecuteDataset(@" SELECT bad.CurrentNext AS currentnext, 
                        bad.VA009_BatchLineDetailCount AS va009_batchlinedetailcount, ba.ChkNoAutoControl AS chknoautocontrol,
                        bad.startchknumber AS startchknumber, bad.endchknumber AS endchknumber, bad.priority AS priority,
                        bad.C_BankAccountdoc_ID AS c_bankaccountdoc_id
                        FROM C_BankAccount ba INNER JOIN C_BankAccountdoc bad ON ba.C_BankAccount_ID = 
                        bad.C_BankAccount_ID WHERE bad.C_BankAccount_ID = " + C_BankAccount_ID + @"
                        AND bad.IsActive = 'Y' AND bad.VA009_PaymentMethod_ID = " + VA009_PaymentMethod_ID + "" +
                        " AND bad.CurrentNext <= bad.endchknumber ORDER BY bad.priority ASC "
                        , null, tr);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                List<CheckDetails> lstdtls = new List<CheckDetails>();
                CheckDetails obj = null;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new CheckDetails();
                    obj.chknoautocontrol = Util.GetValueOfString(ds.Tables[0].Rows[i]["chknoautocontrol"]);
                    obj.currentnext = Util.GetValueOfString(ds.Tables[0].Rows[i]["currentnext"]);
                    obj.va009_batchlinedetailcount = Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_batchlinedetailcount"]);
                    obj.startchknumber = Util.GetValueOfString(ds.Tables[0].Rows[i]["startchknumber"]);
                    obj.endchknumber = Util.GetValueOfString(ds.Tables[0].Rows[i]["endchknumber"]);
                    obj.priority = Util.GetValueOfString(ds.Tables[0].Rows[i]["priority"]);
                    obj.c_bankaccountdoc_id = Util.GetValueOfString(ds.Tables[0].Rows[i]["c_bankaccountdoc_id"]);
                    lstdtls.Add(obj);
                }
                return lstdtls;
            }
            return null;
        }

    }

    public class CheckDetails
    {
        public string currentnext { get; set; }
        public string va009_batchlinedetailcount { get; set; }
        public string chknoautocontrol { get; set; }
        public string startchknumber { get; set; }
        public string endchknumber { get; set; }
        public string priority { get; set; }
        public string c_bankaccountdoc_id { get; set; }
    }
}
