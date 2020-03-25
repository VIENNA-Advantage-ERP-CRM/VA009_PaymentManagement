using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAdvantage.Classes;
using VAdvantage.DataBase;
using VAdvantage.Utility;
using ViennaAdvantage.Model;
using VAdvantage.Model;

namespace ViennaAdvantage.Model
{
    public class MVA009OrderPaySchedule : X_VA009_OrderPaySchedule
    {
        private MOrder _parent = null;
        private static Decimal HUNDRED = 100.0M;

        public MVA009OrderPaySchedule(Ctx ctx, int VA009_OrderPaySchedule_ID, Trx trxName)
            : base(ctx, VA009_OrderPaySchedule_ID, trxName)
        {

        }
        public MVA009OrderPaySchedule(Context ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName) 
        {
 
        }
        public MVA009OrderPaySchedule(MOrder order, MPaySchedule paySchedule)
            : base(order.GetCtx(), 0, order.Get_TrxName())
        {

            _parent = order;
            SetClientOrg(order);
            SetC_Order_ID(order.GetC_Order_ID());            
            SetC_PaySchedule_ID(paySchedule.GetC_PaySchedule_ID());

            //	Amounts
            int scale = MCurrency.GetStdPrecision(GetCtx(), order.GetC_Currency_ID());
            Decimal due = order.GetGrandTotal();
            if (due.CompareTo(Env.ZERO) == 0)
            {
                SetDueAmt(Env.ZERO);
                SetDiscountAmt(Env.ZERO);
                //SetIsValid(false);
            }
            else
            {
                //due = due.multiply(paySchedule.getPercentage()).divide(HUNDRED, scale, Decimal.ROUND_HALF_UP);
                due = Decimal.Multiply(due, Decimal.Divide(paySchedule.GetPercentage(),
                    Decimal.Round(HUNDRED, scale, MidpointRounding.AwayFromZero)));
                SetDueAmt(due);
                Decimal discount = Decimal.Multiply(due, Decimal.Divide(paySchedule.GetDiscount(),
                    Decimal.Round(HUNDRED, scale, MidpointRounding.AwayFromZero)));
                SetDiscountAmt(discount);
                //SetIsValid(true);
            }

            //	Dates		
            DateTime dueDate = TimeUtil.AddDays(order.GetDateOrdered(), paySchedule.GetNetDays());
            SetDueDate(dueDate);
            DateTime discountDate = TimeUtil.AddDays(order.GetDateOrdered(), paySchedule.GetDiscountDays());
            SetDiscountDate(discountDate);
        }
    }
}
