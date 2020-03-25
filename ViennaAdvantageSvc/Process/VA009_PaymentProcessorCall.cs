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
using System.Reflection;


namespace ViennaAdvantage.Process
{
    public class VA009_PaymentProcessorCall : SvrProcess
    {
        string className = string.Empty;
        
        protected override void Prepare()
        {
            MVA009Batch _batch = new MVA009Batch(GetCtx(), GetRecord_ID(), Get_TrxName());
            className = Util.GetValueOfString(DB.ExecuteScalar("SELECT pc.va009_name FROM va009_bankpaymentclass bpc INNER JOIN va009_paymentclass pc ON bpc.va009_paymentclass_id=pc.va009_paymentclass_id WHERE bpc.c_bankaccount_id=" + _batch.GetC_BankAccount_ID() + " AND bpc.ad_client_id=" + GetCtx().GetAD_Client_ID() + "", null, Get_TrxName()));
        }

        protected override string DoIt()
        {

            string[] dotSplit = className.Split('.');
            string methodName = dotSplit[dotSplit.Length - 1]; // Find method Name
            int startindex = className.LastIndexOf('.');
            className = className.Remove(startindex, methodName.Length + 1); // Find Class Path
            object obj=CallAnyClass(methodName, null, className);
            return obj.ToString();
        }

        public Object CallAnyClass(String methodName, object[] args, string fqClassame)
        {
            Type type = null;
            object oRes = null;
            if (type == null)
            {
                type = Type.GetType(fqClassame);
                if (type.IsClass)
                {
                    if (type.GetMethod(methodName) != null)
                    {
                        object oClass = Activator.CreateInstance(type);
                        MethodInfo method = type.GetMethod(methodName);
                        object[] obj = new object[] { GetRecord_ID(),GetCtx(),Get_TrxName() }; // Pass Batch ID 
                        oRes = method.Invoke(oClass, obj);
                    }
                }
            }

            return oRes;
        }
    }
}
