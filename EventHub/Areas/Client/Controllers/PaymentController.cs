using EventHub.Core.Contracts;
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
        private readonly StripeOptions stripeOptions;

        public PaymentController(
            IPaymentService _paymentService,
            IEventService _eventService,
            ITicketService _ticketService,
            IOptions<StripeOptions> _stripeOptions)
        {
            paymentService = _paymentService;
            eventService = _eventService;
            ticketService = _ticketService;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutSeats(Guid eventId, List<Guid> seatIds)
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

            var checkoutUrl = await paymentService.CreateSeatCheckoutSessionAsync(new CreateSeatCheckoutRequest
            {
                EventId = eventId,
                UserId = userId,
                Currency = (reservation.Currency ?? "EUR").ToLowerInvariant(),
                EventName = ev.EventName,
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Lines = reservation.Lines.Select(l => new CheckoutSeatLine
                {
                    TicketId = l.TicketId,
                    SeatNumber = l.SeatNumber,
                    ZoneName = l.ZoneName,
                    UnitPrice = (decimal)l.Price
                }).ToList()
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