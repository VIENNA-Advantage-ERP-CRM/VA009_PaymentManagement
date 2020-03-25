using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;
using ViennaAdvantage.Model;

namespace ViennaAdvantage.Process
{
    class VA009_PostcheckPrint : SvrProcess
    {
        StringBuilder _sql = new StringBuilder();
        DataSet ds = new DataSet();
        int count = 0;
        int PDC_ID = 0;
        int Payment_ID = 0;
        int doctype_ID = 0;
        string docStatus = "";
        string docbasetype = "";
        String _DocBaseType = "";
        int checkNo = 0;
        int Doc_ID = 0;
        int Header_ID = 0;
        int Bank_ID = 0;
        string _CHEQUEDETAILS_ID = "";


        string sqlQuery = "";
        string path = "";

        protected override void Prepare()
        {
        }
        protected override string DoIt()
        {


            MVA027PostDatedCheck pdc = new MVA027PostDatedCheck(GetCtx(), GetRecord_ID(), null);
            PDC_ID = pdc.GetVA027_PostDatedCheck_ID();
            docStatus = pdc.GetDocStatus();
            doctype_ID = pdc.GetC_DocType_ID();

            _sql.Clear();
            try
            {
                _sql.Append("Select DocBaseType from C_DocType WHERE C_DocType_ID=" + doctype_ID);
                docbasetype = Util.GetValueOfString(DB.ExecuteScalar(_sql.ToString(), null, Get_Trx()));
                //ds = DB.ExecuteDataset(_sql.ToString());
                //if (ds != null && ds.Tables[0].Rows.Count > 0)
                //{
                //    docbasetype = ds.Tables[0].Rows[0]["DocBaseType"].ToString();
                //    ds.Clear();
                //}
                if (!String.IsNullOrEmpty(docbasetype))
                    _DocBaseType = docbasetype;

                #region Check Print Working for PDc

                if (PDC_ID > 0 && docStatus == "CO" && docbasetype == "PDP")
                {

                    // MVA027PostDatedCheck pdc = new MVA027PostDatedCheck(GetCtx(), GetRecord_ID(), null);
                    int bankAccount = pdc.GetC_BankAccount_ID();


                    //getting multicheck id
                    if (pdc.IsVA027_MultiCheque())
                    {
                        _sql.Clear();
                        _sql.Append("Select VA027_CHEQUEDETAILS_ID From VA027_CHEQUEDETAILS where isactive='Y' and VA009_PRINTED='N' AND VA027_POSTDATEDCHECK_ID=" + PDC_ID);
                        DataSet ds1 = DB.ExecuteDataset(_sql.ToString(), null, Get_Trx());
                        if (ds1 != null && ds1.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds1.Tables[0].Rows.Count; i++)
                            {
                                if (_CHEQUEDETAILS_ID == "")
                                {
                                    _CHEQUEDETAILS_ID = (ds1.Tables[0].Rows[i]["VA027_CHEQUEDETAILS_ID"]).ToString();                                   
                                }
                                else
                                {
                                    _CHEQUEDETAILS_ID = _CHEQUEDETAILS_ID + ", " + (ds1.Tables[0].Rows[i]["VA027_CHEQUEDETAILS_ID"]).ToString();                                   
                                }
                            }
                        }
                    }
                    //end

                    #region Creating View
                    _sql.Clear();
                    _sql.Append(@"Create OR replace View VA009_PostDataCheck_V AS
                                          SELECT T.AD_CLIENT_ID                           AS AD_CLIENT_ID,
                                          T.AD_ORG_ID                                     AS AD_ORG_ID,
                                          UPPER(T.VA027_PAYEE)                            AS VA027_PAYEE,
                                          VA027_CHECKDATE                                 AS VA027_CHECKDATE,
                                          T.VA027_CHEQUEAMOUNT                            AS VA027_CHEQUEAMOUNT,
                                          ''                                              AS VA009_CANCEL,
                                          UPPER( SPELL_NUMBER(ABS(T.VA027_CHEQUEAMOUNT))) AS AMTINWORD,
                                          T.VA027_POSTDATEDCHECK_ID
                                        FROM
                                          (SELECT   PDC.AD_CLIENT_ID,
                                                    PDC.AD_ORG_ID,
                                                    PDC.VA027_PostDatedCheck_ID,
                                            CASE
                                              WHEN PDC.C_BPartner_ID IS NOT NULL
                                              THEN BP.NAME
                                              ELSE PDC.VA027_PAYEE
                                            END AS VA027_PAYEE,
                                            CASE PDC.VA027_MULTICHEQUE
                                              WHEN 'Y'
                                              THEN DC.VA027_CHECKDATE
                                              ELSE PDC.VA027_CHECKDATE
                                            END AS VA027_CHECKDATE,
                                            CASE PDC.VA027_MULTICHEQUE
                                              WHEN 'Y'
                                              THEN DC.VA027_ChequeAmount
                                              ELSE PDC.VA027_PayAmt
                                            END AS VA027_ChequeAmount
                                          FROM VA027_POSTDATEDCHECK PDC
                                          LEFT JOIN VA027_CHEQUEDETAILS DC
                                          ON DC.VA027_POSTDATEDCHECK_ID =PDC.VA027_POSTDATEDCHECK_ID
                                          LEFT JOIN C_BPARTNER BP
                                          ON BP.C_BPartner_ID=PDC.C_BPartner_ID
                                          Where  PDC.VA027_POSTDATEDCHECK_ID=" + PDC_ID);                                          
                    if (pdc.IsVA027_MultiCheque())
                    {
                        _sql.Append(@" And dc.VA027_CHEQUEDETAILS_ID IN ("+ _CHEQUEDETAILS_ID +")");
                            
                    }
                    _sql.Append(@" ) T");
                    count = 0;
                    count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString(), null, Get_Trx()));
                    _sql.Clear();
                    #endregion

                    // GET data from Bank Window
                    _sql.Clear();
                    _sql.Append("Select NVL(BAD.C_BankAccountDoc_ID,0) C_BankAccountDoc_ID,NVL(BAD.CurrentNext,0) CurrentNext,NVL(BAD.VA009_CheckPrintSetting_ID,0) VA009_CheckPrintSetting_ID, NVL(BA.C_BANK_ID,0) C_BANK_ID From C_BankAccountDoc BAD Left join C_BANKACCOUNT BA ON BA.C_bankAccount_ID=BAD.C_bankAccount_ID Where BAD.C_BankAccount_ID =" + bankAccount);
                    //_sql.Append("Select C_BankAccountDoc_ID,CurrentNext,SqlQuery,ReportPath From C_BankAccountDoc Where C_BankAccount_ID =" + bankAccount);
                    ds = DB.ExecuteDataset(_sql.ToString(), null, Get_Trx());
                    if (ds != null && ds.Tables[0].Rows.Count > 0)
                    {
                        checkNo = Convert.ToInt32(ds.Tables[0].Rows[0]["CurrentNext"]);
                        Doc_ID = Convert.ToInt32(ds.Tables[0].Rows[0]["C_BankAccountDoc_ID"]);
                        Header_ID = Convert.ToInt32(ds.Tables[0].Rows[0]["VA009_CheckPrintSetting_ID"]);
                        Bank_ID = Convert.ToInt32(ds.Tables[0].Rows[0]["C_BANK_ID"]);
                        if (pdc.IsVA027_MultiCheque())
                        {
                            _sql.Clear();
                            _sql.Append("Select count(*) from VA027_CHEQUEDETAILS where VA009_PRINTED='N' and VA027_POSTDATEDCHECK_ID=" + PDC_ID);
                            count = 0;
                            count = Convert.ToInt32(DB.ExecuteScalar(_sql.ToString(), null, Get_Trx()));
                            checkNo = checkNo + count;

                            _sql.Clear();
                            _sql.Append("Update VA027_CHEQUEDETAILS set VA009_Printed='Y' where VA027_POSTDATEDCHECK_ID=" + PDC_ID);
                            DB.ExecuteScalar(_sql.ToString(), null, Get_Trx());
                        }
                        else
                        {
                            checkNo = checkNo + 1;

                            //_sql.Clear();
                            //_sql.Append("Update VA027_CHEQUEDETAILS set VA009_Printed='Y' where VA027_POSTDATEDCHECK_ID=" + PDC_ID);
                            //DB.ExecuteScalar(_sql.ToString());
                        }
                        ds.Clear();
                    }
                    else
                    {
                        _sql.Clear();
                        _sql.Append("Update AD_Process  Set SqlQuery='" + sqlQuery + "' , ReportPath='" + path + "' Where NAme='VA009_CheckPrintReport'");
                        count = 0;
                        count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString(), null, Get_Trx()));
                        return "";
                    }


                    // Check no increase by one
                    _sql.Clear();
                    _sql.Append("Update C_BankAccountDoc Set CurrentNext=" + checkNo + " Where isactive='Y' and  C_BankAccountDoc_ID =" + Doc_ID);
                    count = 0;
                    count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString(), null, Get_Trx()));


                    // Get Data from Print Check Window ( GEt Check Setting)
                    _sql.Clear();
                    _sql.Append(@"SELECT L.REPORTPATH ,
                              L.SQLQUERY
                              FROM VA009_DocPrintConfig L
                              LEFT JOIN VA009_CheckPrintSetting H
                              ON H.VA009_CheckPrintSetting_ID =L.VA009_CheckPrintSetting_ID
                              WHERE L.VA009_CHECKTYPE        ='P1'
                             AND H.VA009_CheckPrintSetting_ID=" + Header_ID + " AND H.C_BANK_ID =" + Bank_ID);
                    ds = DB.ExecuteDataset(_sql.ToString());
                    if (ds != null && ds.Tables[0].Rows.Count > 0)
                    {
                        pdc.SetVA009_Printed(true);
                        //pdc.setva0
                        pdc.Save();
                        sqlQuery = ds.Tables[0].Rows[0]["SQLQUERY"].ToString();
                        path = ds.Tables[0].Rows[0]["REPORTPATH"].ToString();
                        ds.Clear();
                        // update print 
                        _sql.Clear();
                        _sql.Append("Update AD_Process  Set SqlQuery='" + sqlQuery + "' , ReportPath='" + path + "' Where NAme='VA009_CheckPrintReport'");
                        count = 0;
                        count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString(), null, Get_Trx()));

                        PDC_ID = 0;


                    }
                    else
                    {
                        _sql.Clear();
                        _sql.Append("Update AD_Process  Set SqlQuery='" + sqlQuery + "' , ReportPath='" + path + "' Where NAme='VA009_CheckPrintReport'");
                        count = 0;
                        count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString(), null, Get_Trx()));
                        return "";
                    }

                }

                #endregion


                #region Check Print Working for PAyment
                else
                {
                    MPayment pay = new MPayment(GetCtx(), GetRecord_ID(), null);
                    Payment_ID = pay.GetC_Payment_ID();
                    docStatus = pay.GetDocStatus();
                    doctype_ID = pay.GetC_DocType_ID();

                    _sql.Clear();
                    _sql.Append("Select DocBaseType from C_DocType WHERE C_DocType_ID=" + doctype_ID);
                    docbasetype = Util.GetValueOfString(DB.ExecuteScalar(_sql.ToString(), null, Get_Trx()));
                    //ds = DB.ExecuteDataset(_sql.ToString());
                    //if (ds != null && ds.Tables[0].Rows.Count > 0)
                    //{
                    //    docbasetype = ds.Tables[0].Rows[0]["DocBaseType"].ToString();
                    //    ds.Clear();
                    //}


                    if (Payment_ID > 0 && docStatus == "CO" && docbasetype == "APP")
                    {

                        //MPayment pay = new MPayment(GetCtx(), GetRecord_ID(), null);
                        int bankAccount = pay.GetC_BankAccount_ID();
                        pay.SetVA009_Printed(true);
                        pay.Save();


                        #region Creating View
                        _sql.Clear();
                        _sql.Append(@"Create OR replace View VA009_PaymentCheck_V AS
                              SELECT P.AD_CLIENT_ID                  AS AD_CLIENT_ID,
                                    P.AD_ORG_ID                      AS AD_ORG_ID,
                                    ''                               AS VA009_CANCEL,
                                    UPPER(BP.NAME)                   AS VA027_PAYEE,
                                    P.CHECKDATE                      AS VA027_CHECKDATE,
                                    p.PAYAMT                         AS VA027_CHEQUEAMOUNT,
                                    UPPER(SPELL_NUMBER(ABS(PAYAMT))) AS AMTINWORD,
                                    P.C_PAYMENT_ID
                                  FROM C_PAYMENT P
                                  LEFT JOIN C_BPARTNER BP
                                  ON BP.C_BPartner_ID=P.C_BPartner_ID
                                  Where P.C_PAYMENT_ID=" + Payment_ID);
                        count = 0;
                        count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString(), null, Get_Trx()));
                        _sql.Clear();
                        #endregion


                        // GET data from Bank Window
                        _sql.Clear();
                        //_sql.Append("Select BAD.C_BankAccountDoc_ID,BAD.CurrentNext,BAD.VA009_CheckPrintSetting_ID, BA.C_BANK_ID From C_BankAccountDoc BAD Left join C_BANKACCOUNT BA ON BA.C_bankAccount_ID=BAD.C_bankAccount_ID Where BAD.C_BankAccount_ID =" + bankAccount);
                        _sql.Append("Select NVL(BAD.C_BankAccountDoc_ID,0) C_BankAccountDoc_ID,NVL(BAD.CurrentNext,0) CurrentNext,NVL(BAD.VA009_CheckPrintSetting_ID,0) VA009_CheckPrintSetting_ID, NVL(BA.C_BANK_ID,0) C_BANK_ID From C_BankAccountDoc BAD Left join C_BANKACCOUNT BA ON BA.C_bankAccount_ID=BAD.C_bankAccount_ID Where BAD.C_BankAccount_ID =" + bankAccount);
                        //_sql.Append("Select C_BankAccountDoc_ID,CurrentNext,SqlQuery,ReportPath From C_BankAccountDoc Where C_BankAccount_ID =" + bankAccount);
                        ds = DB.ExecuteDataset(_sql.ToString(), null, Get_Trx());
                        if (ds != null && ds.Tables[0].Rows.Count > 0)
                        {
                            checkNo = Convert.ToInt32(ds.Tables[0].Rows[0]["CurrentNext"]);
                            Doc_ID = Convert.ToInt32(ds.Tables[0].Rows[0]["C_BankAccountDoc_ID"]);
                            Header_ID = Convert.ToInt32(ds.Tables[0].Rows[0]["VA009_CheckPrintSetting_ID"]);
                            Bank_ID = Convert.ToInt32(ds.Tables[0].Rows[0]["C_BANK_ID"]);
                            checkNo = checkNo + 1;
                            ds.Clear();
                        }
                        else
                        {
                            _sql.Clear();
                            _sql.Append("Update AD_Process  Set SqlQuery='" + sqlQuery + "' , ReportPath='" + path + "' Where NAme='VA009_CheckPrintReport'");
                            count = 0;
                            count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString(), null, Get_Trx()));
                            return "";
                        }

                        // Check no increase by one
                        _sql.Clear();
                        _sql.Append("Update C_BankAccountDoc Set CurrentNext=" + checkNo + " Where C_BankAccountDoc_ID =" + Doc_ID);
                        count = 0;
                        count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString(), null, Get_Trx()));

                        // Get Data from Print Check Window ( GEt Check Setting)
                        _sql.Clear();
                        _sql.Append(@"SELECT L.REPORTPATH ,
                              L.SQLQUERY
                              FROM VA009_DocPrintConfig L
                              LEFT JOIN VA009_CheckPrintSetting H
                              ON H.VA009_CheckPrintSetting_ID =L.VA009_CheckPrintSetting_ID
                              WHERE L.VA009_CHECKTYPE        ='P2'
                             AND H.VA009_CheckPrintSetting_ID=" + Header_ID + " AND H.C_BANK_ID =" + Bank_ID);
                        ds = DB.ExecuteDataset(_sql.ToString(), null, Get_Trx());
                        if (ds != null && ds.Tables[0].Rows.Count > 0)
                        {
                            sqlQuery = ds.Tables[0].Rows[0]["SQLQUERY"].ToString();
                            path = ds.Tables[0].Rows[0]["REPORTPATH"].ToString();
                            ds.Clear();
                            _sql.Clear();
                            _sql.Append("Update AD_Process  Set SqlQuery='" + sqlQuery + "' , ReportPath='" + path + "' Where NAme='VA009_CheckPrintReport'");
                            count = 0;
                            count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString(), null, Get_Trx()));
                        }
                        else
                        {
                            _sql.Clear();
                            _sql.Append("Update AD_Process  Set SqlQuery='" + sqlQuery + "' , ReportPath='" + path + "' Where NAme='VA009_CheckPrintReport'");
                            count = 0;
                            count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString(), null, Get_Trx()));
                            return "";
                        }


                #endregion
                    }
                }
                if (_DocBaseType != "PDP" && docbasetype != "APP")
                {
                    _sql.Clear();
                    _sql.Append("Update AD_Process  Set SqlQuery='" + sqlQuery + "' , ReportPath='" + path + "' Where NAme='VA009_CheckPrintReport'");
                    count = 0;
                    count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString(), null, Get_Trx()));
                    //log.SaveError("Error", Msg.GetMsg(GetCtx(), "Not Able to print check"));
                }
            }
            catch (Exception ex)
            {
                log.Log(Level.SEVERE, ex.Message.ToString());
            }
            return " ";

        }
    }
}
