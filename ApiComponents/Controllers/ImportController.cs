using ApiComponents.Application.Features.MassiveImports.Command;
using MediatR; // Necesario para ISender
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/massiveImport")]
public class ImportController(ISender sender) : ControllerBase // Inyectamos ISender
{
    [HttpPost("products")]
    public async Task<IActionResult> ImportProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Seleccioná un archivo CSV válido." });

        // Ya no conocemos al servicio, solo enviamos el comando al mediador
        var command = new ImportProductsFromCsvCommand(file);
        var result = await sender.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}