/********************************************************
 * Module Name    : Payment Management
 * Purpose        : Used for Cash Journal related methods
 * Chronological Development
 * VA230     07/Mar/2022
  ******************************************************/
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VA009.Models;
using VAdvantage.Utility;

namespace VA009.Controllers
{
    public class CashJournalController : Controller
    {
        /// <summary>
        /// Get Order payment Schedule Details
        /// Author:VA230
        /// </summary>        
        /// <param name="fields">Order Id</param>
        /// <returns>Order Payment Schedule Detail</returns>
        public JsonResult GetOrderPaySchedDetail(string fields)
        {
            String retJSON = "";
            if (Session["ctx"] != null)
            {
                CashJournalModel obj = new CashJournalModel();
                retJSON = JsonConvert.SerializeObject(obj.GetOrderPaySchedDetail(fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Order Pay Schedule Amount
        /// Author:VA230
        /// </summary>        
        /// <param name="fields">Order Pay ScheduleId</param>
        /// <returns>Due Amount</returns>
        public JsonResult GetPaySheduleAmt(string fields)
        {
            String retJSON = "";
            if (Session["ctx"] != null)
            {
                CashJournalModel obj = new CashJournalModel();
                retJSON = JsonConvert.SerializeObject(obj.GetPaySheduleAmount(fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
    }
}