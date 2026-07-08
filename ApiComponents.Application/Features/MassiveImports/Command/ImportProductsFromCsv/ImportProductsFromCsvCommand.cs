using ApiComponents.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ApiComponents.Application.Features.MassiveImports.Command.ImportProductsFromCsv
{
    public record ImportProductsFromCsvCommand(IFormFile File) : IRequest<ImportResultDto>;
}
