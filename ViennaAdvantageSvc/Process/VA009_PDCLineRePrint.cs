/********************************************************
 * Project Name   : VA009 Payment Management
 * Class Name     : VA009_PDCLineRePrint
 * Purpose        : Re-Print Check From Post Dated Check Line Window
 * Class Used     : SvrProcess, MVA027PostDatedCheck,MVA027ChequeDetails
 * Chronological    Development
 * Arpit Rai        8-May-2017
  ******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using VAdvantage.ProcessEngine;
using System.Data;
using VAdvantage.DataBase;
using VAdvantage.Utility;
using VAdvantage.Logging;
using ViennaAdvantage.Model;
using System.Data.SqlClient;

namespace ViennaAdvantage.Process
{
    class VA009_PDCLineRePrint : SvrProcess
    {
        #region Variables
        StringBuilder _sql = new StringBuilder();
        int count = 0;
        int PDC_id = 0;
        int PDCLine_ID = 0;
        int Header_ID = 0;
        int Bank_ID = 0;
        string sqlQuery = "";
        string path = "";
        String DocStatus = "";
        String DocumentBaseType = "";
        #endregion
        protected override void Prepare()
        {
            //  throw new NotImplementedException();
        }
        protected override string DoIt()
        {
            MVA027ChequeDetails PDCLine = new MVA027ChequeDetails(GetCtx(), GetRecord_ID(), Get_Trx());
            PDCLine_ID = PDCLine.GetVA027_ChequeDetails_ID();
            int PDC_id = PDCLine.GetVA027_PostDatedCheck_ID();
            MVA027PostDatedCheck pdc = new MVA027PostDatedCheck(GetCtx(), PDC_id, Get_Trx());
            DocStatus = pdc.GetDocStatus();
            _sql.Append("Select DocBaseType from C_DocType WHERE C_DocType_ID=" + pdc.GetC_DocType_ID());
            try
            {
                DocumentBaseType = (String)DB.ExecuteScalar(_sql.ToString());
                if (DocStatus == "CO" && DocumentBaseType == "PDP" && PDCLine_ID > 0)
                {
                    #region Creating View
                    _sql.Clear();
                    _sql.Append(@"Create OR replace View VA009_PostDataCheck_V AS
                              SELECT T.AD_CLIENT_ID,
                              T.AD_ORG_ID,
                              T.VA027_ChequeDetails_ID,
                              UPPER(T.VA027_PAYEE) AS VA027_PAYEE,
                              VA027_CHECKDATE,
                              '' AS VA009_Cancel,
                              T.VA027_CHEQUEAMOUNT,
                              UPPER(SPELL_NUMBER(abs(VA027_CHEQUEAMOUNT))) AS AmtInword FROM
                            (SELECT PDC.AD_CLIENT_ID,
                              PDC.AD_ORG_ID,
                              DC.VA027_ChequeDetails_ID,
                              CASE WHEN PDC.C_BPartner_ID IS NOT NULL
                                   THEN BP.NAME
                              ELSE PDC.VA027_PAYEE
                              END AS VA027_PAYEE,
                              DC.VA027_CHECKDATE,DC.VA027_ChequeAmount
                              FROM VA027_CHEQUEDETAILS DC 
                            LEFT JOIN VA027_POSTDATEDCHECK PDC
                            ON DC.VA027_POSTDATEDCHECK_ID    =PDC.VA027_POSTDATEDCHECK_ID
                            LEFT JOIN C_BPartner BP ON
                            BP.C_BPartner_ID=PDC.C_BPartner_ID
                            Where DC.VA027_ChequeDetails_ID=" + PDCLine_ID + @"
                                          ) T");
                    count = 0;
                    count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString()));
                    _sql.Clear();
                    #endregion
                    DataSet ds = new DataSet();

                    int bankAccount = pdc.GetC_BankAccount_ID();
                    // GET data from Bank Window
                    _sql.Clear();
                    _sql.Append("Select NVL(BAD.VA009_CheckPrintSetting_ID,0) VA009_CheckPrintSetting_ID," +
                        " NVL(BA.C_BANK_ID,0) C_BANK_ID From C_BankAccountDoc BAD " +
                        " Left join C_BANKACCOUNT BA ON BA.C_bankAccount_ID=BAD.C_bankAccount_ID" +
                        " Where BAD.C_BankAccount_ID =" + bankAccount);
                    //_sql.Append("Select C_BankAccountDoc_ID,CurrentNext,SqlQuery,ReportPath From C_BankAccountDoc Where C_BankAccount_ID =" + bankAccount);
                    ds = DB.ExecuteDataset(_sql.ToString());
                    if (ds != null && ds.Tables[0].Rows.Count > 0)
                    {
                        Header_ID = Convert.ToInt32(ds.Tables[0].Rows[0]["VA009_CheckPrintSetting_ID"]);
                        Bank_ID = Convert.ToInt32(ds.Tables[0].Rows[0]["C_BANK_ID"]);
                    }
                    else
                    {
                        log.SaveError("Error", Msg.GetMsg(GetCtx(), "There is no Bank Account Related to This Payee"));
                        ds.Dispose();
                        _sql.Clear();
                        _sql.Append("Update AD_Process  Set SqlQuery='" + sqlQuery + "' , ReportPath='" + path + "' Where NAme='VA009_CheckRePrintLineReport'");
                        count = 0;
                        count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString()));
                        return "";
                    }
                    ds.Clear();
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
                        sqlQuery = ds.Tables[0].Rows[0]["SQLQUERY"].ToString();
                        path = ds.Tables[0].Rows[0]["REPORTPATH"].ToString();

                        // update print 
                        _sql.Clear();
                        _sql.Append("Update AD_Process  Set SqlQuery='" + sqlQuery + "' , ReportPath='" + path + "' Where NAme='VA009_CheckRePrintLineReport'");
                        count = 0;
                        count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString()));
                    }
                    else
                    {
                        log.SaveError("Error", Msg.GetMsg(GetCtx(), "There is no Print Format Related to This Payee"));
                        _sql.Clear();
                        _sql.Append("Update AD_Process  Set SqlQuery='" + sqlQuery + "' , ReportPath='" + path + "' Where NAme='VA009_CheckRePrintLineReport'");
                        count = 0;
                        count = Convert.ToInt32(DB.ExecuteQuery(_sql.ToString()));
                        ds.Dispose();

                        return "";

                    }
                    ds.Dispose();

                }

                _sql.Clear();
                return " ";
            }
            catch (Exception ex)
            {
                log.Log(Level.SEVERE, ex.Message.ToString());
                return "";
            }
        }
    }
}
