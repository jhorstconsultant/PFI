using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.SqlServer.Server;
using Mongoose.IDO;
using Mongoose.IDO.Metadata;
using Mongoose.IDO.Protocol;
using PFI.Reporting.BL;
using PFI.Reporting.DA;
using PFI.Reporting.Models;
using static Mongoose.Core.Common.QuickKeywordParser;
//using static Mongoose.Forms.ExportSettings;

namespace PFI.Reporting
{


    [IDOExtensionClass("PFI_WeeklySalesReport")]
    public class PFI_WeeklySalesReport : IDOExtensionClass
    {
        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable PFI_GetWeekendings()
        {
            return Process_PFI_GetWeekendings(base.Context.Commands);
        }

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable PFI_GetReport(DateTime startingWeek, DateTime endingWeek, string startingSalesPerson, string endingSalesPerson)
        {
            return Process_PFI_GetReport(base.Context.Commands, startingWeek, endingWeek, startingSalesPerson, endingSalesPerson);
        }

        [IDOMethod(MethodFlags.None, "Infobar")]
        public int PFI_SalespersonEmail(string salesPerson, out string html, out string Infobar)
        {
            return Process_PFI_SalespersonEmail(base.Context.Commands, salesPerson, out html, out Infobar);
        }

        [IDOMethod(MethodFlags.None, "Infobar")]
        public int PFI_SalespersonAllEmail(out string html, out string Infobar)
        {
            return Process_PFI_SalespersonAllEmail(base.Context.Commands, out html, out Infobar);
        }

        #region "Process"
        public static DataTable Process_PFI_GetWeekendings(IIDOCommands context)
        {
            WeeklySalesReportBL bl;
            DataTable results = null;
            DataRow row;
            DateTime startingWeek = new DateTime(2016, 1, 1);
            DateTime endingWeek = DateTime.Now;
            DateTime currWeek;

            bl = new WeeklySalesReportBL(context);

            results = new DataTable("Results");
            results.Columns.Add(new DataColumn("ue_CLM_Weekending", System.Type.GetType("System.DateTime")));

            //We are going to loop through from starting
            //to ending adding 7 days and adding to results.
            startingWeek = bl.GetLastDayOfWeek(startingWeek);
            endingWeek = bl.GetLastDayOfWeek(endingWeek);
            currWeek = endingWeek;
            while (currWeek >= startingWeek)
            {
                row = results.NewRow();
                row["ue_CLM_Weekending"] = currWeek.Date;
                results.Rows.Add(row);

                currWeek = currWeek.AddDays(-7);
            }

            return results;
            
        }

        public static DataTable Process_PFI_GetReport(IIDOCommands context, DateTime startingWeek, DateTime endingWeek, string startingSalesPerson, string endingSalesPerson)
        {
            WeeklySalesReportBL bl;
            DataTable results = null;
            DateTime currWeek;

            bl = new WeeklySalesReportBL(context);

            bl.SetSalesPersonRange(startingSalesPerson, endingSalesPerson);

            results = bl.SetupDataTable(); //Encapsulated the DT setup for readability.

            //We are going to loop through from starting
            //to ending adding 7 days and adding to results.
            startingWeek = bl.GetLastDayOfWeek(startingWeek);
            endingWeek = bl.GetLastDayOfWeek(endingWeek);
            currWeek = endingWeek;
            while (currWeek >= startingWeek)
            {
                results.Rows.Add(bl.CreateNewRow(results, currWeek));

                currWeek = currWeek.AddDays(-7);
            }

            return results;
        }

        public static int Process_PFI_SalespersonAllEmail(IIDOCommands context, out string html, out string Infobar)
        {
            int result = 0;
            Infobar = String.Empty;
            html = String.Empty;
            WeeklySalesReportBL bl;
            DateTime lastWeek;

            try
            {
                lastWeek = DateTime.Now.AddDays(-7);
                bl = new WeeklySalesReportBL(context);

                html = bl.GetSalespersonEmailBody(lastWeek);
            }
            catch (Exception ex)
            {
                Infobar = ex.Message;
                result = 16;
            }

            return result;
        }

        public static int Process_PFI_SalespersonEmail(IIDOCommands context, string salesPerson, out string html, out string Infobar)
        {
            int result = 0;
            Infobar = String.Empty;
            html = String.Empty;
            WeeklySalesReportBL bl;
            DateTime lastWeek;

            try
            {
                lastWeek = DateTime.Now.AddDays(-7);
                bl = new WeeklySalesReportBL(context);

                html = bl.GetSalespersonEmailBody(lastWeek, salesPerson);
            }
            catch (Exception ex)
            {
                Infobar = ex.Message;
                result = 16;
            }

            return result;
        }

        #endregion

    }
}
