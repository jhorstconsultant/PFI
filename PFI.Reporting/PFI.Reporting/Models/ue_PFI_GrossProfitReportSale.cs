using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFI.Reporting.Models
{
    //ue_PFI_GrossProfitReportSales
    //Anything commented out is on the IDO but not displayed in the "ue_PFI_GrossProfitReportSales" DV.
    public class ue_PFI_GrossProfitReportSale
    {
        public int CoLine { get; set; }
        public decimal Price { get; set; }
        public decimal QtyInvoiced { get; set; }
        public int DerFiscalYear { get; set; }
        public bool SalesPersonOutside { get; set; }
        public string SalesPersonRefNum { get; set; }
        public string SiteRef { get; set; }
        public string RowPointer { get; set; }
        public string ItemProductCode { get; set; }
        public string CoNum { get; set; }
        public long InvSeq { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvNum { get; set; }
        public long InvLine { get; set; }
        public decimal DerExtendedPrice { get; set; }
        public string DerSalesPerson { get; set; }
        public string DerSalesmanName { get; set; }
        public string DerItem { get; set; }
        public bool DerIsNonItemCoLine { get; set; }
        public string DerFamilyCode { get; set; }
        public int CoRelease { get; set; }

        //public string CoLineFamilyCode { get; set; }
        //public string CoLineItem { get; set; }
        //public string COSalesPerson { get; set; }
        //public string CustPo { get; set; }
        //public string CustShipToSalesPerson { get; set; }
        //public string DerEmployeeName { get; set; }
        //public string EmployeeFirstName { get; set; }
        //public string EmployeeLastName { get; set; }
        //public int FiscalYear { get; set; }
        //public string InvhdrBillType { get; set; }
        //public string InvhdrSalesPerson { get; set; }
        //public string InvItem { get; set; }
        //public string ItemFamilyCode { get; set; }
        //public string ItemItem { get; set; }
        //public string ProcessInd { get; set; }
        //public string VendAddrName { get; set; }
    }
}
