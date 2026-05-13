using Xunit;
using Moq;
using Bit.Core.Services;
using Microsoft.Extensions.Logging;

namespace Bit.Core.Tests
{
    public class StripePaymentServiceTests
    {
        [Fact]
        public async Task LogWarning_InvalidTaxId_CallsLogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StripePaymentService>>();
            var taxServiceMock = new Mock<ITaxService>();
            var stripeAdapterMock = new Mock<IStripeAdapter>();
            var globalSettingsMock = new Mock<IGlobalSettings>();
            var featureServiceMock = new Mock<IFeatureService>();
            var pricingClientMock = new Mock<IPricingClient>();

            taxServiceMock.Setup(ts => ts.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string country, string taxId) => null);

            var service = new StripePaymentService(
                null,
                loggerMock.Object,
                stripeAdapterMock.Object,
                null,
                globalSettingsMock.Object,
                featureServiceMock.Object,
                taxServiceMock.Object,
                pricingClientMock.Object);

            // Act
            await service.FinalizeSubscriptionChangeAsync(
                null,
                null,
                new Parameters
                {
                    TaxInformation = new TaxInformation
                    {
                        TaxId = "invalid-tax-id",
                        Country = "US"
                    }
                });

            // Assert
            loggerMock.Verify(l => l.LogWarning(
                "Invalid tax ID '{TaxID}' for country '{Country}'.",
                "invalid-tax-id",
                "US"),
                Times.Once);
        }
    }
}
