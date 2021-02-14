using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SolarCoffee.Services.Customer;
using SolarCoffee.Web.Serialization;
using SolarCoffee.Web.ViewModels;

namespace SolarCoffee.Web.Controllers
{
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ICustomerService _customerService;

        public CustomerController(ILogger<CustomerController> logger, ICustomerService customerService)
        {
            _logger = logger;
            _customerService = customerService;
        }

        [HttpPost("/api/customer")]
        public IActionResult CreateCustomer([FromBody] CustomerModel customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating a new Customer");
            customer.CreatedOn = DateTime.UtcNow;
            customer.UpdatedOn = DateTime.UtcNow;
            var customerData = CustomerMapper.SerializeCustomer(customer);
            var newCustomer = _customerService.CreateCustomer(customerData);
            return Ok(newCustomer);
        }

        [HttpGet("/api/customer")]
        public IActionResult GetCustomers()
        {
            _logger.LogInformation("Getting customers");

            var customers = _customerService.GetAllCustomers();
            var customerModels = CustomerMapper.SerializeCustomers(customers)
                .OrderByDescending(customer => customer.CreatedOn)
                .ToList();
            return Ok(customerModels);
        }

        [HttpDelete("/api/customer/{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            _logger.LogInformation("Deleting Customer");
            var response = _customerService.DeleteCustomer(id);
            return Ok(response);
        }

    }
}