# Reference: Testing Fundamentals (Unit & Integration Testing)

## Role & Responsibility
Establece las directrices para probar de forma aislada la lógica de negocio (Tests Unitarios) y el comportamiento real de los mapeos y consultas (Tests de Persistencia).

## Strict Rules
- **Mocks & Cancellations:** Al mockear los repositorios en pruebas de servicios, utiliza `It.IsAny<CancellationToken>()` para ignorar la verificación estricta de la instancia del token de cancelación, enfocando el test en los datos comerciales.
- **AAA Pattern Structure:** Separa visualmente el código del test con comentarios claros: `// Arrange`, `// Act`, `// Assert`.
- **Advanced Repository Testing:** Para verificar repositorios reales, **NO** utilices el proveedor genérico `UseInMemoryDatabase`. Utiliza **SQLite en modo memoria** (`SqliteConnection("DataSource=:memory:")`), ya que se comporta como una base de datos relacional real y valida restricciones de integridad y tipos de datos.

## Correct Implementation Example (Service Unit Test)
```csharp
public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _repositoryMock;
    private readonly Mock<ILogger<EmployeeService>> _loggerMock;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _repositoryMock = new Mock<IEmployeeRepository>();
        _loggerMock = new Mock<ILogger<EmployeeService>>();
        _service = new EmployeeService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldThrowException_WhenEmployeeIsUnderage()
    {
        // Arrange
        var requestDto = new CreateEmployeeDto { Name = "John Doe", Email = "john@test.com", Age = 17 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => 
            _service.CreateEmployeeAsync(requestDto, CancellationToken.None));

        Assert.Equal("El empleado debe ser mayor de 18 años.", exception.Message);
        
        // Verifica que nunca se persistió
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}