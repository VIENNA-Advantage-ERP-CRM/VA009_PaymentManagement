using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml;
using VAdvantage.DataBase;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;
using ViennaAdvantage.PaymentClass;

namespace ViennaAdvantage.Process
{
   public class VA009_UploadDoc : SvrProcess
    {
        protected override string DoIt()
        {
            return GetMethod(GetCtx(), Get_TrxName());
        }

        protected override void Prepare()
        {
            
        }

        public string GetMethod(Ctx ctx, Trx GetTrx)
        {
            string Hostaddress = string.Empty, Port = string.Empty, UserID = string.Empty, Password = String.Empty;
            string LocalOutput = "", LocalResponse = "", ServerOutput = "", ServerResponse = ""; string Movepath = "";
            string filename = string.Empty; bool status = false;
            string sql = @"SELECT DISTINCT ic.HOSTADDRESS,  ic.HOSTPORT,  ic.USERID,  ic.PASSWORD,  ic.VA017_LOCALOPFOLDER,  ic.VA017_LOCALRESPNSFOLDER, 
                          ic.VA017_SNORKELOPFOLDER,  ic.VA017_SNORKELRESPONSEFOLDER FROM va009_batch b INNER JOIN va017_icici_payment ic ON 
                          ic.c_bankaccount_id=b.c_bankaccount_id WHERE ic.isactive='Y' AND b.processed='Y' ";
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

                    string path = HostingEnvironment.ApplicationPhysicalPath;
                    if (LocalOutput == string.Empty)
                    {
                        LocalOutput = @"\Areas\VA009\VA009Docs\Output";
                    }
                    else
                    {
                        string crpath = LocalOutput;
                        string Mvpath = Movepath;
                        LocalOutput = @"\Areas\VA009\VA009Docs\";
                        LocalOutput += crpath;
                    }
                    path += LocalOutput;
                    Movepath = path + @"\Sent";
                    status = UploadFileToFTP(path, ServerOutput, Hostaddress, UserID, Password, Util.GetValueOfInt(Port), Movepath);
                }
            }
            if (status == true)
                return Msg.GetMsg(ctx, "VA009_Uploaded");
            else
                return Msg.GetMsg(ctx, "VA009_NotUploaded");
        }

        //public bool UploadFileToFTP(string source, string ServerOutput, string destination, string username, string pass, int port, string movepath)
        //{
        //    //string filename = Path.GetFileName(source);
        //    string[] filename = VA009_ICICI_Snorkel.GetFileNames(source);
        //    try
        //    {
        //        foreach (var file in filename)
        //        {
        //            string ftpfullpath = destination;
        //            try
        //            {
        //                if (port > 0)
        //                {
        //                    ftpfullpath = ftpfullpath + ":" + port.ToString();
        //                }
        //                if (ftpfullpath.IndexOf("ftp://") == -1)
        //                {
        //                    ftpfullpath = "ftp://" + ftpfullpath;
        //                }
        //                if (ServerOutput != string.Empty)
        //                {
        //                    if (ServerOutput == "HO")
        //                    {
        //                        ftpfullpath = ftpfullpath + @"/Upload";
        //                    }
        //                    else
        //                        ftpfullpath = ftpfullpath + @"/Areas/VA009/VA009Docs/" + ServerOutput;
        //                }
        //                else
        //                    ftpfullpath = ftpfullpath + @"/Areas/VA009/VA009Docs";
        //                VA009_ICICI_UploadFile.CreateFolderToFTP(ftpfullpath, username, pass);
        //                ftpfullpath += "/" + file;
        //                FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);
        //                ftp.Credentials = new NetworkCredential(username, pass);
        //                ftp.KeepAlive = true;
        //                ftp.UseBinary = true;
        //                //ftp.Method = WebRequestMethods.Ftp.UploadFile;
        //                //ServicePointManager.ServerCertificateValidationCallback =
        //                //delegate(object s, X509Certificate certificate,
        //                //     X509Chain chain, SslPolicyErrors sslPolicyErrors)
        //                //{ return true; };
        //                ftp.EnableSsl = false;
        //                if (File.Exists(ftpfullpath))
        //                {
        //                    File.Delete(ftpfullpath);
        //                }
        //                FileStream fs = File.OpenRead(source + @"/" + file);
        //                byte[] buffer = new byte[fs.Length];
        //                fs.Read(buffer, 0, buffer.Length);
        //                fs.Close();
        //                //using (WebResponse response = ftp.GetResponse())
        //                //{
        //                //    using (Stream stream = response.GetResponseStream())
        //                //    {
        //                //        XmlTextReader reader = new XmlTextReader(stream);
        //                //    }
        //                //}
        //                Stream ftpstream = ftp.GetRequestStream();
        //                ftpstream.Write(buffer, 0, buffer.Length);
        //                ftpstream.Close();

        //                //Move in New Folder
        //                File.Move(source + @"/" + file, movepath + @"/" + file);

        //            }
        //            catch (WebException ex)
        //            {
        //                FtpWebResponse response = (FtpWebResponse)ex.Response;
        //                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
        //                {
        //                    VA009_ICICI_UploadFile.CreateFolderToFTP(ftpfullpath, username, pass);
        //                    FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);
        //                    ftp.Credentials = new NetworkCredential(username, pass);

        //                    ftp.KeepAlive = true;
        //                    ftp.UseBinary = true;
        //                    ftp.Method = WebRequestMethods.Ftp.UploadFile;
        //                    //                 ServicePointManager.ServerCertificateValidationCallback =
        //                    //delegate(object s, X509Certificate certificate,
        //                    //         X509Chain chain, SslPolicyErrors sslPolicyErrors)
        //                    //{ return true; };

        //                    ftp.EnableSsl = false;


        //                    FileStream fs = File.OpenRead(source + @"\" + file);
        //                    byte[] buffer = new byte[fs.Length];
        //                    fs.Read(buffer, 0, buffer.Length);
        //                    fs.Close();

        //                    Stream ftpstream = ftp.GetRequestStream();
        //                    ftpstream.Write(buffer, 0, buffer.Length);
        //                    ftpstream.Close();
        //                    File.Move(source + @"/" + file, movepath + @"/" + file);
        //                    //return true;
        //                }
        //                else
        //                {
        //                    response.Close();
        //                    //return false;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                //return false;
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        return false;
        //    }
        //}

        public bool UploadFileToFTP(string source, string ServerOutput, string destination, string username, string pass, int port, string movepath)
        {
            //string filename = Path.GetFileName(source);
            StringBuilder error = new StringBuilder();
            FtpWebRequest request; string Fname = "",foldername="";
            string[] filename = VA009_ICICI_Snorkel.GetFileNames(source);
            try
            {
                foreach (var file in filename)
                {
                    string ftpfullpath = destination;
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
                        if (ServerOutput != string.Empty)
                        {
                            if (ServerOutput == "HO")
                            {
                                foldername = "Upload";
                            }
                            else
                            {
                                int position = ServerOutput.LastIndexOf('\\');
                                string acc = ServerOutput.Substring(0, position);
                                foldername = ServerOutput.Substring(position + 1, ServerOutput.Length - (acc.Length + 1));
                                ftpfullpath = ftpfullpath + @"/Areas/VA009/VA009Docs/" + acc;
                            }
                        }
                        else
                            ftpfullpath = ftpfullpath + @"/Areas/VA009/VA009Docs";
                        VA009_ICICI_UploadFile.CreateFolderToFTP(ftpfullpath, username, pass);
                         Fname = source + "\\" + file;

                        FileInfo fileInfo = new FileInfo(Fname);

                        request = WebRequest.Create(new Uri(string.Format(@"{0}/{1}/{2}", ftpfullpath, foldername, file))) as FtpWebRequest;
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        request.UseBinary = true;
                        request.UsePassive = true;
                        request.KeepAlive = true;
                        request.Credentials = new NetworkCredential(username, pass);
                        request.ConnectionGroupName = "group";

                        int buffLength = 2048;
                        byte[] buff = new byte[buffLength];
                        int contentLen;
                        FileStream fileStream = fileInfo.OpenRead();//File.OpenRead(source + @"\" + file);
                        Stream stream = request.GetRequestStream();
                        contentLen = fileStream.Read(buff, 0, buffLength);
                        while (contentLen != 0)
                        {
                            stream.Write(buff, 0, contentLen);
                            contentLen = fileStream.Read(buff, 0, buffLength);
                        }
                        stream.Close();
                        fileStream.Close();

                        //Move in New Folder
                        File.Move(source + @"/" + file, movepath + @"/" + file);

                    }
                    catch (WebException ex)
                    {
                        FtpWebResponse response = (FtpWebResponse)ex.Response;
                        if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            FileInfo fileInfo = new FileInfo(Fname);

                            request = WebRequest.Create(new Uri(string.Format(@"{0}/{1}/{2}", ftpfullpath, foldername, file))) as FtpWebRequest;
                            request.Method = WebRequestMethods.Ftp.UploadFile;
                            request.UseBinary = true;
                            request.UsePassive = true;
                            request.KeepAlive = true;
                            request.Credentials = new NetworkCredential(username, pass);
                            request.ConnectionGroupName = "group";

                            int buffLength = 2048;
                            byte[] buff = new byte[buffLength];
                            int contentLen;
                            FileStream fileStream = fileInfo.OpenRead();
                            Stream stream = request.GetRequestStream();
                            contentLen = fileStream.Read(buff, 0, buffLength);
                            while (contentLen != 0)
                            {
                                stream.Write(buff, 0, contentLen);
                                contentLen = fileStream.Read(buff, 0, buffLength);
                            }
                            stream.Close();
                            fileStream.Close();

                            //Move in New Folder
                            File.Move(source + @"/" + file, movepath + @"/" + file);
                        }
                        else
                        {
                            response.Close();
                            //return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        error.Append("ErrorWhileUploadingFileOnFTP:" + ex.Message);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                error.Append("ErrorWhileUploadingFileOnFTP:" + ex.Message);
                return false;
            }
        }

    }
}
