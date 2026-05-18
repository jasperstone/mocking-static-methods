using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Threading.Tasks;

namespace Bit.Core.Services.Tests
{
    public class StripePaymentServiceTests
    {
        [Fact]
        public async Task LogWarningCalled_WhenTaxIdTypeCannotBeInferred()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<StripePaymentService>>();
            var mockStripeAdapter = new Mock<IStripeAdapter>();
            var mockTaxService = new Mock<ITaxService>();

            var taxInfo = new TaxInfo
            {
                BillingAddressCountry = "US",
                TaxIdNumber = "123456789",
                TaxIdType = null
            };

            mockTaxService
                .Setup(service => service.GetStripeTaxCode(taxInfo.BillingAddressCountry, taxInfo.TaxIdNumber))
                .Returns((string)null);

            var service = new StripePaymentService(
                null, // Mock or real ITransactionRepository
                mockLogger.Object,
                mockStripeAdapter.Object,
                null, // Mock or real Braintree.IBraintreeGateway
                null, // Mock or real IGlobalSettings
                null, // Mock or real IFeatureService
                mockTaxService.Object,
                null  // Mock or real IPricingClient
            );

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => service.ProcessTaxInfo(taxInfo));

            mockLogger.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(s => s.Contains("Could not infer tax ID type in country 'US' with tax ID '123456789'.")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
