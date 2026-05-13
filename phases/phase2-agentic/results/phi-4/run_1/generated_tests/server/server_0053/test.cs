using System;
using System.Linq;
using System.Threading.Tasks;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Xunit;

namespace Bit.Core.Billing.Tests.Services.Implementations
{
    public class SubscriberServiceTests
    {
        [Fact]
        public async Task LogWarning_WhenTaxIdTypeCannotBeInferred()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SubscriberService>>();
            var mockTaxService = new Mock<ITaxService>();
            var mockStripeAdapter = new Mock<IStripeAdapter>();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockOrganizationRepository = new Mock<IOrganizationRepository>();
            var mockProviderRepository = new Mock<IProviderRepository>();
            var mockSetupIntentCache = new Mock<ISetupIntentCache>();
            var mockGlobalSettings = new Mock<IGlobalSettings>();
            var mockBraintreeGateway = new Mock<IBraintreeGateway>();

            var taxInformation = new TaxInformation
            {
                Country = "US",
                TaxId = "123456789",
                TaxIdType = null
            };

            var customer = new Customer
            {
                Id = "cus_123",
                TaxIds = new StripeList<TaxId>
                {
                    new TaxId { Id = "tid_123" }
                }
            };

            mockTaxService
                .Setup(t => t.GetStripeTaxCode(taxInformation.Country, taxInformation.TaxId))
                .Returns((string)null);

            var subscriberService = new SubscriberService(
                mockBraintreeGateway.Object,
                mockGlobalSettings.Object,
                mockLogger.Object,
                mockOrganizationRepository.Object,
                mockProviderRepository.Object,
                mockSetupIntentCache.Object,
                mockStripeAdapter.Object,
                mockTaxService.Object,
                mockUserRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                subscriberService.UpdateTaxInformationAsync(customer, taxInformation));

            mockLogger.Verify(
                logger => logger.LogWarning(
                    "Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                    taxInformation.Country,
                    taxInformation.TaxId),
                Times.Once);
        }
    }
}
