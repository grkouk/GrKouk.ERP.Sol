using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Tools
{
    public class UpdateRatesModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IToastNotification _toastNotification;

        public UpdateRatesModel(ApiDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        public IList<ExchangeRate> ExchangeRate { get;set; }

        public  void OnGet()
        {


        }

        public async Task OnPostAsync()
        {
            string BaseUrl = "https://api.exchangeratesapi.io/latest?symbols=USD,BGN";
            var httpClient = new HttpClient();
            
            try
            {
                var uri = new Uri(BaseUrl);

                var response = await httpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var rateResponse = JsonConvert.DeserializeObject<RateResponse>(jsonContent);
                    if (rateResponse != null)
                    {
                        foreach (var rateItem in rateResponse.Rates)
                        {
                            var currencyCode = rateItem.Key;
                            var cur = await _context.Currencies.FirstOrDefaultAsync(p => p.Code == currencyCode);
                            if (cur != null)
                            {
                                var currencyId = cur.Id;
                                var rateToAdd = new ExchangeRate
                                {
                                    CurrencyId = currencyId,
                                    ClosingDate = rateResponse.Date,
                                    Rate = (decimal)rateItem.Value
                                };
                                var rate = await RateExist(rateToAdd.CurrencyId, rateToAdd.ClosingDate);
                                if (rate==null)
                                {
                                    await _context.ExchangeRates.AddAsync(rateToAdd);
                                }
                                else
                                {
                                    rate.Rate = rateToAdd.Rate;
                                    _context.Attach(rate).State = EntityState.Modified;
                                }
                               
                            }

                        }
                        await _context.SaveChangesAsync();
                        _toastNotification.AddSuccessToastMessage("Update rates finished");

                    }

                }



            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                //throw;
            }
        }
        private async Task<ExchangeRate> RateExist(int currencyId, DateTime closeDate)
        {
            var dt = new DateTime(closeDate.Year,closeDate.Month,closeDate.Day);
            var rate = await _context.ExchangeRates
                .Where(e => e.CurrencyId == currencyId 
                            && e.ClosingDate == dt
                            //&& e.ClosingDate.Year == dt.Year 
                            //&& e.ClosingDate.Month == dt.Month 
                            //&& e.ClosingDate.Day == dt.Day
                            //&& e.ClosingDate.Hour==0
                            //&& e.ClosingDate.Minute==0
                            //&& e.ClosingDate.Second==0
                            //&& e.ClosingDate.Millisecond==0
                            )
                .FirstOrDefaultAsync();
            return rate;
        }
    }

    public class RateResponse
    {
        [JsonProperty("base")]
        public string BaseCurrecy { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("rates")]
        public Dictionary<string, double> Rates { get; set; }
    }
}
