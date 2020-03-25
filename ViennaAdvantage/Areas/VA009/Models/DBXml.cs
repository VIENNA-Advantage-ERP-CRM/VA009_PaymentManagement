using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using VAdvantage.DataBase;
using VAdvantage.Utility;
using System.IO.Compression;
using System.Data;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml.Linq;
using ViennaAdvantage.Model;
namespace VA009.Models
{
    public class DBXml
    {
        public string CreateJDBCDataSourceXml(Ctx ctx, string DBResourceName, int RecordID)
        {
            List<Document> obj = new List<Document>();
            string orgName = "Org Name";
            if (RecordID > 0) {
                MVA009Batch batch = new MVA009Batch(ctx, RecordID, null);
                MVA009BatchLineDetails batchDetails = null;
                MVA009PaymentMethod paymthd = null;
                string sql = @"SELECT bd.VA009_BatchLineDetails_ID
                                FROM VA009_BatchLineDetails bd
                                INNER JOIN va009_batchlines bl
                                ON bl.va009_batchlines_id= bd.va009_batchlines_id
                                WHERE VA009_Batch_ID =" + RecordID;
                DataSet dsb = DB.ExecuteDataset(sql);
                if (dsb != null && dsb.Tables[0].Rows.Count > 0)
                {
                    DataSet dsbnk = null;
                    for (int x = 0; x < dsb.Tables[0].Rows.Count; x++) {
                        batchDetails = new MVA009BatchLineDetails(ctx, Util.GetValueOfInt(dsb.Tables[0].Rows[x]["VA009_BatchLineDetails_ID"]), null);
                        paymthd = new MVA009PaymentMethod(ctx, batchDetails.GetVA009_PaymentMethod_ID(), null);
                        Document dataobj = new Document();
                        dataobj.PaymentID = Util.GetValueOfInt(batchDetails.GetVA009_BatchLineDetails_ID());
                        dataobj.PaymentMethod = paymthd.GetVA009_Name();
                        dataobj.DueAmt = batchDetails.GetDueAmt();
                        dataobj.currency =  Util.GetValueOfString(DB.ExecuteScalar("SELECT iso_code FROM c_currency WHERE c_currency_id = " + batchDetails.GetC_Currency_ID()));
                        dataobj.Org = Util.GetValueOfString(DB.ExecuteScalar("SELECT name FROM ad_org WHERE ad_org_id = " + batchDetails.GetAD_Org_ID()));
                        dsbnk = DB.ExecuteDataset("SELECT b.RoutingNo,  ac.IBAN FROM C_Bank b INNER JOIN C_bankaccount ac ON b.c_bank_id= ac.c_bank_id where ac.c_bankaccount_id =" + batch.GetC_BankAccount_ID());
                        if (dsbnk != null && dsbnk.Tables[0].Rows.Count > 0)
                        {
                            dataobj.IBAN = Util.GetValueOfString(dsbnk.Tables[0].Rows[0]["IBAN"]);
                            dataobj.BIC = Util.GetValueOfString(dsbnk.Tables[0].Rows[0]["RoutingNo"]);
                        }
                        orgName = dataobj.Org;
                        obj.Add(dataobj);
                    }
                
                }

            }
            //Get DataBase Details of Vienna Advantage Framework
            string filePath = "";
            VConnection conn = new VConnection();
            conn.SetAttributes(DBConn.CreateConnectionString());
            DataSet ds = new DataSet();
            if (conn.Db_Type == "Oracle")
            {
                #region commented
                //Document obj = new Document();
                //obj.AccountName = "Manjot";
                //obj.AccountNo = "1001";
                //objJDBC.exportedWithPermissions = "false";
                //objJDBC.folder = "/organizations";
                //objJDBC.name = DBResourceName;
                //objJDBC.version = "0";
                //objJDBC.label = DBResourceName;
                //objJDBC.creationDate = DateTime.Now.ToString("O");
                //objJDBC.updateDate = DateTime.Now.ToString("O");
                //objJDBC.driver = "tibcosoftware.jdbc.oracle.OracleDriver";
                //objJDBC.connectionUrl = "jdbc:tibcosoftware:oracle://" + conn.Db_host + ";SID=" + conn.Db_name;
                //objJDBC.connectionUser = conn.Db_uid;
                //objJDBC.connectionPassword = conn.Db_pwd;
                //objJDBC.timezone = "";
                #endregion

                filePath = XmlCreater(obj, true, DBResourceName, orgName);
                return filePath;
            }
            return filePath;
        }
        public string XmlCreater(List<Document> ClassObject, bool subFolder, string FileName, string orgName)
        {
            string filePath = "";
            if (subFolder)
            {
                filePath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "TempDownload\\PAIN008" + FileName + ".xml";
            }
            else
            {
                filePath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "TempDownload\\PAIN008" + FileName + ".xml";
            }
            //XmlTextWriter writer = new XmlTextWriter(File.Create(filePath), System.Text.Encoding.UTF8);
            ////writer.WriteAttributeString("xmlns", "urn:iso:std:iso:20022:texh:xsd:pain.008.001.02");
            ////writer.writee
            //XmlSerializer serializer = new XmlSerializer(ClassObject.GetType());
            //serializer.Serialize(writer, ClassObject);
            //writer.Close();



            //dynamic doc =null;
            //doc.Ele = "Ele1";
            //doc.Ele.childs = new List<string>();
            // doc.Ele2 = "Ele2";
            //doc.Ele2.childs = new List<string>();
            XNamespace ns = "urn:iso:std:iso:20022:texh:xsd:pain.008.001.02";
            XDocument doc = new XDocument(
                new XElement(ns + "Document",
                             new XAttribute("xmlns", ns.NamespaceName),
                             new XAttribute(XNamespace.Xmlns + "xsi",
                                  "http://www.w3.org/2001/XMLSchema-instance"),
                             new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema")));

            XElement ele = new XElement(ns + "CstmrCdtTrfInitn");

            doc.Root.Add(ele);

            XElement ele1 = new XElement(ns + "GrpHdr");
            ele.Add(ele1);

            XElement ele2 = new XElement(ns + "MsgId");
            ele2.Value = "CAV1234";
            ele1.Add(ele2);

            XElement ele3 = new XElement(ns + "CreDtTm");
            ele3.Value = System.DateTime.Now.ToString(("yyyy/MM/dd")) + "T" + System.DateTime.Now.ToString("HH:mm:ss");
            ele1.Add(ele3);

            XElement ele4 = new XElement(ns + "NbOfTxs");
            ele4.Value = "1";
            ele1.Add(ele4);

            XElement ele5 = new XElement(ns + "InitgPty");
            ele.Add(ele5);

            XElement ele6 = new XElement(ns + "Nm");
            ele6.Value = orgName;
            ele5.Add(ele6);

            XElement ele7 = new XElement(ns + "PstlAdr");
            ele5.Add(ele7);

            XElement ele8 = new XElement(ns + "Ctry");
            ele8.Value = "Country";
            ele7.Add(ele8);

            if (ClassObject.Count > 0)
            {
                for (int a = 0; a < ClassObject.Count; a++)
                {
                    XElement ele9 = new XElement(ns + "PmtInf");
                    ele.Add(ele9);

                    XElement ele10 = new XElement(ns + "PmtInfId");
                    ele10.Value = Util.GetValueOfString(ClassObject[a].PaymentID);
                    ele9.Add(ele10);

                    XElement ele11 = new XElement(ns + "PmtMtd");
                    ele11.Value = "DD";
                    ele9.Add(ele11);

                    XElement ele12 = new XElement(ns + "ReqdColltnDt");
                    ele12.Value = System.DateTime.Now.ToShortDateString();
                    ele9.Add(ele12);

                     XElement ele13 = new XElement(ns + "InstdAmt");
                     ele13.Value =  Util.GetValueOfString(ClassObject[a].currency) + Util.GetValueOfString(ClassObject[a].DueAmt);
                     ele9.Add(ele13);

                     XElement ele14 = new XElement(ns + "Cdtr");
                     ele9.Add(ele14);

                     XElement ele15 = new XElement(ns + "Nm");
                     ele15.Value = orgName;
                     ele14.Add(ele15);

                     XElement ele16 = new XElement(ns + "CdtrAcct");
                     ele9.Add(ele16);


                     XElement ele17 = new XElement(ns + "Id");
                     ele16.Add(ele17);

                     XElement ele18 = new XElement(ns + "Othr");
                     ele17.Add(ele18);

                     XElement ele19 = new XElement(ns + "IBAN");
                     ele19.Value = Util.GetValueOfString(ClassObject[a].IBAN);
                     ele18.Add(ele19);

                     XElement ele20 = new XElement(ns + "CdtrAgt");
                     ele9.Add(ele20);

                     XElement ele21 = new XElement(ns + "FinInstnId");
                     ele20.Add(ele21);

                     XElement ele22 = new XElement(ns + "BIC");
                     ele22.Value = Util.GetValueOfString(ClassObject[a].BIC);
                     ele21.Add(ele22);
                    

                }
            }

            doc.Save(filePath);

            return "TempDownload\\PAIN008" + FileName + ".xml";
        }
    }

    public class Document
    {
        public string PaymentMethod { get; set; }
        public int PaymentID { get; set; }
        public decimal DueAmt { get; set; }
        public string currency { get; set; }
        public string Org { get; set; }
        public string IBAN { get; set; }
        public string BIC { get; set; }
    }
}