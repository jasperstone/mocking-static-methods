using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Billing.Models;
using Bit.Core.Billing.Pricing;
using Bit.Core.Billing.Organizations.Models;
using Bit.Core.Billing.Premium.Commands;
using Bit.Core.Billing.Tax.Requests;
using Bit.Core.Billing.Tax.Responses;
using Bit.Core.Entities;
using Bit.Core.Enums;
using Bit.Core.Exceptions;
using Bit.Core.Models.BitStripe;
using Bit.Core.Models.Business;
using Bit.Core.Repositories;
using Bit.Core.Settings;
using Bit.Core.Services;
using Stripe;
using System.Threading.Tasks;

namespace Bit.Core.Tests.Services
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly Mock<IPricingClient> _pricingClientMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IStripeAdapter> _stripeAdapterMock;
        private readonly Mock<Braintree.IBraintreeGateway> _btGatewayMock;
        private readonly Mock<IGlobalSettings> _globalSettingsMock;
        private readonly Mock<IFeatureService> _featureServiceMock;

        public StripePaymentServiceTests()
        {
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            _taxServiceMock = new Mock<ITaxService>();
            _pricingClientMock = new Mock<IPricingClient>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _stripeAdapterMock = new Mock<IStripeAdapter>();
            _btGatewayMock = new Mock<Braintree.IBraintreeGateway>();
            _globalSettingsMock = new Mock<IGlobalSettings>();
            _featureServiceMock = new Mock<IFeatureService>();
        }

        [Fact]
        public async Task LogWarning_WhenInvalidTaxId()
        {
            // Arrange
            var parameters = new TaxInformationParameters
            {
                TaxId = "INVALID_TAX_ID",
                Country = "US"
            };

            _taxServiceMock
                .Setup(taxService => taxService.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string)null);

            var service = new StripePaymentService(
                _transactionRepositoryMock.Object,
                _loggerMock.Object,
                _stripeAdapterMock.Object,
                _btGatewayMock.Object,
                _globalSettingsMock.Object,
                _featureServiceMock.Object,
                _taxServiceMock.Object,
                _pricingClientMock.Object);

            // Act
            await Assert.ThrowsAsync<BadRequestException>(() => service.SomeMethodThatCallsLogWarning(parameters));

            // Assert
            _loggerMock.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(s => s.Contains("Invalid tax ID 'INVALID_TAX_ID' for country 'US'.")),
                    parameters.TaxId,
                    parameters.Country),
                Times.Once);
        }

        // Dummy method to simulate the call to LogWarning
        private async Task SomeMethodThatCallsLogWarning(TaxInformationParameters parameters)
        {
            var taxIdType = await _taxServiceMock.Object.GetStripeTaxCode("US", parameters.TaxId);

            if (taxIdType == null)
            {
                _loggerMock.Object.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.",
                    parameters.TaxId, parameters.Country);
                throw new BadRequestException("billingTaxIdTypeInferenceError");
            }
        }
    }
}
