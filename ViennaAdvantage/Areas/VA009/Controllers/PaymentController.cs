using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using VA009.Models;
using VAdvantage.Utility;
using VIS.Classes;
using System.Dynamic;

namespace VA009.Controllers
{
    public class PaymentController : Controller
    {
        // GET: /VA009/Payment/
        [Authorize]
        public ActionResult Index(int windowno)
        {
            ViewBag.WindowNumber = windowno;
            return PartialView();
        }

        public JsonResult GetData(int pageNo, int pageSize, string whereQry, string OrgWhr, string SearchText, string WhrDueDate, string TransType, string FromDate, string ToDate)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            LoadData _Paydata = _payMdl.GetloadData(pageNo, pageSize, ct, whereQry, OrgWhr, SearchText, WhrDueDate, TransType, FromDate, ToDate);
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBPName(string searchText)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            List<BPDetails> _BPrtdata = _payMdl.GetBPnames(searchText, ct);
            return Json(JsonConvert.SerializeObject(_BPrtdata), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPopUpData(string InvPayids, int bank_id, int acctno, string chkno, string OrderPayids)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            List<PaymentData> _Paydata = _payMdl.GetChquePopUpdata(ct, InvPayids, bank_id, acctno, HttpUtility.HtmlDecode(chkno), OrderPayids);
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GeneratePayments(string PaymentData)
        {
            GeneratePaymt[] arr = JsonConvert.DeserializeObject<GeneratePaymt[]>(PaymentData);

            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            string _Paydata = _payMdl.CreatePayments(ct, arr);
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GenPaymentsCash(string PaymentData, int C_CashBook_ID, decimal BeginningBalance)
        {
            GeneratePaymt[] arr = JsonConvert.DeserializeObject<GeneratePaymt[]>(PaymentData);
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            string _Paydata = _payMdl.CreatePaymentsCash(ct, arr, C_CashBook_ID, BeginningBalance);
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GeneratePaymentsBatch(string PaymentData)
        {
            GeneratePaymt[] arr = JsonConvert.DeserializeObject<GeneratePaymt[]>(PaymentData);
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            string _Paydata = _payMdl.CrateBatchPayment(ct, arr);
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateSplitPayments(string PaymentData, string SplitAmmount)
        {
            GeneratePaymt[] arr = JsonConvert.DeserializeObject<GeneratePaymt[]>(PaymentData);
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            string _Paydata = _payMdl.SplitSchedule(ct, arr, SplitAmmount);
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GeneratePaymentsBatchOfRule(string PaymentData)
        {
            GeneratePaymt[] arr = JsonConvert.DeserializeObject<GeneratePaymt[]>(PaymentData);
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            string _Paydata = _payMdl.GeneratelinesOnRule(ct, arr);
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetConvertedAmt(string PaymentData, int BankAccount, int CurrencyType)
        {
            GeneratePaymt[] arr = JsonConvert.DeserializeObject<GeneratePaymt[]>(PaymentData);
            Ctx ctx = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            List<PaymentData> _Paydata = _payMdl.ConvertedAmt(ctx, arr, BankAccount, CurrencyType);
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCashJournalConvertedAmt(string PaymentData, int CurrencyCashBook, int CurrencyType)
        {
            GeneratePaymt[] arr = JsonConvert.DeserializeObject<GeneratePaymt[]>(PaymentData);
            Ctx ctx = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            List<PaymentData> Paydata = _payMdl.CashBookConvertedAmt(ctx, arr, CurrencyCashBook, CurrencyType);
            return Json(JsonConvert.SerializeObject(Paydata), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetChat(string RecordID)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            string _Paydata = _payMdl.GetLastChat(ct, Util.GetValueOfInt(RecordID));
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/May/2017
        public ActionResult GetCashPayment(string payment_Ids, string order_Ids)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            string cashData = _payMdl.GetCashPayments(payment_Ids, order_Ids, ct);
            return Json(JsonConvert.SerializeObject(cashData), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/June/2017
        public ActionResult LoadBank(string Orgs)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.LoadBank(Orgs, ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/June/2017
        public ActionResult GetBankAccountData(int BankAccount)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.GetBankAccountData(BankAccount, ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/June/2017
        public ActionResult LoadOrganization()
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.LoadOrganization(ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/June/2017
        public ActionResult LoadPaymentMethod()
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.LoadPaymentMethod(ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/June/2017
        public ActionResult LoadStatus()
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.LoadStatus(ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/June/2017
        public ActionResult loadCurrencyType()
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.loadCurrencyType(ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/June/2017
        public ActionResult LoadBankAccount(int Bank_ID, string Orgs)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.LoadBankAccount(Bank_ID, Orgs, ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/June/2017
        public ActionResult GetCurrencyPrecision(int BankAccount_ID, string CurrencyFrom)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.GetCurrencyPrecision(BankAccount_ID, CurrencyFrom, ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/June/2017
        public ActionResult GetDocBaseType(string Payment_ID)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.GetDocBaseType(Payment_ID, ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/June/2017
        public ActionResult LoadCashBook(string Orgs)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.LoadCashBook(Orgs, ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/June/2017
        public ActionResult GetCashBookData(int CashBook)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.GetCashBookData(CashBook, ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 01/June/2017
        public ActionResult LoadBatchPaymentMethod()
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.LoadBatchPaymentMethod(ct)), JsonRequestBehavior.AllowGet);
        }


        //Added by Bharat on 01/June/2017
        public ActionResult GetPaymentRule(int PaymentMethod)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.GetPaymentRule(PaymentMethod, ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Manjot on 12/Dec/2018
        public ActionResult LoadChequePaymentMethod()
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.LoadChequePaymentMethod(ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 05/June/2017
        public ActionResult GetPaymentBaseType(string BatchQry)
        {
            Ctx ct = Session["ctx"] as Ctx;
            BatchQry = SecureEngineBridge.DecryptByClientKey(BatchQry, ct.GetSecureKey());
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.GetPaymentBaseType(BatchQry, ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 05/June/2017
        public ActionResult GetChatID(int RecordID)
        {
            Ctx ct = Session["ctx"] as Ctx;            
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.GetChatID(RecordID, ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 05/June/2017
        public ActionResult GetBatchProcess()
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.GetBatchProcess(ct)), JsonRequestBehavior.AllowGet);
        }

        //Added by Bharat on 05/June/2017
        public ActionResult GetWindowID(string WindowName)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.GetWindowID(WindowName, ct)), JsonRequestBehavior.AllowGet);
        }
        //Added by Manjot on 08/Aug/2017
        public ActionResult GetXMLPath(string FileName, int RecordID)
        {
            Ctx ct = Session["ctx"] as Ctx; 
            DBXml _obj = new DBXml();
            return Json(JsonConvert.SerializeObject(_obj.CreateJDBCDataSourceXml(ct,FileName, RecordID)), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPaymentScheduleBatch(string FileName, int RecordID)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _obj = new PaymentModel();
            List<PayBatchDetails> res = _obj.GetPayScheduleBatch(ct);
            var jRes = Json(JsonConvert.SerializeObject(new { result = res, error = "" }), JsonRequestBehavior.AllowGet);
            jRes.MaxJsonLength = int.MaxValue;
            return jRes;
        }

        /// <summary>
        /// Create Payment Manually
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="InvoiceSchdIDS">List of Selected invoice schedules</param>
        /// <param name="OrderSchdIDS">List of Selected order schedules</param>
        /// <param name="BankID">Bank ID</param>
        /// <param name="BankAccountID">Bank Account ID</param>
        /// <param name="PaymentMethodID">Payment Method ID</param>
        /// <param name="DateAcct">Account Date</param>
        /// <param name="CurrencyType">Currency Type</param>
        ///  <param name="DateTrx">Transaction Date</param>
        /// <returns>Message in JSON Format</returns>
        [HttpPost]
        public ActionResult GeneratePaymentsMannualy(string InvoiceSchdIDS, string OrderSchdIDS, int BankID, int BankAccountID, int PaymentMethodID, string DateAcct, string CurrencyType, string DateTrx)
        {            
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            string _Paydata = _payMdl.CreatePaymentsMannualy(ct, InvoiceSchdIDS, OrderSchdIDS, BankID, BankAccountID, PaymentMethodID, DateAcct, CurrencyType, DateTrx);
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GeneratePaymentsBtoB(string recordData)
        {
            dynamic OData = JsonConvert.DeserializeObject<ExpandoObject>(recordData);
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            string _Paydata = _payMdl.CreatePaymentsBtoB(ct, OData);
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        public ActionResult loadCurrencies()
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.LoadCurrencies(ct)), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To get current Next Number from seleted bank and payment method
        /// </summary>
        /// <param name="C_BankAccount_ID">Bank Account</param>
        /// <param name="VA009_PaymentMethod_ID">Payment Method</param>
        /// <returns>Check Number</returns>
        public ActionResult getCheckNo(int C_BankAccount_ID, int VA009_PaymentMethod_ID)
        {
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.getCheckNo(C_BankAccount_ID, VA009_PaymentMethod_ID)), JsonRequestBehavior.AllowGet);
        }
    }
}
