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

        /// <summary>
        /// After Save Logic Implement
        /// </summary>
        /// <param name="newRecord">Is new Record</param>
        /// <param name="success">Success or not</param>
        /// <returns>True, when save</returns>
        protected override bool AfterSave(bool newRecord, bool success)
        {
            string sql = @"UPDATE VA009_Batchlines SET VA009_DueAmount = 
                          (SELECT SUM(VA009_ConvertedAmt) FROM VA009_BatchLineDetails WHERE IsActive = 'Y' 
                            AND VA009_BatchLines_ID = " + GetVA009_BatchLines_ID() + @" ) 
                            WHERE VA009_BatchLines_ID = " + GetVA009_BatchLines_ID();
            DB.ExecuteQuery(sql, null, Get_Trx());
            return true;
        }

        /// <summary>
        /// to implement delete functionality 
        /// </summary>
        /// <returns>flag whether deletion possible or not</returns>
        protected override bool BeforeDelete()
        {
            int count = Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT COUNT(VA009_BatchLineDetails_ID)
                        FROM VA009_BatchLineDetails WHERE VA009_BatchLines_ID = " + GetVA009_BatchLines_ID() + " AND processed = 'Y' "));
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
                    int schdeuleCount = 0;
                    if (GetC_InvoicePaySchedule_ID() > 0)
                    {
                        schdeuleCount = Util.GetValueOfInt(DB.ExecuteQuery($@" UPDATE C_InvoicePaySchedule SET VA009_ExecutionStatus = 'A' 
                        WHERE C_InvoicePaySchedule_ID = {GetC_InvoicePaySchedule_ID()}" , null , Get_Trx()));
                    }

                    if (Get_ValueAsInt("VA009_OrderPaySchedule_ID") > 0)
                    {
                        schdeuleCount = Util.GetValueOfInt(DB.ExecuteQuery($@" UPDATE VA009_OrderPaySchedule SET VA009_ExecutionStatus = 'A' 
                      WHERE VA009_OrderPaySchedule_ID = {Get_ValueAsInt("VA009_OrderPaySchedule_ID")}", null, Get_Trx()));
                    }

                    if (Get_ValueAsInt("GL_JournalLine_ID") > 0)
                    {
                        schdeuleCount = Util.GetValueOfInt(DB.ExecuteQuery($@" UPDATE GL_JournalLine SET VA009_IsAssignedtoBatch = 'N' 
                       WHERE GL_JournalLine_ID = {Get_ValueAsInt("GL_JournalLine_ID")}", null, Get_Trx()));
                    }
                    if (schdeuleCount > 0)
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
