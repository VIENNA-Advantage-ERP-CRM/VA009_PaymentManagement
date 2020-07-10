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
        /// <summary>
        ///  to get documen number against record id
        /// </summary>
        /// <param name="RecordID">record id</param>
        /// <param name="isBatch">batch or not</param>
        /// <param name="AD_Org_ID">org id</param>
        /// <returns>document number</returns>
        public JsonResult GetDocuentNumber(int RecordID, bool isBatch, int AD_Org_ID)
        {
            string docNo = "";
            if (Session["ctx"] != null)
            {
                string sql = "SELECT DocumentNo from C_Payment WHERE AD_Org_ID = "+ AD_Org_ID + " AND C_Payment_ID =" + RecordID;
                if (isBatch)
                    sql = "SELECT DocumentNo from VA009_Batch WHERE AD_Org_ID = " + AD_Org_ID + " AND VA009_Batch_ID =" + RecordID;
                docNo = Util.GetValueOfString(DB.ExecuteScalar(sql, null, null));
            }
            return Json(docNo, JsonRequestBehavior.AllowGet);
        }
    }
}