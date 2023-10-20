using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFI.Reporting.Models
{
    //ue_PFI_SalespersonFCBudgetAlls
    public class ue_PFI_SalespersonFCBudgetAll
    {
        public string SiteRef { get; set; }
        public decimal CLM_Actual { get;set; }
        public int FiscalYear { get; set; }
        public string FamilyCode { get; set; }
        public string SalesPerson { get; set; }
        public decimal Budget { get; set; }
        public decimal ActualOverride { get; set; }
        public string Notes { get; set; }
        public string FamilyCodeCategory { get; set; }
    }
}
