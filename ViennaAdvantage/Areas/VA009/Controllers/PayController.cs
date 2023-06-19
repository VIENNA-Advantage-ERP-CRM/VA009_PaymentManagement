using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using VA009.Models;
using VAdvantage.Utility;

namespace VA009.Controllers
{
    public class PayController : Controller
    {
        public JsonResult GetInvPaymentMethod(String fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                PayModel paymodel = new PayModel();
                retJSON = JsonConvert.SerializeObject(paymodel.GetInvPaymentMethod(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOrdPaymentMethod(String fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                PayModel paymodel = new PayModel();
                retJSON = JsonConvert.SerializeObject(paymodel.GetOrdPaymentMethod(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDueAmt(String fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel paymodel = new PayModel();
                retJSON = JsonConvert.SerializeObject(paymodel.GetDueAmt(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOpenAmt(String fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel paymodel = new PayModel();
                retJSON = JsonConvert.SerializeObject(paymodel.GetOpenAmt(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSchedule(string fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel payModel = new PayModel();
                retJSON = JsonConvert.SerializeObject(payModel.GetSchedule(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMandate(string fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel payModel = new PayModel();
                retJSON = JsonConvert.SerializeObject(payModel.GetMandate(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPaymentRule(string fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel payModel = new PayModel();
                retJSON = JsonConvert.SerializeObject(payModel.GetPaymentRule(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBPPaymentRule(string fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel payModel = new PayModel();
                retJSON = JsonConvert.SerializeObject(payModel.GetBPPaymentRule(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetInvoicePaymentRule(string fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel payModel = new PayModel();
                retJSON = JsonConvert.SerializeObject(payModel.GetInvoicePaymentRule(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPayAllocateAmt(string fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel payModel = new PayModel();
                retJSON = JsonConvert.SerializeObject(payModel.GetPayAllocateAmt(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBankDetails(String fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel paymodel = new PayModel();
                retJSON = JsonConvert.SerializeObject(paymodel.GetBankDetails(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPayBaseType(String fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel paymodel = new PayModel();
                retJSON = JsonConvert.SerializeObject(paymodel.GetPayBaseType(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBankCurrency(String fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel paymodel = new PayModel();
                retJSON = JsonConvert.SerializeObject(paymodel.GetBankCurrency(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetScheduleData(string fields) 
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel payModel = new PayModel();
                retJSON = JsonConvert.SerializeObject(payModel.GetScheduleData(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// This method is used to get journal detail
        /// </summary>
        /// <param name="fields">This fields is used to display the Parameter</param>
        /// <returns></returns>
        public JsonResult GetjournalDetail(string fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel payModel = new PayModel();
                retJSON = JsonConvert.SerializeObject(payModel.GetJournalDetail(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        // this linkage is used to veify the payment base type of payment method
        public JsonResult VerifyPayMethod(string fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel payModel = new PayModel();
                retJSON = JsonConvert.SerializeObject(payModel.VerifyPayMethod(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To get Bank account Name from business partner bank tab
        /// </summary>
        /// <param name="fields">Bank Account ID</param>
        /// <returns>Name</returns>
        public JsonResult getaccountName(string fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PayModel payModel = new PayModel();
                retJSON = JsonConvert.SerializeObject(payModel.getaccountdetails(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get true or false based on OrderSchedule count
        /// </summary>
        /// <param name="C_Order_ID">C_Order_ID</param>
        /// <returns>true or false based on count</returns>
        public JsonResult GetIsAdvanceOrder(string fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                PayModel payModel = new PayModel();
                retJSON = JsonConvert.SerializeObject(payModel.GetIsAdvanceOrder(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
    }
}