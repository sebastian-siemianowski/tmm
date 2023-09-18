using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tmm.Controllers;
using Tmm.Data;
using Tmm.Models;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Tmm.Tests
{
    public class CustomersControllerTests : IDisposable
    {
        private readonly TmmDbContext _context;
        private List<Customer> mockCustomers;

        public CustomersControllerTests()
        {
            var options = new DbContextOptionsBuilder<TmmDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TmmDbContext(options);

            mockCustomers = new List<Customer>
            {
                new Customer { Id = 1, Title = "Mr", Forename = "John", Surname = "Doe", EmailAddress = "john.doe@example.com", MobileNo = "1234567890" },
                new Customer { Id = 2, Title = "Ms", Forename = "Jane", Surname = "Smith", EmailAddress = "jane.smith@example.com", MobileNo = "0987654321" }
            };

            _context.Customers.AddRange(mockCustomers);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetCustomers_ReturnsAllCustomers()
        {
            var controller = new CustomersController(_context);

            var result = await controller.GetCustomers();

            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count());
        }

        [Fact]
        public async Task GetCustomer_ValidId_ReturnsCustomer()
        {
            var controller = new CustomersController(_context);
            var result = await controller.GetCustomer(1);

            var actionResult = Assert.IsType<ActionResult<Customer>>(result);
            var customer = Assert.IsType<Customer>(actionResult.Value);

            Assert.Equal("John", customer.Forename);
        }

        [Fact]
        public async Task GetCustomer_InvalidId_ReturnsNotFound()
        {
            var controller = new CustomersController(_context);
            var result = await controller.GetCustomer(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task AddCustomer_ValidCustomer_ReturnsCustomer()
        {
            var newCustomer = new Customer { Id = 3, Title = "Dr", Forename = "Sam", Surname = "Brown", EmailAddress = "sam.brown@example.com", MobileNo = "1122334455" };
            
            var controller = new CustomersController(_context);
            var result = await controller.AddCustomer(newCustomer);

            var actionResult = Assert.IsType<ActionResult<Customer>>(result);
            var customer = Assert.IsType<Customer>(actionResult.Value);

            Assert.Equal("Sam", customer.Forename);
        }

        [Fact]
        public async Task DeleteCustomer_ValidId_RemovesCustomer()
        {
            var controller = new CustomersController(_context);
            var result = await controller.DeleteCustomer(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Null(_context.Customers.Find(1));
        }

        [Fact]
        public async Task DeleteCustomer_InvalidId_ReturnsNotFound()
        {
            var controller = new CustomersController(_context);
            var result = await controller.DeleteCustomer(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateCustomer_ValidId_UpdatesCustomer()
        {
            var updatedCustomer = new Customer { Id = 1, Title = "Mr", Forename = "John", Surname = "Dawson", EmailAddress = "john.dawson@example.com", MobileNo = "1234567899" };
            
            var controller = new CustomersController(_context);
            var result = await controller.UpdateCustomer(1, updatedCustomer);

            var updatedEntity = _context.Customers.Find(1);
            Assert.Equal("Dawson", updatedEntity.Surname);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateCustomer_InvalidId_ReturnsBadRequest()
        {
            var updatedCustomer = new Customer { Id = 99, Title = "Mr", Forename = "Invalid", Surname = "User", EmailAddress = "invalid.user@example.com", MobileNo = "9999999999" };
            
            var controller = new CustomersController(_context);
            var result = await controller.UpdateCustomer(99, updatedCustomer);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ListActiveCustomers_ReturnsActiveCustomers()
        {
            mockCustomers[1].IsActive = false;
            _context.SaveChanges();
            
            var controller = new CustomersController(_context);
            var result = await controller.GetActiveCustomers();

            Assert.Single(result.Value);
            Assert.Equal("John", result.Value.First().Forename);
        }

        [Fact]
        public async Task MarkCustomerAsInactive_ValidId_SetsIsActiveToFalse()
        {
            var controller = new CustomersController(_context);
            var result = await controller.MarkAsInactive(1);

            var updatedEntity = _context.Customers.Find(1);
            Assert.False(updatedEntity.IsActive);
        }

        [Fact]
        public async Task MarkCustomerAsInactive_InvalidId_ReturnsNotFound()
        {
            var controller = new CustomersController(_context);
            var result = await controller.MarkAsInactive(99);

            Assert.IsType<NotFoundResult>(result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
