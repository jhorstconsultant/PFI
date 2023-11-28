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
using static Mongoose.Forms.ExportSettings;

namespace PFI.Reporting
{
    
    
    [IDOExtensionClass("PFI_WeeklySalesReport")]
    public class PFI_WeeklySalesReport : IDOExtensionClass
    {
        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable PFI_GetWeekendings()
        {
            WeeklySalesReportBL bl;
            DataTable results = null;
            DataRow row;
            DateTime startingWeek = new DateTime(2016, 1, 1);
            DateTime endingWeek = DateTime.Now;
            DateTime currWeek;

            bl = new WeeklySalesReportBL(base.Context);

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


        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable PFI_GetReport()
        {
            WeeklySalesReportBL bl;
            DataTable results = null;
            
            DateTime startingWeek = new DateTime(2023, 3, 21);
            DateTime endingWeek = DateTime.Now;
            DateTime currWeek;
            
            bl = new WeeklySalesReportBL(base.Context);

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
    }
}
