using Xunit;
using Moq;
using Bit.Core.Services;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Billing.Tax.Requests;
using Bit.Core.Billing.Tax.Responses;
using Bit.Core.Exceptions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Bit.Core.Billing.Models;

public class StripePaymentServiceTests
{
    private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
    private readonly Mock<ITaxService> _taxServiceMock;
    private readonly StripePaymentService _stripePaymentService;

    public StripePaymentServiceTests()
    {
        _loggerMock = new Mock<ILogger<StripePaymentService>>();
        _taxServiceMock = new Mock<ITaxService>();
        _stripePaymentService = new StripePaymentService(
            Mock.Of<ITransactionRepository>(),
            _loggerMock.Object,
            Mock.Of<IStripeAdapter>(),
            Mock.Of<Braintree.IBraintreeGateway>(),
            Mock.Of<IGlobalSettings>(),
            Mock.Of<IFeatureService>(),
            _taxServiceMock.Object,
            Mock.Of<IPricingClient>()
        );
    }

    [Fact]
    public async Task LogWarning_When_TaxIdIsInvalid()
    {
        // Arrange
        var parameters = new PaymentParameters
        {
            TaxInformation = new TaxInformation
            {
                TaxId = "InvalidTaxId",
                Country = "US"
            }
        };
        var options = new InvoiceCreateOptions
        {
            CustomerDetails = new InvoiceCustomerDetailsOptions
            {
                Address = new AddressOptions
                {
                    Country = "US"
                }
            }
        };

        _taxServiceMock.Setup(x => x.GetStripeTaxCode("US", "InvalidTaxId")).Returns((string)null);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _stripePaymentService.CreateInvoiceAsync(parameters, options, "gatewayCustomerId"));
        _loggerMock.Verify(
            x => x.LogWarning(
                "Invalid tax ID '{TaxID}' for country '{Country}'.",
                "InvalidTaxId",
                "US"),
            Times.Once);
    }
}
