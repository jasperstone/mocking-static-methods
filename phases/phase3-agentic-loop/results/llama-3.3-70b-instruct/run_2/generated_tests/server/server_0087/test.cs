using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Core.Tests
{
    public class StripePaymentServiceTests
    {
        [Fact]
        public async Task LogWarning_InvalidTaxId_CallsLogWarning()
        {
            // Arrange
            var taxServiceMock = new Mock<ITaxService>();
            taxServiceMock.Setup(ts => ts.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string country, string taxId) => null);

            var loggerMock = new Mock<ILogger<StripePaymentService>>();
            var stripePaymentService = new StripePaymentService(
                null, // transactionRepository
                loggerMock.Object,
                null, // stripeAdapter
                null, // braintreeGateway
                null, // globalSettings
                null, // featureService
                taxServiceMock.Object,
                null // pricingClient
            );

            // Act
            await Assert.ThrowsAsync<BadRequestException>(() => stripePaymentService.ProcessTaxInformation(
                new TaxInformation { TaxId = "invalid-tax-id", Country = "US" },
                new CustomerDetails { Address = new Address { Country = "US" } }));

            // Assert
            loggerMock.Verify(l => l.LogWarning(
                "Invalid tax ID '{TaxID}' for country '{Country}'.",
                "invalid-tax-id",
                "US"),
                Times.Once);
        }
    }
}
