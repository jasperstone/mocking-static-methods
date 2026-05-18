using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Bit.Core.Services;
using Bit.Core.Models.BitStripe;
using Bit.Core.Models.Business;
using Bit.Core.Exceptions;
using Stripe;

namespace Bit.Core.Tests.Services
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
        private readonly Mock<IStripeAdapter> _stripeAdapterMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly Mock<IPricingClient> _pricingClientMock;

        public StripePaymentServiceTests()
        {
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            _stripeAdapterMock = new Mock<IStripeAdapter>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _taxServiceMock = new Mock<ITaxService>();
            _pricingClientMock = new Mock<IPricingClient>();
        }

        [Fact]
        public async Task FinalizeSubscriptionChangeAsync_ShouldLogWarningAndThrow_WhenTaxIdTypeIsNull()
        {
            // Arrange
            var service = new StripePaymentService(
                _transactionRepositoryMock.Object,
                _loggerMock.Object,
                _stripeAdapterMock.Object,
                null,
                null,
                null,
                _taxServiceMock.Object,
                _pricingClientMock.Object);

            var subscriber = new Mock<ISubscriber>();
            subscriber.Setup(s => s.GatewaySubscriptionId).Returns("subId");

            var sub = new Subscription
            {
                Id = "subId",
                Customer = new Customer
                {
                    Address = new Address { Country = "US" },
                    TaxExempt = StripeConstants.TaxExempt.Reverse,
                    Id = "custId"
                },
                CollectionMethod = "charge_automatically",
                Status = SubscriptionStatuses.Active,
                Items = new SubscriptionItemCollection
                {
                    Data = new[] { new SubscriptionItem { Plan = new Plan { Interval = "month" } } }
                }
            };

            _stripeAdapterMock.Setup(s => s.SubscriptionGetAsync(It.IsAny<string>(), It.IsAny<SubscriptionGetOptions>()))
                .ReturnsAsync(sub);

            _taxServiceMock.Setup(t => t.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string country, string taxId) => null);

            // Act
            await Assert.ThrowsAsync<BadRequestException>(async () =>
                await service.FinalizeSubscriptionChangeAsync(subscriber.Object, new Mock<SubscriptionUpdate>().Object, true));

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.", It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}
