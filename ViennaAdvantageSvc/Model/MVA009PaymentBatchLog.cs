using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAdvantage.DataBase;
using VAdvantage.Utility;

namespace ViennaAdvantage.Model
{
    public class MVA009PaymentBatchLog  : X_VA009_PaymentBatchLog
    {
        public MVA009PaymentBatchLog(Ctx ctx, int VA009_PaymentBatchLog_ID, Trx trx)
            : base(ctx, VA009_PaymentBatchLog_ID, trx)
        { 
        
        }

        public MVA009PaymentBatchLog(Ctx ctx, DataRow dr , Trx trx)
            : base(ctx, dr, trx)
        {

        }

        protected override bool BeforeSave(bool newRecord)
        {
            return base.BeforeSave(newRecord);
        }

        protected override bool AfterSave(bool newRecord, bool success)
        {
            return base.AfterSave(newRecord, success);
        }
    }
}
