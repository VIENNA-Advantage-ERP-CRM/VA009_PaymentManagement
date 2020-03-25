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
    public class MVA009BatchLines: X_VA009_BatchLines
    {
        public MVA009BatchLines(Ctx ctx, int VA009_BatchLines_ID, Trx trxName)
            : base(ctx, VA009_BatchLines_ID, trxName)
        {
        }
         public MVA009BatchLines(Ctx ctx, DataRow rs, Trx trxName)
            : base(ctx, rs, trxName)
        {

        }
    }
}
