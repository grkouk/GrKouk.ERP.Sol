using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Definitions;
using GrKouk.Web.ERP.Data;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Helpers
{
    public static class FiltersHelper
    {
        public static List<SelectListItem> GetRecurringFrequencyList()
        {
            List<SelectListItem> filtersSelectList = new List<SelectListItem>
            {
                new SelectListItem() {Value = "1D", Text = "Ημερήσια"},
                new SelectListItem() {Value = "7D", Text = "Εβδομαδιαία"},
                new SelectListItem() {Value = "1M", Text = "Μηνιαία"},
                new SelectListItem() {Value = "2M", Text = "Διμηνιαία"},
                new SelectListItem() {Value = "3M", Text = "Τριμηνιαία"},
                new SelectListItem() {Value = "6M", Text = "Εξαμηνιαία"},
                new SelectListItem() {Value = "1Y", Text = "Ετήσια"}

            };
            return filtersSelectList;
        }
        public static List<SelectListItem> GetPageSizeFiltersSelectList()
        {
            List<SelectListItem> filtersSelectList = new List<SelectListItem>
            {
                new SelectListItem() {Value = "1", Text = "1"},
                new SelectListItem() {Value = "5", Text = "5"},
                new SelectListItem() {Value = "10", Text = "10"},
                new SelectListItem() {Value = "20", Text = "20"},
                new SelectListItem() {Value = "50", Text = "50"},
                new SelectListItem() {Value = "100", Text = "100"}

            };
            return filtersSelectList;
        }
        public static List<SelectListItem> GetWarehouseItemNaturesList()
        {
            var materialNatures = Enum.GetValues(typeof(WarehouseItemNatureEnum))
                .Cast<WarehouseItemNatureEnum>()
                .Select(c => new SelectListItem()
                {
                    Value = ((int)c).ToString(),
                    Text = c.GetDescription()
                }).ToList();
            //Αλλαγή του στοιχείου 0 από απροσδιόριστο σε {Ολές οι φύσεις είδους}
            materialNatures[0].Text = "{All Natures}";
            return materialNatures;
        }
        public static List<SelectListItem> GetSeriesPayoffWayList()
        {
            var materialNatures = Enum.GetValues(typeof(WarehouseItemNatureEnum))
                .Cast<WarehouseItemNatureEnum>()
                .Select(c => new SelectListItem()
                {
                    Value = ((int)c).ToString(),
                    Text = c.GetDescription()
                }).ToList();
            //Αλλαγή του στοιχείου 0 από απροσδιόριστο σε {Ολές οι φύσεις είδους}
            materialNatures[0].Text = "{All }";
            return materialNatures;
        }

        public static List<SelectListItem> GetCompaniesFilterList(ApiDbContext context)
        {

            //TODO: Use All Companies code from settings page and chenge author?
            var dbCompanies = context.Companies.Where(t => t.Id != 1).OrderBy(p => p.Code).AsNoTracking();
            List<SelectListItem> companiesList = new List<SelectListItem>();
            companiesList.Add(new SelectListItem() { Value = 0.ToString(), Text = "{All Companies}" });
            foreach (var company in dbCompanies)
            {
                companiesList.Add(new SelectListItem() { Value = company.Id.ToString(), Text = company.Code });
            }

            return companiesList;
        }
        public static async Task<List<SelectListItem>> GetCompaniesFilterListAsync(ApiDbContext context)
        {

            var dbCompanies = await context.Companies.Where(t => t.Id != 1).OrderBy(p => p.Code)
                .AsNoTracking()
                .ToListAsync();
            List<SelectListItem> companiesList = new List<SelectListItem>();
            companiesList.Add(new SelectListItem() { Value = 0.ToString(), Text = "{All Companies}" });
            foreach (var company in dbCompanies)
            {
                companiesList.Add(new SelectListItem() { Value = company.Id.ToString(), Text = company.Code });
            }

            return companiesList;
        }
        public static async Task<List<SelectListItem>> GetTransactorsForTypeFilterListAsync(ApiDbContext context, string trType)
        {
            var trTypeObject = await context.TransactorTypes.FirstOrDefaultAsync(p => p.Code == trType);
            int trTypeId = 0;
            if (trTypeObject != null)
            {
                trTypeId = trTypeObject.Id;
            }

            var dbTransactors = await context.Transactors.Where(t => t.TransactorTypeId == trTypeId)
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();


            List<SelectListItem> transactorsList = new List<SelectListItem>
            {
                new SelectListItem() {Value = 0.ToString(), Text = "{No Transactor}"}
            };
            foreach (var item in dbTransactors)
            {
                transactorsList.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.Code });
            }

            return transactorsList;
        }
        public static async Task<List<SelectListItem>> GetTransactorTypeFilterListAsync(ApiDbContext context)
        {
           
            var dbTransactorTypes = await context.TransactorTypes.OrderBy(p => p.Code)
                .AsNoTracking()
                .ToListAsync();
            List<SelectListItem> transactorTypes = new List<SelectListItem>();
            transactorTypes.Add(new SelectListItem() { Value = 0.ToString(), Text = "{All Types}" });
            foreach (var dbTransactorType in dbTransactorTypes)
            {
                transactorTypes.Add(new SelectListItem() { Value = dbTransactorType.Id.ToString(), Text = dbTransactorType.Code });
            }
           
            return transactorTypes;
        }
        public static List<SelectListItem> GetTransactorsForTypeFilterList(ApiDbContext context, string trType)
        {
            var trTypeObject = context.TransactorTypes.FirstOrDefault(p => p.Code == trType);
            int trTypeId = 0;
            if (trTypeObject != null)
            {
                trTypeId = trTypeObject.Id;
            }

            var dbTransactors = context.Transactors.Where(t => t.TransactorTypeId == trTypeId)
                .OrderBy(p => p.Name)
                .AsNoTracking();


            List<SelectListItem> transactorsList = new List<SelectListItem>
            {
                new SelectListItem() {Value = 0.ToString(), Text = "{No Transactor}"}
            };
            foreach (var item in dbTransactors)
            {
                transactorsList.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.Code });
            }

            return transactorsList;
        }
        public static List<SelectListItem> GetCurrenciesFilterList(ApiDbContext context)
        {
            var dbItems = context.Currencies.OrderBy(p => p.Name).AsNoTracking();
            List<SelectListItem> itemsList = new List<SelectListItem>();
            //itemsList.Add(new SelectListItem() { Value = 0.ToString(), Text = "{All Companies}" });
            foreach (var item in dbItems)
            {
                itemsList.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.Code });
            }

            return itemsList;
        }
        public static async Task<List<SelectListItem>> GetCurrenciesFilterListAsync(ApiDbContext context)
        {
            var dbItems = await context.Currencies.OrderBy(p => p.Name).AsNoTracking().ToListAsync();
            List<SelectListItem> itemsList = new List<SelectListItem>();
            //itemsList.Add(new SelectListItem() { Value = 0.ToString(), Text = "{All Companies}" });
            foreach (var item in dbItems)
            {
                itemsList.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.Code });
            }

            return itemsList;
        }
    }
}
