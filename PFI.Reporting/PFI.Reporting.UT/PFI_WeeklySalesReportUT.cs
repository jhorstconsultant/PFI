using Microsoft.VisualStudio.TestTools.UnitTesting;
using PFI.Reporting.BL;
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
            //wsr.PFI_GetWeekendings();
        }

        [TestMethod]
        public void GetLastDayOfWeekUT()
        {
            DateTime dt = GetLastDayOfWeek(new DateTime(2023, 5, 25));
            Assert.IsTrue(dt == new DateTime(2023, 5, 27));
        }

        private DateTime GetLastDayOfWeek(DateTime date)
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
            result.AddDays(diff); //Add the necessary days to get to the end of the week.
            return new DateTime(result.Year, result.Month, result.Day);
        }
    }
}
