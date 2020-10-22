using System;
using System.Collections.Generic;
using System.Text;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Domain.MediaEntities;
using GrKouk.Erp.Domain.RecurringTransactions;
using GrKouk.Erp.Domain.Shared;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Data
{
    public class ApiDbContext : IdentityDbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options)
        {
        }
         public DbSet<Company> Companies { get; set; }
        public DbSet<Transactor> Transactors { get; set; }
        public DbSet<FinDiaryTransaction> FinDiaryTransactions { get; set; }
        public DbSet<FinTransCategory> FinTransCategories { get; set; }
        public DbSet<TransactorType> TransactorTypes { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<FiscalPeriod> FiscalPeriods { get; set; }

        public DbSet<RevenueCentre> RevenueCentres { get; set; }
        public DbSet<CostCentre> CostCentres { get; set; }
        public DbSet<FinancialMovement> FinancialMovements { get; set; }

        public DbSet<FpaDef> FpaKategories { get; set; }
        public DbSet<TransWarehouseDef> TransWarehouseDefs { get; set; }
        public DbSet<TransWarehouseDocTypeDef> TransWarehouseDocTypeDefs { get; set; }
        public DbSet<TransWarehouseDocSeriesDef> TransWarehouseDocSeriesDefs { get; set; }

        public DbSet<MeasureUnit> MeasureUnits { get; set; }
        public DbSet<MaterialCategory> MaterialCategories { get; set; }
        public DbSet<WarehouseItem> WarehouseItems { get; set; }

        public DbSet<WarehouseTransaction> WarehouseTransactions { get; set; }

        public DbSet<BuyDocTypeDef> BuyDocTypeDefs { get; set; }
        public DbSet<BuyDocSeriesDef> BuyDocSeriesDefs { get; set; }
        public DbSet<BuyDocLine> BuyDocLines { get; set; }
        public DbSet<BuyDocument> BuyDocuments { get; set; }
        public DbSet<TransExpenseDef> TransExpenseDefs { get; set; }
        public DbSet<TransactorTransaction> TransactorTransactions { get; set; }
        public DbSet<TransTransactorDef> TransTransactorDefs { get; set; }
        public DbSet<TransTransactorDocTypeDef> TransTransactorDocTypeDefs { get; set; }
        public DbSet<TransTransactorDocSeriesDef> TransTransactorDocSeriesDefs { get; set; }
        public DbSet<SellDocTypeDef> SellDocTypeDefs { get; set; }
        public DbSet<SellDocSeriesDef> SellDocSeriesDefs { get; set; }
        public DbSet<SellDocLine> SellDocLines { get; set; }
        public DbSet<SellDocument> SellDocuments { get; set; }
       // public DbSet<WarehouseItemCode> WarehouseItemsCodes { get; set; }
        public DbSet<DiaryDef> DiaryDefs { get; set; }
        public DbSet<CashRegCategory> CashRegCategories { get; set; }
        public DbSet<ClientProfile> ClientProfiles { get; set; }
        public DbSet<CrCatWarehouseItem> CrCatWarehouseItems { get; set; }
        public DbSet<MediaEntry> MediaEntries { get; set; }
        public DbSet<ProductMedia> ProductMedia { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<ProductRecipeLine> ProductRecipeLines { get; set; }
        public DbSet<ProductRecipe> ProductRecipes { get; set; }
        public DbSet<GlobalSettings> GlobalSettings { get; set; }
        public DbSet<BuyDocTransPaymentMapping> BuyDocTransPaymentMappings { get; set; }
        public DbSet<SellDocTransPaymentMapping> SellDocTransPaymentMappings { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<TransactorCompanyMapping> TransactorCompanyMappings { get; set; }
        public DbSet<SalesChannel> SalesChannels { get; set; }
        public DbSet<WrItemCode> WrItemCodes { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<RecurringTransDocLine>  RecurringTransDocLines        { get; set; }
        public DbSet<RecurringTransDoc> RecurringTransDocs { get; set; }
        public DbSet<CompanyWarehouseItemMapping> CompanyWarehouseItemMappings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FinDiaryTransaction>().HasIndex(c => c.TransactionDate);

            modelBuilder.Entity<FinDiaryTransaction>()
                .HasOne(c => c.FinTransCategory)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinTransCategory>().HasIndex(c => c.Code);

            modelBuilder.Entity<FinDiaryTransaction>()
                .HasOne(en => en.Transactor)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinDiaryTransaction>()
                .HasOne(en => en.Company)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinDiaryTransaction>()
                .HasOne(en => en.CostCentre)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinDiaryTransaction>()
                .HasOne(en => en.RevenueCentre)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<FpaDef>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<FinancialMovement>()
                .HasIndex(c => c.Code)
                .IsUnique();


            modelBuilder.Entity<TransWarehouseDocTypeDef>()
                .HasOne(bd => bd.TransWarehouseDef)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransWarehouseDocSeriesDef>()
                .HasOne(bd => bd.TransWarehouseDocTypeDef)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransWarehouseDef>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<TransWarehouseDocTypeDef>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<TransWarehouseDocSeriesDef>()
                .HasIndex(c => c.Code)
                .IsUnique();


            modelBuilder.Entity<Section>(entity =>
            {
                entity.HasIndex(c => c.Code)
                    .IsUnique();
            });
            modelBuilder.Entity<FiscalPeriod>(entity =>
            {
                entity.HasIndex(c => c.Code)
                    .IsUnique();
            });

            modelBuilder.Entity<MeasureUnit>(entity =>
            {
                entity.HasIndex(p => p.Code).IsUnique();
            });

            modelBuilder.Entity<WarehouseItem>(entity =>
            {
                entity.HasIndex(c => c.Code).IsUnique();
                entity.HasOne(bd => bd.BuyMeasureUnit)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(bd => bd.SecondaryMeasureUnit)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(bd => bd.MainMeasureUnit)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(bd => bd.FpaDef)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(bd => bd.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(bd => bd.MaterialCaterory)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(p => p.WarehouseItemCodes)
                    .WithOne(p => p.WarehouseItem)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasForeignKey(p => p.WarehouseItemId);
                entity.HasMany(p => p.CompanyMappings)
                    .WithOne(p => p.WarehouseItem)
                    .HasForeignKey(p=>p.WarehouseItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<WarehouseTransaction>(entity =>
            {
                entity.HasIndex(p => p.CreatorId);
                entity.HasIndex(p => p.TransDate);
                entity.HasOne(p => p.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.FiscalPeriod)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Section)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.TransWarehouseDocSeries)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.TransWarehouseDocType)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.WarehouseItem)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<TransactorTransaction>(entity =>
            {
                entity.HasIndex(p => p.CreatorId);
                entity.HasIndex(p => p.TransDate);
                entity.HasOne(p => p.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.TransTransactorDocType)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.TransTransactorDocSeries)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.FiscalPeriod)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Section)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Transactor)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(p => p.SalesDocPaymentMappings)
                    .WithOne(p => p.TransactorTransaction)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(p => p.BuyDocPaymentMappings)
                    .WithOne(p => p.TransactorTransaction)
                    .OnDelete(DeleteBehavior.Restrict);

            });
            modelBuilder.Entity<TransExpenseDef>(entity =>
            {
                entity.HasOne(bd => bd.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => c.Code)
                    .IsUnique();
            });

            modelBuilder.Entity<BuyDocTypeDef>(entity =>
            {
                entity.HasIndex(p => p.Code).IsUnique();


                entity.HasOne(p => p.TransTransactorDef)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.TransWarehouseDef)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<BuyDocSeriesDef>(entity =>
            {
                entity.HasIndex(p => p.Code).IsUnique();

                entity.HasOne(p => p.BuyDocTypeDef)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<BuyDocLine>(entity =>
            {
                entity.HasOne(p => p.WarehouseItem)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.BuyDocument)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BuyDocument>(entity =>
            {
                entity.HasIndex(p => p.TransDate);
                entity.HasOne(p => p.Transactor)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Section)
               .WithMany()
               .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.FiscalPeriod)
               .WithMany()
               .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);

            });
            modelBuilder.Entity<TransTransactorDef>(entity =>
            {
                entity.HasOne(bd => bd.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => c.Code)
                    .IsUnique();
            });
            modelBuilder.Entity<TransTransactorDocTypeDef>(entity =>
            {
                entity.HasIndex(c => c.Code).IsUnique();

                entity.HasOne(bd => bd.TransTransactorDef)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<TransTransactorDocSeriesDef>(entity =>
            {
                entity.HasIndex(c => c.Code).IsUnique();

                entity.HasOne(bd => bd.TransTransactorDocTypeDef)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<SellDocTypeDef>(entity =>
            {
                entity.HasIndex(p => p.Code).IsUnique();

                entity.HasOne(p => p.TransTransactorDef)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.TransWarehouseDef)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<SellDocSeriesDef>(entity =>
            {
                entity.HasIndex(p => p.Code).IsUnique();

                entity.HasOne(p => p.SellDocTypeDef)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<SellDocLine>(entity =>
            {
                entity.HasOne(p => p.WarehouseItem)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.SellDocument)
                    .WithMany(c => c.SellDocLines)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SellDocument>(entity =>
            {
                entity.HasIndex(p => p.TransDate);
                entity.HasOne(p => p.SalesChannel)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Transactor)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Section)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.FiscalPeriod)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ClientProfile>(entity =>
            {

                entity.HasIndex(p => p.Code);

                entity.HasOne(p => p.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });

           
            modelBuilder.Entity<WrItemCode>(entity =>
            {
                entity.HasIndex(p => new
                {
                    p.CompanyId,
                    p.CodeType,
                    p.WarehouseItemId,
                    p.TransactorId,
                    p.Code
                }).IsUnique();
                entity.HasIndex(p => p.Code);

            });
            modelBuilder.Entity<CompanyWarehouseItemMapping>(entity =>
            {
                entity.HasKey(p => new
                {
                    p.CompanyId,
                    p.WarehouseItemId,
                });
            });
            modelBuilder.Entity<CrCatWarehouseItem>(entity =>
            {
                entity.HasIndex(p => new
                {
                    p.ClientProfileId,
                    p.CashRegCategoryId,
                    p.WarehouseItemId
                })
                    .IsUnique();
                entity.HasOne(p => p.ClientProfile)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.WarehouseItem)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.CashRegCategory)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ProductMedia>(entity =>
            {
                entity.HasIndex(p => new
                {
                    p.ProductId,
                    p.MediaEntryId
                })
                    .IsUnique();
                entity.HasOne(p => p.Product)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.MediaEntry)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);

            });
            modelBuilder.Entity<BuyDocTransPaymentMapping>(entity =>
            {
                entity.HasIndex(p => new
                {
                    p.BuyDocumentId,
                    p.TransactorTransactionId
                })
                    .IsUnique();
              
                entity.HasOne(p => p.TransactorTransaction)
                    .WithMany(p=>p.BuyDocPaymentMappings)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.BuyDocument)
                    .WithMany(p => p.PaymentMappings)
                    .OnDelete(DeleteBehavior.Restrict);
                
            });
           
            modelBuilder.Entity<ProductRecipeLine>(entity =>
            {
                entity.HasOne(p => p.Product)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.ProductRecipe)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProductRecipe>(entity =>
            {
                entity.HasOne(p => p.Product)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });
           
            modelBuilder.Entity<SellDocTransPaymentMapping>(entity =>
            {
                entity.HasIndex(p => new
                    {
                        p.SellDocumentId,
                        p.TransactorTransactionId
                    })
                    .IsUnique();
               
                entity.HasOne(p => p.TransactorTransaction)
                    .WithMany(p=>p.SalesDocPaymentMappings)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.SellDocument)
                    .WithMany(p => p.PaymentMappings)
                    .OnDelete(DeleteBehavior.Restrict);
               
            });
           
            modelBuilder.Entity<ExchangeRate>(entity =>
            {
                entity.HasIndex(p => p.ClosingDate);
                entity.HasOne(p => p.Currency)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p=>p.Currency)
                    .WithMany(p=>p.Rates)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasForeignKey(p => p.CurrencyId);
            });
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasOne(p => p.Currency)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Transactor>(entity =>
           {
               entity.HasIndex(c => c.Code).IsUnique();
               entity.HasOne(bd => bd.TransactorType)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);

           });
            modelBuilder.Entity<TransactorCompanyMapping>(entity =>
            {
                entity.HasKey(p => new
                {
                    p.CompanyId,
                    p.TransactorId
                });
                entity.HasOne(p => p.Company)
                    .WithMany(p => p.TransactorCompanyMappings)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasForeignKey(p => p.CompanyId);
                entity.HasOne(p => p.Transactor)
                    .WithMany(p => p.TransactorCompanyMappings)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasForeignKey(p => p.TransactorId);
            });
            modelBuilder.Entity<AppSetting>(entity => { entity.HasKey(p => p.Code); });
            modelBuilder.Entity<RecurringTransDocLine>(entity =>
            {
                entity.HasOne(p => p.WarehouseItem)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.RecurringTransDoc)
                 .WithMany(p => p.DocLines)
                 .OnDelete(DeleteBehavior.Restrict)
                 .HasForeignKey(p => p.RecurringTransDocId);
            });

            modelBuilder.Entity<RecurringTransDoc>(entity =>
            {
                entity.HasIndex(p => p.NextTransDate);
                entity.HasOne(p => p.Transactor)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.PaymentMethod)
                 .WithMany()
                 .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Company)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);

            });
        }
    }
}
