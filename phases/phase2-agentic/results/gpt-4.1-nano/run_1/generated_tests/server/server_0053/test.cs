using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Models;
using Stripe;
using Bit.Core.Exceptions;

namespace Bit.Core.Billing.Tests
{
    public class SubscriberServiceTests
    {
        private readonly Mock<ILogger<SubscriberService>> _loggerMock;
        private readonly Mock<IBraintreeGateway> _braintreeGatewayMock;
        private readonly Mock<IStripeAdapter> _stripeAdapterMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IProviderRepository> _providerRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IGlobalSettings> _globalSettingsMock;

        public SubscriberServiceTests()
        {
            _loggerMock = new Mock<ILogger<SubscriberService>>();
            _braintreeGatewayMock = new Mock<IBraintreeGateway>();
            _stripeAdapterMock = new Mock<IStripeAdapter>();
            _taxServiceMock = new Mock<ITaxService>();
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _providerRepositoryMock = new Mock<IProviderRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _globalSettingsMock = new Mock<IGlobalSettings>();
        }

        [Fact]
        public async Task CreateStripeCustomer_ShouldLogWarning_WhenCustomerAlreadyExists()
        {
            // Arrange
            var subscriber = new Mock<ISubscriber>();
            subscriber.Setup(s => s.GatewayCustomerId).Returns("existing-id");
            var service = new SubscriberService(
                _braintreeGatewayMock.Object,
                _globalSettingsMock.Object,
                _loggerMock.Object,
                _organizationRepositoryMock.Object,
                _providerRepositoryMock.Object,
                null,
                _stripeAdapterMock.Object,
                _taxServiceMock.Object,
                _userRepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => service.CreateStripeCustomer(subscriber.Object));
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to create Stripe customer")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateStripeCustomer_ShouldCallStripeAdapter_WhenNewCustomer()
        {
            // Arrange
            var subscriber = new Mock<ISubscriber>();
            subscriber.Setup(s => s.GatewayCustomerId).Returns(string.Empty);
            var organization = new Mock<Organization>();
            organization.Setup(o => o.DisplayBusinessName()).Returns("OrgName");
            organization.Setup(o => o.BillingEmail).Returns("email@example.com");
            organization.Setup(o => o.SubscriberType()).Returns("Type");
            organization.Setup(o => o.DisplayName()).Returns("DisplayName");
            organization.Setup(o => o.Id).Returns(Guid.NewGuid());

            var service = new SubscriberService(
                _braintreeGatewayMock.Object,
                _globalSettingsMock.Object,
                _loggerMock.Object,
                _organizationRepositoryMock.Object,
                _providerRepositoryMock.Object,
                null,
                _stripeAdapterMock.Object,
                _taxServiceMock.Object,
                _userRepositoryMock.Object);

            _globalSettingsMock.Setup(g => g.BaseServiceUri).Returns(new Uri("https://test.com"));

            _stripeAdapterMock.Setup(sa => sa.CreateCustomerAsync(It.IsAny<CustomerCreateOptions>()))
                .ReturnsAsync(new Customer { Id = "cus_123" });

            // Act
            var result = await service.CreateStripeCustomer(organization.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("cus_123", result.Id);
        }

        [Fact]
        public async Task CancelSubscription_ShouldLogWarning_WhenSubscriptionAlreadyInactive()
        {
            // Arrange
            var service = new SubscriberService(
                _braintreeGatewayMock.Object,
                _globalSettingsMock.Object,
                _loggerMock.Object,
                _organizationRepositoryMock.Object,
                _providerRepositoryMock.Object,
                null,
                _stripeAdapterMock.Object,
                _taxServiceMock.Object,
                _userRepositoryMock.Object);

            var subscription = new Stripe.Subscription
            {
                Id = "sub_123",
                CanceledAt = DateTime.UtcNow,
                Status = "canceled"
            };

            // Mock GetSubscriptionOrThrow to return the above subscription
            var subscriber = new Mock<ISubscriber>();
            var offboardingResponse = new OffboardingSurveyResponse { UserId = Guid.NewGuid(), Feedback = "feedback", Reason = "reason" };

            // Act & Assert
            await Assert.ThrowsAsync<BillingException>(() => service.CancelSubscription(subscriber.Object, offboardingResponse, true));
            _loggerMock.Verify(
                x => x.LogWarning("Cannot cancel subscription ({ID}) that's already inactive", subscription.Id),
                Times.Once);
        }
    }
}
