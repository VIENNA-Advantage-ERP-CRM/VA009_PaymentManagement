; VA009 = window.VA009 || {};
; (function (VA009, $) {

    VA009.VA009_DispatchAnalysis = function () {
        this.record_ID = 0;
        this.AD_Window_ID = 0;
        this.table_ID = 0;
        this.windowNo = 0;
        this.curTab = null;
        this.selectedRow = null;
        this.panelWidth;
        var $root;
        var $self = this;
        var outputData = null;
        let productPieChart, positiveChart, regionBarChart, negativeChart;
        var allRecords = [];
        var currentPage = 1;
        var pageSize = 15;
        var isLoading = false;
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
        const pct = (v) => (v === null || v === undefined) ? '-' : (Number(v).toLocaleString('en-US', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }));

        this.init = function () {
            $root = $('<div class="container">' +
                '<div class="content">' +
                '<div class="card metrics-header">' +
                '<p class="metrics-header-text">Key Metrics</p>' +
                '</div>' +
                '<div class="metrics-grid">' +
                '<div class="metric-card bg-yellow">' +
                '<p class="metric-label">Avg. Days Delay</p>' +
                '<p class="metric-value" id = ' + "avReturnTiming" + $self.windowNo + '>0 days</p>' +
                '</div>' +
                '<div class="metric-card bg-pink">' +
                '<p class="metric-label">Unbilled Amount</p>' +
                '<p class="metric-value" id = ' + "ReturnValue" + $self.windowNo + '>₹ 0</p>' +
                '</div>' +
                '<div class="metric-card bg-pink">' +
                '<p class="metric-label">Despatch, Not Invoiced</p>' +
                '<p class="metric-value" id = ' + "QuickReturnTiming" + $self.windowNo + '>0</p>' +
                '</div>' +
                '<div class="metric-card bg-green">' +
                '<p class="metric-label">Invoiced, Not Despatched</p>' +
                '<p class="metric-value" id = ' + "ReturnFrequency" + $self.windowNo + '>0</p>' +
                '</div>' +
                '</div>' +
                '<div class="card metrics-header">' +
                '<p class="metrics-header-text">Despatch Register</p>' +
                '</div>' +
                '<div class="dispatch-grid-container">' +
                '<div id="dispatchGrid' + $self.windowNo + '" style="height:250px;"></div>' +
                '</div>' +
                '<div class="analysis-grid">' +
                '<div class="" id = ' + "product-analysis-piecard" + $self.windowNo + ' style="grid-column: 1 / -1">' +
                '</div>' +
                '<div class="" id = ' + "region-barcard" + $self.windowNo + '>' +
                '</div>' +
                '<div class="" id = ' + "positive-barchart" + $self.windowNo + ' style="grid-column: 1 / -1">' +
                '</div>' +
                '<div class="" id = ' + "negative-barchart" + $self.windowNo + ' style="grid-column: 1 / -1">' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>'
            );
            busyIndicator();
        };

        function busyIndicator() {
            $BusyIndicator = $('<div class="vis-busyindicatorouterwrap"><div class="vis-busyindicatorinnerwrap"><i class="vis-busyindicatordiv"></i></div></div>');
            $BusyIndicator[0].style.visibility = "hidden";
            $root.append($BusyIndicator);
        };
        function createDispatchGrid() {

            var gridId = 'dispatchGrid' + $self.windowNo;
            var $grid = $('#' + gridId);

            if (!$grid.length) return;

            // destroy old grid if exists
            if (w2ui[gridId]) {
                w2ui[gridId].destroy();
            }

            // important: fixed height for scroll
            $grid.css({
                height: '250px',
                width: '100%',
                border: '1px solid #c0c0c0'
            });

            $grid.w2grid({
                name: gridId,
                recordHeight: 30,
                fixedBody: true,   // important for scroll
                show: {
                    columnHeaders: true,
                    toolbar: false,
                    footer: false
                },
                columns: [
                    { field: 'recid', caption: '', size: '30px', attr: 'align=center', sortable: true },
                    { field: 'Depot', caption: 'Depot', size: '120px', sortable: true },
                    { field: 'DocumentNo', caption: 'Document Number', size: '150px', sortable: true },
                    { field: 'DocumentDate', caption: 'Document Date', size: '130px', sortable: true },
                    { field: 'OriginalInvoiceNo', caption: 'Original Invoice No.', size: '170px', sortable: true },
                    { field: 'InvoiceDate', caption: 'Original Invoice Date', size: '170px', sortable: true },
                    { field: 'GatePassNo', caption: 'Gatepass No.', size: '140px', sortable: true },
                    { field: 'GPDate', caption: 'Gatepass Date', size: '140px', sortable: true },
                    { field: 'TransportationName', caption: 'Transporter Name', size: '180px', sortable: true }
                ],
                records: [],
            });

        }
        function loadGridData() {

            currentPage = 1;
            allRecords = [];

            $.ajax({
                url: VIS.Application.contextUrl + "VA009_ReceivableAssesment/GetCancelInvoiceData",
                type: "GET",
                data: { recordId: $self.record_ID },
                success: function (res) {

                    allRecords = [];

                    for (var i = 0; i < res.length; i++) {
                        allRecords.push({
                            recid: i + 1,
                            Depot: res[i].Depot,
                            DocumentNo: res[i].DocumentNo,
                            DocumentDate: res[i].DocumentDate,
                            OriginalInvoiceNo: res[i].OriginalInvoiceNo
                                ? res[i].OriginalInvoiceNo.split('(')[0].trim()
                                : '',
                            InvoiceDate: res[i].OrginalInvDate,
                            GatePassNo: res[i].GatePassNo,
                            GPDate: res[i].GPDate,
                            TransportationName: res[i].TransportationName
                        });
                    }

                    loadNextPage('dispatchGrid' + $self.windowNo);
                }
            });
        }
        function loadNextPage(gridId) {

            var grid = w2ui[gridId];

            var start = (currentPage - 1) * pageSize;
            var end = start + pageSize;

            var newRecords = allRecords.slice(start, end);

            if (newRecords.length === 0) return;

            if (currentPage === 1) {
                grid.clear();
            }

            grid.add(newRecords);
            currentPage++;

            grid.refresh();   // IMPORTANT
            setTimeout(function () {

                var scrollDiv = $('#grid_' + gridId + '_records');

                scrollDiv.off('scroll').on('scroll', function () {

                    if (isLoading) return;

                    var scrollTop = this.scrollTop;
                    var scrollHeight = this.scrollHeight;
                    var height = this.clientHeight;

                    if (scrollTop + height >= scrollHeight - 5) {
                        loadNextPage(gridId);
                    }
                });

            }, 200);
        }
        function SetBusy(value) {
            if (value) {
                $BusyIndicator[0].style.visibility = "visible";
            }
            else {
                $BusyIndicator[0].style.visibility = "hidden";
            }
        }

        this.getRecordData = function () {
            SetBusy(true);
            $.ajax({
                url: VIS.Application.contextUrl + "VA009_ReceivableAssesment/DispatchDataAnalysis",
                type: "POST",
                data: { rec_ID: $self.record_ID },
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
            document.getElementById('avReturnTiming' + $self.windowNo).textContent = (data.summary.average_dispatch_timing_days || 0) + ' days';
            document.getElementById('QuickReturnTiming' + $self.windowNo).textContent = pct(data.summary.TotalRecord || 0);
            document.getElementById('ReturnValue' + $self.windowNo).textContent = fmt((data.summary.UnbilledAmt || 0));
            if (data.InvoiceNotDispatch) {
                document.getElementById('ReturnFrequency' + $self.windowNo).textContent = pct(data.InvoiceNotDispatch.TotalRecord || 0);
            }

            // product pie
            if (data.AGING) {
                const pLabels = data.AGING.map(p => p.aging_bucket);
                const pValues = data.AGING.map(p => p.total_value);
                const productPie_backgroundColor = ['#60a5fa', '#f472b6', '#34d399', '#fbbf24', '#10b981'];
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
                                text: 'Delay in Despatching',
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

            if (data.Depot) {
                // region bars (horizontal stacked style)
                const rLabels = data.Depot.map(r => r.Name);
                const rValues = data.Depot.map(r => r.value);
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
                                    label: 'Despatched Value',
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
                                    text: 'Depot',
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

            if (data.InvQty_MisMatch) {
                createPositiveChart(data.InvQty_MisMatch);
            }

            if (data.DisQty_MisMatch) {
                createNegativeChart(data.DisQty_MisMatch);
            }

            if (callback) {
                callback(false);
            }
        }

        function createPositiveChart(data) {
            //const rLabels = data.map(d => d.invNo + "\n" + d.custName);
            if (positiveChart) {
                positiveChart.destroy();
            }
            $root.find('#positive-barchart' + $self.windowNo).empty();
            const canvaspositiveBarChart = $('<canvas></canvas>');
            //const dynamicHeight = rLabels.length * 20 + 100; // 40px per bar + 100px for padding
            //canvaspositiveBarChart.attr('height', dynamicHeight);
            $root.find('#positive-barchart' + $self.windowNo).append(canvaspositiveBarChart);

            positiveChart = new Chart(canvaspositiveBarChart[0].getContext('2d'), {
                type: 'bar',
                data: {
                    labels: data.map(d => d.invNo),
                    datasets: [{
                        label: 'Over-dispatched Quantity',
                        data: data.map(d => d.DifferenceQty),
                        backgroundColor: 'rgba(16, 185, 129, 0.7)',
                        borderColor: 'rgba(16, 185, 129, 1)',
                        borderWidth: 2
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'top'
                        },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    const item = data[context.dataIndex];
                                    return [
                                        'Customer Name: ' + item.custName,
                                        'Difference Quantity: ' + pct(item.DifferenceQty),
                                        'Invoiced Quantity: ' + pct(item.InvoicedQty),
                                        'Dispatched Quantity: ' + pct(item.DispatchedQty)];
                                }
                            }
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            title: {
                                display: true,
                                text: 'Quantity Difference'
                            }
                        },
                        x: {
                            title: {
                                display: true,
                                text: 'Invoice Number'
                            }
                        }
                    }
                }
            });
        }

        function createNegativeChart(data) {
            if (negativeChart) {
                negativeChart.destroy();
            }

            $root.find('#negative-barchart' + $self.windowNo).empty();
            const canvasnegativeBarChart = $('<canvas></canvas>');
            $root.find('#negative-barchart' + $self.windowNo).append(canvasnegativeBarChart);

            negativeChart = new Chart(canvasnegativeBarChart[0].getContext('2d'), {
                type: 'bar',
                data: {
                    labels: data.map(d => d.invNo),/*d.invNo + "\n" + d.custName*/
                    datasets: [{
                        label: 'Under-dispatched Quantity',
                        data: data.map(d => Math.abs(d.DifferenceQty)),
                        backgroundColor: 'rgba(239, 68, 68, 0.7)',
                        borderColor: 'rgba(239, 68, 68, 1)',
                        borderWidth: 2
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'top'
                        },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    const item = data[context.dataIndex];
                                    return [
                                        'Customer Name: ' + item.custName,
                                        'Difference Quantity: ' + pct(item.DifferenceQty),
                                        'Invoiced Quantity: ' + pct(item.InvoicedQty),
                                        'Dispatched Quantity: ' + pct(item.DispatchedQty)];
                                }
                            }
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            title: {
                                display: true,
                                text: 'Quantity Difference'
                            }
                        },
                        x: {
                            title: {
                                display: true,
                                text: 'Invoice Number'
                            }
                        }
                    }
                }
            });
        }

        this.getRoot = function () {

            window.setTimeout(function () {

                createDispatchGrid();
                loadGridData();

            }, 300);
            return $root;
        };

    };


    VA009.VA009_DispatchAnalysis.prototype.startPanel = function (windowNo, curTab) {
        this.windowNo = windowNo;
        this.curTab = curTab;
        this.table_ID = curTab.getAD_Table_ID();
        this.AD_Window_ID = curTab.getAD_Window_ID();
        this.init();
    };

    /*This function to update tab panel based on selected record*/
    VA009.VA009_DispatchAnalysis.prototype.refreshPanelData = function (recordID, selectedRow) {
        this.record_ID = recordID;
        this.getRecordData();
        // this.createDispatchGrid();
    };

    /*
     This will set width as per window width
     */
    VA009.VA009_DispatchAnalysis.prototype.sizeChanged = function (width) {
        this.panelWidth = 50;
    };

    /*
    Release all variables from memory
    */
    VA009.VA009_DispatchAnalysis.prototype.dispose = function () {
        this.record_ID = 0;
        this.table_ID = 0;
        this.AD_Window_ID = 0;
        this.windowNo = 0;
        this.curTab = null;
        this.panelWidth = null;
        outputData = null;
    }
})(VA009, jQuery);