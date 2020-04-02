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

namespace ViennaAdvantage.Model
{
    public class MVA009Batch: X_VA009_Batch, DocAction
    {
        /**	Process Message 			*/
        private String _processMsg = null;

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

        public bool CloseIt()
        {
            return true;
        }

        public string CompleteIt()
        {
            return DocActionVariables.STATUS_COMPLETED;
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
           return null ;
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

        public string GetProcessMsg()
        {
            return _processMsg ;
        }

        public string GetSummary()
        {
            return null;
        }

        public bool InvalidateIt()
        {
            return true;
        }

        public string PrepareIt()
        {
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

        public bool VoidIt()
        {
            return true;
        }
    }
}
