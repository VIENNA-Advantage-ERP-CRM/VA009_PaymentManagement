/************************************************************
 * Module Name    : VAS
 * Purpose        :Created Widget to get finance data insights
 * chronological  : Development
 * Created Date   : 04 October 2024
 * Created by     : VIS_427
 ***********************************************************/
; VA009 = window.VA009 || {};
; (function (VA009, $) {

    VA009.SaleInsightWidget = function () {
        this.frame;
        this.windowNo;
        this.widgetInfo;
        var $bsyDiv;
        var $self = this;
        var $root = $('<div class="h-100 w-100">');
        var divContainer = null;
        var widgetID = 0;
        var counter = 0;

        this.initalize = function () {
            widgetID = (VIS.Utility.Util.getValueOfInt(this.widgetInfo.AD_UserHomeWidgetID) != 0 ? this.widgetInfo.AD_UserHomeWidgetID : $self.windowNo);
            createBusyIndicator();
            $root.append('<div class="vas-fdi-feed-container">' +
                '<div class="vas-fdi-spaceBetween">' +
                '<h1 class="vas-fdi-widget-head">Data Insights</h1>' +
                '<div class="vas-fdi-marquee" id = "VAS_divContainer_' + widgetID + '">' +
                '<p>' +
                '</p>' +
                '</div>' +
                '</div>');

            divContainer = $root.find("#VAS_divContainer_" + widgetID);
            $root.find(".vas-fdi-refreshIco").hide();
        };

        this.intialLoad = function () {
            divContainer.find('p').empty();
            var isReturnTrx = VIS.Env.getCtx().getWindowContext($self.windowNo, "IsReturnTrx");
            var isSOTrx = VIS.Env.getCtx().getWindowContext($self.windowNo, "IsSOTrx");
            var _AD_Table_ID = VIS.Env.getCtx().getWindowTabContext($self.windowNo, 0, "AD_Table_ID");
            var tabName = VIS.Env.getCtx().getWindowTabContext($self.windowNo, 0, "Name");
            var winDisplayName = VIS.Env.getCtx().getWindowContext($self.windowNo, "WindowName");
            var _AD_Window_ID = VIS.Env.getCtx().getWindowTabContext($self.windowNo, 0, "AD_Window_ID");

            VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA009/VA009_ReceivableAssesment/GetFinInsightsData",
                { IsReturnTrx: isReturnTrx, IsSOTrx: isSOTrx, AD_Table_ID: _AD_Table_ID, TabName: tabName, WinDisplayName: winDisplayName, AD_Window_ID: _AD_Window_ID },
                function (dr) {
                InsightData = dr;
                if (InsightData.length > 0) {
                    for (i = 0; i < InsightData.length; i++) {
                        divContainer.find('p').append(' <div class="vas-fdi-req-generated">' +
                            '<div class="vas-fdi-req-count"></div>' +
                            '<a href="#" class="vas-fdi-reqGen-Txt vas-custom-link" data-name="' + InsightData[i].Name + '" data-dataobject="' + InsightData[i].DataObject + '" data-tableview="' + InsightData[i].TabelView + '" data-ad_org_id="' + InsightData[i].AD_Org_ID + '">' +
                            '<span style="font-size: 1.8em;">' + InsightData[i].Result + '</span>' + '    ' + '<span style="font-size: 1.2em;">' + ExtractText(InsightData[i].DisplayName) + '</span>' +
                            '</a>' +
                            '</div>' +
                            '</div>');
                    }
                }
                else {
                    divContainer.find('p').append('<div class="vas-fdi-notfounddiv" id="vas_norecordcont_' + widgetID + '">' + VIS.Msg.getMsg("VAS_RecordNotFound") + '</div>')
                }

                $bsyDiv[0].style.visibility = "hidden";

                divContainer.find('a').on("click", function () {
                    ++counter;
                    var insightDataView = new VAS.VAS_FinDInsightsGridView();
                    insightDataView.setProperties($(this).attr("data-tableview"), $(this).attr("data-name"), $(this).attr("data-dataobject"), counter, $(this).attr("data-ad_org_id"));
                    insightDataView.Initialize();
                });
            });
        };

        /**
         * This function is used to extract the Data between @@ values and convert it according to culture 
         * @param {any} DisplayName
         */
        function ExtractText(DisplayName) {
            /* Extracted Variable */
            var extractedText = "";

            /* get Extracted Data */
            var matches = DisplayName.match(/@(.+?)@/);
            if (matches && matches[1]) {
                /* Match[1] contain text between @@ */
                extractedText = matches[1];

                /* Convert Original Text with Culture */
                var newText = DisplayName.replace("@" + extractedText + "@", VIS.Msg.getMsg(extractedText));

                /* return Text */
                return newText
            }

            /* When Display Name dont contain text which contain @@ then return Original text */
            return DisplayName;
        };

        function createBusyIndicator() {
            $bsyDiv = $('<div class="vis-busyindicatorouterwrap"><div class="vis-busyindicatorinnerwrap"><i class="vis_widgetloader"></i></div></div>');
            $bsyDiv[0].style.visibility = "visible";
            $root.append($bsyDiv);
        };

        this.getRoot = function () {
            return $root;
        };

        this.refreshWidget = function () {
            $bsyDiv[0].style.visibility = "visible";
            $self.intialLoad();
        };
    };

    VA009.SaleInsightWidget.prototype.init = function (windowNo, frame) {
        this.frame = frame;
        this.widgetInfo = frame.widgetInfo;
        this.windowNo = windowNo;
        this.initalize();
        this.frame.getContentGrid().append(this.getRoot());
        var ssef = this;
        window.setTimeout(function () {
            ssef.intialLoad();
        }, 50);
    };

    VA009.SaleInsightWidget.prototype.widgetSizeChange = function (widget) {
        this.widgetInfo = widget;
    };

    VA009.SaleInsightWidget.prototype.refreshWidget = function () {
        this.refreshWidget();
    };

    VA009.SaleInsightWidget.prototype.dispose = function () {
        this.frame = null;
        this.windowNo = null;
        $bsyDiv = null;
        $self = null;
        $root = null;
    };


})(VA009, jQuery);