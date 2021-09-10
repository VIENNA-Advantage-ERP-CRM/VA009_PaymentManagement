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

        /// <summary>
        /// Get Currency of Bank Account
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="BankAccount_ID">Bank Account</param>
        /// <returns>C_Currency_ID</returns>
        public JsonResult GetBankAccountCurrency(Ctx ctx, int BankAccount_ID)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.GetBankAccountCurrency(ctx, BankAccount_ID)), JsonRequestBehavior.AllowGet);
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
            string _Paydata = _payMdl.CrateBatchPayments(ct, arr);
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

        /// <summary>
        /// Get Updated Converted Amount with Payment Data
        /// </summary>
        /// <param name="PaymentData">Payment Data</param>
        /// <param name="BankAccount">C_BankAccount_ID</param>
        /// <param name="CurrencyType">C_ConversionType_ID</param>
        /// <param name="dateAcct">Account Date</param>
        /// <param name="_org_Id">AD_Org_ID</param>
        /// <returns>returns Payment Data to bind on grid</returns>
        public JsonResult GetConvertedAmt(string PaymentData, int BankAccount, int CurrencyType, DateTime? dateAcct, int _org_Id)
        {
            //GeneratePaymt[] arr = JsonConvert.DeserializeObject<GeneratePaymt[]>(PaymentData);
            //Ctx ctx = Session["ctx"] as Ctx;
            //PaymentModel _payMdl = new PaymentModel();
            //List<PaymentData> _Paydata = _payMdl.ConvertedAmt(ctx, arr, BankAccount, CurrencyType, dateAcct, _org_Id);
            return GetConvertedAmtBatch(PaymentData, BankAccount, CurrencyType, 0, dateAcct, _org_Id);
        }



        /// <summary>
        /// Get Updated Converted Amount with Payment Data
        /// </summary>
        /// <param name="PaymentData">Payment Data</param>
        /// <param name="BankAccount">C_BankAccount_ID</param>
        /// <param name="CurrencyType">C_ConversionType_ID</param>
        /// <param name="ToCurrency">C_Currency_ID</param>
        /// <param name="dateAcct">Account Date</param>
        /// <param name="_org_Id">AD_Org_ID</param>
        /// <returns>returns Payment Data to bind on grid</returns>
        public JsonResult GetConvertedAmtBatch(string PaymentData, int BankAccount, int CurrencyType, int ToCurrency, DateTime? dateAcct, int _org_Id)
        {
            GeneratePaymt[] arr = JsonConvert.DeserializeObject<GeneratePaymt[]>(PaymentData);
            Ctx ctx = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            List<PaymentData> _Paydata = _payMdl.ConvertedAmt(ctx, arr, BankAccount, CurrencyType, ToCurrency, dateAcct, _org_Id);
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get CashLine Data in JSON format
        /// </summary>
        /// <param name="PaymentData">Payment Data</param>
        /// <param name="CurrencyCashBook">CashBook C_Currency_ID</param>
        /// <param name="CurrencyType">C_ConversionType_ID</param>
        /// <param name="dateAcct">DateAcct</param>
        /// <param name="_org_Id">AD_Org_ID</param>
        /// <returns>returns JSON result</returns>
        public JsonResult GetCashJournalConvertedAmt(string PaymentData, int CurrencyCashBook, int CurrencyType, DateTime? dateAcct, int _org_Id)
        {
            GeneratePaymt[] arr = JsonConvert.DeserializeObject<GeneratePaymt[]>(PaymentData);
            Ctx ctx = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            List<PaymentData> Paydata = _payMdl.CashBookConvertedAmt(ctx, arr, CurrencyCashBook, CurrencyType, dateAcct, _org_Id);
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
            return Json(JsonConvert.SerializeObject(_obj.CreateJDBCDataSourceXml(ct, FileName, RecordID)), JsonRequestBehavior.AllowGet);
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
        public ActionResult GeneratePaymentsMannualy(string InvoiceSchdIDS, string OrderSchdIDS, int BankID, int BankAccountID, int PaymentMethodID, string DateAcct, string CurrencyType, string DateTrx, int AD_Org_ID)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            string _Paydata = _payMdl.CreatePaymentsMannualy(ct, InvoiceSchdIDS, OrderSchdIDS, BankID, BankAccountID, PaymentMethodID, DateAcct, CurrencyType, DateTrx, AD_Org_ID);
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
        /// <summary>
        /// To get Document Type
        /// </summary>
        /// <param name="Organization"></param>
        /// <returns>Document Type ID's</returns>
        public ActionResult GetDocumentType(string orgs)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            return Json(JsonConvert.SerializeObject(_payMdl.GetDocumentType(orgs, ct)), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Location against selected Business partner
        /// </summary>
        /// <param name="BP"></param>
        /// <returns></returns>
        public ActionResult GetLocation(string BP)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            List<LocationDetails> _LocationData = _payMdl.GetLocation(BP, ct);
            return Json(JsonConvert.SerializeObject(_LocationData), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Location against selected Business partner
        /// </summary>
        /// <param name="BP"></param>
        /// <returns></returns>
        public ActionResult GetCharge(string orgs)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            List<ChargeDetails> _ChargeData = _payMdl.GetCharge(orgs, ct);
            return Json(JsonConvert.SerializeObject(_ChargeData), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public ActionResult GetBPartnerName(string orgs)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            List<BPDetails> _BPrtdata = _payMdl.GetBPartnerName(orgs, ct);
            return Json(JsonConvert.SerializeObject(_BPrtdata), JsonRequestBehavior.AllowGet);
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
        public ActionResult GeneratePayMannual(string PaymentData)
        {
            Ctx ct = Session["ctx"] as Ctx;
            List<Dictionary<string, string>> pData = null;
            PaymentModel _payMdl = new PaymentModel();
            if (PaymentData != null)
            {
                pData = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(PaymentData);
            }
            string _Paydata = _payMdl.GeneratePayMannual(ct, pData);
            return Json(JsonConvert.SerializeObject(_Paydata), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// to prepare data for Payment File
        /// </summary>
        /// <param name="Payment_ID"> Payment ID</param>
        /// <returns>Data For Payment</returns>
        public ActionResult prepareDataForPaymentFile(String DocNumber, bool isBatch, string AD_Org_ID)
        {
            Ctx ct = Session["ctx"] as Ctx;
            PaymentModel _payMdl = new PaymentModel();
            List<PaymentResponse> obj = new List<PaymentResponse>();
            obj = _payMdl.prepareDataForPaymentFile(ct, DocNumber, isBatch, AD_Org_ID);
            return Json(JsonConvert.SerializeObject(obj), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This function is used to check the conversion rate availabe or not
        /// </summary>
        /// <param name="fields">bank, currencyTo, ConversionType, Date, client , org</param>
        /// <returns>conversion Rate</returns>
        public JsonResult CheckConversionRate(string fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                PaymentModel objConversionModel = new PaymentModel();
                retJSON = JsonConvert.SerializeObject(objConversionModel.CheckConversionRate(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get the C_Process_ID from Bank Account Document Tab from Bank Window
        /// </summary>
        /// <param name="_bankAct_Id">C_BankAccount_ID</param>
        /// <returns>returns C_Process_ID</returns>
        public JsonResult GetProcessId(string fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                PaymentModel objConversionModel = new PaymentModel();
                retJSON = JsonConvert.SerializeObject(objConversionModel.GetProcessId(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get the DocumentTypes based on Batch Payments
        /// </summary>
        /// <param name="ad_org_Id">AD_Org_ID</param>
        /// <returns>List of Document Types</returns>
        public JsonResult LoadTargetType(string ad_org_Id)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                PaymentModel objConversionModel = new PaymentModel();
                retJSON = JsonConvert.SerializeObject(objConversionModel.GetTargetType(ctx, Util.GetValueOfInt(ad_org_Id)));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
    }
}
