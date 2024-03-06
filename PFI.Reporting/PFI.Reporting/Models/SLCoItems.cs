using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFI.Reporting.Models
{
    public class SLCoItems
    {
        public string CoNum { get; set; }
        public DateTime CoOrderDate { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public decimal QtyOrderedConv { get; set; }
        public decimal PriceConv { get; set; }
        public decimal DerNetPrice { get; set; }
        public decimal CostConv { get; set; }
        public string DerDomCurrCode { get; set; }
        public decimal DerQtyReadyConv { get; set; }
        public decimal DerQtyRsvdConv { get; set; }
        public decimal DerQtyPickedConv { get; set; }
        public decimal DerQtyPackedConv { get; set; }
        public decimal DerQtyShippedConv { get; set; }
        public decimal DerQtyInvoicedConv { get; set; }
        public string RowPointer { get; set; }
        public string ue_ItemAllFamilyCode { get; set; }
        public string ue_PFI_FamilyCodeCategory { get; set; }
        public string CoSlsman { get; set; }
    }
}
