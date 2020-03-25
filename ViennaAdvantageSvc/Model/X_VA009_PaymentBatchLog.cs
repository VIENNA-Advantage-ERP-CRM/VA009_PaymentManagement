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
/** Generated Model for VA009_PaymentBatchLog
 *  @author Jagmohan Bhatt (generated) 
 *  @version Vienna Framework 1.1.1 - $Id$ */
public class X_VA009_PaymentBatchLog : PO
{
public X_VA009_PaymentBatchLog (Context ctx, int VA009_PaymentBatchLog_ID, Trx trxName) : base (ctx, VA009_PaymentBatchLog_ID, trxName)
{
/** if (VA009_PaymentBatchLog_ID == 0)
{
SetVA009_PaymentBatchLog_ID (0);
}
 */
}
public X_VA009_PaymentBatchLog(Ctx ctx, int VA009_PaymentBatchLog_ID, Trx trxName)
    : base(ctx, VA009_PaymentBatchLog_ID, trxName)
{
/** if (VA009_PaymentBatchLog_ID == 0)
{
SetVA009_PaymentBatchLog_ID (0);
}
 */
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_VA009_PaymentBatchLog(Context ctx, DataRow rs, Trx trxName)
    : base(ctx, rs, trxName)
{
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_VA009_PaymentBatchLog(Ctx ctx, DataRow rs, Trx trxName)
    : base(ctx, rs, trxName)
{
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_VA009_PaymentBatchLog(Ctx ctx, IDataReader dr, Trx trxName)
    : base(ctx, dr, trxName)
{
}
/** Static Constructor 
 Set Table ID By Table Name
 added by ->Harwinder */
static X_VA009_PaymentBatchLog()
{
 Table_ID = Get_Table_ID(Table_Name);
 model = new KeyNamePair(Table_ID,Table_Name);
}
/** Serial Version No */
static long serialVersionUID = 27728771423502L;
/** Last Updated Timestamp 11/4/2015 7:38:26 PM */
public static long updatedMS = 1446646106713L;
/** AD_Table_ID=1001047 */
public static int Table_ID;
 // =1001047;

/** TableName=VA009_PaymentBatchLog */
public static String Table_Name="VA009_PaymentBatchLog";

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
StringBuilder sb = new StringBuilder ("X_VA009_PaymentBatchLog[").Append(Get_ID()).Append("]");
return sb.ToString();
}
/** Set Description.
@param Description Optional short description of the record */
public void SetDescription (String Description)
{
if (Description != null && Description.Length > 255)
{
log.Warning("Length > 255 - truncated");
Description = Description.Substring(0,255);
}
Set_Value ("Description", Description);
}
/** Get Description.
@return Optional short description of the record */
public String GetDescription() 
{
return (String)Get_Value("Description");
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
/** Set Error.
@param IsError An Error occured in the execution */
public void SetIsError (Boolean IsError)
{
Set_Value ("IsError", IsError);
}
/** Get Error.
@return An Error occured in the execution */
public Boolean IsError() 
{
Object oo = Get_Value("IsError");
if (oo != null) 
{
 if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo);
 return "Y".Equals(oo);
}
return false;
}
/** Set Reference.
@param Reference Reference for this record */
public void SetReference (String Reference)
{
if (Reference != null && Reference.Length > 60)
{
log.Warning("Length > 60 - truncated");
Reference = Reference.Substring(0,60);
}
Set_Value ("Reference", Reference);
}
/** Get Reference.
@return Reference for this record */
public String GetReference() 
{
return (String)Get_Value("Reference");
}
/** Set Summary.
@param Summary Textual summary of this request */
public void SetSummary (String Summary)
{
if (Summary != null && Summary.Length > 2000)
{
log.Warning("Length > 2000 - truncated");
Summary = Summary.Substring(0,2000);
}
Set_Value ("Summary", Summary);
}
/** Get Summary.
@return Textual summary of this request */
public String GetSummary() 
{
return (String)Get_Value("Summary");
}
/** Set Text Message.
@param TextMsg Text Message */
public void SetTextMsg (String TextMsg)
{
if (TextMsg != null && TextMsg.Length > 2000)
{
log.Warning("Length > 2000 - truncated");
TextMsg = TextMsg.Substring(0,2000);
}
Set_Value ("TextMsg", TextMsg);
}
/** Get Text Message.
@return Text Message */
public String GetTextMsg() 
{
return (String)Get_Value("TextMsg");
}
/** Set Batch.
@param VA009_Batch_ID Batch */
public void SetVA009_Batch_ID (int VA009_Batch_ID)
{
if (VA009_Batch_ID <= 0) Set_Value ("VA009_Batch_ID", null);
else
Set_Value ("VA009_Batch_ID", VA009_Batch_ID);
}
/** Get Batch.
@return Batch */
public int GetVA009_Batch_ID() 
{
Object ii = Get_Value("VA009_Batch_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set VA009_PaymentBatchLog_ID.
@param VA009_PaymentBatchLog_ID VA009_PaymentBatchLog_ID */
public void SetVA009_PaymentBatchLog_ID (int VA009_PaymentBatchLog_ID)
{
if (VA009_PaymentBatchLog_ID < 1) throw new ArgumentException ("VA009_PaymentBatchLog_ID is mandatory.");
Set_ValueNoCheck ("VA009_PaymentBatchLog_ID", VA009_PaymentBatchLog_ID);
}
/** Get VA009_PaymentBatchLog_ID.
@return VA009_PaymentBatchLog_ID */
public int GetVA009_PaymentBatchLog_ID() 
{
Object ii = Get_Value("VA009_PaymentBatchLog_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
}

}
