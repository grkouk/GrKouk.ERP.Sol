using System.Collections.Generic;
using System.Linq;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Helpers
{
    public static class SelectListHelpers
    {
        public static List<SelectListItem> GetSectionsList(ApiDbContext context)
        {
            var dbList = context.Sections.OrderBy(p => p.Name).AsNoTracking();
            List<SelectListItem> valuesList = new List<SelectListItem>
            {
                new SelectListItem() { Value = 0.ToString(), Text = "{No Selection}" }
            };
            foreach (var item in dbList)
            {
                valuesList.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.Name });
            }
            return valuesList;
        }
        /// <summary>
        /// Return a select list of CfsDocType Definitions with addition of 0 -> No selection
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetCfaDocSeriesDefNoSelectionList(ApiDbContext context)
        {
            var dbList = context.CashFlowDocSeriesDefs.OrderBy(p => p.Name).AsNoTracking();
            List<SelectListItem> valuesList = new List<SelectListItem>
            {
                new SelectListItem() { Value = 0.ToString(), Text = "{No Selection}" }
            };
            foreach (var item in dbList)
            {
                valuesList.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.Name });
            }
            return valuesList;
        }
        /// <summary>
        /// Return a select list of CfsDocType Definitions
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetCfaDocSeriesDefList(ApiDbContext context)
        {
            var dbList = context.CashFlowDocSeriesDefs.OrderBy(p => p.Name).AsNoTracking();
            List<SelectListItem> valuesList = new List<SelectListItem>();
            
            foreach (var item in dbList)
            {
                valuesList.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.Name });
            }
            return valuesList;
        }

        /// <summary>
        /// Return a select list of Cash Flow Accounts with addition of 0 -> No selection
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetCfAccountsNoSelectionList(ApiDbContext context)
        {
            var dbList = context.CashFlowAccounts.OrderBy(p => p.Name).AsNoTracking();
            List<SelectListItem> valuesList = new List<SelectListItem>
            {
                new SelectListItem() { Value = 0.ToString(), Text = "{No Selection}" }
            };
            foreach (var item in dbList)
            {
                valuesList.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.Name });
            }
            return valuesList;
        }
    }
}