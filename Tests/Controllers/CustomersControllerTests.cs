using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Tmm.Controllers;
using Tmm.Data;
using Tmm.Models;

namespace Tmm.Tests
{
    public class CustomersControllerTests : IDisposable
    {
        private readonly TmmDbContext _context;
        private readonly List<Customer> _mockCustomers;
        private readonly CustomersController _controller;

        // Initializes context, controller, and seeds mock data.
        public CustomersControllerTests()
        {
            _context = CreateInMemoryDbContext();
            _controller = new CustomersController(_context);
            _mockCustomers = GenerateMockCustomers();
            SeedDatabase(_mockCustomers);
        }

        // Creates an in-memory database for testing.
        private TmmDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<TmmDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new TmmDbContext(options);
        }

        // Returns a list of mock customer data.
        private List<Customer> GenerateMockCustomers() => new List<Customer>
        {
            // Sample customer data.
            new Customer { Id = 1, Title = "Mr", Forename = "John", Surname = "Doe", EmailAddress = "john.doe@example.com", MobileNo = "1234567890" },
            new Customer { Id = 2, Title = "Ms", Forename = "Jane", Surname = "Smith", EmailAddress = "jane.smith@example.com", MobileNo = "0987654321" }
        };

        // Seeds the database with mock customer data.
        private void SeedDatabase(List<Customer> customers)
        {
            _context.Customers.AddRange(customers);
            _context.SaveChanges();
        }

        // Tests if all customers are returned.
        [Fact]
        public async Task GetCustomers_ShouldReturnAllCustomers()
        {
            var result = await _controller.GetAllCustomers();
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count());
        }

        // Tests retrieval of a specific customer by ID.
        [Fact]
        public async Task GetCustomer_WithValidId_ShouldReturnCorrectCustomer()
        {
            var result = await _controller.GetCustomerById(1);
            var actionResult = Assert.IsType<ActionResult<Customer>>(result);
            var customer = Assert.IsType<Customer>(actionResult.Value);
            Assert.Equal("John", customer.Forename);
        }

        // Tests response when an invalid ID is used for customer retrieval.
        [Fact]
        public async Task GetCustomer_WithInvalidId_ShouldReturnNotFound()
        {
            var result = await _controller.GetCustomerById(99);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        // Tests if a customer can be added correctly.
        [Fact]
        public async Task AddCustomer_ShouldAddAndReturnNewCustomer_GivenValidInput()
        {
            var newCustomer = new Customer { Id = 3, Title = "Dr", Forename = "Sam", Surname = "Brown", EmailAddress = "sam.brown@example.com", MobileNo = "1122334455" };
            var result = await _controller.CreateCustomer(newCustomer);
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var customer = Assert.IsType<Customer>(actionResult.Value);
            Assert.Equal("Sam", customer.Forename);
        }

        // Tests if a customer can be deleted given a valid ID.
        [Fact]
        public async Task DeleteCustomer_WithValidId_ShouldRemoveCustomer()
        {
            var result = await _controller.DeleteCustomer(1);
            Assert.IsType<NoContentResult>(result);
            Assert.Null(_context.Customers.Find(1));
        }

        // Tests response when attempting to delete a customer with an invalid ID.
        [Fact]
        public async Task DeleteCustomer_WithInvalidId_ShouldReturnNotFound()
        {
            var result = await _controller.DeleteCustomer(99);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // Tests if a customer's details can be updated correctly.
        [Fact]
        public async Task UpdateCustomer_ShouldUpdateCustomerDetails_GivenValidId()
        {
            DetachAllEntities();
            var updatedCustomer = new Customer
            {
                Id = 1,
                Title = "Mr",
                Forename = "John",
                Surname = "Dawson",
                EmailAddress = "john.dawson@example.com",
                MobileNo = "1234567899"
            };
            var result = await _controller.UpdateCustomer(1, updatedCustomer);
            var updatedEntity = _context.Customers.Find(1);
            Assert.Equal("Dawson", updatedEntity.Surname);
            Assert.IsType<NoContentResult>(result);
        }

        // Tests response when attempting to update a customer with an invalid ID.
        [Fact]
        public async Task UpdateCustomer_WithInvalidId_ShouldReturnNotFound()
        {
            var updatedCustomer = new Customer { Id = 99, Title = "Mr", Forename = "Invalid", Surname = "User", EmailAddress = "invalid.user@example.com", MobileNo = "9999999999" };
            var result = await _controller.UpdateCustomer(99, updatedCustomer);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // Tests retrieval of only active customers.
        [Fact]
        public async Task GetActiveCustomers_ShouldReturnAllActiveCustomers()
        {
            var customerToDeactivate = _context.Customers.Find(2);
            customerToDeactivate.IsActive = false;
            _context.SaveChanges();
            var result = await _controller.GetActiveCustomers();
            var response = result.Value;
            Assert.Single(response);
            Assert.Equal("John", response.First().Forename);
        }

        // Tests if a customer's status can be set to inactive correctly.
        [Fact]
        public async Task MarkCustomerAsInactive_WithValidId_ShouldSetIsActiveToFalse()
        {
            var result = await _controller.DeactivateCustomer(1);
            var updatedEntity = _context.Customers.Find(1);
            Assert.False(updatedEntity.IsActive);
        }

        // Tests response when attempting to mark a customer as inactive with an invalid ID.
        [Fact]
        public async Task MarkCustomerAsInactive_WithInvalidId_ShouldReturnNotFound()
        {
            var result = await _controller.DeactivateCustomer(99);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // Detaches all entities to prevent tracking.
        private void DetachAllEntities()
        {
            foreach (var entity in _context.ChangeTracker.Entries())
            {
                entity.State = EntityState.Detached;
            }
        }

        // Clean up resources after tests.
        public void Dispose()
        {
            ClearDatabase();
            _context.Dispose();
        }

        // Clear all data from the in-memory database.
        private void ClearDatabase()
        {
            foreach (var entity in _context.ChangeTracker.Entries<Customer>())
            {
                entity.State = EntityState.Detached;
            }

            foreach (var entity in _context.Customers)
            {
                _context.Customers.Remove(entity);
}

            _context.SaveChanges();
        }
    }
}
