namespace ApiComponents.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> ProcessImage(string imageData, string scheme, string host, CancellationToken cancellationToken = default);
    }
}
