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
    public class MVA009BatchLineDetails : X_VA009_BatchLineDetails
    {
        public MVA009BatchLineDetails(Ctx ctx, int VA009_BatchLineDetails_ID, Trx trxName)
            : base(ctx, VA009_BatchLineDetails_ID, trxName)
        {
        }
        public MVA009BatchLineDetails(Ctx ctx, DataRow rs, Trx trxName)
            : base(ctx, rs, trxName)
        {

        }
        protected override bool AfterSave(bool newRecord, bool success)
        {
            string sql = @"UPDATE va009_batchlines SET VA009_DueAmount = 
                           (SELECT SUM(DueAmt) FROM va009_batchlinedetails WHERE IsActive = 'Y' AND va009_batchlines_ID = " + GetVA009_BatchLines_ID() + " ) "
                           + " WHERE va009_batchlines_ID = " + GetVA009_BatchLines_ID();
            DB.ExecuteQuery(sql, null, Get_Trx());
            return true;
        }
    }
}
