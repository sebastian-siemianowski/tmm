using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tmm.Data;
using Tmm.Models;
using Microsoft.EntityFrameworkCore;

namespace Tmm.Controllers
{
    [ApiController]
    [Route("api/customers/{customerId}/addresses")]
    public class AddressesController : ControllerBase
    {
        private readonly TmmDbContext _context;

        public AddressesController(TmmDbContext context)
        {
            _context = context;
        }

        // GET: api/customers/5/addresses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddressesForCustomer(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);

            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            var addresses = await _context.Addresses.Where(a => a.CustomerId == customerId).ToListAsync();

            return addresses;
        }

        // GET: api/customers/5/addresses/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Address>> GetAddressForCustomer(int customerId, int id)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId);

            if (address == null)
            {
                return NotFound();
            }

            return address;
        }

        // POST: api/customers/5/addresses
        [HttpPost]
        public async Task<ActionResult<Address>> AddAddressForCustomer(int customerId, Address address)
        {
            var customer = await _context.Customers.FindAsync(customerId);

            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            address.CustomerId = customerId;

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAddressForCustomer), new { customerId, id = address.Id }, address);
        }

        // PUT: api/customers/5/addresses/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddressForCustomer(int customerId, int id, Address address)
        {
            if (id != address.Id || customerId != address.CustomerId)
            {
                return BadRequest();
            }

            _context.Entry(address).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Addresses.Any(a => a.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/customers/5/addresses/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddressForCustomer(int customerId, int id)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId);
            
            if (address == null)
            {
                return NotFound();
            }

            var customerAddressesCount = await _context.Addresses.CountAsync(a => a.CustomerId == customerId);
            
            if (customerAddressesCount == 1)
            {
                return BadRequest("A customer must have at least one address.");
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
