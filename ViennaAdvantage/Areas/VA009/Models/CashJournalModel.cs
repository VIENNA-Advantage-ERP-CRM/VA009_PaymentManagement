/********************************************************
 * Module Name    : Payment Management
 * Purpose        : used for Cash Journal related methods
 * Chronological Development
 * VA230     07/Mar/2022
  ******************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using VAdvantage.DataBase;
using VAdvantage.Utility;

namespace VA009.Models
{
    public class CashJournalModel
    {
        /// <summary>
        /// Get Order payment Schedule Details
        /// Author:VA230
        /// </summary>        
        /// <param name="fields">Order Id</param>
        /// <returns>Order Payment Schedule Detail</returns>
        public Dictionary<string, object> GetOrderPaySchedDetail(string fields)
        {
            Dictionary<string, object> retValue = null;
            int Order_ID = Util.GetValueOfInt(fields);
            DataSet _ds = null;
            StringBuilder _Sql = new StringBuilder();
            _Sql.Append(@"SELECT * FROM (SELECT OPS.VA009_OrderPaySchedule_ID,O.IsReturnTrx,OPS.DueAmt,O.IsSOTrx,O.C_BPartner_ID, O.C_Currency_ID
                                                    , O.C_ConversionType_ID,O.C_DocTypeTarget_ID,O.C_BPartner_Location_ID,DT.DocBaseType");
            if (Env.IsModuleInstalled("VA009_"))
            {
                _Sql.Append(@",Ops.VA009_PaymentMethod_ID,pm.VA009_PaymentBaseType");
            }
            _Sql.Append(@" FROM C_Order O
                                    INNER JOIN VA009_OrderPaySchedule Ops ON (O.C_Order_ID = ops.C_Order_ID)
                                    INNER JOIN C_DocType DT ON DT.C_DocType_ID=O.C_DocTypeTarget_ID");
            if (Env.IsModuleInstalled("VA009_"))
            {
                _Sql.Append(@" INNER JOIN VA009_PaymentMethod PM ON(Ops.VA009_PaymentMethod_id = pm.VA009_PaymentMethod_ID)");
            }
            _Sql.Append(@" WHERE
                            Ops.IsActive = 'Y'
                            AND O.C_Order_ID = " + Order_ID + @"
                            AND NVL(Ops.C_Payment_ID, 0)=0 AND NVL(Ops.C_CashLine_ID, 0)=0
                            AND ops.VA009_OrderPaySchedule_ID NOT IN (SELECT nvl(VA009_OrderPaySchedule_ID,0) FROM C_Payment WHERE DocStatus NOT IN ('CO', 'CL' ,'RE','VO'))
                            AND ops.VA009_ExecutionStatus NOT IN ('Y','J')
                            ORDER BY Ops.DueDate ASC
                            ) t WHERE ROWNUM=1");
            try
            {
                _ds = DB.ExecuteDataset(_Sql.ToString(), null, null);
                if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                {
                    retValue = new Dictionary<string, object>();
                    retValue["VA009_OrderPaySchedule_ID"] = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["VA009_OrderPaySchedule_ID"]);
                    retValue["DueAmount"] = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["DueAmt"]);
                    retValue["IsReturnTrx"] = Util.GetValueOfString(_ds.Tables[0].Rows[0]["IsReturnTrx"]);

                    retValue["C_BPartner_ID"] = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BPartner_ID"]);
                    retValue["C_Currency_ID"] = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_Currency_ID"]);
                    retValue["C_ConversionType_ID"] = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_ConversionType_ID"]);
                    retValue["IsSOTrx"] = Util.GetValueOfString(_ds.Tables[0].Rows[0]["IsSOTrx"]);
                    retValue["DocBaseType"] = Util.GetValueOfString(_ds.Tables[0].Rows[0]["DocBaseType"]);
                    retValue["C_BPartner_Location_ID"] = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BPartner_Location_ID"]);
                    //to get VA009_PaymentMethod_ID and VA009_PaymentBaseType To Set the corrosponding value on Cash journal line Window..
                    if (Env.IsModuleInstalled("VA009_"))
                    {
                        retValue["VA009_PaymentMethod_ID"] = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["VA009_PaymentMethod_ID"]);
                        retValue["VA009_PaymentBaseType"] = Util.GetValueOfString(_ds.Tables[0].Rows[0]["VA009_PaymentBaseType"]);
                    }
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                if (_ds != null)
                {
                    _ds.Dispose();
                }
            }
            return retValue;
        }
        /// <summary>
        /// Get Order Pay Schedule Amount
        /// Author:VA230
        /// </summary>        
        /// <param name="fields">Order Pay ScheduleId</param>
        /// <returns>Due Amount</returns>
        public decimal GetPaySheduleAmount(string fields)
        {
            string _sql = "SELECT DueAmt FROM VA009_OrderPaySchedule WHERE VA009_OrderPaySchedule_ID=" + Util.GetValueOfInt(fields);
            return Util.GetValueOfDecimal(DB.ExecuteScalar(_sql, null, null));
        }
    }
}