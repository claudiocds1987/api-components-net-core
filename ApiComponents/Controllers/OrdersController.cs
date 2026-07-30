using ApiComponents.Application.DTOs;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using ApiComponents.Application.Features.Orders.Commands;
using ApiComponents.Application.Features.Orders.Queries;

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

        [HttpGet]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] int _page = 1,
            [FromQuery] int _limit = 25,
            [FromQuery] string _sort = "id",
            [FromQuery] string _order = "desc",
            [FromQuery(Name = "userEmail_like")] string? userEmail_like = null,
            [FromQuery] DateTime? createdAt_from = null,
            [FromQuery] DateTime? createdAt_to = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _sender.Send(new GetAllOrdersQuery(_page, _limit, _sort, _order, userEmail_like, createdAt_from, createdAt_to), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetOrderByIdQuery(id), cancellationToken);
            return Ok(result);
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetOrdersMetrics(CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetOrdersMetricsQuery(), cancellationToken);
            return Ok(result);
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
