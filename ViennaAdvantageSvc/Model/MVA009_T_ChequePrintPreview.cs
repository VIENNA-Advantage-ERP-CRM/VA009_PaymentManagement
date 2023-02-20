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
    public class MVA009_T_ChequePrintPreview : X_VA009_T_ChequePrintPreview
    {
        public MVA009_T_ChequePrintPreview(Ctx ctx, int VA009_T_ChequePrintPreview_ID, Trx trxName)
           : base(ctx, VA009_T_ChequePrintPreview_ID, trxName)
        {
        }
        public MVA009_T_ChequePrintPreview(Ctx ctx, DataRow rs, Trx trxName)
            : base(ctx, rs, trxName)
        {

        }
    }
}
