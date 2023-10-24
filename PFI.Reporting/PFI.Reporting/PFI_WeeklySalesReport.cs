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
using Mongoose.IDO.Protocol;
using PFI.Reporting.DA;
using PFI.Reporting.Models;
using static Mongoose.Core.Common.QuickKeywordParser;

namespace PFI.Reporting
{
    
    
    [IDOExtensionClass("PFI_WeeklySalesReport")]
    public class PFI_WeeklySalesReport : IDOExtensionClass
    {
        private DateTime GetLastDayOfWeek(DateTime date) 
        {
            /*
                Friday	5	
                Indicates Friday.

                Monday	1	
                Indicates Monday.

                Saturday	6	
                Indicates Saturday.

                Sunday	0	
                Indicates Sunday.

                Thursday	4	
                Indicates Thursday.

                Tuesday	2	
                Indicates Tuesday.

                Wednesday	3	
                Indicates Wednesday.
             */

            DayOfWeek dow = date.DayOfWeek;  //Get the week number enum

            int diff = 6 - (int)dow; //Since 6 = Sat we want to take 6-the current day of the week.
            return date.AddDays(diff); //Add the necessary days to get to the end of the week.
        }

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable PFI_GetWeekendings()
        {
            DataAccess dataAccess;
            DataTable results = null;
            DataRow row;
            DateTime startingWeek = new DateTime(2016, 1, 1);
            DateTime endingWeek = DateTime.Now;
            DateTime currWeek;

            dataAccess = new DataAccess(base.Context); //Context is inherited
            results = new DataTable("Results");
            results.Columns.Add(new DataColumn("CLM_Weekending", System.Type.GetType("System.DateTime")));

            //We are going to loop through from starting
            //to ending adding 7 days and adding to results.
            startingWeek = this.GetLastDayOfWeek(startingWeek);
            endingWeek = this.GetLastDayOfWeek(endingWeek);
            currWeek = endingWeek;
            while (currWeek >= startingWeek) 
            {
                row = results.NewRow();
                row["CLM_Weekending"] = currWeek.Date;
                results.Rows.Add(row);

                currWeek = currWeek.AddDays(-7);
            }

            return results;
        }
    }
}
