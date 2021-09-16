using System;
using System.Net;
using System.Windows;
using VAdvantage.Utility;
using System.Data;
using VAdvantage.Model;
using System.Collections.Generic;
using VAdvantage.DataBase;
using ViennaAdvantage.Model;
using VAdvantage.Process;
using System.IO;
using System.Text;
using VAdvantage.Classes;
using VAdvantage.Logging;

namespace ViennaAdvantage.Model
{
    public class MVA009Batch : X_VA009_Batch, DocAction
    {
        /**	Process Message 			*/
        private String _processMsg = null;
        /**	Just Prepared Flag			*/
        private bool _justPrepared = false;
        //	Lines					
        private MVA009BatchLineDetails[] _lines = null;

        public MVA009Batch(Ctx ctx, int VA009_Batch_ID, Trx trxName)
            : base(ctx, VA009_Batch_ID, trxName)
        {
        }
        public MVA009Batch(Ctx ctx, DataRow rs, Trx trxName)
            : base(ctx, rs, trxName)
        {

        }

        public bool ApproveIt()
        {
            return true;
        }

        /// <summary>
        /// Close batch
        /// </summary>
        /// <returns>true/false</returns>
        public bool CloseIt()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(@"SELECT COUNT(va009_batchlines_id) FROM va009_batchlinedetails WHERE va009_batchlines_id IN
                    (SELECT va009_batchlines_id  FROM va009_batchlines  WHERE va009_batch_id= " + GetVA009_Batch_ID()
                    + "  ) AND c_payment_id IS NULL ");
            //Rakesh(VA228):Do not allow to close untill all payment not generated
            if (Util.GetValueOfInt(DB.ExecuteScalar(sql.ToString(), null, null)) > 0)
            {
                _processMsg = Msg.GetMsg(GetCtx(), "VA009_PaymentNotGenerated");
                return false;
            }
            log.Info(ToString());
            SetProcessed(true);
            SetDocAction(DOCACTION_None);
            #region CommentedOldCode
            //StringBuilder sql = new StringBuilder();
            //sql.Append(@"SELECT COUNT(va009_batchlines_id) FROM va009_batchlinedetails WHERE va009_batchlines_id IN
            //        (SELECT va009_batchlines_id  FROM va009_batchlines  WHERE va009_batch_id= " + GetVA009_Batch_ID()
            //        + "  ) AND c_payment_id IS NOT NULL ");
            //if (Util.GetValueOfInt(DB.ExecuteScalar(sql.ToString(), null, null)) > 0)
            //{
            //    return false;
            //}
            //else
            //{
            //    //to update processed to flase when we perform Close.
            //    int updateProcessedBD = Util.GetValueOfInt(DB.ExecuteQuery(@" Update va009_batchlinedetails set processed='N'
            //        WHERE va009_batchlines_id IN (SELECT va009_batchlines_id FROM va009_batchlines WHERE va009_batch_id= " + GetVA009_Batch_ID() +
            //        ")"));

            //    int updateProcessedBL = Util.GetValueOfInt(DB.ExecuteQuery(@" Update va009_batchlines set processed='N' 
            //                    WHERE va009_batch_id=" + GetVA009_Batch_ID()));
            //    if (updateProcessedBL > 0 && updateProcessedBD > 0)
            //    {
            //        //to update execution status to awaited when we perform delete.
            //        int schdeuleCount = Util.GetValueOfInt(DB.ExecuteQuery(@" UPDATE c_invoicepayschedule SET VA009_ExecutionStatus = 'A' WHERE c_invoicepayschedule_id IN
            //    (SELECT c_invoicepayschedule_id  FROM va009_batchlinedetails  WHERE va009_batchlines_id IN (SELECT va009_batchlines_id FROM va009_batchlines WHERE va009_batch_id= " + GetVA009_Batch_ID() + "))"));
            //        int OrdschdeuleCount = Util.GetValueOfInt(DB.ExecuteQuery(@" UPDATE va009_orderpayschedule SET VA009_ExecutionStatus = 'A'
            //    WHERE va009_orderpayschedule_id IN (SELECT va009_orderpayschedule_id  FROM va009_batchlinedetails  WHERE va009_batchlines_id IN (SELECT va009_batchlines_id FROM va009_batchlines WHERE va009_batch_id= " + GetVA009_Batch_ID() + "))"));
            //        if (schdeuleCount > 0 || OrdschdeuleCount > 0)
            //        {
            //            return true;
            //        }
            //    }
            //}
            #endregion
            return true;
        }

        public string CompleteIt()
        {
            //	Re-Check
            if (!_justPrepared)
            {
                String status = PrepareIt();
                if (!DocActionVariables.STATUS_INPROGRESS.Equals(status))
                    return status;
            }
            //to check if payment method is CHECK then skip otherwise set these values
            string _baseType = Util.GetValueOfString(DB.ExecuteScalar(@"SELECT VA009_PaymentBaseType FROM VA009_PaymentMethod WHERE 
                                VA009_PaymentMethod_ID=" + GetVA009_PaymentMethod_ID(), null,
             Get_TrxName()));
            if (_baseType != X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Check && _baseType != X_VA009_PaymentMethod.VA009_PAYMENTBASETYPE_Cash)
            {
                // to check whether partner bank account and account name is selected on batch line or not
                if (Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT COUNT(va009_batchlines_ID) FROM va009_batchlines
                WHERE c_bp_bankaccount_id IS NULL AND a_name IS NULL AND VA009_Batch_ID = " + GetVA009_Batch_ID(), null, null)) > 0)
                {
                    _processMsg = Msg.GetMsg(GetCtx(), "VA009_FillBankAcctName");
                    return DocActionVariables.STATUS_INVALID;
                }
            }
            return DocActionVariables.STATUS_COMPLETED;
        }

        /**
        * 	Get Process Message
        *	@return clear text error message
        */
        public String GetProcessMsg()
        {
            return _processMsg;
        }

        public FileInfo CreatePDF()
        {
            return null;
        }

        public decimal GetApprovalAmt()
        {
            return 0;
        }

        public int GetC_Currency_ID()
        {
            return 0;
        }

        public string GetDocBaseType()
        {
            return null;
        }

        public DateTime? GetDocumentDate()
        {
            return null;
        }

        public string GetDocumentInfo()
        {
            return null;
        }

        public int GetDoc_User_ID()
        {
            return GetCreatedBy();
        }

        public Env.QueryParams GetLineOrgsQueryInfo()
        {
            return null;
        }

        public string GetSummary()
        {
            return null;
        }

        public bool InvalidateIt()
        {
            return true;
        }
        /// <summary>
        /// Prepare batch
        /// </summary>
        /// <returns>status result</returns>
        public string PrepareIt()
        {
            //Rakesh(VA228):Implemented prediod closed and nonbusiness day check
            log.Info(ToString());
            _processMsg = ModelValidationEngine.Get().FireDocValidate(this, ModalValidatorVariables.DOCTIMING_BEFORE_PREPARE);
            if (_processMsg != null)
                return DocActionVariables.STATUS_INVALID;

            //	Std Period open?
            if (!MPeriod.IsOpen(GetCtx(), GetDateAcct(), "BAP", GetAD_Org_ID()))
            {
                _processMsg = "@PeriodClosed@";
                return DocActionVariables.STATUS_INVALID;
            }

            // is Non Business Day?
            // JID_1205: At the trx, need to check any non business day in that org. if not fund then check * org.
            if (MNonBusinessDay.IsNonBusinessDay(GetCtx(), GetDateAcct(), GetAD_Org_ID()))
            {
                _processMsg = VAdvantage.Common.Common.NONBUSINESSDAY;
                return DocActionVariables.STATUS_INVALID;
            }

            //	Lines
            MVA009BatchLineDetails[] lines = GetLines(true);
            if (lines == null || lines.Length == 0)
            {
                _processMsg = "@NoLines@";
                return DocActionVariables.STATUS_INVALID;
            }
            _justPrepared = true;
            if (!DOCACTION_Complete.Equals(GetDocAction()))
                SetDocAction(DOCACTION_Complete);
            return DocActionVariables.STATUS_INPROGRESS;
        }

        public bool ProcessIt(string action)
        {
            _processMsg = null;
            DocumentEngine engine = new DocumentEngine(this, GetDocStatus());
            return engine.ProcessIt(action, GetDocAction());
        }

        public bool ReActivateIt()
        {
            return true;
        }

        public bool RejectIt()
        {
            return true;
        }

        public bool ReverseAccrualIt()
        {
            return true;
        }

        public bool ReverseCorrectIt()
        {
            return true;
        }

        public bool UnlockIt()
        {
            return true;
        }
        /// <summary>
        /// Void batch
        /// </summary>
        /// <returns>true/false</returns>
        public bool VoidIt()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(@"SELECT COUNT(va009_batchlines_id) FROM va009_batchlinedetails WHERE va009_batchlines_id IN
                    (SELECT va009_batchlines_id  FROM va009_batchlines  WHERE va009_batch_id= " + GetVA009_Batch_ID()
                    + "  ) AND c_payment_id IS NOT NULL ");
            //Rakesh(VA228):Do not allow to void if any payment generated
            if (Util.GetValueOfInt(DB.ExecuteScalar(sql.ToString(), null, null)) > 0)
            {
                _processMsg = Msg.GetMsg(GetCtx(), "VA009_PaymentGenerated");
                return false;
            }
            log.Info(ToString());
            SetProcessed(true);
            SetDocAction(DOCACTION_None);
            return true;
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
                if (!GetDocStatus().Equals(DOCSTATUS_InProgress))
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

        /// <summary>
        /// Before Save
        /// </summary>
        /// <param name="newRecord">new</param>
        /// <returns>true if success</returns>
        protected override bool BeforeSave(bool newRecord)
        {
            if (!newRecord && (Is_ValueChanged("DateAcct") || Is_ValueChanged("C_Currency_Id") || Is_ValueChanged("C_ConversionType_ID")))
            {
                int count = Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT Count(VA009_BatchLineDetails_ID) FROM VA009_BatchLineDetails WHERE va009_batchlines_id IN (SELECT va009_batchlines_id 
                                                FROM va009_batchlines WHERE VA009_BATCH_ID=" + GetVA009_Batch_ID() + ")"));
                //Rakesh(VA228):Do not allow to change account date if any batch line detail exists
                if (count > 0)
                {
                    log.SaveError("VA009_PlsDeleteBatchLinesDtl", "");
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Get Lines of Shipment
        /// <param name="requery">requery refresh from db</param>
        /// <returns>lines</returns>
        /// </summary>
        public MVA009BatchLineDetails[] GetLines(bool requery)
        {
            if (_lines != null && !requery)
                return _lines;
            List<MVA009BatchLineDetails> list = new List<MVA009BatchLineDetails>();
            String sql = "SELECT * FROM VA009_BatchLineDetails WHERE va009_batchlines_id IN (SELECT va009_batchlines_id " +
                "FROM va009_batchlines WHERE VA009_BATCH_ID=" + GetVA009_Batch_ID() + ") ORDER BY VA009_BatchLineDetails_ID";
            DataSet ds = null;
            try
            {
                ds = DB.ExecuteDataset(sql, null, Get_TrxName());
                if (ds != null && ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        list.Add(new MVA009BatchLineDetails(GetCtx(), ds.Tables[0].Rows[i], Get_TrxName()));
                    }
                }
                ds = null;
            }
            catch (Exception ex)
            {
                log.Log(Level.SEVERE, sql, ex);
                list = null;
                ds.Dispose();
            }
            ds = null;
            //
            if (list == null)
                return null;
            _lines = new MVA009BatchLineDetails[list.Count];
            _lines = list.ToArray();
            return _lines;
        }
    }
}
