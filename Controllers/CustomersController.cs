using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tmm.Data;
using Tmm.Models;
using Microsoft.EntityFrameworkCore;


namespace Tmm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly TmmDbContext _context;

        public CustomersController(TmmDbContext context)
        {
            _context = context;
        }

        // GET: api/customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        // Other CRUD actions would go here...
    }
}
