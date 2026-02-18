using ApiComponents.DTOs;

namespace ApiComponents.Services
{
    public interface IGeminiService
    {
        Task<string> GetVendedorAnswerAsync(GeminiProductRequestDto request);
        Task<string> AnalyzeSearchAsync(string text);
        Task<GeminiChatResponseDto> ConsultarConCatalogoAsync(string preguntaUsuario);
    }
}
