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
    [IDOExtensionClass("PFI_SalesGoalsByProductsBySalesmanReport")]
    public class PFI_SalesGoalsByProductsBySalesmanReport : IDOExtensionClass
    {
        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable PFI_GetReport(string SiteRef, int? fiscalYearStarting, int? fiscalYearEnding, string familyCodeStarting, string familyCodeEnding, string salesPersonStarting, string salesPersonEnding)
        {
            DataAccess dataAccess;
            SLFamCodeAll[] slFamCodeAlls;
            SLSlsmanAll[] slSlsmanAlls;
            PFI_FiscalYear[] fiscalYears;
            ue_PFI_SalespersonFCBudgetAll[] budgets;
            ue_PFI_SalespersonFCBudgetAll budget;
            ue_PFI_GrossProfitReportSale[] fcSpActuals = null;
            ue_PFI_GrossProfitReportSale[] actuals;
            DataTable results = null;
            DataRow row;
            decimal finalActual;
            decimal finalBudget;
            string infobar = null;
            
            try
            {
                dataAccess = new DataAccess(base.Context); //Context is inherited
                results = new DataTable("Results");
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

                slFamCodeAlls = dataAccess.SLFamCodeAlls(SiteRef);
                slSlsmanAlls = dataAccess.SLSlsmanAlls(SiteRef);
                fiscalYears = dataAccess.FiscalYears(SiteRef);
                budgets = dataAccess.ue_PFI_SalespersonFCBudgetAll(SiteRef);
                actuals = dataAccess.ue_PFI_GrossProfitReportSale(SiteRef);

                if(fiscalYearEnding.HasValue == false)
                    fiscalYearEnding = 9999;

                if (fiscalYearStarting.HasValue == false)
                    fiscalYearStarting = 1900;

                if (string.IsNullOrWhiteSpace(familyCodeStarting))
                    familyCodeStarting = slFamCodeAlls.Select(s=>s.FamilyCode).Min();

                if (string.IsNullOrWhiteSpace(familyCodeEnding))
                    familyCodeEnding = slFamCodeAlls.Select(s => s.FamilyCode).Max();

                if (string.IsNullOrWhiteSpace(salesPersonStarting))
                    salesPersonStarting = slSlsmanAlls.Select(s => s.Slsman).Min();

                if (string.IsNullOrWhiteSpace(salesPersonEnding))
                    salesPersonEnding = slSlsmanAlls.Select(s => s.Slsman).Max();

                foreach (SLFamCodeAll fc in slFamCodeAlls
                    .Where(w => w.FamilyCode.CompareTo(familyCodeStarting) >= 0)
                    .Where(w => w.FamilyCode.CompareTo(familyCodeEnding) <= 0)
                    .OrderBy(o=>o.FamilyCode)) 
                {
                    foreach (SLSlsmanAll sp in slSlsmanAlls
                        .Where(w => w.Slsman.CompareTo(salesPersonStarting) >= 0)
                        .Where(w => w.Slsman.CompareTo(salesPersonEnding) <= 0)
                        .OrderBy(o => o.Slsman))
                    {
                        foreach (PFI_FiscalYear fy in fiscalYears
                            .Where(w=>w.FiscalYear >= fiscalYearStarting)
                            .Where(w => w.FiscalYear <= fiscalYearEnding)
                            .OrderBy(o => o.FiscalYear))
                        {
                            finalActual = 0;
                            finalBudget = 0;
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
                                        && w.SalesPerson.Equals(sp.Slsman)
                                        && w.FamilyCode.Equals(fc.FamilyCode))
                                    .FirstOrDefault();

                                if (budget != null)
                                {
                                    finalBudget = budget.Budget;
                                    row["CLM_Budget"] = finalBudget;
                                    row["CLM_Notes"] = budget.Notes;
                                    row["CLM_ActualOverride"] = budget.ActualOverride;
                                }
                                else
                                {
                                    row["CLM_Notes"] = "No budget records found.";
                                }

                                //If we overrided the actuals then we will report back the override.
                                fcSpActuals = null;
                                if (budget != null && budget.ActualOverride > 0)
                                {
                                    finalActual = budget.ActualOverride;
                                    row["CLM_Actual"] = finalActual;
                                }
                                else
                                {
                                    fcSpActuals = actuals
                                        .Where(w => w.DerFiscalYear == fy.FiscalYear
                                            && w.DerSalesPerson.Equals(sp.Slsman)
                                            && w.DerFamilyCode.Equals(fc.FamilyCode))
                                        .ToArray();
                                }
                                                                
                                //If actuals were found then sum the actuals.
                                if (fcSpActuals != null && fcSpActuals.Count() > 0)
                                {
                                    finalActual = fcSpActuals.Select(s => s.DerExtendedPrice).Sum();
                                    row["CLM_Actual"] = finalActual;
                                }
                                else
                                {
                                    finalActual = 0;
                                    row["CLM_Actual"] = 0;
                                }

                                if (finalActual != 0 || finalBudget != 0)
                                    results.Rows.Add(row);

                            }   
                            catch (Exception ex)
                            {
                                row["CLM_Notes"] = ex.Message;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                infobar = ex.Message;
            }
            //finally
            //{
            //    if(!string.IsNullOrWhiteSpace(infobar))
            //    {
            //        results = new DataTable("Results");
            //        results.Columns.Add(new DataColumn("FamilyCode", System.Type.GetType("System.String")));
            //        results.Columns.Add(new DataColumn("FiscalYear", System.Type.GetType("System.Int32")));
            //        results.Columns.Add(new DataColumn("SalesPerson", System.Type.GetType("System.String")));
            //        results.Columns.Add(new DataColumn("Budget", System.Type.GetType("System.Decimal")));
            //        results.Columns.Add(new DataColumn("ActualOverride", System.Type.GetType("System.Decimal")));
            //        results.Columns.Add(new DataColumn("Notes", System.Type.GetType("System.String")));
            //        results.Columns.Add(new DataColumn("CLM_Actual", System.Type.GetType("System.Decimal")));
            //        results.Columns.Add(new DataColumn("CLM_SiteRef", System.Type.GetType("System.String")));
            //        row = results.NewRow();
            //        row["Notes"] = infobar;
            //        results.Rows.Add(row);
            //    }
            //}

            return results;
        }
    }
}
