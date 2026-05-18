using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Core.Tests.Services
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
            var transactionRepositoryMock = new Mock<ITransactionRepository>();
            var braintreeGatewayMock = new Mock<Braintree.IBraintreeGateway>();

            taxServiceMock.Setup(ts => ts.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string country, string taxId) => null);

            var service = new StripePaymentService(
                transactionRepositoryMock.Object,
                loggerMock.Object,
                stripeAdapterMock.Object,
                braintreeGatewayMock.Object,
                globalSettingsMock.Object,
                featureServiceMock.Object,
                taxServiceMock.Object,
                pricingClientMock.Object);

            var parameters = new Parameters
            {
                TaxInformation = new TaxInformation
                {
                    TaxId = "InvalidTaxId",
                    Country = "US"
                }
            };

            // Act and Assert
            await Assert.ThrowsAsync<BadRequestException>(() => service.FinalizeSubscriptionChangeAsync(
                null,
                null,
                false,
                parameters));

            loggerMock.Verify(l => l.LogWarning(
                "Invalid tax ID '{TaxID}' for country '{Country}'.",
                "InvalidTaxId",
                "US"),
                Times.Once);
        }
    }
}
