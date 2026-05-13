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
            taxServiceMock.Setup(ts => ts.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>())).Returns((string?)null);
            var service = new StripePaymentService(
                Mock.Of<ITransactionRepository>(),
                loggerMock.Object,
                Mock.Of<IStripeAdapter>(),
                Mock.Of<Braintree.IBraintreeGateway>(),
                Mock.Of<IGlobalSettings>(),
                Mock.Of<IFeatureService>(),
                taxServiceMock.Object,
                Mock.Of<IPricingClient>());

            var parameters = new 
            { 
                TaxInformation = new 
                { 
                    TaxId = "12345", 
                    Country = "US" 
                } 
            };
            var plan = new object(); // Replace with actual type
            var options = new object(); // Replace with actual type
            var gatewayCustomerId = string.Empty;

            // Act
            try
            {
                await service.FinalizeSubscriptionChangeAsync(
                    Mock.Of<ISubscriber>(),
                    new SubscriptionUpdate(parameters, plan, options, gatewayCustomerId),
                    true);
            }
            catch (BadRequestException)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogWarning(
                "Invalid tax ID '{TaxID}' for country '{Country}'.",
                parameters.TaxInformation.TaxId,
                parameters.TaxInformation.Country),
                Times.Once);
        }
    }
}
