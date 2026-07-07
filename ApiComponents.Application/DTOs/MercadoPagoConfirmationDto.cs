namespace ApiComponents.Application.DTOs
{
    public class MercadoPagoConfirmationDto
    {
        public string PreferenceId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
