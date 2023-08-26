/********************************************************
 * Module Name    : VA009 (VA Payment Management)
 * Purpose        : Create Payments From Schedules in Different Modes
 * Class Used     : 
 * Chronological Development
 * Manjot Singh     13 Jan 2016
 ******************************************************/
; VA009 = window.VA009 || {};
//java script function closer
; (function (VA009, $) {

    //Form Class Function FullNameSpace
    VA009.PaymentForm = function () {
        // Varriables
        this.frame;
        this.windowNo;
        var $self = this; //scoped self pointer
        var $root = $('<div class="VA009-assign-content">');
        var MainRoot;
        var _leftBar; // Left Panel
        var _MiddlePanel, _whereQry = ""; // Middle Panel
        var _RightPanel, $ConvertedAmt; Orgname = "";
        var _isinvoice = 'Y', _DocType = 'ARI';
        var $Org, $OrgSelected, $payMthd, $PayMSelected, $status, $statusSelected;
        var $togglebtn, lbdata, $lbmain, $divPayment, $BP, $BPSelected, $divBank;
        var pgNo = 1, pgSize = 20, PAGESIZE = 20, $CR_Tab, $CP_Tab, $XML_Tab, Pay_ID = 0; //changed page size
        var isloaded = false, _WhereQuery = "", $divcashbk;
        var orgids = [], bpids = [], SlctdPaymentIds = []; SlctdJournalPaymentIds = [];
        var SelectallOrdIds = [], SelectallInvIds = [], SelectallJournalIds = [];
        var paymntIds = [], statusIds = [];
        var _WhrOrg = "", _WhrPayMtd = "", _Whr_BPrtnr = "", _WhrStatus = "";
        // By Amit - 16-11-2016
        var _WhrTransType = "";
        var transtypes = [];
        var $TransactionType, $TransactionTypeSelected;
        var $FromDate, $ToDate;
        var SlctdOrderPaymentIds = [];
        /* VIS_427 DevOps id:2268 Variable defined to execute Zoom Functionalty for batch*/
        var batchid = 0;
        var Batchsuccesspay;
        /* VIS_427 DevOps id:2238 Variable defined to execute 
         function for making checkbox true  of selected schedules */
        var isReset = false;
        /* VIS_427 DevOps id: 2247 Varibale Defined to get those Business Partner 
         id Which user Unselect from left pannel*/
        var BP_id = null;
        /* VIS_427 Bug Id 2339 array defined to get check from bank*/
        var chknumbers = [];
        var removedcheck = [];// VIS_427 Bug Id 2339 array defined to get those  check which are allocated
        var autocheckCtrl = null; // VIS_427 Bug Id 2339 variable to store auto check value
        var BusinessPartnerIds = []; //VIS_427 DevOps id: 2247 Array defined to get the selected Records of Business partner
        var SlctdBpId = []; //VIS_427 DevOps id: 2247 Array defined to get the unselected Records of Business partner
        //end
        //By Manjot For Batch Functionality 1/8/17
        var batchObjInv = [];
        var batchObjJournal = [];
        var batchObjOrd = [];
        var $xmlpopGrid = null;
        var MsgReturn = "";
        var $SelectedDiv, $chatDiv, $chkicon, $cashicon, $batchicon, $Spliticon, $PayMannualicon, $PayBankToBank, $table, tableleft;
        var popupgrddata = [], Duedateval = [], datasplit = []; var Payrule = "", DueDateSelected = 99;
        var reloaddata = [];
        var chqrecgrd, chqpaygrd, Cashgrd, BatchGrd, Splitgrd;
        var winHeight = $(window).height();
        var record_ID = 0, CashCurrency = 0;
        var $bsyDiv = null, $SrchTxtBox, $SrchBtn, $Duedateul, $DueDateSelected, _BPartnerLookup, _BPControl, _BpartnerSelectedVal;
        var $totalAmt = null;
        // Added by Bharat on 01/May/2017
        var ChequePayDialog = null;
        var ChequeReceDialog = null;
        var CheueRecevableGrid = null;
        var $chequeRecivable = null;
        var CheuePaybleGrid = null;
        var $chequePayble = null;
        var $selectall = null;
        var $refresh = null;
        // to implement cashbook tab
        var $tabCashbook = null;
        var $tabFunds = null;
        var $POP_DateAcct = null;
        //date Trx and Currency type
        var $POP_DateTrx = null;
        var $POP_CurrencyType = null;
        var defaultCurrenyType = 0;
        //added parameter for genearte payment file
        //'M'--- Manual, 'B'---Batch
        var File_Para = 'M';
        var culture = new VIS.CultureSeparator();
        var format = VIS.DisplayType.GetNumberFormat(VIS.DisplayType.Amount);
        var dotFormatter = VIS.Env.isDecimalPoint();
        //Rakesh(VA228):Set type to fetch the target base type
        //AP Receipt 2->AP Payment 3->Cash Journal 4->Batch Payment
        var _TargetBaseType = 1;
        var $resultb2b = null;
        var $batchResult = null;/* VIS_427 DevOps id:2268 Variable defined to execute Zoom Functionalty for batch*/
        var $closeb2b = null;
        var $createNew = null;
        var $ViewBatch = null;
        var $cancel = null;
        var precision = 2; /* VIS_427 DevOps id:2289 Variable defined for precision */
        //varriable to show the message if cheques are not available 
        var _ChequesNotAvailable = false;
        var $btnChequePrint = null, chequePrintParams = [];
        //var elements = [
        //    "VA009_Cancel",
        //];
        //VIS.translatedTexts = VIS.Msg.translate(VIS.Env.getCtx(), elements, true);

        this.Initialize = function () {
            Initialize();
        };

        function Initialize() {
            LoadDesign();
            GetControls();
            InitializeEvents();
            _loadFunctions.loadOrg();
            _loadFunctions.loadPaymentMethods();
            _loadFunctions.loadStatus();
            createBusyIndicator();
            pgNo = 1; //Commented on 10 Jan 2019 "AND cs.AD_Org_ID IN (" + VIS.context.getAD_Org_ID() + ")"
            loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), 99, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
            isloaded = true;
            $BP.autocomplete({
                minLength: 0,
                source: function (request, response) {
                    if (request.term.trim().length == 0) {
                        return;
                    }

                    fillAutoCompleteonTextBox(request.term, response, "GetBPName");
                },
                select: function (ev, ui) {
                    if ((bpids.indexOf(ui.item.ids) != -1)) {
                        return;
                    }

                    if ($BPSelected.find("li").length > 0) // For Continues Selection
                    {
                        $BPSelected.append('<ul><li id=' + "VA009_li_" + ui.item.ids + '><i title="' + VIS.Msg.getMsg("VA009_Cancel") + '"  class="VA009-cross-btn vis vis-mark" id=' + "VA009_Delimg_" + ui.item.ids + '></i> <span>' + ui.item.value + '</span></li>');
                        bpids.push(ui.item.ids); // Add Values in Array For Search
                        whereClause("cb.C_BPartner_ID", bpids);
                    }
                    else {
                        $BPSelected.append('<ul><li id=' + "VA009_li_" + ui.item.ids + '><i title="' + VIS.Msg.getMsg("VA009_Cancel") + '"  class="VA009-cross-btn vis vis-mark" id=' + "VA009_Delimg_" + ui.item.ids + '></i> <span>' + ui.item.value + '</span></li>');
                        bpids.push(ui.item.ids);
                        whereClause("cb.C_BPartner_ID", bpids);
                    }
                    isReset = true;
                    $(this).val('');
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove(); // to remove accordion div 
                    pgNo = 1;
                    //Reset no of pages
                    resetPaging();
                    //loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
                    loadPaymetsAll();
                }
            });
        };
        /**VA230: Reset paging like no of pages */
        function resetPaging() {
            noPages = 1;
        }

        /*VIS_427 DevOps id:2238 Function to clear total amount*/
        function clearamtid() {
            SelectallInvIds = [];
            SelectallOrdIds = [];
            SelectallJournalIds = [];
            BusinessPartnerIds = [];
            $totalAmt.text(0);
            $totalAmt.data('ttlamt', parseFloat(0));
        }
        //******************
        //Custom Design of Paymnet Form
        //******************
        function LoadDesign() {

            // Create Left Panel
            _leftBar = '<div class="VA009-main-container" id="VA009-main-container_' + $self.windowNo + '"> <table id="VA009_table_' + $self.windowNo + '" style="width: 100%;"><tr><td id="VA009_tdLeft_' + $self.windowNo + '" style="width: 200px; position: relative;"> <div class="VA009-left-part vis-leftsidebarouterwrap" id=' + "VA009_leftpart_" + $self.windowNo + '>';

            // Add Toggle button
            _btnToggle = "<div class='VA009-left-title' ><i class='VA009-toggle-btn fa fa-bars' id='" + 'VA009_toggleImg_' + $self.windowNo + "' ></i></div>";
            _leftBar += _btnToggle;

            // Add Search parameters
            _leftBar += '<div class="VA009-left-part" style="overflow:auto;" id=' + "VA009_leftpartdata_" + $self.windowNo + '>';
            _leftBar += '<div class="VA009-left-data"><div class="vis-input-wrap"><div class="vis-control-wrap"> <select id = ' + "VA009_Org_" + $self.windowNo + '> </select> <label>' + VIS.Msg.getMsg("VA009_Org") + '</label></div></div>  <div class="VA009-value-list" id= ' + "VA009_OrgdataDiv_" + $self.windowNo + '> </div> </div>';
            _leftBar += '<div class="VA009-left-data"><div class="vis-input-wrap"><div class="vis-control-wrap">  <input type="text" id = ' + "VA009_BPartner_" + $self.windowNo + '><label>' + VIS.Msg.getMsg("VA009_BPartner") + '</label> </div></div><div class="VA009-value-list" id= ' + "VA009_BPdataDiv_" + $self.windowNo + '>      </div> </div>';
            _leftBar += '<div class="VA009-left-data"><div class="vis-input-wrap"><div class="vis-control-wrap"> <select id = ' + "VA009_PayMthd_" + $self.windowNo + '>   </select><label>' + VIS.Msg.getMsg("VA009_PayMthd") + '</label> </div></div>  <div class="VA009-value-list" id= ' + "VA009_PayMthddataDiv_" + $self.windowNo + '>      </div> </div>';
            _leftBar += '<div class="VA009-left-data"><div class="vis-input-wrap"><div class="vis-control-wrap">  <select id = ' + "VA009_Status_" + $self.windowNo + '>  </select><label>' + VIS.Msg.getMsg("VA009_Status") + '</label></div></div>  <div class="VA009-value-list" id= ' + "VA009_StatusdataDiv_" + $self.windowNo + '>      </div> </div>';
            _leftBar += '<div class="VA009-left-data"><div class="vis-input-wrap"><div class="vis-control-wrap">  <select id = ' + "VA009_DueDate_" + $self.windowNo + '><option value=""></option> <option  value=0>' + VIS.Msg.getMsg("VA009_Current") + '</option>  <option value=7>' + VIS.Msg.getMsg("VA009_7Days") + '</option>  <option value=14 >' + VIS.Msg.getMsg("VA009_14Days") + '</option> <option value=30 >' + VIS.Msg.getMsg("VA009_30Days") + '</option> <option value=60 >' + VIS.Msg.getMsg("VA009_60Days") + '</option> <option value=90>' + VIS.Msg.getMsg("VA009_90Days") + '</option>" </select><label>' + VIS.Msg.getMsg("VA009_Duedate") + '</label></div></div>  <div class="VA009-value-list" id= ' + "VA009_DueDatedataDiv_" + $self.windowNo + '> </div>  </div> ';

            //change by amit - 16-nov-2016
            _leftBar += '<div class="VA009-left-data"><div class="vis-input-wrap"><div class="vis-control-wrap">  <select id = ' + "VA009_TransactionType_" + $self.windowNo + '><option value=""></option> <option  value=0>' + VIS.Msg.getMsg("VA009_Order") + '</option>  <option value=1>' + VIS.Msg.getMsg("VA009_Invoice") + '</option> <option  value=2>' + VIS.Msg.getMsg("VA009_Journal") + '</option>" </select><label>' + VIS.Msg.getMsg("VA009_TransactionType") + '</label></div></div>  <div class="VA009-value-list" id= ' + "VA009_TransactionTypeDiv_" + $self.windowNo + '> </div>  </div>';
            //end
            _leftBar += '<div class="VA009-left-data"><div class="vis-input-wrap"><div class="vis-control-wrap">  <input type="date" max="9999-12-31" id="VA009_FromDate_' + $self.windowNo + '"><label>' + VIS.Msg.getMsg("VA009_FromDate") + '</label></div></div> </div>';

            _leftBar += '<div class="VA009-left-data"><div class="vis-input-wrap"><div class="vis-control-wrap">  <input type="date" max="9999-12-31" id="VA009_ToDate_' + $self.windowNo + '"> <label>' + VIS.Msg.getMsg("VA009_ToDate") + '</label></div></div></div> </div></div></td>';

            //End Left Panel

            MainRoot = _leftBar;
            //Create Content Area'
            MainRoot += '<td><div class="VA009-content-area" id="VA009-content-area_' + $self.windowNo + '">';
            //Create Middle Panel
            _MiddlePanel = '<div class="VA009-middle-wrap" id="VA009-middle-wrap_' + $self.windowNo + '">';

            _MiddlePanel += '<div class="VA009-mid-top-wrap" id="VA009-mid-top-wrap_' + $self.windowNo + '"> ';

            // Add Tabs Current recieveables,Paybales Etc..
            _MiddlePanel += '<div class="VA009-tabs-wrap"> <ul class="VA009-tabs"> <li class="VA009-active-tab" id = ' + "VA009_CR_Tab_" + $self.windowNo + '>' + VIS.Msg.getMsg("VA009_CR_Tab") + '</li> <li  id = ' + "VA009_CP_Tab_" + $self.windowNo + '>' + VIS.Msg.getMsg("VA009_CP_Tab") + '</li>' +
                '<li style="display:none;" id = ' + "VA009_XML_Tab_" + $self.windowNo + '>' + VIS.Msg.getMsg("VA009_GenXML_Tab") + '</li></ul></div>';

            //Add Icons
            _MiddlePanel += '<div class="VA009-icons-wrap">';
            _MiddlePanel += "<span><i class='vis vis-cheque' title='" + VIS.Msg.getMsg("VA009_Check") + "' id='" + 'VA009_Chkimg_' + $self.windowNo + "' ></i></span> <span><i class='vis vis-cash-bag' title='" + VIS.Msg.getMsg("VA009_Cash") + "'  id='" + 'VA009_Cashimg_' + $self.windowNo + "' ></i></span><span><i class='vis vis-open-file' title='" + VIS.Msg.getMsg("VA009_Batch") + "' id='" + 'VA009_Batchimg_' + $self.windowNo + "' ></i></span><span><i class='vis vis-split' title='" + VIS.Msg.getMsg("VA009_Split") + "'  id='" + 'VA009_Splitimg_' + $self.windowNo + "'></i></span><span><i class='vis vis-mobile-card' title='" + VIS.Msg.getMsg("VA009_PayMannual") + "'  id='" + 'VA009_PayMannualimg_' + $self.windowNo + "'></i></span><span><i class='vis vis-bank-transfer' title='" + VIS.Msg.getMsg("VA009_BankToBankTransfer") + "'  id='" + 'VA009_BankToBankimg_' + $self.windowNo + "'></i></span></div>";
            //End Icon's Div

            MainRoot += _MiddlePanel;
            MainRoot += '</div>';

            // Add Search
            MainRoot += '  <div class="VA009-mid-search" id="VA009-mid-search_' + $self.windowNo + '">';
            MainRoot += ' <div class="VA009-midleftsecwrap"><div class="VA009-selectall"><input type="checkbox" id="VA009_SelectAll_' + $self.windowNo + '"> <label>' + VIS.Msg.getMsg("VA009_HeaderSelectAll") + '</label></div>';
            MainRoot += ' <div class="VA009-selectall VA009-total-textDiv"><label style=" font-weight: bold; text-decoration: underline; ">' + VIS.Msg.getMsg("VA009_TotlAmtBase") + ' </label><label id="VA009_TotalSelected_' + $self.windowNo + '"" style="font-weight: bold;"> 0 </label></div></div>';
            MainRoot += '<div class="VA009-search-wrap"> <input value="" placeholder="Search..." type="text" id=' + "VA009_SrchTxtbx_" + $self.windowNo + '> <a class="VA009-search-icon" id=' + "VA009_SrchBtn_" + $self.windowNo + '><span class="vis vis-search"></span></a> </div>';

            //Add Selected Payment
            MainRoot += '</div>';// end of mid-search
            //Payments Container
            MainRoot += ' <div class="VA009-payment-list" id = ' + "VA009_Paymntlst_" + $self.windowNo + '>';

            MainRoot += '</div>';//end of payment-list
            MainRoot += '</div>'; //End  middle-wrap
            // Add Right Panel
            MainRoot += ' <div class="VA009-right-wrap">';
            //Add Tabs
            // Added new cashbook Tab as sggested by Mukesh Sir
            MainRoot += '<div class="VA009-rightsectopwrap"><div class="VA009-tabs-wrap"> <ul class="VA009-right-tabs"> <li class="VA009-active-tab" id=' + "VA009_Funds_" + $self.windowNo + '>' + VIS.Msg.getMsg("VA009_Bank") + '</li><li id=' + "VA009_Cashbooks_" + $self.windowNo + '>' + VIS.Msg.getMsg("VA009_Cashbook") + '</li></ul> </div>';
            MainRoot += '<div class="VA009-icons-wrap"> <ul class="VA009-right-tabs"> <li class="VA009-active-tab" id = ' + "VA009_Refresh_" + $self.windowNo + '>' + VIS.Msg.getMsg("VA009_Refresh") + '</li></ul> </div></div>';

            MainRoot += ' <div class="VA009-right-content" id = ' + "VA009_Banklst_" + $self.windowNo + '>';

            //End Right panel
            MainRoot += '</div></div></td></tr></table>';// End Of Content area
            $root.append(MainRoot); // Append Middle Panel In Root
            // End Root
        };
        //******************
        //Find Controls through ID
        //******************
        function GetControls() {
            $Org = $root.find("#VA009_Org_" + $self.windowNo);
            $OrgSelected = $root.find("#VA009_OrgdataDiv_" + $self.windowNo);
            $payMthd = $root.find("#VA009_PayMthd_" + $self.windowNo);
            $PayMSelected = $root.find("#VA009_PayMthddataDiv_" + $self.windowNo);
            $status = $root.find("#VA009_Status_" + $self.windowNo);
            $statusSelected = $root.find("#VA009_StatusdataDiv_" + $self.windowNo);
            $togglebtn = $root.find("#VA009_toggleImg_" + $self.windowNo);
            lbdata = $root.find("#VA009_leftpartdata_" + $self.windowNo);
            $lbmain = $root.find("#VA009_leftpart_" + $self.windowNo);
            $divPayment = $root.find("#VA009_Paymntlst_" + $self.windowNo);
            $divBank = $root.find("#VA009_Banklst_" + $self.windowNo);
            $CR_Tab = $root.find("#VA009_CR_Tab_" + $self.windowNo);
            $CP_Tab = $root.find("#VA009_CP_Tab_" + $self.windowNo);
            $XML_Tab = $root.find("#VA009_XML_Tab_" + $self.windowNo);
            _SelectedOrgDiv = $root.find("#VA009_OrgdataDiv_" + $self.windowNo);
            $BP = $root.find("#VA009_BPartner_" + $self.windowNo);
            $BPSelected = $root.find("#VA009_BPdataDiv_" + $self.windowNo);
            $SelectedDiv = $root.find("#VA009_Selectedtxt_" + $self.windowNo);
            $chkicon = $root.find("#VA009_Chkimg_" + $self.windowNo);
            $cashicon = $root.find("#VA009_Cashimg_" + $self.windowNo);
            $batchicon = $root.find("#VA009_Batchimg_" + $self.windowNo);
            $Spliticon = $root.find("#VA009_Splitimg_" + $self.windowNo);
            $PayMannualicon = $root.find("#VA009_PayMannualimg_" + $self.windowNo);
            //Bank To Bank transfer
            $PayBankToBank = $root.find("#VA009_BankToBankimg_" + $self.windowNo);
            //End
            $table = $root.find("#VA009_table_" + $self.windowNo);
            tableleft = $root.find("#VA009_tdLeft_" + $self.windowNo);
            $SrchTxtBox = $root.find("#VA009_SrchTxtbx_" + $self.windowNo);
            $SrchBtn = $root.find("#VA009_SrchBtn_" + $self.windowNo);
            $Duedateul = $root.find("#VA009_DueDate_" + $self.windowNo);
            $DueDateSelected = $root.find("#VA009_DueDatedataDiv_" + $self.windowNo);

            //change by amit - 16-nov-2016
            $TransactionType = $root.find("#VA009_TransactionType_" + $self.windowNo);
            $TransactionTypeSelected = $root.find("#VA009_TransactionTypeDiv_" + $self.windowNo);
            //end

            $FromDate = $root.find("#VA009_FromDate_" + $self.windowNo);
            $ToDate = $root.find("#VA009_ToDate_" + $self.windowNo);
            // Changes by Bharat on 29 April 2017
            lbdata.height($lbmain.height() - (43 + 20));
            //end

            //changes by manjot on 11 Nov 2017
            $selectall = $root.find("#VA009_SelectAll_" + $self.windowNo);
            $refresh = $root.find("#VA009_Refresh_" + $self.windowNo);

            // to implement cashbook tab
            $tabCashbook = $root.find("#VA009_Cashbooks_" + $self.windowNo);
            $tabFunds = $root.find("#VA009_Funds_" + $self.windowNo);

            $totalAmt = $root.find("#VA009_TotalSelected_" + $self.windowNo);

        };

        /* VIS_427 DevOps id:2247 Handeled amount issue when user unselect the Business Partner 
         From left pannel*/
        function BpPartnerClear() {
            /*VIS_427 DevOps id:2247 Adding those Business partner records which are unselected from 
              left div*/
            SlctdBpId = jQuery.grep(BusinessPartnerIds, function (value) {
                return value.BP.bpid == BP_id;
            });
            var amt = VIS.Utility.Util.getValueOfDecimal($totalAmt.data('ttlamt')).toFixed(2);
            for (var i = 0; i < SlctdBpId.length; i++) {
                var DeslctPaymt_ID = SlctdBpId[i].BP.uid;
                //removing records after unselecting the business partner
                BusinessPartnerIds = jQuery.grep(BusinessPartnerIds, function (value) {
                    return value.BP.uid != DeslctPaymt_ID;
                });
                SlctdPaymentIds = jQuery.grep(SlctdPaymentIds, function (value) {
                    return value != DeslctPaymt_ID;
                });
                SlctdOrderPaymentIds = jQuery.grep(SlctdOrderPaymentIds, function (value) {
                    return value != DeslctPaymt_ID;
                });
                SlctdJournalPaymentIds = jQuery.grep(SlctdJournalPaymentIds, function (value) {
                    return value != DeslctPaymt_ID;
                });
                batchObjInv = jQuery.grep(batchObjInv, function (value) {
                    return value.ID != DeslctPaymt_ID;
                });
                batchObjOrd = jQuery.grep(batchObjOrd, function (value) {
                    return value.ID != DeslctPaymt_ID;
                });
                batchObjJournal = jQuery.grep(batchObjJournal, function (value) {
                    return value.ID != DeslctPaymt_ID;
                });
                var baseAmt = SlctdBpId[i].BP.baseamt;
                amt = amt - baseAmt;
            }
            $totalAmt.data('ttlamt', parseFloat(amt, 2));
            $totalAmt.text(getFormattednumber(amt, 2));
            SlctdBpId = []; //Clearing the array 
            $('.VA009-payment-list').find('div .row').find('input[data-bpid=' + BP_id + ']').prop('checked', false);
            isReset = true;
        }
        //******************
        //EventHandling
        //******************
        function InitializeEvents() {
            $Org.on("change", function () {
                if ($Org.val() > 0) {
                    if ($OrgSelected.find("li").length > 0) // For Continues Selection
                    {
                        if (orgids.indexOf($Org.val()) == -1) {
                            $OrgSelected.find("ul").append('<li id=' + "VA009_li_" + $Org.val() + '><i title="' + VIS.Msg.getMsg("VA009_Cancel") + '" class="VA009-cross-btn vis vis-mark" id=' + "VA009_Delimg_" + $Org.val() + '></i><span>' + VIS.Utility.encodeText($Org.children()[$Org[0].selectedIndex].text) + '</span></li>'); // Append li in UI
                            orgids.push($Org.val()); // Add Values in Array For Search
                            whereClause("cs.AD_Org_ID", orgids);
                        }
                        else
                            VIS.ADialog.info("VA009_AlrdySlctd");
                    }
                    else {
                        $OrgSelected.append('<ul><li id=' + "VA009_li_" + $Org.val() + '><i title="' + VIS.Msg.getMsg("VA009_Cancel") + '" class="VA009-cross-btn vis vis-mark" id=' + "VA009_Delimg_" + $Org.val() + '></i> <span>' + VIS.Utility.encodeText($Org.children()[$Org[0].selectedIndex].text) + '</span></li>');
                        orgids.push($Org.val());
                        whereClause("cs.AD_Org_ID", orgids);
                    }
                }

                $Org.prop('selectedIndex', 0);
                $divPayment.find('.VA009-payment-wrap').remove();
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; batchObjInv = []; batchObjOrd = []; SlctdJournalPaymentIds = []; batchObjJournal = [];
                resetPaging();
                // loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
                loadPaymetsAll();
                clearamtid();
            });
            $OrgSelected.on("click", function (e) {

                if (e.target.localName == "i") {
                    var Remove = "#" + e.target.id;
                    $(Remove).parent().remove(); // remove li from div

                    var Org_id = e.target.id.slice(e.target.id.lastIndexOf("_") + 1, e.target.id.length);
                    orgids = jQuery.grep(orgids, function (value) {
                        return value != Org_id;
                    });// remove org fro array

                    if ($OrgSelected.find("li").length > 0) // checking for li in selected div of org
                        whereClause("cs.AD_Org_ID", orgids);
                    else {
                        _WhrOrg = "";
                        $OrgSelected.find("ul").remove();
                    }
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; batchObjInv = []; batchObjOrd = []; SlctdJournalPaymentIds = []; batchObjJournal = [];
                    resetPaging();
                    loadPaymetsAll();
                    clearamtid();
                }
            });

            $BPSelected.on("click", function (e) {
                if (e.target.localName == "i") {
                    var Remove = "#" + e.target.id;
                    $(Remove).parent().remove(); // remove li from div

                    BP_id = e.target.id.slice(e.target.id.lastIndexOf("_") + 1, e.target.id.length);
                    bpids = jQuery.grep(bpids, function (value) {
                        return value != BP_id;
                    }); // remove org fro array

                    if ($BPSelected.find("li").length > 0) // checking for li in selected div of org
                        whereClause("cb.C_BPartner_ID", bpids);
                    else {
                        _Whr_BPrtnr = "";
                        $BPSelected.find("ul").remove();
                    }
                    BpPartnerClear(); // VIS_427 DevOps id: 2247 Called function
                    //VIS_427 Handelled amount issue when user select record and click on payment list div
                    var amt = VIS.Utility.Util.getValueOfDecimal($totalAmt.data('ttlamt')).toFixed(2);
                    if (amt == 0.00) {
                        $totalAmt.text(0);
                        $totalAmt.data('ttlamt', parseFloat(0));
                    }
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1; SelectallInvIds = []; SelectallOrdIds = []; SelectallJournalIds = [];
                    //SlctdPaymentIds = []; SlctdOrderPaymentIds = []; batchObjInv = []; batchObjOrd = []; SlctdJournalPaymentIds = []; batchObjJournal = [];
                    resetPaging();
                    loadPaymetsAll();
                    //clearamtid();
                }
            });

            $payMthd.on("change", function () {
                if ($PayMSelected.find("li").length > 0) // For Continues Selection
                {
                    if (paymntIds.indexOf($payMthd.val()) == -1) {
                        $PayMSelected.find("ul").append('<li ><i title="' + VIS.Msg.getMsg("VA009_Cancel") + '" class="VA009-cross-btn vis vis-mark" id=' + "VA009_DelPayimg_" + $payMthd.val() + '> </i><span>' + VIS.Utility.encodeText($payMthd.children()[$payMthd[0].selectedIndex].text) + '</span></li>');
                        paymntIds.push($payMthd.val());
                        whereClause("pm.VA009_PaymentMethod_ID", paymntIds);
                    }
                    else
                        VIS.ADialog.info("VA009_AlrdySlctd");
                }
                else {
                    $PayMSelected.append('<ul id=' + "VA009_PayMthUL_" + $payMthd.val() + ' ><li ><i title="' + VIS.Msg.getMsg("VA009_Cancel") + '" class="VA009-cross-btn vis vis-mark" id=' + "VA009_DelPayimg_" + $payMthd.val() + '></i> <span>' + VIS.Utility.encodeText($payMthd.children()[$payMthd[0].selectedIndex].text) + '</span></li> </ul>');
                    paymntIds.push($payMthd.val());
                    whereClause("pm.VA009_PaymentMethod_ID", paymntIds);
                }
                $payMthd.prop('selectedIndex', 0);
                $divPayment.find('.VA009-payment-wrap').remove();
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; batchObjInv = []; batchObjOrd = []; SlctdJournalPaymentIds = []; batchObjJournal = [];
                resetPaging();
                loadPaymetsAll();
                clearamtid();
            });
            $PayMSelected.on("click", function (e) {

                if (e.target.localName == "i") {
                    var Remove = "#" + e.target.id;
                    $(Remove).parent().remove(); // remove li from ul

                    var Paymt_ID = e.target.id.slice(e.target.id.lastIndexOf("_") + 1, e.target.id.length);
                    paymntIds = jQuery.grep(paymntIds, function (value) {
                        return value != Paymt_ID;
                    });// Remove PaymtID From Array

                    if ($PayMSelected.find("li").length > 0) // checking for li in selected div of org
                        whereClause("pm.VA009_PaymentMethod_ID", paymntIds);
                    else {
                        _WhrPayMtd = "";
                        $PayMSelected.find("ul").remove();
                    }
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; batchObjInv = []; batchObjOrd = []; SlctdJournalPaymentIds = []; batchObjJournal = [];
                    resetPaging();
                    loadPaymetsAll();
                    clearamtid(); //calling function to clear the total amt
                }
            });

            $status.on("change", function () {
                if ($statusSelected.find("li").length > 0) // For Continues Selection
                {
                    if (statusIds.indexOf($status.val()) == -1) {
                        $statusSelected.find("ul").append('<li ><i title="' + VIS.Msg.getMsg("VA009_Cancel") + '" class="VA009-cross-btn vis vis-mark" id=' + "VA009_DelStaimg_" + $status.val() + '></i> <span>' + VIS.Utility.encodeText($status.children()[$status[0].selectedIndex].text) + '</span></li>');
                        statusIds.push($status.val()); // Add Values in Array For Search
                        whereClause("cs.VA009_ExecutionStatus", statusIds);
                    }
                    else
                        VIS.ADialog.info("VA009_AlrdySlctd");
                }
                else {
                    $statusSelected.append('<ul id=' + "VA009_StatusUL_" + $status.val() + ' ><li ><i title="' + VIS.Msg.getMsg("VA009_Cancel") + '" class="VA009-cross-btn vis vis-mark" id=' + "VA009_DelStaimg_" + $status.val() + '></i> <span>' + VIS.Utility.encodeText($status.children()[$status[0].selectedIndex].text) + '</span></li> </ul>');
                    statusIds.push($status.val());
                    whereClause("cs.VA009_ExecutionStatus", statusIds);
                }
                $status.prop('selectedIndex', 0);
                $divPayment.find('.VA009-payment-wrap').remove();
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; batchObjInv = []; batchObjOrd = []; SlctdJournalPaymentIds = []; batchObjJournal = [];
                resetPaging();
                loadPaymetsAll();
                clearamtid(); //calling function to clear the total amt
            });
            $statusSelected.on("click", function (e) {

                if (e.target.localName == "i") {
                    var Remove = "#" + e.target.id;
                    $(Remove).parent().remove(); // Remove li from ul

                    var Status_id = e.target.id.slice(e.target.id.lastIndexOf("_") + 1, e.target.id.length);
                    statusIds.splice(statusIds.indexOf(Status_id), 1); // remove status fro array

                    if ($statusSelected.find("li").length > 0) // checking for li in selected div of org
                        whereClause("cs.VA009_ExecutionStatus", statusIds);
                    else {
                        _WhrStatus = "";
                        $statusSelected.find("ul").remove();
                    }
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; batchObjInv = []; batchObjOrd = []; SlctdJournalPaymentIds = []; batchObjJournal = [];
                    resetPaging();
                    loadPaymetsAll();
                    clearamtid(); //calling function to clear the total amt
                }
            });
            // by Amit - 16-11-2016
            $TransactionType.on("change", function () {
                if ($TransactionTypeSelected.find("li").length > 0) // For Continues Selection
                {
                    if (transtypes.indexOf($TransactionType.val()) == -1) {
                        $TransactionTypeSelected.find("ul").append('<li ><i title="' + VIS.Msg.getMsg("VA009_Cancel") + '" class="VA009-cross-btn vis vis-mark" id=' + "VA009_DelStaimg_" + $TransactionType.val() + '></i> <span>' + VIS.Utility.encodeText($TransactionType.children()[$TransactionType[0].selectedIndex].text) + '</span></li>');
                        transtypes.push($TransactionType.val()); // Add Values in Array For Search
                    }
                    else
                        VIS.ADialog.info("VA009_AlrdySlctd");
                }
                else {
                    // Remove unwanted ul
                    $TransactionTypeSelected.find("ul").remove();

                    $TransactionTypeSelected.append('<ul id=' + "VA009_StatusUL_" + $TransactionType.val() + ' ><li ><i title="' + VIS.Msg.getMsg("VA009_Cancel") + '" class="VA009-cross-btn vis vis-mark" id=' + "VA009_DelStaimg_" + $TransactionType.val() + '></i> <span>' + VIS.Utility.encodeText($TransactionType.children()[$TransactionType[0].selectedIndex].text) + '</span></li> </ul>');
                    transtypes.push($TransactionType.val());
                }
                $TransactionType.prop('selectedIndex', 0);
                $divPayment.find('.VA009-payment-wrap').remove();
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; batchObjInv = []; batchObjOrd = []; SlctdJournalPaymentIds = []; batchObjJournal = [];
                resetPaging();
                _WhrTransType = "";
                for (var i in transtypes) {
                    if (i != 0) {
                        _WhrTransType += ",";
                    }
                    _WhrTransType += "" + transtypes[i] + "";
                }
                loadPaymetsAll();
                clearamtid(); //calling function to clear the total amt
            });
            $TransactionTypeSelected.on("click", function (e) {

                if (e.target.localName == "i") {
                    var Remove = "#" + e.target.id;
                    $(Remove).parent().remove(); // Remove li from ul

                    var transactiontype_id = e.target.id.slice(e.target.id.lastIndexOf("_") + 1, e.target.id.length);
                    transtypes.splice(transtypes.indexOf(transactiontype_id), 1); // remove status fro array
                    _WhrTransType = "";
                    for (var i in transtypes) {
                        if (i != 0) {
                            _WhrTransType += ",";
                        }
                        _WhrTransType += "" + transtypes[i] + "";
                    }

                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; batchObjInv = []; batchObjOrd = []; SlctdJournalPaymentIds = []; batchObjJournal = [];
                    resetPaging();
                    loadPaymetsAll();
                    clearamtid(); //calling function to clear the total amt
                }
            });
            //end

            //used blur incase of change to avoid cal the event immediatly when press change the single character
            $FromDate.on("blur", function () {
                if (Date.parse($FromDate.val()) < 0) {
                    //if user not entered proper Date format 
                    VIS.ADialog.info("VA009_PlzSelectProperDate");
                    return;
                }
                if ($FromDate.val() == "") {
                    //(1052)if date is empty or half entered then clear the field
                    $FromDate.val("");
                    return;
                }
                $divPayment.find('.VA009-payment-wrap').remove();
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; batchObjInv = []; batchObjOrd = []; SlctdJournalPaymentIds = []; batchObjJournal = [];
                resetPaging();
                loadPaymetsAll();
                clearamtid(); //calling function to clear the total amt
            });

            //used blur incase of change to avoid cal the event immediatly when press change the single character
            $ToDate.on("blur", function () {
                if (Date.parse($ToDate.val()) < 0) {
                    //if user not entered proper Date format 
                    VIS.ADialog.info("VA009_PlzSelectProperDate");
                    return;
                }
                if ($ToDate.val() == "") {
                    //(1052)if date is empty or half entered then clear the field
                    $ToDate.val("");
                    return;
                }
                $divPayment.find('.VA009-payment-wrap').remove();
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; batchObjInv = []; batchObjOrd = []; SlctdJournalPaymentIds = []; batchObjJournal = [];
                resetPaging();
                loadPaymetsAll();
                clearamtid(); //calling function to clear the total amt
            });

            $divPayment.on("scroll", paymentScroll);

            //click event for payment records
            $divPayment.on("click", paymentContainerClick);

            $togglebtn.on("click", function (e) {
                e.stopPropagation();
                var w = tableleft.width();
                if (w > 50) {
                    $(lbdata).hide();
                }

                tableleft.animate({
                    "width": w > 50 ? 40 : 200
                }, 300, 'swing', function () {

                    if (w < 51) {
                        $(lbdata).show();
                    }
                });
            });

            $chkicon.on("click", function (e) {
                if ($CP_Tab.hasClass('VA009-active-tab')) {
                    if (SlctdPaymentIds.length > 0 || SlctdOrderPaymentIds.length > 0 || SlctdJournalPaymentIds.length > 0) {
                        //Rakesh(VA228):Set target base type
                        //AP Payment(APP)
                        _TargetBaseType = 2;
                        _loadFunctions.Cheque_Pay_Dialog();
                    }
                    else
                        VIS.ADialog.info("VA009_PlzSelct1Pay");
                }
                else if ($CR_Tab.hasClass('VA009-active-tab')) {
                    if (SlctdPaymentIds.length > 0 || SlctdOrderPaymentIds.length > 0 || SlctdJournalPaymentIds.length > 0) {
                        //AR Receipt (ARR)
                        _TargetBaseType = 1;
                        _loadFunctions.Cheque_Rec_Dialog();
                    }
                    else
                        VIS.ADialog.info("VA009_PlzSelct1Pay");
                }
                else
                    VIS.ADialog.info(_iscustomer + '/' + _isvendor);
            });

            $cashicon.on("click", function (e) {
                //VA230:Select either Invoice or Order schedule only
                if (SlctdPaymentIds.length > 0 && SlctdOrderPaymentIds.length > 0) {
                    VIS.ADialog.info("VA009_SelectEitherInvoiceOrOrderSchedule");
                }
                else if (SlctdJournalPaymentIds.length > 0) {
                    VIS.ADialog.info("VA009_JournalRecordCantSelected");
                }
                else if (SlctdPaymentIds.length > 0 || SlctdOrderPaymentIds.length > 0) {
                    //Cash Journal(CMC)
                    _TargetBaseType = 3;
                    _loadFunctions.Cash_Dialog();
                }
                else
                    VIS.ADialog.info("VA009_PlzSelct1Pay");
            });

            $batchicon.on("click", function (e) {
                //Batch Payment(BAP)
                _TargetBaseType = 4;
                _loadFunctions.Batch_OpenDialog();
            });

            $Spliticon.on("click", function (e) {
                if (SlctdJournalPaymentIds.length > 0) {
                    VIS.ADialog.info("VA009_JournalRecordCantSplitted");
                }
                else if (SlctdPaymentIds.length > 0 || SlctdOrderPaymentIds.length > 0) {
                    if (SlctdPaymentIds.length == 1 && SlctdOrderPaymentIds.length == 0) {
                        _loadFunctions.Split_Dialog();
                    }
                    else if (SlctdPaymentIds.length == 0 && SlctdOrderPaymentIds.length == 1) {
                        _loadFunctions.Split_Dialog();
                    }
                    else {
                        VIS.ADialog.info("VA009_PlzSelctOnly1Pay");
                    }
                }
                else {
                    VIS.ADialog.info("VA009_PlzSelct1Pay");
                }
            });

            $PayMannualicon.on("click", function (e) {
                if (SlctdPaymentIds.length > 0 || SlctdOrderPaymentIds.length > 0 || SlctdJournalPaymentIds.length > 0) {
                    if ($CR_Tab.hasClass('VA009-active-tab')) {
                        _TargetBaseType = 1;
                    }
                    else {
                        _TargetBaseType = 2;
                    }
                    _loadFunctions.Pay_ManualDialog();
                }
                else { //worked for manual payment when user click without selecting any schedule.
                    _loadFunctions.Pay_ManualDialogBP();
                }
            });

            //Bank TO Bank
            $PayBankToBank.on("click", function (e) {
                //bug176:for bank to bank transfer if any schedule is selected then donot let open the dialog 
                if (SlctdPaymentIds.length > 0 || SlctdOrderPaymentIds.length > 0 || SlctdJournalPaymentIds.length > 0) {
                    VIS.ADialog.info("VA009_PlzNotSelctSch");
                }
                else {
                    _loadFunctions.B2B_Dialog();
                }
            });

            $Duedateul.on("change", function (e) {

                if ($DueDateSelected.find("li").length > 0) // For Continues Selection
                {
                    if (Duedateval.indexOf($Duedateul.val()) == -1) {
                        $DueDateSelected.find("ul").append('<li ><i title="' + VIS.Msg.getMsg("VA009_Cancel") + '" class="VA009-cross-btn vis vis-mark" id=' + "VA009_DelDueimg_" + $Duedateul.val() + '></i> <span>' + VIS.Utility.encodeText($Duedateul.children()[$Duedateul[0].selectedIndex].text) + '</span></li>');
                        Duedateval.push($Duedateul.val());
                        whereClause("DueDate", Duedateval);
                    }
                    else
                        VIS.ADialog.info("VA009_AlrdySlctd");
                }
                else {
                    $DueDateSelected.append('<ul id=' + "VA009_PayMthUL_" + $Duedateul.val() + ' ><li ><i title="' + VIS.Msg.getMsg("VA009_Cancel") + '" class="VA009-cross-btn vis vis-mark" id=' + "VA009_DelDueimg_" + $Duedateul.val() + '></i> <span>' + VIS.Utility.encodeText($Duedateul.children()[$Duedateul[0].selectedIndex].text) + '</span></li> </ul>');
                    Duedateval.push($Duedateul.val());
                    whereClause("DueDate", Duedateval);
                }
                $Duedateul.prop('selectedIndex', 0);
                $divPayment.find('.VA009-payment-wrap').remove();
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; SlctdJournalPaymentIds = []; batchObjInv = []; batchObjOrd = []; batchObjJournal = [];
                resetPaging();
                loadPaymetsAll();
                clearamtid(); //calling function to clear the total amt
            });

            $DueDateSelected.on("click", function (e) {

                if (e.target.localName == "i") {
                    var Remove = "#" + e.target.id;
                    $(Remove).parent().remove(); // remove li from div

                    var Due_id = e.target.id.slice(e.target.id.lastIndexOf("_") + 1, e.target.id.length);
                    Duedateval = jQuery.grep(Duedateval, function (value) {
                        return value != Due_id;
                    });// remove org fro array

                    if ($DueDateSelected.find("li").length > 0) // checking for li in selected div of org
                        whereClause("DueDate", Duedateval);
                    else {
                        DueDateSelected = 99;
                        $DueDateSelected.find("ul").remove();
                    }
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1;
                    resetPaging();
                    loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
                }
            });

            $SrchTxtBox.on("keypress", function (e) {
                if (e.keyCode == 13) {
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1;
                    resetPaging();
                    loadPaymetsAll();
                    SelectallJournalIds = [];
                    SelectallInvIds = [];
                    SelectallOrdIds = [];
                    /* VIS_427 DevOps id: 2246 Commented The line In order to
                     get schedules according to Searched Value*/
                    // $SrchTxtBox.val('');
                    isReset = true; /*VIS_427 DevOps id:2238 Assigned value as true to mark chekbox checked*/
                }
            });

            $SrchBtn.on("click", function () {
                $divPayment.find('.VA009-payment-wrap').remove();
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                pgNo = 1;
                resetPaging();
                loadPaymetsAll();
                SelectallJournalIds = [];
                SelectallInvIds = [];
                SelectallOrdIds = [];
                /* VIS_427 DevOps id: 2246 Commented The line In order to
                    get schedules according to Searched Value*/
                // $SrchTxtBox.val('');
                isReset = true; /*VIS_427 DevOps id:2238 Assigned value as true to mark chekbox checked*/
            });

            $CR_Tab.on("click", function (e) {
                $divPayment.find('.VA009-payment-wrap').remove();
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                _DocType = 'ARI'; _isinvoice = 'Y';
                pgNo = 1;
                resetPaging();
                loadPaymetsAll();
                changeTabactive($CR_Tab);
                ClearEasySearch();
            });

            $CP_Tab.on("click", function (e) {
                $divPayment.find('.VA009-payment-wrap').remove();
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                pgNo = 1;
                _DocType = 'API'; _isinvoice = 'Y';
                pgNo = 1;
                resetPaging();
                loadPaymetsAll();
                changeTabactive($CP_Tab);
                ClearEasySearch();
            });

            $XML_Tab.on("click", function (e) {
                changeTabactive($XML_Tab);
                _loadFunctions.XML_Dialog();
            });

            $selectall.on("click", function (e) {
                var target = $(e.target);
                $totalAmt.text(0);
                $totalAmt.data('ttlamt', parseFloat(0));
                if (e.target.type == 'checkbox') {
                    if (target.prop("checked") == true) {
                        $divPayment.find(':checkbox').not(":disabled").prop('checked', true);
                        var v = [];
                        //clear the records from the array
                        SlctdPaymentIds = [];
                        SlctdOrderPaymentIds = [];
                        SlctdJournalPaymentIds = [];
                        BusinessPartnerIds = [];
                        batchObjInv = [];
                        batchObjOrd = [];
                        batchObjJournal = [];
                        for (var i = 0; i < SelectallInvIds.length; i++) {
                            v.target = $($divPayment.find('.VA009-payment-wrap').find('.VA009-clckd-checkbx[data-uid^=' + SelectallInvIds[i] + ']'))[0];
                            if (!v.target == false) {
                                if (!$(v.target).prop("disabled")) {
                                    paymentContainerClick(v);
                                }
                            }
                        }
                        for (var j = 0; j < SelectallOrdIds.length; j++) {
                            v.target = $($divPayment.find('.VA009-payment-wrap').find('.VA009-clckd-checkbx[data-uid^=' + SelectallOrdIds[j] + ']'))[0];
                            if (!v.target == false)
                                paymentContainerClick(v);
                        }
                        for (var j = 0; j < SelectallJournalIds.length; j++) {
                            v.target = $($divPayment.find('.VA009-payment-wrap').find('.VA009-clckd-checkbx[data-uid^=' + SelectallJournalIds[j] + ']'))[0];
                            if (!v.target == false)
                                paymentContainerClick(v);
                        }
                    }
                    else {
                        SlctdPaymentIds = [];
                        SlctdOrderPaymentIds = [];
                        SlctdJournalPaymentIds = [];
                        BusinessPartnerIds = [];
                        $divPayment.find(':checkbox').prop('checked', false);
                        $selectall.find(':checkbox').prop('checked', false);
                        //removing the background color for unselected records
                        $divPayment.find('.VA009-payment-wrap').removeClass("VA009-payment-wrap-selctd");
                        $totalAmt.text(0);
                        $totalAmt.data('ttlamt', parseFloat(0));
                    }
                }

            });

            $refresh.on("click", function (e) {
                $divPayment.find('.VA009-payment-wrap').remove();
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                $tabCashbook.removeClass("VA009-active-tab");
                $tabFunds.addClass("VA009-active-tab");
                _DocType = 'ARI'; _isinvoice = 'Y';
                pgNo = 1;
                resetPaging();
                loadPaymetsAll();
                changeTabactive($CR_Tab);
                ClearEasySearch();
            });
            // click event of cashbook Tab and Funds Tab as sggested by Mukesh Sir
            $tabCashbook.on("click", function (e) {
                $tabCashbook.removeClass("VA009-active-tab");
                $tabFunds.removeClass("VA009-active-tab");
                $tabCashbook.addClass("VA009-active-tab");
                $divBank.find('.VA009-right-data-main').hide();
                $divBank.find('.VA009-accordion').hide();
                $divcashbk = $root.find("#VA009_cashbkdiv_" + $self.windowNo);
                $divcashbk.show();
            });

            $tabFunds.on("click", function (e) {
                $tabCashbook.removeClass("VA009-active-tab");
                $tabFunds.removeClass("VA009-active-tab");
                $tabFunds.addClass("VA009-active-tab");
                $divBank.find('.VA009-right-data-main').show();
                $divBank.find('.VA009-accordion').show();
                $divcashbk = $root.find("#VA009_cashbkdiv_" + $self.windowNo);
                $divcashbk.hide();
            });
        };
        //******************************
        //Change Tab Active Stage
        //******************************
        function changeTabactive($divid) {
            $CR_Tab.removeClass("VA009-active-tab");
            $CP_Tab.removeClass("VA009-active-tab");
            $XML_Tab.removeClass("VA009-active-tab");
            $divid.addClass("VA009-active-tab");
            $selectall.prop('checked', false);
            SlctdPaymentIds = [];
            SlctdOrderPaymentIds = [];
            SlctdJournalPaymentIds = [];
            //$totalAmt.text(0);
            //$totalAmt.data('ttlamt', parseFloat(0));
            clearamtid(); //calling function to clear the total amt
        };
        //******************
        //EventHandling
        //******************
        function paymentContainerClick(e) {

            var chk;
            var target = $(e.target);
            if (target.hasClass('vis vis-edit')) {
                Pay_ID = target.data("uid");
                var InvID = target.data("invoiceid");
                var TransactionType = target.data("transactiontype");

                if (TransactionType == "Invoice") {
                    if ($CR_Tab.hasClass('VA009-active-tab')) {
                        zoomToWindow(InvID, "Invoice (Customer)", "C_Invoice_ID");
                    }
                    else {
                        zoomToWindow(InvID, "Invoice (Vendor)", "C_Invoice_ID");
                    }
                }
                else if (TransactionType == "Order") {
                    if ($CR_Tab.hasClass('VA009-active-tab')) {
                        zoomToWindow(InvID, "Sales Order", "C_Order_ID");
                    }
                    else {
                        zoomToWindow(InvID, "Purchase Order", "C_Order_ID");
                    }
                }
                else if (TransactionType == "GL Journal") {
                    zoomToWindow(InvID, "GL Journal Line", "GL_Journal_ID");
                }
                Pay_ID = 0;
            }
            else if (e.target.type == 'checkbox') {
                if (target.prop("checked") == true) {
                    //added css for selected record
                    target.parents(".VA009-payment-wrap").addClass("VA009-payment-wrap-selctd");
                    //Edit By Amit - 18-11-2016
                    if (target.data("name") == "Invoice") {
                        SlctdPaymentIds.push(target.data("uid"));
                        BusinessPartnerIds.push({ "BP": target.data() }); //VIS_427 Adding the records into array on selection
                        batchObjInv.push({ "ID": target.data("uid"), "PM": target.data() });
                    }
                    else if (target.data("name") == "Order") {
                        SlctdOrderPaymentIds.push(target.data("uid"));
                        BusinessPartnerIds.push({ "BP": target.data() }); //VIS_427 Adding the records into array on selection
                        batchObjOrd.push({ "ID": target.data("uid"), "PM": target.data() });
                    }
                    else {
                        SlctdJournalPaymentIds.push(target.data("uid"));
                        BusinessPartnerIds.push({ "BP": target.data() }); //VIS_427 Adding the records into array on selection
                        batchObjJournal.push({ "ID": target.data("uid"), "PM": target.data() });
                    }
                    //when max payment records selected at that time need to checkall checkbox true.
                    var selRecords = SlctdPaymentIds.length + SlctdOrderPaymentIds.length + SlctdJournalPaymentIds.length;
                    var countRecords = $($divPayment.find(".VA009-payment-wrap")).length;
                    //VIS_427 Handeled issue for select all check box
                    var checkedrecords = $(".VA009-payment-list").find("input[type=checkbox]:checked").length;
                    var uncheckedrecords = $(".VA009-payment-list").find("input[type=checkbox]:not(:checked)").length;
                    if ((countRecords == selRecords && uncheckedrecords == 0) || (countRecords == checkedrecords)) {
                        $selectall.prop('checked', true);
                    }
                    record_ID = target.data("uid");
                    /* VIS_427 DevOps id:2289 getting value according to precision */
                    precision = target.data("precision");
                    var amt = VIS.Utility.Util.getValueOfDecimal($totalAmt.text()).toFixed(precision);
                    amt = VIS.Utility.Util.getValueOfDecimal($totalAmt.data('ttlamt')).toFixed(precision);
                    var baseAmt = VIS.Utility.Util.getValueOfDecimal(target.data("baseamt")).toFixed(precision);
                    if (target.data("docbasetype") == "ARC" || target.data("docbasetype") == "APC") {
                        baseAmt = (-1 * baseAmt);
                    }
                    var actualAmt = VIS.Utility.Util.getValueOfDecimal(amt) + VIS.Utility.Util.getValueOfDecimal(baseAmt);
                    $totalAmt.data('ttlamt', parseFloat(actualAmt, precision));
                    $totalAmt.text(getFormattednumber(actualAmt, precision));
                }
                else {
                    $selectall.prop('checked', false);
                    //removing background color when checkbox false
                    target.parents('.VA009-payment-wrap').removeClass("VA009-payment-wrap-selctd");
                    var DeslctPaymt_ID = target.data("uid");
                    SlctdPaymentIds = jQuery.grep(SlctdPaymentIds, function (value) {
                        return value != DeslctPaymt_ID;
                    });
                    SlctdOrderPaymentIds = jQuery.grep(SlctdOrderPaymentIds, function (value) {
                        return value != DeslctPaymt_ID;
                    });
                    SlctdJournalPaymentIds = jQuery.grep(SlctdJournalPaymentIds, function (value) {
                        return value != DeslctPaymt_ID;
                    });
                    batchObjInv = jQuery.grep(batchObjInv, function (value) {
                        return value.ID != DeslctPaymt_ID;
                    });
                    batchObjOrd = jQuery.grep(batchObjOrd, function (value) {
                        return value.ID != DeslctPaymt_ID;
                    });
                    batchObjJournal = jQuery.grep(batchObjJournal, function (value) {
                        return value.ID != DeslctPaymt_ID;
                    });
                    BusinessPartnerIds = jQuery.grep(BusinessPartnerIds, function (value) {
                        return value.BP.uid != DeslctPaymt_ID;
                    });
                    record_ID = 0;
                    /* VIS_427 DevOps id:2289 getting value according to precision */
                    precision = target.data("precision");
                    var amt = VIS.Utility.Util.getValueOfDecimal($totalAmt.text()).toFixed(precision);
                    amt = VIS.Utility.Util.getValueOfDecimal($totalAmt.data('ttlamt')).toFixed(precision);
                    var baseAmt = VIS.Utility.Util.getValueOfDecimal(target.data("baseamt")).toFixed(precision);
                    if (target.data("docbasetype") == "ARC" || target.data("docbasetype") == "APC") {
                        baseAmt = (-1 * baseAmt);
                    }
                    var actualAmt = VIS.Utility.Util.getValueOfDecimal(amt) - VIS.Utility.Util.getValueOfDecimal(baseAmt);
                    $totalAmt.data('ttlamt', parseFloat(actualAmt, precision));
                    $totalAmt.text(getFormattednumber(actualAmt, precision));
                    //end
                }
            }
            else if (target.hasClass('VA009_AddNoteimg')) {

                record_ID = target.data("uid");
                _loadFunctions.ChatWindow();
            }
            //if (e.target.className == "VA009-info-icon") {
            else if (target.hasClass("VA009-info-icon")) {
                infoWinID = target.data("uid");
                zoomToWindow(infoWinID, "Business Partner Info", "C_BPartner_ID");
            }
            //handled record selection from div level
            //select/unselect the payment record when click on anywhere in the row div
            //handled when click on payment list div area with out having the records in that area
            else if (target.parents(".VA009-payment-wrap") && !target.is(".VA009-payment-list")) {
                //if user click on div class "VA009-payment-wrap" this condition will execute
                if (target.parents(".VA009-payment-wrap").find(".VA009-clckd-checkbx").prop("checked") == undefined) {
                    if (target.find(".VA009-clckd-checkbx").prop("checked")) {
                        $divPayment.find(':checkbox').not(":disabled").prop('checked', false);
                        $divPayment.find('.VA009-payment-wrap').removeClass("VA009-payment-wrap-selctd");
                        $selectall.prop('checked', false);
                        SlctdPaymentIds = [];
                        SlctdOrderPaymentIds = [];
                        SlctdJournalPaymentIds = [];
                        batchObjInv = [];
                        batchObjOrd = [];
                        batchObjJournal = [];
                        BusinessPartnerIds = [];
                        $totalAmt.text(0);
                        $totalAmt.data('ttlamt', parseFloat(0));
                    }
                    else {
                        $divPayment.find(':checkbox').not(":disabled").prop('checked', false);
                        $divPayment.find('.VA009-payment-wrap').removeClass("VA009-payment-wrap-selctd");
                        target.find(".VA009-clckd-checkbx").prop("checked", true);
                        target.addClass("VA009-payment-wrap-selctd");
                        var inputTag = target.find(".VA009-clckd-checkbx")[0];
                        record_ID = VIS.Utility.Util.getValueOfInt(inputTag.dataset["uid"]);
                        SlctdPaymentIds = [];
                        SlctdOrderPaymentIds = [];
                        SlctdJournalPaymentIds = [];
                        batchObjInv = [];
                        batchObjOrd = [];
                        batchObjJournal = [];
                        BusinessPartnerIds = [];
                        if (inputTag.dataset["name"] == "Invoice") {
                            SlctdPaymentIds.push(record_ID);
                            BusinessPartnerIds.push({ "BP": inputTag.dataset });
                            batchObjInv.push({ "ID": record_ID, "PM": inputTag.dataset });
                        }
                        else if (inputTag.dataset["name"] == "Order") {
                            SlctdOrderPaymentIds.push(record_ID);
                            BusinessPartnerIds.push({ "BP": inputTag.dataset });
                            batchObjOrd.push({ "ID": record_ID, "PM": inputTag.dataset });
                        }
                        else {
                            SlctdJournalPaymentIds.push(record_ID);
                            BusinessPartnerIds.push({ "BP": inputTag.dataset });
                            batchObjJournal.push({ "ID": record_ID, "PM": inputTag.dataset });
                        }
                        var selRecords = SlctdPaymentIds.length + SlctdOrderPaymentIds.length + SlctdJournalPaymentIds.length;
                        var countRecords = $($divPayment.find(".VA009-payment-wrap")).length;
                        //VIS_427 Handeled issue for select all check box
                        var checkedrecords = $(".VA009-payment-list").find("input[type=checkbox]:checked").length;
                        var uncheckedrecords = $(".VA009-payment-list").find("input[type=checkbox]:not(:checked)").length;
                        if ((countRecords == selRecords && uncheckedrecords == 0) || (countRecords == checkedrecords)) {
                            $selectall.prop('checked', true);
                        }
                        /* VIS_427 DevOps id:2289 getting value according to precision */
                        precision = VIS.Utility.Util.getValueOfInt(inputTag.dataset["precision"]);
                        var baseAmt = VIS.Utility.Util.getValueOfDecimal(inputTag.dataset["baseamt"]).toFixed(precision);
                        if (inputTag.dataset["docbasetype"] == "ARC" || inputTag.dataset["docbasetype"] == "APC") {
                            baseAmt = (-1 * baseAmt);
                        }
                        $totalAmt.data('ttlamt', parseFloat(baseAmt, precision));
                        $totalAmt.text(getFormattednumber(baseAmt, precision));
                    }
                }
                //if user click on inside div class "VA009-payment-wrap" this condition will execute
                else if (target.parents(".VA009-payment-wrap").find(".VA009-clckd-checkbx").prop("checked")) {
                    $divPayment.find(':checkbox').not(":disabled").prop('checked', false);
                    $divPayment.find('.VA009-payment-wrap').removeClass("VA009-payment-wrap-selctd");
                    $selectall.prop('checked', false);
                    SlctdPaymentIds = [];
                    SlctdOrderPaymentIds = [];
                    SlctdJournalPaymentIds = [];
                    batchObjInv = [];
                    batchObjOrd = [];
                    batchObjJournal = [];
                    BusinessPartnerIds = [];
                    $totalAmt.text(0);
                    $totalAmt.data('ttlamt', parseFloat(0));
                }
                else {
                    $divPayment.find(':checkbox').not(":disabled").prop('checked', false);
                    $divPayment.find('.VA009-payment-wrap').removeClass("VA009-payment-wrap-selctd");
                    if (target.parents(".VA009-payment-wrap").find(".VA009-clckd-checkbx").prop("disabled") == true) {
                        //(1052)hold payment case : donot select record
                        return;
                    }
                    target.parents(".VA009-payment-wrap").find(".VA009-clckd-checkbx").prop("checked", true);
                    target.parents(".VA009-payment-wrap").addClass("VA009-payment-wrap-selctd");
                    var inputTag = target.parents(".VA009-payment-wrap").find(".VA009-clckd-checkbx")[0];
                    record_ID = VIS.Utility.Util.getValueOfInt(inputTag.dataset["uid"]);
                    SlctdPaymentIds = [];
                    SlctdOrderPaymentIds = [];
                    SlctdJournalPaymentIds = [];
                    batchObjInv = [];
                    batchObjOrd = [];
                    batchObjJournal = [];
                    BusinessPartnerIds = [];
                    if (inputTag.dataset["name"] == "Invoice") {
                        SlctdPaymentIds.push(record_ID);
                        BusinessPartnerIds.push({ "BP": inputTag.dataset });
                        batchObjInv.push({ "ID": record_ID, "PM": inputTag.dataset });
                    }
                    else if (inputTag.dataset["name"] == "Order") {
                        SlctdOrderPaymentIds.push(record_ID);
                        BusinessPartnerIds.push({ "BP": inputTag.dataset });
                        batchObjOrd.push({ "ID": record_ID, "PM": inputTag.dataset });
                    }
                    else {
                        SlctdJournalPaymentIds.push(record_ID);
                        BusinessPartnerIds.push({ "BP": inputTag.dataset });
                        batchObjJournal.push({ "ID": record_ID, "PM": inputTag.dataset });
                    }
                    var selRecords = SlctdPaymentIds.length + SlctdOrderPaymentIds.length + SlctdJournalPaymentIds.length;
                    var countRecords = $($divPayment.find(".VA009-payment-wrap")).length;
                    //VIS_427 Handeled issue for select all check box
                    var checkedrecords = $(".VA009-payment-list").find("input[type=checkbox]:checked").length;
                    var uncheckedrecords = $(".VA009-payment-list").find("input[type=checkbox]:not(:checked)").length;
                    if ((countRecords == selRecords && uncheckedrecords == 0) || (countRecords == checkedrecords)) {
                        $selectall.prop('checked', true);
                    }
                    precision = VIS.Utility.Util.getValueOfInt(inputTag.dataset["precision"]);
                    var baseAmt = VIS.Utility.Util.getValueOfDecimal(inputTag.dataset["baseamt"]).toFixed(precision);
                    if (inputTag.dataset["docbasetype"] == "ARC" || inputTag.dataset["docbasetype"] == "APC") {
                        baseAmt = (-1 * baseAmt);
                    }
                    /* VIS_427 DevOps id:2289 getting value according to precision */
                    $totalAmt.data('ttlamt', parseFloat(baseAmt, precision));
                    $totalAmt.text(getFormattednumber(baseAmt,precision));
                }
            }
        };
        //******************
        //Where Clause
        //******************
        function whereClause(colname, val) {
            if (colname.contains("Org"))
                _WhrOrg = ' AND ' + colname + ' IN (' + val + ')';
            else if (colname.contains("PaymentMethod"))
                _WhrPayMtd = ' AND ' + colname + ' IN (' + val + ')';
            else if (colname.contains("BPartner"))
                _Whr_BPrtnr = ' AND ' + colname + ' IN (' + val + ')';
            else if (colname.contains("Status")) {
                if (val != "") {

                    _WhrStatus = ' AND ' + colname + ' IN (';
                    for (var i in val) {
                        _WhrStatus += "'" + val[i] + "',";
                    }
                    _WhrStatus = _WhrStatus.slice(0, -1);
                    _WhrStatus += ')';
                }
            }
            else if (colname.contains("DueDate")) {
                DueDateSelected = Math.max.apply(Math, Duedateval);
            }
        };
        //******************
        //Clear All Search Criteria
        //******************
        function ClearEasySearch() {
            //$BPSelected.find("ul").remove();
            bpids = [];
            SlctdPaymentIds = [];
            SlctdOrderPaymentIds = [];
            SlctdJournalPaymentIds = [];
            $divPayment.find(':checkbox').prop('checked', false);
            $selectall.find(':checkbox').prop('checked', false);
            batchObjInv = [];
            batchObjOrd = [];
            batchObjJournal = [];
            clearamtid();
            //$totalAmt.text(0);
            //$totalAmt.data('ttlamt', parseFloat(0));
        };

        function getFormattednumber(value, precision) {
            if (value && typeof value == 'number') {
                value = parseFloat(value).toLocaleString(undefined, { minimumFractionDigits: precision });
            }
            return value;
        };

        //******************
        //Load Data And Grids on Form Load
        //******************
        var _loadFunctions = {
            LoadTargetDocType: function ($control, basetype) {//Rakesh(VA228):to load all load Document Type based on basetype
                $control.empty();
                var _org_Id = $POP_cmbOrg != null ? $POP_cmbOrg.val() : 0;
                VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadTargetType", { "ad_org_Id": _org_Id, "baseType": basetype }, callbacktargettype);
                function callbacktargettype(dr) {
                    $control.append(" <option value = 0></option>");
                    if (dr != null && dr.length > 0) {
                        for (var i in dr) {
                            $control.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_DocType_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                        }
                    }
                    $control.prop('selectedIndex', 0);
                    $control.addClass('vis-ev-col-mandatory');
                };
            },

            LoadTargetDocTypeB2B: function ($control, basetype, bankAcct_ID) {
                $control.empty();
                if (bankAcct_ID == 0) {
                    //if Bank Account is not selected
                    _org_Id = $OrgCmb != null ? $OrgCmb.val() : 0;
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadTargetType", { "ad_org_Id": _org_Id, "baseType": basetype }, callbacktargettype);
                }
                else {
                    //Load the Document Types as per selected Bank Account's organization
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadBankTargetType", { "BankAcct_ID": bankAcct_ID, "BaseType": basetype }, callbacktargettype);
                }
                function callbacktargettype(dr) {
                    $control.append(" <option value = 0></option>");
                    if (dr != null && dr.length > 0) {
                        for (var i in dr) {
                            $control.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_DocType_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                        }
                    }
                    $control.prop('selectedIndex', 0);
                    $control.addClass('vis-ev-col-mandatory');
                };
            },

            loadOrg: function () {
                VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadOrganization", null, callbackloadorg);
                function callbackloadorg(dr) {
                    var orgindex = 0;
                    $Org.append(" <option value = 0></option>");
                    if (dr.length > 0) {
                        for (var i in dr) {
                            $Org.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].AD_Org_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                        }
                    }
                    $Org.prop('selectedIndex', 0);
                }
            },

            loadPaymentMethods: function () {
                VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadPaymentMethod", null, callbackloadpaymthds);

                function callbackloadpaymthds(dr) {
                    if (dr != null) {
                        $payMthd.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $payMthd.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].VA009_PaymentMethod_ID) + ">" + VIS.Utility.encodeText(dr[i].VA009_Name) + "</option>");
                            }
                        }
                        $payMthd.prop('selectedIndex', 0);
                    }
                }
            },

            loadStatus: function () {
                VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadStatus", null, callbackloadstatus);
                function callbackloadstatus(dr) {
                    if (dr != null) {
                        $status.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $status.append("<option value=" + dr[i].Value + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                            }
                        }
                        $status.prop('selectedIndex', 0);
                    }
                }
            },

            Cheque_Pay_Dialog: function () {
                CheuePaybleGrid, _Cheque_no = "";
                var _C_Bank_ID = 0, _C_BankAccount_ID = 0, cheqAmount;
                $bsyDiv[0].style.visibility = "visible";
                var divAmount, format = null;
                lblAmount = $("<label>");
                cheqAmount = new VIS.Controls.VAmountTextBox("VA009_AccountBalance" + $self.windowNo + "", false, true, true, 50, 100, VIS.DisplayType.Amount, VIS.Msg.getMsg("Amount"));
                lblAmount.append(VIS.Msg.getMsg("VA009_AccountBalance"));
                cheqAmount.setValue(0);
                format = VIS.DisplayType.GetNumberFormat(VIS.DisplayType.Amount);
                divAmount = $("<div class='VA009-popCheck-data input-group vis-input-wrap'>");
                var $divchequeAmtCtrlWrp = $("<div class='vis-control-wrap'>");
                divAmount.append($divchequeAmtCtrlWrp);
                $divchequeAmtCtrlWrp.append(cheqAmount.getControl().attr('placeholder', ' ').attr('data-placeholder', '')).append(lblAmount);
                $chequePayble = $("<div class='VA009-popform-content vis-formouterwrpdiv' style='min-height:450px !important'>");
                _ChequePayble = $("<div class='VA009-popfrm-wrap' style='height:auto;'>");
                _chequedata = $("<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbOrg_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_Org") + "</label>"
                    + "</div></div>"

                    //Rakesh(VA228):Create Doc type element html
                    + "<div class='VA009-popCheck-data input-group vis-input-wrap' > <div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbDocType_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_DocType") + "</label>"
                    + "</div></div> "

                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbBank_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_Bank") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbBankAccount_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_BankAccount") + "</label>"
                    + "</div></div>");

                _chequedata1 = $("<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbPaySelectn_" + $self.windowNo + "'>"
                    + ' <option value="M">' + VIS.Msg.getMsg("VA009_PayManully") + '</option>'
                    + ' <option value="P">' + VIS.Msg.getMsg("VA009_PayPrint") + '</option>'
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_PaymentSelection") + "</label>"
                    + "</div></div>"

                    //Account Date
                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_AccountDate_" + $self.windowNo + "' placeholder=' ' data-placeholder=''>"
                    + "<label>" + VIS.Msg.getMsg("AccountDate") + "</label>"
                    + "</div></div>"

                    //Transaction Date
                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_TransactionDate" + $self.windowNo + "' placeholder=' ' data-placeholder='' >"
                    + "<label>" + VIS.Msg.getMsg("TransactionDate") + "</label>"
                    + "</div></div>"
                    //Currency Type
                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbCurrencyType_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_CurrencyType") + "</label>"
                    + "</div></div>"

                    //payment Method
                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbPaymthd_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_PayMthd") + "</label>"
                    + "</div></div>"

                    //Currenct Next CheckNo
                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='text' id='VA009_Chqnotxt_" + $self.windowNo + "' placeholder=' ' data-placeholder='' disabled > "
                    + "<label>" + VIS.Msg.getMsg("VA009_Chqnolbl") + "</label>"
                    + "</div></div>"

                    //Consolidated check no 
                    + "<div class='VA009-popCheck-data VA009-popformchkctrlwrap' id='VA009_POP_textCheckNoDiv'>"
                    + "<div>"
                    + '<input type="checkbox" id="VA009_chkConsolidate_' + $self.windowNo
                    + '"> <label>' + VIS.Msg.getMsg("VA009_ConsolidateCheck") + '</label>'
                    + "</div></div>"
                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='text' placeholder='" + VIS.Msg.getMsg("VA009_ChkNo") + "' disabled id='VA009_POP_textCheckNo_" + $self.windowNo + "'><label>" + VIS.Msg.getMsg("VA009_ChkNo") + "</label>"
                    + "</div></div> "

                    + "<div class='VA009-grid-container'><div class='VA009-table-container' id='VA009_btnPopupGrid'></div></div>"

                    + "</div>");
                _ChequePayble.append(_chequedata).append(divAmount).append(_chequedata1);
                $chequePayble.append(_ChequePayble);
                CHQPAY_getControls();
                var now = new Date();
                var _today = now.getFullYear() + "-" + (("0" + (now.getMonth() + 1)).slice(-2)) + "-" + (("0" + now.getDate()).slice(-2));
                $POP_DateAcct.val(_today);
                $POP_DateTrx.val(_today);
                ChequePayDialog = new VIS.ChildDialog();
                ChequePayDialog.setContent($chequePayble);
                ChequePayDialog.setTitle(VIS.Msg.getMsg("VA009_LoadChequePayment"));
                ChequePayDialog.setWidth("80%");
                //VA230:Remove outer scroll bar
                //ChequePayDialog.setHeight(window.innerHeight - 75);
                ChequePayDialog.setEnableResize(true);
                ChequePayDialog.setModal(true);
                if (SlctdPaymentIds.toString() != "" || SlctdOrderPaymentIds.toString() != "" || SlctdJournalPaymentIds.toString() != "") {
                    // Added by Bharat on 01/May/2017
                    callbackCashPayments("");
                }
                cheqAmount.addVetoableChangeListener(this);
                function callbackCashPayments(data) {
                    if (data != "") {
                        $bsyDiv[0].style.visibility = "hidden";
                        VIS.ADialog.info("VA009_PleaseSelctonlyCheque");
                    }
                    else {
                        ChequePayDialog.show();
                        //populate banks based on selected organization in dialog
                        loadbanks($POP_cmbBank, VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()));
                        $POP_cmbBank.addClass('vis-ev-col-mandatory');
                        $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                        if ($POP_DateAcct.val()) {
                            $POP_DateAcct.removeClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_DateAcct.addClass('vis-ev-col-mandatory');
                        }
                        CHQPayGrid_Layout();
                        loadgrdPay(callbackchqPay);
                        loadCurrencyType();
                        loadPayMthd();
                        loadOrg();
                        //Rakesh(VA228):10/Sep/2021 -> Load APP target base doc type
                        _loadFunctions.LoadTargetDocType($POP_targetDocType, _TargetBaseType);
                    }
                };

                function CHQPayGrid_Layout() {
                    var _CHQPay_Columns = [];

                    if (_CHQPay_Columns.length == 0) {
                        _CHQPay_Columns.push({ field: "C_Bpartner", caption: VIS.Msg.getMsg("VA009_Vendor"), sortable: true, size: '12%' });
                        _CHQPay_Columns.push({ field: "C_InvoicePaySchedule_ID", caption: VIS.Msg.getMsg("VA009_Schedule"), sortable: true, size: '7%' });
                        _CHQPay_Columns.push({ field: "Description", caption: VIS.Msg.getMsg("Description"), sortable: true, size: '15%', editable: { type: 'text' } });
                        _CHQPay_Columns.push({ field: "CurrencyCode", caption: VIS.Msg.getMsg("VA009_Currency"), sortable: true, size: '8%' });
                        _CHQPay_Columns.push({
                            field: "DueAmt", caption: VIS.Msg.getMsg("VA009_DueAmt"), sortable: true, size: '12%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["DueAmt"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }
                        });
                        _CHQPay_Columns.push({
                            field: "ConvertedAmt", caption: VIS.Msg.getMsg("VA009_ConvertedAmt"), sortable: true, size: '12%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["ConvertedAmt"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }
                        });
                        _CHQPay_Columns.push({
                            field: "VA009_RecivedAmt", caption: VIS.Msg.getMsg("VA009_PayAmt"), sortable: true, size: '12%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["VA009_RecivedAmt"];
                                val = checkcommaordot(event, val, val);
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }, editable: { type: 'number' }
                        });
                        _CHQPay_Columns.push({
                            field: "OverUnder", caption: VIS.Msg.getMsg("VA009_OverUnder"), sortable: true, size: '8%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["OverUnder"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }
                        });
                        _CHQPay_Columns.push({
                            field: "Writeoff", caption: VIS.Msg.getMsg("VA009_Writeoff"), sortable: true, size: '8%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["Writeoff"];
                                val = checkcommaordot(event, val, val);
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }, editable: { type: 'number' }
                        });
                        _CHQPay_Columns.push({
                            field: "Discount", caption: VIS.Msg.getMsg("VA009_Discount"), sortable: true, size: '8%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["Discount"];
                                val = checkcommaordot(event, val, val);
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }, editable: { type: 'number' }
                        });

                        _CHQPay_Columns.push({
                            field: "CheckNumber", caption: VIS.Msg.getMsg("VA009_ChkNo"), sortable: true, size: '12%', render: function (record, index, col_index) {
                                //Update the CheckNumber in form field or grid field either one of the field is changed
                                //it will update when edited field is empty and when do change the value on CheckNumber field on form
                                if (record.changes != undefined && event.currentTarget.id != undefined) {
                                    if (event.currentTarget.id == $POPtxtCheckNumber.toArray()[0].id) {
                                        //wather on grid selected only one then if you change the value on checkNumber it will change grid field and vice versa
                                        if (chqpaygrd.records.length == 1) {
                                            record.changes.CheckNumber = record.CheckNumber;
                                            return record.CheckNumber;
                                        }
                                        else {
                                            if (record.changes.CheckNumber == "" || record.changes.CheckNumber == 0) {
                                                record.changes.CheckNumber = record.CheckNumber;
                                                return record.CheckNumber;
                                            }
                                            else {
                                                return record.changes.CheckNumber;
                                            }
                                        }
                                    }
                                    else {
                                        return record.changes.CheckNumber;
                                    }
                                }
                                else {
                                    return record.CheckNumber;
                                }
                            }, editable: { type: 'alphanumeric', autoFormat: true, groupSymbol: ' ' }
                        });
                        _CHQPay_Columns.push({
                            field: "CheckDate", caption: VIS.Msg.getMsg("VA009_CheckDate"), sortable: true, size: '10%', style: 'text-align: left',
                            render: function (record, index, col_index) {
                                var val;
                                //when user do double click on CheckDate field then mouse over without selecting the value at that time 
                                //record.changes.CheckDate is get empty string value so to avoid that used condtion is compared with empty string
                                if (record.changes == undefined || record.changes.CheckDate == "") {
                                    val = record["CheckDate"];
                                }
                                else {
                                    val = record.changes.CheckDate;
                                }
                                return new Date(val).toLocaleDateString();
                            }, editable: { type: 'date' }
                        });
                        _CHQPay_Columns.push({ field: "recid", caption: VIS.Msg.getMsg("VA009_srno"), sortable: true, size: '1%' });
                        //by Amit - 1-12-2016
                        _CHQPay_Columns.push({ field: "TransactionType", caption: VIS.Msg.getMsg("VA009_TransactionType"), sortable: true, size: '1%' });
                        _CHQPay_Columns.push({ field: "C_BPartner_Location_ID", caption: VIS.Msg.getMsg("C_BPartner_Location_ID"), sortable: true, size: '1%' });
                        _CHQPay_Columns.push({ field: "C_DocType_ID", caption: VIS.Msg.getMsg("C_DocType_ID"), sortable: true, size: '1%' });
                        _CHQPay_Columns.push({ field: "DocBaseType", caption: VIS.Msg.getMsg("DocBaseType"), sortable: true, size: '1%' });
                    }
                    chqpaygrd = null;
                    chqpaygrd = CheuePaybleGrid.w2grid({
                        name: 'CheuePaybleGrid_' + $self.windowNo,
                        recordHeight: 25,
                        columns: _CHQPay_Columns,
                        multiSelect: true,
                        onEditField: function (event) {
                            if (event.column == 6 || event.column == 8 || event.column == 9) {
                                if (chqpaygrd.get(event.recid).TransactionType == 'Order' || chqpaygrd.get(event.recid).TransactionType == 'GL Journal') {
                                    event.isCancelled = true;
                                }
                            }
                            else if ($POp_cmbPaySelectn.val() == "P") {
                                event.isCancelled = true;
                            }
                            event.onComplete = function (event) {
                                id = event.recid;
                                if (event.column == 6 || event.column == 8 || event.column == 9) {
                                    chqpaygrd.records[event.index][chqpaygrd.columns[event.column].field] = checkcommaordot(event, chqpaygrd.records[event.index][chqpaygrd.columns[event.column].field]);
                                    var _value = format.GetFormatAmount(chqpaygrd.records[event.index][chqpaygrd.columns[event.column].field], "init", dotFormatter);
                                    chqpaygrd.records[event.index][chqpaygrd.columns[event.column].field] = format.GetConvertedString(_value, dotFormatter);
                                    $('#grid_CheuePaybleGrid_' + $self.windowNo + '_edit_' + id + '_' + event.column).keydown(function (event) {
                                        var isDotSeparator = culture.isDecimalSeparatorDot(window.navigator.language);

                                        if (!isDotSeparator && (event.keyCode == 190 || event.keyCode == 110)) {// , separator
                                            return false;
                                        }
                                        else if (isDotSeparator && event.keyCode == 188) { // . separator
                                            return false;
                                        }
                                        if (event.target.value.contains(".") && (event.which == 110 || event.which == 190 || event.which == 188)) {
                                            this.value = this.value.replace('.', '');
                                        }
                                        if (event.target.value.contains(",") && (event.which == 110 || event.which == 190 || event.which == 188)) {
                                            this.value = this.value.replace(',', '');
                                        }
                                        if (event.keyCode != 8 && event.keyCode != 9 && (event.keyCode < 37 || event.keyCode > 40) &&
                                            (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)
                                            && event.keyCode != 109 && event.keyCode != 189 && event.keyCode != 110
                                            && event.keyCode != 144 && event.keyCode != 188 && event.keyCode != 190) {
                                            return false;
                                        }
                                    });
                                }
                            };
                        },

                        onChange: function (event) {
                            window.setTimeout(function () {
                                if (chqpaygrd.getChanges(event.recid) != undefined) {
                                    var stdPrecision = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetCurrencyPrecision", { "BankAccount_ID": VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()), "CurrencyFrom": "B" }, null);
                                    if (stdPrecision == null || stdPrecision == 0) {
                                        stdPrecision = 2;
                                    }

                                    chqpaygrd.records[event.index]['ConvertedAmt'] = parseFloat(chqpaygrd.records[event.index]['ConvertedAmt']);
                                    chqpaygrd.records[event.index]['VA009_RecivedAmt'] = parseFloat(chqpaygrd.records[event.index]['VA009_RecivedAmt']);
                                    chqpaygrd.records[event.index]['OverUnder'] = parseFloat(chqpaygrd.records[event.index]['OverUnder']);
                                    chqpaygrd.records[event.index]['Writeoff'] = parseFloat(chqpaygrd.records[event.index]['Writeoff']);
                                    chqpaygrd.records[event.index]['Discount'] = parseFloat(chqpaygrd.records[event.index]['Discount']);
                                    //Received Amount
                                    if (event.column == 6) {
                                        if (event.value_new == "") {
                                            event.value_new = 0;
                                        }
                                        else {
                                            event.value_new = format.GetConvertedNumber(event.value_new, dotFormatter);
                                        }

                                        //else if (event.value_new.toString().contains(',')) {
                                        //    event.value_new = parseFloat(event.value_new.replace(',', '.'));
                                        //}
                                        if (event.value_new > chqpaygrd.records[event.index]['ConvertedAmt']) {
                                            VIS.ADialog.error("MoreScheduleAmount");
                                            event.value_new = event.value_original;
                                            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = event.value_new;
                                            chqpaygrd.get(event.recid).VA009_RecivedAmt = event.value_new;
                                            chqpaygrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                            return;
                                        }
                                        chqpaygrd.records[event.index]['VA009_RecivedAmt'] = event.value_new.toFixed(stdPrecision);
                                        chqpaygrd.refreshCell(event.recid, "VA009_RecivedAmt");

                                        if (chqpaygrd.records[event.index]['PaymwentBaseType'] == "ARR" || chqpaygrd.records[event.index]['PaymwentBaseType'] == "APP") {
                                            if (event.value_new < chqpaygrd.records[event.index]['ConvertedAmt']) {
                                                if (chqpaygrd.records[event.index]['ConvertedAmt'] < 0) {
                                                    if (chqpaygrd.get(event.recid).TransactionType == 'Order') {
                                                        chqpaygrd.get(event.recid).changes.Discount = ((chqpaygrd.records[event.index]['ConvertedAmt']) - event.value_new);
                                                        chqpaygrd.get(event.recid).Discount = (chqpaygrd.get(event.recid).changes.Discount).toFixed(stdPrecision);
                                                        chqpaygrd.get(event.recid).changes.Writeoff = 0;
                                                        chqpaygrd.get(event.recid).Writeoff = 0;
                                                        chqpaygrd.get(event.recid).changes.OverUnder = 0;
                                                        chqpaygrd.get(event.recid).OverUnder = 0;
                                                    }
                                                    else {
                                                        chqpaygrd.get(event.recid).changes.OverUnder = ((chqpaygrd.records[event.index]['ConvertedAmt']) - event.value_new);
                                                        chqpaygrd.get(event.recid).OverUnder = (chqpaygrd.get(event.recid).changes.OverUnder).toFixed(stdPrecision);
                                                        chqpaygrd.get(event.recid).changes.Writeoff = 0;
                                                        chqpaygrd.get(event.recid).changes.Discount = 0;
                                                    }
                                                }
                                                else {
                                                    if (chqpaygrd.get(event.recid).TransactionType == 'Order') {
                                                        chqpaygrd.get(event.recid).changes.Discount = ((chqpaygrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                                        chqpaygrd.records[event.index]['Discount'] = chqpaygrd.get(event.recid).changes.Discount;
                                                        // chqpaygrd.get(event.recid).Discount = chqpaygrd.get(event.recid).changes.Discount;
                                                        chqpaygrd.get(event.recid).changes.Writeoff = 0;
                                                        chqpaygrd.get(event.recid).Writeoff = 0;
                                                        chqpaygrd.get(event.recid).changes.OverUnder = 0;
                                                        chqpaygrd.get(event.recid).OverUnder = 0;
                                                    }
                                                    else {
                                                        // changed by Bharat
                                                        chqpaygrd.get(event.recid).changes.OverUnder = ((chqpaygrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                                        chqpaygrd.records[event.index]['OverUnder'] = chqpaygrd.get(event.recid).changes.OverUnder;
                                                        //chqpaygrd.get(event.recid).OverUnder = VIS.Utility.Util.getValueOfDecimal((chqpaygrd.get(event.recid).changes.OverUnder));
                                                        chqpaygrd.get(event.recid).changes.Writeoff = 0;
                                                        chqpaygrd.get(event.recid).Writeoff = 0;
                                                        chqpaygrd.get(event.recid).changes.Discount = 0;
                                                        //VIS317
                                                        //Devops 1800
                                                        //Getting Discount Value Zero. 
                                                        chqpaygrd.get(event.recid).Discount = 0;
                                                    }
                                                }
                                                chqpaygrd.refreshCell(event.recid, "OverUnder");
                                                chqpaygrd.refreshCell(event.recid, "Discount");
                                                chqpaygrd.refreshCell(event.recid, "Writeoff");
                                            }
                                            else if (event.value_new == chqpaygrd.records[event.index]['ConvertedAmt']) {
                                                chqpaygrd.get(event.recid).changes.OverUnder = 0;
                                                chqpaygrd.get(event.recid).OverUnder = 0;
                                                chqpaygrd.get(event.recid).changes.Discount = 0;
                                                chqpaygrd.get(event.recid).changes.Writeoff = 0;
                                                chqpaygrd.get(event.recid).Discount = 0;
                                                chqpaygrd.get(event.recid).Writeoff = 0;
                                                chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = event.value_new;
                                                chqpaygrd.get(event.recid).VA009_RecivedAmt = event.value_new;
                                                chqpaygrd.refreshCell(event.recid, "OverUnder");
                                                chqpaygrd.refreshCell(event.recid, "Discount");
                                                chqpaygrd.refreshCell(event.recid, "Writeoff");
                                                chqpaygrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                            }
                                            else if (event.value_new > chqpaygrd.records[event.index]['ConvertedAmt']) {
                                                if (chqpaygrd.records[event.index]['ConvertedAmt'] < 0) {
                                                    chqpaygrd.get(event.recid).changes.OverUnder = 0;
                                                    chqpaygrd.get(event.recid).OverUnder = 0;
                                                    chqpaygrd.get(event.recid).changes.Writeoff = ((chqpaygrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                                    chqpaygrd.get(event.recid).Writeoff = VIS.Utility.Util.getValueOfDecimal((chqpaygrd.get(event.recid).changes.Writeoff));
                                                }
                                                else {
                                                    chqpaygrd.get(event.recid).changes.OverUnder = ((chqpaygrd.records[event.index]['ConvertedAmt']) - event.value_new);
                                                    chqpaygrd.records[event.index]['OverUnder'] = chqpaygrd.get(event.recid).changes.OverUnder;
                                                    //chqpaygrd.get(event.recid).OverUnder = (chqpaygrd.get(event.recid).changes.OverUnder).toFixed(stdPrecision);
                                                    chqpaygrd.get(event.recid).changes.Writeoff = 0;
                                                    chqpaygrd.get(event.recid).Writeoff = 0;
                                                }
                                                chqpaygrd.get(event.recid).changes.Discount = 0;
                                                chqpaygrd.get(event.recid).Discount = 0;
                                                chqpaygrd.refreshCell(event.recid, "OverUnder");
                                                chqpaygrd.refreshCell(event.recid, "Discount");
                                                chqpaygrd.refreshCell(event.recid, "Writeoff");
                                            }
                                        }
                                    }
                                    //writeoff
                                    if (event.column == 8) {
                                        if (event.value_new == "") {
                                            event.value_new = 0;
                                        }
                                        else {
                                            event.value_new = format.GetConvertedNumber(event.value_new, dotFormatter);
                                        }
                                        //else if (event.value_new.toString().contains(',')) {
                                        //    event.value_new = parseFloat(event.value_new.replace(',', '.'));
                                        //}

                                        chqpaygrd.records[event.index]['Writeoff'] = event.value_new.toFixed(stdPrecision);
                                        //VIS_427 BugId 2325 not allowing user to enter more writeof amount than converted amount 
                                        if (event.value_new > chqpaygrd.records[event.index]['ConvertedAmt']) {
                                            VIS.ADialog.error("MoreScheduleAmount");
                                            event.value_new = event.value_original;
                                            chqpaygrd.get(event.recid).changes.Writeoff = event.value_new;
                                            chqpaygrd.get(event.recid).Writeoff = event.value_new;
                                            chqpaygrd.refreshCell(event.recid, "Writeoff");
                                            return;
                                        }
                                        if (chqpaygrd.records[event.index]['PaymwentBaseType'] == "ARR" || chqpaygrd.records[event.index]['PaymwentBaseType'] == "APP") {
                                            if (event.value_new > chqpaygrd.records[event.index]['ConvertedAmt']) {
                                                if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                    chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                     //VIS_427 BugId 2325 handled over under to not be negative
                                                    if (chqpaygrd.get(event.recid).changes.OverUnder < 0) {
                                                        VIS.ADialog.error("MoreScheduleAmount");
                                                        chqpaygrd.get(event.recid).changes.OverUnder = chqpaygrd.records[event.index]['OverUnder'];
                                                        chqpaygrd.get(event.recid).changes.Writeoff = event.value_original;
                                                        chqpaygrd.records[event.index]['Writeoff'] = event.value_original;
                                                    }
                                                    else {
                                                        chqpaygrd.records[event.index]['OverUnder'] = Math.abs(chqpaygrd.get(event.recid).changes.OverUnder);
                                                    }
                                                }
                                                else {
                                                    chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (event.value_new - chqpaygrd.records[event.index]['ConvertedAmt']).toFixed(stdPrecision);
                                                    //VIS_427 BugId 2325 handled Received to not be negative
                                                    if (chqpaygrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                        VIS.ADialog.error("MoreScheduleAmount");
                                                        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = chqpaygrd.records[event.index]['VA009_RecivedAmt'];
                                                        chqpaygrd.get(event.recid).changes.Writeoff = event.value_original;
                                                        chqpaygrd.records[event.index]['Writeoff'] = event.value_original;
                                                    }
                                                    else {
                                                        chqpaygrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqpaygrd.get(event.recid).changes.VA009_RecivedAmt);
                                                    }
                                                }
                                                chqpaygrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                chqpaygrd.refreshCell(event.recid, "OverUnder");
                                            }
                                            else if (event.value_new <= chqpaygrd.records[event.index]['ConvertedAmt']) {

                                                if (chqpaygrd.get(event.recid).changes.Discount == undefined && chqpaygrd.get(event.recid).changes.OverUnder == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled over under to not be negative
                                                        if (chqpaygrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.OverUnder = chqpaygrd.records[event.index]['OverUnder'];
                                                            chqpaygrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqpaygrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['OverUnder'] = Math.abs(chqpaygrd.get(event.recid).changes.OverUnder);
                                                        }
                                                    }
                                                    else {
                                                        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + chqpaygrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Recieved to not be negative for payable
                                                        if (chqpaygrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = chqpaygrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqpaygrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqpaygrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqpaygrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else if (chqpaygrd.get(event.recid).changes.Discount == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled over under to not be negative
                                                        if (chqpaygrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.OverUnder = chqpaygrd.records[event.index]['OverUnder'];
                                                            chqpaygrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqpaygrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['OverUnder'] = Math.abs(chqpaygrd.get(event.recid).changes.OverUnder);
                                                        }                                                       
                                                    }
                                                    else {
                                                        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).OverUnder) + chqpaygrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative
                                                        if (chqpaygrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = chqpaygrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqpaygrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqpaygrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqpaygrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else if (chqpaygrd.get(event.recid).changes.OverUnder == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled over under to not be negative
                                                        if (chqpaygrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.OverUnder = chqpaygrd.records[event.index]['OverUnder'];
                                                            chqpaygrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqpaygrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['OverUnder'] = Math.abs(chqpaygrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        chqpaygrd.refreshCell(event.recid, "OverUnder");
                                                    }
                                                    else {
                                                        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative
                                                        if (chqpaygrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = chqpaygrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqpaygrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqpaygrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqpaygrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled over under to not be negative
                                                        if (chqpaygrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.OverUnder = chqpaygrd.records[event.index]['OverUnder'];
                                                            chqpaygrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqpaygrd.records[event.index]['Writeoff'] = event.value_original;;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['OverUnder'] = Math.abs(chqpaygrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        
                                                    }
                                                    else {
                                                        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative
                                                        if (chqpaygrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = chqpaygrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqpaygrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqpaygrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqpaygrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                chqpaygrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                chqpaygrd.refreshCell(event.recid, "OverUnder");
                                            }


                                            if (event.value_new > chqpaygrd.records[event.index]['ConvertedAmt']) {
                                                //if (chqpaygrd.records[event.index]['ConvertedAmt'] < 0) {
                                                //    if (chqpaygrd.get(event.recid).changes.Discount == undefined && chqpaygrd.get(event.recid).changes.OverUnder == undefined) {
                                                //        if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //        }
                                                //        else {
                                                //            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + chqpaygrd.records[event.index]['Discount']))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + chqpaygrd.records[event.index]['Discount']))).toFixed(stdPrecision);
                                                //        }
                                                //    }
                                                //    else if (chqpaygrd.get(event.recid).changes.Discount == undefined) {
                                                //        if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //        }
                                                //        else {
                                                //            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + chqpaygrd.records[event.index]['Discount']))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + chqpaygrd.records[event.index]['Discount']))).toFixed(stdPrecision);
                                                //        }
                                                //    }
                                                //    else if (chqpaygrd.get(event.recid).changes.OverUnder == undefined) {
                                                //        if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //        }
                                                //        else {
                                                //            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount)))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount)))).toFixed(stdPrecision);
                                                //        }
                                                //    }
                                                //    else {
                                                //        if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //        }
                                                //        else {
                                                //            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount)))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount)))).toFixed(stdPrecision);
                                                //        }
                                                //    }
                                                //}
                                                //else {
                                                //    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //    }
                                                //    else {
                                                //        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (event.value_new - chqpaygrd.records[event.index]['ConvertedAmt']).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).VA009_RecivedAmt = (event.value_new - chqpaygrd.records[event.index]['ConvertedAmt']).toFixed(stdPrecision);
                                                //    }
                                                //}
                                                //chqpaygrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                //chqpaygrd.refreshCell(event.recid, "OverUnder");
                                            }
                                            else if (event.value_new <= chqpaygrd.records[event.index]['ConvertedAmt']) {

                                                ////chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.get(chqpaygrd.getSelection())['ConvertedAmt'] - (event.value_new + chqpaygrd.get(chqpaygrd.getSelection())['OverUnder'] + chqpaygrd.get(chqpaygrd.getSelection())['Discount'])) * -1;
                                                //if (chqpaygrd.get(event.recid).changes.Discount == undefined && chqpaygrd.get(event.recid).changes.OverUnder == undefined) {
                                                //    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //    }
                                                //    else {
                                                //        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + chqpaygrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + chqpaygrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                //    }
                                                //}
                                                //else if (chqpaygrd.get(event.recid).changes.Discount == undefined) {
                                                //    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //    }
                                                //    else {
                                                //        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + chqpaygrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + chqpaygrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                //    }
                                                //}
                                                //else if (chqpaygrd.get(event.recid).changes.OverUnder == undefined) {
                                                //    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //    }
                                                //    else {
                                                //        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + chqpaygrd.get(event.recid).changes.Discount)).toFixed(stdPrecision);
                                                //    }
                                                //}
                                                //else {
                                                //    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //    }
                                                //    else {
                                                //        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                //    }
                                                //}
                                                //chqpaygrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                //chqpaygrd.refreshCell(event.recid, "OverUnder");
                                            }

                                        }
                                    }
                                    //discount
                                    if (event.column == 9) {
                                        if (event.value_new == "") {
                                            event.value_new = 0;
                                        }
                                        else {
                                            event.value_new = format.GetConvertedNumber(event.value_new, dotFormatter);
                                        }
                                        //else if (event.value_new.toString().contains(',')) {
                                        //    event.value_new = parseFloat(event.value_new.replace(',', '.'));
                                        //}

                                        chqpaygrd.records[event.index]['Discount'] = event.value_new.toFixed(stdPrecision);
                                        //VIS_427 BugId 2325 not allowing user to enter more Discount amount than converted amount 
                                        if (event.value_new > chqpaygrd.records[event.index]['ConvertedAmt']) {
                                            VIS.ADialog.error("MoreScheduleAmount");
                                            event.value_new = event.value_original;
                                            chqpaygrd.get(event.recid).changes.Discount = event.value_new;
                                            chqpaygrd.get(event.recid).Discount = event.value_new;
                                            chqpaygrd.refreshCell(event.recid, "Discount");
                                            return;
                                        }

                                        if (chqpaygrd.records[event.index]['PaymwentBaseType'] == "ARR" || chqpaygrd.records[event.index]['PaymwentBaseType'] == "APP") {

                                            if (event.value_new > chqpaygrd.records[event.index]['ConvertedAmt']) {
                                                if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                    chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Writeoff))).toFixed(stdPrecision);
                                                    //VIS_427 BugId 2325 handled overunder to not be negative when user changes discount
                                                    if (chqpaygrd.get(event.recid).changes.OverUnder < 0) {
                                                        VIS.ADialog.error("MoreScheduleAmount");
                                                        chqpaygrd.get(event.recid).changes.OverUnder = chqpaygrd.records[event.index]['OverUnder'];
                                                        chqpaygrd.get(event.recid).changes.Discount = event.value_original;
                                                        chqpaygrd.records[event.index]['Discount'] = event.value_original;
                                                    }
                                                    else {
                                                        chqpaygrd.records[event.index]['OverUnder'] = Math.abs(chqpaygrd.get(event.recid).changes.OverUnder);
                                                    }
                                                }
                                                else {
                                                    chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (event.value_new - chqpaygrd.records[event.index]['ConvertedAmt']).toFixed(stdPrecision);
                                                    //VIS_427 BugId 2325 handled Received to not be negative when user changes discount
                                                    if (chqpaygrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                        VIS.ADialog.error("MoreScheduleAmount");
                                                        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = chqpaygrd.records[event.index]['VA009_RecivedAmt'];
                                                        chqpaygrd.get(event.recid).changes.Discount = event.value_original;
                                                        chqpaygrd.records[event.index]['Discount'] = event.value_original;
                                                    }
                                                    else {
                                                        chqpaygrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqpaygrd.get(event.recid).changes.VA009_RecivedAmt);
                                                    }
                                                }
                                                chqpaygrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                chqpaygrd.refreshCell(event.recid, "OverUnder");
                                            }
                                            else if (event.value_new <= chqpaygrd.records[event.index]['ConvertedAmt']) {
                                                //chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.get(chqpaygrd.getSelection())['ConvertedAmt'] - (event.value_new + chqpaygrd.get(chqpaygrd.getSelection())['OverUnder'] + chqpaygrd.get(chqpaygrd.getSelection())['Writeoff'])) * -1;
                                                if (chqpaygrd.get(event.recid).changes.Writeoff == undefined && chqpaygrd.get(event.recid).changes.OverUnder == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Writeoff))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes discount
                                                        if (chqpaygrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.OverUnder = chqpaygrd.records[event.index]['OverUnder'];
                                                            chqpaygrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqpaygrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['OverUnder'] = Math.abs(chqpaygrd.get(event.recid).changes.OverUnder);
                                                        }
                                                    }
                                                    else {
                                                        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + chqpaygrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes discount
                                                        if (chqpaygrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = chqpaygrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqpaygrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqpaygrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqpaygrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else if (chqpaygrd.get(event.recid).changes.Writeoff == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Writeoff))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes discount
                                                        if (chqpaygrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.OverUnder = chqpaygrd.records[event.index]['OverUnder'];
                                                            chqpaygrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqpaygrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['OverUnder'] = Math.abs(chqpaygrd.get(event.recid).changes.OverUnder);
                                                        }
                                                    }
                                                    else {
                                                        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).OverUnder) + chqpaygrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes discount
                                                        if (chqpaygrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = chqpaygrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqpaygrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqpaygrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqpaygrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else if (chqpaygrd.get(event.recid).changes.OverUnder == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Writeoff))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes discount
                                                        if (chqpaygrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.OverUnder = chqpaygrd.records[event.index]['OverUnder'];
                                                            chqpaygrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqpaygrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['OverUnder'] = Math.abs(chqpaygrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        
                                                    }
                                                    else {
                                                        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Writeoff))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes discount
                                                        if (chqpaygrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = chqpaygrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqpaygrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqpaygrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqpaygrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Writeoff))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes discount
                                                        if (chqpaygrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.OverUnder = chqpaygrd.records[event.index]['OverUnder'];
                                                            chqpaygrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqpaygrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['OverUnder'] = Math.abs(chqpaygrd.get(event.recid).changes.OverUnder);
                                                        }
                                                    }
                                                    else {
                                                        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).Writeoff))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes discount
                                                        if (chqpaygrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = chqpaygrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqpaygrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqpaygrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqpaygrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqpaygrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                chqpaygrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                chqpaygrd.refreshCell(event.recid, "OverUnder");
                                            }


                                            if (event.value_new > chqpaygrd.records[event.index]['ConvertedAmt']) {
                                                //if (chqpaygrd.records[event.index]['ConvertedAmt'] < 0) {
                                                //    if (chqpaygrd.get(event.recid).changes.Writeoff == undefined && chqpaygrd.get(event.recid).changes.OverUnder == undefined) {
                                                //        if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //        }
                                                //        else {
                                                //            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + chqpaygrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + chqpaygrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                //        }
                                                //    }
                                                //    else if (chqpaygrd.get(event.recid).changes.Writeoff == undefined) {
                                                //        if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //        }
                                                //        else {
                                                //            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + chqpaygrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + chqpaygrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                //        }
                                                //    }
                                                //    else if (chqpaygrd.get(event.recid).changes.OverUnder == undefined) {
                                                //        if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //        }
                                                //        else {
                                                //            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff)))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff)))).toFixed(stdPrecision);
                                                //        }
                                                //    }
                                                //    else {
                                                //        if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //        }
                                                //        else {
                                                //            chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff)))).toFixed(stdPrecision);
                                                //            chqpaygrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqpaygrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff)))).toFixed(stdPrecision);
                                                //        }
                                                //    }
                                                //}
                                                //else {
                                                //    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //    }
                                                //    else {
                                                //        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (event.value_new - chqpaygrd.records[event.index]['ConvertedAmt']).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).VA009_RecivedAmt = (event.value_new - chqpaygrd.records[event.index]['ConvertedAmt']).toFixed(stdPrecision);
                                                //    }
                                                //}
                                                //chqpaygrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                //chqpaygrd.refreshCell(event.recid, "OverUnder");
                                            }
                                            else if (event.value_new <= chqpaygrd.records[event.index]['ConvertedAmt']) {

                                                ////chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.get(chqpaygrd.getSelection())['ConvertedAmt'] - (event.value_new + chqpaygrd.get(chqpaygrd.getSelection())['OverUnder'] + chqpaygrd.get(chqpaygrd.getSelection())['Writeoff'])) * -1;
                                                //if (chqpaygrd.get(event.recid).changes.Writeoff == undefined && chqpaygrd.get(event.recid).changes.OverUnder == undefined) {
                                                //    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //    }
                                                //    else {
                                                //        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + chqpaygrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + chqpaygrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                //    }
                                                //}
                                                //else if (chqpaygrd.get(event.recid).changes.Writeoff == undefined) {
                                                //    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //    }
                                                //    else {
                                                //        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + chqpaygrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + chqpaygrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                //    }
                                                //}
                                                //else if (chqpaygrd.get(event.recid).changes.OverUnder == undefined) {
                                                //    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //    }
                                                //    else {
                                                //        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //    }
                                                //}
                                                //else {
                                                //    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) > 0) {
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).changes.OverUnder = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqpaygrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //    }
                                                //    else {
                                                //        chqpaygrd.get(event.recid).changes.VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //        chqpaygrd.get(event.recid).VA009_RecivedAmt = (chqpaygrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                //    }
                                                //}
                                                //chqpaygrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                //chqpaygrd.refreshCell(event.recid, "OverUnder");
                                            }
                                        }
                                    }

                                    chqpaygrd.refreshCell(event.recid, "ConvertedAmt");
                                    chqpaygrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                    chqpaygrd.refreshCell(event.recid, "OverUnder");
                                    chqpaygrd.refreshCell(event.recid, "Writeoff");
                                    chqpaygrd.refreshCell(event.recid, "Discount");
                                }
                                if (event.column == 10) {
                                    chqpaygrd.records[event.index]['CheckNumber'] = event.value_new;
                                    //Update $POPtxtCheckNumber when change grid checkDate
                                    //CheckNumber field will update when it grid records are only one
                                    if (chqpaygrd.records.length == 1) {
                                        $POPtxtCheckNumber.val(event.value_new);
                                    }
                                }
                                if (event.column == 11)
                                    //Used do double click on CheckDate field and closed without selecting the value at 
                                    //that time its return empty string to avoid Invalid Date used this below condition compared with empty string
                                    if (event.value_new != "") {
                                        chqpaygrd.records[event.index]['CheckDate'] = event.value_new;
                                    }
                                    else {
                                        chqpaygrd.records[event.index]['CheckDate'] = event.value_previous != "" ? event.value_previous : event.value_original;
                                    }
                                if (event.column == 2)
                                    chqpaygrd.records[event.index]['Description'] = event.value_new;
                            }, 100);
                            //end
                        }
                    })
                    chqpaygrd.hideColumn('recid');
                    chqpaygrd.hideColumn('TransactionType', 'C_BPartner_Location_ID', 'C_DocType_ID', 'DocBaseType');
                };

                function CHQPAY_getControls() {
                    $POP_cmbBank = $chequePayble.find("#VA009_POP_cmbBank_" + $self.windowNo);
                    $POP_cmbBankAccount = $chequePayble.find("#VA009_POP_cmbBankAccount_" + $self.windowNo);
                    $POP_txtAccttNo = $chequePayble.find("#VA009_AccountBalance" + $self.windowNo);
                    $POP_txtChqNo = $chequePayble.find("#VA009_Chqnotxt_" + $self.windowNo);
                    CheuePaybleGrid = $chequePayble.find("#VA009_btnPopupGrid");
                    $POp_cmbPaySelectn = $chequePayble.find("#VA009_POP_cmbPaySelectn_" + $self.windowNo);
                    $pop_cmbCurrencyType = $chequePayble.find("#VA009_POP_cmbCurrencyType_" + $self.windowNo);
                    $POP_PayMthd = $chequePayble.find("#VA009_POP_cmbPaymthd_" + $self.windowNo);
                    $POP_PayMthd.addClass('vis-ev-col-mandatory');
                    $POPtxtCheckNumber = $chequePayble.find("#VA009_POP_textCheckNo_" + $self.windowNo);
                    $POP_DateAcct = $chequePayble.find("#VA009_AccountDate_" + $self.windowNo);
                    $POP_Consolidate = $chequePayble.find("#VA009_chkConsolidate_" + $self.windowNo);
                    //Transaction Date
                    $POP_DateTrx = $chequePayble.find("#VA009_TransactionDate" + $self.windowNo);
                    $POP_cmbOrg = $chequePayble.find("#VA009_POP_cmbOrg_" + $self.windowNo);
                    $POP_cmbOrg.addClass('vis-ev-col-mandatory');
                    //Rakesh(VA228):Store Doc type element in variable
                    $POP_targetDocType = $chequePayble.find("#VA009_POP_cmbDocType_" + $self.windowNo);
                    $POP_targetDocType.addClass('vis-ev-col-mandatory');
                };

                function loadPayMthd() {
                    $POP_PayMthd.empty();
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadChequePaymentMethod", { "Org_ID": $POP_cmbOrg.val() }, callbackloadpaymthds);
                    function callbackloadpaymthds(dr) {
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_PayMthd.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].VA009_PaymentMethod_ID) + ">" + VIS.Utility.encodeText(dr[i].VA009_Name) + "</option>");
                            }
                        }
                        $POP_PayMthd.val(0).prop('selected', true);
                    }
                };

                //to load all organization 
                function loadOrg() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadOrganization", null, callbackloadorg);
                    function callbackloadorg(dr) {
                        $POP_cmbOrg.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_cmbOrg.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].AD_Org_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                            }
                        }
                        $POP_cmbOrg.prop('selectedIndex', 0);
                    };
                };

                function loadgrdPay(callback) {
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetPopUpData",
                        type: "POST",
                        datatype: "json",
                        //contentType: "application/json; charset=utf-8",
                        //async: false,
                        data: ({
                            InvPayids: SlctdPaymentIds.toString(), bank_id: _C_Bank_ID, acctno: _C_BankAccount_ID,
                            chkno: VIS.Utility.encodeText(_Cheque_no), OrderPayids: SlctdOrderPaymentIds.toString(),
                            JournalPayids: SlctdJournalPaymentIds.toString()
                        }),
                        success: function (result) {
                            callback(result);
                        },
                        error: function () {
                            $bsyDiv[0].style.visibility = "hidden";
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                };

                function callbackchqPay(result) {
                    popupgrddata = [];
                    var rslt = JSON.parse(result);
                    reloaddata = rslt;
                    for (var i in rslt) {
                        var line = {};
                        line["recid"] = rslt[i].recid;
                        line["C_Bpartner"] = rslt[i].C_Bpartner;
                        line["C_BPartner_Location_ID"] = rslt[i].C_BPartner_Location_ID;
                        line["C_DocType_ID"] = rslt[i].C_DocType_ID;
                        line["C_Invoice_ID"] = rslt[i].C_Invoice_ID;
                        line["C_BPartner_ID"] = rslt[i].C_BPartner_ID;
                        line["C_InvoicePaySchedule_ID"] = rslt[i].C_InvoicePaySchedule_ID;
                        line["CurrencyCode"] = rslt[i].CurrencyCode;
                        line["DueAmt"] = rslt[i].DueAmt;
                        line["VA009_RecivedAmt"] = rslt[i].DueAmt;
                        line["OverUnder"] = "0";
                        line["Writeoff"] = "0";
                        line["Discount"] = "0";
                        line["ConvertedAmt"] = rslt[i].DueAmt;
                        line["C_Currency_ID"] = rslt[i].C_Currency_ID;
                        line["AD_Org_ID"] = rslt[i].AD_Org_ID;
                        line["AD_Client_ID"] = rslt[i].AD_Client_ID;
                        line["CheckDate"] = new Date();//new Date().toString('dd/MM/yyyy');
                        line["CheckNumber"] = null;
                        line["ValidMonths"] = null;
                        line["VA009_PaymentMode"] = rslt[i].VA009_PaymentMode;
                        line["PaymwentBaseType"] = rslt[i].PaymwentBaseType;
                        line["TransactionType"] = rslt[i].TransactionType;
                        line["DocBaseType"] = rslt[i].DocBaseType;
                        popupgrddata.push(line);
                    }
                    w2utils.encodeTags(popupgrddata);
                    chqpaygrd.add(popupgrddata);
                    $bsyDiv[0].style.visibility = "hidden";
                };

                function callbackchqPayReload(result) {
                    chqpaygrd.clear();
                    popupgrddata = [];
                    var rslt = JSON.parse(result);
                    reloaddata = rslt;
                    for (var i in rslt) {
                        var line = {};
                        line["recid"] = rslt[i].recid;
                        line["C_Bpartner"] = rslt[i].C_Bpartner;
                        line["C_BPartner_Location_ID"] = rslt[i].C_BPartner_Location_ID;
                        line["C_DocType_ID"] = rslt[i].C_DocType_ID;
                        line["C_Invoice_ID"] = rslt[i].C_Invoice_ID;
                        line["C_BPartner_ID"] = rslt[i].C_BPartner_ID;
                        line["C_InvoicePaySchedule_ID"] = rslt[i].C_InvoicePaySchedule_ID;
                        line["CurrencyCode"] = rslt[i].CurrencyCode;
                        line["DueAmt"] = rslt[i].DueAmt;
                        line["VA009_RecivedAmt"] = rslt[i].convertedAmt;
                        line["OverUnder"] = "0";
                        line["Writeoff"] = "0";
                        line["Discount"] = "0";
                        line["ConvertedAmt"] = rslt[i].convertedAmt;
                        line["C_Currency_ID"] = rslt[i].C_Currency_ID;
                        line["AD_Org_ID"] = rslt[i].AD_Org_ID;
                        line["AD_Client_ID"] = rslt[i].AD_Client_ID;
                        line["CheckDate"] = new Date();//new Date().toString('MM/DD/YYYY');
                        line["CheckNumber"] = null;
                        line["ValidMonths"] = null;
                        line["VA009_PaymentMode"] = rslt[i].VA009_PaymentMode;
                        line["PaymwentBaseType"] = rslt[i].PaymwentBaseType;
                        line["TransactionType"] = rslt[i].TransactionType;
                        line["DocBaseType"] = rslt[i].DocBaseType;
                        popupgrddata.push(line);
                    }
                    w2utils.encodeTags(popupgrddata);
                    chqpaygrd.add(popupgrddata);
                };

                function loadCurrencyType() {

                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/loadCurrencyType", null, callbackCurrencyType);

                    function callbackCurrencyType(dr) {
                        $pop_cmbCurrencyType.append("<option value='0'></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $pop_cmbCurrencyType.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                                if (VIS.Utility.encodeText(dr[i].IsDefault) == "Y") {
                                    defaultCurrenyType = VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID);
                                }
                            }
                            //$pop_cmbCurrencyType.prop('selectedIndex', 1);
                            $pop_cmbCurrencyType.val(defaultCurrenyType)
                        }
                    }
                };

                //BY Amit
                //Set Mandatory and Non-Mandatory
                function SetMandatory(value) {
                    if (value)
                        return '#FFB6C1';
                    else
                        return 'White';
                };
                //end

                $POP_cmbOrg.on("change", function () {
                    //to set Org mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) == 0) {
                        $POP_cmbOrg.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        //populate banks based on selected organization in dialog
                        loadbanks($POP_cmbBank, VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()));
                        //refresh the bank and BankAccount dropdowns and make it as mandatory
                        $POP_cmbBank.val(0).prop('selected', true);
                        $POP_cmbBank.addClass('vis-ev-col-mandatory');
                        $POP_cmbBankAccount.empty();
                        $POP_cmbBankAccount.append("<option value='0'></option>");
                        $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                        $POP_cmbOrg.removeClass('vis-ev-col-mandatory');
                        //fetch payment method of selected org
                        loadPayMthd();
                    }
                    //Rakesh(VA228):Reset Document type
                    _loadFunctions.LoadTargetDocType($POP_targetDocType, _TargetBaseType);
                });

                $POP_cmbBank.on("change", function () {
                    $POP_cmbBankAccount.empty();
                    //to set bank mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBank.val()) > 0) {
                        $POP_cmbBank.removeClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_cmbBank.addClass('vis-ev-col-mandatory');
                        $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                    }
                    //end
                    cheqAmount.setValue(0);
                    $POP_txtChqNo.val('');
                    //to get bank account of selected organization assigned by Ashish on 28 May 2020
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadBankAccount", { "Bank_ID": $POP_cmbBank.val(), "Orgs": $POP_cmbOrg.val() }, callbackloadbankAcct);

                    function callbackloadbankAcct(dr) {

                        $POP_cmbBankAccount.append("<option value='0'></option>");
                        if (dr != null) {
                            if (dr.length > 0) {
                                for (var i in dr) {
                                    $POP_cmbBankAccount.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_BankAccount_ID) + ">" + VIS.Utility.encodeText(dr[i].AccountNo) + "</option>");
                                }
                            }
                        }
                    }
                });

                $POP_cmbBankAccount.on("change", function () {
                    //to set bank mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()) > 0) {
                        $POP_cmbBankAccount.removeClass('vis-ev-col-mandatory');
                        $POP_PayMthd.val(0); //(1052)empty payment method on the selection of bank account 
                    }
                    else {
                        $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                    }
                    //end
                    //check weather Process ID binded or not on Bank Account Document tab in Bank window
                    if ($("#VA009_POP_cmbPaySelectn_" + $self.windowNo)[0].value != null && $("#VA009_POP_cmbPaySelectn_" + $self.windowNo)[0].value == "P") {
                        var _process_Id = VIS.dataContext.getJSONRecord("VA009/Payment/GetProcessId", $POP_cmbBankAccount.val());
                        if (!_process_Id) {
                            VIS.ADialog.info("VA009_Plz_Process_IDNotFndBnkActDoc");
                            $POp_cmbPaySelectn.val("M");
                            return false;
                        }
                    }

                    cheqAmount.setValue(0);
                    $POP_txtChqNo.val('');

                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetBankAccountData", { "BankAccount": $POP_cmbBankAccount.val() }, callbackloadbankAcctBal);

                    function callbackloadbankAcctBal(dr) {

                        //change by amit
                        if ($POP_cmbBankAccount.val() == "0") {
                            $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_cmbBankAccount.removeClass('vis-ev-col-mandatory');
                        }
                        //end

                        if (dr != null) {
                            // $POP_txtAccttNo.val(Globalize.format(dr["CurrentBalance"], "N"));
                            //$POP_txtAccttNo.val(parseFloat(dr["CurrentBalance"]).toLocaleString());
                            cheqAmount.setValue(dr["CurrentBalance"]);
                            //$POP_txtChqNo.val(dr["CurrentNext"]); //donot display check no here
                        }
                        //}
                        //dr.dispose();                   

                        // by amit if no record found, then Account Balance and cheque No as 0
                        if (dr == null) {
                            //$POP_txtAccttNo.val("0");
                            cheqAmount.setValue();
                            //$POP_txtChqNo.val("0");// donot display check no here
                        }
                    }

                    //end
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetConvertedAmt",
                        type: "POST",
                        datatype: "json",
                        // contentType: "application/json; charset=utf-8",
                        async: true,
                        data: ({ PaymentData: JSON.stringify(reloaddata), BankAccount: $POP_cmbBankAccount.val(), CurrencyType: $pop_cmbCurrencyType.val(), dateAcct: $POP_DateAcct.val(), _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                        success: function (result) {
                            callbackchqReload(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                });

                $pop_cmbCurrencyType.on("change", function () {
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetConvertedAmt",
                        type: "POST",
                        datatype: "json",
                        // contentType: "application/json; charset=utf-8",
                        async: true,
                        data: ({ PaymentData: JSON.stringify(reloaddata), BankAccount: $POP_cmbBankAccount.val(), CurrencyType: $pop_cmbCurrencyType.val(), dateAcct: $POP_DateAcct.val(), _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                        success: function (result) {
                            callbackchqReload(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                });

                $POP_Consolidate.on("click", function (e) {
                    var target = $(e.target);
                    if (e.target.type == 'checkbox') {
                        if (target.prop("checked") == true) {
                            $POPtxtCheckNumber.removeAttr("disabled");
                            $POPtxtCheckNumber.css('background-color', 'white');
                            $POPtxtCheckNumber.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            //$('#VA009_POP_textCheckNoDiv').css('display', 'none');
                            $POPtxtCheckNumber.val("");
                            $POPtxtCheckNumber.attr('disabled', 'disabled');
                            $POPtxtCheckNumber.removeClass('vis-ev-col-mandatory');
                            $POPtxtCheckNumber.css('background-color', '#ededed');
                        }
                    }

                });

                /*payment method change event - set current Next checkno*/
                $POP_PayMthd.on("change", function (e) {
                    chknumbers = [];// VIS_427 clearing array on change of payment method
                    removedcheck = [];
                    autocheckCtrl = null;
                    $POP_Consolidate.prop("checked", false);
                    $POPtxtCheckNumber.val("");
                    $POPtxtCheckNumber.attr('disabled', 'disabled');
                    if (VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()) > 0) {
                        $POP_PayMthd.removeClass('vis-ev-col-mandatory');
                        if (VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()) == 0) {
                            //if bank account is not selected
                            VIS.ADialog.info("VA009_PLSelectBankAccount", "");
                            return false;

                        }
                        var result = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetBankAccountCheckNo", {
                            "BankAccount": $POP_cmbBankAccount.val(),
                            "PaymentMethod": $POP_PayMthd.val()
                        });
                        if (result != null) {
                            //VIS_427 1st check is populated to current nex field
                            $POP_txtChqNo.val(VIS.Utility.Util.getValueOfInt(result[0]["currentnext"]));
                            autocheckCtrl = result[0]["chknoautocontrol"];// Value handled for true/false
                            for (i = 0; result.length > 0; i++) {
                                //VIS_427 variable deined to get all check numbers to array which starts from current next to end check number according to result set
                                var checkdifference = VIS.Utility.Util.getValueOfInt(result[i]["endchknumber"]) - VIS.Utility.Util.getValueOfInt(result[i]["currentnext"]);
                                for (j = 0; j <= checkdifference; j++) {
                                    chknumbers.push(VIS.Utility.Util.getValueOfInt(result[i]["currentnext"]) + j);
                                }
                            }

                        }
                        else {
                            $POP_txtChqNo.val("0");
                            VIS.ADialog.info("VA009_NoCheckNum", "")
                            return false;
                        }

                    }
                    else {
                        $POP_PayMthd.addClass('vis-ev-col-mandatory');
                    }
                });

                $POPtxtCheckNumber.on("change", function () {
                    //VIS_427 Bug id 2339 cleared the  check number field in grid 
                    for (var k = 0; k < chqpaygrd.records.length; k++) {
                        chqpaygrd.records[k]['CheckNumber'] = '';
                        chqpaygrd.refreshCell(chqpaygrd.records[k].recid, "CheckNumber");
                    }
                    if ($POPtxtCheckNumber.val() == "") {
                        $POPtxtCheckNumber.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POPtxtCheckNumber.removeClass('vis-ev-col-mandatory');
                        CheuePaybleGrid = $chequePayble.find("#VA009_btnPopupGrid");
                        var cBPartnerIds = [];
                        if (removedcheck.length > 0) {
                            /*VIS_427 Bug id 2339 getting those check which are loaded on grid when user change check number field so that
                            if user again use those check they can again appear on grid if payment not done*/
                            for (i = 0; i < removedcheck.length; i++) {
                                chknumbers.push(removedcheck[i]);
                            }
                            chknumbers.sort(function (a, b) { return a - b; });
                            removedcheck = [];

                        }
                        for (var i = 0; i < chqpaygrd.records.length; i++) {
                            if (cBPartnerIds.indexOf(chqpaygrd.records[i]["C_BPartner_ID"]) < 0) {
                                cBPartnerIds.push(chqpaygrd.records[i]["C_BPartner_ID"]);
                                //chknumbers.push($POPtxtCheckNumber.val());
                                if (i == 0) {
                                    chqpaygrd.records[i]['CheckNumber'] = VIS.Utility.Util.getValueOfInt($POPtxtCheckNumber.val());
                                    //VIS_427 Handled case for auto check funtinality otherwise the system should generate check as earlier
                                    if (autocheckCtrl == "Y") {
                                        removedcheck = jQuery.grep(chknumbers, function (value) {
                                            return value <= VIS.Utility.Util.getValueOfInt(chqpaygrd.records[i]['CheckNumber']);
                                        });
                                        chknumbers = jQuery.grep(chknumbers, function (value) {
                                            return value > VIS.Utility.Util.getValueOfInt(chqpaygrd.records[i]['CheckNumber']);
                                        });
                                    }
                                    else {
                                        chknumbers.push($POPtxtCheckNumber.val());
                                    }
                                }
                                else {
                                    //VIS_427 When all check allocated and no check left for next schedule then populated message
                                    if (chknumbers.length == 0) {
                                        VIS.ADialog.info(("VA009_CheckNotAligned"));
                                        return false;
                                    }
                                    if (autocheckCtrl == "Y") {
                                        chqpaygrd.records[i]['CheckNumber'] = VIS.Utility.Util.getValueOfInt(chknumbers[chknumbers.length - chknumbers.length]);
                                        removedcheck.push(chqpaygrd.records[i]['CheckNumber']);
                                        chknumbers = jQuery.grep(chknumbers, function (value) {
                                            return value != VIS.Utility.Util.getValueOfInt(chqpaygrd.records[i]['CheckNumber']);
                                        });
                                    }
                                    else {
                                        chqpaygrd.records[i]['CheckNumber'] = VIS.Utility.Util.getValueOfInt(chknumbers[chknumbers.length - 1]) + 1;
                                        chknumbers.push(VIS.Utility.Util.getValueOfInt(chknumbers[chknumbers.length - 1]) + 1);
                                    }
                                }
                                //$POPtxtCheckNumber.val(VIS.Utility.Util.getValueOfInt(chknumbers[chknumbers.length - 1]) + 1);
                            }
                            else if (cBPartnerIds.indexOf(chqpaygrd.records[i]["C_BPartner_ID"]) > -1) {
                                chqpaygrd.records[i]['CheckNumber'] = chknumbers[cBPartnerIds.indexOf(chqpaygrd.records[i]["C_BPartner_ID"])];
                            }
                            else
                                console.log("Skip");
                           
                            chqpaygrd.refreshCell(chqpaygrd.records[i].recid, "CheckNumber");
                        }
                        console.log("BPartner IDS =  " + cBPartnerIds);
                        //chqpaygrd.records[event.index]['VA009_RecivedAmt'] = event.value_new;
                    }
                    //end
                });

                $POp_cmbPaySelectn.on("change", function () {
                    if ($POp_cmbPaySelectn.val() == "P") {
                        //var obj;
                        $POP_Consolidate.prop('cheched', false);
                        $POP_Consolidate.attr('disabled', 'disabled');
                        $POPtxtCheckNumber.val("");
                        for (var i = 0; i < chqpaygrd.records.length; i++) {
                            if (chqpaygrd.records[i]['TransactionType'] == "Order" || chqpaygrd.records[i]['TransactionType'] == "GL Journal") {
                                VIS.ADialog.info("VA009_CannotPrintOrder");
                                $POp_cmbPaySelectn.val("M");
                                return false;
                            }
                            chqpaygrd.records[i]['CheckNumber'] = "";
                            //to set Current Date as checkDate
                            //chqpaygrd.records[i]['CheckDate'] = null;
                            chqpaygrd.records[i]['CheckDate'] = new Date();
                            chqpaygrd.refreshCell(chqpaygrd.records[i].recid, "CheckNumber");
                            chqpaygrd.refreshCell(chqpaygrd.records[i].recid, "CheckDate");
                        }
                        //Check Process_ID is bind on Bank Account Document window or not!
                        var paramString = $("#VA009_POP_cmbBankAccount_" + $self.windowNo)[0].value > 0 ? $("#VA009_POP_cmbBankAccount_" + $self.windowNo)[0].value : 0;
                        if (paramString) {
                            var _process_Id = VIS.dataContext.getJSONRecord("VA009/Payment/GetProcessId", paramString);
                            if (!_process_Id) {
                                VIS.ADialog.info("VA009_Plz_Process_IDNotFndBnkActDoc");
                                $POp_cmbPaySelectn.val("M");
                                return false;
                            }
                        }
                        else {
                            VIS.ADialog.info("VA009_PLSelectBankAccount");
                            $POp_cmbPaySelectn.val("M");
                            return false;
                        }
                    }
                    else {
                        $POP_Consolidate.removeAttr('disabled');
                    }
                });

                $POP_DateAcct.on("change", function () {
                    //to set Date acct mandatory given by ashish on 28 May 2020
                    if ($POP_DateAcct.val() == "") {
                        $POP_DateAcct.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_DateAcct.removeClass('vis-ev-col-mandatory');
                    }
                    //Devops Task Id- 1636
                    //VIS317  Mandatory Field validation for Account date change.
                    //Case Of Check button.
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PlsSelectOrg"));
                        return false;
                    }
                    if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PlsSelectDocumentType"));
                        return false;
                    }
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBank.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PlsSelectBank"));
                        return false;
                    }
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PlsSelectBankAccount"));
                        return false;
                    }
                    if (VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PlsSelectPaymentMethod"));
                        return false;
                    }
                    if ($POP_DateAcct.val() == "") {
                        VIS.ADialog.info(("VA009_PLSelectAcctDate"));
                        return false;
                    }
                    //VIS317 Invalid Account date check
                    var dateVal = Date.parse($POP_DateAcct.val());
                    var currentTime = new Date(parseInt(dateVal));
                    //check if date is valid
                    //this check will work for 01/01/1970 on words
                    if (!isNaN(currentTime.getTime()) && currentTime.getTime() < 0) {
                        return;
                    }
                    //used ajax call to get Converted Amount
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetConvertedAmt",
                        type: "POST",
                        datatype: "json",
                        // contentType: "application/json; charset=utf-8",
                        async: true,
                        data: ({ PaymentData: JSON.stringify(reloaddata), BankAccount: $POP_cmbBankAccount.val(), CurrencyType: $pop_cmbCurrencyType.val(), dateAcct: $POP_DateAcct.val(), _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                        success: function (result) {
                            callbackchqReload(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                });

                $POP_targetDocType.on("change", function () {
                    //to set target doc type mandatory
                    if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) == 0) {
                        $POP_targetDocType.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_targetDocType.removeClass('vis-ev-col-mandatory');
                    }
                });
                function callbackchqReload(result) {
                    chqpaygrd.clear();
                    popupgrddata = [];
                    var rslt = JSON.parse(result);
                    reloaddata = rslt;
                    for (var i in rslt) {
                        var line = {};
                        line["recid"] = rslt[i].recid;
                        line["C_Bpartner"] = rslt[i].C_Bpartner;
                        line["C_BPartner_Location_ID"] = rslt[i].C_BPartner_Location_ID;
                        line["C_DocType_ID"] = rslt[i].C_DocType_ID;
                        line["C_Invoice_ID"] = rslt[i].C_Invoice_ID;
                        line["C_BPartner_ID"] = rslt[i].C_BPartner_ID;
                        line["C_InvoicePaySchedule_ID"] = rslt[i].C_InvoicePaySchedule_ID;
                        line["CurrencyCode"] = rslt[i].CurrencyCode;
                        line["DueAmt"] = rslt[i].DueAmt;
                        line["VA009_RecivedAmt"] = rslt[i].convertedAmt;
                        line["OverUnder"] = "0";
                        line["Writeoff"] = "0";
                        line["Discount"] = "0";
                        line["ConvertedAmt"] = rslt[i].convertedAmt;
                        line["C_Currency_ID"] = rslt[i].C_Currency_ID;
                        line["AD_Org_ID"] = rslt[i].AD_Org_ID;
                        line["AD_Client_ID"] = rslt[i].AD_Client_ID;
                        line["CheckDate"] = new Date();//new Date().toString('MM/DD/YYYY');
                        line["CheckNumber"] = null;
                        line["ValidMonths"] = null;
                        line["VA009_PaymentMode"] = rslt[i].VA009_PaymentMode;
                        line["PaymwentBaseType"] = rslt[i].PaymwentBaseType;
                        line["TransactionType"] = rslt[i].TransactionType;
                        line["DocBaseType"] = rslt[i].DocBaseType;
                        popupgrddata.push(line);
                    }
                    if (rslt[0].ERROR == "ConversionNotFound") {
                        VIS.ADialog.info("VA009_ConversionNotFound");
                    }
                    w2utils.encodeTags(popupgrddata);
                    chqpaygrd.add(popupgrddata);
                };

                ChequePayDialog.onOkClick = function (event) {
                    var _CollaborateData = [];
                    chqpaygrd.selectAll();
                    var total = 0;
                    if (chqpaygrd.getSelection().length > 0) {
                        if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) > 0) {
                            //Rakesh(VA228):Make Document Type selection mandatory
                            if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) > 0) {
                                if (VIS.Utility.Util.getValueOfInt($POP_cmbBank.val()) > 0) {
                                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()) > 0) {
                                        if (VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()) > 0) {
                                            if ($POP_DateAcct.val() != "" && $POP_DateAcct.val() != null) {
                                                for (var i = 0; i < chqpaygrd.getSelection().length; i++) {
                                                    var _data = {}; var updated = 0;
                                                    _data["C_BPartner_ID"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['C_BPartner_ID'];
                                                    _data["Description"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['Description'];
                                                    _data["C_Invoice_ID"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['C_Invoice_ID'];
                                                    _data["AD_Org_ID"] = VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val());
                                                    //Rakesh(VA228):Set Document Type (13/Sep/2021)
                                                    _data["TargetDocType"] = VIS.Utility.Util.getValueOfInt($POP_targetDocType.val());
                                                    _data["AD_Client_ID"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['AD_Client_ID'];
                                                    _data["C_InvoicePaySchedule_ID"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['C_InvoicePaySchedule_ID'];
                                                    _data["C_BankAccount_ID"] = VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val());
                                                    _data["C_Currency_ID"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['C_Currency_ID'];
                                                    _data["DueAmt"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['DueAmt'];
                                                    _data["ConvertedAmt"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['ConvertedAmt'];
                                                    _data["PaymwentBaseType"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['PaymwentBaseType'];
                                                    _data["TransactionType"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['TransactionType'];
                                                    _data["C_BPartner_Location_ID"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['C_BPartner_Location_ID'];
                                                    _data["C_DocType_ID"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['C_DocType_ID'];
                                                    _data["DocBaseType"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['DocBaseType'];
                                                    //validate zero also, if the value is zero it will ask to enter the recieved amount
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqpaygrd.get(chqpaygrd.getSelection()[i])['VA009_RecivedAmt']) != 0) {
                                                        _data["VA009_RecivedAmt"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['VA009_RecivedAmt'];
                                                    }
                                                    else {
                                                        VIS.ADialog.info(("VA009_PLPayAmt"));
                                                        chqpaygrd.selectNone();
                                                        return false;
                                                    }

                                                    _data["OverUnder"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['OverUnder'];
                                                    _data["Writeoff"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['Writeoff'];
                                                    _data["Discount"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['Discount'];
                                                    //Transaction Date
                                                    _data["DateTrx"] = VIS.Utility.Util.getValueOfDate($POP_DateTrx.val());
                                                    _data["DateAcct"] = VIS.Utility.Util.getValueOfDate($POP_DateAcct.val());
                                                    _data["From"] = "Pay";

                                                    if ($POp_cmbPaySelectn.val() == "M") {
                                                        if (chqpaygrd.get(chqpaygrd.getSelection()[i])['CheckDate'] != "" && chqpaygrd.get(chqpaygrd.getSelection()[i])['CheckDate'] != null) {
                                                            var dt = new Date(chqpaygrd.get(chqpaygrd.getSelection()[i])['CheckDate']);
                                                            dt = new Date(dt.setHours(0, 0, 0, 0));
                                                            var acctdate = new Date($POP_DateAcct.val());
                                                            acctdate = new Date(acctdate.setHours(0, 0, 0, 0));
                                                            if (dt > acctdate) {
                                                                VIS.ADialog.info(("VIS_CheckDateCantbeGreaterSys"));
                                                                chqpaygrd.selectNone();
                                                                return false;
                                                            }
                                                            _data["CheckDate"] = VIS.Utility.Util.getValueOfDate(chqpaygrd.get(chqpaygrd.getSelection()[i])['CheckDate']);
                                                        }
                                                        else {
                                                            VIS.ADialog.info(("VA009_PLCheckDate"));
                                                            chqpaygrd.selectNone();
                                                            return false;
                                                        }

                                                        if (chqpaygrd.get(chqpaygrd.getSelection()[i])['CheckNumber'] != "" && chqpaygrd.get(chqpaygrd.getSelection()[i])['CheckNumber'] != null)
                                                            _data["CheckNumber"] = chqpaygrd.get(chqpaygrd.getSelection()[i])['CheckNumber'];
                                                        else {
                                                            VIS.ADialog.info(("VA009_PLCheckNumber"));
                                                            chqpaygrd.selectNone();
                                                            return false;
                                                        }
                                                    }

                                                    if (chqpaygrd.get(chqpaygrd.getSelection()[i])['VA009_RecivedAmt'] == 0) {
                                                        VIS.ADialog.info(("VA009_PLRecivedAmt"));
                                                        chqpaygrd.selectNone();
                                                        return false;
                                                    }
                                                    _data["CurrencyType"] = $pop_cmbCurrencyType.val();
                                                    _data["VA009_PaymentMethod_ID"] = $POP_PayMthd.val();
                                                    var obj;
                                                    if (i > 0) {
                                                        obj = $.grep(_CollaborateData, function (e) {
                                                            return chqpaygrd.get(chqpaygrd.getSelection()[i])['C_BPartner_ID'] == e.C_BPartner_ID
                                                                && chqpaygrd.get(chqpaygrd.getSelection()[i])['CheckNumber'] == e.CheckNumber
                                                                && chqpaygrd.get(chqpaygrd.getSelection()[i])['TransactionType'] != e.TransactionType;
                                                        });
                                                        if (obj) {
                                                            //VIS_427 DevOps TaskId: 2156 Condition added For order when their is different Transaction
                                                            if (obj.length > 0 && chqpaygrd.get(chqpaygrd.getSelection()[i])['TransactionType'] == "Order") {
                                                                VIS.ADialog.info("VA009_SameCheckNo");
                                                                chqpaygrd.selectNone();
                                                                return false;
                                                            }
                                                        }
                                                    }
                                                    _CollaborateData.push(_data);
                                                }
                                            }
                                            else {
                                                VIS.ADialog.info(("VA009_PLSelectAcctDate"));
                                                chqpaygrd.selectNone();
                                                return false;
                                            }
                                        }
                                        else {
                                            VIS.ADialog.info(("VA009_PLSelectPaymentMethod"));
                                            chqpaygrd.selectNone();
                                            return false;
                                        }
                                    }
                                    else {
                                        VIS.ADialog.info(("VA009_PLSelectBankAccount"));
                                        chqpaygrd.selectNone();
                                        return false;
                                    }
                                }
                                else {
                                    VIS.ADialog.info(("VA009_PLSelectBank"));
                                    chqpaygrd.selectNone();
                                    return false;
                                }
                            } else {
                                VIS.ADialog.info(("VA009_PlsSelectDocumentType"));
                                chqpaygrd.selectNone();
                                return false;
                            }
                        }
                        else {
                            VIS.ADialog.info(("VA009_PlsSelectOrg"));
                            chqpaygrd.selectNone();
                            return false;
                        }
                    }
                    if ($POp_cmbPaySelectn.val() == "M") {
                        $bsyDiv[0].style.visibility = "visible";
                        $.ajax({
                            url: VIS.Application.contextUrl + "VA009/Payment/GeneratePayments",
                            type: "POST",
                            datatype: "json",
                            async: true,
                            data: ({ PaymentData: JSON.stringify(_CollaborateData) }),
                            success: function (result) {
                                callbackPaymnt(result);
                            },
                            error: function (ex) {
                                console.log(ex);
                                $bsyDiv[0].style.visibility = "hidden";
                                VIS.ADialog.error("VA009_ErrorLoadingPayments");
                            }
                        });
                    }
                    else if ($POp_cmbPaySelectn.val() == "P") {
                        //if (total < 0) {
                        //    !VIS.ADialog.confirm("VA009_ContinuePrint?", true, "", "Confirm", function (result) {
                        //        if (result) {
                        //            $bsyDiv[0].style.visibility = "visible";
                        //            $.ajax({
                        //                url: VIS.Application.contextUrl + "VA009/Payment/GeneratePayments",
                        //                type: "POST",
                        //                datatype: "json",
                        //                async: true,
                        //                data: ({ PaymentData: JSON.stringify(_CollaborateData) }),
                        //                success: function (result) {
                        //                    callbackPaymnt(result);
                        //                    ChequePayDialog.close();
                        //                },
                        //                error: function (ex) {
                        //                    console.log(ex);
                        //                    $bsyDiv[0].style.visibility = "hidden";
                        //                    VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        //                }
                        //            });
                        //        }
                        //        else {
                        //            chqpaygrd.selectNone();
                        //            return false;
                        //        }

                        //    });
                        //    return false;
                        //}
                        //else {
                        $bsyDiv[0].style.visibility = "visible";
                        $.ajax({
                            url: VIS.Application.contextUrl + "VA009/Payment/GeneratePaymentsBatchOfRule",
                            type: "POST",
                            datatype: "json",
                            async: true,
                            data: ({ PaymentData: JSON.stringify(_CollaborateData) }),
                            success: function (result) {
                                callbackPaymntonRule(result);
                            },
                            error: function (ex) {
                                console.log(ex);
                                $bsyDiv[0].style.visibility = "hidden";
                                VIS.ADialog.error("VA009_ErrorLoadingPayments");
                            }
                        });
                    }
                    //}
                };

                function callbackPaymnt(result) {
                    result = JSON.parse(result);
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; SlctdJournalPaymentIds = []; batchObjInv = []; batchObjOrd = []; batchObjJournal = [];
                    resetPaging();
                    //after successfully created Payment selectall checkbox should be false
                    $selectall.prop('checked', false);
                    //loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
                    loadPaymetsAll();
                    clearamtid();
                    $bsyDiv[0].style.visibility = "hidden";
                    VIS.ADialog.info("", null, result, null);
                    //w2alert(result.toString());
                };

                function callbackPaymntonRule(result) {
                    result = JSON.parse(result);
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; SlctdJournalPaymentIds = []; batchObjInv = []; batchObjOrd = []; batchObjJournal = [];
                    resetPaging();
                    //after successfully created Payment selectall checkbox should be false
                    $selectall.prop('checked', false);
                    //loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
                    loadPaymetsAll();
                    clearamtid();
                    $bsyDiv[0].style.visibility = "hidden";
                    if (result != "") {
                        VIS.ADialog.info("", null, result, null);
                    }
                    else {
                        var cprint = new VIS.Apps.AForms.VPayPrint();
                        var c = new VIS.CFrame();
                        c.setName(VIS.Msg.getMsg("Check Print"));
                        c.setTitle(VIS.Msg.getMsg("Check Print"));
                        c.hideHeader(true);
                        c.setContent(cprint);
                        c.show();
                        cprint.initialize();
                    }
                };

                ChequePayDialog.onCancelCLick = function () {
                    chqpaygrd.clear();
                    ChequePayDispose();
                };

                ChequePayDialog.onClose = function () {
                    chqpaygrd.clear();
                    ChequePayDispose();

                };
                //reset for Amount Field...
                this.vetoablechange = function (evt) {
                    if (evt.propertyName == "VA009_AccountBalance" + $self.windowNo + "") {
                        cheqAmount.setValue(evt.newValue);
                    }
                };
                function ChequePayDispose() {
                    _Cheque = null;
                    $cheque = null;
                    $chequePayble = null;
                    STAT_cmbBank = null;
                    STAT_cmbBankAccount = null;
                    STAT_txtStatementNo = null;
                    STAT_ctrlLoadFile = null;
                    STAT_ctrlLoadFileTxt = null;
                    chqpaygrd.destroy();
                }
            },

            Cheque_Rec_Dialog: function () {
                CheueRecevableGrid, _Cheque_no = "";
                var _C_Bank_ID = 0, _C_BankAccount_ID = 0;
                $bsyDiv[0].style.visibility = "visible";
                $chequeRecivable = $("<div class='VA009-popform-content vis-formouterwrpdiv' style='min-height:385px !important'>");
                var _ChequeRecevble = "";
                _ChequeRecevble += "<div class='VA009-popfrm-wrap' style='height:auto;'>"
                    + "<div class='VA009-popCheck-data input-group vis-input-wrap' > <div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbOrg_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_Org") + "</label>"
                    + "</div></div> "

                    //Rakesh(VA228):Create Doc type element html
                    + "<div class='VA009-popCheck-data input-group vis-input-wrap' > <div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbDocType_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_DocType") + "</label>"
                    + "</div></div> "

                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbPaymthd_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_PayMthd") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popCheck-data input-group vis-input-wrap' > <div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbBank_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_Bank") + "</label>"
                    + "</div></div> "

                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbBankAccount_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_BankAccount") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbCurrencyType_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_CurrencyType") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_AccountDate_" + $self.windowNo + "'><label>" + VIS.Msg.getMsg("AccountDate") + "</label>"
                    + " </div></div>"
                    //Transaction Date
                    + "<div class='VA009-popCheck-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_TransactionDate" + $self.windowNo + "' ><label>" + VIS.Msg.getMsg("TransactionDate") + "</label>"
                    + "</div></div>"

                    //+ "<div class='VA009-popfrm-wrap'>"
                    + "<div class='VA009-grid-container'><div class='VA009-table-container' id='VA009_btnPopupRecGrid'></div></div>"
                //+ "</div>";

                $chequeRecivable.append(_ChequeRecevble);
                CHQREC_getControls();
                var now = new Date();
                var _today = now.getFullYear() + "-" + (("0" + (now.getMonth() + 1)).slice(-2)) + "-" + (("0" + now.getDate()).slice(-2));
                $POP_DateAcct.val(_today);
                $POP_DateTrx.val(_today);
                ChequeReceDialog = new VIS.ChildDialog();
                ChequeReceDialog.setContent($chequeRecivable);
                ChequeReceDialog.setTitle(VIS.Msg.getMsg("VA009_LoadChequePaymentRec"));
                ChequeReceDialog.setWidth("80%");
                ChequeReceDialog.setEnableResize(true);
                ChequeReceDialog.setModal(true);
                if (SlctdPaymentIds.toString() != "" || SlctdOrderPaymentIds.toString() != "" || SlctdJournalPaymentIds.toString() != "") {
                    // Added by Bharat on 01/May/2017
                    callbackCashReceipt("");
                }

                function callbackCashReceipt(data) {
                    if (data != "") {
                        $bsyDiv[0].style.visibility = "hidden";
                        VIS.ADialog.info(("VA009_PleaseSelctonlyCheque"));
                    }
                    else {
                        ChequeReceDialog.show();
                        CHQRecGrid_Layout();
                        //loadbank();
                        //populate banks based on selected org in dialog
                        loadbanks($RPOP_cmbBank, VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()));
                        $RPOP_cmbBank.addClass('vis-ev-col-mandatory');
                        $RPOP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                        if ($POP_DateAcct.val()) {
                            $POP_DateAcct.removeClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_DateAcct.addClass('vis-ev-col-mandatory');
                        }
                        loadgrd(callbackchqRec);
                        loadCurrencyType();
                        loadPayMthd();
                        loadOrg();
                        //Rakesh(VA228):10/Sep/2021 -> Load APR target base doc type
                        _loadFunctions.LoadTargetDocType($POP_targetDocType, _TargetBaseType);
                    }
                };

                function CHQREC_getControls() {
                    $RPOP_cmbBank = $chequeRecivable.find("#VA009_POP_cmbBank_" + $self.windowNo);
                    $RPOP_cmbBankAccount = $chequeRecivable.find("#VA009_POP_cmbBankAccount_" + $self.windowNo);
                    $POP_PayMthd = $chequeRecivable.find("#VA009_POP_cmbPaymthd_" + $self.windowNo);
                    CheueRecevableGrid = $chequeRecivable.find("#VA009_btnPopupRecGrid");
                    $pop_cmbCurrencyType = $chequeRecivable.find("#VA009_POP_cmbCurrencyType_" + $self.windowNo);
                    $POP_DateAcct = $chequeRecivable.find("#VA009_AccountDate_" + $self.windowNo);
                    $POP_DateTrx = $chequeRecivable.find("#VA009_TransactionDate" + $self.windowNo);
                    $POP_cmbOrg = $chequeRecivable.find("#VA009_POP_cmbOrg_" + $self.windowNo);
                    $POP_cmbOrg.addClass('vis-ev-col-mandatory');
                    //Rakesh(VA228):Store Doc type element in variable
                    $POP_targetDocType = $chequeRecivable.find("#VA009_POP_cmbDocType_" + $self.windowNo);
                    $POP_targetDocType.addClass('vis-ev-col-mandatory');
                };

                function loadCurrencyType() {

                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/loadCurrencyType", null, callbackCurrencyType);

                    function callbackCurrencyType(dr) {

                        $pop_cmbCurrencyType.append("<option value='0'></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $pop_cmbCurrencyType.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                                if (VIS.Utility.encodeText(dr[i].IsDefault) == "Y") {
                                    defaultCurrenyType = VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID);
                                }
                            }
                            //$pop_cmbCurrencyType.prop('selectedIndex', 1);
                            $pop_cmbCurrencyType.val(defaultCurrenyType);
                        }
                    }
                };

                function loadPayMthd() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadChequePaymentMethod", { "Org_ID": null }, callbackloadpaymthds);
                    function callbackloadpaymthds(dr) {
                        if (dr.length > 1)
                            $POP_PayMthd.append(" <option value = 0></option>");

                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_PayMthd.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].VA009_PaymentMethod_ID) + ">" + VIS.Utility.encodeText(dr[i].VA009_Name) + "</option>");
                            }
                        }
                        $POP_PayMthd.prop('selectedIndex', 0);
                    }
                };

                //to load all organization 
                function loadOrg() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadOrganization", null, callbackloadorg);
                    function callbackloadorg(dr) {
                        $POP_cmbOrg.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_cmbOrg.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].AD_Org_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                            }
                        }
                        $POP_cmbOrg.prop('selectedIndex', 0);
                    };
                };

                //BY Amit
                //Set Mandatory and Non-Mandatory
                function SetMandatory(value) {
                    if (value)
                        return '#FFB6C1';
                    else
                        return 'White';
                };
                //end

                $POP_cmbOrg.on("change", function () {
                    //to set Org mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) == 0) {
                        $POP_cmbOrg.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        //populate banks based on selected organization in dialog
                        loadbanks($RPOP_cmbBank, VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()));
                        //refresh the bank and BankAccount dropdowns and make it as mandatory
                        $RPOP_cmbBank.val(0).prop('selected', true);
                        $RPOP_cmbBank.addClass('vis-ev-col-mandatory');
                        $RPOP_cmbBankAccount.empty();
                        $RPOP_cmbBankAccount.append("<option value='0'></option>");
                        $RPOP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                        $POP_cmbOrg.removeClass('vis-ev-col-mandatory');
                    }
                    //Rakesh(VA228):Reset Document type
                    _loadFunctions.LoadTargetDocType($POP_targetDocType, _TargetBaseType);
                });

                $RPOP_cmbBank.on("change", function () {
                    $RPOP_cmbBankAccount.empty();
                    chknumbers = [];
                    removedcheck = [];
                    autocheckCtrl = null;
                    //to set Org mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($RPOP_cmbBank.val()) == 0) {
                        $RPOP_cmbBank.addClass('vis-ev-col-mandatory');
                        $RPOP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $RPOP_cmbBank.removeClass('vis-ev-col-mandatory');
                    }
                    //end
                    //to get bank account of selected organization assigned by Ashish on 28 May 2020
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadBankAccount", { "Bank_ID": $RPOP_cmbBank.val(), "Orgs": $POP_cmbOrg.val() }, callbackloadbankAcct);

                    function callbackloadbankAcct(dr) {

                        $RPOP_cmbBankAccount.append("<option value='0'></option>");
                        if (dr != null) {
                            if (dr.length > 0) {
                                for (var i in dr) {
                                    $RPOP_cmbBankAccount.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_BankAccount_ID) + ">" + VIS.Utility.encodeText(dr[i].AccountNo) + "</option>");
                                }
                            }
                        }
                    }
                });

                $RPOP_cmbBankAccount.on("change", function () {
                    removedcheck = [];
                    chknumbers = [];
                    autocheckCtrl = null;
                    //to set Org mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($RPOP_cmbBankAccount.val()) == 0) {
                        $RPOP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $RPOP_cmbBankAccount.removeClass('vis-ev-col-mandatory');
                    }
                    //end
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetConvertedAmt",
                        type: "POST",
                        datatype: "json",
                        // contentType: "application/json; charset=utf-8",
                        async: true,
                        data: ({ PaymentData: JSON.stringify(reloaddata), BankAccount: $RPOP_cmbBankAccount.val(), CurrencyType: $pop_cmbCurrencyType.val(), dateAcct: $POP_DateAcct.val(), _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                        success: function (result) {
                            callbackchqReload(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                });

                $pop_cmbCurrencyType.on("change", function () {
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetConvertedAmt",
                        type: "POST",
                        datatype: "json",
                        // contentType: "application/json; charset=utf-8",
                        async: true,
                        //passed Org and DateAcct parameters to get the data according to the updated values
                        data: ({ PaymentData: JSON.stringify(reloaddata), BankAccount: $RPOP_cmbBankAccount.val(), CurrencyType: $pop_cmbCurrencyType.val(), dateAcct: $POP_DateAcct.val(), _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                        success: function (result) {
                            callbackchqReload(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                });

                $POP_DateAcct.on("change", function () {
                    //to set Date acct mandatory given by ashish on 28 May 2020
                    if ($POP_DateAcct.val() == "") {
                        $POP_DateAcct.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_DateAcct.removeClass('vis-ev-col-mandatory');
                    }
                    //VIS323 06 sept 2022 Set Mandatory validation for Account date change Ajax call
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PlsSelectOrg"));
                        return false;
                    }
                    if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PlsSelectDocumentType"));
                        return false;
                    }
                    if (VIS.Utility.Util.getValueOfInt($RPOP_cmbBank.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PLSelectBank"));
                        return false;
                    }
                    if (VIS.Utility.Util.getValueOfInt($RPOP_cmbBankAccount.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PLSelectBankAccount"));
                        return false;
                    }
                    if (VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PLSelectPaymentMethod"));
                        return false;
                    }
                    if ($POP_DateAcct.val() == "") {
                        VIS.ADialog.info(("VA009_PLSelectAcctDate"));
                        return false;
                    }
                    //VIS317 Invalid Account date check
                    var dateVal = Date.parse($POP_DateAcct.val());
                    var currentTime = new Date(parseInt(dateVal));
                    //check if date is valid
                    //this check will work for 01/01/1970 on words
                    if (!isNaN(currentTime.getTime()) && currentTime.getTime() < 0) {
                        return;
                    }
                    //Used ajax Cal to get the Converted Amount when change the DateAcct
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetConvertedAmt",
                        type: "POST",
                        datatype: "json",
                        async: true,
                        data: ({ PaymentData: JSON.stringify(reloaddata), BankAccount: $RPOP_cmbBankAccount.val(), CurrencyType: $pop_cmbCurrencyType.val(), dateAcct: $POP_DateAcct.val(), _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                        success: function (result) {
                            callbackchqReload(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });

                });

                $POP_targetDocType.on("change", function () {
                    //to set target doc type mandatory
                    if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) == 0) {
                        $POP_targetDocType.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_targetDocType.removeClass('vis-ev-col-mandatory');
                    }
                });

                function callbackchqReload(result) {
                    chqrecgrd.clear();
                    popupgrddata = [];
                    var rslt = JSON.parse(result);
                    reloaddata = rslt;
                    for (var i in rslt) {
                        var line = {};
                        line["recid"] = rslt[i].recid;
                        line["C_Bpartner"] = rslt[i].C_Bpartner;
                        line["C_BPartner_Location_ID"] = rslt[i].C_BPartner_Location_ID;
                        line["C_DocType_ID"] = rslt[i].C_DocType_ID;
                        line["C_Invoice_ID"] = rslt[i].C_Invoice_ID;
                        line["C_BPartner_ID"] = rslt[i].C_BPartner_ID;
                        line["C_InvoicePaySchedule_ID"] = rslt[i].C_InvoicePaySchedule_ID;
                        line["CurrencyCode"] = rslt[i].CurrencyCode;
                        line["DueAmt"] = rslt[i].DueAmt;
                        line["VA009_RecivedAmt"] = rslt[i].convertedAmt;
                        line["OverUnder"] = "0";
                        line["Writeoff"] = "0";
                        line["Discount"] = "0";
                        line["ConvertedAmt"] = rslt[i].convertedAmt;
                        line["C_Currency_ID"] = rslt[i].C_Currency_ID;
                        line["AD_Org_ID"] = rslt[i].AD_Org_ID;
                        line["AD_Client_ID"] = rslt[i].AD_Client_ID;
                        line["CheckDate"] = new Date();//new Date().toString('MM/DD/YYYY');
                        line["CheckNumber"] = null;
                        line["ValidMonths"] = null;
                        line["VA009_PaymentMode"] = rslt[i].VA009_PaymentMode;
                        line["PaymwentBaseType"] = rslt[i].PaymwentBaseType;
                        line["TransactionType"] = rslt[i].TransactionType;
                        line["DocBaseType"] = rslt[i].DocBaseType;
                        popupgrddata.push(line);
                    }
                    if (rslt[0].ERROR == "ConversionNotFound") {
                        VIS.ADialog.info(("VA009_ConversionNotFound"));
                    }
                    w2utils.encodeTags(popupgrddata);
                    chqrecgrd.add(popupgrddata);
                };

                function CHQRecGrid_Layout() {

                    popupgrddata = [];
                    var _CHQRec_Columns = [];
                    if (_CHQRec_Columns.length == 0) {
                        _CHQRec_Columns.push({ field: "C_Bpartner", caption: VIS.Msg.getMsg("VA009_BPartner"), sortable: true, size: '12%' });
                        //_CHQRec_Columns.push({ field: "C_Invoice_ID", caption: VIS.Msg.getMsg("VA009_Invoice"), sortable: true, size: '10%' });
                        _CHQRec_Columns.push({ field: "C_InvoicePaySchedule_ID", caption: VIS.Msg.getMsg("VA009_Schedule"), sortable: true, size: '8%' });
                        _CHQRec_Columns.push({ field: "Description", caption: VIS.Msg.getMsg("Description"), sortable: true, size: '15%', editable: { type: 'text' } });
                        _CHQRec_Columns.push({ field: "CurrencyCode", caption: VIS.Msg.getMsg("VA009_Currency"), sortable: true, size: '8%' });
                        _CHQRec_Columns.push({
                            field: "DueAmt", caption: VIS.Msg.getMsg("VA009_DueAmt"), sortable: true, size: '12%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["DueAmt"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }
                        });
                        _CHQRec_Columns.push({
                            field: "ConvertedAmt", caption: VIS.Msg.getMsg("VA009_ConvertedAmt"), sortable: true, size: '12%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["ConvertedAmt"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }
                        });
                        _CHQRec_Columns.push({
                            field: "VA009_RecivedAmt", caption: VIS.Msg.getMsg("VA009_ReceivedAmt"), sortable: true, size: '12%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["VA009_RecivedAmt"];
                                val = checkcommaordot(event, val, val);
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }, editable: { type: 'number' }
                        });
                        _CHQRec_Columns.push({
                            field: "OverUnder", caption: VIS.Msg.getMsg("VA009_OverUnder"), sortable: true, size: '8%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["OverUnder"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }
                        });
                        _CHQRec_Columns.push({
                            field: "Writeoff", caption: VIS.Msg.getMsg("VA009_Writeoff"), sortable: true, size: '8%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["Writeoff"];
                                val = checkcommaordot(event, val, val);
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }, editable: { type: 'number' }
                        });
                        _CHQRec_Columns.push({
                            field: "Discount", caption: VIS.Msg.getMsg("VA009_Discount"), sortable: true, size: '8%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["Discount"];
                                val = checkcommaordot(event, val, val);
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }, editable: { type: 'number' }
                        });
                        _CHQRec_Columns.push({ field: "CheckNumber", caption: VIS.Msg.getMsg("VA009_ChkNo"), sortable: true, size: '8%', editable: { type: 'alphanumeric', autoFormat: true, groupSymbol: ' ' } });
                        _CHQRec_Columns.push({
                            field: "CheckDate", caption: VIS.Msg.getMsg("VA009_CheckDate"), sortable: true, size: '10%', style: 'text-align: left',
                            render: function (record, index, col_index) {
                                var val;
                                //when user do double click on CheckDate field then mouse over without selecting the value at that time 
                                //record.changes.CheckDate is get empty string value so to avoid that used condtion is compared with empty string
                                if (record.changes == undefined || record.changes.CheckDate == "") {
                                    val = record["CheckDate"];
                                }
                                else {
                                    val = record.changes.CheckDate;
                                }
                                return new Date(val).toLocaleDateString();
                            }, editable: { type: 'date' }
                        });
                        //_CHQRec_Columns.push({ field: "ValidMonths", caption: VIS.Msg.getMsg("VA009_ValidMonths"), sortable: true, size: '10%', editable: { type: 'text' } });
                        //by Amit - 19-11-2016
                        _CHQRec_Columns.push({ field: "recid", caption: VIS.Msg.getMsg("VA009_srno"), sortable: true, size: '1%' });
                        _CHQRec_Columns.push({ field: "TransactionType", caption: VIS.Msg.getMsg("VA009_TransactionType"), sortable: true, size: '1%' });
                        _CHQRec_Columns.push({ field: "C_BPartner_Location_ID", caption: VIS.Msg.getMsg("C_BPartner_Location_ID"), sortable: true, size: '1%' });
                        _CHQRec_Columns.push({ field: "C_DocType_ID", caption: VIS.Msg.getMsg("C_DocType_ID"), sortable: true, size: '1%' });
                        _CHQRec_Columns.push({ field: "DocBaseType", caption: VIS.Msg.getMsg("DocBaseType"), sortable: true, size: '1%' });
                        //end
                    }
                    chqrecgrd = null;
                    chqrecgrd = CheueRecevableGrid.w2grid({
                        name: 'CheueRecevableGrid_' + $self.windowNo,
                        recordHeight: 25,
                        columns: _CHQRec_Columns,
                        method: 'GET',
                        multiSelect: true,
                        show: {
                            toolbarSave: true
                        },

                        onEditField: function (event) {
                            if (event.column == 6 || event.column == 8 || event.column == 9) {
                                if (chqrecgrd.get(event.recid).TransactionType == 'Order' || chqrecgrd.get(event.recid).TransactionType == 'GL Journal') {
                                    event.isCancelled = true;
                                }
                            }
                            event.onComplete = function (event) {
                                id = event.recid;
                                if (event.column == 8 || event.column == 9 || event.column == 6) {
                                    chqrecgrd.records[event.index][chqrecgrd.columns[event.column].field] = checkcommaordot(event, chqrecgrd.records[event.index][chqrecgrd.columns[event.column].field]);
                                    var _value = format.GetFormatAmount(chqrecgrd.records[event.index][chqrecgrd.columns[event.column].field], "init", dotFormatter);
                                    chqrecgrd.records[event.index][chqrecgrd.columns[event.column].field] = format.GetConvertedString(_value, dotFormatter);
                                    $('#grid_CheueRecevableGrid_' + $self.windowNo + '_edit_' + id + '_' + event.column).keydown(function (event) {
                                        var isDotSeparator = culture.isDecimalSeparatorDot(window.navigator.language);

                                        if (!isDotSeparator && (event.keyCode == 190 || event.keyCode == 110)) {// , separator
                                            return false;
                                        }
                                        else if (isDotSeparator && event.keyCode == 188) { // . separator
                                            return false;
                                        }
                                        if (event.target.value.contains(".") && (event.which == 110 || event.which == 190 || event.which == 188)) {
                                            this.value = this.value.replace('.', '');
                                        }
                                        if (event.target.value.contains(",") && (event.which == 110 || event.which == 190 || event.which == 188)) {
                                            this.value = this.value.replace(',', '');
                                        }
                                        if (event.keyCode != 8 && event.keyCode != 9 && (event.keyCode < 37 || event.keyCode > 40) &&
                                            (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)
                                            && event.keyCode != 109 && event.keyCode != 189 && event.keyCode != 110
                                            && event.keyCode != 144 && event.keyCode != 188 && event.keyCode != 190) {
                                            return false;
                                        }
                                    });
                                }
                            };
                        },

                        onChange: function (event) {   // Added by Bharat on 02/May/2017
                            //change by amit
                            window.setTimeout(function () {
                                if (chqrecgrd.getChanges(event.recid) != undefined) {
                                    var stdPrecision = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetCurrencyPrecision", { "BankAccount_ID": $RPOP_cmbBankAccount.val(), "CurrencyFrom": "B" }, null);
                                    if (stdPrecision == null || stdPrecision == 0) {
                                        stdPrecision = 2;
                                    }
                                    chqrecgrd.records[event.index]['ConvertedAmt'] = parseFloat(chqrecgrd.records[event.index]['ConvertedAmt']);
                                    chqrecgrd.records[event.index]['VA009_RecivedAmt'] = parseFloat(chqrecgrd.records[event.index]['VA009_RecivedAmt']);
                                    chqrecgrd.records[event.index]['OverUnder'] = parseFloat(chqrecgrd.records[event.index]['OverUnder']);
                                    chqrecgrd.records[event.index]['Writeoff'] = parseFloat(chqrecgrd.records[event.index]['Writeoff']);
                                    chqrecgrd.records[event.index]['Discount'] = parseFloat(chqrecgrd.records[event.index]['Discount']);
                                    //Received Amount
                                    if (event.column == 6) {
                                        if (event.value_new == "") {
                                            event.value_new = 0;
                                        }
                                        else {
                                            //  event.value_new = format.GetConvertedNumber(event.value_new, dotFormatter);
                                            event.value_new = format.GetConvertedNumber(event.value_new, dotFormatter);
                                        }
                                        //else if (event.value_new.toString().contains(',')) {
                                        //    event.value_new = parseFloat(event.value_new.replace(',', '.'));
                                        //}
                                        if (event.value_new > chqrecgrd.records[event.index]['ConvertedAmt']) {
                                            VIS.ADialog.error("MoreScheduleAmount");
                                            event.value_new = event.value_original;
                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = event.value_new;
                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = chqrecgrd.get(event.recid).changes.VA009_RecivedAmt;
                                            chqrecgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                            return;
                                        }
                                        chqrecgrd.records[event.index]['VA009_RecivedAmt'] = event.value_new.toFixed(stdPrecision);
                                        chqrecgrd.refreshCell(event.recid, "VA009_RecivedAmt");

                                        if (chqrecgrd.records[event.index]['PaymwentBaseType'] == "ARR" || chqrecgrd.records[event.index]['PaymwentBaseType'] == "APP") {
                                            if (event.value_new > chqrecgrd.records[event.index]['ConvertedAmt']) {
                                                if (chqrecgrd.records[event.index]['ConvertedAmt'] > 0) {
                                                    chqrecgrd.get(event.recid).changes.OverUnder = ((chqrecgrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                                    chqrecgrd.records[event.index]['OverUnder'] = chqrecgrd.get(event.recid).changes.OverUnder;
                                                    chqrecgrd.get(event.recid).changes.Writeoff = 0;
                                                }
                                                else {
                                                    chqrecgrd.get(event.recid).changes.OverUnder = ((chqrecgrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                                    chqrecgrd.records[event.index]['OverUnder'] = chqrecgrd.get(event.recid).changes.OverUnder;
                                                    chqrecgrd.get(event.recid).changes.Writeoff = 0;
                                                    chqrecgrd.get(event.recid).Writeoff = 0;
                                                }
                                                chqrecgrd.get(event.recid).changes.Discount = 0;
                                                chqrecgrd.refreshCell(event.recid, "OverUnder");
                                                chqrecgrd.refreshCell(event.recid, "Discount");
                                                chqrecgrd.refreshCell(event.recid, "Writeoff");
                                            }
                                            else if (event.value_new == chqrecgrd.records[event.index]['ConvertedAmt']) {
                                                chqrecgrd.get(event.recid).changes.OverUnder = 0;
                                                chqrecgrd.get(event.recid).OverUnder = 0;
                                                chqrecgrd.get(event.recid).changes.Discount = 0;
                                                chqrecgrd.get(event.recid).changes.Writeoff = 0;
                                                chqrecgrd.get(event.recid).Discount = 0;
                                                chqrecgrd.get(event.recid).Writeoff = 0;
                                                chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = event.value_new;
                                                chqrecgrd.records[event.index]['VA009_RecivedAmt'] = chqrecgrd.get(event.recid).changes.VA009_RecivedAmt;
                                                chqrecgrd.refreshCell(event.recid, "OverUnder");
                                                chqrecgrd.refreshCell(event.recid, "Discount");
                                                chqrecgrd.refreshCell(event.recid, "Writeoff");
                                                chqrecgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                            }
                                            else if (event.value_new < chqrecgrd.records[event.index]['ConvertedAmt']) {
                                                if (chqrecgrd.records[event.index]['ConvertedAmt'] > 0) {
                                                    if (chqrecgrd.get(event.recid).TransactionType == 'Order') {
                                                        chqrecgrd.get(event.recid).changes.Discount = ((chqrecgrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                                        chqrecgrd.records[event.index]['Discount'] = chqrecgrd.get(event.recid).changes.Discount;
                                                        chqrecgrd.get(event.recid).changes.Writeoff = 0;
                                                        chqrecgrd.get(event.recid).Writeoff = 0;
                                                        chqrecgrd.get(event.recid).changes.OverUnder = 0;
                                                        chqrecgrd.get(event.recid).OverUnder = 0;
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = ((chqrecgrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                                        chqrecgrd.records[event.index]['OverUnder'] = chqrecgrd.get(event.recid).changes.OverUnder;
                                                        chqrecgrd.get(event.recid).changes.Writeoff = 0;
                                                        chqrecgrd.get(event.recid).Writeoff = 0;
                                                        chqrecgrd.get(event.recid).changes.Discount = 0;
                                                        chqrecgrd.get(event.recid).Discount = 0;
                                                    }
                                                }
                                                else {
                                                    if (chqrecgrd.get(event.recid).TransactionType == 'Order') {
                                                        chqrecgrd.get(event.recid).changes.Discount = ((chqrecgrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                                        chqrecgrd.records[event.index]['Discount'] = chqrecgrd.get(event.recid).changes.Discount;
                                                        chqrecgrd.get(event.recid).changes.Writeoff = 0;
                                                        chqrecgrd.get(event.recid).Writeoff = 0;
                                                        chqrecgrd.get(event.recid).changes.OverUnder = 0;
                                                        chqrecgrd.get(event.recid).OverUnder = 0;
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = ((chqrecgrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                                        chqrecgrd.records[event.index]['OverUnder'] = chqrecgrd.get(event.recid).changes.OverUnder;
                                                        chqrecgrd.get(event.recid).changes.Writeoff = 0;
                                                        chqrecgrd.get(event.recid).Writeoff = 0;
                                                        chqrecgrd.get(event.recid).changes.Discount = 0;
                                                        chqrecgrd.get(event.recid).Discount = 0;
                                                    }
                                                }
                                                //chqrecgrd.get(event.recid).changes.Discount = 0;
                                                //chqrecgrd.get(event.recid).Discount = 0;
                                                chqrecgrd.refreshCell(event.recid, "OverUnder");
                                                chqrecgrd.refreshCell(event.recid, "Discount");
                                                chqrecgrd.refreshCell(event.recid, "Writeoff");
                                            }

                                        }
                                    }                                   
                                    //writeoff
                                    if (event.column == 8) {
                                        if (event.value_new == "") {
                                            event.value_new = 0;
                                        }
                                        else {
                                            //  event.value_new = parseFloat(checkcommaordot(event, event.value_new, parseFloat(chqrecgrd.records[event.index]['Writeoff'])));
                                            event.value_new = format.GetConvertedNumber(event.value_new, dotFormatter);
                                        }
                                        //else if (event.value_new.toString().contains(',')) {
                                        //    event.value_new = parseFloat(event.value_new.replace(',', '.'));
                                        //}
                                        chqrecgrd.records[event.index]['Writeoff'] = event.value_new.toFixed(stdPrecision);
                                        //VIS_427 BugId 2325 not allowing user to enter more writeof amount than converted amount 
                                        if (event.value_new > chqrecgrd.records[event.index]['ConvertedAmt']) {
                                            VIS.ADialog.error("MoreScheduleAmount");
                                            event.value_new = event.value_original;
                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_new;
                                            chqrecgrd.records[event.index]['Writeoff'] = chqrecgrd.get(event.recid).changes.Writeoff;
                                            chqrecgrd.refreshCell(event.recid, "Writeoff");
                                            return;
                                        }

                                        if (chqrecgrd.records[event.index]['PaymwentBaseType'] == "APP") {
                                            if (event.value_new > chqrecgrd.records[event.index]['ConvertedAmt']) {
                                                chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = ((event.value_new - chqrecgrd.records[event.index]['ConvertedAmt']) * -1).toFixed(stdPrecision);
                                                chqrecgrd.records[event.index]['VA009_RecivedAmt'] = chqrecgrd.get(event.recid).changes.VA009_RecivedAmt;
                                                chqrecgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                            }
                                            else if (event.value_new < chqrecgrd.records[event.index]['ConvertedAmt']) {
                                                if (chqrecgrd.get(event.recid).changes.Discount == undefined && chqrecgrd.get(event.recid).changes.OverUnder == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        chqrecgrd.refreshCell(event.recid, "Writeoff");
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + chqrecgrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else if (chqrecgrd.get(event.recid).changes.Discount == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        chqrecgrd.refreshCell(event.recid, "Writeoff");
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).OverUnder) + chqrecgrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else if (chqrecgrd.get(event.recid).changes.OverUnder == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        chqrecgrd.refreshCell(event.recid, "Writeoff");
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        chqrecgrd.refreshCell(event.recid, "Writeoff");
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                chqrecgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                chqrecgrd.refreshCell(event.recid, "OverUnder");
                                            }
                                        }
                                        else if (chqrecgrd.records[event.index]['PaymwentBaseType'] == "ARR") {
                                            if (event.value_new > chqrecgrd.records[event.index]['ConvertedAmt']) {
                                                if (chqrecgrd.records[event.index]['ConvertedAmt'] < 0) {
                                                    //if (chqrecgrd.get(event.recid).changes.Discount == undefined && chqrecgrd.get(event.recid).changes.OverUnder == undefined) {
                                                    //    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                    //    }
                                                    //    else {
                                                    //        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + chqrecgrd.records[event.index]['Discount']))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + chqrecgrd.records[event.index]['Discount']))).toFixed(stdPrecision);
                                                    //    }
                                                    //}
                                                    //else if (chqrecgrd.get(event.recid).changes.Discount == undefined) {
                                                    //    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                    //    }
                                                    //    else {
                                                    //        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.OverUnder) + chqrecgrd.records[event.index]['Discount']))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.OverUnder) + chqrecgrd.records[event.index]['Discount']))).toFixed(stdPrecision);
                                                    //    }
                                                    //}
                                                    //else if (chqrecgrd.get(event.recid).changes.OverUnder == undefined) {
                                                    //    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                    //    }
                                                    //    else {
                                                    //        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Discount)))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Discount)))).toFixed(stdPrecision);
                                                    //    }
                                                    //}
                                                    //else {
                                                    //    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Discount))).toFixed(stdPrecision);
                                                    //    }
                                                    //    else {
                                                    //        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Discount)))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Discount)))).toFixed(stdPrecision);
                                                    //    }
                                                    //}
                                                }
                                                else {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        chqrecgrd.refreshCell(event.recid, "OverUnder");
                                                        chqrecgrd.refreshCell(event.recid, "Writeoff");
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (event.value_new - chqrecgrd.records[event.index]['ConvertedAmt']).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                chqrecgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                chqrecgrd.refreshCell(event.recid, "OverUnder");
                                            }
                                            else if (event.value_new <= chqrecgrd.records[event.index]['ConvertedAmt']) {
                                                //chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + chqrecgrd.records[event.index]['Discount'])) * -1;
                                                if (chqrecgrd.get(event.recid).changes.Discount == undefined && chqrecgrd.get(event.recid).changes.OverUnder == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        chqrecgrd.refreshCell(event.recid, "OverUnder")
                                                        chqrecgrd.refreshCell(event.recid, "Writeoff");
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + chqrecgrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else if (chqrecgrd.get(event.recid).changes.Discount == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        chqrecgrd.refreshCell(event.recid, "OverUnder");
                                                        chqrecgrd.refreshCell(event.recid, "Writeoff");
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).OverUnder) + chqrecgrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else if (chqrecgrd.get(event.recid).changes.OverUnder == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        chqrecgrd.refreshCell(event.recid, "OverUnder");
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        chqrecgrd.refreshCell(event.recid, "OverUnder");
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes writeoff
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                            chqrecgrd.records[event.index]['Writeoff'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                chqrecgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                chqrecgrd.refreshCell(event.recid, "OverUnder");
                                            }
                                        }
                                    }
                                    //discount
                                    if (event.column == 9) {
                                        if (event.value_new == "") {
                                            event.value_new = 0;
                                        }
                                        else {
                                            event.value_new = format.GetConvertedNumber(event.value_new, dotFormatter);
                                        }
                                        //else if (event.value_new.toString().contains(',')) {
                                        //    event.value_new = parseFloat(event.value_new.replace(',', '.'));
                                        //}

                                        chqrecgrd.records[event.index]['Discount'] = event.value_new.toFixed(stdPrecision);
                                        //VIS_427 BugId 2325 not allowing user to enter more discount amount than converted amount 
                                        if (event.value_new > chqrecgrd.records[event.index]['ConvertedAmt']) {
                                            VIS.ADialog.error("MoreScheduleAmount");
                                            event.value_new = event.value_original;
                                            chqrecgrd.get(event.recid).changes.Discount = event.value_new;
                                            chqrecgrd.records[event.index]['Discount'] = chqrecgrd.get(event.recid).changes.Discount;
                                            chqrecgrd.refreshCell(event.recid, "Discount");
                                            return;
                                        }

                                        if (chqrecgrd.records[event.index]['PaymwentBaseType'] == "APP") {
                                            if (event.value_new > chqrecgrd.records[event.index]['ConvertedAmt']) {
                                                chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = ((event.value_new - chqrecgrd.records[event.index]['ConvertedAmt']) * -1).toFixed(stdPrecision);
                                                chqrecgrd.records[event.index]['VA009_RecivedAmt'] = chqrecgrd.get(event.recid).changes.VA009_RecivedAmt;
                                                chqrecgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                            }
                                            else if (event.value_new < chqrecgrd.records[event.index]['ConvertedAmt']) {
                                                if (chqrecgrd.get(event.recid).changes.Writeoff == undefined && chqrecgrd.get(event.recid).changes.OverUnder == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + chqrecgrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else if (chqrecgrd.get(event.recid).changes.Writeoff == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.OverUnder) + chqrecgrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled received to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else if (chqrecgrd.get(event.recid).changes.OverUnder == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                chqrecgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                chqrecgrd.refreshCell(event.recid, "OverUnder");
                                            }
                                        }
                                        else if (chqrecgrd.records[event.index]['PaymwentBaseType'] == "ARR") {
                                            if (event.value_new > chqrecgrd.records[event.index]['ConvertedAmt']) {
                                                if (chqrecgrd.records[event.index]['ConvertedAmt'] < 0) {
                                                    //if (chqrecgrd.get(event.recid).changes.Writeoff == undefined && chqrecgrd.get(event.recid).changes.OverUnder == undefined) {
                                                    //    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                    //    }
                                                    //    else {
                                                    //        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                    //    }
                                                    //}
                                                    //else if (chqrecgrd.get(event.recid).changes.Writeoff == undefined) {
                                                    //    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                    //    }
                                                    //    else {
                                                    //        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.OverUnder) + chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.OverUnder) + chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                    //    }
                                                    //}
                                                    //else if (chqrecgrd.get(event.recid).changes.OverUnder == undefined) {
                                                    //    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                    //    }
                                                    //    else {
                                                    //        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Writeoff)))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Writeoff)))).toFixed(stdPrecision);
                                                    //    }
                                                    //}
                                                    //else {
                                                    //    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                    //    }
                                                    //    else {
                                                    //        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Writeoff)))).toFixed(stdPrecision);
                                                    //        chqrecgrd.get(event.recid).VA009_RecivedAmt = (-1 * ((-1 * chqrecgrd.records[event.index]['ConvertedAmt']) + (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Writeoff)))).toFixed(stdPrecision);
                                                    //    }
                                                    //}
                                                }
                                                else {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (event.value_new - chqrecgrd.records[event.index]['ConvertedAmt']).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled Received to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                chqrecgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                chqrecgrd.refreshCell(event.recid, "OverUnder");
                                            }
                                            else if (event.value_new <= chqrecgrd.records[event.index]['ConvertedAmt']) {
                                                //chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + chqrecgrd.records[event.index]['Writeoff'])) * -1;
                                                if (chqrecgrd.get(event.recid).changes.Writeoff == undefined && chqrecgrd.get(event.recid).changes.OverUnder == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        chqrecgrd.refreshCell(event.recid, "OverUnder");
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + chqrecgrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled received to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else if (chqrecgrd.get(event.recid).changes.Writeoff == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                        chqrecgrd.refreshCell(event.recid, "OverUnder");
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.OverUnder) + chqrecgrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled received to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else if (chqrecgrd.get(event.recid).changes.OverUnder == undefined) {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled received to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                else {
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['OverUnder']) > 0) {
                                                        chqrecgrd.get(event.recid).changes.OverUnder = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + chqrecgrd.records[event.index]['VA009_RecivedAmt'] + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.OverUnder < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.OverUnder = chqrecgrd.records[event.index]['OverUnder'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['OverUnder'] = Math.abs(chqrecgrd.get(event.recid).changes.OverUnder);
                                                        }
                                                    }
                                                    else {
                                                        chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = (chqrecgrd.records[event.index]['ConvertedAmt'] - (event.value_new + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.OverUnder) + VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(event.recid).changes.Writeoff))).toFixed(stdPrecision);
                                                        //VIS_427 BugId 2325 handled received to not be negative when user changes Discount
                                                        if (chqrecgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                            VIS.ADialog.error("MoreScheduleAmount");
                                                            chqrecgrd.get(event.recid).changes.VA009_RecivedAmt = chqrecgrd.records[event.index]['VA009_RecivedAmt'];
                                                            chqrecgrd.get(event.recid).changes.Discount = event.value_original;
                                                            chqrecgrd.records[event.index]['Discount'] = event.value_original;
                                                        }
                                                        else {
                                                            chqrecgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(chqrecgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                        }
                                                    }
                                                }
                                                chqrecgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                                chqrecgrd.refreshCell(event.recid, "OverUnder");
                                            }
                                        }
                                    }

                                    chqrecgrd.refreshCell(event.recid, "OverUnder");
                                    chqrecgrd.refreshCell(event.recid, "Discount");
                                    chqrecgrd.refreshCell(event.recid, "Writeoff");
                                    chqrecgrd.refreshCell(event.recid, "ConvertedAmt");
                                    chqrecgrd.refreshCell(event.recid, "VA009_RecivedAmt");

                                }
                                if (event.column == 10)
                                    chqrecgrd.records[event.index]['CheckNumber'] = event.value_new;
                                if (event.column == 11) {
                                    //Used do double click on CheckDate field and closed without selecting the value at 
                                    //that time its return empty string to avoid Invalid Date used this below condition compared with empty string
                                    if (event.value_new != "") {
                                        chqrecgrd.records[event.index]['CheckDate'] = event.value_new;
                                    }
                                    else {
                                        chqrecgrd.records[event.index]['CheckDate'] = event.value_previous != "" ? event.value_previous : event.value_original;
                                    }
                                }
                                if (event.column == 2)
                                    chqrecgrd.records[event.index]['Description'] = event.value_new;
                            }, 100);
                            //end
                        }
                    })
                    chqrecgrd.hideColumn('recid');
                    chqrecgrd.hideColumn('TransactionType');
                    chqrecgrd.hideColumn('C_BPartner_Location_ID', 'C_DocType_ID');
                };

                function loadgrd(callback) {
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetPopUpData",
                        type: "POST",
                        datatype: "json",
                        //contentType: "application/json; charset=utf-8",
                        //async: false,
                        data: ({
                            InvPayids: SlctdPaymentIds.toString(), bank_id: _C_Bank_ID, acctno: _C_BankAccount_ID,
                            chkno: VIS.Utility.encodeText(_Cheque_no), OrderPayids: SlctdOrderPaymentIds.toString(),
                            JournalPayids: SlctdJournalPaymentIds.toString(),
                        }),
                        success: function (result) {
                            callback(result);
                        },
                        error: function () {
                            $bsyDiv[0].style.visibility = "hidden";
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                };

                function callbackchqRec(result) {
                    popupgrddata = [];
                    var rslt = JSON.parse(result);
                    reloaddata = rslt;
                    for (var i in rslt) {
                        var line = {};
                        line["recid"] = rslt[i].recid;
                        line["C_Bpartner"] = rslt[i].C_Bpartner;
                        line["C_BPartner_Location_ID"] = rslt[i].C_BPartner_Location_ID;
                        line["C_DocType_ID"] = rslt[i].C_DocType_ID;
                        line["C_Invoice_ID"] = rslt[i].C_Invoice_ID;
                        line["C_BPartner_ID"] = rslt[i].C_BPartner_ID;
                        line["C_InvoicePaySchedule_ID"] = rslt[i].C_InvoicePaySchedule_ID;
                        line["CurrencyCode"] = rslt[i].CurrencyCode;
                        line["DueAmt"] = rslt[i].DueAmt;
                        line["VA009_RecivedAmt"] = rslt[i].DueAmt;
                        line["OverUnder"] = "0";
                        line["Writeoff"] = "0";
                        line["Discount"] = "0";
                        line["ConvertedAmt"] = rslt[i].DueAmt;
                        line["C_Currency_ID"] = rslt[i].C_Currency_ID;
                        line["AD_Org_ID"] = rslt[i].AD_Org_ID;
                        line["AD_Client_ID"] = rslt[i].AD_Client_ID;
                        line["CheckDate"] = new Date();//new Date().toString('MM/DD/YYYY');
                        line["CheckNumber"] = null;
                        line["ValidMonths"] = null;
                        line["VA009_PaymentMode"] = rslt[i].VA009_PaymentMode;
                        line["TransactionType"] = rslt[i].TransactionType;
                        line["PaymwentBaseType"] = rslt[i].PaymwentBaseType;
                        line["DocBaseType"] = rslt[i].DocBaseType;
                        popupgrddata.push(line);
                    }
                    w2utils.encodeTags(popupgrddata);
                    chqrecgrd.add(popupgrddata);
                    $bsyDiv[0].style.visibility = "hidden";
                };

                ChequeReceDialog.onOkClick = function () {
                    var _CollaborateData = [];
                    chqrecgrd.sort(['C_BPartner_ID', 'CheckNumber'], 'asc');
                    chqrecgrd.selectAll();

                    if (chqrecgrd.getSelection().length > 0) {
                        if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) > 0) {
                            //Rakesh(VA228):Make Document Type selection mandatory
                            if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) > 0) {
                                if (VIS.Utility.Util.getValueOfInt($RPOP_cmbBank.val()) > 0) {
                                    if (VIS.Utility.Util.getValueOfInt($RPOP_cmbBankAccount.val()) > 0) {
                                        if (VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()) > 0) {
                                            if ($POP_DateAcct.val() != "" && $POP_DateAcct.val() != null) {
                                                for (var i = 0; i < chqrecgrd.getSelection().length; i++) {
                                                    var _data = {}; var updated = 0;
                                                    _data["C_BPartner_ID"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['C_BPartner_ID'];
                                                    _data["Description"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['Description'];
                                                    _data["C_Invoice_ID"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['C_Invoice_ID'];
                                                    _data["AD_Org_ID"] = VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val());
                                                    //Rakesh(VA228):Set Document Type (13/Sep/2021)
                                                    _data["TargetDocType"] = VIS.Utility.Util.getValueOfInt($POP_targetDocType.val());
                                                    _data["AD_Client_ID"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['AD_Client_ID'];
                                                    _data["C_Currency_ID"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['C_Currency_ID'];
                                                    _data["C_InvoicePaySchedule_ID"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['C_InvoicePaySchedule_ID'];
                                                    _data["TransactionType"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['TransactionType'];
                                                    _data["C_BPartner_Location_ID"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['C_BPartner_Location_ID'];
                                                    _data["C_DocType_ID"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['C_DocType_ID'];
                                                    _data["DocBaseType"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['DocBaseType'];
                                                    if (chqrecgrd.get(chqrecgrd.getSelection()[i])['CheckDate'] != "") {
                                                        var dt = new Date(chqrecgrd.get(chqrecgrd.getSelection()[i])['CheckDate']);
                                                        dt = new Date(dt.setHours(0, 0, 0, 0));
                                                        var acctdate = new Date($POP_DateAcct.val());
                                                        acctdate = new Date(acctdate.setHours(0, 0, 0, 0));
                                                        if (dt > acctdate) {
                                                            VIS.ADialog.info(("VIS_CheckDateCantbeGreaterSys"));
                                                            chqrecgrd.selectNone();
                                                            return false;
                                                        }
                                                        _data["CheckDate"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['CheckDate'];
                                                    }
                                                    else {
                                                        VIS.ADialog.info(("VA009_PLCheckDate"));
                                                        chqrecgrd.selectNone();
                                                        return false;
                                                    }
                                                    //Transaction Date
                                                    _data["DateTrx"] = VIS.Utility.Util.getValueOfDate($POP_DateTrx.val());
                                                    _data["DateAcct"] = VIS.Utility.Util.getValueOfDate($POP_DateAcct.val());

                                                    _data["C_BankAccount_ID"] = VIS.Utility.Util.getValueOfInt($RPOP_cmbBankAccount.val());
                                                    _data["C_Bank_ID"] = VIS.Utility.Util.getValueOfInt($RPOP_cmbBank.val());
                                                    _data["DueAmt"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['DueAmt'];
                                                    _data["ConvertedAmt"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['ConvertedAmt'];
                                                    _data["PaymwentBaseType"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['PaymwentBaseType'];
                                                    _data["OverUnder"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['OverUnder'];
                                                    _data["Discount"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['Discount'];
                                                    _data["Writeoff"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['Writeoff'];

                                                    _data["From"] = "Rec";
                                                    if (chqrecgrd.get(chqrecgrd.getSelection()[i])['CheckNumber'] != null && chqrecgrd.get(chqrecgrd.getSelection()[i])['CheckNumber'] != "")
                                                        _data["CheckNumber"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['CheckNumber'];
                                                    else {
                                                        VIS.ADialog.info(("VA009_PLCheckNumber"));
                                                        chqrecgrd.selectNone();
                                                        return false;
                                                    }
                                                    //If the VA009_RecivedAmt value is "0" then the below condition will not work
                                                    if (VIS.Utility.Util.getValueOfDecimal(chqrecgrd.get(chqrecgrd.getSelection()[i])['VA009_RecivedAmt']) != 0) {
                                                        _data["VA009_RecivedAmt"] = chqrecgrd.get(chqrecgrd.getSelection()[i])['VA009_RecivedAmt'];
                                                    }
                                                    else {
                                                        VIS.ADialog.info(("VA009_PLRecivedAmt"));
                                                        chqrecgrd.selectNone();
                                                        return false;
                                                    }
                                                    _data["CurrencyType"] = $pop_cmbCurrencyType.val();
                                                    _data["VA009_PaymentMethod_ID"] = $POP_PayMthd.val();
                                                    _CollaborateData.push(_data);
                                                }
                                            }
                                            else {
                                                VIS.ADialog.info(("VA009_PLSelectAcctDate"));
                                                chqrecgrd.selectNone();
                                                return false;
                                            }
                                        }
                                        else {
                                            VIS.ADialog.info(("VA009_PLSelectPaymentMethod"));
                                            chqrecgrd.selectNone();
                                            return false;
                                        }
                                    }
                                    else {
                                        VIS.ADialog.info(("VA009_PLSelectBankAccount"));
                                        chqrecgrd.selectNone();
                                        return false;
                                    }
                                }
                                else {
                                    VIS.ADialog.info(("VA009_PLSelectBank"));
                                    chqrecgrd.selectNone();
                                    return false;
                                }
                            }
                            else {
                                VIS.ADialog.info(("VA009_PlsSelectDocumentType"));
                                chqrecgrd.selectNone();
                                return false;
                            }
                        }
                        else {
                            VIS.ADialog.info(("VA009_PlsSelectOrg"));
                            chqrecgrd.selectNone();
                            return false;
                        }
                    }
                    $bsyDiv[0].style.visibility = "visible";
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GeneratePayments",
                        type: "POST",
                        datatype: "json",
                        // contentType: "application/json; charset=utf-8",
                        async: true,
                        data: ({ PaymentData: JSON.stringify(_CollaborateData) }),
                        success: function (result) {
                            callbackPaymnt(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            $bsyDiv[0].style.visibility = "hidden";
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                };

                function callbackPaymnt(result) {
                    result = JSON.parse(result);
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; SlctdJournalPaymentIds = []; batchObjInv = []; batchObjOrd = []; batchObjJournal = [];
                    resetPaging();
                    //after successfully created Payment selectall checkbox should be false
                    $selectall.prop('checked', false);
                    //loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
                    loadPaymetsAll();
                    clearamtid();
                    $bsyDiv[0].style.visibility = "hidden";
                    VIS.ADialog.info("", null, result, null);
                    //w2alert(result.toString());
                };

                ChequeReceDialog.onCancelCLick = function () {
                    chqrecgrd.clear();
                    ChequeRecDispose();
                };

                ChequeReceDialog.onClose = function () {
                    chqrecgrd.clear();
                    ChequeRecDispose();

                };

                function ChequeRecDispose() {
                    _ChequeRecevble = null;
                    $chequeRecivable = null;
                    STAT_cmbBank = null;
                    STAT_cmbBankAccount = null;
                    STAT_txtStatementNo = null;
                    STAT_ctrlLoadFile = null;
                    STAT_ctrlLoadFileTxt = null;
                    chqrecgrd.destroy();
                }
            },

            Cash_Dialog: function () {
                var CashGrid, _Cheque_no = "";
                var _C_Bank_ID = 0, _C_BankAccount_ID = 0, cashAmount = 0;
                $bsyDiv[0].style.visibility = "visible";
                var divAmount, format = null;
                lblAmount = $("<label>");
                cashAmount = new VIS.Controls.VAmountTextBox("VA009_POP_Txtopngbal_" + $self.windowNo + "", false, true, true, 50, 100, VIS.DisplayType.Amount, VIS.Msg.getMsg("Amount"));
                lblAmount.append(VIS.Msg.getMsg("VA009_lblopngbal"));
                cashAmount.setValue(0);
                format = VIS.DisplayType.GetNumberFormat(VIS.DisplayType.Amount);
                divAmount = $("<div class='VA009-popform-data input-group vis-input-wrap'>");
                divAmountCtrlWrap = $("<div class='vis-control-wrap'>");
                divAmount.append(divAmountCtrlWrap);
                divAmountCtrlWrap.append(cashAmount.getControl().attr('placeholder', ' ').attr('data-placeholder', '')).append(lblAmount);
                $cash = $("<div class='VA009-popform-content vis-formouterwrpdiv' style='min-height:385px !important'>");
                //var _Cash = "";
                _Cash = $("<div class='VA009-popfrm-wrap' style='height:auto;'>");

                cashData = $("<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbOrg_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_Org") + "</label>"
                    + "</div></div>"

                    //Rakesh(VA228):Create Doc type element html
                    + "<div class='VA009-popform-data input-group vis-input-wrap' > <div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbDocType_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_DocType") + "</label>"
                    + "</div></div> "

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_Txtcashbk_" + $self.windowNo + "'> </select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_lblcashbook") + "</label>"
                    + "</div></div> ");

                //+ "<div class='VA009-popform-data'>"
                //+ "<label>" + VIS.Msg.getMsg("VA009_lblopngbal") + "</label>"
                //+ "<input type='text' id='VA009_POP_Txtopngbal_" + $self.windowNo + "' disabled/> </div>"

                cashDataa = $("<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbCurrencyType_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_CurrencyType") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_AccountDate_" + $self.windowNo + "' placeholder=' ' data-placeholder=''> "
                    + "<label>" + VIS.Msg.getMsg("AccountDate") + "</label>"
                    + "</div></div>"
                    //Transaction Date
                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_TransactionDate" + $self.windowNo + "' placeholder=' ' data-placeholder='' >"
                    + "<label>" + VIS.Msg.getMsg("TransactionDate") + "</label>"
                    + "</div> </div>"

                    + "<div class='VA009-table-container' id='VA009_btnPopupGrid'>  </div>"
                    + "</div>");
                _Cash.append(cashData).append(divAmount).append(cashDataa);
                $cash.append(_Cash);
                Cash_getControls();

                var now = new Date();
                var _today = now.getFullYear() + "-" + (("0" + (now.getMonth() + 1)).slice(-2)) + "-" + (("0" + now.getDate()).slice(-2));
                $POP_DateAcct.val(_today);
                //Transaction Date
                $POP_DateTrx.val(_today);
                var CashDialog = new VIS.ChildDialog();

                CashDialog.setContent($cash);
                //VA230:Set Cash dialog header based on ARR and APP
                if ($CR_Tab.hasClass('VA009-active-tab')) //ARR
                    CashDialog.setTitle(VIS.Msg.getMsg("VA009_LoadCashReceipts"));
                else
                    CashDialog.setTitle(VIS.Msg.getMsg("VA009_LoadCashPayment"));
                CashDialog.setWidth("60%");
                //VA230:Remove outer scroll bar
                //CashDialog.setHeight(window.innerHeight - 120);
                CashDialog.setEnableResize(true);
                CashDialog.setModal(true);
                if (SlctdPaymentIds.toString() != "" || SlctdOrderPaymentIds.toString() != "" || SlctdJournalPaymentIds.toString() != "") {
                    var cash = null;
                    if (cash == null) {
                        CashDialog.show();
                        CashGrid_Layout();
                        loadgrdCash(callbackCASHPay);
                        //load cashbook based on selected Org_ID
                        //loadcashbook();
                        loadCurrencyType();
                        loadOrg();
                        //Rakesh(VA228):10/Sep/2021 -> Load APR target base doc type
                        _loadFunctions.LoadTargetDocType($POP_targetDocType, _TargetBaseType);
                    }
                    else {
                        $bsyDiv[0].style.visibility = "hidden";
                        VIS.ADialog.info(("VA009_PleaseSelctonlyCash"));
                    }
                }
                cashAmount.addVetoableChangeListener(this);
                function loadCurrencyType() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/loadCurrencyType", null, callbackCurrencyType);

                    function callbackCurrencyType(dr) {
                        $pop_cmbCurrencyType.append("<option value='0'></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $pop_cmbCurrencyType.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                                if (VIS.Utility.encodeText(dr[i].IsDefault) == "Y") {
                                    defaultCurrenyType = VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID);
                                }
                            }
                            //$pop_cmbCurrencyType.prop('selectedIndex', 1);
                            $pop_cmbCurrencyType.val(defaultCurrenyType)
                        }
                    }
                };

                //to load all organization 
                function loadOrg() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadOrganization", null, callbackloadorg);
                    function callbackloadorg(dr) {
                        $POP_cmbOrg.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_cmbOrg.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].AD_Org_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                            }
                        }
                        $POP_cmbOrg.prop('selectedIndex', 0);
                    };
                };

                //Set Mandatory and Non-Mandatory
                function SetMandatory(value) {
                    if (value)
                        return '#FFB6C1';
                    else
                        return 'White';
                };

                function CashGrid_Layout() {
                    var hideColumn = false;
                    //VA230:If order selected then hide columns Received Amount/OverUnder/Writeoff/Discount
                    if ((SlctdOrderPaymentIds.length > 0 || SlctdJournalPaymentIds.length > 0) && SlctdPaymentIds.length == 0) {
                        hideColumn = true;
                    }
                    var _Cash_Columns = [];
                    if (_Cash_Columns.length == 0) {
                        _Cash_Columns.push({ field: "C_Bpartner", caption: VIS.Msg.getMsg("VA009_BPartner"), sortable: true, size: '15%' });
                        _Cash_Columns.push({ field: "Description", caption: VIS.Msg.getMsg("Description"), sortable: true, size: '15%', editable: { type: 'text' } });
                        _Cash_Columns.push({ field: "CurrencyCode", caption: VIS.Msg.getMsg("VA009_Currency"), sortable: true, size: '8%' });
                        _Cash_Columns.push({
                            field: "DueAmt", caption: VIS.Msg.getMsg("VA009_DueAmt"), sortable: true, size: '12%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["DueAmt"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }
                        });
                        _Cash_Columns.push({
                            field: "ConvertedAmt", caption: VIS.Msg.getMsg("VA009_ConvertedAmt"), sortable: true, size: '13%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["ConvertedAmt"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }
                        });
                        _Cash_Columns.push({
                            field: "VA009_RecivedAmt", caption: VIS.Msg.getMsg("VA009_ReceivedAmt"), sortable: true, size: '12%', style: 'text-align: right', hidden: hideColumn, render: function (record, index, col_index) {
                                var val = record["VA009_RecivedAmt"];
                                val = checkcommaordot(event, val, val);
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }, editable: { type: 'number' }
                        });
                        //changeby amit
                        _Cash_Columns.push({
                            field: "OverUnder", caption: VIS.Msg.getMsg("VA009_OverUnder"), sortable: true, size: '9%', style: 'text-align: right', hidden: hideColumn, render: function (record, index, col_index) {
                                var val = record["OverUnder"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }
                        });
                        _Cash_Columns.push({
                            field: "Writeoff", caption: VIS.Msg.getMsg("VA009_Writeoff"), sortable: true, size: '9%', style: 'text-align: right', hidden: hideColumn, render: function (record, index, col_index) {
                                var val = record["Writeoff"];
                                val = checkcommaordot(event, val, val);
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }, editable: { type: 'number' }
                        });
                        _Cash_Columns.push({
                            field: "Discount", caption: VIS.Msg.getMsg("VA009_Discount"), sortable: true, size: '9%', style: 'text-align: right', hidden: hideColumn, render: function (record, index, col_index) {
                                var val = record["Discount"];
                                val = checkcommaordot(event, val, val);
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }, editable: { type: 'number' }
                        });
                        _Cash_Columns.push({ field: "recid", caption: VIS.Msg.getMsg("VA009_srno"), sortable: true, size: '1%' });
                        _Cash_Columns.push({ field: "C_BPartner_Location_ID", caption: VIS.Msg.getMsg("C_BPartner_Location_ID"), sortable: true, size: '1%' });
                        _Cash_Columns.push({ field: "C_DocType_ID", caption: VIS.Msg.getMsg("C_DocType_ID"), sortable: true, size: '1%' });
                        _Cash_Columns.push({ field: "DocBaseType", caption: VIS.Msg.getMsg("DocBaseType"), sortable: true, size: '1%' });
                    }
                    Cashgrd = null;
                    Cashgrd = CashGrid.w2grid({
                        name: 'CashGrid',
                        recordHeight: 25,
                        columns: _Cash_Columns,
                        method: 'GET',
                        multiSelect: true,

                        onEditField: function (event) {
                            if (event.column == 6 || event.column == 8 || event.column == 9) {
                                if (Cashgrd.get(event.recid).TransactionType == 'Order' ||
                                    Cashgrd.get(event.recid).TransactionType == 'GL Journal') {
                                    event.isCancelled = true;
                                }
                            }
                            event.onComplete = function (event) {
                                id = event.recid;
                                if (event.column == 8 || event.column == 7 || event.column == 5) {
                                    Cashgrd.records[event.index][Cashgrd.columns[event.column].field] = checkcommaordot(event, Cashgrd.records[event.index][Cashgrd.columns[event.column].field]);
                                    var _value = format.GetFormatAmount(Cashgrd.records[event.index][Cashgrd.columns[event.column].field], "init", dotFormatter);
                                    Cashgrd.records[event.index][Cashgrd.columns[event.column].field] = format.GetConvertedString(_value, dotFormatter);
                                    $('#grid_CashGrid_edit_' + id + '_' + event.column).keydown(function (event) {
                                        var isDotSeparator = culture.isDecimalSeparatorDot(window.navigator.language);

                                        if (!isDotSeparator && (event.keyCode == 190 || event.keyCode == 110)) {// , separator
                                            return false;
                                        }
                                        else if (isDotSeparator && event.keyCode == 188) { // . separator
                                            return false;
                                        }
                                        if (event.target.value.contains(".") && (event.which == 110 || event.which == 190 || event.which == 188)) {
                                            this.value = this.value.replace('.', '');
                                        }
                                        if (event.target.value.contains(",") && (event.which == 110 || event.which == 190 || event.which == 188)) {
                                            this.value = this.value.replace(',', '');
                                        }
                                        if (event.keyCode != 8 && event.keyCode != 9 && (event.keyCode < 37 || event.keyCode > 40) &&
                                            (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)
                                            && event.keyCode != 109 && event.keyCode != 189 && event.keyCode != 110
                                            && event.keyCode != 144 && event.keyCode != 188 && event.keyCode != 190) {
                                            return false;
                                        }
                                    });
                                }
                            };
                        }
                    }),
                        Cashgrd.hideColumn('recid', 'C_BPartner_Location_ID', 'C_DocType_ID', 'DocBaseType');
                };

                function loadcashbook() {
                    //selected Org_Id assign to a variable & passed that variable as parameter
                    var org_Id = $POP_cmbOrg.val();
                    if (org_Id != null && org_Id != "")
                        VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadCashBook", { "Orgs": org_Id }, callbackloadcashbook);

                    function callbackloadcashbook(dr) {
                        $Cash_cmbcashbk.empty();
                        $Cash_cmbcashbk.addClass('vis-ev-col-mandatory');
                        $Cash_cmbcashbk.append("<option value='0'></option>");
                        //avoid null exception used below condition 'dr != null' 
                        if (dr != null && dr.length > 0) {
                            for (var i in dr) {
                                $Cash_cmbcashbk.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_CashBook_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                            }
                        }
                        $Cash_cmbcashbk.prop('selectedIndex', 0);
                    }
                };

                $Cash_cmbcashbk.on("change", function () {
                    cashAmount.setValue(0);
                    //to set cashbook mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($Cash_cmbcashbk.val()) == 0) {
                        $Cash_cmbcashbk.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $Cash_cmbcashbk.removeClass('vis-ev-col-mandatory');
                    }
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetCashBookData", { "CashBook": $Cash_cmbcashbk.val() }, callbackloadcashbk);
                    var currency = 0;
                    function callbackloadcashbk(dr) {
                        if (dr != null) {
                            cashAmount.setValue(dr["CompletedBalance"]);
                            currency = dr["C_Currency_ID"];
                            CashCurrency = currency;
                        }

                        //change by Amit
                        window.setTimeout(function () {
                            $.ajax({
                                url: VIS.Application.contextUrl + "VA009/Payment/GetCashJournalConvertedAmt",
                                type: "POST",
                                datatype: "json",
                                async: true,
                                //added Org and AccDate parameters to get the result with respect to the input values
                                data: ({ PaymentData: JSON.stringify(reloaddata), CurrencyCashBook: currency, CurrencyType: $pop_cmbCurrencyType.val(), dateAcct: $POP_DateAcct.val(), _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                                success: function (result) {
                                    callbackCASHPay(result);
                                },
                                error: function (ex) {
                                    console.log(ex);
                                    VIS.ADialog.error("VA009_ErrorLoadingPayments");
                                }
                            });
                        }, 200);
                        //end
                    }
                });

                $pop_cmbCurrencyType.on("change", function () {
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetCashJournalConvertedAmt",
                        type: "POST",
                        datatype: "json",
                        async: true,
                        data: ({ PaymentData: JSON.stringify(reloaddata), CurrencyCashBook: CashCurrency, CurrencyType: $pop_cmbCurrencyType.val(), dateAcct: $POP_DateAcct.val(), _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                        success: function (result) {
                            callbackCASHPay(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                });

                //on change event for DateAcct it should convert the Amount accordingly
                $POP_DateAcct.on("change", function () {
                    if ($POP_DateAcct.val()) {
                        $POP_DateAcct.removeClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_DateAcct.addClass('vis-ev-col-mandatory');
                    }
                    if ($POP_DateAcct.val() == "") {
                        VIS.ADialog.info(("VA009_PLSelectAcctDate"));
                        return false;
                    }
                    //VIS317 Invalid Account date check
                    var dateVal = Date.parse($POP_DateAcct.val());
                    var currentTime = new Date(parseInt(dateVal));
                    //check if date is valid
                    //this check will work for 01/01/1970 on words
                    if (!isNaN(currentTime.getTime()) && currentTime.getTime() < 0) {
                        return;
                    }
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetCashJournalConvertedAmt",
                        type: "POST",
                        datatype: "json",
                        async: true,
                        data: ({ PaymentData: JSON.stringify(reloaddata), CurrencyCashBook: CashCurrency, CurrencyType: $pop_cmbCurrencyType.val(), dateAcct: $POP_DateAcct.val(), _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                        success: function (result) {
                            callbackCASHPay(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                });

                $POP_cmbOrg.on("change", function () {
                    //to set org mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) == 0) {
                        $POP_cmbOrg.addClass('vis-ev-col-mandatory');
                        //clear the Options in Cashbook field
                        $Cash_cmbcashbk.empty();
                    }
                    else {
                        $POP_cmbOrg.removeClass('vis-ev-col-mandatory');
                        //based on selected org load the Cashbook
                        loadcashbook();
                    }
                    //Rakesh(VA228):Reset Document type
                    _loadFunctions.LoadTargetDocType($POP_targetDocType, _TargetBaseType);
                });

                $POP_targetDocType.on("change", function () {
                    //to set target doc type mandatory
                    if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) == 0) {
                        $POP_targetDocType.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_targetDocType.removeClass('vis-ev-col-mandatory');
                    }
                });
                function Cash_getControls() {
                    $Cash_cmbcashbk = $cash.find("#VA009_POP_Txtcashbk_" + $self.windowNo);
                    //$Cash_OpngBal = $cash.find("#VA009_POP_Txtopngbal_" + $self.windowNo);
                    CashGrid = $cash.find("#VA009_btnPopupGrid");
                    $pop_cmbCurrencyType = $cash.find("#VA009_POP_cmbCurrencyType_" + $self.windowNo);
                    $POP_DateAcct = $cash.find("#VA009_AccountDate_" + $self.windowNo);
                    //Transaction Date
                    $POP_DateTrx = $cash.find("#VA009_TransactionDate" + $self.windowNo);
                    $POP_cmbOrg = $cash.find("#VA009_POP_cmbOrg_" + $self.windowNo);
                    $POP_cmbOrg.addClass('vis-ev-col-mandatory');
                    //Rakesh(VA228):Store Doc type element in variable
                    $POP_targetDocType = $cash.find("#VA009_POP_cmbDocType_" + $self.windowNo);
                    $POP_targetDocType.addClass('vis-ev-col-mandatory');
                };

                function loadgrdCash(callback) {
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetPopUpData",
                        type: "POST",
                        datatype: "json",
                        //contentType: "application/json; charset=utf-8",
                        //async: false,
                        data: ({ InvPayids: SlctdPaymentIds.toString(), bank_id: _C_Bank_ID, acctno: _C_BankAccount_ID, chkno: VIS.Utility.encodeText(_Cheque_no), OrderPayids: SlctdOrderPaymentIds.toString() }),
                        success: function (result) {
                            callback(result);
                        },
                        error: function () {
                            $bsyDiv[0].style.visibility = "hidden";
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                };

                function callbackCASHPay(result) {
                    //amit
                    Cashgrd.clear();
                    //end

                    popupgrddata = [];
                    var rslt = JSON.parse(result);
                    reloaddata = rslt;
                    for (var i in rslt) {
                        var line = {};
                        line["recid"] = rslt[i].recid;
                        line["C_Bpartner"] = rslt[i].C_Bpartner;
                        line["C_BPartner_Location_ID"] = rslt[i].C_BPartner_Location_ID;
                        line["C_DocType_ID"] = rslt[i].C_DocType_ID;
                        line["C_Invoice_ID"] = rslt[i].C_Invoice_ID;
                        line["C_InvoicePaySchedule_ID"] = rslt[i].C_InvoicePaySchedule_ID;
                        line["CurrencyCode"] = rslt[i].CurrencyCode;
                        line["DueAmt"] = rslt[i].DueAmt;
                        line["C_BPartner_ID"] = rslt[i].C_BPartner_ID;
                        line["C_Currency_ID"] = rslt[i].C_Currency_ID;
                        line["AD_Org_ID"] = rslt[i].AD_Org_ID;
                        line["AD_Client_ID"] = rslt[i].AD_Client_ID;
                        line["VA009_PaymentMode"] = rslt[i].VA009_PaymentMode;
                        line["ConvertedAmt"] = rslt[i].convertedAmt;
                        line["PaymwentBaseType"] = rslt[i].PaymwentBaseType;
                        line["OverUnder"] = "0";
                        line["Writeoff"] = "0";
                        line["Discount"] = "0";
                        line["VA009_RecivedAmt"] = rslt[i].convertedAmt;
                        //VA230:Set TransactionType
                        line["TransactionType"] = rslt[i].TransactionType;
                        line["DocBaseType"] = rslt[i].DocBaseType;
                        popupgrddata.push(line);
                    }
                    if (rslt[0].ERROR == "ConversionNotFound") {
                        VIS.ADialog.info(("VA009_ConversionNotFound"));
                    }
                    else if (rslt[0].ERROR == "VA009_DocTypeNotFound") {
                        VIS.ADialog.info(("VA009_DocTypeNotFound"));
                    }
                    w2utils.encodeTags(popupgrddata);
                    Cashgrd.add(popupgrddata);
                    $bsyDiv[0].style.visibility = "hidden";
                };

                Cashgrd.on('change', function (event) {
                    //change by amit
                    window.setTimeout(function () {
                        if (w2ui.CashGrid.getChanges(event.recid) != undefined) {
                            //var sql = "SELECT stdprecision FROM c_currency WHERE c_currency_id =   (SELECT c_currency_id FROM c_cashbook WHERE C_Cashbook_ID=" + $Cash_cmbcashbk.val() + " ) ";
                            //var stdPrecision = VIS.DB.executeScalar(sql, null, null);
                            var stdPrecision = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetCurrencyPrecision", { "BankAccount_ID": $Cash_cmbcashbk.val(), "CurrencyFrom": "C" }, null);
                            if (stdPrecision == null || stdPrecision == 0) {
                                stdPrecision = 2;
                            }
                            // Description
                            if (event.column == 1)
                                Cashgrd.records[event.index]['Description'] = event.value_new;

                            Cashgrd.records[event.index]['ConvertedAmt'] = parseFloat(Cashgrd.records[event.index]['ConvertedAmt']);
                            Cashgrd.records[event.index]['VA009_RecivedAmt'] = parseFloat(Cashgrd.records[event.index]['VA009_RecivedAmt']);
                            Cashgrd.records[event.index]['OverUnder'] = parseFloat(Cashgrd.records[event.index]['OverUnder']);
                            Cashgrd.records[event.index]['Writeoff'] = parseFloat(Cashgrd.records[event.index]['Writeoff']);
                            Cashgrd.records[event.index]['Discount'] = parseFloat(Cashgrd.records[event.index]['Discount']);
                            //Received Amount
                            if (event.column == 5) {
                                if (event.value_new == "") {
                                    event.value_new = 0;
                                }
                                else {
                                    event.value_new = format.GetConvertedNumber(event.value_new, dotFormatter);
                                }
                                //else if (event.value_new.toString().contains(',')) {
                                //    event.value_new = parseFloat(event.value_new.replace(',', '.'));
                                //}
                                if (event.value_new > Cashgrd.records[event.index]['ConvertedAmt']) {
                                    VIS.ADialog.error("MoreScheduleAmount");
                                    event.value_new = event.value_original;
                                    Cashgrd.get(event.recid).changes.VA009_RecivedAmt = event.value_new;
                                    Cashgrd.records[event.index]['VA009_RecivedAmt'] = Cashgrd.get(event.recid).changes.VA009_RecivedAmt;
                                    Cashgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                    return;
                                }
                                Cashgrd.records[event.index]['VA009_RecivedAmt'] = event.value_new.toFixed(stdPrecision);
                                Cashgrd.refreshCell(event.recid, "VA009_RecivedAmt");

                                if (Cashgrd.records[event.index]['PaymwentBaseType'] == "ARR" || Cashgrd.records[event.index]['PaymwentBaseType'] == "APP") {

                                    if (event.value_new > Cashgrd.records[event.index]['ConvertedAmt']) {
                                        if (Cashgrd.records[event.index]['ConvertedAmt'] > 0) {
                                            Cashgrd.get(event.recid).changes.OverUnder = ((Cashgrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                            if (Cashgrd.get(event.recid).changes.OverUnder < 0) {
                                                VIS.ADialog.error("MoreScheduleAmount");
                                                Cashgrd.get(event.recid).changes.OverUnder = Cashgrd.records[event.index]['OverUnder'];
                                            }
                                            else {
                                                Cashgrd.records[event.index]['OverUnder'] = Cashgrd.get(event.recid).changes.OverUnder;
                                            }
                                            Cashgrd.get(event.recid).changes.Writeoff = 0;
                                        }
                                        else {
                                            Cashgrd.get(event.recid).changes.OverUnder = ((Cashgrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                            if (Cashgrd.get(event.recid).changes.OverUnder < 0) {
                                                VIS.ADialog.error("MoreScheduleAmount");
                                                Cashgrd.get(event.recid).changes.OverUnder = Cashgrd.records[event.index]['OverUnder'];
                                            }
                                            else {
                                                Cashgrd.records[event.index]['OverUnder'] = Cashgrd.get(event.recid).changes.OverUnder;
                                            }
                                            Cashgrd.get(event.recid).changes.Writeoff = 0;
                                            Cashgrd.get(event.recid).Writeoff = 0;
                                        }
                                        Cashgrd.get(event.recid).changes.Discount = 0;
                                        Cashgrd.refreshCell(event.recid, "OverUnder");
                                        Cashgrd.refreshCell(event.recid, "Discount");
                                        Cashgrd.refreshCell(event.recid, "Writeoff");
                                    }

                                    else if (event.value_new == Cashgrd.records[event.index]['ConvertedAmt']) {
                                        Cashgrd.get(event.recid).changes.OverUnder = 0;
                                        Cashgrd.get(event.recid).OverUnder = 0;
                                        Cashgrd.get(event.recid).changes.Discount = 0;
                                        Cashgrd.get(event.recid).changes.Writeoff = 0;
                                        Cashgrd.get(event.recid).Discount = 0;
                                        Cashgrd.get(event.recid).Writeoff = 0;
                                        Cashgrd.get(event.recid).changes.VA009_RecivedAmt = event.value_new;
                                        Cashgrd.records[event.index]['VA009_RecivedAmt'] = event.value_new;
                                        Cashgrd.refreshCell(event.recid, "OverUnder");
                                        Cashgrd.refreshCell(event.recid, "Discount");
                                        Cashgrd.refreshCell(event.recid, "Writeoff");
                                        Cashgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                    }
                                    else if (event.value_new < Cashgrd.records[event.index]['ConvertedAmt']) {
                                        if (Cashgrd.records[event.index]['ConvertedAmt'] > 0) {
                                            Cashgrd.get(event.recid).changes.OverUnder = ((Cashgrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                            if (Cashgrd.get(event.recid).changes.OverUnder < 0) {
                                                VIS.ADialog.error("MoreScheduleAmount");
                                            }
                                            else {
                                                Cashgrd.records[event.index]['OverUnder'] = Cashgrd.get(event.recid).changes.OverUnder;
                                            }
                                            Cashgrd.get(event.recid).changes.Writeoff = 0;
                                            Cashgrd.get(event.recid).Writeoff = 0;
                                        }
                                        else {
                                            Cashgrd.get(event.recid).changes.Writeoff = 0;
                                            Cashgrd.get(event.recid).Writeoff = 0;
                                            Cashgrd.get(event.recid).changes.OverUnder = ((Cashgrd.records[event.index]['ConvertedAmt']) - event.value_new).toFixed(stdPrecision);
                                            if (Cashgrd.get(event.recid).changes.OverUnder < 0) {
                                                VIS.ADialog.error("MoreScheduleAmount");
                                            }
                                            else {
                                                Cashgrd.records[event.index]['OverUnder'] = Cashgrd.get(event.recid).changes.OverUnder;
                                            }
                                        }
                                        Cashgrd.get(event.recid).changes.Discount = 0;
                                        Cashgrd.get(event.recid).Discount = 0;
                                        Cashgrd.refreshCell(event.recid, "OverUnder");
                                        Cashgrd.refreshCell(event.recid, "Discount");
                                        Cashgrd.refreshCell(event.recid, "Writeoff");
                                    }
                                }
                            }

                            //writeoff
                            if (event.column == 7) {
                                if (event.value_new == "") {
                                    event.value_new = 0;
                                }
                                else {
                                    event.value_new = format.GetConvertedNumber(event.value_new, dotFormatter);
                                }
                                if (event.value_new > Cashgrd.records[event.index]['ConvertedAmt']) {
                                    VIS.ADialog.error("MoreScheduleAmount");
                                    event.value_new = event.value_original;
                                    Cashgrd.get(event.recid).changes.Writeoff = event.value_new;
                                    Cashgrd.get(event.recid).Writeoff = event.value_new;
                                    Cashgrd.refreshCell(event.recid, "Writeoff");
                                    return;
                                }
                                //else if (event.value_new.toString().contains(',')) {
                                //    event.value_new = parseFloat(event.value_new.replace(',', '.'));
                                //}

                                Cashgrd.records[event.index]['Writeoff'] = event.value_new.toFixed(stdPrecision);

                                if (Cashgrd.records[event.index]['PaymwentBaseType'] == "ARR" || Cashgrd.records[event.index]['PaymwentBaseType'] == "APP") {
                                    if (event.value_new > Cashgrd.records[event.index]['ConvertedAmt']) {
                                        if (Cashgrd.records[event.index]['ConvertedAmt'] < 0) {
                                            if (Cashgrd.get(event.recid).changes.Discount == undefined && Cashgrd.get(event.recid).changes.OverUnder == undefined) {
                                                w2ui.CashGrid.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * Cashgrd.records[event.index]['ConvertedAmt']) + (event.value_new + Cashgrd.records[event.index]['OverUnder'] + Cashgrd.records[event.index]['Discount']))).toFixed(stdPrecision);
                                                Cashgrd.records[event.index]['VA009_RecivedAmt'] = Cashgrd.get(event.recid).changes.VA009_RecivedAmt;
                                            }
                                            else if (Cashgrd.get(event.recid).changes.Discount == undefined) {
                                                w2ui.CashGrid.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * Cashgrd.records[event.index]['ConvertedAmt']) + (event.value_new + parseFloat(Cashgrd.get(event.recid).OverUnder) + Cashgrd.records[event.index]['Discount']))).toFixed(stdPrecision);
                                                Cashgrd.records[event.index]['VA009_RecivedAmt'] = Cashgrd.get(event.recid).changes.VA009_RecivedAmt;
                                            }
                                            else if (Cashgrd.get(event.recid).changes.OverUnder == undefined) {
                                                w2ui.CashGrid.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * Cashgrd.records[event.index]['ConvertedAmt']) + (event.value_new + Cashgrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).Discount)))).toFixed(stdPrecision);
                                                Cashgrd.records[event.index]['VA009_RecivedAmt'] = Cashgrd.get(event.recid).changes.VA009_RecivedAmt;
                                            }
                                            else {
                                                Cashgrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * Cashgrd.records[event.index]['ConvertedAmt']) + (event.value_new + parseFloat(Cashgrd.get(event.recid).OverUnder) + VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).Discount)))).toFixed(stdPrecision);
                                                Cashgrd.records[event.index]['VA009_RecivedAmt'] = Cashgrd.get(event.recid).changes.VA009_RecivedAmt;
                                            }
                                        }
                                        else {
                                            Cashgrd.get(event.recid).changes.VA009_RecivedAmt = (event.value_new - Cashgrd.records[event.index]['ConvertedAmt']).toFixed(stdPrecision);
                                            Cashgrd.records[event.index]['VA009_RecivedAmt'] = Cashgrd.get(event.recid).changes.VA009_RecivedAmt;
                                        }
                                        Cashgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                    }
                                    else if (event.value_new <= Cashgrd.records[event.index]['ConvertedAmt']) {
                                        if (!Cashgrd.get(event.recid).changes.Writeoff) {
                                            Cashgrd.get(event.recid).changes.Writeoff = 0;
                                        }

                                        if (Cashgrd.get(event.recid).changes.Discount == undefined && Cashgrd.get(event.recid).changes.OverUnder == undefined) {
                                            if (VIS.Utility.Util.getValueOfDecimal((Cashgrd.records[event.index]['OverUnder'])) > 0) {
                                                w2ui.CashGrid.get(event.recid).changes.OverUnder = (Cashgrd.records[event.index]['ConvertedAmt'] - (event.value_new + Cashgrd.records[event.index]['VA009_RecivedAmt'] + Cashgrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                if (Cashgrd.get(event.recid).changes.OverUnder < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.OverUnder = Cashgrd.records[event.index]['OverUnder'];
                                                    Cashgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                    Cashgrd.records[event.index]['Writeoff'] = event.value_original;
                                                    Cashgrd.refreshCell(event.recid, "Writeoff");
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['OverUnder'] = Math.abs(Cashgrd.get(event.recid).changes.OverUnder);
                                                }
                                            }
                                            else {
                                                w2ui.CashGrid.get(event.recid).changes.VA009_RecivedAmt = (Cashgrd.records[event.index]['ConvertedAmt'] - (event.value_new + Cashgrd.records[event.index]['OverUnder'] + Cashgrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                 //VIS_427 BugId 2325 handled received to not be negative when user changes writeoff
                                                if (Cashgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.VA009_RecivedAmt = Cashgrd.records[event.index]['VA009_RecivedAmt'];
                                                    Cashgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                    Cashgrd.records[event.index]['Writeoff'] = event.value_original;
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(Cashgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                }
                                            }
                                        }
                                        else if (Cashgrd.get(event.recid).changes.Discount == undefined) {

                                            if (VIS.Utility.Util.getValueOfDecimal((Cashgrd.records[event.index]['OverUnder'])) > 0) {
                                                w2ui.CashGrid.get(event.recid).changes.OverUnder = (Cashgrd.records[event.index]['ConvertedAmt'] - (event.value_new + Cashgrd.records[event.index]['VA009_RecivedAmt'] + Cashgrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                 //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                if (Cashgrd.get(event.recid).changes.OverUnder < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.OverUnder = Cashgrd.records[event.index]['OverUnder'];
                                                    Cashgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                    Cashgrd.records[event.index]['Writeoff'] = event.value_original;
                                                    Cashgrd.refreshCell(event.recid, "Writeoff");
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['OverUnder'] = Math.abs(Cashgrd.get(event.recid).changes.OverUnder);
                                                }
                                            }
                                            else {
                                                w2ui.CashGrid.get(event.recid).changes.VA009_RecivedAmt = (Cashgrd.records[event.index]['ConvertedAmt'] - (event.value_new + parseFloat(Cashgrd.get(event.recid).OverUnder) + Cashgrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                 //VIS_427 BugId 2325 handled received to not be negative when user changes writeoff
                                                if (Cashgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.VA009_RecivedAmt = Cashgrd.records[event.index]['VA009_RecivedAmt'];
                                                    Cashgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                    Cashgrd.records[event.index]['Writeoff'] = event.value_original;
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(Cashgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                }
                                            }
                                        }
                                        else if (Cashgrd.get(event.recid).changes.OverUnder == undefined) {
                                            if (VIS.Utility.Util.getValueOfDecimal((Cashgrd.records[event.index]['OverUnder'])) > 0) {
                                                w2ui.CashGrid.get(event.recid).changes.OverUnder = (Cashgrd.records[event.index]['ConvertedAmt'] - (event.value_new + Cashgrd.records[event.index]['VA009_RecivedAmt'] + Cashgrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                 //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                if (Cashgrd.get(event.recid).changes.OverUnder < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.records[event.index]['Writeoff'] = event.value_original;
                                                    Cashgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                    Cashgrd.refreshCell(event.recid, "Writeoff");
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['OverUnder'] = Math.abs(Cashgrd.get(event.recid).changes.OverUnder);
                                                }
                                            }
                                            else {
                                                w2ui.CashGrid.get(event.recid).changes.VA009_RecivedAmt = (Cashgrd.records[event.index]['ConvertedAmt'] - (event.value_new + Cashgrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                 //VIS_427 BugId 2325 handled received to not be negative when user changes writeoff
                                                if (Cashgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.VA009_RecivedAmt = Cashgrd.records[event.index]['VA009_RecivedAmt'];
                                                    Cashgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                    Cashgrd.records[event.index]['Writeoff'] = event.value_original;
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(Cashgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                }
                                            }
                                        }
                                        else {
                                            if (VIS.Utility.Util.getValueOfDecimal((Cashgrd.records[event.index]['OverUnder'])) > 0) {
                                                w2ui.CashGrid.get(event.recid).changes.OverUnder = (Cashgrd.records[event.index]['ConvertedAmt'] - (event.value_new + Cashgrd.records[event.index]['VA009_RecivedAmt'] + Cashgrd.records[event.index]['Discount'])).toFixed(stdPrecision);
                                                 //VIS_427 BugId 2325 handled overunder to not be negative when user changes writeoff
                                                if (Cashgrd.get(event.recid).changes.OverUnder < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.OverUnder = Cashgrd.records[event.index]['OverUnder'];
                                                    Cashgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                    Cashgrd.records[event.index]['Writeoff'] = event.value_original;
                                                    Cashgrd.refreshCell(event.recid, "Writeoff");
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['OverUnder'] = Math.abs(Cashgrd.get(event.recid).changes.OverUnder);
                                                }
                                            }
                                            else {
                                                Cashgrd.get(event.recid).changes.VA009_RecivedAmt = (Cashgrd.records[event.index]['ConvertedAmt'] - (event.value_new + parseFloat(Cashgrd.get(event.recid).OverUnder) + VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).Discount))).toFixed(stdPrecision);
                                                 //VIS_427 BugId 2325 handled received to not be negative when user changes writeoff
                                                if (Cashgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.VA009_RecivedAmt = Cashgrd.records[event.index]['VA009_RecivedAmt'];
                                                    Cashgrd.get(event.recid).changes.Writeoff = event.value_original;
                                                    Cashgrd.records[event.index]['Writeoff'] = event.value_original;
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(Cashgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                }
                                            }
                                        }
                                        Cashgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                        Cashgrd.refreshCell(event.recid, "OverUnder");
                                    }
                                }
                            }
                            //discount
                            if (event.column == 8) {
                                if (event.value_new == "") {
                                    event.value_new = 0;
                                }
                                else {
                                    event.value_new = format.GetConvertedNumber(event.value_new, dotFormatter);
                                }
                                //else if (event.value_new.toString().contains(',')) {
                                //    event.value_new = parseFloat(event.value_new.replace(',', '.'));
                                //}

                                Cashgrd.records[event.index]['Discount'] = event.value_new.toFixed(stdPrecision);

                                if (event.value_new > Cashgrd.records[event.index]['ConvertedAmt']) {
                                    VIS.ADialog.error("MoreScheduleAmount");
                                    event.value_new = event.value_original;
                                    Cashgrd.get(event.recid).changes.Discount = event.value_new;
                                    Cashgrd.get(event.recid).Discount = event.value_new;
                                    Cashgrd.refreshCell(event.recid, "Discount");
                                    return;
                                }

                                if (Cashgrd.records[event.index]['PaymwentBaseType'] == "ARR" || Cashgrd.records[event.index]['PaymwentBaseType'] == "APP") {
                                    if (event.value_new > Cashgrd.records[event.index]['ConvertedAmt']) {
                                        if (Cashgrd.records[event.index]['ConvertedAmt'] < 0) {
                                            if (Cashgrd.get(event.recid).changes.Writeoff == undefined && Cashgrd.get(event.recid).changes.OverUnder == undefined) {
                                                w2ui.CashGrid.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).ConvertedAmt)) + (event.value_new + Cashgrd.records[event.index]['OverUnder'] + Cashgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                Cashgrd.records[event.index]['VA009_RecivedAmt'] = Cashgrd.get(event.recid).changes.VA009_RecivedAmt;
                                            }
                                            else if (Cashgrd.get(event.recid).changes.Writeoff == undefined) {
                                                w2ui.CashGrid.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).ConvertedAmt)) + (event.value_new + parseFloat(Cashgrd.get(event.recid).OverUnder) + Cashgrd.records[event.index]['Writeoff']))).toFixed(stdPrecision);
                                                Cashgrd.records[event.index]['VA009_RecivedAmt'] = Cashgrd.get(event.recid).changes.VA009_RecivedAmt;
                                            }
                                            else if (Cashgrd.get(event.recid).changes.OverUnder == undefined) {
                                                w2ui.CashGrid.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).ConvertedAmt)) + (event.value_new + Cashgrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).Writeoff)))).toFixed(stdPrecision);
                                                Cashgrd.records[event.index]['VA009_RecivedAmt'] = Cashgrd.get(event.recid).changes.VA009_RecivedAmt;
                                            }
                                            else {
                                                Cashgrd.get(event.recid).changes.VA009_RecivedAmt = (-1 * ((-1 * VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).ConvertedAmt)) + (event.value_new + parseFloat(Cashgrd.get(event.recid).OverUnder) + VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).Writeoff)))).toFixed(stdPrecision);
                                                Cashgrd.records[event.index]['VA009_RecivedAmt'] = Cashgrd.get(event.recid).changes.VA009_RecivedAmt;
                                            }
                                        }
                                        else {
                                            Cashgrd.get(event.recid).changes.VA009_RecivedAmt = (event.value_new - VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).ConvertedAmt)).toFixed(stdPrecision);
                                            Cashgrd.records[event.index]['VA009_RecivedAmt'] = Cashgrd.get(event.recid).changes.VA009_RecivedAmt;
                                        }
                                        Cashgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                    }
                                    else if (event.value_new <= Cashgrd.records[event.index]['ConvertedAmt']) {

                                        if (!Cashgrd.get(event.recid).changes.Writeoff) {
                                            Cashgrd.get(event.recid).changes.Writeoff = 0;
                                        }

                                        if (Cashgrd.get(event.recid).changes.Writeoff == undefined && Cashgrd.get(event.recid).changes.OverUnder == undefined) {
                                            if (VIS.Utility.Util.getValueOfDecimal((Cashgrd.records[event.index]['OverUnder'])) > 0) {
                                                w2ui.CashGrid.get(event.recid).changes.OverUnder = (Cashgrd.records[event.index]['ConvertedAmt'] - (event.value_new + Cashgrd.records[event.index]['VA009_RecivedAmt'] + Cashgrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                if (Cashgrd.get(event.recid).changes.OverUnder < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.OverUnder = Cashgrd.records[event.index]['OverUnder'];
                                                    Cashgrd.get(event.recid).changes.Discount = event.value_original;
                                                    Cashgrd.records[event.index]['Discount'] = event.value_original;
                                                    Cashgrd.refreshCell(event.recid, "Discount");
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['OverUnder'] = Math.abs(Cashgrd.get(event.recid).changes.OverUnder);
                                                }
                                            }
                                            else {
                                                w2ui.CashGrid.get(event.recid).changes.VA009_RecivedAmt = (VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).ConvertedAmt) - (event.value_new + Cashgrd.records[event.index]['OverUnder'] + Cashgrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                //VIS_427 BugId 2325 handled received to not be negative when user changes Discount
                                                if (Cashgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.VA009_RecivedAmt = Cashgrd.records[event.index]['VA009_RecivedAmt'];
                                                    Cashgrd.get(event.recid).changes.Discount = event.value_original;
                                                    Cashgrd.records[event.index]['Discount'] = event.value_original;
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(Cashgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                }
                                            }
                                        }
                                        else if (Cashgrd.get(event.recid).changes.Writeoff == undefined) {
                                            if (VIS.Utility.Util.getValueOfDecimal((Cashgrd.records[event.index]['OverUnder'])) > 0) {
                                                w2ui.CashGrid.get(event.recid).changes.OverUnder = (Cashgrd.records[event.index]['ConvertedAmt'] - (event.value_new + Cashgrd.records[event.index]['VA009_RecivedAmt'] + Cashgrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                  //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                if (Cashgrd.get(event.recid).changes.OverUnder < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.OverUnder = Cashgrd.records[event.index]['OverUnder'];
                                                    Cashgrd.get(event.recid).changes.Discount = event.value_original;
                                                    Cashgrd.records[event.index]['Discount'] = event.value_original;
                                                    Cashgrd.refreshCell(event.recid, "Discount");
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['OverUnder'] = Math.abs(Cashgrd.get(event.recid).changes.OverUnder);
                                                }
                                            }
                                            else {
                                                w2ui.CashGrid.get(event.recid).changes.VA009_RecivedAmt = (VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).ConvertedAmt) - (event.value_new + parseFloat(Cashgrd.get(event.recid).OverUnder) + Cashgrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                //VIS_427 BugId 2325 handled received to not be negative when user changes Discount
                                                if (Cashgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.VA009_RecivedAmt = Cashgrd.records[event.index]['VA009_RecivedAmt'];
                                                    Cashgrd.get(event.recid).changes.Discount = event.value_original;
                                                    Cashgrd.records[event.index]['Discount'] = event.value_original;
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(Cashgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                }
                                            }
                                        }
                                        else if (Cashgrd.get(event.recid).changes.OverUnder == undefined) {
                                            if (VIS.Utility.Util.getValueOfDecimal((Cashgrd.records[event.index]['OverUnder'])) > 0) {
                                                w2ui.CashGrid.get(event.recid).changes.OverUnder = (Cashgrd.records[event.index]['ConvertedAmt'] - (event.value_new + Cashgrd.records[event.index]['VA009_RecivedAmt'] + Cashgrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                if (Cashgrd.get(event.recid).changes.OverUnder < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.OverUnder = Cashgrd.records[event.index]['OverUnder'];
                                                    Cashgrd.get(event.recid).changes.Discount = event.value_original;
                                                    Cashgrd.records[event.index]['Discount'] = event.value_original;
                                                    Cashgrd.refreshCell(event.recid, "Discount");
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['OverUnder'] = Math.abs(Cashgrd.get(event.recid).changes.OverUnder);
                                                }
                                            }
                                            else {
                                                w2ui.CashGrid.get(event.recid).changes.VA009_RecivedAmt = (VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).ConvertedAmt) - (event.value_new + Cashgrd.records[event.index]['OverUnder'] + VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).Writeoff))).toFixed(stdPrecision);
                                                //VIS_427 BugId 2325 handled received to not be negative when user changes Discount
                                                if (Cashgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.VA009_RecivedAmt = Cashgrd.records[event.index]['VA009_RecivedAmt'];
                                                    Cashgrd.get(event.recid).changes.Discount = event.value_original;
                                                    Cashgrd.records[event.index]['Discount'] = event.value_original;
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(Cashgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                }
                                            }
                                        }
                                        else {
                                            if (VIS.Utility.Util.getValueOfDecimal((Cashgrd.records[event.index]['OverUnder'])) > 0) {
                                                w2ui.CashGrid.get(event.recid).changes.OverUnder = (Cashgrd.records[event.index]['ConvertedAmt'] - (event.value_new + Cashgrd.records[event.index]['VA009_RecivedAmt'] + Cashgrd.records[event.index]['Writeoff'])).toFixed(stdPrecision);
                                                //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                if (Cashgrd.get(event.recid).changes.OverUnder < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.OverUnder = Cashgrd.records[event.index]['OverUnder'];
                                                    Cashgrd.get(event.recid).changes.Discount = event.value_original;
                                                    Cashgrd.records[event.index]['Discount'] = event.value_original;
                                                    Cashgrd.refreshCell(event.recid, "Discount");
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['OverUnder'] = Math.abs(Cashgrd.get(event.recid).changes.OverUnder);
                                                }
                                            }
                                            else {
                                                Cashgrd.get(event.recid).changes.VA009_RecivedAmt = (VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).ConvertedAmt) - (event.value_new + parseFloat(Cashgrd.get(event.recid).OverUnder) + VIS.Utility.Util.getValueOfDecimal(Cashgrd.get(event.recid).Writeoff))).toFixed(stdPrecision);
                                                //VIS_427 BugId 2325 handled overunder to not be negative when user changes Discount
                                                if (Cashgrd.get(event.recid).changes.VA009_RecivedAmt < 0) {
                                                    VIS.ADialog.error("MoreScheduleAmount");
                                                    Cashgrd.get(event.recid).changes.VA009_RecivedAmt = Cashgrd.records[event.index]['VA009_RecivedAmt'];
                                                    Cashgrd.get(event.recid).changes.Discount = event.value_original;
                                                    Cashgrd.records[event.index]['Discount'] = event.value_original;
                                                }
                                                else {
                                                    Cashgrd.records[event.index]['VA009_RecivedAmt'] = Math.abs(Cashgrd.get(event.recid).changes.VA009_RecivedAmt);
                                                }
                                            }
                                        }
                                        Cashgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                                        Cashgrd.refreshCell(event.recid, "OverUnder");
                                    }
                                }
                            }

                            Cashgrd.refreshCell(event.recid, "ConvertedAmt");
                            Cashgrd.refreshCell(event.recid, "VA009_RecivedAmt");
                            Cashgrd.refreshCell(event.recid, "OverUnder");
                            Cashgrd.refreshCell(event.recid, "Writeoff");
                            Cashgrd.refreshCell(event.recid, "Discount");
                        }
                    }, 100);
                    //end
                });

                CashDialog.onOkClick = function () {

                    var _CollaborateData = [];
                    Cashgrd.selectAll();
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) > 0) {
                        //Rakesh(VA228):Make Document Type selection mandatory
                        if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) > 0) {
                            if (VIS.Utility.Util.getValueOfInt($Cash_cmbcashbk.val()) > 0) {
                                if ($POP_DateAcct.val() != "" && $POP_DateAcct.val() != null) {
                                    if ($POP_DateTrx.val() != "" && $POP_DateTrx.val() != null) {
                                        if (Cashgrd.getSelection().length > 0) {
                                            for (var i = 0; i < Cashgrd.getSelection().length; i++) {
                                                var _data = {};
                                                _data["C_BPartner_ID"] = Cashgrd.get(Cashgrd.getSelection()[i])['C_BPartner_ID'];
                                                _data["Description"] = Cashgrd.get(Cashgrd.getSelection()[i])['Description'];
                                                _data["C_Invoice_ID"] = Cashgrd.get(Cashgrd.getSelection()[i])['C_Invoice_ID'];
                                                _data["AD_Org_ID"] = VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val());
                                                _data["AD_Client_ID"] = Cashgrd.get(Cashgrd.getSelection()[i])['AD_Client_ID'];
                                                _data["C_Currency_ID"] = Cashgrd.get(Cashgrd.getSelection()[i])['C_Currency_ID'];
                                                _data["C_Currency_ID"] = Cashgrd.get(Cashgrd.getSelection()[i])["C_Currency_ID"];
                                                _data["C_InvoicePaySchedule_ID"] = Cashgrd.get(Cashgrd.getSelection()[i])["C_InvoicePaySchedule_ID"];
                                                _data["C_BPartner_Location_ID"] = Cashgrd.get(Cashgrd.getSelection()[i])['C_BPartner_Location_ID'];
                                                _data["C_DocType_ID"] = Cashgrd.get(Cashgrd.getSelection()[i])['C_DocType_ID'];
                                                _data["DocBaseType"] = Cashgrd.get(Cashgrd.getSelection()[i])['DocBaseType'];

                                                //change amit
                                                //if (Cashgrd.get(Cashgrd.get(Cashgrd.getSelection()[i])['recid']).changes.VA009_RecivedAmt != undefined) {
                                                //    _data["VA009_RecivedAmt"] = Cashgrd.get(Cashgrd.get(Cashgrd.getSelection()[i])['recid']).changes.VA009_RecivedAmt;
                                                //}
                                                //else
                                                if (Cashgrd.get(Cashgrd.getSelection()[i])['VA009_RecivedAmt'] != null && Cashgrd.get(Cashgrd.getSelection()[i])['VA009_RecivedAmt'] > 0) {
                                                    //_data["VA009_RecivedAmt"] = Cashgrd.get(Cashgrd.get(Cashgrd.getSelection()[i])['recid']).changes.VA009_RecivedAmt;
                                                    _data["VA009_RecivedAmt"] = Cashgrd.get(Cashgrd.getSelection()[i])['VA009_RecivedAmt'];
                                                }
                                                else {
                                                    VIS.ADialog.info(("VA009_PLRecivedAmt"));
                                                    Cashgrd.selectNone();
                                                    return false;
                                                }
                                                //if (Cashgrd.get(Cashgrd.get(Cashgrd.getSelection()[i])['recid']).changes.OverUnder != undefined) {
                                                //    _data["OverUnder"] = Cashgrd.get(Cashgrd.get(Cashgrd.getSelection()[i])['recid']).changes.OverUnder;
                                                //}
                                                //else {
                                                _data["OverUnder"] = Cashgrd.get(Cashgrd.getSelection()[i])['OverUnder'];
                                                //}
                                                //if (Cashgrd.get(Cashgrd.get(Cashgrd.getSelection()[i])['recid']).changes.Writeoff != undefined) {
                                                //    _data["Writeoff"] = Cashgrd.get(Cashgrd.get(Cashgrd.getSelection()[i])['recid']).changes.Writeoff;
                                                //}
                                                //else {
                                                _data["Writeoff"] = Cashgrd.get(Cashgrd.getSelection()[i])['Writeoff'];
                                                //}
                                                //if (Cashgrd.get(Cashgrd.get(Cashgrd.getSelection()[i])['recid']).changes.Discount != undefined) {
                                                //    _data["Discount"] = Cashgrd.get(Cashgrd.get(Cashgrd.getSelection()[i])['recid']).changes.Discount;
                                                //}
                                                //else {
                                                _data["Discount"] = Cashgrd.get(Cashgrd.getSelection()[i])['Discount'];
                                                //}
                                                //amit
                                                // Added by Bharat
                                                _data["ConvertedAmt"] = Cashgrd.get(Cashgrd.getSelection()[i])['ConvertedAmt'];
                                                _data["PaymwentBaseType"] = Cashgrd.get(Cashgrd.getSelection()[i])['PaymwentBaseType'];
                                                _data["CurrencyType"] = $pop_cmbCurrencyType.val();

                                                _data["DateAcct"] = VIS.Utility.Util.getValueOfDate($POP_DateAcct.val())
                                                //Transaction Date
                                                _data["DateTrx"] = VIS.Utility.Util.getValueOfDate($POP_DateTrx.val());
                                                //Rakesh(VA228):Set Document Type (13/Sep/2021)
                                                _data["TargetDocType"] = VIS.Utility.Util.getValueOfInt($POP_targetDocType.val());
                                                //VA230:Set TransactionType
                                                _data["TransactionType"] = Cashgrd.get(Cashgrd.getSelection()[i])['TransactionType'];
                                                _CollaborateData.push(_data);
                                            }
                                        }
                                        $bsyDiv[0].style.visibility = "visible";
                                        $.ajax({
                                            url: VIS.Application.contextUrl + "VA009/Payment/GenPaymentsCash",
                                            type: "POST",
                                            datatype: "json",
                                            // contentType: "application/json; charset=utf-8",
                                            async: true,
                                            data: ({ PaymentData: JSON.stringify(_CollaborateData), C_CashBook_ID: parseInt($Cash_cmbcashbk.val()), BeginningBalance: cashAmount.getValue() }),
                                            success: function (result) {
                                                callbackCashPaymnt(result);
                                            },
                                            error: function (ex) {
                                                console.log(ex);
                                                $bsyDiv[0].style.visibility = "hidden";
                                                VIS.ADialog.error("VA009_ErrorLoadingPayments");
                                            }
                                        });
                                    } else {
                                        VIS.ADialog.info(("VA009_PLSelectConversionDate"));
                                        Cashgrd.selectNone();
                                        return false;
                                    }
                                }
                                else {
                                    VIS.ADialog.info(("VA009_PLSelectAcctDate"));
                                    Cashgrd.selectNone();
                                    return false;
                                }

                            }
                            else {
                                VIS.ADialog.info(("VA009_PLSelectCashbook"));
                                Cashgrd.selectNone();
                                return false;
                            }
                        } else {
                            VIS.ADialog.info(("VA009_PlsSelectDocumentType"));
                            Cashgrd.selectNone();
                            return false;
                        }
                    }
                    else {
                        VIS.ADialog.info(("VA009_PlsSelectOrg"));
                        Cashgrd.selectNone();
                        return false;
                    }
                };

                function callbackCashPaymnt(result) {
                    result = JSON.parse(result);
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; SlctdJournalPaymentIds = []; batchObjInv = []; batchObjOrd = []; batchObjJournal = [];
                    resetPaging();
                    //after successfully created Payment selectall checkbox should be false
                    $selectall.prop('checked', false);
                    //loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
                    loadPaymetsAll();
                    clearamtid();
                    $bsyDiv[0].style.visibility = "hidden";
                    //VIS.ADialog.info("", null, result, null);
                    // changed info message window to Error message window according to requirement
                    VIS.ADialog.info("", null, result, null);
                    //w2alert(result.toString());
                };

                CashDialog.onCancelCLick = function () {
                    w2ui['CashGrid'].clear();
                    CashDispose();
                };

                CashDialog.onClose = function () {
                    w2ui['CashGrid'].clear();
                    CashDispose();

                };
                this.vetoablechange = function (evt) {
                    console.log(evt.propertyName);
                    if (evt.propertyName == "VA009_POP_Txtopngbal_" + $self.windowNo + "") {
                        cashAmount.setValue(evt.newValue);
                    }
                };
                function CashDispose() {
                    _Cash = null;
                    $cash = null;
                    STAT_cmbBank = null;
                    STAT_cmbBankAccount = null;
                    STAT_txtStatementNo = null;
                    STAT_ctrlLoadFile = null;
                    STAT_ctrlLoadFileTxt = null;
                    w2ui['CashGrid'].destroy();
                }
            },

            Batch_OpenDialog: function () {

                $opnbatch = $("<div class='VA009-popform-content' style='min-height:25px !important'>");
                var _openbatch = "";
                _openbatch += "<div> <div class='VA009-popfrmradiowrp'>"
                    + "<input type='radio' name='VA009_Sel" + $self.windowNo + "'  value='S' checked>"
                    + "<label>" + VIS.Msg.getMsg("VA009_BasedOnSelection") + "</label></div>"
                    + "<div class='VA009-popfrmradiowrp'><input type='radio' name='VA009_Sel" + $self.windowNo + "'  value='R'><label>" + VIS.Msg.getMsg("VA009_BasedOnRule") + '</label></div>';

                $opnbatch.append(_openbatch);
                //Batch_getControls();

                var BatchDialog = new VIS.ChildDialog();
                var Selected = "";
                BatchDialog.setContent($opnbatch);
                BatchDialog.setTitle(VIS.Msg.getMsg("VA009_LoadBatchPayment"));
                BatchDialog.setWidth("30%");
                BatchDialog.setEnableResize(true);
                BatchDialog.setModal(true);
                BatchDialog.show();

                BatchDialog.onOkClick = function () {
                    Selected = $opnbatch.find("input[name='VA009_Sel" + $self.windowNo + "']:checked").val();
                    $bsyDiv[0].style.visibility = "visible";
                    if (Selected == "S") {
                        if (SlctdPaymentIds.toString() != "" || SlctdOrderPaymentIds.toString() != "" || SlctdJournalPaymentIds.toString() != "") {

                            _loadFunctions.Batch_Dialog();

                        }
                        else
                            VIS.ADialog.info(("VA009_PlzSelct1Pay"));
                    }
                    else if (Selected == "R") {
                        //var _Qry = "SELECT pm.va009_paymentbasetype FROM c_invoicepayschedule cs INNER JOIN va009_paymentmethod pm " +
                        //    " ON pm.va009_paymentmethod_id      =cs.va009_paymentmethod_id WHERE cs.c_invoicepayschedule_id IN (";
                        //if (SlctdPaymentIds.length == 0) {
                        //    _Qry += 0;
                        //}
                        //else {
                        //    _Qry += SlctdPaymentIds.toString();
                        //}
                        //_Qry += " ) AND pm.va009_paymentbasetype  IN ('S','B') GROUP BY pm.va009_paymentbasetype ";

                        //var _sql = VIS.secureEngine.encrypt(_Qry);
                        ////var paystatus = VIS.DB.executeDataSet(_Qry);
                        //var paystatus = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetPaymentBaseType", { "BatchQry": _sql }, null);

                        //if (paystatus != null && paystatus.tables[0].rows.length > 0) {
                        //if (paystatus != null && paystatus.length > 0) {
                        //    VIS.ADialog.info(("VA009_CantSelectCashandChque"));
                        //}
                        //else {
                        generateLines();
                        //}

                    }
                    $bsyDiv[0].style.visibility = "hidden";
                };

                BatchDialog.onCancelCLick = function () {
                    BatchDispose();
                };

                BatchDialog.onClose = function () {
                    BatchDispose();

                };

                function BatchDispose() {
                    _openbatch = null;
                    $opnbatch = null;
                }
            },

            Batch_Dialog: function () {
                var InvScheduleLookup = null;
                var BatchGrid, _Cheque_no = "";
                var _C_Bank_ID = 0, _C_BankAccount_ID = 0;
                //Varriables
                var TotalAPC = 0, TotalAPI = 0;
                $batch = $("<div class='VA009-popform-content vis-formouterwrpdiv' style='min-height:395px !important'>");
                var _batch = "";
                _batch += "<div class='VA009-popfrm-wrap' style='height:auto;'>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbOrg_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_Org") + "</label>"
                    + "</div></div>"

                    //added new field called Target Type
                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_TargetType_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_DocType") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbBank_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_Bank") + "</label>"
                    + "</div></div> "

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbBankAccount_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_BankAccount") + "</label>"
                    + "</div></div>"

                    //creae new form field  currency 
                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbCurrency_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_Currency") + "</label>"
                    + "</div></div>"
                    //creae new form field  currency type
                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbCurrencyType_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_CurrencyType") + "</label>"
                    + "</div></div>"

                    // Payment method and overwrite payment method Suggested by Ashish and Rajni

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbPaymthd_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_PayMthd") + "</label>"
                    + "</div></div>"

                    //VA230:Append account date
                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_AccountDate_" + $self.windowNo + "' placeholder=' ' data-placeholder=''> "
                    + "<label>" + VIS.Msg.getMsg("AccountDate") + "</label>"
                    + "</div></div>"
                    //re-arranged the place of the filed
                    + "<div class='VA009-popform-data'><div class='VA009-popFormInn'>"
                    //+ "<label style='display: none;'>" + VIS.Msg.getMsg("VA009_OverwritePayMthd") + "</label>"
                    + "<label class='vis-ec-col-lblchkbox'><input type='checkbox' disabled id='VA009_OverwritePayMthd_" + $self.windowNo + "'>&nbsp;" + VIS.Msg.getMsg("VA009_OverwritePayMthd") + '</label>'

                    //+ "<div class='vis-control-wrap'>"
                    //+ "<label style='display: none;'>" + VIS.Msg.getMsg("VA009_Consolidate") + "</label>"
                    //added new button to show check details if payment method is cheque
                    + "<label class='vis-ec-col-lblchkbox'><input type='checkbox' id='VA009_Consolidate_" + $self.windowNo + "'>&nbsp;" + VIS.Msg.getMsg("VA009_Consolidate") + '</label> </div></div>'

                    + "<div class='VA009-popform-data'><div class='VA009-popFormInn'>"
                    + " <a class='btn VA009-blueBtn VA009-batchCheckDetail' id='VA009_btnCheckDetails_" + $self.windowNo + "'> " + VIS.Msg.getMsg("VA009_CheckDetails") + "</a >"
                    + " </div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap' style='display:none !important'><div class='vis-control-wrap'>"
                    + "<select style='display:none !important' id='VA009_POP_cmbCurrencyType_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label style='display:none !important'>" + VIS.Msg.getMsg("VA009_CurrencyType") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-table-container' style='height:300px;' id='VA009_btnPopupGrid'> </div>"
                    + "</div>";
                /* VIS_427 DevOps id:2268 Dialog for payment success*/
                $batchResult = $("<div>"
                    + "<label class='VA009-FontStyle mb-3' id='VA009_Note_" + $self.windowNo + "'></label>"
                    + "</div>");
                $resltbtns = $("<div class='VA009-ButtonDivStyle'>" +
                    "<div class='d-flex align-items-center justify-content-end'>" +
                    "<button class='ui-button mr-3' id='VA009_ViewBatch_" + $self.windowNo + "'>" + VIS.Msg.getMsg("VA009_ViewBatch") + "</button>" +
                    "<button class= 'ui-button' id ='VA009_Cancel_" + $self.windowNo + "'>" + VIS.Msg.getMsg("Cancel") + "</button >" +
                    "</div >" +
                    "</div>"
                );

                $batch.append(_batch);
                var BatchDialog = new VIS.ChildDialog();
                BatchDialog.setContent($batch);
                BatchDialog.setTitle(VIS.Msg.getMsg("VA009_LoadBatchPayment"));
                BatchDialog.setWidth("60%");
                //VA230:Remove outer scroll bar
                //BatchDialog.setHeight(window.innerHeight - 80);
                BatchDialog.setEnableResize(true);
                BatchDialog.setModal(true);
                BatchDialog.show();
                Batch_getControls();
                BatchGrid_Layout();
                /* VIS_427 DevOps id:2268 Appended buttons*/
                $batchResult.append($resltbtns);
                $ViewBatch = $batchResult.find("#VA009_ViewBatch_" + $self.windowNo);
                $cancel = $batchResult.find("#VA009_Cancel_" + $self.windowNo);
                loadgrdBatch(callbackBatch);
                loadOrg();
                //Rakesh(VA228):10/Sep/2021 -> Load APP target base doc type
                _loadFunctions.LoadTargetDocType($POP_targetDocType, _TargetBaseType);
                //populate banks based on selected organization in dialog
                loadbanks($POP_cmbBank, VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()));
                //Set system date as accountdate
                var now = new Date();
                var _today = now.getFullYear() + "-" + (("0" + (now.getMonth() + 1)).slice(-2)) + "-" + (("0" + now.getDate()).slice(-2));
                $POP_DateAcct.val(_today);
                $POP_cmbBank.addClass('vis-ev-col-mandatory');
                $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                //set payment method mandatory assigned by ashish 28 May 2020
                $POP_PayMthd.addClass('vis-ev-col-mandatory');
                loadPayMthd();
                $POP_cmbCurrency.addClass('vis-ev-col-mandatory');
                loadCurrency();
                loadCurrencyType();
                InvScheduleLookup = VIS.MLookupFactory.get(VIS.Env.getCtx(), $self.windowNo, 0, VIS.DisplayType.TableDir, "C_InvoicePaySchedule_ID", 0, false, "C_InvoicePaySchedule.IsActive='Y' ");

                function BatchGrid_Layout() {

                    var _batch_Columns = [];
                    if (_batch_Columns.length == 0) {
                        _batch_Columns.push({ field: "recid", caption: VIS.Msg.getMsg("VA009_srno"), sortable: true, size: '10%' });
                        _batch_Columns.push({ field: "C_Bpartner", caption: VIS.Msg.getMsg("VA009_BPartner"), sortable: true, size: '10%' });
                        //_batch_Columns.push({ field: "C_Invoice_ID", caption: VIS.Msg.getMsg("VA009_Invoice"), sortable: true, size: '10%' });
                        _batch_Columns.push({
                            field: "C_InvoicePaySchedule_ID", caption: VIS.Msg.getMsg("VA009_Schedule"), sortable: true, size: '10%',
                            render: function (record, index, col_index) {
                                var l = InvScheduleLookup;
                                var val = record["C_InvoicePaySchedule_ID"];
                                var d;
                                if (l) {
                                    d = l.getDisplay(val);
                                }
                                return d;
                            }
                        });
                        _batch_Columns.push({ field: "CurrencyCode", caption: VIS.Msg.getMsg("VA009_Currency"), sortable: true, size: '10%' });
                        _batch_Columns.push({
                            field: "DueAmt", caption: VIS.Msg.getMsg("VA009_DueAmt"), sortable: true, size: '10%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["DueAmt"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }
                        });
                        // _batch_Columns.push({ field: "VA009_RecivedAmt", caption: VIS.Msg.getMsg("VA009_PayAmt"), sortable: true, size: '10%', render: 'float:2',style: 'text-align: left', editable: { type: 'float' } });
                        _batch_Columns.push({
                            field: "ConvertedAmt", caption: VIS.Msg.getMsg("VA009_ConvertedAmt"), sortable: true, size: '13%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["ConvertedAmt"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }
                        });
                        _batch_Columns.push({ field: "CheckNumber", caption: VIS.Msg.getMsg("VA009_ChkNo"), sortable: true, size: '10%', editable: { type: 'alphanumeric', autoFormat: true, groupSymbol: ' ' } });
                        _batch_Columns.push({
                            field: "CheckDate", caption: VIS.Msg.getMsg("VA009_CheckDate"), sortable: true, size: '10%',
                            render: function (record, index, col_index) {
                                var val;
                                //when user do double click on CheckDate field then mouse over without selecting the value at that time 
                                //record.changes.CheckDate is get empty string value so to avoid that used condtion is compared with empty string
                                if (record.changes == undefined || record.changes.CheckDate == "") {
                                    val = record["CheckDate"];
                                }
                                else {
                                    val = record.changes.CheckDate;
                                }
                                return new Date(val).toLocaleDateString();
                            }, style: 'text-align: left', editable: { type: 'date' }
                        });
                        //_batch_Columns.push({ field: "ValidMonths", caption: VIS.Msg.getMsg("VA009_ValidMonths"), sortable: true, size: '10%', editable: { type: 'text' } });
                        _batch_Columns.push({ field: "Mandate", caption: VIS.Msg.getMsg("VA009_Mandate"), sortable: true, size: '10%', editable: { type: 'text' } });
                        //by Amit - 1-12-2016
                        _batch_Columns.push({ field: "TransactionType", caption: VIS.Msg.getMsg("VA009_TransactionType"), sortable: true, size: '1%' });
                        //Rakesh(VA228):Set conversion rate type on 17/Sep/2021
                        _batch_Columns.push({ field: "ConversionTypeId", caption: VIS.Msg.getMsg("VA009_ConversionType"), hidden: true, sortable: true, size: '0%' });
                        _batch_Columns.push({ field: "DiscountAmount", caption: VIS.Msg.getMsg("DiscountAmount"), hidden: true, sortable: true, size: '0%' });
                        _batch_Columns.push({ field: "ConvertedDiscountAmount", caption: VIS.Msg.getMsg("ConvertedDiscountAmount"), hidden: true, sortable: true, size: '0%' });
                        _batch_Columns.push({ field: "DiscountDate", caption: VIS.Msg.getMsg("DiscountDate"), hidden: true, sortable: true, size: '0%' });
                        _batch_Columns.push({ field: "C_BPartner_Location_ID", caption: VIS.Msg.getMsg("C_BPartner_Location_ID"), hidden: true, sortable: true, size: '0%' });
                        _batch_Columns.push({ field: "C_DocType_ID", caption: VIS.Msg.getMsg("C_DocType_ID"), hidden: true, sortable: true, size: '0%' });
                        _batch_Columns.push({ field: "DocBaseType", caption: VIS.Msg.getMsg("DocBaseType"), hidden: true, sortable: true, size: '0%' });
                        _batch_Columns.push({ field: "PaymwentBaseType", caption: VIS.Msg.getMsg("PaymwentBaseType"), hidden: true, sortable: true, size: '0%' });
                    }
                    BatchGrd = null;
                    BatchGrd = BatchGrid.w2grid({
                        name: 'BatchGrid',
                        recordHeight: 25,
                        multiSelect: true,
                        columns: _batch_Columns
                        //method: 'GET',
                        //show: {
                        //    selectColumn: true
                        //}
                    }),
                        BatchGrd.hideColumn('recid', 'CheckNumber', 'CheckDate', 'ValidMonths', 'Mandate',
                            'TransactionType', 'ConversionTypeId', 'DiscountAmount', "ConvertedDiscountAmount",
                            'DiscountDate');

                };

                function Batch_getControls() {
                    $POP_cmbBank = $batch.find("#VA009_POP_cmbBank_" + $self.windowNo);
                    $POP_cmbBankAccount = $batch.find("#VA009_POP_cmbBankAccount_" + $self.windowNo);
                    $POP_cmbCurrency = $batch.find("#VA009_POP_cmbCurrency_" + $self.windowNo);
                    $POP_PayMthd = $batch.find("#VA009_POP_cmbPaymthd_" + $self.windowNo);
                    BatchGrid = $batch.find("#VA009_btnPopupGrid");
                    $consolidate = $batch.find("#VA009_Consolidate_" + $self.windowNo);
                    $overwritepay = $batch.find("#VA009_OverwritePayMthd_" + $self.windowNo);
                    $pop_cmbCurrencyType = $batch.find("#VA009_POP_cmbCurrencyType_" + $self.windowNo);
                    $POP_cmbOrg = $batch.find("#VA009_POP_cmbOrg_" + $self.windowNo);
                    $POP_cmbOrg.addClass('vis-ev-col-mandatory');
                    $successNoteofbatch = $batchResult.find("#VA009_Note_" + $self.windowNo);
                    //added new button to show check details if payment method is cheque
                    $POP_BtnChkDetails = $batch.find("#VA009_btnCheckDetails_" + $self.windowNo);
                    if ($CP_Tab.hasClass('VA009-active-tab') && ($('option:selected', $POP_PayMthd).attr('PaymentBaseType') == "S"))
                        $POP_BtnChkDetails.show();
                    else
                        $POP_BtnChkDetails.hide();
                    //get the Id of Target Type field
                    $POP_targetDocType = $batch.find("#VA009_POP_TargetType_" + $self.windowNo);
                    //set Target Type as Mandatory
                    $POP_targetDocType.addClass('vis-ev-col-mandatory');
                    // Payment method and overwrite payment method Suggested by Ashish and Rajni
                    $consolidate.prop('checked', true);
                    $overwritepay.prop('checked', true);
                    //VA230:Get AccountDate control
                    $POP_DateAcct = $batch.find("#VA009_AccountDate_" + $self.windowNo);
                };

                function loadgrdBatch(callback) {
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetPopUpData",
                        type: "POST",
                        datatype: "json",
                        //contentType: "application/json; charset=utf-8",
                        //async: false,
                        data: ({
                            InvPayids: SlctdPaymentIds.toString(), bank_id: _C_Bank_ID, acctno: _C_BankAccount_ID,
                            chkno: VIS.Utility.encodeText(_Cheque_no), OrderPayids: SlctdOrderPaymentIds.toString(),
                            JournalPayids: SlctdJournalPaymentIds.toString()
                        }),
                        success: function (result) {
                            callback(result);
                        },
                        error: function () {
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                };

                function callbackBatch(result) {
                    popupgrddata = [];
                    var rslt = JSON.parse(result);
                    TotalAPC = 0, TotalAPI = 0;
                    reloaddata = rslt;
                    for (var i in rslt) {
                        var line = {};
                        //Add the corrosponding value in the varriable based on DocBaseType
                        if (rslt[i].DocBaseType == "API") {
                            TotalAPI += rslt[i].convertedAmt;
                        }
                        else if (rslt[i].DocBaseType == "APC") {
                            TotalAPC += rslt[i].convertedAmt;
                        }
                        line["recid"] = rslt[i].recid;
                        line["C_Bpartner"] = rslt[i].C_Bpartner;
                        line["C_BPartner_Location_ID"] = rslt[i].C_BPartner_Location_ID;
                        line["C_DocType_ID"] = rslt[i].C_DocType_ID;
                        line["C_Invoice_ID"] = rslt[i].C_Invoice_ID;
                        line["C_InvoicePaySchedule_ID"] = rslt[i].C_InvoicePaySchedule_ID;
                        line["CurrencyCode"] = rslt[i].CurrencyCode;
                        line["DueAmt"] = rslt[i].DueAmt;
                        line["ConvertedAmt"] = rslt[i].convertedAmt;
                        line["C_BPartner_ID"] = rslt[i].C_BPartner_ID;
                        line["C_Currency_ID"] = rslt[i].C_Currency_ID;
                        line["AD_Org_ID"] = rslt[i].AD_Org_ID;
                        line["AD_Client_ID"] = rslt[i].AD_Client_ID;
                        line["CheckDate"] = new Date();//new Date().toString('MM/DD/YYYY');
                        line["CheckNumber"] = null;
                        line["ValidMonths"] = null;
                        line["VA009_PaymentMethod_ID"] = rslt[i].VA009_PaymentMethod_ID;
                        line["TransactionType"] = rslt[i].TransactionType;
                        line["PaymwentBaseType"] = rslt[i].PaymwentBaseType;
                        //Rakesh(VA228):Set conversion type and discount amount/date
                        line["ConversionTypeId"] = rslt[i].ConversionTypeId;
                        line["DiscountAmount"] = rslt[i].DiscountAmount;
                        line["ConvertedDiscountAmount"] = rslt[i].ConvertedDiscountAmount;
                        line["DiscountDate"] = rslt[i].DiscountDate;
                        line["TotalAPC"] = rslt[i].TotalAPC;
                        line["TotalAPI"] = rslt[i].TotalAPI;
                        line["DocBaseType"] = rslt[i].DocBaseType;
                        line["IsAPCGreater"] = rslt[i].IsAPCGreater;
                        line["IsAPCExists"] = rslt[i].IsAPCExists;
                        line["PrintConvertedAmt"] = rslt[i].convertedAmt;
                        popupgrddata.push(line);
                    }
                    w2utils.encodeTags(popupgrddata);
                    BatchGrd.add(popupgrddata);
                };

                function loadCurrencyType() {

                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/loadCurrencyType", null, callbackCurrencyType);

                    function callbackCurrencyType(dr) {
                        $pop_cmbCurrencyType.append("<option value='0'></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $pop_cmbCurrencyType.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                                if (VIS.Utility.encodeText(dr[i].IsDefault) == "Y") {
                                    defaultCurrenyType = VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID);
                                }
                            }
                            //$pop_cmbCurrencyType.prop('selectedIndex', 1);
                            $pop_cmbCurrencyType.val(defaultCurrenyType)
                        }
                    }
                };

                //to load all organization 
                function loadOrg() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadOrganization", null, callbackloadorg);
                    function callbackloadorg(dr) {
                        $POP_cmbOrg.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_cmbOrg.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].AD_Org_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                            }
                        }
                        $POP_cmbOrg.prop('selectedIndex', 0);
                    };
                };

                //to load TargetType DocTypes
                function loadTargetType() {
                    $POP_targetDocType.empty();
                    var _org_Id = $POP_cmbOrg != null ? $POP_cmbOrg.val() : 0;
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadTargetType", { "ad_org_Id": _org_Id, "basetype": 3 }, callbacktargettype);
                    function callbacktargettype(dr) {
                        $POP_targetDocType.append(" <option value = 0></option>");
                        if (dr != null && dr.length > 0) {
                            for (var i in dr) {
                                $POP_targetDocType.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_DocType_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                            }
                        }
                        $POP_targetDocType.prop('selectedIndex', 0);
                        $POP_targetDocType.addClass('vis-ev-col-mandatory');
                    };
                };

                //Set Mandatory and Non-Mandatory
                function SetMandatory(value) {
                    if (value)
                        return '#FFB6C1';
                    else
                        return 'White';
                };
                //load currencies
                function loadCurrency() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/loadCurrencies", null, callbackCurrencyType);

                    function callbackCurrencyType(dr) {
                        $POP_cmbCurrency.append("<option value='0'></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_cmbCurrency.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_Currency_ID) + ">" + VIS.Utility.encodeText(dr[i].ISO_Code) + "</option>");
                            }
                            $POP_cmbCurrency.prop('selectedIndex', 0);
                        }
                    }
                };
                //end
                $POP_cmbOrg.on("change", function () {
                    //to set org mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) == 0) {
                        $POP_cmbOrg.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        //populate banks based on selected organization in dialog
                        loadbanks($POP_cmbBank, VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()));
                        //refresh the bank and BankAccount dropdowns and make it as mandatory
                        $POP_cmbBank.val(0).prop('selected', true);
                        $POP_cmbBank.addClass('vis-ev-col-mandatory');
                        $POP_cmbBankAccount.empty();
                        $POP_cmbBankAccount.append("<option value='0'></option>");
                        $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                        $POP_cmbOrg.removeClass('vis-ev-col-mandatory');
                    }
                    //get the Target Doc Types
                    //loadTargetType();
                    //Rakesh(VA228):10/Sep/2021 -> Load APP target base doc type
                    _loadFunctions.LoadTargetDocType($POP_targetDocType, _TargetBaseType);
                });

                //on change event for Target Type field
                $POP_targetDocType.on("change", function () {
                    //to $POP_targetDocType org mandatory
                    if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) == 0) {
                        $POP_targetDocType.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        //remove mandatory class if value > zero
                        $POP_targetDocType.removeClass('vis-ev-col-mandatory');
                    }
                });

                $POP_cmbBank.on('change', function () {
                    //to set bank mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBank.val()) > 0) {
                        $POP_cmbBank.removeClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_cmbBank.addClass('vis-ev-col-mandatory');
                    }
                    $POP_cmbBankAccount.empty();
                    //added mandatory class
                    $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                    //to get bank account of selected organization assigned by Ashish on 28 May 2020
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadBankAccount", { "Bank_ID": $POP_cmbBank.val(), "Orgs": $POP_cmbOrg.val() }, callbackloadbankAcct);

                    function callbackloadbankAcct(dr) {
                        $POP_cmbBankAccount.append("<option value='0'></option>");
                        if (dr != null) {
                            if (dr.length > 0) {
                                for (var i in dr) {
                                    $POP_cmbBankAccount.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_BankAccount_ID) + ">" + VIS.Utility.encodeText(dr[i].AccountNo) + "</option>");
                                }
                            }
                        }
                    }
                });

                $POP_cmbBankAccount.on("change", function () {
                    //to set bank account mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()) == 0) {
                        $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_cmbBankAccount.removeClass('vis-ev-col-mandatory');
                    }

                    //end
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetConvertedAmt",
                        type: "POST",
                        datatype: "json",
                        // contentType: "application/json; charset=utf-8",
                        async: true,
                        //fixed issue with null exeception
                        data: ({ PaymentData: JSON.stringify(reloaddata), BankAccount: $POP_cmbBankAccount.val(), CurrencyType: $pop_cmbCurrencyType.val(), dateAcct: $POP_DateAcct != null ? $POP_DateAcct.val() : null, _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                        success: function (result) {
                            callbackchqReload(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });

                    //get and set Currency of  Bank Account
                    var Currency = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetBankAccountCurrency", { "BankAccount_ID": $POP_cmbBankAccount.val() });
                    if (Currency > 0) {
                        $POP_cmbCurrency.val(Currency);
                        $POP_cmbCurrency.removeClass('vis-ev-col-mandatory');
                    }
                });

                //on the change of Currency type get the  converted amount
                $pop_cmbCurrencyType.on("change", function () {
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetConvertedAmtBatch",
                        type: "POST",
                        datatype: "json",
                        // contentType: "application/json; charset=utf-8",
                        async: true,
                        data: ({ PaymentData: JSON.stringify(reloaddata), BankAccount: $POP_cmbBankAccount.val(), CurrencyType: $pop_cmbCurrencyType.val(), ToCurrency: $POP_cmbCurrency.val(), dateAcct: $POP_DateAcct != null ? $POP_DateAcct.val() : null, _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                        success: function (result) {
                            callbackchqReload(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                });

                //added new button to show check details if payment method is cheque
                $POP_BtnChkDetails.on("click", function (e) {

                    if (parseInt($POP_cmbBankAccount.val()) > 0) {

                        if (parseInt($POP_PayMthd.val()) > 0) {
                            $.ajax({
                                url: VIS.Application.contextUrl + "VA009/Payment/GetCheckDetails",
                                type: "POST",
                                datatype: "json",
                                // contentType: "application/json; charset=utf-8",
                                async: true,
                                data: ({
                                    C_BankAccount_ID: parseInt($POP_cmbBankAccount.val()),
                                    VA009_PaymentMethod_ID: parseInt($POP_PayMthd.val())
                                }),
                                success: function (result) {
                                    callbackgetCheckDetails(result);
                                },
                                error: function (ex) {
                                    console.log(ex);
                                    VIS.ADialog.error("VA009_ErrorLoadingPayments");
                                }
                            });
                            function callbackgetCheckDetails(dr) {
                                if (dr != null) {
                                    console.log(dr);
                                    _loadFunctions.ChequeDetails_Dialog(dr, $consolidate.prop('checked'));
                                }
                            }
                        }
                        else {
                            VIS.ADialog.info("VA009_PlsSelectPayMethod");
                            BatchGrd.selectNone();
                            return false;
                        }
                    }
                    else {
                        VIS.ADialog.info("VA009_PLSelectBankAccount");
                        BatchGrd.selectNone();
                        return false;
                    }


                });

                //on the change of Currency get the  converted amount
                $POP_cmbCurrency.on("change", function () {
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbCurrency.val()) == 0) {
                        $POP_cmbCurrency.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_cmbCurrency.removeClass('vis-ev-col-mandatory');
                    }
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetConvertedAmtBatch",
                        type: "POST",
                        datatype: "json",
                        // contentType: "application/json; charset=utf-8",
                        async: true,
                        data: ({ PaymentData: JSON.stringify(reloaddata), BankAccount: $POP_cmbBankAccount.val(), CurrencyType: $pop_cmbCurrencyType.val(), ToCurrency: $POP_cmbCurrency.val(), dateAcct: $POP_DateAcct != null ? $POP_DateAcct.val() : null, _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                        success: function (result) {
                            callbackchqReload(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                });

                //VA230:on change event for DateAcct it should convert the Amount accordingly
                $POP_DateAcct.on("change", function () {
                    if ($POP_DateAcct.val()) {
                        $POP_DateAcct.removeClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_DateAcct.addClass('vis-ev-col-mandatory');
                    }
                    // Devops Task Id - 1637
                    //VIS317  Mandatory Field validation for Account date change.
                    //case Of Create Batch (Based On Selection).
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PlsSelectOrg"));
                        return false;
                    }
                    if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PlsSelectDocType"));
                        return false;
                    }
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBank.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PlsSelectBank"));
                        return false;
                    }
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PlsSelectBankAccount"));
                        return false;
                    }
                    if (VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()) <= 0) {
                        VIS.ADialog.info(("VA009_PlsSelectPaymentMethod"));
                        return false;
                    }
                    if ($POP_DateAcct.val() == "") {
                        VIS.ADialog.info(("VA009_PLSelectAcctDate"));
                        return false;
                    }
                    //VIS317 Invalid Account date check
                    var dateVal = Date.parse($POP_DateAcct.val());
                    var currentTime = new Date(parseInt(dateVal));
                    //check if date is valid
                    //this check will work for 01/01/1970 on words
                    if (!isNaN(currentTime.getTime()) && currentTime.getTime() < 0) {
                        return;
                    }
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetConvertedAmtBatch",
                        type: "POST",
                        datatype: "json",
                        async: true,
                        data: ({ PaymentData: JSON.stringify(reloaddata), BankAccount: $POP_cmbBankAccount.val(), CurrencyType: $pop_cmbCurrencyType.val(), ToCurrency: $POP_cmbCurrency.val(), dateAcct: $POP_DateAcct != null ? $POP_DateAcct.val() : null, _org_Id: $POP_cmbOrg.val() <= 0 ? 0 : $POP_cmbOrg.val() }),
                        success: function (result) {
                            callbackchqReload(result);
                        },
                        error: function (ex) {
                            console.log(ex);
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                });
                function callbackchqReload(result) {
                    BatchGrd.clear();
                    popupgrddata = [];
                    TotalAPC = 0, TotalAPI = 0;
                    var rslt = JSON.parse(result);
                    reloaddata = rslt;
                    for (var i in rslt) {
                        var line = {};
                        //Add the corrosponding value in the varriable based on DocBaseType
                        if (rslt[i].DocBaseType == "API") {
                            TotalAPI += rslt[i].convertedAmt;
                        }
                        else if (rslt[i].DocBaseType == "APC") {
                            TotalAPC += rslt[i].convertedAmt;
                        }
                        line["recid"] = rslt[i].recid;
                        line["C_Bpartner"] = rslt[i].C_Bpartner;
                        line["C_BPartner_Location_ID"] = rslt[i].C_BPartner_Location_ID;
                        line["C_DocType_ID"] = rslt[i].C_DocType_ID;
                        line["C_Invoice_ID"] = rslt[i].C_Invoice_ID;
                        line["C_InvoicePaySchedule_ID"] = rslt[i].C_InvoicePaySchedule_ID;
                        line["CurrencyCode"] = rslt[i].CurrencyCode;
                        line["DueAmt"] = rslt[i].DueAmt;
                        line["ConvertedAmt"] = rslt[i].convertedAmt;
                        // line["VA009_RecivedAmt"] = Globalize.format(rslt[i].VA009_RecivedAmt, "N");
                        line["C_BPartner_ID"] = rslt[i].C_BPartner_ID;
                        line["C_Currency_ID"] = rslt[i].C_Currency_ID;
                        line["AD_Org_ID"] = rslt[i].AD_Org_ID;
                        line["AD_Client_ID"] = rslt[i].AD_Client_ID;
                        line["CheckDate"] = new Date();//new Date().toString('MM/DD/YYYY');
                        line["CheckNumber"] = null;
                        line["ValidMonths"] = null;
                        line["VA009_PaymentMethod_ID"] = rslt[i].VA009_PaymentMethod_ID;
                        line["TransactionType"] = rslt[i].TransactionType;
                        line["PaymwentBaseType"] = rslt[i].PaymwentBaseType;
                        //Rakesh(VA228):Set conversion type
                        line["ConversionTypeId"] = rslt[i].ConversionTypeId;
                        line["DiscountAmount"] = rslt[i].DiscountAmount;
                        line["ConvertedDiscountAmount"] = rslt[i].ConvertedDiscountAmount;
                        line["DiscountDate"] = rslt[i].DiscountDate;
                        line["TotalAPC"] = rslt[i].TotalAPC;
                        line["TotalAPI"] = rslt[i].TotalAPI;
                        line["DocBaseType"] = rslt[i].DocBaseType;
                        line["IsAPCGreater"] = rslt[i].IsAPCGreater;
                        line["IsAPCExists"] = rslt[i].IsAPCExists;
                        line["PrintConvertedAmt"] = rslt[i].convertedAmt;
                        popupgrddata.push(line);
                    }
                    if (rslt[0].ERROR == "ConversionNotFound") {
                        VIS.ADialog.info("VA009_ConversionNotFound");
                    }
                    w2utils.encodeTags(popupgrddata);
                    BatchGrd.add(popupgrddata);
                };

                function loadPayMthd() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadBatchPaymentMethod", null, callbackloadpaymthds);
                    function callbackloadpaymthds(dr) {
                        $POP_PayMthd.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_PayMthd.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].VA009_PaymentMethod_ID) + " PaymentBaseType=" + dr[i].VA009_PaymentBaseType + ">" + VIS.Utility.encodeText(dr[i].VA009_Name) + "</option>");
                            }
                        }
                        $POP_PayMthd.prop('selectedIndex', 0);
                    }
                };

                BatchGrd.on('change', function (event) {
                    if (event.column == 7)
                        BatchGrd.records[event.index]['CheckNumber'] = event.value_new;
                    if (event.column == 8) {
                        //Used do double click on CheckDate field and closed without selecting the value at 
                        //that time its return empty string to avoid Invalid Date used this below condition compared with empty string
                        if (event.value_new != "") {
                            BatchGrd.records[event.index]['CheckDate'] = event.value_new;
                        }
                        else {
                            BatchGrd.records[event.index]['CheckDate'] = event.value_previous != "" ? event.value_previous : event.value_original;
                        }
                    }
                    if (event.column == 9)
                        BatchGrd.records[event.index]['ValidMonths'] = event.value_new;
                });

                $POP_PayMthd.on('change', function (event) {
                    //to set payment method mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()) > 0) {
                        $POP_PayMthd.removeClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_PayMthd.addClass('vis-ev-col-mandatory');
                    }
                    //added new button to show check details if payment method is cheque
                    if ($CP_Tab.hasClass('VA009-active-tab') &&
                        ($('option:selected', $POP_PayMthd).attr('PaymentBaseType') == "S"))
                        $POP_BtnChkDetails.show();
                    else
                        $POP_BtnChkDetails.hide();

                    Payrule = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetPaymentRule", { "PaymentMethod": $POP_PayMthd.val() }, null);
                    if (Payrule == "S")
                        BatchGrd.showColumn('CheckNumber', 'CheckDate', 'ValidMonths', 'Mandate');
                    else
                        BatchGrd.hideColumn('recid', 'CheckNumber', 'CheckDate', 'ValidMonths', 'Mandate');
                });

                BatchDialog.onOkClick = function () {

                    var _CollaborateData = [];
                    BatchGrd.selectAll();
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) > 0) {
                        //Target Type is Mandatory
                        if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) > 0) {
                            if (VIS.Utility.Util.getValueOfInt($POP_cmbBank.val()) > 0) {
                                if (VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()) > 0) {
                                    if (VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()) > 0) {
                                        //if payment method is check and AP Invoice amount is less than AP Credit memo then show the msg.
                                        if ($('option:selected', $POP_PayMthd).attr('PaymentBaseType') == "S") {
                                            if (TotalAPI - TotalAPC < 0) {
                                                VIS.ADialog.info("VA009_NotAllowed");
                                                $bsyDiv[0].style.visibility = "hidden";
                                                BatchGrd.selectNone();
                                                return false;
                                            }
                                        }

                                        if ($POP_DateAcct.val() != "" && $POP_DateAcct.val() != null) { //VA230:AccountDate mandatory check
                                            if (BatchGrd.getSelection().length > 0) {
                                                for (var i = 0; i < BatchGrd.getSelection().length; i++) {
                                                    var _data = {};
                                                    _data["C_BPartner_ID"] = BatchGrd.get(BatchGrd.getSelection()[i])['C_BPartner_ID'];
                                                    _data["C_BPartner_Location_ID"] = BatchGrd.get(BatchGrd.getSelection()[i])['C_BPartner_Location_ID'];
                                                    _data["C_Invoice_ID"] = BatchGrd.get(BatchGrd.getSelection()[i])['C_Invoice_ID'];
                                                    _data["C_InvoicePaySchedule_ID"] = BatchGrd.get(BatchGrd.getSelection()[i])['C_InvoicePaySchedule_ID'];
                                                    _data["AD_Org_ID"] = VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val());
                                                    //Target Doc Type
                                                    _data["TargetDocType"] = VIS.Utility.Util.getValueOfInt($POP_targetDocType.val());
                                                    _data["AD_Client_ID"] = BatchGrd.get(BatchGrd.getSelection()[i])['AD_Client_ID'];
                                                    _data["C_Currency_ID"] = BatchGrd.get(BatchGrd.getSelection()[i])['C_Currency_ID'];
                                                    _data["TransactionType"] = BatchGrd.get(BatchGrd.getSelection()[i])['TransactionType'];
                                                    _data["DueAmt"] = BatchGrd.get(BatchGrd.getSelection()[i])['DueAmt'];
                                                    _data["C_Bank_ID"] = $POP_cmbBank.val();
                                                    _data["C_BankAccount_ID"] = $POP_cmbBankAccount.val();
                                                    //Rakesh(VA228):Set converion type/discount amount and date assigned by amit done on 17/sep/2021
                                                    _data["ConversionTypeId"] = BatchGrd.get(BatchGrd.getSelection()[i])['ConversionTypeId'];
                                                    _data["DiscountAmount"] = BatchGrd.get(BatchGrd.getSelection()[i])['DiscountAmount'];
                                                    _data["ConvertedDiscountAmount"] = BatchGrd.get(BatchGrd.getSelection()[i])['ConvertedDiscountAmount'];
                                                    _data["DiscountDate"] = BatchGrd.get(BatchGrd.getSelection()[i])['DiscountDate'] != null ?
                                                        VIS.Utility.Util.getValueOfDate(BatchGrd.get(BatchGrd.getSelection()[i])['DiscountDate']) : null;
                                                    _data["C_DocType_ID"] = BatchGrd.get(BatchGrd.getSelection()[i])['C_DocType_ID'];
                                                    _data["DocBaseType"] = BatchGrd.get(BatchGrd.getSelection()[i])['DocBaseType'];
                                                    _data["PaymwentBaseType"] = BatchGrd.get(BatchGrd.getSelection()[i])['PaymwentBaseType'];
                                                    _data["TotalAPI"] = VIS.Utility.Util.getValueOfDecimal(BatchGrd.get(BatchGrd.getSelection()[i])['TotalAPI']);
                                                    _data["TotalAPC"] = VIS.Utility.Util.getValueOfDecimal(BatchGrd.get(BatchGrd.getSelection()[i])['TotalAPC']);

                                                    if (_data["DueAmt"] != 0 && BatchGrd.get(BatchGrd.getSelection()[i])['ConvertedAmt'] == 0) {
                                                        //ConvertedAmt is zero then show the message Conversion Rate not found
                                                        VIS.ADialog.info("VA009_ConversionNotFound");
                                                        return false;
                                                    }
                                                    else
                                                        _data["ConvertedAmt"] = BatchGrd.get(BatchGrd.getSelection()[i])['ConvertedAmt'];

                                                    if ($overwritepay.prop('checked') == true) {
                                                        _data["VA009_PaymentMethod_ID"] = $POP_PayMthd.val();
                                                        _data["isOverwrite"] = "Y";
                                                    }
                                                    else {
                                                        _data["VA009_PaymentMethod_ID"] = BatchGrd.get(BatchGrd.getSelection()[i])['VA009_PaymentMethod_ID'];
                                                        _data["isOverwrite"] = "N";
                                                    }
                                                    if ($consolidate.prop('checked') == true) {
                                                        _data["isconsolidate"] = "Y";
                                                    }
                                                    else {
                                                        _data["isconsolidate"] = "N";
                                                    }
                                                    if (Payrule == "S") {
                                                        var dt = new Date(BatchGrd.get(BatchGrd.getSelection()[i])['CheckDate']);
                                                        _data["CheckDate"] = VIS.Utility.Util.getValueOfDate(BatchGrd.get(BatchGrd.getSelection()[i])['CheckDate']);

                                                        if (BatchGrd.get(BatchGrd.getSelection()[i])['CheckNumber'] != null)
                                                            _data["CheckNumber"] = BatchGrd.get(BatchGrd.getSelection()[i])['CheckNumber'];
                                                        else {
                                                            VIS.ADialog.info("VA009_PLCheckNumber");
                                                            BatchGrd.selectNone();
                                                            return false;
                                                        }
                                                    }
                                                    else {
                                                        _data["CheckNumber"] = null;
                                                        _data["CheckDate"] = null;
                                                        _data["ValidMonths"] = null;
                                                    }
                                                    _data["CurrencyType"] = $pop_cmbCurrencyType.val();
                                                    _data["HeaderCurrency"] = $POP_cmbCurrency.val();
                                                    //VA230:Get accountdate
                                                    _data["DateAcct"] = VIS.Utility.Util.getValueOfDate($POP_DateAcct.val());
                                                    _CollaborateData.push(_data);
                                                }
                                                _CollaborateData.sort(SortingAccordingBPLocation); //VIS_427 Devops Taskid: 2238 Called Sorting function for batch
                                            }
                                            $bsyDiv[0].style.visibility = "visible";
                                            $.ajax({
                                                url: VIS.Application.contextUrl + "VA009/Payment/GeneratePaymentsBatch",
                                                type: "POST",
                                                datatype: "json",
                                                // contentType: "application/json; charset=utf-8",
                                                async: true,
                                                data: ({ PaymentData: JSON.stringify(_CollaborateData) }),
                                                success: function (result) {
                                                    callbackBatchPay(result);
                                                },
                                                error: function (ex) {
                                                    console.log(ex);
                                                    $bsyDiv[0].style.visibility = "hidden";
                                                    VIS.ADialog.error("VA009_ErrorLoadingPayments");
                                                }
                                            });
                                        } else {
                                            VIS.ADialog.info("VA009_PLSelectAcctDate");
                                            BatchGrd.selectNone();
                                            return false;
                                        }
                                    }
                                    else {
                                        VIS.ADialog.info("VA009_PlsSelectPayMethod");
                                        BatchGrd.selectNone();
                                        return false;
                                    }
                                }
                                else {
                                    VIS.ADialog.info("VA009_PLSelectBankAccount");
                                    BatchGrd.selectNone();
                                    return false;
                                }
                            }
                            else {
                                VIS.ADialog.info("VA009_PLSelectBank");
                                BatchGrd.selectNone();
                                return false;
                            }
                        }
                        else {
                            //Target Type field Validation Message
                            VIS.ADialog.info(("VA009_PLSelectTargetDocType"));
                            BatchGrd.selectNone();
                            return false;
                        }
                    }
                    else {
                        VIS.ADialog.info(("VA009_PlsSelectOrg"));
                        BatchGrd.selectNone();
                        return false;
                    }
                };

                function callbackBatchPay(result) {
                    result = JSON.parse(result);
                    result = result.split(',');/* VIS_427 DevOps id:2268 Splitted the string*/
                    DocNumber = "";
                    batchid = parseInt(result[1]);
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; SlctdJournalPaymentIds = []; batchObjInv = []; batchObjOrd = []; batchObjJournal = [];
                    resetPaging();
                    //after successfully created Payment selectall checkbox should be false
                    $selectall.prop('checked', false);
                    //loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
                    loadPaymetsAll();
                    clearamtid();
                    $bsyDiv[0].style.visibility = "hidden";
                    if (DocNumber != "") {
                        w2confirm(VIS.Msg.getMsg('VA009_GenPaymentFile'))
                            .yes(function () {
                                $selectall.prop('checked', false);
                                $divPayment.find(':checkbox').prop('checked', false);
                                prepareDataForPaymentFile(DocNumber, true);
                            })
                            .no(function () {
                                $bsyDiv[0].style.visibility = "hidden";
                                VIS.ADialog.info("", null, result[0], "");
                            });
                    }
                    else {
                        if (result != null) {
                            /* VIS_427 DevOps id:2268 Created child dialog for payment success*/
                            Batchsuccesspay = new VIS.ChildDialog();
                            $successNoteofbatch.text(result[0]);
                            $successNoteofbatch.css('visibility', 'visible');
                            Batchsuccesspay.setContent($batchResult);
                            Batchsuccesspay.setTitle(VIS.Msg.getMsg("VA009_LoadBatchPayment"));
                            Batchsuccesspay.setWidth("36%");
                            Batchsuccesspay.show();
                            Batchsuccesspay.hidebuttons();
                        }
                    }
                };
                /* VIS_427 DevOps id:2268 Zoom functionality added to zoom batch window*/
                $ViewBatch.on("click", function () {
                    if (batchid > 0) {
                        zoomToWindow(batchid, "VA009_PaymentBatch", "VA009_Batch_ID");
                    }
                    Batchsuccesspay.close();
                });
                $cancel.on("click", function () {
                    Batchsuccesspay.close();
                });

                BatchDialog.onCancelCLick = function () {
                    w2ui['BatchGrid'].clear();
                    BatchDispose();
                };

                BatchDialog.onClose = function () {
                    w2ui['BatchGrid'].clear();
                    BatchDispose();

                };

                function BatchDispose() {
                    _batch = null;
                    $batch = null;
                    STAT_cmbBank = null;
                    STAT_cmbBankAccount = null;
                    STAT_txtStatementNo = null;
                    STAT_ctrlLoadFile = null;
                    STAT_ctrlLoadFileTxt = null;
                    w2ui['BatchGrid'].destroy();
                };
            },

            Split_Dialog: function () {
                var SplitGrid, _Cheque_no = "";
                var _C_Bank_ID = 0, _C_BankAccount_ID = 0;
                $bsyDiv[0].style.visibility = "visible";
                $split = $("<div class='VA009-popform-content vis-formouterwrpdiv' style='min-height:333px !important'>");
                var _split = "";
                _split += "<div class='VA009-popform-data input-group vis-input-wrap' style='height:auto;'><div class='vis-control-wrap'>"
                    + "<input type='text' step='any' id='VA009_POP_TxtSplitAmt_" + $self.windowNo + "'  placeholder=' ' data-placeholder=''>"
                    + "<label>" + VIS.Msg.getMsg("VA009_lblSplitAmt") + "</label></div>"
                    + "  <a tabindex='' class='btn VA009-blueBtn' id='VA009_btnSplitAmt_" + $self.windowNo + "' style='margin-top: 0px !important; margin-left: 5px;'>Split Schedule</a> </div>"

                    + "  <div class='VA009-table-container' id='VA009_btnPopupGrid'>  </div> "
                    + "</div>";

                $split.append(_split);
                Split_getControls();

                var SplitDialog = new VIS.ChildDialog();

                SplitDialog.setContent($split);
                SplitDialog.setTitle(VIS.Msg.getMsg("VA009_LoadSplitPayment"));
                SplitDialog.setWidth("60%");
                SplitDialog.setEnableResize(true);
                SplitDialog.setModal(true);
                SplitDialog.show();
                SplitGrid_Layout();
                loadgrdSplit(callbackSplitPay);

                function SplitGrid_Layout() {

                    var _Split_Columns = [];
                    if (_Split_Columns.length == 0) {
                        _Split_Columns.push({ field: "recid", caption: VIS.Msg.getMsg("VA009_srno"), sortable: true, size: '10%' });
                        _Split_Columns.push({ field: "C_Bpartner", caption: VIS.Msg.getMsg("VA009_BPartner"), sortable: true, size: '10%' });
                        _Split_Columns.push({ field: "DocumentNo", caption: VIS.Msg.getMsg("VA009_Invoice/Order"), sortable: true, size: '10%' });
                        _Split_Columns.push({ field: "CurrencyCode", caption: VIS.Msg.getMsg("VA009_Currency"), sortable: true, size: '10%' });
                        _Split_Columns.push({
                            field: "DueAmt", caption: VIS.Msg.getMsg("VA009_DueAmt"), sortable: true, size: '10%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["DueAmt"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits: precision });
                            }
                        });
                        _Split_Columns.push({
                            field: "DueDate", caption: VIS.Msg.getMsg("VA009_DueDate"), sortable: true, size: '10%',
                            render: function (record, index, col_index) {
                                var val;
                                if (record.changes == undefined || record.changes.DueDate == "") {
                                    val = record["DueDate"];
                                }
                                else {
                                    //DueDate Can't be less than the DateAcct
                                    //some times not getting proper result if use toLocaleString()
                                    if (new Date(record.changes.DueDate) >= new Date(Splitgrd.records[index]['DateAcct'])) {
                                        val = record.changes.DueDate;
                                    }
                                    else {
                                        val = record["DueDate"];
                                        record.changes.DueDate = record["DueDate"];
                                    }
                                }
                                return new Date(val).toLocaleDateString();
                            }, style: 'text-align: left', editable: { type: 'date' }
                        });
                        //_Split_Columns.push({ field: "DueDate", caption: VIS.Msg.getMsg("VA009_DueDate"), sortable: true, size: '10%', render: 'date', style: 'text-align: left', editable: { type: 'date' } });
                        //by Amit - 1-12-2016
                        _Split_Columns.push({ field: "TransactionType", caption: VIS.Msg.getMsg("VA009_TransactionType"), sortable: true, size: '1%' });
                        //end
                    }
                    Splitgrd = null;
                    Splitgrd = SplitGrid.w2grid({
                        name: 'SplitGrid',
                        recordHeight: 25,
                        columns: _Split_Columns,
                        method: 'GET',
                        multiSelect: true
                        //show: {
                        //    selectColumn: true
                        //}
                    }),
                        Splitgrd.hideColumn('recid');
                    Splitgrd.hideColumn('TransactionType');

                };

                function Split_getControls() {
                    SplitGrid = $split.find("#VA009_btnPopupGrid");
                    $TxtSplitAmt = $split.find("#VA009_POP_TxtSplitAmt_" + $self.windowNo);
                    $BtnSplit = $split.find("#VA009_btnSplitAmt_" + $self.windowNo);
                };

                function loadgrdSplit(callback) {
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GetPopUpData",
                        type: "GET",
                        datatype: "json",
                        contentType: "application/json; charset=utf-8",
                        //async: false,
                        data: ({ InvPayids: SlctdPaymentIds.toString(), bank_id: _C_Bank_ID, acctno: _C_BankAccount_ID, chkno: VIS.Utility.encodeText(_Cheque_no), OrderPayids: SlctdOrderPaymentIds.toString() }),
                        success: function (result) {
                            callback(result);
                        },
                        error: function () {
                            $bsyDiv[0].style.visibility = "hidden";
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                };

                function callbackSplitPay(result) {
                    Splitgrd.clear();
                    popupgrddata = [];
                    var Recid = 0;
                    var rslt = JSON.parse(result);
                    for (var i in rslt) {
                        Recid = Recid + 1;
                        var line = {};
                        //line["recid"] = rslt[i].recid;     / done by Bharat
                        line["recid"] = Recid;
                        line["C_Bpartner"] = rslt[i].C_Bpartner;
                        line["C_Invoice_ID"] = rslt[i].C_Invoice_ID;
                        line["DocumentNo"] = rslt[i].DocumentNo;
                        line["C_InvoicePaySchedule_ID"] = rslt[i].C_InvoicePaySchedule_ID;
                        line["CurrencyCode"] = rslt[i].CurrencyCode;
                        line["DueAmt"] = rslt[i].DueAmt;
                        var date = new Date(rslt[i].DueDate);
                        date.setMinutes(-date.getTimezoneOffset() + date.getMinutes());
                        newValue = date.toISOString();
                        var val = newValue.substring(0, newValue.length - 1);
                        var indexTime = newValue.indexOf("T");
                        line["DueDate"] = val.substring(0, indexTime);
                        //line["DueDate"] = Globalize.format(rslt[i].DueDate);
                        line["C_BPartner_ID"] = rslt[i].C_BPartner_ID;
                        line["C_Currency_ID"] = rslt[i].C_Currency_ID;
                        line["AD_Org_ID"] = rslt[i].AD_Org_ID;
                        line["AD_Client_ID"] = rslt[i].AD_Client_ID;
                        line["TransactionType"] = rslt[i].TransactionType;
                        line["DateAcct"] = rslt[i].DateAcct;
                        popupgrddata.push(line);
                    }
                    w2utils.encodeTags(popupgrddata);
                    Splitgrd.add(popupgrddata);
                    $bsyDiv[0].style.visibility = "hidden";
                };

                Splitgrd.on('change', function (event) {

                    if (event.column == 5)
                        //handled on change event as well DueDate can't less than the DateAcct
                        if (event.value_new != "") {
                            //some times not getting proper result if use toLocaleString()
                            if (new Date(event.value_new) >= new Date(Splitgrd.records[event.index]['DateAcct'])) {
                                Splitgrd.records[event.index]['DueDate'] = event.value_new;
                            }
                            else {
                                Splitgrd.records[event.index]['DueDate'] = event.value_previous == "" ? event.value_original : event.value_previous;
                                event.value_previous = event.value_previous == "" ? event.value_original : event.value_previous;
                                //message text size not more 22 characters
                                //hide of Date picker div before Pop-up will be displayed
                                window.setTimeout(function () {
                                    VIS.ADialog.info("VA009_PlsSelctDueDateGreaterThanTrxDate");
                                }, 5);
                                //return false;
                            }
                        }
                });

                $TxtSplitAmt.on('keypress', function (event) {
                    var isDotSeparator = culture.isDecimalSeparatorDot(window.navigator.language);
                    if ((event.keyCode != 13) && (event.which != 46 || $(this).val().indexOf('.') != -1) && (event.which != 8 && event.which != 0 && (event.which < 48 || event.which > 57)) && (event.keyCode == 45) && (event.keyCode != 44)) {
                        return false;
                    }

                    if (!isDotSeparator && event.keyCode == 46) {// , separator
                        return false;
                    }
                    if (isDotSeparator && event.keyCode == 44) { // . separator
                        return false;
                    }

                    if (event.target.value.contains(".") && (event.which == 46 || event.which == 44)) {
                        if (event.target.value.indexOf('.') > -1) {
                            event.target.value = event.target.value.replace('.', '');
                        }
                    }
                    if (event.target.value.contains(",") && (event.which == 46 || event.which == 44)) {
                        if (event.target.value.indexOf(',') > -1) {
                            event.target.value = event.target.value.replace(',', '');
                        }
                    }

                    if (event.keyCode == 13) {
                        //Splitgrd.selectAll();
                        if (Splitgrd.getSelection().length == 0) {
                            VIS.ADialog.info("PlsSelectRecord");
                            return false;
                        }
                        if ($TxtSplitAmt.val() != "") {
                            if (parseFloat(Splitgrd.get(Splitgrd.getSelection()[0])['DueAmt']) < parseFloat($TxtSplitAmt.val())) {
                                VIS.ADialog.info("Can't Enter Greater Than Due Amt");
                            }
                            else if (parseFloat(Splitgrd.get(Splitgrd.getSelection()[0])['DueAmt']) > 0) {
                                if (parseFloat($TxtSplitAmt.val()) < 0)
                                    VIS.ADialog.info("Can't Enter Negative Amt");
                                else
                                    SplitSchedule();
                            }
                            else if (parseFloat(Splitgrd.get(Splitgrd.getSelection()[0])['DueAmt']) < 0) {
                                if (parseFloat($TxtSplitAmt.val()) > 0)
                                    VIS.ADialog.info("Can't Enter Positive Amt");
                                else
                                    SplitSchedule();
                            }
                            else {
                                SplitSchedule();
                            }
                        }
                        else {
                            VIS.ADialog.info("VA009_PLEnterSplitAmt");
                            //Splitgrd.selectNone();
                            return false;
                        }
                    }
                });

                $BtnSplit.on("click", function (e) {
                    //Splitgrd.selectAll();
                    // Added by Bharat
                    if (Splitgrd.getSelection().length == 0) {
                        VIS.ADialog.info("PlsSelectRecord");
                        return false;
                    }
                    var DueAmt = parseFloat(Splitgrd.get(Splitgrd.getSelection()[0])['DueAmt']);
                    var SplitAMt = $TxtSplitAmt.val();
                    if (SplitAMt != "") {
                        SplitAMt = parseFloat($TxtSplitAmt.val());
                        // IF Value in Negative 
                        if (DueAmt < 0) {
                            DueAmt = -1 * DueAmt;
                        }
                        if (SplitAMt < 0) {
                            SplitAMt = -1 * SplitAMt;
                        }

                        //if (SplitAMt != "") {   done by Bharat
                        //JID_1932_1if dueamt = spliamt then ne need to split the schedule
                        if (DueAmt <= SplitAMt) {
                            VIS.ADialog.info("VA009_CantEnterGreaterAmt");
                        }
                        else if (DueAmt > 0) {
                            if (SplitAMt < 0)
                                VIS.ADialog.info("VA009_CantEnterNegtveAmt");
                            else if (SplitAMt == 0)
                                VIS.ADialog.info("VA009_CantEnterZeroAmt");
                            else
                                SplitSchedule();
                        }
                        else if (DueAmt < 0) {
                            if (SplitAMt > 0)
                                VIS.ADialog.info("VA009_CantEnterPositiveAmt");
                            else
                                SplitSchedule();
                        }
                        else {
                            SplitSchedule();
                        }
                        //}
                    }
                    else {
                        VIS.ADialog.info("VA009_PLEnterSplitAmt");
                        //Splitgrd.selectNone();
                        return false;
                    }
                });

                function SplitSchedule() {
                    _CollaborateData = [];  //done By Bharat
                    //Splitgrd.selectAll();
                    var DueAmt = parseFloat(Splitgrd.get(Splitgrd.getSelection()[0])['DueAmt']);
                    var SplitAMt = checkcommaordot(event, $TxtSplitAmt.val(), $TxtSplitAmt.val());
                    SplitAMt = parseFloat(SplitAMt);
                    // IF Value in Negative 
                    if (DueAmt < 0) {
                        DueAmt = -1 * DueAmt;
                    }
                    if (SplitAMt < 0) {
                        SplitAMt = -1 * SplitAMt;
                    }

                    var lines = (DueAmt / SplitAMt);
                    var TotalLines = Math.ceil(lines);
                    if (TotalLines > 0) {
                        //for (var i = 0; i < TotalLines - 1; i++) {  done by Bharat
                        var _data = {};
                        var recid = Splitgrd.records[Splitgrd.records.length - 1].recid + 1;
                        _data["recid"] = recid;
                        _data["C_BPartner_ID"] = Splitgrd.get(Splitgrd.getSelection()[0])['C_BPartner_ID'];
                        _data["C_Bpartner"] = Splitgrd.get(Splitgrd.getSelection()[0])['C_Bpartner'];
                        _data["CurrencyCode"] = Splitgrd.get(Splitgrd.getSelection()[0])['CurrencyCode'];
                        _data["DocumentNo"] = Splitgrd.get(Splitgrd.getSelection()[0])['DocumentNo'];
                        _data["C_Invoice_ID"] = Splitgrd.get(Splitgrd.getSelection()[0])['C_Invoice_ID'];
                        _data["AD_Org_ID"] = Splitgrd.get(Splitgrd.getSelection()[0])['AD_Org_ID'];
                        _data["AD_Client_ID"] = Splitgrd.get(Splitgrd.getSelection()[0])['AD_Client_ID'];
                        _data["C_Currency_ID"] = Splitgrd.get(Splitgrd.getSelection()[0])['C_Currency_ID'];
                        _data["DueAmt"] = SplitAMt.toFixed(2);
                        _data["C_InvoicePaySchedule_ID"] = Splitgrd.get(Splitgrd.getSelection()[0])['C_InvoicePaySchedule_ID'];
                        _data["DueDate"] = Splitgrd.get(Splitgrd.getSelection()[0])['DueDate'];
                        //add DateAcct to Compare the Edited DueDate with Account Date because User Can't able to select Previous Date than AcctDate
                        _data["DateAcct"] = Splitgrd.get(Splitgrd.getSelection()[0])['DateAcct'];
                        _data["TransactionType"] = Splitgrd.get(Splitgrd.getSelection()[0])['TransactionType'];
                        _CollaborateData.push(_data);
                        //}
                        //var TotalUpdatedAmt = SplitAMt * (TotalLines - 1);   done by Bharat
                        var TotalUpdatedAmt = SplitAMt;
                        if (DueAmt - TotalUpdatedAmt > 0) {

                            var amt = DueAmt - TotalUpdatedAmt;
                            Splitgrd.get(Splitgrd.getSelection()[0]).DueAmt = amt.toFixed(2);
                        }
                    }
                    //Splitgrd.clear();
                    w2utils.encodeTags(_CollaborateData);
                    Splitgrd.add(_CollaborateData);
                    $TxtSplitAmt.val('');
                    Splitgrd.refresh();
                };

                SplitDialog.onOkClick = function () {
                    datasplit = [];
                    datasplit = Splitgrd.records //no of records on grid
                    if (Splitgrd.getSelection().length > 0 || datasplit.length > 1) {
                        Splitgrd.selectAll();

                        //JID_1932_2 check if amount is splitted 
                        if (datasplit.length == 1) {
                            VIS.ADialog.info("VA009_SplitRecordFirst");
                            return false;
                        }
                        for (var i = 0; i < datasplit.length; i++) {
                            if (Splitgrd.get(Splitgrd.getSelection()[i])['DueDate'] == null) {
                                VIS.ADialog.info("VA009_PlzSelctDueDate");
                                return false;
                            }
                        }
                        //}
                        $bsyDiv[0].style.visibility = "visible";
                        $.ajax({
                            url: VIS.Application.contextUrl + "VA009/Payment/CreateSplitPayments",
                            type: "POST",
                            datatype: "json",
                            // contentType: "application/json; charset=utf-8",
                            async: true,
                            data: ({ PaymentData: JSON.stringify(datasplit), SplitAmmount: $TxtSplitAmt.val() }),
                            success: function (result) {
                                callbackSplit(result);
                            },
                            error: function (ex) {

                                console.log(ex);
                                $bsyDiv[0].style.visibility = "hidden";
                                VIS.ADialog.error("VA009_ErrorLoadingPayments");
                            }
                        });
                    }
                    else {
                        VIS.ADialog.info("VA009_PlzSplitRecord");
                        return false;
                    }
                };

                function callbackSplit(result) {
                    result = JSON.parse(result);
                    $divPayment.find('.VA009-payment-wrap').remove();
                    $divBank.find('.VA009-right-data-main').remove();
                    $divBank.find('.VA009-accordion').remove();
                    pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; SlctdJournalPaymentIds = []; batchObjInv = []; batchObjOrd = []; batchObjJournal = [];
                    resetPaging();
                    //after successfully created Payment selectall checkbox should be false
                    $selectall.prop('checked', false);
                    //loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
                    loadPaymetsAll();
                    clearamtid();
                    $bsyDiv[0].style.visibility = "hidden";
                    VIS.ADialog.info(result);
                };

                SplitDialog.onCancelCLick = function () {
                    w2ui['SplitGrid'].clear();
                    SplitDispose();
                };

                SplitDialog.onClose = function () {
                    w2ui['SplitGrid'].clear();
                    SplitDispose();

                };

                function SplitDispose() {
                    $split = null;
                    _split = null;
                    w2ui['SplitGrid'].destroy();
                }
            },

            XML_Dialog: function () {
                if (SlctdPaymentIds.length > 0 || SlctdOrderPaymentIds.length > 0) {
                    File_Para = 'B';
                    w2confirm('Do you want to clear the selection and proceed for SEPA XML Generation Process?')
                        .yes(function () {
                            SlctdPaymentIds = [];
                            SlctdOrderPaymentIds = [];
                            $selectall.prop('checked', false);
                            $divPayment.find(':checkbox').prop('checked', false);
                            generateXMLDialog(File_Para);
                        })
                        .no(function () {
                            console.log("user clicked NO")
                        });
                }
                else {
                    generateXMLDialog(File_Para);
                }
            },

            ChatWindow: function () {
                var dr = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetChatID", { "RecordID": record_ID }, null);
                if (dr != null) {
                    var tableid = dr["AD_Table_ID"];
                    var chatid = dr["CM_Chat_ID"];
                    var chat = new VIS.Chat(record_ID, chatid, tableid, "", this.windowNo);
                    chat.show();
                    chat.onClose = function () {
                        $.ajax({
                            url: VIS.Application.contextUrl + "VA009/Payment/GetChat",
                            type: "POST",
                            datatype: "json",
                            // contentType: "application/json; charset=utf-8",
                            async: true,
                            data: ({ RecordID: record_ID }),
                            success: function (result) {
                                callbackChat(result);
                            },
                            error: function (ex) {

                                console.log(ex);
                                $bsyDiv[0].style.visibility = "hidden";
                                VIS.ADialog.error("VA009_ErrorLoadingPayments");
                            }
                        });

                    }
                };
                function callbackChat(result) {
                    result = JSON.parse(result);
                    $bsyDiv[0].style.visibility = "hidden";
                    $chatDiv = $divPayment.find("#VA009-Chatcolor-gray_" + record_ID);
                    $chatDiv.html("");
                    $chatDiv.append('<span class=VA009-Chatcolor-gray>' + result + '</span>');

                };
            },

            B2B_Dialog: function () {
                //btn Get Next Cheqe Number $OrgCmb,
                var $From_cmbBank, $cmbCurrencyType, $paymentMthd, $cmbCurrencies, $txtAmt, txtAmount, $txtCheckNo, $checkDate, $getNextChkno, $btnPanel;
                var $trnsDate, $acctDate;
                var $isPayment, $isReceipt;
                var organizationids = [];
                var divAmount, format = null;
                lblAmount = $("<label>");
                txtAmount = new VIS.Controls.VAmountTextBox("VA009_Amount" + $self.windowNo + "", false, false, true, 50, 100, VIS.DisplayType.Amount, VIS.Msg.getMsg("Amount"));
                lblAmount.append(VIS.Msg.getMsg("Amount"));
                txtAmount.setValue(0);
                format = VIS.DisplayType.GetNumberFormat(VIS.DisplayType.Amount);
                divAmount = $("<div class='VA009-popform-data input-group vis-input-wrap'>");
                var $divb2bAmountCtrlWrp = $("<div class='vis-control-wrap'>");
                divAmount.append($divb2bAmountCtrlWrp);
                $divb2bAmountCtrlWrp.append(txtAmount.getControl().attr('placeholder', ' ').attr('data-placeholder', '')).append(lblAmount);
                $b2b = $("<div class='VA009-popform-content vis-forms-container vis-formouterwrpdiv' style='min-height:350px !important'>");
                //var _b2b = "";
                //_b2b.append(divAmount);
                _b2b = $("<div class='VA009-popfrm-wrap' style='height:auto;'>");
                _b2bdata = $("<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbOrg_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_Org") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data VA009-popformchkctrlwrap'>"
                    //+ "<label style='visibility: hidden;'>" + VIS.Msg.getMsg("VA009_FromBank") + "</label>"
                    + "<div> <label class='vis-ec-col-lblchkbox'><input type='checkbox' id=VA009_Payment_" + $self.windowNo + ">Payment</label></div><div> <label class='vis-ec-col-lblchkbox'><input type='checkbox' id=VA009_Receipt_" + $self.windowNo + ">Receipt</label></div>"
                    + "</div> "

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbFromBank_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_FromBank") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbToBank_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_ToBank") + "</label>"
                    + "</div></div>"

                    //Rakesh(VA228):Create Doc type element html
                    + "<div class='VA009-popform-data input-group vis-input-wrap' > <div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbAPDocType_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_APDocType") + "</label>"
                    + "</div></div> "

                    //Rakesh(VA228):Create Doc type element html
                    + "<div class='VA009-popform-data input-group vis-input-wrap' > <div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbARDocType_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_ARDocType") + "</label>"
                    + "</div></div> ");

                _b2bdata1 = $("<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_cmbPayMthd_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_PayMethodlbl") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data VA009-b2b-popup input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='text' placeholder=' ' data-placeholder='' class='vis-ev-col-readonly' id='VA009_Chqnotxt_" + $self.windowNo + "' disabled/><label>" + VIS.Msg.getMsg("VA009_ChkNo") + "</label></div><a href='javascript:void(0)' id='VA009_getCheckNo_" + $self.windowNo + "' style=margin-left:4px;>" + VIS.Msg.getMsg("GetNextCheckNo") + "</a>"
                    + "</div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_CheckDate_" + $self.windowNo + "' placeholder=' ' data-placeholder='' class='vis-ev-col-readonly' disabled><label>" + VIS.Msg.getMsg("VA009_CheckDate") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbCurrency_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("Currency") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_cmbCurrencyType_" + $self.windowNo + "'>"
                    + "</select><label>" + VIS.Msg.getMsg("VA009_CurrencyType") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_TransactionDate" + $self.windowNo + "' placeholder=' ' data-placeholder=''><label>" + VIS.Msg.getMsg("TransactionDate") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_AccountDate" + $self.windowNo + "' placeholder=' ' data-placeholder=''><label>" + VIS.Msg.getMsg("AccountDate") + "</label>"
                    + "</div></div>"
                    + "</div>"
                );

                $Note = $("<div class='VA009-popform-data' align='left'  margin-top:10Px;'>"
                    + "<label style='color:red; visibility: hidden;' id='VA009_Note" + $self.windowNo + "'>Please Select Org.</label>"
                    + "</div>");
                _b2b.append(_b2bdata).append(divAmount).append(_b2bdata1);
                $b2b.append(_b2b);

                //1052-- create note div and buttons to display after the creation of payment
                $resultb2b = $("<div style='min-height:75px !important'>"
                    + "<label style='color:red; visibility: hidden;' id='VA009_Note" + $self.windowNo + "'>Please Select Org.</label>"
                    + "</div>");
                $resltbtns = $("<button class='ui-button' id='VA009_CreateNew_" + $self.windowNo + "'style = 'float: right; margin-right: 12px; margin-bottom:12px; visibility:hidden;display:none;'>" + VIS.Msg.getMsg("CreateNew") + "</button>"
                    + "<button class= 'ui-button' id = 'VA009_Close_" + $self.windowNo + "'style = 'float: right; margin-right: 12px;margin-bottom:12px; visibility:hidden;display:none;'>" + VIS.Msg.getMsg("close") + "</button>");

                var b2bDialog = new VIS.ChildDialog();
                $root = b2bDialog.getRoot();
                b2bDialog.setContent($b2b);
                b2bDialog.setTitle(VIS.Msg.getMsg("VA009_BankToBankTransfer"));
                //1052--set height to remove scroll bar
                //b2bDialog.setHeight(window.innerHeight - 220);
                b2bDialog.setWidth("60%");
                b2bDialog.setEnableResize(true);
                b2bDialog.setModal(true);
                b2bDialog.show();
                b2b_getControls($root);
                //1052-- append  note div and button in dialogs button pane
                $btnPanel.append($Note).append($resltbtns);
                $createNew = $root.dialog('widget').find("#VA009_CreateNew_" + $self.windowNo);
                $closeb2b = $root.dialog('widget').find("#VA009_Close_" + $self.windowNo);
                $note = $root.dialog('widget').find("#VA009_Note" + $self.windowNo);

                var now = new Date();
                var _today = now.getFullYear() + "-" + (("0" + (now.getMonth() + 1)).slice(-2)) + "-" + (("0" + now.getDate()).slice(-2));
                $trnsDate.val(_today);
                $acctDate.val(_today);
                InitializeEvents();
                loadOrg();
                loadCurrency();
                loadCurrencyType();
                loadPaymentMthd();
                loadBankAccount($From_cmbBank, organizationids);
                loadToBankAccount($To_cmbBank, organizationids);
                //Rakesh(VA228):Load Target Docuement Type APR and APP
                _loadFunctions.LoadTargetDocTypeB2B($POP_ARTargetDocType, 1, 0);
                _loadFunctions.LoadTargetDocTypeB2B($POP_APTargetDocType, 2, 0);

                txtAmount.addVetoableChangeListener(this);
                b2bDialog.onOkClick = function () {
                    //function reverseFormatNumber(val, locale) {
                    //    var group = new Intl.NumberFormat(locale).format(1111).replace(/1/g, '');
                    //    var decimal = new Intl.NumberFormat(locale).format(1.1).replace(/1/g, '');
                    //    var reversedVal = val.replace(new RegExp('\\' + group, 'g'), '');
                    //    reversedVal = reversedVal.replace(new RegExp('\\' + decimal, 'g'), '.');
                    //    return Number.isNaN(reversedVal) ? 0 : reversedVal;
                    //}
                    //var amount = reverseFormatNumber($txtAmt.val(), window.navigator.language);

                    if (parseInt($OrgCmb.val()) == 0) {
                        $note.text(VIS.Msg.getMsg("VA009_PlsSelectOrg"));
                        return $note.css('visibility', 'visible');
                    }

                    if (!$isReceipt.prop('checked') && !$isPayment.prop('checked')) {
                        $note.text(VIS.Msg.getMsg("VA009_PlsSelectPaymentOrReceipt"));
                        return $note.css('visibility', 'visible');
                    }

                    if (parseInt($From_cmbBank.val()) == 0) {
                        $note.text(VIS.Msg.getMsg("VA009_PlsSelectBankFrom"));
                        return $note.css('visibility', 'visible');
                    }

                    if (parseInt($To_cmbBank.val()) == 0) {
                        $note.text(VIS.Msg.getMsg("VA009_PlsSelectBankTo"));
                        return $note.css('visibility', 'visible');
                    }

                    if (parseInt($POP_APTargetDocType.val()) == 0) {
                        $note.text(VIS.Msg.getMsg("VA009_PlsSelectAPDocType"));
                        return $note.css('visibility', 'visible');
                    }

                    if (parseInt($POP_ARTargetDocType.val()) == 0) {
                        $note.text(VIS.Msg.getMsg("VA009_PlsSelectARDocType"));
                        return $note.css('visibility', 'visible');
                    }

                    if (parseInt($From_cmbBank.val()) == parseInt($To_cmbBank.val())) {
                        $note.text(VIS.Msg.getMsg("VA009_Banknotbesame"));
                        return $note.css('visibility', 'visible');
                    }

                    if (parseFloat(txtAmount.getValue()) == 0) {
                        $note.text(VIS.Msg.getMsg("VA009_PlsEnterAmount"));
                        return $note.css('visibility', 'visible');
                    }

                    if (parseInt($paymentMthd.val()) == 0) {
                        $note.text(VIS.Msg.getMsg("VA009_PlsSelectPayMethod"));
                        return $note.css('visibility', 'visible');
                    }
                    else if ($('option:selected', $paymentMthd).attr('paybase') == "S") {
                        if ($txtCheckNo.val() == "") {
                            $note.text(VIS.Msg.getMsg("VA009_PLCheckNumber"));
                            return $note.css('visibility', 'visible');
                        }

                        if ($checkDate.val() == "" || $checkDate.val() == null) {
                            $note.text(VIS.Msg.getMsg("VA009_PLCheckDate"));
                            return $note.css('visibility', 'visible');
                        }
                        var dt = new Date($checkDate.val());
                        dt = new Date(dt.setHours(0, 0, 0, 0));
                        var acctdate = new Date($acctDate.val());
                        acctdate = new Date(acctdate.setHours(0, 0, 0, 0));
                        if (dt > acctdate) {
                            $note.text(VIS.Msg.getMsg("VIS_CheckDateCantbeGreaterSys"));
                            return $note.css('visibility', 'visible');
                        }
                    }

                    if (parseInt($cmbCurrencies.val()) == 0) {
                        $note.text(VIS.Msg.getMsg("VA009_PlsSelectCurrency"));
                        return $note.css('visibility', 'visible');
                    }

                    var paramString = $From_cmbBank.val() + "," + $cmbCurrencies.val() + "," + $acctDate.val().toString() +
                        "," + $cmbCurrencyType.val() + "," + VIS.Env.getCtx().getAD_Client_ID() + "," + $OrgCmb.val();
                    var dr = VIS.dataContext.getJSONRecord("VA009/Payment/CheckConversionRate", paramString);
                    if (dr <= 0) {
                        $note.text(VIS.Msg.getMsg("VA009_ConversionRateNotFound"));
                        return $note.css('visibility', 'visible');
                    }

                    $note.css('visibility', 'hidden');
                    /** 
                        VA230:If checkno textfield value is not equal to currentNextChequeNo and docbasetype is AP Payment then override autocheck marked as true
                   */
                    isOverrideAutoCheck = false;
                    if (($isPayment.prop('checked') && $txtCheckNo.val() != currentNextChequeNo && currentNextChequeNo != "")) {
                        isOverrideAutoCheck = true;
                    }
                    var _data = {};
                    console.log(parseFloat(txtAmount.getValue()));
                    _data = {
                        OrgID: parseInt($OrgCmb.val()), isReceipt: $isReceipt.prop('checked'), isPayment: $isPayment.prop('checked'), fromBank: parseInt($From_cmbBank.val()),
                        toBank: parseInt($To_cmbBank.val()), amount: parseFloat(txtAmount.getValue()), paymentMethod: parseInt($paymentMthd.val()), currencyID: parseInt($cmbCurrencies.val()),
                        currencyType: $cmbCurrencyType.val(), transDate: VIS.Utility.Util.getValueOfDate($trnsDate.val()), acctDate: VIS.Utility.Util.getValueOfDate($acctDate.val()),
                        clientID: VIS.Env.getCtx().getAD_Client_ID(), PayBase: $('option:selected', $paymentMthd).attr('paybase'), CheckNo: $txtCheckNo.val(), CheckDate: VIS.Utility.Util.getValueOfDate($checkDate.val()),
                        APDocumentTypeId: VIS.Utility.Util.getValueOfInt($POP_APTargetDocType.val()), ARDocumentTypeId: VIS.Utility.Util.getValueOfInt($POP_ARTargetDocType.val()),
                        IsOverrideAutoCheck: isOverrideAutoCheck
                    };
                    MsgReturn = "";
                    b2bPayment(_data);
                    return false;
                };

                b2bDialog.onCancelCLick = function () {
                    B2BDispose();
                };

                b2bDialog.onClose = function () {
                    B2BDispose();
                };

                this.vetoablechange = function (evt) {
                    console.log(evt.propertyName);
                    if (evt.propertyName == "VA009_Amount" + $self.windowNo + "") {
                        txtAmount.setValue(evt.newValue);
                    }
                };

                function InitializeEvents() {

                    $isPayment.on("click", function (e) {
                        var target = $(e.target);
                        if (e.target.type == 'checkbox') {
                            if (target.prop("checked") == true) {
                                $isReceipt.prop('checked', false);
                                loadBankAccount($From_cmbBank, organizationids);
                                loadToBankAccount($To_cmbBank, []);
                            }
                            $txtCheckNo.val("");
                            currentNextChequeNo = "";
                        }
                    });

                    $isReceipt.on("click", function (e) {
                        var target = $(e.target);
                        if (e.target.type == 'checkbox') {
                            if (target.prop("checked") == true) {
                                $isPayment.prop('checked', false);
                                loadToBankAccount($To_cmbBank, organizationids);
                                loadBankAccount($From_cmbBank, []);
                            }
                            $txtCheckNo.val("");
                            currentNextChequeNo = "";
                        }
                    });

                    $OrgCmb.on("change", function () {

                        //#Commented Code because we don't need to chnage the selected Payment or Receipt Checkbox at the time of Org Change

                        //if ($isReceipt.prop('checked') || $isPayment.prop('checked')) {
                        //    $isReceipt.prop('checked', false);
                        //    $isPayment.prop('checked', false);
                        //    organizationids = [];
                        //}

                        //# end Commented code  
                        if (parseInt($OrgCmb.val()) > 0) {
                            organizationids = [];
                            organizationids.push($OrgCmb.val());
                            $OrgCmb.removeClass('vis-ev-col-mandatory');

                            if ($isPayment.prop("checked") == true) {
                                loadBankAccount($From_cmbBank, organizationids);
                                loadToBankAccount($To_cmbBank, []);
                            }
                            if ($isReceipt.prop("checked") == true) {
                                loadToBankAccount($To_cmbBank, organizationids);
                                loadBankAccount($From_cmbBank, []);
                            }
                        }
                        else {
                            $OrgCmb.addClass('vis-ev-col-mandatory');
                        }
                        //Rakesh(VA228):Reset Document Type
                        _loadFunctions.LoadTargetDocTypeB2B($POP_ARTargetDocType, 1, 0);
                        _loadFunctions.LoadTargetDocTypeB2B($POP_APTargetDocType, 2, 0);
                        $txtCheckNo.val("");
                        currentNextChequeNo = "";
                    });

                    $From_cmbBank.on("change", function () {
                        if (parseInt($From_cmbBank.val()) > 0) {
                            $From_cmbBank.removeClass('vis-ev-col-mandatory');
                            //Document Type as per Bank Account's Org
                            _loadFunctions.LoadTargetDocTypeB2B($POP_APTargetDocType, 2, $From_cmbBank.val());

                        }
                        else {
                            $From_cmbBank.addClass('vis-ev-col-mandatory');
                            _loadFunctions.LoadTargetDocTypeB2B($POP_APTargetDocType, 2, 0);
                        }
                        //VA230:Get checkno
                        getCheckNo();
                    });

                    $To_cmbBank.on("change", function () {
                        if (parseInt($To_cmbBank.val()) > 0) {
                            $To_cmbBank.removeClass('vis-ev-col-mandatory');
                            //Document Type as per Bank Account's Org
                            _loadFunctions.LoadTargetDocTypeB2B($POP_ARTargetDocType, 1, $To_cmbBank.val());
                        }
                        else {
                            $To_cmbBank.addClass('vis-ev-col-mandatory');
                            _loadFunctions.LoadTargetDocTypeB2B($POP_ARTargetDocType, 1, 0);

                        }
                    });

                    $cmbCurrencies.on("change", function () {
                        if (parseInt($cmbCurrencies.val()) > 0) {
                            $cmbCurrencies.removeClass('vis-ev-col-mandatory');
                        }
                        else {
                            $cmbCurrencies.addClass('vis-ev-col-mandatory');
                        }
                    });

                    // change the color if the value is change other than 0
                    txtAmount.getControl().on("change", function () {
                        if (parseInt(txtAmount.getValue()) > 0) {
                            txtAmount.getControl().removeClass('vis-ev-col-mandatory');
                        }
                        else {
                            txtAmount.getControl().addClass('vis-ev-col-mandatory');
                        }
                    });
                    $paymentMthd.on("change", function () {
                        if (parseInt($paymentMthd.val()) > 0) {
                            $paymentMthd.removeClass('vis-ev-col-mandatory');
                        }
                        else {
                            $paymentMthd.addClass('vis-ev-col-mandatory');
                        }

                        if ($('option:selected', $paymentMthd).attr('paybase') == "S") {
                            $txtCheckNo.removeAttr("disabled");
                            $txtCheckNo.css('background-color', 'white');
                            $txtCheckNo.addClass('vis-ev-col-mandatory');
                            $checkDate.removeAttr("disabled");
                            $checkDate.css('background-color', 'white');
                            $checkDate.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $txtCheckNo.val("");
                            $txtCheckNo.attr('disabled', 'disabled');
                            $txtCheckNo.removeClass('vis-ev-col-mandatory');
                            $txtCheckNo.css('background-color', '#ededed');
                            $checkDate.val("");
                            $checkDate.attr('disabled', 'disabled');
                            $checkDate.removeClass('vis-ev-col-mandatory');
                            $checkDate.css('background-color', '#ededed');
                            currentNextChequeNo = "";
                        }
                        getCheckNo();
                    });

                    //click event of get check no button
                    $getNextChkno.on("click", function () {

                        if (parseInt($paymentMthd.val()) > 0) {
                            if ($('option:selected', $paymentMthd).attr('paybase') == "S") {
                                getCheckNo();
                            }
                        }
                    });

                    $POP_APTargetDocType.on("change", function () {
                        //to set target doc type mandatory
                        if (VIS.Utility.Util.getValueOfInt($POP_APTargetDocType.val()) == 0) {
                            $POP_APTargetDocType.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_APTargetDocType.removeClass('vis-ev-col-mandatory');
                        }
                    });

                    $POP_ARTargetDocType.on("change", function () {
                        //to set target doc type mandatory
                        if (VIS.Utility.Util.getValueOfInt($POP_ARTargetDocType.val()) == 0) {
                            $POP_ARTargetDocType.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_ARTargetDocType.removeClass('vis-ev-col-mandatory');
                        }
                    });
                };

                //Set Mandatory and Non-Mandatory
                function SetMandatory(value) {
                    if (value)
                        return '#FFB6C1';
                    else
                        return 'White';
                };

                function b2b_getControls($root) {
                    $OrgCmb = $b2b.find("#VA009_POP_cmbOrg_" + $self.windowNo);
                    $From_cmbBank = $b2b.find("#VA009_POP_cmbFromBank_" + $self.windowNo);
                    $To_cmbBank = $b2b.find("#VA009_POP_cmbToBank_" + $self.windowNo);
                    $isPayment = $b2b.find("#VA009_Payment_" + $self.windowNo);
                    $isReceipt = $b2b.find("#VA009_Receipt_" + $self.windowNo);
                    $cmbCurrencyType = $b2b.find("#VA009_cmbCurrencyType_" + $self.windowNo);
                    $paymentMthd = $b2b.find("#VA009_cmbPayMthd_" + $self.windowNo);
                    $cmbCurrencies = $b2b.find("#VA009_POP_cmbCurrency_" + $self.windowNo);
                    $txtAmt = $b2b.find("#VA009_Amount" + $self.windowNo);
                    $trnsDate = $b2b.find("#VA009_TransactionDate" + $self.windowNo);
                    $acctDate = $b2b.find("#VA009_AccountDate" + $self.windowNo);
                    $txtCheckNo = $b2b.find("#VA009_Chqnotxt_" + $self.windowNo);
                    $checkDate = $b2b.find("#VA009_CheckDate_" + $self.windowNo);
                    //$note = $b2b.find("#VA009_Note" + $self.windowNo);
                    $getNextChkno = $b2b.find("#VA009_getCheckNo_" + $self.windowNo);
                    //(1052): get button div
                    $btnPanel = $root.dialog('widget').find('.ui-dialog-buttonpane');
                    $successNote = $resultb2b.find("#VA009_Note" + $self.windowNo);
                    $OrgCmb.addClass('vis-ev-col-mandatory');
                    $From_cmbBank.addClass('vis-ev-col-mandatory');
                    $To_cmbBank.addClass('vis-ev-col-mandatory');
                    txtAmount.getControl().addClass('vis-ev-col-mandatory');//for default color to control
                    $cmbCurrencies.addClass('vis-ev-col-mandatory');
                    $paymentMthd.addClass('vis-ev-col-mandatory');
                    //Rakesh(VA228):Store Doc type element in variable
                    $POP_APTargetDocType = $b2b.find("#VA009_POP_cmbAPDocType_" + $self.windowNo);
                    $POP_APTargetDocType.addClass('vis-ev-col-mandatory');
                    $POP_ARTargetDocType = $b2b.find("#VA009_POP_cmbARDocType_" + $self.windowNo);
                    $POP_ARTargetDocType.addClass('vis-ev-col-mandatory');
                };

                $txtCheckNo.on('keydown keyup', function (e) {
                    var value = String.fromCharCode(e.which) || e.key;
                    var regExp = /[0-9\.\,]/;
                    // Only numbers, alphabets
                    if (!regExp.test(value)
                        && e.which != 8   // backspace
                        && e.which != 46  // delete
                        && (e.which < 65 || e.which > 90)
                        && (e.which < 37 || e.which > 40)
                        && e.which != 39 && e.which != 37  //arrow left and right
                        && (e.which < 96 || e.which > 105))// for NumPad
                    {
                        return false;
                    }
                    if (e.shiftKey) {
                        e.preventDefault();
                    }
                });
                function loadOrg() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadOrganization", null, callbackloadorg);
                    function callbackloadorg(dr) {
                        $OrgCmb.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $OrgCmb.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].AD_Org_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                            }
                        }
                        $OrgCmb.prop('selectedIndex', 0);
                    };
                };

                function loadPaymentMthd() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadPaymentMethod", null, callbackloadpaymthds);
                    function callbackloadpaymthds(dr) {
                        $paymentMthd.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                if (("LCPB").contains(dr[i].VA009_PaymentBaseType)) {
                                }
                                else {
                                    $paymentMthd.append("<option paybase = " + dr[i].VA009_PaymentBaseType + " value=" + VIS.Utility.Util.getValueOfInt(dr[i].VA009_PaymentMethod_ID) + ">"
                                        + VIS.Utility.encodeText(dr[i].VA009_Name) + " </option>");
                                }
                            }
                        }
                        $paymentMthd.prop('selectedIndex', 0);
                    }
                };

                function loadCurrencyType() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/loadCurrencyType", null, callbackCurrencyType);

                    function callbackCurrencyType(dr) {
                        $cmbCurrencyType.append("<option value='0'></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $cmbCurrencyType.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                                if (VIS.Utility.encodeText(dr[i].IsDefault) == "Y") {
                                    defaultCurrenyType = VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID);
                                }
                            }
                            //$cmbCurrencyType.prop('selectedIndex', 1);
                            $cmbCurrencyType.val(defaultCurrenyType)
                        }
                    }
                };

                function loadCurrency() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/loadCurrencies", null, callbackCurrencyType);

                    function callbackCurrencyType(dr) {
                        $cmbCurrencies.append("<option value='0'></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $cmbCurrencies.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_Currency_ID) + ">" + VIS.Utility.encodeText(dr[i].ISO_Code) + "</option>");
                            }
                            $cmbCurrencies.prop('selectedIndex', 0);
                        }
                    }
                };

                function loadBankAccount($cmbBankAccount, organizationids) {

                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadBankAccount", { "Bank_ID": 0, "Orgs": organizationids.toString() }, callbackloadbankAcct);

                    function callbackloadbankAcct(dr) {
                        $cmbBankAccount.find('option').remove();
                        $cmbBankAccount.append("<option value='0'></option>");
                        $cmbBankAccount.addClass('vis-ev-col-mandatory');
                        if (dr != null) {
                            if (dr.length > 0) {
                                for (var i in dr) {
                                    $cmbBankAccount.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_BankAccount_ID) + ">" + VIS.Utility.encodeText(dr[i].AccountNo) + "</option>");
                                }
                            }
                        }
                    }
                };

                function loadToBankAccount($cmbToBankAccount, organizationids) {

                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadBankAccount", { "Bank_ID": 0, "Orgs": organizationids.toString() }, callbackloadbankAcct);

                    function callbackloadbankAcct(dr) {
                        $cmbToBankAccount.find('option').remove();
                        $cmbToBankAccount.append("<option value='0'></option>");
                        $cmbToBankAccount.addClass('vis-ev-col-mandatory');
                        if (dr != null) {
                            if (dr.length > 0) {
                                for (var i in dr) {
                                    $cmbToBankAccount.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_BankAccount_ID) + ">" + VIS.Utility.encodeText(dr[i].AccountNo) + "</option>");
                                }
                            }
                        }
                    }
                };
                var currentNextChequeNo = "";
                /**VA230:To get current Next Cheque Number based seleted bank, payment method document base type is AP Payment */
                function getCheckNo() {
                    if (VIS.Utility.Util.getValueOfInt($From_cmbBank.val()) > 0 && VIS.Utility.Util.getValueOfInt($paymentMthd.val()) > 0 && $('option:selected', $paymentMthd).attr('paybase') == "S") {

                        VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/getCheckNo", { "C_BankAccount_ID": parseInt($From_cmbBank.val()), "VA009_PaymentMethod_ID": parseInt($paymentMthd.val()) }, callbackgetCheckNo);

                        function callbackgetCheckNo(dr) {
                            $txtCheckNo.val();
                            $note.css('visibility', 'hidden');
                            if (dr != null) {
                                $txtCheckNo.val(dr);
                                $txtCheckNo.removeClass('vis-ev-col-mandatory');
                                //store current next cheque number and compare it before payment creation
                                currentNextChequeNo = dr;
                            }
                        }
                    } else {
                        $txtCheckNo.val("");
                        currentNextChequeNo = "";
                    }
                };

                //reset for Amount Field...
                this.vetoablechange = function (evt) {
                    if (evt.propertyName == "VA009_Amount" + $self.windowNo + "") {
                        txtAmount.setValue(evt.newValue);
                    }
                };

                function B2BDispose() {
                    $b2b = null;
                    _b2b = null;
                }

                /**
                 * Create Payment and if successfully created then overwrite dialog with new content
                 * @param {any} _data
                 * @param {any} b2bDialog
                 */
                function b2bPayment(_data) {
                    $bsyDiv[0].style.visibility = "visible";
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GeneratePaymentsBtoB",
                        type: "POST",
                        datatype: "json",
                        contentType: 'application/json',
                        //async: false,
                        data: JSON.stringify({ recordData: JSON.stringify(_data) }),
                        success: function (result) {
                            MsgReturn = JSON.parse(result);
                            if (MsgReturn.success != null) {
                                $successNote.text(MsgReturn.success);
                                $successNote.css('visibility', 'visible');
                                b2bDialog.setContent($resultb2b);
                                //  b2bDialog.setHeight(window.innerHeight - 385);
                                b2bDialog.setWidth("32%");
                                b2bDialog.show();
                                b2bDialog.hidebuttons();
                                $createNew.css({ 'visibility': 'visible', 'display': 'block' });
                                $closeb2b.css({ 'visibility': 'visible', 'display': 'block' });
                                $note.css('display', 'none');
                                b2bEventIntialization();
                            }
                            else if (MsgReturn.error != null) {
                                $note.text(MsgReturn.error);
                                $note.css('visibility', 'visible');
                                $bsyDiv[0].style.visibility = "hidden";
                            }
                            else {
                                VIS.ADialog.info("VA009_SavedSuccessfully");
                            }
                            $bsyDiv[0].style.visibility = "hidden";
                        },
                        error: function (ex) {
                            MsgReturn = ex;
                            $note.text(MsgReturn);
                            $note.css('visibility', 'visible');
                            $bsyDiv[0].style.visibility = "hidden";
                        }
                    });
                };

                /**
                 * Intialize events after successfull ajax call                 
                 */
                function b2bEventIntialization() {

                    $createNew.on("click", function () {
                        InitializeEvents();
                        //clear the controls and overwrite the dialog with old content.
                        $OrgCmb.val(0);
                        $isReceipt.prop('checked', false);
                        $isPayment.prop('checked', false);
                        $From_cmbBank.val(0);
                        $To_cmbBank.val(0);
                        txtAmount.setValue(0);
                        $paymentMthd.val(0);
                        $cmbCurrencies.val(0);
                        $cmbCurrencyType.val(defaultCurrenyType);
                        $trnsDate.val(_today);
                        $acctDate.val(_today);
                        $txtCheckNo.val(0);
                        $checkDate.val("");
                        $POP_APTargetDocType.val("");
                        $POP_ARTargetDocType.val("");

                        b2bDialog.setContent($b2b);
                        //b2bDialog.setHeight(window.innerHeight - 180);
                        b2bDialog.setWidth("60%");
                        b2bDialog.setEnableResize(true);
                        b2bDialog.setModal(true);
                        b2bDialog.show();
                        $note.css('display', 'block');
                        $createNew.css({ 'visibility': 'hidden', 'display': 'none' });
                        $closeb2b.css({ 'visibility': 'hidden', 'display': 'none' });
                        $root.dialog('widget').find('.ui-dialog-buttonpane').css('padding', '');
                        $root.dialog('widget').find('.ui-dialog-buttonpane').css('margin-top', '');
                    });


                    $closeb2b.on("click", function () {
                        //close the dialog
                        b2bDialog.close();
                    });
                }

            },

            Pay_ManualDialog: function () {
                $payManual = $("<div class='VA009-popform-content vis-formouterwrpdiv' style='min-height:210px !important'>");
                var _payManual = "";
                _payManual += "<div class='VA009-popfrm-wrap' style='height:auto !important'>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbOrg_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_Org") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbDocType_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_DocType") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbBank_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_Bank") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbBankAccount_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_BankAccount") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_cmbPayMthd_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_PayMethodlbl") + "</label>"
                    + "</div ></div > "

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_AccountDate_" + $self.windowNo + "' placeholder=' ' data-placeholder=''><label>" + VIS.Msg.getMsg("AccountDate") + "</label>"
                    + " </div></div>"
                    //currency type list
                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_cmbCurrencyType_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_CurrencyType") + "</label>"
                    + "</div></div>"
                    //trx date
                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_TransactionDate" + $self.windowNo + "'  placeholder=' ' data-placeholder=''><label>" + VIS.Msg.getMsg("TransactionDate") + "</label>"
                    + " </div></div>"

                    + "</div>";
                $payManual.append(_payManual);
                payManualGetControls();

                var now = new Date();
                var _today = now.getFullYear() + "-" + (("0" + (now.getMonth() + 1)).slice(-2)) + "-" + (("0" + now.getDate()).slice(-2));
                $POP_DateAcct.val(_today);
                $POP_DateTrx.val(_today);
                var manualDialog = new VIS.ChildDialog();
                manualDialog.setContent($payManual);
                manualDialog.setTitle(VIS.Msg.getMsg("VA009_PayMannual"));
                manualDialog.setWidth("65%");
                manualDialog.setEnableResize(true);
                manualDialog.setModal(true);
                manualDialog.show();
                loadOrg();
                //populate banks based on selected organization in dialog
                loadbanks($POP_cmbBank, VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()));
                loadPayMthd();
                loadCurrencyType();
                //Rakesh(VA228):10/Sep/2021 -> Load APR target base doc type
                _loadFunctions.LoadTargetDocType($POP_targetDocType, _TargetBaseType);

                function payManualGetControls() {
                    $POP_cmbBank = $payManual.find("#VA009_POP_cmbBank_" + $self.windowNo);
                    $POP_cmbBankAccount = $payManual.find("#VA009_POP_cmbBankAccount_" + $self.windowNo);
                    $POP_PayMthd = $payManual.find("#VA009_cmbPayMthd_" + $self.windowNo);
                    $POP_DateAcct = $payManual.find("#VA009_AccountDate_" + $self.windowNo);
                    $POP_CurrencyType = $payManual.find("#VA009_cmbCurrencyType_" + $self.windowNo);
                    $POP_DateTrx = $payManual.find("#VA009_TransactionDate" + $self.windowNo);
                    $POP_cmbOrg = $payManual.find("#VA009_POP_cmbOrg_" + $self.windowNo);
                    $POP_cmbOrg.addClass('vis-ev-col-mandatory');
                    $POP_PayMthd.addClass('vis-ev-col-mandatory');
                    $POP_CurrencyType.addClass('vis-ev-col-mandatory');
                    $POP_cmbBank.addClass('vis-ev-col-mandatory');
                    $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                    //Rakesh(VA228):Store Doc type element in variable
                    $POP_targetDocType = $payManual.find("#VA009_POP_cmbDocType_" + $self.windowNo);
                    $POP_targetDocType.addClass('vis-ev-col-mandatory');
                };

                $POP_cmbOrg.on("change", function () {
                    //to set org mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) == 0) {
                        $POP_cmbOrg.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        //populate banks based on selected organization in dialog
                        loadbanks($POP_cmbBank, VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()));
                        //refresh the bank and BankAccount dropdowns and make it as mandatory
                        $POP_cmbBank.val(0).prop('selected', true);
                        $POP_cmbBank.addClass('vis-ev-col-mandatory');
                        $POP_cmbBankAccount.empty();
                        $POP_cmbBankAccount.append("<option value='0'></option>");
                        $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                        $POP_cmbOrg.removeClass('vis-ev-col-mandatory');
                    }
                    //Rakesh(VA228):Reset Document type
                    _loadFunctions.LoadTargetDocType($POP_targetDocType, _TargetBaseType);
                });

                $POP_cmbBank.on("change", function () {
                    $POP_cmbBankAccount.empty();
                    $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                    //to set org mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBank.val()) == 0) {
                        $POP_cmbBank.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_cmbBank.removeClass('vis-ev-col-mandatory');
                    }
                    //to get bank account of selected organization assigned by Ashish on 28 May 2020
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadBankAccount", { "Bank_ID": $POP_cmbBank.val(), "Orgs": $POP_cmbOrg.val() }, callbackloadbankAcct);

                    function callbackloadbankAcct(dr) {
                        $POP_cmbBankAccount.append("<option value='0'></option>");
                        if (dr != null) {
                            if (dr.length > 0) {
                                for (var i in dr) {
                                    $POP_cmbBankAccount.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_BankAccount_ID) + ">" + VIS.Utility.encodeText(dr[i].AccountNo) + "</option>");
                                }
                            }
                        }
                    }
                });

                $POP_CurrencyType.on("change", function () {
                    //to set org mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_CurrencyType.val()) == 0) {
                        $POP_CurrencyType.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_CurrencyType.removeClass('vis-ev-col-mandatory');
                    }
                });

                $POP_PayMthd.on("change", function () {
                    //to set PAYMENT MENNDATORY mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()) == 0) {
                        $POP_PayMthd.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_PayMthd.removeClass('vis-ev-col-mandatory');
                    }
                });

                $POP_DateAcct.on("change", function () {
                    //to set Date acct mandatory given by ashish on 28 May 2020
                    if ($POP_DateAcct.val() == "") {
                        $POP_DateAcct.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_DateAcct.removeClass('vis-ev-col-mandatory');
                    }
                });

                $POP_cmbBankAccount.on("change", function () {
                    //to set bank account mandatory given by ashish on 28 May 2020
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()) == 0) {
                        $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_cmbBankAccount.removeClass('vis-ev-col-mandatory');
                    }
                });

                $POP_targetDocType.on("change", function () {
                    //to set target doc type mandatory
                    if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) == 0) {
                        $POP_targetDocType.addClass('vis-ev-col-mandatory');
                    }
                    else {
                        $POP_targetDocType.removeClass('vis-ev-col-mandatory');
                    }
                });

                //to load all organization 
                function loadOrg() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadOrganization", null, callbackloadorg);
                    function callbackloadorg(dr) {
                        $POP_cmbOrg.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_cmbOrg.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].AD_Org_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                            }
                        }
                        $POP_cmbOrg.prop('selectedIndex', 0);
                    };
                };

                function loadPayMthd() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadPaymentMethod", null, callbackloadpaymthds);
                    function callbackloadpaymthds(dr) {
                        if (dr.length > 1)
                            $POP_PayMthd.append(" <option value = 0></option>");

                        if (dr.length > 0) {
                            for (var i in dr) {
                                if (("LCPBS").contains(dr[i].VA009_PaymentBaseType)) {
                                }
                                else
                                    $POP_PayMthd.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].VA009_PaymentMethod_ID) + ">" + VIS.Utility.encodeText(dr[i].VA009_Name) + "</option>");
                            }
                        }
                        $POP_PayMthd.prop('selectedIndex', 0);
                    }
                };
                //load currency type
                function loadCurrencyType() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/loadCurrencyType", null, callbackCurrencyType);

                    function callbackCurrencyType(dr) {
                        $POP_CurrencyType.append("<option value='0'></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_CurrencyType.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                                if (VIS.Utility.encodeText(dr[i].IsDefault) == "Y") {
                                    defaultCurrenyType = VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID);
                                }
                            }
                        }
                    }
                };

                function SetMandatory(value) {
                    if (value)
                        return '#FFB6C1';
                    else
                        return 'White';
                };

                manualDialog.onOkClick = function () {
                    if (SlctdPaymentIds.length > 0 || SlctdOrderPaymentIds.length > 0 || SlctdJournalPaymentIds.length > 0) {
                        if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) > 0) {
                            //Rakesh(VA228):Make Document Type selection mandatory
                            if (VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()) > 0) {
                                if (VIS.Utility.Util.getValueOfInt($POP_cmbBank.val()) > 0) {
                                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()) > 0) {
                                        if (VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()) > 0) {
                                            if ($POP_DateAcct.val() != "" && $POP_DateAcct.val() != null) {
                                                //Check CurrencyType Selected or not
                                                if ($POP_CurrencyType.val() > 0) {
                                                    //var isValid = validateManualPayment(); //commented by Manjot suggested by Pradeep and Puneet
                                                    var isValid = true;
                                                    if (isValid) {
                                                        $bsyDiv[0].style.visibility = "visible";
                                                        $.ajax({
                                                            url: VIS.Application.contextUrl + "VA009/Payment/GeneratePaymentsMannualy",
                                                            type: "POST",
                                                            datatype: "json",
                                                            // contentType: "application/json; charset=utf-8",
                                                            async: true,
                                                            data: ({
                                                                InvoiceSchdIDS: SlctdPaymentIds.toString(), OrderSchdIDS: SlctdOrderPaymentIds.toString(), BankID: VIS.Utility.Util.getValueOfInt($POP_cmbBank.val()),
                                                                BankAccountID: VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()), PaymentMethodID: VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()),
                                                                DateAcct: $POP_DateAcct.val(), CurrencyType: $POP_CurrencyType.val(), DateTrx: $POP_DateTrx.val(), AD_Org_ID: VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val())
                                                                , docTypeID: VIS.Utility.Util.getValueOfInt($POP_targetDocType.val()),
                                                                JournalSchdIDS: SlctdJournalPaymentIds.toString()
                                                            }),
                                                            success: function (result) {
                                                                result = JSON.parse(result);
                                                                $divPayment.find('.VA009-payment-wrap').remove();
                                                                $divBank.find('.VA009-right-data-main').remove();
                                                                $divBank.find('.VA009-accordion').remove();
                                                                pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; batchObjInv = []; batchObjOrd = []; SlctdJournalPaymentIds = [];
                                                                resetPaging();
                                                                //after successfully created Payment selectall checkbox should be false
                                                                $selectall.prop('checked', false);
                                                                //loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
                                                                loadPaymetsAll();
                                                                clearamtid();
                                                                $bsyDiv[0].style.visibility = "hidden";
                                                                VIS.ADialog.info("", null, result, "");
                                                            },
                                                            error: function (ex) {
                                                                console.log(ex);
                                                                $bsyDiv[0].style.visibility = "hidden";
                                                                VIS.ADialog.error("VA009_ErrorLoadingPayments");
                                                            }
                                                        });
                                                    }
                                                    else {
                                                        VIS.ADialog.info("VA009_PlzSelctapprPM");
                                                        return false;
                                                    }
                                                }
                                                else {
                                                    VIS.ADialog.info("VA009_SelectConversionType");
                                                    return false;
                                                }
                                            }
                                            else {
                                                VIS.ADialog.info(("VA009_PLSelectAcctDate"));
                                                return false;
                                            }
                                        }
                                        else {
                                            VIS.ADialog.info("VA009_PLSelectPaymentMethod");
                                            return false;
                                        }
                                    }
                                    else {
                                        VIS.ADialog.info("VA009_PLSelectBankAccount");
                                        return false;
                                    }
                                }
                                else {
                                    VIS.ADialog.info("VA009_PLSelectBank");
                                    return false;
                                }
                            } else {
                                VIS.ADialog.info(("VA009_PlsSelectDocumentType"));
                                Cashgrd.selectNone();
                                return false;
                            }
                        }
                        else {
                            VIS.ADialog.info(("VA009_PlsSelectOrg"));
                            return false;
                        }
                    }
                    else
                        VIS.ADialog.info("VA009_PlzSelct1Pay");
                };

                manualDialog.onCancelCLick = function () {
                    payManualDispose();
                };

                manualDialog.onClose = function () {
                    payManualDispose();

                };

                function payManualDispose() {
                    $payManual = null;
                    _payManual = null;
                };

            },

            Pay_ManualDialogBP: function () {
                var payAmount;
                lblAmount = $("<label>");
                payAmount = new VIS.Controls.VAmountTextBox("VA009_cmbAmt_" + $self.windowNo + "", false, false, true, 50, 100, VIS.DisplayType.Amount, VIS.Msg.getMsg("Amount"));//for amount textbox control
                lblAmount.append(VIS.Msg.getMsg("Amount"));
                payAmount.setValue(0);
                format = VIS.DisplayType.GetNumberFormat(VIS.DisplayType.Amount);//formating the value of amount into number
                divAmount = $("<div class='VA009-popform-data input-group vis-input-wrap'>");
                var $DivAmtCtrlWrp = $("<div class='vis-control-wrap'>");
                divAmount.append($DivAmtCtrlWrp);
                $DivAmtCtrlWrp.append(payAmount.getControl().attr('placeholder', ' ').attr('data-placeholder', '')).append(lblAmount);
                $payManual = $("<div class='VA009-popform-content vis-formouterwrpdiv' style='min-height:435px'>");
                //var _payManual = "";
                _payManual = $("<div class='VA009-popfrm-wrap' style='height:auto !important'>");

                _addOrg = $("<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbOrg_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_Org") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbDocType_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_DocType") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbBank_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_Bank") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbBankAccount_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_BankAccount") + "</label>"
                    + "</div></div>"

                    //trx date
                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_TransactionDate_" + $self.windowNo + "' placeholder=' ' data-placeholder=''> "
                    + "<label>" + VIS.Msg.getMsg("TransactionDate") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<input type='date' max='9999-12-31' id='VA009_AccountDate_" + $self.windowNo + "' placeholder=' ' data-placeholder=''>"
                    + "<label>" + VIS.Msg.getMsg("AccountDate") + "</label>"
                    + "</div> </div>"


                    + "<div class='VA009-popform-data input-group vis-input-wrap' id='VA009_POP_cmbBP_" + $self.windowNo + "'>"
                    //+ "<label>" + VIS.Msg.getMsg("VA009_BPartner") + "</label>"
                    //+ "<div></div>"
                    + "</div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbLocation_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("BPLocation") + "</label>"
                    + "</div></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_POP_cmbCurrency_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_Currency") + "</label>"
                    + "</div></div>"

                    //currency type list
                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_cmbCurrencyType_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_CurrencyType") + "</label>"
                    + "</div></div>"

                    //+ "<div class='VA009-popform-data'>"
                    //+ "<label>" + VIS.Msg.getMsg("Charge") + "</label>"
                    //+ "<select id='VA009_cmbCharge_" + $self.windowNo + "'>"
                    //+ "</select></div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap' id='VA009_Charge_" + $self.windowNo + "'>"
                    //+ "<label>" + VIS.Msg.getMsg("Charge") + "</label>"
                    + "</div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap'><div class='vis-control-wrap'>"
                    + "<select id='VA009_cmbPayMthd_" + $self.windowNo + "'>"
                    + "</select>"
                    + "<label>" + VIS.Msg.getMsg("VA009_PayMethodlbl") + "</label>"
                    + "</div></div>");

                //      +"<div class='VA009-popform-data'>"
                //      + "<label>" + VIS.Msg.getMsg("Amount") + "</label>"
                //      + "<input type='text' id='VA009_cmbAmt_" + $self.windowNo + "' style='height: 30px;border-radius: 0;'> </div>"

                _addAmt = ("<div class='VA009-popform-data input-group vis-input-wrap' id= VA009_DivCheck_" + $self.windowNo + "><div class='vis-control-wrap'>"
                    + "<input type='text' id='VA009_txtCheck_" + $self.windowNo + "' placeholder=' ' data-placeholder=''>"
                    + "<label>" + VIS.Msg.getMsg("VA009_ChequeNo") + "</label>"
                    + "</div> </div>"

                    + "<div class='VA009-popform-data input-group vis-input-wrap' id= VA009_DivCheckDate_" + $self.windowNo + "><div class='vis-control-wrap'>"
                    + "<input type='Date' max='9999-12-31' id='VA009_chkDate_" + $self.windowNo + "' placeholder=' ' data-placeholder=''>"
                    + "<label>" + VIS.Msg.getMsg("VA009_CheckDate") + "</label>"
                    + "</div> </div>"
                    + "</div>"
                    + "</div>");
                _payManual.append(_addOrg).append(divAmount).append(_addAmt);//append the child tags to parent div
                $payManual.append(_payManual);
                payManualGetControls();
                var manualDialog = new VIS.ChildDialog();
                manualDialog.setContent($payManual);
                manualDialog.setTitle(VIS.Msg.getMsg("VA009_PayMannual"));
                // manualDialog.setHeight(window.innerHeight - 120);
                manualDialog.setWidth("65%");
                manualDialog.setEnableResize(true);
                manualDialog.setModal(true);
                manualDialog.show();
                InitializeEvents()
                loadAllData();
                payAmount.addVetoableChangeListener(this);
                function SetMandatory(value) {
                    if (value)
                        return '#FFB6C1';
                    else
                        return 'White';
                };
                function payManualGetControls() {
                    $POP_cmbOrg = $payManual.find("#VA009_POP_cmbOrg_" + $self.windowNo);
                    $POP_cmbDocType = $payManual.find("#VA009_POP_cmbDocType_" + $self.windowNo);
                    $POP_DateTrx = $payManual.find("#VA009_TransactionDate_" + $self.windowNo);
                    $POP_DateAcct = $payManual.find("#VA009_AccountDate_" + $self.windowNo);
                    $POP_cmbBank = $payManual.find("#VA009_POP_cmbBank_" + $self.windowNo);
                    $POP_cmbBankAccount = $payManual.find("#VA009_POP_cmbBankAccount_" + $self.windowNo);
                    $POP_cmbBP = $payManual.find("#VA009_POP_cmbBP_" + $self.windowNo);
                    $POP_lookCharge = $payManual.find("#VA009_Charge_" + $self.windowNo);
                    //BPartner look Up
                    _BusinessPartnerLookUp = VIS.MLookupFactory.getMLookUp(VIS.Env.getCtx(), $self.windowNo, 3499, VIS.DisplayType.Search);
                    $BpartnerControl = new VIS.Controls.VTextBoxButton("C_BPartner_ID", true, false, true, VIS.DisplayType.Search, _BusinessPartnerLookUp);
                    var $POP_cmbBPCtrlwrp = $('<div class="vis-control-wrap">');
                    var $POP_cmbBPBtnwrp = $('<div class="input-group-append">');
                    $POP_cmbBP.append($POP_cmbBPCtrlwrp);
                    $POP_cmbBP.append($POP_cmbBPBtnwrp);
                    $POP_cmbBPCtrlwrp.append($BpartnerControl.getControl().attr('placeholder', ' ').attr('data-placeholder', '').attr('data-hasbtn', ' ')).append("<label>" + VIS.Msg.getMsg("VA009_BPartner") + "</label>");
                    $POP_cmbBPBtnwrp.append($BpartnerControl.getBtn(0));
                    $BpartnerControl.getControl().addClass('vis-ev-col-mandatory');
                    //end
                    //Charge look Up
                    //VIS.MLookupFactory.get(VIS.Env.getCtx(), $self.windowNo, 3787, VIS.DisplayType.TableDir, "C_Charge_ID", 0, false, null);
                    //_charge = new VIS.Controls.VComboBox("C_Charge_ID", true, false, true, lookup, 50);

                    _ChargeLookUp = VIS.MLookupFactory.getMLookUp(VIS.Env.getCtx(), $self.windowNo, 3787, VIS.DisplayType.Search);
                    $ChargeControl = new VIS.Controls.VTextBoxButton("C_Charge_ID", true, false, true, VIS.DisplayType.Search, _ChargeLookUp);
                    var $pop_lookchargeCtrlwrp = $('<div class="vis-control-wrap">');
                    var $pop_lookchargeBtnwrp = $('<div class="input-group-append">');
                    $POP_lookCharge.append($pop_lookchargeCtrlwrp);
                    $POP_lookCharge.append($pop_lookchargeBtnwrp);
                    $pop_lookchargeCtrlwrp.append($ChargeControl.getControl().attr('placeholder', ' ').attr('data-placeholder', '').attr('data-hasbtn', ' ')).append("<label>" + VIS.Msg.getMsg("Charge") + "</label>");
                    $pop_lookchargeBtnwrp.append($ChargeControl.getBtn(0));
                    //end

                    $POP_cmbLocation = $payManual.find("#VA009_POP_cmbLocation_" + $self.windowNo);
                    $POP_Currency = $payManual.find("#VA009_POP_cmbCurrency_" + $self.windowNo);
                    $POP_CurrencyType = $payManual.find("#VA009_cmbCurrencyType_" + $self.windowNo);
                    //$POP_Charge = $payManual.find("#VA009_cmbCharge_" + $self.windowNo);
                    $POP_PayMthd = $payManual.find("#VA009_cmbPayMthd_" + $self.windowNo);
                    //$POP_Amt = $payManual.find("#VA009_cmbAmt_" + $self.windowNo);
                    $POP_ChkNo = $payManual.find("#VA009_txtCheck_" + $self.windowNo);
                    $POP_ChkDate = $payManual.find("#VA009_chkDate_" + $self.windowNo);
                    $DivChkNo = $payManual.find("#VA009_DivCheck_" + $self.windowNo);
                    $DivChkDate = $payManual.find("#VA009_DivCheckDate_" + $self.windowNo);
                    $POP_DateAcct.addClass('vis-ev-col-mandatory');
                    $POP_DateTrx.addClass('vis-ev-col-mandatory');
                    $POP_cmbOrg.addClass('vis-ev-col-mandatory');
                    $POP_cmbDocType.addClass('vis-ev-col-mandatory');
                    $POP_cmbBank.addClass('vis-ev-col-mandatory');
                    $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                    $POP_Currency.addClass('vis-ev-col-mandatory');
                    $POP_PayMthd.addClass('vis-ev-col-mandatory');
                    payAmount.getControl().addClass('vis-ev-col-mandatory');//for default background color to control
                    $POP_CurrencyType.addClass('vis-ev-col-mandatory');
                    $POP_cmbLocation.addClass('vis-ev-col-mandatory');
                };

                // for restricting the special keys from the user
                $POP_ChkNo.on('keydown keyup', function (e) {
                    var value = String.fromCharCode(e.which) || e.key;
                    var regExp = /[0-9\.\,]/;
                    // Only numbers, alphabets
                    if (!regExp.test(value)
                        && e.which != 8   // backspace
                        && e.which != 46  // delete
                        && (e.which < 65 || e.which > 90)
                        && (e.which < 37 || e.which > 40)
                        && e.which != 39 && e.which != 37  //arrow left and right
                        && (e.which < 96 || e.which > 105))// for NumPad
                    {
                        return false;
                    }
                    if (e.shiftKey) {
                        e.preventDefault();
                    }
                });
                function InitializeEvents() {
                    $POP_cmbBank.on("change", function () {
                        $POP_cmbBankAccount.empty();
                        //to set bank account mandatory given by ashish on 28 May 2020
                        if (VIS.Utility.Util.getValueOfInt($POP_cmbBank.val()) == 0) {
                            $POP_cmbBank.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_cmbBank.removeClass('vis-ev-col-mandatory');
                        }
                        //to get bank account of selected organization assigned by Ashish on 28 May 2020
                        VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadBankAccount", { "Bank_ID": $POP_cmbBank.val(), "Orgs": $POP_cmbOrg.val() }, callbackloadbankAcct);

                        function callbackloadbankAcct(dr) {
                            $POP_cmbBankAccount.empty();
                            $POP_cmbBankAccount.append("<option value='0'></option>");
                            if (dr != null) {
                                if (dr.length > 0) {
                                    for (var i in dr) {
                                        $POP_cmbBankAccount.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_BankAccount_ID) + ">" + VIS.Utility.encodeText(dr[i].AccountNo) + "</option>");
                                    }
                                }
                            }
                        }
                    });
                    $POP_cmbBankAccount.on("change", function () {
                        //to set bank account mandatory given by ashish on 28 May 2020
                        if (VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()) == 0) {
                            $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_cmbBankAccount.removeClass('vis-ev-col-mandatory');
                        }
                        getCheckNo();
                    });
                    $POP_cmbDocType.on("change", function () {
                        if (VIS.Utility.Util.getValueOfInt($POP_cmbDocType.val()) == 0) {
                            $POP_cmbDocType.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_cmbDocType.removeClass('vis-ev-col-mandatory');
                        }
                        getCheckNo();
                    });
                    $POP_DateTrx.on("change", function () {
                        //Devops Task Id - 1634
                        //VIS317 Handled Account Date should be fill Automatically same as Enter Transaction date.
                        //Can Enter Account date Manually Also.
                        if ($POP_DateTrx.val() == "") {
                            $POP_DateTrx.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_DateTrx.removeClass('vis-ev-col-mandatory');
                            $POP_DateAcct.val($POP_DateTrx.val());
                            $POP_DateAcct.removeClass('vis-ev-col-mandatory');
                        }

                    });
                    $POP_DateAcct.on("change", function () {
                        if ($POP_DateAcct.val() == "") {
                            $POP_DateAcct.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_DateAcct.removeClass('vis-ev-col-mandatory');
                        }
                    });
                    $POP_cmbOrg.on("change", function () {
                        $POP_cmbBank.empty();
                        $POP_cmbDocType.empty();
                        $POP_cmbBankAccount.empty();
                        $POP_cmbLocation.empty();
                        $POP_cmbDocType.addClass('vis-ev-col-mandatory');
                        $POP_cmbBank.addClass('vis-ev-col-mandatory');
                        $POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                        // $POP_Charge.empty();
                        $POP_PayMthd.empty();
                        //change by Amit
                        if ($POP_cmbOrg.val() == "0") {
                            $POP_cmbOrg.addClass('vis-ev-col-mandatory');
                            //$POP_cmbBankAccount.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_cmbOrg.removeClass('vis-ev-col-mandatory');
                        }
                        loadDocType();
                        loadbanks($POP_cmbBank);
                        //loadBP();
                        //loadCharge();
                        loadCurrencyType();
                        loadCurrencies();
                        loadPaymentMthd();
                    });
                    $BpartnerControl.fireValueChanged = function () {
                        if ($BpartnerControl.value != null) {
                            $BpartnerControl.getControl().removeClass('vis-ev-col-mandatory');
                        }
                        loadLocation();
                    };
                    $POP_cmbLocation.on("change", function () {
                        if ($POP_cmbLocation.val() == "0") {
                            $POP_cmbLocation.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_cmbLocation.removeClass('vis-ev-col-mandatory');
                        }
                    });
                    $POP_Currency.on("change", function () {
                        if ($POP_Currency.val() == "0") {
                            $POP_Currency.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_Currency.removeClass('vis-ev-col-mandatory');
                        }
                    });
                    $POP_CurrencyType.on("change", function () {
                        if ($POP_CurrencyType.val() == "0") {
                            $POP_CurrencyType.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_CurrencyType.removeClass('vis-ev-col-mandatory');
                        }
                    });
                    //Devops Task Id - 1634
                    //VIS317  cheque date On Blur event 
                    //Handled the Cheque date should not be greater than Account date.
                    $POP_ChkDate.on("blur", function () {
                        if ($POP_ChkDate.val() > $POP_DateAcct.val()) {
                            $POP_ChkDate.val("");
                            VIS.ADialog.info(("VIS_CheckDateCantbeGreaterSys"));
                            return false;
                        }
                    });
                    $POP_PayMthd.on("change", function () {
                        if ($POP_PayMthd.val() == "0") {
                            $POP_PayMthd.addClass('vis-ev-col-mandatory');
                        }
                        else {
                            $POP_PayMthd.removeClass('vis-ev-col-mandatory');
                        }
                        if ($('option:selected', $POP_PayMthd).attr('paybase') == "S") {
                            $DivChkDate.show();
                            $DivChkNo.show();
                        }
                        else {
                            $DivChkDate.hide();
                            $DivChkNo.hide();
                        }
                        getCheckNo();
                    });

                    //onchange for changinng background-color if value is entered and validate
                    payAmount.getControl().on('change', function () {
                        if (parseFloat(payAmount.getValue()) > 0) {
                            payAmount.getControl().removeClass('vis-ev-col-mandatory');
                        }
                        else {
                            payAmount.getControl().addClass('vis-ev-col-mandatory');
                        }
                    });
                };
                var currentNextChequeNo = "";
                /**VA230:To get current Next Cheque Number based seleted bank, payment method document base type is AP Payment */
                function getCheckNo() {
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()) > 0 && VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()) > 0
                        && $('option:selected', $POP_PayMthd).attr('paybase') == "S" && $('option:selected', $POP_cmbDocType).attr('docbasetype') == "APP") {

                        VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/getCheckNo", { "C_BankAccount_ID": parseInt($POP_cmbBankAccount.val()), "VA009_PaymentMethod_ID": parseInt($POP_PayMthd.val()) }, callbackgetCheckNo);

                        function callbackgetCheckNo(dr) {
                            $POP_ChkNo.val('');
                            if (dr != "") {
                                $POP_ChkNo.val(dr);
                                //store current next cheque number and compare it before payment creation
                                currentNextChequeNo = dr;
                            }
                            else {
                                VIS.ADialog.info("VA009_CheckNotFound");
                            }
                        }
                    } else {
                        $POP_ChkNo.val('');
                        currentNextChequeNo = "";
                    }
                };
                function loadAllData() {
                    loadOrg();
                };
                function loadOrg() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadOrganization", null, callbackloadorg);
                    function callbackloadorg(dr) {
                        $POP_cmbOrg.empty();
                        $POP_cmbOrg.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_cmbOrg.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].AD_Org_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                            }
                        }
                        $POP_cmbOrg.prop('selectedIndex', 0);
                    };
                };
                //function loadBP() {
                //    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetBPartnerName", { "orgs": $POP_cmbOrg.val() }, callbackloadBP);
                //    function callbackloadBP(dr) {
                //        $POP_cmbBP.append(" <option value = 0></option>");
                //        if (dr.length > 0) {
                //            for (var i in dr) {
                //                $POP_cmbBP.append("<option value=" + VIS.Utility.Util.getValueOfString(dr[i].C_BPartner_ID) + ">" + VIS.Utility.Util.getValueOfString(dr[i].Name) + "</option>");
                //            }
                //        }
                //        $POP_cmbBP.prop('selectedIndex', 0);
                //    }
                //};
                function loadLocation() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetLocation", { "BP": $BpartnerControl.value }, callbackloadLocation);
                    function callbackloadLocation(dr) {
                        $POP_cmbLocation.empty();
                        $POP_cmbLocation.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_cmbLocation.append("<option value=" + VIS.Utility.Util.getValueOfString(dr[i].C_Location_ID) + ">" + VIS.Utility.Util.getValueOfString(dr[i].Name) + "</option>");
                            }
                        }
                        $POP_cmbLocation.prop('selectedIndex', 0);
                    }
                };
                function loadPaymentMthd() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadPaymentMethod", null, callbackloadpaymthds);
                    function callbackloadpaymthds(dr) {
                        $POP_PayMthd.empty();
                        $POP_PayMthd.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                if (("LCPB").contains(dr[i].VA009_PaymentBaseType)) {
                                }
                                else {
                                    $POP_PayMthd.append("<option paybase = " + dr[i].VA009_PaymentBaseType + " value=" + VIS.Utility.Util.getValueOfInt(dr[i].VA009_PaymentMethod_ID) + ">"
                                        + VIS.Utility.encodeText(dr[i].VA009_Name) + " </option>");
                                }
                            }
                        }
                        $POP_PayMthd.prop('selectedIndex', 0);
                    }
                };
                function loadDocType() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetDocumentType", { "orgs": $POP_cmbOrg.val() }, callbackloadDocType);
                    function callbackloadDocType(dr) {
                        $POP_cmbDocType.empty();
                        $POP_cmbDocType.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                //VA230:get docbasetype to check payment type
                                $POP_cmbDocType.append("<option docbasetype=" + VIS.Utility.Util.getValueOfString(dr[i]["DocBaseType"]) + " value=" + VIS.Utility.Util.getValueOfInt(dr[i]["C_DocType_ID"]) + ">" + VIS.Utility.Util.getValueOfString(dr[i]["Name"]) + "</option>");
                            }
                        }
                        $POP_cmbDocType.prop('selectedIndex', 0);
                    }
                };
                //   function loadCharge() {
                //    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetCharge", { "orgs": $POP_cmbOrg.val() }, callbackloadCharge);
                //    function callbackloadCharge(dr) {
                //        $POP_Charge.empty();
                //        $POP_Charge.append(" <option value = 0></option>");
                //        if (dr.length > 0) {
                //            for (var i in dr) {
                //                $POP_Charge.append("<option value=" + VIS.Utility.Util.getValueOfString(dr[i].C_Charge_ID) + ">" + VIS.Utility.Util.getValueOfString(dr[i].Name) + "</option>");
                //            }
                //        }
                //        $POP_Charge.prop('selectedIndex', 0);
                //    }
                //};
                function loadbanks(bankcmbdiv) {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadBank", { "Orgs": $POP_cmbOrg.val() }, callbackloadbank);
                    function callbackloadbank(dr) {
                        if (dr.length > 0) {
                            bankcmbdiv.find('option').remove();
                            bankcmbdiv.append("<option value='0'></option>");
                            for (var i in dr) {
                                bankcmbdiv.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_Bank_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                            }
                            bankcmbdiv.prop('selectedIndex', 0);
                        }
                    }
                };
                function loadCurrencies() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadCurrencies", null, callbackloadCurrencies);
                    function callbackloadCurrencies(dr) {
                        $POP_Currency.empty();
                        $POP_Currency.append(" <option value = 0></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_Currency.append("<option value=" + VIS.Utility.Util.getValueOfString(dr[i].C_Currency_ID) + ">" + VIS.Utility.Util.getValueOfString(dr[i].ISO_Code) + "</option>");
                            }
                        }
                        $POP_Currency.prop('selectedIndex', 0);
                    }
                };
                function loadCurrencyType() {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/loadCurrencyType", null, callbackCurrencyType);
                    function callbackCurrencyType(dr) {
                        $POP_CurrencyType.empty();
                        $POP_CurrencyType.append("<option value='0'></option>");
                        if (dr.length > 0) {
                            for (var i in dr) {
                                $POP_CurrencyType.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                                if (VIS.Utility.encodeText(dr[i].IsDefault) == "Y") {
                                    defaultCurrenyType = VIS.Utility.Util.getValueOfInt(dr[i].C_ConversionType_ID);
                                }
                            }
                        }
                    }
                };
                payAmount.addVetoableChangeListener(this);
                manualDialog.onOkClick = function () {
                    if (!isValidPayment()) {
                        return false;
                    }
                    else {
                        createPayment();
                    }
                };

                manualDialog.onCancelCLick = function () {
                    payManualDispose();
                };

                manualDialog.onClose = function () {
                    payManualDispose();

                };
                this.vetoablechange = function (evt) {
                    if (evt.propertyName == "VA009_cmbAmt_" + $self.windowNo + "") {
                        payAmount.setValue(evt.newValue);
                    }
                };
                function payManualDispose() {
                    $payManual = null;
                    _payManual = null;
                };

                function isValidPayment() {
                    if (VIS.Utility.Util.getValueOfInt($POP_cmbOrg.val()) > 0) {
                        if (VIS.Utility.Util.getValueOfInt($POP_cmbDocType.val()) > 0) {
                            if (VIS.Utility.Util.getValueOfInt($POP_cmbBank.val()) > 0) {
                                if (VIS.Utility.Util.getValueOfInt($POP_cmbBankAccount.val()) > 0) {
                                    if ($POP_DateTrx.val() != "" && $POP_DateTrx.val() != null) {
                                        if ($POP_DateAcct.val() != "" && $POP_DateAcct.val() != null) {
                                            if (VIS.Utility.Util.getValueOfInt($BpartnerControl.value) > 0) {
                                                if (VIS.Utility.Util.getValueOfInt($POP_cmbLocation.val()) > 0) {
                                                    if (VIS.Utility.Util.getValueOfInt($POP_Currency.val()) > 0) {
                                                        if (VIS.Utility.Util.getValueOfInt($POP_CurrencyType.val()) > 0) {
                                                            if (VIS.Utility.Util.getValueOfInt($POP_PayMthd.val()) > 0) {
                                                                if (VIS.Utility.Util.getValueOfDecimal(payAmount.getValue()) > 0) {
                                                                    if ($('option:selected', $POP_PayMthd).attr('paybase') == "S") {
                                                                        if ($POP_ChkNo.val() != "" && $POP_ChkDate.val() != "") {
                                                                            return true;
                                                                        }
                                                                        else {
                                                                            VIS.ADialog.info("VA009_PLCheckNumber&Date");
                                                                            return false;
                                                                        }
                                                                    }
                                                                    return true;
                                                                }
                                                                else {
                                                                    VIS.ADialog.info("VA009_PlsEnterAmount");
                                                                    return false;
                                                                }
                                                            }
                                                            else {
                                                                VIS.ADialog.info("VA009_PlsSelectPayMethod");
                                                                return false;
                                                            }
                                                        }
                                                        else {
                                                            VIS.ADialog.info("VA009_PlsSelectCurrencyType");
                                                            return false;
                                                        }
                                                    }
                                                    else {
                                                        VIS.ADialog.info("VA009_PlsSelectCurrency");
                                                        return false;
                                                    }
                                                }
                                                else {
                                                    VIS.ADialog.info("SelectBPLocation");
                                                    return false;
                                                }
                                            }
                                            else {
                                                VIS.ADialog.info("SelectBusinessPartnerFirst");
                                                return false;
                                            }
                                        }
                                        else {
                                            VIS.ADialog.info("VA009_PllsSelTAcctdate");
                                            return false;
                                        }
                                    }
                                    else {
                                        VIS.ADialog.info("PllsSelTdate");
                                        return false;
                                    }
                                }
                                else {
                                    VIS.ADialog.info("VA009_PLSelectBankAccount");
                                    return false;
                                }
                            }
                            else {
                                VIS.ADialog.info("VA009_PLSelectBank");
                                return false;
                            }
                        }
                        else {
                            VIS.ADialog.info("VA009_PlsSelectDocumentType");
                            return false;
                        }
                    }
                    else {
                        VIS.ADialog.info("VA009_PlsSelectOrg");
                        return false;
                    }
                };

                function createPayment() {
                    var PaymentData = [];
                    var DocNumber = "";
                    var isOverrideAutoCheck = false;
                    /** 
                    VA230:If checkno textfield value is not equal to currentNextChequeNo and docbasetype is AP Payment then override autocheck marked as true
                    */
                    if (($('option:selected', $POP_cmbDocType).attr('docbasetype') == "APP" && $POP_ChkNo.val() != currentNextChequeNo && currentNextChequeNo != "")) {
                        isOverrideAutoCheck = true;
                    }
                    PaymentData.push({
                        Org: $POP_cmbOrg.val(), DocType: $POP_cmbDocType.val(), BankID: $POP_cmbBank.val(),
                        BankAccountID: $POP_cmbBankAccount.val(), DateTrx: $POP_DateTrx.val(), DateAcct: $POP_DateAcct.val(),
                        BPID: $BpartnerControl.value, BPLocation: $POP_cmbLocation.val(), CurrencyID: $POP_Currency.val(),
                        CurrencyType: $POP_CurrencyType.val(), PaymentMethod: $POP_PayMthd.val(), PaymentAmount: payAmount.getValue(),
                        CheckNo: $POP_ChkNo.val(), CheckDate: $POP_ChkDate.val(), charge: VIS.Utility.Util.getValueOfInt($ChargeControl.value), IsOverrideAutoCheck: isOverrideAutoCheck
                    });
                    $bsyDiv[0].style.visibility = "visible";
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/GeneratePayMannual",
                        type: "POST",
                        datatype: "json",
                        // contentType: "application/json; charset=utf-8",
                        async: true,
                        data: ({
                            PaymentData: JSON.stringify(PaymentData)
                        }),
                        success: function (result) {
                            result = JSON.parse(result);
                            DocNumber = "";
                            //var SpilitedData = result.split(".", result.length);
                            //DocNumber = VIS.Utility.Util.getValueOfString(SpilitedData[1]);
                            //console.log(DocNumber);
                            $divPayment.find('.VA009-payment-wrap').remove();
                            $divBank.find('.VA009-right-data-main').remove();
                            $divBank.find('.VA009-accordion').remove();
                            pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; SlctdJournalPaymentIds = []; batchObjInv = []; batchObjOrd = []; batchObjJournal = [];
                            resetPaging();
                            loadPaymetsAll();
                            clearamtid();
                            $bsyDiv[0].style.visibility = "hidden";
                            //VIS.ADialog.info("", null, result, "");
                            File_Para = 'M';
                            if (DocNumber != "") {
                                w2confirm(VIS.Msg.getMsg('VA009_GenPaymentFile'))
                                    .yes(function () {
                                        SlctdPaymentIds = [];
                                        SlctdOrderPaymentIds = [];
                                        SlctdJournalPaymentIds = [];
                                        $selectall.prop('checked', false);
                                        $divPayment.find(':checkbox').prop('checked', false);
                                        prepareDataForPaymentFile(DocNumber, false);
                                    })
                                    .no(function () {
                                        $bsyDiv[0].style.visibility = "hidden";
                                        VIS.ADialog.info("", null, result, "");
                                        console.log("user clicked NO")
                                    });
                            }
                            else {
                                VIS.ADialog.info("", null, result, "");
                            }
                        },
                        error: function (ex) {
                            console.log(ex);
                            $bsyDiv[0].style.visibility = "hidden";
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                };

            },

            //added new button to show check details if payment method is cheque
            //cheque details dialouge for showing thr details of cheque and based on batch line detail count
            ChequeDetails_Dialog: function (ds, isConsolidate) {

                $opnChkDtls = $("<div class='VA009-popform-content' style='min-height:25px !important'>");
                var _opnChkDtls = "";
                var BPLocLookup = null;
                var btnPrintCheque = $("<button class= 'ui-button' id = 'VA009_PrintCheque" + $self.windowNo + "'>" + VIS.Msg.getMsg("VA009_PrintCheque") + "</button>");
                _opnChkDtls += "<div class='VA009-table-container' style='height:300px;' id='VA009_ChkDetailsGrid_" + $self.windowNo + "'> </div>'";
                $opnChkDtls.append(_opnChkDtls).append(btnPrintCheque);
                var _chequeDetailsGrid = $opnChkDtls.find("#VA009_ChkDetailsGrid_" + $self.windowNo);
                $btnChequePrint = $opnChkDtls.find("#VA009_PrintCheque" + $self.windowNo);
                //False everytime when dialog opens to show the message if cheques are not available 
                _ChequesNotAvailable = false;
                var chequeDetailsDialog = new VIS.ChildDialog();
                chequeDetailsDialog.setContent($opnChkDtls);
                chequeDetailsDialog.setTitle(VIS.Msg.getMsg("VA009_CheckDetails"));
                chequeDetailsDialog.setWidth("50%");
                chequeDetailsDialog.setEnableResize(true);
                chequeDetailsDialog.setModal(true);
                chequeDetailsDialog.show();
                chequeDetailsDialog.hidebuttons();
                ChequeDetailsGrid_Layout();
                BPLocLookup = VIS.MLookupFactory.get(VIS.Env.getCtx(), $self.windowNo, 0, VIS.DisplayType.TableDir, "C_BPartner_Location_ID", 0, false, "C_BPartner_Location.IsActive='Y' ");
                loadChkDtlsGridData(JSON.parse(ds), ChequeDetailsGrd, JSON.parse(JSON.stringify(BatchGrd.records)), isConsolidate);
                chequeDetailsDialog.onOkClick = function () {
                    chequeDetailsDispose();
                };
                chequeDetailsDialog.onCancelCLick = function () {
                    chequeDetailsDispose();
                };
                chequeDetailsDialog.onClose = function () {
                    chequeDetailsDispose();
                };
                $btnChequePrint.on("click", function (e) {
                    $bsyDiv[0].style.visibility = "visible";
                    $.ajax({
                        url: VIS.Application.contextUrl + "VA009/Payment/SavePrintCheckDetails",
                        type: "POST",
                        datatype: "json",
                        async: true,
                        data: ({ "ChequeData": JSON.stringify(chequePrintParams), "BankId": $POP_cmbBank.val(), "BankAccId": $POP_cmbBankAccount.val(), "IsConsolidate": ($consolidate.is(':checked')) ? 'Y' : 'N' }),
                        success: function (data) {
                            if (data != null) {
                                data = JSON.parse(data);
                                var Ids = data.split(';');
                                if (Ids && Ids.length > 2) {
                                    var prin = new VIS.APrint(Ids[0], Ids[1], Ids[2], $self.windowNo, null);//process, tableid , recordid
                                    prin.startPdf();
                                }
                            }
                            $bsyDiv[0].style.visibility = "hidden";
                        },
                        error: function (ex) {
                            console.log(ex);
                            $bsyDiv[0].style.visibility = "hidden";
                            VIS.ADialog.error("VA009_ErrorLoadingPayments");
                        }
                    });
                });
                function chequeDetailsDispose() {
                    w2ui["VA009_ChkDetailsGrid_" + $self.windowNo].clear();
                    _opnChkDtls = null;
                    $opnChkDtls = null;
                    w2ui["VA009_ChkDetailsGrid_" + $self.windowNo].destroy();
                };

                function ChequeDetailsGrid_Layout() {
                    var _chkDetails_Columns = [];
                    if (_chkDetails_Columns.length == 0) {
                        _chkDetails_Columns.push({ field: "recid", caption: VIS.Msg.getMsg("VA009_srno"), sortable: true, size: '10%' });
                        _chkDetails_Columns.push({ field: "C_Bpartner", caption: VIS.Msg.getMsg("VA009_BPartner"), sortable: true, size: '10%' });
                        _chkDetails_Columns.push({
                            field: "C_BPartner_Location_ID", caption: VIS.Msg.getMsg("VA009_PayLocation"), sortable: true, size: '15%', render: function (record, index, col_index) {
                                var l = BPLocLookup;
                                var val = record["C_BPartner_Location_ID"];
                                var d;
                                if (l) {
                                    d = l.getDisplay(val);
                                }
                                return d;
                            }
                        });
                        _chkDetails_Columns.push({ field: "C_InvoicePaySchedule_ID", caption: VIS.Msg.getMsg("VA009_Schedule"), hidden: true, sortable: true, size: '0%' });
                        _chkDetails_Columns.push({ field: "CurrencyCode", caption: VIS.Msg.getMsg("VA009_Currency"), hidden: true, sortable: true, size: '10%' });
                        _chkDetails_Columns.push({ field: "DueAmt", caption: VIS.Msg.getMsg("VA009_DueAmt"), hidden: true, sortable: true, size: '10%', style: 'text-align: right' });
                        _chkDetails_Columns.push({
                            field: "ConvertedAmt", caption: VIS.Msg.getMsg("Amount"), sortable: true, size: '13%', style: 'text-align: right', render: function (record, index, col_index) {
                                var val = record["ConvertedAmt"];
                                return parseFloat(val).toLocaleString(window.navigator.language, { minimumFractionDigits:precision});
                            }
                        });
                        _chkDetails_Columns.push({ field: "CheckNumber", caption: VIS.Msg.getMsg("VA009_ChkNo"), sortable: true, size: '10%' });
                        _chkDetails_Columns.push({
                            field: "CheckDate", caption: VIS.Msg.getMsg("VA009_CheckDate"), sortable: true, size: '12%',
                            render: function (record, index, col_index) {
                                var val;
                                if (record.changes == undefined || record.changes.CheckDate == "") {
                                    val = record["CheckDate"];
                                }
                                else {
                                    val = record.changes.CheckDate;
                                }
                                return new Date(val).toLocaleDateString();
                            }, style: 'text-align: left'
                        });
                        _chkDetails_Columns.push({ field: "Mandate", caption: VIS.Msg.getMsg("VA009_Mandate"), hidden: true, sortable: true, size: '10%' });
                        _chkDetails_Columns.push({ field: "TransactionType", caption: VIS.Msg.getMsg("VA009_TransactionType"), sortable: true, size: '1%' });
                        _chkDetails_Columns.push({ field: "ConversionTypeId", caption: VIS.Msg.getMsg("VA009_ConversionType"), hidden: true, sortable: true, size: '0%' });
                        _chkDetails_Columns.push({ field: "DiscountAmount", caption: VIS.Msg.getMsg("DiscountAmount"), hidden: true, sortable: true, size: '0%' });
                        _chkDetails_Columns.push({ field: "ConvertedDiscountAmount", caption: VIS.Msg.getMsg("ConvertedDiscountAmount"), hidden: true, sortable: true, size: '0%' });
                        _chkDetails_Columns.push({ field: "DiscountDate", caption: VIS.Msg.getMsg("DiscountDate"), hidden: true, sortable: true, size: '0%' });
                    }
                    ChequeDetailsGrd = null;
                    ChequeDetailsGrd = _chequeDetailsGrid.w2grid({
                        name: 'VA009_ChkDetailsGrid_' + $self.windowNo,
                        recordHeight: 25,
                        multiSelect: true,
                        columns: _chkDetails_Columns
                    }),
                        ChequeDetailsGrd.hideColumn('recid', 'CurrencyCode', 'C_InvoicePaySchedule_ID', 'DueAmt', 'ValidMonths', 'Mandate', 'TransactionType', 'ConversionTypeId', 'DiscountAmount', "ConvertedDiscountAmount", 'DiscountDate');
                };

                //load data in cheque details grid.
                //ds--- it contains the data array of cheque series from bank document.
                //ChequeDetailsGrd--- Grid in which data will be shown
                //SelectedRecords----records which are selected for batch
                //isConsolidate--- flag for consolidate funatioanlity.
                function loadChkDtlsGridData(ds, ChequeDetailsGrd, SelectedRecords, isConsolidate) {

                    SelectedRecords.sort(SortingAccordingBPLocation);  //VIS_427 Calling the function for sorting

                    if (SelectedRecords.length > 0) {
                        var checkNum = 0; var recds = []; var amt = 0;
                        var maxLineCount = 0; var ChkAutoControl = "N";
                        var chk;
                        if (ds != null && ds.length > 0) {
                            checkNum = parseInt(ds[0]["currentnext"]);
                            maxLineCount = parseInt(ds[0]["va009_batchlinedetailcount"]);
                            ChkAutoControl = ds[0]["chknoautocontrol"];
                            // it will contain the next check number to be assigned
                            ds[0]["ASSIGNEDCHKNUM"] = checkNum;
                            //it contains the total lines created
                            ds[0]["TOTALLINESCOUNT"] = 0;
                            chk = ds;
                        }
                        //if no bank account document record found then show msg cheques not available
                        else {
                            chequeDetailsDispose();
                            chequeDetailsDialog.close();
                            VIS.ADialog.info("VA009_ChequesNotAvail");
                        }

                        if (ChkAutoControl == "Y") {
                            for (i = 0; i < SelectedRecords.length; i++) {

                                if (SelectedRecords[i].IsAPCGreater) {
                                    chequeDetailsDispose();
                                    chequeDetailsDialog.close();
                                    VIS.ADialog.info("VA009_APCreditIsGreater");
                                    return false;
                                }
                                if (!isConsolidate && SelectedRecords[i].IsAPCExists) {
                                    chequeDetailsDispose();
                                    chequeDetailsDialog.close();
                                    VIS.ADialog.info("VA009_APCreditExists");
                                    return false;
                                }
                                chk = getNextCheckNumberBasedOnAssigned(ds, checkNum);
                                if (chk != null) {
                                    checkNum = chk["ASSIGNEDCHKNUM"];
                                }

                                if (i == 0) {
                                    SelectedRecords[i]["CheckNumber"] = checkNum;
                                    SelectedRecords[i]["TotalLinesCount"] = 1;
                                    SelectedRecords[i]["MergeIds"] = VIS.Utility.Util.getValueOfString(SelectedRecords[i].recid);
                                    checkNum = checkNum + 1;
                                    ds[0]["TOTALLINESCOUNT"] = 1;
                                    recds.push(SelectedRecords[i]);
                                }
                                else {
                                    //Find the selected object into the array based on Business Partner, Location, Lines Count and DocBaseType.
                                    var filterObj = recds.filter(function (e) {
                                        if (isConsolidate) {
                                            if ((e.DocBaseType == "API" || e.DocBaseType == "APC") && (SelectedRecords[i].DocBaseType == "API" || SelectedRecords[i].DocBaseType == "APC")) {
                                                return (
                                                    e.C_BPartner_ID == SelectedRecords[i].C_BPartner_ID
                                                    && e.C_BPartner_Location_ID == SelectedRecords[i].C_BPartner_Location_ID
                                                    && e.TotalLinesCount < maxLineCount);
                                            }
                                        }
                                    });

                                    //if record not found then add the checknumber and Increase total count in array
                                    if (filterObj.length == 0) {
                                        SelectedRecords[i]["CheckNumber"] = checkNum;
                                        checkNum = checkNum + 1;
                                        SelectedRecords[i]["TotalLinesCount"] = 1;
                                        SelectedRecords[i]["MergeIds"] = VIS.Utility.Util.getValueOfString(SelectedRecords[i].recid);
                                        ds[0]["TOTALLINESCOUNT"] = 1;
                                        recds.push(SelectedRecords[i]);
                                    }
                                    //if record already found then add the checknumber and Increase total count
                                    else {
                                        //API==API OR APC==APC
                                        if (filterObj[0]["DocBaseType"] == SelectedRecords[i]["DocBaseType"]) {
                                            //amt = parseFloat(filterObj[0]["DueAmt"]) + parseFloat(SelectedRecords[i]["DueAmt"]);
                                            amt = parseFloat(filterObj[0]["ConvertedAmt"]) + parseFloat(SelectedRecords[i]["ConvertedAmt"]);
                                        }
                                        else {
                                            //Old Record DocBaseType = API AND New Record is API&& SelectedRecords[i].IsAPCExists
                                            if (SelectedRecords[i]["DocBaseType"] == "API") {
                                                //API > APC
                                                if (parseFloat(SelectedRecords[i]["ConvertedAmt"]) > parseFloat(filterObj[0]["ConvertedAmt"]))
                                                    amt = parseFloat(SelectedRecords[i]["ConvertedAmt"]) - parseFloat(filterObj[0]["ConvertedAmt"]);
                                            }
                                            //Old Record DocBaseType = APC AND New Record is APC
                                            else if (SelectedRecords[i]["DocBaseType"] == "APC") {
                                                //APC < API
                                                if (parseFloat(SelectedRecords[i]["ConvertedAmt"]) < parseFloat(filterObj[0]["ConvertedAmt"]))
                                                    amt = parseFloat(filterObj[0]["ConvertedAmt"]) - parseFloat(SelectedRecords[i]["ConvertedAmt"]);
                                            }//if record is ordertype then Amt calculated
                                            else if (SelectedRecords[i]["DocBaseType"] == "POO") {
                                                amt = parseFloat(filterObj[0]["ConvertedAmt"]) + parseFloat(SelectedRecords[i]["ConvertedAmt"]);
                                            }//In case of order and document types are not same
                                            // amt = parseFloat(filterObj[0]["DueAmt"]) + parseFloat(SelectedRecords[i]["DueAmt"]);
                                            //amt = parseFloat(filterObj[0]["ConvertedAmt"]) + parseFloat(SelectedRecords[i]["ConvertedAmt"]);
                                        }
                                        SelectedRecords[i]["CheckNumber"] = filterObj[0]["CheckNumber"];
                                        SelectedRecords[i]["ConvertedAmt"] = amt;
                                        filterObj[0]["MergeIds"] += "," + VIS.Utility.Util.getValueOfString(SelectedRecords[i].recid);
                                        filterObj[0]["ConvertedAmt"] = amt;
                                        filterObj[0]["TotalLinesCount"] = parseInt(filterObj[0]["TotalLinesCount"]) + 1;
                                        ds[0]["TOTALLINESCOUNT"] = filterObj[0]["TotalLinesCount"];
                                    }
                                }
                                if (chk != null) {
                                    for (j = 0; j < ds.length; j++) {
                                        if (parseInt(ds[j]["c_bankaccountdoc_id"]) == parseInt(chk["c_bankaccountdoc_id"])) {
                                            ds[j]["ASSIGNEDCHKNUM"] = checkNum;
                                        }
                                    }

                                }
                            }
                            console.log(recds);
                            preparedataforChqPrint(recds, SelectedRecords);
                            chequePrintParams = SelectedRecords;//Get all popup records with change in Converted Amt and Due Amt
                            w2utils.encodeTags(recds);
                            ChequeDetailsGrd.add(recds);
                        }
                        else {
                            chequeDetailsDispose();
                            chequeDetailsDialog.close();
                            VIS.ADialog.info("VA009_PlzCngStngOfAutoCntrl");
                        }
                    }
                    //show msg if cheques are not available 
                    if (_ChequesNotAvailable) {
                        VIS.ADialog.info("VA009_ChequesNotAvail");
                    }
                };

                function preparedataforChqPrint(recds, SelectedRecords) {
                    var recIds = [];
                    if (recds.length > 0) {
                        for (var i = 0; i < recds.length; i++) {
                            recIds = recds[i]["MergeIds"].split(",");
                            if (recIds.length > 0) {
                                recIds.forEach(function (e) {
                                    upd_obj = SelectedRecords.findIndex(obj => parseInt(obj.recid) == parseInt(e));
                                    if (upd_obj != -1)
                                        SelectedRecords[upd_obj]["ConvertedAmt"] = parseFloat(recds[i]["ConvertedAmt"]);
                                });
                            }
                        }
                    }
                }

                //get the check series based on priority and check number.
                function getNextCheckNumberBasedOnAssigned(Chk_DT, chkNum) {
                    var chkDtl = [];
                    var isNewSeries = false;
                    for (C_Num = 0; C_Num < Chk_DT.length; C_Num++) {
                        if (Chk_DT[C_Num].hasOwnProperty("ASSIGNEDCHKNUM")) {
                            if (parseInt(Chk_DT[C_Num]["ASSIGNEDCHKNUM"]) <= parseInt(Chk_DT[C_Num]["endchknumber"])) {
                                chkDtl = Chk_DT[C_Num];
                                isNewSeries = false;
                                break;
                            }
                        }
                        else {
                            Chk_DT[C_Num]["ASSIGNEDCHKNUM"] = parseInt(Chk_DT[C_Num]["currentnext"]);
                            Chk_DT[C_Num]["TOTALLINESCOUNT"] = 0;
                            chkDtl = Chk_DT[C_Num];
                            isNewSeries = true;
                            break;
                        }
                    }
                    //Needs to set balnk in cheque number and show the message if cheques are not available 
                    if (!isNewSeries && chkDtl != null)
                        chkDtl["ASSIGNEDCHKNUM"] = parseInt(chkNum);
                    if (chkDtl == null || (chkDtl != null && chkDtl.length == 0)) {
                        chkDtl["ASSIGNEDCHKNUM"] = "";
                        _ChequesNotAvailable = true;
                    }
                    return chkDtl;
                };
            }
        };

        /* VIS_427 Devops Taskid: 2238 Sorting of Records according to business partner location */
        function SortingAccordingBPLocation(RecordId1, RecordId2) {
            if (RecordId1.C_BPartner_Location_ID < RecordId2.C_BPartner_Location_ID) {
                return -1;
            }
            if (RecordId1.C_BPartner_Location_ID > RecordId2.C_BPartner_Location_ID) {
                return 1;
            }
            return 0;
        }

        //*******************************************
        //Function to load Banks 
        //Parameter: Combobox of Bank, OrgIds Array
        //*******************************************
        function loadbanks(bankcmbdiv, ids) {
            VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/LoadBank", { "Orgs": ids.toString() }, callbackloadbank);
            function callbackloadbank(dr) {
                if (dr != null && dr.length > 0) {
                    bankcmbdiv.find('option').remove();
                    bankcmbdiv.append("<option value='0'></option>");
                    for (var i in dr) {
                        bankcmbdiv.append("<option value=" + VIS.Utility.Util.getValueOfInt(dr[i].C_Bank_ID) + ">" + VIS.Utility.encodeText(dr[i].Name) + "</option>");
                    }
                    bankcmbdiv.prop('selectedIndex', 0);
                }
            }
        };

        function paymentScroll() {
            //VA230:Added 5 into scrolltop and innerHeight to solve somtimes condition not matched issue
            if ($(this).scrollTop() + $(this).innerHeight() + 5 >= this.scrollHeight) {
                if (pgNo < noPages) {
                    pgNo++;
                    if (orgids.length == 0) {
                        if (VIS.context.getAD_Org_ID() <= 0) {
                            _WhrOrg = "";
                        }
                        else {
                            _WhrOrg = "AND cs.AD_Org_ID IN (" + VIS.context.getAD_Org_ID() + ")";
                        }
                    }
                    //loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
                    loadPaymetsAll();
                    isReset = true; /*VIS_427 DevOps id:2238 Assigned value as true to mark chekbox checked*/
                }
            }
        };

        //to generate data for file creation
        function prepareDataForPaymentFile(DocNumber, isBatch) {
            $bsyDiv[0].style.visibility = "visible";
            $.ajax({
                url: VIS.Application.contextUrl + "VA009/Payment/prepareDataForPaymentFile",
                type: "GET",
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                async: true,
                data: ({ "DocNumber": DocNumber, "isBatch": isBatch }),//these parameteres are not used in controller now
                success: function (result) {
                    result = JSON.parse(result);
                    for (var i in result) {
                        if (result[i]._error != null) {
                            var error = result[i]._error;
                            if (isBatch)
                                VIS.ADialog.info(error + "," + VIS.Msg.getMsg("BatchIsCompleted") + DocNumber)
                            else
                                VIS.ADialog.info(error + "," + VIS.Msg.getMsg("VA009_PaymentCompletedWith") + DocNumber)
                        }
                        else {
                            downloadURL("\\PaymentFiles\\" + result[i]._filename, i);
                        }
                    }
                    $bsyDiv[0].style.visibility = "hidden";
                },
                error: function () {
                    VIS.ADialog.error("VA009_ErrorLoadingPayments");
                    $bsyDiv[0].style.visibility = "hidden";
                }
            });
        };

        function generateXMLDialog(sourceBtn) {
            $xml = $("<div class='VA009-popform-content' style='min-height:333px !important'>");
            var _xml = "";
            _xml += "<div class='VA009-popform-data'>"
                + "<label>" + VIS.Msg.getMsg("VA009_GenXML_Tab") + "</label>"
                + "</div>"

                + "  <div class='VA009-table-container' id='VA009_btnPopupGrid'>  </div> "
                + "</div>";

            $xml.append(_xml);
            var XmlDialog = new VIS.ChildDialog();

            XmlDialog.setContent($xml);
            XmlDialog.setTitle(VIS.Msg.getMsg("VA009_GenXML_Tab"));
            XmlDialog.setWidth("60%");
            XmlDialog.setEnableResize(true);
            XmlDialog.setModal(true);
            XmlDialog.show();
            $xmlpopGrid = $xml.find("#VA009_btnPopupGrid");
            if (sourceBtn == "B") { // for Batch
                getScheduleBatchDetails();
            }
            else if (sourceBtn == "M") { // for Manual Payment

            }
            XmlDialog.onOkClick = function () {
                VIS.ADialog.info("VA009_GenXML_Tab");

            };

            XmlDialog.onCancelCLick = function () {
                XmlDispose();
            };

            XmlDialog.onClose = function () {
                XmlDispose();

            };

            function XmlDispose() {
                $xml = null;
                _xml = null;
            }

        };

        //*************************************
        //Function For Split Payment
        //*************************************
        function generateLines() {
            try {
                var dr = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetBatchProcess", null, null);
                if (dr != null) {
                    var processId = dr["AD_Process_ID"];
                    var name = dr["Name"]
                    var className = dr["ClassName"]
                    var type = dr["EntityType"]
                    if (processId > 0) {
                        var pp = new VIS.ParameterDialog($self.windowNo, $self);
                        pp.initDialog(processId, name, type, className, false);
                        pp.onCloseMain = function (output) {
                            if (!output) {
                                //process now
                                $bsyDiv[0].style.visibility = "hidden";
                            }
                            else {
                                $bsyDiv[0].style.visibility = "visible";
                            }
                        }
                        pp.showDialog();
                        pp = null;
                    }
                    else {
                        VIS.ADialog.info('ProcessNotFound', true, "", "");
                    }
                }
                $divPayment.find('.VA009-payment-wrap').remove();
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                pgNo = 1; SlctdPaymentIds = []; SlctdOrderPaymentIds = []; SlctdJournalPaymentIds = []; batchObjInv = []; batchObjOrd = []; batchObjJournal = [];
                resetPaging();
                //loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
                loadPaymetsAll();
                clearamtid();
                $bsyDiv[0].style.visibility = "hidden";
            }
            catch (e) {
            }
        };
        //*************************************
        //Fill Business Partner AutoComplete
        //*************************************
        function fillAutoCompleteonTextBox(text, response, funName) {
            $.ajax({
                url: VIS.Application.contextUrl + "VA009/Payment/" + funName,
                dataType: "json",
                data: {
                    searchText: text
                },
                success: function (data) {
                    var result = JSON.parse(data);
                    datasource = [];
                    response($.map(result, function (item) {
                        return {
                            label: item.Name,
                            value: item.Name,
                            ids: item.C_BPartner_ID
                        }
                    }));
                    //$($self.div).autocomplete("search", "");
                    //$($self.div).trigger("focus");
                    $($BP).autocomplete("search", "");
                    $($BP).trigger("focus");
                }
            });
        };
        //*****************************
        //Fill Data in Middle Div (Payments Data)
        //***************************************
        function loadPaymets(_isInv, _DocBaseTyp, pgNo, pgSize, _orgwhr, _PaymWhr, _statuswhr, _BPWhr, _SearchText, DueDateSelected, _TransTypewhr, Frmdate, Todate, callback) {
            //VA230:Handle amount search according to culture in searchtext field
            var txtSearchText = convertSearchAmountToDotFormat();
            $bsyDiv[0].style.visibility = "visible";
            _whereQry = _PaymWhr + _statuswhr + _BPWhr + _orgwhr;
            FinalWhereQry(_isInv, _DocBaseTyp, _whereQry);
            $.ajax({
                url: VIS.Application.contextUrl + "VA009/Payment/GetData",
                type: "GET",
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                async: true,
                data: ({ pageNo: pgNo, pageSize: pgSize, whereQry: _WhereQuery, OrgWhr: _orgwhr, SearchText: txtSearchText, WhrDueDate: DueDateSelected, TransType: _TransTypewhr, FromDate: Frmdate, ToDate: Todate }),
                success: function (result) {
                    callback(result);
                },
                error: function () {
                    VIS.ADialog.error("VA009_ErrorLoadingPayments");
                    $bsyDiv[0].style.visibility = "hidden";
                }
            });
        };
        // changed design of form requested by mukesh sir
        function loadcallback(result) {
            var id = 0; var imgname = ""; var dsgn = "";
            var data = JSON.parse(result);
            //handled no records found when filter the records from left panel 
            if (data.paymentdata.length == 0) {
                if ($selectall.is(":checked")) {
                    $selectall.trigger("click");
                }
            }

            //If payment schedule found
            if (data.paymentdata.length > 0) {
                if (pgNo == 1 && data.paymentdata.length > 0) {
                    paymentCount = data.paymentdata[0].paymentCount;
                    noPages = Math.ceil(paymentCount / PAGESIZE);
                }
                for (var i in data.paymentdata) {
                    id = id + 1;
                    if (data.paymentdata[i].TransactionType == "Invoice") {
                        SelectallInvIds.indexOf(data.paymentdata[i].C_InvoicePaySchedule_ID) === -1 ? SelectallInvIds.push(data.paymentdata[i].C_InvoicePaySchedule_ID) : console.log("This item already exists");
                    }
                    else if (data.paymentdata[i].TransactionType == "Order") {
                        SelectallOrdIds.indexOf(data.paymentdata[i].C_InvoicePaySchedule_ID) === -1 ? SelectallOrdIds.push(data.paymentdata[i].C_InvoicePaySchedule_ID) : console.log("This item already exists");
                    }
                    else {
                        SelectallJournalIds.indexOf(data.paymentdata[i].C_InvoicePaySchedule_ID) === -1 ? SelectallJournalIds.push(data.paymentdata[i].C_InvoicePaySchedule_ID) : console.log("This item already exists");
                    }
                    if (data.paymentdata[i].VA009_plannedduedate != null) {
                        var Dt = new Date(data.paymentdata[i].VA009_plannedduedate);
                        Dt = Dt.toLocaleDateString();
                        var VA009_plannedduedate = Dt;
                    }
                    else {
                        var VA009_plannedduedate = data.paymentdata[i].Systemdate;
                    }
                    if (data.paymentdata[i].VA009_FollowupDate != null) {
                        var DtF = new Date(data.paymentdata[i].VA009_FollowupDate);
                        DtF = DtF.toLocaleDateString();
                        var VA009_FollowupDate = DtF;
                    }
                    else {
                        var VA009_FollowupDate = data.paymentdata[i].Systemdate;
                    }
                    //var DueAmt = data.paymentdata[i].DueAmt;//not in use

                    var _SameCurrency = 'N';
                    if (data.paymentdata[i].CurrencyCode == data.paymentdata[i].BaseCurrencyCode) {
                        _SameCurrency = 'Y';
                    }
                    if (data.paymentdata[i].PaymwentBaseType == "S") {
                        imgname = "cheque-white.png";
                    }
                    else if (data.paymentdata[i].PaymwentBaseType == "B") {
                        imgname = "cash-white.png";
                    }
                    else {
                        imgname = "batch-white.png";
                    }

                    dsgn = '<div class="VA009-payment-wrap" data-UID="' + data.paymentdata[i].C_InvoicePaySchedule_ID + '"> <div class="row" data-UID="' + data.paymentdata[i].C_InvoicePaySchedule_ID + '">' +
                        '<div class="col-md-4 col-sm-4 width-sm-35 VA009-padd-right-0"> <input type="checkbox" class="VA009-clckd-checkbx" data-UID="' + data.paymentdata[i].C_InvoicePaySchedule_ID
                        + '" data-BaseAmt="' + data.paymentdata[i].BaseAmt + '" data-NAME="' + data.paymentdata[i].TransactionType + '" data-bpid="' + data.paymentdata[i].C_BPartner_ID + '" data-PaymentRule="' + data.paymentdata[i].PaymentRule
                        + '" data-PaymentType="' + data.paymentdata[i].PaymentType + '" data-precision="' + data.paymentdata[i].precision + '" data-PaymentTriggerBy="' + data.paymentdata[i].PaymentTriggerBy + '" data-PaymwentBaseType="' + data.paymentdata[i].PaymwentBaseType
                        + '" data-DocBaseType="' + data.paymentdata[i].DocBaseType + '" data-CurrencyCode="' + data.paymentdata[i].CurrencyCode + '"  alt="' + VIS.Msg.getMsg("VA009_Select") + '" title="' + VIS.Msg.getMsg("VA009_Select")
                        + '" ' + (data.paymentdata[i].IsHoldPayment == "Y" ? "disabled" : "") + '>' +
                        '<div class="VA009-pay-left">' +
                        '<div class="VA009-pay-img-wrap">' +
                        "<span class='VA009-pay-img'><img src='" + VIS.Application.contextUrl + "Areas/VA009/Images/" + imgname + "' alt=''></span> <span class='VA009-pay-status'>" + data.paymentdata[i].VA009_ExecutionStatus + "</span> </div>" +
                        ' <div class="VA009-pay-text"><p>' + data.paymentdata[i].C_Bpartner + '</p><span>' + data.paymentdata[i].C_BP_Group + '</span> <span>' + data.paymentdata[i].DocumentNo + '</span> <span>' + data.paymentdata[i].VA009_PaymentMethod + '</span> </div>' +
                        '</div></div>'
                        + '<div class="col-md-3 col-sm-3 width-sm-30 sm-padd VA009-padd-right-0">';

                    if (VA009_FollowupDate != "" && VA009_plannedduedate != "")
                        dsgn += ' <div class="VA009-left-data VA009-pay-mid-sec"><span title="Due Date">' + VA009_plannedduedate + '</span> <span class="glyphicon glyphicon-play play-icon"></span> <span title="Planned Due Date">' + VA009_FollowupDate + '</span> </div> ';
                    else if ((VA009_FollowupDate == "" && VA009_plannedduedate != ""))
                        dsgn += ' <div class="VA009-left-data VA009-pay-mid-sec"><span title="Due Date">' + VA009_plannedduedate + '</span></div> ';
                    else if ((VA009_FollowupDate != "" && VA009_plannedduedate == ""))
                        dsgn += ' <div class="VA009-left-data VA009-pay-mid-sec"><span title="Planned Due Date">' + VA009_FollowupDate + '</span></div> ';

                    if (Globalize.format(data.paymentdata[i].VA009_RecivedAmt > 0)) {
                        dsgn += ' <div class="VA009-left-data VA009-pay-mid-sec"> <span class="VA009-color-gray" title="' + (data.paymentdata[i].TransactionType == "Invoice" ? VIS.Msg.getMsg("InvoiceAmount") : VIS.Msg.getMsg("VA009_OrderAmount")) + '">' + data.paymentdata[i].CurrencyCode + ' ' + parseFloat(data.paymentdata[i].TotalInvAmt).toLocaleString()
                            + ' </span> <br> <span class="glyphicon glyphicon-ok play-icon"></span> <span class="VA009-color-gray" title="' + (_DocType == "ARI" ? VIS.Msg.getMsg("VA009_TotRecAmt") : VIS.Msg.getMsg("VA009_TotPaidAmt")) + '">'
                            + data.paymentdata[i].CurrencyCode + ' ' + parseFloat(data.paymentdata[i].VA009_RecivedAmt).toLocaleString() + ' </span></div> ';
                    }
                    else {
                        dsgn += ' <div class="VA009-left-data VA009-pay-mid-sec"> <span class="VA009-color-gray" title="' + (data.paymentdata[i].TransactionType == "Invoice" ? VIS.Msg.getMsg("InvoiceAmount") : VIS.Msg.getMsg("VA009_OrderAmount")) + '">' + data.paymentdata[i].CurrencyCode + ' ' + parseFloat(data.paymentdata[i].TotalInvAmt).toLocaleString() + ' </span> </div> ';
                    }

                    dsgn += ' <div class="VA009-left-data VA009-pay-mid-sec"> <span data-UID="' + data.paymentdata[i].C_InvoicePaySchedule_ID + '" class="VA009_AddNote" style=" cursor: pointer;"><i class="VA009_AddNoteimg fa fa-list-alt" data-UID="' + data.paymentdata[i].C_InvoicePaySchedule_ID + '" title="' + VIS.Msg.getMsg("VA009_AddNote") + '" > </i></span> </div> ' +
                        ' <div class="VA009-left-data VA009-pay-mid-sec" id="VA009-LastChat_' + $self.windowNo + '"> <span class="VA009-Chatcolor-gray" id=VA009-Chatcolor-gray_' + data.paymentdata[i].C_InvoicePaySchedule_ID + '>' + data.paymentdata[i].LastChat + '</span> </div> ' +
                        ' </div> ' + '<div class="col-md-2 col-sm-2 width-sm-20 sm-padd VA009-padd-right-0">'
                        + '<div class="VA009-transactionType"> <span>' + data.paymentdata[i].TransactionType + (data.paymentdata[i].IsHoldPayment == "Y" ? " (" + VIS.Msg.getMsg("VA009_HoldPayment") + ")" : "") + '</span> '
                    if (data.paymentdata[i].DocBaseType == "APC" || data.paymentdata[i].DocBaseType == "ARC") {
                        dsgn += '<br><span style="text-decoration: underline;color: red;font-size: 12px;">Credit Memo</span> </div></div>';
                    }
                    else {
                        dsgn += ' </div></div>';
                    }
                    dsgn += ' <div class="col-md-3 col-sm-3"> ' + ' <div class="VA009-right-part"><span class="vis vis-edit" data-UID="' + data.paymentdata[i].C_InvoicePaySchedule_ID + '" data-InvoiceID="' + data.paymentdata[i].C_Invoice_ID + '" data-TransactionType ="' + data.paymentdata[i].TransactionType
                        + '" data-IsHoldPayment ="' + data.paymentdata[i].IsHoldPayment + '"  alt="' + VIS.Msg.getMsg("VA009_Edit") + '" title="' + VIS.Msg.getMsg("VA009_Edit") + '"></span> <span class="VA009-info-icon vis vis-info" data-UID="' + data.paymentdata[i].C_BPartner_ID + '" alt="' + VIS.Msg.getMsg("VA009_Info") + '" title="' + VIS.Msg.getMsg("VA009_Info") + '"></span><div class="VA009-pay-amount" id=' + "VA009_ConvertedAmt_" + $self.windowNo + '_' + data.paymentdata[i].C_InvoicePaySchedule_ID + '> <span title="Amount Due">' + data.paymentdata[i].CurrencyCode + ' ' + parseFloat(data.paymentdata[i].DueAmt).toLocaleString(window.navigator.language, { minimumFractionDigits: data.paymentdata[i].precision }) + '</span><br> </div> </div> ' +
                        '</div></div></div>';


                    $divPayment.append(dsgn);
                    //$ConvertedAmt = $root.find("#VA009_ConvertedAmt_" + $self.windowNo + '_' + data.paymentdata[i].C_InvoicePaySchedule_ID);
                    if (_SameCurrency == 'N') {
                        $ConvertedAmt = $root.find("#VA009_ConvertedAmt_" + $self.windowNo + '_' + data.paymentdata[i].C_InvoicePaySchedule_ID);
                        $ConvertedAmt.append('<span class="VA009-color-gray" title="Amount Due">' + data.paymentdata[i].BaseCurrencyCode + ' ' + parseFloat(data.paymentdata[i].convertedAmt).toLocaleString(window.navigator.language, { minimumFractionDigits: data.paymentdata[i].precision }) + '</span> ');
                    }
                }

                if ($selectall.is(":checked")) {
                    $selectall.prop('checked', false);
                    $selectall.trigger("click");
                }
            }

            //if banks and accounts found
            if (data.bankdetails.length > 0) {
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
                var _prvbankid = 0; var _prvCurrencyCode = ""; var bnkdiv;
                var TotalAll = 0, TotalUnreAll = 0;
                var TotalAmtBank = [], BankCurrCode = [], UnReconsiledAmtTotal = [];
                var $divpanel;
                var index;
                var $divbnknew; var colorclass = 'pull-right VA009-color-green', ULcolorclass = 'pull-right VA009-color-green', Ulcolor = 'VA009-color-green';
                var $divaccordion = $('<div class="VA009-accordion"></div>');
                for (var j in data.bankdetails) {
                    if (data.bankdetails[j].CurrentBalance < 0)
                        colorclass = 'pull-right VA009-color-red';
                    if (data.bankdetails[j].UnreconsiledAmt < 0)
                        ULcolorclass = 'pull-right VA009-color-red';
                    if (_prvCurrencyCode == data.bankdetails[j].CurrencyCode1) {
                        $divbnknew = $divBank.find("#VA009_bankdtl_" + data.bankdetails[j].CurrencyCode1);
                        //bnkdiv = '<p class="VA009-data-top"><span class="pull-head"> ' + data.bankdetails[j].BankName + ' ' + data.bankdetails[j].BankAccountNumber + '</span> <span class="pull-left"> ' + VIS.Msg.getMsg("VA009_Reconciled") + ' </span> <span class= "' + colorclass + '">' + data.bankdetails[j].CurrencyCode1 + ' ' + Globalize.format(data.bankdetails[j].CurrentBalance, "N") + '</span> </p>'
                        //+ '<p class="VA009-data-bot">  <span class="pull-left">' + VIS.Msg.getMsg("VA009_Unreconciled") + '</span>  <span class="' + ULcolorclass + '">' + data.bankdetails[j].CurrencyCode1 + ' ' + Globalize.format(data.bankdetails[j].UnreconsiledAmt, "N") + '</span>   </p>';
                        bnkdiv = '<span class="pull-head"> ' + data.bankdetails[j].BankName + ' ' + data.bankdetails[j].BankAccountNumber + '</span> <p style="margin-bottom: 0;">' + VIS.Msg.getMsg("VA009_Reconciled") + ' <span class="' + colorclass + '">' + data.bankdetails[j].CurrencyCode1 + ' ' + parseFloat(data.bankdetails[j].CurrentBalance).toLocaleString() + '</span></p> <p>' + VIS.Msg.getMsg("VA009_Unreconciled") + ' <a class="' + ULcolorclass + '">' + data.bankdetails[j].CurrencyCode1 + ' ' + parseFloat(data.bankdetails[j].UnreconsiledAmt).toLocaleString() + '</a></p>';
                        $divbnknew.append(bnkdiv);
                        TotalAll = TotalAll + data.bankdetails[j].CurrentBalance;
                        TotalUnreAll = TotalUnreAll + data.bankdetails[j].UnreconsiledAmt;
                    }
                    else {
                        $divpanel = '<div class="panel-group" id="accordion_' + data.bankdetails[j].CurrencyCode1 + '" role="tablist" aria-multiselectable="true">'
                            + '<div class="panel panel-default">'
                            + '<div class="panel-heading" role="tab" id="headingOne_' + data.bankdetails[j].CurrencyCode1 + '">'
                            + '<h4 class="panel-title">'
                            + '<a role="button" data-toggle="collapse" data-parent="#accordion_' + data.bankdetails[j].CurrencyCode1 + '" href="#collapseOne_' + data.bankdetails[j].CurrencyCode1 + '" aria-expanded="true" aria-controls="collapseOne" class="VA009-Accordion-head" id=VA009_TotalAmtCurr_' + data.bankdetails[j].CurrencyCode1 + '>'
                            + '</a>'
                            + '</h4>'
                            + '</div>'
                            + '<div id="collapseOne_' + data.bankdetails[j].CurrencyCode1 + '" class="panel-collapse collapse" role="tabpanel" aria-labelledby="headingOne" style="height: auto;">'
                            + '<div class="panel-body" id=VA009_bankdtl_' + data.bankdetails[j].CurrencyCode1 + '>'
                            + '<span class="pull-head"> ' + data.bankdetails[j].BankName + ' ' + data.bankdetails[j].BankAccountNumber + '</span>'
                            + '<p style="margin-bottom: 0;">' + VIS.Msg.getMsg("VA009_Reconciled") + ' <span class="' + colorclass + '">' + data.bankdetails[j].CurrencyCode1 + ' ' + parseFloat(data.bankdetails[j].CurrentBalance).toLocaleString() + '</span></p> <p>' + VIS.Msg.getMsg("VA009_Unreconciled") + ' <a class="' + ULcolorclass + '">' + data.bankdetails[j].CurrencyCode1 + ' ' + parseFloat(data.bankdetails[j].UnreconsiledAmt).toLocaleString() + '</a></p>'
                            //+ '<p class="VA009-data-top">  <span class="pull-head"> ' + data.bankdetails[j].BankName + ' ' + data.bankdetails[j].BankAccountNumber + '</span><span class="pull-left">' + VIS.Msg.getMsg("VA009_Reconciled") + '  </span> <span class="' + colorclass + '">' + data.bankdetails[j].CurrencyCode1 + ' ' + Globalize.format(data.bankdetails[j].CurrentBalance, "N") + '</span> </p>'
                            // + '<p class="VA009-data-bot">  <span class="pull-left">' + VIS.Msg.getMsg("VA009_Unreconciled") + '</span>  <span class="' + ULcolorclass + '">' + data.bankdetails[j].CurrencyCode1 + ' ' + Globalize.format(data.bankdetails[j].UnreconsiledAmt, "N") + '</span>   </p> </div>';
                            + ' </div></div>'
                            + '</div> '
                            + '</div>';

                        //$divBank.append(bnkdiv);
                        $divaccordion.append($divpanel);
                        $divBank.append($divaccordion);
                        TotalAll += data.bankdetails[j].CurrentBalance;
                        TotalUnreAll += data.bankdetails[j].UnreconsiledAmt;
                    }
                    if (BankCurrCode.indexOf(data.bankdetails[j].CurrencyCode1) == -1) {
                        BankCurrCode.push(data.bankdetails[j].CurrencyCode1);
                        index = BankCurrCode.indexOf(data.bankdetails[j].CurrencyCode1);
                        TotalAmtBank[index] = 0; UnReconsiledAmtTotal[index] = 0;
                    }
                    else {
                        index = BankCurrCode.indexOf(data.bankdetails[j].CurrencyCode1);
                    }
                    TotalAmtBank[index] = parseFloat(TotalAmtBank[index]) + parseFloat(data.bankdetails[j].CurrentBalance);
                    UnReconsiledAmtTotal[index] = parseFloat(UnReconsiledAmtTotal[index]) + parseFloat(data.bankdetails[j].UnreconsiledAmt);
                    _prvbankid = data.bankdetails[j].C_Bank_ID;
                    _prvCurrencyCode = data.bankdetails[j].CurrencyCode1;
                    colorclass = 'pull-right VA009-color-green';
                    ULcolorclass = 'pull-right VA009-color-green';
                }
                var $divttlAmtCurr;
                for (var k in BankCurrCode) {
                    $divttlAmtCurr = $divBank.find("#VA009_TotalAmtCurr_" + BankCurrCode[k]);
                    if (TotalAmtBank[k] < 0) {
                        colorclass = 'VA009-color-red';
                    }
                    else {
                        colorclass = 'VA009-color-green';
                    }

                    if (UnReconsiledAmtTotal[k] < 0)
                        Ulcolor = 'VA009-color-red';
                    else
                        Ulcolor = 'VA009-color-green';

                    $divttlAmtCurr.append('<div class="VA009-panel-head-total"> <div><span class="VA009-head-currency-name">' + BankCurrCode[k] + '</span><span><i class="glyphicon glyphicon-chevron-down pull-right"></i></span></div><p>' + VIS.Msg.getMsg("VA009_Reconciled") + ' <span class="' + colorclass + '">' + BankCurrCode[k] + ' ' + parseFloat(TotalAmtBank[k]).toLocaleString() + ' </span></p> <p>' + VIS.Msg.getMsg("VA009_Unreconciled") + ' <a class="VA009-color-green">' + BankCurrCode[k] + ' ' + parseFloat(UnReconsiledAmtTotal[k]).toLocaleString() + '</a></p></div>');
                }
                $divBank.append('</div></div>');

            }
            else {
                $divBank.find('.VA009-right-data-main').remove();
                $divBank.find('.VA009-accordion').remove();
            }
            //if cashbooks found
            if (data.Cbk.length > 0) {
                $divBank.append(' <div class="VA009-right-data-main" style="display: none;" id=' + "VA009_cashbkdiv_" + $self.windowNo + '> </div>');
                $divcashbk = $root.find("#VA009_cashbkdiv_" + $self.windowNo);
                $divcashbk.append(' <h5>' + VIS.Msg.getMsg("VA009_Cashbook") + '</h5> ');
                for (var K in data.Cbk) {
                    if (data.Cbk[K].Csb_Amt < 0)
                        colorclass = 'pull-right VA009-color-red';
                    $divcashbk.append('<div class="VA009-right-data">  <p class="VA009-data-top">  <span class="pull-left"> ' + data.Cbk[K].CashBookName + '</span> <span class="' + colorclass + '">' + data.Cbk[K].CBCurrencyCode + ' ' + parseFloat(data.Cbk[K].Csb_Amt).toLocaleString() + '</span>  </p>  </div>');
                    colorclass = 'pull-right VA009-color-green';
                }
            }
            $BP.val("");
            if (!($selectall.is(":checked"))) {
                /*VIS_427 Devops_ID:2238  Commented in order to restrict amount 
                 to not zero  on scrolling down of design*/
                //$totalAmt.text(0);
                // $totalAmt.data('ttlamt', parseFloat(0));  
            }
            /* VIS_427 Devops_ID:2238 called the function */
            if (isReset) {
                isReset = false;
                Checkboxtrue();
            }
            $bsyDiv[0].style.visibility = "hidden";
        };
        //End 

        /* VIS_427 Devops_ID:2238 Created the function in order to mark 
        checkbox true on search and scroll*/
        function Checkboxtrue() {
            for (var i = 0; i < SlctdPaymentIds.length; i++) {
                $('.VA009-payment-list').find('div .row').find('input[data-uid=' + SlctdPaymentIds[i] + ']').prop('checked', true);
                $('.VA009-payment-wrap[data-uid=' + SlctdPaymentIds[i] + ']').addClass("VA009-payment-wrap-selctd");
            }
            for (var i = 0; i < SlctdOrderPaymentIds.length; i++) {
                $('.VA009-payment-list').find('div .row').find('input[data-uid=' + SlctdOrderPaymentIds[i] + ']').prop('checked', true);
                $('.VA009-payment-wrap[data-uid=' + SlctdOrderPaymentIds[i] + ']').addClass("VA009-payment-wrap-selctd");
            }
            for (var i = 0; i < SlctdJournalPaymentIds.length; i++) {
                $('.VA009-payment-list').find('div .row').find('input[data-uid=' + SlctdJournalPaymentIds[i] + ']').prop('checked', true);
                $('.VA009-payment-wrap[data-uid=' + SlctdJournalPaymentIds[i] + ']').addClass("VA009-payment-wrap-selctd");
            }
        }

        /**VA230:Convert search amount to dot format */
        function convertSearchAmountToDotFormat() {
            //Get decimal seperator
            var isDotSeparator = culture.isDecimalSeparatorDot(window.navigator.language);
            var txtValue = $SrchTxtBox.val();

            //format should not be dot format
            if (txtValue != '' && !isDotSeparator) {
                //search text should not contains multisearh (= operator) 
                if (!txtValue.contains("=")) {
                    if (txtValue.contains(",")) {
                        //replace , with . to search value on server side
                        txtValue = txtValue.replace(',', '.');
                    }
                }
            }
            return txtValue;
        }
        //***************************************

        //****Check For Batch Payments Cases ****//
        function validateBatchCurrency() {
            if (SlctdPaymentIds.toString() != "") {
                var isValid = false;
                var obj;
                var currency = "";
                var count = 0;
                if (batchObjInv) {
                    obj = $.grep(batchObjInv, function (e) {
                        if (count == 0) {
                            currency = e.PM.currencycode;
                            count = 1;
                        }
                        if (currency == e.PM.currencycode) {
                            isValid = true;
                        }
                        else {
                            isValid = false;
                            return e;
                        }
                    });
                }
                if (obj) {
                    if (obj.length == 0) { isValid = true; }
                    else { isValid = false; }
                }
            }
            return isValid;
        };

        function validateBatchPayments() {
            if (SlctdPaymentIds.toString() != "") {
                var isValid = false;
                var obj;
                if (batchObjInv) {
                    obj = $.grep(batchObjInv, function (e) {

                        if (e.PM.paymentrule == "E") { //EFT

                            if (("D").contains(e.PM.paymwentbasetype))// Direct Debit-D
                            {
                                if (e.PM.paymenttype == "B") {// Batch
                                    if (e.PM.paymenttriggerby == "S") { // Push By Sender
                                        if (e.PM.docbasetype == "API") {
                                            isValid = true;
                                        }
                                        else {
                                            isValid = false;
                                            return e;
                                        }
                                    }
                                    else { //  Pull by recipient
                                        if (e.PM.docbasetype == "API") {
                                            isValid = false;
                                            return e;
                                        }
                                        else {
                                            isValid = true;
                                        }
                                    }
                                }
                                else if (e.PM.paymenttype == "S") { //Single
                                    if (e.PM.paymenttriggerby == "S") { // Push By Sender
                                        if (e.PM.docbasetype == "API") {
                                            isValid = true;
                                        }
                                        else {
                                            isValid = false;
                                            return e;
                                        }
                                    }
                                    else { //  Pull by recipient
                                        if (e.PM.docbasetype == "API") {
                                            isValid = false;
                                            return e;
                                        }
                                        else {
                                            isValid = true;
                                        }
                                    }
                                }
                            }
                            else if (("W").contains(e.PM.paymwentbasetype))// WireTransfer-W
                            {
                                if (e.PM.paymenttype == "B") { // Batch
                                    if (e.PM.paymenttriggerby == "S") { // Push By Sender
                                        if (e.PM.docbasetype == "ARI") {
                                            isValid = false;
                                            return e;
                                        }
                                        else {
                                            isValid = true;
                                        }
                                    }
                                    else { //  Pull by recipient
                                        if (e.PM.docbasetype == "ARI") {
                                            isValid = true;
                                        }
                                        else {
                                            isValid = false;
                                            return e;
                                        }
                                    }
                                }
                                else if (e.PM.paymenttype == "S") {// Single
                                    if (e.PM.paymenttriggerby == "S") { // Push By Sender
                                        if (e.PM.docbasetype == "ARI") {
                                            isValid = false;
                                            return e;
                                        }
                                        else {
                                            isValid = true;
                                        }
                                    }
                                    else { //  Pull by recipient
                                        if (e.PM.docbasetype == "ARI") {
                                            isValid = true;
                                        }
                                        else {
                                            isValid = false;
                                            return e;
                                        }
                                    }
                                }
                            }
                            else {
                                isValid = false;
                                return e;
                            }

                        }
                        else { //Mannual
                            isValid = false;
                            return e;
                        }
                    });
                }
                if (obj) {
                    if (obj.length == 0) { isValid = true; }
                    else { isValid = false; }
                }
            }
            return isValid;
        };

        function validateManualPayment() {

            if (SlctdPaymentIds.toString() != "") {
                var isValid = false;
                var obj;
                if (batchObjInv) {
                    obj = $.grep(batchObjInv, function (e) {

                        if (("BCS").contains(e.PM.paymwentbasetype)) {
                            isValid = false;
                            return e;
                        }
                        else {
                            isValid = true;
                        }
                    });
                }
                if (obj) {
                    if (obj.length == 0) { isValid = true; }
                    else { isValid = false; }
                }
            }
            return isValid;
        };
        //*****************************************

        function getScheduleBatchDetails() {
            $bsyDiv[0].style.visibility = "visible";
            $.ajax({
                url: VIS.Application.contextUrl + "VA009/Payment/GetPaymentScheduleBatch",
                type: "GET",
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                async: true,
                data: ({ "FileName": "Hell", "RecordID": 1221 }),//these parameteres are not used in controller now
                success: function (result) {
                    result = JSON.parse(result);
                    batchcallback(result);
                },
                error: function () {
                    VIS.ADialog.error("VA009_ErrorLoadingPayments");
                    $bsyDiv[0].style.visibility = "hidden";
                }
            });
        };

        function batchcallback(data) {
            $bsyDiv[0].style.visibility = "hidden";
            console.log(data.result);
            console.log(data);
            if (data.result) {
                var dsgn, imgname = '';
                for (var i = 0; i < data.result.length; i++) {
                    dsgn = '<div class="VA009-payment-wrap" data-UID="' + data.result[i].DocumentNo + '"> <div class="row" data-UID="' + data.result[i].DocumentNo + '">' +
                        '<div class="col-md-4 col-sm-4 width-sm-35"> <input type="checkbox" class="VA009-clckd-checkbx" data-UID="' + data.result[i].DocumentNo + '" data-BaseAmt="' + data.paymentdata[i].BaseAmt + '"  data-NAME="' + data.result[i].DocumentNo + '" data-PaymentRule="' + data.result[i].PaymentRule + '" data-PaymentType="' + data.result[i].PaymentType + '" data-PaymentTriggerBy="' + data.result[i].PaymentTriggerBy + '" data-PaymwentBaseType="' + data.result[i].PaymwentBaseType + '" data-DocBaseType="' + data.result[i].DocBaseType + '" data-CurrencyCode="' + data.result[i].CurrencyCode + '"  alt="' + VIS.Msg.getMsg("VA009_Select") + '" title="' + VIS.Msg.getMsg("VA009_Select") + '">' +
                        '<div class="VA009-pay-left">' +
                        '<div class="VA009-pay-img-wrap">' +
                        "<span class='VA009-pay-img'><img src='" + VIS.Application.contextUrl + "Areas/VA009/Images/" + imgname + "' alt=''></span> <span class='VA009-pay-status'>" + data.result[i].VA009_ExecutionStatus + "</span> </div>" +
                        ' <div class="VA009-pay-text"><p>' + data.result[i].C_Bpartner + '</p><span>' + data.result[i].C_BP_Group + '</span> <span>' + data.result[i].DocumentNo + '</span> <span>' + data.result[i].PaymentMethod + '</span> </div>' +
                        '</div></div>'
                        + '<div class="col-md-3 col-sm-3 width-sm-30 sm-padd VA009-padd-right-0">';

                    if (data.result[i].VA009_DocumentDate != "")
                        dsgn += ' <div class="VA009-left-data VA009-pay-mid-sec"><span title="Due Date">' + data.result[i].VA009_DocumentDate + '</span> </div> ';

                    if (Globalize.format(data.result[i].VA009_RecivedAmt > 0)) {
                        dsgn += ' <div class="VA009-left-data VA009-pay-mid-sec"> <span class="VA009-color-gray" title="Invoice Amount">' + data.result[i].ISO_CODE + ' ' + Globalize.format(data.result[i].VA009_ConvertedAmt, "N") + ' </span> <br> <span class="glyphicon glyphicon-ok play-icon"></span> <span class="VA009-color-gray" title="Total Recieved Amount">' + data.result[i].CurrencyCode + ' ' + Globalize.format(data.result[i].VA009_RecivedAmt, "N") + ' </span></div> ';
                    }
                    else {
                        dsgn += ' <div class="VA009-left-data VA009-pay-mid-sec"> <span class="VA009-color-gray" title="Invoice Amount">' + data.result[i].ISO_CODE + ' ' + Globalize.format(data.result[i].VA009_ConvertedAmt, "N") + ' </span> </div> ';
                    }

                    dsgn += ' <div class="VA009-left-data VA009-pay-mid-sec"> <span data-UID="' + data.result[i].DocumentNo + '" class="VA009_AddNote" style=" cursor: pointer;"><img class="VA009_AddNoteimg" data-UID="' + data.result[i].DocumentNo + '" alt="' + VIS.Msg.getMsg("VA009_AddNote") + '" title="' + VIS.Msg.getMsg("VA009_AddNote") + '" src="' + VIS.Application.contextUrl + "Areas/VA009/Images/add-note.png" + '"> </img></span> </div> ' +
                        ' <div class="VA009-left-data VA009-pay-mid-sec" id="VA009-LastChat_' + $self.windowNo + '"> <span class="VA009-Chatcolor-gray" id=VA009-Chatcolor-gray_' + data.result[i].DocumentNo + '>' + data.result[i].LastChat + '</span> </div> ' +
                        ' </div> ' + '<div class="col-md-2 col-sm-2 width-sm-20 sm-padd VA009-padd-right-0">'
                        + '<div class="VA009-transactionType"> <span>' + data.result[i].TransactionType + '</span> '
                    if (data.result[i].DocBaseType == "APC" || data.result[i].DocBaseType == "ARC") {
                        dsgn += '<br><span style="text-decoration: underline;color: red;font-size: 12px;">Credit Memo</span> </div></div>';
                    }
                    else {
                        dsgn += ' </div></div>';
                    }
                    dsgn += ' <div class="col-md-3 col-sm-3"> ' + ' <div class="VA009-right-part"><span class="vis vis-edit" data-UID="' + data.result[i].DocumentNo + '" data-InvoiceID="' + data.result[i].C_Invoice_ID + '" data-TransactionType ="' + data.result[i].TransactionType + '" alt="' + VIS.Msg.getMsg("VA009_Edit") + '" title="' + VIS.Msg.getMsg("VA009_Edit") + '"></span> <span class="VA009-info-icon vis vis-info" data-UID="' + data.result[i].C_BPartner_ID + '" alt="' + VIS.Msg.getMsg("VA009_Info") + '" title="' + VIS.Msg.getMsg("VA009_Info") + '"></span><div class="VA009-pay-amount" id=' + "VA009_ConvertedAmt_" + $self.windowNo + '_' + data.result[i].DocumentNo + '> <span title="Amount Due">' + data.result[i].ISO_CODE + ' ' + Globalize.format(data.result[i].VA009_ConvertedAmt, "N") + '</span><br> </div> </div> ' +
                        '</div></div></div>';

                    $xmlpopGrid.append(dsgn);
                }
            }
            //else
            //    alert("bye");
        };

        //*****************
        //Zoom On Window
        //*****************
        var zoomToWindow = function (record_id, windowName, colname) {
            var ad_window_Id = 0;
            try {
                ad_window_Id = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/Payment/GetWindowID", { "WindowName": windowName }, null);
                if (ad_window_Id > 0) {
                    var zoomQuery = new VIS.Query();
                    zoomQuery.addRestriction(colname, VIS.Query.prototype.EQUAL, record_id);
                    VIS.viewManager.startWindow(ad_window_Id, zoomQuery);
                }
            }
            catch (e) {
                console.log(e);
            }
        };

        //*****************
        //Load BusyDiv
        //*****************
        function createBusyIndicator() {
            $bsyDiv = $('<div class="vis-busyindicatorouterwrap va012-busy-bank-statement" ><div class="vis-busyindicatorinnerwrap"><i class="vis-busyindicatordiv"></i></div></div>');
            //$bsyDiv.css({
            //    "position": "absolute", "width": "98%", "height": "97%", 'text-align': 'center', 'z-index': '999'
            //});
            $bsyDiv[0].style.visibility = "visible";
            $root.append($bsyDiv);
        };
        //******************
        //Final Where Clause
        //******************
        var FinalWhereQry = function (_isInv, _DocBaseType, _whereQry) {
            if (_isInv == 'Y')
                _isInv = "IS NOT NULL";
            else
                _isInv = "IS NULL";
            if (_DocBaseType == 'ARI') {
                _WhereQuery = " WHERE cs.c_invoice_id " + _isInv + " AND cs.va009_ispaid    = 'N' AND cd.docbasetype IN  ('ARI','ARC' , 'SOO') AND inv.docstatus IN ('CO','CL')";
                _WhereQuery += _whereQry;
            }
            else if (_DocBaseType == 'API') {
                _WhereQuery = " WHERE cs.c_invoice_id " + _isInv + " AND cs.va009_ispaid    = 'N' AND cd.docbasetype IN  ('API','APC' , 'POO') AND inv.docstatus IN ('CO','CL')";
                _WhereQuery += _whereQry;
            }
        };

        //LoadPayments Function to load the payments
        function loadPaymetsAll() {
            loadPaymets(_isinvoice, _DocType, pgNo, pgSize, _WhrOrg, _WhrPayMtd, _WhrStatus, _Whr_BPrtnr, $SrchTxtBox.val(), DueDateSelected, _WhrTransType, $FromDate.val(), $ToDate.val(), loadcallback);
        };
        //end
        function checkcommaordot(event, val, amt) {
            var foundComma = false;
            if (event == undefined) {
                return val;
            }
            event.value_new = VIS.Utility.Util.getValueOfString(val);
            if (event.value_new.contains(".")) {
                foundComma = true;
                var indices = [];
                for (var i = 0; i < event.value_new.length; i++) {
                    if (event.value_new[i] === ".")
                        indices.push(i);
                }
                if (indices.length > 1) {
                    event.value_new = removeAllButLast(event.value_new, '.');
                }
            }
            if (event.value_new.contains(",")) {
                if (foundComma) {
                    event.value_new = removeAllButLast(event.value_new, ',');
                }
                else {
                    var indices = [];
                    for (var i = 0; i < event.value_new.length; i++) {
                        if (event.value_new[i] === ",")
                            indices.push(i);
                    }
                    if (indices.length > 1) {
                        event.value_new = removeAllButLast(event.value_new, ',');
                    }
                    else {
                        event.value_new = event.value_new.replace(",", ".");
                    }
                }
            }
            if (event.value_new == "") {
                event.value_new = "0";
            }
            return event.value_new;
        };

        function removeAllButLast(amt, seprator) {
            var parts = amt.split(seprator);
            amt = parts.slice(0, -1).join('') + '.' + parts.slice(-1);
            if (amt.indexOf('.') == (amt.length - 1)) {
                amt = amt.replace(".", "");
            }
            return amt;
        };

        var downloadURL = function (url, no) {
            //if ($idown) {
            //    $idown.attr('src', url);
            //} else {
            var $idown;  // Keep it outside of the function, so it's initialized once.
            $idown = $('<iframe>', { id: 'idown_' + no, src: url }).hide().appendTo('body');
            //}
        };
        //*******************
        //Get Root
        //*******************
        this.getRoot = function () {
            return $root;
        };
        //********************
        //Dispose Component
        //********************
        this.disposeComponent = function () {
            $self = null;
            if ($root)
                $root.remove();
            $root = null;
            MainRoot = null;
            _leftBar = null; // Left Panel
            _MiddlePanel = null, _whereQry = null; // Middle Panel
            _RightPanel = null, $ConvertedAmt = null;
            _iscustomer = null, _isvendor = null, _isinvoice = null;
            $Org = null, $OrgSelected = null, $payMthd = null, $PayMSelected = null, $status = null, $statusSelected = null;
            $togglebtn = null, $lbdata = null, $lbmain = null, $divPayment = null, $BP = null, $BPSelected = null, $divBank = null;
            pgNo = null, pgSize = null, PAGESIZE = null, $CR_Tab = null, $CP_Tab = null, $UCR_Tab = null, $UCP_Tab = null, Pay_ID = null;
            isloaded = null, _WhereQuery = null, $divcashbk = null;
            orgids = null, bpids = null, SlctdPaymentIds = null; SlctdOrderPaymentIds = null; SlctdJournalPaymentIds = null; SelectallInvIds = null; SelectallOrdIds = null; SelectallJournalIds = null;
            paymntIds = null, statusIds = null; BP_id = null; batchObjInv = []; batchObjOrd = []; batchObjJournal = []; BusinessPartnerIds = null;
            _WhrOrg = null, _WhrPayMtd = null, _Whr_BPrtnr = null, _WhrStatus = null;
            $SelectedDiv = null, $chkicon = null, $cashicon = null, $batchicon = null, $Spliticon = null; precision = null; batchid = null;
            Batchsuccesspay = null; $ViewBatch = null; $cancel = null; $batchResult = null;
            popupgrddata = null; autocheckCtrl = null;
            CheueRecevableGrid = null, chqrecgrd = null, $SrchTxtBox = null, $btnChequePrint = null, chequePrintParams = null; chknumbers = []; removedcheck = [];
        };
        //********************
        //Set Size OF Div's
        //********************
        this.setSize = function () {
            //Set Payment Form Design, on refresh with mutiple tabs
            //$table.height($(".VA009-main-container").height());
            var container_h = $("#VA009-main-container_" + $self.windowNo).height();
            var midpanel_h = $("#VA009-middle-wrap_" + $self.windowNo).height();
            var leftpanel_h = 0;
            if (container_h == 0) {
                container_h = window.innerHeight - (40 + 43 + 24); // window height - (Header panel - Title Panel - Footer panel)
                midpanel_h = container_h - 85;
                leftpanel_h = container_h - 90;
            }
            $table.height(container_h);
            //If left Panel height is > 0 then this condtion will execute
            if (leftpanel_h <= $lbmain.height()) {
                leftpanel_h = $lbmain.height();
                midpanel_h = container_h;
            }
            //$("#VA009-content-area_" + $self.windowNo).height($("#VA009-main-container_" + $self.windowNo).height() - 20);
            $("#VA009-content-area_" + $self.windowNo).height(container_h - 20);
            //$("#VA009_Paymntlst_" + $self.windowNo).height($("#VA009-middle-wrap_" + $self.windowNo).height() - $("#VA009-mid-top-wrap_" + $self.windowNo).height() - $("#VA009-mid-search_" + $self.windowNo).height() - 42);
            $("#VA009_Paymntlst_" + $self.windowNo).height(midpanel_h - $("#VA009-mid-top-wrap_" + $self.windowNo).height() - $("#VA009-mid-search_" + $self.windowNo).height() - 42);
            //lbdata.height($lbmain.height() - (43 + 20));
            //VA230:Fixed scroll issue on left search panel
            lbdata.height(container_h - 54);
        };

        this.lockUI = function () {

        };

        this.unlockUI = function (pi) {
        };

        this.parentcall = function (output) {
            $bsyDiv[0].style.visibility = "hidden";
        }

        this.initial = function () {

        };
    };

    VA009.PaymentForm.prototype.init = function (windowNo, frame) {
        //Assign to this Varable
        this.windowNo = windowNo;
        this.frame = frame;
        this.Initialize();
        // frame.setTitle("VA009_PaymentForm");
        this.frame.getContentGrid().append(this.getRoot());
        this.setSize();

    };

    VA009.PaymentForm.prototype.dispose = function () {
        /*CleanUp Code */
        //dispose this component
        this.disposeComponent();
        //call frame dispose function
        if (this.frame)
            this.frame.dispose();
        this.frame = null;
    };

})(VA009, jQuery);
