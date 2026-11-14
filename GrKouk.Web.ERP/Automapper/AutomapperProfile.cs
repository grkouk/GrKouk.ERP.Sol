using AutoMapper;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Domain.MediaEntities;
using GrKouk.Erp.Domain.RecurringTransactions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Domain.Warehouses;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Erp.Dtos.CashFlowAccounts;
using GrKouk.Erp.Dtos.CashFlowTransactions;
using GrKouk.Erp.Dtos.CashRegister;
using GrKouk.Erp.Dtos.Diaries;
using GrKouk.Erp.Dtos.FinDiary;
using GrKouk.Erp.Dtos.Media;
using GrKouk.Erp.Dtos.ProductRecipies;
using GrKouk.Erp.Dtos.RecurringTransactions;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Erp.Dtos.Transactors;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Erp.Dtos.WarehouseItems;
using GrKouk.Erp.Dtos.Warehouses;
using GrKouk.Erp.Dtos.WarehouseTransactions;
using GrKouk.Web.ERP.Helpers;

namespace GrKouk.Web.ERP.Automapper {
    public class AutoMapperProfile : Profile {
        public AutoMapperProfile() {
            //.ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name));

            CreateMap<BuyDocument, BuyDocCreateAjaxNoLinesDto>().ReverseMap();
            CreateMap<BuyDocument, BuyDocModifyAjaxNoLinesDto>().ReverseMap();
            CreateMap<BuyDocModifyAjaxDto, BuyDocModifyAjaxNoLinesDto>().ReverseMap();
            CreateMap<BuyDocCreateAjaxDto, BuyDocCreateAjaxNoLinesDto>().ReverseMap();
            CreateMap<BuyDocCreateAjaxNoLinesDto, BuyDocLineAjaxDto>().ReverseMap();
            CreateMap<BuyDocument, BuyDocListDto>()
                .ForMember(dest => dest.CompanyCurrencyId,
                    opt => opt.MapFrom(src => src.Company.CurrencyId)).ReverseMap();
            CreateMap<BuyDocument, BuyDocModifyDto>().ReverseMap();
            CreateMap<BuyDocLine, BuyDocLineModifyDto>().ReverseMap();
            CreateMap<TransactorTransaction, BuyDocCreateAjaxDto>().ReverseMap();
            CreateMap<TransactorTransaction, BuyDocModifyAjaxDto>().ReverseMap();
            CreateMap<TransactorTransCreateDto, BuyDocModifyAjaxDto>().ReverseMap();

            CreateMap<SellDocument, SellDocListDto>()
                .ForMember(dest => dest.CompanyCurrencyId,
                    opt => opt.MapFrom(src => src.Company.CurrencyId)).ReverseMap();
            CreateMap<SellDocument, SellDocCreateAjaxNoLinesDto>().ReverseMap();
            CreateMap<SellDocCreateAjaxDto, SellDocCreateAjaxNoLinesDto>().ReverseMap();
            CreateMap<SellDocument, SellDocModifyAjaxNoLinesDto>().ReverseMap();
            CreateMap<SellDocument, SellDocLineAjaxDto>().ReverseMap();
            CreateMap<SellDocument, SellDocModifyDto>().ReverseMap();
            CreateMap<SellDocLine, SellDocLineModifyDto>()
                .ForMember(d => d.WarehouseItemNature,
                    opt => opt.MapFrom(s => (int)s.WarehouseItem.WarehouseItemNature))
                .ReverseMap();

            CreateMap<SellDocModifyAjaxDto, SellDocModifyAjaxNoLinesDto>();
            CreateMap<TransactorTransaction, SellDocCreateAjaxDto>().ReverseMap();
            CreateMap<TransactorTransaction, SellDocModifyAjaxDto>().ReverseMap();
            CreateMap<TransactorTransCreateDto, SellDocModifyAjaxDto>().ReverseMap();


            CreateMap<RecurringTransDoc, RecurringDocCreateAjaxNoLinesDto>().ReverseMap();
            CreateMap<RecurringTransDocCreateAjaxDto, RecurringDocCreateAjaxNoLinesDto>().ReverseMap();
            CreateMap<RecurringTransDocModifyAjaxDto, RecurringTransDocModifyAjaxNoLinesDto>().ReverseMap();
            CreateMap<RecurringTransDocModifyAjaxNoLinesDto, RecurringTransDoc>().ReverseMap();
            CreateMap<RecurringTransDoc, BuyDocModifyAjaxNoLinesDto>().ReverseMap();
            CreateMap<BuyDocCreateAjaxNoLinesDto, BuyDocLineAjaxDto>().ReverseMap();
            CreateMap<RecurringTransDoc, RecurringDocModifyDto>()
                .ForMember(dest => dest.BuyDocLines,
                    opt => opt.MapFrom(src => src.DocLines));


            CreateMap<RecurringTransDoc, RecurringDocListDto>()
                .ForMember(dest => dest.CompanyCurrencyId,
                    opt => opt.MapFrom(src => src.Company.CurrencyId)).ReverseMap();
            CreateMap<BuyDocument, RecurringDocModifyDto>()
                .ForMember(dest => dest.DocSeriesId,
                    opt => opt.MapFrom(src => src.BuyDocSeriesId))
                .ReverseMap();
            CreateMap<BuyDocLine, RecurringDocLineModifyDto>().ReverseMap();
            CreateMap<SellDocument, RecurringDocModifyDto>()
               .ForMember(dest => dest.DocSeriesId,
                   opt => opt.MapFrom(src => src.SellDocSeriesId))
               .ForMember(dest => dest.BuyDocLines,
               opt => opt.MapFrom(src => src.SellDocLines))
               .ReverseMap();
            CreateMap<SellDocLine, RecurringDocLineModifyDto>();
            CreateMap<RecurringTransDocLine, RecurringDocLineModifyDto>();
            CreateMap<RecurringTransDoc, BuyDocCreateAjaxDto>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TransDate,
                    opt => opt.MapFrom(src => src.NextTransDate))
                .ForMember(dest => dest.BuyDocSeriesId,
                    opt => opt.MapFrom(src => src.DocSeriesId))
                .ForMember(dest => dest.BuyDocLines,
                    opt => opt.MapFrom(src => src.DocLines));
            CreateMap<RecurringTransDocLine, BuyDocLineAjaxDto>()
                .ForMember(d => d.Q1,
                    opt => opt.MapFrom(src => src.Quontity1))
                .ForMember(d => d.Q2,
                    opt => opt.MapFrom(src => src.Quontity2))
                .ForMember(d => d.MainUnitId,
                    opt => opt.MapFrom(src => src.PrimaryUnitId))
                .ForMember(d => d.SecUnitId,
                    opt => opt.MapFrom(src => src.SecondaryUnitId))
                .ForMember(d => d.Price,
                    opt => opt.MapFrom(src => src.UnitPrice));
            CreateMap<RecurringTransDoc, SellDocCreateAjaxDto>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TransDate,
                    opt => opt.MapFrom(src => src.NextTransDate))
                .ForMember(dest => dest.SellDocSeriesId,
                    opt => opt.MapFrom(src => src.DocSeriesId))
                .ForMember(dest => dest.SellDocLines,
                    opt => opt.MapFrom(src => src.DocLines));
            CreateMap<RecurringTransDocLine, SellDocLineAjaxDto>()
                .ForMember(d => d.Q1,
                    opt => opt.MapFrom(src => src.Quontity1))
                .ForMember(d => d.Q2,
                    opt => opt.MapFrom(src => src.Quontity2))
                .ForMember(d => d.MainUnitId,
                    opt => opt.MapFrom(src => src.PrimaryUnitId))
                .ForMember(d => d.SecUnitId,
                    opt => opt.MapFrom(src => src.SecondaryUnitId))
                .ForMember(d => d.Price,
                    opt => opt.MapFrom(src => src.UnitPrice));

        }
    }
}
