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
    public class AddressesControllerTests
    {
        private readonly TmmDbContext _context;
        private readonly AddressesController _controller;
        private List<Address> _addresses;

        public AddressesControllerTests()
        {
            // Using in-memory database for testing.
            var options = new DbContextOptionsBuilder<TmmDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryTmmDbForTesting")
                .Options;

            _context = new TmmDbContext(options);

            // Sample data to use for testing.
            _addresses = new List<Address>
            {
                new Address { Id = 1, AddressLine1 = "123 Main St", CustomerId = 1 },
                new Address { Id = 2, AddressLine1 = "456 Side St", CustomerId = 1 }
            };

            _context.Addresses.AddRange(_addresses);
            _context.SaveChanges();

            _controller = new AddressesController(_context);
        }

        [Fact]
        public async Task GetAddressesForCustomer_ReturnsAllAddressesForCustomer()
        {
            // Act
            var result = await _controller.GetAddressesForCustomer(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Address>>>(result);
            var returnValue = Assert.IsType<List<Address>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetAddressForCustomer_ReturnsSpecificAddress()
        {
            // Act
            var result = await _controller.GetAddressForCustomer(1, 1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Address>>(result);
            var returnValue = Assert.IsType<Address>(actionResult.Value);
            Assert.Equal(1, returnValue.Id);
        }

        [Fact]
        public async Task AddAddressForCustomer_AddsAddressSuccessfully()
        {
            var newAddress = new Address { AddressLine1 = "789 Another St", CustomerId = 1 };

            // Act
            var result = await _controller.AddAddressForCustomer(1, newAddress);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Address>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Address>(createdAtActionResult.Value);
            Assert.Equal("789 Another St", returnValue.AddressLine1);
        }

        [Fact]
        public async Task UpdateAddressForCustomer_UpdatesAddressSuccessfully()
        {
            var updatedAddress = new Address { Id = 1, AddressLine1 = "1234 Updated St", CustomerId = 1 };

            // Act
            var result = await _controller.UpdateAddressForCustomer(1, 1, updatedAddress);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAddressForCustomer_DeletesAddressSuccessfully()
        {
            // Act
            var result = await _controller.DeleteAddressForCustomer(1, 2);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.DoesNotContain(_addresses, a => a.Id == 2);
        }

        [Fact]
        public async Task DeleteAddressForCustomer_ReturnsBadRequest_WhenOnlyOneAddressExists()
        {
            // Let's simulate a situation where there's only one address.
            _addresses.RemoveAll(a => a.Id != 1);

            // Act
            var result = await _controller.DeleteAddressForCustomer(1, 1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
