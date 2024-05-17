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
using PFI.Reporting.BL;
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
            DataTable results = null;
            SalesGoalsByProductsBySalesmanReportBL bl = new SalesGoalsByProductsBySalesmanReportBL(base.Context.Commands);

            results = bl.PFI_GetReport(SiteRef, fiscalYearStarting, fiscalYearEnding, familyCodeStarting, familyCodeEnding, salesPersonStarting, salesPersonEnding);

            return results;
        }

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable PFI_GetFCDetailReport(string SiteRef, int? fiscalYearStarting, int? fiscalYearEnding, string familyCodeStarting, string familyCodeEnding, string salesPersonStarting, string salesPersonEnding)
        {
            DataTable results = null;
            SalesGoalsByProductsBySalesmanReportBL bl = new SalesGoalsByProductsBySalesmanReportBL(base.Context.Commands);

            results = bl.PFI_GetFCDetailReport(SiteRef, fiscalYearStarting, fiscalYearEnding, familyCodeStarting, familyCodeEnding, salesPersonStarting, salesPersonEnding);

            return results;
        }

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable PFI_GetPCDetailReport(string SiteRef, int? fiscalYearStarting, int? fiscalYearEnding, string familyCodeStarting, string familyCodeEnding, string salesPersonStarting, string salesPersonEnding, string productCodeStarting, string productCodeEnding)
        {
            DataTable results = null;
            SalesGoalsByProductsBySalesmanReportBL bl = new SalesGoalsByProductsBySalesmanReportBL(base.Context.Commands);

            results = bl.PFI_GetPCDetailReport(SiteRef, fiscalYearStarting, fiscalYearEnding, familyCodeStarting, familyCodeEnding, salesPersonStarting, salesPersonEnding, productCodeStarting, productCodeEnding);

            return results;
        }

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable PFI_GetItemDetailReport(string SiteRef, int? fiscalYearStarting, int? fiscalYearEnding, string familyCodeStarting, string familyCodeEnding, string salesPersonStarting, string salesPersonEnding, string productCodeStarting, string productCodeEnding, string itemStarting, string itemEnding)
        {
            DataTable results = null;
            SalesGoalsByProductsBySalesmanReportBL bl = new SalesGoalsByProductsBySalesmanReportBL(base.Context.Commands);

            results = bl.PFI_GetItemDetailReport(SiteRef, fiscalYearStarting, fiscalYearEnding, familyCodeStarting, familyCodeEnding, salesPersonStarting, salesPersonEnding, productCodeStarting, productCodeEnding, itemStarting, itemEnding);

            return results;
        }
    }
}
