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

namespace Tests
{
    public class AddressesControllerTests : IDisposable
    {
        private readonly TmmDbContext _context;
        private readonly AddressesController _controller;
        private List<Address> _testAddresses;

        public AddressesControllerTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<TmmDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            _context = new TmmDbContext(options);
            SeedDatabaseWithTestData();
            _controller = new AddressesController(_context);
        }

        private void SeedDatabaseWithTestData()
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

            _testAddresses = new List<Address>
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

            _context.Addresses.AddRange(_testAddresses);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAddressesForCustomer_ShouldReturnAllAddressesForGivenCustomer()
        {
            var result = await _controller.GetAddressesForCustomer(1);
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Address>>>(result);
            var addresses = Assert.IsType<List<Address>>(actionResult.Value);

            Assert.Equal(_testAddresses.Count, addresses.Count);
        }

        [Fact]
        public async Task GetAddressesForCustomer_ShouldReturnNotFoundForNonExistentCustomer()
        {
            var result = await _controller.GetAddressesForCustomer(999);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAddressForCustomer_ShouldReturnCorrectAddress()
        {
            var result = await _controller.GetAddressForCustomer(1, 1);
            var actionResult = Assert.IsType<ActionResult<Address>>(result);
            var address = Assert.IsType<Address>(actionResult.Value);

            Assert.Equal(_testAddresses[0].AddressLine1, address.AddressLine1);
        }

        [Fact]
        public async Task GetAddressForCustomer_ShouldReturnNotFoundForNonExistentAddress()
        {
            var result = await _controller.GetAddressForCustomer(1, 999);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddAddressForCustomer_ShouldIncreaseAddressCount()
        {
            var newAddress = new Address
            {
                AddressLine1 = "789 Second St",
                County = "Test County 2",
                Postcode = "11122",
                Town = "AnotherTown"
            };

            await _controller.AddAddressForCustomer(1, newAddress);

            var addresses = await _context.Addresses.ToListAsync();
            Assert.Equal(_testAddresses.Count + 1, addresses.Count);
        }

        [Fact]
        public async Task AddAddressForCustomer_ShouldReturnBadRequestForNullAddress()
        {
            var result = await _controller.AddAddressForCustomer(1, null);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Address cannot be null.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddAddressForCustomer_ShouldReturnNotFoundForNonExistentCustomer()
        {
            var newAddress = new Address
            {
                AddressLine1 = "789 Second St",
                County = "Test County 2",
                Postcode = "11122",
                Town = "AnotherTown"
            };
            var result = await _controller.AddAddressForCustomer(999, newAddress);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAddressForCustomer_ShouldModifyAddressDetails()
        {
            var updatedAddress = new Address
            {
                Id = 1,
                CustomerId = 1,
                AddressLine1 = "Updated Main St",
                AddressLine2 = "Apt 4B",
                County = "Updated County",
                Postcode = "54321",
                Town = "UpdatedTown"
            };

            await _controller.UpdateAddressForCustomer(1, 1, updatedAddress);

            var address = await _context.Addresses.FindAsync(1);
            Assert.Equal("Updated Main St", address.AddressLine1);
        }

        [Fact]
        public async Task UpdateAddressForCustomer_ShouldReturnBadRequestForMismatchedIds()
        {
            var updatedAddress = new Address
            {
                Id = 2,
                CustomerId = 1,
                AddressLine1 = "Updated Main St",
            };
            var result = await _controller.UpdateAddressForCustomer(1, 1, updatedAddress);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Mismatched address or customer IDs.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteAddressForCustomer_ShouldDecreaseAddressCount()
        {
            await _controller.DeleteAddressForCustomer(1, 1);

            var addresses = await _context.Addresses.ToListAsync();
            Assert.Equal(_testAddresses.Count - 1, addresses.Count);
        }

        [Fact]
        public async Task DeleteAddressForCustomer_ShouldReturnBadRequestWhenLastAddress()
        {
            await _controller.DeleteAddressForCustomer(1, 2);
            var result = await _controller.DeleteAddressForCustomer(1, 1);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("A customer must have at least one address.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddAddressForCustomer_WhenAddingMainAddress_ShouldUnsetExistingMainAddress()
        {
            var newMainAddress = new Address
            {
                AddressLine1 = "789 Main St",
                IsMain = true,
                Postcode = "YourPostcodeHere",
                Town = "YourTownHere"
            };


            await _controller.AddAddressForCustomer(1, newMainAddress);

            var addresses = await _context.Addresses.Where(a => a.CustomerId == 1).ToListAsync();
            Assert.Single(addresses.Where(a => a.IsMain));
        }

        [Fact]
        public async Task UpdateAddressForCustomer_WhenUpdatingToMainAddress_ShouldUnsetExistingMainAddress()
        {
            var updatedMainAddress = new Address
            {
                Id = 1,
                CustomerId = 1,
                AddressLine1 = "123 Main St",
                IsMain = true
            };
            await _controller.UpdateAddressForCustomer(1, 1, updatedMainAddress);

            var addresses = await _context.Addresses.Where(a => a.CustomerId == 1).ToListAsync();
            Assert.Single(addresses.Where(a => a.IsMain));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
