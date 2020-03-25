using System;
using System.Net;
using System.Windows;
using VAdvantage.Utility;
using System.Data;
using VAdvantage.Model;
using System.Collections.Generic;
using VAdvantage.DataBase;
using ViennaAdvantage.Model;


namespace ViennaAdvantage.Model
{
    public class MVA009PaymentMethod : X_VA009_PaymentMethod
    {
        public MVA009PaymentMethod(Ctx ctx, int VA009_PaymentMethod_ID, Trx trxName)
            : base(ctx, VA009_PaymentMethod_ID, trxName)
        {
        }
        public MVA009PaymentMethod(Ctx ctx, DataRow rs, Trx trxName)
            : base(ctx, rs, trxName)
        {

        }

        protected override bool BeforeSave(bool newRecord)
        {
            // when we save/update record -
            // for payment base type = cash
            // not to save duplicate data based on payment base type / tenant / Currency
            //Issue ID -- SI_0519_4 (System is giving message - combination already exist. on change on search key and name of existing record.)
            string sql = @"SELECT COUNT(*) FROM VA009_PaymentMethod WHERE IsActive = 'Y' AND VA009_PaymentBaseType = 'B' 
                  AND NVL(C_Currency_ID , 0) = " + GetC_Currency_ID() + @" AND AD_Client_ID = " + GetAD_Client_ID();

            if (!newRecord)
                sql += " AND VA009_PaymentMethod_ID !=" + GetVA009_PaymentMethod_ID(); 

            if (GetVA009_PaymentBaseType() == "B" && IsActive() &&
                Util.GetValueOfInt(DB.ExecuteScalar(sql, null, Get_Trx())) > 0)
            {
                log.SaveError("Error", Msg.GetMsg(GetCtx(), "VA009_duplicateRecordNotSave"));
                return false;
            }
            return true;
        }
    }
}

