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
            DataAccess dataAccess;
            DataTable results = null;
            DataRow row;
            DateTime startingWeek = new DateTime(2016, 1, 1);
            DateTime endingWeek = DateTime.Now;
            DateTime currWeek;

            dataAccess = new DataAccess(base.Context); //Context is inherited
            results = new DataTable("Results");
            results.Columns.Add(new DataColumn("ue_CLM_Weekending", System.Type.GetType("System.DateTime")));

            //We are going to loop through from starting
            //to ending adding 7 days and adding to results.
            startingWeek = this.GetLastDayOfWeek(startingWeek);
            endingWeek = this.GetLastDayOfWeek(endingWeek);
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
            DataAccess dataAccess;
            DataTable results = null;
            
            DateTime startingWeek = new DateTime(2022, 1, 1);
            DateTime endingWeek = DateTime.Now;
            DateTime currWeek;
            
            dataAccess = new DataAccess(base.Context); //Context is inherited

            results = SetupDataTable(); //Encapsulated the DT setup for readability.

            //We are going to loop through from starting
            //to ending adding 7 days and adding to results.
            startingWeek = this.GetLastDayOfWeek(startingWeek);
            endingWeek = this.GetLastDayOfWeek(endingWeek);
            currWeek = endingWeek;
            while (currWeek >= startingWeek)
            {
                results.Rows.Add(CreateNewRow(dataAccess,results, currWeek));

                currWeek = currWeek.AddDays(-7);
            }

            return results;
        }

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

        private DataTable SetupDataTable()
        {
            DataTable results = null;

            results = new DataTable("Results");
            results.Columns.Add(new DataColumn("ue_CLM_Weekending", System.Type.GetType("System.DateTime")));

            results.Columns.Add(new DataColumn("ue_CLM_AirlessBlasting", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_Blasting", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_BlastingSupplies", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_Chemtrol", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_ChemtrolWaste", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_FinishingEquipment", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_FinishingSupplies", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_MiscEquip", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_Rosler", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_Tumbling", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_WashingEquipment", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_ZeroClemco", System.Type.GetType("System.Decimal")));

            return results;
        }

        private DataRow CreateNewRow(DataAccess dataAccess, DataTable dataTable, DateTime week)
        {
            DataRow row;
            ue_PFI_FamilyCodeCategories[] fcc = null;

            fcc = dataAccess.ue_PFI_FamilyCodeCategories();

            row = dataTable.NewRow();
            row["ue_CLM_Weekending"] = week.Date;

            row["ue_CLM_AirlessBlasting"] = GetCategoryTotal(fcc.Where(w=>w.FamilyCodeCategory.Equals("Airless Blasting")).FirstOrDefault(), dataAccess, week);
            row["ue_CLM_Blasting"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Blasting")).FirstOrDefault(), dataAccess, week);
            row["ue_CLM_BlastingSupplies"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Blast Supplies")).FirstOrDefault(), dataAccess, week);
            row["ue_CLM_Chemtrol"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Chemtrol")).FirstOrDefault(), dataAccess, week);
            row["ue_CLM_ChemtrolWaste"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Chemtrol Waste")).FirstOrDefault(), dataAccess, week);
            row["ue_CLM_FinishingEquipment"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Finishing Equipment")).FirstOrDefault(), dataAccess, week);
            row["ue_CLM_FinishingSupplies"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Finishing Supplies")).FirstOrDefault(), dataAccess, week);
            row["ue_CLM_MiscEquip"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Misc Equip.")).FirstOrDefault(), dataAccess, week);
            row["ue_CLM_Rosler"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Rosler")).FirstOrDefault(), dataAccess, week);
            row["ue_CLM_Tumbling"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Tumbling")).FirstOrDefault(), dataAccess, week);
            row["ue_CLM_WashingEquipment"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Washing Equipment")).FirstOrDefault(), dataAccess, week);
            row["ue_CLM_ZeroClemco"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Zero / Clemco")).FirstOrDefault(), dataAccess, week);

            return row;
        }

        private decimal GetCategoryTotal(ue_PFI_FamilyCodeCategories familyCodeCategory, DataAccess dataAccess, DateTime week)
        {
            if(familyCodeCategory.BookingInvoiceCode.Equals("B"))
                return 40;
            else if (familyCodeCategory.BookingInvoiceCode.Equals("I"))
                return 41;
            else
                return 42;
        }

    }
}
