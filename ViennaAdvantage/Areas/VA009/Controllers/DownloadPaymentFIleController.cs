using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using VA009.Models;
using VAdvantage.DataBase;
using VAdvantage.Utility;

namespace VA009.Controllers
{
    public class DownloadPaymentFIleController : Controller
    {

        public JsonResult GetDocuentNumber(int RecordID, bool isBatch)
        {
            string docNo = "";
            if (Session["ctx"] != null)
            {
                string sql = "SELECT DocumentNo from C_Payment WHERE C_Payment_ID=" + RecordID;
                if (isBatch)
                    sql = "SELECT DocumentNo from VA009_Batch WHERE VA009_Batch_ID=" + RecordID;
                docNo = Util.GetValueOfString(DB.ExecuteScalar(sql, null, null));
            }
            return Json(docNo, JsonRequestBehavior.AllowGet);
        }
    }
}