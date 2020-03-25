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
    VA009.DownloadXML = function () {
        // Varriables
        this.frame;
        this.windowNo;
        var $bsyDiv;
        var $self = this; //scoped self pointer
        var $root = $('<div class="VA009-assign-content">');
        var _mainDiv;
        var $DownloadXML;
        var recordID = 0;
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
            _mainDiv = '<div style="padding: 10px; " class="VA009-tabs-wrap"> <ul class="VA009-right-tabs"> <li class="VA009-active-tab" id = ' + "VA009_DownlaodXML_" + $self.windowNo + '>Generate XML</li></ul> </div>';
            $root.append(_mainDiv);
        };

        function GetControls() {
            $DownloadXML = $root.find("#VA009_DownlaodXML_" + $self.windowNo);
        };

        function InitializeEvents() {
            $DownloadXML.on("touchstart click", function (e) {
                GenerateanddownlaodXML(callbackXML);
            });
        };

        function GenerateanddownlaodXML(callback) {
            //$bsyDiv[0].style.visibility = "visible";
            console.log("RecordID " + recordID);
            $.ajax({
                url: VIS.Application.contextUrl + "VA009/Payment/GetXMLPath",
                type: "GET",
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                //async: false,
                data: ({ "FileName": "Hell", "RecordID": recordID }),
                success: function (result) {
                    callback(result);
                },
                error: function () {
                    VIS.ADialog.error("VA009_ErrorLoadingPayments");
                }
            });
        };

        function callbackXML(result) {
            var rslt = JSON.parse(result);
            if (rslt != "") {
                var link = document.createElement('a');
                if (typeof link.download === 'string') {
                    link.href = rslt;
                    link.setAttribute('download', 'XMLFile');
                    link.name = 'abc';
                    //Firefox requires the link to be in the body
                    document.body.appendChild(link);
                    //simulate click
                    link.click();
                    //remove the link when done
                    document.body.removeChild(link);
                } else {
                    window.open(uri);
                }
            }
            //$bsyDiv[0].style.visibility = "hidden";
        };

        this.getRoot = function () {
            return $root;
        };

        function createBusyIndicator() {
            $bsyDiv = $("<div class='vis-apanel-busy'>");
            $bsyDiv.css({
                "position": "absolute", "width": "98%", "height": "97%", 'text-align': 'center', 'z-index': '999'
            });
            $bsyDiv[0].style.visibility = "visible";
            $root.append($bsyDiv);
        };

    };

    VA009.DownloadXML.prototype.init = function (windowNo, frame) {
        //Assign to this Varable
        this.windowNo = windowNo;
        this.frame = frame;
        this.Initialize();
        this.frame.getContentGrid().append(this.getRoot());
        //this.setSize();

    };

})(VA009, jQuery);