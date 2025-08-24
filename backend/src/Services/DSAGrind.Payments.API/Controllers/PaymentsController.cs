using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DSAGrind.Payments.API.Services;
using DSAGrind.Models.DTOs;

namespace DSAGrind.Payments.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService, 
        ISubscriptionService subscriptionService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    [HttpPost("create-intent")]
    public async Task<ActionResult<PaymentIntentDto>> CreatePaymentIntent([FromBody] CreatePaymentIntentRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var createRequest = new CreatePaymentRequestDto
            {
                Amount = request.Amount,
                Currency = request.Currency,
                Description = request.Description,
                Metadata = request.Metadata
            };

            var paymentIntent = await _paymentService.CreatePaymentIntentAsync(createRequest, userId);
            return Ok(paymentIntent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent");
            return StatusCode(500, new { message = "An error occurred while creating payment intent" });
        }
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<PaymentResultDto>> ConfirmPayment([FromBody] ConfirmPaymentRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Get the payment to verify it exists and belongs to the user
            var payment = await _paymentService.GetPaymentAsync(request.PaymentIntentId, userId);
            if (payment == null)
            {
                return NotFound(new { message = "Payment not found" });
            }

            // Create a payment result based on the payment status
            var result = new PaymentResultDto
            {
                PaymentId = payment.Id,
                Status = payment.Status,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Success = payment.Status == "succeeded",
                ProcessedAt = DateTime.UtcNow
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment");
            return StatusCode(500, new { message = "An error occurred while confirming payment" });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<PaymentDto>>> GetPaymentHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var payments = await _paymentService.GetUserPaymentsAsync(userId, page, pageSize);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history");
            return StatusCode(500, new { message = "An error occurred while getting payment history" });
        }
    }

    [HttpGet("{paymentId}")]
    public async Task<ActionResult<PaymentDto>> GetPayment(string paymentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var payment = await _paymentService.GetPaymentAsync(paymentId, userId);
            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment {PaymentId}", paymentId);
            return StatusCode(500, new { message = "An error occurred while getting payment" });
        }
    }

    [HttpPost("subscriptions/create")]
    public async Task<ActionResult<SubscriptionDto>> CreateSubscription([FromBody] CreateSubscriptionRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var subscription = await _subscriptionService.CreateSubscriptionAsync(request, userId);
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription");
            return StatusCode(500, new { message = "An error occurred while creating subscription" });
        }
    }

    [HttpPost("subscriptions/{subscriptionId}/cancel")]
    public async Task<IActionResult> CancelSubscription(string subscriptionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _subscriptionService.CancelSubscriptionAsync(subscriptionId, userId);
            if (!success)
            {
                return BadRequest(new { message = "Failed to cancel subscription" });
            }

            return Ok(new { message = "Subscription cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, new { message = "An error occurred while cancelling subscription" });
        }
    }

    [HttpGet("subscriptions/current")]
    public async Task<ActionResult<SubscriptionDto>> GetCurrentSubscription()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var subscription = await _subscriptionService.GetUserSubscriptionAsync(userId);
            if (subscription == null)
            {
                return NotFound();
            }

            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current subscription");
            return StatusCode(500, new { message = "An error occurred while getting subscription" });
        }
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook()
    {
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? "";
            
            var success = await _paymentService.ProcessWebhookAsync(json, signature);
            
            if (!success)
            {
                return BadRequest();
            }
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling webhook");
            return StatusCode(500);
        }
    }

    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<ActionResult<List<SubscriptionPlanDto>>> GetSubscriptionPlans()
    {
        try
        {
            var plans = await _subscriptionService.GetSubscriptionPlansAsync();
            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plans");
            return StatusCode(500, new { message = "An error occurred while getting plans" });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}