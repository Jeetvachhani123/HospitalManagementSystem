using HospitalMS.BL.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace HospitalMS.BL.Services;

public class StripePaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly IBillingService _billingService;
    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger, IBillingService billingService)
    {
        _configuration = configuration;
        _logger = logger;
        _billingService = billingService;
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    // create checkout session
    public async Task<string> CreateCheckoutSessionAsync(int invoiceId, decimal amount, string currency, string successUrl, string cancelUrl)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(amount * 100),
                            Currency = currency,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Invoice #{invoiceId}",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "InvoiceId", invoiceId.ToString() }
                }
            };
            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe API error creating checkout session for invoice {InvoiceId}", invoiceId);
            throw;
        }
    }

    // handle stripe webhook
    public async Task<bool> HandleWebhookAsync(string json, string signature)
    {
        try
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session != null && session.Metadata.ContainsKey("InvoiceId"))
                {
                    if (int.TryParse(session.Metadata["InvoiceId"], out int invoiceId))
                    {
                        await _billingService.ProcessPaymentAsync(invoiceId, "Stripe");
                        _logger.LogInformation("Successfully processed Stripe payment for Invoice {InvoiceId}", invoiceId);
                        return true;
                    }
                }
            }
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook error");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Stripe webhook");
            return false;
        }
    }
}