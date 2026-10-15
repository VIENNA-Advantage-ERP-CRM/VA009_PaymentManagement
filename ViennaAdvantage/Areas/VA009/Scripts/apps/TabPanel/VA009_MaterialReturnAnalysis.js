; VA009 = window.VA009 || {};
; (function (VA009, $) {

    VA009.VA009_MaterialReturnAnalysis = function () {
        this.record_ID = 0;
        this.AD_Window_ID = 0;
        this.table_ID = 0;
        this.windowNo = 0;
        this.curTab = null;
        this.selectedRow = null;
        this.panelWidth;
        var $root;
        var $self = this;
        var thread_ID = "";
        var currChatID = -1;
        var currQuestionID = -1;
        var outputData = null;
        var name = [];
        var depot = [];
        var popup = null;
        var selectedDepot = "";
        var selectedName = "";
        var IsApplyButtonClicked = false;
        var IsDepClickedAfterApply = false;
        var msg = "Regenerate and please call the tool for function analyze_sales_cn_dn_register";
        let stackedBarChart, productPieChart, valueCompareChart, regionBarChart, monthlyChart, vendorAnalysisChart;
        const fmt = (v) => {
            if (v === null || v === undefined) return '-';
            if (Math.abs(v) >= 100000)
                return '₹ ' + (v / 1000).toLocaleString('en-US', {
                    minimumFractionDigits: 0,
                    maximumFractionDigits: 0
                }) + ' k';
            return '₹ ' + Number(v).toLocaleString('en-US', {
                minimumFractionDigits: 0,
                maximumFractionDigits: 0
            });
        }
        const pct = (v) => (v === null || v === undefined) ? '-' : (Number(v).toFixed(0) + '%');

        this.init = function () {
            $root = $('<div class="container">' +
                '<div class="content">' +
                '<div class="card chart-section">' +
                '<div class="chart-header">' +
                '<div class="percentage-display">' +
                '<p class="percentage-label">Return percentage based on Sale</p>' +
                '<p class="percentage-value" id=' + "overall-return-perct" + $self.windowNo + '>0%</p>' +
                '</div>' +
                '<div class="tab-buttons">' +
                '<button class="tab-btn active">Customer</button>' +
                '<button class="tab-btn btn-depot">Depot</button>' +
                /* '<button class="tab-btn" style = "Visibility:hidden"><i class="fa fa-filter vas-exinvd-filterIcon"></i></button>' +*/
                /*'<button class="tab-btn">Sales Rep</button>' +*/
                '</div>' +
                "<div class='vas-exinvd-filter dropdown'>" +
                "<div class='vas-exinvd-icondiv'>" +
                "<span class='vas-exinvd-filterspn btn d-flex position-relative' " +
                "id='vas_exinvd_dropdownMenu_" + $self.windowNo + "' " +
                "title='Depot wise details'>" +
                "<i class='fa fa-filter vas-exinvd-filterIcon'></i>" +
                "</span>" +
                "</div>" +
                "<div class='vas-exinvd-filterPopupWrap' id='vas_exinvd_FilterPopupWrap_" +
                $self.windowNo + "' style='display:none;'></div>" +
                "</div>" +
                '</div>' +
                '<div id=' + "barChart" + $self.windowNo + ' class="bar-chart"></div>' +
                '<div id=' + "legend" + $self.windowNo + ' class="legend-compact"></div>' +
                '</div>' +
                '<div class="card metrics-header">' +
                '<p class="metrics-header-text">Key Metrics</p>' +
                '</div>' +
                '<div class="metrics-grid">' +
                '<div class="metric-card bg-yellow">' +
                '<p class="metric-label">Avg. Return Timing</p>' +
                '<p class="metric-value" id = ' + "avReturnTiming" + $self.windowNo + '>0 days</p>' +
                '</div>' +
                '<div class="metric-card bg-pink">' +
                '<p class="metric-label">Quick Returns (< 7 days)</p>' +
                '<p class="metric-value" id = ' + "QuickReturnTiming" + $self.windowNo + '>0</p>' +
                '</div>' +
                '<div class="metric-card bg-green">' +
                '<p class="metric-label">Return Frequency / month</p>' +
                '<p class="metric-value" id = ' + "ReturnFrequency" + $self.windowNo + '>0</p>' +
                '</div>' +
                '<div class="metric-card bg-pink">' +
                '<p class="metric-label">Total Return Value</p>' +
                '<p class="metric-value" id = ' + "ReturnValue" + $self.windowNo + '>₹ 0</p>' +
                '</div>' +
                '</div>' +
                '<div class="analysis-grid">' +
                '<div class="" id = ' + "product-analysis-piecard" + $self.windowNo + '>' +
                '</div>' +
                '<div class="" id = ' + "returnvalue-compared-barcard" + $self.windowNo + '>' +
                '</div>' +
                '<div class="" id = ' + "region-barcard" + $self.windowNo + '>' +
                '</div>' +
                '<div class="" id = ' + "yearly-chart-linecard" + $self.windowNo + '>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>'
            );
            busyIndicator();
            //ApplyFilterButton();
            $root.find("#vas_exinvd_dropdownMenu_" + $self.windowNo).on("click", function () {

                var popup = $root.find("#vas_exinvd_FilterPopupWrap_" + $self.windowNo);

                if (popup.is(":visible")) {
                    popup.hide();
                }
                else {
                    GetCreditNotedata();
                    popup.show();
                }
            });
            initializeTabButtons();
        };

        function ApplyFilterButton() {
            /*  if ($root.find('.vas-exinvd-filter').length === 0) {*/

            //var html =
            //    "<div class='vas-exinvd-filter dropdown'>" +
            //    "<div class='vas-exinvd-icondiv'>" +
            //    "<span class='vas-exinvd-filterspn btn d-flex position-relative' " +
            //    "id='vas_exinvd_dropdownMenu_" + $self.windowNo + "'>" +
            //    "<i class='fa fa-filter vas-exinvd-filterIcon'></i>" +
            //    "</span>" +
            //    "</div>" +
            //    "<div class='vas-exinvd-filterPopupWrap' id='vas_exinvd_FilterPopupWrap_" +
            //    $self.windowNo + "' style='display:none;'></div>" +
            //    "</div>";

            //$root.find('.tab-buttons').append($(html));

            // CLICK EVENT

            // }
        }

        function CreateDesignForFilter() {
            popup = $('<div class="vas-exinvd-filter-flyout">');
            $root.append(popup);
            popup.empty();

            //var header =
            //    "<div class='vas-filter-header'>" +
            //    "<span class='vas-filter-title'>Filter</span>" +
            //    "<span class='vas-filter-close' id='closePopup_" + $self.windowNo + "'>&times;</span>" +
            //    "</div>";
            var layout =
                "<div class='filter-row'>" +
                "<label class='vas-filter-label'>Depot</label>" +
                "<select id='ddlDepot' class='vas-filter-select'></select>" +
                "</div>";
            //"<div class='filter-row'>" +
            //"<label class='vas-filter-label'>Customer Name</label>" +
            //"<select id='ddlName' class='vas-filter-select'></select>" +
            //"</div>";
            var ApplyBtn = $('<div class="vas-flyout-footer">' +
                '<button id="VAS_Apply_' + $self.windowNo + '" class="VIS_Pref_btn-2 vas-expay-filtbtn">' + VIS.Msg.getMsg("VAS_Apply") + '</button>' +
                '</div>');
            var CloseBtn = $('<div class="vas-flyout-footer">' +
                '<button id="VAS_Close_' + $self.windowNo + '" class="VIS_Pref_btn-2 vas-expay-filtbtn va009-close">' + VIS.Msg.getMsg("VAS_Close") + '</button>' +
                '</div>'
            );
            //  popup.append(header);
            popup.append(layout);
            popup.append(ApplyBtn).append(CloseBtn);

            var $depotDD = popup.find("#ddlDepot");
            // var $nameDD = popup.find("#ddlName");

            // Populate the dropdowns with options
            for (var i = 0; i < depot.length; i++) {
                $depotDD.append("<option value='" + depot[i] + "'>" + depot[i] + "</option>");
            }
            //$nameDD.append("<option value=''></option>");
            //for (var j = 0; j < name.length; j++) {
            //    $nameDD.append("<option value='" + name[j] + "'>" + name[j] + "</option>");
            //}

            // Close popup on close button click
            CloseBtn.find("#VAS_Close_" + $self.windowNo).off("click").on("click", function () {
                popup.hide();
            });

            // Apply button click event
            ApplyBtn.find("#VAS_Apply_" + $self.windowNo).off("click").on("click", function () {
                // Get selected values
                selectedDepot = $depotDD.val();
                //selectedName = $nameDD.val();
                IsApplyButtonClicked = true;
                // Check if Depot is selected (mandatory field)
                if (!selectedDepot) {
                    VIS.ADialog.info("Depot is mandatory!");
                    return; // Do not hide popup if Depot is not selected
                }
                $self.getRecordData(selectedDepot, selectedName);
                // If validation passes, hide the popup and process the data
                popup.hide();
            });

        }


        function GetCreditNotedata() {
            SetBusy(true);
            $.ajax({
                url: VIS.Application.contextUrl + "VA009_ReceivableAssesment/GetCreditNotedata",
                type: "POST",
                data: { rec_ID: $self.record_ID },
                success: function (data) {

                    depot = [];
                    name = [];

                    if (data) {
                        data = JSON.parse(data);

                        for (var i = 0; i < data.length; i++) {
                            if (data[i].Type == "Depot" && VIS.Utility.Util.getValueOfString(data[i].Value)) {
                                depot.push(data[i].Value);
                            }
                            else {
                                name.push(data[i].Value);
                            }
                        }
                    }

                    CreateDesignForFilter();
                    SetBusy(false);
                },
                error: function () {
                    SetBusy(false);
                }
            });
        }

        function busyIndicator() {
            $BusyIndicator = $('<div class="vis-busyindicatorouterwrap"><div class="vis-busyindicatorinnerwrap"><i class="vis-busyindicatordiv"></i></div></div>');
            $BusyIndicator[0].style.visibility = "hidden";
            $root.append($BusyIndicator);
        };

        function SetBusy(value) {
            if (value) {
                $BusyIndicator[0].style.visibility = "visible";
            }
            else {
                $BusyIndicator[0].style.visibility = "hidden";
            }
        }

        this.getRecordDetail = function () {
            SetBusy(true);
            $.ajax({
                url: VIS.Application.contextUrl + "VA009_ReceivableAssesment/GetRecordDetail",
                type: "POST",
                data: { Screen_ID: $self.AD_Window_ID, Tab_ID: $self.curTab.getAD_Tab_ID(), rec_ID: $self.record_ID, tableName: $self.curTab.getTableName() },
                success: function (data) {
                    if (data != null) {
                        data = JSON.parse(data);
                        if (data.ThreadID == '' || data.ThreadID == null) {
                            clearValues();
                            VIS.ADialog.info("", null, "Thread not found", "");
                            SetBusy(false);
                        }
                        else {
                            console.log("Thread ID -- " + data.ThreadID);
                            msg = "Regenerate and please call the tool for function analyze_sales_cn_dn_register";
                            currChatID = -1;
                            currQuestionID = -1;
                            thread_ID = data.ThreadID;
                            getAIFunctionData(data.ThreadID, callbackSuccess);
                        };
                    }
                },
                error: function () {
                    SetBusy(false);
                }
            });
        };

        function getAIFunctionData(RecThreadID, callbackSuccess) {
            $.ajax({
                url: VIS.Application.contextUrl + "VAI01/VAI01_AIAssistant/ChatRequest",
                type: "POST",
                data: { Question: msg, ThreadID: RecThreadID, ChatID: currChatID, Question_ID: currQuestionID },
                success: function (data) {
                    if (data != null) {
                        data = JSON.parse(data);
                        let dataVar = JSON.parse(data);
                        currChatID = dataVar.chat_id;
                        console.log('Msg:' + msg + ' - function_calling : ' + dataVar.function_calling);
                        if (dataVar.question_id > 0) {
                            if (dataVar.function_calling) {
                                currQuestionID = dataVar.question_id;
                                var strTool = JSON.parse(dataVar.answer);
                                console.log(strTool.tool_input);
                                outputData = strTool.tool_input;
                                createCharts(strTool.tool_input);

                                /* End of thread with success*/
                                if (callbackSuccess) {
                                    SetBusy(false);
                                    msg = "Success"
                                    callbackSuccess();
                                }
                            }
                            else {
                                /* Make Busy Indicator false when function_calling is false*/
                                SetBusy(false);
                            }
                        }
                    }
                },
                error: function () {
                    setBusy(false);
                }
            });
        };

        function callbackSuccess() {
            //getAIFunctionData(thread_ID);
        };

        this.getRecordData = function (depot, customerName) {
            SetBusy(true);
            $.ajax({
                url: VIS.Application.contextUrl + "VA009_ReceivableAssesment/CustomerReturnAnalysis",
                type: "POST",
                data: { rec_ID: $self.record_ID, Depot: VIS.secureEngine.encrypt(depot), CustName: VIS.secureEngine.encrypt(customerName) },
                success: function (data) {
                    if (data) {
                        data = JSON.parse(data);
                        outputData = data;
                        console.log(outputData);
                        createCharts(data, SetBusy);
                    }
                    else {
                        SetBusy(false);
                    }
                },
                error: function () {
                    SetBusy(false);
                }
            });
        };

        function createCharts(data, callback) {
            // Overall number
            if (data.summary) {
                document.getElementById('overall-return-perct' + $self.windowNo).textContent = pct(data.summary.overall_return_percentage);
                document.getElementById('avReturnTiming' + $self.windowNo).textContent = (data.summary.average_return_timing_days || 0) + ' days';
                document.getElementById('QuickReturnTiming' + $self.windowNo).textContent = (data.summary.quick_returns_within_2_days || 0) + ' %';
            }
            if (data.timing_analysis) {
                document.getElementById('ReturnFrequency' + $self.windowNo).textContent = fmt(Math.round((data.timing_analysis.return_trend_monthly || []).reduce((s, m) => s + (m.returns || 0), 0) / 12 /*/ 1000*/) || 0);
            }
            document.getElementById('ReturnValue' + $self.windowNo).textContent = fmt((data.value_comparison.current_period_value || 0));

            // stacked bar - top customers 
            //stackedBarChartDesign(data);
            //fillStackbarLegend(data);
            if (!IsDepClickedAfterApply) {
                updateChartByCategory('Customer', data);
            }
            else {
                updateChartByCategory('Depot', data);
                IsDepClickedAfterApply = false;
            }

            // product pie
            if (data.product_analysis) {
                const pLabels = data.product_analysis.map(p => p.product);
                const pValues = data.product_analysis.map(p => p.value);
                const productPie_backgroundColor = ['#60a5fa', '#f472b6', '#34d399', '#fbbf24'];
                if (productPieChart) productPieChart.destroy();
                if (!$root.find('#product-analysis-piecard' + $self.windowNo).hasClass("product-analysis-card")) {
                    $root.find('#product-analysis-piecard' + $self.windowNo).addClass("product-analysis-card");
                }
                $root.find('#product-analysis-piecard' + $self.windowNo).empty();
                const canvas = $('<canvas></canvas>');
                $root.find('#product-analysis-piecard' + $self.windowNo).append(canvas);
                productPieChart = new Chart(canvas[0].getContext('2d'), {
                    type: 'doughnut', data: { labels: pLabels, datasets: [{ data: pValues, backgroundColor: productPie_backgroundColor }] },
                    options: {
                        responsive: true,
                        radius: '80%',
                        plugins: {
                            legend: {
                                display: true,
                                position: 'right',
                                labels: {
                                    generateLabels: function (chart) {
                                        const data = chart.data;
                                        const labels = data.labels;
                                        return labels.map((label, i) => ({
                                            text: label,
                                            fillStyle: productPie_backgroundColor[i] || '#000000',
                                            strokeStyle: 'transparent',
                                            lineWidth: 0
                                        }));
                                    },
                                    boxWidth: 10
                                },
                            },
                            title: {
                                display: true,
                                text: 'Product Level Analysis',
                                font: {
                                    size: 14,
                                    weight: 'bold'
                                },
                                padding: {
                                    top: 10,
                                    bottom: 20
                                },
                                color: '#333',
                                align: 'start'
                            },
                            tooltip: {
                                enabled: true,
                                mode: 'index',
                                callbacks: {
                                    label: function (tooltipItem) {
                                        const dataIndex = tooltipItem.dataIndex;
                                        const datasetIndex = tooltipItem.datasetIndex;
                                        const dataset = tooltipItem.chart.data.datasets[datasetIndex];
                                        const labels = tooltipItem.chart.data.labels;
                                        const value = dataset.data[dataIndex];
                                        return labels[dataIndex] + ': ' + fmt(value);
                                    }
                                }
                            },
                        },
                        maintainAspectRatio: false
                    }
                });
            }

            // value compare horizontal
            if (data.value_comparison) {
                if (valueCompareChart) valueCompareChart.destroy();
                if (!$root.find('#returnvalue-compared-barcard' + $self.windowNo).hasClass("returnvalue-compared-card")) {
                    $root.find('#returnvalue-compared-barcard' + $self.windowNo).addClass("returnvalue-compared-card");
                }
                $root.find('#returnvalue-compared-barcard' + $self.windowNo).empty();
                const canvasvalCompBarChart = $('<canvas></canvas>');
                //const dynamicHeight1 = 2 * 32 + 100; // 40px per bar + 100px for padding
                //canvasvalCompBarChart.attr('height', dynamicHeight1);
                $root.find('#returnvalue-compared-barcard' + $self.windowNo).append(canvasvalCompBarChart);
                valueCompareChart = new Chart(canvasvalCompBarChart[0].getContext('2d'),
                    {
                        type: 'bar',
                        data: {
                            labels: ['Previous', 'Current'],
                            datasets: [{
                                label: 'Return Value',
                                data: [data.value_comparison.previous_period_value, data.value_comparison.current_period_value],
                                backgroundColor: ['#93c5fd', '#fb7185'],
                                barPercentage: 1,       // width of each bar (0 to 1)
                            }]
                        },
                        options: {
                            indexAxis: 'y',
                            plugins: {
                                title: {
                                    display: true,
                                    text: 'Return Value Compared',
                                    font: {
                                        size: 14,
                                        weight: 'bold'
                                    },
                                    padding: {
                                        top: 10,
                                        bottom: 20
                                    },
                                    color: '#333',
                                    align: 'start'
                                },
                                legend:
                                {
                                    display: true,
                                    position: 'right',
                                    labels: {
                                        generateLabels: function (chart) {
                                            const data = chart.data;
                                            const labels = data.labels;
                                            return labels.map((label, i) => ({
                                                text: label,
                                                fillStyle: ['#93c5fd', '#fb7185'][i] || '#000000',
                                                strokeStyle: 'transparent',
                                                lineWidth: 0
                                            }));
                                        },
                                        boxWidth: 10
                                    },
                                },
                                tooltip: {
                                    enabled: true,
                                    callbacks: {
                                        label: function (context) {
                                            const value = context.parsed.x;
                                            return context.dataset.label + ': ' + value.toLocaleString('en-US', {
                                                minimumFractionDigits: 2,
                                                maximumFractionDigits: 2
                                            });
                                        }
                                    }
                                }
                            },
                            scales:
                            {
                                x:
                                {
                                    grid: {
                                        display: false
                                    },
                                    ticks:
                                    {
                                        callback: function (v) {
                                            return fmt(v);
                                        }
                                    }
                                },
                                y: {
                                    grid: {
                                        display: false
                                    }
                                }
                            },
                            maintainAspectRatio: false
                        }
                    });
            }

            if (data.region_analysis) {
                // region bars (horizontal stacked style)
                const rLabels = data.region_analysis.map(r => r.region);
                const rValues = data.region_analysis.map(r => r.value);
                if (regionBarChart) regionBarChart.destroy();
                if (!$root.find('#region-barcard' + $self.windowNo).hasClass("region-card")) {
                    $root.find('#region-barcard' + $self.windowNo).addClass("region-card");
                }
                $root.find('#region-barcard' + $self.windowNo).empty();
                const canvasdepotBarChart = $('<canvas></canvas>');
                const dynamicHeight = rLabels.length * 40 + 100; // 40px per bar + 100px for padding
                canvasdepotBarChart.attr('height', dynamicHeight);
                $root.find('#region-barcard' + $self.windowNo).append(canvasdepotBarChart);
                regionBarChart = new Chart(canvasdepotBarChart[0].getContext('2d'),
                    {
                        type: 'bar',
                        data:
                        {
                            labels: rLabels,
                            datasets: [
                                {
                                    label: 'Return Value',
                                    data: rValues,
                                    backgroundColor: ['#6ee7b7', '#fca5a5', '#93c5fd', '#fbbf24']
                                }
                            ]
                        },
                        options:
                        {
                            indexAxis: 'y',
                            plugins:
                            {
                                title: {
                                    display: true,
                                    text: 'Depot/Region Returns',
                                    font: {
                                        size: 14,
                                        weight: 'bold'
                                    },
                                    padding: {
                                        top: 10,
                                        bottom: 20
                                    },
                                    color: '#333',
                                    align: 'start'
                                },
                                legend:
                                {
                                    display: true,
                                    position: 'right',
                                    labels: {
                                        generateLabels: function (chart) {
                                            const data = chart.data;
                                            const labels = data.labels;
                                            return labels.map((label, i) => ({
                                                text: label,
                                                fillStyle: ['#6ee7b7', '#fca5a5', '#93c5fd', '#fbbf24'][i] || '#000000',
                                                strokeStyle: 'transparent',
                                                lineWidth: 0
                                            }));
                                        },
                                        boxWidth: 10
                                    },
                                },
                                tooltip: {
                                    enabled: true,
                                    callbacks: {
                                        label: function (context) {
                                            const value = context.parsed.x;
                                            return context.dataset.label + ': ' + value.toLocaleString('en-US', {
                                                minimumFractionDigits: 2,
                                                maximumFractionDigits: 2
                                            });
                                        }
                                    }
                                }
                            }, scales:
                            {
                                x:
                                {
                                    //grid: {
                                    //    display: false
                                    //},
                                    ticks:
                                    {
                                        callback: function (v) { return fmt(v); }
                                    }
                                },
                                y: {
                                    grid: {
                                        display: false
                                    }
                                }
                            },
                            maintainAspectRatio: false
                        }
                    });
            }

            // monthly trend
            if (data.timing_analysis) {
                const months = data.timing_analysis.return_trend_monthly.map(m => m.month);
                const sales = data.timing_analysis.return_trend_monthly.map(m => m.sales);
                const returns = data.timing_analysis.return_trend_monthly.map(m => m.returns);
                if (monthlyChart) monthlyChart.destroy();
                if (!$root.find('#yearly-chart-linecard' + $self.windowNo).hasClass("yearly-chart-card")) {
                    $root.find('#yearly-chart-linecard' + $self.windowNo).addClass("yearly-chart-card");
                }
                $root.find('#yearly-chart-linecard' + $self.windowNo).empty();
                const canvasyearlyLineChart = $('<canvas></canvas>');
                const dynamicHeightmonthtrend = 260;
                canvasyearlyLineChart.attr('height', dynamicHeightmonthtrend);
                $root.find('#yearly-chart-linecard' + $self.windowNo).append(canvasyearlyLineChart);
                monthlyChart = new Chart(canvasyearlyLineChart[0].getContext('2d'),
                    {
                        type: 'bar',
                        data:
                        {
                            labels: months,
                            datasets:
                                [
                                    {
                                        label: 'Sales',
                                        data: sales,
                                        borderColor: 'rgba(0,0,0,0)', // Transparent border color
                                        borderWidth: 0, // No border
                                        backgroundColor: 'rgb(0, 102, 204, 0.6)',
                                        tension: 0.3,
                                        fill: false,
                                        barPercentage: 0.4, // width of each bar (0 to 1)
                                    },
                                    {
                                        label: 'Returns',
                                        data: returns,
                                        borderColor: 'rgba(0,0,0,0)', // Transparent border color
                                        borderWidth: 0, // No border
                                        backgroundColor: 'rgba(204, 0, 0, 0.6)',
                                        tension: 0.3,
                                        fill: false,
                                        barPercentage: 0.4, // width of each bar (0 to 1)
                                    }
                                ]
                        },
                        options:
                        {
                            plugins:
                            {
                                title: {
                                    display: true,
                                    text: 'Period wise Sales / Returns',
                                    font: {
                                        size: 14,
                                        weight: 'bold'
                                    },
                                    padding: {
                                        top: 10,
                                        bottom: 20
                                    },
                                    color: '#333',
                                    align: 'start'
                                },
                                legend:
                                {
                                    position: 'bottom'
                                }
                            },
                            scales:
                            {
                                y:
                                {
                                    ticks:
                                    {
                                        callback: function (v) {
                                            return fmt(v);
                                        }
                                    }
                                }
                            },
                            maintainAspectRatio: false
                        }
                    });
            }

            if (callback) {
                callback(false);
            }
        }

        function stackedBarChartDesign(data) {
            $root.find('#barChart' + $self.windowNo).empty();
            const customers = data.return_breakdown.by_customer || [];
            const customersTop4 = customers.slice(0, 4);
            const totalValue = customersTop4.reduce((sum, c) => sum + c.value, 0);
            // Calculate percentages based on value
            const labels = customersTop4.map(c => c.customer);
            const values = customersTop4.map(c => (c.value / totalValue * 100).toFixed(1));
            const colors = ['#3b82f6', '#a78bfa', '#fb923c', '#9ca3af'];

            const dataValue = {
                labels: [''], // Single row
                datasets: labels.slice(0, 4).map((label, i) => ({
                    label: label,
                    data: [values[i]],
                    backgroundColor: colors[i],
                    borderWidth: 0
                }))
            };

            const config = {
                type: 'bar',
                data: dataValue,
                options: {
                    indexAxis: 'y', // Makes it horizontal
                    responsive: true,
                    maintainAspectRatio: true,
                    aspectRatio: 8,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'bottom'
                        },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    return context.dataset.label + ': ' + context.parsed.x + '%';
                                }
                            }
                        }
                    },
                    scales: {
                        x: {
                            stacked: true,
                            display: false, // Hide x-axis
                            max: 100
                        },
                        y: {
                            stacked: true,
                            display: false // Hide y-axis
                        }
                    }
                }
            }

            const canvas = $('<canvas></canvas>');
            $root.find('#barChart' + $self.windowNo).append(canvas);

            // Initialize the chart with the new data
            const ctx = canvas[0].getContext('2d');
            new Chart(ctx, config);
        };

        function fillStackbarLegend(data) {
            // legend list
            const customers = data.return_breakdown.by_customer || [];
            const colors = ['#3b82f6', '#a78bfa', '#fb923c', '#9ca3af'];

            const legendEl = document.getElementById('legend' + $self.windowNo); legendEl.innerHTML = '';
            customers.forEach((c, i) => {
                if (i >= 4) return;
                const it = document.createElement('div'); it.className = 'legend-item';
                const d = document.createElement('div'); d.className = 'dot'; d.style.background = colors[i % colors.length];
                const t = document.createElement('div'); t.innerHTML = `<div style="font-weight:700">${c.customer}</div><div class="muted">${fmt((c.value || 0))}</div>`;
                it.appendChild(d); it.appendChild(t); legendEl.appendChild(it);
            });
        }

        function updateChartByCategory(category, data) {
            const categoryConfig = {
                'Customer': { key: 'by_customer', labelField: 'customer' },
                'Depot': { key: 'by_depot', labelField: 'depot' },
                'Product': { key: 'by_product', labelField: 'product' },
                'Sales Rep': { key: 'by_sales_rep', labelField: 'sales_rep' }
            };

            const config = categoryConfig[category];
            const categoryData = data.return_breakdown[config.key] || [];

            // Update your existing functions with the category data
            createReturnBarChart(categoryData, config.labelField)
            //stackedBarChartDesignGeneric(categoryData, config.labelField);
            //fillStackbarLegendGeneric(categoryData, config.labelField);
        }

        // Modified generic version of your chart function
        function stackedBarChartDesignGeneric(items, labelField) {
            $root.find('#barChart' + $self.windowNo).empty();

            const itemsTop4 = items.slice(0, 4);
            const totalValue = itemsTop4.reduce((sum, item) => sum + item.value, 0);

            const labels = itemsTop4.map(item => item[labelField]);
            const values = itemsTop4.map(item => (item.value / totalValue * 100).toFixed(1));
            const colors = ['#3b82f6', '#a78bfa', '#fb923c', '#9ca3af'];

            const dataValue = {
                labels: [''],
                datasets: labels.map((label, i) => ({
                    label: label,
                    data: [values[i]],
                    backgroundColor: colors[i],
                    borderWidth: 0
                }))
            };

            const config = {
                type: 'bar',
                data: dataValue,
                options: {
                    indexAxis: 'y',
                    responsive: true,
                    maintainAspectRatio: false,
                    aspectRatio: 8,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'bottom',
                            labels: {

                                font: {
                                    size: 10
                                },
                                //generateLabels: function (chart) {
                                //    const labels = Chart.defaults.plugins.legend.labels.generateLabels(chart);
                                //    labels.forEach(label => {
                                //        // Truncate long text
                                //        if (label.text.length > 20) {
                                //            label.text = label.text.substring(0, 20) + '...';
                                //        }
                                //    });
                                //    return labels;
                                //}
                            }
                        },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    return context.dataset.label + ': ' + context.parsed.x + '%';
                                }
                            }
                        }
                    },
                    scales: {
                        x: {
                            stacked: true,
                            display: false,
                            max: 100
                        },
                        y: {
                            stacked: true,
                            display: false
                        }
                    }
                }
            };

            const canvas = $('<canvas></canvas>');
            const dynamicHeight = 110; // 100px for padding
            canvas.attr('height', dynamicHeight);
            $root.find('#barChart' + $self.windowNo).append(canvas);
            const ctx = canvas[0].getContext('2d');
            new Chart(ctx, config);
        }

        function fillStackbarLegendGeneric(items, labelField) {
            const colors = ['#3b82f6', '#a78bfa', '#fb923c', '#9ca3af'];
            const legendEl = document.getElementById('legend' + $self.windowNo);
            legendEl.innerHTML = '';

            items.slice(0, 4).forEach((item, i) => {
                const it = document.createElement('div');
                it.className = 'legend-item';

                const d = document.createElement('div');
                d.className = 'dot';
                d.style.background = colors[i];

                const t = document.createElement('div');
                t.innerHTML = `<div style="font-weight:700">${item[labelField]}</div><div class="muted">${fmt((item.value || 0))}</div>`;

                it.appendChild(d);
                it.appendChild(t);
                legendEl.appendChild(it);
            });
        }

        function createReturnBarChart(items, labelField) {
            const rLabels = items.map(r => r[labelField]);
            const rValues = items.map(r => r.value);

            if (vendorAnalysisChart) {
                vendorAnalysisChart.destroy();
            }
            $root.find('#barChart' + $self.windowNo).empty();
            const canvaspositiveBarChart = $('<canvas></canvas>');
            $root.find('#barChart' + $self.windowNo).append(canvaspositiveBarChart);

            vendorAnalysisChart = new Chart(canvaspositiveBarChart[0].getContext('2d'), {
                type: 'bar',
                data: {
                    labels: rLabels,
                    datasets: [{
                        label: 'Vendor Analysis',
                        data: rValues,
                        barPercentage: labelField == "depot" || selectedDepot != "" ? 0.6 : 1,
                        backgroundColor: [
                            "rgba(75, 192, 192, 0.7)",
                            "rgba(54, 162, 235, 0.7)",
                            "rgba(255, 99, 132, 0.7)",
                            "rgba(255, 159, 64, 0.7)",
                            "rgba(153, 102, 255, 0.7)"
                        ],
                        borderColor: [
                            "rgba(75, 192, 192, 1)",
                            "rgba(54, 162, 235, 1)",
                            "rgba(255, 99, 132, 1)",
                            "rgba(255, 159, 64, 1)",
                            "rgba(153, 102, 255, 1)"
                        ],
                        borderWidth: 2
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    indexAxis: 'y',
                    plugins: {
                        legend: {
                            display: false,
                            position: 'top'
                        },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    const custName = context.label;
                                    const Value = context.formattedValue
                                    return Value;
                                }
                            }
                        }
                    },
                    scales: {
                        y: {
                            //beginAtZero: true,
                            title: {
                                display: true,
                                text: selectedDepot,
                                font: {
                                    weight: 'bold'
                                },
                            },
                            ticks: {
                                callback: function (value, index) {
                                    const label = this.getLabelForValue(value);
                                    return label.length > 20 ? label.substring(0, 20) + "…" : label;
                                }
                            },
                            font: {
                                size: 10,        // font size
                                weight: 500   // font weight (normal, bold, 600, etc.)
                            },
                            grid: {
                                display: false
                            }
                        },
                        x: {
                            title: {
                                display: true,
                                text: 'Value',
                                font: {
                                    //size: 14,
                                    weight: 'bold'
                                },
                            }
                        }
                    }
                }
            });
        }

        // Initialize
        function initializeTabButtons() {
            const tabButtons = $root.find('.tab-btn');

            tabButtons.each(function () {
                $(this).on('click', function () {
                    // Remove active class from all buttons
                    tabButtons.removeClass('active');

                    // Add active class to clicked button
                    $(this).addClass('active');
                    if ($(this).text() == "Depot" && IsApplyButtonClicked) {
                        IsDepClickedAfterApply = true;
                    }
                    //else {
                    //    $root.find('.vas-exinvd-filter').remove();
                    //}
                    // Update chart
                    if (outputData && !IsApplyButtonClicked) {
                        updateChartByCategory($(this).text().trim(), outputData);
                        if (popup != null) {
                            popup.hide();
                        }
                    }
                    else {
                        $self.getRecordData("", "");
                        //setTimeout(function () {
                        //    updateChartByCategory($(this).text().trim(), outputData);
                        //}, 500);
                        IsApplyButtonClicked = false;
                        selectedName = "";
                        selectedDepot = "";
                        popup.hide();
                    }
                });
            });

        }

        this.getRoot = function () {
            return $root;
        };

    };


    VA009.VA009_MaterialReturnAnalysis.prototype.startPanel = function (windowNo, curTab) {
        this.windowNo = windowNo;
        this.curTab = curTab;
        this.table_ID = curTab.getAD_Table_ID();
        this.AD_Window_ID = curTab.getAD_Window_ID();
        this.init();
    };

    /*This function to update tab panel based on selected record*/
    VA009.VA009_MaterialReturnAnalysis.prototype.refreshPanelData = function (recordID, selectedRow) {
        this.record_ID = recordID;
        // this.getRecordDetail();
        this.getRecordData("", "");
    };

    /*
     This will set width as per window width
     */
    VA009.VA009_MaterialReturnAnalysis.prototype.sizeChanged = function (width) {
        this.panelWidth = 50;
    };

    /*
    Release all variables from memory
    */
    VA009.VA009_MaterialReturnAnalysis.prototype.dispose = function () {
        this.record_ID = 0;
        this.table_ID = 0;
        this.AD_Window_ID = 0;
        this.windowNo = 0;
        this.curTab = null;
        this.panelWidth = null;
        outputData = null;
    }
})(VA009, jQuery);