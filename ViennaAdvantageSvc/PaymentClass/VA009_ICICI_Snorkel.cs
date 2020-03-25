using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Web.Hosting;
using VAdvantage.Model;
using VAdvantage.DataBase;
using System.Net;
using System.Web;
using System.IO;
using VAdvantage.Utility;
using VAdvantage.Model;
using ViennaAdvantage.Model;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using ViennaAdvantage.Process;

namespace ViennaAdvantage.PaymentClass
{
    public class VA009_ICICI_Snorkel
    {
        public object GetMethod(int VA009_Batch_ID, Ctx ctx, Trx GetTrx)
        {
            //VA009_DownloadAllHO u = new VA009_DownloadAllHO();
            //u.DownloadMethod(ctx, GetTrx);
            MVA009Batch _batch = new MVA009Batch(ctx, VA009_Batch_ID, GetTrx);
            string sql = string.Empty; string filename = string.Empty;
            string foldername = string.Empty; bool module = false;
            int _VA017 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA017_'  AND IsActive = 'Y'", null, GetTrx));
            if (_VA017 > 0)
            {
                foldername = Util.GetValueOfString(DB.ExecuteScalar("SELECT VA017_LocalOpFolder FROM VA017_ICICI_Payment WHERE C_BankAccount_ID=" + _batch.GetC_BankAccount_ID(), null, GetTrx));
                module = true;
            }
            if (module == true)
            {
                DataSet ds = new DataSet();
                sql = @"SELECT bac.accountno AS Batch_Account_NO,ICI.VA017_OutwardFilePrefix, ICI.VA017_ResponseFilePrefix,  cbp.accountno AS BP_Account_No,  cbp.a_name    AS Account_Name, 
                        bld.dueamt,  pm.VA009_TransferCode as va009_paymentbasetype,  to_date(sysdate,'DD-MON-YYYY')  AS DateTrx,  bank.routingno AS IFSC,  usr.phone ,  cbp.a_email AS Account_Email,
                        bld.c_invoicepayschedule_id,  cbp.a_street  ||cbp.a_city  || cbp.a_state  ||cbp.a_country AS address,  bl.va009_batchlines_id, 
                        bld.va009_batchlinedetails_ID ,  bld.discountamt,  inv.issotrx,  inv.isreturntrx,  bld.ad_org_id,  bld.ad_client_id ,  
                        doc.DocBaseType FROM va009_batchlinedetails bld INNER JOIN va009_batchlines bl ON bl.va009_batchlines_id=bld.va009_batchlines_id 
                        LEFT JOIN c_bp_bankaccount CBP ON cbp.c_bpartner_id=bl.c_bpartner_id INNER JOIN va009_batch b ON b.va009_batch_id =bl.va009_batch_id 
                        INNER JOIN va009_paymentmethod pm ON pm.va009_paymentmethod_id=bld.va009_paymentmethod_id INNER JOIN c_bankaccount bac ON 
                        bac.c_bankaccount_id=b.c_bankaccount_id LEFT JOIN C_Bank Bank ON Bank.c_bank_id=CBP.C_Bank_ID LEFT JOIN ad_user usr ON 
                        usr.c_bpartner_id=bl.c_bpartner_id INNER JOIN c_invoice inv ON inv.c_invoice_id = bld.c_invoice_id INNER JOIN VA017_ICICI_Payment ICI ON ICI.c_bankaccount_id =b.c_bankaccount_id INNER JOIN C_DocType doc ON 
                        doc.C_Doctype_ID= inv.C_Doctype_ID WHERE NVL(bl.c_payment_id , 0) = 0  AND NVL(bld.c_payment_id , 0)  = 0 AND b.va009_batch_id=" + _batch.GetVA009_Batch_ID();
                ds = null;
                ds = DB.ExecuteDataset(sql);
                decimal amt = 0;
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    StringBuilder stre = new StringBuilder();
                    string PaymentModeVal = "";
                    stre.Append("DebitAcNo|BeneficiaryAcNo|BeneficiaryName|Amount|PayMode|Date(DD-MMM-YYYY)|IFSC|BeneMobileNo|BeneEmail-Id|PaymnetDetail|BeneficiaryMailingAddress|CreditNarration|BatchDetailID|BankTrxID|PaymentStatus");
                    stre.AppendLine("");
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        if (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["dueamt"]) != 0)
                        {
                            filename = Util.GetValueOfString(ds.Tables[0].Rows[i]["VA017_OutwardFilePrefix"]) + Util.GetValueOfString(ds.Tables[0].Rows[i]["Batch_Account_NO"]);
                            if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "API" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARC")
                            {
                                if (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["dueamt"]) < 0)
                                {
                                    amt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["dueamt"]) * -1;
                                }
                                else
                                {
                                    amt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["dueamt"]);
                                }
                                if (Util.GetValueOfString(ds.Tables[0].Rows[i]["IFSC"]).Contains("ICIC"))
                                {
                                    PaymentModeVal = "I";
                                }
                                else
                                {
                                    PaymentModeVal = Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_paymentbasetype"]);
                                }

                                stre.AppendLine(ds.Tables[0].Rows[i]["Batch_Account_NO"] + "|" + ds.Tables[0].Rows[i]["BP_Account_No"] +
                                         "|" + ds.Tables[0].Rows[i]["Account_Name"] + "|" + amt +
                                         "|" + PaymentModeVal + "|" + String.Format("{0:dd/MM/yy}", ds.Tables[0].Rows[i]["DateTrx"]) +
                                         "|" + ds.Tables[0].Rows[i]["IFSC"] + "|" + ds.Tables[0].Rows[i]["phone"] +
                                         "|" + ds.Tables[0].Rows[i]["Account_Email"] + "|" + ds.Tables[0].Rows[i]["c_invoicepayschedule_id"] +
                                         "|" + ds.Tables[0].Rows[i]["address"] + "|" + ds.Tables[0].Rows[i]["va009_batchlines_id"] +
                                         "|" + ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]);
                            }
                            else if (Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "ARI" || Util.GetValueOfString(ds.Tables[0].Rows[i]["DocBaseType"]) == "APC")
                            {
                                if (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["dueamt"]) < 0)
                                {
                                    amt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["dueamt"]) * -1;
                                }
                                else
                                {
                                    amt = Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["dueamt"]);
                                }
                                if (Util.GetValueOfString(ds.Tables[0].Rows[i]["IFSC"]) == "ICIC")
                                {
                                    PaymentModeVal = "I";
                                }
                                else
                                {
                                    PaymentModeVal = Util.GetValueOfString(ds.Tables[0].Rows[i]["va009_paymentbasetype"]);
                                }
                                stre.AppendLine(ds.Tables[0].Rows[i]["BP_Account_No"] + "|" + ds.Tables[0].Rows[i]["Batch_Account_NO"] +
                                         "|" + ds.Tables[0].Rows[i]["Account_Name"] + "|" + amt +
                                         "|" + PaymentModeVal + "|" + String.Format("{0:dd/MM/yy}", ds.Tables[0].Rows[i]["DateTrx"]) +
                                         "|" + ds.Tables[0].Rows[i]["IFSC"] + "|" + ds.Tables[0].Rows[i]["phone"] +
                                         "|" + ds.Tables[0].Rows[i]["Account_Email"] + "|" + ds.Tables[0].Rows[i]["c_invoicepayschedule_id"] +
                                         "|" + ds.Tables[0].Rows[i]["address"] + "|" + ds.Tables[0].Rows[i]["va009_batchlines_id"] +
                                         "|" + ds.Tables[0].Rows[i]["va009_batchlinedetails_ID"]);
                            }
                        }
                    }
                    filename += "_" + _batch.GetVA009_Batch_ID() + "_" + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + "_" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second;
                    CreateLocalCSVFile(stre.ToString(), filename, foldername, module);
                }
                return Msg.GetMsg(ctx, "VA009_FileCreated");
            }
            else
                return Msg.GetMsg(ctx, "VA009_ICICIModuleNotInstalled");
        }

        public static string[] GetFileNames(string path)
        {
            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }
            return files;
        }

        public static void CreateLocalCSVFile(string str, string filename, string folder, bool IsModuleDownloaded)
        {
            string path = string.Empty;
            if (!IsModuleDownloaded)
                folder = @"\Areas\VA009\VA009Docs\Output";
            else
            {
                if (folder == string.Empty)
                    folder = @"\Areas\VA009\VA009Docs\Output";
                else
                {
                    string crpath = folder;
                    folder = @"\Areas\VA009\VA009Docs\";
                    folder += crpath;
                }
            }

            if (!Directory.Exists(HostingEnvironment.ApplicationPhysicalPath + folder))
            {
                Directory.CreateDirectory(HostingEnvironment.ApplicationPhysicalPath + folder);
                Directory.CreateDirectory(HostingEnvironment.ApplicationPhysicalPath + folder + @"\Sent");
            }
            path = HostingEnvironment.ApplicationPhysicalPath + folder + @"\" + filename + ".csv";
            FileStream fs1E = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw1E = new StreamWriter(fs1E);
            sw1E.BaseStream.Seek(0, SeekOrigin.End);
            sw1E.WriteLine(str);
            sw1E.Flush();
            sw1E.Close();
        }

    }

   public class VA009_ICICI_UploadFile
   {
       public static bool CreateFolderToFTP(string ftpfullpath,string username,string pass )
       {
           bool status=VA009_ICICI_UploadFile.DirectoryExistsOnFTP(ftpfullpath, username, pass);
           if (!status)
           {
               FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);
               ftp.Method = WebRequestMethods.Ftp.MakeDirectory;
               ftp.Credentials = new NetworkCredential(username, pass);
               WebResponse response = ftp.GetResponse();
               return true;
           }
           else
               return true;
       }

       public static bool DirectoryExistsOnFTP(string ftpfullpath, string username, string pass)
       {
           bool count = false;
           try
           {
               FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);
               ftpRequest.Credentials = new NetworkCredential(username, pass);
               ftpRequest.KeepAlive = true;
               ftpRequest.UseBinary = true;
               ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
               //ServicePointManager.ServerCertificateValidationCallback =
               //delegate(object s, X509Certificate certificate,
               //     X509Chain chain, SslPolicyErrors sslPolicyErrors)
               //{ return true; };
               //ftpRequest.EnableSsl = false;
               
               using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
               {
                   count= true;
               }
           }
           catch (WebException ex)
           {
               if (ex.Response != null)
               {
                   FtpWebResponse response = (FtpWebResponse)ex.Response;
                   if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                   {
                       count= false;  
                   }
               }
           }
           return count;
       }

       public static string[] ListDirectory(string ftpfullpath, string username, string pass)
       {
           var list = new List<string>();
           FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);
           ftpRequest.Credentials = new NetworkCredential(username, pass);
           ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;

           using (var response = (FtpWebResponse)ftpRequest.GetResponse())
           {
               using (var stream = response.GetResponseStream())
               {
                   using (var reader = new StreamReader(stream, true))
                   {
                       while (!reader.EndOfStream)
                       {
                           list.Add(reader.ReadLine());
                       }
                   }
               }
           }

           return list.ToArray();
       }

   }
}
