using System;
using System.Threading.Tasks;
using Bit.Core.Billing.Tax.Models;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Exceptions;
using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Xunit;

namespace Bit.Core.Tests.Services
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
        private readonly Mock<IStripeAdapter> _stripeAdapterMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly StripePaymentService _service;

        public StripePaymentServiceTests()
        {
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            _stripeAdapterMock = new Mock<IStripeAdapter>();
            _taxServiceMock = new Mock<ITaxService>();

            // Other dependencies are not needed for this test, so we pass null or mocks
            _service = new StripePaymentService(
                transactionRepository: null,
                logger: _loggerMock.Object,
                stripeAdapter: _stripeAdapterMock.Object,
                braintreeGateway: null,
                globalSettings: null,
                featureService: null,
                taxService: _taxServiceMock.Object,
                pricingClient: null);
        }

        [Fact]
        public async Task UpdateCustomerTaxIdAsync_LogsWarningAndThrows_WhenTaxIdTypeCannotBeInferred()
        {
            // Arrange
            var taxInfo = new BillingTaxInfo
            {
                BillingAddressCountry = "DE",
                TaxIdNumber = "123456789",
                TaxIdType = null
            };

            var customer = new Customer
            {
                Id = "cus_123",
                TaxIds = new TaxIdList { Data = new System.Collections.Generic.List<TaxId>() }
            };

            _stripeAdapterMock.Setup(x => x.CustomerGetAsync(It.IsAny<string>(), It.IsAny<CustomerGetOptions>()))
                .ReturnsAsync(customer);

            _stripeAdapterMock.Setup(x => x.TaxIdDeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _taxServiceMock.Setup(x => x.GetStripeTaxCode(taxInfo.BillingAddressCountry, taxInfo.TaxIdNumber))
                .Returns((string)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
                _service.UpdateCustomerTaxIdAsync("org_123", taxInfo));

            Assert.Equal("billingTaxIdTypeInferenceError", ex.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not infer tax ID type")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal stub classes to support the test
    public class BillingTaxInfo
    {
        public string BillingAddressCountry { get; set; }
        public string TaxIdNumber { get; set; }
        public string TaxIdType { get; set; }
    }

    public class Customer
    {
        public string Id { get; set; }
        public TaxIdList TaxIds { get; set; }
    }

    public class TaxIdList
    {
        public System.Collections.Generic.List<TaxId> Data { get; set; } = new System.Collections.Generic.List<TaxId>();
    }

    public class TaxId
    {
        public string Id { get; set; }
    }
}
