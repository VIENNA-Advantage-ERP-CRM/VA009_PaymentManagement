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
    using System.Data;/** Generated Model for VA009_T_ChequePrintPreview
 *  @author Raghu (Updated) 
 *  @version Vienna Framework 1.1.1 - $Id$ */
    public class X_VA009_T_ChequePrintPreview : PO
    {
        public X_VA009_T_ChequePrintPreview(Context ctx, int VA009_T_ChequePrintPreview_ID, Trx trxName) : base(ctx, VA009_T_ChequePrintPreview_ID, trxName)
        {/** if (VA009_T_ChequePrintPreview_ID == 0){SetVA009_T_ChequePrintPreview_ID (0);} */
        }
        public X_VA009_T_ChequePrintPreview(Ctx ctx, int VA009_T_ChequePrintPreview_ID, Trx trxName) : base(ctx, VA009_T_ChequePrintPreview_ID, trxName)
        {/** if (VA009_T_ChequePrintPreview_ID == 0){SetVA009_T_ChequePrintPreview_ID (0);} */
        }/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
        public X_VA009_T_ChequePrintPreview(Context ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName) { }/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
        public X_VA009_T_ChequePrintPreview(Ctx ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName) { }/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
        public X_VA009_T_ChequePrintPreview(Ctx ctx, IDataReader dr, Trx trxName) : base(ctx, dr, trxName) { }/** Static Constructor 
 Set Table ID By Table Name
 added by ->Harwinder */
        static X_VA009_T_ChequePrintPreview() { Table_ID = Get_Table_ID(Table_Name); model = new KeyNamePair(Table_ID, Table_Name); }/** Serial Version No */
        static long serialVersionUID = 27956660809732L;/** Last Updated Timestamp 1/24/2023 4:44:52 AM */
        public static long updatedMS = 1674535492943L;/** AD_Table_ID=1002043 */
        public static int Table_ID; // =1002043;
        /** TableName=VA009_T_ChequePrintPreview */
        public static String Table_Name = "VA009_T_ChequePrintPreview";
        protected static KeyNamePair model; protected Decimal accessLevel = new Decimal(3);/** AccessLevel
@return 3 - Client - Org 
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
        public override String ToString() { StringBuilder sb = new StringBuilder("X_VA009_T_ChequePrintPreview[").Append(Get_ID()).Append("]"); return sb.ToString(); }/** Set Business Partner.
@param C_BPartner_ID Identifies a Customer/Prospect */
        public void SetC_BPartner_ID(int C_BPartner_ID)
        {
            if (C_BPartner_ID <= 0) Set_Value("C_BPartner_ID", null);
            else
                Set_Value("C_BPartner_ID", C_BPartner_ID);
        }/** Get Business Partner.
@return Identifies a Customer/Prospect */
        public int GetC_BPartner_ID() { Object ii = Get_Value("C_BPartner_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Location.
@param C_BPartner_Location_ID Identifies the address for this Account/Prospect. */
        public void SetC_BPartner_Location_ID(int C_BPartner_Location_ID)
        {
            if (C_BPartner_Location_ID <= 0) Set_Value("C_BPartner_Location_ID", null);
            else
                Set_Value("C_BPartner_Location_ID", C_BPartner_Location_ID);
        }/** Get Location.
@return Identifies the address for this Account/Prospect. */
        public int GetC_BPartner_Location_ID() { Object ii = Get_Value("C_BPartner_Location_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Bank Account.
@param C_BankAccount_ID Account at the Bank */
        public void SetC_BankAccount_ID(int C_BankAccount_ID)
        {
            if (C_BankAccount_ID <= 0) Set_Value("C_BankAccount_ID", null);
            else
                Set_Value("C_BankAccount_ID", C_BankAccount_ID);
        }/** Get Bank Account.
@return Account at the Bank */
        public int GetC_BankAccount_ID() { Object ii = Get_Value("C_BankAccount_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Bank.
@param C_Bank_ID Bank */
        public void SetC_Bank_ID(int C_Bank_ID)
        {
            if (C_Bank_ID <= 0) Set_Value("C_Bank_ID", null);
            else
                Set_Value("C_Bank_ID", C_Bank_ID);
        }/** Get Bank.
@return Bank */
        public int GetC_Bank_ID() { Object ii = Get_Value("C_Bank_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Currency.
@param C_Currency_ID The Currency for this record */
        public void SetC_Currency_ID(int C_Currency_ID)
        {
            if (C_Currency_ID <= 0) Set_Value("C_Currency_ID", null);
            else
                Set_Value("C_Currency_ID", C_Currency_ID);
        }/** Get Currency.
@return The Currency for this record */
        public int GetC_Currency_ID() { Object ii = Get_Value("C_Currency_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Invoice Schedule.
@param C_InvoiceSchedule_ID Schedule for generating Invoices */
        public void SetC_InvoiceSchedule_ID(int C_InvoiceSchedule_ID)
        {
            if (C_InvoiceSchedule_ID <= 0) Set_Value("C_InvoiceSchedule_ID", null);
            else
                Set_Value("C_InvoiceSchedule_ID", C_InvoiceSchedule_ID);
        }/** Get Invoice Schedule.
@return Schedule for generating Invoices */
        public int GetC_InvoiceSchedule_ID() { Object ii = Get_Value("C_InvoiceSchedule_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Invoice.
@param C_Invoice_ID Invoice Identifier */
        public void SetC_Invoice_ID(int C_Invoice_ID)
        {
            if (C_Invoice_ID <= 0) Set_Value("C_Invoice_ID", null);
            else
                Set_Value("C_Invoice_ID", C_Invoice_ID);
        }/** Get Invoice.
@return Invoice Identifier */
        public int GetC_Invoice_ID() { Object ii = Get_Value("C_Invoice_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }/** Set Export.
@param Export_ID Export */
        public void SetExport_ID(String Export_ID) { if (Export_ID != null && Export_ID.Length > 50) { log.Warning("Length > 50 - truncated"); Export_ID = Export_ID.Substring(0, 50); } Set_Value("Export_ID", Export_ID); }/** Get Export.
@return Export */
        public String GetExport_ID() { return (String)Get_Value("Export_ID"); }/** Set Cheque Amount.
@param VA009_ChequeAmount Cheque Amount */
        public void SetVA009_ChequeAmount(Decimal? VA009_ChequeAmount) { Set_Value("VA009_ChequeAmount", (Decimal?)VA009_ChequeAmount); }/** Get Cheque Amount.
@return Cheque Amount */
        public Decimal GetVA009_ChequeAmount() { Object bd = Get_Value("VA009_ChequeAmount"); if (bd == null) return Env.ZERO; return Convert.ToDecimal(bd); }/** Set Cheque Date.
@param VA009_ChequeDate Cheque Date */
        public void SetVA009_ChequeDate(DateTime? VA009_ChequeDate) { Set_Value("VA009_ChequeDate", (DateTime?)VA009_ChequeDate); }/** Get Cheque Date.
@return Cheque Date */
        public DateTime? GetVA009_ChequeDate() { return (DateTime?)Get_Value("VA009_ChequeDate"); }/** Set Cheque Number.
@param VA009_ChequeNumber Cheque Number */
        public void SetVA009_ChequeNumber(Decimal? VA009_ChequeNumber) { Set_Value("VA009_ChequeNumber", (Decimal?)VA009_ChequeNumber); }/** Get Cheque Number.
@return Cheque Number */
        public Decimal GetVA009_ChequeNumber() { Object bd = Get_Value("VA009_ChequeNumber"); if (bd == null) return Env.ZERO; return Convert.ToDecimal(bd); }/** Set Due Amount.
@param VA009_DueAmount Due Amount */
        public void SetVA009_DueAmount(Decimal? VA009_DueAmount) { Set_Value("VA009_DueAmount", (Decimal?)VA009_DueAmount); }/** Get Due Amount.
@return Due Amount */
        public Decimal GetVA009_DueAmount() { Object bd = Get_Value("VA009_DueAmount"); if (bd == null) return Env.ZERO; return Convert.ToDecimal(bd); }/** Set VA009_T_ChequePrintPreview_ID.
@param VA009_T_ChequePrintPreview_ID VA009_T_ChequePrintPreview_ID */
        public void SetVA009_T_ChequePrintPreview_ID(int VA009_T_ChequePrintPreview_ID) { if (VA009_T_ChequePrintPreview_ID < 1) throw new ArgumentException("VA009_T_ChequePrintPreview_ID is mandatory."); Set_ValueNoCheck("VA009_T_ChequePrintPreview_ID", VA009_T_ChequePrintPreview_ID); }/** Get VA009_T_ChequePrintPreview_ID.
@return VA009_T_ChequePrintPreview_ID */
        public int GetVA009_T_ChequePrintPreview_ID() { Object ii = Get_Value("VA009_T_ChequePrintPreview_ID"); if (ii == null) return 0; return Convert.ToInt32(ii); }
    }
}