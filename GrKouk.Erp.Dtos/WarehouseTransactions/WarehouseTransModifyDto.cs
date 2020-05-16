using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.WarehouseTransactions
{
    public class WarehouseTransModifyDto
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime TransDate { get; set; }
        [Display(Name = "Series")]
        public int TransWarehouseDocSeriesId { get; set; }
        [Display(Name = "Ref")]
        public string TransRefCode { get; set; }
        [Display(Name = "WarehouseItem")]
        public int WarehouseItemId { get; set; }
        [Display(Name = "Fiscal Period")]
        public int FiscalPeriodId { get; set; }
        public WarehouseTransactionTypeEnum TransactionType { get; set; }
        public InventoryActionEnum InventoryAction { get; set; }
        public InventoryValueActionEnum InventoryValueAction { get; set; }
        public int SectionId { get; set; }
        [Display(Name = "Main Unit")]
        public int PrimaryUnitId { get; set; }
        [Display(Name = "Sec.Unit")]
        public int SecondaryUnitId { get; set; }
        public decimal UnitFactor { get; set; }
        public string PrimaryUnitCode { get; set; }
        public string SecondaryUnitCode { get; set; }
        [Display(Name = "Q1")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double Quontity1 { get; set; }
        [Display(Name = "Q2")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double Quontity2 { get; set; }
        [Display(Name = "Fpa Rate")]
        public decimal FpaRate { get; set; }
        [Display(Name = "Disc.Rate")]
        public decimal DiscountRate { get; set; }
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }
        /// <summary>
        /// UnitExpenses Είναι το ποσό που προέρχετε από τα επιμεριζόμενα έξοδα
        /// που καταχωρούντε στο τιμολόγιο αγοράς και επιβαρ'υνει την κοστολόγηση του είδους
        /// Δεν επηρεάζει τα νούμερα της κίνησης ούτε καταζχωρήται στην κίνηση.
        /// Υπολογίζεται μόνο από την κίνηση του τιμολογίου από τις γραμμές επιμεριζόμενων εξόδων
        /// </summary>
        [Display(Name = "Unit Expenses")]
        public decimal UnitExpenses { get; set; }
        [Display(Name = "Vat Amount")]
        public decimal AmountFpa { get; set; }
        [Display(Name = "Net Amount")]
        public decimal AmountNet { get; set; }
        [Display(Name = "Discount Amount")]
        public decimal AmountDiscount { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Sum Amount")]
        public decimal AmountSum => (AmountNet+AmountFpa-AmountDiscount);

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string Etiology { get; set; }
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
