using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Utility;

namespace VA009.Models
{
    public class VA009_PaymentClass
    {
        /**	Output file				*/
        private FileStream _file = null;
        static VLogger _log = VLogger.GetVLogger("PaymentFormFile");
        decimal totalHash = decimal.Zero;

        /// <summary>
        /// To generate Payment file and it will return the File Path
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="Payment_ID">Payment Or Batch ID</param>
        /// <param name="isBatch"> Is Batch Check</param>
        /// <returns></returns>
        public List<PaymentResponse> ExportPaymentFile(Ctx ctx, int Payment_ID, bool isBatch)
        {
            string _filePath = HostingEnvironment.ApplicationPhysicalPath + @"\\PaymentFiles";
            string fileName = string.Empty;
            string documentno = string.Empty;
            List<PaymentResponse> batchResponse = new List<PaymentResponse>();
            PaymentResponse _obj = null;
            bool created = false;
            DataSet ds = null;
            if (isBatch)
            {
                ds = DB.ExecuteDataset(@" SELECT b.VA009_Batch_ID, ba.CMS01_CorporateID, b.C_BankAccount_ID,  b.VA009_DocumentDate,  b.documentno
                    FROM VA009_Batch b INNER JOIN C_BankAccount ba ON b.C_BankAccount_ID=ba.C_BankAccount_ID
                    WHERE b.VA009_Batch_ID = " + Payment_ID);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    documentno = RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]));
                    _obj = new PaymentResponse();
                    fileName = String.Format("{0,-4}_{1,-6}_{2,-8}_{3,-3}{4,-4}",
                             "EPAY", Util.GetValueOfString(ds.Tables[0].Rows[0]["CMS01_CorporateID"]),
                             DateTime.Now.ToString("ddMMyyyy"),
                             documentno.Substring(documentno.Length - 3, 3)
                             , ".DAT");
                    created = CreateFiles(_filePath, true, false, ctx, Util.GetValueOfInt(ds.Tables[0].Rows[0]["VA009_Batch_ID"]), fileName, isBatch);
                    if (created)
                    {
                        _obj._filename = fileName;
                        _obj._path = _filePath;
                    }
                    else
                    {
                        _log.SaveError("Error: ", "File not created- " + _filePath + "- " + fileName);
                        _obj._error = Msg.GetMsg(ctx, "VA009_FileExist");
                    }
                    batchResponse.Add(_obj);
                }

            }
            else
            {
                _obj = new PaymentResponse();
                ds = DB.ExecuteDataset(@"SELECT p.C_BankAccount_ID, ba.CMS01_CorporateID, p.DateAcct,  p.documentno FROM c_payment p
                        INNER JOIN C_BankAccount ba ON p.C_BankAccount_ID=ba.C_BankAccount_ID
                            WHERE p.c_payment_id = " + Payment_ID);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    documentno = RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]));
                    fileName = String.Format("{0,-4}_{1,-6}_{2,-8}_{3,-3}{4,-4}",
                                  "EPAY", Util.GetValueOfString(ds.Tables[0].Rows[0]["CMS01_CorporateID"]),
                                  DateTime.Now.ToString("ddMMyyyy"),
                                  documentno.Substring(documentno.Length - 3, 3)
                                  , ".DAT");
                    created = CreateFiles(_filePath, true, false, ctx, Payment_ID, fileName, isBatch);
                    if (created)
                    {
                        _obj._filename = fileName;
                        _obj._path = _filePath;
                    }
                    else
                    {
                        _log.SaveError("Error: ", "File not created- " + _filePath + "- " + fileName);
                        _obj._error = Msg.GetMsg(ctx, "VA009_FileExist");
                    }
                    batchResponse.Add(_obj);
                }
            }
            return batchResponse;
        }

        /// <summary>
        /// To generate Payment file CSV Format and it will return the File Path
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="Payment_ID">Payment Or Batch ID</param>
        /// <param name="isBatch"> Is Batch Check</param>
        /// <returns></returns>
        public List<PaymentResponse> ExportPaymentFileCSV(Ctx ctx, int Payment_ID, bool isBatch)
        {
            string _filePath = HostingEnvironment.ApplicationPhysicalPath + @"\\PaymentFiles";
            string fileName = string.Empty;
            string documentno = string.Empty;
            List<PaymentResponse> batchResponse = new List<PaymentResponse>();
            PaymentResponse _obj = null;
            bool created = false;
            DataSet ds = null;
            int VA009_PayFileCount = 0;
            if (isBatch)
            {
                ds = DB.ExecuteDataset(@" SELECT b.documentno, b.VA009_Batch_ID, b.VA009_DocumentDate, NVL(b.VA009_PayFileCount,0) AS VA009_PayFileCount,
                                            NVL(ba.CMS01_CorporateID,'000000') AS CMS01_CorporateID 
                    FROM VA009_Batch b INNER JOIN C_BankAccount ba ON (b.C_BankAccount_ID = ba.C_BankAccount_ID)  WHERE b.VA009_Batch_ID = " + Payment_ID);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    VA009_PayFileCount = Util.GetValueOfInt(ds.Tables[0].Rows[0]["VA009_PayFileCount"]);
                    documentno = RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["CMS01_CorporateID"]));
                    _obj = new PaymentResponse();


                    //Name should Start with "PAYMENT" and set the length of document number
                    try
                    {
                    checkfile:
                        fileName = String.Format("{0,-7}_{1,-" + documentno.Length + "}_{2,-8}_{3,-3}{4,-4}",
                                 "PAYMENT", documentno, Convert.ToDateTime(ds.Tables[0].Rows[0]["VA009_DocumentDate"]).ToString("ddMMyyyy"), Util.GetValueOfInt(VA009_PayFileCount + 1).ToString("D3"), ".CSV");

                        if (File.Exists(_filePath + "\\" + fileName))
                        {
                            VA009_PayFileCount++;
                            goto checkfile;
                        }
                    }
                    catch { }

                    created = CreateCSVFile(_filePath, true, false, ctx, Util.GetValueOfInt(ds.Tables[0].Rows[0]["VA009_Batch_ID"]), fileName, isBatch);
                    if (created)
                    {
                        DB.ExecuteQuery("UPDATE VA009_Batch SET VA009_PayFileCount = " + Util.GetValueOfInt(VA009_PayFileCount + 1).ToString("D3") + " WHERE VA009_Batch_ID=" + Payment_ID);
                        _obj._filename = fileName;
                        _obj._path = _filePath;
                    }
                    else
                    {
                        _log.SaveError("Error: ", "File not created- " + _filePath + "- " + fileName);
                        _obj._error = Msg.GetMsg(ctx, "VA009_FileExist");
                    }
                    batchResponse.Add(_obj);
                }

            }
            else
            {
                _obj = new PaymentResponse();
                ds = DB.ExecuteDataset(@"SELECT documentno,DateAcct,NVL(VA009_PayFileCount,0) AS VA009_PayFileCount FROM c_payment WHERE c_payment_id = " + Payment_ID);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    documentno = RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]));
                    //Name should Start with "PAYMENT" and set the length of document numer
                    fileName = String.Format("{0,-7}_{1,-" + documentno.Length + "}_{2,-8}_{3,-3}{4,-4}",
                                  "PAYMENT", documentno, Convert.ToDateTime(ds.Tables[0].Rows[0]["DateAcct"]).ToString("ddMMyyyy"), Util.GetValueOfInt(Util.GetValueOfInt(ds.Tables[0].Rows[0]["VA009_PayFileCount"]) + 1).ToString("D3"), ".CSV");
                    created = CreateCSVFile(_filePath, true, false, ctx, Payment_ID, fileName, isBatch);
                    if (created)
                    {
                        DB.ExecuteQuery("UPDATE C_Payment SET VA009_PayFileCount = " + Util.GetValueOfInt(Util.GetValueOfInt(ds.Tables[0].Rows[0]["VA009_PayFileCount"]) + 1).ToString("D3") + " WHERE C_Payment_ID=" + Payment_ID);
                        _obj._filename = fileName;
                        _obj._path = _filePath;
                    }
                    else
                    {
                        _log.SaveError("Error: ", "File not created- " + _filePath + "- " + fileName);
                        _obj._error = Msg.GetMsg(ctx, "VA009_FileExist");
                    }
                    batchResponse.Add(_obj);
                }
            }
            return batchResponse;
        }

        #region Create DAT File Format For CMS Specific
        /// <summary>
        /// It will Create a Dat File
        /// </summary>
        /// <param name="baseDirName"> Directory Name </param>
        /// <param name="createLogDir">Create Log Directory</param>
        /// <param name="isClient">Check FOr Client Side</param>
        /// <param name="ct">Context</param>
        /// <param name="paymentID">Payment or Batch ID</param>
        /// <param name="filenameFinal">File Name With Extention</param>
        /// <param name="isBatch">Is Batch Check</param>
        /// <returns></returns>
        private bool CreateFiles(String baseDirName, bool createLogDir, bool isClient, Ctx ct, int paymentID, string filenameFinal, bool isBatch)
        {
            String fileName = baseDirName;
            int index = fileName.LastIndexOf('\\');
            String Sufix = fileName.Substring(index + 1, fileName.Length - (index + 1));
            fileName = fileName.Substring(0, index);
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    DirectoryInfo dir = new DirectoryInfo(fileName);
                    if (!dir.Exists)
                    {
                        fileName = "";
                    }
                    _log.SaveError("Error: ", "File Name Empty " + "- " + fileName);
                }
                if (!string.IsNullOrEmpty(fileName) && createLogDir)
                {
                    fileName += Path.DirectorySeparatorChar + "PaymentFiles";
                    DirectoryInfo dir = new DirectoryInfo(fileName);

                    if (!dir.Exists)
                        dir.Create();
                    _log.SaveError("Error: ", "Create Directory- " + "- " + fileName);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        fileName += Path.DirectorySeparatorChar;
                        if (isClient)
                            fileName += Sufix;


                        //_fileNameDate = GetFileNameDate();
                        //fileName += _fileNameDate + "_";
                        fileName += filenameFinal;
                        //for (int i = 0; i < 100; i++)
                        //{
                        _log.SaveError("Error: ", "File Name Not NUll- " + "- " + fileName);
                        String finalName = fileName;
                        if (!File.Exists(finalName))
                        {
                            _log.SaveError("Error: ", "File not Exist- " + "- " + fileName);
                            FileStream file = new FileStream(finalName, FileMode.OpenOrCreate, FileAccess.Write);
                            _file = file;
                            _file.Close();
                            file.Close();
                            using (StreamWriter outputFile = new StreamWriter(finalName, true))
                            {
                                outputFile.WriteLine(createHeaderFormats(ct, paymentID, filenameFinal, isBatch));
                                outputFile.WriteLine(createDatas(ct, paymentID, isBatch));
                                outputFile.WriteLine(createFooterFormats(ct, paymentID, isBatch));
                            }
                            //break;
                        }
                        else
                        {
                            _log.SaveError("Error: ", "File Already Exist- " + "- " + fileName);
                        }
                        //}
                        if (_file == null)		//	Fallback create temp file
                        {
                            _log.SaveError("Error: ", "File Null- " + "- " + fileName);
                            _file = new FileStream(HostingEnvironment.ApplicationPhysicalPath + "\\" + "TempDownload" + "\\" + "PaymentFile.dat", FileMode.OpenOrCreate, FileAccess.Write);
                            _file.Close();//close the file which is used by system process
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.SaveError("Error: ", e.Message + "---File Null- " + "- " + fileName);
                Console.WriteLine(e.Message);
                _file = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create Header Format For DAT File
        /// </summary>
        /// <param name="ct">Context Object</param>
        /// <param name="paymentID">Payment Or Batch ID</param>
        /// <param name="filenameFinal">File Name</param>
        /// <param name="isBatch">Is Batch Check</param>
        /// <returns>Header Data</returns>
        public string createHeaderFormats(Ctx ct, int paymentID, string filenameFinal, bool isBatch)
        {
            StringBuilder header = new StringBuilder();
            //header.Append(filenameFinal);
            StringBuilder sql = new StringBuilder();
            if (isBatch)
            {
                sql.Clear();
                //--oi.CMS01_BRegNo,--oi.CMS01_BPAddress
                sql.Append(@"SELECT ba.CMS01_CorporateID, ba.C_BankAccount_ID,  p.VA009_DocumentDate AS Dateacct,  p.documentno,
                ba.Name,  ba.AccountNo,  (SELECT SUM(VA009_ConvertedAmt)  FROM VA009_BatchLineDetails bld
                INNER JOIN VA009_BatchLines bl  ON bld.VA009_BatchLines_ID=bl.VA009_BatchLines_ID
                INNER JOIN VA009_Batch b   ON bl.VA009_Batch_ID = b.VA009_Batch_ID   INNER JOIN C_Payment p
                ON p.C_Payment_ID      = bld.C_Payment_ID WHERE b.VA009_Batch_ID =" + paymentID + @"  ) AS payamt,
                oi.CMS01_BRegNo,  oi.Phone,  oi.C_Location_ID, oi.CMS01_BPAddress FROM VA009_Batch p
                INNER JOIN C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID INNER JOIN AD_OrgInfo oi
                ON oi.AD_Org_ID        =p.AD_Org_ID WHERE p.VA009_Batch_ID =" + paymentID);
            }
            else
            {
                sql.Clear();
                sql.Append(@"SELECT ba.C_BankAccount_ID,  p.DateAcct,  p.documentno,  ba.Name,  ba.AccountNo,  p.payamt,
                                ba.CMS01_CorporateID,
                                 oi.CMS01_BRegNo, oi.Phone,   oi.C_Location_ID,  
                                oi.CMS01_BPAddress
                                FROM c_payment p INNER JOIN 
                               C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID INNER JOIN AD_OrgInfo oi ON oi.AD_Org_ID
                               =p.AD_Org_ID WHERE p.c_payment_id= " + paymentID);
            }
            DataSet ds = DB.ExecuteDataset(sql.ToString());
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                header.Append(String.Format("\n{0,2}{1,8}{2,6}{3,40}{4,14}{5,8},{6,10}{7,15}{8,15}{9,20}{10,35}{11,35}\n",
                                "00", "EPAYMENT", Util.GetValueOfString(ds.Tables[0].Rows[0]["CMS01_CorporateID"]),
                                Util.GetValueOfString(ds.Tables[0].Rows[0]["Name"]),
                                Util.GetValueOfString(ds.Tables[0].Rows[0]["AccountNo"]),
                                Convert.ToDateTime(ds.Tables[0].Rows[0]["DateAcct"]).ToString("ddMMyyyy"),
                                RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"])),
                                decimal.Round(Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["payamt"]), 2),
                                Util.GetValueOfString(ds.Tables[0].Rows[0]["CMS01_BRegNo"]),
                                Util.GetValueOfString(ds.Tables[0].Rows[0]["Phone"]),
                                getLocationName(Util.GetValueOfInt(ds.Tables[0].Rows[0]["C_Location_ID"])),
                                getLocationName(Util.GetValueOfInt(ds.Tables[0].Rows[0]["CMS01_BPAddress"]))
                                ));

            }
            //outputFile.WriteLine(header);
            return header.ToString();
        }

        /// <summary>
        /// To create Data for DAT File
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="paymentID">Payment Or Batch ID</param>
        /// <param name="isBatch">Batch Check</param>
        /// <returns>Data For DAT File</returns>
        public string createDatas(Ctx ct, int paymentID, bool isBatch)
        {
            StringBuilder RowsData = new StringBuilder();
            StringBuilder sql = new StringBuilder();
            if (isBatch)
            {
                sql.Append(@"SELECT ba.C_BankAccount_ID,  p.DateAcct,  b.RoutingNo,  p.documentno,
                           ba.Name,  ba.AccountNo,  p.payamt, oi.CMS01_BRegNo,
                           oi.Phone,  oi.C_Location_ID, oi.CMS01_BPAddress,  bp.A_Name,
                           bp.AccountNo AS BPAcctNo,  u.email FROM c_payment p INNER JOIN C_BankAccount ba
                           ON ba.C_BankAccount_ID=p.C_BankAccount_ID INNER JOIN C_Bank b ON b.C_Bank_ID= ba.C_bank_ID
                           INNER JOIN AD_OrgInfo oi ON oi.AD_Org_ID =p.AD_Org_ID LEFT JOIN C_BP_BankAccount bp
                           ON bp.C_BPartner_ID=p.C_BPartner_ID LEFT JOIN AD_USer u ON u.C_BPartner_ID  = p.C_BPartner_ID
                           WHERE p.c_payment_id IN ( SELECT p.C_Payment_ID FROM VA009_BatchLineDetails bld
                           INNER JOIN VA009_BatchLines bl ON bld.VA009_BatchLines_ID=bl.VA009_BatchLines_ID
                           INNER JOIN VA009_Batch b ON bl.VA009_Batch_ID = b.VA009_Batch_ID INNER JOIN C_Payment p
                           ON p.C_Payment_ID      = bld.C_Payment_ID WHERE b.VA009_Batch_ID = " + paymentID + ")");

            }
            else
            {
                sql.Append(@"SELECT ba.C_BankAccount_ID,  p.DateAcct,  b.RoutingNo,  p.documentno,
                           ba.Name,  ba.AccountNo,  p.payamt,  oi.CMS01_BRegNo,
                           oi.Phone,  oi.C_Location_ID, oi.CMS01_BPAddress,  bp.A_Name,
                           bp.AccountNo AS BPAcctNo,  u.email FROM c_payment p INNER JOIN C_BankAccount ba
                           ON ba.C_BankAccount_ID=p.C_BankAccount_ID INNER JOIN C_Bank b ON b.C_Bank_ID= ba.C_bank_ID
                           INNER JOIN AD_OrgInfo oi ON oi.AD_Org_ID =p.AD_Org_ID LEFT JOIN C_BP_BankAccount bp
                           ON bp.C_BPartner_ID=p.C_BPartner_ID LEFT JOIN AD_USer u ON u.C_BPartner_ID  = p.C_BPartner_ID
                           WHERE p.c_payment_id=" + paymentID);
            }
            DataSet ds = DB.ExecuteDataset(sql.ToString());
            //--oi.CMS01_BRegNo,--oi.CMS01_BPAddress
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    RowsData.Append(String.Format("{0,2}{1,20}{2,15}{3,17}{4,5}{5,1},{6,40}{7,20}{8,15}{9,40}{10,40}{11,40}{12,15}{13,2150}{14,2}{15,50}\n",
                                    "10", RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["documentno"])),
                                    decimal.Round(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["payamt"]), 2),
                                     Util.GetValueOfString(ds.Tables[0].Rows[i]["RoutingNo"]),
                                     "IFT00", "1", Util.GetValueOfString(ds.Tables[0].Rows[i]["A_Name"]),
                                    Util.GetValueOfString(ds.Tables[0].Rows[i]["BPAcctNo"]),
                                    Util.GetValueOfString(ds.Tables[0].Rows[i]["CMS01_BRegNo"]),
                                    getLocationName(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Location_ID"])),
                                    getLocationName(Util.GetValueOfInt(ds.Tables[0].Rows[i]["CMS01_BPAddress"])),
                                    getLocationName(Util.GetValueOfInt(ds.Tables[0].Rows[i]["CMS01_BPAddress"])),
                                    decimal.Round(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["payamt"]), 2),
                                    string.Empty, string.Empty, Util.GetValueOfString(ds.Tables[0].Rows[i]["email"])
                                    ));
                }
            }

            return RowsData.ToString();
        }

        /// <summary>
        /// To Create Footer Data
        /// </summary>
        /// <param name="ct">COntext</param>
        /// <param name="paymentID">Payment Or Batch ID</param>
        /// <param name="isBatch">Batch Check</param>
        /// <returns>Data For Footer</returns>
        public string createFooterFormats(Ctx ct, int paymentID, bool isBatch)
        {
            StringBuilder footer = new StringBuilder();
            StringBuilder sql = new StringBuilder();
            if (isBatch)
            {
                sql.Append(@"SELECT ba.CMS01_CorporateID, ba.CMS01_HashTotal, ba.C_BankAccount_ID,  p.documentno FROM VA009_Batch p 
                        INNER JOIN C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID 
                        WHERE p.VA009_Batch_ID= " + paymentID);
            }
            else
            {
                sql.Append(@"SELECT ba.CMS01_CorporateID, ba.CMS01_HashTotal, ba.C_BankAccount_ID,  p.documentno, p.payamt FROM c_payment p INNER JOIN 
                               C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID  WHERE p.c_payment_id= " + paymentID);
            }
            DataSet ds = DB.ExecuteDataset(sql.ToString());
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                footer.Append(String.Format("{0,2}{1,6}{2,10}{3,6}{4,15}{5,2378}",
                                "99", Util.GetValueOfString(ds.Tables[0].Rows[0]["CMS01_CorporateID"]),
                                RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"])),
                                1,
                                Util.GetValueOfString(ds.Tables[0].Rows[0]["CMS01_HashTotal"]), string.Empty
                                ));

            }
            return footer.ToString();
        }
        #endregion

        #region Create CSV File Format For CMS Specific

        /// <summary>
        /// It will Create a CSV File
        /// </summary>
        /// <param name="baseDirName"> Directory Name </param>
        /// <param name="createLogDir">Create Log Directory</param>
        /// <param name="isClient">Check FOr Client Side</param>
        /// <param name="ct">Context</param>
        /// <param name="paymentID">Payment or Batch ID</param>
        /// <param name="filenameFinal">File Name With Extention</param>
        /// <param name="isBatch">Is Batch Check</param>
        /// <returns></returns>
        private bool CreateCSVFile(String baseDirName, bool createLogDir, bool isClient, Ctx ct, int paymentID, string filenameFinal, bool isBatch)
        {
            String fileName = baseDirName;
            int index = fileName.LastIndexOf('\\');
            String Sufix = fileName.Substring(index + 1, fileName.Length - (index + 1));
            fileName = fileName.Substring(0, index);
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    DirectoryInfo dir = new DirectoryInfo(fileName);
                    if (!dir.Exists)
                    {
                        fileName = "";
                    }
                    _log.SaveError("Error: ", "File Name Empty " + "- " + fileName);
                }
                if (!string.IsNullOrEmpty(fileName) && createLogDir)
                {
                    fileName += Path.DirectorySeparatorChar + "PaymentFiles";
                    DirectoryInfo dir = new DirectoryInfo(fileName);

                    if (!dir.Exists)
                        dir.Create();
                    _log.SaveError("Error: ", "Create Directory- " + "- " + fileName);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        fileName += Path.DirectorySeparatorChar;
                        if (isClient)
                            fileName += Sufix;

                        //_fileNameDate = GetFileNameDate();
                        //fileName += _fileNameDate + "_";
                        fileName += filenameFinal;
                        //for (int i = 0; i < 100; i++)
                        //{
                        _log.SaveError("Error: ", "File Name Not NUll- " + "- " + fileName);
                        String finalName = fileName;
                        if (!File.Exists(finalName))
                        {
                            _log.SaveError("Error: ", "File not Exist- " + "- " + fileName);
                            FileStream file = new FileStream(finalName, FileMode.OpenOrCreate, FileAccess.Write);
                            _file = file;
                            _file.Close();
                            file.Close();
                            using (StreamWriter outputFile = new StreamWriter(finalName, true))
                            {
                                outputFile.WriteLine(createCSVHeaderFormat(ct, paymentID, filenameFinal, isBatch));
                                outputFile.WriteLine(createTransactionDataCSV(ct, paymentID, isBatch));
                                outputFile.WriteLine(createCSVFooterFormat(ct, paymentID, isBatch));
                            }
                            //break;
                        }
                        else
                        {
                            _log.SaveError("Error: ", "File Already Exist- " + "- " + fileName);
                        }
                        //}
                        if (_file == null)		//	Fallback create temp file
                        {
                            _log.SaveError("Error: ", "File Null- " + "- " + fileName);
                            _file = new FileStream(HostingEnvironment.ApplicationPhysicalPath + "\\" + Environment.GetEnvironmentVariable("TEMP") + "\\" + "PaymentFile.dat", FileMode.OpenOrCreate, FileAccess.Write);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.SaveError("Error: ", e.Message + "---File Null- " + "- " + fileName);
                Console.WriteLine(e.Message);
                _file = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create Header Format For CSV File
        /// </summary>
        /// <param name="ct">Context Object</param>
        /// <param name="paymentID">Payment Or Batch ID</param>
        /// <param name="filenameFinal">File Name</param>
        /// <param name="isBatch">Is Batch Check</param>
        /// <returns>Header Data</returns>
        public string createCSVHeaderFormat(Ctx ct, int paymentID, string filenameFinal, bool isBatch)
        {
            StringBuilder header = new StringBuilder();
            //header.Append(filenameFinal);
            StringBuilder sql = new StringBuilder();
            if (isBatch)
            {
                sql.Clear();
                //--oi.CMS01_BRegNo,--oi.CMS01_BPAddress
                sql.Append(@"SELECT ba.CMS01_CorporateID, ba.C_BankAccount_ID,  p.VA009_DocumentDate AS Dateacct,  p.documentno,
                ba.Name,  ba.AccountNo,  (SELECT SUM(VA009_ConvertedAmt)  FROM VA009_BatchLineDetails bld
                INNER JOIN VA009_BatchLines bl  ON bld.VA009_BatchLines_ID=bl.VA009_BatchLines_ID
                INNER JOIN VA009_Batch b   ON bl.VA009_Batch_ID = b.VA009_Batch_ID   INNER JOIN C_Payment p
                ON p.C_Payment_ID      = bld.C_Payment_ID WHERE b.VA009_Batch_ID =" + paymentID + @"  ) AS payamt,
                oi.CMS01_BRegNo,  oi.Phone,  oi.C_Location_ID, oi.CMS01_BPAddress FROM VA009_Batch p
                INNER JOIN C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID INNER JOIN AD_OrgInfo oi
                ON oi.AD_Org_ID        =p.AD_Org_ID WHERE p.VA009_Batch_ID =" + paymentID);
            }
            else
            {
                sql.Clear();
                sql.Append(@"SELECT ba.C_BankAccount_ID,  p.DateAcct,  p.documentno,  ba.Name,  ba.AccountNo,  p.payamt,
                                ba.CMS01_CorporateID,
                                 oi.CMS01_BRegNo, oi.Phone,   oi.C_Location_ID,  
                                oi.CMS01_BPAddress
                                FROM c_payment p INNER JOIN 
                               C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID INNER JOIN AD_OrgInfo oi ON oi.AD_Org_ID
                               =p.AD_Org_ID WHERE p.c_payment_id= " + paymentID);
            }
            string dateFr = string.Empty;
            DataSet ds = DB.ExecuteDataset(sql.ToString());
            string BankName = string.Empty;
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                //to calculate if bank name is more than 40 character then we need to trim it by 40 character.
                BankName = Util.GetValueOfString(ds.Tables[0].Rows[0]["Name"]);
                if (BankName.Length > 40)
                {
                    BankName = RemoveSpecialCharacters(BankName).Substring(0, 40);
                }
                dateFr = Convert.ToDateTime(ds.Tables[0].Rows[0]["DateAcct"]).ToString("ddMMyyyy").ToString();
                // Need to replace BulkPayment with  01 and set allignment to left to right added blank column to match sequence
                header.Append(String.Format("{0,-2},{1,-" + Util.GetValueOfString("01").Length + "}," +
                    "{2,-" + Util.GetValueOfString(ds.Tables[0].Rows[0]["CMS01_CorporateID"]).Length + "}," +
                    "{3,-" + string.Empty.Length + "},{4,-" + BankName.Length + "}," +
                    "{5,-" + RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["AccountNo"])).Length + "}," +
                    "{6,-8},{7,-" + RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"])).Length + "}," +
                    "{8,-" + RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["CMS01_BRegNo"])).Length + "}," +
                    "{9,1},{10,1},{11,1},{12,1},{13,1},{14,-" + string.Empty.Length + "},{15,-" + string.Empty.Length + "}," +
                    "{16,-" + string.Empty.Length + "},{17,-" + string.Empty.Length + "},{18,-" + string.Empty.Length + "}",
                                "00", "01", Util.GetValueOfString(ds.Tables[0].Rows[0]["CMS01_CorporateID"]), string.Empty,
                                BankName,
                                RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["AccountNo"])),
                                dateFr,
                                RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"])),
                                RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["CMS01_BRegNo"])), "A", "N", "N", "N", "I",
                                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));

            }
            return header.ToString();
        }

        /// <summary>
        /// To create Transaction Data for CSV File
        /// </summary>
        /// <param name="ct">Context</param>
        /// <param name="paymentID">Payment Or Batch ID</param>
        /// <param name="isBatch">Batch Check</param>
        /// <returns>Data For CSV File</returns>
        public string createTransactionDataCSV(Ctx ct, int paymentID, bool isBatch)
        {
            StringBuilder RowsData = new StringBuilder();
            StringBuilder sql = new StringBuilder();
            if (isBatch)
            {
                sql.Append(@"SELECT ba.C_BankAccount_ID,  p.DateAcct,  b.RoutingNo,  p.documentno,p.description,
                           ba.Name,  ba.AccountNo,  p.payamt, oi.CMS01_BRegNo,cp.CMS01_IsResident,cp.ReferenceNo,
                           oi.Phone,  oi.C_Location_ID, oi.CMS01_BPAddress, bpl.C_Location_ID as BPAddress, bp.A_Name,cp.CMS01_BeneficiaryIDIndicator,
                           bp.AccountNo AS BPAcctNo, p.RoutingNo as swiftcode,  p.AccountNo as Acctnumber,  p.A_Name as AcctName, 
                           u.email FROM c_payment p INNER JOIN C_BankAccount ba
                           ON ba.C_BankAccount_ID=p.C_BankAccount_ID INNER JOIN C_Bank b ON b.C_Bank_ID= ba.C_bank_ID
                            INNER JOIN C_BPartner cp ON cp.C_BPartner_ID= p.C_BPartner_ID
                           INNER JOIN C_BPartner_Location bpl ON bpl.C_BPartner_Location_ID=p.C_BPartner_Location_ID
                           INNER JOIN AD_OrgInfo oi ON oi.AD_Org_ID =p.AD_Org_ID LEFT JOIN C_BP_BankAccount bp
                           ON bp.C_BPartner_ID=p.C_BPartner_ID LEFT JOIN AD_USer u ON u.C_BPartner_ID  = p.C_BPartner_ID
                           WHERE p.c_payment_id IN ( SELECT p.C_Payment_ID FROM VA009_BatchLineDetails bld
                           INNER JOIN VA009_BatchLines bl ON bld.VA009_BatchLines_ID=bl.VA009_BatchLines_ID
                           INNER JOIN VA009_Batch b ON bl.VA009_Batch_ID = b.VA009_Batch_ID INNER JOIN C_Payment p
                           ON p.C_Payment_ID= bld.C_Payment_ID WHERE b.VA009_Batch_ID = " + paymentID + " ) " +
                           @"AND bp.C_BP_BankAccount_ID=p.C_BP_BankAccount_ID  GROUP BY ba.C_BankAccount_ID,  p.DateAcct,  b.RoutingNo,  p.documentno,p.description,
                           ba.Name, ba.AccountNo, p.payamt, oi.CMS01_BRegNo, cp.CMS01_IsResident, cp.ReferenceNo,
                           oi.Phone, oi.C_Location_ID, oi.CMS01_BPAddress, bpl.C_Location_ID, bp.A_Name, cp.CMS01_BeneficiaryIDIndicator,
                           bp.AccountNo, p.RoutingNo, p.AccountNo, p.A_Name,
                           u.email ");

            }
            else
            {
                //cp.CMS01_BeneficiaryIDIndicator,
                sql.Append(@"SELECT ba.C_BankAccount_ID,  p.DateAcct,  b.RoutingNo,  p.documentno,p.description,
                           ba.Name,  ba.AccountNo,  p.payamt,  oi.CMS01_BRegNo,cp.CMS01_IsResident,cp.ReferenceNo,
                           oi.Phone,  oi.C_Location_ID, oi.CMS01_BPAddress, bpl.C_Location_ID as BPAddress,  bp.A_Name, cp.CMS01_BeneficiaryIDIndicator,
                           bp.AccountNo AS BPAcctNo, p.RoutingNo as swiftcode,  p.AccountNo as Acctnumber,  p.A_Name as AcctName,
                           u.email FROM c_payment p INNER JOIN C_BankAccount ba
                           ON ba.C_BankAccount_ID=p.C_BankAccount_ID INNER JOIN C_Bank b ON b.C_Bank_ID= ba.C_bank_ID
                           INNER JOIN C_BPartner cp ON cp.C_BPartner_ID= p.C_BPartner_ID
                           INNER JOIN C_BPartner_Location bpl ON bpl.C_BPartner_Location_ID=p.C_BPartner_Location_ID
                           INNER JOIN AD_OrgInfo oi ON oi.AD_Org_ID =p.AD_Org_ID LEFT JOIN C_BP_BankAccount bp
                           ON bp.C_BPartner_ID=p.C_BPartner_ID LEFT JOIN AD_USer u ON u.C_BPartner_ID  = p.C_BPartner_ID
                           WHERE p.c_payment_id=" + paymentID + " AND bp.C_BP_BankAccount_ID=p.C_BP_BankAccount_ID  GROUP BY " +
                           @" ba.C_BankAccount_ID,  p.DateAcct,  b.RoutingNo,  p.documentno, p.description,
                           ba.Name, ba.AccountNo, p.payamt, oi.CMS01_BRegNo, cp.CMS01_IsResident, cp.ReferenceNo,
                           oi.Phone, oi.C_Location_ID, oi.CMS01_BPAddress, bpl.C_Location_ID ,  bp.A_Name, cp.CMS01_BeneficiaryIDIndicator,
                           bp.AccountNo, p.RoutingNo, p.AccountNo,  p.A_Name,  u.email ");
            }
            DataSet ds = DB.ExecuteDataset(sql.ToString());
            int length = 0;
            //--oi.CMS01_BRegNo,--oi.CMS01_BPAddress
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                string isresident = "2", idIndicator = string.Empty;
                string formatStringNewLine = string.Empty;
                string bpAddress = string.Empty;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        formatStringNewLine = string.Empty;
                    }
                    else
                    {
                        formatStringNewLine = "\n";
                    }

                    if (Util.GetValueOfString(ds.Tables[0].Rows[i]["CMS01_IsResident"]).Equals("Y"))
                    {
                        isresident = "1";
                    }
                    else
                    {
                        isresident = "2";
                    }

                    if (Util.GetValueOfString(ds.Tables[0].Rows[i]["CMS01_BeneficiaryIDIndicator"]).Equals("NI"))
                    {
                        idIndicator = "01";
                    }
                    else if (Util.GetValueOfString(ds.Tables[0].Rows[i]["CMS01_BeneficiaryIDIndicator"]).Equals("OI"))
                    {
                        idIndicator = "02";
                    }
                    else if (Util.GetValueOfString(ds.Tables[0].Rows[i]["CMS01_BeneficiaryIDIndicator"]).Equals("PN"))
                    {
                        idIndicator = "03";
                    }
                    else if (Util.GetValueOfString(ds.Tables[0].Rows[i]["CMS01_BeneficiaryIDIndicator"]).Equals("AP"))
                    {
                        idIndicator = "04";
                    }
                    else
                    {
                        idIndicator = "05";
                    }
                    // to get business partner address
                    string bpAdd1 = string.Empty, bpAdd2 = string.Empty, bpAdd3 = string.Empty;
                    bpAddress = RemoveSpecialCharacters(getLocationName(Util.GetValueOfInt(ds.Tables[0].Rows[i]["BPAddress"])));
                    if (bpAddress.Length > 0)
                    {
                        if (bpAddress.Length <= 40)
                        {
                            bpAdd1 = bpAddress.Substring(0, bpAddress.Length);
                        }
                        if (bpAddress.Length > 40)
                        {
                            bpAdd1 = bpAddress.Substring(0, 40);
                        }
                        if (bpAddress.Length > 40 && bpAddress.Length <= 80)
                        {
                            bpAdd2 = bpAddress.Substring(40, bpAddress.Length - 40);
                        }
                        if (bpAddress.Length > 80 && bpAddress.Length <= 120)
                        {
                            bpAdd3 = bpAddress.Substring(80, bpAddress.Length - 80);
                        }
                        if (bpAddress.Length > 120)
                        {
                            bpAdd3 = bpAddress.Substring(80, 40);
                        }
                    }
                    //to append blank column to match the sequence of the file and columns
                    RowsData.Append(String.Format("" + formatStringNewLine + "{0,-2},{1,-" + Util.GetValueOfString("10").Length + "}," +
                        "{2,-" + RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["documentno"])).Length + "}," +
                        "{3,-" + Util.GetValueOfDouble(decimal.Round(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["payamt"]), 2)).ToString().Length + "}," +
                        "{4,-" + RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["swiftcode"])).Length + "}," +
                        "{5,1},{6,-" + RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["AcctName"])).Length + "}," +
                        "{7,-" + RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["Acctnumber"])).Length + "}," +
                        "{8,-2},{9,-" + RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["ReferenceNo"])).Length + "}," +
                        "{10,-" + bpAdd1.Length + "}," +
                        "{11,-" + bpAdd2.Length + "}," +
                        "{12,-" + bpAdd3.Length + "}," +
                        "{13,-" + Util.GetValueOfString("Payment").Length + "},{14,-" + string.Empty.Length + "}," +
                        "{15,-" + string.Empty.Length + "},{16,-" + string.Empty.Length + "},{17,-" + string.Empty.Length + "},{18,-" + string.Empty.Length + "}," +
                        "{19,-" + string.Empty.Length + "},{20,-" + string.Empty.Length + "},{21,-" + string.Empty.Length + "},{22,-" + string.Empty.Length + "}," +
                        "{23,-" + string.Empty.Length + "},{24,-" + string.Empty.Length + "},{25,-" + string.Empty.Length + "},{26,-" + string.Empty.Length + "}," +
                        "{27,-" + Util.GetValueOfString(ds.Tables[0].Rows[i]["email"]).Length + "},{28,-" + string.Empty.Length + "}," +
                        "{29,-" + string.Empty.Length + "},{30,-" + string.Empty.Length + "},{31,-" + string.Empty.Length + "},{32,-" + string.Empty.Length + "}",
                                    "10", "10", RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["documentno"])),
                                    decimal.Round(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["payamt"]), 2),
                                     RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["swiftcode"])),
                                     isresident,
                                    RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["AcctName"])),
                                    RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["Acctnumber"])),
                                    idIndicator,
                                    RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["ReferenceNo"])),
                                    bpAdd1, bpAdd2, bpAdd3,
                                    Util.GetValueOfString("Payment"), string.Empty, string.Empty,
                                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                    Util.GetValueOfString(ds.Tables[0].Rows[i]["email"]),
                                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty
                                    ));
                    length = Util.GetValueOfString("AUTOCREDIT" + RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["documentno"])) + " RM " + decimal.Round(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["payamt"]), 2).ToString()).Length;
                    RowsData.Append(String.Format("\n{0,-2},{1,-" + length + "},{2,-" + string.Empty.Length + "},{3,-" + string.Empty.Length + "},{4,-" + string.Empty.Length + "},{5,-" + string.Empty.Length + "},{6,-" + string.Empty.Length + "}",
                               "20", "AUTOCREDIT" + RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["documentno"])) + " RM " + decimal.Round(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["payamt"]), 2).ToString(),
                               string.Empty, string.Empty, string.Empty, string.Empty, string.Empty
                               ));
                    totalHash += claculateHashTotal(RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[i]["Acctnumber"])), Decimal.Truncate(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["payamt"])));
                }
            }

            return RowsData.ToString();
        }

        /// <summary>
        /// To Create Footer Data FOr CSV
        /// </summary>
        /// <param name="ct">COntext</param>
        /// <param name="paymentID">Payment Or Batch ID</param>
        /// <param name="isBatch">Batch Check</param>
        /// <returns>Data For Footer</returns>
        public string createCSVFooterFormat(Ctx ct, int paymentID, bool isBatch)
        {
            StringBuilder footer = new StringBuilder();
            StringBuilder sql = new StringBuilder();
            DataSet dss = new DataSet();
            int paymentCount = 1;
            decimal totalAmt = 0, hashTotal = 0;
            if (isBatch)
            {
                sql.Append(@"SELECT ba.CMS01_CorporateID, ba.CMS01_HashTotal, ba.C_BankAccount_ID,  p.documentno, ba.AccountNo, pa.AccountNo as Acctnumber FROM VA009_Batch p 
                        INNER JOIN C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID INNER JOIN va009_batchlines bl ON bl.VA009_Batch_ID=p.VA009_Batch_ID LEFT 
                        JOIN C_Payment pa ON pa.C_Payment_ID=bl.c_payment_id WHERE p.VA009_Batch_ID= " + paymentID);

                paymentCount = Util.GetValueOfInt(DB.ExecuteScalar($@"SELECT Count(Distinct bld.C_Payment_ID) FROM VA009_BatchLineDetails bld 
                                INNER JOIN VA009_BatchLines bl ON (bl.VA009_BatchLines_ID = bld.VA009_BatchLines_ID)
                                WHERE bl.VA009_Batch_ID = {paymentID}"));
                totalAmt = Util.GetValueOfDecimal(DB.ExecuteScalar(@" SELECT SUM(p.payamt) FROM c_payment p WHERE c_payment_id IN   (SELECT DISTINCT c_payment_id
                            FROM va009_batchlinedetails   WHERE va009_batchlines_id IN    (SELECT va009_batchlines_id    FROM va009_batchlines     WHERE VA009_Batch_ID = " + paymentID + ") ) "));
            }
            else
            {
                sql.Append(@"SELECT ba.CMS01_CorporateID, ba.CMS01_HashTotal, ba.C_BankAccount_ID,  p.documentno, p.payamt,bp.AccountNo,  p.AccountNo as Acctnumber   FROM c_payment p INNER JOIN 
                               C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID LEFT JOIN C_BP_BankAccount bp
                           ON bp.C_BPartner_ID=p.C_BPartner_ID WHERE p.c_payment_id= " + paymentID);
            }
            DataSet ds = DB.ExecuteDataset(sql.ToString());
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                if (!isBatch)
                {
                    totalAmt = Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["payamt"]);
                }
                //hashTotal = claculateHashTotal(RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["Acctnumber"])), Decimal.Truncate(totalAmt));
                hashTotal = totalHash;
                decimal b = hashTotal * 24;
                decimal c = b + 2994;
                hashTotal = Decimal.Truncate(c / 285);
                //to return only the integral part of decimal.

                //set allignment from left to right
                footer.Append(String.Format("{0,-2},{1,-" + RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"])).Length + "}," +
                    "{2,-" + paymentCount.ToString().Length + "},{3,-" + Util.GetValueOfDouble(decimal.Round(totalAmt, 2)).ToString().Length + "}," +
                    "{4,-" + hashTotal.ToString().Length + "},{5,-" + string.Empty.Length + "},{6,-" + string.Empty.Length + "},{7,-" + string.Empty.Length + "}," +
                    "{8,-" + string.Empty.Length + "},{9,-" + string.Empty.Length + "}",
                            "99", RemoveSpecialCharacters(Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"])),
                            paymentCount, decimal.Round(totalAmt, 2),
                            hashTotal, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty
                            ));

            }
            return footer.ToString();
        }
        /// <summary>
        /// TO GET HASH TOTAL FORMULA GIVEN BY CMS BANK PDF
        /// </summary>
        /// <param name="accountno">ACCOUNT NO</param>
        /// <param name="amount">AMOUNT</param>
        /// <returns>HASH TOTAL</returns>
        public decimal claculateHashTotal(string accountno, decimal amount)
        {
            decimal amt = decimal.Zero;
            if (amount.ToString().Length > 6)
            {
                amt = Util.GetValueOfDecimal(amount.ToString().Substring(amount.ToString().Length - 6, 6));
            }
            else
            {
                amt = amount;
            }
            decimal hashamount = Util.GetValueOfDecimal(accountno.Substring(accountno.Length - 6, 6)) * amt;
            return hashamount;
            ////FORMULA GIVEN BY CMS BANK 
            //decimal calculateHash = (Util.GetValueOfDecimal(accountno.Substring(accountno.Length - 6, 6)) * amount);
            //decimal b = calculateHash * 24;
            //decimal c = b + 2994;
            //decimal hashTotal = c / 285;
            ////to return only the integral part of decimal.
            //return Decimal.Truncate(hashTotal);
        }
        #endregion


        /// <summary>
        /// Get The Name of the location
        /// </summary>
        /// <param name="c_location_id">Location ID</param>
        /// <returns>Name of the location</returns>
        public string getLocationName(int c_location_id)
        {
            return Util.GetValueOfString(DB.ExecuteScalar(@"SELECT (NVL(cn.Name,'')|| ' ' || NVL(C_Location.ADDRESS1, '')|| ' '|| NVL(C_Location.ADDRESS2, '')
                              || ' '|| NVL(C_Location.ADDRESS3, '')|| ' '|| NVL(C_Location.ADDRESS4, '')|| ' '|| NVL(C_Location.CITY, '')
                              || ' '|| NVL(C_Location.REGIONNAME, '')|| ' '|| NVL(C_Location.POSTAL, '')|| ' '|| NVL(C_Location.POSTAL_ADD, '')) AS address
                              FROM C_Location C_Location LEFT JOIN C_Country cn ON cn.C_COUNTRY_ID = C_Location.C_COUNTRY_ID WHERE C_Location.C_location_ID =" + c_location_id));
        }

        /// <summary>
        /// To remove special characters from string
        /// </summary>
        /// <param name="str">Filename</param>
        /// <returns>stringwith no special characters</returns>
        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.^\\w\\s]+", "", RegexOptions.Compiled);
        }
    }
}