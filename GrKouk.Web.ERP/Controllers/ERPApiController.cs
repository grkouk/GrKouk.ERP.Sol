using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;


namespace GrKouk.Web.ERP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErpApiController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public ErpApiController(ApiDbContext context)
        {
            _context = context;
        }
        
    }
}

