using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrKouk.Web.ERP.Helpers
{
     public class DateFilterDates
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string FilterName { get; set; }

    }
    public static class DateFilter
    {
        public static List<SelectListItem> GetDateFiltersSelectList()
        {
            var thisYear=DateTime.Today.Year;
            int twoYearsAgo = thisYear - 2;
            int threeYearsAgo = thisYear - 3;
            List<SelectListItem> datePeriods = new List<SelectListItem>
            {
                new SelectListItem() {Value = "CURMONTH", Text = "Τρέχων Μήνας"},
                new SelectListItem() {Value = "30DAYS", Text = "30 Ημέρες"},
                new SelectListItem() {Value = "60DAYS", Text = "60 Ημέρες"},
                new SelectListItem() {Value = "90DAYS", Text = "90 Ημέρες"},
                new SelectListItem() {Value = "360DAYS", Text = "360 Ημέρες"},
                new SelectListItem() {Value = "PREMONTH", Text = "Προηγ.Μήνας"},
                new SelectListItem() {Value = "CURYEAR", Text = "Τρέχων Ετος"},
                new SelectListItem() {Value = "JANCURYEAR", Text = "Ιανουάριος"},
                new SelectListItem() {Value = "FEBCURYEAR", Text = "Φεβρουάριος"},
                new SelectListItem() {Value = "MARCURYEAR", Text = "Μάρτιος"},
                new SelectListItem() {Value = "APRCURYEAR", Text = "Απρίλιος"},
                new SelectListItem() {Value = "MAICURYEAR", Text = "Μάιος"},
                new SelectListItem() {Value = "JUNCURYEAR", Text = "Ιούνιος"},
                new SelectListItem() {Value = "JULCURYEAR", Text = "Ιούλιος"},
                new SelectListItem() {Value = "AUGCURYEAR", Text = "Αύγουστος"},
                new SelectListItem() {Value = "SEPCURYEAR", Text = "Σεπτέμβριος"},
                new SelectListItem() {Value = "OKTCURYEAR", Text = "Οκτώβριος"},
                new SelectListItem() {Value = "NOVCURYEAR", Text = "Νοέβριος"},
                new SelectListItem() {Value = "DECCURYEAR", Text = "Δεκέμβριος"},
                new SelectListItem() {Value = "LASTYEAR", Text = "Προηγ. Ετος"},
                new SelectListItem() {Value = "2YEARAGO", Text = twoYearsAgo.ToString()},
                new SelectListItem() {Value = "3YEARAGO", Text = threeYearsAgo.ToString()},
                //new SelectListItem() {Value = "JANLASTYEAR", Text = "Ιαν.Προηγ. Ετος"},
                //new SelectListItem() {Value = "FEBLASTYEAR", Text = "Φεβ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "MARLASTYEAR", Text = "Μαρ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "APRLASTYEAR", Text = "Απρ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "MAILASTYEAR", Text = "Μαι.Προηγ. Ετος"},
                //new SelectListItem() {Value = "JUNLASTYEAR", Text = "Ιουν.Προηγ. Ετος"},
                //new SelectListItem() {Value = "JULLASTYEAR", Text = "Ιουλ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "AUGLASTYEAR", Text = "Αυγ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "SEPLASTYEAR", Text = "Σεπ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "OKTLASTYEAR", Text = "Οκτ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "NOVLASTYEAR", Text = "Νοε.Προηγ. Ετος"},
                //new SelectListItem() {Value = "DECLASTYEAR", Text = "Δεκ.Προηγ. Ετος"},
                new SelectListItem() {Value = "ALL", Text = "{All}"}
            };
            return datePeriods;
        }
        public static List<SelectListItem> GetRecTransDateFiltersSelectList()
        {
            var thisYear = DateTime.Today.Year;
            int twoYearsAgo = thisYear - 2;
            int threeYearsAgo = thisYear - 3;
            List<SelectListItem> datePeriods = new List<SelectListItem>
            {
                new SelectListItem() {Value = "CURMONTH", Text = "Τρέχων Μήνας"},
                new SelectListItem() {Value = "30DAYS", Text = "Επόμενες 30 Ημέρες"},
                new SelectListItem() {Value = "60DAYS", Text = "Επόμενες 60 Ημέρες"},
                new SelectListItem() {Value = "90DAYS", Text = "Επόμενες 90 Ημέρες"},
                new SelectListItem() {Value = "360DAYS", Text = "Επόμενες 360 Ημέρες"},
                new SelectListItem() {Value = "PREMONTH", Text = "Προηγ.Μήνας"},
                new SelectListItem() {Value = "CURYEAR", Text = "Τρέχων Ετος"},
                new SelectListItem() {Value = "JANCURYEAR", Text = "Ιανουάριος"},
                new SelectListItem() {Value = "FEBCURYEAR", Text = "Φεβρουάριος"},
                new SelectListItem() {Value = "MARCURYEAR", Text = "Μάρτιος"},
                new SelectListItem() {Value = "APRCURYEAR", Text = "Απρίλιος"},
                new SelectListItem() {Value = "MAICURYEAR", Text = "Μάιος"},
                new SelectListItem() {Value = "JUNCURYEAR", Text = "Ιούνιος"},
                new SelectListItem() {Value = "JULCURYEAR", Text = "Ιούλιος"},
                new SelectListItem() {Value = "AUGCURYEAR", Text = "Αύγουστος"},
                new SelectListItem() {Value = "SEPCURYEAR", Text = "Σεπτέμβριος"},
                new SelectListItem() {Value = "OKTCURYEAR", Text = "Οκτώβριος"},
                new SelectListItem() {Value = "NOVCURYEAR", Text = "Νοέβριος"},
                new SelectListItem() {Value = "DECCURYEAR", Text = "Δεκέμβριος"},
                new SelectListItem() {Value = "LASTYEAR", Text = "Προηγ. Ετος"},
                new SelectListItem() {Value = "2YEARAGO", Text = twoYearsAgo.ToString()},
                new SelectListItem() {Value = "3YEARAGO", Text = threeYearsAgo.ToString()},
                //new SelectListItem() {Value = "JANLASTYEAR", Text = "Ιαν.Προηγ. Ετος"},
                //new SelectListItem() {Value = "FEBLASTYEAR", Text = "Φεβ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "MARLASTYEAR", Text = "Μαρ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "APRLASTYEAR", Text = "Απρ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "MAILASTYEAR", Text = "Μαι.Προηγ. Ετος"},
                //new SelectListItem() {Value = "JUNLASTYEAR", Text = "Ιουν.Προηγ. Ετος"},
                //new SelectListItem() {Value = "JULLASTYEAR", Text = "Ιουλ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "AUGLASTYEAR", Text = "Αυγ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "SEPLASTYEAR", Text = "Σεπ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "OKTLASTYEAR", Text = "Οκτ.Προηγ. Ετος"},
                //new SelectListItem() {Value = "NOVLASTYEAR", Text = "Νοε.Προηγ. Ετος"},
                //new SelectListItem() {Value = "DECLASTYEAR", Text = "Δεκ.Προηγ. Ετος"},
                new SelectListItem() {Value = "ALL", Text = "{All}"}
            };
            return datePeriods;
        }
        public static DateFilterDates GetDateFilterDates(string dateFilterName)
        {

            DateFilterDates dateFilter = new DateFilterDates
            {
                FilterName = dateFilterName
            };

            switch (dateFilterName)
            {
                case "CURMONTH":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    dateFilter.ToDate = DateTime.Now;
                    break;
                case "PREMONTH":
                    var m = DateTime.Now.Month;
                    var y = DateTime.Now.Year;
                    if (m>1)
                    {
                        m--;

                    }
                    else
                    {
                        m = 12;
                        y--;
                    }
                    dateFilter.FromDate = new DateTime(y, m, 1);
                    dateFilter.ToDate = new DateTime(y, m, DateTime.DaysInMonth(y, m));
                    break;
                case "30DAYS":
                    dateFilter.ToDate = DateTime.Now;
                    dateFilter.FromDate = dateFilter.ToDate.AddDays(-30);
                    break;
                case "60DAYS":
                    dateFilter.ToDate = DateTime.Now;
                    dateFilter.FromDate = dateFilter.ToDate.AddDays(-60);
                    break;
                case "90DAYS":
                    dateFilter.ToDate = DateTime.Now;
                    dateFilter.FromDate = dateFilter.ToDate.AddDays(-90);
                    break;
                case "360DAYS":
                    dateFilter.ToDate = DateTime.Now;
                    dateFilter.FromDate = dateFilter.ToDate.AddDays(-360);
                    break;
                case "CURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 1, 1);
                    dateFilter.ToDate = DateTime.Now;
                    break;
                case "2YEARAGO":
                    int thisYear = DateTime.Today.Year;
                    int fYear = thisYear - 2;
                    dateFilter.FromDate = new DateTime(fYear, 1, 1);
                    dateFilter.ToDate = new DateTime(fYear, 1, DateTime.DaysInMonth(fYear, 1));
                    break;
                case "3YEARAGO":
                    int thisYear1 = DateTime.Today.Year;
                    int fYear1 = thisYear1 - 3;
                    dateFilter.FromDate = new DateTime(fYear1, 1, 1);
                    dateFilter.ToDate = new DateTime(fYear1, 1, DateTime.DaysInMonth(fYear1, 1));
                    break;
                case "JANCURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 1, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 1, DateTime.DaysInMonth(DateTime.Now.Year, 1));
                    break;
              
                case "FEBCURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 2, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 2, DateTime.DaysInMonth(DateTime.Now.Year, 2));
                    break;
                case "MARCURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 3, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 3, DateTime.DaysInMonth(DateTime.Now.Year, 3));
                    break;
                case "APRCURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 4, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 4, DateTime.DaysInMonth(DateTime.Now.Year, 4));
                    break;
                case "MAICURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 5, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 5, DateTime.DaysInMonth(DateTime.Now.Year, 5));
                    break;
                case "JUNCURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 6, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 6, DateTime.DaysInMonth(DateTime.Now.Year, 6));
                    break;
                case "JULCURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 7, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 7, DateTime.DaysInMonth(DateTime.Now.Year, 7));
                    break;
                case "AUGCURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 8, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 8, DateTime.DaysInMonth(DateTime.Now.Year, 8));
                    break;
                case "SEPCURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 9, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 9, DateTime.DaysInMonth(DateTime.Now.Year, 9));
                    break;
                case "OKTCURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 10, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 10, DateTime.DaysInMonth(DateTime.Now.Year, 10));
                    break;
                case "NOVCURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 11, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 11, DateTime.DaysInMonth(DateTime.Now.Year, 11));
                    break;
                case "DECCURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 12, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 12, DateTime.DaysInMonth(DateTime.Now.Year, 12));
                    break;
                case "LASTYEAR":
                    
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 1, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 12, DateTime.DaysInMonth(DateTime.Now.Year-1, 12));
                    break;
                case "JANLASTYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 1, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 1, DateTime.DaysInMonth(DateTime.Now.Year-1, 1));
                    break;
                case "FEBLASTYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 2, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 2, DateTime.DaysInMonth(DateTime.Now.Year-1, 2));
                    break;
                case "MARLASTYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 3, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 3, DateTime.DaysInMonth(DateTime.Now.Year-1, 3));
                    break;
                case "APRLASTYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 4, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 4, DateTime.DaysInMonth(DateTime.Now.Year-1, 4));
                    break;
                case "MAILASTYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 5, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 5, DateTime.DaysInMonth(DateTime.Now.Year-1, 5));
                    break;
                case "JUNLASTYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 6, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 6, DateTime.DaysInMonth(DateTime.Now.Year-1, 6));
                    break;
                case "JULLASTYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 7, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 7, DateTime.DaysInMonth(DateTime.Now.Year-1, 7));
                    break;
                case "AUGLASTYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 8, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 8, DateTime.DaysInMonth(DateTime.Now.Year-1, 8));
                    break;
                case "SEPLASTYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 9, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 9, DateTime.DaysInMonth(DateTime.Now.Year-1, 9));
                    break;
                case "OKTLASTYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 10, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 10, DateTime.DaysInMonth(DateTime.Now.Year-1, 10));
                    break;
                case "NOVLASTYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 11, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 11, DateTime.DaysInMonth(DateTime.Now.Year-1, 11));
                    break;
                case "DECLASTYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year-1, 12, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year-1, 12, DateTime.DaysInMonth(DateTime.Now.Year-1, 12));
                    break;
                case "ALL":
                    dateFilter.FromDate = new DateTime(1966, 8, 1);
                    dateFilter.ToDate =  DateTime.MaxValue;
                    break;
                default:
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    dateFilter.ToDate = DateTime.Now;
                    dateFilter.FilterName = "CURMONTH";
                    break;

            }
            return dateFilter;
        }
         public static DateFilterDates GetRecTransDateFilterDates(string dateFilterName)
        {

            DateFilterDates dateFilter = new DateFilterDates
            {
                FilterName = dateFilterName
            };

            switch (dateFilterName)
            {
                case "CURMONTH":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    var cM = DateTime.Now.Month;
                    var cY = DateTime.Now.Year;
                    var daysInCurMonth = DateTime.DaysInMonth(cY, cM);
                    dateFilter.ToDate = new DateTime(cY, cM, daysInCurMonth);;
                    break;
                case "PREMONTH":
                    var m = DateTime.Now.Month;
                    var y = DateTime.Now.Year;
                    if (m>1)
                    {
                        m--;

                    }
                    else
                    {
                        m = 12;
                        y--;
                    }
                    dateFilter.FromDate = new DateTime(y, m, 1);
                    dateFilter.ToDate = new DateTime(y, m, DateTime.DaysInMonth(y, m));
                    break;
                case "30DAYS":
                    dateFilter.FromDate = DateTime.Now;
                    dateFilter.ToDate = dateFilter.FromDate.AddDays(30);
                    break;
                case "60DAYS":
                    dateFilter.FromDate = DateTime.Now;
                    dateFilter.ToDate = dateFilter.FromDate.AddDays(60);
                    break;
                case "90DAYS":
                    dateFilter.FromDate = DateTime.Now;;
                    dateFilter.ToDate = dateFilter.FromDate.AddDays(90);
                    break;
                case "360DAYS":
                    dateFilter.FromDate = DateTime.Now;
                    dateFilter.ToDate = dateFilter.FromDate.AddDays(360);
                    break;
                case "CURYEAR":
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, 1, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 12, 31);
                    break;
                case "LASTYEAR":
                    var ty = DateTime.Now.Year;
                    ty--;
                    dateFilter.FromDate = new DateTime(ty, 1, 1);
                    dateFilter.ToDate = new DateTime(ty, 12, DateTime.DaysInMonth(ty, 12));
                    break;
                case "ALL":
                    dateFilter.FromDate = new DateTime(1966, 8, 1);
                    dateFilter.ToDate =  DateTime.MaxValue;
                    break;
                default:
                    dateFilter.FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    dateFilter.ToDate = new DateTime(DateTime.Now.Year, 1, 1);;
                    dateFilter.FilterName = "CURYEAR";
                    break;

            }
            return dateFilter;
        }
    }
}
