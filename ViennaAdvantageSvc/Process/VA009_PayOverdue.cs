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
using System;

namespace ViennaAdvantage.Process
{
   public class VA009_PayOverdue : SvrProcess
    {
        int _docType = 0;
        int _payschedule = 0;
        DateTime? _DateDoc_From = null;
        DateTime? _DateDoc_To = null;
        DateTime? _duedate = null;
        DateTime? _sysdate = null;
       
        protected override string DoIt()
        {
            StringBuilder _sql = new StringBuilder();
            _sql.Clear();
            _sql.Append("SELECT cs.C_InvoicePaySchedule_ID,  cs.DueDate FROM C_InvoicePaySchedule cs INNER JOIN c_doctype cd ON cs.c_doctype_id=cd.c_doctype_id");
            _sql.Append(" Where cs.IsActive = 'Y' and cs.ad_client_id=" + GetAD_Client_ID());
            if (_docType > 0)
            {
                _sql.Append(" and cs.c_doctype_id=" + _docType);
            }
            if (_DateDoc_From != null && _DateDoc_To != null) {
                //_sql.Append(" and cs.duedate between " + _DateDoc_From + "");
                //_sql.Append(" and trunc(DueDate) between " + GlobalVariable.TO_DATE(_DateDoc_From, true) + " and " + GlobalVariable.TO_DATE(_DateDoc_To, true));
                _sql.Append(" and cs.DueDate BETWEEN ");
                _sql.Append(GlobalVariable.TO_DATE(_DateDoc_From, true) + " And ");
                _sql.Append(GlobalVariable.TO_DATE(_DateDoc_To, true));
            }
            else if (_DateDoc_From != null && _DateDoc_To == null)
            {
                _sql.Append(" and cs.DueDate >=" + GlobalVariable.TO_DATE(_DateDoc_From, true));
            }
            else if (_DateDoc_From == null && _DateDoc_To != null)
            {
                _sql.Append(" and cs.DueDate  <=" + GlobalVariable.TO_DATE(_DateDoc_To, true));
            }


            DataSet ds = new DataSet();
            ds = DB.ExecuteDataset(_sql.ToString());
            if (ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    _payschedule = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_InvoicePaySchedule_ID"]);
                    _duedate= Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["DueDate"]);
                    _sysdate=System.DateTime.Now;
                    if (_duedate < _sysdate)
                    {
                        MInvoicePaySchedule PaySchedule = new MInvoicePaySchedule(GetCtx(), _payschedule, null);
                        PaySchedule.SetVA009_ExecutionStatus("O");
                        if (!PaySchedule.Save())
                        {
                        }
                    }
                }
                return Msg.GetMsg(GetCtx(), "VA009_ShduleCrted");
            }
            return Msg.GetMsg(GetCtx(), "VA009_ShduleNtFound");
        }

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
                else if (name.Equals("DueDate"))
                {
                    _DateDoc_From = (DateTime?)(para[i].GetParameter());
                    _DateDoc_To = (DateTime?)(para[i].GetParameter_To());
                }

            }
        }
    }
}
