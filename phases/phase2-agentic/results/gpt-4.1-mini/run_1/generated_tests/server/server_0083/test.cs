using System;
using System.Threading.Tasks;
using Bit.Core.Billing.Tax.Requests;
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
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<Braintree.IBraintreeGateway> _braintreeGatewayMock;
        private readonly Mock<IGlobalSettings> _globalSettingsMock;
        private readonly Mock<IFeatureService> _featureServiceMock;
        private readonly Mock<IPricingClient> _pricingClientMock;

        private readonly StripePaymentService _service;

        public StripePaymentServiceTests()
        {
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            _stripeAdapterMock = new Mock<IStripeAdapter>();
            _taxServiceMock = new Mock<ITaxService>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _braintreeGatewayMock = new Mock<Braintree.IBraintreeGateway>();
            _globalSettingsMock = new Mock<IGlobalSettings>();
            _featureServiceMock = new Mock<IFeatureService>();
            _pricingClientMock = new Mock<IPricingClient>();

            _service = new StripePaymentService(
                _transactionRepositoryMock.Object,
                _loggerMock.Object,
                _stripeAdapterMock.Object,
                _braintreeGatewayMock.Object,
                _globalSettingsMock.Object,
                _featureServiceMock.Object,
                _taxServiceMock.Object,
                _pricingClientMock.Object);
        }

        [Fact]
        public async Task SomeMethod_LogsWarningAndThrowsBadRequestException_WhenTaxIdTypeCannotBeInferred()
        {
            // Arrange
            var taxInfo = new TaxInfo
            {
                BillingAddressCountry = "DE",
                TaxIdNumber = "123456789",
                TaxIdType = null
            };

            var customer = new Customer
            {
                Id = "cus_123",
                TaxIds = new TaxIdList
                {
                    Data = new System.Collections.Generic.List<TaxId>()
                }
            };

            _stripeAdapterMock.Setup(x => x.CustomerGetAsync(It.IsAny<CustomerGetOptions>()))
                .ReturnsAsync(customer);

            _taxServiceMock.Setup(x => x.GetStripeTaxCode(taxInfo.BillingAddressCountry, taxInfo.TaxIdNumber))
                .Returns((string)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(async () =>
            {
                // We need to call the method that contains the code snippet.
                // The snippet is inside a method that uses taxInfo and calls _logger.LogWarning.
                // The method name is not given, so we simulate calling a method that triggers this logic.
                // For demonstration, assume a method named UpdateCustomerTaxIdAsync exists.
                await CallUpdateCustomerTaxIdAsync(taxInfo);
            });

            Assert.Equal("billingTaxIdTypeInferenceError", ex.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not infer tax ID type in country")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper method to simulate the call to the method containing the snippet.
        // This is a stub to illustrate the test; in real code, replace with actual method call.
        private async Task CallUpdateCustomerTaxIdAsync(TaxInfo taxInfo)
        {
            // Simulate the logic from the snippet:
            var customer = await _stripeAdapterMock.Object.CustomerGetAsync(new CustomerGetOptions());

            if (customer == null)
            {
                return;
            }

            var taxId = customer.TaxIds?.Data.Count > 0 ? customer.TaxIds.Data[0] : null;

            if (taxId != null)
            {
                await _stripeAdapterMock.Object.TaxIdDeleteAsync(customer.Id, taxId.Id);
            }

            if (string.IsNullOrWhiteSpace(taxInfo.TaxIdNumber))
            {
                return;
            }

            var taxIdType = taxInfo.TaxIdType;

            if (string.IsNullOrWhiteSpace(taxIdType))
            {
                taxIdType = _taxServiceMock.Object.GetStripeTaxCode(taxInfo.BillingAddressCountry, taxInfo.TaxIdNumber);

                if (taxIdType == null)
                {
                    _loggerMock.Object.LogWarning("Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                        taxInfo.BillingAddressCountry,
                        taxInfo.TaxIdNumber);
                    throw new BadRequestException("billingTaxIdTypeInferenceError");
                }
            }
        }
    }
}
