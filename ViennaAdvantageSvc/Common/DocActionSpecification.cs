using System;
using System.Net;
using VAdvantage.Process;
using System.Collections.Generic;
using ViennaAdvantage.Model;

namespace ViennaAdvantage.Common
{
     //public class DocActionSpecification:VAdvantage.Interface.ModuleDocAction
    public class DocActionSpecification {
        
        public  string[] GetDocAtion(int AD_Table_ID, string docStatus)
        {

            List<String> opt = new List<String>();
            if (AD_Table_ID == MVA009Batch.Table_ID)
            {
                //	Draft                       ..  DR/IP/IN
                if (docStatus.Equals(DocumentEngine.STATUS_DRAFTED))
                {
                    opt.Add(DocumentEngine.ACTION_COMPLETE);
                    opt.Add(DocumentEngine.ACTION_PREPARE);
                    opt.Add(DocumentEngine.ACTION_VOID);
                    
                }
                else if (docStatus.Equals(DocumentEngine.STATUS_INPROGRESS))
                {
                    opt.Add(DocumentEngine.ACTION_COMPLETE);                    
                    opt.Add(DocumentEngine.ACTION_VOID);
                }
                else if(docStatus.Equals(DocumentEngine.STATUS_INVALID))
                {
                    opt.Add(DocumentEngine.ACTION_PREPARE);
                    opt.Add(DocumentEngine.ACTION_COMPLETE);
                    opt.Add(DocumentEngine.ACTION_VOID);
                }
                //	Complete                    ..  CO
                else if (docStatus.Equals(DocumentEngine.STATUS_COMPLETED))
                {
                    //opt.Add(DocumentEngine.ACTION_REACTIVATE);
                    //opt.Add(DocumentEngine.ACTION_REVERSE_CORRECT);
                    opt.Add(DocumentEngine.ACTION_CLOSE);
                    opt.Add(DocumentEngine.ACTION_VOID);
                }


            }
            return opt.ToArray();

        }
    }
}
