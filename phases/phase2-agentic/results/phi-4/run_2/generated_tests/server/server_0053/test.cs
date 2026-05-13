using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Bit.Core.Billing.Services.Implementations;
using Stripe;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Exceptions;

namespace Bit.Core.Billing.Tests.Services.Implementations
{
    public class SubscriberServiceTests
    {
        private readonly Mock<ILogger<SubscriberService>> _loggerMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly Mock<IStripeAdapter> _stripeAdapterMock;
        private readonly SubscriberService _subscriberService;

        public SubscriberServiceTests()
        {
            _loggerMock = new Mock<ILogger<SubscriberService>>();
            _taxServiceMock = new Mock<ITaxService>();
            _stripeAdapterMock = new Mock<IStripeAdapter>();

            // Initialize other dependencies as needed
            // For simplicity, we're not initializing all dependencies here
            _subscriberService = new SubscriberService(
                null, // braintreeGateway
                null, // globalSettings
                _loggerMock.Object,
                null, // organizationRepository
                null, // providerRepository
                null, // setupIntentCache
                _stripeAdapterMock.Object,
                _taxServiceMock.Object,
                null // userRepository
            );
        }

        [Fact]
        public async Task LogWarning_WhenTaxIdTypeCannotBeInferred()
        {
            // Arrange
            var taxInformation = new TaxInformation
            {
                Country = "Country",
                TaxId = "TaxID",
                TaxIdType = null
            };

            var customer = new Customer
            {
                Id = "customer_id",
                TaxIds = null
            };

            _taxServiceMock
                .Setup(taxService => taxService.GetStripeTaxCode(taxInformation.Country, taxInformation.TaxId))
                .Returns((string)null);

            // Act
            await _subscriberService.UpdateTaxInformation(customer, taxInformation);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(message => message.Contains("Could not infer tax ID type in country 'Country' with tax ID 'TaxID'.")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
