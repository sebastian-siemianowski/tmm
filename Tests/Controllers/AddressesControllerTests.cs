using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tmm.Controllers;
using Tmm.Data;
using Tmm.Models;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class AddressesControllerTests : IDisposable
    {
        private readonly TmmDbContext _context;
        private readonly AddressesController _controller;
        private List<Address> _addresses;

        public AddressesControllerTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<TmmDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            _context = new TmmDbContext(options);
            SeedData();
            _controller = new AddressesController(_context);
        }

        private void SeedData()
        {
            var customer = new Customer
            {
                Title = "Mr.",
                Forename = "John",
                Surname = "Doe",
                EmailAddress = "johndoe@example.com",
                MobileNo = "123-456-7890"
            };

            _context.Customers.Add(customer);
            _context.SaveChanges();

            _addresses = new List<Address>
            {
                new Address 
                { 
                    AddressLine1 = "123 Main St", 
                    AddressLine2 = "Apt 4B", 
                    County = "Test County", 
                    Postcode = "12345", 
                    Town = "TestTown",
                    CustomerId = customer.Id
                },
                new Address 
                { 
                    AddressLine1 = "456 Side St", 
                    AddressLine2 = "Suite 7A",
                    County = "Test County", 
                    Postcode = "67890", 
                    Town = "TestTown",
                    CustomerId = customer.Id
                }
            };

            _context.Addresses.AddRange(_addresses);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAddressesForCustomer_ReturnsAllAddressesForCustomer()
        {
            var result = await _controller.GetAddressesForCustomer(1);
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Address>>>(result);
            var returnValue = Assert.IsType<List<Address>>(actionResult.Value);
            Assert.Equal(_addresses.Count, returnValue.Count);
        }

        [Fact]
        public async Task GetAddressForCustomer_ReturnsSpecificAddress()
        {
            var firstAddressId = _addresses[0].Id;
            var result = await _controller.GetAddressForCustomer(1, firstAddressId);
            var actionResult = Assert.IsType<ActionResult<Address>>(result);
            var returnValue = Assert.IsType<Address>(actionResult.Value);
            Assert.Equal(firstAddressId, returnValue.Id);
        }

        [Fact]
        public async Task AddAddressForCustomer_AddsAddressSuccessfully()
        {
            var newAddress = new Address 
            { 
                AddressLine1 = "789 Another St", 
                AddressLine2 = "Unit 10",
                County = "Another County",
                Postcode = "54321",
                Town = "AnotherTown",
                CustomerId = 1 
            };

            var result = await _controller.AddAddressForCustomer(1, newAddress);
            var actionResult = Assert.IsType<ActionResult<Address>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Address>(createdAtActionResult.Value);
            Assert.Equal("789 Another St", returnValue.AddressLine1);
        }

        [Fact]
        public async Task UpdateAddressForCustomer_UpdatesAddressSuccessfully()
        {
            var firstAddressId = _addresses[0].Id;
            var updatedAddress = new Address 
            { 
                Id = firstAddressId, 
                AddressLine1 = "1234 Updated St", 
                AddressLine2 = "Apt 4B Updated",
                County = "Updated County",
                Postcode = "11223",
                Town = "UpdatedTown",
                CustomerId = 1 
            };

            var result = await _controller.UpdateAddressForCustomer(1, firstAddressId, updatedAddress);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAddressForCustomer_DeletesAddressSuccessfully()
        {
            var secondAddressId = _addresses[1].Id;
            var result = await _controller.DeleteAddressForCustomer(1, secondAddressId);
            Assert.IsType<NoContentResult>(result);
            Assert.DoesNotContain(_context.Addresses, a => a.Id == secondAddressId);
        }

        [Fact]
        public async Task DeleteAddressForCustomer_ReturnsBadRequest_WhenOnlyOneAddressExists()
        {
            var addressToKeep = _addresses[0];
            foreach (var address in _context.Addresses)
            {
                if (address.Id != addressToKeep.Id)
                {
                    _context.Addresses.Remove(address);
                }
            }
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteAddressForCustomer(1, addressToKeep.Id);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
