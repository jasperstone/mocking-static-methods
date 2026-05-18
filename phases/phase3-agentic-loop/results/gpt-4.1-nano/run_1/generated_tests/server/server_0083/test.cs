using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Bit.Core.Services;
using Bit.Core.Models.BitStripe;
using Stripe;
using System;
using System.Collections.Generic;

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
        public async Task FinalizeSubscriptionChangeAsync_ShouldLogWarning_WhenTaxIdTypeInferenceFails()
        {
            // Arrange
            var customer = new Customer
            {
                Id = "cus_123",
                TaxIds = new List<TaxId> { new TaxId { Id = "taxid_1" } }
            };

            var sub = new Subscription
            {
                Id = "sub_123",
                Status = SubscriptionStatuses.Active,
                CustomerId = customer.Id,
                Customer = customer,
                CollectionMethod = "charge_automatically",
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

            var subscriberMock = new Mock<ISubscriber>();
            subscriberMock.Setup(s => s.GatewaySubscriptionId).Returns("sub_123");

            _stripeAdapterMock.Setup(s => s.SubscriptionGetAsync(It.IsAny<string>(), It.IsAny<SubscriptionGetOptions>()))
                .ReturnsAsync(sub);

            _taxServiceMock.Setup(t => t.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string country, string taxId) => null);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(async () =>
                await _service.FinalizeSubscriptionChangeAsync(subscriberMock.Object, new CompleteSubscriptionUpdate(), true));

            // Verify that LogWarning was called with the expected message
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(msg => msg.Contains("Could not infer tax ID type")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
