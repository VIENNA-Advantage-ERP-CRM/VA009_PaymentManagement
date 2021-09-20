namespace ViennaAdvantage.Model
{

    /** Generated Model - DO NOT CHANGE */
    using System;
    using System.Text;
    using VAdvantage.DataBase;
    using VAdvantage.Common;
    using VAdvantage.Classes;
    using VAdvantage.Process;
    using VAdvantage.Model;
    using VAdvantage.Utility;
    using System.Data;
    /** Generated Model for VA009_Batch
     *  @author Jagmohan Bhatt (generated) 
     *  @version Vienna Framework 1.1.1 - $Id$ */
    public class X_VA009_Batch : PO
    {
        public X_VA009_Batch(Context ctx, int VA009_Batch_ID, Trx trxName) : base(ctx, VA009_Batch_ID, trxName)
        {
            /** if (VA009_Batch_ID == 0)
{
SetDocumentNo (null);
SetVA009_Batch_ID (0);
}
             */
        }
        public X_VA009_Batch(Ctx ctx, int VA009_Batch_ID, Trx trxName)
            : base(ctx, VA009_Batch_ID, trxName)
        {
            /** if (VA009_Batch_ID == 0)
{
SetDocumentNo (null);
SetVA009_Batch_ID (0);
}
             */
        }
        /** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
        public X_VA009_Batch(Context ctx, DataRow rs, Trx trxName)
            : base(ctx, rs, trxName)
        {
        }
        /** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
        public X_VA009_Batch(Ctx ctx, DataRow rs, Trx trxName)
            : base(ctx, rs, trxName)
        {
        }
        /** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
        public X_VA009_Batch(Ctx ctx, IDataReader dr, Trx trxName)
            : base(ctx, dr, trxName)
        {
        }
        /** Static Constructor 
         Set Table ID By Table Name
         added by ->Harwinder */
        static X_VA009_Batch()
        {
            Table_ID = Get_Table_ID(Table_Name);
            model = new KeyNamePair(Table_ID, Table_Name);
        }
        /** Serial Version No */
        static long serialVersionUID = 27728081212925L;
        /** Last Updated Timestamp 10/27/2015 7:54:56 PM */
        public static long updatedMS = 1445955896136L;
        /** AD_Table_ID=1001014 */
        public static int Table_ID;
        // =1001014;

        /** TableName=VA009_Batch */
        public static String Table_Name = "VA009_Batch";

        protected static KeyNamePair model;
        protected Decimal accessLevel = new Decimal(7);
        /** AccessLevel
@return 7 - System - Client - Org 
*/
        protected override int Get_AccessLevel()
        {
            return Convert.ToInt32(accessLevel.ToString());
        }
        /** Load Meta Data
@param ctx context
@return PO Info
*/
        protected override POInfo InitPO(Context ctx)
        {
            POInfo poi = POInfo.GetPOInfo(ctx, Table_ID);
            return poi;
        }
        /** Load Meta Data
@param ctx context
@return PO Info
*/
        protected override POInfo InitPO(Ctx ctx)
        {
            POInfo poi = POInfo.GetPOInfo(ctx, Table_ID);
            return poi;
        }
        /** Info
@return info
*/
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder("X_VA009_Batch[").Append(Get_ID()).Append("]");
            return sb.ToString();
        }
        /** Set Bank Account.
@param C_BankAccount_ID Account at the Bank */
        public void SetC_BankAccount_ID(int C_BankAccount_ID)
        {
            if (C_BankAccount_ID <= 0) Set_Value("C_BankAccount_ID", null);
            else
                Set_Value("C_BankAccount_ID", C_BankAccount_ID);
        }
        /** Get Bank Account.
@return Account at the Bank */
        public int GetC_BankAccount_ID()
        {
            Object ii = Get_Value("C_BankAccount_ID");
            if (ii == null) return 0;
            return Convert.ToInt32(ii);
        }
        /** Set Bank.
@param C_Bank_ID Bank */
        public void SetC_Bank_ID(int C_Bank_ID)
        {
            if (C_Bank_ID <= 0) Set_Value("C_Bank_ID", null);
            else
                Set_Value("C_Bank_ID", C_Bank_ID);
        }
        /** Get Bank.
@return Bank */
        public int GetC_Bank_ID()
        {
            Object ii = Get_Value("C_Bank_ID");
            if (ii == null) return 0;
            return Convert.ToInt32(ii);
        }
        /** Set DocumentNo.
@param DocumentNo Document sequence number of the document */
        public void SetDocumentNo(String DocumentNo)
        {
            if (DocumentNo == null) throw new ArgumentException("DocumentNo is mandatory.");
            if (DocumentNo.Length > 14)
            {
                log.Warning("Length > 14 - truncated");
                DocumentNo = DocumentNo.Substring(0, 14);
            }
            Set_ValueNoCheck("DocumentNo", DocumentNo);
        }
        /** Get DocumentNo.
@return Document sequence number of the document */
        public String GetDocumentNo()
        {
            return (String)Get_Value("DocumentNo");
        }
        /** Set Export.
@param Export_ID Export */
        public void SetExport_ID(String Export_ID)
        {
            if (Export_ID != null && Export_ID.Length > 50)
            {
                log.Warning("Length > 50 - truncated");
                Export_ID = Export_ID.Substring(0, 50);
            }
            Set_Value("Export_ID", Export_ID);
        }
        /** Get Export.
@return Export */
        public String GetExport_ID()
        {
            return (String)Get_Value("Export_ID");
        }
        /** Set Processed.
@param Processed The document has been processed */
        public void SetProcessed(Boolean Processed)
        {
            Set_Value("Processed", Processed);
        }
        /** Get Processed.
@return The document has been processed */
        public Boolean IsProcessed()
        {
            Object oo = Get_Value("Processed");
            if (oo != null)
            {
                if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo);
                return "Y".Equals(oo);
            }
            return false;
        }
        /** Set Batch.
@param VA009_Batch_ID Batch */
        public void SetVA009_Batch_ID(int VA009_Batch_ID)
        {
            if (VA009_Batch_ID < 1) throw new ArgumentException("VA009_Batch_ID is mandatory.");
            Set_ValueNoCheck("VA009_Batch_ID", VA009_Batch_ID);
        }
        /** Get Batch.
@return Batch */
        public int GetVA009_Batch_ID()
        {
            Object ii = Get_Value("VA009_Batch_ID");
            if (ii == null) return 0;
            return Convert.ToInt32(ii);
        }
        /** Set Consolidate Payment.
@param VA009_Consolidate Consolidate Payment */
        public void SetVA009_Consolidate(Boolean VA009_Consolidate)
        {
            Set_Value("VA009_Consolidate", VA009_Consolidate);
        }
        /** Get Consolidate Payment.
@return Consolidate Payment */
        public Boolean IsVA009_Consolidate()
        {
            Object oo = Get_Value("VA009_Consolidate");
            if (oo != null)
            {
                if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo);
                return "Y".Equals(oo);
            }
            return false;
        }
        /** Set Description.
@param VA009_Description Description */
        public void SetVA009_Description(String VA009_Description)
        {
            if (VA009_Description != null && VA009_Description.Length > 50)
            {
                log.Warning("Length > 50 - truncated");
                VA009_Description = VA009_Description.Substring(0, 50);
            }
            Set_Value("VA009_Description", VA009_Description);
        }
        /** Get Description.
@return Description */
        public String GetVA009_Description()
        {
            return (String)Get_Value("VA009_Description");
        }
        /** Set Document Date .
@param VA009_DocumentDate Document Date  */
        public void SetVA009_DocumentDate(DateTime? VA009_DocumentDate)
        {
            Set_Value("VA009_DocumentDate", (DateTime?)VA009_DocumentDate);
        }
        /** Get Document Date .
@return Document Date  */
        public DateTime? GetVA009_DocumentDate()
        {
            return (DateTime?)Get_Value("VA009_DocumentDate");
        }
        /** Set File Number.
@param VA009_FileNo File Number */
        public void SetVA009_FileNo(String VA009_FileNo)
        {
            if (VA009_FileNo != null && VA009_FileNo.Length > 50)
            {
                log.Warning("Length > 50 - truncated");
                VA009_FileNo = VA009_FileNo.Substring(0, 50);
            }
            Set_Value("VA009_FileNo", VA009_FileNo);
        }
        /** Get File Number.
@return File Number */
        public String GetVA009_FileNo()
        {
            return (String)Get_Value("VA009_FileNo");
        }
        /** Set Generate File.
@param VA009_GenerateFile Generate File */
        public void SetVA009_GenerateFile(String VA009_GenerateFile)
        {
            if (VA009_GenerateFile != null && VA009_GenerateFile.Length > 10)
            {
                log.Warning("Length > 10 - truncated");
                VA009_GenerateFile = VA009_GenerateFile.Substring(0, 10);
            }
            Set_Value("VA009_GenerateFile", VA009_GenerateFile);
        }
        /** Get Generate File.
@return Generate File */
        public String GetVA009_GenerateFile()
        {
            return (String)Get_Value("VA009_GenerateFile");
        }
        /** Set Generate Lines.
@param VA009_GenerateLines Generate Lines */
        public void SetVA009_GenerateLines(String VA009_GenerateLines)
        {
            if (VA009_GenerateLines != null && VA009_GenerateLines.Length > 10)
            {
                log.Warning("Length > 10 - truncated");
                VA009_GenerateLines = VA009_GenerateLines.Substring(0, 10);
            }
            Set_Value("VA009_GenerateLines", VA009_GenerateLines);
        }
        /** Get Generate Lines.
@return Generate Lines */
        public String GetVA009_GenerateLines()
        {
            return (String)Get_Value("VA009_GenerateLines");
        }
        /** Set Generate Payment.
@param VA009_GeneratePayment Generate Payment */
        public void SetVA009_GeneratePayment(String VA009_GeneratePayment)
        {
            if (VA009_GeneratePayment != null && VA009_GeneratePayment.Length > 10)
            {
                log.Warning("Length > 10 - truncated");
                VA009_GeneratePayment = VA009_GeneratePayment.Substring(0, 10);
            }
            Set_Value("VA009_GeneratePayment", VA009_GeneratePayment);
        }
        /** Get Generate Payment.
@return Generate Payment */
        public String GetVA009_GeneratePayment()
        {
            return (String)Get_Value("VA009_GeneratePayment");
        }
        /** Set Payment Method.
@param VA009_PaymentMethod_ID Payment Method */
        public void SetVA009_PaymentMethod_ID(int VA009_PaymentMethod_ID)
        {
            if (VA009_PaymentMethod_ID <= 0) Set_Value("VA009_PaymentMethod_ID", null);
            else
                Set_Value("VA009_PaymentMethod_ID", VA009_PaymentMethod_ID);
        }
        /** Get Payment Method.
@return Payment Method */
        public int GetVA009_PaymentMethod_ID()
        {
            Object ii = Get_Value("VA009_PaymentMethod_ID");
            if (ii == null) return 0;
            return Convert.ToInt32(ii);
        }

        /** VA009_PaymentRule AD_Reference_ID=1000413 */
        public static int VA009_PAYMENTRULE_AD_Reference_ID = 1000413;
        /** EFT = E */
        public static String VA009_PAYMENTRULE_EFT = "E";
        /** Manual = M */
        public static String VA009_PAYMENTRULE_Manual = "M";
        /** Is test a valid value.
@param test testvalue
@returns true if valid **/
        public bool IsVA009_PaymentRuleValid(String test)
        {
            return test == null || test.Equals("E") || test.Equals("M");
        }
        /** Set Payment Rule.
@param VA009_PaymentRule Payment Rule */
        public void SetVA009_PaymentRule(String VA009_PaymentRule)
        {
            if (!IsVA009_PaymentRuleValid(VA009_PaymentRule))
                throw new ArgumentException("VA009_PaymentRule Invalid value - " + VA009_PaymentRule + " - Reference_ID=1000413 - E - M");
            if (VA009_PaymentRule != null && VA009_PaymentRule.Length > 1)
            {
                log.Warning("Length > 1 - truncated");
                VA009_PaymentRule = VA009_PaymentRule.Substring(0, 1);
            }
            Set_Value("VA009_PaymentRule", VA009_PaymentRule);
        }
        /** Get Payment Rule.
@return Payment Rule */
        public String GetVA009_PaymentRule()
        {
            return (String)Get_Value("VA009_PaymentRule");
        }
        /** Set Payment Status.
@param VA009_PaymentStatus Payment Status */
        public void SetVA009_PaymentStatus(String VA009_PaymentStatus)
        {
            if (VA009_PaymentStatus != null && VA009_PaymentStatus.Length > 10)
            {
                log.Warning("Length > 10 - truncated");
                VA009_PaymentStatus = VA009_PaymentStatus.Substring(0, 10);
            }
            Set_Value("VA009_PaymentStatus", VA009_PaymentStatus);
        }
        /** Get Payment Status.
@return Payment Status */
        public String GetVA009_PaymentStatus()
        {
            return (String)Get_Value("VA009_PaymentStatus");
        }

        /** VA009_PaymentTrigger AD_Reference_ID=1000401 */
        public static int VA009_PAYMENTTRIGGER_AD_Reference_ID = 1000401;
        /** Pull By Recipient = R */
        public static String VA009_PAYMENTTRIGGER_PullByRecipient = "R";
        /** Push By Sender = S */
        public static String VA009_PAYMENTTRIGGER_PushBySender = "S";
        /** Is test a valid value.
@param test testvalue
@returns true if valid **/
        public bool IsVA009_PaymentTriggerValid(String test)
        {
            return test == null || test.Equals("R") || test.Equals("S");
        }
        /** Set Payment Trigger By.
@param VA009_PaymentTrigger Payment Trigger By */
        public void SetVA009_PaymentTrigger(String VA009_PaymentTrigger)
        {
            if (!IsVA009_PaymentTriggerValid(VA009_PaymentTrigger))
                throw new ArgumentException("VA009_PaymentTrigger Invalid value - " + VA009_PaymentTrigger + " - Reference_ID=1000401 - R - S");
            if (VA009_PaymentTrigger != null && VA009_PaymentTrigger.Length > 1)
            {
                log.Warning("Length > 1 - truncated");
                VA009_PaymentTrigger = VA009_PaymentTrigger.Substring(0, 1);
            }
            Set_Value("VA009_PaymentTrigger", VA009_PaymentTrigger);
        }
        /** Get Payment Trigger By.
@return Payment Trigger By */
        public String GetVA009_PaymentTrigger()
        {
            return (String)Get_Value("VA009_PaymentTrigger");
        }
        /** Set Total Amount.
@param VA009_TotalAmt Total Amount */
        public void SetVA009_TotalAmt(Decimal? VA009_TotalAmt)
        {
            Set_Value("VA009_TotalAmt", (Decimal?)VA009_TotalAmt);
        }
        /** Get Total Amount.
@return Total Amount */
        public Decimal GetVA009_TotalAmt()
        {
            Object bd = Get_Value("VA009_TotalAmt");
            if (bd == null) return Env.ZERO;
            return Convert.ToDecimal(bd);
        }

        /** DocAction AD_Reference_ID=135 */
        public static int DOCACTION_AD_Reference_ID = 135;/** <None> = -- */
        public static String DOCACTION_None = "--";/** Approve = AP */
        public static String DOCACTION_Approve = "AP";/** Close = CL */
        public static String DOCACTION_Close = "CL";/** Complete = CO */
        public static String DOCACTION_Complete = "CO";/** Invalidate = IN */
        public static String DOCACTION_Invalidate = "IN";/** Post = PO */
        public static String DOCACTION_Post = "PO";/** Prepare = PR */
        public static String DOCACTION_Prepare = "PR";/** Reverse - Accrual = RA */
        public static String DOCACTION_Reverse_Accrual = "RA";/** Reverse - Correct = RC */
        public static String DOCACTION_Reverse_Correct = "RC";/** Re-activate = RE */
        public static String DOCACTION_Re_Activate = "RE";/** Reject = RJ */
        public static String DOCACTION_Reject = "RJ";/** Void = VO */
        public static String DOCACTION_Void = "VO";/** Wait Complete = WC */
        public static String DOCACTION_WaitComplete = "WC";/** Unlock = XL */
        public static String DOCACTION_Unlock = "XL";/** Is test a valid value.
@param test testvalue
@returns true if valid **/
        public bool IsDocActionValid(String test) { return test.Equals("--") || test.Equals("AP") || test.Equals("CL") || test.Equals("CO") || test.Equals("IN") || test.Equals("PO") || test.Equals("PR") || test.Equals("RA") || test.Equals("RC") || test.Equals("RE") || test.Equals("RJ") || test.Equals("VO") || test.Equals("WC") || test.Equals("XL"); }/** Set Document Action.
@param DocAction The targeted status of the document */
        public void SetDocAction(String DocAction)
        {
            if (DocAction == null) throw new ArgumentException("DocAction is mandatory"); if (!IsDocActionValid(DocAction))
                throw new ArgumentException("DocAction Invalid value - " + DocAction + " - Reference_ID=135 - -- - AP - CL - CO - IN - PO - PR - RA - RC - RE - RJ - VO - WC - XL"); if (DocAction.Length > 2) { log.Warning("Length > 2 - truncated"); DocAction = DocAction.Substring(0, 2); }
            Set_Value("DocAction", DocAction);
        }/** Get Document Action.
@return The targeted status of the document */
        public String GetDocAction() { return (String)Get_Value("DocAction"); }
        /** DocStatus AD_Reference_ID=131 */
        public static int DOCSTATUS_AD_Reference_ID = 131;/** Unknown = ?? */
        public static String DOCSTATUS_Unknown = "??";/** Approved = AP */
        public static String DOCSTATUS_Approved = "AP";/** Closed = CL */
        public static String DOCSTATUS_Closed = "CL";/** Completed = CO */
        public static String DOCSTATUS_Completed = "CO";/** Drafted = DR */
        public static String DOCSTATUS_Drafted = "DR";/** Invalid = IN */
        public static String DOCSTATUS_Invalid = "IN";/** In Progress = IP */
        public static String DOCSTATUS_InProgress = "IP";/** Not Approved = NA */
        public static String DOCSTATUS_NotApproved = "NA";/** Reversed = RE */
        public static String DOCSTATUS_Reversed = "RE";/** Voided = VO */
        public static String DOCSTATUS_Voided = "VO";/** Waiting Confirmation = WC */
        public static String DOCSTATUS_WaitingConfirmation = "WC";/** Waiting Payment = WP */
        public static String DOCSTATUS_WaitingPayment = "WP";/** Is test a valid value.
@param test testvalue
@returns true if valid **/
        public bool IsDocStatusValid(String test) { return test.Equals("??") || test.Equals("AP") || test.Equals("CL") || test.Equals("CO") || test.Equals("DR") || test.Equals("IN") || test.Equals("IP") || test.Equals("NA") || test.Equals("RE") || test.Equals("VO") || test.Equals("WC") || test.Equals("WP"); }/** Set Document Status.
@param DocStatus The current status of the document */
        public void SetDocStatus(String DocStatus)
        {
            if (DocStatus == null) throw new ArgumentException("DocStatus is mandatory"); if (!IsDocStatusValid(DocStatus))
                throw new ArgumentException("DocStatus Invalid value - " + DocStatus + " - Reference_ID=131 - ?? - AP - CL - CO - DR - IN - IP - NA - RE - VO - WC - WP"); if (DocStatus.Length > 2) { log.Warning("Length > 2 - truncated"); DocStatus = DocStatus.Substring(0, 2); }
            Set_Value("DocStatus", DocStatus);
        }/** Get Document Status.
@return The current status of the document */
        public String GetDocStatus() { return (String)Get_Value("DocStatus"); }/** Set Account Date.
@param DateAcct General Ledger Date */
        public void SetDateAcct(DateTime? DateAcct) { Set_Value("DateAcct", (DateTime?)DateAcct); }/** Get Account Date.
@return General Ledger Date */
        public DateTime? GetDateAcct() { return (DateTime?)Get_Value("DateAcct"); }/** Set Currency Rate Type.
@param C_ConversionType_ID Currency Conversion Rate Type */
        public void SetC_ConversionType_ID(int C_ConversionType_ID)
        {
            if (C_ConversionType_ID <= 0) Set_Value("C_ConversionType_ID", null);
            else
                Set_Value("C_ConversionType_ID", C_ConversionType_ID);
        }/** Get Currency Rate Type.
@return Currency Conversion Rate Type */
        public int GetC_ConversionType_ID() { Object ii = Get_Value("C_ConversionType_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Currency.
@param C_Currency_ID The Currency for this record */
        public void SetC_Currency_ID(int C_Currency_ID)
        {
            if (C_Currency_ID <= 0) Set_Value("C_Currency_ID", null);
            else
                Set_Value("C_Currency_ID", C_Currency_ID);
        }/** Get Currency.
@return The Currency for this record */
        public int GetC_Currency_ID() { Object ii = Get_Value("C_Currency_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Document Type.
@param C_DocType_ID Document type or rules */
        public void SetC_DocType_ID(int C_DocType_ID)
        {
            if (C_DocType_ID <= 0) Set_Value("C_DocType_ID", null);
            else
                Set_Value("C_DocType_ID", C_DocType_ID);
        }/** Get Document Type.
@return Document type or rules */
        public int GetC_DocType_ID() { Object ii = Get_Value("C_DocType_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }
    }

}
