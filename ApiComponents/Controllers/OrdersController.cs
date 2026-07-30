using ApiComponents.Application.DTOs;
using ApiComponents.Application.Features.Orders.Commands.CheckoutOrder;
using ApiComponents.Application.Features.Orders.Commands.ProcessPaymentReturn;
using ApiComponents.Application.Features.Orders.Commands.ConfirmPayment;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly ISender _sender;

        public OrdersController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CartDto cart, CancellationToken cancellationToken)
        {
            var preferenceId = await _sender.Send(new CheckoutOrderCommand(cart), cancellationToken);
            return Ok(new { id = preferenceId });
        }

        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentCommand command, CancellationToken cancellationToken)
        {
            var message = await _sender.Send(command, cancellationToken);
            return Ok(new { message });
        }

        [HttpGet("payment-return")]
        public async Task<IActionResult> PaymentReturn(
             [FromQuery] string status,
             [FromQuery] string preference_id,
             [FromQuery] string external_reference,
             CancellationToken cancellationToken)
        {
            var redirectUrl = await _sender.Send(
                new ProcessPaymentReturnCommand(status, preference_id, external_reference, Request.Host.Host), 
                cancellationToken);
                
            return Redirect(redirectUrl);
        }
    }
}
