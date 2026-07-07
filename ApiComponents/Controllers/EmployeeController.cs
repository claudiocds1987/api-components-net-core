using ApiComponents.Application.DTOs;
using ApiComponents.Domain.Models;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // --- READ: GET Paged (Filtrado y Paginación) ---
        [HttpGet("paged")]
        public async Task<ActionResult<PaginatedList<Employee>>> GetEmployeesPaged(
            [FromQuery] EmployeeQueryParams queryParams)
        {
            var pagedList = await _employeeService.GetPagedEmployeesAsync(queryParams);
            return Ok(pagedList);
        }

        // --- READ: GET All (Sin Paginación) ---
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        // --- READ: GET by ID ---
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                return employee;
            }
            // CAMBIO AQUÍ: Nombre completo para evitar ambigüedad con GreenDonut (GraphQL)
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // --- UPDATE: PUT ---
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.id)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del empleado en el cuerpo.");
            }

            try
            {
                await _employeeService.UpdateEmployeeAsync(id, employee);
            }
            // CAMBIO AQUÍ: Nombre completo
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }

        // --- CREATE: POST (Individual) ---
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            try
            {
                await _employeeService.AddEmployeeAsync(employee);
                return CreatedAtAction("GetEmployee", new { id = employee.id }, employee);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // --- CREATE: POST (Batch/Lista) ---
        [HttpPost("batch")]
        public async Task<IActionResult> PostEmployeeList([FromBody] List<Employee> employees)
        {
            if (employees == null || !employees.Any())
            {
                return BadRequest("La lista de empleados no puede estar vacía.");
            }

            try
            {
                await _employeeService.AddEmployeeListAsync(employees);

                return Ok(new
                {
                    message = "Procesamiento por lotes completado con éxito.",
                    count = employees.Count
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Ocurrió un error al procesar la lista en el servidor.",
                    details = ex.Message
                });
            }
        }

        // --- DELETE: DELETE ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                await _employeeService.DeleteEmployeeAsync(id);
                return NoContent();
            }
            // CAMBIO AQUÍ: Nombre completo
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}