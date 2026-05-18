using Xunit;
using Moq;
using Bit.Core.Services;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Billing.Tax.Models;
using Bit.Core.Billing.Constants;
using Bit.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Threading.Tasks;

public class StripePaymentServiceTests
{
    private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
    private readonly Mock<IStripeAdapter> _stripeAdapterMock;
    private readonly Mock<ITaxService> _taxServiceMock;
    private readonly StripePaymentService _stripePaymentService;

    public StripePaymentServiceTests()
    {
        _loggerMock = new Mock<ILogger<StripePaymentService>>();
        _stripeAdapterMock = new Mock<IStripeAdapter>();
        _taxServiceMock = new Mock<ITaxService>();

        _stripePaymentService = new StripePaymentService(
            null,
            _loggerMock.Object,
            _stripeAdapterMock.Object,
            null,
            null,
            null,
            _taxServiceMock.Object,
            null);
    }

    [Fact]
    public async Task LogWarning_WhenTaxIdTypeIsNullAndInferenceFails()
    {
        // Arrange
        var taxInfo = new TaxInformation
        {
            BillingAddressCountry = "US",
            TaxIdNumber = "12345",
            TaxIdType = null
        };

        _taxServiceMock.Setup(x => x.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string)null);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _stripePaymentService.UpdateTaxInfoAsync(taxInfo));

        _loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.Once);
    }
}
