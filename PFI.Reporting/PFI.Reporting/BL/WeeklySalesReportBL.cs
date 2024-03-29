﻿using Mongoose.Forms;
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
            FiscalYearPeriods = DataAccess.FiscalYearPeriods();
            Actuals = DataAccess.ue_PFI_GrossProfitReportSale(SiteRef);
            ActualOverrides = GetActualOverrides(SiteRef);
            Bookings = DataAccess.SLCoItems();
            FamilyCodeCategories = GetFamilyCodeCategories();
        }

        public void SetSalesPersonRange(string startingSalesPerson, string endingSalesPerson)
        {
            SalesPersions = DataAccess.SLSlsmanAlls(SiteRef);
            StartingSalesPerson = startingSalesPerson;
            EndingSalesPerson = endingSalesPerson;

            if (string.IsNullOrWhiteSpace(StartingSalesPerson))
                StartingSalesPerson = SalesPersions.Select(s => s.Slsman).Min();

            if (string.IsNullOrWhiteSpace(EndingSalesPerson))
                EndingSalesPerson = SalesPersions.Select(s => s.Slsman).Max();
        }

        public SLSlsmanAll[] SalesPersions { get; set; }
        public string StartingSalesPerson { get; set; }
        public string EndingSalesPerson { get; set; }
        public string SiteRef { get; set; }
        public PFIDataAccess DataAccess { get; set; }
        public FiscalYearPeriods[] FiscalYearPeriods { get; set; }
        public ue_PFI_GrossProfitReportSale[] Actuals { get; set; }
        public ue_PFI_SPFCActOverrideAll[] ActualOverrides { get; set; }
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

            results.Columns.Add(new DataColumn("ue_CLM_Tumbling", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_Blasting", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_Chemtrol", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_VibratoryMedia", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_BlastMedia", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_SpareParts", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_SupplierCompounds", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_VibratoryEquipment", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_BlastEquipment", System.Type.GetType("System.Decimal")));
            results.Columns.Add(new DataColumn("ue_CLM_Equipment", System.Type.GetType("System.Decimal")));

            return results;
        }
        public DataRow CreateNewRow(DataTable dataTable, DateTime week)
        {
            DataRow row;
            ue_PFI_FamilyCodeCategories[] fcc = null;

            fcc = DataAccess.ue_PFI_FamilyCodeCategories();

            row = dataTable.NewRow();
            row["ue_CLM_Weekending"] = week.Date;

            row["ue_CLM_Tumbling"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("TUMBLING")).FirstOrDefault(), week);
            row["ue_CLM_Blasting"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("BLASTING")).FirstOrDefault(), week);
            row["ue_CLM_Chemtrol"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("CHEMTROL")).FirstOrDefault(), week);
            row["ue_CLM_VibratoryMedia"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("VIBRATORY MEDIA")).FirstOrDefault(), week);
            row["ue_CLM_BlastMedia"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("BLAST MEDIA")).FirstOrDefault(), week);
            row["ue_CLM_SpareParts"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("SPARE PARTS")).FirstOrDefault(), week);
            row["ue_CLM_SupplierCompounds"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("SUPPLIER COMPOUNDS")).FirstOrDefault(), week);
            row["ue_CLM_VibratoryEquipment"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("VIBRATORY EQUIPMENT")).FirstOrDefault(), week);
            row["ue_CLM_BlastEquipment"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("BLAST EQUIPMENT")).FirstOrDefault(), week);
            row["ue_CLM_Equipment"] = GetCategoryTotal(fcc.Where(w => w.FamilyCodeCategory.Equals("EQUIPMENT")).FirstOrDefault(), week);

            return row;
        }
        public decimal GetCategoryTotal(ue_PFI_FamilyCodeCategories familyCodeCategory, DateTime week)
        {
            decimal result = -1;
            decimal? actualOverride;

            week = new DateTime(week.Year, week.Month, week.Day);

            //Check for an override senario.  Per Sherry B if an override exists even booking driven
            //  family code categories will be overridden.  
            actualOverride = ActualOverrides
                .Where(w => 
                    w.FamilyCodeCategory.Trim().ToLower().Equals(familyCodeCategory.FamilyCodeCategory.Trim().ToLower())
                    && w.FiscalPeriodEnd == week)
                .Where(w => w.SalesPerson.CompareTo(StartingSalesPerson) >= 0)
                .Where(w => w.SalesPerson.CompareTo(EndingSalesPerson) <= 0)
                .Select(s => s.ActualOverride).Sum();

            if (actualOverride.HasValue == true && actualOverride > 0)
            {
                result = actualOverride.Value;
            }
            else if (familyCodeCategory.BookingInvoiceCode == "I")
            {
                result = 0;
                foreach (string fc in FamilyCodeCategories
                    .Where(w => w.Item2.Trim().ToLower().Equals(familyCodeCategory.FamilyCodeCategory.Trim().ToLower()))
                    .Select(s => s.Item1.Trim().ToLower()))
                {
                    result += Actuals
                        .Where(w => w.DerFamilyCode.Trim().ToLower().Equals(fc)
                            && GetLastDayOfWeek(w.InvoiceDate.Date) == week.Date)
                        .Where(w => w.DerSalesPerson.CompareTo(StartingSalesPerson) >= 0)
                        .Where(w => w.DerSalesPerson.CompareTo(EndingSalesPerson) <= 0)
                        .Select(s => s.DerExtendedPrice)
                        .Sum();
                }
            }
            else
            {
                result = 0;

                result = Bookings
                        .Where(w => 
                            w.ue_PFI_FamilyCodeCategory.Trim().ToLower().Equals(familyCodeCategory.FamilyCodeCategory.Trim().ToLower())
                            && GetLastDayOfWeek(w.CoOrderDate).Date == week.Date
                        )
                        .Where(w => w.CoSlsman.CompareTo(StartingSalesPerson) >= 0)
                        .Where(w => w.CoSlsman.CompareTo(EndingSalesPerson) <= 0)
                        .Select(s => s.DerNetPrice).Sum();
            }

            return result;
        }
        private ue_PFI_SPFCActOverrideAll[] GetActualOverrides(string SiteRef)
        {
            ue_PFI_SPFCActOverrideAll[] actualOverrides;
            DateTime end;
            FiscalYearPeriods fcPeriod;

            actualOverrides = DataAccess.ue_PFI_SPFCActOverrideAll(SiteRef);

            foreach(ue_PFI_SPFCActOverrideAll ao in actualOverrides) 
            {
                fcPeriod = FiscalYearPeriods.Where(w => w.FiscalYear == ao.FiscalYear).FirstOrDefault();
                if(fcPeriod != null) 
                {
                    switch (ao.FiscalPeriod)
                    {
                        case 1:
                            end = fcPeriod.EndPeriod1;
                            break;
                        case 2:
                            end = fcPeriod.EndPeriod2; 
                            break;
                        case 3:
                            end = fcPeriod.EndPeriod3;
                            break;
                        case 4:
                            end = fcPeriod.EndPeriod4;
                            break;
                        case 5:
                            end = fcPeriod.EndPeriod5;
                            break;
                        case 6:
                            end = fcPeriod.EndPeriod6;
                            break;
                        case 7:
                            end = fcPeriod.EndPeriod7;
                            break;
                        case 8:
                            end = fcPeriod.EndPeriod8;
                            break;
                        case 9:
                            end = fcPeriod.EndPeriod9;
                            break;
                        case 10:
                            end = fcPeriod.EndPeriod10;
                            break;
                        case 11:
                            end = fcPeriod.EndPeriod11;
                            break;
                        case 12:
                            end = fcPeriod.EndPeriod12;
                            break;
                        case 13:
                            end = fcPeriod.EndPeriod13;
                            break;
                        default:
                            end = DateTime.Now.Date;
                            break;
                    }
                    ao.FiscalPeriodEnd = GetLastDayOfWeek(end.Date);
                }
                else
                {
                    ao.FiscalPeriodEnd = GetLastDayOfWeek(DateTime.Now.Date);
                }
            }

            return actualOverrides;
        }
    }
}
