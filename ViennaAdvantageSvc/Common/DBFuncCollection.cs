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
            int conversionType_ID = ctx.GetContextAsInt("#C_ConversionType_ID");

            if (DB.IsOracle())
            {
                if (TransTypes.Count() == 0 || TransTypes.Count() == 2 || TransTypes[0] == 1)
                {
                    sql.Append(@"SELECT t.VA009_PaymentMode,  t.c_Bpartner_id,  t.C_invoice_ID,  t.DocumentNo,  t.C_Bpartner,  t.c_bp_group_id,  t.c_bp_group,  
                         t.C_InvoicePaySchedule_ID,  t.VA009_PaymentMethod_ID,  t.VA009_PaymentMethod,  t.va009_paymentbasetype,  t.VA009_PaymentRule,  t.VA009_PaymentType,  t.VA009_PaymentTrigger,
                         t.va009_plannedduedate, t.VA009_FollowupDate,  t.VA009_RecivedAmt,  t.DueAmt, t.VA009_OpenAmnt, t.VA009_ExecutionStatus,  t.ad_org_id,  t.ad_client_id ,  t.C_Currency_ID,  
                         t.ISO_CODE, t.basecurrency, t.multiplyrate, t.Due_Date_Diff, t.basecurrencycode,t.GrandTotal, t.va009_transactiontype, t.IsHoldPayment FROM (");

                    string query = @"SELECT pm.VA009_PaymentMode,cb.c_Bpartner_id, cs.C_invoice_ID,inv.DocumentNo, cb.name as C_Bpartner, cb.c_bp_group_id, cbg.name as c_bp_group, cs.C_InvoicePaySchedule_ID,
                         pm.VA009_PaymentMethod_ID, pm.VA009_name as VA009_PaymentMethod,pm.va009_paymentbasetype,pm.VA009_PaymentRule, pm.VA009_PaymentType, pm.VA009_PaymentTrigger,
                         cs.duedate as va009_plannedduedate,
                         cs.VA009_PlannedDueDate as VA009_FollowupDate,inv.VA009_PaidAmount AS VA009_RecivedAmt,
                         CASE WHEN (cd.DOCBASETYPE IN ('ARI','APC')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) WHEN (cd.DOCBASETYPE IN ('API','ARC'))     
                         THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) * 1  END AS DueAmt,
                         cs.VA009_OpenAmnt, rsf.name as VA009_ExecutionStatus,  cs.ad_org_id,  cs.ad_client_id ,
                         inv.C_Currency_ID,  cc.ISO_CODE, ac.c_currency_id as basecurrency,  CURRENCYRATE(cc.C_CURRENCY_ID,cy.C_CURRENCY_ID,TRUNC(sysdate)," + conversionType_ID
                             + @",inv.AD_Client_ID,inv.AD_ORG_ID) as multiplyrate, cy.ISO_CODE as basecurrencycode,inv.GrandTotal, (to_date(TO_CHAR(TRUNC(cs.VA009_PlannedDueDate)),'dd/mm/yyyy')
                        -to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy')) as Due_Date_Diff,cs.duedate, 'Invoice' AS VA009_TransactionType, cs.IsHoldPayment FROM 
                         C_InvoicePaySchedule cs INNER JOIN VA009_PaymentMethod pm ON pm.VA009_PaymentMethod_ID=cs.VA009_PaymentMethod_ID INNER JOIN C_Doctype 
                         cd ON cs.C_Doctype_ID=cd.C_Doctype_ID INNER JOIN ad_ref_list rsf ON rsf.value= cs.VA009_ExecutionStatus INNER JOIN ad_reference re ON 
                         rsf.ad_reference_id=re.ad_reference_id LEFT JOIN C_invoice inv ON inv.C_Invoice_ID=cs.C_invoice_ID LEFT JOIN C_BPartner cb ON 
                         cb.c_bpartner_id=inv.c_bpartner_id INNER JOIN c_bp_group cbg ON cb.c_bp_group_id=cbg.c_bp_group_id INNER JOIN C_Currency cc ON 
                         inv.C_Currency_ID=cc.C_Currency_ID INNER JOIN AD_ClientInfo aclnt ON aclnt.AD_Client_ID =cs.AD_Client_ID INNER JOIN C_acctschema ac ON 
                         ac.C_AcctSchema_ID =aclnt.C_AcctSchema1_ID INNER JOIN C_CURRENCY CY ON AC.C_CURRENCY_ID=CY.C_CURRENCY_ID  " +
                             whereQry + @"AND re.name= 'VA009_ExecutionStatus' AND re.Export_ID='VA009_20000279' AND rsf.value NOT IN ( 'Y','J')
                         AND cs.AD_Client_ID = " + ctx.GetAD_Client_ID() + " AND NVL(cs.C_Payment_ID , 0) = 0 AND NVL(cs.C_CashLine_ID , 0) = 0 AND cs.VA009_IsPaid = 'N' ";

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
                if (TransTypes.Count() == 0 || TransTypes.Count() == 2)
                {
                    sql.Append(" UNION ");
                }
                if (TransTypes.Count() == 0 || TransTypes.Count() == 2 || TransTypes[0] == 0)
                {
                    sql.Append(@"SELECT t.VA009_PaymentMode,  t.c_Bpartner_id,  t.C_invoice_ID,  t.DocumentNo,  t.C_Bpartner,  t.c_bp_group_id,  t.c_bp_group,  t.C_InvoicePaySchedule_ID,
                        t.VA009_PaymentMethod_ID,  t.VA009_PaymentMethod,  t.va009_paymentbasetype, t.VA009_PaymentRule,  t.VA009_PaymentType,  t.VA009_PaymentTrigger,  t.va009_plannedduedate, 
                        t.VA009_FollowupDate,  t.VA009_RecivedAmt, t.DueAmt, t.VA009_OpenAmnt,  t.VA009_ExecutionStatus,  t.ad_org_id,  t.ad_client_id ,  t.C_Currency_ID,  t.ISO_CODE,  t.basecurrency, 
                        t.multiplyrate, t.Due_Date_Diff, t.basecurrencycode, t.GrandTotal, t.va009_transactiontype, t.IsHoldPayment FROM ( ");

                    string query = @" SELECT pm.VA009_PaymentMode, cb.c_Bpartner_id, cs.C_Order_ID AS C_invoice_ID, inv.DocumentNo, cb.name AS C_Bpartner, cb.c_bp_group_id,
                        cbg.name AS c_bp_group, cs.VA009_OrderPaySchedule_ID AS C_InvoicePaySchedule_ID, pm.VA009_PaymentMethod_ID, pm.VA009_name AS VA009_PaymentMethod, pm.va009_paymentbasetype,
                        pm.VA009_PaymentRule, pm.VA009_PaymentType, pm.VA009_PaymentTrigger, cs.duedate AS va009_plannedduedate, cs.VA009_PlannedDueDate  AS VA009_FollowupDate,    
                        0 AS VA009_RecivedAmt, 
                        CASE  WHEN (cd.DOCBASETYPE IN ('SOO','APC')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) WHEN (cd.DOCBASETYPE IN ('POO','ARC')) 
                        THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) * 1 END AS DueAmt,
                        cs.VA009_OpenAmnt, rsf.name AS VA009_ExecutionStatus, cs.ad_org_id, cs.ad_client_id, inv.C_Currency_ID, cc.ISO_CODE, ac.c_currency_id  AS basecurrency,
                        CURRENCYRATE(cc.C_CURRENCY_ID,cy.C_CURRENCY_ID,TRUNC(sysdate)," + conversionType_ID + @",inv.AD_Client_ID,inv.AD_ORG_ID) AS multiplyrate,  cy.ISO_CODE AS basecurrencycode,
                        inv.GrandTotal, (to_date(TO_CHAR(TRUNC(cs.VA009_PlannedDueDate)),'dd/mm/yyyy') -to_date(TO_CHAR(TRUNC(sysdate)),'dd/mm/yyyy')) AS Due_Date_Diff,
                        cs.duedate, 'Order' AS VA009_TransactionType, 'N' AS IsHoldPayment
                        FROM VA009_OrderPaySchedule cs INNER JOIN VA009_PaymentMethod pm   ON pm.VA009_PaymentMethod_ID=cs.VA009_PaymentMethod_ID
                        INNER JOIN ad_ref_list rsf  ON rsf.value= cs.VA009_ExecutionStatus  INNER JOIN ad_reference re  ON (rsf.ad_reference_id=re.ad_reference_id
                        AND re.name = 'VA009_ExecutionStatus')  INNER JOIN C_Order inv  ON inv.C_Order_ID=cs.C_Order_ID  INNER JOIN C_Doctype cd
                        ON inv.C_Doctype_ID=cd.C_Doctype_ID  INNER JOIN C_BPartner cb  ON cb.c_bpartner_id=inv.c_bpartner_id  INNER JOIN c_bp_group cbg  ON cb.c_bp_group_id=cbg.c_bp_group_id
                        INNER JOIN C_Currency cc  ON inv.C_Currency_ID=cc.C_Currency_ID  INNER JOIN AD_ClientInfo aclnt  ON aclnt.AD_Client_ID =cs.AD_Client_ID
                        INNER JOIN C_acctschema ac  ON ac.C_AcctSchema_ID =aclnt.C_AcctSchema1_ID  INNER JOIN C_CURRENCY CY  ON AC.C_CURRENCY_ID=CY.C_CURRENCY_ID " +
                            whereQry.Replace("c_invoice_id", "C_Order_ID") + @" AND re.name= 'VA009_ExecutionStatus' AND re.Export_ID='VA009_20000279' AND rsf.value NOT IN ( 'Y','J')
                        AND cs.AD_Client_ID = " + ctx.GetAD_Client_ID() + " AND NVL(cs.C_Payment_ID , 0) = 0 AND NVL(cs.C_CashLine_ID , 0) = 0 AND cs.VA009_IsPaid = 'N' ";

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
            }
            else if (DB.IsPostgreSQL())
            {
                if (TransTypes.Count() == 0 || TransTypes.Count() == 2 || TransTypes[0] == 1)
                {
                    sql.Append(@"SELECT t.VA009_PaymentMode,  t.c_Bpartner_id,  t.C_invoice_ID,  t.DocumentNo,  t.C_Bpartner,  t.c_bp_group_id,  t.c_bp_group,  
                         t.C_InvoicePaySchedule_ID,  t.VA009_PaymentMethod_ID,  t.VA009_PaymentMethod,  t.va009_paymentbasetype,  t.VA009_PaymentRule,  t.VA009_PaymentType,  t.VA009_PaymentTrigger,
                         t.va009_plannedduedate, t.VA009_FollowupDate,  t.VA009_RecivedAmt,  t.DueAmt, t.VA009_OpenAmnt, t.VA009_ExecutionStatus,  t.ad_org_id,  t.ad_client_id ,  t.C_Currency_ID,  
                         t.ISO_CODE, t.basecurrency, t.multiplyrate, t.Due_Date_Diff, t.basecurrencycode,t.GrandTotal, t.va009_transactiontype, t.IsHoldPayment FROM (");

                    string query = @"SELECT pm.VA009_PaymentMode,cb.c_Bpartner_id, cs.C_invoice_ID,inv.DocumentNo, cb.name as C_Bpartner, cb.c_bp_group_id, cbg.name as c_bp_group, cs.C_InvoicePaySchedule_ID,
                         pm.VA009_PaymentMethod_ID, pm.VA009_name as VA009_PaymentMethod,pm.va009_paymentbasetype,pm.VA009_PaymentRule, pm.VA009_PaymentType, pm.VA009_PaymentTrigger,
                         cs.duedate as va009_plannedduedate,
                         cs.VA009_PlannedDueDate as VA009_FollowupDate,inv.VA009_PaidAmount AS VA009_RecivedAmt,
                         CASE WHEN (cd.DOCBASETYPE IN ('ARI','APC')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) WHEN (cd.DOCBASETYPE IN ('API','ARC'))     
                         THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) * 1  END AS DueAmt,
                         cs.VA009_OpenAmnt, rsf.name as VA009_ExecutionStatus,  cs.ad_org_id,  cs.ad_client_id ,
                         inv.C_Currency_ID,  cc.ISO_CODE, ac.c_currency_id as basecurrency,  CURRENCYRATE(cc.C_CURRENCY_ID,cy.C_CURRENCY_ID,TRUNC(sysdate)," + conversionType_ID
                             + @",inv.AD_Client_ID,inv.AD_ORG_ID) as multiplyrate, cy.ISO_CODE as basecurrencycode,inv.GrandTotal, 
                         DATE_PART('day', (to_date(TO_CHAR(TRUNC(cs.VA009_PlannedDueDate),'dd/mm/yyyy'),'dd/mm/yyyy')-to_date(TO_CHAR(TRUNC(sysdate),'dd/mm/yyyy'),'dd/mm/yyyy'))) 
                         as Due_Date_Diff,cs.duedate, 'Invoice' AS VA009_TransactionType, cs.IsHoldPayment FROM 
                         C_InvoicePaySchedule cs INNER JOIN VA009_PaymentMethod pm ON pm.VA009_PaymentMethod_ID=cs.VA009_PaymentMethod_ID INNER JOIN C_Doctype 
                         cd ON cs.C_Doctype_ID=cd.C_Doctype_ID INNER JOIN ad_ref_list rsf ON rsf.value= cs.VA009_ExecutionStatus INNER JOIN ad_reference re ON 
                         rsf.ad_reference_id=re.ad_reference_id LEFT JOIN C_invoice inv ON inv.C_Invoice_ID=cs.C_invoice_ID LEFT JOIN C_BPartner cb ON 
                         cb.c_bpartner_id=inv.c_bpartner_id INNER JOIN c_bp_group cbg ON cb.c_bp_group_id=cbg.c_bp_group_id INNER JOIN C_Currency cc ON 
                         inv.C_Currency_ID=cc.C_Currency_ID INNER JOIN AD_ClientInfo aclnt ON aclnt.AD_Client_ID =cs.AD_Client_ID INNER JOIN C_acctschema ac ON 
                         ac.C_AcctSchema_ID =aclnt.C_AcctSchema1_ID INNER JOIN C_CURRENCY CY ON AC.C_CURRENCY_ID=CY.C_CURRENCY_ID  " +
                             whereQry + @"AND re.name= 'VA009_ExecutionStatus' AND re.Export_ID='VA009_20000279' AND rsf.value NOT IN ( 'Y','J')
                         AND cs.AD_Client_ID = " + ctx.GetAD_Client_ID() + " AND NVL(cs.C_Payment_ID , 0) = 0 AND NVL(cs.C_CashLine_ID , 0) = 0 AND cs.VA009_IsPaid = 'N' ";

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
                    sql.Append(@"SELECT t.VA009_PaymentMode,  t.c_Bpartner_id,  t.C_invoice_ID,  t.DocumentNo,  t.C_Bpartner,  t.c_bp_group_id,  t.c_bp_group,  t.C_InvoicePaySchedule_ID,
                        t.VA009_PaymentMethod_ID,  t.VA009_PaymentMethod,  t.va009_paymentbasetype, t.VA009_PaymentRule,  t.VA009_PaymentType,  t.VA009_PaymentTrigger,  t.va009_plannedduedate, 
                        t.VA009_FollowupDate,  t.VA009_RecivedAmt, t.DueAmt, t.VA009_OpenAmnt,  t.VA009_ExecutionStatus,  t.ad_org_id,  t.ad_client_id ,  t.C_Currency_ID,  t.ISO_CODE,  t.basecurrency, 
                        t.multiplyrate, t.Due_Date_Diff, t.basecurrencycode, t.GrandTotal, t.va009_transactiontype, t.IsHoldPayment FROM ( ");

                    string query = @" SELECT pm.VA009_PaymentMode, cb.c_Bpartner_id, cs.C_Order_ID AS C_invoice_ID, inv.DocumentNo, cb.name AS C_Bpartner, cb.c_bp_group_id,
                        cbg.name AS c_bp_group, cs.VA009_OrderPaySchedule_ID AS C_InvoicePaySchedule_ID, pm.VA009_PaymentMethod_ID, pm.VA009_name AS VA009_PaymentMethod, pm.va009_paymentbasetype,
                        pm.VA009_PaymentRule, pm.VA009_PaymentType, pm.VA009_PaymentTrigger, cs.duedate AS va009_plannedduedate, cs.VA009_PlannedDueDate  AS VA009_FollowupDate,    
                        0 AS VA009_RecivedAmt, 
                        CASE  WHEN (cd.DOCBASETYPE IN ('SOO','APC')) THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) WHEN (cd.DOCBASETYPE IN ('POO','ARC')) 
                        THEN ROUND(cs.DUEAMT,NVL(CY.StdPrecision,2)) * 1 END AS DueAmt,
                        cs.VA009_OpenAmnt, rsf.name AS VA009_ExecutionStatus, cs.ad_org_id, cs.ad_client_id, inv.C_Currency_ID, cc.ISO_CODE, ac.c_currency_id  AS basecurrency,
                        CURRENCYRATE(cc.C_CURRENCY_ID,cy.C_CURRENCY_ID,TRUNC(sysdate)," + conversionType_ID + @",inv.AD_Client_ID,inv.AD_ORG_ID) AS multiplyrate,  cy.ISO_CODE AS basecurrencycode,
                        inv.GrandTotal, DATE_PART('day', (to_date(TO_CHAR(TRUNC(cs.VA009_PlannedDueDate),'dd/mm/yyyy'),'dd/mm/yyyy') -to_date(TO_CHAR(TRUNC(sysdate),'dd/mm/yyyy'),'dd/mm/yyyy'))) AS Due_Date_Diff,
                        cs.duedate, 'Order' AS VA009_TransactionType, 'N' AS IsHoldPayment
                        FROM VA009_OrderPaySchedule cs INNER JOIN VA009_PaymentMethod pm   ON pm.VA009_PaymentMethod_ID=cs.VA009_PaymentMethod_ID
                        INNER JOIN ad_ref_list rsf  ON rsf.value= cs.VA009_ExecutionStatus  INNER JOIN ad_reference re  ON (rsf.ad_reference_id=re.ad_reference_id
                        AND re.name = 'VA009_ExecutionStatus')  INNER JOIN C_Order inv  ON inv.C_Order_ID=cs.C_Order_ID  INNER JOIN C_Doctype cd
                        ON inv.C_Doctype_ID=cd.C_Doctype_ID  INNER JOIN C_BPartner cb  ON cb.c_bpartner_id=inv.c_bpartner_id  INNER JOIN c_bp_group cbg  ON cb.c_bp_group_id=cbg.c_bp_group_id
                        INNER JOIN C_Currency cc  ON inv.C_Currency_ID=cc.C_Currency_ID  INNER JOIN AD_ClientInfo aclnt  ON aclnt.AD_Client_ID =cs.AD_Client_ID
                        INNER JOIN C_acctschema ac  ON ac.C_AcctSchema_ID =aclnt.C_AcctSchema1_ID  INNER JOIN C_CURRENCY CY  ON AC.C_CURRENCY_ID=CY.C_CURRENCY_ID " +
                            whereQry.Replace("c_invoice_id", "C_Order_ID") + @" AND re.name= 'VA009_ExecutionStatus' AND re.Export_ID='VA009_20000279' AND rsf.value NOT IN ( 'Y','J')
                        AND cs.AD_Client_ID = " + ctx.GetAD_Client_ID() + " AND NVL(cs.C_Payment_ID , 0) = 0 AND NVL(cs.C_CashLine_ID , 0) = 0 AND cs.VA009_IsPaid = 'N' ";

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
    }
}
