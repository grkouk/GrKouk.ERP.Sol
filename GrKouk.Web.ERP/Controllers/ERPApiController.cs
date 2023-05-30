using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace GrKouk.Web.ERP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErpApiController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly ILogger<ErpApiController> _logger;

        public ErpApiController(ApiDbContext context, ILogger<ErpApiController> logger)
        {
            _context = context;
            _logger = logger;
        }
        
    }
}

