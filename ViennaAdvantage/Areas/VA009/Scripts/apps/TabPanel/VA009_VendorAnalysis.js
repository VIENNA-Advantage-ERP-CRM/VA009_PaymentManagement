; VA009 = window.VA009 || {};
; (function (VA009, $) {

    VA009.VA009_VendorAnalysis = function () {
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
        var msg = "Regenerate Data based on function calculate_vendor_kpis";
        let vendorAnalysisChart, QualityComplianceRadialChart;
        const labels = [];
        const data = [];

        this.init = function () {
            $root = $('<div class="VA009-workflows-flyout" style="position: relative; right: 0; top: 0; width:100%; height: 100%;">' +
                //'<h1 class="VA009-vendor-analysis-heading">Vendor Analysis Details</h1>' +
                //'<p class="VA009_DetailedMsg"></p>' +
                '<div class="VA009-vendor-analysis-container">' +
                '<div class="VA009-OnTime-Delivery" id = ' + "VA009-OnTime-Delivery" + $self.windowNo + '></div>' +
                '<p class="VA009-OnTime-Delivery-detail" style= "margin-top: 33px !important" id = ' + "VA009-OnTime-Delivery-detail" + $self.windowNo + '></p>' +
                '</div>' +
                '<div class="VA009-vendor-analysis-container">' +
                '<div class="VA009-Quality-compliance-bar" id = ' + "VA009-Quality-compliance-bar" + $self.windowNo + '></div>' +
                '<p class="VA009-Quality-compliance-detail" style= "margin-top: 33px !important" id = ' + "VA009-Quality-compliance-detail" + $self.windowNo + '></p>' +
                '</div>' +
                '<div class="card metrics-header" style= "margin-top: 18px; font-weight: 600;">' +
                '<p class="metrics-header-text">Key Metrics</p>' +
                '</div>' +
                '<div class="metrics-grid">' +
                '<div class="metric-card bg-yellow">' +
                '<p class="metric-label">Rejection Ratio</p>' +
                '<p class="metric-value" id = ' + "vendor-RejectionRatio" + $self.windowNo + '>0</p>' +
                '</div>' +
                '<div class="metric-card bg-pink" style = "grid-column: 2 / 4">' +
                '<p class="metric-label">Dispute</p>' +
                '<p class="metric-value" id = ' + "vendor-dispute" + $self.windowNo + '>0</p>' +
                '</div>' +
                '</div>' +
                /*'<div class="VA009-VendorAnalysis" id = ' + "VA009-VendorAnalysis" + $self.windowNo + '></div>' +*/
                '</div>' +
                '</div>');
            busyIndicator();
        };

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
                            msg = "Regenerate Data based on function calculate_vendor_kpis";
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

        function clearValues() {
            //$root.find(".VA009_DetailedMsg").contents().remove();
            $root.find(".VA009-OnTime-Delivery-detail").contents().remove();
            $root.find(".VA009-Quality-compliance-detail").contents().remove();
            $root.find('#VA009-VendorAnalysis' + $self.windowNo).empty();
        }

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

                                //$root.find(".VA009_DetailedMsg").contents().remove();
                                //if (strTool.tool_input.detailed_summary) {
                                //    $root.find(".VA009_DetailedMsg").text(strTool.tool_input.detailed_summary);
                                //}

                                if (strTool.tool_input.keymetrics) {
                                    //createPositiveChart(strTool.tool_input.keymetrics);

                                    document.getElementById('vendor-RejectionRatio' + $self.windowNo).textContent = (strTool.tool_input.keymetrics.return_rejection_ratio.toFixed(2) || 0) + ' %';
                                    document.getElementById('vendor-dispute' + $self.windowNo).textContent = (strTool.tool_input.keymetrics.dispute_count_resolution_time || 0);

                                    $root.find(".VA009-OnTime-Delivery-detail").contents().remove();
                                    if (strTool.tool_input.detailed_summary_on_time_delivery_rate) {
                                        $root.find(".VA009-OnTime-Delivery-detail").text(strTool.tool_input.detailed_summary_on_time_delivery_rate);
                                    }

                                    $root.find(".VA009-Quality-compliance-detail").contents().remove();
                                    if (strTool.tool_input.detailed_summary_quality_compliance) {
                                        $root.find(".VA009-Quality-compliance-detail").text(strTool.tool_input.detailed_summary_quality_compliance);
                                    }

                                    gaugeChartImplement(strTool.tool_input.keymetrics);

                                    QualityComplianceChart(strTool.tool_input.keymetrics);
                                }

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
                    else {
                        SetBusy(false);
                    }
                },
                error: function () {
                    SetBusy(false);
                }
            });
        };

        function callbackSuccess() {
            //getAIFunctionData(thread_ID);
        }

        function prepareDataArray(vendorData) {
            labels.length = 0;
            data.length = 0;
            for (const key in vendorData) {
                labels.push(snakeToLabel(key)); // push JSON keys as labels

                if (key === "dispute_count_resolution_time") {
                    // extract numeric dispute count from "3 dispute / 3 days"
                    const disputeCount = parseInt(vendorData[key]);
                    data.push(disputeCount);
                } else {
                    data.push(vendorData[key]); // push numeric values
                }
            }
        };

        function snakeToLabel(text) {
            return text
                .replace(/_/g, " ")                     // replace "_" with space
                .replace(/\b\w/g, c => c.toUpperCase()); // capitalize first character of every word
        }

        this.getRoot = function () {
            return $root;
        };

        function createPositiveChart(res) {
            prepareDataArray(res);
            if (vendorAnalysisChart) {
                vendorAnalysisChart.destroy();
            }
            $root.find('#VA009-VendorAnalysis' + $self.windowNo).empty();
            const canvaspositiveBarChart = $('<canvas></canvas>');
            $root.find('#VA009-VendorAnalysis' + $self.windowNo).append(canvaspositiveBarChart);

            vendorAnalysisChart = new Chart(canvaspositiveBarChart[0].getContext('2d'), {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Vendor Analysis',
                        data: data,
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
                                    const item = data[context.dataIndex];
                                    return item;
                                }
                            }
                        }
                    },
                    scales: {
                        y: {
                            //beginAtZero: true,
                            //title: {
                            //    display: true,
                            //    text: 'Quantity Difference'
                            //}
                            grid: {
                                display: false
                            }
                        },
                        x: {
                            title: {
                                display: true,
                                text: 'Percentage',
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

        function gaugeChartImplement(result) {
            $root.find(".VA009-OnTime-Delivery").empty();

            // Define static labels and colors
            const backgroundColors = [
                'rgba(59, 177, 67)',   // Green
                'rgba(241, 196, 15)',   // Blue
                'rgba(231, 76, 60)'  // Red
            ];

            const labels = ["Low Risk", "Medium Risk", "High Risk"];

            const RiskAsseementData = [25, 50, 25]; /*Green (0-25) → Low risk, Yellow (25-75) → Medium risk, Red (75-100) → High risk*/

            if (result.on_time_delivery_rate.toFixed(2) > 100) {
                result.on_time_delivery_rate = 100;
            }

            // Prepare the data object for the chart
            const data = {
                datasets: [{
                    label: labels,
                    value: [result.on_time_delivery_rate.toFixed(2), 100 - result.on_time_delivery_rate.toFixed(2)],
                    /*minValue: 0,*/
                    backgroundColor: backgroundColors,
                    data: RiskAsseementData,
                    borderColor: 'rgba(0,0,0,0)', // Transparent border color
                    borderWidth: 0, // No border
                    circumference: 180,
                    rotation: 270,
                    cutout: '75%',
                    needleValue: 100 - result.on_time_delivery_rate.toFixed(2)
                }],
            };
            const plugin = {
                beforeInit: function (chart) {
                    const originalFit = chart.legend.fit;
                    chart.legend.fit = function fit() {
                        originalFit.bind(chart.legend)();
                        this.height += -10;
                    }
                }
            };

            const gaugeNeedle = {
                id: 'gaugeNeedle',
                afterDatasetsDraw: function (chart, args, plugins) {
                    const { ctx, data } = chart;

                    ctx.save();
                    const xCenter = chart.getDatasetMeta(0).data[0].x;
                    const yCenter = chart.getDatasetMeta(0).data[0].y;
                    const outerRadius = chart.getDatasetMeta(0).data[0].outerRadius;
                    const innerRadius = chart.getDatasetMeta(0).data[0].innerRadius;
                    const sliceWidth = (outerRadius - innerRadius) / 2;
                    const radius = 20;
                    const angle = Math.PI / 180;

                    const needleValue = data.datasets[0].needleValue;
                    const dataTotal = data.datasets[0].data.reduce((a, b) => a + b, 0);

                    const circumference = ((chart.getDatasetMeta(0).data[0].circumference / Math.PI) / data.datasets[0].data[0]) * needleValue;

                    ctx.translate(xCenter, yCenter);
                    ctx.rotate(Math.PI * (circumference + 1.5));

                    // needle
                    ctx.beginPath();
                    ctx.strokeStyle = 'grey';
                    ctx.fillStyle = 'grey';
                    ctx.lineWidth = 4;
                    ctx.moveTo(0 - 4, 0);/*Need width inc or decrease */
                    ctx.lineTo(0, 0 - (innerRadius + sliceWidth) * 0.8);/*Need width inc or decrease */
                    ctx.lineTo(0 + 4, 0);/*Need width inc or decrease */
                    ctx.closePath();
                    ctx.stroke();
                    ctx.fill();


                    // dot
                    ctx.beginPath();
                    //ctx.arc(0, 0, radius, 0, angle * 360, false);
                    ctx.arc(0, 0, 6, 0, Math.PI * 2); // small circle in the middle
                    ctx.fill();
                    ctx.restore();
                }
            };

            // gaugeFlowMeter plugin block
            const gaugeFlowMeter = {
                id: 'gaugeFlowMeter',
                afterDatasetsDraw: function (chart, args, plugins) {
                    const { ctx, data } = chart;

                    ctx.save();

                    const needleValue = data.datasets[0].needleValue;
                    const xCenter = chart.getDatasetMeta(0).data[0].x;
                    const yCenter = chart.getDatasetMeta(0).data[0].y;

                    const circumference = ((chart.getDatasetMeta(0).data[0].circumference / Math.PI) / data.datasets[0].data[0]) * needleValue;
                    const percentageValue = circumference * 100;

                    // label
                    ctx.font = 'bold 15px sans-serif';
                    ctx.fillStyle = 'grey';
                    ctx.textAlign = 'center';
                    ctx.fillText(result.on_time_delivery_rate.toFixed(2), xCenter, yCenter + 30);

                    ctx.font = 'bold 15px sans-serif';
                    var OnTimeDeliveryRisk = '';
                    if (result.on_time_delivery_rate.toFixed(2) < 25) {
                        ctx.fillStyle = 'rgba(59, 177, 67)';
                        OnTimeDeliveryRisk = 'High Risk';
                    }
                    else if (result.on_time_delivery_rate.toFixed(2) < 75) {
                        ctx.fillStyle = 'rgba(241, 196, 15)';
                        OnTimeDeliveryRisk = 'Medium Risk';
                    }
                    else {
                        ctx.fillStyle = 'rgba(231, 76, 60)';
                        OnTimeDeliveryRisk = 'Low Risk';
                    }
                    ctx.fillText(OnTimeDeliveryRisk, xCenter, yCenter + 50);
                }
            };

            // gaugeLabels plugin block
            const gaugeLabels = {
                id: 'gaugeLabels',
                afterDatasetsDraw: function (chart, args, plugins) {
                    const { ctx, chartArea: { left, right } } = chart;
                    const xCenter = chart.getDatasetMeta(0).data[0].x;
                    const yCenter = chart.getDatasetMeta(0).data[0].y;

                    const outerRadius = chart.getDatasetMeta(0).data[0].outerRadius;
                    const innerRadius = chart.getDatasetMeta(0).data[0].innerRadius;
                    const sliceWidth = (outerRadius - innerRadius) / 2;

                    ctx.translate(xCenter, yCenter);

                    ctx.font = 'bold 15px sans-serif';
                    ctx.fillStyle = 'black';
                    ctx.textAlign = 'center';
                    ctx.fillText(0, 0 - innerRadius - sliceWidth, 0 + 20);
                    ctx.fillText(100, 0 + innerRadius + sliceWidth, 0 + 20);
                    ctx.restore();
                }
            };

            // Define the chart configuration for Doughnut chart
            const config = {
                type: 'doughnut',
                data: data,
                options: {
                    responsive: true,
                    //cutout: '70%', // Adjust this value to decrease the outer radius
                    //needle: {
                    //    radiusPercentage: 2,
                    //    widthPercentage: 3.2,
                    //    lengthPercentage: 80,
                    //    color: 'rgba(59, 19, 111)'
                    //},
                    //valueLabel: {
                    //    display: false
                    //},
                    plugins: {
                        title: {
                            display: true,
                            text: 'On Time Delivery Rate',
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
                        legend: {
                            display: true,
                        },
                        tooltip: {
                            callbacks: {
                                label: function (tooltipItem) {
                                    const dataIndex = tooltipItem.dataIndex;
                                    const datasetIndex = tooltipItem.datasetIndex;
                                    const dataset = tooltipItem.chart.data.datasets[datasetIndex];
                                    const labels = dataset.label;
                                    const value = dataset.data[dataIndex];
                                    return labels[dataIndex] + ': ' + value;
                                }
                            }
                        },
                        datalabels: {
                            display: false,
                            color: '#000',
                            anchor: 'end',
                            align: 'end',
                            formatter: function (value) {
                                return value; // Return value for external use only
                            },
                            font: {
                                weight: 'bold'
                            }
                        }
                    },
                    rotation: -90,
                    circumference: 180,
                    layout: { padding: { top: 0, left: 0, right: 0, bottom: 15 } }
                },
                plugins: [gaugeNeedle, gaugeFlowMeter, gaugeLabels, plugin]
            };

            // Create a new canvas element and append it to the root
            const canvas = $('<canvas></canvas>');
            $root.find(".VA009-OnTime-Delivery").append(canvas);

            // Initialize the chart with the new data
            const ctx = canvas[0].getContext('2d');
            new Chart(ctx, config);
        }

        function QualityComplianceChart(result) {
            // Destroy existing chart if it exists
            if (QualityComplianceRadialChart) {
                QualityComplianceRadialChart.destroy();
            }
            $root.find('#VA009-Quality-compliance-bar' + $self.windowNo).empty();
            const canvaspositiveradialChart = $('<canvas></canvas>');
            $root.find('#VA009-Quality-compliance-bar' + $self.windowNo).append(canvaspositiveradialChart);

            // Calculate remaining percentage
            const remaining = 100 - result.quality_compliance_rate.toFixed(2);

            // Determine color based on value
            let color;
            if (result.quality_compliance_rate.toFixed(2) >= 90) {
                color = '#10b981'; // green
            } else if (result.quality_compliance_rate.toFixed(2) >= 70) {
                color = '#f59e0b'; // orange
            } else {
                color = '#ef4444'; // red
            }

            QualityComplianceRadialChart = new Chart(canvaspositiveradialChart[0].getContext('2d'), {
                type: 'doughnut',
                data: {
                    labels: ['Compliance', 'Remaining'],
                    datasets: [{
                        data: [result.quality_compliance_rate.toFixed(2), remaining],
                        backgroundColor: [color, '#e5e7eb'],
                        borderWidth: 0,
                        circumference: 360,
                        rotation: 0
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    cutout: '75%',
                    plugins: {
                        title: {
                            display: true,
                            text: 'Quality Compliance',
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
                        legend: {
                            display: false
                        },
                        tooltip: {
                            enabled: true,
                            callbacks: {
                                label: function (context) {
                                    return context.label + ': ' + context.parsed.toFixed(2) + '%';
                                }
                            }
                        }
                    },
                    animation: {
                        animateRotate: true,
                        animateScale: true
                    }
                },
                //plugins: [{
                //    id: 'centerText',
                //    beforeDraw: function (chart) {
                //        const width = chart.width;
                //        const height = chart.height;
                //        const ctx = chart.ctx;
                //        ctx.restore();

                //        const fontSize = (height / 100).toFixed(2);
                //        ctx.font = fontSize * 4 + "em sans-serif";
                //        ctx.textBaseline = "middle";
                //        ctx.fillStyle = color;

                //        const text = result.quality_compliance_rate.toFixed(2) + "%";
                //        const textX = Math.round((width - ctx.measureText(text).width) / 2);
                //        const textY = height / 2;

                //        ctx.fillText(text, textX, textY);
                //        ctx.save();
                //    }
                //}]
            });
        };
    };


    VA009.VA009_VendorAnalysis.prototype.startPanel = function (windowNo, curTab) {
        this.windowNo = windowNo;
        this.curTab = curTab;
        this.table_ID = curTab.getAD_Table_ID();
        this.AD_Window_ID = curTab.getAD_Window_ID();
        this.init();
    };

    /*This function to update tab panel based on selected record*/
    VA009.VA009_VendorAnalysis.prototype.refreshPanelData = function (recordID, selectedRow) {
        this.record_ID = recordID;
        this.getRecordDetail();
    };

    /*
     This will set width as per window width
     */
    VA009.VA009_VendorAnalysis.prototype.sizeChanged = function (width) {
        this.panelWidth = 50;
    };

    /*
    Release all variables from memory
    */
    VA009.VA009_VendorAnalysis.prototype.dispose = function () {
        this.record_ID = 0;
        this.table_ID = 0;
        this.AD_Window_ID = 0;
        this.windowNo = 0;
        this.curTab = null;
        this.panelWidth = null;
    }
})(VA009, jQuery);