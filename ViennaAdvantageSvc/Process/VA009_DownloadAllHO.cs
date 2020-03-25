using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using VAdvantage.DataBase;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;

namespace ViennaAdvantage.Process
{
    public class VA009_DownloadAllHO : SvrProcess
    {
        protected override string DoIt()
        {
            return DownloadMethod(GetCtx(), Get_TrxName());
        }

        protected override void Prepare()
        {
           // throw new NotImplementedException();
        }
        public string DownloadMethod(Ctx ctx, Trx GetTrx)
        {
            string Hostaddress = string.Empty, Port = string.Empty, UserID = string.Empty, Password = String.Empty;
            string LocalOutput = string.Empty, LocalResponse =string.Empty, ServerOutput = string.Empty, ServerResponse = string.Empty;
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

                    status = DownloadFileFromSnorkel(Hostaddress, ServerResponse, LocalResponse, UserID, Password, Util.GetValueOfInt(Port), accountno);
                }
            }
            if (status == true)
                return Msg.GetMsg(ctx, "VA009_Uploaded");
            else
                return Msg.GetMsg(ctx, "VA009_NotUploaded");
        }

        public bool DownloadFileFromSnorkel(string source, string ServerResponse, string destination, string username, string pass, int port, string accountno)
        {
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
                    ftpfullpath = ftpfullpath + @"/" + ServerResponse;

                string path = HostingEnvironment.ApplicationPhysicalPath;

                destination = @"\Areas\VA009\VA009Docs\Download";
                MOVEPATH = path + @"\" + "Consumed";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Directory.CreateDirectory(path + @"\"+ "Consumed");
                }
                FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpRequest.Credentials = new NetworkCredential(username, pass);
           
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
                        path = ftpfullpath + @"\" + directories[i].ToString();
                        string trnsfrpth = HostingEnvironment.ApplicationPhysicalPath + destination + @"\" + directories[i].ToString();
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
                        if (directories[i].Contains(accountno))
                        {
                            VA009_DownloadDoc.GetDataTabletFromCSVFile(trnsfrpth,MOVEPATH);
                        }
                    }
                }
                return true;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);
                    ftp.Credentials = new NetworkCredential(username, pass);

                    ftp.KeepAlive = true;
                    ftp.UseBinary = true;
                    ftp.Method = WebRequestMethods.Ftp.ListDirectory;

                    FtpWebResponse responseEx = (FtpWebResponse)ftp.GetResponse();
                    
                    return true;
                }
                else
                {
                    response.Close();
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
