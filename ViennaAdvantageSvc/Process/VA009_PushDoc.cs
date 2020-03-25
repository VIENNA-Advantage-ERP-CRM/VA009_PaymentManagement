using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAdvantage.Model;
using VAdvantage.ProcessEngine;
using System.Data;
using VAdvantage.DataBase;
using ViennaAdvantage.Model;
using System.Text.RegularExpressions;
using VAdvantage.Utility;
using VAdvantage.Logging;
using System.Net;
using System.IO;
//using ViennaAdvantage.VA007;
using System.ServiceModel;
//using EDIFACT;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace ViennaAdvantage.Process
{
    public class VA009_PushDoc
    {
        public string DoIt()
        {
            if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Areas/VA009/VA009Docs/Output")))
            {
                Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("~/Areas/VA009/VA009Docs/Output"));       //Create Thumbnail Folder if doesnot exists
            }
            string path = System.Web.HttpContext.Current.Server.MapPath("~/Areas/VA009/VA009Docs/Output");
            string[] filenames = GetFileNames(path);
            foreach (var file in filenames)
            {
               //EDIMessage msg = null;
                string sourcepath = @"" + path + "/" + file;
                try
                {
                    string _sql = "select hostaddress,hostport,userid,password from c_paymentprocessor where c_paymentprocessor_id=1000000";

                    DataSet ds = null;
                    List<ftp> ftps = new List<ftp>();
                    try
                    {
                        ds = DB.ExecuteDataset(_sql);
                        if (ds != null)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                {
                                    ftp ft = new ftp();
                                    //destination = Util.GetValueOfString(ds.Tables[0].Rows[i]["va007_host"]);
                                    //usename = Util.GetValueOfString(ds.Tables[0].Rows[i]["va007_account"]);
                                    //pass = Util.GetValueOfString(ds.Tables[0].Rows[i]["va007_password"]);
                                    //port = Util.GetValueOfInt(ds.Tables[0].Rows[i]["VA007_PORTNO"]);
                                    ft.destination = Util.GetValueOfString(ds.Tables[0].Rows[i]["hostaddress"]);
                                    ft.username = Util.GetValueOfString(ds.Tables[0].Rows[i]["userid"]);
                                    ft.pass = Util.GetValueOfString(ds.Tables[0].Rows[i]["password"]);
                                    ft.port = Util.GetValueOfInt(ds.Tables[0].Rows[i]["hostport"]);
                                    ftps.Add(ft);
                                }
                            }
                        }
                        if (ds != null) { ds.Dispose(); }
                    }
                    catch
                    {
                        if (ds != null) { ds.Dispose(); }
                        continue;
                    }
                    foreach (var ft in ftps)
                    {
                        if (sourcepath == "" || ft.destination == "" || ft.username == "" || ft.pass == "")
                        {
                            continue;
                        }
                        bool status = UploadFileToFTP(sourcepath, ft.destination, ft.username, ft.pass, ft.port);
                        if (status)
                        {
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            string FilePath = Path.Combine(@"" + path + "/" + file);
                            if (!Directory.Exists(path + "/Sent"))
                            {
                                Directory.CreateDirectory(path + "/Sent");
                            }
                            if (File.Exists(@"" + path + "/Sent/" + file))
                            {
                                File.Delete(@"" + path + "/Sent/" + file);
                            }
                            File.Create(@"" + path + "/Sent/" + file).Close();
                            byte[] fileBytes = File.ReadAllBytes(FilePath);

                            using (FileStream fs = new FileStream(@"" + path + "/Sent/" + file, FileMode.Open,
                       FileAccess.ReadWrite, FileShare.Write))
                            {
                                fs.Seek(0, SeekOrigin.Begin);
                                fs.Write(fileBytes, 0, fileBytes.Length);
                            }
                            if (File.Exists(FilePath))
                            {
                                File.Delete(FilePath);
                            }
                            break;

                        }
                    }
                    

                }
                catch (Exception)
                {

                }
                //   EDIMessage msg = new EDIMessage(@"D:\EDIDoc.txt");

                //var abc= msg.
            }
            return "";



        }
        
        public static void LogEntry(string str, string srv)
        {

            FileStream fs1E = new FileStream(srv, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw1E = new StreamWriter(fs1E);
            sw1E.BaseStream.Seek(0, SeekOrigin.End);
            sw1E.WriteLine(str);
            sw1E.Flush();
            sw1E.Close();

        }
        
        private static string[] GetFileNames(string path)
        {

            // if (Directory.Exists(Path.Combine(Server.MapPath("~/Apk"), "Downloads")))
            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }
            return files;
        }

        private static bool UploadFileToFTP(string source, string destination, string username, string pass, int port)
        {
            string filename = Path.GetFileName(source);
            string ftpfullpath = destination;
            try
            {
                if (port > 0)
                {
                    ftpfullpath = ftpfullpath + ":" + port.ToString();
                }
                if (ftpfullpath.IndexOf("ftp://")==-1)
                {
                    ftpfullpath = "ftp://" + ftpfullpath ;    
                }

                ftpfullpath = ftpfullpath + "/" + filename;
                FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);
                ftp.Credentials = new NetworkCredential(username, pass);

                ftp.KeepAlive = true;
                ftp.UseBinary = true;
                ftp.Method = WebRequestMethods.Ftp.UploadFile;
                ServicePointManager.ServerCertificateValidationCallback =
    delegate(object s, X509Certificate certificate,
             X509Chain chain, SslPolicyErrors sslPolicyErrors)
    { return true; };
                ftp.EnableSsl = true;
                if (File.Exists(ftpfullpath))
                {
                    File.Delete(ftpfullpath);
                }
                FileStream fs = File.OpenRead(source);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();

                Stream ftpstream = ftp.GetRequestStream();
                ftpstream.Write(buffer, 0, buffer.Length);
                ftpstream.Close();
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
                    ftp.Method = WebRequestMethods.Ftp.UploadFile;
                    ServicePointManager.ServerCertificateValidationCallback =
   delegate(object s, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
   { return true; };

                    ftp.EnableSsl = true;


                    FileStream fs = File.OpenRead(source);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();

                    Stream ftpstream = ftp.GetRequestStream();
                    ftpstream.Write(buffer, 0, buffer.Length);
                    ftpstream.Close();

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
            return true;
        }


    }

    public class ftp
    {
        public string destination { get; set; }
        public string username { get; set; }
        public string pass { get; set; }
        public int port { get; set; }
       
    }
}
