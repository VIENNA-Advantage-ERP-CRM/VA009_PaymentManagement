using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAdvantage.DataBase;
using VAdvantage.Model;
using VAdvantage.Utility;
using ViennaAdvantage.Model;

namespace ViennaAdvantage.Model
{
    public class VA009_CreatePayments
    {
        string IsBankresponse = "N";
        string msg = "VA009_PymentSaved";
        StringBuilder sql = new StringBuilder();
        int countresponse = 0;
        MVA009BatchLineDetails batchLineDetails = null;
        MVA009BatchLines batchLines = null;

        private List<int> payment = new List<int>();
        private List<int> viewAllocationId = new List<int>();
        private string tenderType = string.Empty;
        private string createdDocType = string.Empty;

        private string viewAllocationRecords = string.Empty;
        private string PaymentCreated = string.Empty;
        private string errorMsg = string.Empty;

        private string paymentDocumentNo = string.Empty;
        private string allocationDocumentNo = string.Empty;


        public string DoIt(int recordID, Ctx ct, Trx trx, int CurrencyType_ID)
        {
            //Check Bank Response
            //            sql.Append(@"SELECT count(bd.VA009_BankResponse) FROM va009_batchlinedetails bd INNER JOIN va009_batchlines bl ON bl.va009_batchlines_id=bd.va009_batchlines_id
            //                          WHERE bl.va009_batch_id=" + recordID + " AND bd.VA009_BankResponse='IP' AND bd.AD_Client_ID = " + ct.GetAD_Client_ID() + " Group by bd.VA009_BankResponse ");
            //            countresponse = Util.GetValueOfInt(DB.ExecuteScalar(sql.ToString(), null,trx));
            sql.Clear();
            sql.Append(@"SELECT b.c_bankaccount_id,  bl.c_bpartner_id,  bld.c_currency_id,  bld.c_invoice_id,  bld.dueamt, bld.VA009_ConvertedAmt,  bld.discountamt, bld.va009_batchlinedetails_ID , bl.va009_batchlines_id , 
                                     bld.discountdate, inv.issotrx,  inv.isreturntrx, bld.c_invoicepayschedule_id, bld.ad_org_id, bld.ad_client_id , doc.DocBaseType , bld.va009_paymentmethod_id , bl.VA009_DueAmount
                                 FROM va009_batchlinedetails bld INNER JOIN va009_batchlines bl ON bl.va009_batchlines_id=bld.va009_batchlines_id 
                                 INNER JOIN va009_batch b ON b.va009_batch_id =bl.va009_batch_id INNER JOIN c_invoice inv ON inv.c_invoice_id = bld.c_invoice_id
                                 INNER JOIN C_DocType doc ON doc.C_Doctype_ID = inv.C_Doctype_ID
                                 WHERE  NVL(bl.c_payment_id , 0) = 0 AND NVL(bld.c_payment_id , 0) = 0 AND NVL(bld.C_AllocationHdr_ID , 0) = 0 AND  b.va009_batch_id    =" + recordID);

            if (IsBankresponse == "Y" && countresponse == 0)
                sql.Append(" AND bld.va009_bankresponse='RE' ORDER BY bl.c_bpartner_id ASC ");
            else if (IsBankresponse == "N")
                sql.Append(" ORDER BY bld.va009_batchlines_id ,  bl.c_bpartner_id ASC ");
            //else if (IsBankresponse == "Y" && countresponse > 0)
            //    return Msg.GetMsg(ct, "VA009_AllResponseNotAvailable");

            DataSet ds = DB.ExecuteDataset(sql.ToString(), null, trx);

            MVA009Batch _batch = new MVA009Batch(ct, recordID, trx);

            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                #region Consolidate  = true
                if (_batch.IsVA009_Consolidate() == true)
                {
                    int c_currency_id = 0; int Bpartner_ID = 0; int C_Payment_ID = 0, batchline_id = 0, allocationHeader = 0;

                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        #region Create View Allocation Header and line when the Due Amount on Batch line = 0
                        if (c_currency_id == Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]) &&
                           Bpartner_ID == Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]) &&
                           batchline_id == Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlines_id"]) &&
                            Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_DueAmount"]) == 0)
                        {
                            MAllocationLine alloclne = new MAllocationLine(ct, 0, trx);
                            alloclne.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                            alloclne.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                            alloclne.SetC_AllocationHdr_ID(allocationHeader);
                            alloclne.SetC_BPartner_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]));
                            alloclne.SetC_Invoice_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoice_id"]));
                            alloclne.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoicepayschedule_id"]));
                            alloclne.SetDateTrx(System.DateTime.Now.ToLocalTime());
                            alloclne.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                            alloclne.SetDiscountAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                            if (!alloclne.Save(trx))
                            {
                                msg = Msg.GetMsg(ct, "VA009_PymentNotSaved");
                                ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                    recordID, ppE.ToString(), ct, trx);
                                trx.Rollback();
                                payment.Clear();
                                viewAllocationId.Clear();
                                allocationDocumentNo = string.Empty;
                                paymentDocumentNo = string.Empty;
                                break;
                            }
                            else
                            {
                                // set Allocation ID on Batch Line Details
                                batchLineDetails = new MVA009BatchLineDetails(ct, Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]), trx);
                                batchLineDetails.SetC_AllocationHdr_ID(allocationHeader);
                                batchLineDetails.Save();
                            }
                        }
                        else if (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_DueAmount"]) == 0)
                        {
                            MAllocationHdr allocHdr = new MAllocationHdr(ct, 0, trx);
                            allocHdr.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                            allocHdr.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                            allocHdr.SetDateAcct(System.DateTime.Now.ToLocalTime());
                            allocHdr.SetDateTrx(System.DateTime.Now.ToLocalTime());
                            allocHdr.SetC_Currency_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]));
                            allocHdr.SetDocStatus("DR");
                            allocHdr.SetDocAction("CO");
                            if (!allocHdr.Save(trx))
                            {
                                msg = Msg.GetMsg(ct, "VA009_PymentNotSaved");
                                ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                    recordID, ppE.ToString(), ct, trx);
                                trx.Rollback();
                                payment.Clear();
                                viewAllocationId.Clear();
                                allocationDocumentNo = string.Empty;
                                paymentDocumentNo = string.Empty;
                                break;
                            }
                            else
                            {
                                if (!viewAllocationId.Contains(allocHdr.GetC_AllocationHdr_ID()))
                                {
                                    viewAllocationId.Add(allocHdr.GetC_AllocationHdr_ID());
                                }
                                MAllocationLine alloclne = new MAllocationLine(ct, 0, trx);
                                alloclne.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                                alloclne.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                                alloclne.SetC_AllocationHdr_ID(allocHdr.GetC_AllocationHdr_ID());
                                alloclne.SetC_BPartner_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]));
                                alloclne.SetC_Invoice_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoice_id"]));
                                alloclne.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoicepayschedule_id"]));
                                alloclne.SetDateTrx(System.DateTime.Now.ToLocalTime());
                                alloclne.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                                alloclne.SetDiscountAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                                if (!alloclne.Save(trx))
                                {
                                    msg = Msg.GetMsg(ct, "VA009_PymentNotSaved");
                                    ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                    SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                        recordID, ppE.ToString(), ct, trx);
                                    trx.Rollback();
                                    payment.Clear();
                                    viewAllocationId.Clear();
                                    allocationDocumentNo = string.Empty;
                                    paymentDocumentNo = string.Empty;
                                    break;
                                }
                                else
                                {
                                    c_currency_id = Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]);
                                    Bpartner_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]);
                                    batchline_id = Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlines_id"]);
                                    allocationHeader = allocHdr.GetC_AllocationHdr_ID();
                                    allocationDocumentNo += allocHdr.GetDocumentNo() + " , ";

                                    // set Allocation ID on Batch Line 
                                    //batchLines = new MVA009BatchLines(ct, Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlines_id"]), trx.trx);
                                    //batchLineDetails.SetC_AllocationHdr_ID(allocationHeader);
                                    //batchLines.Save();

                                    // set Allocation ID on Batch Line Details
                                    batchLineDetails = new MVA009BatchLineDetails(ct, Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]), trx);
                                    batchLineDetails.SetC_AllocationHdr_ID(allocationHeader);
                                    batchLineDetails.Save();
                                }
                            }
                        }
                        #endregion

                        #region Create a new entry of payment Allocate against same payment and the condition
                        else if (c_currency_id == Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]) &&
                            Bpartner_ID == Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]) &&
                            batchline_id == Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlines_id"]))
                        {
                            MPaymentAllocate PayAlocate = new MPaymentAllocate(ct, 0, trx);
                            PayAlocate.SetC_Payment_ID(C_Payment_ID);
                            PayAlocate.SetC_Invoice_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoice_id"]));
                            PayAlocate.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoicepayschedule_id"]));
                            //if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC")
                            //{
                            //    PayAlocate.SetDiscountAmt(-1 * Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                            //    PayAlocate.SetAmount(-1 * Util.GetValueOfInt(ds.Tables[0].Rows[i]["dueamt"]));
                            //    PayAlocate.SetInvoiceAmt(-1 * Util.GetValueOfInt(ds.Tables[0].Rows[i]["dueamt"]));
                            //}
                            //else
                            //{
                            PayAlocate.SetDiscountAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                            PayAlocate.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                            PayAlocate.SetInvoiceAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["dueamt"]));
                            //}
                            PayAlocate.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                            PayAlocate.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                            PayAlocate.SetWriteOffAmt(0);
                            PayAlocate.SetOverUnderAmt(0);
                            if (!PayAlocate.Save())
                            {
                                msg = Msg.GetMsg(ct, "VA009_PymentAllocateNotSaved");
                                ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                    recordID, ppE.ToString(), ct, trx);
                                trx.Rollback();
                                payment.Clear();
                                viewAllocationId.Clear();
                                allocationDocumentNo = string.Empty;
                                paymentDocumentNo = string.Empty;
                                break;
                            }
                            else
                            {
                                batchLineDetails = new MVA009BatchLineDetails(ct, Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]), trx);
                                batchLineDetails.SetC_Payment_ID(C_Payment_ID);
                                batchLineDetails.Save();
                            }
                        }
                        #endregion

                        #region Create a new payment
                        else
                        {
                            MPayment _pay = new MPayment(ct, 0, trx);
                            int C_Doctype_ID = GetDocumnetType(Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]), ct);
                            _pay.SetC_DocType_ID(C_Doctype_ID);
                            _pay.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                            _pay.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                            _pay.SetDateAcct(System.DateTime.Now);
                            _pay.SetDateTrx(System.DateTime.Now);
                            _pay.SetC_BankAccount_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bankaccount_id"]));
                            _pay.SetC_BPartner_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]));
                            #region to set bank account of business partner and name on batch line
                            if (Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]) > 0)
                            {
                                DataSet ds1 = new DataSet();
                                ds1 = DB.ExecuteDataset(@" SELECT MAX(C_BP_BankAccount_ID) as C_BP_BankAccount_ID,
                                  a_name FROM C_BP_BankAccount WHERE C_BPartner_ID = " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]) + " AND "
                                       + " AD_Org_ID =" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]) + " GROUP BY C_BP_BankAccount_ID, a_name ");
                                if (ds1.Tables != null && ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                                {
                                    //if partner bank account is not present then set null because constraint null is on ther payment table and it will not allow to save zero.
                                    if (Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]) > 0)
                                        _pay.Set_Value("C_BP_BankAccount_ID", Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                                    else
                                        _pay.Set_Value("C_BP_BankAccount_ID", null);
                                    _pay.Set_Value("a_name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                                }
                            }
                            #endregion
                            _pay.SetC_Currency_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]));
                            _pay.SetC_ConversionType_ID(CurrencyType_ID);
                            _pay.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_paymentmethod_id"]));
                            tenderType = Util.GetValueOfString(DB.ExecuteScalar(@"select VA009_PAYMENTBASETYPE from VA009_PAYMENTMETHOD where VA009_PAYMENTMETHOD_ID=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_paymentmethod_id"])));
                            if (tenderType == "K")          // Credit Card
                            {
                                _pay.SetTenderType("C");
                            }
                            else if (tenderType == "D")   // Direct Debit
                            {
                                _pay.SetTenderType("D");
                            }
                            else if (tenderType == "S")    // Check
                            {
                                _pay.SetTenderType("K");
                            }
                            else if (tenderType == "T")    // Direct Deposit
                            {
                                _pay.SetTenderType("A");
                            }
                            else
                            {
                                _pay.SetTenderType("A");
                            }
                            if (!_pay.Save())
                            {
                                msg = Msg.GetMsg(ct, "VA009_PymentNotSaved");
                                ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                    recordID, ppE.ToString(), ct, trx);
                                trx.Rollback();
                                payment.Clear();
                                viewAllocationId.Clear();
                                allocationDocumentNo = string.Empty;
                                paymentDocumentNo = string.Empty;
                                break;
                            }
                            else
                            {
                                if (!payment.Contains(_pay.GetC_Payment_ID()))
                                {
                                    payment.Add(_pay.GetC_Payment_ID());
                                }
                                c_currency_id = Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]);
                                Bpartner_ID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]);
                                C_Payment_ID = _pay.GetC_Payment_ID();
                                batchline_id = Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlines_id"]);
                                paymentDocumentNo += _pay.GetDocumentNo() + " , ";

                                MPaymentAllocate PayAlocate = new MPaymentAllocate(ct, 0, trx);
                                PayAlocate.SetC_Payment_ID(C_Payment_ID);
                                PayAlocate.SetC_Invoice_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoice_id"]));
                                PayAlocate.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoicepayschedule_id"]));
                                //if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC")
                                //{
                                //    PayAlocate.SetDiscountAmt(-1 * Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                                //    PayAlocate.SetAmount(-1 * Util.GetValueOfInt(ds.Tables[0].Rows[i]["dueamt"]));
                                //    PayAlocate.SetInvoiceAmt(-1 * Util.GetValueOfInt(ds.Tables[0].Rows[i]["dueamt"]));
                                //}
                                //else
                                //{
                                PayAlocate.SetDiscountAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                                PayAlocate.SetAmount(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                                PayAlocate.SetInvoiceAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["dueamt"]));
                                //}
                                PayAlocate.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                                PayAlocate.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                                PayAlocate.SetWriteOffAmt(0);
                                PayAlocate.SetOverUnderAmt(0);
                                if (!PayAlocate.Save())
                                {
                                    msg = Msg.GetMsg(ct, "VA009_PymentAllocateNotSaved");
                                    ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                                    SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                        recordID, ppE.ToString(), ct, trx);
                                    trx.Rollback();
                                    payment.Clear();
                                    viewAllocationId.Clear();
                                    allocationDocumentNo = string.Empty;
                                    paymentDocumentNo = string.Empty;
                                    break;
                                }
                                else
                                {
                                    batchLineDetails = new MVA009BatchLineDetails(ct, Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]), trx);
                                    batchLineDetails.SetC_Payment_ID(_pay.GetC_Payment_ID());
                                    batchLineDetails.Save();

                                    batchLines = new MVA009BatchLines(ct, Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlines_id"]), trx);
                                    batchLines.SetC_Payment_ID(_pay.GetC_Payment_ID());
                                    batchLines.Save();
                                }
                            }
                        }
                        #endregion
                    }

                    // Complete the Consolidate Records of payment
                    for (int i = 0; i < payment.Count(); i++)
                    {
                        MPayment completePayment = new MPayment(ct, payment[i], trx);
                        if (completePayment.CompleteIt() == "CO")
                        {
                            completePayment.SetDocStatus("CO");
                            completePayment.SetDocAction("CL");
                            completePayment.Save();
                        }
                    }

                    // Complete the Consolidate Records of View allocation 
                    for (int i = 0; i < viewAllocationId.Count(); i++)
                    {
                        MAllocationHdr completeAllocation = new MAllocationHdr(ct, viewAllocationId[i], trx);
                        if (completeAllocation.CompleteIt() == "CO")
                        {
                            completeAllocation.SetDocStatus("CO");
                            completeAllocation.SetDocAction("CL");
                            completeAllocation.Save();
                        }
                    }
                }
                #endregion

                #region Consolidate = false
                else if (_batch.IsVA009_Consolidate() == false)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        MPayment _pay = new MPayment(ct, 0, trx);
                        int C_Doctype_ID = GetDocumnetType(Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]), ct);
                        _pay.SetC_DocType_ID(C_Doctype_ID);
                        _pay.SetC_Invoice_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoice_id"]));
                        _pay.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_invoicepayschedule_id"]));
                        _pay.SetAD_Client_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]));
                        _pay.SetAD_Org_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]));
                        _pay.SetDateAcct(System.DateTime.Now);
                        _pay.SetDateTrx(System.DateTime.Now);
                        _pay.SetC_ConversionType_ID(CurrencyType_ID);
                        _pay.SetC_BankAccount_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bankaccount_id"]));
                        _pay.SetC_BPartner_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]));
                        #region to set bank account of business partner and name on batch line
                        if (Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]) > 0)
                        {
                            DataSet ds1 = new DataSet();
                            ds1 = DB.ExecuteDataset(@" SELECT MAX(C_BP_BankAccount_ID) as C_BP_BankAccount_ID,
                                  a_name FROM C_BP_BankAccount WHERE C_BPartner_ID = " + Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_bpartner_id"]) + " AND "
                                   + " AD_Org_ID =" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]) + " GROUP BY C_BP_BankAccount_ID, a_name ");
                            if (ds1.Tables != null && ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                            {
                                _pay.Set_Value("C_BP_BankAccount_ID", Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]));
                                //if partner bank account is not present then set null because constraint null is on ther payment table and it will not allow to save zero.
                                if (Util.GetValueOfInt(ds1.Tables[0].Rows[0]["C_BP_BankAccount_ID"]) == 0)
                                    _pay.Set_Value("C_BP_BankAccount_ID", null);
                                _pay.Set_Value("a_name", Util.GetValueOfString(ds1.Tables[0].Rows[0]["a_name"]));
                            }
                        }
                        #endregion
                        //if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC")
                        //{
                        //    _pay.SetDiscountAmt(-1 * Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                        //    _pay.SetPayAmt(-1 * Util.GetValueOfInt(ds.Tables[0].Rows[i]["dueamt"]));
                        //}
                        //else
                        //{
                        _pay.SetDiscountAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["discountamt"]));
                        //_pay.SetPayAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["dueamt"]));
                        _pay.SetPayAmt(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["VA009_ConvertedAmt"]));
                        //}
                        _pay.SetC_Currency_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["c_currency_id"]));
                        _pay.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_paymentmethod_id"]));
                        tenderType = Util.GetValueOfString(DB.ExecuteScalar(@"select VA009_PAYMENTBASETYPE from VA009_PAYMENTMETHOD where VA009_PAYMENTMETHOD_ID=" + Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_paymentmethod_id"])));
                        if (tenderType == "K")          // Credit Card
                        {
                            _pay.SetTenderType("C");
                        }
                        else if (tenderType == "D")   // Direct Debit
                        {
                            _pay.SetTenderType("D");
                        }
                        else if (tenderType == "S")    // Check
                        {
                            _pay.SetTenderType("K");
                        }
                        else if (tenderType == "T")    // Direct Deposit
                        {
                            _pay.SetTenderType("A");
                        }
                        else
                        {
                            _pay.SetTenderType("A");
                        }
                        if (!_pay.Save(trx))
                        {
                            msg = Msg.GetMsg(ct, "VA009_PymentNotSaved");
                            ValueNamePair ppE = VAdvantage.Logging.VLogger.RetrieveError();
                            SavePaymentBachLog(Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_client_id"]), Util.GetValueOfInt(ds.Tables[0].Rows[i]["ad_org_id"]),
                                                recordID, ppE.ToString(), ct, trx);
                            trx.Rollback();
                            allocationDocumentNo = string.Empty;
                            paymentDocumentNo = string.Empty;
                            break;
                        }
                        else
                        {
                            paymentDocumentNo += _pay.GetDocumentNo() + " , ";
                            batchLineDetails = new MVA009BatchLineDetails(ct, Util.GetValueOfInt(ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]), trx);
                            batchLineDetails.SetC_Payment_ID(_pay.GetC_Payment_ID());
                            batchLineDetails.Save(trx);
                            if (_pay.CompleteIt() == "CO")
                            {
                                _pay.SetDocStatus("CO");
                                _pay.SetDocAction("CL");
                                _pay.Save(trx);
                            }
                        }
                    }
                }
                #endregion
            }
            else
                return msg = Msg.GetMsg(ct, "VA009_LinesNotAvailable");

            if (paymentDocumentNo != "" || allocationDocumentNo != "")
            {
                SaveRecordPaymentBachLog(_batch.GetAD_Client_ID(), _batch.GetAD_Org_ID(), recordID, paymentDocumentNo, allocationDocumentNo, ct, trx);
            }

            return msg;
        }

        public int GetDocbaseType(string issotrx, string isreturntrx, Ctx ct)
        {
            string Docbasetype = string.Empty;

            if (issotrx == "Y" && isreturntrx == "N")
                Docbasetype = "ARR";
            else if (issotrx == "N" && isreturntrx == "N")
                Docbasetype = "APP";
            else if (issotrx == "N" && isreturntrx == "Y")
                Docbasetype = "ARR";
            else if (issotrx == "Y" && isreturntrx == "Y")
                Docbasetype = "APP";

            int DocType_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select  dt.c_doctype_id  From C_doctype DT inner join c_docbasetype DBT On dt.docbasetype=dbt.docbasetype where dbt.docbasetype='" + Docbasetype + "' AND dt.IsActive = 'Y' AND (DT.ad_org_id = " + ct.GetAD_Org_ID() + " or  DT.ad_org_id = 0) AND DT.AD_Client_ID = " + ct.GetAD_Client_ID()));
            return DocType_ID;
        }

        public int GetDocumnetType(string docbasetype, Ctx ct)
        {
            if (docbasetype == "ARI")
                docbasetype = "ARR";
            else if (docbasetype == "API")
                docbasetype = "APP";
            else if (docbasetype == "ARC") // with reverse entry
                docbasetype = "ARR";
            else if (docbasetype == "APC") // with reverse entry
                docbasetype = "APP";
            int DocType_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select  min(dt.c_doctype_id)  From C_doctype DT inner join c_docbasetype DBT On dt.docbasetype=dbt.docbasetype where dbt.docbasetype='" + docbasetype + "' AND dt.IsActive = 'Y' AND (DT.ad_org_id = " + ct.GetAD_Org_ID() + " or  DT.ad_org_id = 0) AND DT.AD_Client_ID = " + ct.GetAD_Client_ID()));
            return DocType_ID;
        }

        public void SavePaymentBachLog(int ClientId, int OrgId, int BatchId, string ErrorMsg, Ctx ct, Trx trx)
        {
            MVA009PaymentBatchLog paymentBatchLog = new MVA009PaymentBatchLog(ct, 0, trx);
            paymentBatchLog.SetAD_Client_ID(ClientId);
            paymentBatchLog.SetAD_Org_ID(OrgId);
            paymentBatchLog.SetVA009_Batch_ID(BatchId);
            paymentBatchLog.SetIsError(true);
            paymentBatchLog.SetTextMsg(errorMsg);
            paymentBatchLog.Save();
        }

        public void SaveRecordPaymentBachLog(int ClientId, int OrgId, int BatchId, string paymentDocumentNo, string allocationDocumentNo, Ctx ct, Trx trx)
        {
            MVA009PaymentBatchLog paymentBatchLog = new MVA009PaymentBatchLog(ct, 0, trx);
            paymentBatchLog.SetAD_Client_ID(ClientId);
            paymentBatchLog.SetAD_Org_ID(OrgId);
            paymentBatchLog.SetVA009_Batch_ID(BatchId);
            paymentBatchLog.SetIsError(false);
            if (paymentDocumentNo != "" && allocationDocumentNo != "")
            {
                paymentBatchLog.SetSummary("Payment Generated Document No are : " + paymentDocumentNo.Substring(0, paymentDocumentNo.Length - 2) + " And View Allocation Generated Document no are : " + allocationDocumentNo.Substring(0, allocationDocumentNo.Length - 2));
            }
            else if (paymentDocumentNo != "")
            {
                paymentBatchLog.SetSummary("Payment Generated Document No are : " + paymentDocumentNo.Substring(0, paymentDocumentNo.Length - 2));
            }
            else if (allocationDocumentNo != "")
            {
                paymentBatchLog.SetSummary("View Allocation Generated Document no are : " + allocationDocumentNo.Substring(0, allocationDocumentNo.Length - 2));
            }
            paymentBatchLog.Save();
        }
    }
}
