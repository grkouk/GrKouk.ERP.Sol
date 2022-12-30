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
    public class ERPApiController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public ERPApiController(ApiDbContext context)
        {
            _context = context;
        }
        
    }
}

