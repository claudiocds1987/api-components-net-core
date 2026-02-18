using ApiComponents.DTOs;

namespace ApiComponents.Services
{
    public interface IGeminiService
    {
        Task<string> GetSellerAnswerAsync(GeminiProductRequestDto request);
        Task<string> AnalyzeSearchAsync(string text);
        Task<GeminiChatResponseDto> QueryCatalogAsync(string userQuestion);
    }
}
