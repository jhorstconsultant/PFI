//using Mongoose.Forms;
using Mongoose.IDO;
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
//using static Mongoose.Forms.ExportSettings;

namespace PFI.Reporting.BL
{
    public class WeeklySalesReportBL
    {
        public WeeklySalesReportBL(IIDOCommands context) 
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
            SalesPersons = DataAccess.SLSlsmanAlls(SiteRef);
            StartingSalesPerson = startingSalesPerson;
            EndingSalesPerson = endingSalesPerson;

            if (string.IsNullOrWhiteSpace(StartingSalesPerson))
                StartingSalesPerson = SalesPersons.Select(s => s.Slsman).Min();

            if (string.IsNullOrWhiteSpace(EndingSalesPerson))
                EndingSalesPerson = SalesPersons.Select(s => s.Slsman).Max();

            StartingSalesPerson = StartingSalesPerson.ToLower();
            EndingSalesPerson = EndingSalesPerson.ToLower();
        }

        public void SetSalesPersonRange()
        {
            SalesPersons = DataAccess.SLSlsmanAlls(SiteRef);

            if (string.IsNullOrWhiteSpace(StartingSalesPerson))
                StartingSalesPerson = SalesPersons.Select(s => s.Slsman).Min();

            if (string.IsNullOrWhiteSpace(EndingSalesPerson))
                EndingSalesPerson = SalesPersons.Select(s => s.Slsman).Max();
        }

        public SLSlsmanAll[] SalesPersons { get; set; }
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
            ue_PFI_FCToFCCategoryMaps[] maps;

            maps = DataAccess.ue_PFI_FCToFCCategoryMaps();

            return maps.Select(s=>new Tuple<string,string>(s.FamilyCode,s.FamilyCodeCategory)).Distinct().ToArray();
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

            try
            {

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
            }catch(Exception ex) 
            {
                result = 0;
                throw new Exception("Error in GetCategoryTotal");
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
        public string GetSalespersonEmailBody(DateTime priorWeek, string salesPersonId)
        {
            DateTime startingWeek;
            DateTime endingWeek;
            DateTime currWeek;
            DataTable dt = null;
            String salesPersonName = String.Empty;
            SalespersonEmailWeek[] salespersonEmailWeeks;
            string html;
            SLSlsmanAll salesPerson;

            try
            {
                //C# is case sensitive.  I am lower casing everything in DB so need to lower case here too.
                salesPersonId = salesPersonId.ToLower().Trim();

                //The 4-4-5 starts in october.  
                //To get the correct october we need to figure out if we passed october this year.
                if (DateTime.Now.Month >= 10)
                    startingWeek = new DateTime(DateTime.Now.Year, 10, 1);
                else
                    startingWeek = new DateTime((DateTime.Now.Year - 1), 10, 1);
                
                startingWeek = GetLastDayOfWeek(startingWeek);
                endingWeek = GetLastDayOfWeek(priorWeek);
                
                if (string.IsNullOrWhiteSpace(salesPersonId) == false)
                {
                    SetSalesPersonRange(salesPersonId, salesPersonId);
                    
                    salesPerson = SalesPersons.Where(w => w.Slsman.Equals(salesPersonId)).FirstOrDefault();

                    if (salesPerson != null)
                    {
                        if(string.IsNullOrWhiteSpace(salesPerson.DerSlsmanName) == false) 
                            salesPersonName = salesPerson.DerSlsmanName;
                        else
                            salesPersonName = salesPerson.Slsman.ToUpper();
                    }
                    else
                    {
                        salesPersonName = "Salesperson not found";
                    }
                }
                
                dt = SetupDataTable(); //Encapsulated the DT setup for readability.
                                
                //Load the result data into a DataTable
                currWeek = startingWeek;
                while (currWeek <= endingWeek)
                {
                    dt.Rows.Add(CreateNewRow(dt, currWeek));
                    currWeek = currWeek.AddDays(7);
                }

                //Convert DataTable into an array of models
                salespersonEmailWeeks = GetSalespersonEmailWeeks(dt, startingWeek);

                //convert model array into html email body.
                html = GetEmailHTML(salesPersonName, endingWeek, salespersonEmailWeeks);

            }
            catch(Exception ex) 
            {
                throw new Exception($"Error in GetSalespersonEmailBody!! {ex.Message}");
            }

            return html;
        }
        public string GetSalespersonEmailBody(DateTime priorWeek)
        {
            DateTime startingWeek;
            DateTime endingWeek;
            DateTime currWeek;
            DataTable dt = null;
            SalespersonEmailWeek[] salespersonEmailWeeks;
            string html;

            //The 4-4-5 starts in october.  
            //To get the correct october we need to figure out if we passed october this year.
            if (DateTime.Now.Month >= 10)
                startingWeek = new DateTime(DateTime.Now.Year, 10, 1);
            else
                startingWeek = new DateTime((DateTime.Now.Year - 1), 10, 1);

            startingWeek = GetLastDayOfWeek(startingWeek);
            endingWeek = GetLastDayOfWeek(priorWeek);

            SetSalesPersonRange();

            dt = SetupDataTable(); //Encapsulated the DT setup for readability.

            //Load the result data into a DataTable
            currWeek = startingWeek;
            while (currWeek <= endingWeek)
            {
                dt.Rows.Add(CreateNewRow(dt, currWeek));
                currWeek = currWeek.AddDays(7);
            }

            //Convert DataTable into an array of models
            salespersonEmailWeeks = GetSalespersonEmailWeeks(dt, startingWeek);

            //convert model array into html email body.
            html = GetEmailHTML(String.Empty, endingWeek, salespersonEmailWeeks);

            return html;
        }
        private DateTime[] GetFourFourFive(DateTime startDate)
        {
            List<DateTime> fourFourFiveSeq;
            DateTime fourFourFive;

            //Fill in the fourFourFiveSeq             
            fourFourFiveSeq = new List<DateTime>();
            fourFourFive = startDate;
            fourFourFiveSeq.Add(fourFourFive); // 10 / 07 / 2023
            fourFourFive = fourFourFive.AddDays(28);
            fourFourFiveSeq.Add(fourFourFive); // 11 / 04 / 2023
            fourFourFive = fourFourFive.AddDays(28);
            fourFourFiveSeq.Add(fourFourFive); // 12 / 02 / 2023
            fourFourFive = fourFourFive.AddDays(35);
            fourFourFiveSeq.Add(fourFourFive); // 01 / 06 / 2024

            fourFourFive = fourFourFive.AddDays(28);
            fourFourFiveSeq.Add(fourFourFive); // 02 / 03 / 2024 
            fourFourFive = fourFourFive.AddDays(28);
            fourFourFiveSeq.Add(fourFourFive); // 03 / 02 / 2024 
            fourFourFive = fourFourFive.AddDays(35);
            fourFourFiveSeq.Add(fourFourFive); // 04 / 06 / 2024 

            fourFourFive = fourFourFive.AddDays(28);
            fourFourFiveSeq.Add(fourFourFive); // 05 / 04 / 2024 
            fourFourFive = fourFourFive.AddDays(28);
            fourFourFiveSeq.Add(fourFourFive); // 06 / 01 / 2024 
            fourFourFive = fourFourFive.AddDays(35);
            fourFourFiveSeq.Add(fourFourFive); // 07 / 06 / 2024 

            fourFourFive = fourFourFive.AddDays(28);
            fourFourFiveSeq.Add(fourFourFive); // 08 / 03 / 2024  
            fourFourFive = fourFourFive.AddDays(28);
            fourFourFiveSeq.Add(fourFourFive.AddDays(28)); // 08 / 31 / 2024 
            fourFourFive = fourFourFive.AddDays(35);
            fourFourFiveSeq.Add(fourFourFive); // 10 / 05 / 2024 

            return fourFourFiveSeq.ToArray();
        }
        private SalespersonEmailWeek[] GetSalespersonEmailWeeks(DataTable dt, DateTime startingWeek)
        {
            List<SalespersonEmailWeek> salespersonEmailWeeks;
            DateTime[] fourFourFiveSeq;

            fourFourFiveSeq = GetFourFourFive(startingWeek); //Using october as the start of the year load an array with 4-4-5 week dates.
            salespersonEmailWeeks = new List<SalespersonEmailWeek>();

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    salespersonEmailWeeks.Add(new SalespersonEmailWeek()
                    {
                        WeekDate = (DateTime)row["ue_CLM_Weekending"],
                        Tumbling = (Decimal)row["ue_CLM_Tumbling"],
                        Blasting = (Decimal)row["ue_CLM_Blasting"],
                        Chemtrol = (Decimal)row["ue_CLM_Chemtrol"],
                        VibratoryMedia = (Decimal)row["ue_CLM_VibratoryMedia"],
                        BlastMedia = (Decimal)row["ue_CLM_BlastMedia"],
                        SpareParts = (Decimal)row["ue_CLM_SpareParts"],
                        SupplierCompounds = (Decimal)row["ue_CLM_SupplierCompounds"],
                        VibratoryEquipment = (Decimal)row["ue_CLM_VibratoryEquipment"],
                        BlastEquipment = (Decimal)row["ue_CLM_BlastEquipment"],
                        Equipment = (Decimal)row["ue_CLM_Equipment"]
                    });
                }
            }

            foreach (DateTime d in fourFourFiveSeq.OrderBy(o => o))
            {
                foreach (SalespersonEmailWeek sew in salespersonEmailWeeks.OrderBy(o => o.WeekDate))
                {
                    if (sew.WeekDate >= d)
                    {
                        sew.FourFourFiveDate = d;
                    }
                }
            }

            return salespersonEmailWeeks.ToArray();
        }
        private string GetEmailHTML(string salesPersonName, DateTime endingWeek, SalespersonEmailWeek[] salespersonEmailWeeks)
        {
            StringBuilder sb = new StringBuilder();
            SalespersonEmailWeek[] comparison;
            DateTime earliestDate;

            comparison = GetEmailComparision(salespersonEmailWeeks);

            earliestDate = salespersonEmailWeeks.Select(s => s.FourFourFiveDate).Min();

            sb.AppendLine("<style>");
            sb.AppendLine("\ttable {");
            sb.AppendLine("\t\tfont-family: arial, sans-serif;");
            sb.AppendLine("\t\tborder-collapse: collapse;");
            sb.AppendLine("\t\twidth: 100%;");
            sb.AppendLine("\t}");
            sb.AppendLine("");
            sb.AppendLine("\th1, h2 {");
            sb.AppendLine("\t\tfont-family: arial, sans-serif;");
            sb.AppendLine("\t\tborder-collapse: collapse;");
            sb.AppendLine("\t\twidth: 100%;");
            sb.AppendLine("\t}");
            sb.AppendLine("\ttd, th {");
            sb.AppendLine("\t\tborder: 1px solid #dddddd;");
            sb.AppendLine("\t\ttext-align: left;");
            sb.AppendLine("\t\tpadding: 8px;");
            sb.AppendLine("\t}");
            sb.AppendLine("");
            sb.AppendLine("\t.total {");
            sb.AppendLine("\t\tborder-top: 3x solid #000000; background-color: #ffffff; font-weight: bold;");
            sb.AppendLine("\t}");
            sb.AppendLine("");
            sb.AppendLine("\ttr:nth-child(even) {");
            sb.AppendLine("\t\tbackground-color: #dddddd;");
            sb.AppendLine("\t}");
            sb.AppendLine("\t.seperator  {");
            sb.AppendLine("\t\tbackground-color: #ffffff;");
            sb.AppendLine("\t}");
            sb.AppendLine("</style>");

            if (!string.IsNullOrWhiteSpace(salesPersonName))
                sb.AppendLine($"<h1><b>Salesperson:</b> {salesPersonName}</h1>");
            else
                sb.AppendLine($"<h1><b>All</b></h1>");

            sb.AppendLine($"<h2><b>Weekending:</b> {endingWeek.ToString("yyyy-MM-dd")}</h2>");

            sb.AppendLine("<table>");

            foreach (DateTime fff in salespersonEmailWeeks.OrderByDescending(o => o.WeekDate).Select(s=>s.FourFourFiveDate).Distinct())
            {
                sb.AppendLine("\t<tr>");
                sb.AppendLine("\t\t<th>W/E</th>");
                sb.AppendLine("\t\t<th>Tumbling</th>");
                sb.AppendLine("\t\t<th>Blasting</th>");
                sb.AppendLine("\t\t<th>Chemtrol</th>");
                sb.AppendLine("\t\t<th>Vibratory Media</th>");
                sb.AppendLine("\t\t<th>Blast Media</th>");
                sb.AppendLine("\t\t<th>Spare Parts</th>");
                sb.AppendLine("\t\t<th>Supplier Compounds</th>");
                sb.AppendLine("\t\t<th>Vibratory Equipment</th>");
                sb.AppendLine("\t\t<th>Blast Equipment</th>");
                sb.AppendLine("\t\t<th>Equipment</th>");
                sb.AppendLine("\t\t<th>TOTAL W/E</th>");
                sb.AppendLine("\t</tr>");
                
                foreach (SalespersonEmailWeek sew in salespersonEmailWeeks.Where(w=>w.FourFourFiveDate == fff).OrderByDescending(o => o.WeekDate))
                {
                    sb.AppendLine("\t<tr>");
                    sb.AppendLine($"\t<td>{sew.WeekDate.ToString("yyyy-MM-dd")}</td>");
                    sb.AppendLine($"\t<td>{FormatCurrency(sew.Tumbling)}</td>");
                    sb.AppendLine($"\t<td>{FormatCurrency(sew.Blasting)}</td>");
                    sb.AppendLine($"\t<td>{FormatCurrency(sew.Chemtrol)}</td>");
                    sb.AppendLine($"\t<td>{FormatCurrency(sew.VibratoryMedia)}</td>");
                    sb.AppendLine($"\t<td>{FormatCurrency(sew.BlastMedia)}</td>");
                    sb.AppendLine($"\t<td>{FormatCurrency(sew.SpareParts)}</td>");
                    sb.AppendLine($"\t<td>{FormatCurrency(sew.SupplierCompounds)}</td>");
                    sb.AppendLine($"\t<td>{FormatCurrency(sew.VibratoryEquipment)}</td>");
                    sb.AppendLine($"\t<td>{FormatCurrency(sew.BlastEquipment)}</td>");
                    sb.AppendLine($"\t<td>{FormatCurrency(sew.Equipment)}</td>");
                    sb.AppendLine($"\t<td>{FormatCurrency(sew.Total)}</td>");
                    sb.AppendLine("\t</tr>");
                }

                sb.AppendLine("\t<tr>");
                sb.AppendLine($"\t<td style='background-color: #ffffff'></td>");
                sb.AppendLine($"\t<td class='total'>{FormatCurrency(salespersonEmailWeeks.Where(w=>w.FourFourFiveDate == fff).Sum(s=>s.Tumbling))}</td>");
                sb.AppendLine($"\t<td class='total'>{FormatCurrency(salespersonEmailWeeks.Where(w=>w.FourFourFiveDate == fff).Sum(s=>s.Blasting))}</td>");
                sb.AppendLine($"\t<td class='total'>{FormatCurrency(salespersonEmailWeeks.Where(w=>w.FourFourFiveDate == fff).Sum(s=>s.Chemtrol))}</td>");
                sb.AppendLine($"\t<td class='total'>{FormatCurrency(salespersonEmailWeeks.Where(w=>w.FourFourFiveDate == fff).Sum(s=>s.VibratoryMedia))}</td>");
                sb.AppendLine($"\t<td class='total'>{FormatCurrency(salespersonEmailWeeks.Where(w=>w.FourFourFiveDate == fff).Sum(s=>s.BlastMedia))}</td>");
                sb.AppendLine($"\t<td class='total'>{FormatCurrency(salespersonEmailWeeks.Where(w=>w.FourFourFiveDate == fff).Sum(s=>s.SpareParts))}</td>");
                sb.AppendLine($"\t<td class='total'>{FormatCurrency(salespersonEmailWeeks.Where(w=>w.FourFourFiveDate == fff).Sum(s=>s.SupplierCompounds))}</td>");
                sb.AppendLine($"\t<td class='total'>{FormatCurrency(salespersonEmailWeeks.Where(w=>w.FourFourFiveDate == fff).Sum(s=>s.VibratoryEquipment))}</td>");
                sb.AppendLine($"\t<td class='total'>{FormatCurrency(salespersonEmailWeeks.Where(w=>w.FourFourFiveDate == fff).Sum(s=>s.BlastEquipment))}</td>");
                sb.AppendLine($"\t<td class='total'>{FormatCurrency(salespersonEmailWeeks.Where(w=>w.FourFourFiveDate == fff).Sum(s=>s.Equipment))}</td>");
                sb.AppendLine($"\t<td class='total'>{FormatCurrency(salespersonEmailWeeks.Where(w=>w.FourFourFiveDate == fff).Sum(s=>s.Total))}</td>");
                sb.AppendLine("\t</tr>");

                //We don't want to show the comparison section if it is the last or bottom of the email.
                //     Nothing to compare too!
                if (earliestDate != fff)
                {
                    sb.AppendLine("\t<tr>");
                    sb.AppendLine("\t<td class='seperator' colspan='12'></td>");
                    sb.AppendLine("\t</tr>");

                    sb.AppendLine("\t<tr>");
                    sb.AppendLine($"\t<td class='seperator' style='font-weight: bold'>Comparison</td>");
                    sb.AppendLine(GetStyledComparisonLine(comparison.Where(w => w.FourFourFiveDate == fff).Sum(s => s.Tumbling)));
                    sb.AppendLine(GetStyledComparisonLine(comparison.Where(w => w.FourFourFiveDate == fff).Sum(s => s.Blasting)));
                    sb.AppendLine(GetStyledComparisonLine(comparison.Where(w => w.FourFourFiveDate == fff).Sum(s => s.Chemtrol)));
                    sb.AppendLine(GetStyledComparisonLine(comparison.Where(w => w.FourFourFiveDate == fff).Sum(s => s.VibratoryMedia)));
                    sb.AppendLine(GetStyledComparisonLine(comparison.Where(w => w.FourFourFiveDate == fff).Sum(s => s.BlastMedia)));
                    sb.AppendLine(GetStyledComparisonLine(comparison.Where(w => w.FourFourFiveDate == fff).Sum(s => s.SpareParts)));
                    sb.AppendLine(GetStyledComparisonLine(comparison.Where(w => w.FourFourFiveDate == fff).Sum(s => s.SupplierCompounds)));
                    sb.AppendLine(GetStyledComparisonLine(comparison.Where(w => w.FourFourFiveDate == fff).Sum(s => s.VibratoryEquipment)));
                    sb.AppendLine(GetStyledComparisonLine(comparison.Where(w => w.FourFourFiveDate == fff).Sum(s => s.BlastEquipment)));
                    sb.AppendLine(GetStyledComparisonLine(comparison.Where(w => w.FourFourFiveDate == fff).Sum(s => s.Equipment)));
                    sb.AppendLine(GetStyledComparisonLine(comparison.Where(w => w.FourFourFiveDate == fff).Sum(s => s.Total)));
                    sb.AppendLine("\t</tr>");
                }

                sb.AppendLine("\t<tr>");
                sb.AppendLine("\t<td class='seperator' colspan='12'></td>");
                sb.AppendLine("\t</tr>");
            }

            sb.AppendLine("</table>");

            return sb.ToString();
        }
        private string FormatCurrency(Decimal amount)
        {
            if (amount == 0)
                return "$ - ";
            else
                return amount.ToString("C");
        }
        private SalespersonEmailWeek[] GetEmailComparision(SalespersonEmailWeek[] salespersonEmailWeeks)
        {
            List<SalespersonEmailWeek> results = new List<SalespersonEmailWeek>();

            foreach (DateTime fff in salespersonEmailWeeks.OrderByDescending(o => o.WeekDate).Select(s => s.FourFourFiveDate).Distinct().OrderBy(o=>o))
            {
                results.Add(new SalespersonEmailWeek() {
                    WeekDate = fff,
                    FourFourFiveDate = fff,
                    Tumbling = salespersonEmailWeeks.Where(w => w.FourFourFiveDate == fff).Sum(s => s.Tumbling),
                    Blasting = salespersonEmailWeeks.Where(w => w.FourFourFiveDate == fff).Sum(s => s.Blasting),
                    Chemtrol = salespersonEmailWeeks.Where(w => w.FourFourFiveDate == fff).Sum(s => s.Chemtrol),
                    VibratoryMedia = salespersonEmailWeeks.Where(w => w.FourFourFiveDate == fff).Sum(s => s.VibratoryMedia),
                    BlastMedia = salespersonEmailWeeks.Where(w => w.FourFourFiveDate == fff).Sum(s => s.BlastMedia),
                    SpareParts = salespersonEmailWeeks.Where(w => w.FourFourFiveDate == fff).Sum(s => s.SpareParts),
                    SupplierCompounds = salespersonEmailWeeks.Where(w => w.FourFourFiveDate == fff).Sum(s => s.SupplierCompounds),
                    VibratoryEquipment = salespersonEmailWeeks.Where(w => w.FourFourFiveDate == fff).Sum(s => s.VibratoryEquipment),
                    BlastEquipment = salespersonEmailWeeks.Where(w => w.FourFourFiveDate == fff).Sum(s => s.BlastEquipment),
                    Equipment = salespersonEmailWeeks.Where(w => w.FourFourFiveDate == fff).Sum(s => s.Equipment)
                });
            }

            results = results.OrderByDescending(o=>o.FourFourFiveDate).ToList();

            for (int i = 0; i < (results.Count - 1); i++)
            {
                results[i].Tumbling = results[i].Tumbling - results[(i + 1)].Tumbling;
                results[i].Blasting = results[i].Blasting - results[(i + 1)].Blasting;
                results[i].Chemtrol = results[i].Chemtrol - results[(i + 1)].Chemtrol;
                results[i].VibratoryMedia = results[i].VibratoryMedia - results[(i + 1)].VibratoryMedia;
                results[i].BlastMedia = results[i].BlastMedia - results[(i + 1)].BlastMedia;
                results[i].SpareParts = results[i].SpareParts - results[(i + 1)].SpareParts;
                results[i].SupplierCompounds = results[i].SupplierCompounds - results[(i + 1)].SupplierCompounds;
                results[i].VibratoryEquipment = results[i].VibratoryEquipment - results[(i + 1)].VibratoryEquipment;
                results[i].BlastEquipment = results[i].BlastEquipment - results[(i + 1)].BlastEquipment;
                results[i].Equipment = results[i].Equipment - results[(i + 1)].Equipment;
            }

            return results.ToArray();
        }
        private string GetStyledComparisonLine(decimal compare)
        {
            if(compare > 0)
                return $"\t<td class='seperator' style='font-weight: bold; color:MediumSeaGreen;'>{FormatCurrency(compare)}</td>";
            else if(compare < 0)
                return $"\t<td class='seperator' style='font-weight: bold; color:Tomato;'>{FormatCurrency(compare)}</td>";
            else
                return $"\t<td class='seperator'>{FormatCurrency(compare)}</td>";
        }
    }
}
