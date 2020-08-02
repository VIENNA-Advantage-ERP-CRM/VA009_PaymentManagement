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

        /// <summary>
        /// to implement delete functionality 
        /// </summary>
        /// <returns>flag whether deletion possible or not</returns>
        protected override bool BeforeDelete()
        {
            int count = Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT COUNT(*)
                        FROM va009_batchlinedetails WHERE va009_batchlines_id = " + GetVA009_BatchLines_ID() + " AND processed = 'Y' "));
            if (count > 0)
            {
                return false;
            }
            else
            {
                string docstatus = Util.GetValueOfString(DB.ExecuteScalar(@"SELECT VA009_Batch.DOCSTATUS
                                    FROM va009_batchlinedetails va009_batchlinedetails INNER JOIN va009_batchlines va009_batchlines
                                    ON va009_batchlines.va009_batchlines_ID=va009_batchlinedetails.va009_batchlines_ID
                                    INNER JOIN VA009_Batch VA009_Batch ON VA009_Batch.VA009_Batch_ID =va009_batchlines.VA009_Batch_ID
                                    WHERE va009_batchlinedetails.va009_batchlinedetails_id=" + GetVA009_BatchLineDetails_ID(), null, Get_Trx()));
                if (!docstatus.Equals(MVA009Batch.DOCSTATUS_InProgress))
                {
                    //to update execution status to awaited when we perform delete.
                    int schdeuleCount = Util.GetValueOfInt(DB.ExecuteQuery(@" UPDATE c_invoicepayschedule SET VA009_ExecutionStatus = 'A' WHERE c_invoicepayschedule_id IN
                (SELECT c_invoicepayschedule_id  FROM va009_batchlinedetails  WHERE va009_batchlines_id = " + GetVA009_BatchLines_ID() + "  )"));
                    int OrdschdeuleCount = Util.GetValueOfInt(DB.ExecuteQuery(@" UPDATE va009_orderpayschedule SET VA009_ExecutionStatus = 'A'
                WHERE va009_orderpayschedule_id IN (SELECT va009_orderpayschedule_id  FROM va009_batchlinedetails  WHERE va009_batchlines_id = " + GetVA009_BatchLines_ID() + "  )"));
                    if (schdeuleCount > 0 || OrdschdeuleCount > 0)
                    {
                        return true;
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
