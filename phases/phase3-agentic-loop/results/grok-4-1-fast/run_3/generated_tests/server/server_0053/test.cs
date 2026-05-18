#nullable disable

using Bit.Core.Billing.Caches;
using Bit.Core.Billing.Services;
using Bit.Core.Billing.Tax.Models;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Repositories;
using Bit.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Stripe;
using Xunit;

namespace Bit.Core.Billing.Services.Implementations.Tests;

public class SubscriberServiceTests
{
    private readonly Mock<IBraintreeGateway> _braintreeMock;
    private readonly Mock<IGlobalSettings> _globalSettingsMock;
    private readonly Mock<ILogger<SubscriberService>> _loggerMock;
    private readonly Mock<IOrganizationRepository> _orgRepoMock;
    private readonly Mock<IProviderRepository> _providerRepoMock;
    private readonly Mock<ISetupIntentCache> _setupIntentCacheMock;
    private readonly Mock<IStripeAdapter> _stripeAdapterMock;
    private readonly Mock<ITaxService> _taxServiceMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ISubscriber> _subscriberMock;

    public SubscriberServiceTests()
    {
        _braintreeMock = new Mock<IBraintreeGateway>();
        _globalSettingsMock = new Mock<IGlobalSettings>();
        _loggerMock = new Mock<ILogger<SubscriberService>>();
        _orgRepoMock = new Mock<IOrganizationRepository>();
        _providerRepoMock = new Mock<IProviderRepository>();
        _setupIntentCacheMock = new Mock<ISetupIntentCache>();
        _stripeAdapterMock = new Mock<IStripeAdapter>();
        _taxServiceMock = new Mock<ITaxService>();
        _userRepoMock = new Mock<IUserRepository>();
        _subscriberMock = new Mock<ISubscriber>();
        
        _subscriberMock.Setup(x => x.GatewayCustomerId).Returns("cus_test");
        _stripeAdapterMock.Setup(x => x.CustomerRetrieveAsync("cus_test", It.IsAny<CustomerGetOptions>()))
            .ReturnsAsync(new Customer());
    }

    [Fact]
    public async Task UpdateTaxInformationAsync_LogsWarningWhenTaxIdTypeCannotBeInferred()
    {
        // Arrange
        var taxInformation = new TaxInformation(
            Country: "ES",
            PostalCode: "28001",
            TaxId: "A12345678",
            TaxIdType: "", // Empty to trigger inference
            Line1: "Test Street",
            Line2: null,
            City: "Madrid",
            State: ""
        );

        _taxServiceMock.Setup(x => x.GetStripeTaxCode("ES", "A12345678")).Returns((string)null);

        // Setup logger to capture the exact LogWarning call
        _loggerMock.Setup(x => x.LogWarning(
            It.Is<string>(msg => msg == "Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'"),
            It.IsAny<object[]>(),
            It.IsAny<object[]>()));

        var service = new SubscriberService(
            _braintreeMock.Object,
            _globalSettingsMock.Object,
            _loggerMock.Object,
            _orgRepoMock.Object,
            _providerRepoMock.Object,
            _setupIntentCacheMock.Object,
            _stripeAdapterMock.Object,
            _taxServiceMock.Object,
            _userRepoMock.Object
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateTaxInformationAsync(_subscriberMock.Object, taxInformation));

        Assert.Equal("billingTaxIdTypeInferenceError", exception.Error);

        // Verify the specific warning was logged with correct parameters
        _loggerMock.Verify(
            x => x.LogWarning(
                "Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                It.Is<string>(country => country == "ES"),
                It.Is<string>(taxId => taxId == "A12345678")),
            Times.Once);
    }
}
