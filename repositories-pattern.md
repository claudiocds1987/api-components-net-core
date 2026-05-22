# Reference: Repository Pattern (Data Access Layer)

## Role & Responsibility
Encapsula por completo el mecanismo de acceso a datos (`AppDbContext`), aislando las consultas de EF Core y exponiendo métodos estrictamente asíncronos y cancelables.

## Strict Rules
- **DbContext Bound:** El `AppDbContext` solo vive dentro de la implementación del repositorio.
- **Full Async & Cancellation:** Todos los comandos y consultas que ejecuten I/O contra la BD deben llevar el sufijo `Async`, recibir y pasar el `CancellationToken`.
- **Correct Save Changes Signature:** El método encargado de confirmar la transacción de EF Core debe retornar un `Task<int>`, indicando el número de entidades afectadas en la base de datos.
- **Evaluated Return Types:** No expongas `IQueryable` hacia el servicio; ejecuta la consulta usando operadores asíncronos (`ToListAsync`, `FirstOrDefaultAsync`) antes de devolver la respuesta.

## Correct Implementation Example
```csharp
public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(Employee employee, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Employees.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Employees.AnyAsync(e => e.Email == email, cancellationToken);
    }

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken)
    {
        await _context.Employees.AddAsync(employee, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}