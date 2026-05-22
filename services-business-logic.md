# Reference: Services (Business & Application Logic Layer)

## Role & Responsibility
La capa de servicios procesa toda la lógica de negocio y validaciones funcionales. Coordina la interacción con la persistencia y registra eventos clave de forma estructurada.

## Strict Rules
- **Validation Domain:** Todas las validaciones comerciales (ej. verificar la edad en `EmployeeService.cs`) se resuelven aquí antes de tocar los repositorios.
- **Asynchronous & Cancellable:** Todos los métodos deben aceptar y propagar el `CancellationToken cancellationToken` hacia la capa de repositorios.
- **Structured Logging:** Se debe inyectar `ILogger<T>` para registrar el flujo de la aplicación. 
  - **REGLA CRÍTICA:** Utilizar siempre **Mensajes Estructurados con Parámetros** (ej: `_logger.LogInformation("Procesando {Email}", email)`). 
  - **PROHIBIDO:** Usar interpolación de strings (`$"Procesando {email}"`) dentro de los métodos de log, ya que destruye la indexación de logs en producción.

## Correct Implementation Example
```csharp
public interface IEmployeeService
{
    Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken cancellationToken);
}

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando creación de empleado con email: {Email}", dto.Email);

        // 1. Validación de Regla de Negocio en C# puro
        if (dto.Age < 18)
        {
            _logger.LogWarning("Intento de registro fallido: El empleado {Email} es menor de edad.", dto.Email);
            throw new BusinessException("El empleado debe ser mayor de 18 años.");
        }

        // 2. Validación asíncrona y cancelable contra persistencia
        var emailExists = await _employeeRepository.ExistsByEmailAsync(dto.Email, cancellationToken);
        if (emailExists)
        {
            _logger.LogWarning("Intento de registro fallido: El email {Email} ya existe.", dto.Email);
            throw new BusinessException("El correo electrónico ya se encuentra registrado.");
        }

        var employee = new Employee 
        { 
            Name = dto.Name, 
            Email = dto.Email, 
            Age = dto.Age 
        };
        
        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Empleado creado exitosamente con ID: {EmployeeId}", employee.Id);

        return new EmployeeResponseDto { Id = employee.Id, Name = employee.Name };
    }
}