using EventHub.Core.Contracts;
using EventHub.Core.Models.Currency;
using EventHub.Core.Models.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace EventHub.Areas.Client.Controllers
{
    public class PaymentController : BaseController
    {
        private readonly IPaymentService paymentService;
        private readonly IEventService eventService;
        private readonly ITicketService ticketService;
        private readonly ICurrencyExchangeRateService currencyExchangeRateService;
        private readonly StripeOptions stripeOptions;
        private readonly CurrencyOptions currencyOptions;

        public PaymentController(
            IPaymentService _paymentService,
            IEventService _eventService,
            ITicketService _ticketService,
            ICurrencyExchangeRateService _currencyExchangeRateService,
            IOptions<StripeOptions> _stripeOptions,
            IOptions<CurrencyOptions> _currencyOptions)
        {
            paymentService = _paymentService;
            eventService = _eventService;
            ticketService = _ticketService;
            currencyExchangeRateService = _currencyExchangeRateService;
            stripeOptions = _stripeOptions.Value;
            currencyOptions = _currencyOptions.Value;
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var model = await paymentService.GetPaymentHistoryAsync(userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Guid eventId, int quantity, decimal unitPrice, string? checkoutCurrency)
        {
            var ev = await eventService.GetPublishedEventByIdAsync(eventId);
            if (ev == null) return NotFound();

            var maxQuantity = Math.Min(10, ev.TotalTickets - ev.TicketsSold);
            if (quantity < 1 || quantity > maxQuantity)
            {
                TempData["Error"] = "Choose a valid ticket quantity.";
                return RedirectToAction("Buy", "Events", new { id = eventId });
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var successUrl = Url.Action("Success", "Payment", new { area = "Client" }, Request.Scheme)!;
            var cancelUrl = Url.Action("Cancel", "Payment", new { area = "Client", eventId }, Request.Scheme)!;

            try
            {
                var currency = NormalizeCheckoutCurrency(checkoutCurrency);
                var sourceUnitPrice = ev.PriceAmount
                    ?? (unitPrice > 0 ? unitPrice : ev.BasePrice);
                var checkoutUnitPrice = await ConvertCheckoutAmountAsync(
                    sourceUnitPrice > 0 ? sourceUnitPrice : 1.00m,
                    currency);

                var checkoutUrl = await paymentService.CreateCheckoutSessionAsync(new CreateCheckoutRequest
                {
                    EventId = eventId,
                    UserId = userId,
                    Quantity = quantity,
                    UnitPrice = checkoutUnitPrice,
                    Currency = currency.ToLowerInvariant(),
                    EventName = ev.EventName,
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl
                });

                return Redirect(checkoutUrl);
            }
            catch (Stripe.StripeException)
            {
                TempData["Error"] = "Unable to start Stripe checkout. Please try again.";
                return RedirectToAction("Buy", "Events", new { id = eventId });
            }
            catch (InvalidOperationException)
            {
                TempData["Error"] = "Unable to convert the selected currency. Please try EUR or another currency.";
                return RedirectToAction("Buy", "Events", new { id = eventId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutSeats(Guid eventId, List<Guid> seatIds, string? checkoutCurrency)
        {
            if (seatIds == null || seatIds.Count == 0)
            {
                TempData["Error"] = "Please pick at least one seat.";
                return RedirectToAction("Buy", "Events", new { id = eventId });
            }

            var ev = await eventService.GetPublishedEventByIdAsync(eventId);
            if (ev == null) return NotFound();

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var reservation = await ticketService.ReserveSeatsAsync(eventId, userId, seatIds);
            if (!reservation.Success)
            {
                TempData["Error"] = reservation.ErrorMessage ?? "Unable to reserve seats.";
                return RedirectToAction("Buy", "Events", new { id = eventId });
            }

            var successUrl = Url.Action("Success", "Payment", new { area = "Client" }, Request.Scheme)!;
            var cancelUrl = Url.Action("Cancel", "Payment", new { area = "Client", eventId }, Request.Scheme)!;

            try
            {
                var currency = NormalizeCheckoutCurrency(checkoutCurrency);
                var lines = new List<CheckoutSeatLine>();
                foreach (var line in reservation.Lines)
                {
                    lines.Add(new CheckoutSeatLine
                    {
                        TicketId = line.TicketId,
                        SeatNumber = line.SeatNumber,
                        ZoneName = line.ZoneName,
                        UnitPrice = await ConvertCheckoutAmountAsync((decimal)line.Price, currency)
                    });
                }

                var checkoutUrl = await paymentService.CreateSeatCheckoutSessionAsync(new CreateSeatCheckoutRequest
                {
                    EventId = eventId,
                    UserId = userId,
                    Currency = currency.ToLowerInvariant(),
                    EventName = ev.EventName,
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    Lines = lines
                });

                return Redirect(checkoutUrl);
            }
            catch (Stripe.StripeException)
            {
                TempData["Error"] = "Unable to start Stripe checkout. Please try again.";
                return RedirectToAction("Buy", "Events", new { id = eventId });
            }
            catch (InvalidOperationException)
            {
                TempData["Error"] = "Unable to convert the selected currency. Please try EUR or another currency.";
                return RedirectToAction("Buy", "Events", new { id = eventId });
            }
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(Guid eventId)
        {
            var ev = await eventService.GetPublishedEventByIdAsync(eventId);
            ViewBag.EventId = eventId;
            ViewBag.EventName = ev?.EventName ?? "the event";
            return View();
        }

        private string NormalizeCheckoutCurrency(string? currency)
        {
            var normalized = string.IsNullOrWhiteSpace(currency)
                ? "EUR"
                : currency.Trim().ToUpperInvariant();

            var supported = (currencyOptions.SupportedCurrencies == null || currencyOptions.SupportedCurrencies.Length == 0
                    ? ["EUR", "USD", "JPY", "GBP", "AUD", "CAD", "CHF", "CNY", "SEK", "NZD", "TRY"]
                    : currencyOptions.SupportedCurrencies)
                .Select(c => c.Trim().ToUpperInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return supported.Contains(normalized) ? normalized : "EUR";
        }

        private async Task<decimal> ConvertCheckoutAmountAsync(decimal eurAmount, string currency)
        {
            var normalized = NormalizeCheckoutCurrency(currency);

            if (normalized == "EUR")
            {
                return Math.Round(eurAmount, 2, MidpointRounding.AwayFromZero);
            }

            return await currencyExchangeRateService.ConvertAsync(eurAmount, "EUR", normalized);
        }
    }

    [AllowAnonymous]
    [ApiController]
    [Route("api/stripe/webhook")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IPaymentService paymentService;
        private readonly StripeOptions stripeOptions;

        public StripeWebhookController(
            IPaymentService _paymentService,
            IOptions<StripeOptions> _stripeOptions)
        {
            paymentService = _paymentService;
            stripeOptions = _stripeOptions.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var payload = await new StreamReader(Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();

            try
            {
                await paymentService.HandleWebhookAsync(payload, signature);
                return Ok();
            }
            catch (Stripe.StripeException)
            {
                return BadRequest("Invalid Stripe signature.");
            }
        }
    }
}
