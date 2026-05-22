using ApiComponents.DTOs;

namespace ApiComponents.Services
{
    public interface IGeminiService
    {
        Task<string> GetSellerAnswerAsync(GeminiProductRequestDto request, CancellationToken cancellationToken = default);
        Task<string> AnalyzeSearchAsync(string text, CancellationToken cancellationToken = default);
        Task<GeminiChatResponseDto> QueryCatalogAsync(string userQuestion, CancellationToken cancellationToken = default);
    }
}
