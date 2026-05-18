using Bit.Core.Billing.Tax.Services;
using Bit.Core.Exceptions;
using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bit.Core.Tests.Services.Implementations;

public class StripePaymentServiceTests
{
    private readonly Mock<ILogger<StripePaymentService>> _mockLogger;
    private readonly Mock<ITaxService> _mockTaxService;
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly Mock<Braintree.IBraintreeGateway> _mockBraintreeGateway;
    private readonly Mock<IStripeAdapter> _mockStripeAdapter;
    private readonly Mock<IGlobalSettings> _mockGlobalSettings;
    private readonly Mock<IFeatureService> _mockFeatureService;
    private readonly Mock<IPricingClient> _mockPricingClient;

    public StripePaymentServiceTests()
    {
        _mockLogger = new Mock<ILogger<StripePaymentService>>();
        _mockTaxService = new Mock<ITaxService>();
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockBraintreeGateway = new Mock<Braintree.IBraintreeGateway>();
        _mockStripeAdapter = new Mock<IStripeAdapter>();
        _mockGlobalSettings = new Mock<IGlobalSettings>();
        _mockFeatureService = new Mock<IFeatureService>();
        _mockPricingClient = new Mock<IPricingClient>();
    }

    [Fact]
    public async Task FinalizeSubscriptionChangeAsync_LogsWarningAndThrows_WhenTaxIdTypeIsNull()
    {
        // Arrange
        var mockSubscriber = new Mock<ISubscriber>();
        mockSubscriber.Setup(s => s.GatewaySubscriptionId).Returns("sub_test");

        var mockCustomer = new Mock<Customer>();
        mockCustomer.Setup(c => c.Address).Returns(new AddressOptions { Country = "ES" });
        mockCustomer.Setup(c => c.Id).Returns("cus_test");

        var mockSubscription = new Mock<Subscription>();
        mockSubscription.Setup(s => s.Id).Returns("sub_test");
        mockSubscription.Setup(s => s.CustomerId).Returns("cus_test");
        mockSubscription.Setup(s => s.Customer).Returns(mockCustomer.Object);
        mockSubscription.Setup(s => s.Status).Returns("active");

        _mockStripeAdapter.Setup(s => s.SubscriptionGetAsync("sub_test", It.IsAny<SubscriptionGetOptions>()))
            .ReturnsAsync(mockSubscription.Object);

        _mockTaxService.Setup(x => x.GetStripeTaxCode("ES", "invalid_tax_id"))
            .Returns((string)null);

        var service = CreateSut();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.FinalizeSubscriptionChangeAsync(mockSubscriber.Object, It.IsAny<SubscriptionUpdate>(), false));

        Assert.Equal("billingTaxIdTypeInferenceError", exception.Message);

        // Verify LogWarning was called with correct parameters
        _mockLogger.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                "Invalid tax ID '{TaxID}' for country '{Country}'.",
                "invalid_tax_id",
                "ES"),
            Times.Once);
    }

    private StripePaymentService CreateSut() => new(
        _mockTransactionRepository.Object,
        _mockLogger.Object,
        _mockStripeAdapter.Object,
        _mockBraintreeGateway.Object,
        _mockGlobalSettings.Object,
        _mockFeatureService.Object,
        _mockTaxService.Object,
        _mockPricingClient.Object);
}
