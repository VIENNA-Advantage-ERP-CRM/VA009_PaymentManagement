; VA009 = window.VA009 || {};
; (function (VA009, $) {

    VA009.VA009_CustomerInvoices = function () {
        this.record_ID = 0;
        this.AD_Window_ID = 0;
        this.table_ID = 0;
        this.windowNo = 0;
        this.curTab = null;
        this.selectedRow = null;
        var TotalPageCount = 0;
        this.panelWidth;
        var $root;
        var $BusyIndicator;
        var $self = this;
        var ulPaging = null;
        var liFirstPage = null;
        var liPrevPage = null;
        var cmbPage = null;
        var liCurrPage = null;
        var liNextPage = null;
        var liLastPage = null;
        var divPaging = null;
        // Pagination variables
        var pageNo = 1;
        var pageSize = 10;
        var hasMoreRecords = true;
        var isLoading = false;
        var C_BPartner_ID = 0;

        // Default active state
        var currentTab = 'Outstanding';
        var divPaging = $('<div class="VA009-scheduledata-Paging mr-3">');

        this.init = function () {
            $root = $('<div class="VA009-container">' +
                '<div class="VA009-invoice-card">' +
                '<div class="tabs">' +
                '<button class="tab active">Outstanding</button>' +
                '<button class="tab">History</button>' +
                '</div>' +
                '<div class="VA009-table-header">' +
                '<div class="VA009-col-invoice-lbl">Invoice #</div>' +
                '<div class="VA009-col-amount-lbl">Amount</div>' +
                '<div class="VA009-col-due-lbl">Due Date</div>' +
                '<div class="VA009-col-status-lbl">Status</div>' +
                '<div class="VA009-col-action-lbl">Action</div>' +
                '</div>' +
                '<div id="VA009-invoiceList_' + $self.windowNo + '" class="VA009-invoice-list-wrapper"></div>' +
                '</div>' +
                '</div>');
            createPageSettings();
            $root.append(divPaging);
            busyIndicator();
            // initScroll(); // Infinite scroll setup
            bindTabClicks();
        };
        /**
  *  function used to reset the paging according to the 
  *  current page count 
  * @param {any} TotalPageCount
  */
        function resetPageCtrls(TotalPageCount) {
            cmbPage.empty();
            if (TotalPageCount > 0) {
                for (var i = 0; i < TotalPageCount; i++) {
                    cmbPage.append($("<option value=" + (i + 1) + ">" + (i + 1) + "</option>"))
                }
                cmbPage.val(pageNo);


                if (TotalPageCount > pageNo) {
                    liNextPage.css("opacity", "1");
                    liLastPage.css("opacity", "1");
                }
                else {
                    liNextPage.css("opacity", "0.6");
                    liLastPage.css("opacity", "0.6");
                }

                if (pageNo > 1) {
                    liFirstPage.css("opacity", "1");
                    liPrevPage.css("opacity", "1");
                }
                else {
                    liFirstPage.css("opacity", "0.6");
                    liPrevPage.css("opacity", "0.6");
                }

                if (TotalPageCount == 1) {
                    liFirstPage.css("opacity", "0.6");
                    liPrevPage.css("opacity", "0.6");
                    liNextPage.css("opacity", "0.6");
                    liLastPage.css("opacity", "0.6");
                }
            }
            else {
                liFirstPage.css("opacity", "0.6");
                liPrevPage.css("opacity", "0.6");
                liNextPage.css("opacity", "0.6");
                liLastPage.css("opacity", "0.6");
            }
        };
        /*function is used to create the paging div*/
        function createPageSettings() {
            ulPaging = $('<ul class="vis-statusbar-ul">');
            liFirstPage = $('<li style="opacity: 1;"><div><i class="vis vis-shiftleft" title="First Page" style="opacity: 0.6;"></i></div></li>');
            liPrevPage = $('<li style="opacity: 1;"><div><i class="vis vis-pageup" title="Page Up" style="opacity: 0.6;"></i></div></li>');
            cmbPage = $('<select>');
            liCurrPage = $('<li>').append(cmbPage);
            liNextPage = $('<li style="opacity: 1;"><div><i class="vis vis-pagedown" title="Page Down" style="opacity: 0.6;"></i></div></li>');
            liLastPage = $('<li style="opacity: 1;"><div><i class="vis vis-shiftright" title="Last Page" style="opacity: 0.6;"></i></div></li>');
            ulPaging.append(liFirstPage).append(liPrevPage).append(liCurrPage).append(liNextPage).append(liLastPage);
            divPaging.append(ulPaging);
            pageEvents();
        };
        /* function used to create click events for the paging arrows */
        function pageEvents() {
            liFirstPage.on("click", function () {
                if ($(this).css("opacity") == "1") {
                    pageNo = 1;
                    $self.getInvoiceData(false, C_BPartner_ID);
                }
            });
            liPrevPage.on("click", function () {
                if ($(this).css("opacity") == "1") {
                    pageNo--;
                    $self.getInvoiceData(false, C_BPartner_ID);
                }
            });
            liNextPage.on("click", function () {
                if ($(this).css("opacity") == "1") {
                    pageNo++;
                    $self.getInvoiceData(false, C_BPartner_ID);
                }
            });
            liLastPage.on("click", function () {
                if ($(this).css("opacity") == "1") {
                    pageNo = parseInt(cmbPage.find("Option:last").val());
                    $self.getInvoiceData(false, C_BPartner_ID);
                }
            });
            cmbPage.on("change", function () {
                pageNo = cmbPage.val();
                $self.getInvoiceData(false, C_BPartner_ID);
            });
        };
        function bindTabClicks() {
            // Tab click event
            $root.find('.tab').on('click', function () {
                var tab = $(this).text();
                if (tab !== currentTab) {
                    hideTableColumns(tab);
                    hasMoreRecords = true;
                    currentTab = tab;
                    // Switch active class on tabs
                    $root.find('.tab').removeClass('active');
                    $(this).addClass('active');
                    // Fetch data based on selected tab
                    $self.getInvoiceData(true, C_BPartner_ID); // Reset the list and fetch
                }
            });
        }
        function hideTableColumns(tab) {
            if (tab === "Outstanding") {
                $('.VA009-col-action-lbl').show();
                $('.VA009-col-invoice-lbl, .VA009-col-amount-lbl, .VA009-col-due-lbl, .VA009-col-status-lbl').css('width', '20%');
                $('.VA009-col-due-lbl').text("Due Date");

            }
            else if (tab === "History") {
                $('.VA009-col-action-lbl').hide();
                $('.VA009-col-invoice-lbl, .VA009-col-amount-lbl, .VA009-col-due-lbl, .VA009-col-status-lbl').css('width', '25%');
                $('.VA009-col-due-lbl').text("Payment")
            }
        };

        function busyIndicator() {
            $BusyIndicator = $('<div class="vis-busyindicatorouterwrap"><div class="vis-busyindicatorinnerwrap"><i class="vis-busyindicatordiv"></i></div></div>');
            $BusyIndicator[0].style.visibility = "hidden";
            $root.append($BusyIndicator);
        }

        function SetBusy(value) {
            if (value) {
                $BusyIndicator[0].style.visibility = "visible";
            } else {
                $BusyIndicator[0].style.visibility = "hidden";
            }
        }

        // Fetch invoices based on current tab
        this.getInvoiceData = function (reset = true, Record_ID) {
            /* if (!hasMoreRecords || isLoading) return;*/

            if (reset) {
                pageNo = 1;
                hasMoreRecords = true;
            }

            $root.find('#VA009-invoiceList_' + $self.windowNo).empty();
            isLoading = true;
            SetBusy(true);

            $.ajax({
                url: VIS.Application.contextUrl + "VA009/VA009_ReceivableAssesment/GetCustScheduleData",
                type: "GET",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: { cusId: Record_ID, pageNo: pageNo, pageSize: pageSize, isPaid: currentTab === 'Outstanding' ? "N" : "Y" },
                success: function (data) {

                    isLoading = false;
                    var data = JSON.parse(data);
                    if (data && data.length > 0) {
                        TotalPageCount = Math.ceil(data[0].RecordCount / pageSize);
                        $root.find('#VA009-invoiceList_' + $self.windowNo).append(buildInvoiceList(data));
                        $root.find('.VAS-scheduledata-Paging').show();
                        resetPageCtrls(TotalPageCount);
                        //if (data.length < pageSize) {
                        //    hasMoreRecords = false; // No more pages
                        //} else {
                        //    pageNo++; // Next page ready
                        //}
                    } else {
                        hasMoreRecords = false;
                        if (pageNo === 1) {
                            $root.find('#VA009-invoiceList_' + $self.windowNo).append('<div class="VA009-vas-igwidg-notfounddiv">No Invoice Data Found</div>');
                            $root.find('.VAS-scheduledata-Paging').hide();
                        }
                    }
                    SetBusy(false);
                },
                error: function (error) {
                    SetBusy(false);
                    isLoading = false;
                    console.log(error);
                }
            });
        }

        function getStatusClass(status) {
            if (status === "Over Due") return "VA009-overdue";
            if (status === "Pending") return "VA009-pending";
            if (status === "Paid") return "VA009-paid";
            return "";
        }

        function formatCurrency(amount, CurName) {
            return CurName + ' ' + '<div class="VA009-invoice-date">' + Number(amount).toLocaleString(window.navigator.language, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + '</div>';
        }

        function formatDate(dateStr) {
            var date = new Date(dateStr);
            return date.toLocaleDateString();
        }

        function buildInvoiceList(data) {
            var $container = $('<div></div>'); // temporary container

            for (var i = 0; i < data.length; i++) {
                var item = data[i];
                var statusClass = getStatusClass(item.Status);

                var $row = $(
                    '<div class="VA009-invoice-row" data-id="' + item.C_InvoicePaySchedule_ID + '" data-status="' + item.Status + '">' +
                        '<div class="VA009-col-invoice-doc">' + item.DocumentNo + '<div class="VA009-invoice-date">' + formatDate(item.DateInvoiced) + '</div>' + '</div>' +
                        '<div class="VA009-col-amount">' + formatCurrency(item.DueAmt, item.CurName) + '</div>' +
                    '<div class="VA009-col-due">' + (item.IsPaid == "Y" ? (item.PayDocumentNo + '<div class="VA009-invoice-date">' + formatDate(item.DueDate) + '</div>') : formatDate(item.DueDate)) + '</div>' +
                        '<div class="VA009-col-status">' +
                        '<div class="VA009-status-badge">' +
                        '<span class="VA009-status-dot ' + statusClass + '"></span>' +
                        '<span>' + item.Status + '</span>' +
                        '</div>' +
                        '</div>' + (item.IsPaid == "N" ?
                            ('<div class="VA009-col-action">' +
                            '<button class="VA009-icon-btn send-mail" data-doc="' + item.DocumentNo + '"><i class="vis vis-eml" title="Email" style="opacity: 1;"></i></button>' +
                            '</div>') : "") +/*📧*/
                    '</div>'
                );

                $container.append($row);
            }

            return $container.children(); // return rows only
        }

        // Send Email button
        $(document).on('click', '.send-mail', function () {
            var docNo = $(this).data('doc');
            alert('Send Email for invoice: ' + docNo);
            // TODO: AJAX call to send email
        });

        this.getRoot = function () {
            return $root;
        };
        VA009.VA009_CustomerInvoices.prototype.startPanel = function (windowNo, curTab) {
            this.windowNo = windowNo;
            this.curTab = curTab;
            this.table_ID = curTab.getAD_Table_ID();
            this.AD_Window_ID = curTab.getAD_Window_ID();
            this.init();
        };

        VA009.VA009_CustomerInvoices.prototype.refreshPanelData = function (recordID, selectedRow) {
            this.record_ID = recordID;
            C_BPartner_ID = recordID
            pageNo = 1;
            pageSize = 10;
            hasMoreRecords = true;
            isLoading = false;
            this.selectedRow = selectedRow;
            this.getInvoiceData(true, recordID);
        };

        VA009.VA009_CustomerInvoices.prototype.sizeChanged = function (width) {
            this.panelWidth = 50;
        };

        VA009.VA009_CustomerInvoices.prototype.dispose = function () {
            this.record_ID = 0;
            this.table_ID = 0;
            this.AD_Window_ID = 0;
            this.windowNo = 0;
            this.curTab = null;
            this.panelWidth = null;
            outputData = null;
        }

    };


})(VA009, jQuery);
