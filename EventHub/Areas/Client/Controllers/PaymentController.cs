using EventHub.Core.Contracts;
using EventHub.Core.Models.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EventHub.Areas.Client.Controllers
{
    public class PaymentController : BaseController
    {
        private readonly IPaymentService paymentService;
        private readonly IEventService eventService;
        private readonly StripeOptions stripeOptions;

        public PaymentController(
            IPaymentService _paymentService,
            IEventService _eventService,
            IOptions<StripeOptions> _stripeOptions)
        {
            paymentService = _paymentService;
            eventService = _eventService;
            stripeOptions = _stripeOptions.Value;
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
        public async Task<IActionResult> Checkout(Guid eventId, int quantity, decimal unitPrice)
        {
            var ev = await eventService.GetPublishedEventByIdAsync(eventId);
            if (ev == null) return NotFound();

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var successUrl = Url.Action("Success", "Payment", new { area = "Client" }, Request.Scheme)!;
            var cancelUrl = Url.Action("Cancel", "Payment", new { area = "Client", eventId }, Request.Scheme)!;

            var checkoutUrl = await paymentService.CreateCheckoutSessionAsync(new CreateCheckoutRequest
            {
                EventId = eventId,
                UserId = userId,
                Quantity = quantity,
                UnitPrice = unitPrice > 0 ? unitPrice : 1.00m, // minimum 1.00 for Stripe
                Currency = "eur",
                EventName = ev.EventName,
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            });

            return Redirect(checkoutUrl);
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