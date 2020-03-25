using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAdvantage.Process;
using VAdvantage.Classes;
using VAdvantage.Model;
using VAdvantage.DataBase;
using VAdvantage.SqlExec;
using System.Data;
using System.Data.SqlClient;
using VAdvantage.Logging;
using VAdvantage.Utility;
using System.Globalization;
using VAdvantage.ProcessEngine;
using VAdvantage.Report;
using ViennaAdvantage.Model;

namespace ViennaAdvantage.Process
{
    public class VA009_CustomerPaymentRpt : SvrProcess
    {
        #region Variables

        private int _C_BankAccount_ID_ = 0;
        private string _C_BPartner_ID_ = "";
        string[] _C_BPartnerList = null;
        private string _VA009_PaymentBaseType_ = "";
        private DateTime? _DateAcct_From = null;
        private DateTime? _DateAcct_To = null;
        private int _C_Charge_ID_ = 0;
        private int _C_ProjectRef_ID_ = 0;
        private int _C_Campaign_ID_ = 0;
        private string _C_Currency_ID_ = null;
        string[] _C_Currency_ID_List = null;
        private StringBuilder _Sql = new StringBuilder();
        private int no = 0;

        #endregion

        // Getting Parameters in variables
        protected override void Prepare()
        {
            //throw new NotImplementedException();
            ProcessInfoParameter[] param = GetParameter();
            for (int i = 0; i < param.Length; i++)
            {
                String name = param[i].GetParameterName();
                if (param[i].GetParameter() == null)
                {
                    ;
                }
                else if (name.Equals("C_BankAccount_ID"))
                {
                    _C_BankAccount_ID_ = Util.GetValueOfInt((param[i].GetParameter()));
                }
                else if (name.Equals("C_BPartner_ID"))
                {
                    _C_BPartner_ID_ = Util.GetValueOfString((param[i].GetParameter()));
                    _C_BPartnerList = _C_BPartner_ID_.Split(',');
                }
                else if (name.Equals("DateTrx"))
                {
                    _DateAcct_From = Util.GetValueOfDateTime(param[i].GetParameter());
                    _DateAcct_To = Util.GetValueOfDateTime(param[i].GetParameter_To());
                }
                else if (name.Equals("VA009_PaymentBaseType"))
                {
                    _VA009_PaymentBaseType_ = param[i].GetParameter().ToString();
                }
                else if (name.Equals("C_ProjectRef_ID"))
                {
                    _C_ProjectRef_ID_ = Util.GetValueOfInt((param[i].GetParameter()));
                }
                else if (name.Equals("C_Campaign_ID"))
                {
                    _C_Campaign_ID_ = Util.GetValueOfInt((param[i].GetParameter()));
                }
                else if (name.Equals("C_Currency_ID"))
                {
                    _C_Currency_ID_ = Util.GetValueOfString((param[i].GetParameter()));
                    _C_Currency_ID_List = _C_Currency_ID_.Split(',');
                }
            }

        }
        // Main Method for all 
        // view creation
        protected override string DoIt()
        {
            int _CountVA027 = Util.GetValueOfInt(DB.ExecuteScalar("Select Count(*) From AD_ModuleInfo Where Prefix like 'VA027_'"));
            // Creating View for the report
            _Sql.Append(@"CREATE OR REPLACE FORCE  VIEW  VA009_CustomerPaymentRpt_V
                                AS
                                SELECT C_PROJECTREF_ID,datetrx,T.C_CAMPAIGN_ID,C_CHARGE_ID,C_Bankaccount_Id,T.ad_client_id,T.ad_org_id,View_ID,documentno, 
                                  T.c_doctype_id,CB.name, T.C_BPartner_ID,T.C_Currency_Id,Va009_Executionstatus,Va009_Paymentbasetype,Payamt,Writeoffamt,Discountamt,
                                  Overunderamt,
                                  Isallocated,TRANSACTIONREFERENCE from (
                                  SELECT P.C_PROJECTREF_ID,
                                    p.datetrx AS datetrx,
                                    P.C_CAMPAIGN_ID,
                                    P.C_CHARGE_ID,
                                    P.C_Bankaccount_Id ,
                                    p.ad_client_id,
                                    p.ad_org_id,
                                    p.c_payment_id AS View_ID,
                                    p.documentno,
                                    p.c_doctype_id,
                                    p.C_BPartner_ID,
                                    P.C_Currency_Id,
                                    P.Va009_Executionstatus,
                                    Payment.Va009_Paymentbasetype ,
                                    ROUND(P.Payamt,4) as Payamt,
                                    ROUND(P.Writeoffamt,4) as Writeoffamt,
                                    ROUND(P.Discountamt,4) as Discountamt,
                                    ROUND(P.Overunderamt,4) as Overunderamt,
                                      CASE P.Isallocated
                                      WHEN 'Y'
                                      THEN 'YES'
                                      WHEN 'N'
                                      THEN 'NO'
                                    END AS Isallocated ,
                                  rtrim(
                                      CASE
                                        WHEN P.C_INVOICE_ID IS NOT NULL
                                        THEN CAST (INV.DOCUMENTNO AS VARCHAR(100))
                                          ||'-IN,'
                                        WHEN P.C_ORDER_ID IS NOT NULL
                                        THEN CAST(ORD.DOCUMENTNO AS VARCHAR (100))
                                          ||'-OR,'
                                        WHEN p.C_Charge_ID IS NOT NULL
                                        THEN p.C_Charge_ID
                                          ||'-CH,'
                                        WHEN P.C_Invoice_Id IS NULL
                                        AND P.C_Order_Id    IS NULL
                                        AND P.C_Charge_Id   IS NULL
                                        THEN
                                          (SELECT DISTINCT(GETMULTIPLEINVOICEID(C_PAYMENT_ID))
                                            ||'-IN,'
                                          FROM C_PaymentAllocate
                                          WHERE C_Payment_Id=P.C_Payment_Id
                                          )
                                      END
                                      ||
                                      CASE
                                        WHEN P.C_PROJECT_ID IS NOT NULL
                                        THEN CAST(PR.NAME AS VARCHAR(100 ))
                                          ||'-OP,'
                                      END
                                      ||
                                      CASE
                                        WHEN P.C_PROJECTREF_ID IS NOT NULL
                                        THEN CAST(PRRF.NAME AS VARCHAR(100 ))
                                          ||'-PR,'
                                      END
                                      ||
                                      CASE
                                        WHEN P.C_CAMPAIGN_ID IS NOT NULL
                                        THEN CAST(CM.NAME AS VARCHAR(100))
                                          ||'-CA,' ");
            if (_CountVA027 > 0)
            {
                _Sql.Append(@" END
                                      ||
                                      CASE
                                        WHEN P.Va027_Postdatedcheck_Id IS NOT NULL
                                        THEN CAST(PDC.DOCUMENTNO AS VARCHAR(100))
                                          ||'-PDC,' ");
            }
            _Sql.Append(@" END,',') AS TRANSACTIONREFERENCE
                                  FROM C_Payment P
                                  LEFT JOIN C_PaymentAllocate PA
                                  ON P.C_PAYMENT_ID =PA.C_PAYMENT_ID
                                  LEFT JOIN C_Project PR
                                  ON PR.C_PROJECT_ID=P.C_PROJECT_ID
                                  LEFT JOIN C_Project PRrf
                                  ON PRrf.C_PROJECT_ID=P.C_ProjectRef_ID
                                  LEFT JOIN C_Campaign CM
                                  ON CM.C_CAMPAIGN_ID=P.C_CAMPAIGN_ID
                                  LEFT JOIN C_INVOICE INV
                                  ON P.C_INVOICE_ID=INV.C_INVOICE_ID
                                  LEFT JOIN C_ORDER ORD
                                  ON P.C_Order_Id =Ord.C_Order_Id
                                  LEFT JOIN Va009_Paymentmethod Payment
                                  ON Payment.Va009_Paymentmethod_Id =P.Va009_Paymentmethod_Id ");
            if (_CountVA027 > 0)
            {
                _Sql.Append(@" LEFT JOIN Va027_Postdatedcheck Pdc
                                  ON Pdc.Va027_Postdatedcheck_Id=P.Va027_Postdatedcheck_Id
                                  AND Pdc.C_Doctype_Id         IN
                                    (SELECT C_DocType_ID
                                    FROM c_Doctype cd
                                    INNER JOIN C_Docbasetype Cdb
                                    ON (Cd.Docbasetype  =Cdb.Docbasetype
                                    AND cdb.docbasetype ='PDR')
                                    ) ");
            }
            _Sql.Append(@" WHERE P.Docstatus                IN ('CL','CO')
                                  AND P.IsReceipt                   ='Y'
                                  UNION ALL
                                  SELECT 0 AS C_PROJECTREF_ID,
                                    c.dateacct AS datetrx,      
                                    C.C_CAMPAIGN_ID,
                                    CL.C_CHARGE_ID,
                                    0 AS C_Bankaccount_Id ,
                                    CL.ad_client_id,
                                    CL.ad_org_id,
                                    C.C_Cash_ID AS View_ID,
                                    C.documentno,
                                    C.c_doctype_id,
                                    CL.C_BPartner_ID,
                                    CL.C_Currency_Id,
                                    ''     AS Va009_Executionstatus ,
                                    'B' AS Va009_Paymentbasetype,
                                    CL.Amount,
                                    CL.Writeoffamt,
                                    CL.Discountamt,
                                    CL.Overunderamt,
                                    CASE CL.Isallocated
                                      WHEN 'Y'
                                      THEN 'YES'
                                      WHEN 'N'
                                      THEN 'NO'
                                    END AS Isallocated,
                                     rtrim( CASE
                                      WHEN CL.C_INVOICE_ID IS NOT NULL
                                      THEN CAST (Inv.Documentno AS VARCHAR(100))
                                        ||'-IN ,'
                                      WHEN CL.C_Charge_ID IS NOT NULL
                                      THEN CL.C_Charge_ID
                                        ||'-CH,'
                                    END
                                        ||
                                    CASE
                                      WHEN C.C_PROJECT_ID IS NOT NULL
                                      THEN CAST(PR.NAME AS VARCHAR(100 ))
                                        ||'-OP,'
                                    END
                                    ||
                                    CASE
                                      WHEN C.C_CAMPAIGN_ID IS NOT NULL
                                      THEN CAST(CM.NAME AS VARCHAR(100))
                                        ||'-CA,'
                                     END,',') AS TRANSACTIONREFERENCE
                                  FROM C_Cash C
                                  INNER JOIN C_CashLine CL
                                  ON C.C_Cash_ID=CL.C_Cash_ID
                                  LEFT JOIN C_INVOICE INV
                                  ON CL.C_INVOICE_ID=INV.C_INVOICE_ID
                                  LEFT JOIN C_Project PR
                                  ON PR.C_PROJECT_ID=C.C_PROJECT_ID
                                    LEFT JOIN C_DOCTYPE DOC
                                    ON doc.C_DOCTYPE_ID=inv.C_DOCTYPE_ID
                                  LEFT JOIN C_Campaign CM
                                  ON CM.C_CAMPAIGN_ID    =C.C_CAMPAIGN_ID
                                  WHERE C.Docstatus     IN ('CL','CO') and (
                                  case
                                WHEN DOC.DOCBASETYPE  ='ARC'
                                AND CL.VSS_PAYMENTTYPE='P'
                                then 'Y'
                                WHEN DOC.DOCBASETYPE  ='APC'
                                AND CL.VSS_PAYMENTTYPE='R'
                                THEN 'N'
                                WHEN CL.VSS_PAYMENTTYPE='R'
                                THEN 'Y'
                                ELSE 'N'
                                END ='Y')
                                ) t
                                left join c_bpartner cb
                                on cb.c_bpartner_id=t.c_bpartner_id");

            // Appending where conditions according to selection of parameters

            if (_DateAcct_From != null && _DateAcct_To != null)
            {
                _Sql.Append(" WHERE   T.DATETRX Between " + GlobalVariable.TO_DATE(_DateAcct_From, true) + " AND " + GlobalVariable.TO_DATE(_DateAcct_To, true));
            }
            else if (_DateAcct_From != null && _DateAcct_To == null)
            {
                _Sql.Append(" AND   I.DATETRX >= " + GlobalVariable.TO_DATE(_DateAcct_From, true));
            }
            else if (_DateAcct_From == null && _DateAcct_To != null)
            {
                _Sql.Append(" AND   I.DATETRX <= " + GlobalVariable.TO_DATE(_DateAcct_To, true));
            }
            if (_C_BankAccount_ID_ > 0)
            {
                _Sql.Append(" AND C_BankAccount_ID = " + _C_BankAccount_ID_);
            }
            if (!string.IsNullOrEmpty(_C_BPartner_ID_))
            {
                _Sql.Append(" AND T.C_BPARTNER_ID  IN (" + _C_BPartner_ID_ + " )");
            }
            if (!string.IsNullOrEmpty(_VA009_PaymentBaseType_))
            {
                if ((_VA009_PaymentBaseType_ == "B"))
                {
                    _Sql.Append(" AND VA009_PAYMENTBASETYPE IN('C','B')");
                }
                else
                {
                    _Sql.Append(" AND VA009_PAYMENTBASETYPE IN('" + _VA009_PaymentBaseType_ + "')");
                }
            }
            if (_C_ProjectRef_ID_ > 0)
            {
                _Sql.Append(" AND  C_PROJECTREF_ID IN(" + _C_ProjectRef_ID_ + " )");
            }

            if (_C_Campaign_ID_ > 0)
            {
                _Sql.Append(" AND T.C_CAMPAIGN_ID =" + _C_Campaign_ID_ + " ");
            }
            if (!string.IsNullOrEmpty(_C_Currency_ID_))
            {
                _Sql.Append(" AND  T.C_CURRENCY_ID IN (" + _C_Currency_ID_ + ")");
            }


            // Execution of view
            try
            {
                no = DB.ExecuteQuery(_Sql.ToString());
                // if got error while creating view this will drop the view
                if (no == -1)
                {
                    DB.ExecuteQuery("DROP VIEW VA009_CustomerPaymentRpt_V");
                }
            }

            catch (Exception ex)
            {
                log.Log(Level.SEVERE, ex.Message.ToString());
            }
            _Sql.Clear();


            #region create sub reoprt view
            // This view is created for subreport currency wise
            _Sql.Append(@"Create or Replace Force view Va009_CustPaymentCurr_V AS 
                                         SELECT ");
            if (_C_BankAccount_ID_ > 0)
            {
                _Sql.Append(_C_BankAccount_ID_ + @" AS C_BankAccount_ID,");
            }
            if (_DateAcct_To != null)
            {
                _Sql.Append(GlobalVariable.TO_DATE(_DateAcct_To, true) + " AS DateTrx,");
            }
            else
            {
                _Sql.Append(GlobalVariable.TO_DATE(_DateAcct_From, true) + " AS DateTrx,");
            }
            if (!String.IsNullOrEmpty(_C_BPartner_ID_))
            {
                if (_C_BPartnerList.Length > 0)
                {
                    _Sql.Append(_C_BPartnerList[0].ToString() + " AS C_BPARTNER_ID,");
                }
            }
            if (!string.IsNullOrEmpty(_VA009_PaymentBaseType_))
            {
                _Sql.Append("'" + _VA009_PaymentBaseType_ + @"' AS VA009_PAYMENTBASETYPE,");
            }
            if (_C_ProjectRef_ID_ > 0)
            {
                _Sql.Append(_C_ProjectRef_ID_ + @" AS  C_PROJECTREF_ID ,");
            }

            if (_C_Campaign_ID_ > 0)
            {
                _Sql.Append(_C_Campaign_ID_ + @" AS C_CAMPAIGN_ID ,");
            }
            _Sql.Append(@"        ad_client_id, 0 as ad_org_id, 
                                  C_CURRENCY_ID,
                                  ROUND(SUM(PAYAMT),4)     as PAYAMT,
                                  ROUND(SUM(WRITEOFFAMT),4)   as WRITEOFFAMT,
                                  ROUND(SUM(DISCOUNTAMT),4)  as DISCOUNTAMT,
                                  ROUND(SUM(OVERUNDERAMT),4) as   OVERUNDERAMT
                                  FROM VA009_CustomerPaymentRpt_V
                                  ");
            _Sql.Append(" GROUP BY C_CURRENCY_ID,ad_client_id");
            try
            {
                no = DB.ExecuteQuery(_Sql.ToString());
                // if got error while creating view this will drop the view
                if (no == -1)
                {
                    DB.ExecuteQuery("DROP VIEW Va009_CustPaymentCurr_V");
                }
            }
            catch (Exception ex)
            {
                log.Log(Level.SEVERE, ex.Message.ToString());
            }
            _Sql.Clear();
            #endregion

            return "";
        }
    }
}
