using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.Diaries;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrKouk.Web.ERP.Pages.MainEntities.Diaries
{
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public CreateModel(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult OnGet()
        {
            LoadCombos();
            return Page();
        }
        private void LoadCombos()
        {

            var diaryTypes = Enum.GetValues(typeof(DiaryTypeEnum))
                .Cast<DiaryTypeEnum>()
                .Select(c => new SelectListItem()
                {
                    Value = c.ToString(),
                    Text = c.GetDescription()
                }).ToList();

            #region CommentOut
            //foreach (DiaryTypeEnum value in Enum.GetValues(typeof(DiaryTypeEnum)))
            //{
            //    var a = value.GetDescription()
            //}
            //List<SelectListItem> diaryTypes = new List<SelectListItem>
            //{
            //    new SelectListItem() {Value = DiaryTypeEnum.DiaryTypeEnumBuys.ToString(), Text = "Ημερολόγιο Αγορών"},
            //    new SelectListItem() {Value = DiaryTypeEnum.DiaryTypeEnumSales.ToString(), Text = "Ημερολόγιο Πωλήσεων"},
            //    new SelectListItem() {Value = DiaryTypeEnum.DiaryTypeEnumExpenses.ToString(), Text = "Ημερολόγιο Εξόδων"}
            //};


            #endregion

            var BuyDocTypeListJs = _context.BuyDocTypeDefs.OrderBy(p => p.Name)
                .Select(p => new DiaryDocTypeItem()
                {

                    Title = p.Name,
                    Value = p.Id
                }).ToList();


            var SellDocTypeListJs = _context.SellDocTypeDefs.OrderBy(p => p.Name)
                .Select(p => new DiaryDocTypeItem()
                {
                    Title = p.Name,
                    Value = p.Id
                }).ToList();

            var TransactorDocTypeListJs = _context.TransTransactorDocTypeDefs.OrderBy(p => p.Name)
                .Select(p => new DiaryDocTypeItem()
                {
                    Title = p.Name,
                    Value = p.Id
                }).ToList();

            var WarehouseDocTypeListJs = _context.TransWarehouseDocTypeDefs.OrderBy(p => p.Name)
                .Select(p => new DiaryDocTypeItem()
                {
                    Title = p.Name,
                    Value = p.Id
                }).ToList();
            #region CommentOut
            //var BuyDocTypeListJs = _context.BuyDocTypeDefs.OrderBy(p => p.Name)
            //   .ProjectTo<DiaryDocTypeItem>(_mapper.ConfigurationProvider).ToList();

            //var SellDocTypeListJs = _context.SellDocTypeDefs.OrderBy(p => p.Name)
            //    .ProjectTo<DiaryDocTypeItem>(_mapper.ConfigurationProvider).ToList();


            #endregion

            var materialNatureList = Enum.GetValues(typeof(WarehouseItemNatureEnum))
                .Cast<WarehouseItemNatureEnum>()
                .Select(c => new UISelectTypeItem()
                {
                    ValueInt =(int) c,
                    Title = c.GetDescription()
                }).ToList();

            var transactorTypeList = _context.TransactorTypes.OrderBy(p => p.Name)
                .Select(p => new UISelectTypeItem()
                {
                    Title = p.Name,
                    Value = p.Id.ToString()
                }).ToList();

            ViewData["diaryTypes"] = new SelectList(diaryTypes, "Value", "Text");
            ViewData["transactorTypes"] = new SelectList(transactorTypeList, "Value", "Title");
            ViewData["MaterialNatureTypes"] = new SelectList(materialNatureList, "ValueInt", "Title");

            //ViewData["BuyDocTypeList"] = new SelectList(_context.BuyDocTypeDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["BuyDocTypeListJs"] = BuyDocTypeListJs;
            ViewData["SellDocTypeListJs"] = SellDocTypeListJs;
            ViewData["TransactorDocTypeListJs"] = TransactorDocTypeListJs;
            ViewData["WarehouseDocTypeListJs"] = WarehouseDocTypeListJs;

            ViewData["MaterialNaturesList"] = materialNatureList;
            ViewData["TransactorTypeList"] = transactorTypeList;
        }

        [BindProperty]
        public DiaryDto ItemVm { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var diaryDef = _mapper.Map<DiaryDef>(ItemVm);
            _context.DiaryDefs.Add(diaryDef);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}