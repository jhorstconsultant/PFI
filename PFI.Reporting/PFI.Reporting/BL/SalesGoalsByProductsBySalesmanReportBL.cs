using Infor.DocumentManagement.ICP;
//using Mongoose.Forms;
using Mongoose.IDO;
using Mongoose.IDO.Metadata;
using Mongoose.IDO.Protocol;
using Newtonsoft.Json;
using PFI.Reporting.DA;
using PFI.Reporting.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ICP.Standard.Signature.V1.UserFilterResponse;
using static Mongoose.Core.Common.QuickKeywordParser;
//using static Mongoose.Forms.ExportSettings;

namespace PFI.Reporting.BL
{
    public class SalesGoalsByProductsBySalesmanReportBL
    {
        public SalesGoalsByProductsBySalesmanReportBL(IIDOCommands context)
        {
            this.Context = context;
            this.DataAccess = new PFIDataAccess(context); 
        }

        private IIDOCommands Context { get; set; }
        private PFIDataAccess DataAccess { get; set; }

        public DataTable PFI_GetReport(string SiteRef, int? fiscalYearStarting, int? fiscalYearEnding, string familyCodeStarting, string familyCodeEnding, string salesPersonStarting, string salesPersonEnding)
        {
            SLFamCodeAll[] slFamCodeAlls;
            SLSlsmanAll[] slSlsmanAlls;
            PFI_FiscalYear[] fiscalYears;
            ue_PFI_SalespersonFCBudgetAll[] budgets;
            ue_PFI_SPFCActOverrideAll[] actualOverrides;
            ue_PFI_SalespersonFCBudgetAll budget;
            ue_PFI_GrossProfitReportSale[] fcSpActuals = null;
            ue_PFI_GrossProfitReportSale[] actuals;
            DataTable results = null;
            DataRow row;
            decimal? finalActual;
            decimal finalBudget;
            decimal actualOverrideTotal;
            string infobar = null;

            try
            {
                results = SetupDataTable("PFI_GetReport");
                slFamCodeAlls = DataAccess.SLFamCodeAlls(SiteRef);
                slSlsmanAlls = DataAccess.SLSlsmanAlls(SiteRef);
                fiscalYears = DataAccess.FiscalYears(SiteRef);
                budgets = DataAccess.ue_PFI_SalespersonFCBudgetAll(SiteRef);
                actuals = DataAccess.ue_PFI_GrossProfitReportSale(SiteRef);
                actualOverrides = DataAccess.ue_PFI_SPFCActOverrideAll(SiteRef);

                if (fiscalYearEnding.HasValue == false)
                    fiscalYearEnding = 9999;

                if (fiscalYearStarting.HasValue == false)
                    fiscalYearStarting = 1900;

                if (string.IsNullOrWhiteSpace(familyCodeStarting))
                    familyCodeStarting = slFamCodeAlls.Select(s => s.FamilyCode).Min();

                if (string.IsNullOrWhiteSpace(familyCodeEnding))
                    familyCodeEnding = slFamCodeAlls.Select(s => s.FamilyCode).Max();

                if (string.IsNullOrWhiteSpace(salesPersonStarting))
                    salesPersonStarting = slSlsmanAlls.Select(s => s.Slsman).Min();

                if (string.IsNullOrWhiteSpace(salesPersonEnding))
                    salesPersonEnding = slSlsmanAlls.Select(s => s.Slsman).Max();

                foreach (SLFamCodeAll fc in slFamCodeAlls
                    .Where(w => w.FamilyCode.CompareTo(familyCodeStarting) >= 0)
                    .Where(w => w.FamilyCode.CompareTo(familyCodeEnding) <= 0)
                    .OrderBy(o => o.FamilyCode))
                {
                    foreach (SLSlsmanAll sp in slSlsmanAlls
                        .Where(w => w.Slsman.CompareTo(salesPersonStarting) >= 0)
                        .Where(w => w.Slsman.CompareTo(salesPersonEnding) <= 0)
                        .OrderBy(o => o.Slsman))
                    {
                        foreach (PFI_FiscalYear fy in fiscalYears
                            .Where(w => w.FiscalYear >= fiscalYearStarting)
                            .Where(w => w.FiscalYear <= fiscalYearEnding)
                            .OrderBy(o => o.FiscalYear))
                        {
                            finalActual = 0;
                            finalBudget = 0;
                            actualOverrideTotal = 0;
                            row = results.NewRow();
                            row["CLM_SiteRef"] = fc.SiteRef;
                            row["CLM_FamilyCode"] = fc.FamilyCode;
                            row["CLM_FamilyCodeDesc"] = fc.Description;
                            row["CLM_FiscalYear"] = fy.FiscalYear;
                            row["CLM_SalesPerson"] = sp.Slsman;
                            row["CLM_SalesPersonDesc"] = sp.DerSlsmanName;

                            try
                            {
                                budget = null;
                                budget = budgets
                                    .Where(w => w.FiscalYear == fy.FiscalYear
                                        && w.SalesPerson.Trim().ToLower().Equals(sp.Slsman.Trim().ToLower())
                                        && w.FamilyCode.Trim().ToLower().Equals(fc.FamilyCode.Trim().ToLower()))
                                    .FirstOrDefault();

                                //Get the total actual override
                                actualOverrideTotal = actualOverrides
                                    .Where(w => w.FiscalYear == fy.FiscalYear
                                        && w.SalesPerson.Trim().ToLower().Equals(sp.Slsman.Trim().ToLower())
                                        && w.FamilyCode.Trim().ToLower().Equals(fc.FamilyCode.Trim().ToLower()))
                                    .Select(s => s.ActualOverride).Sum();

                                if (budget != null)
                                {
                                    finalBudget = budget.Budget;
                                    row["CLM_Budget"] = finalBudget;
                                    row["CLM_Notes"] = budget.Notes;
                                    row["CLM_FamilyCodeCategory"] = budget.FamilyCodeCategory;
                                }
                                else
                                {
                                    row["CLM_Notes"] = "No budget records found.";
                                }

                                //If we overrided the actuals then we will report back the override.
                                fcSpActuals = null;
                                if (actualOverrideTotal != null && actualOverrideTotal > 0)
                                {
                                    finalActual = actualOverrideTotal;
                                    row["CLM_Actual"] = actualOverrideTotal;
                                    row["CLM_ActualOverride"] = actualOverrideTotal;
                                }
                                //Otherwise get the real actuals and set the override to 0.
                                else
                                {
                                    row["CLM_ActualOverride"] = 0;
                                    fcSpActuals = actuals
                                        .Where(w => w.DerFiscalYear == fy.FiscalYear
                                            && w.DerSalesPerson.Equals(sp.Slsman)
                                            && w.DerFamilyCode.Equals(fc.FamilyCode))
                                        .ToArray();

                                    //If actuals were found then sum the actuals.
                                    if (fcSpActuals != null && fcSpActuals.Count() > 0)
                                    {
                                        finalActual = fcSpActuals.Select(s => s.DerExtendedPrice).Sum();
                                        row["CLM_Actual"] = finalActual;
                                    }
                                }

                                if (finalActual.HasValue == false)
                                {
                                    finalActual = 0;
                                    row["CLM_Actual"] = 0;
                                    row["CLM_ActualOverride"] = 0;
                                }

                                if (finalActual != 0 || finalBudget != 0)
                                    results.Rows.Add(row);
                            }
                            catch (Exception ex)
                            {
                                row["CLM_Notes"] = ex.Message;
                                results.Rows.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                infobar = ex.Message;
            }

            return results;
        }


        public DataTable PFI_GetFCDetailReport(string SiteRef, int? fiscalYearStarting, int? fiscalYearEnding, string familyCodeStarting, string familyCodeEnding, string salesPersonStarting, string salesPersonEnding)
        {
            DataTable results = null;
            PFI_FiscalYear[] fiscalYears;
            SLFamCodeAll[] slFamCodeAlls;
            SLSlsmanAll[] slSlsmanAlls;
            AmountByPeriod amountByPeriod;
            AmountByPeriod actual;
            AmountByPeriod ytdGoal;
            AmountByPeriod ytdActual;
            ue_PFI_GrossProfitReportSale[] actuals; 
            ue_PFI_SPFCActOverrideAll[] actualOverrides;
            FiscalYearPeriods[] fiscalYearPeriods;
            ue_PFI_SalespersonFCBudgetAll[] budgets;

            results = SetupDataTable("PFI_GetFCDetailReport");
            fiscalYears = DataAccess.FiscalYears(SiteRef);
            slFamCodeAlls = DataAccess.SLFamCodeAlls(SiteRef);
            slSlsmanAlls = DataAccess.SLSlsmanAlls(SiteRef);

            //Needed for Actuals
            fiscalYearPeriods = DataAccess.FiscalYearPeriods()
                .Where(w => w.FiscalYear >= fiscalYearStarting)
                .Where(w => w.FiscalYear <= fiscalYearEnding)
                .ToArray(); //TODO - Pass in parameters so it gets results by fy
            actuals = DataAccess.ue_PFI_GrossProfitReportSale(SiteRef);     //TODO - Pass in parameters so it gets results by fc and sp.
            actualOverrides = DataAccess.ue_PFI_SPFCActOverrideAll(SiteRef);//TODO - Pass in parameters so it gets results by fc and sp.
            
            //Needed for YTD Goals
            budgets = DataAccess.ue_PFI_SalespersonFCBudgetAll(SiteRef);

            //Need to default any parameters which are null
            if (fiscalYearEnding.HasValue == false)
                fiscalYearEnding = 9999;

            if (fiscalYearStarting.HasValue == false)
                fiscalYearStarting = 1900;

            if (string.IsNullOrWhiteSpace(familyCodeStarting))
                familyCodeStarting = slFamCodeAlls.Select(s => s.FamilyCode).Min();

            if (string.IsNullOrWhiteSpace(familyCodeEnding))
                familyCodeEnding = slFamCodeAlls.Select(s => s.FamilyCode).Max();

            if (string.IsNullOrWhiteSpace(salesPersonStarting))
                salesPersonStarting = slSlsmanAlls.Select(s => s.Slsman).Min();

            if (string.IsNullOrWhiteSpace(salesPersonEnding))
                salesPersonEnding = slSlsmanAlls.Select(s => s.Slsman).Max();

            //Loop through each dataset
            foreach (PFI_FiscalYear fy in fiscalYears
                    .Where(w => w.FiscalYear >= fiscalYearStarting)
                    .Where(w => w.FiscalYear <= fiscalYearEnding)
                    .OrderBy(o => o.FiscalYear))
            {
                foreach(SLFamCodeAll fc in slFamCodeAlls
                    .Where(w => w.FamilyCode.CompareTo(familyCodeStarting) >= 0)
                    .Where(w => w.FamilyCode.CompareTo(familyCodeEnding) <= 0)
                    .OrderBy(o => o.FamilyCode))
                {
                    //Loop through each salesperson and get a grand total accross all of them.
                    actual = new AmountByPeriod();
                    ytdGoal  = new AmountByPeriod();
                    ytdActual  = new AmountByPeriod();
                    foreach (SLSlsmanAll sp in slSlsmanAlls
                        .Where(w => w.Slsman.CompareTo(salesPersonStarting) >= 0)
                        .Where(w => w.Slsman.CompareTo(salesPersonEnding) <= 0)
                        .OrderBy(o => o.Slsman))
                    {
                        //In an effort to increase readability I am first setting the new amount by period in a temp variable then sending it into the sum function.
                        //Actuals
                        amountByPeriod = GetActuals(SiteRef, fy.FiscalYear, fc.FamilyCode, sp.Slsman
                            , actuals, actualOverrides, fiscalYearPeriods);
                        actual    = this.SumAmountByPeriod(actual, amountByPeriod);

                        //Year to Date Goals
                        amountByPeriod = GetYTDGoals(SiteRef, fy.FiscalYear, fc.FamilyCode, sp.Slsman
                            , budgets);
                        ytdGoal   = this.SumAmountByPeriod(ytdGoal, amountByPeriod);
                        
                        //Year to Date Actuals
                        amountByPeriod = GetYTDActuals(actual);
                        ytdActual = this.SumAmountByPeriod(ytdActual, amountByPeriod);
                    }

                    

                    //throw (new Exception($"" +
                    //    $"results.Rows.Count = {results.Rows.Count}" +
                    //    $"{SiteRef}" +
                    //    $"fiscalYearEnding {fiscalYearEnding}   fiscalYearStarting {fiscalYearStarting}" +
                    //    $"familyCodeEnding {familyCodeEnding}   familyCodeStarting {familyCodeStarting}" +
                    //    $"salesPersonEnding {salesPersonEnding} salesPersonStarting {salesPersonStarting}" +
                    //    $"FY count {fiscalYears.Count()} " +
                    //    $"slFamCodeAlls {slFamCodeAlls.Count()}" +
                    //    $"slSlsmanAlls {slSlsmanAlls.Count()}"
                    //    ));

                    //Log a row for the sum totals by FC.
                    AddRowFC(results, SiteRef, fc.FamilyCode, fc.Description, fy.FiscalYear
                            , actual 
                            , ytdGoal  
                            , ytdActual
                            , null
                            );
                }
            }

            //actuals = new AmountByPeriod();
            //ytdGoals = new AmountByPeriod();
            //ytdActuals = new AmountByPeriod();

            //AddRowFC(results, SiteRef, "test", "test", 2024
            //    , actuals
            //    , ytdGoals
            //    , ytdActuals
            //    , "test"
            //);

            //throw (new Exception($"" +
            //    $"results.Rows.Count = {results.Rows.Count}" +
            //    $"{SiteRef}" +
            //    $"fiscalYearEnding {fiscalYearEnding}   fiscalYearStarting {fiscalYearStarting}" +
            //    $"familyCodeEnding {familyCodeEnding}   familyCodeStarting {familyCodeStarting}" +
            //    $"salesPersonEnding {salesPersonEnding} salesPersonStarting {salesPersonStarting}" +
            //    $"FY count {fiscalYears.Count()} " +
            //    $"slFamCodeAlls {slFamCodeAlls.Count()}" +
            //    $"slSlsmanAlls {slSlsmanAlls.Count()}"
            //    ));

            return results;
        }
        public DataTable PFI_GetPCDetailReport(string SiteRef, int? fiscalYearStarting, int? fiscalYearEnding, string familyCodeStarting, string familyCodeEnding, string salesPersonStarting, string salesPersonEnding, string productCodeStarting, string productCodeEnding)
        {
            DataTable results = null;

            return results;
        }
        public DataTable PFI_GetItemDetailReport(string SiteRef, int? fiscalYearStarting, int? fiscalYearEnding, string familyCodeStarting, string familyCodeEnding, string salesPersonStarting, string salesPersonEnding, string productCodeStarting, string productCodeEnding, string itemStarting, string itemEnding)
        {
            DataTable results = null;

            return results;
        }
        private DataTable SetupDataTable(string reportName)
        {
            DataTable results = null;

            results = new DataTable("Results");

            if (reportName.Equals("PFI_GetReport"))
            {
                results.Columns.Add(new DataColumn("CLM_FamilyCode", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("CLM_FamilyCodeDesc", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("CLM_FiscalYear", System.Type.GetType("System.Int32")));
                results.Columns.Add(new DataColumn("CLM_SalesPerson", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("CLM_SalesPersonDesc", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("CLM_Budget", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_ActualOverride", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_Notes", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("CLM_Actual", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_SiteRef", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("RowPointer", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("CLM_FamilyCodeCategory", System.Type.GetType("System.String")));
            }
            else if (reportName.Equals("PFI_GetFCDetailReport"))
            {
                results.Columns.Add(new DataColumn("CLM_FamilyCode", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("CLM_FamilyCodeDesc", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("CLM_FiscalYear", System.Type.GetType("System.Int32")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual01", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual02", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual03", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual04", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual05", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual06", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual07", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual08", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual09", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual10", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual11", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual12", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual13", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal01", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal02", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal03", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal04", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal05", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal06", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal07", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal08", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal09", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal10", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal11", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal12", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal13", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual01", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual02", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual03", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual04", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual05", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual06", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual07", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual08", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual09", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual10", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual11", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual12", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual13", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_Notes", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("RowPointer", System.Type.GetType("System.Guid")));
                results.Columns.Add(new DataColumn("CLM_SiteRef", System.Type.GetType("System.String")));
            }
            else if (reportName.Equals("PFI_GetPCDetailReport"))
            {
                results.Columns.Add(new DataColumn("CLM_ProductCode", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("CLM_ProductCodeDesc", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("CLM_FiscalYear", System.Type.GetType("System.Int32")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual01", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual02", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual03", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual04", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual05", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual06", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual07", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual08", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual09", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual10", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual11", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual12", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual13", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal01", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal02", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal03", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal04", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal05", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal06", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal07", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal08", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal09", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal10", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal11", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal12", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal13", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual01", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual02", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual03", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual04", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual05", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual06", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual07", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual08", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual09", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual10", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual11", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual12", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual13", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_Notes", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("RowPointer", System.Type.GetType("System.Guid")));
                results.Columns.Add(new DataColumn("CLM_SiteRef", System.Type.GetType("System.String")));
            }
            else if (reportName.Equals("PFI_GetItemDetailReport"))
            {

                results.Columns.Add(new DataColumn("CLM_Item", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("CLM_ItemDesc", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("CLM_FiscalYear", System.Type.GetType("System.Int32")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual01", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual02", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual03", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual04", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual05", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual06", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual07", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual08", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual09", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual10", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual11", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual12", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_PeriodActual13", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal01", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal02", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal03", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal04", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal05", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal06", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal07", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal08", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal09", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal10", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal11", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal12", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_YTDGoal13", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual01", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual02", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual03", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual04", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual05", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual06", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual07", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual08", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual09", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual10", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual11", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual12", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CNH_YTDActual13", System.Type.GetType("System.Decimal")));
                results.Columns.Add(new DataColumn("CLM_Notes", System.Type.GetType("System.String")));
                results.Columns.Add(new DataColumn("RowPointer", System.Type.GetType("System.Guid")));
                results.Columns.Add(new DataColumn("CLM_SiteRef", System.Type.GetType("System.String")));
            }

            return results;
        }
    
        private AmountByPeriod GetYTDGoals(string siteRef, int fiscalYear, string familyCode, string salesPerson
            , ue_PFI_SalespersonFCBudgetAll[] budgets)
        {
            AmountByPeriod result = null;
            //ue_PFI_SalespersonFCBudgetAll[] budgets;
            decimal twelfth;

            //budgets = DataAccess.ue_PFI_SalespersonFCBudgetAll(siteRef); //TODO - Pass in parameters so it gets results by fc and sp.

            foreach(var b in budgets
                    .Where(w => w.FiscalYear == fiscalYear)
                    .Where(w => w.FamilyCode.Equals(familyCode))
                    .Where(w => w.SalesPerson.Equals(salesPerson))
                    )
            {
                if (b.Budget > 0)
                {
                    result = new AmountByPeriod();
                    twelfth = b.Budget / 12;
                    result.P01 = twelfth;
                    result.P02 = twelfth + result.P01;
                    result.P03 = twelfth + result.P02;
                    result.P04 = twelfth + result.P03;
                    result.P05 = twelfth + result.P04;
                    result.P06 = twelfth + result.P05;
                    result.P07 = twelfth + result.P06;
                    result.P08 = twelfth + result.P07;
                    result.P09 = twelfth + result.P08;
                    result.P10 = twelfth + result.P09;
                    result.P11 = twelfth + result.P10;
                    result.P12 = twelfth + result.P11;
                    result.P13 = 0;
                }
            }

            if(result == null)
            {
                result = new AmountByPeriod();
                result.P01 = 0;
                result.P02 = 0;
                result.P03 = 0;
                result.P04 = 0;
                result.P05 = 0;
                result.P06 = 0;
                result.P07 = 0;
                result.P08 = 0;
                result.P09 = 0;
                result.P10 = 0;
                result.P11 = 0;
                result.P12 = 0;
                result.P13 = 0;
            }
            
            return result;
        }

        private AmountByPeriod GetYTDActuals(AmountByPeriod actuals)
        {
            AmountByPeriod result = new AmountByPeriod();

            result = actuals;

            //Our goal with YTD is to sum up the prior periods.
            //So by getting the normal actuals not YTD the months
            //already have the actuals they naturally earned in that month.
            //So going down the line in order we can get YTD by adding the
            //prior period to the current one.
            result.P02 = result.P02 + result.P01;
            result.P03 = result.P03 + result.P02;
            result.P04 = result.P04 + result.P03;
            result.P05 = result.P05 + result.P04;
            result.P06 = result.P06 + result.P05;
            result.P07 = result.P07 + result.P06;
            result.P08 = result.P08 + result.P07;
            result.P09 = result.P09 + result.P08;
            result.P10 = result.P10 + result.P09;
            result.P11 = result.P11 + result.P10;
            result.P12 = result.P12 + result.P11;
            result.P13 = result.P13 + result.P12;

            return result;
        }

        private AmountByPeriod SumAmountByPeriod(AmountByPeriod a1, AmountByPeriod a2)
        {
            AmountByPeriod result = new AmountByPeriod();
            result.P01 = a1.P01 + a2.P01;
            result.P02 = a1.P02 + a2.P02;
            result.P03 = a1.P03 + a2.P03;
            result.P04 = a1.P04 + a2.P04;
            result.P05 = a1.P05 + a2.P05;
            result.P06 = a1.P06 + a2.P06;
            result.P07 = a1.P07 + a2.P07;
            result.P08 = a1.P08 + a2.P08;
            result.P09 = a1.P09 + a2.P09;
            result.P10 = a1.P10 + a2.P10;
            result.P11 = a1.P11 + a2.P11;
            result.P12 = a1.P12 + a2.P12;
            result.P13 = a1.P13 + a2.P13;

            return result;
        }

        private AmountByPeriod GetActuals(string siteRef, int fiscalYear, string familyCode, string salesPerson,
            ue_PFI_GrossProfitReportSale[] actuals,
            ue_PFI_SPFCActOverrideAll[] actualOverrides,
            FiscalYearPeriods[] fiscalYearPeriods
            )
        {
            AmountByPeriod result;
            FiscalYearPeriods[] currentFiscalYearPeriods;
            int period;

            currentFiscalYearPeriods = fiscalYearPeriods.Where(w=>w.FiscalYear == fiscalYear).ToArray();

            //Initialize the data.
            result = new AmountByPeriod();
            result.P01 = 0;
            result.P02 = 0;
            result.P03 = 0;
            result.P04 = 0;
            result.P05 = 0;
            result.P06 = 0;
            result.P07 = 0;
            result.P08 = 0;
            result.P09 = 0;
            result.P10 = 0;
            result.P11 = 0;
            result.P12 = 0;
            result.P13 = 0;


            //throw (new Exception($"" +
            //    $"actual = {JsonConvert.SerializeObject(actuals.Where(w => w.DerFiscalYear == fiscalYear).Where(w => w.DerFamilyCode.Trim().ToLower().Equals(familyCode.Trim().ToLower())).Where(w => w.DerSalesPerson.Trim().ToLower().Equals(salesPerson.Trim().ToLower())).ToArray())}"
            //    ));

            //Loop through the actuals and sum up actuals based on what period the date lands.
            foreach (var a in actuals
                .Where(w => w.DerFiscalYear == fiscalYear)
                .Where(w => w.DerFamilyCode.Trim().ToLower().Equals(familyCode.Trim().ToLower()))
                .Where(w => w.DerSalesPerson.Trim().ToLower().Equals(salesPerson.Trim().ToLower())))
            {
                //Figure out what period that date falls into.
                period = -1; //Impossible state
                if(currentFiscalYearPeriods.Where(w => w.EndPeriod1 >= a.InvoiceDate).Count() > 0)
                {
                    period = 1;
                }else if (currentFiscalYearPeriods.Where(w => w.EndPeriod2 >= a.InvoiceDate).Count() > 0)
                {  
                    period = 2; 
                }
                else if (currentFiscalYearPeriods.Where(w => w.EndPeriod3 >= a.InvoiceDate).Count() > 0)
                {
                    period = 3;
                }
                else if (currentFiscalYearPeriods.Where(w => w.EndPeriod4 >= a.InvoiceDate).Count() > 0)
                {
                    period = 4;
                }
                else if (currentFiscalYearPeriods.Where(w => w.EndPeriod5 >= a.InvoiceDate).Count() > 0)
                {
                    period = 5;
                }
                else if (currentFiscalYearPeriods.Where(w => w.EndPeriod6 >= a.InvoiceDate).Count() > 0)
                {
                    period = 6;
                }
                else if (currentFiscalYearPeriods.Where(w => w.EndPeriod7 >= a.InvoiceDate).Count() > 0)
                {
                    period = 7;
                }
                else if (currentFiscalYearPeriods.Where(w => w.EndPeriod8 >= a.InvoiceDate).Count() > 0)
                {
                    period = 8;
                }
                else if (currentFiscalYearPeriods.Where(w => w.EndPeriod9 >= a.InvoiceDate).Count() > 0)
                {
                    period = 9;
                }
                else if (currentFiscalYearPeriods.Where(w => w.EndPeriod10 >= a.InvoiceDate).Count() > 0)
                {
                    period = 10;
                }
                else if (currentFiscalYearPeriods.Where(w => w.EndPeriod11 >= a.InvoiceDate).Count() > 0)
                {
                    period = 11;
                }
                else if (currentFiscalYearPeriods.Where(w => w.EndPeriod12 >= a.InvoiceDate).Count() > 0)
                {
                    period = 12;
                }
                else if (currentFiscalYearPeriods.Where(w => w.EndPeriod13 >= a.InvoiceDate).Count() > 0)
                {
                    period = 13;
                }

                //Sum up the price into the right period bucket.
                switch (period)
                {
                    case 1:
                        result.P01 += a.DerExtendedPrice;
                        break;
                    case 2:
                        result.P02 += a.DerExtendedPrice;
                        break;
                    case 3:
                        result.P03 += a.DerExtendedPrice;
                        break;
                    case 4:
                        result.P04 += a.DerExtendedPrice;
                        break;
                    case 5:
                        result.P05 += a.DerExtendedPrice;
                        break;
                    case 6:
                        result.P06 += a.DerExtendedPrice;
                        break;
                    case 7:
                        result.P07 += a.DerExtendedPrice;
                        break;
                    case 8:
                        result.P08 += a.DerExtendedPrice;
                        break;
                    case 9:
                        result.P09 += a.DerExtendedPrice;
                        break;
                    case 10:
                        result.P10 += a.DerExtendedPrice;
                        break;
                    case 11:
                        result.P11 += a.DerExtendedPrice;
                        break;
                    case 12:
                        result.P12 += a.DerExtendedPrice;
                        break;
                    case 13:
                        result.P13 += a.DerExtendedPrice;
                        break;
                }
                
            }

            ////Load overrides into the periods
            ////This will replace anything found in actuals for that period.
            //foreach (var a in actualOverrides
            //    .Where(w => w.FiscalYear == fiscalYear)
            //    .Where(w => w.FamilyCode.Trim().ToLower().Equals(familyCode.Trim().ToLower()))
            //    .Where(w => w.SalesPerson.Trim().ToLower().Equals(salesPerson.Trim().ToLower()))
            //    .Where(w => w.ActualOverride > 0))
            //{
            //    switch (a.FiscalPeriod)
            //    {
            //        case 1:
            //            result.P01 = a.ActualOverride;
            //            break;
            //        case 2:
            //            result.P02 = a.ActualOverride;
            //            break;
            //        case 3:
            //            result.P03 = a.ActualOverride;
            //            break;
            //        case 4:
            //            result.P04 = a.ActualOverride;
            //            break;
            //        case 5:
            //            result.P05 = a.ActualOverride;
            //            break;
            //        case 6:
            //            result.P06 = a.ActualOverride;
            //            break;
            //        case 7:
            //            result.P07 = a.ActualOverride;
            //            break;
            //        case 8:
            //            result.P08 = a.ActualOverride;
            //            break;
            //        case 9:
            //            result.P09 = a.ActualOverride;
            //            break;
            //        case 10:
            //            result.P10 = a.ActualOverride;
            //            break;
            //        case 11:
            //            result.P11 = a.ActualOverride;
            //            break;
            //        case 12:
            //            result.P12 = a.ActualOverride;
            //            break;
            //        case 13:
            //            result.P13 = a.ActualOverride;
            //            break;
            //    }
            //}

            return result;
        }

        private void AddRowFC(DataTable dt
            , string  CLM_SiteRef
            , string  CLM_FamilyCode
            , string  CLM_FamilyCodeDesc
            , int     CLM_FiscalYear
            , AmountByPeriod PeriodActual
            , AmountByPeriod PeriodYTDGoal
            , AmountByPeriod PeriodYTDActual
            , string CLM_Notes)
        {
            Guid rowPointer = Guid.NewGuid();
            DataRow row;

            row = dt.NewRow();
            
            row["CLM_FamilyCode"] = CLM_FamilyCode;
            row["CLM_FamilyCodeDesc"] = CLM_FamilyCodeDesc;
            row["CLM_FiscalYear"] = CLM_FiscalYear;
            
            row["CLM_PeriodActual01"] = PeriodActual.P01;
            row["CLM_PeriodActual02"] = PeriodActual.P02;
            row["CLM_PeriodActual03"] = PeriodActual.P03;
            row["CLM_PeriodActual04"] = PeriodActual.P04;
            row["CLM_PeriodActual05"] = PeriodActual.P05;
            row["CLM_PeriodActual06"] = PeriodActual.P06;
            row["CLM_PeriodActual07"] = PeriodActual.P07;
            row["CLM_PeriodActual08"] = PeriodActual.P08;
            row["CLM_PeriodActual09"] = PeriodActual.P09;
            row["CLM_PeriodActual10"] = PeriodActual.P10;
            row["CLM_PeriodActual11"] = PeriodActual.P11;
            row["CLM_PeriodActual12"] = PeriodActual.P12;
            row["CLM_PeriodActual13"] = PeriodActual.P13;
            
            row["CLM_YTDGoal01"] = PeriodYTDGoal.P01;
            row["CLM_YTDGoal02"] = PeriodYTDGoal.P02;
            row["CLM_YTDGoal03"] = PeriodYTDGoal.P03;
            row["CLM_YTDGoal04"] = PeriodYTDGoal.P04;
            row["CLM_YTDGoal05"] = PeriodYTDGoal.P05;
            row["CLM_YTDGoal06"] = PeriodYTDGoal.P06;
            row["CLM_YTDGoal07"] = PeriodYTDGoal.P07;
            row["CLM_YTDGoal08"] = PeriodYTDGoal.P08;
            row["CLM_YTDGoal09"] = PeriodYTDGoal.P09;
            row["CLM_YTDGoal10"] = PeriodYTDGoal.P10;
            row["CLM_YTDGoal11"] = PeriodYTDGoal.P11;
            row["CLM_YTDGoal12"] = PeriodYTDGoal.P12;
            row["CLM_YTDGoal13"] = PeriodYTDGoal.P13;
           
            row["CNH_YTDActual01"] = PeriodYTDActual.P01;
            row["CNH_YTDActual02"] = PeriodYTDActual.P02;
            row["CNH_YTDActual03"] = PeriodYTDActual.P03;
            row["CNH_YTDActual04"] = PeriodYTDActual.P04;
            row["CNH_YTDActual05"] = PeriodYTDActual.P05;
            row["CNH_YTDActual06"] = PeriodYTDActual.P06;
            row["CNH_YTDActual07"] = PeriodYTDActual.P07;
            row["CNH_YTDActual08"] = PeriodYTDActual.P08;
            row["CNH_YTDActual09"] = PeriodYTDActual.P09;
            row["CNH_YTDActual10"] = PeriodYTDActual.P10;
            row["CNH_YTDActual11"] = PeriodYTDActual.P11;
            row["CNH_YTDActual12"] = PeriodYTDActual.P12;
            row["CNH_YTDActual13"] = PeriodYTDActual.P13;

            row["CLM_Notes"] = CLM_Notes;
            row["RowPointer"] = rowPointer;
            row["CLM_SiteRef"] = CLM_SiteRef;

            dt.Rows.Add(row);
        }

    }
}
