using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Core.Services;
using Bit.Core.Models.BitStripe;
using Bit.Core.Billing.Models;
using System.Threading.Tasks;
using Stripe;
using System.Collections.Generic;
using System.Linq;

namespace Bit.Core.Tests.Services
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
        private readonly Mock<IStripeAdapter> _stripeAdapterMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly Mock<IPricingClient> _pricingClientMock;
        private readonly StripePaymentService _service;

        public StripePaymentServiceTests()
        {
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            _stripeAdapterMock = new Mock<IStripeAdapter>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _taxServiceMock = new Mock<ITaxService>();
            _pricingClientMock = new Mock<IPricingClient>();

            _service = new StripePaymentService(
                _transactionRepositoryMock.Object,
                _loggerMock.Object,
                _stripeAdapterMock.Object,
                null,
                null,
                null,
                _taxServiceMock.Object,
                _pricingClientMock.Object);
        }

        [Fact]
        public async Task LogWarning_Called_When_TaxIdType_IsNull()
        {
            // Arrange
            var taxInfo = new TaxInfo
            {
                BillingAddressCountry = "US",
                TaxIdNumber = "123456789",
                TaxIdType = null
            };

            var customer = new Customer
            {
                Id = "cus_123",
                TaxIds = new List<TaxId> { new TaxId { Id = "taxid_1" } }
            };

            var sub = new Subscription
            {
                Id = "sub_123",
                CustomerId = "cus_123",
                Customer = customer,
                CollectionMethod = "charge_automatically",
                Status = SubscriptionStatuses.Active,
                Items = new SubscriptionItemCollection
                {
                    Data = new List<SubscriptionItem>
                    {
                        new SubscriptionItem
                        {
                            Plan = new Plan { Interval = "month" }
                        }
                    }
                }
            };

            var subscriptionUpdate = new CompleteSubscriptionUpdate();

            _stripeAdapterMock.Setup(x => x.SubscriptionGetAsync(It.IsAny<string>(), It.IsAny<SubscriptionGetOptions>()))
                .ReturnsAsync(sub);

            _taxServiceMock.Setup(x => x.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((country, taxId) => null);

            // Act
            await Assert.ThrowsAsync<BadRequestException>(async () =>
            {
                await _service.FinalizeSubscriptionChangeAsync(
                    new DummySubscriber { GatewaySubscriptionId = "sub_123" },
                    subscriptionUpdate,
                    false);
            });

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Could not infer tax ID type")),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        // Dummy classes for testing
        private class DummySubscriber : ISubscriber
        {
            public string GatewaySubscriptionId { get; set; }
        }

        private class TaxInfo
        {
            public string BillingAddressCountry { get; set; }
            public string TaxIdNumber { get; set; }
            public string TaxIdType { get; set; }
        }
    }
}
