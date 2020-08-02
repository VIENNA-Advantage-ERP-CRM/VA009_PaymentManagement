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
    public class MVA009BatchLines : X_VA009_BatchLines
    {
        public MVA009BatchLines(Ctx ctx, int VA009_BatchLines_ID, Trx trxName)
            : base(ctx, VA009_BatchLines_ID, trxName)
        {
        }
        public MVA009BatchLines(Ctx ctx, DataRow rs, Trx trxName)
           : base(ctx, rs, trxName)
        {

        }
        /// <summary>
        /// to implement delete functionality 
        /// </summary>
        /// <returns>flag whether deletion possible or not</returns>
        protected override bool BeforeDelete()
        {
            int count = Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT COUNT(*)
                        FROM va009_batchlines WHERE va009_batch_id = " + GetVA009_Batch_ID() + " AND processed = 'Y' "));
            if (count > 0)
            {
                return false;
            }
            else
            {
                string docstatus = Util.GetValueOfString(DB.ExecuteScalar("SELECT DOCSTATUS FROM VA009_Batch WHERE VA009_Batch_ID = " + GetVA009_Batch_ID(), null, Get_Trx()));
                if (!docstatus.Equals(MVA009Batch.DOCSTATUS_InProgress))
                {
                    //to update va009executionstatus to awaited 
                    int updateProcessedBD = Util.GetValueOfInt(DB.ExecuteQuery(@" Update va009_batchlinedetails set processed='N'
                    WHERE va009_batchlines_id IN (SELECT va009_batchlines_id FROM va009_batchlines WHERE va009_batch_id= " + GetVA009_Batch_ID() +
                        ")"));
                    if (updateProcessedBD > 0)
                    {
                        //to update execution status to awaited when we perform delete.
                        int schdeuleCount = Util.GetValueOfInt(DB.ExecuteQuery(@" UPDATE c_invoicepayschedule SET VA009_ExecutionStatus = 'A' WHERE c_invoicepayschedule_id IN
                (SELECT c_invoicepayschedule_id  FROM va009_batchlinedetails  WHERE va009_batchlines_id IN (SELECT va009_batchlines_id FROM va009_batchlines WHERE va009_batch_id= " + GetVA009_Batch_ID() + "))"));
                        int OrdschdeuleCount = Util.GetValueOfInt(DB.ExecuteQuery(@" UPDATE va009_orderpayschedule SET VA009_ExecutionStatus = 'A'
                WHERE va009_orderpayschedule_id IN (SELECT va009_orderpayschedule_id  FROM va009_batchlinedetails  WHERE va009_batchlines_id IN (SELECT va009_batchlines_id FROM va009_batchlines WHERE va009_batch_id= " + GetVA009_Batch_ID() + "))"));
                        if (schdeuleCount > 0 || OrdschdeuleCount > 0)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;

        }
    }
}
