using Mongoose.Forms;
using Mongoose.IDO;
using Newtonsoft.Json;
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
        public WeeklySalesReportBL(IIDOExtensionClassContext context) 
        {
            DataAccess = new PFIDataAccess(context); //Context is inherited
            SiteRef = DataAccess.GetCurrentSite();
            Actuals = DataAccess.ue_PFI_GrossProfitReportSale(SiteRef);
            Bookings = DataAccess.SLCoItems();
            FamilyCodeCategories = GetFamilyCodeCategories();
        }
        public string SiteRef { get; set; }
        public PFIDataAccess DataAccess { get; set; }
        public ue_PFI_GrossProfitReportSale[] Actuals { get; set; }
        public SLCoItems[] Bookings { get; set; }
        public Tuple<string,string>[] FamilyCodeCategories { get; set; }
        public DateTime GetLastDayOfWeek(DateTime date)
        {
            DateTime result = new DateTime(date.Year, date.Month, date.Day);
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

            DayOfWeek dow = result.DayOfWeek;  //Get the week number enum

            int diff = 6 - (int)dow; //Since 6 = Sat we want to take 6-the current day of the week.
            result = result.AddDays(diff); //Add the necessary days to get to the end of the week.
            return new DateTime(result.Year, result.Month, result.Day);
        }
        public Tuple<string, string>[] GetFamilyCodeCategories()
        {
            ue_PFI_SalespersonFCBudgetAll[] budgets;

            budgets = DataAccess.ue_PFI_SalespersonFCBudgetAll(SiteRef);

            return budgets.Select(s=>new Tuple<string,string>(s.FamilyCode,s.FamilyCodeCategory)).Distinct().ToArray();
        }
        public DataTable SetupDataTable()
        {
            DataTable results;

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
        public DataRow CreateNewRow(DataTable dataTable, DateTime week)
        {
            DataRow row;
            ue_PFI_FamilyCodeCategories[] fcc = null;

            fcc = DataAccess.ue_PFI_FamilyCodeCategories();

            row = dataTable.NewRow();
            row["ue_CLM_Weekending"] = week.Date;

            row["ue_CLM_AirlessBlasting"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Airless Blasting")).FirstOrDefault(), week);
            row["ue_CLM_Blasting"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Blasting")).FirstOrDefault(), week);
            row["ue_CLM_BlastingSupplies"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Blast Supplies")).FirstOrDefault(), week);
            row["ue_CLM_Chemtrol"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Chemtrol")).FirstOrDefault(), week);
            row["ue_CLM_ChemtrolWaste"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Chemtrol Waste")).FirstOrDefault(), week);
            row["ue_CLM_FinishingEquipment"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Finishing Equipment")).FirstOrDefault(), week);
            row["ue_CLM_FinishingSupplies"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Finishing Supplies")).FirstOrDefault(), week);
            row["ue_CLM_MiscEquip"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Misc Equip.")).FirstOrDefault(), week);
            row["ue_CLM_Rosler"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Rosler")).FirstOrDefault(), week);
            row["ue_CLM_Tumbling"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Tumbling")).FirstOrDefault(), week);
            row["ue_CLM_WashingEquipment"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Washing Equipment")).FirstOrDefault(), week);
            row["ue_CLM_ZeroClemco"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("Zero / Clemco")).FirstOrDefault(), week);

            return row;
        }
        public decimal GetCategoryTotal(ue_PFI_FamilyCodeCategories familyCodeCategory, DateTime week)
        {
            decimal result = -1;
            List<SLCoItems> debug = new List<SLCoItems>();

            week = new DateTime(week.Year, week.Month, week.Day);

            if (familyCodeCategory.BookingInvoiceCode == "I")
            {
                result = 0;
                foreach (string fc in FamilyCodeCategories
                    .Where(w=>w.Item2.Trim().ToLower().Equals(familyCodeCategory.FamilyCodeCategory.Trim().ToLower()))
                    .Select(s=>s.Item1.Trim().ToLower()))
                {
                    //result += 1;
                    result += Actuals.Where(w => w.DerFamilyCode.Trim().ToLower().Equals(fc)
                        && GetLastDayOfWeek(w.InvoiceDate.Date) == week.Date
                        ).Select(s => s.DerExtendedPrice).Sum();
                }
            }
            else if (familyCodeCategory.BookingInvoiceCode == "B")
            {
                result = 0;

                result += Bookings.Where(w =>
                        w.ue_PFI_FamilyCodeCategory.Trim().ToLower().Equals(familyCodeCategory.FamilyCodeCategory.Trim().ToLower())
                        && GetLastDayOfWeek(w.CoOrderDate).Date == week.Date
                        )
                    .Select(s => s.DerNetPrice).Sum();
            }
            else
            {
                if (SiteRef.Equals("PRECISIO"))
                {
                    result = 2;
                }
                else if (SiteRef.Equals("CHECKERS"))
                {
                    result = 12;
                }
                else
                {
                    result = 42;
                }
            }

            return result;
        }
    }
}
