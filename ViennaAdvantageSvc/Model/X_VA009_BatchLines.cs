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
    using System.Data;/** Generated Model for VA009_BatchLines
 *  @author Raghu (Updated) 
 *  @version Vienna Framework 1.1.1 - $Id$ */
    public class X_VA009_BatchLines : PO
    {
        public X_VA009_BatchLines(Context ctx, int VA009_BatchLines_ID, Trx trxName) : base(ctx, VA009_BatchLines_ID, trxName)
        {/** if (VA009_BatchLines_ID == 0){SetVA009_BatchLines_ID (0);SetVA009_Batch_ID (0);} */
        }
        public X_VA009_BatchLines(Ctx ctx, int VA009_BatchLines_ID, Trx trxName) : base(ctx, VA009_BatchLines_ID, trxName)
        {/** if (VA009_BatchLines_ID == 0){SetVA009_BatchLines_ID (0);SetVA009_Batch_ID (0);} */
        }/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
        public X_VA009_BatchLines(Context ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName) { }/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
        public X_VA009_BatchLines(Ctx ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName) { }/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
        public X_VA009_BatchLines(Ctx ctx, IDataReader dr, Trx trxName) : base(ctx, dr, trxName) { }/** Static Constructor 
 Set Table ID By Table Name
 added by ->Harwinder */
        static X_VA009_BatchLines() { Table_ID = Get_Table_ID(Table_Name); model = new KeyNamePair(Table_ID, Table_Name); }/** Serial Version No */
        static long serialVersionUID = 27927030408561L;/** Last Updated Timestamp 2/15/2022 6:04:51 AM */
        public static long updatedMS = 1644905091772L;/** AD_Table_ID=1001015 */
        public static int Table_ID; // =1001015;
        /** TableName=VA009_BatchLines */
        public static String Table_Name = "VA009_BatchLines";
        protected static KeyNamePair model; protected Decimal accessLevel = new Decimal(7);/** AccessLevel
@return 7 - System - Client - Org 
*/
        protected override int Get_AccessLevel() { return Convert.ToInt32(accessLevel.ToString()); }/** Load Meta Data
@param ctx context
@return PO Info
*/
        protected override POInfo InitPO(Context ctx) { POInfo poi = POInfo.GetPOInfo(ctx, Table_ID); return poi; }/** Load Meta Data
@param ctx context
@return PO Info
*/
        protected override POInfo InitPO(Ctx ctx) { POInfo poi = POInfo.GetPOInfo(ctx, Table_ID); return poi; }/** Info
@return info
*/
        public override String ToString() { StringBuilder sb = new StringBuilder("X_VA009_BatchLines[").Append(Get_ID()).Append("]"); return sb.ToString(); }/** Set Account Name.
@param A_Name Name on Credit Card or Account holder */
        public void SetA_Name(String A_Name) { if (A_Name != null && A_Name.Length > 60) { log.Warning("Length > 60 - truncated"); A_Name = A_Name.Substring(0, 60); } Set_Value("A_Name", A_Name); }/** Get Account Name.
@return Name on Credit Card or Account holder */
        public String GetA_Name() { return (String)Get_Value("A_Name"); }/** Set Account No.
@param AccountNo Account Number */
        public void SetAccountNo(String AccountNo) { if (AccountNo != null && AccountNo.Length > 20) { log.Warning("Length > 20 - truncated"); AccountNo = AccountNo.Substring(0, 20); } Set_Value("AccountNo", AccountNo); }/** Get Account No.
@return Account Number */
        public String GetAccountNo() { return (String)Get_Value("AccountNo"); }/** Set Partner Bank Account.
@param C_BP_BankAccount_ID Bank Account of the Business Partner */
        public void SetC_BP_BankAccount_ID(int C_BP_BankAccount_ID)
        {
            if (C_BP_BankAccount_ID <= 0) Set_Value("C_BP_BankAccount_ID", null);
            else
                Set_Value("C_BP_BankAccount_ID", C_BP_BankAccount_ID);
        }/** Get Partner Bank Account.
@return Bank Account of the Business Partner */
        public int GetC_BP_BankAccount_ID() { Object ii = Get_Value("C_BP_BankAccount_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Business Partner.
@param C_BPartner_ID Identifies a Customer/Prospect */
        public void SetC_BPartner_ID(int C_BPartner_ID)
        {
            if (C_BPartner_ID <= 0) Set_Value("C_BPartner_ID", null);
            else
                Set_Value("C_BPartner_ID", C_BPartner_ID);
        }/** Get Business Partner.
@return Identifies a Customer/Prospect */
        public int GetC_BPartner_ID() { Object ii = Get_Value("C_BPartner_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Cash Journal Line.
@param C_CashLine_ID Cash Journal Line */
        public void SetC_CashLine_ID(int C_CashLine_ID)
        {
            if (C_CashLine_ID <= 0) Set_Value("C_CashLine_ID", null);
            else
                Set_Value("C_CashLine_ID", C_CashLine_ID);
        }/** Get Cash Journal Line.
@return Cash Journal Line */
        public int GetC_CashLine_ID() { Object ii = Get_Value("C_CashLine_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Cash Journal.
@param C_Cash_ID Cash Journal */
        public void SetC_Cash_ID(int C_Cash_ID)
        {
            if (C_Cash_ID <= 0) Set_Value("C_Cash_ID", null);
            else
                Set_Value("C_Cash_ID", C_Cash_ID);
        }/** Get Cash Journal.
@return Cash Journal */
        public int GetC_Cash_ID() { Object ii = Get_Value("C_Cash_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Payment.
@param C_Payment_ID Payment identifier */
        public void SetC_Payment_ID(int C_Payment_ID)
        {
            if (C_Payment_ID <= 0) Set_Value("C_Payment_ID", null);
            else
                Set_Value("C_Payment_ID", C_Payment_ID);
        }/** Get Payment.
@return Payment identifier */
        public int GetC_Payment_ID() { Object ii = Get_Value("C_Payment_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Export.
@param Export_ID Export */
        public void SetExport_ID(String Export_ID) { if (Export_ID != null && Export_ID.Length > 50) { log.Warning("Length > 50 - truncated"); Export_ID = Export_ID.Substring(0, 50); } Set_Value("Export_ID", Export_ID); }/** Get Export.
@return Export */
        public String GetExport_ID() { return (String)Get_Value("Export_ID"); }/** Set Processed.
@param Processed The document has been processed */
        public void SetProcessed(Boolean Processed) { Set_Value("Processed", Processed); }/** Get Processed.
@return The document has been processed */
        public Boolean IsProcessed() { Object oo = Get_Value("Processed"); if (oo != null) { if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo); return "Y".Equals(oo); } return false; }/** Set Routing No.
@param RoutingNo Bank Routing Number */
        public void SetRoutingNo(String RoutingNo) { if (RoutingNo != null && RoutingNo.Length > 20) { log.Warning("Length > 20 - truncated"); RoutingNo = RoutingNo.Substring(0, 20); } Set_Value("RoutingNo", RoutingNo); }/** Get Routing No.
@return Bank Routing Number */
        public String GetRoutingNo() { return (String)Get_Value("RoutingNo"); }/** Set BP Mandate.
@param VA009_BPMandate_ID BP Mandate */
        public void SetVA009_BPMandate_ID(int VA009_BPMandate_ID)
        {
            if (VA009_BPMandate_ID <= 0) Set_Value("VA009_BPMandate_ID", null);
            else
                Set_Value("VA009_BPMandate_ID", VA009_BPMandate_ID);
        }/** Get BP Mandate.
@return BP Mandate */
        public int GetVA009_BPMandate_ID() { Object ii = Get_Value("VA009_BPMandate_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Batch Lines.
@param VA009_BatchLines_ID Batch Lines */
        public void SetVA009_BatchLines_ID(int VA009_BatchLines_ID) { if (VA009_BatchLines_ID < 1) throw new ArgumentException("VA009_BatchLines_ID is mandatory."); Set_ValueNoCheck("VA009_BatchLines_ID", VA009_BatchLines_ID); }/** Get Batch Lines.
@return Batch Lines */
        public int GetVA009_BatchLines_ID() { Object ii = Get_Value("VA009_BatchLines_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Batch.
@param VA009_Batch_ID Batch */
        public void SetVA009_Batch_ID(int VA009_Batch_ID) { if (VA009_Batch_ID < 1) throw new ArgumentException("VA009_Batch_ID is mandatory."); Set_ValueNoCheck("VA009_Batch_ID", VA009_Batch_ID); }/** Get Batch.
@return Batch */
        public int GetVA009_Batch_ID() { Object ii = Get_Value("VA009_Batch_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Consolidate Payment.
@param VA009_Consolidate Consolidate Payment */
        public void SetVA009_Consolidate(Boolean VA009_Consolidate) { Set_Value("VA009_Consolidate", VA009_Consolidate); }/** Get Consolidate Payment.
@return Consolidate Payment */
        public Boolean IsVA009_Consolidate() { Object oo = Get_Value("VA009_Consolidate"); if (oo != null) { if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo); return "Y".Equals(oo); } return false; }/** Set Description.
@param VA009_Description Description */
        public void SetVA009_Description(String VA009_Description) { if (VA009_Description != null && VA009_Description.Length > 50) { log.Warning("Length > 50 - truncated"); VA009_Description = VA009_Description.Substring(0, 50); } Set_Value("VA009_Description", VA009_Description); }/** Get Description.
@return Description */
        public String GetVA009_Description() { return (String)Get_Value("VA009_Description"); }/** Set Due Amount.
@param VA009_DueAmount Due Amount */
        public void SetVA009_DueAmount(Decimal? VA009_DueAmount) { Set_Value("VA009_DueAmount", (Decimal?)VA009_DueAmount); }/** Get Due Amount.
@return Due Amount */
        public Decimal GetVA009_DueAmount() { Object bd = Get_Value("VA009_DueAmount"); if (bd == null) return Env.ZERO; return Convert.ToDecimal(bd); }
        /** VA009_PaymentLocation_ID AD_Reference_ID=159 */
        public static int VA009_PAYMENTLOCATION_ID_AD_Reference_ID = 159;/** Set Payment Location.
@param VA009_PaymentLocation_ID This field is used to show locations of Business Partner where Remit To Address is checked (true) */
        public void SetVA009_PaymentLocation_ID(int VA009_PaymentLocation_ID)
        {
            if (VA009_PaymentLocation_ID <= 0) Set_Value("VA009_PaymentLocation_ID", null);
            else
                Set_Value("VA009_PaymentLocation_ID", VA009_PaymentLocation_ID);
        }/** Get Payment Location.
@return This field is used to show locations of Business Partner where Remit To Address is checked (true) */
        public int GetVA009_PaymentLocation_ID() { Object ii = Get_Value("VA009_PaymentLocation_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }
        /** VA009_ReceiptLocation_ID AD_Reference_ID=159 */
        public static int VA009_RECEIPTLOCATION_ID_AD_Reference_ID = 159;/** Set Receipt Location.
@param VA009_ReceiptLocation_ID This field is used to show locations of Business Partner where Pay From Address is checked (true) */
        public void SetVA009_ReceiptLocation_ID(int VA009_ReceiptLocation_ID)
        {
            if (VA009_ReceiptLocation_ID <= 0) Set_Value("VA009_ReceiptLocation_ID", null);
            else
                Set_Value("VA009_ReceiptLocation_ID", VA009_ReceiptLocation_ID);
        }/** Get Receipt Location.
@return This field is used to show locations of Business Partner where Pay From Address is checked (true) */
        public int GetVA009_ReceiptLocation_ID() { Object ii = Get_Value("VA009_ReceiptLocation_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }
    }
}