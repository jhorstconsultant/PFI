using PFI.Reporting.DA;
using PFI.Reporting.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFI.Reporting.BL
{
    public class WeeklySalesReportBL
    {
        public DateTime GetLastDayOfWeek(DateTime date)
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

        public DataTable SetupDataTable()
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

        public DataRow CreateNewRow(DataAccess dataAccess, DataTable dataTable, DateTime week)
        {
            DataRow row;
            ue_PFI_FamilyCodeCategories[] fcc = null;

            fcc = dataAccess.ue_PFI_FamilyCodeCategories();

            row = dataTable.NewRow();
            row["ue_CLM_Weekending"] = week.Date;

            row["ue_CLM_AirlessBlasting"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Airless Blasting")).FirstOrDefault(), dataAccess, week);
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

        public decimal GetCategoryTotal(ue_PFI_FamilyCodeCategories familyCodeCategory, DataAccess dataAccess, DateTime week)
        {
            if (familyCodeCategory.BookingInvoiceCode.Equals("B"))
                return 40;
            else if (familyCodeCategory.BookingInvoiceCode.Equals("I"))
                return 41;
            else
                return 42;
        }
    }
}
