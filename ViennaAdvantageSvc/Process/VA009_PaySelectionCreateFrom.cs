using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;

namespace ViennaAdvantage.Process
{
    public class VA009_PaySelectionCreateFrom : SvrProcess
    {
        //Only When Discount		
        private bool _OnlyDiscount = false;
        //Only when Due				
        private bool _OnlyDue = false;
        //Include Disputed		
        private bool _IncludeInDispute = false;
        //Match Requirement		
        private String _MatchRequirementI = X_C_Invoice.MATCHREQUIREMENTI_None;
        //Payment Rule			
        private String _PaymentRule = null;
        //Payment Method
        private int VA009_PaymentMethod_ID = 0;
        //BPartner				
        private int _C_BPartner_ID = 0;
        //BPartner Group			
        private int _C_BP_Group_ID = 0;
        //Payment Selection		
        private int _C_PaySelection_ID = 0;
        //Document No From		   
        private String _DocumentNo_From = null;
        //Document No To			
        private String _DocumentNo_To = null;
        //Org IDS
        private string _AD_Org_ID = null;
        //Sales Order Tranactions
        private bool _isSOTrx = false;
        private bool VA009_Trx = false;
        MPaySelection psel = null;
        StringBuilder parentIDs = new StringBuilder();

        /// <summary>
        /// Prepare - e.g., get Parameters.
        /// </summary>
        protected override void Prepare()
        {
            ProcessInfoParameter[] para = GetParameter();
            for (int i = 0; i < para.Length; i++)
            {
                String name = para[i].GetParameterName();
                if (para[i].GetParameter() == null)
                {
                    ;
                }
                else if (name.Equals("OnlyDiscount"))
                {
                    _OnlyDiscount = "Y".Equals(para[i].GetParameter());
                }
                else if (name.Equals("OnlyDue"))
                {
                    _OnlyDue = "Y".Equals(para[i].GetParameter());
                }
                else if (name.Equals("IncludeInDispute"))
                {
                    _IncludeInDispute = "Y".Equals(para[i].GetParameter());
                }
                else if (name.Equals("VA009_PaymentMethod_ID"))
                {
                    VA009_PaymentMethod_ID = para[i].GetParameterAsInt();
                    _PaymentRule = (String)(DB.ExecuteScalar("SELECT VA009_PaymentBaseType FROM VA009_PaymentMethod WHERE IsActive='Y' AND VA009_PaymentMethod_ID=" + VA009_PaymentMethod_ID));
                }
                else if (name.Equals("DocumentNo"))
                {
                    _DocumentNo_From = (String)para[i].GetParameter();
                    _DocumentNo_To = (String)para[i].GetParameter_To();
                }
                else if (name.Equals("C_BPartner_ID"))
                {
                    _C_BPartner_ID = para[i].GetParameterAsInt();
                }
                else if (name.Equals("C_BP_Group_ID"))
                {
                    _C_BP_Group_ID = para[i].GetParameterAsInt();
                }
                else if (name.Equals("AD_Org_ID"))
                {
                    _AD_Org_ID = para[i].GetParameter().ToString();
                }
                //else if (name.Equals("IsSOTrx")) {

                //    _isSOTrx = "Y".Equals(para[i].GetParameter());
                //}
                else if (name.Equals("VA009_Trx"))
                {
                    VA009_Trx = true;
                    if ("S".Equals(para[i].GetParameter()))
                        _isSOTrx = true;
                    else
                        _isSOTrx = false;
                }
                else
                {
                    log.Log(Level.SEVERE, "Unknown Parameter: " + name);
                }
            }
            _C_PaySelection_ID = GetRecord_ID();
            if (_DocumentNo_From != null && _DocumentNo_From.Length == 0)
            {
                _DocumentNo_From = null;
            }
            if (_DocumentNo_To != null && _DocumentNo_To.Length == 0)
            {
                _DocumentNo_To = null;
            }
        }

        /// <summary>
        /// Perform Process.
        /// </summary>
        /// <returns>Message </returns>
        protected override String DoIt()
        {
            //int count = 0;
            psel = new MPaySelection(GetCtx(), _C_PaySelection_ID, Get_TrxName());

            #region Old Query
            //log.Info("C_PaySelection_ID=" + _C_PaySelection_ID
            //    + ", OnlyDiscount=" + _OnlyDiscount + ", OnlyDue=" + _OnlyDue
            //    + ", IncludeInDispute=" + _IncludeInDispute
            //    + ", MatchRequirement=" + _MatchRequirementI
            //    + ", PaymentRule=" + _PaymentRule
            //    + ", C_BP_Group_ID=" + _C_BP_Group_ID + ", C_BPartner_ID=" + _C_BPartner_ID);

            //MPaySelection psel = new MPaySelection(GetCtx(), _C_PaySelection_ID, Get_TrxName());
            //int C_CurrencyTo_ID = psel.GetC_Currency_ID();
            //if (psel.Get_ID() == 0)
            //{
            //    throw new ArgumentException("Not found C_PaySelection_ID=" + _C_PaySelection_ID);
            //}
            //if (psel.IsProcessed())
            //{
            //    throw new ArgumentException("@Processed@");
            //}
            ////	psel.getPayDate();

            //String sql = "SELECT C_Invoice_ID,"
            //    //	Open
            //    + " currencyConvert(invoiceOpen(i.C_Invoice_ID, 0)"
            //        + ",i.C_Currency_ID, " + C_CurrencyTo_ID + "," + DB.TO_DATE(psel.GetPayDate(), true) + ", i.C_ConversionType_ID,i.AD_Client_ID,i.AD_Org_ID),"	//	##1/2 Currency_To,PayDate
            //    //	Discount
            //    + " currencyConvert(paymentTermDiscount(i.GrandTotal,i.C_Currency_ID,i.C_PaymentTerm_ID,i.DateInvoiced, " + DB.TO_DATE(psel.GetPayDate(), true) + ")"	//	##3 PayDate
            //        + ",i.C_Currency_ID," + C_CurrencyTo_ID + "," + DB.TO_DATE(psel.GetPayDate(), true) + ",i.C_ConversionType_ID,i.AD_Client_ID,i.AD_Org_ID),"	//	##4/5 Currency_To,PayDate
            //    + " PaymentRule, IsSOTrx "		//	4..6
            //    + "FROM C_Invoice i "
            //    + "WHERE";
            ////if (_PaymentRule != null && _PaymentRule.Equals(X_C_Invoice.PAYMENTRULE_DirectDebit))
            ////{
            ////    sql += " IsSOTrx='Y'";
            ////}
            ////else
            ////{
            ////    sql += " IsSOTrx='N'";
            ////}
            //sql += " IsPaid='N' AND DocStatus IN ('CO','CL')" // ##6
            //    + " AND AD_Client_ID=" + psel.GetAD_Client_ID()				//	##7
            //    //	Existing Payments - Will reselect Invoice if prepared but not paid 
            //    + " AND NOT EXISTS (SELECT * FROM C_PaySelectionLine psl "
            //        + "WHERE i.C_Invoice_ID=psl.C_Invoice_ID AND psl.IsActive='Y'"
            //        + " AND psl.C_PaySelectionCheck_ID IS NOT NULL)";
            //count = 7;
            ////	Disputed
            //if (!_IncludeInDispute)
            //{
            //    sql += " AND i.IsInDispute='N'";
            //}
            ////	PaymentRule (optional)
            //if (_PaymentRule != null && _PaymentRule != " ")
            //{
            //    sql += " AND PaymentRule='" + _PaymentRule + "'";		//	##
            //    count += 1;
            //}
            ////	OnlyDiscount
            //if (_OnlyDiscount)
            //{
            //    if (_OnlyDue)
            //    {
            //        sql += " AND (";
            //    }
            //    else
            //    {
            //        sql += " AND ";
            //    }
            //    sql += "paymentTermDiscount(invoiceOpen(C_Invoice_ID, 0), C_Currency_ID, C_PaymentTerm_ID, DateInvoiced, " + DB.TO_DATE(psel.GetPayDate(), true) + ") > 0";	//	##
            //    count += 1;
            //}
            ////	OnlyDue
            //if (_OnlyDue)
            //{
            //    if (_OnlyDiscount)
            //    {
            //        sql += " OR ";
            //    }
            //    else
            //    {
            //        sql += " AND ";
            //    }
            //    sql += "paymentTermDueDays(C_PaymentTerm_ID, DateInvoiced, " + DB.TO_DATE(psel.GetPayDate(), true) + ") >= 0";	//	##
            //    count += 1;
            //    if (_OnlyDiscount)
            //    {
            //        sql += ")";
            //    }
            //}
            ////	Business Partner
            //if (_C_BPartner_ID != 0 && _C_BPartner_ID != -1)
            //{
            //    sql += " AND C_BPartner_ID=" + _C_BPartner_ID;	//	##
            //    count += 1;
            //}
            ////	Business Partner Group
            //else if (_C_BP_Group_ID != 0 && _C_BP_Group_ID != -1)
            //{
            //    sql += " AND EXISTS (SELECT * FROM C_BPartner bp "
            //        + "WHERE bp.C_BPartner_ID=i.C_BPartner_ID AND bp.C_BP_Group_ID=" + _C_BP_Group_ID + ")";	//	##
            //    count += 1;
            //}
            ////	PO Matching Requiremnent
            //if (_MatchRequirementI.Equals(X_C_Invoice.MATCHREQUIREMENTI_PurchaseOrder)
            //    || _MatchRequirementI.Equals(X_C_Invoice.MATCHREQUIREMENTI_PurchaseOrderAndReceipt))
            //{
            //    sql += " AND i._MatchRequirementI NOT IN ('N','R')"
            //        + " AND EXISTS (SELECT * FROM C_InvoiceLine il "
            //        + "WHERE i.C_Invoice_ID=il.C_Invoice_ID"
            //        + " AND QtyInvoiced IN (SELECT SUM(Qty) FROM M_MatchPO m "
            //            + "WHERE il.C_InvoiceLine_ID=m.C_InvoiceLine_ID))";
            //}
            ////	Receipt Matching Requiremnent
            //if (_MatchRequirementI.Equals(X_C_Invoice.MATCHREQUIREMENTI_Receipt)
            //    || _MatchRequirementI.Equals(X_C_Invoice.MATCHREQUIREMENTI_PurchaseOrderAndReceipt))
            //{
            //    sql += " AND i._MatchRequirementI NOT IN ('N','P')"
            //        + " AND EXISTS (SELECT * FROM C_InvoiceLine il "
            //        + "WHERE i.C_Invoice_ID=il.C_Invoice_ID"
            //        + " AND QtyInvoiced IN (SELECT SUM(Qty) FROM M_MatchInv m "
            //            + "WHERE il.C_InvoiceLine_ID=m.C_InvoiceLine_ID))";
            //}

            ////	Document No
            //else if (_DocumentNo_From != null && _DocumentNo_To != null)
            //{
            //    sql += " AND i.DocumentNo BETWEEN "
            //        + DB.TO_STRING(_DocumentNo_From) + " AND "
            //        + DB.TO_STRING(_DocumentNo_To);
            //}
            //else if (_DocumentNo_From != null)
            //{
            //    sql += " AND ";
            //    if (_DocumentNo_From.IndexOf('%') == -1)
            //    {
            //        sql += "i.DocumentNo >= "
            //            + DB.TO_STRING(_DocumentNo_From);
            //    }
            //    else
            //    {
            //        sql += "i.DocumentNo LIKE "
            //            + DB.TO_STRING(_DocumentNo_From);
            //    }
            //}
            #endregion

            String sql = CreateQuery();
            //
            int lines = 0;
            IDataReader idr = null;

            try
            {
                idr = DB.ExecuteReader(sql, null, Get_TrxName());
                while (idr.Read())
                {
                    int C_InvoicePaySchedule_ID = Util.GetValueOfInt(idr[0]);
                    int C_Invoice_ID = Util.GetValueOfInt(idr[1]);//  rs.getInt(1);
                    Decimal PayAmt = Util.GetValueOfDecimal(idr[2]); //rs.getBigDecimal(2);
                    if (C_Invoice_ID == 0 || Env.ZERO.CompareTo(PayAmt) == 0)
                    {
                        continue;
                    }

                    Decimal DiscountAmt = Util.GetValueOfDecimal(idr[5]);//rs.getBigDecimal(5);
                    Decimal DiscountAmt2 = Util.GetValueOfDecimal(idr[6]);//rs.getBigDecimal(6);
                    // OnlyDiscount
                    if (_OnlyDiscount)
                    {
                        if (DiscountAmt == 0 && DiscountAmt2 == 0)
                            continue; //Skip this if discount is ZERO in the case of ONLY DISCOUNT 
                    }

                    if (DiscountAmt2 > 0)
                        DiscountAmt = DiscountAmt2;

                    String PaymentRule = Util.GetValueOfString(idr[3]); //rs.getString(3);
                    bool isSOTrx = "Y".Equals(Util.GetValueOfString(idr[4]));//rs.getString(4));
                    //
                    lines++;
                    MPaySelectionLine pselLine = new MPaySelectionLine(psel, lines * 10, PaymentRule);
                    pselLine.SetInvoice(C_Invoice_ID, isSOTrx,
                        PayAmt, Decimal.Subtract(PayAmt, DiscountAmt), DiscountAmt);

                    if (VA009_PaymentMethod_ID == 0)
                    {
                        MInvoicePaySchedule ips = new MInvoicePaySchedule(GetCtx(), C_InvoicePaySchedule_ID, Get_TrxName());
                        pselLine.Set_Value("VA009_PaymentMethod_ID", ips.GetVA009_PaymentMethod_ID());
                        _PaymentRule = (String)(DB.ExecuteScalar("SELECT VA009_PaymentBaseType FROM VA009_PaymentMethod WHERE IsActive='Y' AND VA009_PaymentMethod_ID=" + ips.GetVA009_PaymentMethod_ID()));
                        pselLine.SetPaymentRule(_PaymentRule);
                    }
                    else
                    {
                        pselLine.Set_Value("VA009_PaymentMethod_ID", VA009_PaymentMethod_ID);
                        pselLine.SetPaymentRule(_PaymentRule);
                    }
                    pselLine.SetC_InvoicePaySchedule_ID(C_InvoicePaySchedule_ID); // Set Invoice Pay Schedule
                    if (!pselLine.Save())
                    {
                        if (idr != null)
                        {
                            idr.Close();
                            idr = null;
                        }
                        //return GetReterivedError(pselLine, "Cannot save MPaySelectionLine");
                        throw new Exception("Cannot save MPaySelectionLine");
                    }
                }
                idr.Close();

            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                }
                log.Log(Level.SEVERE, sql, e);
            }

            return "@C_PaySelectionLine_ID@  - #" + lines;
        }

        /// <summary>
        /// Create Query
        /// </summary>
        /// <returns>String, Query</returns>
        public string CreateQuery()
        {
            int count = 0;
            log.Info("C_PaySelection_ID=" + _C_PaySelection_ID
                + ", OnlyDiscount=" + _OnlyDiscount + ", OnlyDue=" + _OnlyDue
                + ", IncludeInDispute=" + _IncludeInDispute
                + ", MatchRequirement=" + _MatchRequirementI
                + ", PaymentRule=" + _PaymentRule
                + ", C_BP_Group_ID=" + _C_BP_Group_ID + ", C_BPartner_ID=" + _C_BPartner_ID);

            psel = new MPaySelection(GetCtx(), _C_PaySelection_ID, Get_TrxName());
            int C_CurrencyTo_ID = psel.GetC_Currency_ID();
            if (psel.Get_ID() == 0)
            {
                throw new ArgumentException("Not found C_PaySelection_ID=" + _C_PaySelection_ID);
            }
            if (psel.IsProcessed())
            {
                throw new ArgumentException("@Processed@");
            }

            StringBuilder sql = new StringBuilder("SELECT ips.C_INVOICEPAYSCHEDULE_ID, i.C_Invoice_ID,"
                //	Open Amount
                         + " NVL(currencyConvert(ips.DUEAMT, ips.C_Currency_ID, " + C_CurrencyTo_ID + "," + DB.TO_DATE(psel.GetPayDate(), true)
                         + ", i.C_ConversionType_ID, ips.AD_Client_ID, ips.AD_Org_ID),0) as OpenAmt,"
                         + "  i.PaymentRule, i.IsSOTrx, CASE WHEN (" + DB.TO_DATE(psel.GetPayDate(), true) + " <= IPS.DISCOUNTDATE) THEN IPS.DISCOUNTAMT ELSE 0 END AS DISCOUNT1, CASE WHEN ("
                         + DB.TO_DATE(psel.GetPayDate(), true) + " > IPS.DISCOUNTDATE AND " + DB.TO_DATE(psel.GetPayDate(), true) + "  <= IPS.DISCOUNTDAYS2) THEN IPS.DISCOUNT2 ELSE 0 END AS DISCOUNT2 "
                         + "  FROM C_INVOICEPAYSCHEDULE ips INNER JOIN C_INVOICE i ON ips.C_INVOICE_ID=i.C_INVOICE_ID"
                         + " WHERE ips.VA009_IsPaid='N' AND ips.IsHoldPayment='N' AND i.DocStatus IN ('CO','CL')" // ##6
                         + " AND ips.AD_Client_ID=" + psel.GetAD_Client_ID()				//	##7
                //	Existing Payments - Will reselect Invoice if prepared but not paid 
                         + " AND IPS.C_INVOICEPAYSCHEDULE_ID NOT IN (SELECT C_INVOICEPAYSCHEDULE_ID FROM C_PAYSELECTIONLINE PSL WHERE PSL.C_INVOICEPAYSCHEDULE_ID = IPS.C_INVOICEPAYSCHEDULE_ID)"
                         + " AND NOT EXISTS (SELECT * FROM C_PaySelectionLine psl WHERE ips.C_Invoice_ID=psl.C_Invoice_ID AND psl.IsActive='Y' AND psl.C_PaySelectionCheck_ID IS NOT NULL)");

            //	Disputed
            if (!_IncludeInDispute)
            {
                sql.Append(" AND i.IsInDispute='N'");
            }

            // Payment Method
            if (VA009_PaymentMethod_ID > 0)
            {
                sql.Append(" AND ips.VA009_PaymentMethod_ID= " + VA009_PaymentMethod_ID);		//	##
                count += 1;
            }

            //	OnlyDue
            if (_OnlyDue)
            {                
                sql.Append(" AND ips.DueDate <= " + DB.TO_DATE(psel.GetPayDate(), true));
                count += 1;
            }

            if (_C_BPartner_ID != 0 && _C_BPartner_ID != -1)
            {
                sql.Append(" AND i.C_BPartner_ID=" + _C_BPartner_ID);	//	##
                count += 1;
            }

            //	Business Partner Group
            if (_C_BP_Group_ID != 0 && _C_BP_Group_ID != -1)
            {
                sql.Append(" AND EXISTS (SELECT * FROM C_BPartner bp "
                    + "WHERE bp.C_BPartner_ID=i.C_BPartner_ID AND bp.C_BP_Group_ID=" + _C_BP_Group_ID + ")");	//	##
                count += 1;
            }

            //	Document No
            if (_DocumentNo_From != null && _DocumentNo_To != null)
            {
                sql.Append(" AND i.DocumentNo BETWEEN "
                    + DB.TO_STRING(_DocumentNo_From) + " AND "
                    + DB.TO_STRING(_DocumentNo_To));
            }
            else if (_DocumentNo_From != null)
            {
                sql.Append(" AND ");
                if (_DocumentNo_From.IndexOf('%') == -1)
                {
                    sql.Append("i.DocumentNo >= "
                        + DB.TO_STRING(_DocumentNo_From));
                }
                else
                {
                    sql.Append("i.DocumentNo LIKE "
                        + DB.TO_STRING(_DocumentNo_From));
                }
            }

            // Organization
            if (_AD_Org_ID != null && _AD_Org_ID != string.Empty)
            {
                int tableID = PO.Get_Table_ID("AD_Org");
                sql.Append(@" AND ips.AD_Org_ID IN (SELECT AD_ORG.AD_ORG_ID  FROM AD_TreeNode AD_TreeNode INNER JOIN AD_Tree AD_Tree ON AD_Tree.AD_Tree_ID =AD_TreeNode.AD_Tree_ID
                INNER JOIN AD_ORG AD_ORG ON AD_ORG.AD_ORG_ID = AD_TREENODE.NODE_ID WHERE AD_TreeNode.AD_Tree_ID IN (SELECT AD_Tree_ID FROM AD_Tree WHERE AD_Client_ID= " + GetCtx().GetAD_Client_ID() + @"
                AND AD_Table_ID=" + tableID + " AND IsActive ='Y' AND ISALLNODES='Y' AND ISDEFAULT='Y') AND (AD_ORG.AD_ORG_ID IN (" + _AD_Org_ID + ") OR AD_TREENODE.PARENT_ID IN (" + _AD_Org_ID + @"))
                AND AD_ORG.IsSummary ='N')");                
            }

            // Transaction Type
            if (VA009_Trx)
            {
                if (_isSOTrx)
                {
                    sql.Append(" AND i.IsSOTrx='Y'");
                }
                else
                {
                    sql.Append(" AND i.IsSOTrx='N'");
                }
            }
            return sql.ToString();
        }

        private void GetChildNodesID(int currentnode, string tableName, int treeID, string adtableName)
        {
            if (parentIDs.Length == 0)
            {
                parentIDs.Append(currentnode);
            }
            else
            {
                parentIDs.Append(",").Append(currentnode);
            }

            string sql = "SELECT pr.node_ID FROM " + tableName + "   pr JOIN " + adtableName + " mp on pr.Node_ID=mp." + adtableName + "_id  WHERE pr.AD_Tree_ID=" + treeID + " AND pr.Parent_ID = " + currentnode + " AND mp.ISActive='Y' AND mp.IsSummary='Y'";

            DataSet ds = DB.ExecuteDataset(sql);
            if (ds == null || ds.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                {
                    GetChildNodesID(Convert.ToInt32(ds.Tables[0].Rows[j]["node_ID"]), tableName, treeID, adtableName);
                }
            }
        }
    }
}

