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
/** Generated Model for VA009_BatchLineDetails
 *  @author Jagmohan Bhatt (generated) 
 *  @version Vienna Framework 1.1.1 - $Id$ */
public class X_VA009_BatchLineDetails : PO
{
public X_VA009_BatchLineDetails (Context ctx, int VA009_BatchLineDetails_ID, Trx trxName) : base (ctx, VA009_BatchLineDetails_ID, trxName)
{
/** if (VA009_BatchLineDetails_ID == 0)
{
SetVA009_BatchLineDetails_ID (0);
SetVA009_BatchLines_ID (0);
}
 */
}
public X_VA009_BatchLineDetails(Ctx ctx, int VA009_BatchLineDetails_ID, Trx trxName)
    : base(ctx, VA009_BatchLineDetails_ID, trxName)
{
/** if (VA009_BatchLineDetails_ID == 0)
{
SetVA009_BatchLineDetails_ID (0);
SetVA009_BatchLines_ID (0);
}
 */
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_VA009_BatchLineDetails(Context ctx, DataRow rs, Trx trxName)
    : base(ctx, rs, trxName)
{
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_VA009_BatchLineDetails(Ctx ctx, DataRow rs, Trx trxName)
    : base(ctx, rs, trxName)
{
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_VA009_BatchLineDetails(Ctx ctx, IDataReader dr, Trx trxName)
    : base(ctx, dr, trxName)
{
}
/** Static Constructor 
 Set Table ID By Table Name
 added by ->Harwinder */
static X_VA009_BatchLineDetails()
{
 Table_ID = Get_Table_ID(Table_Name);
 model = new KeyNamePair(Table_ID,Table_Name);
}
/** Serial Version No */
static long serialVersionUID = 27732557419698L;
/** Last Updated Timestamp 12/18/2015 3:18:23 PM */
public static long updatedMS = 1450432102909L;
/** AD_Table_ID=1001016 */
public static int Table_ID;
 // =1001016;

/** TableName=VA009_BatchLineDetails */
public static String Table_Name="VA009_BatchLineDetails";

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
protected override POInfo InitPO (Context ctx)
{
POInfo poi = POInfo.GetPOInfo (ctx, Table_ID);
return poi;
}
/** Load Meta Data
@param ctx context
@return PO Info
*/
protected override POInfo InitPO (Ctx ctx)
{
POInfo poi = POInfo.GetPOInfo (ctx, Table_ID);
return poi;
}
/** Info
@return info
*/
public override String ToString()
{
StringBuilder sb = new StringBuilder ("X_VA009_BatchLineDetails[").Append(Get_ID()).Append("]");
return sb.ToString();
}
/** Set Allocation.
@param C_AllocationHdr_ID Payment allocation */
public void SetC_AllocationHdr_ID (int C_AllocationHdr_ID)
{
if (C_AllocationHdr_ID <= 0) Set_Value ("C_AllocationHdr_ID", null);
else
Set_Value ("C_AllocationHdr_ID", C_AllocationHdr_ID);
}
/** Get Allocation.
@return Payment allocation */
public int GetC_AllocationHdr_ID() 
{
Object ii = Get_Value("C_AllocationHdr_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Cash Journal Line.
@param C_CashLine_ID Cash Journal Line */
public void SetC_CashLine_ID (int C_CashLine_ID)
{
if (C_CashLine_ID <= 0) Set_Value ("C_CashLine_ID", null);
else
Set_Value ("C_CashLine_ID", C_CashLine_ID);
}
/** Get Cash Journal Line.
@return Cash Journal Line */
public int GetC_CashLine_ID() 
{
Object ii = Get_Value("C_CashLine_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Cash Journal.
@param C_Cash_ID Cash Journal */
public void SetC_Cash_ID (int C_Cash_ID)
{
if (C_Cash_ID <= 0) Set_Value ("C_Cash_ID", null);
else
Set_Value ("C_Cash_ID", C_Cash_ID);
}
/** Get Cash Journal.
@return Cash Journal */
public int GetC_Cash_ID() 
{
Object ii = Get_Value("C_Cash_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Currency.
@param C_Currency_ID The Currency for this record */
public void SetC_Currency_ID (int C_Currency_ID)
{
if (C_Currency_ID <= 0) Set_Value ("C_Currency_ID", null);
else
Set_Value ("C_Currency_ID", C_Currency_ID);
}
/** Get Currency.
@return The Currency for this record */
public int GetC_Currency_ID() 
{
Object ii = Get_Value("C_Currency_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Invoice Payment Schedule.
@param C_InvoicePaySchedule_ID Invoice Payment Schedule */
public void SetC_InvoicePaySchedule_ID (int C_InvoicePaySchedule_ID)
{
if (C_InvoicePaySchedule_ID <= 0) Set_Value ("C_InvoicePaySchedule_ID", null);
else
Set_Value ("C_InvoicePaySchedule_ID", C_InvoicePaySchedule_ID);
}
/** Get Invoice Payment Schedule.
@return Invoice Payment Schedule */
public int GetC_InvoicePaySchedule_ID() 
{
Object ii = Get_Value("C_InvoicePaySchedule_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Invoice.
@param C_Invoice_ID Invoice Identifier */
public void SetC_Invoice_ID (int C_Invoice_ID)
{
if (C_Invoice_ID <= 0) Set_Value ("C_Invoice_ID", null);
else
Set_Value ("C_Invoice_ID", C_Invoice_ID);
}
/** Get Invoice.
@return Invoice Identifier */
public int GetC_Invoice_ID() 
{
Object ii = Get_Value("C_Invoice_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Payment.
@param C_Payment_ID Payment identifier */
public void SetC_Payment_ID (int C_Payment_ID)
{
if (C_Payment_ID <= 0) Set_Value ("C_Payment_ID", null);
else
Set_Value ("C_Payment_ID", C_Payment_ID);
}
/** Get Payment.
@return Payment identifier */
public int GetC_Payment_ID() 
{
Object ii = Get_Value("C_Payment_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Discount Amount.
@param DiscountAmt Calculated amount of discount */
public void SetDiscountAmt (Decimal? DiscountAmt)
{
Set_Value ("DiscountAmt", (Decimal?)DiscountAmt);
}
/** Get Discount Amount.
@return Calculated amount of discount */
public Decimal GetDiscountAmt() 
{
Object bd =Get_Value("DiscountAmt");
if (bd == null) return Env.ZERO;
return  Convert.ToDecimal(bd);
}
/** Set Discount Date.
@param DiscountDate Last Date for payments with discount */
public void SetDiscountDate (DateTime? DiscountDate)
{
Set_Value ("DiscountDate", (DateTime?)DiscountDate);
}
/** Get Discount Date.
@return Last Date for payments with discount */
public DateTime? GetDiscountDate() 
{
return (DateTime?)Get_Value("DiscountDate");
}
/** Set Amount due.
@param DueAmt Amount of the payment due */
public void SetDueAmt (Decimal? DueAmt)
{
Set_Value ("DueAmt", (Decimal?)DueAmt);
}
/** Get Amount due.
@return Amount of the payment due */
public Decimal GetDueAmt() 
{
Object bd =Get_Value("DueAmt");
if (bd == null) return Env.ZERO;
return  Convert.ToDecimal(bd);
}
/** Set Due Date.
@param DueDate Date when the payment is due */
public void SetDueDate (DateTime? DueDate)
{
Set_Value ("DueDate", (DateTime?)DueDate);
}
/** Get Due Date.
@return Date when the payment is due */
public DateTime? GetDueDate() 
{
return (DateTime?)Get_Value("DueDate");
}
/** Set Export.
@param Export_ID Export */
public void SetExport_ID (String Export_ID)
{
if (Export_ID != null && Export_ID.Length > 50)
{
log.Warning("Length > 50 - truncated");
Export_ID = Export_ID.Substring(0,50);
}
Set_Value ("Export_ID", Export_ID);
}
/** Get Export.
@return Export */
public String GetExport_ID() 
{
return (String)Get_Value("Export_ID");
}
/** Set Processed.
@param Processed The document has been processed */
public void SetProcessed (Boolean Processed)
{
Set_Value ("Processed", Processed);
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

/** VA009_BankResponse AD_Reference_ID=1000414 */
public static int VA009_BANKRESPONSE_AD_Reference_ID=1000414;
/** In Progress = IP */
public static String VA009_BANKRESPONSE_InProgress = "IP";
/** Success = RE */
public static String VA009_BANKRESPONSE_Success = "RE";
/** Rejected = RJ */
public static String VA009_BANKRESPONSE_Rejected = "RJ";
/** Is test a valid value.
@param test testvalue
@returns true if valid **/
public bool IsVA009_BankResponseValid (String test)
{
return test == null || test.Equals("IP") || test.Equals("RE") || test.Equals("RJ");
}
/** Set Bank Response.
@param VA009_BankResponse Bank Response */
public void SetVA009_BankResponse (String VA009_BankResponse)
{
if (!IsVA009_BankResponseValid(VA009_BankResponse))
throw new ArgumentException ("VA009_BankResponse Invalid value - " + VA009_BankResponse + " - Reference_ID=1000414 - IP - RE - RJ");
if (VA009_BankResponse != null && VA009_BankResponse.Length > 2)
{
log.Warning("Length > 2 - truncated");
VA009_BankResponse = VA009_BankResponse.Substring(0,2);
}
Set_Value ("VA009_BankResponse", VA009_BankResponse);
}
/** Get Bank Response.
@return Bank Response */
public String GetVA009_BankResponse() 
{
return (String)Get_Value("VA009_BankResponse");
}
/** Set Batch Line Details.
@param VA009_BatchLineDetails_ID Batch Line Details */
public void SetVA009_BatchLineDetails_ID (int VA009_BatchLineDetails_ID)
{
if (VA009_BatchLineDetails_ID < 1) throw new ArgumentException ("VA009_BatchLineDetails_ID is mandatory.");
Set_ValueNoCheck ("VA009_BatchLineDetails_ID", VA009_BatchLineDetails_ID);
}
/** Get Batch Line Details.
@return Batch Line Details */
public int GetVA009_BatchLineDetails_ID() 
{
Object ii = Get_Value("VA009_BatchLineDetails_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Batch Lines.
@param VA009_BatchLines_ID Batch Lines */
public void SetVA009_BatchLines_ID (int VA009_BatchLines_ID)
{
if (VA009_BatchLines_ID < 1) throw new ArgumentException ("VA009_BatchLines_ID is mandatory.");
Set_ValueNoCheck ("VA009_BatchLines_ID", VA009_BatchLines_ID);
}
/** Get Batch Lines.
@return Batch Lines */
public int GetVA009_BatchLines_ID() 
{
Object ii = Get_Value("VA009_BatchLines_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set VA009_ConvertedAmt.
@param VA009_ConvertedAmt VA009_ConvertedAmt */
public void SetVA009_ConvertedAmt (Decimal? VA009_ConvertedAmt)
{
Set_Value ("VA009_ConvertedAmt", (Decimal?)VA009_ConvertedAmt);
}
/** Get VA009_ConvertedAmt.
@return VA009_ConvertedAmt */
public Decimal GetVA009_ConvertedAmt() 
{
Object bd =Get_Value("VA009_ConvertedAmt");
if (bd == null) return Env.ZERO;
return  Convert.ToDecimal(bd);
}
/** Set Description.
@param VA009_Description Description */
public void SetVA009_Description (String VA009_Description)
{
if (VA009_Description != null && VA009_Description.Length > 50)
{
log.Warning("Length > 50 - truncated");
VA009_Description = VA009_Description.Substring(0,50);
}
Set_Value ("VA009_Description", VA009_Description);
}
/** Get Description.
@return Description */
public String GetVA009_Description() 
{
return (String)Get_Value("VA009_Description");
}
/** Set Payment Method.
@param VA009_PaymentMethod_ID Payment Method */
public void SetVA009_PaymentMethod_ID (int VA009_PaymentMethod_ID)
{
if (VA009_PaymentMethod_ID <= 0) Set_Value ("VA009_PaymentMethod_ID", null);
else
Set_Value ("VA009_PaymentMethod_ID", VA009_PaymentMethod_ID);
}
/** Get Payment Method.
@return Payment Method */
public int GetVA009_PaymentMethod_ID() 
{
Object ii = Get_Value("VA009_PaymentMethod_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set VA009_TrxID.
@param VA009_TrxID VA009_TrxID */
public void SetVA009_TrxID (String VA009_TrxID)
{
if (VA009_TrxID != null && VA009_TrxID.Length > 14)
{
log.Warning("Length > 14 - truncated");
VA009_TrxID = VA009_TrxID.Substring(0,14);
}
Set_Value ("VA009_TrxID", VA009_TrxID);
}
/** Get VA009_TrxID.
@return VA009_TrxID */
public String GetVA009_TrxID() 
{
return (String)Get_Value("VA009_TrxID");
}
/** Set ConversionType.
@param C_ConversionType_ID ConversionType Identifier */
public void SetC_ConversionType_ID(int C_ConversionType_ID)
{
    if (C_ConversionType_ID <= 0) Set_Value("C_ConversionType_ID", null);
    else
        Set_Value("C_ConversionType_ID", C_ConversionType_ID);
}
/** Get ConversionType.
@return ConversionType Identifier */
public int GetC_ConversionType_ID()
{
    Object ii = Get_Value("C_ConversionType_ID");
    if (ii == null) return 0;
    return Convert.ToInt32(ii);
}
}

}
