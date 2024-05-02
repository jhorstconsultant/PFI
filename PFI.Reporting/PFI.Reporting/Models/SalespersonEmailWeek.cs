using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFI.Reporting.Models
{
    public class SalespersonEmailWeek
    {
        public DateTime FourFourFiveDate { get; set; }
        public DateTime WeekDate { get; set; }
        public decimal Tumbling { get; set; }
        public decimal Blasting { get; set; }
        public decimal Chemtrol { get; set; }
        public decimal VibratoryMedia { get; set; }
        public decimal BlastMedia { get; set; }
        public decimal SpareParts { get; set; }
        public decimal SupplierCompounds { get; set; }
        public decimal VibratoryEquipment { get; set; }
        public decimal BlastEquipment { get; set; }
        public decimal Equipment { get; set; }
        public decimal Total { get
            {
                return (Tumbling + Blasting + Chemtrol +
                    VibratoryMedia + BlastMedia +
                    SpareParts + SupplierCompounds +
                    VibratoryEquipment + BlastEquipment +
                    Equipment);
            } 
            set 
            { 
            } 
        }
    }
}
