using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tmm.Data;
using Tmm.Models;

namespace Tmm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly TmmDbContext _context;

        public CustomersController(TmmDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAllCustomers()
        {
            return await _context.Customers.Include(c => c.Addresses).ToListAsync();
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult<Customer>> GetCustomerById(int id)
        {
            var customer = await _context.Customers.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }
            return customer;
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<Customer>> CreateCustomer([FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest("Customer object is null.");
            }

            // Check if email already exists
            if (await _context.Customers.AnyAsync(c => c.EmailAddress == customer.EmailAddress))
            {
                return Conflict("A customer with the provided email address already exists.");
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest("Customer object is null.");
            }

            if (id != customer.Id)
            {
                return BadRequest("Customer ID mismatch.");
            }

            // Check if email already exists for a different customer
            if (await _context.Customers.AnyAsync(c => c.EmailAddress == customer.EmailAddress && c.Id != id))
            {
                return Conflict("Another customer with the provided email address already exists.");
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound("Customer not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpGet]
        [Route("active")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetActiveCustomers()
        {
            var activeCustomers = await _context.Customers
                                      .Where(c => c.IsActive)
                                      .Include(c => c.Addresses)
                                      .ToListAsync();

            return activeCustomers;
        }

        [HttpPut]
        [Route("deactivate/{id:int}")]
        public async Task<IActionResult> DeactivateCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            customer.IsActive = false;
            _context.Entry(customer).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(customer);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}
