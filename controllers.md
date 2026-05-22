# Reference: Controllers (Presentation Layer)

## Role & Responsibility
Los controladores (`Controllers`) pertenecen exclusivamente a la Capa de Presentación. Su única función es recibir solicitudes HTTP, delegar el procesamiento a la capa de negocio pasando el token de cancelación y retornar respuestas HTTP estandarizadas.

## Strict Rules
- **No Business Logic & No Data Access:** Prohibido meter validaciones comerciales o inyectar el `AppDbContext` aquí.
- **CancellationToken Propagation:** Todos los métodos de acción deben recibir un `CancellationToken cancellationToken` desde los parámetros de ASP.NET Core y propagarlo obligatoriamente a los métodos del servicio. Esto permite abortar operaciones costosas si el cliente cancela la petición.
- **Clean Contracts:** No manejes bloques `try-catch` masivos aquí. Las excepciones de negocio (`BusinessException`) se capturan y formatean automáticamente a nivel global a través de la infraestructura del framework (ej. `IExceptionHandler` regresando `ProblemDetails`).

## Correct Implementation Example
```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeResponseDto>> Create(
        [FromBody] CreateEmployeeDto request, 
        CancellationToken cancellationToken)
    {
        var result = await _employeeService.CreateEmployeeAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeResponseDto>> GetById(
        int id, 
        CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id, cancellationToken);
        if (employee == null) return NotFound();
        
        return Ok(employee);
    }
}