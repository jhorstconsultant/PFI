using Mongoose.IDO;
using Mongoose.IDO.Protocol;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PFI.Reporting;

namespace PFI.Reporting.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string slserver = "https://csi10a.erpsl.inforcloudsuite.com/IDORequestService/RequestService.aspx";

            Client oclient = new Client(slserver, IDOProtocol.Http);
            DataTable dtResults = new DataTable();

            try
            {
                OpenSessionResponseData res = oclient.OpenSession("PFICATestUser", "K39FarpkgAfvCEDP5uTzZe", "PRECISIONFINISHI_PRD_PRECISIO");

                if (res.Result.ToString() != "Success") { throw new Exception(res.Result.ToString()); }

                string html = "";
                string ib = "";
                int result;

                //result = PFI_WeeklySalesReport.Process_PFI_SalespersonEmail(oclient, "3A", out html, out ib);
                dtResults = PFI_WeeklySalesReport.Process_PFI_GetReport(oclient,
                    new DateTime(2024, 3, 30),
                    new DateTime(2024, 3, 30),
                    "3A", "3A");

                Console.WriteLine(html);

                Console.WriteLine(ib);

                //Console.WriteLine(result);

                //Console.WriteLine(dtResults.Rows.Count);
                //
                //string curr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                //
                //output_results(dtResults, "C:\\temp\\output_" + curr + ".csv");
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
            finally
            {
                oclient.CloseSession();
                Console.Read();
            }
        }

        static void output_results(DataTable data, string path)
        {
            StringBuilder sb = new StringBuilder();

            foreach (DataColumn col in data.Columns)
            {
                sb.Append(col.ColumnName + ',');
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append(Environment.NewLine);

            foreach (DataRow row in data.Rows)
            {
                foreach (DataColumn col in data.Columns)
                {
                    sb.Append(row[col].ToString().Replace(",", "|") + ",");
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append(Environment.NewLine);
            }

            StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8);

            sw.WriteLine(sb);
            sw.Close();
        }
    }
}

