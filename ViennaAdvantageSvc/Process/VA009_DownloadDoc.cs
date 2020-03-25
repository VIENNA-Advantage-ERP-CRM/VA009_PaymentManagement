using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using Microsoft.VisualBasic.FileIO;
using VAdvantage.DataBase;
using VAdvantage.ImpExp;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;

namespace ViennaAdvantage.Process
{
   public class VA009_DownloadDoc : SvrProcess
    {
        protected override string DoIt()
        {
            return DownloadMethod(GetCtx(), Get_TrxName());
        }

        protected override void Prepare()
        {
            //throw new NotImplementedException();
        }

        public string DownloadMethod(Ctx ctx, Trx GetTrx)
        {
            string Hostaddress = string.Empty, Port = string.Empty, UserID = string.Empty, Password = String.Empty;
            string LocalOutput = "", LocalResponse = "", ServerOutput = "", ServerResponse = "";
            string filename = string.Empty; bool status = false; string accountno = string.Empty;
            string sql = @"SELECT DISTINCT ic.HOSTADDRESS,  ic.HOSTPORT,  ic.USERID,  ic.PASSWORD,  ic.VA017_LOCALOPFOLDER,  ic.VA017_LOCALRESPNSFOLDER, 
                          ic.VA017_SNORKELOPFOLDER,  ic.VA017_SNORKELRESPONSEFOLDER,ba.accountno FROM va009_batch b INNER JOIN va017_icici_payment ic ON 
                          ic.c_bankaccount_id=b.c_bankaccount_id INNER JOIN c_bankaccount ba ON ba.c_bankaccount_id=b.c_bankaccount_id WHERE ic.isactive='Y' AND b.processed='Y' ";

            DataSet ds = new DataSet();
            ds = DB.ExecuteDataset(sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Hostaddress = Util.GetValueOfString(ds.Tables[0].Rows[i]["HOSTADDRESS"]);
                    Port = Util.GetValueOfString(ds.Tables[0].Rows[i]["HOSTPORT"]);
                    UserID = Util.GetValueOfString(ds.Tables[0].Rows[i]["USERID"]);
                    Password = Util.GetValueOfString(ds.Tables[0].Rows[i]["PASSWORD"]);
                    LocalOutput = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA017_LOCALOPFOLDER"]);
                    LocalResponse = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA017_LOCALRESPNSFOLDER"]);
                    ServerOutput = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA017_SNORKELOPFOLDER"]);
                    ServerResponse = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA017_SNORKELRESPONSEFOLDER"]);
                    accountno = Util.GetValueOfString(ds.Tables[0].Rows[i]["accountno"]);

                    status = DownloadFileToFTP(Hostaddress, ServerResponse, LocalResponse, UserID, Password, Util.GetValueOfInt(Port), accountno);
                }
            }
            if (status == true)
                return Msg.GetMsg(ctx, "VA009_Uploaded");
            else
                return Msg.GetMsg(ctx, "VA009_NotUploaded");
        }

        public bool DownloadFileToFTP(string source, string ServerResponse, string destination, string username, string pass, int port, string accountno)
        {
            // string filename = Path.GetFileName(source);
            string MOVEPATH = "";
            string ftpfullpath = source;
            try
            {
                if (port > 0)
                {
                    ftpfullpath = ftpfullpath + ":" + port.ToString();
                }
                if (ftpfullpath.IndexOf("ftp://") == -1)
                {
                    ftpfullpath = "ftp://" + ftpfullpath;
                }
                if (ServerResponse != string.Empty)
                {
                    if (ServerResponse == "reports")
                    {
                        ftpfullpath = ftpfullpath + @"/reports/";
                    }
                    else
                    ftpfullpath = ftpfullpath + @"/Areas/VA009/VA009Docs/" + ServerResponse + @"/";
                }
                else
                    ftpfullpath = ftpfullpath + @"/Areas/VA009/VA009Docs/Download/";

                string path = HostingEnvironment.ApplicationPhysicalPath;
                if (destination == string.Empty)
                    destination = @"\Areas\VA009\VA009Docs\Response";
                else
                {
                    string crpath = destination;
                    destination = @"\Areas\VA009\VA009Docs\";
                    destination += crpath;
                }
                path += destination;
                MOVEPATH = path + @"\" + "Processed";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Directory.CreateDirectory(path + @"\" + "Processed");
                }
                FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);
                ftpRequest.Credentials = new NetworkCredential(username, pass);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                List<string> directories = new List<string>();

                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    directories.Add(line);
                    line = streamReader.ReadLine();
                }
                streamReader.Close();


                using (WebClient ftpClient = new WebClient())
                {
                    ftpClient.Credentials = new System.Net.NetworkCredential(username, pass);

                    for (int i = 0; i <= directories.Count - 1; i++)
                    {
                        if (directories[i].Contains("DMAKS_"))
                        {
                            // string[] filename = ListDirectory(ftpfullpath + "/" + directories[i].ToString(), username, pass);
                            // foreach (var file in filename)
                            // {
                            path = ftpfullpath + @"\" + directories[i].ToString();
                            string trnsfrpth = HostingEnvironment.ApplicationPhysicalPath + destination + @"\" + directories[i].ToString();
                            //trnsfrpth=trnsfrpth.Remove((trnsfrpth.Length-3),3);
                            //trnsfrpth= trnsfrpth + "xlsx";
                            MOVEPATH += @"\" + directories[i].ToString();
                            if (!Directory.Exists(HostingEnvironment.ApplicationPhysicalPath + destination))
                            {
                                Directory.CreateDirectory(HostingEnvironment.ApplicationPhysicalPath + destination);
                            }
                            ftpClient.DownloadFile(path, trnsfrpth);

                            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
                            request.Credentials = new NetworkCredential(username, pass);
                            request.Method = WebRequestMethods.Ftp.DeleteFile;

                            FtpWebResponse Delresponse = (FtpWebResponse)request.GetResponse();
                            Delresponse.Close();

                            GetDataTabletFromCSVFile(trnsfrpth, MOVEPATH);
                            //}
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static DataTable GetDataTabletFromCSVFile(string csv_file_path, string MOVEPATH)
        {
            DataTable csvData = new DataTable();

            #region Commented
            //try
            //{
            //    using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
            //    {
            //        csvReader.SetDelimiters(new string[] { "|" });
            //        csvReader.HasFieldsEnclosedInQuotes = true;
            //        string[] colFields = csvReader.ReadFields();
            //        foreach (string column in colFields)
            //        {
            //            DataColumn datecolumn = new DataColumn(column);
            //            datecolumn.AllowDBNull = true;
            //            csvData.Columns.Add(datecolumn);
            //        }
            //        while (!csvReader.EndOfData)
            //        {
            //            string[] fieldData = csvReader.ReadFields();
            //            //Making empty value as null
            //            for (int i = 0; i < fieldData.Length; i++)
            //            {
            //                if (fieldData[i] == "")
            //                {
            //                    fieldData[i] = null;
            //                }
            //            }
            //            csvData.Rows.Add(fieldData);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //}
            #endregion

           // csvData = ImportFromCSV(csv_file_path, false);
            //csvData = ReadCSV(csv_file_path);
            csvData = ReadData(csv_file_path);

            if (csvData != null && csvData.Rows.Count > 0)
            {
                for (int i = 0; i < csvData.Rows.Count; i++)
                {
                    int Batchdtlline_Id = Util.GetValueOfInt(csvData.Rows[i][11]);
                    string status = Util.GetValueOfString(csvData.Rows[i][13]);

                    if (status == "P")
                        status = "RE";
                    else if (status == "C")
                        status = "RJ";
                    else if (status == "A")
                        status = "IP";

                    int count = DB.ExecuteQuery("UPDATE va009_batchlinedetails set va009_bankresponse='" + status + "' WHERE va009_batchlinedetails_id=" + Batchdtlline_Id, null, null);
                }
                    //Move in New Folder
                    File.Move(csv_file_path, MOVEPATH);
            }
            return csvData;
        }

        #region  Function to Import data from CSV File
        public static DataSet ImportFromCSV(string _FileLocation, bool _HasHeader)
        {
            string HDR = _HasHeader ? "Yes" : "No";
            string strConn = string.Empty;
            string _fileExtension = _FileLocation.Substring(_FileLocation.LastIndexOf('.')).ToLower();

            if (_fileExtension == ".xlsx")
            {
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _FileLocation + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
            }
            else if (_fileExtension == ".xls")
            {
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _FileLocation + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
            }
            else
            {
                strConn = string.Format(
                        @"Provider=Microsoft.Jet.OleDb.4.0; Data Source={0};Extended Properties=""Text;HDR=YES;FMT=Delimited""",
                            Path.GetDirectoryName(_FileLocation));
            }
            DataSet output = new DataSet();

            try
            {
                if (_fileExtension == ".xlsx" || _fileExtension == ".xls")
                {
                    using (OleDbConnection oledbconn = new OleDbConnection(strConn))
                    {
                        oledbconn.Open();

                        DataTable schemaTable = oledbconn.GetOleDbSchemaTable(
                       OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                        foreach (DataRow schemaRow in schemaTable.Rows)
                        {
                            string sheet = schemaRow["TABLE_NAME"].ToString();

                            if (!sheet.EndsWith("_"))
                            {
                                try
                                {
                                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", oledbconn);
                                    cmd.CommandType = CommandType.Text;

                                    DataTable outputTable = new DataTable(sheet);
                                    output.Tables.Add(outputTable);
                                    new OleDbDataAdapter(cmd).Fill(outputTable);
                                }
                                catch (Exception ex)
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (OleDbConnection conn = new OleDbConnection(strConn))
                    {
                        conn.Open();
                        DataTable schemaTable = conn.GetOleDbSchemaTable(
                            OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                        foreach (DataRow schemaRow in schemaTable.Rows)
                        {
                            string sheet = schemaRow["TABLE_NAME"].ToString();

                            if (!sheet.EndsWith("_"))
                            {
                                try
                                {
                                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
                                    cmd.CommandType = CommandType.Text;
                                    DataTable outputTable = new DataTable(sheet);
                                    output.Tables.Add(outputTable);
                                    new OleDbDataAdapter(cmd).Fill(outputTable);
                                }
                                catch (Exception ex)
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return output;
            }
            return output;
        }
        #endregion

        public static DataSet ReadCSV(string filePath)
        {
            DataSet dsCSV = new DataSet();
            try
            {
                // Creates and opens an ODBC connection
                int intLengthOfFileName = filePath.Trim().Length;
                int intLastIndex = filePath.Trim().LastIndexOf("\\");
                string mstrFileName = filePath.Trim().Substring(intLastIndex + 1, intLengthOfFileName - (intLastIndex + 1));
                string mstrFilePath = filePath.Trim().Substring(0, intLastIndex);
                string strConnString = "Driver={Microsoft Text Driver (*.txt; *.csv)};Dbq=" + mstrFilePath.Trim() + ";Extensions=asc,csv,tab,txt;Persist Security Info=False";
                string sql_select;
                OdbcConnection conn;
                conn = new OdbcConnection(strConnString.Trim());
                conn.Open();
                //Creates the select command text
                sql_select = "select * from [" + mstrFileName.Trim() + "]";
                // Creates the data adapter
                OdbcDataAdapter obj_oledb_da = new OdbcDataAdapter(sql_select, conn);
                //Fills dataset with the records from CSV file
                //obj_oledb_da.Fill(ds, "csv");
                obj_oledb_da.Fill(dsCSV, "csv");
                //closes the connection
                conn.Close();
            }
            catch (Exception e) //Error
            {
            }
            return dsCSV;
        }

        public static DataTable Read(string filePath)
        {
            DataTable dt = new DataTable();
            ExcelReader reader = new ExcelReader(1, 1);
            dt = reader.ExtractDataTable(filePath, "Sheet1$");
            return dt;

        }

        public static DataTable ReadData(string filePath)
        {
            //According to the first line in the text file, create a dataTable
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[16]{ new DataColumn("DebitAcNo"), new DataColumn("BeneficiaryAcNo"), new DataColumn("BeneficiaryName"), new DataColumn("Amount"), new DataColumn("PayMode"), new DataColumn("Date(DD-MMM-YYYY)"),new DataColumn("IFSC"), new DataColumn("BeneMobileNo"), new DataColumn("BeneEmail-Id"), new DataColumn("PaymnetDetail"), new DataColumn("BeneficiaryMailingAddress"), new DataColumn("CreditNarration"),new DataColumn("BatchDetailID"), new DataColumn("BankTrxID"),new DataColumn("PaymentStatus"),new DataColumn("Details") });
            StringBuilder sb = new StringBuilder();
            List<string> list = new List<string>();
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (sr.Peek() > 0)
                {
                    string val = sr.ReadLine();
                    if (val != "")
                    {
                        list.Add(val); //Using readline method to read text file.
                    }
                }
            }
            //ignore the fist line (title line).
            for (int i = 0; i < list.Count; i++)
            {
                string[] strlist = list[i].Split('|'); //using string.split() method to split the string.
                dt.Rows.Add(strlist[0], strlist[1], strlist[2], strlist[3], strlist[4], strlist[5], strlist[6], strlist[7], strlist[8], strlist[9], strlist[10], strlist[11], strlist[12], strlist[13], strlist[14]);

                //If you want to insert it into database, you could insert from here.
            }
            return dt;
        }

    }
}
