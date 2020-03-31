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
    VA009.DownloadDATFile = function () {
        // Varriables
        this.frame;
        this.windowNo;
        var $bsyDiv;
        var $self = this; //scoped self pointer
        var $root = $('<div class="VA009-assign-content">');
        var _mainDiv;
        var $DownloadDATFile;
        var recordID = 0;
        var docNo = "";
        var batchWindow = false;
        this.setRecordID = function (RecordID) {
            recordID = RecordID;
        };

        this.Initialize = function () {
            LoadDesign();
            GetControls();
            InitializeEvents();
            //createBusyIndicator();
        };

        function LoadDesign() {
            _mainDiv = '<div style="padding: 10px; " class="VA009-tabs-wrap"> <ul class="VA009-right-tabs"> <li class="VA009-active-tab" id = ' + "VA009_DownlaodXML_" + $self.windowNo + '>Generate Payment File</li></ul> </div>';
            $root.append(_mainDiv);
        };

        function GetControls() {
            $DownloadDATFile = $root.find("#VA009_DownlaodXML_" + $self.windowNo);
        };

        function InitializeEvents() {
            $DownloadDATFile.on("touchstart click", function (e) {
                getDocumentNumber(recordID);
            });
        };

        function prepareDataForPaymentFile(DocNumber, isBatch) {
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
                },
                error: function () {
                    VIS.ADialog.error("VA009_ErrorLoadingPayments");
                }
            });
        };

        function getDocumentNumber(RecordID) {
            if (VIS.context.getContext($self.windowNo, "WindowName") == "Payment") {
                batchWindow = false;
            }
            else {
                batchWindow = true;
            }

            $.ajax({
                url: VIS.Application.contextUrl + "VA009/DownloadPaymentFIle/GetDocuentNumber",
                type: "GET",
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                async: true,
                data: ({ "RecordID": RecordID, "isBatch": batchWindow }),
                success: function (result) {
                    result = JSON.parse(result);
                    docNo = result;
                    prepareDataForPaymentFile(docNo, batchWindow);
                },
                error: function () {
                    VIS.ADialog.error("VA009_ErrorLoadingPayments");
                }
            });
        };

        var downloadURL = function (url, no) {
            //if ($idown) {
            //    $idown.attr('src', url);
            //} else {
            var $idown;  // Keep it outside of the function, so it's initialized once.
            $idown = $('<iframe>', { id: 'idown_' + no, src: url }).hide().appendTo('body');
            //}
        };

        this.getRoot = function () {
            return $root;
        };

    };

    VA009.DownloadDATFile.prototype.init = function (windowNo, frame) {
        //Assign to this Varable
        this.windowNo = windowNo;
        this.frame = frame;
        this.Initialize();
        this.frame.getContentGrid().append(this.getRoot());
        //this.setSize();

    };

})(VA009, jQuery);