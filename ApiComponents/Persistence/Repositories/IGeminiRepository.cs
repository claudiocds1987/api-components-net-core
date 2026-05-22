namespace ApiComponents.Persistence.Repositories
{
    public interface IGeminiRepository
    {
        Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
