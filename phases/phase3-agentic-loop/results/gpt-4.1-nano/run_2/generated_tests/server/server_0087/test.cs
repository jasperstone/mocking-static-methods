using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Core.Services;
using Bit.Core.Models.BitStripe;
using Bit.Core.Models.Business;
using System.Threading.Tasks;
using Stripe;
using System;

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
        public async Task FinalizeSubscriptionChangeAsync_Should_Log_Warning_When_TaxIdType_Is_Null()
        {
            // Arrange
            var subscriber = new Mock<ISubscriber>();
            subscriber.Setup(s => s.GatewaySubscriptionId).Returns("subId");
            var subscriptionUpdate = new Mock<SubscriptionUpdate>();
            var sub = new Subscription
            {
                Id = "subId",
                Status = SubscriptionStatuses.Active,
                Customer = new Customer
                {
                    Address = new Address { Country = "US" },
                    TaxExempt = StripeConstants.TaxExempt.None,
                    Discount = null,
                    TaxIds = null
                },
                CollectionMethod = "charge_automatically",
                Items = new SubscriptionItemCollection
                {
                    Data = new[] { new SubscriptionItem { Plan = new Plan { Interval = "month" } } }
                }
            };

            _stripeAdapterMock.Setup(s => s.SubscriptionGetAsync(It.IsAny<string>(), It.IsAny<SubscriptionGetOptions>()))
                .ReturnsAsync(sub);
            _taxServiceMock.Setup(t => t.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string, string) => null);
            _stripeAdapterMock.Setup(s => s.InvoiceGetAsync(It.IsAny<string>(), It.IsAny<InvoiceGetOptions>()))
                .ReturnsAsync(new Invoice { AmountDue = 100 });
            _stripeAdapterMock.Setup(s => s.SubscriptionUpdateAsync(It.IsAny<string>(), It.IsAny<SubscriptionUpdateOptions>()))
                .ReturnsAsync(new Subscription { Id = "subId" });
            _stripeAdapterMock.Setup(s => s.InvoiceFinalizeInvoiceAsync(It.IsAny<string>(), It.IsAny<InvoiceFinalizeOptions>()))
                .ReturnsAsync(new Invoice { Id = "invId" });
            _stripeAdapterMock.Setup(s => s.InvoiceSendInvoiceAsync(It.IsAny<string>(), It.IsAny<InvoiceSendOptions>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(async () =>
            {
                await _service.FinalizeSubscriptionChangeAsync(subscriber.Object, subscriptionUpdate.Object, true);
            });

            // Verify that LogWarning was called with expected message
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(msg => msg.Contains("Invalid tax ID")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
