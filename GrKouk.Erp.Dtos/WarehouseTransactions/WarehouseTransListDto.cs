using System;
using System.ComponentModel.DataAnnotations;
using GrKouk.Erp.Definitions;

namespace GrKouk.Erp.Dtos.WarehouseTransactions
{
    public class WarehouseTransListDto
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:ddd dd MMM yyyy}")]
        [Display(Name = "Date")]
        public DateTime TransDate { get; set; }

        public int TransWarehouseDocSeriesId { get; set; }
        [Display(Name = "Series")]
        public string TransWarehouseDocSeriesName { get; set; }
        [Display(Name = "Series")]
        public string TransWarehouseDocSeriesCode { get; set; }
        [Display(Name = "Ref.")]
        public string TransRefCode { get; set; }
        [Display(Name = "Section")]
        public string SectionCode { get; set; }
        public int CreatorId { get; set; }
        public int WarehouseItemId { get; set; }
        [Display(Name = "WarehouseItem")]
        public string WarehouseItemName { get; set; }

        public WarehouseTransactionTypeEnum TransactionType { get; set; }

        public InventoryActionEnum InventoryAction { get; set; }
        public InventoryValueActionEnum InventoryValueAction { get; set; }
        //Added 2019-------------
        public InventoryActionEnum InvoicedVolumeAction { get; set; }
        public InventoryValueActionEnum InvoicedValueAction { get; set; }
        //----------------------
        public double Quontity1 { get; set; }
        public double Quontity2 { get; set; }
  // Ονομαστική τιμή: η αρχική τιμή που έχει οριστεί χωρίς εκπτώσεις /επιβαρύνσεις.
        [Display(Name = "Τιμή Μονάδας")]
        public decimal UnitPrice { get; set; }
        [Display(Name = "Εξοδα Μονάδος")]
        public decimal UnitExpenses { get; set; }
        // Τιμή μείον έκπτωση: η τιμή μείον την έκπτωση γραμμής.
        [Display(Name = "Τιμή μετά Εκπτ.")]
        public decimal UnitPriceAfterDiscount => InvoicedQ1==0?0: InvoicedValueNetto / InvoicedQ1;

        // Ανηγμένη τιμή: η τιμή μείον εκπτώσεις, συν τυχόν επιβαρύνσεις.
        [Display(Name = "Ανηγμένη Τιμή")]
        public decimal UnitPriceFinal { get; set; }
        
        public decimal AmountFpa { get; set; }
        public decimal AmountNet { get; set; }

        public decimal AmountDiscount { get; set; }
        public decimal TransQ1 { get; set; }
        public decimal TransQ2 { get; set; }
        public decimal TransFpaAmount { get; set; }
        public decimal TransNetAmount { get; set; }
        public decimal TransDiscountAmount { get; set; }
        public decimal TotalAmount
        {
            get => TransNetAmount + TransFpaAmount - TransDiscountAmount;
        }
        public decimal InvoicedQ1 =>
            (InvoicedVolumeAction.Equals(InventoryActionEnum.InventoryActionEnumImport) ||
             InvoicedVolumeAction.Equals(InventoryActionEnum.InventoryActionEnumExport)
                ? (decimal)Quontity1
                : ((InvoicedVolumeAction.Equals(InventoryActionEnum.InventoryActionEnumNegativeImport) ||
                    InvoicedVolumeAction.Equals(InventoryActionEnum.InventoryActionEnumNegativeExport)
                        ? (decimal)(-1 * Quontity1)
                        : 0)
                )
            );
        public decimal InvoicedQ2 =>
            (InvoicedVolumeAction.Equals( InventoryActionEnum.InventoryActionEnumImport) ||
             InvoicedVolumeAction.Equals( InventoryActionEnum.InventoryActionEnumExport)
                ? (decimal) Quontity2
                : ((InvoicedVolumeAction.Equals( InventoryActionEnum.InventoryActionEnumNegativeImport) ||
                    InvoicedVolumeAction.Equals( InventoryActionEnum.InventoryActionEnumNegativeExport) 
                        ? (decimal) (-1*Quontity2)
                        : 0)
                )
            );
        [Display(Name = "Αξία Τιμολόγησης")]
        public decimal InvoicedValue
        {
            get
            {
                decimal ret = 0;
                if (InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumIncrease) || 
                    InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumDecrease))
                {
                    ret = AmountNet + AmountFpa - AmountDiscount;
                }
                else if(InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumNegativeIncrease) ||
                        InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumNegativeDecrease))
                {
                    ret = (AmountNet + AmountFpa - AmountDiscount) * -1;

                }
                else
                {
                    ret = 0;
                }

                return ret;

                #region CommentOut

                //return (InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumIncrease) ||
                //        InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumDecrease)
                //        ? AmountNet + AmountFpa - AmountDiscount
                //        : ((InvoicedValueAction.Equals(
                //                InventoryValueActionEnum.InventoryValueActionEnumNegativeIncrease) ||
                //            InvoicedValueAction.Equals(
                //                InventoryValueActionEnum.InventoryValueActionEnumNegativeDecrease)
                //                ? (AmountNet + AmountFpa - AmountDiscount) * -1
                //                : 0)
                //        )
                //    );

                #endregion
            }
        }
        public decimal InvoicedValueNetto
        {
            get
            {
                decimal ret = 0;
                if (InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumIncrease) ||
                    InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumDecrease))
                {
                    ret = AmountNet  - AmountDiscount;
                }
                else if (InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumNegativeIncrease) ||
                         InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumNegativeDecrease))
                {
                    ret = (AmountNet - AmountDiscount) * -1;

                }
                else
                {
                    ret = 0;
                }

                return ret;

            }
        }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Import Value")]
        public decimal ImportAmount
        {
            get => (InventoryValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumIncrease) ||
                    InventoryValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumNegativeIncrease)
                ? TotalAmount
                : 0);

        }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Export Value")]
        public decimal ExportAmount => (InventoryValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumDecrease) ||
                                        InventoryValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumNegativeDecrease)
            ? TotalAmount
            : 0);



        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Import Units")]
        public decimal ImportUnits
        {
            get => (InventoryAction.Equals(InventoryActionEnum.InventoryActionEnumImport) ||
                    InventoryAction.Equals(InventoryActionEnum.InventoryActionEnumNegativeImport)
                ? TransQ1
                : 0);

        }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Export Units")]
        public decimal ExportUnits => (InventoryAction.Equals(InventoryActionEnum.InventoryActionEnumExport) ||
                                       InventoryAction.Equals(InventoryActionEnum.InventoryActionEnumNegativeExport)
            ? TransQ1
            : 0);

        //-2019
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Inv.Imp.Value")]
        public decimal InvoicedImportAmount
        {
            get => (InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumIncrease) ||
                    InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumNegativeIncrease)
                ? this.InvoicedValue
                : 0);

        }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Inv.Exp.Value")]
        public decimal InvoicedExportAmount => (InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumDecrease) ||
                                                InvoicedValueAction.Equals(InventoryValueActionEnum.InventoryValueActionEnumNegativeDecrease)
            ? this.InvoicedValue
            : 0);



        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Inv.Imp.Units")]
        public decimal InvoicedImportUnits
        {
            get => (InvoicedVolumeAction.Equals(InventoryActionEnum.InventoryActionEnumImport) ||
                    InvoicedVolumeAction.Equals(InventoryActionEnum.InventoryActionEnumNegativeImport)
                ? InvoicedQ1
                : 0);

        }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Inv.Exp.Units")]
        public decimal InvoicedExportUnits => (InvoicedVolumeAction.Equals(InventoryActionEnum.InventoryActionEnumExport) ||
                                               InvoicedVolumeAction.Equals(InventoryActionEnum.InventoryActionEnumNegativeExport)
            ? InvoicedQ1
            : 0);
        //------


        public int CompanyId { get; set; }
        [Display(Name = "Company")]
        public string CompanyCode { get; set; }
        public int CompanyCurrencyId { get; set; }
        //TODO: Να δώ το θέμα με τις αξίες Trans* κάτι. γιατί όταν αποθηκεύετε μπορεί να είναι αρνητικό. 
        //Αυτές οι αξίες δεν μπορούν να χρησιμοποιηθούν ως αξίες για τις τιμολογημένες τιμές. 
    }
}
