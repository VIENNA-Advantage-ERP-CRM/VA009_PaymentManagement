; VA009 = window.VA009 || {};
; (function (VA009, $) {

    VA009.VA009_ReceivableAssesment = function () {
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
        var msg = "Regenerate Get Data based on Receivable Assesment function";
        msg = "Regenerate Data based on Credit Risk Analysis";

        this.init = function () {
            $root = $('<div class="VA009-workflows-flyout" style="position: relative; right: 0; top: 0; width:100%; height: 100%;">' +
                '<div class="VA009-flyout-header">' +
                '<h1>Credit Risk Analysis</h1>' +
                '</div>' +
                '<div class="VA009-flyout-body">' +
                '<div class="VA009-payment-average">' +
                '<div class="VA009-average-graph">' +
                '</div>' +
                '<div class="VA009-content-group">' +
                '<div class="VA009-cutomer-msg VA009_CustomerMsg"><i class="fa fa-smile-o" aria-hidden="true"></i></div>' +
                '<p class="VA009_DetailedMsg"></p>' +
                '<div class="VA009-date-graph"></div>' +
                '</div>' +
                '</div>' +
                '<h1 class="VA009-sec-heading">Key Metrics</h1>' +
                '<div class="VA009-keyfeature-items">' +
                ' <div class="VA009-feature-box VA009-status-yellow">' +
                '<div class="VA009-feaature-label">Avg. Payment Delay</div>' +
                '<div class="VA009-feature-value VA009_PayDelay"></div>' +
                '</div>' +
                '<div class="VA009-feature-box VA009-status-green">' +
                '<div class="VA009-feaature-label">On-Time Payments</div>' +
                '<div class="VA009-feature-value VA009_OnTimePay"></div>' +
                '</div>' +
                '<div class="VA009-feature-box VA009-status-green">' +
                '<div class="VA009-feaature-label">Probability Pecentage</div>' +
                '<div class="VA009-feature-value VA009_Outstanding"></div>' +
                '</div>' +
                '<div class="VA009-feature-box VA009-status-red">' +
                '<div class="VA009-feaature-label">Credit Limit Usage</div>' +
                '<div class="VA009-feature-value VA009_credUsePercentage"></div>' +
                '</div>' +
                '</div>' +
                '<div class="VA009-AgingAndCLV">' +
                '<div class="VA009-InvoiceAging"></div>' +
                '<div class="VA009-CLVChart"></div>' +
                '</div>' +
                '<h1 class="VA009-sec-heading">Actions</h1>' +
                '<div class="VA009-action-items">' +
                '<div class="VA009-action-box"><img src="images/credit-report.png" alt=""> <div class="VA009-action-value">View Detailed Credit Report</div> </div>' +
                '<div class="VA009-action-box"><img src="images/payment-reminder.png" alt=""> <div class="VA009-action-value">Set Payment Reminder</div> </div>' +
                '<div class="VA009-action-box VA009-red-box"><img src="images/flag.png" alt=""> <div class="VA009-action-value">Flag for Review</div> </div>' +
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
                            msg = "Regenerate Data based on Credit Risk Analysis";
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
            $root.find(".VA009_PayDelay").contents().remove();
            $root.find(".VA009_Outstanding").contents().remove();
            $root.find(".VA009_OnTimePay").contents().remove();
            $root.find(".VA009_credUsePercentage").contents().remove();
            $root.find(".VA009_DetailedMsg").contents().remove();
            $root.find(".VA009_CustomerMsg").contents().filter(function () {
                return this.nodeType === 3;
            }).remove();
            $root.find(".VA009-average-graph").empty();
            $root.find(".VA009-date-graph").empty();
            $root.find(".VA009-InvoiceAging").empty();
            $root.find(".VA009-CLVChart").empty();
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
                                SetInputs(strTool.tool_input);
                                gaugeChartImplement(strTool.tool_input);
                                PaymentTrend(strTool.tool_input);
                                if (strTool.tool_input.invoice_aging) {
                                    InvoiceAging(strTool.tool_input);
                                }
                                if (strTool.tool_input.customer_lifetime_value) {
                                    CustomerLifeTimeValue(strTool.tool_input);
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
                },
                error: function () {
                    setBusy(false);
                }
            });
        };

        function callbackSuccess() {
            //getAIFunctionData(thread_ID);
        }

        this.getRoot = function () {
            return $root;
        };

        function gaugeChartImplement(result) {
            $root.find(".VA009-average-graph").empty();

            // Define static labels and colors
            const backgroundColors = [
                'rgba(59, 177, 67)',   // Yellow
                'rgba(241, 196, 15)',   // Blue
                'rgba(231, 76, 60)'  // Red
            ];

            const labels = ["Low Risk", "Medium Risk", "High Risk"];

            const RiskAsseementData = [25, 50, 25]; /*Green (0-25) → Low risk, Yellow (25-75) → Medium risk, Red (75-100) → High risk*/

            // Prepare the data object for the chart
            const data = {
                datasets: [{
                    label: labels,
                    value: [result.risk_score, 100 - result.risk_score],
                    /*minValue: 0,*/
                    backgroundColor: backgroundColors,
                    data: RiskAsseementData,
                    borderColor: 'rgba(0,0,0,0)', // Transparent border color
                    borderWidth: 0, // No border
                    circumference: 180,
                    rotation: 270,
                    cutout: '75%',
                    needleValue: result.risk_score
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
                afterDatasetsDraw: function(chart, args, plugins) {
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
                afterDatasetsDraw: function(chart, args, plugins) {
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
                    ctx.fillText(result.risk_score, xCenter, yCenter + 30);

                    ctx.font = 'bold 15px sans-serif';
                    if (result.risk_score < 25) {
                        ctx.fillStyle = 'rgba(59, 177, 67)';
                    }
                    else if (result.risk_score < 75) {
                        ctx.fillStyle = 'rgba(241, 196, 15)';
                    }
                    else {
                        ctx.fillStyle = 'rgba(231, 76, 60)';
                    }
                    ctx.fillText(result.risk_level, xCenter, yCenter + 50);
                }
            };

            // gaugeLabels plugin block
            const gaugeLabels = {
                id: 'gaugeLabels',
                afterDatasetsDraw: function(chart, args, plugins) {
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
            $root.find(".VA009-average-graph").append(canvas);

            // Initialize the chart with the new data
            const ctx = canvas[0].getContext('2d');
            new Chart(ctx, config);
        }

        function PaymentTrend(result) {
            $root.find(".VA009-date-graph").empty();
            const labels = result.payment_trend.map(item => item.month);
            const payScoreData = result.payment_trend.map(item => item.score);
            const data = {
                labels: labels,
                datasets: [
                    {
                        label: "Payment Trend",
                        data: payScoreData,
                        fill: false,
                        borderColor: 'rgb(75, 192, 192)',
                        tension: 0.1,
                        segment: {
                            borderColor: ctx => {
                                const current = ctx.p1.parsed.y;
                                const previous = ctx.p0.parsed.y;

                                // Color based on trend
                                if (current > previous) {
                                    return 'rgb(34, 197, 94)'; // Green for increase
                                } else if (current < previous) {
                                    return 'rgb(239, 68, 68)'; // Red for decrease
                                }
                                return 'rgb(75, 192, 192)'; // Default color
                            }
                        },
                        pointBackgroundColor: ctx => {
                            let value = 0;
                            if (ctx.parsed != undefined && ctx.parsed.y) {
                                value = ctx.parsed.y;
                            }
                            // Color based on value thresholds
                            if (value >= 20) {
                                return 'rgb(34, 197, 94)'; // Green for high values
                            } else if (value >= 10) {
                                return 'rgb(234, 179, 8)'; // Yellow for medium values
                            } else {
                                return 'rgb(239, 68, 68)'; // Red for low values
                            }
                        },
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2,
                        pointRadius: 6,
                        datalabels: {
                            backgroundColor: ctx => {
                                const value = ctx.dataset.data[ctx.dataIndex];
                                if (value >= 20) {
                                    return 'rgba(34, 197, 94, 0.2)';
                                } else if (value >= 10) {
                                    return 'rgba(234, 179, 8, 0.2)';
                                } else {
                                    return 'rgba(239, 68, 68, 0.2)';
                                }
                            },
                            color: ctx => {
                                const value = ctx.dataset.data[ctx.dataIndex];
                                if (value >= 20) {
                                    return 'rgb(34, 197, 94)';
                                } else if (value >= 10) {
                                    return 'rgb(234, 179, 8)';
                                } else {
                                    return 'rgb(239, 68, 68)';
                                }
                            },
                            borderRadius: 3,
                            padding: 6,
                            font: {
                                weight: 'bold',
                                size: 12
                            },
                            align: 'center',
                            anchor: 'end'
                        }
                    }
                ]
            };

            const config = {
                type: 'line',
                data: data,
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom' // Position the legend at the bottom
                        },
                        datalabels: {
                            display: true
                        }
                    }
                }
            };

            // Create a new canvas element and append it to the root
            const canvas = $('<canvas></canvas>');
            $root.find(".VA009-date-graph").append(canvas);

            // Initialize the chart with the new data
            const ctx = canvas[0].getContext('2d');
            new Chart(ctx, config);
        }

        function InvoiceAging(result) {
            $root.find(".VA009-InvoiceAging").empty();
            const labels = ["0-30 Days", "31-60 Days", "Above 60 Days"];
            const InvAgingData = Object.values(result.invoice_aging);
            let isAllZero = Object.values(result.invoice_aging).every(value => value === 0);
            if (!isAllZero) {
                const backgroundColors = [
                    'rgba(255, 206, 86, 0.7)',   // Yellow
                    'rgba(0, 187, 0, 0.7)',      // Green
                    'rgba(255, 105, 180, 0.7)'   // Pink
                ];

                // Prepare the data object for the chart
                const data = {
                    labels: labels, // Dynamic labels
                    datasets: [{
                        backgroundColor: backgroundColors,
                        data: InvAgingData,
                        borderColor: 'rgba(0,0,0,0)', // Transparent border color
                        borderWidth: 0 // No border
                    }],
                };

                // Define the chart configuration for Doughnut chart
                const config = {
                    type: 'doughnut',
                    data: data,
                    options: {
                        responsive: true,
                        maintainAspectRatio: false, // Makes the chart responsive
                        radius: '80%', // Adjust this value to decrease the outer radius
                        plugins: {
                            //title: {
                            //    display: true,
                            //    text: 'Invoice Aging',
                            //    font: {
                            //        size: 16,
                            //        weight: 'bold'
                            //    },
                            //    padding: {
                            //        top: 5,
                            //        bottom: 60
                            //    },
                            //    color: '#333',
                            //    position: 'bottom'
                            //},
                            title: {
                                display: true,
                                text: 'Invoice Aging',
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
                                position: 'right', // Positioning the legend on the right
                                labels: {
                                    generateLabels: function (chart) {
                                        const data = chart.data;
                                        const labels = data.labels;
                                        return labels.map((label, i) => ({
                                            text: label,
                                            fillStyle: backgroundColors[i] || '#000000',
                                            strokeStyle: 'transparent',
                                            lineWidth: 0
                                        }));
                                    },
                                    boxWidth: 10
                                },
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
                                        return labels[dataIndex] + ': ' + value;
                                    }
                                }
                            },
                        }
                    }
                };

                // Create a new canvas element and append it to the root
                const canvas = $('<canvas></canvas>');
                var invoiceAgeChart = $root.find('.VA009-InvoiceAging');
                invoiceAgeChart.append(canvas);

                // Initialize the chart with the new data
                const ctx = canvas[0].getContext('2d');
                new Chart(ctx, config);
            }
        }

        function CustomerLifeTimeValue(result) {
            $root.find(".VA009-CLVChart").empty();
            const labels = ["Customer", "Average"];
            const clvData = Object.values(result.customer_lifetime_value);
            const backgroundColors = [
                'rgba(72, 207, 173, 0.8)',   // Teal/Green for Customer
                'rgba(240, 240, 240, 0.8)'   // Light gray for Average
            ];

            // Prepare the data object for the chart
            const data = {
                labels: labels,
                datasets: [{
                    label: 'CLV',
                    backgroundColor: backgroundColors,
                    data: clvData,
                    borderColor: 'rgba(0,0,0,0)',
                    borderWidth: 0,
                    barThickness: 30,
                    borderRadius: 4
                }]
            };

            // Define the chart configuration for horizontal bar chart
            const config = {
                type: 'bar',
                data: data,
                options: {
                    indexAxis: 'y', // This makes it horizontal
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Customer Lifetime Value (CLV vs Avg)',
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
                            display: false // Hide legend since we have labels on the axis
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
                    scales: {
                        x: {
                            beginAtZero: true,
                            ticks: {
                                callback: function (value) {
                                    return (value / 1000).toFixed(2) + 'm'; // Format as "100.00m"
                                }
                            },
                            grid: {
                                display: true,
                                color: 'rgba(0, 0, 0, 0.05)'
                            }
                        },
                        y: {
                            grid: {
                                display: false
                            },
                            ticks: {
                                font: {
                                    size: 12
                                }
                            }
                        }
                    }
                }
            };

            // Create a new canvas element and append it to the root
            const canvas = $('<canvas></canvas>');
            var clvChart = $root.find('.VA009-CLVChart'); // Update with your container class
            clvChart.append(canvas);

            // Initialize the chart with the new data
            const ctx = canvas[0].getContext('2d');
            new Chart(ctx, config);
        }

        function SetInputs(result) {
            $root.find(".VA009_PayDelay").contents().remove();
            if (result.avg_payment_delay_days != undefined) {
                $root.find(".VA009_PayDelay").text(result.avg_payment_delay_days.toFixed(2) + "%");
            }

            $root.find(".VA009_Outstanding").contents().remove();
            if (result.default_probability_percent != undefined) {
                $root.find(".VA009_Outstanding").text(result.default_probability_percent.toFixed(2) + "%");
            }

            $root.find(".VA009_OnTimePay").contents().remove();
            if (result.on_time_payments_percent != undefined) {
                $root.find(".VA009_OnTimePay").text(result.on_time_payments_percent.toFixed(2) + "%");
            }

            $root.find(".VA009_credUsePercentage").contents().remove();
            if (result.credit_limit_usage_percent != undefined) {
                $root.find(".VA009_credUsePercentage").text(result.credit_limit_usage_percent.toFixed(2) + "%");
            }

            let $msgDiv = $root.find(".VA009_CustomerMsg");
            $msgDiv.contents().filter(function () {
                return this.nodeType === 3;
            }).remove();

            if (result.comment_summary) {
                $msgDiv.find("i").after(result.comment_summary);
                $msgDiv.find("i").removeClass("fa-smile-o fa-meh-o fa-frown-o");
                if (result.risk_score <= 25) {
                    $msgDiv.find("i").addClass("fa-smile-o").css("color", "rgba(59, 177, 67)"); // green
                }
                else if (result.risk_score > 25 && result.risk_score <= 75) {
                    $msgDiv.find("i").addClass("fa-meh-o").css("color", "rgba(241, 196, 15)"); // green
                }
                else {
                    $msgDiv.find("i").addClass("fa-frown-o").css("color", "rgba(231, 76, 60)"); // green
                }
            }

            $root.find(".VA009_DetailedMsg").contents().remove();
            if (result.detailed_summary) {
                $root.find(".VA009_DetailedMsg").text(result.detailed_summary);
            }

        }

    };


    VA009.VA009_ReceivableAssesment.prototype.startPanel = function (windowNo, curTab) {
        this.windowNo = windowNo;
        this.curTab = curTab;
        this.table_ID = curTab.getAD_Table_ID();
        this.AD_Window_ID = curTab.getAD_Window_ID();
        this.init();
    };

    /*This function to update tab panel based on selected record*/
    VA009.VA009_ReceivableAssesment.prototype.refreshPanelData = function (recordID, selectedRow) {
        this.record_ID = recordID;
        this.getRecordDetail();
    };

    /*
     This will set width as per window width
     */
    VA009.VA009_ReceivableAssesment.prototype.sizeChanged = function (width) {
        this.panelWidth = 50;
    };

    /*
    Release all variables from memory
    */
    VA009.VA009_ReceivableAssesment.prototype.dispose = function () {
        this.record_ID = 0;
        this.table_ID = 0;
        this.AD_Window_ID = 0;
        this.windowNo = 0;
        this.curTab = null;
        this.panelWidth = null;
    }
})(VA009, jQuery);