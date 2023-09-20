using System;
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddressesForCustomer(int customerId)
        {
            if (!await _context.Customers.AnyAsync(c => c.Id == customerId))
            {
                return NotFound("Customer not found.");
            }

            return await _context.Addresses.Where(a => a.CustomerId == customerId).ToListAsync();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Address>> GetAddressForCustomer(int customerId, int id)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId);
            if (address == null)
            {
                return NotFound("Address not found.");
            }
            return address;
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<Address>> AddAddressForCustomer(int customerId, Address address)
        {
            if (address == null)
            {
                return BadRequest("Address cannot be null.");
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            if (address.IsMain)
            {
                var existingMainAddress = await _context.Addresses
                                               .Where(a => a.CustomerId == customerId && a.IsMain)
                                               .FirstOrDefaultAsync();
                if (existingMainAddress != null)
                {
                    existingMainAddress.IsMain = false;
                    _context.Entry(existingMainAddress).State = EntityState.Modified;
                }
            }

            address.CustomerId = customerId;
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAddressForCustomer), new { customerId, id = address.Id }, address);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateAddressForCustomer(int customerId, int id, Address address)
        {
            if (id != address.Id || customerId != address.CustomerId)
            {
                return BadRequest("Mismatched address or customer IDs.");
            }

            // Fetch the address entity from the database.
            var existingAddress = await _context.Addresses.SingleOrDefaultAsync(a => a.Id == id);
            if (existingAddress == null)
            {
                return NotFound("Address not found.");
            }

            // Update the properties of the fetched address with values from the provided address.
            existingAddress.AddressLine1 = address.AddressLine1;
            existingAddress.AddressLine2 = address.AddressLine2;
            existingAddress.Town = address.Town;
            existingAddress.County = address.County;
            existingAddress.Postcode = address.Postcode;
            existingAddress.Country = address.Country;
            existingAddress.IsMain = address.IsMain;
            // ... continue this for all other properties of the Address entity ...

            if (address.IsMain)
            {
                var existingMainAddress = await _context.Addresses
                                                   .Where(a => a.CustomerId == customerId && a.IsMain && a.Id != id)
                                                   .FirstOrDefaultAsync();
                if (existingMainAddress != null)
                {
                    existingMainAddress.IsMain = false;
                    _context.Entry(existingMainAddress).State = EntityState.Modified;
                }
            }

            _context.Entry(existingAddress).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Addresses.Any(a => a.Id == id))
                {
                    return NotFound("Address not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [HttpDelete]
        [Route("{id}")]
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