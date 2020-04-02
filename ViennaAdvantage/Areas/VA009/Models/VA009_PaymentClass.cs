using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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
        //public List<PaymentResponse> ExportPaymentFile(Ctx ctx, int Payment_ID, bool isBatch)
        //{
        //    string _filePath = HostingEnvironment.ApplicationPhysicalPath + @"\\PaymentFiles";
        //    string fileName = string.Empty;
        //    List<PaymentResponse> batchResponse = new List<PaymentResponse>();
        //    PaymentResponse _obj = null;
        //    bool created = false;
        //    DataSet ds = null;
        //    if (isBatch)
        //    {
        //        ds = DB.ExecuteDataset(@" SELECT bld.C_Payment_ID,  p.C_BankAccount_ID,  p.DateAcct,  p.documentno
        //            FROM VA009_BatchLineDetails bld INNER JOIN VA009_BatchLines bl ON bld.VA009_BatchLines_ID=bl.VA009_BatchLines_ID
        //            INNER JOIN VA009_Batch b ON bl.VA009_Batch_ID = b.VA009_Batch_ID LEFT JOIN C_Payment p ON p.C_Payment_ID      = bld.C_Payment_ID
        //            WHERE b.VA009_Batch_ID = " + Payment_ID);
        //        if (ds != null && ds.Tables[0].Rows.Count > 0)
        //        {
        //            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //            {
        //                _obj = new PaymentResponse();
        //                fileName = String.Format("{0,-4}_{1,6}_{2,8}_{3,3}{4,4}",
        //                         "EPAY", Util.GetValueOfString(ds.Tables[0].Rows[i]["C_BankAccount_ID"]),
        //                         Convert.ToDateTime(ds.Tables[0].Rows[i]["DateAcct"]).ToString("ddMMyyyy"),
        //                         Util.GetValueOfString(ds.Tables[0].Rows[i]["documentno"]).Substring(Util.GetValueOfString(ds.Tables[0].Rows[i]["documentno"]).Length - 3, 3)
        //                         , ".DAT");
        //                created = CreateFile(_filePath, true, false, ctx, Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Payment_ID"]), fileName);
        //                if (created)
        //                {
        //                    _obj._filename = fileName;
        //                    _obj._path = _filePath;
        //                }
        //                else
        //                {
        //                    _log.SaveError("Error: ", "File not created- " + _filePath + "- " + fileName);
        //                    _obj._error = Msg.GetMsg(ctx, "VA009_Error");
        //                }
        //                batchResponse.Add(_obj);

        //            }
        //        }

        //    }
        //    else
        //    {
        //        _obj = new PaymentResponse();
        //        ds = DB.ExecuteDataset(@"SELECT C_BankAccount_ID,  DateAcct,  documentno FROM c_payment
        //                    WHERE c_payment_id = " + Payment_ID);
        //        if (ds != null && ds.Tables[0].Rows.Count > 0)
        //        {
        //            fileName = String.Format("{0,-4}_{1,6}_{2,8}_{3,3}{4,4}",
        //                          "EPAY", Util.GetValueOfString(ds.Tables[0].Rows[0]["C_BankAccount_ID"]),
        //                          Convert.ToDateTime(ds.Tables[0].Rows[0]["DateAcct"]).ToString("ddMMyyyy"),
        //                          Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]).Substring(Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]).Length - 3, 3)
        //                          , ".DAT");
        //            created = CreateFile(_filePath, true, false, ctx, Payment_ID, fileName);
        //            if (created)
        //            {
        //                _obj._filename = fileName;
        //                _obj._path = _filePath;
        //            }
        //            else
        //            {
        //                _log.SaveError("Error: ", "File not created- " + _filePath + "- " + fileName);
        //                _obj._error = Msg.GetMsg(ctx, "VA009_Error");
        //            }
        //            batchResponse.Add(_obj);
        //        }
        //    }
        //    return batchResponse;
        //}

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
            List<PaymentResponse> batchResponse = new List<PaymentResponse>();
            PaymentResponse _obj = null;
            bool created = false;
            DataSet ds = null;
            if (isBatch)
            {
                ds = DB.ExecuteDataset(@" SELECT b.VA009_Batch_ID,  b.C_BankAccount_ID,  b.VA009_DocumentDate,  b.documentno
                    FROM VA009_Batch b WHERE b.VA009_Batch_ID = " + Payment_ID);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    _obj = new PaymentResponse();
                    fileName = String.Format("{0,-4}_{1,6}_{2,8}_{3,3}{4,4}",
                             "EPAY", Util.GetValueOfString(ds.Tables[0].Rows[0]["C_BankAccount_ID"]),
                             Convert.ToDateTime(ds.Tables[0].Rows[0]["VA009_DocumentDate"]).ToString("ddMMyyyy"),
                             Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]).Substring(Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]).Length - 3, 3)
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
                ds = DB.ExecuteDataset(@"SELECT C_BankAccount_ID,  DateAcct,  documentno FROM c_payment
                            WHERE c_payment_id = " + Payment_ID);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    fileName = String.Format("{0,-4}_{1,6}_{2,8}_{3,3}{4,4}",
                                  "EPAY", Util.GetValueOfString(ds.Tables[0].Rows[0]["C_BankAccount_ID"]),
                                  Convert.ToDateTime(ds.Tables[0].Rows[0]["DateAcct"]).ToString("ddMMyyyy"),
                                  Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]).Substring(Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]).Length - 3, 3)
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
       
        //private bool CreateFile(String baseDirName, bool createLogDir, bool isClient, Ctx ct, int paymentID, string filenameFinal)
        //{
        //    String fileName = baseDirName;
        //    int index = fileName.LastIndexOf('\\');
        //    String Sufix = fileName.Substring(index + 1, fileName.Length - (index + 1));
        //    fileName = fileName.Substring(0, index);
        //    try
        //    {
        //        if (string.IsNullOrEmpty(fileName))
        //        {
        //            DirectoryInfo dir = new DirectoryInfo(fileName);
        //            if (!dir.Exists)
        //            {
        //                fileName = "";
        //            }
        //            _log.SaveError("Error: ", "File Name Empty " + "- " + fileName);
        //        }
        //        if (!string.IsNullOrEmpty(fileName) && createLogDir)
        //        {
        //            fileName += Path.DirectorySeparatorChar + "PaymentFiles";
        //            DirectoryInfo dir = new DirectoryInfo(fileName);

        //            if (!dir.Exists)
        //                dir.Create();
        //            _log.SaveError("Error: ", "Create Directory- " + "- " + fileName);
        //            if (!string.IsNullOrEmpty(fileName))
        //            {
        //                fileName += Path.DirectorySeparatorChar;
        //                if (isClient)
        //                    fileName += Sufix;


        //                //_fileNameDate = GetFileNameDate();
        //                //fileName += _fileNameDate + "_";
        //                fileName += filenameFinal;
        //                //for (int i = 0; i < 100; i++)
        //                //{
        //                _log.SaveError("Error: ", "File Name Not NUll- " + "- " + fileName);
        //                String finalName = fileName;
        //                if (!File.Exists(finalName))
        //                {
        //                    _log.SaveError("Error: ", "File not Exist- " + "- " + fileName);
        //                    FileStream file = new FileStream(finalName, FileMode.OpenOrCreate, FileAccess.Write);
        //                    _file = file;
        //                    _file.Close();
        //                    file.Close();
        //                    using (StreamWriter outputFile = new StreamWriter(finalName, true))
        //                    {
        //                        outputFile.WriteLine(createHeaderFormat(ct, paymentID, filenameFinal));
        //                        outputFile.WriteLine(createData(ct, paymentID));
        //                        outputFile.WriteLine(createFooterFormat(ct, paymentID));
        //                    }
        //                    //break;
        //                }
        //                else
        //                {
        //                    _log.SaveError("Error: ", "File Already Exist- " + "- " + fileName);
        //                }
        //                //}
        //                if (_file == null)		//	Fallback create temp file
        //                {
        //                    _log.SaveError("Error: ", "File Null- " + "- " + fileName);
        //                    _file = new FileStream(HostingEnvironment.ApplicationPhysicalPath + "\\" + Environment.GetEnvironmentVariable("TEMP") + "\\" + "PaymentFile.dat", FileMode.OpenOrCreate, FileAccess.Write);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _log.SaveError("Error: ", e.Message + "---File Null- " + "- " + fileName);
        //        Console.WriteLine(e.Message);
        //        _file = null;
        //        return false;
        //    }
        //    return true;
        //}

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

        //public string createHeaderFormat(Ctx ct, int paymentID, string filenameFinal)
        //{
        //    StringBuilder header = new StringBuilder();
        //    header.Append(filenameFinal);
        //    //outputFile.WriteLine(header);
        //    DataSet ds = DB.ExecuteDataset(@"SELECT ba.C_BankAccount_ID,  p.DateAcct,  p.documentno,  ba.Name,  ba.AccountNo,  p.payamt,
        //                       --oi.CMS01_BRegNo,
        //                         'test' as CMS01_BRegNo, oi.Phone,   oi.C_Location_ID,  
        //                        --oi.CMS01_BPAddress 
        //                       0 as CMS01_BPAddress FROM c_payment p INNER JOIN 
        //                       C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID INNER JOIN AD_OrgInfo oi ON oi.AD_Org_ID
        //                       =p.AD_Org_ID WHERE p.c_payment_id= " + paymentID);
        //    if (ds != null && ds.Tables[0].Rows.Count > 0)
        //    {
        //        header.Append(String.Format("{0,2}{1,8}{2,6}{3,40}{4,14}{5,8},{6,10}{7,15}{8,15}{9,20}{10,35}{11,35}",
        //                        "00", "EPAYMENT", Util.GetValueOfString(ds.Tables[0].Rows[0]["C_BankAccount_ID"]),
        //                        Util.GetValueOfString(ds.Tables[0].Rows[0]["Name"]),
        //                        Util.GetValueOfString(ds.Tables[0].Rows[0]["AccountNo"]),
        //                        Convert.ToDateTime(ds.Tables[0].Rows[0]["DateAcct"]).ToString("ddMMyyyy"),
        //                        Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]),
        //                        decimal.Round(Util.GetValueOfDecimal(ds.Tables[0].Rows[0]["payamt"]), 2),
        //                        Util.GetValueOfString(ds.Tables[0].Rows[0]["CMS01_BRegNo"]),
        //                        Util.GetValueOfString(ds.Tables[0].Rows[0]["Phone"]),
        //                        getLocationName(Util.GetValueOfInt(ds.Tables[0].Rows[0]["C_Location_ID"])),
        //                        getLocationName(Util.GetValueOfInt(ds.Tables[0].Rows[0]["CMS01_BPAddress"]))
        //                        ));

        //    }
        //    //outputFile.WriteLine(header);
        //    return header.ToString();
        //}

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
            header.Append(filenameFinal);
            StringBuilder sql = new StringBuilder();
            if (isBatch)
            {
                sql.Clear();
                //--oi.CMS01_BRegNo,--oi.CMS01_BPAddress
                sql.Append(@"SELECT ba.C_BankAccount_ID,  p.VA009_DocumentDate AS Dateacct,  p.documentno,
                ba.Name,  ba.AccountNo,  (SELECT SUM(VA009_ConvertedAmt)  FROM VA009_BatchLineDetails bld
                INNER JOIN VA009_BatchLines bl  ON bld.VA009_BatchLines_ID=bl.VA009_BatchLines_ID
                INNER JOIN VA009_Batch b   ON bl.VA009_Batch_ID = b.VA009_Batch_ID   INNER JOIN C_Payment p
                ON p.C_Payment_ID      = bld.C_Payment_ID  WHERE b.VA009_Batch_ID =" + paymentID + @"  ) AS payamt,
               oi.CMS01_BRegNo,  oi.Phone,  oi.C_Location_ID, 0 AS CMS01_BPAddress FROM VA009_Batch p
                INNER JOIN C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID INNER JOIN AD_OrgInfo oi
                ON oi.AD_Org_ID        =p.AD_Org_ID WHERE p.VA009_Batch_ID =" + paymentID);
            }
            else
            {
                sql.Clear();
                sql.Append(@"SELECT ba.C_BankAccount_ID,  p.DateAcct,  p.documentno,  ba.Name,  ba.AccountNo,  p.payamt,
                               --oi.CMS01_BRegNo,
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
                                "00", "EPAYMENT", Util.GetValueOfString(ds.Tables[0].Rows[0]["C_BankAccount_ID"]),
                                Util.GetValueOfString(ds.Tables[0].Rows[0]["Name"]),
                                Util.GetValueOfString(ds.Tables[0].Rows[0]["AccountNo"]),
                                Convert.ToDateTime(ds.Tables[0].Rows[0]["DateAcct"]).ToString("ddMMyyyy"),
                                Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]),
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

        //public string createData(Ctx ct, int paymentID)
        //{
        //    StringBuilder RowsData = new StringBuilder();
        //    DataSet ds = DB.ExecuteDataset(@"SELECT ba.C_BankAccount_ID,  p.DateAcct,  b.RoutingNo,  p.documentno,
        //                   ba.Name,  ba.AccountNo,  p.payamt,  'test' AS CMS01_BRegNo,
        //                   oi.Phone,  oi.C_Location_ID, 0 AS CMS01_BPAddress,  bp.A_Name,
        //                   bp.AccountNo AS BPAcctNo,  u.email FROM c_payment p INNER JOIN C_BankAccount ba
        //                   ON ba.C_BankAccount_ID=p.C_BankAccount_ID INNER JOIN C_Bank b ON b.C_Bank_ID= ba.C_bank_ID
        //                   INNER JOIN AD_OrgInfo oi ON oi.AD_Org_ID =p.AD_Org_ID LEFT JOIN C_BP_BankAccount bp
        //                   ON bp.C_BPartner_ID=p.C_BPartner_ID LEFT JOIN AD_USer u ON u.C_BPartner_ID  = p.C_BPartner_ID
        //                   WHERE p.c_payment_id=" + paymentID);
        //    //--oi.CMS01_BRegNo,--oi.CMS01_BPAddress
        //    if (ds != null && ds.Tables[0].Rows.Count > 0)
        //    {
        //        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //        {
        //            RowsData.Append(String.Format("{0,2}{1,20}{2,15}{3,17}{4,5}{5,1},{6,40}{7,20}{8,15}{9,40}{10,40}{11,40}{12,15}{13,2150}{14,2}{15,50}",
        //                            "10", Util.GetValueOfString(ds.Tables[0].Rows[i]["documentno"]),
        //                            decimal.Round(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["payamt"]), 2),
        //                             Util.GetValueOfString(ds.Tables[0].Rows[i]["RoutingNo"]),
        //                             "IFT00", "1", Util.GetValueOfString(ds.Tables[0].Rows[i]["A_Name"]),
        //                            Util.GetValueOfString(ds.Tables[0].Rows[i]["BPAcctNo"]),
        //                            Util.GetValueOfString(ds.Tables[0].Rows[i]["CMS01_BRegNo"]),
        //                            getLocationName(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_Location_ID"])),
        //                            getLocationName(Util.GetValueOfInt(ds.Tables[0].Rows[i]["CMS01_BPAddress"])),
        //                            getLocationName(Util.GetValueOfInt(ds.Tables[0].Rows[i]["CMS01_BPAddress"])),
        //                            decimal.Round(Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["payamt"]), 2),
        //                            string.Empty, string.Empty, Util.GetValueOfString(ds.Tables[0].Rows[i]["email"])
        //                            ));
        //        }
        //    }

        //    return RowsData.ToString();
        //}

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
                                    "10", Util.GetValueOfString(ds.Tables[0].Rows[i]["documentno"]),
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

        //public string createFooterFormat(Ctx ct, int paymentID)
        //{
        //    StringBuilder footer = new StringBuilder();
        //    DataSet ds = DB.ExecuteDataset(@"SELECT ba.C_BankAccount_ID,  p.documentno, p.payamt
        //                       FROM c_payment p INNER JOIN 
        //                       C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID  WHERE p.c_payment_id= " + paymentID);
        //    if (ds != null && ds.Tables[0].Rows.Count > 0)
        //    {
        //        footer.Append(String.Format("{0,2}{1,6}{2,10}{3,6}{4,15}{5,2378}",
        //                        "99", Util.GetValueOfString(ds.Tables[0].Rows[0]["C_BankAccount_ID"]),
        //                        Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]),
        //                        1,
        //                        string.Empty, string.Empty
        //                        ));

        //    }
        //    return footer.ToString();
        //}

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
                sql.Append(@"SELECT ba.C_BankAccount_ID,  p.documentno FROM VA009_Batch p 
                        INNER JOIN C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID 
                        WHERE p.VA009_Batch_ID= " + paymentID);
            }
            else
            {
                sql.Append(@"SELECT ba.C_BankAccount_ID,  p.documentno, p.payamt FROM c_payment p INNER JOIN 
                               C_BankAccount ba ON ba.C_BankAccount_ID=p.C_BankAccount_ID  WHERE p.c_payment_id= " + paymentID);
            }
            DataSet ds = DB.ExecuteDataset(sql.ToString());
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                footer.Append(String.Format("{0,2}{1,6}{2,10}{3,6}{4,15}{5,2378}",
                                "99", Util.GetValueOfString(ds.Tables[0].Rows[0]["C_BankAccount_ID"]),
                                Util.GetValueOfString(ds.Tables[0].Rows[0]["documentno"]),
                                1,
                                string.Empty, string.Empty
                                ));

            }
            return footer.ToString();
        }
    }
}