; VA011 = window.VA011 || {};

; (function (VA011, $) {

    //Form Class function fullnamespace
    VA011.InventoryForm = function () {

        this.frame;
        this.windowNo;

        var $self = this; //scoped self pointer
        var $root = $('<div class="vis-group-assign-content" style="height:100%">');
        var $table;
        var $td0leftbar, $btnlbToggle, $ulLefttoolbar, $divlbMain, $td2_tr1, $divSearch, $inputSearchProd, $inputSearchBtnProd; //left bar
        var $divBottomMiddle = null;
        var $divOrg = null;
        var $divWarehouse = null;
        var $divPLV = null;
        var $divSupplier = null;
        var $divCheckBoxWarehouse = null;
        var $divCheckBoxPLV = null;
        var $divCheckBoxSupplier = null;
        var $divSupplier = null;
        var $divCheckBoxOrg = null;
        var $divCheckBoxCategories = null;
        var $divProductCategories = null;
        var $lb;
        var $tr;
        var $leftPanel = $('<div style="display: inline-block;float:left;height:100%;width:200px;background:rgb(87, 87, 87);">');
        var $catPanel = null;
        var $bsyDiv = null;
        var middleMain = null;
        var $middlePanel = null;
        var $rightPanel = null;
        var $searchProduct = null;
        var $queryCat = null;
        var $searchProdBtn = null;
        var $btnCreateProd = null;
        var $divReplenish = $('<div style="display:none;"></div>'); //layout
        var $divReplenishRuleAll = $('<div class="112233" style="display:block; height:87%"></div>'); //layout
        var $divRepRuleAllTop = $('<div  style="width:auto; margin-bottom:10px;"></div>');
        var $divRepAllPop = $('<div class="11223344" style="overflow-y:auto;margin-right:-2px;margin-bottom:15px; height:100%"></div>');
        var $cmbRepAllWarehouse = $('<select class="vis-statusbar-combo" style="width:200px;"></select>');
        var chldDlgRepAll = null;
        var $divImgAdd = null;
        var $divImgSaveAll = null;
        var repProducts = [];

        var $div = $('<div style="overflow-y:auto;margin-right:-2px;margin-bottom:15px"></div>');
        var ch = null;
        var $divUom = null;
        var $divUomGroup = null;
        var divUom = null;
        var divConversion = null;
        var $divVarient = null;
        var $divCartdata = null;
        var divCart = null;
        var divNewCart = null;
        var divCartList = null;
        var $divLeftTree = null;
        var divtree = null;
        var divVarient = null;
        var divAttr = null;
        // Header Button (Tabs)
        var $divHeadVarientProdTop = null;
        var $divHeadVarientProdBottom = null;
        var $divHeadDetails = null;
        var $divImgDet = null;
        var $rightFullDetails = null;
        var $divRightSection = null;

        var txtScan = null;
        var cmbCart = null;
        var cart = 0;
        var btnNewCart = null;
        var btnEditCart = null;
        var btnRefreshCart = null;
        var btnSaveScan = null;
        var btnCancelScan = null;

        var btnZoomProduct = null;

        // Grids
        // Product
        var $divProdGrid = null;
        var dProdGrid = null;
        var grdProdValues = [];

        var $divRepTopGrid = null;
        var dRepTopGrid = null;
        var grdRepTopValues = [];

        // Bottom Div
        // Variant
        var $divVariantGrid = null;
        var dVariantGrid = null;
        var grdVariantProdValues = [];

        // Locator
        var $divLocatorGrid = null;
        var dLocatorGrid = null;
        var grdLocatorProdValues = [];

        // Ordered
        var $divOrderedGrid = null;
        var dOrderedGrid = null;
        var grdOrderedProdValues = [];

        // Replenished
        var $divReplenishedGrid = null;
        var dReplenishedGrid = null;
        var grdReplenishedProdValues = [];

        // Demand
        var $divDemandGrid = null;
        var dDemandGrid = null;
        var grdDemandProdValues = [];

        // Transactions
        var $divTransactionsGrid = null;
        var dTransactionsGrid = null;
        var grdTransactionsProdValues = [];

        // Replenishment Bottom
        var $divReplenishmentBGrid = null;
        var dReplenishmentBGrid = null;
        var grdReplenishmentBValues = [];
        // Bottom Grids

        // Right Panel Grids
        // Related
        var $divRelatedGrid = null;
        var dRelatedGrid = null;
        var grdRelatedProdValues = [];

        // Suppliers Right Panel
        var $divSuppliersRightGrid = null;
        var dSuppliersRightGrid = null;
        var grdSuppliersRightProdValues = [];

        // Kits Right Panel
        var $divKitsGrid = null;
        var dKitsGrid = null;
        var grdKitsProdValues = [];

        // Substitute
        var $divSubstituteGrid = null;
        var dSubstituteGrid = null;
        var grdSubstituteProdValues = [];

        // Right Panel Grids

        // ReplenishmentPopup Bottom
        var $divReplenishmentPopGrid = null;
        var dReplenishmentPopGrid = null;
        var grdReplenishmentPopValues = [];
        // Bottom Grids

        var searchProdText = "";
        var uomArray = [];
        var repTypeArray = [];

        var cmbRepType = null;
        var counter = 0;
        var prodCat_ID = 0;
        var selOrgs = [];
        var selWh = [];
        var selPLV = [];
        var selSupp = [];
        var selCat = [];
        var $statusProdDiv = null;
        var $ulStatusProdDiv = null;
        var AD_Column_ID = 0;
        var cmbUOM = null;
        var $cmbPageNo = $('<select class="vis-statusbar-combo" style="width:50px;"></select>');
        var $liPrev = null;
        var $liFirst = null;
        var $liNext = null;
        var $liLast = null;
        var pgNo = 1;
        var pageno = 1;
        var pgSize = 20;
        var totalRecords = 0;
        var initLoad = true;
        var execPaging = false;
        var isReplenishTypeLoaded = false;
        var cmbCreate = null;
        var cmbDocType = null;
        var cmbDocStatus = null;
        var cmbWarehouses = null;
        var cmbSuppliers = null;

        var listKeyDocType = [];
        var listNameDocType = [];
        var listValueDocType = [];

        var listSelProducts = [];

        var zoomIcon = null;
        var calculateReplenishLI = null;
        var generateReplenishLI = null;
        var addToCartLI = null;
        var divZoomProdName = null;
        var displayRepRuleAllLI = null;

        var _product_ID = 0;
        var _docStatus = "";
        var imgUsrImage = null;

        var calculateReplenishSelected = false;

        var selectedBottomTab =
            {
                Locator: "L",
                Variants: "V",
                Ordered: "O",
                Replenished: "R",
                Demand: "D",
                Transactions: "T",
                Replenishment: "P",
            };

        var bottomTabSelction = "V";

        var cartSelectionValues = [];
        var multipleRecSelected = false;

        // Right Panel Section
        var detailsSection = null;
        var substituteSection = null;
        var relatedSection = null;
        var suppliersSection = null;
        var kitsSection = null;

        var rightPanelSection = null;
        var substituteGridSection = null;
        var relatedGridSection = null;
        var suppliersGridSection = null;
        var kitsGridSection = null;

        var listWarehouses = [];
        var listProductsAll = [];
        // Right Panel Section

        this.initalize = function () {

            createBusyIndicator();
            createPanels();
            replenishPanel();
            replenishRulePanel();
            events();
            /// $root.load(VIS.Application.contextUrl + "Inventory/Index?windowno=" + $self.windowNo, function () {
            //window.setTimeout(function () {
            //    initLoad = false;
            //    imgUsrImage = $('<img style="max-height: 100%; max-width: 100%;" src=' + VIS.Application.contextUrl + 'Areas/VA011/Images/img-defult.png>');
            //    $(".VA011_image-wrap").append(imgUsrImage);
            //    btnZoomProduct = $("#VA011_ZoomProduct");

            //    btnZoomProduct.on(VIS.Events.onTouchStartOrClick, function (e) {
            //        zoomToWindow(_product_ID, "Product");
            //    });

            //    //bindProductGrid(false);
            //    LoadWarehouses();
            //}, 200);
            // });
        };

        this.initalLoad = function () {
            initLoad = false;
            window.setTimeout(function () {
                imgUsrImage = $('<img style="max-height: 100%; max-width: 100%;" src=' + VIS.Application.contextUrl + 'Areas/VA011/Images/img-defult.png>');
                $(".VA011_image-wrap").append(imgUsrImage);
                btnZoomProduct = $("#VA011_ZoomProduct");

                btnZoomProduct.on(VIS.Events.onTouchStartOrClick, function (e) {
                    zoomToWindow(_product_ID, "Product");
                });

                calculateReplenishLI = $("#VA011_calculateReplenishLI" + $self.windowNo);
                generateReplenishLI = $("#VA011_generateReplenishLI" + $self.windowNo);
                addToCartLI = $("#VA011_addtoCartLI" + $self.windowNo);
                divZoomProdName = $("#VA011_ProdDetZoomName" + $self.windowNo);
                displayRepRuleAllLI = $("#VA011_DisplayReplenishRuleLI_" + $self.windowNo);

                detailsSection = $("#VA011_btnDetails_" + $self.windowNo);
                substituteSection = $("#VA011_btnSubsti_" + $self.windowNo);
                relatedSection = $("#VA011_btnRelated_" + $self.windowNo);
                suppliersSection = $("#VA011_btnSuppliers_" + $self.windowNo);
                kitsSection = $("#VA011_btnKits_" + $self.windowNo);

                rightPanelSection = $("#VA011_RightSection" + $self.windowNo);
                substituteGridSection = $("#VA011_grdSubstitute_" + $self.windowNo);
                relatedGridSection = $("#VA011_grdRelated_" + $self.windowNo);
                suppliersGridSection = $("#VA011_grdSuppliersRight_" + $self.windowNo);
                kitsGridSection = $("#VA011_grdKits_" + $self.windowNo);

                if (calculateReplenishLI) {
                    calculateReplenishLI.css("display", "block");
                };

                if (addToCartLI) {
                    addToCartLI.css("display", "block");
                };
                if (displayRepRuleAllLI) {
                    displayRepRuleAllLI.css("display", "block");
                };

                //if (generateReplenishLI) {
                //    generateReplenishLI.css("display", "none");
                //};

                //bindProductGrid(false);
                LoadProducts();
            }, 200);
        };

        function LoadProducts() {
            VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/GetProductsAll", { "AD_Client_ID": VIS.context.getContext("#AD_Client_ID") }, LoadProductsCallBack);
        };

        function LoadProductsCallBack(dr) {
            if (dr.length > 0) {
                for (var i = 0; i < dr.length; i++) {
                    key = VIS.Utility.Util.getValueOfInt(dr[i].ID);
                    value = dr[i].Name;
                    cmbWarehouses.append(" <option value=" + key + ">" + VIS.Utility.encodeText(value) + "</option>");
                    listProductsAll.push({ id: key, text: value });
                }
            }
            LoadWarehouses();
        };

        function LoadWarehouses() {
            cmbWarehouses.empty();
            var qry = "SELECT M_Warehouse_ID, Name FROM M_Warehouse WHERE AD_Client_ID = " + VIS.context.getContext("#AD_Client_ID") + " AND IsActive = 'Y'";
            VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/GetOrgWarehouseAll", { "value": selWh, "orgs": selOrgs, "fill": false }, LoadWarehouseCallBack);
            //VIS.DB.executeReader(qry, null, LoadWarehouseCallBack);
        };

        function LoadWarehouseCallBack(dr) {

            //cmbWarehouses.append(" <option value = 0></option>");
            if (dr.length > 0) {
                for (var i = 0; i < dr.length; i++) {
                    key = VIS.Utility.Util.getValueOfInt(dr[i].ID);
                    value = dr[i].Name;
                    cmbWarehouses.append(" <option value=" + key + ">" + VIS.Utility.encodeText(value) + "</option>");
                    listWarehouses.push({ id: key, text: value });
                }
            }
            //while (dr.read()) {
            //    key = VIS.Utility.Util.getValueOfInt(dr.getString(0));
            //    value = dr.getString(1);
            //    cmbWarehouses.append(" <option value=" + key + ">" + VIS.Utility.encodeText(value) + "</option>");
            //}
            //dr.close();
            LoadSuppliers();
        };

        function LoadSuppliers() {
            cmbSuppliers.empty();
            var qry = "SELECT C_BPartner_ID, Name FROM C_BPartner WHERE AD_Client_ID = " + VIS.context.getContext("#AD_Client_ID") + " AND IsActive = 'Y' AND IsVendor = 'Y'";
            VIS.DB.executeReader(qry, null, LoadSuppliersCallBack);
        };

        function LoadSuppliersCallBack(dr) {
            cmbSuppliers.append(" <option value = 0></option>");
            while (dr.read()) {
                key = VIS.Utility.Util.getValueOfInt(dr.getString(0));
                value = dr.getString(1);
                cmbSuppliers.append(" <option value=" + key + ">" + VIS.Utility.encodeText(value) + "</option>");
            }
            dr.close();
            LoadUOM();
        };

        function loadCategories(pgNo, pgSize) {
            $.ajax({
                url: VIS.Application.contextUrl + "Inventory/GetProdCats",
                type: "GET",
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                async: false,
                data: ({ pageNo: pgNo, pageSize: pgSize }),
                success: function (result) {
                    var data = JSON.parse(result);
                    for (item in data) {
                        $ulLefttoolbar.append('<li procatid = ' + data[item].M_ProdCatID + '><span procatid = '
                        + data[item].M_ProdCatID + '>' + data[item].Catname + ' (' + data[item].ProdCount + ')</span></li>');
                    }
                },
                error: function () {
                    VIS.ADialog.error("VA011_ErrorLoadingProdCats");
                    $bsyDiv[0].style.visibility = 'hidden';
                }
            });
        };

        var self = this; //scoped self pointer

        function fillAutoCompleteonTextBoxWarehouse(text, response, fillAll, funName) {
            dProdGrid.refresh();
            dVariantGrid.refresh();
            $.ajax({
                url: VIS.Application.contextUrl + "Inventory/" + funName,
                dataType: "json",
                data: {
                    value: text,
                    orgs: selOrgs,
                    fill: fillAll
                },
                success: function (data) {

                    var result = JSON.parse(data);
                    datasource = [];
                    response($.map(result, function (item) {
                        return {
                            label: item.Name,
                            value: item.Name,
                            ids: item.ID
                        }
                    }));
                    $(self.div).autocomplete("search", "");
                    $(self.div).trigger("focus");
                }
            });
        };

        function fillAutoCompleteonTextBox(text, response, fillAll, funName) {
            dProdGrid.refresh();
            dVariantGrid.refresh();
            $.ajax({
                url: VIS.Application.contextUrl + "Inventory/" + funName,
                dataType: "json",
                data: {
                    value: text,
                    fill: fillAll
                },
                success: function (data) {

                    var result = JSON.parse(data);
                    datasource = [];
                    response($.map(result, function (item) {
                        return {
                            label: item.Name,
                            value: item.Name,
                            ids: item.ID
                        }
                    }));
                    $(self.div).autocomplete("search", "");
                    $(self.div).trigger("focus");
                }
            });
        };

        function createPanels() {

            $td0leftbar = $("<td class='VA011-Left-Bar'>");
            $lb = $("<div class='vis-apanel-lb' style='overflow:auto'>");

            $btnlbToggle = $("<div class='vis-apanel-lb-toggle' ><img class='vis-apanel-lb-img' src='" + VIS.Application.contextUrl + "Areas/VIS/Images/base/mt24.png'></div>");
            $ulLefttoolbar = $("<ul>");
            $divlbMain = $('<div class="vis-apanel-lb-main VA011-div-left-main">');

            $divSearch = $('<div class="VA011-searchDiv"></div>');
            $inputSearchProd = $('<input style="width:138px; margin:5px; float:left" placeholder="' + VIS.Msg.getMsg("Search") + '">');
            $inputSearchBtnProd = $('<input class=" vis-group-pointer vis-group-ass-btns vis-group-search-icon" title=' + VIS.Msg.getMsg("VA011_Search") + ' style="float:left; border-radius: 5px; margin-top: 5px;" type="button">');
            $divSearch.append($inputSearchProd).append($inputSearchBtnProd);
            $divlbMain.append($divSearch);

            // Append Org TextBox
            $divCheckBoxOrg = $('<div class="VA011-checkboxlist" style="color:#fff;" >');
            $divOrg = $('<input class="VA011-inputLeftSearch" placeholder="' + VIS.Msg.getMsg("Organization") + '" ><img class="VA011-imgCombo" style="display:inline;margin-top:-3px;margin-left:-15px;cursor:pointer" src="Areas/VA011/Images/open-arrow.png">');
            $divOrg.autocomplete({
                minLength: 0,
                source: function (request, response) {
                    if (request.term.trim().length == 0) {
                        return;
                    }

                    fillAutoCompleteonTextBox(request.term, response, false, "GetOrgs");
                },
                select: function (ev, ui) {
                    debugger;
                    if (isInList(ui.item.ids, selOrgs)) {
                        return;
                    }

                    if (ui.item.ids != 9999) {
                        selOrgs.push(ui.item.ids);
                    }
                    else {
                        $divCheckBoxOrg.children().remove();
                        selOrgs = selOrgs.splice();
                    }
                    //$divCheckBoxOrg.append('<div class="VA011-clsDIV"><input type="checkbox" id=VA011_chkOrg' + counter + ' checked value="' + ui.item.ids + '"><span id=VA011_spnOrg' + counter + '>' + ui.item.value + '</span></br></div>');
                    $divCheckBoxOrg.append('<div class="VA011-clsDIV"><img id=VA011_chkOrg' + counter + ' style="display:inline;margin-top:-3px;margin-left:5px;cursor:pointer" value="' + ui.item.ids + '" src="Areas/VA011/Images/cross.png"><span style="margin-left:5px" id=VA011_spnOrg' + counter + '>' + ui.item.value + '</span></br></div>');
                    counter++;
                    pgNo = 1;
                    bindProductGrid(false);
                }
            });
            $divlbMain.append($divOrg);
            $divlbMain.append($divCheckBoxOrg);
            // Append Org TextBox

            // Append Warehosue TextBox
            $divCheckBoxWarehouse = $('<div class="VA011-checkboxlist" style="color:#fff;" >');
            $divWarehouse = $('<input class="VA011-inputLeftSearch" placeholder="' + VIS.Msg.getMsg("Warehouse") + '" ><img class="VA011-imgCombo" style="display:inline;margin-top:-3px;margin-left:-15px;cursor:pointer" src="Areas/VA011/Images/open-arrow.png">');
            $divWarehouse.autocomplete({
                minLength: 0,
                source: function (request, response) {

                    if (request.term.trim().length == 0) {
                        return;
                    }

                    fillAutoCompleteonTextBoxWarehouse(request.term, response, false, "GetOrgWarehouse");
                },
                select: function (ev, ui) {
                    if (isInList(ui.item.ids, selWh)) {
                        return;
                    }
                    if (ui.item.ids != 9999) {
                        selWh.push(ui.item.ids);
                    }
                    else {
                        $divCheckBoxWarehouse.children().remove();
                        selWh = selWh.splice();
                    }
                    //$divCheckBoxWarehouse.append('<div class="VA011-clsDIV"><input type="checkbox" id=VA011_chkWh' + counter + ' checked value="' + ui.item.ids + '"><span id=VA011_spnWh' + counter + '>' + ui.item.value + '</span></br></div>');
                    $divCheckBoxWarehouse.append('<div class="VA011-clsDIV"><img id=VA011_chkWh' + counter + ' style="display:inline;margin-top:-3px;margin-left:5px;cursor:pointer" value="' + ui.item.ids + '" src="Areas/VA011/Images/cross.png"><span style="margin-left:5px" id=VA011_spnWh' + counter + '>' + ui.item.value + '</span></br></div>');
                    counter++;
                    pgNo = 1;
                    bindProductGrid(false);
                }
            });
            $divlbMain.append($divWarehouse);
            $divlbMain.append($divCheckBoxWarehouse);
            // Append Warehosue TextBox

            // Append PriceListVersion TextBox
            $divCheckBoxPLV = $('<div class="VA011-checkboxlist" style="color:#fff;" >');
            $divPLV = $('<input class="VA011-inputLeftSearch" placeholder="' + VIS.Msg.getMsg("PriceListVersion") + '" ><img class="VA011-imgCombo" style="display:inline;margin-top:-3px;margin-left:-15px;cursor:pointer"  src="Areas/VA011/Images/open-arrow.png">');
            $divPLV.autocomplete({
                minLength: 0,
                source: function (request, response) {

                    if (request.term.trim().length == 0) {
                        return;
                    }

                    fillAutoCompleteonTextBox(request.term, response, false, "GetPLV");
                },
                select: function (ev, ui) {
                    if (isInList(ui.item.ids, selPLV)) {
                        return;
                    }
                    if (ui.item.ids != 9999) {
                        selPLV.push(ui.item.ids);
                    }
                    else {
                        $divCheckBoxPLV.children().remove();
                        selPLV = selPLV.splice();
                    }
                    //$divCheckBoxPLV.append('<div class="VA011-clsDIV"><input type="checkbox" id=VA011_chkPLV' + counter + ' checked value="' + ui.item.ids + '"><span id=VA011_spnPLV' + counter + '>' + ui.item.value + '</span></br></div>');
                    $divCheckBoxPLV.append('<div class="VA011-clsDIV"><img id=VA011_chkPLV' + counter + ' style="display:inline;margin-top:-3px;margin-left:5px;cursor:pointer" value="' + ui.item.ids + '" src="Areas/VA011/Images/cross.png"><span style="margin-left:5px" id=VA011_spnPLV' + counter + '>' + ui.item.value + '</span></br></div>');
                    counter++;
                    pgNo = 1;
                    bindProductGrid(false);
                }
            });
            $divlbMain.append($divPLV);
            $divlbMain.append($divCheckBoxPLV);
            // Append PriceListVersion TextBox

            // Append Supplier TextBox
            $divCheckBoxSupplier = $('<div class="VA011-checkboxlist" style="color:#fff;" >');
            $divSupplier = $('<input class="VA011-inputLeftSearch" placeholder="' + VIS.Msg.getMsg("VA011_Supplier") + '" ><img class="VA011-imgCombo" style="display:inline;margin-top:-3px;margin-left:-15px;cursor:pointer" src="Areas/VA011/Images/open-arrow.png">');
            $divSupplier.autocomplete({
                minLength: 0,
                source: function (request, response) {

                    if (request.term.trim().length == 0) {
                        return;
                    }

                    fillAutoCompleteonTextBox(request.term, response, false, "GetSupplier");
                },
                select: function (ev, ui) {
                    if (isInList(ui.item.ids, selSupp)) {
                        return;
                    }
                    if (ui.item.ids != 9999) {
                        selSupp.push(ui.item.ids);
                    }
                    else {
                        $divCheckBoxSupplier.children().remove();
                        selSupp = selSupp.splice();
                    }
                    //$divCheckBoxSupplier.append('<div class="VA011-clsDIV"><input type="checkbox" id=VA011_chkSupplier' + counter + ' checked value="' + ui.item.ids + '"><span id=VA011_spnSupplier' + counter + '>' + ui.item.value + '</span></br></div>');
                    $divCheckBoxSupplier.append('<div class="VA011-clsDIV"><img id=VA011_chkSupplier' + counter + ' style="display:inline;margin-top:-3px;margin-left:5px;cursor:pointer" value="' + ui.item.ids + '" src="Areas/VA011/Images/cross.png"><span style="margin-left:5px" id=VA011_spnSupplier' + counter + '>' + ui.item.value + '</span></br></div>');
                    counter++;
                    pgNo = 1;
                    bindProductGrid(false);
                }
            });

            $divlbMain.append($divSupplier);
            $divlbMain.append($divCheckBoxSupplier);
            // Append Supplier TextBox

            // Append Categories TextBox
            $divCheckBoxCategories = $('<div class="VA011-checkboxlist" style="color:#fff;" >');
            $divProductCategories = $('<input class="VA011-inputLeftSearch" placeholder="' + VIS.Msg.getMsg("VA011_ProductCategory") + '" ><img class="VA011-imgCombo" style="display:inline;margin-top:-3px;margin-left:-15px;cursor:pointer"  src="Areas/VA011/Images/open-arrow.png">');
            $divProductCategories.autocomplete({

                minLength: 0,
                source: function (request, response) {

                    if (request.term.trim().length == 0) {
                        return;
                    }

                    fillAutoCompleteonTextBox(request.term, response, false, "GetProductCategories");
                },
                select: function (ev, ui) {

                    if (isInList(ui.item.ids, selCat)) {
                        return;
                    }
                    if (ui.item.ids != 9999) {
                        selCat.push(ui.item.ids);
                    }
                    else {
                        $divCheckBoxCategories.children().remove();
                        selCat = selCat.splice();
                    }
                    //$divCheckBoxCategories.append('<div class="VA011-clsDIV"><input type="checkbox" id=VA011_chkCategories' + counter + ' checked value="' + ui.item.ids + '"><span id=VA011_spnCategories' + counter + '>' + ui.item.value + '</span></br></div>');
                    $divCheckBoxCategories.append('<div class="VA011-clsDIV"><img id=VA011_chkCategories' + counter + ' style="display:inline;margin-top:-3px;margin-left:5px;cursor:pointer" value="' + ui.item.ids + '" src="Areas/VA011/Images/cross.png"><span style="margin-left:5px" id=VA011_spnCategories' + counter + '>' + ui.item.value + '</span></br></div>');
                    counter++;
                    pgNo = 1;
                    bindProductGrid(false);
                }
            });

            $divlbMain.append($divProductCategories);
            $divlbMain.append($divCheckBoxCategories);
            // Append Categories TextBox

            //$divlbMain.append('<div class="VA011-left-cat-panel"> <h4>' + VIS.Msg.getMsg("VA011_Categories") + '</h4></div>');
            $divlbMain.append($ulLefttoolbar);
            $lb.append($btnlbToggle);
            $lb.append($divlbMain);
            $td0leftbar.append($lb);

            //// Added Headers here for middle DIV
            //$divHeadVarientProdTop = $('<div class="VA011-right-head VA011-tab-control" style="padding: 0px; "><ul class="VA011-tabs" style="overflow: auto;white-space: nowrap;">'
            //    + '<li class="VA011-selectedTab" id="VA011_StockDet_' + $self.windowNo + '" style="margin: 0 0px 0 2px;">' + VIS.Msg.getMsg("VA011_StockDet") + '</li>'
            //    + '<li id="VA011_GenReplenish_' + $self.windowNo + '" style="margin: 0 0px 0 2px;">' + VIS.Msg.getMsg("VA011_Replenishments") + '</li>'
            //    + '<li id="VA011_btnGenerateReplenish_' + $self.windowNo + '" style="margin: 0 0px 0 2px;float:right;display:none">' + VIS.Msg.getMsg("VA011_GenerateReplenishment") + '</li>'
            //    + '<li id="VA011_btnReplenish_' + $self.windowNo + '" style="margin: 0 0px 0 2px;float:right;">' + VIS.Msg.getMsg("VA011_CalculatReplenishment") + '</li>'
            //    + '</ul></div>');

            // Added Headers here for middle DIV 
            $divHeadVarientProdTop = $('<div class="VA011-right-head VA011-tab-control" style="padding: 0px; width:auto;"><ul class="VA011-tabs" style="overflow: auto;white-space: nowrap;">'
                + '<li class="VA011-selectedTab" id="VA011_StockDet_' + $self.windowNo + '" style="margin: 0 0px 0 2px;">' + VIS.Msg.getMsg("VA011_StockDet") + '</li>'
                + '<li id="VA011_GenReplenish_' + $self.windowNo + '" style="margin: 0 0px 0 2px;">' + VIS.Msg.getMsg("VA011_Replenishments") + '</li>'

                + '</ul></div>'
                // Div Cart Label
                + '<div class="VA011_CartLabel"><span id="VA011_CartLabel' + $self.windowNo + '" style=" margin-top: 10px; height: 74px; font-weight: bolder; color:grey; font-size: larger;"></span></div>'
                // Div Icons Replenishment
                + '<div style="float:right; margin-top: 5px;">'
                + '<li id="VA011_generateReplenishLI' + $self.windowNo + '" style="margin: 0 0px 0 2px;float:right;background:none; margin-left: 15px; display:none"><div><img style="cursor:pointer" title=' + VIS.Msg.getMsg("VA011_GenerateReplenishment") + ' id="VA011_btnGenerateReplenish_' + $self.windowNo + '" style="opacity: 1; display:none;" src="' + VIS.Application.contextUrl + 'Areas/VA011/Images/genrat--replenishment.png"></div></li>'
                + '<li id="VA011_calculateReplenishLI' + $self.windowNo + '" style="margin: 0 0px 0 2px;float:right;background:none; margin-left: 15px; display:none;"><div><img style="cursor:pointer" title=' + VIS.Msg.getMsg("VA011_CalculateReplenishment") + ' id="VA011_btnReplenish_' + $self.windowNo + '" style="opacity: 1;" src="' + VIS.Application.contextUrl + 'Areas/VA011/Images/calculate.png"></div></li>'
                + '<li id="VA011_addtoCartLI' + $self.windowNo + '" style="margin: 0 0px 0 2px;float:right;background:none; margin-left: 15px; display:none;"><div><img style="cursor:pointer" title=' + VIS.Msg.getMsg("VA011_AddToCart") + ' id="VA011_btnAddToCart_' + $self.windowNo + '" style="opacity: 1;" src="' + VIS.Application.contextUrl + 'Areas/VA011/Images/cart1.png"></div></li>'
                + '<li id="VA011_DisplayReplenishRuleLI_' + $self.windowNo + '" style="margin: 0 0px 0 2px;float:right;background:none; margin-left: 15px; display:none;"><div><img style="cursor:pointer" title=' + VIS.Msg.getMsg("VA011_ReplenishmentRules") + ' id="VA011_DisplayReplenishRule_' + $self.windowNo + '" style="opacity: 1;" src="' + VIS.Application.contextUrl + 'Areas/VA011/Images/edt.png"></div></li>'
                + ' </div>');

            var $divTopMiddle = $('<div id="111" style="width:100%; height:50%; float:left; padding-bottom:10px "></div>');

            // Added Grid here for middle DIV (Product Grid)
            $divProdGrid = $('<div class="VA011-gridCls"></div>');

            $divRepTopGrid = $('<div id="VA011_ReplenishmentGridTop_' + $self.windowNo + '" class="VA011-gridCls" style="display:none"></div>');

            $divHeadVarientProdBottom = $('<div class="VA011-right-head VA011-tab-control" style="padding: 0px; margin-top: 10px"><ul class="VA011-tabs VA011_bottomGridHeaderSec" style="overflow: auto;white-space: nowrap;">' +
              ' <li class="VA011-selectedTab" id="VA011_btnVariant_' + $self.windowNo + '" style="margin: 0 0px 0 2px;">' + VIS.Msg.getMsg("VA011_Variants") + '</li>' +
              ' <li id="VA011_btnLocators_' + $self.windowNo + '" style="margin: 0 0px 0 2px;">' + VIS.Msg.getMsg("VA011_Locators") + '</li>' +
              ' <li id="VA011_btnOrdered' + $self.windowNo + '" style="margin: 0 0px 0 2px;">' + VIS.Msg.getMsg("VA011_Ordered") + '</li>' +
              ' <li  id="VA011_btnReplenished_' + $self.windowNo + '" style="margin: 0 0px 0 2px;">' + VIS.Msg.getMsg("VA011_Replenished") + '</li>' +
              ' <li  id="VA011_btnDemand_' + $self.windowNo + '" style="margin: 0 0px 0 2px;">' + VIS.Msg.getMsg("VA011_Demand") + '</li> ' +
              ' <li  id="VA011_btnTransactions_' + $self.windowNo + '" style="margin: 0 0px 0 2px;">' + VIS.Msg.getMsg("VA011_Transactions") + '</li>' +
              ' <li  id="VA011_btnReplenishmentB_' + $self.windowNo + '" style="margin: 0 0px 0 2px;">' + VIS.Msg.getMsg("VA011_Replenishment") + '</li>' +
              ' </ul></div>');

            $divBottomMiddle = $('<div id="VA011_bottomGridPanelDiv' + $self.windowNo + '" style="width:100%; float:left; height:50%"></div>');
            $divBottomMiddle.append($divHeadVarientProdBottom);

            //$middlePanel = $('<div class="VA011-middle-left-main" style="height:100%">').append($divHeadVarientProdTop);
            $middlePanel = $('<div class="VA011-middle-left-main" style="height:100%">').append($divTopMiddle).append($divBottomMiddle);

            $rightPanel = $('<div id="1234" class="VA011-middle-right-main">');

            // Right Panel Product Details and Other Tabs
            var $rightmain = $('<div class="VA011_form-wrap" style="height:100%"></div>');

            $divRightSection = $('<div id="VA011_RightSection' + $self.windowNo + '" style=width:100%; height:100%>');

            $divHeadDetails = $('<div class="VA011_form-tabs" style="padding-bottom:0px">'
                + '<ul class="VA011-tabs" style="overflow:auto; white-space:nowrap">'
                + '<li id="VA011_btnDetails_' + $self.windowNo + '" class="VA011-selectedTab">' + VIS.Msg.getMsg("VA011_Details") + '</li>'
                + '<li id="VA011_btnSubsti_' + $self.windowNo + '">' + VIS.Msg.getMsg("VA011_Substitute") + '</li>'
                + '<li id="VA011_btnRelated_' + $self.windowNo + '">' + VIS.Msg.getMsg("VA011_Related") + '</li>'
                + '<li id="VA011_btnSuppliers_' + $self.windowNo + '">' + VIS.Msg.getMsg("VA011_Suppliers") + '</li>'
                + '<li id="VA011_btnKits_' + $self.windowNo + '">' + VIS.Msg.getMsg("VA011_Kits") + '</li>'
                + '<li id="VA011_btnCart_' + $self.windowNo + '">' + VIS.Msg.getMsg("VA011_Cart") + '</li>'
                + '</ul></div>');

            $rightmain.append($divHeadDetails);

            $divImgDet = $('<div class="VA011_form-top" >'
                + '<div class="VA011_form-top-fields" style="float:left; width:60%">'
                + '<div id="VA011_ProdDetZoomName' + $self.windowNo + '" style= "display:none"><h4 id="VA011_prodName_' + $self.windowNo + '" style="float:left; word-wrap: break-word;"></h4><span id="VA011_ZoomProduct" title=' + VIS.Msg.getMsg("VA011_ZoomToProduct") + ' class="VA011-icons VA011-icons-font glyphicon glyphicon-edit" style="margin-top:5px"></span></div>'
                + '<div style="float:left; width:100%" class="VA011_data-wrap" id="VA011_UPC' + $self.windowNo + '">' // UPC Numbers to be shown in this DIV
                + '<p>' + VIS.Msg.getMsg("VA011_UPC") + '</p>'
                + '</div>'
                + '<div style="float:left;" class="VA011_data-wrap" id="VA011_AttributeSet' + $self.windowNo + '">'
                + '<p>' + VIS.Msg.getMsg("VA011_AttributeSet") + '</p>'
                + '</div></div><!-- end of form-top-fields -->'
                + '<div class="VA011_image-wrap" style="text-align: center; line-height: 158px;"></div></div>');
            //+ '<div class="image-area"><img src="img/img-defult.png" alt=""></div></div><!-- end of image-wrap --></div><!-- end of form-top -->');

            $divRightSection.append($divImgDet);

            $rightFullDetails = $('<div class="VA011_form-fullFields"><div class="VA011_form-tabs" >'
                + '<ul class="VA011-tabs">'
                + '<li id="VA011_btnDetails_' + $self.windowNo + '" class="VA011-selectedTab">' + VIS.Msg.getMsg("VA011_Details") + '</li>'
                + '<li style="display:none;" id="VA011_btnStatistics_' + $self.windowNo + '">' + VIS.Msg.getMsg("VA011_Statistics") + '</li>'
                + '</ul>'
                + '</div>'
                + '<div class="VA011_form-data"><label>' + VIS.Msg.getMsg("VA011_Weight") + '</label><input id="VA011_inputWeight_' + $self.windowNo + '" readonly="readOnly" type="text"></div>'
                + '<div class="VA011_form-data"><label>' + VIS.Msg.getMsg("VA011_Volume") + '</label><input id="VA011_inputVolume_' + $self.windowNo + '" readonly="readOnly" type="text"></div>'
                + '<div class="VA011_form-data"><label>' + VIS.Msg.getMsg("VA011_Tare") + '</label><input id="VA011_inputTare_' + $self.windowNo + '" readonly="readOnly" type="text"></div>'
                + '<div class="VA011_form-data"><label>' + VIS.Msg.getMsg("VA011_Locator") + '</label><input id="VA011_inputLocator_' + $self.windowNo + '" readonly="readOnly" type="text"></div>'
                + '<div class="VA011_form-data"><label>' + VIS.Msg.getMsg("VA011_ExpiryDays") + '</label><input id="VA011_inputExpDays_' + $self.windowNo + '" readonly="readOnly" type="text"></div>'
                + '<div class="VA011_form-data"><label>' + VIS.Msg.getMsg("VA011_UOM") + '</label><input id="VA011_inputUOM_' + $self.windowNo + '" readonly="readOnly" type="text"></div>'
                + '</div><!-- end of form-fullFields -->');

            $divRightSection.append($rightFullDetails);

            $rightmain.append($divRightSection);

            $divSubstituteGrid = $('<div  id="VA011_grdSubstitute_' + $self.windowNo + '" class="VA011_form-wrap VA011-rightPanelGridCls" style="display:none; padding:0px"></div>');
            gridSubstitutePanel();
            bindSubstituteGrid();
            $rightmain.append($divSubstituteGrid);

            $divRelatedGrid = $('<div  id="VA011_grdRelated_' + $self.windowNo + '" class="VA011_form-wrap VA011-rightPanelGridCls" style="display:none; padding:0px"></div>');
            gridRelatedPanel();
            bindRelatedGrid();
            $rightmain.append($divRelatedGrid);

            $divSuppliersRightGrid = $('<div  id="VA011_grdSuppliersRight_' + $self.windowNo + '" class="VA011_form-wrap VA011-rightPanelGridCls" style="display:none; padding:0px"></div>');
            gridSuppliersRightPanel();
            bindSuppliersRightGrid();
            $rightmain.append($divSuppliersRightGrid);

            $divKitsGrid = $('<div  id="VA011_grdKits_' + $self.windowNo + '" class="VA011_form-wrap VA011-rightPanelGridCls" style="display:none; padding:0px"></div>');
            gridKitsPanel();
            bindKitsGrid();
            $rightmain.append($divKitsGrid);

            $rightmain.append('<div id="VA011_divCart_' + $self.windowNo + '" class="VA011-right-head"><div class="VA011-conversion-data"><label>' + VIS.Msg.getMsg("VA011_Cart") +
            '</label></div><div class="VA011-conversion-data"><span class="VA011-cart-update" style="display:none;"></span></div>' +
            '<div id="VA011_divCartList_' + $self.windowNo + '" class="VA011-conv-data"><div class="VA011-conversion-data"><select id="VA011_cmbCart_' + $self.windowNo + '"></select></div>' +
            '<div style="float:left;margin-top: 5px;"><span class="VA011-icons glyphicon glyphicon-plus VA011-icons-font" title="' + VIS.Msg.getMsg("VA011_AddNewCart") + '"></span>' +
            '<span class="VA011-icons glyphicon glyphicon-edit VA011-icons-font" title="' + VIS.Msg.getMsg("Edit") + '"></span><span class="VA011-icons glyphicon glyphicon-refresh VA011-icons-font" title="' + VIS.Msg.getMsg("VA011_Refresh") + '"></span></div></div>' +
            //<input class="vis-group-add-btn vis-group-pointer vis-group-addLeft vis-group-ass-btns" type="button"></div></div>' +
            '<div id="VA011_divNewCart_' + $self.windowNo + '" class="VA011-conv-data"><div class="VA011-conversion-data"><input id="VA011_scanName_' + $self.windowNo + '"></div>' +
            '<div style="float:left;margin-top: 5px;"><span id="VA011_SaveScanName_' + $self.windowNo + '" class="VA011-icons glyphicon glyphicon-floppy-disk VA011-icons-font" tabindex="0" title="' + VIS.Msg.getMsg("Save") + '">' +
            '</span><span id="VA011_btnCancelScan_' + $self.windowNo + '" class="VA011-icons glyphicon glyphicon-remove-circle VA011-icons-font" tabindex="0" title="' + VIS.Msg.getMsg("Cancel") + '"></span></div></div></div>');
            $divCartdata = $('<div class="VA011-rightPanelCartGridCls" >');
            //$divCartdata.append($divCart);
            $rightmain.append($divCartdata);
            divCart = $rightmain.find("#VA011_divCart_" + $self.windowNo);
            divCartList = $rightmain.find("#VA011_divCartList_" + $self.windowNo);
            divNewCart = $rightmain.find("#VA011_divNewCart_" + $self.windowNo);
            divCart.hide();
            divNewCart.hide();
            $divCartdata.hide();
            btnNewCart = divCart.find(".glyphicon-plus");
            btnEditCart = divCart.find(".glyphicon-edit");
            btnRefreshCart = divCart.find(".glyphicon-refresh");
            cmbCart = divCart.find("#VA011_cmbCart_" + $self.windowNo);
            txtScan = $rightmain.find("#VA011_scanName_" + $self.windowNo);
            btnSaveScan = $rightmain.find("#VA011_SaveScanName_" + $self.windowNo);
            btnCancelScan = $rightmain.find("#VA011_btnCancelScan_" + $self.windowNo);
            $rightmain.find('.VA011-cart-update').text(VIS.Msg.getMsg("Updated"));

            $rightPanel.append($rightmain);

            // Right Panel

            $td2_tr1 = $("<td>");
            middleMain = $('<div style="position: relative;width: 100%;height: 100%;">');
            var middleMaindiv = $('<div class="VA011-middle-div-main">').append($middlePanel).append($rightPanel).append();

            middleMain.append(middleMaindiv);
            $td2_tr1.append(middleMain);
            $tr = $("<tr>").append($td0leftbar).append($td2_tr1); //row 1
            $table = $("<table style='width:100%;height:100%;background-color:white;' >"); //main root
            $table.append($tr);
            $root.append($table);

            //$divCheckBoxOrg.append('<div class="VA011-clsDIV"><input type="checkbox" id=VA011_chkOrg' + counter + ' checked value="' + VIS.context.getContext("#AD_Org_ID") + '"><span id=VA011_spnOrg' + counter + '> ' + VIS.context.getContext("#AD_Org_Name") + ' </span></br></div>');
            $divCheckBoxOrg.append('<div class="VA011-clsDIV"><img id=VA011_chkOrg' + counter + ' style="display:inline;margin-top:-3px;margin-left:5px;cursor:pointer" value="' + VIS.context.getContext("#AD_Org_ID") + '" src="Areas/VA011/Images/cross.png"><span style="margin-left:5px" id=VA011_spnOrg' + counter + '>' + VIS.context.getContext("#AD_Org_Name") + '</span></br></div>');
            selOrgs.push(VIS.Utility.Util.getValueOfInt(VIS.context.getContext("#AD_Org_ID")));
            counter++;

            if (VIS.Utility.Util.getValueOfInt(VIS.context.getContext("#M_Warehouse_ID")) > 0) {
                //$divCheckBoxWarehouse.append('<div class="VA011-clsDIV"><input type="checkbox" id=VA011_chkWh' + counter + ' checked value="' + VIS.context.getContext("#M_Warehouse_ID") + '"><span id=VA011_spnWh' + counter + '>' + VIS.context.getContext("#M_Warehouse_Name") + '</span></br></div>');
                $divCheckBoxWarehouse.append('<div class="VA011-clsDIV"><img id=VA011_chkWh' + counter + ' style="display:inline;margin-top:-3px;margin-left:5px;cursor:pointer" value="' + VIS.context.getContext("#M_Warehouse_ID") + '" src="Areas/VA011/Images/cross.png"><span style="margin-left:5px" id=VA011_spnWh' + counter + '>' + VIS.context.getContext("#M_Warehouse_Name") + '</span></br></div>');
                selWh.push(VIS.Utility.Util.getValueOfInt(VIS.context.getContext("#M_Warehouse_ID")));
                counter++;
            }

            gridProductPanel();
            bindProductGrid(false);
            $divTopMiddle.append($divHeadVarientProdTop).append($divProdGrid);

            // Replenishment Top Grid
            gridReplenishTopPanel();
            bindReplenishTopGrid();
            $divTopMiddle.append($divRepTopGrid);

            /////////////////////////////////////  Stauts Bar Paging for Product Grid    ///////////////////////////////////////////////

            // Product Status Bar Div
            $statusProdDiv = $('<div style="width:100%;background-color:lightgray; float:left;"></div>');
            $ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            // For showing Result li
            // + '<li><span class="vis-statusbar-statusDB"></span></li>'

            $liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            $ulStatusProdDiv.append($liFirst);

            $liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            $ulStatusProdDiv.append($liPrev);

            var $li = $('<li style="float:left"></li>');
            $li.append($cmbPageNo);
            $ulStatusProdDiv.append($li);

            $liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            $ulStatusProdDiv.append($liNext);

            $liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            $ulStatusProdDiv.append($liLast);

            $statusProdDiv.append($ulStatusProdDiv);

            $divTopMiddle.append($statusProdDiv);
            /////////////////////////////////////  Stauts Bar Paging for Product Grid    ///////////////////////////////////////////////

            // Bottom Section Variants/ Ordered/ Transactions
            // Added Grid here for middle DIV Bottom Section (Product Variants etc.)
            $divVariantGrid = $('<div class="VA011-gridCls"></div>');
            gridVariantPanel();
            bindVariantGrid();
            $divBottomMiddle.append($divVariantGrid);

            // Added Grid here for middle DIV Bottom Section (Product Locator etc.)
            $divLocatorGrid = $('<div class="VA011-gridCls" style="display:none"></div>');
            gridLocatorPanel();
            bindLocatorGrid();
            $divBottomMiddle.append($divLocatorGrid);

            // Added Grid here for middle DIV Bottom Section (Product Ordered etc.)
            $divOrderedGrid = $('<div class="VA011-gridCls" style="display:none"></div>');
            gridOrderedPanel();
            bindOrderedGrid();
            $divBottomMiddle.append($divOrderedGrid);

            // Added Grid here for middle DIV Bottom Section (Product Replenished etc.)
            $divReplenishedGrid = $('<div class="VA011-gridCls" style="display:none"></div>');
            gridReplenishedPanel();
            bindReplenishedGrid();
            $divBottomMiddle.append($divReplenishedGrid);

            // Added Grid here for middle DIV Bottom Section (Product Demand etc.)
            $divDemandGrid = $('<div class="VA011-gridCls" style="display:none"></div>');
            gridDemandPanel();
            bindDemandGrid();
            $divBottomMiddle.append($divDemandGrid);

            // Added Grid here for middle DIV Bottom Section (Product Transactions etc.)
            $divTransactionsGrid = $('<div class="VA011-gridCls" style="display:none"></div>');
            gridTransactionsPanel();
            bindTransactionsGrid();
            $divBottomMiddle.append($divTransactionsGrid);

            // Added Grid here for middle DIV Bottom Section (Replenishment Bottom etc.)
            $divReplenishmentBGrid = $('<div class="VA011-gridCls" style="display:none"></div>');
            //if (isReplenishTypeLoaded) {
            //    gridReplenishmentBPanel();
            //    bindReplenishmentBGrid();
            //    $divBottomMiddle.append($divReplenishmentBGrid);
            //}

            // loadCategories(pageno, pgSize);
            middlePanel();
            rightPanel();
        };

        function isInList(value, array) {
            return array.indexOf(value) > -1;
        }

        function CartPanel() {
            //$divCart.append($divCartMain);
            cartGrid = null;
            cartGrid = $divCartdata.w2grid({
                name: 'gridcart_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: true,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: true,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },
                records: [],
                columns: [
                    { field: "product_ID", caption: "product_ID", sortable: false, size: '80px', display: false },
                    { field: "Product", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.translate(VIS.Env.getCtx(), "Product") + '</span></div>', sortable: false, size: '35%', hidden: false },
                    { field: "Qty", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getElement(VIS.Env.getCtx(), "Quantity") + '</span></div>', sortable: false, size: '15%', hidden: false, editable: { type: 'float' }, render: 'number:1' },
                    {
                        field: "C_Uom_ID", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getElement(VIS.Env.getCtx(), "C_UOM_ID") + '</span></div>', sortable: false, size: '15%', hidden: false, editable: { type: 'select', items: uomArray, showAll: true },
                        render: function (record, index, col_index) {
                            var html = '';
                            for (var p in uomArray) {
                                if (uomArray[p].id == this.getCellValue(index, col_index)) html = uomArray[p].text;
                            }
                            return html;
                        }
                    },
                    { field: "attribute_ID", caption: VIS.Msg.getElement(VIS.Env.getCtx(), "M_AttributeSetInstance_ID"), sortable: false, size: '80px', display: false },
                    {
                        field: "Attribute", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.translate(VIS.Env.getCtx(), "Attribute") + '</span></div>', sortable: false, size: '35%', hidden: false,
                        render: function () {
                            return '<div><input type=text readonly="readonly" style= "width:85%; border:none" ></input><img src="' + VIS.Application.contextUrl + 'Areas/VIS/Images/base/MultiX16.png" alt="Attribute Set Instance" title="Attribute Set Instance" style="opacity:1;float:right;"></div>';
                        }
                    },
                    { field: "UPC", caption: VIS.Msg.getElement(VIS.Env.getCtx(), "UPC"), sortable: false, size: '80px', editable: { type: 'text' } },
                    { field: "LineID", caption: VIS.Msg.getElement(VIS.Env.getCtx(), "VAICNT_InventoryCountLine_ID"), sortable: false, size: '80px', display: false },
                    { field: "updated", caption: VIS.Msg.getElement(VIS.Env.getCtx(), "Updated"), sortable: false, size: '80px', display: false }
                ],

                onChange: function (event) {

                    cartGrid.records[event.index]["updated"] = true;
                    if (event.column == 2) {
                        cartGrid.records[event.index]["Qty"] = event.value_new;
                    }
                    else if (event.column == 3) {
                        cartGrid.records[event.index]["C_Uom_ID"] = event.value_new;
                    }
                },

                onClick: function (event) {
                    if (event.column == 5 && cartGrid.records.length > 0) {

                        var qry = "SELECT M_AttributeSet_ID FROM M_Product WHERE M_Product_ID = " + VIS.Utility.Util.getValueOfInt(cartGrid.records[event.recid - 1]["product_ID"]);
                        var mattsetid = VIS.Utility.Util.getValueOfInt(VIS.DB.executeScalar(qry));
                        if (mattsetid != 0) {
                            var productWindow = AD_Column_ID == 8418;		//	HARDCODED
                            var M_Locator_ID = VIS.context.getContextAsInt($self.windowNo, "M_Locator_ID");
                            var C_BPartner_ID = VIS.context.getContextAsInt($self.windowNo, "C_BPartner_ID");
                            var obj = new VIS.PAttributesForm(VIS.Utility.Util.getValueOfInt(cartGrid.records[event.recid - 1]["attribute_ID"]), VIS.Utility.Util.getValueOfInt(cartGrid.records[event.recid - 1]["product_ID"]), M_Locator_ID, C_BPartner_ID, productWindow, AD_Column_ID, $self.windowNo);
                            if (obj.hasAttribute) {
                                obj.showDialog();
                            }
                            obj.onClose = function (mAttributeSetInstanceId, name, mLocatorId) {
                                if (cartGrid.records[event.recid - 1]["attribute_ID"] != mAttributeSetInstanceId) {
                                    cartGrid.records[event.recid - 1]["attribute_ID"] = mAttributeSetInstanceId;
                                    cartGrid.records[event.recid - 1]["Attribute"] = name;
                                    $("#grid_gridcart_" + $self.windowNo + "_rec_" + event.recid).find("input[type=text]").val(name);
                                    cartGrid.records[event.recid - 1]["updated"] = true;
                                }
                            };
                        }
                        else {
                            return;
                        }
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                    deleteInventory();
                },
                onSubmit: function (event) {
                    event.preventDefault();
                    updateInventory();
                },
                onColumnOnOff: function (event) {
                    event.onComplete = function () {
                        BindCartGrid();
                    }
                },
            });
            cartGrid.hideColumn('product_ID');
            cartGrid.hideColumn('attribute_ID');
            cartGrid.hideColumn('UPC');
            cartGrid.hideColumn('LineID');
            cartGrid.hideColumn('updated');
            LoadCart();
        };

        var LoadCart = function () {
            cmbCart.empty();
            var qry = "SELECT VAICNT_InventoryCount_ID,VAICNT_ScanName FROM VAICNT_InventoryCount WHERE IsActive = 'Y' AND VAICNT_TransactionType = 'OT' ORDER BY VAICNT_ScanName";
            VIS.DB.executeReader(qry, null, LoadcartCallBack);
        }

        function LoadcartCallBack(dr) {
            cmbCart.append(" <option value = 0></option>");
            while (dr.read()) {
                key = VIS.Utility.Util.getValueOfInt(dr.getString(0));
                value = dr.getString(1);
                cmbCart.append(" <option value=" + key + ">" + VIS.Utility.encodeText(value) + "</option>");
            }
            dr.close();
            cmbCart.val(cart);
            $("#VA011_CartLabel" + $self.windowNo).text(cmbCart.find('option:selected').text());
            LoadReplenishType();
        };

        function GenerateBarcode(upc, divbarcode) {
            var btypeEan13 = "ean13";
            var btypeEan8 = "ean8";
            var btypeC39 = "code39";
            var btypeC128 = "code128"
            var renderer = "css";
            var settings = {
                output: renderer,
                bgColor: "#FFFFFF",
                color: "#000000",
                barWidth: 1,
                barHeight: 30,
                moduleSize: 2,
                addQuietZone: 1
            };
            if (upc.length == 0) {
                divbarcode.empty();
            }
            else if (upc.length == 12) {
                divbarcode.barcode(upc, btypeEan13, settings);
            }
            else if (upc.length == 7) {
                divbarcode.barcode(upc, btypeEan8, settings);
            }
            else if (upc.length == 4) {
                divbarcode.barcode(upc, btypeC39, settings);
            }
            else {
                divbarcode.barcode(upc, btypeC128, settings);
            }
        };

        function BindCartGrid() {

            if (!initLoad) {
                $("#VA011_RightSection" + $self.windowNo).css("display", "none");
                $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "none");
                $("#VA011_grdRelated_" + $self.windowNo).css("display", "none");
                $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "none");
                $("#VA011_grdKits_" + $self.windowNo).css("display", "none");
                $("#VA011_divCart_" + $self.windowNo).css("display", "block");
                divCart.show();
                $divCartdata.show();
                $bsyDiv[0].style.visibility = "visible";
                multiValues = [];
                cartGrid.clear();
                if (cmbCart.val() > 0) {
                    var sqlaa = "";
                    var ds = null;
                    var sqlaa = "SELECT po.VAICNT_InventoryCountLine_ID,po.M_Product_ID,prd.Name, po.C_UOM_ID, u.Name AS UOM, po.UPC, po.M_AttributeSetInstance_ID, ats.Description, po.VAICNT_Quantity," +
                            " prd.M_AttributeSet_ID FROM VAICNT_InventoryCountLine po LEFT JOIN C_UOM u ON po.C_UOM_ID = u.C_UOM_ID LEFT JOIN M_Product prd" +
                            " ON po.M_Product_ID= prd.M_Product_ID LEFT JOIN M_AttributeSetInstance ats ON po.M_AttributeSetInstance_ID = ats.M_AttributeSetInstance_ID" +
                            " WHERE po.VAICNT_InventoryCount_ID = " + cmbCart.val();
                    VIS.DB.executeDataSet(sqlaa.toString(), null, BindCartCallBack);
                }
                else {
                    $bsyDiv[0].style.visibility = "hidden";
                }
            }
        };

        function BindCartCallBack(ds) {
            var Recid = 0;
            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;
                    multiValues.push(
                    {
                        recid: Recid,
                        LineID: ds.tables[0].rows[i].cells.vaicnt_inventorycountline_id,
                        product_ID: ds.tables[0].rows[i].cells.m_product_id,
                        Product: ds.tables[0].rows[i].cells.name,
                        C_Uom_ID: VIS.Utility.Util.getValueOfInt(ds.tables[0].rows[i].cells.c_uom_id),
                        attribute_ID: VIS.Utility.Util.getValueOfInt(ds.tables[0].rows[i].cells.m_attributesetinstance_id),
                        Attribute: ds.tables[0].rows[i].cells.description,
                        UPC: ds.tables[0].rows[i].cells.upc,
                        Qty: VIS.Utility.Util.getValueOfDecimal(ds.tables[0].rows[i].cells.vaicnt_quantity),
                        updated: false
                    });
                }
            }

            w2utils.encodeTags(multiValues);
            cartGrid.add(multiValues);
            for (var k = 0; k < cartGrid.records.length; k++) {
                $("#grid_gridcart_" + $self.windowNo + "_rec_" + cartGrid.records[k].recid).find("input[type=text]").val(multiValues[k].Attribute);
                var qry = "SELECT M_AttributeSet_ID FROM M_Product WHERE M_Product_ID = " + multiValues[k].product_ID;
                if (VIS.Utility.Util.getValueOfInt(VIS.DB.executeScalar(qry)) <= 0) {
                    $("#grid_gridcart_" + $self.windowNo + "_rec_" + cartGrid.records[k].recid).find("input:not([type='checkbox'])").hide();
                    $("#grid_gridcart_" + $self.windowNo + "_rec_" + cartGrid.records[k].recid).find("img").hide();
                }
            }
            multiValues = [];
            $bsyDiv[0].style.visibility = "hidden";
        };

        // Create Substitue Panel Grid at the Bottom
        function gridSubstitutePanel() {
            dSubstituteGrid = null;
            dSubstituteGrid = $divSubstituteGrid.w2grid({
                name: 'VA011_gridSubstitute_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    { field: "Product", caption: VIS.Msg.getMsg("VA011_Product"), sortable: false, size: '36%' },
                    { field: "QtyOnHand", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_OnHand") + '</span></div>', sortable: false, size: '16%', hidden: false, render: 'number:1' },
                    { field: "UOM", caption: VIS.Msg.getMsg("VA011_UOM"), sortable: false, size: '16%' },
                    { field: "Reserved", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Reserved") + '</span></div>', sortable: false, size: '10%', hidden: false, render: 'number:1' },
                    { field: "ATP", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_ATP") + '</span></div>', sortable: false, size: '10%', hidden: false, render: 'number:1' },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                },
                onClick: function (event) {
                    if (event.column == 5 && dSubstituteGrid.records.length > 0) {
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                },
                onSubmit: function (event) {
                },
            });

            //
            //// Product Status Bar Div
            //$statusProdDiv = $('<div style="width:100%;height:30px;background-color:lightgray"></div>');
            //$ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            //// For showing Result li
            //// + '<li><span class="vis-statusbar-statusDB"></span></li>'

            //$liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            //$ulStatusProdDiv.append($liFirst);

            //$liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            //$ulStatusProdDiv.append($liPrev);

            //var $li = $('<li style="float:left"></li>');
            //$li.append($cmbPageNo);
            //$ulStatusProdDiv.append($li);

            //$liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            //$ulStatusProdDiv.append($liNext);

            //$liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            //$ulStatusProdDiv.append($liLast);

            //$statusProdDiv.append($ulStatusProdDiv);

            //$middlePanel.append($statusProdDiv);

            //$divVariantGrid.css("display", "none");

            dSubstituteGrid.hideColumn('attribute_ID');
            dSubstituteGrid.hideColumn('M_Product_ID');
        };

        // Bind Substitute Grid 
        function bindSubstituteGrid() {

            if (!initLoad) {

                $("#VA011_RightSection" + $self.windowNo).css("display", "none");
                $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "block");
                $("#VA011_gridRelated_" + $self.windowNo).css("display", "none");
                $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "none");
                $("#VA011_grdKits_" + $self.windowNo).css("display", "none");
                divCart.hide();
                $divCartdata.hide();
                $bsyDiv[0].style.visibility = "visible";

                grdSubstituteProdValues = [];

                var Recid = 0;
                dSubstituteGrid.clear();
                Recid = Recid + 1;

                var sqlVar = "";

                sqlVar = "SELECT DISTINCT p.Name as Product, p.M_Product_ID, u.Name AS UOM , (bomQtyOnHand(p.M_Product_ID,w.M_Warehouse_ID,0)) AS QtyOnHand,"
                    + " bomQtyAvailable(p.M_Product_ID,w.M_Warehouse_ID,0) AS QtyAvailable, (bomQtyReserved(p.M_Product_ID,w.M_Warehouse_ID,0))  AS QtyReserved"
                    + " FROM M_Substitute s INNER JOIN M_Product p ON (p.M_Product_ID = s.SUBSTITUTE_ID) INNER JOIN C_UOM u ON (p.C_UOM_ID = u.C_UOM_ID) LEFT OUTER JOIN M_Storage st "
                    + " ON (st.M_Product_ID = p.M_Product_ID) LEFT OUTER JOIN M_Locator l ON (st.M_Locator_ID = l.M_Locator_ID) LEFT OUTER JOIN M_Warehouse w ON (w.M_Warehouse_ID = l.M_Warehouse_ID)"
                    + " WHERE s.IsActive='Y' AND s.M_Product_ID = " + _product_ID;

                var whString = "";
                for (var w = 0; w < selWh.length; w++) {
                    if (whString.length > 0) {
                        whString = whString + ", " + selWh[w];
                    }
                    else {
                        whString = whString + selWh[w];
                    }
                }

                if (whString.length > 0) {
                    sqlVar += " AND w.M_Warehouse_ID IN (" + whString + ")";
                }

                VIS.DB.executeReader(sqlVar.toString(), null, callbackSubstituteGrid);
            }
        };

        function callbackSubstituteGrid(ds) {

            var Recid = 0;
            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;
                    grdSubstituteProdValues.push(
                    {
                        recid: Recid,
                        Product: ds.tables[0].rows[i].cells.product,
                        QtyOnHand: ds.tables[0].rows[i].cells.qtyonhand,
                        UOM: ds.tables[0].rows[i].cells.uom,
                        Reserved: ds.tables[0].rows[i].cells.qtyreserved,
                        ATP: ds.tables[0].rows[i].cells.qtyavailable,
                        M_Product_ID: ds.tables[0].rows[i].cells.m_product_id,
                    });
                }
            }

            w2utils.encodeTags(grdSubstituteProdValues);

            dSubstituteGrid.add(grdSubstituteProdValues);

            grdSubstituteProdValues = [];

            if (!initLoad) {
                $bsyDiv[0].style.visibility = "hidden";
            }
        };

        // Create Related Panel Grid at the Bottom
        function gridRelatedPanel() {
            dRelatedGrid = null;
            dRelatedGrid = $divRelatedGrid.w2grid({
                name: 'VA011_gridRelated_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    { field: "Product", caption: VIS.Msg.getMsg("VA011_Product"), sortable: false, size: '36%' },
                    { field: "QtyOnHand", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_OnHand") + '</span></div>', sortable: false, size: '16%', hidden: false, render: 'number:1' },
                    { field: "UOM", caption: VIS.Msg.getMsg("VA011_UOM"), sortable: false, size: '16%' },
                    { field: "Reserved", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Reserved") + '</span></div>', sortable: false, size: '16%', hidden: false, render: 'number:1' },
                    { field: "ATP", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_ATP") + '</span></div>', sortable: false, size: '16%', hidden: false, render: 'number:1' },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                },
                onClick: function (event) {
                    if (event.column == 5 && dRelatedGrid.records.length > 0) {
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                },
                onSubmit: function (event) {
                },
            });

            //
            //// Product Status Bar Div
            //$statusProdDiv = $('<div style="width:100%;height:30px;background-color:lightgray"></div>');
            //$ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            //// For showing Result li
            //// + '<li><span class="vis-statusbar-statusDB"></span></li>'

            //$liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            //$ulStatusProdDiv.append($liFirst);

            //$liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            //$ulStatusProdDiv.append($liPrev);

            //var $li = $('<li style="float:left"></li>');
            //$li.append($cmbPageNo);
            //$ulStatusProdDiv.append($li);

            //$liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            //$ulStatusProdDiv.append($liNext);

            //$liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            //$ulStatusProdDiv.append($liLast);

            //$statusProdDiv.append($ulStatusProdDiv);

            //$middlePanel.append($statusProdDiv);

            //$divVariantGrid.css("display", "none");

            dRelatedGrid.hideColumn('M_Product_ID');
        };

        // Bind Related Grid 
        function bindRelatedGrid() {

            if (!initLoad) {

                $("#VA011_RightSection" + $self.windowNo).css("display", "none");
                $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "none");
                $("#VA011_grdRelated_" + $self.windowNo).css("display", "block");
                $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "none");
                $("#VA011_grdKits_" + $self.windowNo).css("display", "none");
                divCart.hide();
                $divCartdata.hide();
                $bsyDiv[0].style.visibility = "visible";

                grdRelatedProdValues = [];

                var Recid = 0;
                dRelatedGrid.clear();
                Recid = Recid + 1;

                var sqlVar = "";

                sqlVar = "SELECT DISTINCT p.Name as Product, p.M_Product_ID, u.Name AS UOM , (bomQtyOnHand(p.M_Product_ID,w.M_Warehouse_ID,0)) AS QtyOnHand,"
                    + " bomQtyAvailable(p.M_Product_ID,w.M_Warehouse_ID,0) AS QtyAvailable, (bomQtyReserved(p.M_Product_ID,w.M_Warehouse_ID,0))  AS QtyReserved"
                    + " FROM M_RelatedProduct s INNER JOIN M_Product p ON (p.M_Product_ID = s.RelatedProduct_ID) INNER JOIN C_UOM u ON (p.C_UOM_ID = u.C_UOM_ID) LEFT OUTER JOIN M_Storage st "
                    + " ON (st.M_Product_ID = p.M_Product_ID) LEFT OUTER JOIN M_Locator l ON (st.M_Locator_ID = l.M_Locator_ID) LEFT OUTER JOIN M_Warehouse w ON (w.M_Warehouse_ID = l.M_Warehouse_ID)"
                    + " WHERE s.IsActive='Y' AND s.M_Product_ID = " + _product_ID;

                var whString = "";
                for (var w = 0; w < selWh.length; w++) {
                    if (whString.length > 0) {
                        whString = whString + ", " + selWh[w];
                    }
                    else {
                        whString = whString + selWh[w];
                    }
                }

                if (whString.length > 0) {
                    sqlVar += " AND w.M_Warehouse_ID IN (" + whString + ")";
                }

                VIS.DB.executeReader(sqlVar.toString(), null, callbackRelatedGrid);
            }
        };

        function callbackRelatedGrid(ds) {

            var Recid = 0;
            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;
                    grdRelatedProdValues.push(
                    {
                        recid: Recid,
                        Product: ds.tables[0].rows[i].cells.product,
                        QtyOnHand: ds.tables[0].rows[i].cells.qtyonhand,
                        UOM: ds.tables[0].rows[i].cells.uom,
                        Reserved: ds.tables[0].rows[i].cells.qtyreserved,
                        ATP: ds.tables[0].rows[i].cells.qtyavailable,
                        M_Product_ID: ds.tables[0].rows[i].cells.m_product_id,
                    });
                }
            }

            w2utils.encodeTags(grdRelatedProdValues);

            dRelatedGrid.add(grdRelatedProdValues);

            grdRelatedProdValues = [];

            if (!initLoad) {
                $bsyDiv[0].style.visibility = "hidden";
            }
        };

        // Create Suppliers Panel Grid at Right
        function gridSuppliersRightPanel() {
            dSuppliersRightGrid = null;
            dSuppliersRightGrid = $divSuppliersRightGrid.w2grid({
                name: 'VA011_gridSuppliersRight_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    { field: "Supplier", caption: VIS.Msg.getMsg("VA011_Supplier"), sortable: false, size: '36%' },
                    { field: "QtyOrderPack", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_QtyOrderPack") + '</span></div>', sortable: false, size: '16%', hidden: false, render: 'number:1' },
                    { field: "UOM", caption: VIS.Msg.getMsg("VA011_UOM"), sortable: false, size: '16%' },
                    { field: "MinOrder", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_MinOrder") + '</span></div>', sortable: false, size: '10%', hidden: false, render: 'number:1' },
                    { field: "DeliveryTime", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_DeliveryTime") + '</span></div>', sortable: false, size: '10%', hidden: false, render: 'number:1' },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                },
                onClick: function (event) {
                    if (event.column == 5 && dSuppliersRightGrid.records.length > 0) {
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                },
                onSubmit: function (event) {
                },
            });

            //
            //// Product Status Bar Div
            //$statusProdDiv = $('<div style="width:100%;height:30px;background-color:lightgray"></div>');
            //$ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            //// For showing Result li
            //// + '<li><span class="vis-statusbar-statusDB"></span></li>'

            //$liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            //$ulStatusProdDiv.append($liFirst);

            //$liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            //$ulStatusProdDiv.append($liPrev);

            //var $li = $('<li style="float:left"></li>');
            //$li.append($cmbPageNo);
            //$ulStatusProdDiv.append($li);

            //$liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            //$ulStatusProdDiv.append($liNext);

            //$liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            //$ulStatusProdDiv.append($liLast);

            //$statusProdDiv.append($ulStatusProdDiv);

            //$middlePanel.append($statusProdDiv);

            //$divVariantGrid.css("display", "none");

            dSuppliersRightGrid.hideColumn('attribute_ID');
            dSuppliersRightGrid.hideColumn('M_Product_ID');
        };

        // Bind Suppliers Right Grid 
        function bindSuppliersRightGrid() {

            if (!initLoad) {

                $("#VA011_RightSection" + $self.windowNo).css("display", "none");
                $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "none");
                $("#VA011_grdRelated_" + $self.windowNo).css("display", "none");
                $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "block");
                $("#VA011_grdKits_" + $self.windowNo).css("display", "none");
                divCart.hide();
                $divCartdata.hide();
                $bsyDiv[0].style.visibility = "visible";

                grdSuppliersRightProdValues = [];

                var Recid = 0;
                dSuppliersRightGrid.clear();
                Recid = Recid + 1;

                var sqlVar = "";

                sqlVar = "SELECT bp.name  AS Supplier, po.order_pack AS QtyOrderPack, u.Name AS UOM, po.order_min AS MinOrder, po.deliverytime_promised AS DeliveryTime, po.M_Product_ID"
                    + " FROM M_Product_PO po INNER JOIN C_BPartner bp  ON (bp.C_BPartner_ID = po.C_BPartner_ID) Left outer join C_UOM u on (u.C_UOM_ID = po.C_UOM_ID)"
                    + " WHERE po.IsActive = 'Y' AND po.M_Product_ID = " + _product_ID;

                VIS.DB.executeReader(sqlVar.toString(), null, callbackSuppliersRightGrid);
            }
        };

        function callbackSuppliersRightGrid(ds) {

            var Recid = 0;
            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;
                    grdSuppliersRightProdValues.push(
                    {
                        recid: Recid,
                        Supplier: ds.tables[0].rows[i].cells.supplier,
                        QtyOrderPack: ds.tables[0].rows[i].cells.qtyorderpack,
                        UOM: ds.tables[0].rows[i].cells.uom,
                        MinOrder: ds.tables[0].rows[i].cells.minorder,
                        DeliveryTime: ds.tables[0].rows[i].cells.deliverytime,
                        M_Product_ID: ds.tables[0].rows[i].cells.m_product_id,
                    });
                }
            }

            w2utils.encodeTags(grdSuppliersRightProdValues);

            dSuppliersRightGrid.add(grdSuppliersRightProdValues);

            grdSuppliersRightProdValues = [];

            if (!initLoad) {
                $bsyDiv[0].style.visibility = "hidden";
            }
        };

        // Create Kits Panel Grid at Right
        function gridKitsPanel() {
            dKitsGrid = null;
            dKitsGrid = $divKitsGrid.w2grid({
                name: 'VA011_gridKits_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    { field: "Product", caption: VIS.Msg.getMsg("VA011_Product"), sortable: false, size: '36%' },
                    { field: "Factor", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Factor") + '</span></div>', sortable: false, size: '10%', hidden: false, render: 'number:1' },
                    { field: "UOM", caption: VIS.Msg.getMsg("VA011_UOM"), sortable: false, size: '16%' },
                    { field: "QtyOnHand", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_OnHand") + '</span></div>', sortable: false, size: '16%', hidden: false, render: 'number:1' },
                    { field: "ATP", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_ATP") + '</span></div>', sortable: false, size: '10%', hidden: false, render: 'number:1' },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                },
                onClick: function (event) {
                    if (event.column == 5 && dKitsGrid.records.length > 0) {
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                },
                onSubmit: function (event) {
                },
            });

            //
            //// Product Status Bar Div
            //$statusProdDiv = $('<div style="width:100%;height:30px;background-color:lightgray"></div>');
            //$ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            //// For showing Result li
            //// + '<li><span class="vis-statusbar-statusDB"></span></li>'

            //$liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            //$ulStatusProdDiv.append($liFirst);

            //$liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            //$ulStatusProdDiv.append($liPrev);

            //var $li = $('<li style="float:left"></li>');
            //$li.append($cmbPageNo);
            //$ulStatusProdDiv.append($li);

            //$liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            //$ulStatusProdDiv.append($liNext);

            //$liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            //$ulStatusProdDiv.append($liLast);

            //$statusProdDiv.append($ulStatusProdDiv);

            //$middlePanel.append($statusProdDiv);

            //$divVariantGrid.css("display", "none");

            dKitsGrid.hideColumn('attribute_ID');
            dKitsGrid.hideColumn('M_Product_ID');
        };

        // Bind Kits Grid 
        function bindKitsGrid() {

            if (!initLoad) {

                $("#VA011_RightSection" + $self.windowNo).css("display", "none");
                $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "none");
                $("#VA011_grdRelated_" + $self.windowNo).css("display", "none");
                $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "none");
                $("#VA011_grdKits_" + $self.windowNo).css("display", "block");
                divCart.hide();
                $divCartdata.hide();
                $bsyDiv[0].style.visibility = "visible";

                grdKitsProdValues = [];

                var Recid = 0;
                dKitsGrid.clear();
                Recid = Recid + 1;

                var sqlVar = "";

                sqlVar = "SELECT DISTINCT p.Name as Product, u.Name as UOM, bomQtyOnHand(b.M_Product_ID,w.M_Warehouse_ID,0) AS QtyOnHand, bomQtyAvailable(b.M_Product_ID,w.M_Warehouse_ID,0) AS QtyAvailable,"
                    + " b.BOMQty AS Factor FROM M_Product_BOM b INNER JOIN M_Product p ON p.M_Product_ID = b.M_Product_ID INNER JOIN C_UOM u ON (p.C_UOM_ID = u.C_UOM_ID) "
                    + " LEFT OUTER JOIN M_Storage st ON (st.M_Product_ID = p.M_Product_ID) LEFT OUTER JOIN M_Locator l ON (st.M_Locator_ID = l.M_Locator_ID) LEFT OUTER JOIN M_Warehouse w "
                    + " ON (w.M_Warehouse_ID    = l.M_Warehouse_ID) WHERE b.IsActive='Y' AND b.M_ProductBOM_ID = " + _product_ID;

                var whString = "";
                for (var w = 0; w < selWh.length; w++) {
                    if (whString.length > 0) {
                        whString = whString + ", " + selWh[w];
                    }
                    else {
                        whString = whString + selWh[w];
                    }
                }

                if (whString.length > 0) {
                    sqlVar += " AND w.M_Warehouse_ID IN (" + whString + ")";
                }

                VIS.DB.executeReader(sqlVar.toString(), null, callbackKitsGrid);
            }
        };

        function callbackKitsGrid(ds) {

            var Recid = 0;
            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;
                    grdKitsProdValues.push(
                    {
                        recid: Recid,
                        Product: ds.tables[0].rows[i].cells.product,
                        Factor: ds.tables[0].rows[i].cells.factor,
                        UOM: ds.tables[0].rows[i].cells.uom,
                        QtyOnHand: ds.tables[0].rows[i].cells.qtyonhand,
                        ATP: ds.tables[0].rows[i].cells.qtyavailable,
                        M_Product_ID: ds.tables[0].rows[i].cells.m_product_id,
                    });
                }
            }

            w2utils.encodeTags(grdKitsProdValues);

            dKitsGrid.add(grdKitsProdValues);

            grdKitsProdValues = [];

            if (!initLoad) {
                $bsyDiv[0].style.visibility = "hidden";
            }
        };

        // Create Variant Panel Grid at the Bottom
        function gridVariantPanel() {

            dVariantGrid = null;
            dVariantGrid = $divVariantGrid.w2grid({
                name: 'VA011_gridVariant_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    //{ field: "attribute_ID", caption: "", sortable: false, size: '80px', display: false },
                    //{
                    //    field: "Attribute", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Attribute") + '</span></div>', sortable: false, size: '40%', hidden: false,
                    //    render: function () {
                    //        return '<div><input type=text readonly="readonly" style= "width:85%; border:none" ></input><img src="' + VIS.Application.contextUrl + 'Areas/VIS/Images/base/MultiX16.png" alt="Attribute Set Instance" title="Attribute Set Instance" style="opacity:1;float:right;"></div>';
                    //    }
                    //},
                    { field: "Attribute", caption: VIS.Msg.getMsg("VA011_Attribute"), sortable: false, size: '29%' },
                    { field: "QtyOnHand", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_OnHand") + '</span></div>', sortable: false, size: '11%', hidden: false, render: 'number:1' },
                    { field: "UOM", caption: VIS.Msg.getMsg("VA011_UOM"), sortable: false, size: '11%' },
                    //{
                    //    field: "C_UOM_ID", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("C_UOM_ID") + '</span></div>', sortable: false, size: '10%', hidden: false, editable: { type: 'select', items: uomArray, showAll: true },
                    //    render: function (record, index, col_index) {
                    //        var html = '';
                    //        for (var p in uomArray) {
                    //            if (uomArray[p].id == this.getCellValue(index, col_index)) html = uomArray[p].text;
                    //        }
                    //        return html;
                    //    }
                    //},
                    { field: "UPC", caption: VIS.Msg.getMsg("VA011_UPC"), sortable: false, size: '11%' },
                    { field: "SerialNo", caption: VIS.Msg.getMsg("VA011_SerialNo"), sortable: false, size: '11%' },
                    { field: "LotNo", caption: VIS.Msg.getMsg("VA011_LotNo"), sortable: false, size: '11%' },
                    { field: "ExpDate", caption: VIS.Msg.getMsg("VA011_ExpiryDate"), sortable: false, size: '16%' },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                },
                onClick: function (event) {
                    if (event.column == 5 && dVariantGrid.records.length > 0) {
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                },
                onSubmit: function (event) {
                },
            });

            //
            //// Product Status Bar Div
            //$statusProdDiv = $('<div style="width:100%;height:30px;background-color:lightgray"></div>');
            //$ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            //// For showing Result li
            //// + '<li><span class="vis-statusbar-statusDB"></span></li>'

            //$liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            //$ulStatusProdDiv.append($liFirst);

            //$liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            //$ulStatusProdDiv.append($liPrev);

            //var $li = $('<li style="float:left"></li>');
            //$li.append($cmbPageNo);
            //$ulStatusProdDiv.append($li);

            //$liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            //$ulStatusProdDiv.append($liNext);

            //$liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            //$ulStatusProdDiv.append($liLast);

            //$statusProdDiv.append($ulStatusProdDiv);

            //$middlePanel.append($statusProdDiv);

            //$divVariantGrid.css("display", "none");

            dVariantGrid.hideColumn('attribute_ID');
            dVariantGrid.hideColumn('M_Product_ID');
        };

        // Bind Variant Grid 
        function bindVariantGrid() {

            if (!initLoad) {
                $bsyDiv[0].style.visibility = "visible";

                grdVariantProdValues = [];

                var Recid = 0;
                dVariantGrid.clear();
                Recid = Recid + 1;

                var sqlVar = "";

                if (selWh.length > 0) {
                    var whString = "";
                    for (var w = 0; w < selWh.length; w++) {
                        if (w == 0) {
                            sqlVar = " SELECT distinct p.Name, p.M_Product_ID, patr.UPC, u.Name as UOM, ats.lot, ats.serno, ats.guaranteedate, bomQtyOnHandAttr(p.M_Product_ID, s.M_AttributeSetInstance_ID ,w.M_Warehouse_ID,0) AS QtyOnHand, "
                + " s.M_AttributeSetInstance_ID, ats.Description FROM M_Storage s INNER JOIN M_AttributeSetInstance ats ON (s.M_AttributeSetInstance_ID = ats.M_AttributeSetInstance_ID) "
                + " INNER JOIN M_LOcator l ON (l.M_Locator_ID = s.M_Locator_ID) INNER JOIN M_Warehouse w ON (w.M_Warehouse_ID = l.M_Warehouse_ID) "
               + " LEFT OUTER JOIN M_ProductAttributes patr ON (patr.M_AttributeSetInstance_ID = ats.M_AttributeSetInstance_ID) INNER JOIN M_Product p ON (p.M_Product_ID = s.M_Product_ID) INNER JOIN C_UOM u ON (p.C_UOM_ID = u.C_UOM_ID) "
                + "WHERE w.M_Warehouse_ID = " + selWh[w] + " AND s.M_Product_ID = " + _product_ID + " AND bomQtyOnHandAttr(p.M_Product_ID, s.M_AttributeSetInstance_ID ,w.M_Warehouse_ID,0) > 0 ";
                        }
                        else {
                            sqlVar += "  UNION SELECT distinct p.Name, p.M_Product_ID, patr.UPC, u.Name as UOM, ats.lot, ats.serno, ats.guaranteedate, bomQtyOnHandAttr(p.M_Product_ID, s.M_AttributeSetInstance_ID ,w.M_Warehouse_ID,0) AS QtyOnHand, "
               + " s.M_AttributeSetInstance_ID, ats.Description FROM M_Storage s INNER JOIN M_AttributeSetInstance ats ON (s.M_AttributeSetInstance_ID = ats.M_AttributeSetInstance_ID) "
               + " INNER JOIN M_LOcator l ON (l.M_Locator_ID = s.M_Locator_ID) INNER JOIN M_Warehouse w ON (w.M_Warehouse_ID = l.M_Warehouse_ID) "
              + " LEFT OUTER JOIN M_ProductAttributes patr ON (patr.M_AttributeSetInstance_ID = ats.M_AttributeSetInstance_ID) INNER JOIN M_Product p ON (p.M_Product_ID = s.M_Product_ID) INNER JOIN C_UOM u ON (p.C_UOM_ID = u.C_UOM_ID) "
               + "WHERE w.M_Warehouse_ID = " + selWh[w] + " AND s.M_Product_ID = " + _product_ID + " AND bomQtyOnHandAttr(p.M_Product_ID, s.M_AttributeSetInstance_ID ,w.M_Warehouse_ID,0) > 0 ";
                        }
                    }
                }
                else {
                    sqlVar = "SELECT distinct p.Name, p.M_Product_ID, patr.UPC, u.Name as UOM, ats.lot, ats.serno, ats.guaranteedate, bomQtyOnHandAttr(p.M_Product_ID, s.M_AttributeSetInstance_ID ,w.M_Warehouse_ID,0) AS QtyOnHand, "
                      + " s.M_AttributeSetInstance_ID, ats.Description FROM M_Storage s INNER JOIN M_AttributeSetInstance ats ON (s.M_AttributeSetInstance_ID = ats.M_AttributeSetInstance_ID) "
                      + " INNER JOIN M_LOcator l ON (l.M_Locator_ID = s.M_Locator_ID) INNER JOIN M_Warehouse w ON (w.M_Warehouse_ID = l.M_Warehouse_ID) "
                      + " LEFT OUTER JOIN M_ProductAttributes patr ON (patr.M_AttributeSetInstance_ID = ats.M_AttributeSetInstance_ID) INNER JOIN M_Product p ON (p.M_Product_ID = s.M_Product_ID) INNER JOIN C_UOM u ON (p.C_UOM_ID = u.C_UOM_ID) "
                      + "WHERE s.M_Product_ID = " + _product_ID + " AND bomQtyOnHandAttr(p.M_Product_ID, s.M_AttributeSetInstance_ID ,w.M_Warehouse_ID,0) > 0 ";
                }

                VIS.DB.executeReader(sqlVar.toString(), null, callbackVariantGrid);

            }
        };

        function callbackVariantGrid(ds) {

            var Recid = 0;
            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;
                    grdVariantProdValues.push(
                    {
                        recid: Recid,
                        Attribute: ds.tables[0].rows[i].cells.description,
                        QtyOnHand: ds.tables[0].rows[i].cells.qtyonhand,
                        UOM: ds.tables[0].rows[i].cells.uom,
                        UPC: ds.tables[0].rows[i].cells.upc,
                        SerialNo: ds.tables[0].rows[i].cells.serno,
                        LotNo: ds.tables[0].rows[i].cells.lot,
                        ExpDate: getDate(ds.tables[0].rows[i].cells.guaranteedate),
                        M_Product_ID: ds.tables[0].rows[i].cells.m_product_id,
                    });
                }
            }

            //while (dr.read()) {
            //    Recid = Recid + 1;
            //    grdVariantProdValues.push(
            //    {
            //        recid: Recid,
            //        //attribute_ID: dr.getString(8),
            //        Attribute: dr.getString(9),
            //        QtyOnHand: dr.getString(7),
            //        UOM: dr.getString(3),
            //        //C_UOM_ID: dr.getString(3),
            //        UPC: dr.getString(2),
            //        SerialNo: dr.getString(5),
            //        LotNo: dr.getString(4),
            //        ExpDate: dr.getString(6),
            //        M_Product_ID: dr.getString(1),
            //    });
            //}
            //dr.close();

            w2utils.encodeTags(grdVariantProdValues);

            dVariantGrid.add(grdVariantProdValues);

            //for (var k = 0; k < w2ui['VA011_gridVariant_' + $self.windowNo].records.length; k++) {
            //    $("#grid_VA011_gridVariant_" + $self.windowNo + "_rec_" + w2ui['VA011_gridVariant_' + $self.windowNo].records[k].recid).find("input[type=text]").val(grdVariantProdValues[k].Attribute);
            //    //var qry = "SELECT M_AttributeSet_ID FROM M_Product WHERE M_Product_ID = " + multiValues[k].product_ID;
            //    //if (VIS.Utility.Util.getValueOfInt(VIS.DB.executeScalar(qry)) > 0) {
            //    //    $("#grid_gridcart_" + $self.windowNo + "_rec_" + w2ui['gridcart_' + $self.windowNo].records[k].recid).find("input[type=text]").css("background-color", "#ffb6c1");
            //    //}
            //}

            grdVariantProdValues = [];

            if (!initLoad) {
                $bsyDiv[0].style.visibility = "hidden";
            }
        };

        // Create Locator Panel Grid at the Bottom
        function gridLocatorPanel() {

            dLocatorGrid = null;
            dLocatorGrid = $divLocatorGrid.w2grid({
                name: 'VA011_gridLocator_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    { field: "Warehouse", caption: VIS.Msg.getMsg("VA011_Warehouse"), sortable: false, size: '15%' },
                    { field: "Locator", caption: VIS.Msg.getMsg("VA011_Locator"), sortable: false, size: '15%' },
                    { field: "Attribute", caption: VIS.Msg.getMsg("VA011_UPC"), sortable: false, size: '20%' },
                    { field: "Quantity", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Quantity") + '</span></div>', sortable: false, size: '15%', hidden: false, render: 'number:1' },
                    { field: "Unconfirmed", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Unconfirmed") + '</span></div>', sortable: false, size: '15%', hidden: false, render: 'number:1' },
                    { field: "LastReceipt", caption: VIS.Msg.getMsg("VA011_LastReceipt"), sortable: false, size: '15%' },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                },
                onClick: function (event) {
                    if (event.column == 5 && dLocatorGrid.records.length > 0) {
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                },
                onSubmit: function (event) {
                },
            });

            //
            //// Product Status Bar Div
            //$statusProdDiv = $('<div style="width:100%;height:30px;background-color:lightgray"></div>');
            //$ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            //// For showing Result li
            //// + '<li><span class="vis-statusbar-statusDB"></span></li>'

            //$liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            //$ulStatusProdDiv.append($liFirst);

            //$liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            //$ulStatusProdDiv.append($liPrev);

            //var $li = $('<li style="float:left"></li>');
            //$li.append($cmbPageNo);
            //$ulStatusProdDiv.append($li);

            //$liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            //$ulStatusProdDiv.append($liNext);

            //$liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            //$ulStatusProdDiv.append($liLast);

            //$statusProdDiv.append($ulStatusProdDiv);

            //$middlePanel.append($statusProdDiv);

            //$divVariantGrid.css("display", "none");

            dLocatorGrid.hideColumn('attribute_ID');

            dLocatorGrid.hideColumn('M_Product_ID');
        };

        // Bind Locator Grid 
        function bindLocatorGrid() {

            if (!initLoad) {
                $bsyDiv[0].style.visibility = "visible";

                grdLocatorProdValues = [];

                var Recid = 0;
                dLocatorGrid.clear();
                Recid = Recid + 1;

                var qryLoc = "";
                var selQuery = "SELECT p.Name, p.M_Product_ID, l.M_Locator_ID, p.Value, w.Name as Warehouse, l.Value as Locator, s.M_AttributeSetInstance_ID, asi.Description, (SELECT MAX(io.DateAcct) FROM M_InoutLine iol "
                             + " INNER JOIN M_Inout io ON (io.M_Inout_ID      = iol.M_Inout_ID) WHERE iol.M_Product_ID = p.M_Product_ID  AND io.IsSOTrx = 'N' AND iol.M_Locator_ID = l.M_Locator_ID) as LastReceipt, "
                             + " (SELECT NVL(SUM(lc.targetqty),0) FROM m_inoutlineconfirm lc INNER JOIN m_inoutconfirm ioc ON (ioc.M_InoutConfirm_ID = lc.M_InoutConfirm_ID) INNER JOIN m_inoutline iol ON (iol.M_Inoutline_ID = lc.m_inoutLine_ID) INNER JOIN m_inout io ON (iol.M_Inout_ID = io.m_inout_ID) "
                             + " WHERE ioc.docstatus NOT IN  ('CO', 'CL')  AND io.IsSOTrx = 'N'  AND iol.M_Locator_ID  = l.M_Locator_ID) as QtyUnconfirmed, "
                             + " SUM(s.QtyOnHand) AS QtyOnHand "
                             + " FROM M_Product p INNER JOIN M_Storage s  ON (s.M_Product_ID = p.M_Product_ID) INNER JOIN M_Locator l ON (l.M_Locator_ID = s.M_Locator_ID) INNER JOIN M_Warehouse w "
                             + " ON (l.M_Warehouse_ID = w.M_Warehouse_ID) LEFT OUTER JOIN M_AttributeSetInstance asi  ON (s.M_AttributeSetInstance_ID = asi.M_AttributeSetInstance_ID) WHERE p.M_Product_ID = " + _product_ID + " AND p.AD_Client_ID = " + VIS.context.getContext("#AD_Client_ID");
                var groupBySec = " GROUP BY p.Name, p.M_Product_ID, l.M_Locator_ID, p.Value, w.Name, l.Value, s.M_AttributeSetInstance_ID, asi.Description, "
                             + " w.M_Warehouse_ID Having bomQtyOnHand(p.M_Product_ID,w.M_Warehouse_ID,l.M_Locator_ID) > 0  ";

                if (selWh.length > 0) {
                    for (var w = 0; w < selWh.length; w++) {
                        if (w == 0) {
                            qryLoc += " ( " + selQuery + " AND w.M_Warehouse_ID = " + selWh[w] + groupBySec;
                        }
                        else {
                            qryLoc += " UNION " + selQuery + " AND w.M_Warehouse_ID = " + selWh[w] + groupBySec;
                        }
                    }
                    qryLoc += "  ) ORDER BY Name";
                }
                else {
                    qryLoc = selQuery + groupBySec + " Order by p.name ";
                }



                VIS.DB.executeDataSet(qryLoc.toString(), null, callbackLocatorGrid);
            }
        };

        function callbackLocatorGrid(ds) {
            var Recid = 0;
            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;

                    grdLocatorProdValues.push(
                    {
                        recid: Recid,
                        Warehouse: ds.tables[0].rows[i].cells.warehouse,
                        Locator: ds.tables[0].rows[i].cells.locator,
                        Quantity: ds.tables[0].rows[i].cells.qtyonhand,
                        Unconfirmed: ds.tables[0].rows[i].cells.qtyunconfirmed,
                        Attribute: ds.tables[0].rows[i].cells.description,
                        LastReceipt: getDate(ds.tables[0].rows[i].cells.lastreceipt),
                        M_Product_ID: ds.tables[0].rows[i].cells.m_product_id,
                    });
                }
            }

            w2utils.encodeTags(grdLocatorProdValues);

            dLocatorGrid.add(grdLocatorProdValues);

            grdLocatorProdValues = [];

            $bsyDiv[0].style.visibility = "hidden";
        };

        // Create Ordered Panel Grid at the Bottom
        function gridOrderedPanel() {


            dOrderedGrid = null;
            dOrderedGrid = $divOrderedGrid.w2grid({
                name: 'VA011_gridOrdered_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    { field: "DatePromised", caption: VIS.Msg.getMsg("VA011_DatePromised"), sortable: false, size: '20%' },
                    { field: "Quantity", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Quantity") + '</span></div>', sortable: false, size: '20%', hidden: false, render: 'number:1' },
                    { field: "DateOrdered", caption: VIS.Msg.getMsg("VA011_DateOrdered"), sortable: false, size: '20%' },
                    { field: "Supplier", caption: VIS.Msg.getMsg("VA011_Supplier"), sortable: false, size: '20%' },
                    { field: "QtyReserved", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_QtyReserved") + '</span></div>', sortable: false, size: '10%', hidden: false, render: 'number:1' },
                    //  { field: "DeliveryTime", caption: VIS.Msg.getMsg("VA011_DeliveryTime"), sortable: false, size: '18%' },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                },
                onClick: function (event) {
                    if (event.column == 5 && dOrderedGrid.records.length > 0) {
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                },
                onSubmit: function (event) {
                },
            });

            //
            //// Product Status Bar Div
            //$statusProdDiv = $('<div style="width:100%;height:30px;background-color:lightgray"></div>');
            //$ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            //// For showing Result li
            //// + '<li><span class="vis-statusbar-statusDB"></span></li>'

            //$liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            //$ulStatusProdDiv.append($liFirst);

            //$liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            //$ulStatusProdDiv.append($liPrev);

            //var $li = $('<li style="float:left"></li>');
            //$li.append($cmbPageNo);
            //$ulStatusProdDiv.append($li);

            //$liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            //$ulStatusProdDiv.append($liNext);

            //$liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            //$ulStatusProdDiv.append($liLast);

            //$statusProdDiv.append($ulStatusProdDiv);

            //$middlePanel.append($statusProdDiv);

            //$divVariantGrid.css("display", "none");

            dOrderedGrid.hideColumn('attribute_ID');
            dOrderedGrid.hideColumn('M_Product_ID');
        };

        // Bind Ordered Grid 
        function bindOrderedGrid() {

            if (!initLoad) {
                $bsyDiv[0].style.visibility = "visible";

                grdOrderedProdValues = [];

                var Recid = 0;
                dOrderedGrid.clear();
                Recid = Recid + 1;

                var whString = "";
                for (var w = 0; w < selWh.length; w++) {
                    if (whString.length > 0) {
                        whString = whString + ", " + selWh[w];
                    }
                    else {
                        whString = whString + selWh[w];
                    }
                }

                var sqlOrd = "SELECT ol.datepromised, o.dateordered, ol.m_product_ID, ol.qtyordered, ol.qtyentered, ol.qtyreserved, bp.name as supplier FROM c_order o INNER JOIN c_Orderline ol ON (ol.c_ORder_ID = o.C_Order_ID) "
                   + " inner join c_bpartner bp on (bp.c_BPartner_ID = o.C_Bpartner_ID) WHERE o.IsSOTrx = 'N' AND o.IsReturnTrx = 'N' AND ol.QtyReserved > 0 AND o.DocStatus IN ('CO', 'CL') AND ol.M_Product_ID = " + _product_ID;

                if (whString.length > 0) {
                    sqlOrd += " AND o.M_Warehouse_ID IN (" + whString + ")";
                }

                sqlOrd += " ORDER BY o.Created DESC";

                VIS.DB.executeDataSet(sqlOrd.toString(), null, callbackOrderedGrid);
            }
        };

        function callbackOrderedGrid(ds) {
            var Recid = 0;
            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;

                    grdOrderedProdValues.push(
                {
                    recid: Recid,
                    DatePromised: getDate(ds.tables[0].rows[i].cells.datepromised),
                    Quantity: ds.tables[0].rows[i].cells.qtyordered,
                    DateOrdered: getDate(ds.tables[0].rows[i].cells.dateordered),
                    Supplier: ds.tables[0].rows[i].cells.supplier,
                    QtyReserved: ds.tables[0].rows[i].cells.qtyreserved,
                    M_Product_ID: 0,
                });
                }
            }
            w2utils.encodeTags(grdOrderedProdValues);

            dOrderedGrid.add(grdOrderedProdValues);

            grdOrderedProdValues = [];

            $bsyDiv[0].style.visibility = "hidden";
        };

        // Create Replenished Panel Grid at the Bottom
        function gridReplenishedPanel() {

            dReplenishedGrid = null;
            dReplenishedGrid = $divReplenishedGrid.w2grid({
                name: 'VA011_gridReplenished_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    { field: "RequisitionNo", caption: VIS.Msg.getMsg("VA011_RequisitionNo"), sortable: false, size: '20%' },
                    { field: "Date", caption: VIS.Msg.getMsg("VA011_Date"), sortable: false, size: '20%' },
                    { field: "QtyDemanded", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_QtyDemanded") + '</span></div>', sortable: false, size: '20%', hidden: false, render: 'number:1' },
                    { field: "QtyReceived", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_QtyReceived") + '</span></div>', sortable: false, size: '20%', hidden: false, render: 'number:1' },
                    { field: "QtyPending", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_QtyPending") + '</span></div>', sortable: false, size: '20%', hidden: false, render: 'number:1' },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                },
                onClick: function (event) {
                    if (event.column == 5 && dReplenishedGrid.records.length > 0) {
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                },
                onSubmit: function (event) {
                },
            });

            //
            //// Product Status Bar Div
            //$statusProdDiv = $('<div style="width:100%;height:30px;background-color:lightgray"></div>');
            //$ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            //// For showing Result li
            //// + '<li><span class="vis-statusbar-statusDB"></span></li>'

            //$liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            //$ulStatusProdDiv.append($liFirst);

            //$liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            //$ulStatusProdDiv.append($liPrev);

            //var $li = $('<li style="float:left"></li>');
            //$li.append($cmbPageNo);
            //$ulStatusProdDiv.append($li);

            //$liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            //$ulStatusProdDiv.append($liNext);

            //$liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            //$ulStatusProdDiv.append($liLast);

            //$statusProdDiv.append($ulStatusProdDiv);

            //$middlePanel.append($statusProdDiv);

            //$divVariantGrid.css("display", "none");

            dReplenishedGrid.hideColumn('attribute_ID');
            dReplenishedGrid.hideColumn('M_Product_ID');
        };

        // Bind Replenished Grid 
        function bindReplenishedGrid() {

            if (!initLoad) {
                $bsyDiv[0].style.visibility = "visible";

                grdReplenishedProdValues = [];

                var Recid = 0;
                dReplenishedGrid.clear();
                Recid = Recid + 1;

                // VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/GetReplenishments", { "Warehouses": selWh, "M_Product_ID": _product_ID }, callbackReplenishGrid);

                if (selWh.length > 0) {
                    for (var w = 0; w < selWh.length; w++) {
                        var sqlRep = "SELECT r.DocumentNo, r.datedoc, rl.Qty, rl.M_Product_ID, rl.DTD001_DeliveredQty, CASE WHEN (rl.Qty - rl.DTD001_DeliveredQty) > 0 THEN (rl.Qty - rl.DTD001_DeliveredQty) ELSE 0 END AS QtyPending "
                        + " FROM m_requisitionline rl INNER JOIN M_Requisition r ON (r.M_Requisition_ID = rl.M_Requisition_ID) INNER JOIN m_product p ON (p.M_Product_ID = rl.M_Product_ID) WHERE rl.M_Product_ID = " + _product_ID
                        + " AND r.M_Warehouse_ID = " + selWh[w];

                        var dsRep = VIS.DB.executeDataSet(sqlRep.toString(), null, null);
                        if (dsRep != null && dsRep.tables[0].rows.length > 0) {
                            for (var i = 0; i < dsRep.tables[0].rows.length; i++) {
                                Recid = Recid + 1;
                                grdReplenishedProdValues.push(
                                {
                                    recid: Recid,
                                    RequisitionNo: dsRep.tables[0].rows[i].cells.documentno,
                                    Date: getDate(dsRep.tables[0].rows[i].cells.datedoc),
                                    QtyDemanded: dsRep.tables[0].rows[i].cells.qty,
                                    QtyReceived: dsRep.tables[0].rows[i].cells.dtd001_deliveredqty,
                                    QtyPending: dsRep.tables[0].rows[i].cells.qtypending,
                                    // LastReceipt: dsRep.tables[0].rows[i].cells.lastreceipt,
                                    M_Product_ID: dsRep.tables[0].rows[i].cells.m_product_id,
                                });
                            }
                        }

                        w2utils.encodeTags(grdReplenishedProdValues);
                    }
                }
                else {
                    for (var w = 0; w < selWh.length; w++) {
                        var sqlRep = "SELECT r.DocumentNo, r.datedoc, rl.Qty, rl.M_Product_ID, rl.DTD001_DeliveredQty, CASE WHEN (rl.Qty - rl.DTD001_DeliveredQty) > 0 THEN (rl.Qty - rl.DTD001_DeliveredQty) ELSE 0 END AS QtyPending "
                        + " FROM m_requisitionline rl INNER JOIN M_Requisition r ON (r.M_Requisition_ID = rl.M_Requisition_ID) INNER JOIN m_product p ON (p.M_Product_ID = rl.M_Product_ID) WHERE rl.M_Product_ID = " + _product_ID;

                        var dsRep = VIS.DB.executeDataSet(sqlRep.toString(), null, null);
                        if (dsRep != null && dsRep.tables[0].rows.length > 0) {
                            for (var i = 0; i < dsRep.tables[0].rows.length; i++) {
                                Recid = Recid + 1;
                                grdReplenishedProdValues.push(
                                {
                                    recid: Recid,
                                    RequisitionNo: dsRep.tables[0].rows[i].cells.documentno,
                                    Date: getDate(dsRep.tables[0].rows[i].cells.datedoc),
                                    QtyDemanded: dsRep.tables[0].rows[i].cells.qty,
                                    QtyReceived: dsRep.tables[0].rows[i].cells.dtd001_deliveredqty,
                                    QtyPending: dsRep.tables[0].rows[i].cells.qtypending,
                                    // LastReceipt: dsRep.tables[0].rows[i].cells.lastreceipt,
                                    M_Product_ID: dsRep.tables[0].rows[i].cells.m_product_id,
                                });
                            }
                        }

                        w2utils.encodeTags(grdReplenishedProdValues);
                    }
                }

                dReplenishedGrid.add(grdReplenishedProdValues);

                grdReplenishedProdValues = [];

                $bsyDiv[0].style.visibility = "hidden";
            }
        };

        function callbackReplenishGrid(ds) {

            var Recid = 0;

            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;

                    grdReplenishedProdValues.push(
                                 {
                                     recid: Recid,
                                     RequisitionNo: dsRep.tables[0].rows[i].cells.documentno,
                                     Date: getDate(dsRep.tables[0].rows[i].cells.datedoc),
                                     QtyDemanded: dsRep.tables[0].rows[i].cells.qty,
                                     QtyReceived: dsRep.tables[0].rows[i].cells.dtd001_deliveredqty,
                                     QtyPending: dsRep.tables[0].rows[i].cells.qtypending,
                                     // LastReceipt: dsRep.tables[0].rows[i].cells.lastreceipt,
                                     M_Product_ID: dsRep.tables[0].rows[i].cells.m_product_id,
                                 });
                }
            }

            w2utils.encodeTags(grdReplenishedProdValues);

            dReplenishedGrid.add(grdReplenishedProdValues);

            grdReplenishedProdValues = [];

            $bsyDiv[0].style.visibility = "hidden";
        };

        // Create Demand Panel Grid at the Bottom
        function gridDemandPanel() {

            dDemandGrid = null;
            dDemandGrid = $divDemandGrid.w2grid({
                name: 'VA011_gridDemand_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    { field: "DocumentType", caption: VIS.Msg.getMsg("VA011_DocumentType"), sortable: false, size: '16%' },
                    { field: "DocumentNo", caption: VIS.Msg.getMsg("VA011_DocumentNo"), sortable: false, size: '16%' },
                    { field: "Quantity", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Quantity") + '</span></div>', sortable: false, size: '16%', hidden: false, render: 'number:1' },
                    { field: "DatePromised", caption: VIS.Msg.getMsg("VA011_DatePromised"), sortable: false, size: '16%' },
                    { field: "DemandedBy", caption: VIS.Msg.getMsg("VA011_DemandedBy"), sortable: false, size: '18%' },
                    { field: "AvailabilityStatus", caption: VIS.Msg.getMsg("VA011_AvailabilityStatus"), sortable: false, size: '18%', display: false },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                },
                onClick: function (event) {
                    if (event.column == 5 && dDemandGrid.records.length > 0) {
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                },
                onSubmit: function (event) {
                },
            });

            //
            //// Product Status Bar Div
            //$statusProdDiv = $('<div style="width:100%;height:30px;background-color:lightgray"></div>');
            //$ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            //// For showing Result li
            //// + '<li><span class="vis-statusbar-statusDB"></span></li>'

            //$liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            //$ulStatusProdDiv.append($liFirst);

            //$liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            //$ulStatusProdDiv.append($liPrev);

            //var $li = $('<li style="float:left"></li>');
            //$li.append($cmbPageNo);
            //$ulStatusProdDiv.append($li);

            //$liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            //$ulStatusProdDiv.append($liNext);

            //$liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            //$ulStatusProdDiv.append($liLast);

            //$statusProdDiv.append($ulStatusProdDiv);

            //$middlePanel.append($statusProdDiv);

            //$divVariantGrid.css("display", "none");

            dDemandGrid.hideColumn('attribute_ID');
            dDemandGrid.hideColumn('AvailabilityStatus');
            dDemandGrid.hideColumn('M_Product_ID');
        };

        // Bind Demand Grid 
        function bindDemandGrid() {

            if (!initLoad) {
                $bsyDiv[0].style.visibility = "visible";

                grdDemandProdValues = [];

                var Recid = 0;
                dDemandGrid.clear();
                Recid = Recid + 1;

                var sqlDmd = "SELECT * FROM ( SELECT o.Created, o.documentno, ol.QtyReserved AS qtyentered, dt.name as DocType, o.datepromised, bp.name as demandedby FROM C_Order o INNER JOIN c_orderline ol ON (ol.C_Order_ID = o.C_Order_ID) "
                + " INNER JOIN C_DocType dt ON (o.C_DocTypeTarget_ID = dt.C_DocType_ID) INNER JOIN C_BPartner bp ON (bp.C_BPartner_ID = o.C_BPartner_ID) WHERE o.IsSOTrx = 'Y' AND o.IsReturnTrx = 'N' AND o.DocStatus IN ('CO', 'CL') AND ol.QtyReserved >0 AND ol.M_Product_ID = " + _product_ID;

                var orgString = "";
                if (selOrgs.length > 0) {
                    for (var w = 0; w < selOrgs.length; w++) {
                        if (orgString.length > 0) {
                            orgString = orgString + ", " + selOrgs[w];
                        }
                        else {
                            orgString += "0, ";
                            orgString += selOrgs[w];
                        }
                    }
                    sqlDmd += " AND o.AD_Org_ID IN (0, " + orgString + ")";
                }

                var whString = "";
                if (selWh.length > 0) {
                    for (var w = 0; w < selWh.length; w++) {
                        if (whString.length > 0) {
                            whString = whString + ", " + selWh[w];
                        }
                        else {
                            whString = whString + selWh[w];
                        }
                    }
                    sqlDmd += " AND o.M_Warehouse_ID IN (" + whString + ")";
                }

                sqlDmd += " UNION SELECT r.Created, r.DocumentNo, rl.DTD001_ReservedQty as qtyentered, dt.name AS DocType, r.daterequired as datepromised, w.name AS demandedby  FROM m_requisitionline rl "
                    + " INNER JOIN M_Requisition r ON (r.M_Requisition_ID = rl.M_Requisition_ID) INNER JOIN C_DocType dt ON (r.C_DocType_ID = dt.C_DocType_ID) INNER JOIN M_Warehouse w "
                    + " ON (r.M_Warehouse_ID = w.M_Warehouse_ID) INNER JOIN m_product p ON (p.M_Product_ID = rl.M_Product_ID) WHERE r.DocStatus IN ('CO', 'CL') AND rl.DTD001_ReservedQty > 0 AND r.IsActive = 'Y' AND rl.M_Product_ID = " + _product_ID;
                if (orgString.length > 0) {
                    sqlDmd += " AND r.AD_Org_ID IN (0, " + orgString + ")";
                }

                if (whString.length > 0) {
                    sqlDmd += " AND r.M_Warehouse_ID IN (0, " + whString + ")";
                }

                // Append Manufacturing Order Records only if Manufacturing Module Exists
                var sqlMFGModule = "SELECT COUNT(*) FROM AD_ModuleInfo WHERE Prefix = 'VAMFG_' AND IsActive = 'Y'";
                var moduleExists = VIS.DB.executeScalar(sqlMFGModule.toString(), null, null);
                if (moduleExists > 0) {
                    sqlDmd += " UNION SELECT wo.Created, wo.documentno, wo.vamfg_qtyentered AS qtyentered, dt.Name AS doctype, wo.VAMFG_DateScheduleTo as datepromised, bp.name as demandedby FROM vamfg_M_workorder wo "
                        + " INNER JOIN c_doctype dt ON (dt.C_DocType_ID = wo.C_DocType_ID) LEFT OUTER JOIN C_BPartner bp ON (bp.C_BPartner_ID = wo.C_BPartner_ID) INNER JOIN m_product p "
                        + " ON (p.M_Product_ID = wo.M_Product_ID) WHERE wo.DocStatus IN ('CO', 'CL') AND wo.IsActive   = 'Y' AND wo.M_Product_ID = " + _product_ID;
                    if (orgString.length > 0) {
                        sqlDmd += " AND wo.AD_Org_ID IN (0, " + orgString + ")";
                    }

                    if (whString.length > 0) {
                        sqlDmd += " AND wo.M_Warehouse_ID IN (0, " + whString + ")";
                    }
                }

                sqlDmd += ") ORDER BY Created DESC";
                // Append Manufacturing Order Records only if Manufacturing Module Exists



                VIS.DB.executeDataSet(sqlDmd.toString(), null, callbackDemandGrid);
            }
        };

        function callbackDemandGrid(ds) {
            var Recid = 0;

            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;
                    grdDemandProdValues.push(
                        {
                            recid: Recid,
                            DocumentType: ds.tables[0].rows[i].cells.doctype,
                            DocumentNo: ds.tables[0].rows[i].cells.documentno,
                            Quantity: ds.tables[0].rows[i].cells.qtyentered,
                            DatePromised: getDate(ds.tables[0].rows[i].cells.datepromised),
                            DemandedBy: ds.tables[0].rows[i].cells.demandedby,
                            AvailabilityStatus: "",
                            M_Product_ID: 0,
                        });
                }
            }

            w2utils.encodeTags(grdDemandProdValues);

            dDemandGrid.add(grdDemandProdValues);

            grdDemandProdValues = [];

            $bsyDiv[0].style.visibility = "hidden";
        };

        // Create Transactions Panel Grid at the Bottom
        function gridTransactionsPanel() {

            dTransactionsGrid = null;
            dTransactionsGrid = $divTransactionsGrid.w2grid({
                name: 'VA011_gridTransactions_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    { field: "DocumentType", caption: VIS.Msg.getMsg("VA011_DocumentType"), sortable: false, size: '12%' },
                    { field: "DocumentNo", caption: VIS.Msg.getMsg("VA011_DocumentNo"), sortable: false, size: '12%' },
                    { field: "Locator", caption: VIS.Msg.getMsg("VA011_Locator"), sortable: false, size: '12%' },
                    { field: "Date", caption: VIS.Msg.getMsg("VA011_Date"), sortable: false, size: '12%' },
                    { field: "InventoryIn", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_InventoryIn") + '</span></div>', sortable: false, size: '9%', hidden: false, render: 'number:1' },
                    { field: "InventoryOut", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_InventoryOut") + '</span></div>', sortable: false, size: '9%', hidden: false, render: 'number:1' },
                    { field: "Attribute", caption: VIS.Msg.getMsg("VA011_Attribute"), sortable: false, size: '20%' },
                    //{ field: "attribute_ID", caption: "", sortable: false, size: '80px', display: false },
                    //{
                    //    field: "Attribute", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_AttributeTransactions") + '</span></div>', sortable: false, size: '25%', hidden: false,
                    //    render: function () {
                    //        return '<div><input type=text readonly="readonly" style= "width:85%; border:none" ></input><img src="' + VIS.Application.contextUrl + 'Areas/VIS/Images/base/MultiX16.png" alt="Attribute Set Instance" title="Attribute Set Instance" style="opacity:1;float:right;"></div>';
                    //    }
                    //},
                    { field: "Balance", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Balance") + '</span></div>', sortable: false, size: '9%', hidden: false, render: 'number:1' },
                    //{
                    //    field: "C_UOM_ID", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("C_UOM_ID") + '</span></div>', sortable: false, size: '12%', hidden: false, editable: { type: 'select', items: uomArray, showAll: true },
                    //    render: function (record, index, col_index) {
                    //        var html = '';
                    //        for (var p in uomArray) {
                    //            if (uomArray[p].id == this.getCellValue(index, col_index)) html = uomArray[p].text;
                    //        }
                    //        return html;
                    //    }
                    //},
                    //{ field: "UPC", caption: VIS.Msg.getMsg("VA011_UPC"), sortable: false, size: '80px' },
                    //{ field: "SerialNo", caption: VIS.Msg.getMsg("VA011_SerialNo"), sortable: false, size: '80px' },
                    //{ field: "LotNo", caption: VIS.Msg.getMsg("VA011_LotNo"), sortable: false, size: '80px' },
                    //{ field: "ExpDate", caption: VIS.Msg.getMsg("VA011_ExpiryDate"), sortable: false, size: '80px' },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                },
                onClick: function (event) {
                    if (event.column == 5 && dTransactionsGrid.records.length > 0) {
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                },
                onSubmit: function (event) {
                },
            });

            //
            //// Product Status Bar Div
            //$statusProdDiv = $('<div style="width:100%;height:30px;background-color:lightgray"></div>');
            //$ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            //// For showing Result li
            //// + '<li><span class="vis-statusbar-statusDB"></span></li>'

            //$liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            //$ulStatusProdDiv.append($liFirst);

            //$liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            //$ulStatusProdDiv.append($liPrev);

            //var $li = $('<li style="float:left"></li>');
            //$li.append($cmbPageNo);
            //$ulStatusProdDiv.append($li);

            //$liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            //$ulStatusProdDiv.append($liNext);

            //$liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            //$ulStatusProdDiv.append($liLast);

            //$statusProdDiv.append($ulStatusProdDiv);

            //$middlePanel.append($statusProdDiv);

            //$divVariantGrid.css("display", "none");

            dTransactionsGrid.hideColumn('attribute_ID');

            dTransactionsGrid.hideColumn('M_Product_ID');
        };

        // Bind Transactions Grid 
        function bindTransactionsGrid() {

            if (!initLoad) {
                $bsyDiv[0].style.visibility = "visible";

                grdTransactionsProdValues = [];

                var Recid = 0;
                dTransactionsGrid.clear();
                Recid = Recid + 1;

                var sqlTrx = "SELECT t.M_Product_ID, t.M_AttributeSetInstance_ID, asi.description, t.MOVEMENTQTY , t.MOVEMENTTYPE, t.CurrentQty, t.MovementDate, lc.Value AS Locator, t.M_Transaction_ID, iol.M_InOutLine_ID, ivl.M_InventoryLine_ID, mvl.M_MovementLine_ID, "
                + " Case when t.MovementQty > 0 Then t.movementQty else 0 end as InventoryIn, Case when t.MovementQty < 0 Then t.movementQty else 0 end as InventoryOut, "
                + " CASE WHEN NVL(iol.M_InOutLine_ID,0) > 0 THEN iol.M_InoutLine_ID WHEN NVL(ivl.M_InventoryLine_ID,0) > 0 THEN ivl.M_InventoryLine_ID WHEN NVL(mvl.M_MovementLine_ID,0) > 0 THEN mvl.M_MovementLine_ID END AS ID, "
                + " CASE WHEN NVL(iol.M_InOutLine_ID,0) > 0 THEN io.DocumentNo WHEN NVL(ivl.M_InventoryLine_ID,0) > 0 THEN iv.DocumentNo WHEN NVL(mvl.M_MovementLine_ID,0) > 0 THEN mv.DocumentNo END AS DocumentNo,"
                + " CASE WHEN NVL(iol.M_InOutLine_ID,0) > 0 THEN dtio.Name WHEN NVL(ivl.M_InventoryLine_ID,0) > 0 THEN dtiv.Name WHEN NVL(mvl.M_MovementLine_ID,0) > 0 THEN dtmv.Name END AS DocType "
                + " FROM M_Transaction t LEFT OUTER JOIN M_InoutLine iol ON (iol.M_InOutLine_ID = t.M_InOutLine_ID) LEFT OUTER JOIN M_Inout io ON (iol.M_InOut_ID = io.M_InOut_ID) LEFT OUTER JOIN C_DocType dtio "
                + " ON (dtio.C_DocType_ID = io.C_DocType_ID) LEFT OUTER JOIN M_inventoryLine ivl ON (ivl.M_inventoryLine_ID = t.M_inventoryLine_ID) LEFT OUTER JOIN M_Inventory iv ON (ivl.M_Inventory_ID = iv.M_Inventory_ID) "
                + " LEFT OUTER JOIN C_DocType dtiv ON (dtiv.C_DocType_ID = iv.C_DocType_ID) LEFT OUTER JOIN M_MovementLine mvl ON (mvl.M_MovementLine_ID = t.M_MovementLine_ID) LEFT OUTER JOIN M_Movement mv "
                + " ON (mvl.M_Movement_ID = mv.M_Movement_ID) LEFT OUTER JOIN C_DocType dtmv ON (dtmv.C_DocType_ID = mv.C_DocType_ID) LEFT OUTER JOIN M_AttributeSetInstance asi  ON (t.M_AttributeSetInstance_ID = asi.M_AttributeSetInstance_ID) LEFT OUTER JOIN M_Locator lc ON (lc.M_Locator_ID = t.M_Locator_ID) WHERE t.M_Product_ID = " + _product_ID;

                if (selWh.length > 0) {
                    var whString = "";
                    for (var w = 0; w < selWh.length; w++) {
                        if (whString.length > 0) {
                            whString = whString + ", " + selWh[w];
                        }
                        else {
                            whString = whString + selWh[w];
                        }
                    }
                    sqlTrx += " AND t.M_Locator_ID IN (Select loc.M_Locator_ID from m_warehouse wh inner join m_locator loc on (loc.m_warehouse_ID = wh.M_Warehouse_ID) where wh.m_warehouse_ID in (" + whString + ")) ";
                }

                sqlTrx += " ORDER BY t.MovementDate DESC";

                /*Left outer join M_ProductionLine pdl
                on (pdl.M_ProductionLine_ID = t.M_ProductionLine_ID)
                Left outer join M_Production pd
                on (pdl.M_Production_ID = pd.M_Production_ID)*/

                VIS.DB.executeDataSet(sqlTrx.toString(), null, callbackBindTransactionsGrid);

            }
        };

        function callbackBindTransactionsGrid(ds) {


            var Recid = 0;

            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;
                    var invIn = "";
                    var InvOut = "";
                    if (ds.tables[0].rows[i].cells.inventoryin > 0) {
                        invIn = ds.tables[0].rows[i].cells.inventoryin;
                    }

                    if (ds.tables[0].rows[i].cells.inventoryout < 0) {
                        InvOut = ds.tables[0].rows[i].cells.inventoryout;
                    }

                    grdTransactionsProdValues.push(
                {
                    recid: Recid,
                    DocumentType: ds.tables[0].rows[i].cells.doctype,
                    DocumentNo: ds.tables[0].rows[i].cells.documentno,
                    Locator: ds.tables[0].rows[i].cells.locator,
                    Date: getDate(ds.tables[0].rows[i].cells.movementdate),
                    InventoryIn: invIn,
                    InventoryOut: InvOut,
                    // attribute_ID: "",
                    Attribute: ds.tables[0].rows[i].cells.description,
                    Balance: ds.tables[0].rows[i].cells.currentqty,
                    // C_UOM_ID: "",
                    M_Product_ID: 0,
                });
                }
            }

            w2utils.encodeTags(grdTransactionsProdValues);

            dTransactionsGrid.add(grdTransactionsProdValues);

            grdTransactionsProdValues = [];

            $bsyDiv[0].style.visibility = "hidden";
        };


        // Create ReplenishmentPop Panel Grid 
        function gridReplenishmentPopPanel() {

            dReplenishmentPopGrid = null;
            dReplenishmentPopGrid = $divReplenishmentPopGrid.w2grid({
                name: 'VA011_gridReplenishmentPop_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                { field: "Product", caption: VIS.Msg.getMsg("VA011_Product"), sortable: false, size: '20%' },
    //{
    //    field: "Product", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Product") + '</span></div>', sortable: false, size: '20%', hidden: false, editable: { type: 'select', items: listProductsAll, showAll: true },
    //    render: function (record, index, col_index) {
    //        var html = '';
    //        for (var p in listProductsAll) {
    //            if (listProductsAll[p].id == this.getCellValue(index, col_index)) html = listProductsAll[p].text;
    //        }
    //        return html;
    //    }
    //},
                    //{ field: "Warehouse", caption: VIS.Msg.getMsg("VA011_Warehouse"), sortable: false, size: '20%' },
                    //{
                    //    field: "Warehouse", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Warehouse") + '</span></div>', sortable: false, size: '20%', hidden: false, editable: { type: 'select', items: listWarehouses, showAll: true },
                    //    render: function (record, index, col_index) {
                    //        var html = '';
                    //        for (var p in listWarehouses) {
                    //            if (listWarehouses[p].id == this.getCellValue(index, col_index)) html = listWarehouses[p].text;
                    //        }
                    //        return html;
                    //    }
                    //},
                    {
                        field: "Type", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Type") + '</span></div>', sortable: false, size: '20%', hidden: false, editable: { type: 'select', items: repTypeArray, showAll: true },
                        render: function (record, index, col_index) {
                            var html = '';
                            for (var p in repTypeArray) {
                                if (repTypeArray[p].id == this.getCellValue(index, col_index)) html = repTypeArray[p].text;
                            }
                            return html;
                        }
                    },
                    { field: "Min", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Min") + '</span></div>', sortable: false, size: '8%', hidden: false, render: 'number:1', editable: { type: 'float' } },
                    { field: "Max", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Max") + '</span></div>', sortable: false, size: '8%', hidden: false, render: 'number:1', editable: { type: 'float' } },
                    { field: "Qty", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_MinOrderQuantity") + '</span></div>', sortable: false, size: '8%', hidden: false, render: 'number:1', editable: { type: 'float' } },
                    { field: "OrderPack", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_OrderPack") + '</span></div>', sortable: false, size: '8%', hidden: false, render: 'number:1', editable: { type: 'float' } },
                    //{ field: "SourceWarehouse", caption: VIS.Msg.getMsg("VA011_SourceWarehouse"), sortable: false, size: '20%' },
                    {
                        field: "SourceWarehouse", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_SourceWarehouse") + '</span></div>', sortable: false, size: '20%', hidden: false, editable: { type: 'select', items: listWarehouses, showAll: true },
                        render: function (record, index, col_index) {
                            var html = '';
                            for (var p in listWarehouses) {
                                if (listWarehouses[p].id == this.getCellValue(index, col_index)) html = listWarehouses[p].text;
                            }
                            return html;
                        }
                    },

                    //{ field: "attribute_ID", caption: "", sortable: false, size: '80px', display: false },
                    //{
                    //    field: "Attribute", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_AttributeTransactions") + '</span></div>', sortable: false, size: '25%', hidden: false,
                    //    render: function () {
                    //        return '<div><input type=text readonly="readonly" style= "width:85%; border:none" ></input><img src="' + VIS.Application.contextUrl + 'Areas/VIS/Images/base/MultiX16.png" alt="Attribute Set Instance" title="Attribute Set Instance" style="opacity:1;float:right;"></div>';
                    //    }
                    //},
                    //{
                    //    field: "C_UOM_ID", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("C_UOM_ID") + '</span></div>', sortable: false, size: '12%', hidden: false, editable: { type: 'select', items: uomArray, showAll: true },
                    //    render: function (record, index, col_index) {
                    //        var html = '';
                    //        for (var p in uomArray) {
                    //            if (uomArray[p].id == this.getCellValue(index, col_index)) html = uomArray[p].text;
                    //        }
                    //        return html;
                    //    }
                    //},
                    //{ field: "UPC", caption: VIS.Msg.getMsg("VA011_UPC"), sortable: false, size: '80px' },
                    //{ field: "SerialNo", caption: VIS.Msg.getMsg("VA011_SerialNo"), sortable: false, size: '80px' },
                    //{ field: "LotNo", caption: VIS.Msg.getMsg("VA011_LotNo"), sortable: false, size: '80px' },
                    //{ field: "ExpDate", caption: VIS.Msg.getMsg("VA011_ExpiryDate"), sortable: false, size: '80px' },
                    //{
                    //    field: "Save", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Save") + '</span></div>', sortable: false, size: '8%', hidden: false,
                    //    render: function () {
                    //        return '<div style="text-align: center;"><img src="' + VIS.Application.contextUrl + 'Areas/VIS/Images/Save22.png" alt="Attribute Set Instance" title="Attribute Set Instance" style="opacity:1;"></div>';
                    //    }
                    //},
                    //{ field: "M_Warehouse_ID", caption: "M_Warehouse_ID", sortable: false, size: '80px', display: false },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                    debugger;
                    //w2ui['gridprice_' + $self.windowNo].records[event.index]["updated"] = true;
                    if (event.column == 0) {
                        w2ui['VA011_gridReplenishmentPop_' + $self.windowNo].records[event.index]["Product"] = event.value_new;
                    }
                    //if (event.column == 1) {
                    //    w2ui['VA011_gridReplenishmentPop_' + $self.windowNo].records[event.index]["Warehouse"] = event.value_new;
                    //}
                    if (event.column == 1) {
                        w2ui['VA011_gridReplenishmentPop_' + $self.windowNo].records[event.index]["Type"] = event.value_new;
                    }
                    if (event.column == 2) {
                        w2ui['VA011_gridReplenishmentPop_' + $self.windowNo].records[event.index]["Min"] = event.value_new;
                    }
                    else if (event.column == 3) {
                        w2ui['VA011_gridReplenishmentPop_' + $self.windowNo].records[event.index]["Max"] = event.value_new;
                    }
                    else if (event.column == 4) {
                        w2ui['VA011_gridReplenishmentPop_' + $self.windowNo].records[event.index]["Qty"] = event.value_new;
                    }
                    else if (event.column == 5) {
                        w2ui['VA011_gridReplenishmentPop_' + $self.windowNo].records[event.index]["OrderPack"] = event.value_new;
                    }
                    else if (event.column == 6) {
                        w2ui['VA011_gridReplenishmentPop_' + $self.windowNo].records[event.index]["SourceWarehouse"] = event.value_new;
                    }
                },
                onClick: function (event) {

                    if (event.column == 8 && dReplenishmentPopGrid.records.length > 0) {
                        if (dReplenishmentPopGrid.records[event.recid - 1] != null) {

                            $bsyDiv[0].style.visibility = "visible";
                            debugger;
                            var warehoue_ID = $cmbRepAllWarehouse.val();
                            var prod_ID = dReplenishmentPopGrid.records[event.recid - 1].M_Product_ID;
                            var type = dReplenishmentPopGrid.records[event.recid - 1].Type;
                            var min = dReplenishmentPopGrid.records[event.recid - 1].Min;
                            var max = dReplenishmentPopGrid.records[event.recid - 1].Max;
                            var qty = dReplenishmentPopGrid.records[event.recid - 1].Qty;
                            var SourceWarehouse = dReplenishmentPopGrid.records[event.recid - 1].SourceWarehouse;

                            if (prod_ID == null || prod_ID == "") {
                                alert(VIS.Msg.getMsg("VA011_ProductMandatory"));
                                $bsyDiv[0].style.visibility = "hidden";
                                return;
                            }
                            if (warehoue_ID == null || warehoue_ID == "") {
                                alert(VIS.Msg.getMsg("VA011_WarehouseMandatory"));
                                $bsyDiv[0].style.visibility = "hidden";
                                return;
                            }

                            if (type == null || type == "") {
                                alert(VIS.Msg.getMsg("VA011_ReplenishTypeMandatory"));
                                $bsyDiv[0].style.visibility = "hidden";
                                return;
                            }

                            if (SourceWarehouse == null || SourceWarehouse == "") {
                                SourceWarehouse = 0;
                            }
                            if (qty == null || qty == "") {
                                qty = 0;
                            }
                            if (min == null || min == "") {
                                min = 0;
                            }
                            if (max == null || max == "") {
                                max = 0;
                            }
                            var orderPack = dReplenishmentPopGrid.records[event.recid - 1].OrderPack;
                            if (orderPack == null || orderPack == "") {
                                orderPack = 0;
                            }

                            VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/SaveReplenishment", { "M_Product_ID": prod_ID, "M_Warehouse_ID": warehoue_ID, "Type": type, "Min": min, "Max": max, "Qty": qty, "OrderPack": orderPack, "SourceWarehouse": SourceWarehouse }, callbackSaveReplenishmentPop);

                        }
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                },
                onSubmit: function (event) {
                },
            });

            //
            //// Product Status Bar Div
            //$statusProdDiv = $('<div style="width:100%;height:30px;background-color:lightgray"></div>');
            //$ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            //// For showing Result li
            //// + '<li><span class="vis-statusbar-statusDB"></span></li>'

            //$liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            //$ulStatusProdDiv.append($liFirst);

            //$liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            //$ulStatusProdDiv.append($liPrev);

            //var $li = $('<li style="float:left"></li>');
            //$li.append($cmbPageNo);
            //$ulStatusProdDiv.append($li);

            //$liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            //$ulStatusProdDiv.append($liNext);

            //$liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            //$ulStatusProdDiv.append($liLast);

            //$statusProdDiv.append($ulStatusProdDiv);

            //$middlePanel.append($statusProdDiv);

            //$divVariantGrid.css("display", "none");

            dReplenishmentPopGrid.hideColumn('M_Warehouse_ID');

            dReplenishmentPopGrid.hideColumn('M_Product_ID');
        };

        function callbackSaveReplenishmentPop(data) {
            dProdGrid.selectNone();
            disableSectionsAfterReplenishPop();
            //$bsyDiv[0].style.visibility = "hidden";
            //bindReplenishmentPopGrid();
        };

        // Bind Replenishment Popup Grid 
        function bindReplenishmentPopGrid() {

            if (!initLoad) {

                $bsyDiv[0].style.visibility = "visible";

                grdReplenishmentPopValues = [];

                var Recid = 0;
                if (dReplenishmentPopGrid != null) {
                    dReplenishmentPopGrid.clear();
                }
                Recid = Recid + 1;

                var sqlDTDModuleChk = "SELECT COUNT(*) FROM AD_ModuleInfo WHERE Prefix = 'DTD001_'";

                VIS.DB.executeScalar(sqlDTDModuleChk.toString(), null, callbackModuleCheckRepAll);

                //$bsyDiv[0].style.visibility = "hidden";
            }
        };

        function callbackModuleCheckRepAll(moduleExists) {
            debugger;
            var sqlRep = "";

            if (moduleExists > 0) {
                sqlRep = "SELECT p.Name AS Product, w.Name      AS Warehouse, w.M_Warehouse_ID, NVL(rep.Level_Max,0) AS Maxi, NVL(rep.Level_Min,0)            AS Mini,  NVL(rep.ReplenishType,0)        AS rtype, "
                + " NVL(DTD001_MinOrderQty,0)       AS Qty,  NVL(DTD001_OrderPackQty,0)      AS OrderPack,  rep.M_WarehouseSource_ID AS SourceWarehouse,  p.M_Product_ID FROM M_Product p "
                + " Left Outer Join M_Replenish rep ON (p.M_Product_ID = rep.M_Product_ID) Left JOIN M_Warehouse w ON (w.M_Warehouse_ID = rep.M_Warehouse_ID) LEFT JOIN M_Warehouse w1 "
                + " ON (w1.M_Warehouse_ID   = rep.M_WarehouseSource_ID) WHERE w.M_Warehouse_ID = " + $cmbRepAllWarehouse.val() + " AND p.IsActive      = 'Y' AND p.AD_Client_ID        = " + VIS.context.getContext("#AD_Client_ID");
            }
            else {
                sqlRep = "SELECT p.Name as Product, w.Name AS Warehouse, w.M_Warehouse_ID, NVL(rep.Level_Max,0) AS Maxi, NVL(rep.Level_Min,0) AS Mini, rep.ReplenishType AS rtype, 0 AS Qty, 0 AS OrderPack,"
                    + " rep.M_WarehouseSource_ID AS SourceWarehouse, p.M_Product_ID FROM M_Product p LEFT JOIN M_Replenish rep ON (p.M_Product_ID = rep.M_Product_ID) LEFT JOIN M_Warehouse w "
                    + " ON (w.M_Warehouse_ID = rep.M_Warehouse_ID) LEFT JOIN M_Warehouse w1 ON (w1.M_Warehouse_ID = rep.M_WarehouseSource_ID) WHERE w.M_Warehouse_ID = " + $cmbRepAllWarehouse.val() + " AND p.IsActive = 'Y' AND p.AD_Client_ID = " + VIS.context.getContext("#AD_Client_ID");
            }

            var sqlWhere = " AND p.M_Product_ID IN (";
            var recSelected = false;
            if (dProdGrid.getSelection().length > 0) {
                for (var j = 0; j < dProdGrid.records.length; j++) {
                    if (dProdGrid.getSelection().map(function (item) { return item == dProdGrid.records[j].recid; }).indexOf(true) >= 0) {
                        recSelected = true;
                        sqlWhere += dProdGrid.records[j].M_Product_ID + ",";
                    }
                }
                sqlWhere = sqlWhere.substring(0, sqlWhere.length - 1);
                sqlWhere += ")";
            }

            if (recSelected) {
                sqlRep += sqlWhere;
            }

            //if (selWh.length > 0) {
            //    var whString = "";
            //    for (var w = 0; w < selWh.length; w++) {
            //        if (whString.length > 0) {
            //            whString = whString + ", " + selWh[w];
            //        }
            //        else {
            //            whString = whString + selWh[w];
            //        }
            //    }
            //    sqlRep += " AND w.M_Warehouse_ID IN (" + whString + ")";
            //}

            VIS.DB.executeDataSet(sqlRep.toString(), null, callbackBindReplenishmentPopGrid);
        };

       

        function callbackBindReplenishmentPopGrid(ds) {

            var Recid = 0;
            repProducts = [];
            grdReplenishmentPopValues = [];

            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;

                    grdReplenishmentPopValues.push(
                {
                    recid: Recid,
                    Product: ds.tables[0].rows[i].cells.product,
                    Warehouse: ds.tables[0].rows[i].cells.m_warehouse_id,
                    Type: ds.tables[0].rows[i].cells.rtype,
                    Min: ds.tables[0].rows[i].cells.mini,
                    Max: ds.tables[0].rows[i].cells.maxi,
                    Qty: ds.tables[0].rows[i].cells.qty,
                    OrderPack: ds.tables[0].rows[i].cells.orderpack,
                    SourceWarehouse: ds.tables[0].rows[i].cells.sourcewarehouse,
                    // M_Warehouse_ID: ds.tables[0].rows[i].cells.m_warehouse_id,
                    // C_UOM_ID: "",
                    M_Product_ID: ds.tables[0].rows[i].cells.m_product_id,
                });
                }
            }

            callbackModuleCheckRepAllWhNull();

        };

        function callbackModuleCheckRepAllWhNull(moduleExists) {
            debugger;

            var Recid = grdReplenishmentPopValues.length;

            for (var i = 0; i < dProdGrid.getSelection().length; i++) {
                if (grdReplenishmentPopValues.map(function (item) { return item.M_Product_ID == dProdGrid.records[dProdGrid.getSelection()[i] - 1].M_Product_ID; }).indexOf(true) >= 0) {

                }
                else {
                    Recid = Recid + 1;

                    grdReplenishmentPopValues.push(
                {
                    recid: Recid,
                    Product: dProdGrid.records[dProdGrid.getSelection()[i] -1].Product ,
                    Warehouse: "",
                    Type: "1",
                    Min: 0,
                    Max: 0,
                    Qty: 0,
                    OrderPack: 0,
                    SourceWarehouse: "",
                    // M_Warehouse_ID: ds.tables[0].rows[i].cells.m_warehouse_id,
                    // C_UOM_ID: "",
                    M_Product_ID: dProdGrid.records[dProdGrid.getSelection()[i] - 1].M_Product_ID,
                });
                }
            }

            w2utils.encodeTags(grdReplenishmentPopValues);

            dReplenishmentPopGrid.add(grdReplenishmentPopValues);

            $bsyDiv[0].style.visibility = "hidden";

            //var sqlRep = "";

            //if (moduleExists > 0) {
            //    sqlRep = "SELECT p.Name AS Product, w.Name      AS Warehouse, w.M_Warehouse_ID, NVL(rep.Level_Max,0) AS Maxi, NVL(rep.Level_Min,0)            AS Mini,  NVL(rep.ReplenishType,0)        AS rtype, "
            //    + " NVL(DTD001_MinOrderQty,0)       AS Qty,  NVL(DTD001_OrderPackQty,0)      AS OrderPack,  rep.M_WarehouseSource_ID AS SourceWarehouse,  p.M_Product_ID FROM M_Product p "
            //    + " Left Outer Join M_Replenish rep ON (p.M_Product_ID = rep.M_Product_ID) Left JOIN M_Warehouse w ON (w.M_Warehouse_ID = rep.M_Warehouse_ID) LEFT JOIN M_Warehouse w1 "
            //    + " ON (w1.M_Warehouse_ID   = rep.M_WarehouseSource_ID) WHERE w.M_Warehouse_ID IS NULL AND p.IsActive      = 'Y' AND p.AD_Client_ID        = " + VIS.context.getContext("#AD_Client_ID");
            //}
            //else {
            //    sqlRep = "SELECT p.Name as Product, w.Name AS Warehouse, w.M_Warehouse_ID, NVL(rep.Level_Max,0) AS Maxi, NVL(rep.Level_Min,0) AS Mini, rep.ReplenishType AS rtype, 0 AS Qty, 0 AS OrderPack,"
            //        + " rep.M_WarehouseSource_ID AS SourceWarehouse, p.M_Product_ID FROM M_Product p Left JOIN M_Replenish rep ON (p.M_Product_ID = rep.M_Product_ID) Left JOIN M_Warehouse w "
            //        + " ON (w.M_Warehouse_ID = rep.M_Warehouse_ID) LEFT JOIN M_Warehouse w1 ON (w1.M_Warehouse_ID = rep.M_WarehouseSource_ID) WHERE w.M_Warehouse_ID IS NULL AND p.IsActive = 'Y' AND p.AD_Client_ID = " + VIS.context.getContext("#AD_Client_ID");
            //}

            //var sqlWhere = " AND p.M_Product_ID IN (";
            //var recSelected = false;
            //if (dProdGrid.getSelection().length > 0) {
            //    for (var j = 0; j < dProdGrid.records.length; j++) {
            //        if (dProdGrid.getSelection().map(function (item) { return item == dProdGrid.records[j].recid; }).indexOf(true) >= 0) {
            //            recSelected = true;
            //            sqlWhere += dProdGrid.records[j].M_Product_ID + ",";
            //        }
            //    }
            //    sqlWhere = sqlWhere.substring(0, sqlWhere.length - 1);
            //    sqlWhere += ")";
            //}

            //if (recSelected) {
            //    sqlRep += sqlWhere;
            //}

            //VIS.DB.executeDataSet(sqlRep.toString(), null, callbackBindReplenishmentPopGridWhNull);
        };

        function callbackBindReplenishmentPopGridWhNull(ds) {

            var Recid = grdReplenishmentPopValues.length;

            // grdReplenishmentPopValues = [];

            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;

                    grdReplenishmentPopValues.push(
                {
                    recid: Recid,
                    Product: ds.tables[0].rows[i].cells.product,
                    Warehouse: ds.tables[0].rows[i].cells.m_warehouse_id,
                    Type: ds.tables[0].rows[i].cells.rtype,
                    Min: ds.tables[0].rows[i].cells.mini,
                    Max: ds.tables[0].rows[i].cells.maxi,
                    Qty: ds.tables[0].rows[i].cells.qty,
                    OrderPack: ds.tables[0].rows[i].cells.orderpack,
                    SourceWarehouse: ds.tables[0].rows[i].cells.sourcewarehouse,
                    // M_Warehouse_ID: ds.tables[0].rows[i].cells.m_warehouse_id,
                    // C_UOM_ID: "",
                    M_Product_ID: ds.tables[0].rows[i].cells.m_product_id,
                });
                }
            }

            w2utils.encodeTags(grdReplenishmentPopValues);

            dReplenishmentPopGrid.add(grdReplenishmentPopValues);

            $bsyDiv[0].style.visibility = "hidden";
        };

        // Create ReplenishmentB Panel Grid at the Bottom
        function gridReplenishmentBPanel() {

            dReplenishmentBGrid = null;
            dReplenishmentBGrid = $divReplenishmentBGrid.w2grid({
                name: 'VA011_gridReplenishmentB_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    { field: "Warehouse", caption: VIS.Msg.getMsg("VA011_Warehouse"), sortable: false, size: '20%' },
                    {
                        field: "Type", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Type") + '</span></div>', sortable: false, size: '20%', hidden: false, editable: { type: 'select', items: repTypeArray, showAll: true },
                        render: function (record, index, col_index) {
                            var html = '';
                            for (var p in repTypeArray) {
                                if (repTypeArray[p].id == this.getCellValue(index, col_index)) html = repTypeArray[p].text;
                            }
                            return html;
                        }
                    },
                    { field: "Min", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Min") + '</span></div>', sortable: false, size: '8%', hidden: false, render: 'number:1', editable: { type: 'float' } },
                    { field: "Max", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Max") + '</span></div>', sortable: false, size: '8%', hidden: false, render: 'number:1', editable: { type: 'float' } },
                    { field: "Qty", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_MinOrderQuantity") + '</span></div>', sortable: false, size: '8%', hidden: false, render: 'number:1', editable: { type: 'float' } },
                    { field: "OrderPack", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_OrderPack") + '</span></div>', sortable: false, size: '8%', hidden: false, render: 'number:1', editable: { type: 'float' } },
                    //{ field: "SourceWarehouse", caption: VIS.Msg.getMsg("VA011_SourceWarehouse"), sortable: false, size: '20%' },
                       {
                           field: "SourceWarehouse", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_SourceWarehouse") + '</span></div>', sortable: false, size: '20%', hidden: false, editable: { type: 'select', items: listWarehouses, showAll: true },
                           render: function (record, index, col_index) {
                               var html = '';
                               for (var p in listWarehouses) {
                                   if (listWarehouses[p].id == this.getCellValue(index, col_index)) html = listWarehouses[p].text;
                               }
                               return html;
                           }
                       },
                    //{ field: "attribute_ID", caption: "", sortable: false, size: '80px', display: false },
                    //{
                    //    field: "Attribute", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_AttributeTransactions") + '</span></div>', sortable: false, size: '25%', hidden: false,
                    //    render: function () {
                    //        return '<div><input type=text readonly="readonly" style= "width:85%; border:none" ></input><img src="' + VIS.Application.contextUrl + 'Areas/VIS/Images/base/MultiX16.png" alt="Attribute Set Instance" title="Attribute Set Instance" style="opacity:1;float:right;"></div>';
                    //    }
                    //},
                    //{
                    //    field: "C_UOM_ID", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("C_UOM_ID") + '</span></div>', sortable: false, size: '12%', hidden: false, editable: { type: 'select', items: uomArray, showAll: true },
                    //    render: function (record, index, col_index) {
                    //        var html = '';
                    //        for (var p in uomArray) {
                    //            if (uomArray[p].id == this.getCellValue(index, col_index)) html = uomArray[p].text;
                    //        }
                    //        return html;
                    //    }
                    //},
                    //{ field: "UPC", caption: VIS.Msg.getMsg("VA011_UPC"), sortable: false, size: '80px' },
                    //{ field: "SerialNo", caption: VIS.Msg.getMsg("VA011_SerialNo"), sortable: false, size: '80px' },
                    //{ field: "LotNo", caption: VIS.Msg.getMsg("VA011_LotNo"), sortable: false, size: '80px' },
                    //{ field: "ExpDate", caption: VIS.Msg.getMsg("VA011_ExpiryDate"), sortable: false, size: '80px' },
                    {
                        field: "Save", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Save") + '</span></div>', sortable: false, size: '8%', hidden: false,
                        render: function () {
                            return '<div style="text-align: center;"><img src="' + VIS.Application.contextUrl + 'Areas/VIS/Images/Save22.png" alt="Attribute Set Instance" title="Attribute Set Instance" style="opacity:1;"></div>';
                        }
                    },
                    { field: "M_Warehouse_ID", caption: "M_Warehouse_ID", sortable: false, size: '80px', display: false },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                    //w2ui['gridprice_' + $self.windowNo].records[event.index]["updated"] = true;
                    if (event.column == 1) {
                        w2ui['VA011_gridReplenishmentB_' + $self.windowNo].records[event.index]["Type"] = event.value_new;
                    }
                    if (event.column == 2) {
                        w2ui['VA011_gridReplenishmentB_' + $self.windowNo].records[event.index]["Min"] = event.value_new;
                    }
                    else if (event.column == 3) {
                        w2ui['VA011_gridReplenishmentB_' + $self.windowNo].records[event.index]["Max"] = event.value_new;
                    }
                    else if (event.column == 4) {
                        w2ui['VA011_gridReplenishmentB_' + $self.windowNo].records[event.index]["Qty"] = event.value_new;
                    }
                    else if (event.column == 5) {
                        w2ui['VA011_gridReplenishmentB_' + $self.windowNo].records[event.index]["OrderPack"] = event.value_new;
                    }
                    else if (event.column == 6) {
                        w2ui['VA011_gridReplenishmentB_' + $self.windowNo].records[event.index]["SourceWarehouse"] = event.value_new;
                    }
                },
                onClick: function (event) {

                    if (event.column == 7 && dReplenishmentBGrid.records.length > 0) {
                        if (dReplenishmentBGrid.records[event.recid - 1] != null) {

                            $bsyDiv[0].style.visibility = "visible";

                            var warehoue_ID = dReplenishmentBGrid.records[event.recid - 1].M_Warehouse_ID;
                            var prod_ID = dReplenishmentBGrid.records[event.recid - 1].M_Product_ID;
                            var type = dReplenishmentBGrid.records[event.recid - 1].Type;
                            var min = dReplenishmentBGrid.records[event.recid - 1].Min;
                            var max = dReplenishmentBGrid.records[event.recid - 1].Max;
                            var qty = dReplenishmentBGrid.records[event.recid - 1].Qty;
                            var SourceWarehouse = dReplenishmentBGrid.records[event.recid - 1].SourceWarehouse;
                            if (qty == null) {
                                qty = 0;
                            }
                            var orderPack = dReplenishmentBGrid.records[event.recid - 1].OrderPack;
                            if (orderPack == null) {
                                orderPack = 0;
                            }
                            if (SourceWarehouse == null) {
                                SourceWarehouse = 0;
                            }

                            VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/SaveReplenishment", { "M_Product_ID": prod_ID, "M_Warehouse_ID": warehoue_ID, "Type": type, "Min": min, "Max": max, "Qty": qty, "OrderPack": orderPack, "SourceWarehouse": SourceWarehouse }, callbackSaveReplenishment);

                        }
                    }
                },
                onDelete: function (event) {
                    event.preventDefault();
                },
                onSubmit: function (event) {
                },
            });

            //
            //// Product Status Bar Div
            //$statusProdDiv = $('<div style="width:100%;height:30px;background-color:lightgray"></div>');
            //$ulStatusProdDiv = $('<ul class="vis-statusbar-ul" style="float:right"></ul>');

            //// For showing Result li
            //// + '<li><span class="vis-statusbar-statusDB"></span></li>'

            //$liFirst = $('<li  style="opacity: 0.6; float:left"><div><img style="opacity: 1;" action="first" title="First Page" alt="First Page" src="Areas/VIS/Images/base/PageFirst16.png"></div></li>');
            //$ulStatusProdDiv.append($liFirst);

            //$liPrev = $('<li style="opacity: 0.6;  float:left"><div><img style="opacity: 1;" action="prev" title="Page Up" alt="Page Up" src="Areas/VIS/Images/base/PageUp16.png"></div></li>');
            //$ulStatusProdDiv.append($liPrev);

            //var $li = $('<li style="float:left"></li>');
            //$li.append($cmbPageNo);
            //$ulStatusProdDiv.append($li);

            //$liNext = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="next" title="Page Down" alt="Page Down" src="Areas/VIS/Images/base/PageDown16.png"></div></li>');
            //$ulStatusProdDiv.append($liNext);

            //$liLast = ('<li style="opacity: 1;  float:left"><div><img style="opacity: 1;" action="last" title="Last Page" alt="Last Page" src="Areas/VIS/Images/base/PageLast16.png"></div></li>');
            //$ulStatusProdDiv.append($liLast);

            //$statusProdDiv.append($ulStatusProdDiv);

            //$middlePanel.append($statusProdDiv);

            //$divVariantGrid.css("display", "none");

            dReplenishmentBGrid.hideColumn('M_Warehouse_ID');

            dReplenishmentBGrid.hideColumn('M_Product_ID');
        };

        // Bind ReplenishmentB Grid 
        function bindReplenishmentBGrid() {

            if (!initLoad) {

                $bsyDiv[0].style.visibility = "visible";

                grdReplenishmentBValues = [];

                var Recid = 0;
                if (dReplenishmentBGrid != null) {
                    dReplenishmentBGrid.clear();
                }
                Recid = Recid + 1;

                var sqlDTDModuleChk = "SELECT COUNT(*) FROM AD_ModuleInfo WHERE Prefix = 'DTD001_'";

                VIS.DB.executeScalar(sqlDTDModuleChk.toString(), null, callbackModuleCheck);
            }
        };

        function callbackModuleCheck(moduleExists) {

            var sqlRep = "";

            if (moduleExists > 0) {
                sqlRep = "SELECT w.Name AS Warehouse, w.M_Warehouse_ID, rep.Level_Max AS Maxi, rep.Level_Min AS Mini, rep.ReplenishType AS rtype, DTD001_MinOrderQty AS Qty, DTD001_OrderPackQty AS OrderPack,"
                   + " rep.M_WarehouseSource_ID AS SourceWarehouse, p.M_Product_ID FROM M_Replenish rep INNER JOIN M_Product p ON (p.M_Product_ID = rep.M_Product_ID) INNER JOIN M_Warehouse w "
                   + " ON (w.M_Warehouse_ID = rep.M_Warehouse_ID) LEFT JOIN M_Warehouse w1 ON (w1.M_Warehouse_ID = rep.M_WarehouseSource_ID) WHERE rep.IsActive = 'Y' AND p.M_Product_ID    = " + _product_ID;
            }
            else {
                sqlRep = "SELECT w.Name AS Warehouse, w.M_Warehouse_ID, rep.Level_Max AS Maxi, rep.Level_Min AS Mini, rep.ReplenishType AS rtype, 0 AS Qty, 0 AS OrderPack,"
                    + " rep.M_WarehouseSource_ID AS SourceWarehouse, p.M_Product_ID FROM M_Replenish rep INNER JOIN M_Product p ON (p.M_Product_ID = rep.M_Product_ID) INNER JOIN M_Warehouse w "
                    + " ON (w.M_Warehouse_ID = rep.M_Warehouse_ID) LEFT JOIN M_Warehouse w1 ON (w1.M_Warehouse_ID = rep.M_WarehouseSource_ID) WHERE rep.IsActive = 'Y' AND p.M_Product_ID    = " + _product_ID;
            }

            if (selWh.length > 0) {
                var whString = "";
                for (var w = 0; w < selWh.length; w++) {
                    if (whString.length > 0) {
                        whString = whString + ", " + selWh[w];
                    }
                    else {
                        whString = whString + selWh[w];
                    }
                }
                sqlRep += " AND w.M_Warehouse_ID IN (" + whString + ")";
            }

            VIS.DB.executeDataSet(sqlRep.toString(), null, callbackBindReplenishmentBGrid);
        };

        function callbackBindReplenishmentBGrid(ds) {

            var Recid = 0;

            if (ds != null && ds.tables[0].rows.length > 0) {
                for (var i = 0; i < ds.tables[0].rows.length; i++) {
                    Recid = Recid + 1;

                    grdReplenishmentBValues.push(
                {
                    recid: Recid,
                    Warehouse: ds.tables[0].rows[i].cells.warehouse,
                    Type: ds.tables[0].rows[i].cells.rtype,
                    Min: ds.tables[0].rows[i].cells.mini,
                    Max: ds.tables[0].rows[i].cells.maxi,
                    Qty: ds.tables[0].rows[i].cells.qty,
                    OrderPack: ds.tables[0].rows[i].cells.orderpack,
                    SourceWarehouse: ds.tables[0].rows[i].cells.sourcewarehouse,
                    M_Warehouse_ID: ds.tables[0].rows[i].cells.m_warehouse_id,
                    // C_UOM_ID: "",
                    M_Product_ID: ds.tables[0].rows[i].cells.m_product_id,
                });
                }
            }

            w2utils.encodeTags(grdReplenishmentBValues);

            dReplenishmentBGrid.add(grdReplenishmentBValues);

            grdReplenishmentBValues = [];

            $bsyDiv[0].style.visibility = "hidden";
        };

        function callbackSaveReplenishment(data) {
            bindReplenishmentBGrid();
        };

        // Create Product Panel Grid at the Top
        function gridReplenishTopPanel() {

            dRepTopGrid = null;
            dRepTopGrid = $divRepTopGrid.w2grid({
                name: 'VA011_gridRepTop_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: true,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false },
                    { field: "AD_Client_ID", caption: "AD_Client_ID", sortable: false, size: '80px', display: false },
                    { field: "AD_Org_ID", caption: "AD_Org_ID", sortable: false, size: '80px', display: false },
                    { field: "RepCreate", caption: "RepCreate", sortable: false, size: '80px', display: false },
                    { field: "C_DocType_ID", caption: "C_DocType_ID", sortable: false, size: '80px', display: false },
                    { field: "M_PriceList_ID", caption: "M_PriceList_ID", sortable: false, size: '80px', display: false },
                    { field: "M_WarehouseSource_ID", caption: "M_WarehouseSource_ID", sortable: false, size: '80px', display: false },
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false },
                    { field: "C_BPartner_ID", caption: "C_BPartner_ID", sortable: false, size: '80px', display: false },
                    { field: "M_Warehouse_ID", caption: "M_Warehouse_ID", sortable: false, size: '80px', display: false },
                    { field: "DocStatus", caption: "DocStatus", sortable: false, size: '35%', hidden: false, display: false },

                    { field: "Product", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("Product") + '</span></div>', sortable: false, size: '12%', hidden: false },
                    { field: "Warehouse", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Warehouse") + '</span></div>', sortable: false, size: '12%', hidden: false },
                    { field: "SourceWarehouse", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_SourceWarehouse") + '</span></div>', sortable: false, size: '12%', hidden: false },
                    { field: "ReplenishmentType", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_ReplenishmentType") + '</span></div>', sortable: false, size: '9%', hidden: false },
                    { field: "Max", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Max") + '</span></div>', sortable: false, size: '9%', hidden: false, render: 'number:1' },
                    { field: "Min", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Min") + '</span></div>', sortable: false, size: '9%', hidden: false, render: 'number:1' },
                    { field: "QtyOnHand", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_OnHand") + '</span></div>', sortable: false, size: '9%', hidden: false, render: 'number:1' },
                    { field: "Ordered", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Ordered") + '</span></div>', sortable: false, size: '9%', hidden: false, render: 'number:1' },
                    { field: "ReqReserved", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_ReqReserved") + '</span></div>', sortable: false, size: '9%', hidden: false, render: 'number:1' },
                    { field: "Reserved", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Reserved") + '</span></div>', sortable: false, size: '9%', hidden: false, render: 'number:1' },
                    { field: "QtyToOrder", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_QtyToOrder") + '</span></div>', sortable: false, size: '9%', hidden: false, editable: { type: 'float' }, render: 'number:1' },
                ],
                records: [

                ],
                onChange: function (event) {

                    //w2ui['VA011_gridProd_' + $self.windowNo].records[event.index]["Updated"] = true;
                    //if (event.column == 2) {
                    //    w2ui['VA011_gridProd_' + $self.windowNo].records[event.index]["Qty"] = event.value_new;
                    //}
                    //else if (event.column == 3) {
                    //    w2ui['VA011_gridProd_' + $self.windowNo].records[event.index]["C_Uom_ID"] = event.value_new;
                    //}
                },

                onSelect: function (event) {

                    if (dRepTopGrid.records[event.recid - 1] != null) {

                    }
                    else {

                    }
                },

                onUnselect: function (event) {

                    event.onComplete = function () {
                        if (dRepTopGrid.getSelection().length == 1) {
                            //_product_ID = VIS.Utility.Util.getValueOfInt(dProdGrid.records[dProdGrid.getSelection() - 1]["M_Product_ID"]);
                            //$divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                            //$divLocatorGrid.css("display", "none");
                            //$divOrderedGrid.css("display", "none");
                            //$divTransactionsGrid.css("display", "none");
                            //$divDemandGrid.css("display", "none");
                            //$divReplenishedGrid.css("display", "none");
                            //$divVariantGrid.css("display", "block");
                            //$("#VA011_btnVariant_" + $self.windowNo).addClass("VA011-selectedTab");
                            //bindVariantGrid();
                        }
                        else {
                            //_product_ID = 0;
                            //$divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                            //$divLocatorGrid.css("display", "none");
                            //$divOrderedGrid.css("display", "none");
                            //$divTransactionsGrid.css("display", "none");
                            //$divDemandGrid.css("display", "none");
                            //$divReplenishedGrid.css("display", "none");
                            //$divVariantGrid.css("display", "block");
                            //$("#VA011_btnVariant_" + $self.windowNo).addClass("VA011-selectedTab");
                            //bindVariantGrid();
                        }
                    }
                },

                //onClick: function (event) {

                //},

                onDelete: function (event) {
                    event.preventDefault();
                },

                onSubmit: function (event) {
                    // alert("Submitted");
                },
            });

            dRepTopGrid.hideColumn('M_Product_ID');
            dRepTopGrid.hideColumn('AD_Client_ID');
            dRepTopGrid.hideColumn('AD_Org_ID');
            dRepTopGrid.hideColumn('RepCreate');
            dRepTopGrid.hideColumn('C_DocType_ID');
            dRepTopGrid.hideColumn('M_PriceList_ID');
            dRepTopGrid.hideColumn('M_WarehouseSource_ID');
            dRepTopGrid.hideColumn('C_BPartner_ID');
            dRepTopGrid.hideColumn('M_Warehouse_ID');
            dRepTopGrid.hideColumn('DocStatus');
            dRepTopGrid.hideColumn('ReplenishmentType');

        }

        // Bind Product Grid 
        function bindReplenishTopGrid() {

            if (!initLoad) {
                _product_ID = 0;

                $bsyDiv[0].style.visibility = "visible";

                // $cmbPageNo.empty();

                //var whString = "";
                //for (var w = 0; w < selWh.length; w++) {
                //    if (whString.length > 0) {
                //        whString = whString + ", " + selWh[w];
                //    }
                //    else {
                //        whString = whString + selWh[w];
                //    }
                //}

                //var orgString = "";
                //for (var w = 0; w < selOrgs.length; w++) {
                //    if (orgString.length > 0) {
                //        orgString = orgString + ", " + selOrgs[w];
                //    }
                //    else {
                //        orgString += "0, ";
                //        orgString += selOrgs[w];
                //    }
                //}

                //var plvString = "";
                //for (var w = 0; w < selPLV.length; w++) {
                //    if (plvString.length > 0) {
                //        plvString = plvString + ", " + selPLV[w];
                //    }
                //    else {
                //        plvString = plvString + selPLV[w];
                //    }
                //}

                //var suppString = "";
                //for (var w = 0; w < selSupp.length; w++) {
                //    if (suppString.length > 0) {
                //        suppString = suppString + ", " + selSupp[w];
                //    }
                //    else {
                //        suppString = suppString + selSupp[w];
                //    }
                //}

                //var prodCatString = "";
                //for (var w = 0; w < selCat.length; w++) {
                //    if (prodCatString.length > 0) {
                //        prodCatString = prodCatString + ", " + selCat[w];
                //    }
                //    else {
                //        prodCatString = prodCatString + selCat[w];
                //    }
                //}

                //if (!execPaging) {
                //    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/GetProductCount", { "searchText": searchProdText, "warehouse_IDs": whString, "org_IDs": orgString, "plv_IDs": plvString, "supp_IDs": suppString, "prodCat_IDs": prodCatString }, callbackProdCount);
                //}
                //else {
                //    $.ajax({
                //        url: VIS.Application.contextUrl + "Inventory/GetProducts",
                //        dataType: "json",
                //        // async: false,
                //        data: {
                //            searchText: searchProdText,
                //            warehouse_IDs: whString,
                //            org_IDs: orgString,
                //            plv_IDs: plvString,
                //            supp_IDs: suppString,
                //            prodCat_IDs: prodCatString,
                //            pageNo: pgNo,
                //            pageSize: pgSize
                //        },
                //        success: function (data) {
                //            callbackBindProdGrid(data);
                //        }
                //    });
                //}
            }
        };


        // Create Product Panel Grid at the Top
        function gridProductPanel() {

            dProdGrid = null;
            dProdGrid = $divProdGrid.w2grid({
                name: 'VA011_gridProd_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: true,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: true,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: true,   // indicates if toolbar delete button is visible
                    toolbarSave: true,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },

                columns: [
                    { field: "M_Product_ID", caption: "M_Product_ID", sortable: false, size: '80px', display: false },
                    { field: "Product", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("Product") + '</span></div>', sortable: false, size: '35%', hidden: false },
                    { field: "QtyOnHand", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_OnHand") + '</span></div>', sortable: false, size: '13%', hidden: false, render: 'number:1' },
                    { field: "UOM", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_UOM") + '</span></div>', sortable: false, size: '13%', hidden: false },
                    //{
                    //    field: "C_UOM_ID", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("C_UOM_ID") + '</span></div>', sortable: false, size: '15%', hidden: false, editable: { type: 'select', items: uomArray, showAll: true },
                    //    render: function (record, index, col_index) {
                    //        var html = '';
                    //        for (var p in uomArray) {
                    //            if (uomArray[p].id == this.getCellValue(index, col_index)) html = uomArray[p].text;
                    //        }
                    //        return html;
                    //    }
                    //},
                    { field: "Reserved", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Reserved") + '</span></div>', sortable: false, size: '15%', hidden: false, render: 'number:1' },
                    { field: "QtyAvailable", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_ATP") + '</span></div>', sortable: false, size: '15%', hidden: false, render: 'number:1' },
                    { field: "UnConfirmed", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Unconfirmed") + '</span></div>', sortable: false, size: '15%', hidden: false, render: 'number:1' },
                    { field: "Ordered", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Ordered") + '</span></div>', sortable: false, size: '15%', hidden: false, render: 'number:1' },
                    { field: "Demanded", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Demanded") + '</span></div>', sortable: false, size: '15%', hidden: false, render: 'number:1' },
                    { field: "TillReorder", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_TillReorder") + '</span></div>', sortable: false, size: '15%', hidden: false, render: 'number:1' },
                    { field: "QtyToReplenish", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_QtyToReplenish") + '</span></div>', sortable: false, size: '15%', hidden: false, render: 'number:1' },
                    {
                        field: "Status", caption: '<div style="text-align: center;" ><span>' + VIS.Msg.getMsg("VA011_Status") + '</span></div>', sortable: false, size: '15%', hidden: false,
                        render: function () {
                            //return '<div><input type=text readonly="readonly" style= "width:85%; border:none;display:none;" ></input><img id="avchvvvv" src="' + VIS.Application.contextUrl + 'Areas/VIS/Images/base/MultiX16.png" alt="Attribute Set Instance" title="Attribute Set Instance" style="opacity:1;float:right;"></div>';
                            return '<div style="text-align: center;"><img src="" alt="" title="Attribute Set Instance" style="opacity:1;"></div>';
                        }
                    },
                    { field: "C_UOM_ID", caption: "C_UOM_ID", sortable: false, size: '80px', display: false },
                    { field: "MinLevel", caption: "MinLevel", sortable: false, size: '13%', hidden: false, render: 'number:1' },
                   // { field: "UPC", caption: VIS.Msg.getElement(VIS.Env.getCtx(), "UPC"), sortable: false, size: '80px' },
                   // { field: "LineID", caption: VIS.Msg.getElement(VIS.Env.getCtx(), "VAICNT_InventoryCountLine_ID"), sortable: false, size: '80px', display: false },
                   //{ field: "Updated", caption: VIS.Msg.getElement(VIS.Env.getCtx(), "Updated"), sortable: false, size: '80px', display: false }
                ],
                records: [

                ],
                onChange: function (event) {

                    //w2ui['VA011_gridProd_' + $self.windowNo].records[event.index]["Updated"] = true;
                    //if (event.column == 2) {
                    //    w2ui['VA011_gridProd_' + $self.windowNo].records[event.index]["Qty"] = event.value_new;
                    //}
                    //else if (event.column == 3) {
                    //    w2ui['VA011_gridProd_' + $self.windowNo].records[event.index]["C_Uom_ID"] = event.value_new;
                    //}
                },

                onSelect: function (event) {


                    event.onComplete = function () {
                        if (dProdGrid.getSelection().length > 1) {
                            multipleRecSelected = true;
                            disableSections();
                            return;
                        }
                        else {
                            multipleRecSelected = false;
                            disableSections();
                        }

                        if (dProdGrid.records[event.recid - 1] != null) {

                            _product_ID = VIS.Utility.Util.getValueOfInt(dProdGrid.records[event.recid - 1]["M_Product_ID"]);
                            listSelProducts.push(_product_ID);
                            //$divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                            //$divLocatorGrid.css("display", "none");
                            //$divOrderedGrid.css("display", "none");
                            //$divTransactionsGrid.css("display", "none");
                            //$divDemandGrid.css("display", "none");
                            //$divReplenishedGrid.css("display", "none");
                            //$divReplenishmentBGrid.css("display", "none");
                            //$divVariantGrid.css("display", "block");
                            //$("#VA011_btnVariant_" + $self.windowNo).addClass("VA011-selectedTab");
                            setProductDetails();
                        }
                        else {
                            _product_ID = 0;
                            listSelProducts = listSelProducts.splice();
                            //$divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                            //$divLocatorGrid.css("display", "none");
                            //$divOrderedGrid.css("display", "none");
                            //$divTransactionsGrid.css("display", "none");
                            //$divDemandGrid.css("display", "none");
                            //$divReplenishedGrid.css("display", "none");
                            //$divReplenishmentBGrid.css("display", "none");
                            //$divVariantGrid.css("display", "block");
                            //$("#VA011_btnVariant_" + $self.windowNo).addClass("VA011-selectedTab");
                            setProductDetails();
                        }
                    };
                },

                onUnselect: function (event) {

                    event.onComplete = function () {

                        if (dProdGrid.getSelection().length == 1) {
                            multipleRecSelected = false;
                            disableSections();

                            _product_ID = VIS.Utility.Util.getValueOfInt(dProdGrid.records[dProdGrid.getSelection() - 1]["M_Product_ID"]);
                            //listSelProducts.push(_product_ID);
                            //$divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                            //$divLocatorGrid.css("display", "none");
                            //$divOrderedGrid.css("display", "none");
                            //$divTransactionsGrid.css("display", "none");
                            //$divDemandGrid.css("display", "none");
                            //$divReplenishedGrid.css("display", "none");
                            //$divVariantGrid.css("display", "block");
                            //$("#VA011_btnVariant_" + $self.windowNo).addClass("VA011-selectedTab");
                            setProductDetails();
                            //bindVariantGrid();
                        }
                        else {
                            _product_ID = 0;
                            listSelProducts = listSelProducts.splice();

                            divZoomProdName.css("display", "none");
                            $("#VA011_prodName_" + $self.windowNo).text("");
                            $("#VA011_inputWeight_" + $self.windowNo).val("");
                            $("#VA011_inputVolume_" + $self.windowNo).val("");
                            $("#VA011_inputTare_" + $self.windowNo).val("");
                            $("#VA011_inputLocator_" + $self.windowNo).val("");
                            $("#VA011_inputExpDays_" + $self.windowNo).val("");
                            $("#VA011_inputUOM_" + $self.windowNo).val("");
                            $("#VA011_UPC" + $self.windowNo).find("span").remove();
                            $("#VA011_UPC" + $self.windowNo).append('<span></span>');
                            $("#VA011_AttributeSet" + $self.windowNo).find("span").remove();
                            $("#VA011_AttributeSet" + $self.windowNo).append('<span></span>');
                            var imageUrl = "";
                            if (imageUrl != null) {
                                if (imageUrl != "") {
                                    imageUrl = imageUrl.substring(imageUrl.lastIndexOf("/") + 1, imageUrl.length);
                                    var d = new Date();
                                    imgUsrImage.removeAttr("src");
                                    imgUsrImage.attr("src", VIS.Application.contextUrl + "Images/Thumb140x120/" + imageUrl + "?" + d.getTime());
                                }
                                else {
                                    imgUsrImage.removeAttr("src").attr("src", VIS.Application.contextUrl + "Areas/VA011/Images/img-defult.png");
                                }
                            }
                            else {
                                imgUsrImage.removeAttr("src").attr("src", VIS.Application.contextUrl + "Areas/VA011/Images/img-defult.png");
                            }


                            //$divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                            //$divLocatorGrid.css("display", "none");
                            //$divOrderedGrid.css("display", "none");
                            //$divTransactionsGrid.css("display", "none");
                            //$divDemandGrid.css("display", "none");
                            //$divReplenishedGrid.css("display", "none");
                            //$divVariantGrid.css("display", "block");
                            //$("#VA011_btnVariant_" + $self.windowNo).addClass("VA011-selectedTab");
                            //setProductDetails();
                            //bindVariantGrid();
                        }
                    }
                },

                //onClick: function (event) {
                //    
                //    //alert("clicked");
                //    if (event.column == 5 && w2ui['VA011_gridProd_' + $self.windowNo].records.length > 0) {

                //        //var qry = "SELECT M_AttributeSet_ID FROM M_Product WHERE M_Product_ID = " + VIS.Utility.Util.getValueOfInt(w2ui['VA011_gridProd_' + $self.windowNo].records[event.recid - 1]["M_Product_ID"]);
                //        //var mattsetid = VIS.Utility.Util.getValueOfInt(VIS.DB.executeScalar(qry));
                //        //if (mattsetid != 0) {
                //        //    var productWindow = AD_Column_ID == 8418;		//	HARDCODED
                //        //    var M_Locator_ID = VIS.context.getContextAsInt($self.windowNo, "M_Locator_ID");
                //        //    var C_BPartner_ID = VIS.context.getContextAsInt($self.windowNo, "C_BPartner_ID");
                //        //    var obj = new VIS.PAttributesForm(VIS.Utility.Util.getValueOfInt(w2ui['VA011_gridProd_' + $self.windowNo].records[event.recid - 1]["Attribute_ID"]), VIS.Utility.Util.getValueOfInt(w2ui['VA011_gridProd_' + $self.windowNo].records[event.recid - 1]["M_Product_ID"]), M_Locator_ID, C_BPartner_ID, productWindow, AD_Column_ID, $self.windowNo);
                //        //    if (obj.hasAttribute) {
                //        //        obj.showDialog();
                //        //    }
                //        //    obj.onClose = function (mAttributeSetInstanceId, name, mLocatorId) {
                //        //        w2ui['VA011_gridProd_' + $self.windowNo].records[event.recid - 1]["attribute_ID"] = mAttributeSetInstanceId;
                //        //        w2ui['VA011_gridProd_' + $self.windowNo].records[event.recid - 1]["Attribute"] = name;
                //        //        $("#grid_gridcart_" + $self.windowNo + "_rec_" + event.recid).find("input[type=text]").val(name);
                //        //    };
                //        //}
                //        //else {
                //        //    return;
                //        //}
                //    }
                //},

                onDelete: function (event) {
                    event.preventDefault();
                },

                onSubmit: function (event) {
                    // alert("Submitted");
                },
            });

            dProdGrid.hideColumn('M_Product_ID');
            dProdGrid.hideColumn('C_UOM_ID');
            dProdGrid.hideColumn('MinLevel');
            dProdGrid.hideColumn('TillReorder');

        }

        function setProductDetails() {
            $bsyDiv[0].style.visibility = "visible";
            VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/GetProductDetails", { "M_Product_ID": _product_ID }, callbackProdDetails);
        };

        function callbackProdDetails(data) {


            if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_RightSection") >= 0) {
                $("#VA011_RightSection" + $self.windowNo).css("display", "block");
                $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "none");
                $("#VA011_grdRelated_" + $self.windowNo).css("display", "none");
                $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "none");
                $("#VA011_grdKits_" + $self.windowNo).css("display", "none");
                divCart.hide();
                $divCartdata.hide();

                $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                $("#VA011_btnDetails_" + $self.windowNo).addClass("VA011-selectedTab");
            }
            else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnDetails") >= 0) {
                $("#VA011_RightSection" + $self.windowNo).css("display", "block");
                $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "none");
                $("#VA011_grdRelated_" + $self.windowNo).css("display", "none");
                $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "none");
                $("#VA011_grdKits_" + $self.windowNo).css("display", "none");
                divCart.hide();
                $divCartdata.hide();

                $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                $("#VA011_btnDetails_" + $self.windowNo).addClass("VA011-selectedTab");
            }
            else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnSubsti") >= 0) {
                //$divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "block");
                bindSubstituteGrid();
            }
            else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnRelated") >= 0) {
                //$divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                $("#VA011_grdRelated_" + $self.windowNo).css("display", "block");
                bindRelatedGrid();
            }
            else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnSuppliers") >= 0) {
                // $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "block");
                bindSuppliersRightGrid();
            }
            else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnKits") >= 0) {
                //$divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                $("#VA011_grdKits_" + $self.windowNo).css("display", "block");
                bindKitsGrid();
            }
            else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnCart") >= 0) {
                //$divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                $("#VA011_divCart_" + $self.windowNo).css("display", "block");
                //BindCartGrid();
            }

            if (data.Table[0] != null) {
                divZoomProdName.css("display", "block");
                $("#VA011_prodName_" + $self.windowNo).text(data.Table[0].NAME);
                $("#VA011_inputWeight_" + $self.windowNo).val(data.Table[0].WEIGHT);
                $("#VA011_inputVolume_" + $self.windowNo).val(data.Table[0].VOLUME);
                $("#VA011_inputTare_" + $self.windowNo).val(data.Table[0].TARE);
                $("#VA011_inputLocator_" + $self.windowNo).val(data.Table[0].LOCATOR);
                $("#VA011_inputExpDays_" + $self.windowNo).val(data.Table[0].GUARANTEEDAYS);
                $("#VA011_inputUOM_" + $self.windowNo).val(data.Table[0].UOM);
                $("#VA011_UPC" + $self.windowNo).find("span").remove();
                $("#VA011_UPC" + $self.windowNo).append('<span>' + data.Table[0].UPC + '</span>');
                $("#VA011_AttributeSet" + $self.windowNo).find("span").remove();
                $("#VA011_AttributeSet" + $self.windowNo).append('<span>' + data.Table[0].ATTRIBUTE + '</span>');
                var imageUrl = data.Table[0].IMAGEURL;
                if (imageUrl != null) {
                    if (imageUrl != "") {
                        imageUrl = imageUrl.substring(imageUrl.lastIndexOf("/") + 1, imageUrl.length);
                        var d = new Date();
                        imgUsrImage.removeAttr("src");
                        imgUsrImage.attr("src", VIS.Application.contextUrl + "Images/Thumb140x120/" + imageUrl + "?" + d.getTime());
                    }
                    else {
                        imgUsrImage.removeAttr("src").attr("src", VIS.Application.contextUrl + "Areas/VA011/Images/img-defult.png");
                    }
                }
                else {
                    imgUsrImage.removeAttr("src").attr("src", VIS.Application.contextUrl + "Areas/VA011/Images/img-defult.png");
                }
                //GenerateBarcode(data.Table[0].UPC.trim(), $(".VA011_image-wrap").find("span"));

                if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnVariant") >= 0) {
                    bindVariantGrid();
                }
                else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnReplenished") >= 0) {
                    bindReplenishedGrid();
                }
                else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnLocators") >= 0) {
                    bindLocatorGrid();
                }
                else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnOrdered") >= 0) {
                    bindOrderedGrid();
                }
                else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnDemand") >= 0) {
                    bindDemandGrid();
                }
                else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnTransactions") >= 0) {
                    bindTransactionsGrid();
                }
                else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnReplenishmentB") >= 0) {
                    bindReplenishmentBGrid();
                }
            }
            else {
                divZoomProdName.css("display", "none");
                $("#VA011_prodName_" + $self.windowNo).text("");
                $("#VA011_inputWeight_" + $self.windowNo).val("");
                $("#VA011_inputVolume_" + $self.windowNo).val("");
                $("#VA011_inputTare_" + $self.windowNo).val("");
                $("#VA011_inputLocator_" + $self.windowNo).val("");
                $("#VA011_inputExpDays_" + $self.windowNo).val("");
                $("#VA011_inputUOM_" + $self.windowNo).val("");
                $("#VA011_UPC" + $self.windowNo).find("span").remove();
                $("#VA011_UPC" + $self.windowNo).append('<span></span>');
                $("#VA011_AttributeSet" + $self.windowNo).find("span").remove();
                $("#VA011_AttributeSet" + $self.windowNo).append('<span></span>');
                var imageUrl = "";
                if (imageUrl != null) {
                    if (imageUrl != "") {
                        imageUrl = imageUrl.substring(imageUrl.lastIndexOf("/") + 1, imageUrl.length);
                        var d = new Date();
                        imgUsrImage.removeAttr("src");
                        imgUsrImage.attr("src", VIS.Application.contextUrl + "Images/Thumb140x120/" + imageUrl + "?" + d.getTime());
                    }
                    else {
                        imgUsrImage.removeAttr("src").attr("src", VIS.Application.contextUrl + "Areas/VA011/Images/img-defult.png");
                    }
                }
                else {
                    imgUsrImage.removeAttr("src").attr("src", VIS.Application.contextUrl + "Areas/VA011/Images/img-defult.png");
                }

                bindVariantGrid();
            }
        };

        // Bind Product Grid 
        function bindProductGrid(btnReplenish) {
            calculateReplenishSelected = false;
            if (!btnReplenish) {
                $divHeadVarientProdTop.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                $divHeadVarientProdTop.find("#VA011_StockDet_" + $self.windowNo).addClass("VA011-selectedTab");
            }

            if (!initLoad) {

                _product_ID = 0;
                $statusProdDiv.css("display", "block");

                $bsyDiv[0].style.visibility = "visible";

                // $cmbPageNo.empty();

                var whString = "";
                for (var w = 0; w < selWh.length; w++) {
                    if (whString.length > 0) {
                        whString = whString + ", " + selWh[w];
                    }
                    else {
                        whString = whString + selWh[w];
                    }
                }

                var orgString = "";
                for (var w = 0; w < selOrgs.length; w++) {
                    if (orgString.length > 0) {
                        orgString = orgString + ", " + selOrgs[w];
                    }
                    else {
                        orgString += "0, ";
                        orgString += selOrgs[w];
                    }
                }

                var plvString = "";
                for (var w = 0; w < selPLV.length; w++) {
                    if (plvString.length > 0) {
                        plvString = plvString + ", " + selPLV[w];
                    }
                    else {
                        plvString = plvString + selPLV[w];
                    }
                }

                var suppString = "";
                for (var w = 0; w < selSupp.length; w++) {
                    if (suppString.length > 0) {
                        suppString = suppString + ", " + selSupp[w];
                    }
                    else {
                        suppString = suppString + selSupp[w];
                    }
                }

                var prodCatString = "";
                for (var w = 0; w < selCat.length; w++) {
                    if (prodCatString.length > 0) {
                        prodCatString = prodCatString + ", " + selCat[w];
                    }
                    else {
                        prodCatString = prodCatString + selCat[w];
                    }
                }

                if (!execPaging) {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/GetProductCount", { "searchText": searchProdText, "warehouse_IDs": whString, "org_IDs": orgString, "plv_IDs": plvString, "supp_IDs": suppString, "prodCat_IDs": prodCatString }, callbackProdCount);
                }
                else {
                    $.ajax({
                        url: VIS.Application.contextUrl + "Inventory/GetProducts",
                        dataType: "json",
                        // async: false,
                        data: {
                            searchText: searchProdText,
                            warehouse_IDs: whString,
                            org_IDs: orgString,
                            plv_IDs: plvString,
                            supp_IDs: suppString,
                            prodCat_IDs: prodCatString,
                            pageNo: pgNo,
                            pageSize: pgSize
                        },
                        success: function (data) {
                            callbackBindProdGrid(data);
                        }
                    });
                }
            }
        };

        function callbackProdCount(count) {

            //var countProd = VIS.DB.executeScalar(sqlProdCount.toString(), null, null);
            totalRecords = count;

            var countPg = calculateNoofPages();

            $cmbPageNo.empty();

            var whString = "";
            for (var w = 0; w < selWh.length; w++) {
                if (whString.length > 0) {
                    whString = whString + ", " + selWh[w];
                }
                else {
                    whString = whString + selWh[w];
                }
            }

            var orgString = "";
            for (var w = 0; w < selOrgs.length; w++) {
                if (orgString.length > 0) {
                    orgString = orgString + ", " + selOrgs[w];
                }
                else {
                    orgString += "0, ";
                    orgString += selOrgs[w];
                }
            }

            var plvString = "";
            for (var w = 0; w < selPLV.length; w++) {
                if (plvString.length > 0) {
                    plvString = plvString + ", " + selPLV[w];
                }
                else {
                    plvString = plvString + selPLV[w];
                }
            }

            var suppString = "";
            for (var w = 0; w < selSupp.length; w++) {
                if (suppString.length > 0) {
                    suppString = suppString + ", " + selSupp[w];
                }
                else {
                    suppString = suppString + selSupp[w];
                }
            }

            var prodCatString = "";
            for (var w = 0; w < selCat.length; w++) {
                if (prodCatString.length > 0) {
                    prodCatString = prodCatString + ", " + selCat[w];
                }
                else {
                    prodCatString = prodCatString + selCat[w];
                }
            }

            for (var i = 1; i < countPg + 1; i++) {
                $cmbPageNo.append('<option value=' + i + '>' + i + '</option>');
            }

            $.ajax({
                url: VIS.Application.contextUrl + "Inventory/GetProducts",
                dataType: "json",
                // async: false,
                data: {
                    searchText: searchProdText,
                    warehouse_IDs: whString,
                    org_IDs: orgString,
                    plv_IDs: plvString,
                    supp_IDs: suppString,
                    prodCat_IDs: prodCatString,
                    pageNo: pgNo,
                    pageSize: pgSize
                },
                success: function (data) {
                    callbackBindProdGrid(data);
                }
            });
        };

        // Callback to Bind Product Grid Records Based on the filters applied
        function callbackBindProdGrid(res) {

            multipleRecSelected = false;
            disableSections();

            execPaging = false;
            grdProdValues = [];
            dProdGrid.clear();
            var Recid = 0;

            var data = jQuery.parseJSON(res);

            // Bind Grid Product Here
            for (var i = 0; i < data.length; i++) {

                Recid = Recid + 1;
                grdProdValues.push(
                {
                    recid: Recid,
                    M_Product_ID: data[i].M_Product_ID,
                    Product: data[i].Name,
                    QtyOnHand: data[i].QtyOnHand,
                    UOM: data[i].UOM,
                    C_UOM_ID: data[i].C_UOM_ID,
                    Reserved: data[i].Reserved,
                    QtyAvailable: data[i].QtyAvailable,
                    UnConfirmed: data[i].UnConfirmed,
                    Ordered: data[i].Ordered,
                    Demanded: data[i].Demanded,
                    TillReorder: data[i].TillReorder,
                    QtyToReplenish: data[i].QtyToReplenish,
                    MinLevel: data[i].MinLevel,
                    Status: ""
                });
            }

            w2utils.encodeTags(grdProdValues);
            dProdGrid.add(grdProdValues);
            dProdGrid.selectNone();
            _product_ID = 0;
            grdProdValues = [];

            resetBottomGrid();

            if ($cmbPageNo[0] != null)
                $cmbPageNo[0].selectedIndex = pgNo - 1;

            $divProdGrid.css("display", "block");
            $divRepTopGrid.css("display", "none");

            $("#VA011_btnGenerateReplenish_" + $self.windowNo).css("display", "none");
            $("#VA011_btnReplenish_" + $self.windowNo).css("display", "block");

            if (calculateReplenishLI) {
                calculateReplenishLI.css("display", "block");
            };

            if (addToCartLI) {
                addToCartLI.css("display", "block");
            };

            if (displayRepRuleAllLI) {
                displayRepRuleAllLI.css("display", "block");
            };

            if (generateReplenishLI) {
                generateReplenishLI.css("display", "none");
            };

            $statusProdDiv.css("display", "block");

            $divProductCategories.val("");
            $divOrg.val("");
            $divSupplier.val("");
            $divPLV.val("");
            $divWarehouse.val("");
            $($divSearch.find("input")[0]).val("");
            $($divSearch.find("input")[0]).focus();

            setProductDetails();

            //$bsyDiv[0].style.visibility = "hidden";
        }

        // Callback to Bind Product Grid Records After calculating the Replenishment Qty
        function callbackBindProdReplenish(res) {
            // Bind Grid Product Here

            calculateReplenishSelected = true;
            for (var i = 0; i < res.length; i++) {
                dProdGrid.records[i]["QtyToReplenish"] = res[i];
            }
            dProdGrid.refresh();
            gridResizing();
            $statusProdDiv.css("display", "block");
            if (!initLoad) {
                $bsyDiv[0].style.visibility = "hidden";
            }
        }

        // function to keep Images in the grid as it is on grid Refresh or Resize
        function gridResizing() {
            //if ($("#VA011_btnReplenish_" + $self.windowNo).hasClass("VA011-selectedTab")) {
            if (calculateReplenishSelected) {
                if (dProdGrid.records != null) {
                    for (var i = 0; i < dProdGrid.records.length; i++) {

                        var src = "";
                        if ((dProdGrid.records[i].QtyOnHand > dProdGrid.records[i].MinLevel) && ((dProdGrid.records[i].QtyOnHand - dProdGrid.records[i].Demanded
                            + dProdGrid.records[i].Ordered + dProdGrid.records[i].UnConfirmed) > dProdGrid.records[i].MinLevel)) {
                            src = VIS.Application.contextUrl + "Areas/VA011/Images/green.png";
                        }
                        else if (((dProdGrid.records[i].QtyOnHand < dProdGrid.records[i].MinLevel) && ((dProdGrid.records[i].QtyOnHand - dProdGrid.records[i].Demanded
                            + dProdGrid.records[i].Ordered + dProdGrid.records[i].UnConfirmed) > dProdGrid.records[i].MinLevel)) ||
                            ((dProdGrid.records[i].QtyOnHand > dProdGrid.records[i].MinLevel) && ((dProdGrid.records[i].QtyOnHand - dProdGrid.records[i].Demanded
                            + dProdGrid.records[i].Ordered + dProdGrid.records[i].UnConfirmed) < dProdGrid.records[i].MinLevel))
                            ) {
                            src = VIS.Application.contextUrl + "Areas/VA011/Images/yellow.png";
                        }
                        else if ((dProdGrid.records[i].QtyOnHand < dProdGrid.records[i].MinLevel) && ((dProdGrid.records[i].QtyOnHand - dProdGrid.records[i].Demanded
                            + dProdGrid.records[i].Ordered + dProdGrid.records[i].UnConfirmed) < dProdGrid.records[i].MinLevel)) {
                            src = VIS.Application.contextUrl + "Areas/VA011/Images/red.png";
                        }

                        //if (dProdGrid.records[i]["QtyToReplenish"] < 0) {
                        //    src = VIS.Application.contextUrl + "Areas/VA011/Images/green.png";
                        //}
                        //else if (dProdGrid.records[i]["QtyToReplenish"] > 0) {
                        //    src = VIS.Application.contextUrl + "Areas/VA011/Images/red.png";
                        //}
                        //else if (dProdGrid.records[i]["QtyToReplenish"] == 0) {
                        //    src = VIS.Application.contextUrl + "Areas/VA011/Images/yellow.png";
                        //}
                        var rec = i + 1;
                        $("#grid_VA011_gridProd_" + $self.windowNo + "_rec_" + rec).find("img").prop("src", src);
                    }
                }
            }
            // }
        };

        // Reset Bottom Grid Panel
        function resetBottomGrid() {
            $divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
            $divLocatorGrid.css("display", "none");
            $divOrderedGrid.css("display", "none");
            $divTransactionsGrid.css("display", "none");
            $divDemandGrid.css("display", "none");
            $divReplenishedGrid.css("display", "none");
            $divVariantGrid.css("display", "block");
            $divReplenishmentBGrid.css("display", "none");
            $("#VA011_btnVariant_" + $self.windowNo).addClass("VA011-selectedTab");

            dTransactionsGrid.clear();
            dDemandGrid.clear();
            dReplenishedGrid.clear();
            dOrderedGrid.clear();
            dLocatorGrid.clear();
            dVariantGrid.clear();
            if (dReplenishmentBGrid != null) {
                dReplenishmentBGrid.clear();
            }
        };

        // Create busy Indicator
        function createBusyIndicator() {
            $bsyDiv = $("<div class='vis-apanel-busy'>");
            $bsyDiv.css({
                "position": "absolute", "width": "98%", "height": "97%", 'text-align': 'center', 'z-index': '999'
            });
            $bsyDiv[0].style.visibility = "visible";
            $root.append($bsyDiv);
        };

        function middlePanel() {

        };

        function rightPanel() {
            if (!initLoad) {
                $bsyDiv[0].style.visibility = "hidden";
            }
        };

        function replenishPanel() {
            $divReplenish.append($div);
            $div.append('<div class="VA011-product-data"><label>' + VIS.Msg.getMsg("VA011_Warehouse") + '</label><select class="vis-gc-vpanel-table-mandatory" id="VA011_cmbWarehouse_' + $self.windowNo + '"></select></div>'
                      + '<div class="VA011-product-data"><label>' + VIS.Msg.getMsg("VA011_ReplenishmentCreate") + '</label><select class="vis-gc-vpanel-table-mandatory" id="VA011_cmbCat_' + $self.windowNo + '"></select></div>'
                      + '<div class="VA011-product-data"><label>' + VIS.Msg.getMsg("VA011_DocumentType") + '</label><select id="VA011_cmbType_' + $self.windowNo + '" ></select></div>'
                      + '<div class="VA011-product-data"><label>' + VIS.Msg.getMsg("VA011_Vendor") + '</label><select id="VA011_cmbVendor_' + $self.windowNo + '"></select></div>'
                      + '<div class="VA011-product-data"><label>' + VIS.Msg.getMsg("VA011_DocStatus") + '</label><select class="vis-gc-vpanel-table-mandatory" id="VA011_cmbDocStatus_' + $self.windowNo + '" ></select></div>'
                      + '<div class="VA011-product-data"><input id="VA011_ConsiderOrderPack' + $self.windowNo + '" type="checkbox"><span>' + VIS.Msg.getMsg("VA011_ConsiderOrderPack") + '</span></div>');

            cmbCreate = $div.find("#VA011_cmbCat_" + $self.windowNo);
            cmbDocType = $div.find("#VA011_cmbType_" + $self.windowNo);
            cmbDocStatus = $div.find("#VA011_cmbDocStatus_" + $self.windowNo);
            cmbWarehouses = $div.find("#VA011_cmbWarehouse_" + $self.windowNo);
            cmbSuppliers = $div.find("#VA011_cmbVendor_" + $self.windowNo);
        };

        // Replenish Rule Pop Up
        function replenishRulePanel() {
            //$divImgAdd = $('<img class="VA011_RRulePopUp"  src=' + VIS.Application.contextUrl + 'Areas/VA011/Images/add.png>');
            //$divRepRuleAllTop.append($divImgAdd);
            //$divRepAllWarehouse = $('<img class="VA011_RRulePopUp"  src=' + VIS.Application.contextUrl + 'Areas/VA011/Images/add.png>');
            $divRepRuleAllTop.append($cmbRepAllWarehouse);
            //$divImgSaveAll = $('<img class="VA011_RRulePopUp" style="margin-right:5px" src=' + VIS.Application.contextUrl + 'Areas/VA011/Images/save-uom.png>');
            //$divRepRuleAllTop.append($divImgSaveAll);
            $divReplenishRuleAll.append($divRepRuleAllTop).append($divRepAllPop);
            $divReplenishmentPopGrid = $('<div class="VA011-gridCls" style="display:block; height:100%"></div>');
            $divRepAllPop.append($divReplenishmentPopGrid);
        };

        function callbackLoadCmbCreate(dr) {

            //cmbCreate.append(" <option value = 0></option>");
            while (dr.read()) {
                key = dr.getString(0);
                value = dr.getString(1);
                cmbCreate.append(" <option value=" + key + ">" + VIS.Utility.encodeText(value) + "</option>");
            }
            dr.close();

            LoadDocTypeCombo();
            //cmbCart.val(cart);
        };

        function LoadDocTypeCombo() {
            cmbDocType.empty();
            var qry = "SELECT C_DocType_ID, Name, DocBaseType FROM C_DocType WHERE DocBaseType IN (SELECT Value FROM AD_Ref_List WHERE AD_Reference_ID =(SELECT AD_Reference_ID FROM AD_Reference WHERE Name = 'M_Replenishment Create')) AND IsReturnTrx ='N' AND AD_Client_ID = " + VIS.context.getContext("#AD_Client_ID");
            VIS.DB.executeReader(qry, null, callbackLoadCmbDocType);
        };

        function callbackLoadCmbDocType(dr) {
            //cmbDocType.append(" <option value = 0></option>");
            while (dr.read()) {
                key = (dr.getString(0));
                listKeyDocType.push(key);
                value = dr.getString(1);
                listNameDocType.push(value);
                listValueDocType.push(dr.getString(2));
                //cmbDocType.append(" <option value=" + key + ">" + VIS.Utility.encodeText(value) + "</option>");
            }
            dr.close();
            LoadDocStatusCombo();
            //cmbCart.val(cart);
        };

        function callbackLoadDocStatus(dr) {

            cmbDocStatus.append(" <option value = 0></option>");
            while (dr.read()) {
                key = dr.getString(0);
                value = dr.getString(1);
                cmbDocStatus.append(" <option value=" + key + ">" + VIS.Utility.encodeText(value) + "</option>");
            }
            dr.close();

            bindProductGrid(false);
            //cmbCart.val(cart);
        };

        function LoadDocStatusCombo() {
            cmbDocStatus.empty();
            var qry = "SELECT Value,  Name FROM AD_Ref_List WHERE ad_reference_ID = (SELECT AD_Reference_ID FROM AD_Reference  WHERE Name = '_Document Status') AND Value IN ('DR', 'CO', 'IP')";
            VIS.DB.executeReader(qry, null, callbackLoadDocStatus);
        };

        function clearRightPanel() {
            // btnUom.removeClass("VA011-selectedTab");
            btnVarient.removeClass("VA011-selectedTab");
            cons = [];
            attributes = [];
            attrArray = {};
            c_UomConv_ID = 0;
            m_attribute_ID = 0;
            $divVarient.find(".VA011-checkbox").prop("checked", false);
            $divVarient.find(".VA011-uom-wrap").removeClass('vis-group-selected-op vis-group-selected-opbackground');
            $divUomGroup.find(".VA011-checkbox").prop("checked", false);
            $divUomGroup.find(".VA011-uom-wrap").removeClass('vis-group-selected-op vis-group-selected-opbackground');
            cancelConversion();
            clearVarient();
            divUom.hide();
            $divUomGroup.hide();
            divVarient.hide();
            $divVarient.hide();
            $divLeftTree.hide();
            //$rightPanel.css("opacity", 0.6);
            //$rightPanel.css("background-color", "#f1f1f1");
        };

        function LoadReplenishType() {
            repTypeArray = [];
            var qry = "SELECT Value, Name FROM AD_Ref_List WHERE AD_Reference_ID = (SELECT AD_Reference_ID FROM AD_Reference WHERE Name = 'M_Replenish Type' ) AND Value IN ('1','2')";
            VIS.DB.executeReader(qry, null, callbackReplenishType);
        };

        function callbackReplenishType(dr) {
            while (dr.read()) {
                key = (dr.getString(0));
                value = dr.getString(1);
                repTypeArray.push({ id: key, text: value });
            }
            dr.close();

            isReplenishTypeLoaded = true;

            gridReplenishmentBPanel();

            //bindReplenishmentBGrid();
            $divBottomMiddle.append($divReplenishmentBGrid);

            LoadCreateCombo();
        };

        function LoadCreateCombo() {
            cmbCreate.empty();
            var qry = "SELECT Value, Name FROM AD_Ref_List WHERE ad_reference_ID = (SELECT AD_Reference_ID FROM AD_Reference  WHERE Name = 'M_Replenishment Create')";
            VIS.DB.executeReader(qry, null, callbackLoadCmbCreate);
        };

        function LoadUOM() {
            uomArray = [];
            uomArray.push({ id: 0, text: VIS.Msg.getMsg("Select") });
            var qry = "SELECT C_UOM_ID,Name FROM C_UOM WHERE IsActive = 'Y'";
            var sql = VIS.MRole.getDefault().addAccessSQL(qry, "C_UOM", VIS.MRole.SQL_FULLYQUALIFIED, VIS.MRole.SQL_RO) // fully qualidfied - RO
            VIS.DB.executeReader(sql.toString(), null, LoadUOMCallBack);
        };

        function LoadUOMCallBack(dr) {
            while (dr.read()) {
                key = VIS.Utility.Util.getValueOfInt(dr.getString(0));
                value = dr.getString(1);
                uomArray.push({ id: key, text: value });
            }
            dr.close();
            CartPanel();
        };

        function calculateNoofPages() {
            var noofPages = 1;
            var rem = totalRecords % pgSize;
            if (rem != 0) {
                noofPages = parseInt(totalRecords / pgSize) + 1;
            }
            else {
                noofPages = parseInt(totalRecords / pgSize);
            }
            return noofPages;
        };

        function calculateReplenish() {
            if (dProdGrid.records.length > 0) {
                var prods = [];
                for (var k = 0; k < dProdGrid.records.length; k++) {
                    prods.push(VIS.Utility.Util.getValueOfInt(dProdGrid.records[k].M_Product_ID));
                }
                $bsyDiv[0].style.visibility = "visible";
                VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/GetReplenish", { "ColumnName": prods }, callbackBindProdReplenish);
            }
        };

        function ReplenishEvents() {
            ch.onOkClick = function (e) {


                var warehouse_ID = cmbWarehouses.val();
                var bp_ID = cmbSuppliers.val();
                var docType = cmbDocType.val();
                if (docType == null) {
                    docType = 0;
                }
                var docStatus = cmbDocStatus.val();
                _docStatus = docStatus;
                var create = cmbCreate.val();
                if (bp_ID == null) {
                    bp_ID = 0;
                }
                var considerOrderPack = "N";
                if ($("#VA011_ConsiderOrderPack" + $self.windowNo)[0].checked) {
                    considerOrderPack = "Y";
                }

                if (warehouse_ID == null || docStatus == null || create == null) {
                    VIS.ADialog.error("FillMandatory");
                    return false;
                }

                $bsyDiv[0].style.visibility = "visible";

                VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/GenerateReplenishmentReport", { "M_Warehouse_ID": warehouse_ID, "C_BPartner_ID": bp_ID, "C_DocType_ID": docType, "DocStatus": docStatus, "Create": create, "OrderPack": considerOrderPack }, callbackReplenishTopGrid);
            };

            ch.onCancelClick = function () {
                //ClearProdData();
            };

            cmbCreate.on("change", function () {
                cmbDocType.empty();

                for (var i = 0; i < listValueDocType.length; i++) {
                    if (listValueDocType[i] == cmbCreate.val())
                        cmbDocType.append(" <option value=" + listKeyDocType[i] + ">" + VIS.Utility.encodeText(listNameDocType[i]) + "</option>");
                }
            });

            cmbDocType.on("change", function (e) {

            });

            cmbWarehouses.on("change", function (e) {
                console.log("Warehouse Change Event Fired");
            });
        };

        function callbackReplenishTopGrid(data) {
            $divHeadVarientProdTop.find(".VA011-tabs").children().removeClass("VA011-selectedTab");

            $("#VA011_GenReplenish_" + $self.windowNo).addClass("VA011-selectedTab");

            $("#VA011_btnReplenish_" + $self.windowNo).css("display", "none");
            $("#VA011_btnGenerateReplenish_" + $self.windowNo).css("display", "block");

            if (calculateReplenishLI) {
                calculateReplenishLI.css("display", "none");
            };

            if (addToCartLI) {
                addToCartLI.css("display", "none");
            };

            if (displayRepRuleAllLI) {
                displayRepRuleAllLI.css("display", "none");
            };

            if (generateReplenishLI) {
                generateReplenishLI.css("display", "block");
            };

            $statusProdDiv.css("display", "none");

            grdPrepTopValues = [];
            dRepTopGrid.clear();
            var Recid = 0;

            if (data.Table != null) {
                // Bind Grid Product Here
                for (var i = 0; i < data.Table.length; i++) {
                    Recid = Recid + 1;
                    grdPrepTopValues.push(
                    {
                        recid: Recid,
                        AD_Client_ID: data.Table[i].AD_CLIENT_ID,
                        AD_Org_ID: data.Table[i].AD_ORG_ID,
                        RepCreate: data.Table[i].REPLENISHMENTCREATE,
                        M_PriceList_ID: data.Table[i].M_PRICELIST_ID,
                        M_WarehouseSource_ID: data.Table[i].M_WAREHOUSESOURCE_ID,
                        M_Product_ID: data.Table[i].M_PRODUCT_ID,
                        M_Warehouse_ID: data.Table[i].M_WAREHOUSE_ID,
                        C_BPartner_ID: data.Table[i].C_BPARTNER_ID,
                        C_DocType_ID: data.Table[i].C_DOCTYPE_ID,
                        DocStatus: _docStatus,
                        Product: data.Table[i].PRODUCT,
                        Warehouse: data.Table[i].WAREHOUSE,
                        SourceWarehouse: data.Table[i].SOURCEWAREHOUSE,
                        ReplenishmentType: data.Table[i].REPLENISHTYPE,
                        Max: data.Table[i].LEVEL_MAX,
                        Min: data.Table[i].LEVEL_MIN,
                        QtyOnHand: data.Table[i].QTYONHAND,
                        Ordered: data.Table[i].QTYORDERED,
                        ReqReserved: data.Table[i].DTD001_QTYRESERVED,
                        Reserved: data.Table[i].QTYRESERVED,
                        QtyToOrder: data.Table[i].QTYTOORDER
                    });
                }

            }
            w2utils.encodeTags(grdPrepTopValues);
            dRepTopGrid.add(grdPrepTopValues);
            dRepTopGrid.selectNone();
            _product_ID = 0;
            grdPrepTopValues = [];

            $divProdGrid.css("display", "none");
            $divRepTopGrid.css("display", "block");
            $bsyDiv[0].style.visibility = "hidden";
        };

        function bindleftPanelWarehouseCombos(ctrl, funName) {
            VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/" + funName, { "Value": "", "orgs": selOrgs, "fill": true }, callbackBindOrgWhCombo);
        };

        function callbackBindOrgWhCombo(result) {
            datasource = [];

            for (var i = 0; i < result.length; i++) {
                datasource.push({ 'label': result[i].Name, 'value': result[i].Name, 'ids': result[i].ID });
            }

            $($divWarehouse[0]).autocomplete('option', 'source', datasource)
            $($divWarehouse[0]).autocomplete("search", "");
            $($divWarehouse[0]).trigger("focus");
        };

        function bindleftPanelCombos(ctrl, funName) {
            $.ajax({
                url: VIS.Application.contextUrl + "Inventory/" + funName,
                datatype: "json",
                type: "get",
                cache: false,
                data: { value: "", fill: true },
                success: function (data) {

                    var result = JSON.parse(data);
                    datasource = [];

                    for (var i = 0; i < result.length; i++) {
                        datasource.push({ 'label': result[i].Name, 'value': result[i].Name, 'ids': result[i].ID });
                    }

                    $(ctrl[0]).autocomplete('option', 'source', datasource)
                    $(ctrl[0]).autocomplete("search", "");
                    $(ctrl[0]).trigger("focus");

                    //if (SelectedValues == null || SelectedValues == undefined) {
                    //    $(self.div).autocomplete("search", "");
                    //    $(self.div).trigger("focus");
                    //}

                    //SelectedValues = null;
                }
            });
        };

        var records = [];
        function generateReplenishments() {
            debugger;
            var x = 0;
            if (dRepTopGrid.getSelection().length > 0) {

                $bsyDiv[0].style.visibility = "visible";

                for (var j = 0; j < dRepTopGrid.records.length; j++) {
                    if (dRepTopGrid.getSelection().map(function (item) { return item == dRepTopGrid.records[j].recid; }).indexOf(true) >= 0) {
                        var obj = {
                            "AD_Client_ID": dRepTopGrid.records[j].AD_Client_ID, "AD_Org_ID": dRepTopGrid.records[j].AD_Org_ID,
                            "RepCreate": dRepTopGrid.records[j].RepCreate, "M_PriceList_ID": dRepTopGrid.records[j].M_PriceList_ID,
                            "C_BPartner_ID": dRepTopGrid.records[j].C_BPartner_ID, "C_DocType_ID": dRepTopGrid.records[j].C_DocType_ID,
                            "DocStatus": dRepTopGrid.records[j].DocStatus, "M_Product_ID": dRepTopGrid.records[j].M_Product_ID,
                            "M_WarehouseSource_ID": dRepTopGrid.records[j].M_WarehouseSource_ID, "M_Warehouse_ID": dRepTopGrid.records[j].M_Warehouse_ID,
                            "Max": dRepTopGrid.records[j].Max, "Min": dRepTopGrid.records[j].Min,
                            "Ordered": dRepTopGrid.records[j].Ordered, "QtyOnHand": dRepTopGrid.records[j].QtyOnHand,
                            "QtyToOrder": dRepTopGrid.records[j].QtyToOrder, "ReplenishmentType": dRepTopGrid.records[j].ReplenishmentType,
                            "ReqReserved": dRepTopGrid.records[j].ReqReserved, "Reserved": dRepTopGrid.records[j].Reserved,
                        };
                        records.push(obj);
                    }
                }

                VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/GenerateReps", { "Reps": records }, callbackReps);
            }
            else {
                alert(VIS.Msg.getMsg("VA011_PleaseSelectaRecord"));
                $bsyDiv[0].style.visibility = "hidden";
            }
        };

        function callbackReps(data) {
            console.log(data);
            records = [];

            $divProdGrid.css("display", "block");
            $divRepTopGrid.css("display", "none");
            if (data != null) {
                if (data != "") {
                    alert(data);
                }
            }
            bindProductGrid(false);
            //$bsyDiv[0].style.visibility = "hidden";
        };

        //Event
        function events() {

            $divProductCategories.on(VIS.Events.onTouchStartOrClick, function (e) {
                if (e.target.className.indexOf("VA011-imgCombo") >= 0) {
                    bindleftPanelCombos($divProductCategories, "GetProductCategories");
                }
            });

            $divOrg.on(VIS.Events.onTouchStartOrClick, function (e) {
                if (e.target.className.indexOf("VA011-imgCombo") >= 0) {
                    bindleftPanelCombos($divOrg, "GetOrgs");
                }
            });

            $divWarehouse.on(VIS.Events.onTouchStartOrClick, function (e) {
                if (e.target.className.indexOf("VA011-imgCombo") >= 0) {
                    bindleftPanelWarehouseCombos($divWarehouse, "GetOrgWarehouse");
                }
            });

            $divPLV.on(VIS.Events.onTouchStartOrClick, function (e) {
                if (e.target.className.indexOf("VA011-imgCombo") >= 0) {
                    bindleftPanelCombos($divPLV, "GetPLV");
                }
            });

            $divSupplier.on(VIS.Events.onTouchStartOrClick, function (e) {
                if (e.target.className.indexOf("VA011-imgCombo") >= 0) {
                    bindleftPanelCombos($divSupplier, "GetSupplier");
                }
            });

            $inputSearchBtnProd.on(VIS.Events.onTouchStartOrClick, function (e) {
                searchProdText = $inputSearchProd.val();
                pgNo = 1;
                bindProductGrid(false);
            });

            $inputSearchProd.on("keyup", function (e) {
                if (event.keyCode == 13) {
                    searchProdText = $inputSearchProd.val();
                    pgNo = 1;
                    bindProductGrid(false);
                }
            });

            $divHeadDetails.on(VIS.Events.onTouchStartOrClick, "LI", function (e) {
                var btn = $(e.target).attr("id");
                if (btn.indexOf("VA011_btnDetails_") >= 0) {
                    if (!multipleRecSelected) {
                        $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                        $("#" + btn).addClass("VA011-selectedTab");
                        setProductDetails();
                    }
                }
                else if (btn.indexOf("VA011_btnSubsti_") >= 0) {
                    if (!multipleRecSelected) {
                        $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                        $("#" + btn).addClass("VA011-selectedTab");
                        bindSubstituteGrid();
                    }
                }
                else if (btn.indexOf("VA011_btnRelated_") >= 0) {
                    if (!multipleRecSelected) {
                        $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                        $("#" + btn).addClass("VA011-selectedTab");
                        bindRelatedGrid();
                    }
                }
                else if (btn.indexOf("VA011_btnSuppliers_") >= 0) {
                    if (!multipleRecSelected) {
                        $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                        $("#" + btn).addClass("VA011-selectedTab");
                        bindSuppliersRightGrid();
                    }
                }
                else if (btn.indexOf("VA011_btnKits_") >= 0) {
                    if (!multipleRecSelected) {
                        $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                        $("#" + btn).addClass("VA011-selectedTab");
                        bindKitsGrid();
                    }
                }
                else if (btn.indexOf("VA011_btnCart_") >= 0) {
                    $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                    $("#" + btn).addClass("VA011-selectedTab");
                    BindCartGrid();
                }
            });

            $rightFullDetails.on(VIS.Events.onTouchStartOrClick, "LI", function (e) {
                var btn = $(e.target).attr("id");
                //if (btn.indexOf("VA011_btnDetails_") >= 0) {
                //    $rightFullDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                //    $("#" + btn).addClass("VA011-selectedTab");
                //}
                //else if (btn.indexOf("VA011_btnStatistics_") >= 0) {
                //    $rightFullDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                //    $("#" + btn).addClass("VA011-selectedTab");
                //}
            });

            $divHeadVarientProdTop.on(VIS.Events.onTouchStartOrClick, "LI", function (e) {
                var btn = $(e.target).attr("id");
                if (btn.indexOf("VA011_btnReplenish_") >= 0) {

                    // $divHeadVarientProdTop.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                    //$("#" + btn).addClass("VA011-selectedTab");
                    $divProdGrid.css("display", "block");
                    $divRepTopGrid.css("display", "none");
                    calculateReplenish();
                }
                else if (btn.indexOf("VA011_StockDet_") >= 0) {
                    $bsyDiv[0].style.visibility = "visible";
                    $divProdGrid.css("display", "block");
                    $divRepTopGrid.css("display", "none");
                    bindProductGrid(false);
                }
                else if (btn.indexOf("VA011_btnGenerateReplenish_") >= 0) {
                    $bsyDiv[0].style.visibility = "visible";
                    generateReplenishments();
                }
                else if (btn.indexOf("VA011_GenReplenish_") >= 0) {

                    cmbDocStatus[0].selectedIndex = 1;
                    ch = new VIS.ChildDialog();
                    ch.setContent($divReplenish);
                    $divReplenish.show();
                    ch.setHeight(500);
                    ch.setWidth(320);
                    ch.setTitle(VIS.Msg.getMsg("VA011_Replenishments"));
                    ch.setModal(true);
                    //Ok Button Click
                    //ch.onOkClick =
                    //Disposing Everything on Close
                    ch.onClose = function () {
                        //ClearProdData();
                    };
                    ch.show();
                    cmbDocType[0].selectedIndex = -1;
                    cmbCreate[0].selectedIndex = -1;
                    cmbWarehouses[0].selectedIndex = -1;
                    cmbSuppliers[0].selectedIndex = -1;
                    ReplenishEvents();
                }
                else if (btn.indexOf("VA011_btnAddToCart_") >= 0) {
                    if (parseFloat(cart) > 0) {
                        if (dProdGrid.getSelection() <= 0) {
                            alert(VIS.Msg.getMsg("VA011_PleaseSelectaRecord"));
                        }
                        else {
                            cartSelectionValues = [];
                            var selRecords = dProdGrid.getSelection();
                            var Recid = 1;
                            for (var i = 0; i < selRecords.length; i++) {
                                cartSelectionValues.push(
                               {
                                   recid: Recid,
                                   product_ID: dProdGrid.records[selRecords[i] - 1].M_Product_ID,
                                   Product: dProdGrid.records[selRecords[i] - 1].Product,
                                   C_Uom_ID: dProdGrid.records[selRecords[i] - 1].C_UOM_ID,
                                   Attribute_ID: 0,
                                   Attribute: "",
                                   UPC: "",
                                   Qty: 1
                               });
                                Recid++;
                            }

                            saveInventory();
                        }
                    }
                    else {
                        alert(VIS.Msg.getMsg("VA011_CartNotSelected"));
                    }
                    //alert("Add to Cart");
                }
                else if (btn.indexOf("VA011_DisplayReplenishRule_") >= 0) {
                    debugger;

                    if (dProdGrid.getSelection().length <= 0) {
                        alert(VIS.Msg.getMsg("VA011_PleaseSelectaRecord"));
                        $bsyDiv[0].style.visibility = "hidden";
                        return;
                    }

                    cmbDocStatus[0].selectedIndex = 1;
                    chldDlgRepAll = new VIS.ChildDialog();
                    chldDlgRepAll.setContent($divReplenishRuleAll);

                    $divReplenishRuleAll.show();
                    chldDlgRepAll.setHeight(400);
                    chldDlgRepAll.setWidth(800);
                    chldDlgRepAll.setTitle(VIS.Msg.getMsg("VA011_ReplenishmentsRule"));
                    chldDlgRepAll.setModal(true);

                    //Ok Button Click
                    //ch.onOkClick =
                    //Disposing Everything on Close
                    chldDlgRepAll.onClose = function () {
                        // dProdGrid.selectNone();
                        // ClearProdData();
                    };

                    var selectedWhID = 0;
                    if (selWh.length > 0) {
                        selectedWhID = selWh[0];
                    }
                    else {
                        selectedWhID = VIS.context.getContext("#M_Warehouse_ID");
                    }

                    var ind = 0;

                    if (dReplenishmentPopGrid == null) {
                        gridReplenishmentPopPanel();
                        for (var i = 0; i < listWarehouses.length; i++) {
                            if (selectedWhID == listWarehouses[i].id) {
                                ind = i;
                            }
                            $cmbRepAllWarehouse.append('<option value=' + listWarehouses[i].id + '>' + listWarehouses[i].text + '</option>');
                        }
                        $cmbRepAllWarehouse[0].selectedIndex = ind;
                    }
                    else {
                        for (var i = 0; i < listWarehouses.length; i++) {
                            if (selectedWhID == listWarehouses[i].id) {
                                ind = i;
                            }
                            //$cmbRepAllWarehouse.append('<option value=' + listWarehouses[i].id + '>' + listWarehouses[i].text + '</option>');
                        }
                        $cmbRepAllWarehouse[0].selectedIndex = ind;
                    }


                    chldDlgRepAll.show();

                    bindReplenishmentPopGrid();



                    //$divImgAdd.on(VIS.Events.onTouchStartOrClick, function (e) {
                    //    debugger;

                    //    if (grdReplenishmentPopValues.length > 0) {
                    //        if (grdReplenishmentPopValues[0].M_Product_ID == "") {
                    //            alert(VIS.Msg.getMsg("VA011_ProductMandatory"));
                    //            return;
                    //        }
                    //        else if (grdReplenishmentPopValues[0].M_Warehouse_ID == "") {
                    //            alert(VIS.Msg.getMsg("VA011_WarehouseMandatory"));
                    //            return;
                    //        }
                    //    }

                    //    if (dReplenishmentPopGrid != null) {
                    //        dReplenishmentPopGrid.clear();
                    //    }

                    //    var newRecord = [];
                    //    newRecord.push(
                    //        {
                    //            recid: 1,
                    //            Product: "",
                    //            Warehouse: "",
                    //            Type: "",
                    //            Min: 0,
                    //            Max: 0,
                    //            Qty: 1,
                    //            OrderPack: 1,
                    //            SourceWarehouse: "",
                    //            M_Product_ID: "",
                    //        });

                    //    for (var i = 0; i < grdReplenishmentPopValues.length; i++) {
                    //        grdReplenishmentPopValues[0].recid = grdReplenishmentPopValues[0].recid + 1;
                    //    }

                    //    grdReplenishmentPopValues.unshift(newRecord[0]);

                    //    w2utils.encodeTags(grdReplenishmentPopValues);

                    //    dReplenishmentPopGrid.add(grdReplenishmentPopValues);

                    //});

                    ReplenishAllPopEvents();
                }
            });

            

            function ReplenishAllPopEvents() {

                $cmbRepAllWarehouse.on("change", function () {
                    bindReplenishmentPopGrid();
                });

                chldDlgRepAll.onCancelClick = function () {
                    //dProdGrid.selectNone();
                    //ClearProdData();
                };

                chldDlgRepAll.onOkClick = function (e) {
                    var recordsRepPopAll = [];
                    var sqlWhere = "";

                    $bsyDiv[0].style.visibility = "visible";
                    debugger;
                    for (var j = 0; j < dReplenishmentPopGrid.records.length; j++) {
                        //if (dReplenishmentPopGrid.getSelection().map(function (item) { return item == dReplenishmentPopGrid.records[j].recid; }).indexOf(true) >= 0) {
                        recSelected = true;
                        sqlWhere += dProdGrid.records[j].M_Product_ID + ",";
                        //}

                        var warehoue_ID = $cmbRepAllWarehouse.val();
                        var prod_ID = dReplenishmentPopGrid.records[j].M_Product_ID;
                        var type = dReplenishmentPopGrid.records[j].Type;
                        var min = dReplenishmentPopGrid.records[j].Min;
                        var max = dReplenishmentPopGrid.records[j].Max;
                        var qty = dReplenishmentPopGrid.records[j].Qty;
                        var SourceWarehouse = dReplenishmentPopGrid.records[j].SourceWarehouse;
                        var orderPack = dReplenishmentPopGrid.records[j].OrderPack;
                        if (type == "" || type == null) {
                            alert(VIS.Msg.getMsg("VA011_ReplenishTypeNotSetFor") + " : " + dReplenishmentPopGrid.records[j].Product);
                            $bsyDiv[0].style.visibility = "hidden";
                            return false;
                        }

                        var obj = {
                            "M_Warehouse_ID": $cmbRepAllWarehouse.val(), "M_Product_ID": dReplenishmentPopGrid.records[j].M_Product_ID,
                            "RepType": dReplenishmentPopGrid.records[j].Type, "Min": dReplenishmentPopGrid.records[j].Min,
                            "Max": dReplenishmentPopGrid.records[j].Max, "OrderPack": dReplenishmentPopGrid.records[j].OrderPack,
                            "Qty": dReplenishmentPopGrid.records[j].Qty,
                            "SourceWarehouse": dReplenishmentPopGrid.records[j].SourceWarehouse,
                        };
                        recordsRepPopAll.push(obj);
                    }

                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/SaveReplenishmentRuleAll", { "RepAll": recordsRepPopAll }, callbackSaveReplenishmentPop);

                };
            };

            $divHeadVarientProdBottom.on(VIS.Events.onTouchStartOrClick, "LI", function (e) {
                if (!multipleRecSelected) {
                    var btn = $(e.target).attr("id");
                    if (btn.indexOf("VA011_btnVariant_") >= 0) {
                        $divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                        $divLocatorGrid.css("display", "none");
                        $divOrderedGrid.css("display", "none");
                        $divTransactionsGrid.css("display", "none");
                        $divDemandGrid.css("display", "none");
                        $divReplenishedGrid.css("display", "none");
                        $divVariantGrid.css("display", "block");
                        $divReplenishmentBGrid.css("display", "none");
                        $("#" + btn).addClass("VA011-selectedTab");
                        bindVariantGrid();
                    }
                    else if (btn.indexOf("VA011_btnLocators_") >= 0) {
                        $divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                        $("#" + btn).addClass("VA011-selectedTab");
                        $divOrderedGrid.css("display", "none");
                        $divTransactionsGrid.css("display", "none");
                        $divDemandGrid.css("display", "none");
                        $divReplenishedGrid.css("display", "none");
                        $divVariantGrid.css("display", "none");
                        $divLocatorGrid.css("display", "block");
                        $divReplenishmentBGrid.css("display", "none");
                        bindLocatorGrid();
                    }
                    else if (btn.indexOf("VA011_btnOrdered") >= 0) {
                        $divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                        $("#" + btn).addClass("VA011-selectedTab");
                        $divOrderedGrid.css("display", "block");
                        $divTransactionsGrid.css("display", "none");
                        $divDemandGrid.css("display", "none");
                        $divReplenishedGrid.css("display", "none");
                        $divVariantGrid.css("display", "none");
                        $divLocatorGrid.css("display", "none");
                        $divReplenishmentBGrid.css("display", "none");
                        bindOrderedGrid();
                    }
                    else if (btn.indexOf("VA011_btnReplenished_") >= 0) {
                        $divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                        $("#" + btn).addClass("VA011-selectedTab");
                        $divOrderedGrid.css("display", "none");
                        $divTransactionsGrid.css("display", "none");
                        $divDemandGrid.css("display", "none");
                        $divReplenishedGrid.css("display", "block");
                        $divVariantGrid.css("display", "none");
                        $divLocatorGrid.css("display", "none");
                        $divReplenishmentBGrid.css("display", "none");
                        bindReplenishedGrid();
                    }
                    else if (btn.indexOf("VA011_btnDemand_") >= 0) {
                        $divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                        $("#" + btn).addClass("VA011-selectedTab");
                        $divOrderedGrid.css("display", "none");
                        $divTransactionsGrid.css("display", "none");
                        $divDemandGrid.css("display", "block");
                        $divReplenishedGrid.css("display", "none");
                        $divVariantGrid.css("display", "none");
                        $divLocatorGrid.css("display", "none");
                        $divReplenishmentBGrid.css("display", "none");
                        bindDemandGrid();
                    }
                    else if (btn.indexOf("VA011_btnTransactions_") >= 0) {
                        $divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                        $("#" + btn).addClass("VA011-selectedTab");
                        $divOrderedGrid.css("display", "none");
                        $divTransactionsGrid.css("display", "block");
                        $divDemandGrid.css("display", "none");
                        $divReplenishedGrid.css("display", "none");
                        $divVariantGrid.css("display", "none");
                        $divLocatorGrid.css("display", "none");
                        $divReplenishmentBGrid.css("display", "none");
                        bindTransactionsGrid();
                    }
                    else if (btn.indexOf("VA011_btnReplenishmentB_") >= 0) {
                        $divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
                        $divLocatorGrid.css("display", "none");
                        $divOrderedGrid.css("display", "none");
                        $divTransactionsGrid.css("display", "none");
                        $divDemandGrid.css("display", "none");
                        $divReplenishedGrid.css("display", "none");
                        $divVariantGrid.css("display", "none");
                        $divReplenishmentBGrid.css("display", "block");
                        $("#" + btn).addClass("VA011-selectedTab");
                        bindReplenishmentBGrid();
                    }
                }
                else {

                }
            });

            if ($ulLefttoolbar) {
                $ulLefttoolbar.on(VIS.Events.onTouchStartOrClick, "LI", function (e) {

                    pcat_ID = VIS.Utility.Util.getValueOfInt($(e.target).attr("procatid"));
                    var cname = $(e.target).text();
                    cname = " -> " + cname.substr(0, cname.indexOf("("));
                    if (pcat_ID > 0) {
                        $ulLefttoolbar.find("LI").removeClass("VA011-cat-selected");
                        $ulLefttoolbar.find("LI").css("opacity", 0.6);
                        $ulLefttoolbar.find("li[procatid='" + pcat_ID + "']").addClass("VA011-cat-selected");
                        $ulLefttoolbar.find("li[procatid='" + pcat_ID + "']").css("opacity", 1);
                        prodCat_ID = pcat_ID;
                        pgNo = 1;
                        bindProductGrid(false);
                    }
                });
            }

            $cmbPageNo.on("change", function (e) {
                if (pgNo != $cmbPageNo.val()) {
                    pgNo = $cmbPageNo.val();
                    bindProductGrid(true);
                    if ($divHeadVarientProdTop.find("#VA011_btnReplenish_" + $self.windowNo).hasClass("VA011-selectedTab")) {
                        calculateReplenish();
                    }
                }
            });

            $ulStatusProdDiv.on(VIS.Events.onTouchStartOrClick, "LI", function (e) {

                if ($(e.target).attr("action") == "first") {
                    if (pgNo != 1) {
                        pgNo = 1;
                        execPaging = true;
                        bindProductGrid(true);
                        if ($divHeadVarientProdTop.find("#VA011_btnReplenish_" + $self.windowNo).hasClass("VA011-selectedTab")) {
                            calculateReplenish();
                        }

                    }
                }
                else if ($(e.target).attr("action") == "prev") {
                    if (pgNo != 1) {
                        pgNo--;
                        execPaging = true;
                        bindProductGrid(true);
                        if ($divHeadVarientProdTop.find("#VA011_btnReplenish_" + $self.windowNo).hasClass("VA011-selectedTab")) {
                            calculateReplenish();
                        }
                    }
                }
                else if ($(e.target).attr("action") == "next") {

                    var totRec = calculateNoofPages();
                    if (pgNo >= totRec) {
                        return;
                    }
                    pgNo++;
                    execPaging = true;
                    bindProductGrid(true);
                    if ($divHeadVarientProdTop.find("#VA011_btnReplenish_" + $self.windowNo).hasClass("VA011-selectedTab")) {
                        calculateReplenish();
                    }
                }
                else if ($(e.target).attr("action") == "last") {
                    var totRec = calculateNoofPages();
                    if (pgNo >= totRec) {
                        return;
                    }
                    execPaging = true;
                    pgNo = totRec;
                    bindProductGrid(true);
                    if ($divHeadVarientProdTop.find("#VA011_btnReplenish_" + $self.windowNo).hasClass("VA011-selectedTab")) {
                        calculateReplenish();
                    }
                }

            });

            $btnlbToggle.on(VIS.Events.onTouchStartOrClick, function (e) {

                e.stopPropagation();

                var w = $td0leftbar.width();
                var wr = $td2_tr1.width();
                if (w > 50) {
                    $divlbMain.hide();
                }

                $td0leftbar.animate({
                    "width": w > 50 ? 40 : 200
                }, 300, 'swing', function () {

                    if (w < 50) {
                        $divlbMain.show();
                    }

                    dProdGrid.resize();
                    dProdGrid.refresh();
                    gridResizing();
                    dVariantGrid.resize();
                    dDemandGrid.resize();
                    dOrderedGrid.refresh();
                    dReplenishedGrid.resize();
                    dLocatorGrid.resize();
                    dTransactionsGrid.resize();
                    dReplenishmentBGrid.resize();
                });
            });

            $divCheckBoxOrg.on(VIS.Events.onTouchStartOrClick, function (ev) {
                var tgt = ev.target;
                if (tgt.className != "VA011-clsDIV") {
                    var ind = -1;
                    var orgID = -1;
                    debugger;
                    if (tgt.id.indexOf("VA011_spnOrg") >= 0) {
                        var val = tgt.id.replace("VA011_spnOrg", "");
                        // orgID = $("#VA011_chkOrg" + val).val();
                        orgID = $("#VA011_chkOrg" + val).attr("value");
                    }
                    else if (tgt.id.indexOf("VA011_chkOrg") >= 0) {
                        orgID = $("#" + tgt.id).attr("value");
                        //var orgID = $("#" + tgt.id).val();
                    }

                    ind = selOrgs.indexOf(VIS.Utility.Util.getValueOfInt(orgID));
                    if (ind > -1) {
                        //selOrgs.splice(ind);
                        selOrgs = jQuery.grep(selOrgs, function (value) {
                            return value != orgID;
                        });
                    }
                    tgt.parentElement.remove();
                    pgNo = 1;
                    bindProductGrid(false);
                }
            });

            $divCheckBoxPLV.on(VIS.Events.onTouchStartOrClick, function (ev) {
                var tgt = ev.target;
                if (tgt.className != "VA011-clsDIV") {
                    var ind = -1;
                    var plvID = -1;
                    if (tgt.id.indexOf("VA011_spnPLV") >= 0) {
                        var val = tgt.id.replace("VA011_spnPLV", "");
                        // plvID = $("#VA011_chkPLV" + val).val();
                        plvID = $("#VA011_chkPLV" + val).attr("value");
                    }
                    else if (tgt.id.indexOf("VA011_chkPLV") >= 0) {
                        //plvID = $("#" + tgt.id).val();
                        plvID = $("#" + tgt.id).attr("value");
                    }

                    ind = selPLV.indexOf(VIS.Utility.Util.getValueOfInt(plvID));
                    if (ind > -1) {
                        //selPLV.splice(ind);
                        selPLV = jQuery.grep(selPLV, function (value) {
                            return value != plvID;
                        });
                    }
                    tgt.parentElement.remove();
                    pgNo = 1;
                    bindProductGrid(false);
                }
            });

            $divCheckBoxSupplier.on(VIS.Events.onTouchStartOrClick, function (ev) {

                var tgt = ev.target;
                if (tgt.className != "VA011-clsDIV") {
                    var ind = -1;
                    var suppID = -1;
                    if (tgt.id.indexOf("VA011_spnSupplier") >= 0) {
                        var val = tgt.id.replace("VA011_spnSupplier", "");
                        //suppID = $("#VA011_chkSupplier" + val).val();
                        suppID = $("#VA011_chkSupplier" + val).attr("value");
                    }
                    else if (tgt.id.indexOf("VA011_chkSupplier") >= 0) {
                        // suppID = $("#" + tgt.id).val();
                        suppID = $("#" + tgt.id).attr("value");
                    }
                    ind = selSupp.indexOf(VIS.Utility.Util.getValueOfInt(suppID));
                    if (ind > -1) {
                        //selSupp.splice(ind);
                        selSupp = jQuery.grep(selSupp, function (value) {
                            return value != suppID;
                        });
                    }
                    tgt.parentElement.remove();
                    pgNo = 1;
                    bindProductGrid(false);
                }
            });

            $divCheckBoxWarehouse.on(VIS.Events.onTouchStartOrClick, function (ev) {
                var tgt = ev.target;
                if (tgt.className != "VA011-clsDIV") {
                    var ind = -1;
                    var whID = -1;
                    if (tgt.id.indexOf("VA011_spnWh") >= 0) {
                        var val = tgt.id.replace("VA011_spnWh", "");
                        //whID = $("#VA011_chkWh" + val).val();
                        whID = $("#VA011_chkWh" + val).attr("value");
                    }
                    else if (tgt.id.indexOf("VA011_chkWh") >= 0) {
                        whID = $("#" + tgt.id).attr("value");
                        //whID = $("#" + tgt.id).val();
                    }

                    ind = selWh.indexOf(VIS.Utility.Util.getValueOfInt(whID));
                    if (ind > -1) {
                        //selWh.splice(ind);
                        selWh = jQuery.grep(selWh, function (value) {
                            return value != whID;
                        });
                    }

                    tgt.parentElement.remove();
                    pgNo = 1;
                    bindProductGrid(false);
                }
            });

            $divCheckBoxCategories.on(VIS.Events.onTouchStartOrClick, function (ev) {

                var tgt = ev.target;
                if (tgt.className != "VA011-clsDIV") {
                    var ind = -1;
                    var catID = -1;
                    if (tgt.id.indexOf("VA011_spnCategories") >= 0) {
                        var val = tgt.id.replace("VA011_spnCategories", "");
                        //catID = $("#VA011_chkCategories" + val).val();
                        catID = $("#VA011_chkCategories" + val).attr("value");
                    }
                    else if (tgt.id.indexOf("VA011_chkCategories") >= 0) {
                        //catID = $("#" + tgt.id).val();
                        catID = $("#" + tgt.id).attr("value");
                    }

                    ind = selCat.indexOf(VIS.Utility.Util.getValueOfInt(catID));
                    if (ind > -1) {
                        //selCat.splice(ind);
                        selCat = jQuery.grep(selCat, function (value) {
                            return value != catID;
                        });
                    }

                    tgt.parentElement.remove();
                    pgNo = 1;
                    bindProductGrid(false);
                }
            });

            btnNewCart.on(VIS.Events.onTouchStartOrClick, function (e) {
                divCartList.hide();
                divNewCart.show();
            });

            btnEditCart.on(VIS.Events.onTouchStartOrClick, function (e) {
                zoomToWindow(cmbCart.val(), "VAICNT_InventoryCount");
            });

            btnRefreshCart.on(VIS.Events.onTouchStartOrClick, function (e) {
                LoadCart();
            });

            btnSaveScan.on(VIS.Events.onTouchStartOrClick, function (e) {
                if (VIS.Utility.Util.getValueOfString(txtScan.val()) == "") {
                    VIS.ADialog.error("VA011_EnterScanName");
                    return;
                }
                else {
                    saveInventoryCount();
                }
            });

            btnSaveScan.on("keydown", function (e) {
                if (e.keyCode == 13) {
                    if (VIS.Utility.Util.getValueOfString(txtScan.val()) == "") {
                        VIS.ADialog.error("VA011EnterScanName");
                        return;
                    }
                    else {
                        saveInventoryCount();
                    }
                }
            });

            btnCancelScan.on(VIS.Events.onTouchStartOrClick, function (e) {
                divNewCart.hide();
                divCartList.show();
                txtScan.val("");
            });

            btnCancelScan.on("keydown", function (e) {
                if (e.keyCode == 13) {
                    divNewCart.hide();
                    divCartList.show();
                    txtScan.val("");
                }
            });

            cmbCart.on("change", function () {
                cart = cmbCart.val();
                $("#VA011_CartLabel" + $self.windowNo).text(cmbCart.find('option:selected').text());
                //if (cmbCart.val() == 0) {
                //    $divHeadProd.find("#VA005_cartInfo_" + $self.windowNo).text(VIS.Msg.getMsg("None"));
                //}
                //else {
                //    $divHeadProd.find("#VA005_cartInfo_" + $self.windowNo).text(cmbCart.find('option:selected').text());
                //}
                BindCartGrid();
            });
        }

        var zoomToWindow = function (record_id, windowName) {

            var sql = "select ad_window_id from ad_window where name = '" + windowName + "'";// Upper( name)=Upper('user' )
            var ad_window_Id = 0;
            try {
                var dr = VIS.DB.executeDataReader(sql);
                if (dr.read()) {
                    ad_window_Id = dr.getInt(0);
                }
                dr.dispose();
                if (ad_window_Id > 0) {
                    var zoomQuery = new VIS.Query();
                    if (windowName == "Product")
                        zoomQuery.addRestriction("M_Product_ID", VIS.Query.prototype.EQUAL, record_id);
                    else if (windowName == "Attribute Set")
                        zoomQuery.addRestriction("M_AttributeSet_ID", VIS.Query.prototype.EQUAL, record_id);
                    else if (windowName == "Product Category")
                        zoomQuery.addRestriction("M_Product_Category_ID", VIS.Query.prototype.EQUAL, record_id);
                    else if (windowName == "Tax Category")
                        zoomQuery.addRestriction("C_TaxCategory_ID", VIS.Query.prototype.EQUAL, record_id);
                    else if (windowName == "Unit of Measure")
                        zoomQuery.addRestriction("C_Uom_ID", VIS.Query.prototype.EQUAL, record_id);
                    else if (windowName == "VAICNT_InventoryCount")
                        zoomQuery.addRestriction("VAICNT_InventoryCount_ID", VIS.Query.prototype.EQUAL, record_id);
                    zoomQuery.setRecordCount(1);
                    VIS.viewManager.startWindow(ad_window_Id, zoomQuery);
                    if (ch != null) {
                        ch.close();
                    }
                }
            }
            catch (e) {
                console.log(e);
            }
        };

        function saveInventoryCount() {

            $bsyDiv[0].style.visibility = "visible";
            $.ajax({
                type: "POST",
                url: VIS.Application.contextUrl + "Inventory/saveInventoryCount",
                dataType: "json",
                //async: false,
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ ColumnName: txtScan.val() }),
                success: function (data) {

                    if (data.result != "") {
                        divNewCart.hide();
                        divCartList.show();
                        txtScan.val("");
                        cart = data.result;
                        LoadCart();

                        
                      

                        BindCartGrid();
                    }
                    else {
                        //VIS.ADialog.error(data.result);
                        $bsyDiv[0].style.visibility = "hidden";
                        VIS.ADialog.error(RecordNotSaved);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    //
                    console.log(textStatus);
                    $bsyDiv[0].style.visibility = "hidden";
                    alert(errorThrown);
                    return;
                }
            });
        };

        function updateInventory() {

            var savedCart = [];
            for (item in cartGrid.records) {
                if (cartGrid.records[item].updated == true) {
                    savedCart.push(cartGrid.records[item]);
                }
            }
            if (savedCart.length > 0) {
                $bsyDiv[0].style.visibility = "visible";
                $.ajax({
                    type: "POST",
                    url: VIS.Application.contextUrl + "Inventory/updateInventory",
                    dataType: "json",
                    async: false,
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ count_id: cmbCart.val(), ColumnName: savedCart }),
                    success: function (data) {

                        $bsyDiv[0].style.visibility = "hidden";
                        if (data.result == "") {
                            BindCartGrid();
                            $(".VA005-cart-update").fadeIn();
                            $(".VA005-cart-update").fadeOut(2000);
                        }
                        else {
                            VIS.ADialog.error(data.result);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        //
                        console.log(textStatus);
                        $bsyDiv[0].style.visibility = "hidden";
                        alert("RecordNotSaved");
                        return;
                    }
                });
            }
        };

        function deleteInventory() {

            $bsyDiv[0].style.visibility = "visible";
            var deleteCart = [];
            var selection = cartGrid.getSelection();
            for (item in selection) {
                deleteCart.push(cartGrid.get(selection[item]));
            }
            $.ajax({
                type: "POST",
                url: VIS.Application.contextUrl + "Inventory/deleteInventory",
                dataType: "json",
                async: false,
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ ColumnName: deleteCart }),
                success: function (data) {

                    $bsyDiv[0].style.visibility = "hidden";
                    if (data.result == "") {
                        BindCartGrid();
                        cartGrid.selectNone();
                    }
                    else {
                        VIS.ADialog.error(data.result);
                        BindCartGrid();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    //
                    console.log(textStatus);
                    $bsyDiv[0].style.visibility = "hidden";
                    alert("DeleteError");
                    return;
                }
            });
        };

        function saveInventory() {

            if (cartSelectionValues.length > 0) {
                $bsyDiv[0].style.visibility = "visible";

                VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Inventory/SaveInventory", { "Count_ID": cmbCart.val(), "ColumnName": cartSelectionValues }, callbackSaveInventory);

                //$.ajax({
                //    type: "POST",
                //    url: VIS.Application.contextUrl + "Inventory/SaveInventory",
                //    dataType: "json",
                //    //async: false,
                //    contentType: "application/json; charset=utf-8",
                //    data: JSON.stringify({ Count_ID: cmbCart.val(), ColumnName: cartSelectionValues }),
                //    success: function (data) {
                //        
                //        if (data.result == "") {
                //            BindCartGrid();
                //        }
                //        else {
                //            VIS.ADialog.error(data.result);
                //        }
                //        cartSelectionValues = [];
                //        $bsyDiv[0].style.visibility = "hidden";
                //    },
                //    error: function (jqXHR, textStatus, errorThrown) {
                //        //
                //        console.log(textStatus);
                //        $bsyDiv[0].style.visibility = "hidden";
                //        alert("RecordNotSaved");
                //        cartSelectionValues = [];
                //        return;
                //    }
                //});
            }
            else {

            }
        };

        function callbackSaveInventory(data) {

            if (data.result == "") {
                BindCartGrid();
            }
            else {
                VIS.ADialog.error(data.result);
            }
            cartSelectionValues = [];
            $bsyDiv[0].style.visibility = "hidden";
        };

        function getDate(dateToConvert) {
            var dateConverted = dateToConvert;
            if (dateConverted != "") {
                dateConverted = new Date(dateConverted);
                dateConverted = dateConverted.toDateString();
            }
            return dateConverted;
        }

        function disableSectionsAfterReplenishPop() {

            _product_ID = 0;
            $divBottomMiddle.css("opacity", "1");
            detailsSection.css("opacity", "1");
            substituteSection.css("opacity", "1");
            relatedSection.css("opacity", "1");
            suppliersSection.css("opacity", "1");
            kitsSection.css("opacity", "1");

            rightPanelSection.css("opacity", "1");
            substituteGridSection.css("opacity", "1");
            relatedGridSection.css("opacity", "1");
            suppliersGridSection.css("opacity", "1");
            kitsGridSection.css("opacity", "1");
            _product_ID = 0;
            // if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_RightSection") >= 0) {

            $("#VA011_RightSection" + $self.windowNo).css("display", "block");
            $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "none");
            $("#VA011_grdRelated_" + $self.windowNo).css("display", "none");
            $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "none");
            $("#VA011_grdKits_" + $self.windowNo).css("display", "none");
            divCart.hide();
            $divCartdata.hide();

            $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
            $("#VA011_btnDetails_" + $self.windowNo).addClass("VA011-selectedTab");

            // }
            //else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnDetails") >= 0) {
            //    $("#VA011_RightSection" + $self.windowNo).css("display", "block");
            //    $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "none");
            //    $("#VA011_grdRelated_" + $self.windowNo).css("display", "none");
            //    $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "none");
            //    $("#VA011_grdKits_" + $self.windowNo).css("display", "none");
            //    divCart.hide();
            //    $divCartdata.hide();

            //    $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
            //    $("#VA011_btnDetails_" + $self.windowNo).addClass("VA011-selectedTab");
            //}
            //else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnSubsti") >= 0) {
            //    //$divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
            //    $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "block");
            //    bindSubstituteGrid();
            //}
            //else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnRelated") >= 0) {
            //    //$divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
            //    $("#VA011_grdRelated_" + $self.windowNo).css("display", "block");
            //    bindRelatedGrid();
            //}
            //else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnSuppliers") >= 0) {
            //    // $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
            //    $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "block");
            //    bindSuppliersRightGrid();
            //}
            //else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnKits") >= 0) {
            //    //$divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
            //    $("#VA011_grdKits_" + $self.windowNo).css("display", "block");
            //    bindKitsGrid();
            //}
            //else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnCart") >= 0) {
            //    //$divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
            //    $("#VA011_divCart_" + $self.windowNo).css("display", "block");
            //    //BindCartGrid();
            //}

            divZoomProdName.css("display", "none");

            $("#VA011_prodName_" + $self.windowNo).text("");
            $("#VA011_inputWeight_" + $self.windowNo).val("");
            $("#VA011_inputVolume_" + $self.windowNo).val("");
            $("#VA011_inputTare_" + $self.windowNo).val("");
            $("#VA011_inputLocator_" + $self.windowNo).val("");
            $("#VA011_inputExpDays_" + $self.windowNo).val("");
            $("#VA011_inputUOM_" + $self.windowNo).val("");
            $("#VA011_UPC" + $self.windowNo).find("span").remove();
            $("#VA011_UPC" + $self.windowNo).append('<span></span>');
            $("#VA011_AttributeSet" + $self.windowNo).find("span").remove();
            $("#VA011_AttributeSet" + $self.windowNo).append('<span></span>');
            var imageUrl = "";
            if (imageUrl != null) {
                if (imageUrl != "") {
                    imageUrl = imageUrl.substring(imageUrl.lastIndexOf("/") + 1, imageUrl.length);
                    var d = new Date();
                    imgUsrImage.removeAttr("src");
                    imgUsrImage.attr("src", VIS.Application.contextUrl + "Images/Thumb140x120/" + imageUrl + "?" + d.getTime());
                }
                else {
                    imgUsrImage.removeAttr("src").attr("src", VIS.Application.contextUrl + "Areas/VA011/Images/img-defult.png");
                }
            }
            else {
                imgUsrImage.removeAttr("src").attr("src", VIS.Application.contextUrl + "Areas/VA011/Images/img-defult.png");
            }

            $divHeadVarientProdBottom.find(".VA011_bottomGridHeaderSec").children().removeClass("VA011-selectedTab");
            $divLocatorGrid.css("display", "none");
            $divOrderedGrid.css("display", "none");
            $divTransactionsGrid.css("display", "none");
            $divDemandGrid.css("display", "none");
            $divReplenishedGrid.css("display", "none");
            $divVariantGrid.css("display", "block");
            $divReplenishmentBGrid.css("display", "none");
            $("#VA011_btnVariant_" + $self.windowNo).addClass("VA011-selectedTab");

            bindVariantGrid();

            //if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnVariant") >= 0) {
            //    bindVariantGrid();
            //}
            //else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnReplenished") >= 0) {
            //    bindReplenishedGrid();
            //}
            //else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnLocators") >= 0) {
            //    bindLocatorGrid();
            //}
            //else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnOrdered") >= 0) {
            //    bindOrderedGrid();
            //}
            //else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnDemand") >= 0) {
            //    bindDemandGrid();
            //}
            //else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnTransactions") >= 0) {
            //    bindTransactionsGrid();
            //}
            //else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnReplenishmentB") >= 0) {
            //    bindReplenishmentBGrid();
            //}
        };

        function disableSections() {
            if (multipleRecSelected) {
                $divBottomMiddle.css("opacity", "0.4");
                detailsSection.css("opacity", "0.4");
                substituteSection.css("opacity", "0.4");
                relatedSection.css("opacity", "0.4");
                suppliersSection.css("opacity", "0.4");
                kitsSection.css("opacity", "0.4");

                rightPanelSection.css("opacity", "0.4");
                substituteGridSection.css("opacity", "0.4");
                relatedGridSection.css("opacity", "0.4");
                suppliersGridSection.css("opacity", "0.4");
                kitsGridSection.css("opacity", "0.4");
                _product_ID = 0;
                if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_RightSection") >= 0) {
                    $("#VA011_RightSection" + $self.windowNo).css("display", "block");
                    $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "none");
                    $("#VA011_grdRelated_" + $self.windowNo).css("display", "none");
                    $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "none");
                    $("#VA011_grdKits_" + $self.windowNo).css("display", "none");
                    divCart.hide();
                    $divCartdata.hide();

                    $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                    $("#VA011_btnDetails_" + $self.windowNo).addClass("VA011-selectedTab");
                }
                else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnDetails") >= 0) {
                    $("#VA011_RightSection" + $self.windowNo).css("display", "block");
                    $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "none");
                    $("#VA011_grdRelated_" + $self.windowNo).css("display", "none");
                    $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "none");
                    $("#VA011_grdKits_" + $self.windowNo).css("display", "none");
                    divCart.hide();
                    $divCartdata.hide();

                    $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                    $("#VA011_btnDetails_" + $self.windowNo).addClass("VA011-selectedTab");
                }
                else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnSubsti") >= 0) {
                    //$divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                    $("#VA011_grdSubstitute_" + $self.windowNo).css("display", "block");
                    bindSubstituteGrid();
                }
                else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnRelated") >= 0) {
                    //$divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                    $("#VA011_grdRelated_" + $self.windowNo).css("display", "block");
                    bindRelatedGrid();
                }
                else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnSuppliers") >= 0) {
                    // $divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                    $("#VA011_grdSuppliersRight_" + $self.windowNo).css("display", "block");
                    bindSuppliersRightGrid();
                }
                else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnKits") >= 0) {
                    //$divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                    $("#VA011_grdKits_" + $self.windowNo).css("display", "block");
                    bindKitsGrid();
                }
                else if ($divHeadDetails.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnCart") >= 0) {
                    //$divHeadDetails.find(".VA011-tabs").children().removeClass("VA011-selectedTab");
                    $("#VA011_divCart_" + $self.windowNo).css("display", "block");
                    //BindCartGrid();
                }

                divZoomProdName.css("display", "none");

                $("#VA011_prodName_" + $self.windowNo).text("");
                $("#VA011_inputWeight_" + $self.windowNo).val("");
                $("#VA011_inputVolume_" + $self.windowNo).val("");
                $("#VA011_inputTare_" + $self.windowNo).val("");
                $("#VA011_inputLocator_" + $self.windowNo).val("");
                $("#VA011_inputExpDays_" + $self.windowNo).val("");
                $("#VA011_inputUOM_" + $self.windowNo).val("");
                $("#VA011_UPC" + $self.windowNo).find("span").remove();
                $("#VA011_UPC" + $self.windowNo).append('<span></span>');
                $("#VA011_AttributeSet" + $self.windowNo).find("span").remove();
                $("#VA011_AttributeSet" + $self.windowNo).append('<span></span>');
                var imageUrl = "";
                if (imageUrl != null) {
                    if (imageUrl != "") {
                        imageUrl = imageUrl.substring(imageUrl.lastIndexOf("/") + 1, imageUrl.length);
                        var d = new Date();
                        imgUsrImage.removeAttr("src");
                        imgUsrImage.attr("src", VIS.Application.contextUrl + "Images/Thumb140x120/" + imageUrl + "?" + d.getTime());
                    }
                    else {
                        imgUsrImage.removeAttr("src").attr("src", VIS.Application.contextUrl + "Areas/VA011/Images/img-defult.png");
                    }
                }
                else {
                    imgUsrImage.removeAttr("src").attr("src", VIS.Application.contextUrl + "Areas/VA011/Images/img-defult.png");
                }



                if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnVariant") >= 0) {
                    bindVariantGrid();
                }
                else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnReplenished") >= 0) {
                    bindReplenishedGrid();
                }
                else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnLocators") >= 0) {
                    bindLocatorGrid();
                }
                else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnOrdered") >= 0) {
                    bindOrderedGrid();
                }
                else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnDemand") >= 0) {
                    bindDemandGrid();
                }
                else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnTransactions") >= 0) {
                    bindTransactionsGrid();
                }
                else if ($divHeadVarientProdBottom.find(".VA011-selectedTab")[0].id.indexOf("VA011_btnReplenishmentB") >= 0) {
                    bindReplenishmentBGrid();
                }
            }
            else {
                $divBottomMiddle.css("opacity", "1");
                detailsSection.css("opacity", "1");
                substituteSection.css("opacity", "1");
                relatedSection.css("opacity", "1");
                suppliersSection.css("opacity", "1");
                kitsSection.css("opacity", "1");

                rightPanelSection.css("opacity", "1");
                substituteGridSection.css("opacity", "1");
                relatedGridSection.css("opacity", "1");
                suppliersGridSection.css("opacity", "1");
                kitsGridSection.css("opacity", "1");
            }
        };

        //Privilized function
        this.getRoot = function () {
            return $root;
        };

        this.disposeComponent = function () {

            self = null;
            if ($root)
                $root.remove();
            $root = null;

            this.getRoot = null;
            this.disposeComponent = null;
        };
    };

    //Must Implement with same parameter
    VA011.InventoryForm.prototype.init = function (windowNo, frame) {
        this.frame = frame;
        this.windowNo = windowNo;
        this.initalize();
        this.frame.getContentGrid().append(this.getRoot());
        this.initalLoad();
        w2ui['VA011_gridVariant_' + windowNo].refresh();
    };

    VA011.InventoryForm.prototype.sizeChanged = function (height, width) {

    };

    //Must implement dispose
    VA011.InventoryForm.prototype.dispose = function () {
        /*CleanUp Code */
        //dispose this component
        this.disposeComponent();

        //call frame dispose function
        if (this.frame)
            this.frame.dispose();
        this.frame = null;
    };

})(VA011, jQuery);