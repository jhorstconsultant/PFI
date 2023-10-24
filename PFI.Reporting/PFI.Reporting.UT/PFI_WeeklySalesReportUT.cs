using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PFI.Reporting.UT
{
    [TestClass]
    public class PFI_WeeklySalesReportUT
    {
        [TestMethod]
        public void PFI_GetWeekendings()
        {
            PFI_WeeklySalesReport wsr = new PFI_WeeklySalesReport();
            wsr.PFI_GetWeekendings();
        }
    }
}
