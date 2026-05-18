using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Tax.Services;
using Stripe;

public class TaxInformation
{
    public string Country { get; set; }
    public string TaxId { get; set; }
    public string TaxIdType { get; set; }
}

public class Customer
{
    public string Id { get; set; }
    public List<TaxId> TaxIds { get; set; }
}

public class TaxId
{
    public string Id { get; set; }
}

public class SubscriberServiceTests
{
    private readonly Mock<ILogger<SubscriberService>> _loggerMock;
    private readonly Mock<ITaxService> _taxServiceMock;
    private readonly Mock<IStripeAdapter> _stripeAdapterMock;
    private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IBraintreeGateway> _braintreeGatewayMock;
    private readonly Mock<IGlobalSettings> _globalSettingsMock;
    private readonly Mock<ISetupIntentCache> _setupIntentCacheMock;

    public SubscriberServiceTests()
    {
        _loggerMock = new Mock<ILogger<SubscriberService>>();
        _taxServiceMock = new Mock<ITaxService>();
        _stripeAdapterMock = new Mock<IStripeAdapter>();
        _organizationRepositoryMock = new Mock<IOrganizationRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _braintreeGatewayMock = new Mock<IBraintreeGateway>();
        _globalSettingsMock = new Mock<IGlobalSettings>();
        _setupIntentCacheMock = new Mock<ISetupIntentCache>();
    }

    [Fact]
    public async Task LogWarningIsCalled_WhenTaxIdTypeCannotBeInferred()
    {
        // Arrange
        var taxInformation = new TaxInformation
        {
            Country = "US",
            TaxId = "123456789",
            TaxIdType = null
        };

        var customer = new Customer
        {
            Id = "cus_123",
            TaxIds = null
        };

        _taxServiceMock
            .Setup(t => t.GetStripeTaxCode(taxInformation.Country, taxInformation.TaxId))
            .Returns((string)null);

        var service = new SubscriberService(
            _braintreeGatewayMock.Object,
            _globalSettingsMock.Object,
            _loggerMock.Object,
            _organizationRepositoryMock.Object,
            null, // providerRepository
            _setupIntentCacheMock.Object,
            _stripeAdapterMock.Object,
            _taxServiceMock.Object,
            _userRepositoryMock.Object);

        // Act
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateTaxInformation(customer, taxInformation));

        // Assert
        _loggerMock.Verify(
            logger => logger.LogWarning(
                It.Is<string>(s => s.Contains("Could not infer tax ID type in country 'US' with tax ID '123456789'.")),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
