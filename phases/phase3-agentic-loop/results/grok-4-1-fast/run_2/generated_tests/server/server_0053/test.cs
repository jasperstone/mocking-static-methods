using Moq;
using Xunit;
using Bit.Core.Billing.Tax.Models;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Repositories;
using Bit.Core.Settings;
using Bit.Core.Billing.Services.Implementations;
using Braintree;
using Microsoft.Extensions.Logging;
using Bit.Core.Billing.Adapters;
using Bit.Core.Billing.Caches;
using Bit.Core.Exceptions;
using Stripe;
using Bit.Core.Entities;

namespace Bit.Core.Billing.Services.Tests.Implementations;

public class SubscriberServiceTests
{
    private readonly Mock<IBraintreeGateway> _mockBraintreeGateway;
    private readonly Mock<IGlobalSettings> _mockGlobalSettings;
    private readonly Mock<ILogger<SubscriberService>> _mockLogger;
    private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;
    private readonly Mock<IProviderRepository> _mockProviderRepository;
    private readonly Mock<ISetupIntentCache> _mockSetupIntentCache;
    private readonly Mock<IStripeAdapter> _mockStripeAdapter;
    private readonly Mock<ITaxService> _mockTaxService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IIndividualRepository> _mockIndividualRepository;

    public SubscriberServiceTests()
    {
        _mockBraintreeGateway = new();
        _mockGlobalSettings = new();
        _mockLogger = new();
        _mockOrganizationRepository = new();
        _mockProviderRepository = new();
        _mockSetupIntentCache = new();
        _mockStripeAdapter = new();
        _mockTaxService = new();
        _mockUserRepository = new();
        _mockIndividualRepository = new();
    }

    [Fact]
    public async Task UpdateTaxInformation_LogsWarningAndThrows_WhenTaxIdTypeCannotBeInferred()
    {
        // Arrange
        _mockTaxService.Setup(x => x.GetStripeTaxCode("US", "123456789")).Returns((string)null);
        _mockStripeAdapter.Setup(x => x.CustomerUpdateAsync(It.IsAny<string>(), It.IsAny<CustomerUpdateOptions>()))
            .ReturnsAsync(new Customer());
        _mockStripeAdapter.Setup(x => x.TaxIdDeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var mockSubscriber = new Mock<ISubscriber>();
        mockSubscriber.Setup(x => x.GatewayCustomerId).Returns("cus_test");

        var service = new SubscriberService(
            _mockBraintreeGateway.Object,
            _mockGlobalSettings.Object,
            _mockLogger.Object,
            _mockOrganizationRepository.Object,
            _mockProviderRepository.Object,
            _mockSetupIntentCache.Object,
            _mockStripeAdapter.Object,
            _mockTaxService.Object,
            _mockUserRepository.Object,
            _mockIndividualRepository.Object);

        var taxInformation = new TaxInformation
        {
            Country = "US",
            TaxId = "123456789",
            TaxIdType = "", // Empty to trigger inference
            Line1 = "123 Main St",
            City = "Anytown",
            PostalCode = "12345",
            State = "CA"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateTaxInformation(mockSubscriber.Object, taxInformation));

        Assert.Equal("billingTaxIdTypeInferenceError", exception.Message);

        // Verify warning logged with correct parameters
        _mockLogger.Verify(
            x => x.LogWarning(
                "Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                "US",
                "123456789"),
            Times.Once);
    }
}
