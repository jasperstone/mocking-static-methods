using System.Threading.Tasks;
using Bit.Core.Exceptions;
using Bit.Core.Models.Business;
using Bit.Core.Services;
using Bit.Core.Services.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Xunit;

namespace Bit.Core.Tests.Services.Implementations
{
    public class StripePaymentServiceTests
    {
        [Fact]
        public async Task FinalizeSubscriptionChangeAsync_InvalidTaxId_LogsWarningAndThrows()
        {
            // Arrange
            var transactionRepoMock = new Mock<ITransactionRepository>();
            var loggerMock = new Mock<ILogger<StripePaymentService>>();
            var stripeAdapterMock = new Mock<IStripeAdapter>();
            var braintreeGatewayMock = new Mock<Braintree.IBraintreeGateway>();
            var globalSettingsMock = new Mock<IGlobalSettings>();
            var featureServiceMock = new Mock<IFeatureService>();
            var taxServiceMock = new Mock<ITaxService>();
            var pricingClientMock = new Mock<IPricingClient>();

            var service = new StripePaymentService(
                transactionRepoMock.Object,
                loggerMock.Object,
                stripeAdapterMock.Object,
                braintreeGatewayMock.Object,
                globalSettingsMock.Object,
                featureServiceMock.Object,
                taxServiceMock.Object,
                pricingClientMock.Object);

            var subscriberMock = new Mock<ISubscriber>();
            subscriberMock.SetupGet(s => s.GatewaySubscriptionId).Returns("sub_123");

            var subscriptionUpdateMock = new Mock<SubscriptionUpdate>();
            subscriptionUpdateMock.Setup(su => su.UpdateNeeded(It.IsAny<Subscription>())).Returns(true);
            subscriptionUpdateMock.Setup(su => su.UpgradeItemsOptions(It.IsAny<Subscription>())).Returns(new System.Collections.Generic.List<SubscriptionItemOptions>());

            // Setup subscription to be returned by StripeAdapter
            var subscription = new Subscription
            {
                Id = "sub_123",
                Status = SubscriptionStatuses.Active,
                CustomerId = "cus_123",
                Customer = new Customer
                {
                    Id = "cus_123",
                    Address = new Address { Country = "FR" },
                    TaxExempt = StripeConstants.TaxExempt.None
                },
                Items = new StripeList<SubscriptionItem>
                {
                    Data = new System.Collections.Generic.List<SubscriptionItem>
                    {
                        new SubscriptionItem
                        {
                            Plan = new Plan { Interval = "month" }
                        }
                    }
                },
                CollectionMethod = "send_invoice",
                DaysUntilDue = 1
            };

            stripeAdapterMock.Setup(sa => sa.SubscriptionGetAsync("sub_123", It.IsAny<SubscriptionGetOptions>()))
                .ReturnsAsync(subscription);

            // Setup tax service to return null taxIdType to trigger the warning and exception
            taxServiceMock.Setup(ts => ts.GetStripeTaxCode("FR", "invalidTaxId")).Returns((string)null);

            // Setup parameters with invalid tax id
            var parameters = new
            {
                TaxInformation = new
                {
                    TaxId = "invalidTaxId",
                    Country = "FR"
                }
            };

            // We need to call the private method FinalizeSubscriptionChangeAsync indirectly.
            // The code snippet shows the warning is logged when taxIdType is null.
            // The taxIdType is retrieved from _taxService.GetStripeTaxCode with country and taxId from parameters.
            // So we simulate a call that triggers this condition.
            // The method that uses parameters.TaxInformation.TaxId is not shown fully, so we simulate by calling a public method that calls FinalizeSubscriptionChangeAsync.
            // We will create a minimal ISubscriber implementation with GatewaySubscriptionId and a SubscriptionUpdate that triggers UpdateNeeded true.

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(async () =>
            {
                // We simulate the call by invoking FinalizeSubscriptionChangeAsync via reflection because it's private.
                var method = typeof(StripePaymentService).GetMethod("FinalizeSubscriptionChangeAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                await (Task<string>)method.Invoke(service, new object[] { subscriberMock.Object, subscriptionUpdateMock.Object, false });
            });

            Assert.Equal("billingTaxIdTypeInferenceError", ex.Message);

            // Verify that LogWarning was called with expected message and parameters
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid tax ID")),
                    It.IsAny<System.Exception>(),
                    It.IsAny<System.Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }
}
