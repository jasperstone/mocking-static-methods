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
            var parameters = new CreateInvoiceParameters
            {
                TaxInformation = new TaxInformation
                {
                    TaxId = "INVALID_TAX_ID",
                    Country = "US"
                }
            };

            _taxServiceMock
                .Setup(taxService => taxService.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((TaxCode)null);

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
            await service.CreateInvoiceAsync(parameters);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(s => s.Contains("Invalid tax ID 'INVALID_TAX_ID' for country 'US'.")),
                    parameters.TaxInformation.TaxId,
                    parameters.TaxInformation.Country),
                Times.Once);
        }
    }
}
