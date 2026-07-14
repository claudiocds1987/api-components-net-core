using ApiComponents.Application.DTOs;
using ApiComponents.Domain.Models;
using ApiComponents.Application.Features.Employees.Queries.GetPagedEmployees;
using ApiComponents.Application.Features.Employees.Queries.GetAllEmployees;
using ApiComponents.Application.Features.Employees.Queries.GetEmployeeById;
using ApiComponents.Application.Features.Employees.Commands.AddEmployee;
using ApiComponents.Application.Features.Employees.Commands.UpdateEmployee;
using ApiComponents.Application.Features.Employees.Commands.DeleteEmployee;
using ApiComponents.Application.Features.Employees.Commands.AddEmployeeList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(ISender sender) : ControllerBase
    {
        // --- READ: GET Paged (Filtrado y Paginación) ---
        [HttpGet("paged")]
        public async Task<ActionResult<PaginatedList<Employee>>> GetEmployeesPaged(
            [FromQuery] EmployeeQueryParams queryParams)
        {
            var pagedList = await sender.Send(new GetPagedEmployeesQuery(queryParams));
            return Ok(pagedList);
        }

        // --- READ: GET All (Sin Paginación) ---
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var employees = await sender.Send(new GetAllEmployeesQuery());
            return Ok(employees);
        }

        // --- READ: GET by ID ---
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await sender.Send(new GetEmployeeByIdQuery(id));
            return Ok(employee);
        }

        // --- UPDATE: PUT ---
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.id)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del empleado en el cuerpo.");
            }

            await sender.Send(new UpdateEmployeeCommand(id, employee));
            return NoContent();
        }

        // --- CREATE: POST (Individual) ---
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            await sender.Send(new AddEmployeeCommand(employee));
            return CreatedAtAction("GetEmployee", new { id = employee.id }, employee);
        }

        // --- CREATE: POST (Batch/Lista) ---
        [HttpPost("batch")]
        public async Task<IActionResult> PostEmployeeList([FromBody] List<Employee> employees)
        {
            if (employees == null || !employees.Any())
            {
                return BadRequest("La lista de empleados no puede estar vacía.");
            }

            await sender.Send(new AddEmployeeListCommand(employees));
            
            return Ok(new
            {
                message = "Procesamiento por lotes completado con éxito.",
                count = employees.Count
            });
        }

        // --- DELETE: DELETE ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            await sender.Send(new DeleteEmployeeCommand(id));
            return NoContent();
        }
    }
}