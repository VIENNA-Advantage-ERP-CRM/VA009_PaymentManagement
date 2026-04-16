using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using VAdvantage.DataBase;
using VAdvantage.Model;
using VAdvantage.Utility;

namespace VA009.Models
{
    public class VA009_ReceivableAssesmentModel
    {

        public dynamic GetRecordDetail(int Screen_ID, int tabID, int rec_ID, string tableName)
        {
            string sql = string.Empty;
            dynamic retObj = new ExpandoObject();

            if (tableName.Equals("C_Invoice"))
            {
                sql = $@"SELECT C_BPartner_ID FROM C_Invoice WHERE C_Invoice_ID = {rec_ID}";
                rec_ID = Util.GetValueOfInt(DB.ExecuteScalar(sql));

                sql = $@"SELECT t.AD_Window_ID, t.AD_Tab_ID FROM AD_Window w INNER JOIN AD_Tab t ON (w.AD_Window_ID = t.AD_Window_ID) 
                            WHERE w.Name = 'VAS_CustomerMaster' AND t.TabLevel = 0 and t.Name = 'Customer'";
                DataSet ds = DB.ExecuteDataset(sql);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    Screen_ID = Util.GetValueOfInt(ds.Tables[0].Rows[0]["AD_Window_ID"]);
                    tabID = Util.GetValueOfInt(ds.Tables[0].Rows[0]["AD_Tab_ID"]);
                }
            }

            sql = $@"SELECT assThread.VAI01_ThreadID FROM VAI01_AssistantScreen assScreen 
                    INNER JOIN VAI01_AssistantThread assThread ON (assScreen.VAI01_AssistantScreen_ID = assThread.VAI01_AssistantScreen_ID) 
                    WHERE assScreen.IsActive = 'Y' AND assThread.IsActive = 'Y' 
                          AND assScreen.AD_Window_ID = {Screen_ID}  AND assScreen.AD_Tab_ID = {tabID} AND assThread.VAI01_RecordID = {rec_ID}";
            retObj.ThreadID = DB.ExecuteScalar(sql);

            return retObj;
        }

        public string CustomerReturnAnalysis(int Record_ID, string Depot, string CustName)
        {
            var result = new ExpandoObject() as IDictionary<string, object>;
            decimal avReturndays = 0;
            decimal retunrDaysPercentage = 0;
            string dept = Depot != "" ? " AND VA009_Depot='" + Depot + "' " : " ";
            string cusName = CustName != "" ? " AND Name='" + CustName + "' " : " ";
            string finalJson = string.Empty;
            string sql = string.Empty;
            sql = $@"WITH return_diff AS (
                    SELECT 
                        VA009_OriginalInvoiceNo,
                        VA009_DOCUMENTDATE,
                        TO_DATE(REGEXP_SUBSTR(VA009_OriginalInvoiceNo, '\(([^)]*)\)', 1, 1, NULL, 1), 'DD-MON-YYYY') AS extracted_date,
                        VA009_DOCUMENTDATE - TO_DATE(REGEXP_SUBSTR(VA009_OriginalInvoiceNo, '\(([^)]*)\)', 1, 1, NULL, 1), 'DD-MON-YYYY') AS diff_days
                    FROM VA009_CreditNotes
                    WHERE VA009_DocumentType = 'CM' 
                    AND VA009_CreditNoteRegister_ID = {Record_ID}  
                      AND VA009_OriginalInvoiceNo IS NOT NULL
                    " + dept + cusName + $@"
                )
                SELECT
                    ROUND(AVG(diff_days), 2) AS avg_return_days,
                    COUNT(CASE WHEN diff_days < 7 THEN 1 END) AS quick_returns_count,
                    ROUND(100 * COUNT(CASE WHEN diff_days < 7 THEN 1 END) / COUNT(*), 2) AS quick_returns_percentage
                FROM return_diff";
            DataSet dsAverageReturns = DB.ExecuteDataset(sql);
            if (dsAverageReturns != null && dsAverageReturns.Tables.Count > 0 && dsAverageReturns.Tables[0].Rows.Count > 0)
            {
                avReturndays = Util.GetValueOfDecimal(dsAverageReturns.Tables[0].Rows[0]["avg_return_days"]);
                retunrDaysPercentage = Util.GetValueOfDecimal(dsAverageReturns.Tables[0].Rows[0]["quick_returns_percentage"]);
            }


            sql = $@"SELECT 'TOP_4_CUSTOMERS' AS REPORT_TYPE,
                           NAME,
                           Total_Return_Amount,
                           NULL AS Financial_Year,
                           NULL AS REGION,
                           NULL AS DEPOT,
                            NULL AS MONTH_NAME,
                            NULL AS Net_Return_Value,
                           NULL AS Net_Sale_Value
                    FROM (
                        SELECT NAME,
                               SUM(
                                   CASE WHEN VA009_DocumentType = 'CM' THEN -VA009_FinalAmount
                                        WHEN VA009_DocumentType = 'INV' THEN  0
                                    ELSE 0
                                   END
                               ) AS Total_Return_Amount
                        FROM VA009_CreditNotes  WHERE VA009_CreditNoteRegister_ID = {Record_ID} " + dept + cusName + $@"
                        AND VA009_DOCUMENTDATE >= ADD_MONTHS(TRUNC(CURRENT_DATE,'YYYY'),3)
                        AND VA009_DOCUMENTDATE < ADD_MONTHS(TRUNC(CURRENT_DATE,'YYYY'),15)
                        GROUP BY NAME
                        ORDER BY Total_Return_Amount DESC
                    ) WHERE ROWNUM <= 10
                    UNION ALL
                    SELECT 'TOP_4_Depots' AS REPORT_TYPE,
                           NULL AS NAME,
                           Total_Return_Value AS Total_Return_Amount,
                           NULL AS Financial_Year,
                           NULL AS REGION,
                           DEPOT AS DEPOT,
                           NULL AS MONTH_NAME,
                           NULL AS Net_Return_Value,
                           NULL AS Net_Sale_Value
                    FROM (
                        SELECT 
                            VA009_DEPOT AS DEPOT,
                            SUM(
                                CASE WHEN VA009_DocumentType = 'CM' THEN -VA009_FinalAmount
                                     WHEN VA009_DocumentType = 'INV' THEN  0
                                    ELSE 0
                                END
                            ) AS Total_Return_Value
                        FROM VA009_CreditNotes
                        WHERE VA009_CreditNoteRegister_ID = {Record_ID} " + dept + cusName + $@"
                        AND VA009_DOCUMENTDATE >= ADD_MONTHS(TRUNC(CURRENT_DATE,'YYYY'),3)
                        AND VA009_DOCUMENTDATE < ADD_MONTHS(TRUNC(CURRENT_DATE,'YYYY'),15)
                        GROUP BY VA009_DEPOT
                        ORDER BY Total_Return_Value DESC
                    )
                    WHERE ROWNUM <= 4
                    UNION ALL
                    SELECT 'RETURN_BY_FINANCIAL_YEAR' AS REPORT_TYPE,
                           NULL AS NAME,
                           NULL AS Total_Return_Amount,
                           NULL AS Financial_Year,
                           NULL AS REGION,
                           NULL AS DEPOT,
                           NULL AS MONTH_NAME,
                           SUM(CASE WHEN fin_year = 'CURRENT' THEN NVL(final_amt, 0) ELSE 0 END) AS Net_Return_Value,
                           SUM(CASE WHEN fin_year = 'PREVIOUS' THEN NVL(final_amt, 0) ELSE 0 END) AS Net_Sale_Value
                    FROM (
                        SELECT 
                            CASE
                                WHEN (VA009_DOCUMENTDATE >= ADD_MONTHS(TRUNC(CURRENT_DATE,'YYYY'),3)
                                   AND VA009_DOCUMENTDATE <  ADD_MONTHS(TRUNC(CURRENT_DATE,'YYYY'),15))
                                    THEN 'CURRENT'
                                WHEN (VA009_DOCUMENTDATE >= ADD_MONTHS(TRUNC(CURRENT_DATE,'YYYY')-1,-9)+1
                                   AND VA009_DOCUMENTDATE <=  ADD_MONTHS(TRUNC(CURRENT_DATE,'YYYY')-1,3))
                                    THEN 'PREVIOUS'
                            END AS fin_year,
                            CASE WHEN VA009_DocumentType = 'CM' THEN -VA009_FinalAmount
                                 WHEN VA009_DocumentType = 'INV' THEN  0
                                 ELSE 0
                            END AS final_amt
                        FROM VA009_CreditNotes
                        WHERE VA009_CreditNoteRegister_ID = {Record_ID} " + dept + cusName + $@"
                    )
                    UNION ALL
                    SELECT 'RETURN_BY_REGION_DEPOT' AS REPORT_TYPE,
                           NULL AS NAME,
                           Total_Return_Value,
                           NULL AS Financial_Year,
                           VA009_REGION AS REGION,
                           VA009_DEPOT AS DEPOT,
                            NULL AS MONTH_NAME,
                           NULL AS Net_Return_Value,
                           NULL AS Net_Sale_Value
                    FROM (
                        SELECT 
                            VA009_REGION,
                            VA009_DEPOT,
                            SUM(
                                CASE WHEN VA009_DocumentType = 'CM' THEN -VA009_FinalAmount
                                     WHEN VA009_DocumentType = 'INV' THEN  0
                                    ELSE 0
                                END
                            ) AS Total_Return_Value
                        FROM VA009_CreditNotes  WHERE VA009_CreditNoteRegister_ID = {Record_ID} " + dept + cusName + $@"
                        AND VA009_DOCUMENTDATE >= ADD_MONTHS(TRUNC(CURRENT_DATE,'YYYY'),3)
                        AND VA009_DOCUMENTDATE < ADD_MONTHS(TRUNC(CURRENT_DATE,'YYYY'),15)
                        GROUP BY VA009_REGION, VA009_DEPOT
                    )
                    UNION ALL
                    SELECT 'TOTAL_SALE_RETURN' AS REPORT_TYPE,
                           NULL AS NAME,
                           NULL AS Total_Return_Amount,
                           NULL AS Financial_Year,
                           NULL AS REGION,
                           NULL AS DEPOT,
                            TO_CHAR(VA009_DOCUMENTDATE,'Mon-YYYY') AS MONTH_NAME,
                           SUM(
                                CASE WHEN VA009_DocumentType = 'CM' THEN -VA009_FinalAmount
                                     WHEN VA009_DocumentType = 'INV' THEN  0
                                    ELSE 0
                                END
                           ) AS Net_Return_Value,
                            SUM(
                                CASE WHEN VA009_DocumentType = 'CM' THEN 0
                                     WHEN VA009_DocumentType = 'INV' THEN  VA009_FinalAmount
                                    ELSE 0
                                END
                           ) AS Net_Sale_Value
                    FROM VA009_CreditNotes WHERE VA009_CreditNoteRegister_ID = {Record_ID} " + dept + cusName + $@"
                    AND VA009_DOCUMENTDATE >= ADD_MONTHS(TRUNC(CURRENT_DATE,'YYYY'),3)
                    AND VA009_DOCUMENTDATE < ADD_MONTHS(TRUNC(CURRENT_DATE,'YYYY'),15)
                    GROUP BY TO_CHAR(VA009_DOCUMENTDATE,'Mon-YYYY')
                    /* ORDER BY REPORT_TYPE, MONTH_NAME */ ";

            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var grouped = ds.Tables[0].AsEnumerable().GroupBy(x => x["REPORT_TYPE"].ToString());

                foreach (var group in grouped)
                {
                    switch (group.Key)
                    {
                        case "RETURN_BY_REGION_DEPOT":
                            result["region_analysis"] = group.Select(r => new
                            {
                                region = $"{r["DEPOT"]}",/*$"{r["REGION"]} -> {r["DEPOT"]}",*/
                                value = Convert.ToDecimal(r["Total_Return_Amount"])
                            }).ToList();
                            break;

                        case "TOTAL_SALE_RETURN":
                            var totalSale = group.Sum(r => Convert.ToDecimal(r["Net_Sale_Value"]));
                            var totalReturn = group.Sum(r => Convert.ToDecimal(r["Net_Return_Value"]));
                            var returnPercentage = totalSale > 0 ? Math.Round((totalReturn / totalSale) * 100, 2) : 0;
                            result["summary"] = new
                            {
                                overall_return_percentage = returnPercentage,
                                average_return_timing_days = avReturndays,
                                quick_returns_within_2_days = retunrDaysPercentage
                            };

                            result["timing_analysis"] = new
                            {
                                return_trend_monthly = group.Select(x => new
                                {
                                    month = x["MONTH_NAME"]?.ToString(),
                                    sales = Convert.ToDecimal(x["Net_Sale_Value"]),
                                    returns = Convert.ToDecimal(x["Net_Return_Value"])
                                }).ToList()
                            };
                            break;

                        case "RETURN_BY_FINANCIAL_YEAR":
                            result["value_comparison"] = new
                            {
                                current_period_value = Convert.ToDecimal(group.FirstOrDefault()?["Net_Return_Value"] ?? 0),
                                previous_period_value = Convert.ToDecimal(group.FirstOrDefault()?["Net_Sale_Value"] ?? 0)
                            };
                            break;

                        case "TOP_4_CUSTOMERS":
                        case "TOP_4_Depots":
                            if (!result.ContainsKey("return_breakdown"))
                                result["return_breakdown"] = new ExpandoObject();
                            var breakdown = result["return_breakdown"] as IDictionary<string, object>;

                            if (group.Key == "TOP_4_CUSTOMERS")
                            {
                                var total = group.Sum(r => Convert.ToDecimal(r["Total_Return_Amount"]));

                                breakdown["by_customer"] = group.Select(r => new
                                {
                                    customer = r["NAME"]?.ToString(),
                                    value = Convert.ToDecimal(r["Total_Return_Amount"]),
                                    percentage = total > 0 ? Math.Round(Convert.ToDecimal(r["Total_Return_Amount"]) / total * 100, 2) : 0
                                }).ToList();
                            };

                            if (group.Key == "TOP_4_Depots")
                            {
                                breakdown["by_depot"] = group.Select(r => new
                                {
                                    depot = r["DEPOT"]?.ToString(),
                                    value = Convert.ToDecimal(r["Total_Return_Amount"])
                                }).ToList();
                            }

                            break;
                    }
                }

                finalJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            else
            {

            }
            return finalJson;
        }

        public string DispatchDataAnalysis(int Record_ID)
        {
            var result = new ExpandoObject() as IDictionary<string, object>;

            string finalJson = string.Empty;
            string sql = string.Empty;
            sql = $@"WITH return_diff AS (
                    SELECT 
                        dr.va009_invoiceno,
                        dr.va009_invoicedate,
                        dr.va009_receiptdate,
                        dr.va009_registration,
                        dr.va009_name,
                        dr.va009_depot,
                        dr.va009_currentdepot,
                        dr.va009_ratelocation,
                        dr.va009_litres,
                        dr.va009_kilograms,
                        dr.VA009_Weight AS va009_total,
                        dr.VA009_GatePassType,
                        (trunc(VA009_GPDate) - trunc(dr.va009_invoicedate)) AS days_pending
                    FROM VA009_CreditNoteRegister cnr
                    INNER JOIN VA009_DispatchRegister dr 
                        ON dr.VA009_CreditNoteRegister_ID = cnr.VA009_CreditNoteRegister_ID
                    WHERE cnr.VA009_CreditNoteRegister_ID = {Record_ID} 
                        /* AND TRUNC(cnr.VA009_FromDate) <= TRUNC(dr.va009_invoicedate) */
                        AND NOT EXISTS (
                        SELECT 1
                        FROM VA009_CreditNotes cn
                        WHERE cn.VA009_CreditNoteRegister_ID = cnr.VA009_CreditNoteRegister_ID
                          AND cn.VA009_DocumentNo = dr.VA009_InvoiceNo
                          AND cn.VA009_DocumentType = 'INV' 
                    )
                ),
                Inv_NotDispatch AS ( SELECT COUNT(dr.VA009_CreditNotes_ID) AS TotalRecord
                 FROM VA009_CreditNoteRegister cnr
                    INNER JOIN VA009_CreditNotes dr 
                        ON dr.VA009_CreditNoteRegister_ID = cnr.VA009_CreditNoteRegister_ID
                    WHERE cnr.VA009_CreditNoteRegister_ID = {Record_ID}  
                        AND dr.VA009_DocumentType = 'INV' 
                        AND NOT EXISTS (
                        SELECT 1
                        FROM VA009_DispatchRegister cn
                        WHERE cn.VA009_CreditNoteRegister_ID = cnr.VA009_CreditNoteRegister_ID
                          AND dr.VA009_DocumentNo = cn.VA009_InvoiceNo
                    )
                ),
                Qty_MisMatch AS (
                 SELECT DISTINCT dr.VA009_DocumentNo,dr.Name, 
                        dr.VA009_Litres + dr.VA009_Kilograms as InvoicedQty, 
                        cn.va009_litres + cn.va009_kilograms as DispatchedQty,
                        ABS((dr.VA009_Litres + dr.VA009_Kilograms) - (cn.VA009_Litres + cn.VA009_Kilograms)) AS QtyDiff
                    FROM VA009_CreditNoteRegister cnr
                    INNER JOIN VA009_CreditNotes dr ON dr.VA009_CreditNoteRegister_ID = cnr.VA009_CreditNoteRegister_ID 
                    INNER JOIN VA009_DispatchRegister cn ON cn.VA009_CreditNoteRegister_ID = cnr.VA009_CreditNoteRegister_ID AND dr.VA009_DocumentNo = cn.VA009_InvoiceNo
                    WHERE dr.va009_litres + dr.va009_kilograms <> cn.va009_litres + cn.va009_kilograms
                    AND  dr.VA009_DocumentType = 'INV' AND cnr.VA009_CreditNoteRegister_ID = {Record_ID} 
                ),
                aging AS (
                    SELECT
                        CASE
                            WHEN days_pending BETWEEN 0 AND 7 THEN '0-7 Days'
                            WHEN days_pending BETWEEN 8 AND 15 THEN '8-15 Days'
                            WHEN days_pending BETWEEN 16 AND 30 THEN '16-30 Days'
                            WHEN days_pending BETWEEN 31 AND 60 THEN '31-60 Days'
                            ELSE '60+ Days'
                        END AS bucket,
                        SUM(va009_total) AS total_value,
                        COUNT(*) AS record_count
                    FROM return_diff
                    GROUP BY
                        CASE
                            WHEN days_pending BETWEEN 0 AND 7 THEN '0-7 Days'
                            WHEN days_pending BETWEEN 8 AND 15 THEN '8-15 Days'
                            WHEN days_pending BETWEEN 16 AND 30 THEN '16-30 Days'
                            WHEN days_pending BETWEEN 31 AND 60 THEN '31-60 Days'
                            ELSE '60+ Days'
                        END
                    ORDER BY
                        CASE
                            WHEN days_pending BETWEEN 0 AND 7 THEN '0-7 Days'
                            WHEN days_pending BETWEEN 8 AND 15 THEN '8-15 Days'
                            WHEN days_pending BETWEEN 16 AND 30 THEN '16-30 Days'
                            WHEN days_pending BETWEEN 31 AND 60 THEN '31-60 Days'
                            ELSE '60+ Days'
                        END
                ),
                depot AS (
                    SELECT 
                        va009_depot,
                        SUM(va009_total) AS pending_value,
                        COUNT(*) AS pending_count
                    FROM return_diff
                    GROUP BY va009_depot
                    ORDER BY pending_value DESC
                    FETCH FIRST 6 ROWS ONLY 
                )
                        SELECT 
                            'SUMMARY' AS SECTION,
                            '' AS Value,
                            '' AS Value2,
                            ROUND((SELECT AVG(days_pending) FROM return_diff), 2) AS COL_1,
                            (SELECT SUM(va009_total) FROM return_diff) AS COL_2,
                            (SELECT COUNT(*) FROM return_diff WHERE VA009_GatePassType = 'IN') AS COL_3,
                            (SELECT MAX(days_pending) FROM return_diff) AS COL_4
                        FROM dual
                        UNION ALL 
                        SELECT 
                            'InvoiceNotDispatch' AS SECTION,
                            '' AS Value,
                            '' AS Value2,
                            0 AS COL_1,
                            0 AS COL_2,
                            TotalRecord AS COL_3,
                            0 AS COL_4
                        FROM Inv_NotDispatch
                        UNION ALL
                        SELECT 
                            'AGING' AS SECTION,
                            bucket AS Value,
                            '' AS Value2,
                            total_value AS COL_1,
                            record_count AS COL_2,
                            NULL AS COL_3,
                            NULL AS COL_4
                        FROM aging
                        UNION ALL
                        SELECT 
                            'Depot' AS SECTION,
                            to_char(va009_depot) AS Value,
                            '' AS Value2,
                            pending_value AS COL_1,
                            pending_count AS COL_2,
                            NULL AS COL_3,
                            NULL AS COL_4
                        FROM depot
                        UNION ALL 
                        (SELECT
                           'InvQty_MisMatch' AS Section, 
                           to_char(Name) AS Value, 
                           to_char(VA009_DocumentNo) AS Value2,
                           InvoicedQty AS Col_1,
                           DispatchedQty AS Col_2,
                           InvoicedQty - DispatchedQty AS COL_3,
                            NULL AS COL_4
                       FROM Qty_MisMatch 
                       WHERE InvoicedQty > DispatchedQty
                       ORDER BY InvoicedQty - DispatchedQty DESC
                       FETCH FIRST 5 ROWS ONLY)
                       UNION ALL 
                        (SELECT
                           'DisQty_MisMatch' AS Section, 
                           to_char(Name) AS Value, 
                           to_char(VA009_DocumentNo) AS Value2,
                           InvoicedQty AS Col_1,
                           DispatchedQty AS Col_2,
                           InvoicedQty - DispatchedQty AS COL_3,
                            NULL AS COL_4
                       FROM Qty_MisMatch 
                       WHERE InvoicedQty < DispatchedQty
                       ORDER BY DispatchedQty - InvoicedQty DESC
                       FETCH FIRST 5 ROWS ONLY)";
            DataSet ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var grouped = ds.Tables[0].AsEnumerable().GroupBy(x => x["SECTION"].ToString());

                foreach (var group in grouped)
                {
                    switch (group.Key)
                    {
                        case "AGING":
                            result["AGING"] = group.Select(r => new
                            {
                                aging_bucket = $"{r["Value"]}",
                                total_value = Convert.ToDecimal(r["COL_1"]),
                                rec_count = Convert.ToDecimal(r["COL_2"])
                            }).ToList();
                            break;

                        case "SUMMARY":
                            result["summary"] = new
                            {
                                average_dispatch_timing_days = Convert.ToDecimal(group.FirstOrDefault()?["COL_1"] ?? 0),
                                UnbilledAmt = Convert.ToDecimal(group.FirstOrDefault()?["COL_2"] ?? 0),
                                TotalRecord = Convert.ToDecimal(group.FirstOrDefault()?["COL_3"] ?? 0),
                                max_notinvoiced_days = Convert.ToDecimal(group.FirstOrDefault()?["COL_4"] ?? 0)
                            };
                            break;

                        case "InvoiceNotDispatch":
                            result["InvoiceNotDispatch"] = new
                            {
                                TotalRecord = Convert.ToDecimal(group.FirstOrDefault()?["COL_3"] ?? 0),
                            };
                            break;

                        case "Depot":
                            result["Depot"] = group.Select(r => new
                            {
                                Name = $"{r["Value"]}",
                                value = Convert.ToDecimal(r["COL_1"]),
                                rec_count = Convert.ToDecimal(r["COL_2"])
                            }).ToList();
                            break;

                        case "InvQty_MisMatch":
                            result["InvQty_MisMatch"] = group.Select(r => new
                            {
                                custName = $"{r["Value"]}",
                                invNo = $"{r["Value2"]}",
                                InvoicedQty = Convert.ToDecimal(r["COL_1"]),
                                DispatchedQty = Convert.ToDecimal(r["COL_2"]),
                                DifferenceQty = Convert.ToDecimal(r["COL_3"])
                            }).ToList();
                            break;

                        case "DisQty_MisMatch":
                            result["DisQty_MisMatch"] = group.Select(r => new
                            {
                                custName = $"{r["Value"]}",
                                invNo = $"{r["Value2"]}",
                                InvoicedQty = Convert.ToDecimal(r["COL_1"]),
                                DispatchedQty = Convert.ToDecimal(r["COL_2"]),
                                DifferenceQty = Convert.ToDecimal(r["COL_3"])
                            }).ToList();
                            break;
                    }
                }

                finalJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            else
            {

            }
            return finalJson;
        }
        public List<dynamic> GetCreditNotedata(Ctx ctx, int rec_ID)
        {
            List<dynamic> retData = new List<dynamic>();

            // 1️⃣ VA009_Depot fetch karna
            string sqlDepot = "SELECT DISTINCT VA009_Depot FROM VA009_CreditNotes WHERE VA009_CreditNoteRegister_ID=" + rec_ID;
            DataSet dsDepot = DB.ExecuteDataset(sqlDepot);
            if (dsDepot != null && dsDepot.Tables.Count > 0 && dsDepot.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dsDepot.Tables[0].Rows)
                {
                    dynamic obj = new ExpandoObject();
                    obj.Type = "Depot";
                    obj.Value = Util.GetValueOfString(row["VA009_Depot"]);
                    retData.Add(obj);
                }
            }

            // 2️⃣ Name fetch karna
            string sqlName = "SELECT DISTINCT Name FROM VA009_CreditNotes WHERE VA009_CreditNoteRegister_ID=" + rec_ID;
            DataSet dsName = DB.ExecuteDataset(sqlName);
            if (dsName != null && dsName.Tables.Count > 0 && dsName.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dsName.Tables[0].Rows)
                {
                    dynamic obj = new ExpandoObject();
                    obj.Type = "Name";
                    obj.Value = Util.GetValueOfString(row["Name"]);
                    retData.Add(obj);
                }
            }

            return retData;
        }

        public List<dynamic> GetFinInsightsData(Ctx ctx, string IsReturnTrx, string IsSOTrx, int AD_Table_ID, string TabName, string WinDisplayName, int AD_Window_ID)
        {
            List<dynamic> retData = new List<dynamic>();
            String sql = @"SELECT VA113_DataObject,Name,VA113_REF_TABLE_VIEW, DisplayName, VA113_Result,AD_Org_ID FROM VA113_INSIGHTS";
            if (MOrder.Table_ID.Equals(AD_Table_ID) && (!string.IsNullOrEmpty(IsReturnTrx) && IsReturnTrx.Equals("N")) && (!string.IsNullOrEmpty(IsSOTrx) && IsSOTrx.Equals("Y")))
            {
                sql += " WHERE VA113_DataObject = 'Sales Order'";
            }
            else if (MOrder.Table_ID.Equals(AD_Table_ID) && (!string.IsNullOrEmpty(IsReturnTrx) && IsReturnTrx.Equals("N")) && (!string.IsNullOrEmpty(IsSOTrx) && IsSOTrx.Equals("N")))
            {
                sql += " WHERE VA113_DataObject = 'Purchase Order'";
            }
            else if (MOrder.Table_ID.Equals(AD_Table_ID) && (!string.IsNullOrEmpty(IsReturnTrx) && IsReturnTrx.Equals("Y")) && (!string.IsNullOrEmpty(IsSOTrx) && IsSOTrx.Equals("Y")))
            {
                sql += " WHERE VA113_DataObject = 'Customer RMA'";
            }
            else if (MOrder.Table_ID.Equals(AD_Table_ID) && (!string.IsNullOrEmpty(IsReturnTrx) && IsReturnTrx.Equals("Y")) && (!string.IsNullOrEmpty(IsSOTrx) && IsSOTrx.Equals("N")))
            {
                sql += " WHERE VA113_DataObject = 'Vendor RMA'";
            }
            else if (MInOut.Table_ID.Equals(AD_Table_ID) && (!string.IsNullOrEmpty(IsReturnTrx) && IsReturnTrx.Equals("N")) && (!string.IsNullOrEmpty(IsSOTrx) && IsSOTrx.Equals("Y")))
            {
                sql += " WHERE VA113_DataObject = 'Delivery Order'";
            }
            else if (MInOut.Table_ID.Equals(AD_Table_ID) && (!string.IsNullOrEmpty(IsReturnTrx) && IsReturnTrx.Equals("N")) && (!string.IsNullOrEmpty(IsSOTrx) && IsSOTrx.Equals("N")))
            {
                sql += " WHERE VA113_DataObject = 'GRN'";
            }
            else if (MInOut.Table_ID.Equals(AD_Table_ID) && (!string.IsNullOrEmpty(IsReturnTrx) && IsReturnTrx.Equals("Y")) && (!string.IsNullOrEmpty(IsSOTrx) && IsSOTrx.Equals("Y")))
            {
                sql += " WHERE VA113_DataObject = 'Customer Return'";
            }
            else if (MInOut.Table_ID.Equals(AD_Table_ID) && (!string.IsNullOrEmpty(IsReturnTrx) && IsReturnTrx.Equals("Y")) && (!string.IsNullOrEmpty(IsSOTrx) && IsSOTrx.Equals("N")))
            {
                sql += " WHERE VA113_DataObject = 'Vendor Return'";
            }
            else
            {
                sql += " WHERE VA113_DataObject = " + GlobalVariable.TO_STRING(WinDisplayName);
            }
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "VA113_INSIGHTS", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql, null, null);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    dynamic obj = new ExpandoObject();
                    obj.DataObject = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA113_DataObject"]);
                    obj.Name = Util.GetValueOfString(ds.Tables[0].Rows[i]["Name"]);
                    obj.TabelView = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA113_REF_TABLE_VIEW"]);
                    obj.DisplayName = Util.GetValueOfString(ds.Tables[0].Rows[i]["DisplayName"]);
                    obj.Result = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA113_Result"]);
                    obj.AD_Org_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["AD_Org_ID"]);
                    retData.Add(obj);
                }
            }
            return retData;
        }
        /// <summary>
        /// This Function is use to get the data of invoice schedule and payment/cash associated with it
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="InvoiceId">InvoiceId</param>
        /// <param name="pageNo">pageNo</param>
        /// <param name="pageSize">pageSize</param>
        /// <returns>returns the data</returns>
        /// <author>VIS_427</author>
        public List<dynamic> GetCustScheduleData(Ctx ctx, int CustId, int pageNo, int pageSize, string isPaid)
        {
            List<dynamic> InvocieTaxTabPanel = new List<dynamic>();
            String sql = @"SELECT
                               cs.DueDate,
                               cs.DueAmt,
                               cs.VA009_IsPaid,
                               cy.StdPrecision,
                               ci.DocumentNo,
                               cy.ISO_CODE,
                               ci.dateinvoiced
                           FROM 
                               C_InvoicePaySchedule cs 
                           INNER JOIN 
                               C_Invoice ci ON (ci.C_Invoice_ID = cs.C_Invoice_ID)
                           INNER JOIN 
                               C_Currency cy ON (cy.C_Currency_ID = ci.C_Currency_ID)
                           INNER JOIN C_BPartner cb ON (cb.C_BPartner_ID=ci.C_BPartner_ID)
                           WHERE 
                               cb.C_BPartner_ID  = " + CustId;
            if (!string.IsNullOrEmpty(isPaid))
            {
                sql += " AND cs.VA009_IsPaid = '" + isPaid + "'";
            }
            sql += " ORDER BY cs.DueDate ";

            DataSet ds = DB.ExecuteDataset(sql.ToString(), null, null, pageSize, pageNo);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                int RecordCount = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) FROM (" + sql + ")t", null, null));
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DateTime dueDate = Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["DueDate"]).Value;
                    DateTime today = DateTime.Now.Date;

                    dynamic obj = new ExpandoObject();

                    obj.RecordCount = RecordCount;
                    obj.LineNum = i + 1;
                    obj.DueAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["DueAmt"]);
                    obj.IsPaid = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA009_IsPaid"]);
                    obj.DocumentNo = Util.GetValueOfString(ds.Tables[0].Rows[i]["DocumentNo"]);
                    obj.DueDate = dueDate;
                    obj.DateInvoiced = Util.GetValueOfDateTime(ds.Tables[0].Rows[i]["dateinvoiced"]).Value; ;
                    obj.stdPrecision = Util.GetValueOfInt(ds.Tables[0].Rows[i]["StdPrecision"]);
                    obj.CurName = Util.GetValueOfString(ds.Tables[0].Rows[i]["ISO_CODE"]);

                    if (Util.GetValueOfString(ds.Tables[0].Rows[i]["VA009_IsPaid"]).Equals("N"))
                    {
                        if (dueDate < today)
                        {
                            obj.Status = "Over Due";
                        }
                        else
                        {
                            obj.Status = "Pending";
                        }
                    }
                    else
                    {
                        obj.Status = "Paid";
                    }

                    InvocieTaxTabPanel.Add(obj);
                }
            }
            return InvocieTaxTabPanel;
        }

        public List<CancelInvoiceDto> GetCancelInvoiceData(Ctx ctx, int creditNoteRegisterId)
        {
            var list = new List<CancelInvoiceDto>();

            string sql = @"
    SELECT 
        tl.VA009_GatePassNo,
        cn.VA009_OriginalInvoiceNo,
        tl.VA009_GPDate,
        tl.VA009_TransportationName,
        cn.VA009_DocumentNo,
        cn.VA009_DocName,
        cn.VA009_Depot,
        cn.VA009_DocumentDate,
        dr.VA009_InvoiceDate
    FROM VA009_TransportLiability tl
    INNER JOIN VA009_DispatchRegister dr 
        ON (dr.VA009_GatePassNo = tl.VA009_GatePassNo AND dr.VA009_GPDate = tl.VA009_GPDate)
    INNER JOIN VA009_CreditNotes cn
        ON (dr.VA009_InvoiceNo =
           SUBSTR(cn.VA009_OriginalInvoiceNo,
                  1,
                  INSTR(cn.VA009_OriginalInvoiceNo, ' ') - 1)
       AND TRUNC(dr.VA009_InvoiceDate) =
           TO_DATE(
               TRIM(
                   SUBSTR(
                       cn.VA009_OriginalInvoiceNo,
                       INSTR(cn.VA009_OriginalInvoiceNo, '(') + 1,
                       INSTR(cn.VA009_OriginalInvoiceNo, ')') 
                       - INSTR(cn.VA009_OriginalInvoiceNo, '(') - 1
                   )
               ),
               'DD-MON-YYYY'
           ))
    WHERE UPPER(cn.VA009_DocName) LIKE '%CANCEL%' 
      AND cn.VA009_CreditNoteRegister_ID = " + creditNoteRegisterId;

            DataSet ds = DB.ExecuteDataset(sql);

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    DateTime? gpDate = Util.GetValueOfDateTime(r["VA009_GPDate"]);
                    DateTime? docDate = Util.GetValueOfDateTime(r["VA009_DocumentDate"]);

                    list.Add(new CancelInvoiceDto
                    {
                        GatePassNo = Util.GetValueOfString(r["VA009_GatePassNo"]) ?? "",

                        OriginalInvoiceNo = Util.GetValueOfString(r["VA009_OriginalInvoiceNo"]) ?? "",

                        GPDate = gpDate.HasValue
                                    ? gpDate.Value.ToString("dd-MMM-yyyy")
                                    : "-",   // 👈 default

                        TransportationName = Util.GetValueOfString(r["VA009_TransportationName"]) ?? "",

                        DocumentNo = Util.GetValueOfString(r["VA009_DocumentNo"]) ?? "",

                        DocName = Util.GetValueOfString(r["VA009_DocName"]) ?? "",

                        Depot = Util.GetValueOfString(r["VA009_Depot"]) ?? "",

                        DocumentDate = docDate.HasValue
                                        ? docDate.Value.ToString("dd-MMM-yyyy")
                                        : "-",  // 👈 default,
                        OrginalInvDate = Util.GetValueOfDateTime(r["VA009_InvoiceDate"]).Value.ToString("dd-MMM-yyyy")
                    });
                }
            }

            return list;
        }
    }
    public class CancelInvoiceDto
    {
        public string GatePassNo { get; set; }
        public string OriginalInvoiceNo { get; set; }
        public string GPDate { get; set; }
        public string TransportationName { get; set; }
        public string DocumentNo { get; set; }
        public string DocName { get; set; }
        public string OrginalInvDate { get; set; }
        public string Depot { get; set; }
        public string DocumentDate { get; set; }
    }
}