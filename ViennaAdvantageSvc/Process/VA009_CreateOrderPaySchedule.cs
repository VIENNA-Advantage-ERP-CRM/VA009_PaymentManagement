using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;
using ViennaAdvantage.Model;

namespace ViennaAdvantage.Process
{
    class VA009_CreateOrderPaySchedule : SvrProcess
    {

        MOrder order = null;
        MPaymentTerm pt = null;
        MPaySchedule[] _schedule = null;

        protected override string DoIt()
        {
            order = new MOrder(GetCtx(), GetRecord_ID(), Get_TrxName());
            pt = new MPaymentTerm(GetCtx(), order.GetC_PaymentTerm_ID(), Get_TrxName());
            CreateOrderPaySchedule();
            return " ";
        }

        protected override void Prepare()
        {

            //throw new NotImplementedException();
        }

        /// <summary>
        /// Get Payment Term Schedules
        /// </summary>
        /// <param name="requery">true or False</param>
        /// <returns>List of Schedules</returns>
        private MPaySchedule[] GetSchedule(bool requery)
        {
            MPaySchedule ps = null;
            if (_schedule != null && !requery)
                return _schedule;

            // JID_0831: If Payment term header is valid but having lines with advance and Inactive. System should consider that as 100% immedate. However, system is creating schedule of advance on order.
            String sql = "SELECT * FROM C_PaySchedule WHERE IsActive = 'Y' AND VA009_Advance = 'Y' AND  C_PaymentTerm_ID=" + order.GetC_PaymentTerm_ID() + " ORDER BY NetDays";
            List<MPaySchedule> list = new List<MPaySchedule>();
            try
            {
                DataSet ds = VAdvantage.DataBase.DB.ExecuteDataset(sql, null, Get_TrxName());
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ps = new MPaySchedule(GetCtx(), dr, Get_TrxName());
                        ps.SetParent(pt);
                        list.Add(ps);
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(Level.SEVERE, "GetSchedule", e);
            }

            _schedule = new MPaySchedule[list.Count];
            _schedule = list.ToArray();
            return _schedule;
        }

        /// <summary>
        /// Create Order Payment Schedules
        /// </summary>
        private void CreateOrderPaySchedule()
        {
            if (order == null || order.Get_ID() == 0)
            {
                log.Log(Level.SEVERE, "No valid order - " + order);
                //return false;
            }


            GetSchedule(true);
            DeleteOrderPaySchedule(order.GetC_Order_ID(), order.Get_TrxName());
            if (pt.IsVA009_Advance() && pt.IsValid())
            {
                ApplyAdvanceTermSchedule(order);
            }

            else
            {
                for (int i = 0; i < _schedule.Length; i++)
                {
                    MPaySchedule _sch = new MPaySchedule(GetCtx(), _schedule[i].GetC_PaySchedule_ID(), Get_TrxName());
                    if (_sch.IsVA009_Advance())
                        ApplyAdvanceSchedule(order, _sch);
                    //break;
                }
            }
        }

        /// <summary>
        /// Apply Payment Term without schedule to Order
        /// </summary>
        /// <param name="order">Order</param>
        private void ApplyAdvanceTermSchedule(MOrder order)
        {
            //for(int i=0;i<_schedule.Length;i++))
            StringBuilder _sql = new StringBuilder();
            Decimal remainder = order.GetGrandTotal();
            MVA009OrderPaySchedule schedule = new MVA009OrderPaySchedule(GetCtx(), 0, Get_TrxName());
            MPaymentTerm payterm = new MPaymentTerm(GetCtx(), order.GetC_PaymentTerm_ID(), Get_TrxName());
            schedule.SetAD_Client_ID(order.GetAD_Client_ID());
            schedule.SetAD_Org_ID(order.GetAD_Org_ID());
            schedule.SetC_Order_ID(order.GetC_Order_ID());
            schedule.SetC_PaymentTerm_ID(order.GetC_PaymentTerm_ID());
            if (schedule.Get_ColumnIndex("C_DocType_ID") >= 0)
            {
                schedule.SetC_DocType_ID(order.GetC_DocType_ID());
            }
            schedule.SetVA009_PaymentMethod_ID(order.GetVA009_PaymentMethod_ID());

            //schedule.SetDueDate(GetDueDate(order));           

            // Get Next Business Day if Next Business Days check box is set to true
            DateTime? payDueDate = null;
            if (payterm.IsNextBusinessDay())
            {
                payDueDate = payterm.GetNextBusinessDate(TimeUtil.AddDays(order.GetDateOrdered(), payterm.GetNetDays()), order.GetAD_Org_ID());
            }
            else
            {
                payDueDate = TimeUtil.AddDays(order.GetDateOrdered(), payterm.GetNetDays());
            }
            schedule.SetDueDate(payDueDate);

            schedule.SetDueAmt(order.GetGrandTotal());

            if (payterm.IsNextBusinessDay())
            {
                payDueDate = payterm.GetNextBusinessDate(TimeUtil.AddDays(order.GetDateOrdered(), payterm.GetDiscountDays()), order.GetAD_Org_ID());
            }
            else
            {
                payDueDate = TimeUtil.AddDays(order.GetDateOrdered(), payterm.GetDiscountDays());
            }
            schedule.SetDiscountDate(payDueDate);

            //schedule.SetDiscountDate(order.GetDateOrdered().Value.AddDays(Util.GetValueOfInt(payterm.GetDiscountDays())));
            schedule.SetDiscountAmt((Util.GetValueOfDecimal((order.GetGrandTotal() * payterm.GetDiscount()) / 100)));

            if (payterm.IsNextBusinessDay())
            {
                payDueDate = payterm.GetNextBusinessDate(TimeUtil.AddDays(order.GetDateOrdered(), payterm.GetDiscountDays2()), order.GetAD_Org_ID());
            }
            else
            {
                payDueDate = TimeUtil.AddDays(order.GetDateOrdered(), payterm.GetDiscountDays2());
            }
            schedule.SetDiscountDays2(payDueDate);

            //schedule.SetDiscountDays2(order.GetDateOrdered().Value.AddDays(Util.GetValueOfInt(payterm.GetDiscountDays2())));
            schedule.SetDiscount2((Util.GetValueOfDecimal((order.GetGrandTotal() * payterm.GetDiscount2()) / 100)));

            schedule.SetVA009_PlannedDueDate(GetDueDate(order));

            int BaseCurrency = GetCtx().GetContextAsInt("$C_Currency_ID");

            if (BaseCurrency != order.GetC_Currency_ID())
            {
                decimal multiplyRate = MConversionRate.GetRate(order.GetC_Currency_ID(), BaseCurrency, order.GetDateAcct(), order.GetC_ConversionType_ID(), order.GetAD_Client_ID(), order.GetAD_Org_ID());
                schedule.SetVA009_OpenAmnt(order.GetGrandTotal() * multiplyRate);
            }
            else
            {
                schedule.SetVA009_OpenAmnt(order.GetGrandTotal());
            }

            schedule.SetC_Currency_ID(order.GetC_Currency_ID());
            schedule.SetVA009_BseCurrncy(BaseCurrency);
            schedule.SetVA009_OpnAmntInvce(order.GetGrandTotal());
            schedule.SetC_BPartner_ID(order.GetC_BPartner_ID());

            MOrder _Order = new MOrder(GetCtx(), order.GetC_Order_ID(), Get_TrxName());
            //schedule.SetVA009_PaymentMethod_ID(_Order.GetVA009_PaymentMethod_ID());
            //schedule.SetC_PaymentTerm_ID(_Order.GetC_PaymentTerm_ID());

            int _graceDay = payterm.GetGraceDays();
            DateTime? _followUpDay = GetDueDate(order);
            schedule.SetVA009_FollowupDate(_followUpDay.Value.AddDays(_graceDay));
            _sql.Clear();
            _sql.Append("SELECT VA009_PaymentMode, VA009_PaymentType, VA009_PaymentTrigger FROM VA009_PaymentMethod WHERE VA009_PaymentMethod_ID="
                + order.GetVA009_PaymentMethod_ID() + " AND IsActive = 'Y' AND AD_Client_ID = " + order.GetAD_Client_ID());
            DataSet ds = new DataSet();
            ds = DB.ExecuteDataset(_sql.ToString());

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                schedule.SetVA009_PaymentMode(Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PaymentMode"]));
                schedule.SetVA009_PaymentType(Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PaymentType"]));
                schedule.SetVA009_PaymentTrigger(Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PaymentTrigger"]));
                schedule.SetVA009_ExecutionStatus("A");
            }

            //	updateInvoice            
            if (!schedule.Save())
            {
                ValueNamePair pp = VLogger.RetrieveError();
                log.Info("Error found during creation of Order Schedule against Order ID = " + order.GetC_Order_ID() +
                           " Error Name is " + pp.GetName());
            }
        }

        /// <summary>
        /// Get Due Date based on the settings on Payment Term.
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Datetime, Due Date</returns>
        private DateTime? GetDueDate(MOrder order)
        {
            MPaymentTerm payterm = new MPaymentTerm(GetCtx(), order.GetC_PaymentTerm_ID(), Get_TrxName());
            String _sql = "SELECT PAYMENTTERMDUEDATE (C_PaymentTerm_ID, DateOrdered) AS DueDate FROM C_Order WHERE C_Order_ID=" + order.GetC_Order_ID();
            DateTime? _dueDate = Util.GetValueOfDateTime(DB.ExecuteScalar(_sql.ToString(), null, Get_TrxName()));
            if (_dueDate == Util.GetValueOfDateTime("1/1/0001 12:00:00 AM"))
                _dueDate = DateTime.Now;
            return _dueDate;
        }

        /// <summary>
        /// Apply Payment Term with schedule to order
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="_sch">Payment Term Schedule</param>
        private void ApplyAdvanceSchedule(MOrder order, MPaySchedule _sch)
        {
            MVA009OrderPaySchedule ips = null;
            Decimal remainder = order.GetGrandTotal();

            #region IsAdvance true on Schedule
            if (_sch.IsVA009_Advance())
            {
                ips = new MVA009OrderPaySchedule(order, _sch);
                ips.SetVA009_ExecutionStatus("A");

                MOrder _Order = new MOrder(GetCtx(), order.GetC_Order_ID(), Get_TrxName());
                ips.SetVA009_PaymentMethod_ID(order.GetVA009_PaymentMethod_ID());
                ips.SetC_PaymentTerm_ID(order.GetC_PaymentTerm_ID());
                ips.SetVA009_GrandTotal(order.GetGrandTotal());

                MPaymentTerm payterm = new MPaymentTerm(GetCtx(), order.GetC_PaymentTerm_ID(), Get_TrxName());
                int _graceDay = payterm.GetGraceDays();
                ips.SetVA009_FollowupDate(ips.GetDueDate().Value.AddDays(_graceDay));
                ips.SetVA009_PlannedDueDate(ips.GetDueDate());

                int BaseCurrency = GetCtx().GetContextAsInt("$C_Currency_ID");

                if (BaseCurrency != order.GetC_Currency_ID())
                {
                    decimal multiplyRate = MConversionRate.GetRate(order.GetC_Currency_ID(), BaseCurrency, order.GetDateAcct(), order.GetC_ConversionType_ID(), order.GetAD_Client_ID(), order.GetAD_Org_ID());
                    ips.SetVA009_OpenAmnt(ips.GetDueAmt() * multiplyRate);
                }
                else
                {
                    ips.SetVA009_OpenAmnt(ips.GetDueAmt());
                }

                // Get Next Business Day if Next Business Days check box is set to true
                DateTime? payDueDate = null;
                if (payterm.IsNextBusinessDay())
                {
                    payDueDate = payterm.GetNextBusinessDate(ips.GetDueDate(), order.GetAD_Org_ID());
                    ips.SetDueDate(payDueDate);

                    payDueDate = payterm.GetNextBusinessDate(ips.GetDiscountDate(), order.GetAD_Org_ID());
                    ips.SetDiscountDate(payDueDate);
                }

                ips.SetC_Currency_ID(order.GetC_Currency_ID());
                ips.SetVA009_BseCurrncy(BaseCurrency);
                ips.SetVA009_OpnAmntInvce(ips.GetDueAmt());
                ips.SetC_BPartner_ID(order.GetC_BPartner_ID());

                string sql = "SELECT VA009_PaymentMode, VA009_PaymentType, VA009_PaymentTrigger FROM VA009_PaymentMethod WHERE VA009_PaymentMethod_ID="
                + order.GetVA009_PaymentMethod_ID() + " AND IsActive = 'Y' AND AD_Client_ID = " + order.GetAD_Client_ID();
                DataSet ds = new DataSet();
                ds = DB.ExecuteDataset(sql);
                if (ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    ips.SetVA009_PaymentMode(Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PaymentMode"]));
                    if (!String.IsNullOrEmpty(Convert.ToString(ds.Tables[0].Rows[0]["VA009_PaymentType"])))
                    {
                        ips.SetVA009_PaymentType(Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PaymentType"]));
                    }
                    ips.SetVA009_PaymentTrigger(Util.GetValueOfString(ds.Tables[0].Rows[0]["VA009_PaymentTrigger"]));
                    ips.SetVA009_ExecutionStatus("A");
                }
                ips.SetProcessed(true);
                ips.Save(order.Get_TrxName());
                log.Fine(ips.ToString());
                remainder = Decimal.Subtract(remainder, ips.GetDueAmt());
            }
            #endregion

        }

        /// <summary>
        /// Delete existing order Payment Schedule
        /// </summary>
        /// <param name="C_order_ID">Order ID</param>
        /// <param name="trxName">Transaction</param>
        private void DeleteOrderPaySchedule(int C_order_ID, Trx trxName)
        {
            String sql = "DELETE FROM VA009_OrderPaySchedule WHERE C_order_ID=" + order.GetC_Order_ID();
            int no = VAdvantage.DataBase.DB.ExecuteQuery(sql, null, trxName);
            log.Fine("C_order_ID=" + C_order_ID + " - #" + no);
        }
    }
}