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
using VAdvantage.ProcessEngine;

namespace ViennaAdvantage.Process
{
   public class VA009_ReadAllFiles : SvrProcess
    {
        protected override string DoIt()
        {
           // GetDataTabletFromCSVFile();
            return "";
            //throw new NotImplementedException();
        }

        protected override void Prepare()
        {
           // throw new NotImplementedException();
        }

        public static DataTable GetDataTabletFromCSVFile(string csv_file_path)
        {
            DataTable csvData = new DataTable();
            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                {
                    csvReader.SetDelimiters(new string[] { "|" });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            if (csvData!= null && csvData.Rows.Count > 0)
            {
                for (int i = 0; i < csvData.Rows.Count; i++)
                {
                    int Batchdtlline_Id = Util.GetValueOfInt(csvData.Rows[i]["BatchDetailID"]);
                    string status = Util.GetValueOfString(csvData.Rows[i]["PaymentStatus"]);
                    int count=DB.ExecuteQuery("UPDATE va009_batchlinedetails set va009_bankresponse=" + status + " WHERE va009_batchlinedetails_id=" + Batchdtlline_Id, null, null);
                }
            }
            return csvData;
        }
    }
}
