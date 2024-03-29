﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GrKouk.Erp.Domain.MediaEntities;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Erp.Dtos.WarehouseItems;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using NToastNotify.Helpers;
using Syncfusion.EJ2.Base;

namespace GrKouk.Web.ERP.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AutoCompleteController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public AutoCompleteController(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet("GetShortcutsAutoCompleteData")]
        public async Task<IActionResult> GetShortcutsAutoCompleteData([FromQuery] ShortCutsAutoCompleteRequest request)
        {
            string term = request.Term;
            //Transactors
            IQueryable<Transactor> transactorFullListIq = _context.Transactors;
            List<Ej2ShortcutAutoCompleteItem> items=new();

            if (!string.IsNullOrEmpty(term))
            {
                transactorFullListIq = transactorFullListIq.Where(p => p.Name.Contains(term) || p.Code.Contains(term));
            }

            IEnumerable<Ej2ShortcutAutoCompleteItem> transactorItems = await transactorFullListIq

                .Select(p => new Ej2ShortcutAutoCompleteItem
                {
                    Text = p.Name,
                    Value = p.Id,
                    ImgUrl = Url.Content("~/productimages/" + "noimage.jpg"),
                    ItemType = Ej2ShortcutAutoCompleteItemType.ShortcutItemTransactor
                })
                .ToListAsync();

            items.AddRange(transactorItems);
            



            IQueryable<WarehouseItem> warehouseItem4FullListIq = _context.WarehouseItems;


            if (!string.IsNullOrEmpty(term))
            {
                warehouseItem4FullListIq = warehouseItem4FullListIq.Where(p => p.Name.Contains(term) || p.Code.Contains(term));
            }

            IEnumerable<Ej2ShortcutAutoCompleteItem> warehouseItems = await warehouseItem4FullListIq
                .ProjectTo<WarehouseItemSearchListDto>(_mapper.ConfigurationProvider)
                .Select(p => new Ej2ShortcutAutoCompleteItem { Text = p.Label, Value = p.Id, ItemType = Ej2ShortcutAutoCompleteItemType.ShortcutItemWarehouseItem })
                .ToListAsync();

            foreach (var productItem in warehouseItems)
            {
                ProductMedia productMedia;

                try
                {
                    productMedia = await _context.ProductMedia
                        .Include(p => p.MediaEntry)
                        .AsNoTracking()
                        .SingleOrDefaultAsync(p => p.ProductId == productItem.Value);
                    if (productMedia != null)
                    {
                        productItem.ImgUrl = Url.Content("~/productimages/" + productMedia.MediaEntry.MediaFile);
                    }
                    else
                    {
                        productItem.ImgUrl = Url.Content("~/productimages/" + "noimage.jpg");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    productItem.ImgUrl = Url.Content("~/productimages/" + "noimage.jpg");
                }
            }
            items.AddRange(warehouseItems);
            items.Sort(delegate (Ej2ShortcutAutoCompleteItem x, Ej2ShortcutAutoCompleteItem y)
            {
                if (x.Text == null && y.Text == null) return 0;
                else if (x.Text == null) return -1;
                else if (y.Text == null) return 1;
                else return x.Text.CompareTo(y.Text);
            });

            return Ok(new { result = items });
        }


        [HttpGet("GetAutoCompleteSellProducts")]
        public async Task<IActionResult> GetAutoCompleteSellProducts([FromQuery] AutoCompleteRequest request)
        {
            IQueryable<WarehouseItem> fullListIq = _context.WarehouseItems;

            int companyId = request.CompanyId;
            if (companyId > 1)
            {
                //Not all companies 
                fullListIq = fullListIq.Where(p => p.CompanyId == companyId || p.CompanyId == 1);
            }

            int seriesId = request.SeriesId;

            var series = await _context.SellDocSeriesDefs
                .Include(p => p.SellDocTypeDef)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == seriesId);
            if (series != null)
            {
                var docType = series.SellDocTypeDef;
                var itemNatures = docType.SelectedWarehouseItemNatures;
                if (!string.IsNullOrEmpty(itemNatures))
                {
                    var natures = Array.ConvertAll(docType.SelectedWarehouseItemNatures.Split(","), int.Parse);
                    //var natures = docType.SelectedWarehouseItemNatures;
                    fullListIq = fullListIq.Where(p => natures.Contains((int)p.WarehouseItemNature));
                }
            }

            string term = request.Term;
            if (!string.IsNullOrEmpty(term))
            {
                fullListIq = fullListIq.Where(p => p.Name.Contains(term) || p.Code.Contains(term));
            }

            IEnumerable<Ej2AutoCompleteItem> items = await fullListIq
                .ProjectTo<WarehouseItemSearchListDto>(_mapper.ConfigurationProvider)
                .Select(p => new Ej2AutoCompleteItem { Text = p.Label, Value = p.Id })
                .ToListAsync();

            foreach (var productItem in items)
            {
                ProductMedia productMedia;

                try
                {
                    productMedia = await _context.ProductMedia
                        .Include(p => p.MediaEntry)
                        .AsNoTracking()
                        .SingleOrDefaultAsync(p => p.ProductId == productItem.Value);
                    if (productMedia != null)
                    {
                        productItem.ImgUrl = Url.Content("~/productimages/" + productMedia.MediaEntry.MediaFile);
                    }
                    else
                    {
                        productItem.ImgUrl = Url.Content("~/productimages/" + "noimage.jpg");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    productItem.ImgUrl = Url.Content("~/productimages/" + "noimage.jpg");
                }
            }
            return Ok(new { result = items });
        }

        [HttpPost("GetSellDocAutoComplete")]
        public async Task<IActionResult> GetSellDocAutoComplete([FromBody] AutoCompleteDataManagerRequest request)
        {
            IQueryable<WarehouseItem> fullListIq = _context.WarehouseItems;

            int companyId = request.CompanyId;
            if (companyId > 1)
            {
                //Not all companies 
                fullListIq = fullListIq.Where(p => p.CompanyId == companyId || p.CompanyId == 1);
            }

            int seriesId = request.SeriesId;

            var series = await _context.SellDocSeriesDefs
                .Include(p => p.SellDocTypeDef)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == seriesId);
            if (series != null)
            {
                var docType = series.SellDocTypeDef;
                var itemNatures = docType.SelectedWarehouseItemNatures;
                if (!string.IsNullOrEmpty(itemNatures))
                {
                    var natures = Array.ConvertAll(docType.SelectedWarehouseItemNatures.Split(","), int.Parse);
                    //var natures = docType.SelectedWarehouseItemNatures;
                    fullListIq = fullListIq.Where(p => natures.Contains((int)p.WarehouseItemNature));
                }
            }

            string term = "";
            if (request.Where != null && request.Where.Count > 0) //Filtering
            {
                term = request.Where[0].value.ToString();
            }

            if (!string.IsNullOrEmpty(term))
            {
                fullListIq = fullListIq.Where(p => p.Name.Contains(term) || p.Code.Contains(term));
            }

            IEnumerable<Ej2AutoCompleteItem> items = await fullListIq
                .ProjectTo<WarehouseItemSearchListDto>(_mapper.ConfigurationProvider)
                .Select(p => new Ej2AutoCompleteItem { Text = p.Label, Value = p.Id })
                .ToListAsync();


            //--------------------------

            DataOperations operation = new DataOperations();
            if (request.Search != null && request.Search.Count > 0)
            {
                items = operation.PerformSearching(items, request.Search); //Search
            }

            //if (request.Where != null && request.Where.Count > 0) //Filtering
            //{
            //    items = operation.PerformFiltering(items, request.Where, request.Where[0].Operator);
            //}
            if (request.Sorted != null && request.Sorted.Count > 0) //Sorting
            {
                items = operation.PerformSorting(items, request.Sorted);
            }

            var resultCount = items.Count();
            if (request.Skip != 0)
            {
                items = operation.PerformSkip(items, request.Skip); //Paging
            }

            if (request.Take != 0)
            {
                items = operation.PerformTake(items, request.Take);
            }


            return request.RequiresCounts ? Ok(new { result = items, count = resultCount }) : Ok(new { result = items });
        }
    }

    public class AutoCompleteRequest
    {
        [JsonProperty(PropertyName = "companyId", Required = Required.Default)]
        public int CompanyId { get; set; }
        [JsonProperty(PropertyName = "seriesId", Required = Required.Default)]
        public int SeriesId { get; set; }
        [JsonProperty(PropertyName = "term", Required = Required.Default)]
        public string Term { get; set; }
    }
    public class ShortCutsAutoCompleteRequest
    {
        //[JsonProperty(PropertyName = "companyId", Required = Required.Default)]
        //public int CompanyId { get; set; }
        //[JsonProperty(PropertyName = "seriesId", Required = Required.Default)]
        //public int SeriesId { get; set; }
        [JsonProperty(PropertyName = "term", Required = Required.Default)]
        public string Term { get; set; }
    }
    public class Ej2ShortcutAutoCompleteItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
        public Ej2ShortcutAutoCompleteItemType ItemType { get; set; }
        public string ImgUrl { get; set; }
    }
    public enum Ej2ShortcutAutoCompleteItemType
    {
        ShortcutItemTransactor = 1,
        ShortcutItemWarehouseItem = 2
    }
}