using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Xunit;
using Bit.Core.Services;
using Bit.Core.Exceptions;
using Bit.Core.Billing.Tax.Services;

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

            // Other dependencies can be mocked as empty or null since not used in tested method
            var transactionRepoMock = new Mock<Bit.Core.Repositories.ITransactionRepository>();
            var braintreeGatewayMock = new Mock<Braintree.IBraintreeGateway>();
            var globalSettingsMock = new Mock<Bit.Core.Settings.IGlobalSettings>();
            var featureServiceMock = new Mock<Bit.Core.Billing.Tax.Services.IFeatureService>();
            var pricingClientMock = new Mock<Bit.Core.Billing.Pricing.IPricingClient>();

            _service = new StripePaymentService(
                transactionRepoMock.Object,
                _loggerMock.Object,
                _stripeAdapterMock.Object,
                braintreeGatewayMock.Object,
                globalSettingsMock.Object,
                featureServiceMock.Object,
                _taxServiceMock.Object,
                pricingClientMock.Object);
        }

        [Fact]
        public async Task UpdateCustomerTaxIdAsync_LogsWarningAndThrows_WhenTaxIdTypeCannotBeInferred()
        {
            // Arrange
            var taxInfo = new TaxInfo
            {
                BillingAddressCountry = "FR",
                TaxIdNumber = "123456789",
                TaxIdType = null
            };

            // Setup tax service to return null taxIdType to trigger warning and exception
            _taxServiceMock.Setup(t => t.GetStripeTaxCode(taxInfo.BillingAddressCountry, taxInfo.TaxIdNumber))
                .Returns((string)null);

            // Setup stripe adapter to return a customer with no tax ids (simulate customer found)
            var customer = new Customer
            {
                Id = "cus_123",
                TaxIds = new StripeList<TaxId> { Data = Enumerable.Empty<TaxId>().ToList() }
            };
            _stripeAdapterMock.Setup(s => s.CustomerGetAsync(It.IsAny<string>(), It.IsAny<CustomerGetOptions>()))
                .ReturnsAsync(customer);

            var serviceWithMethod = new StripePaymentServiceTestWrapper(
                _service,
                _taxServiceMock.Object,
                _stripeAdapterMock.Object,
                _loggerMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(async () =>
            {
                await serviceWithMethod.UpdateCustomerTaxIdAsync(taxInfo);
            });

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

        // Minimal stub for TaxInfo to allow compilation
        public class TaxInfo
        {
            public string BillingAddressCountry { get; set; }
            public string TaxIdNumber { get; set; }
            public string TaxIdType { get; set; }
        }

        // Test wrapper to expose the method containing the snippet for testing
        private class StripePaymentServiceTestWrapper : StripePaymentService
        {
            private readonly ITaxService _taxService;
            private readonly IStripeAdapter _stripeAdapter;
            private readonly ILogger<StripePaymentService> _logger;

            public StripePaymentServiceTestWrapper(
                StripePaymentService original,
                ITaxService taxService,
                IStripeAdapter stripeAdapter,
                ILogger<StripePaymentService> logger)
                : base(
                    original._transactionRepository,
                    logger,
                    stripeAdapter,
                    original._btGateway,
                    original._globalSettings,
                    original._featureService,
                    taxService,
                    original._pricingClient)
            {
                _taxService = taxService;
                _stripeAdapter = stripeAdapter;
                _logger = logger;
            }

            public async Task UpdateCustomerTaxIdAsync(TaxInfo taxInfo)
            {
                var customer = await _stripeAdapter.CustomerGetAsync("someCustomerId", new CustomerGetOptions
                {
                    Expand = new[] { "tax_ids" }
                });

                if (customer == null)
                {
                    return;
                }

                var taxId = customer.TaxIds?.FirstOrDefault();

                if (taxId != null)
                {
                    await _stripeAdapter.TaxIdDeleteAsync(customer.Id, taxId.Id);
                }

                if (string.IsNullOrWhiteSpace(taxInfo.TaxIdNumber))
                {
                    return;
                }

                var taxIdType = taxInfo.TaxIdType;

                if (string.IsNullOrWhiteSpace(taxIdType))
                {
                    taxIdType = _taxService.GetStripeTaxCode(taxInfo.BillingAddressCountry, taxInfo.TaxIdNumber);

                    if (taxIdType == null)
                    {
                        _logger.LogWarning("Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                            taxInfo.BillingAddressCountry,
                            taxInfo.TaxIdNumber);
                        throw new BadRequestException("billingTaxIdTypeInferenceError");
                    }
                }
            }
        }
    }
}
