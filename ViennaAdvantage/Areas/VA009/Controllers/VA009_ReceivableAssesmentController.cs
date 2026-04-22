using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VA009.Models;
using VAdvantage.Utility;
using VIS.Classes;

namespace VA009.Controllers
{
    public class VA009_ReceivableAssesmentController : Controller
    {
        // GET: VA009/ReceivableAssesment
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetRecordDetail(int Screen_ID, int Tab_ID, int rec_ID, string tableName)
        {
            Ctx ct = Session["ctx"] as Ctx;
            VA009_ReceivableAssesmentModel obhRecModel = new VA009_ReceivableAssesmentModel();
            return Json(JsonConvert.SerializeObject(obhRecModel.GetRecordDetail(Screen_ID, Tab_ID, rec_ID, tableName)), JsonRequestBehavior.AllowGet);
        }

        public JsonResult CustomerReturnAnalysis(int rec_ID, string Depot, string CustName)
        {
            Ctx ct = Session["ctx"] as Ctx;
            if (!string.IsNullOrEmpty(Depot))
            {
                Depot = SecureEngineBridge.DecryptByClientKey(Depot, ct.GetSecureKey());
                if (!QueryValidator.IsValid(Depot))
                {
                    Depot = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(CustName))
            {
                CustName = SecureEngineBridge.DecryptByClientKey(CustName, ct.GetSecureKey());
                if (!QueryValidator.IsValid(CustName))
                {
                    CustName = string.Empty;
                }
            }
            VA009_ReceivableAssesmentModel obhRecModel = new VA009_ReceivableAssesmentModel();
            return Json(obhRecModel.CustomerReturnAnalysis(rec_ID, Depot, CustName), JsonRequestBehavior.AllowGet);
        }

        public JsonResult DispatchDataAnalysis(int rec_ID)
        {
            Ctx ct = Session["ctx"] as Ctx;
            VA009_ReceivableAssesmentModel obhRecModel = new VA009_ReceivableAssesmentModel();
            return Json(obhRecModel.DispatchDataAnalysis(rec_ID), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFinInsightsData(string IsReturnTrx, string IsSOTrx, int AD_Table_ID, string TabName, string WinDisplayName, int AD_Window_ID)
        {
            Ctx ctx = Session["ctx"] as Ctx;
            if (!string.IsNullOrEmpty(TabName))
            {
                TabName = SecureEngineBridge.DecryptByClientKey(TabName, ctx.GetSecureKey());
                if (!QueryValidator.IsValid(TabName))
                {
                    TabName = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(WinDisplayName))
            {
                WinDisplayName = SecureEngineBridge.DecryptByClientKey(WinDisplayName, ctx.GetSecureKey());
                if (!QueryValidator.IsValid(WinDisplayName))
                {
                    WinDisplayName = string.Empty;
                }
            }
            VA009_ReceivableAssesmentModel yearBasedExpenseData = new VA009_ReceivableAssesmentModel();
            List<dynamic> ExpenseData = yearBasedExpenseData.GetFinInsightsData(ctx, IsReturnTrx, IsSOTrx, AD_Table_ID, TabName, WinDisplayName, AD_Window_ID);
            return Json(JsonConvert.SerializeObject(ExpenseData), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCreditNotedata(int rec_ID)
        {
            Ctx ctx = Session["ctx"] as Ctx;
            VA009_ReceivableAssesmentModel yearBasedExpenseData = new VA009_ReceivableAssesmentModel();
            List<dynamic> ExpenseData = yearBasedExpenseData.GetCreditNotedata(ctx, rec_ID);
            return Json(JsonConvert.SerializeObject(ExpenseData), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCustScheduleData(int cusId, int pageNo, int pageSize, string isPaid)
        {
            Ctx ctx = Session["ctx"] as Ctx;
            VA009_ReceivableAssesmentModel objScheduleData = new VA009_ReceivableAssesmentModel();
            List<dynamic> result = objScheduleData.GetCustScheduleData(ctx, cusId, pageNo, pageSize, isPaid);
            return Json(JsonConvert.SerializeObject(result), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCancelInvoiceData(int recordId)
        {
            Ctx ctx = Session["ctx"] as Ctx;
            VA009_ReceivableAssesmentModel objScheduleData = new VA009_ReceivableAssesmentModel();
            var data = objScheduleData.GetCancelInvoiceData(ctx, recordId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}