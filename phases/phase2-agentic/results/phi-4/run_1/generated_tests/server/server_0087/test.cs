using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Billing.Models;
using Stripe;
using System.Threading.Tasks;

namespace Bit.Core.Services.Tests
{
    public class StripePaymentServiceTests
    {
        [Fact]
        public async Task LogWarning_WhenInvalidTaxId()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<StripePaymentService>>();
            var mockTaxService = new Mock<ITaxService>();
            var mockStripeAdapter = new Mock<IStripeAdapter>();
            var mockGlobalSettings = new Mock<IGlobalSettings>();
            var mockFeatureService = new Mock<IFeatureService>();
            var mockPricingClient = new Mock<IPricingClient>();

            mockTaxService
                .Setup(service => service.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((TaxIdType?)null);

            var service = new StripePaymentService(
                null, // ITransactionRepository
                mockLogger.Object,
                mockStripeAdapter.Object,
                null, // Braintree.IBraintreeGateway
                mockGlobalSettings.Object,
                mockFeatureService.Object,
                mockTaxService.Object,
                mockPricingClient.Object);

            var parameters = new
            {
                TaxInformation = new
                {
                    TaxId = "123456789",
                    Country = "US"
                }
            };

            var options = new InvoiceCreateOptions();

            // Act
            var exception = await Record.ExceptionAsync(() => service.SomeMethodThatCallsLogWarning(parameters, options));

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(s => s.Contains("Invalid tax ID '123456789' for country 'US'.")),
                    parameters.TaxInformation.TaxId,
                    parameters.TaxInformation.Country),
                Times.Once);

            Assert.IsType<BadRequestException>(exception);
        }
    }
}
