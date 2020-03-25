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
/** Generated Model for VA009_PaymentMethod
 *  @author Jagmohan Bhatt (generated) 
 *  @version Vienna Framework 1.1.1 - $Id$ */
public class X_VA009_PaymentMethod : PO
{
public X_VA009_PaymentMethod (Context ctx, int VA009_PaymentMethod_ID, Trx trxName) : base (ctx, VA009_PaymentMethod_ID, trxName)
{
/** if (VA009_PaymentMethod_ID == 0)
{
SetVA009_PaymentBaseType (null);	// S
SetVA009_PaymentMethod_ID (0);
}
 */
}
public X_VA009_PaymentMethod(Ctx ctx, int VA009_PaymentMethod_ID, Trx trxName)
    : base(ctx, VA009_PaymentMethod_ID, trxName)
{
/** if (VA009_PaymentMethod_ID == 0)
{
SetVA009_PaymentBaseType (null);	// S
SetVA009_PaymentMethod_ID (0);
}
 */
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_VA009_PaymentMethod(Context ctx, DataRow rs, Trx trxName)
    : base(ctx, rs, trxName)
{
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_VA009_PaymentMethod(Ctx ctx, DataRow rs, Trx trxName)
    : base(ctx, rs, trxName)
{
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_VA009_PaymentMethod(Ctx ctx, IDataReader dr, Trx trxName)
    : base(ctx, dr, trxName)
{
}
/** Static Constructor 
 Set Table ID By Table Name
 added by ->Harwinder */
static X_VA009_PaymentMethod()
{
 Table_ID = Get_Table_ID(Table_Name);
 model = new KeyNamePair(Table_ID,Table_Name);
}
/** Serial Version No */
static long serialVersionUID = 27727470258703L;
/** Last Updated Timestamp 10/20/2015 6:12:22 PM */
public static long updatedMS = 1445344941914L;
/** AD_Table_ID=1001011 */
public static int Table_ID;
 // =1001011;

/** TableName=VA009_PaymentMethod */
public static String Table_Name="VA009_PaymentMethod";

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
StringBuilder sb = new StringBuilder ("X_VA009_PaymentMethod[").Append(Get_ID()).Append("]");
return sb.ToString();
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
/** Set File Generation Class.
@param VA009_FileClass File Generation Class */
public void SetVA009_FileClass (String VA009_FileClass)
{
if (VA009_FileClass != null && VA009_FileClass.Length > 100)
{
log.Warning("Length > 100 - truncated");
VA009_FileClass = VA009_FileClass.Substring(0,100);
}
Set_Value ("VA009_FileClass", VA009_FileClass);
}
/** Get File Generation Class.
@return File Generation Class */
public String GetVA009_FileClass() 
{
return (String)Get_Value("VA009_FileClass");
}
/** Set VA009_InitiatePay.
@param VA009_InitiatePay VA009_InitiatePay */
public void SetVA009_InitiatePay (Boolean VA009_InitiatePay)
{
Set_Value ("VA009_InitiatePay", VA009_InitiatePay);
}
/** Get VA009_InitiatePay.
@return VA009_InitiatePay */
public Boolean IsVA009_InitiatePay() 
{
Object oo = Get_Value("VA009_InitiatePay");
if (oo != null) 
{
 if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo);
 return "Y".Equals(oo);
}
return false;
}
/** Set Mandate Required.
@param VA009_IsMandate Mandate Required */
public void SetVA009_IsMandate (Boolean VA009_IsMandate)
{
Set_Value ("VA009_IsMandate", VA009_IsMandate);
}
/** Get Mandate Required.
@return Mandate Required */
public Boolean IsVA009_IsMandate() 
{
Object oo = Get_Value("VA009_IsMandate");
if (oo != null) 
{
 if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo);
 return "Y".Equals(oo);
}
return false;
}
/** Set Name.
@param VA009_Name Name */
public void SetVA009_Name (String VA009_Name)
{
if (VA009_Name != null && VA009_Name.Length > 100)
{
log.Warning("Length > 100 - truncated");
VA009_Name = VA009_Name.Substring(0,100);
}
Set_Value ("VA009_Name", VA009_Name);
}
/** Get Name.
@return Name */
public String GetVA009_Name() 
{
return (String)Get_Value("VA009_Name");
}

/** VA009_PaymentBaseType AD_Reference_ID=1000412 */
public static int VA009_PAYMENTBASETYPE_AD_Reference_ID=1000412;
/** Cash = B */
public static String VA009_PAYMENTBASETYPE_Cash = "B";
/** Direct Debit = D */
public static String VA009_PAYMENTBASETYPE_DirectDebit = "D";
/** Credit Card = K */
public static String VA009_PAYMENTBASETYPE_CreditCard = "K";
/** Check = S */
public static String VA009_PAYMENTBASETYPE_Check = "S";
/** Direct Deposit = T */
public static String VA009_PAYMENTBASETYPE_DirectDeposit = "T";
/** Wire Transfer = W */
public static String VA009_PAYMENTBASETYPE_WireTransfer = "W";
/** Is test a valid value.
@param test testvalue
@returns true if valid **/
public bool IsVA009_PaymentBaseTypeValid (String test)
{
return test.Equals("B") || test.Equals("D") || test.Equals("K") || test.Equals("S") || test.Equals("T") || test.Equals("W");
}
/** Set VA009_PaymentBaseType.
@param VA009_PaymentBaseType VA009_PaymentBaseType */
public void SetVA009_PaymentBaseType (String VA009_PaymentBaseType)
{
if (VA009_PaymentBaseType == null) throw new ArgumentException ("VA009_PaymentBaseType is mandatory");
if (!IsVA009_PaymentBaseTypeValid(VA009_PaymentBaseType))
throw new ArgumentException ("VA009_PaymentBaseType Invalid value - " + VA009_PaymentBaseType + " - Reference_ID=1000412 - B - D - K - S - T - W");
if (VA009_PaymentBaseType.Length > 1)
{
log.Warning("Length > 1 - truncated");
VA009_PaymentBaseType = VA009_PaymentBaseType.Substring(0,1);
}
Set_Value ("VA009_PaymentBaseType", VA009_PaymentBaseType);
}
/** Get VA009_PaymentBaseType.
@return VA009_PaymentBaseType */
public String GetVA009_PaymentBaseType() 
{
return (String)Get_Value("VA009_PaymentBaseType");
}
/** Set Payment Method.
@param VA009_PaymentMethod_ID Payment Method */
public void SetVA009_PaymentMethod_ID (int VA009_PaymentMethod_ID)
{
if (VA009_PaymentMethod_ID < 1) throw new ArgumentException ("VA009_PaymentMethod_ID is mandatory.");
Set_ValueNoCheck ("VA009_PaymentMethod_ID", VA009_PaymentMethod_ID);
}
/** Get Payment Method.
@return Payment Method */
public int GetVA009_PaymentMethod_ID() 
{
Object ii = Get_Value("VA009_PaymentMethod_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}

/** VA009_PaymentMode AD_Reference_ID=1000400 */
public static int VA009_PAYMENTMODE_AD_Reference_ID=1000400;
/** Bank = B */
public static String VA009_PAYMENTMODE_Bank = "B";
/** Cash = C */
public static String VA009_PAYMENTMODE_Cash = "C";
/** Is test a valid value.
@param test testvalue
@returns true if valid **/
public bool IsVA009_PaymentModeValid (String test)
{
return test == null || test.Equals("B") || test.Equals("C");
}
/** Set Payment Mode.
@param VA009_PaymentMode Payment Mode */
public void SetVA009_PaymentMode (String VA009_PaymentMode)
{
if (!IsVA009_PaymentModeValid(VA009_PaymentMode))
throw new ArgumentException ("VA009_PaymentMode Invalid value - " + VA009_PaymentMode + " - Reference_ID=1000400 - B - C");
if (VA009_PaymentMode != null && VA009_PaymentMode.Length > 1)
{
log.Warning("Length > 1 - truncated");
VA009_PaymentMode = VA009_PaymentMode.Substring(0,1);
}
Set_Value ("VA009_PaymentMode", VA009_PaymentMode);
}
/** Get Payment Mode.
@return Payment Mode */
public String GetVA009_PaymentMode() 
{
return (String)Get_Value("VA009_PaymentMode");
}

/** VA009_PaymentRule AD_Reference_ID=1000413 */
public static int VA009_PAYMENTRULE_AD_Reference_ID=1000413;
/** EFT = E */
public static String VA009_PAYMENTRULE_EFT = "E";
/** Manual = M */
public static String VA009_PAYMENTRULE_Manual = "M";
/** Is test a valid value.
@param test testvalue
@returns true if valid **/
public bool IsVA009_PaymentRuleValid (String test)
{
return test == null || test.Equals("E") || test.Equals("M");
}
/** Set Payment Rule.
@param VA009_PaymentRule Payment Rule */
public void SetVA009_PaymentRule (String VA009_PaymentRule)
{
if (!IsVA009_PaymentRuleValid(VA009_PaymentRule))
throw new ArgumentException ("VA009_PaymentRule Invalid value - " + VA009_PaymentRule + " - Reference_ID=1000413 - E - M");
if (VA009_PaymentRule != null && VA009_PaymentRule.Length > 1)
{
log.Warning("Length > 1 - truncated");
VA009_PaymentRule = VA009_PaymentRule.Substring(0,1);
}
Set_Value ("VA009_PaymentRule", VA009_PaymentRule);
}
/** Get Payment Rule.
@return Payment Rule */
public String GetVA009_PaymentRule() 
{
return (String)Get_Value("VA009_PaymentRule");
}

/** VA009_PaymentTrigger AD_Reference_ID=1000401 */
public static int VA009_PAYMENTTRIGGER_AD_Reference_ID=1000401;
/** Pull By Recipient = R */
public static String VA009_PAYMENTTRIGGER_PullByRecipient = "R";
/** Push By Sender = S */
public static String VA009_PAYMENTTRIGGER_PushBySender = "S";
/** Is test a valid value.
@param test testvalue
@returns true if valid **/
public bool IsVA009_PaymentTriggerValid (String test)
{
return test == null || test.Equals("R") || test.Equals("S");
}
/** Set Payment Trigger By.
@param VA009_PaymentTrigger Payment Trigger By */
public void SetVA009_PaymentTrigger (String VA009_PaymentTrigger)
{
if (!IsVA009_PaymentTriggerValid(VA009_PaymentTrigger))
throw new ArgumentException ("VA009_PaymentTrigger Invalid value - " + VA009_PaymentTrigger + " - Reference_ID=1000401 - R - S");
if (VA009_PaymentTrigger != null && VA009_PaymentTrigger.Length > 1)
{
log.Warning("Length > 1 - truncated");
VA009_PaymentTrigger = VA009_PaymentTrigger.Substring(0,1);
}
Set_Value ("VA009_PaymentTrigger", VA009_PaymentTrigger);
}
/** Get Payment Trigger By.
@return Payment Trigger By */
public String GetVA009_PaymentTrigger() 
{
return (String)Get_Value("VA009_PaymentTrigger");
}

/** VA009_PaymentType AD_Reference_ID=1000402 */
public static int VA009_PAYMENTTYPE_AD_Reference_ID=1000402;
/** Batch = B */
public static String VA009_PAYMENTTYPE_Batch = "B";
/** Single = S */
public static String VA009_PAYMENTTYPE_Single = "S";
/** Is test a valid value.
@param test testvalue
@returns true if valid **/
public bool IsVA009_PaymentTypeValid (String test)
{
return test == null || test.Equals("B") || test.Equals("S");
}
/** Set Payment Type.
@param VA009_PaymentType Payment Type */
public void SetVA009_PaymentType (String VA009_PaymentType)
{
if (!IsVA009_PaymentTypeValid(VA009_PaymentType))
throw new ArgumentException ("VA009_PaymentType Invalid value - " + VA009_PaymentType + " - Reference_ID=1000402 - B - S");
if (VA009_PaymentType != null && VA009_PaymentType.Length > 1)
{
log.Warning("Length > 1 - truncated");
VA009_PaymentType = VA009_PaymentType.Substring(0,1);
}
Set_Value ("VA009_PaymentType", VA009_PaymentType);
}
/** Get Payment Type.
@return Payment Type */
public String GetVA009_PaymentType() 
{
return (String)Get_Value("VA009_PaymentType");
}
/** Set Search Key.
@param Value Search key for the record in the format required - must be unique */
public void SetValue (String Value)
{
if (Value != null && Value.Length > 100)
{
log.Warning("Length > 100 - truncated");
Value = Value.Substring(0,100);
}
Set_Value ("Value", Value);
}
/** Get Search Key.
@return Search key for the record in the format required - must be unique */
public String GetValue() 
{
return (String)Get_Value("Value");
}
}

}
