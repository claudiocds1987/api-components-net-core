using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Services; // Usamos el namespace del servicio
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiComponents.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        //  Inyección de la Interfaz de Servicio 🌟
        private readonly IEmployeeService _employeeService;

        // Constructor con inyección de dependencia del servicio
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
                return employee; // ASP.NET Core automáticamente devuelve 200 OK
            }
            catch (KeyNotFoundException)
            {
                // El servicio lanza KeyNotFoundException si el empleado no existe
                return NotFound();
            }
            // NOTA: Si el servicio lanza una ArgumentException (ej. ID inválido),
            // se podría atrapar aquí para devolver un BadRequest (400).
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
                // Delega la lógica de actualización al servicio
                await _employeeService.UpdateEmployeeAsync(id, employee);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(); // El servicio no encontró el empleado
            }
            // En una implementación real, aquí se podrían capturar excepciones de validación
            // lanzadas por el servicio para devolver un BadRequest (400).

            return NoContent(); // 204 No Content, estándar para PUT exitoso
        }

        // --- CREATE: POST (Individual) ---
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            try
            {
                // Delega la lógica de creación (incluyendo validaciones de negocio) al servicio
                await _employeeService.AddEmployeeAsync(employee);

                // Devuelve 201 CreatedAtAction
                return CreatedAtAction("GetEmployee", new { id = employee.id }, employee);
            }
            catch (ApplicationException ex)
            {
                // Capturar excepciones de validación de negocio (ej. "empleado menor de 18")
                return BadRequest(ex.Message);
            }
        }

        // --- CREATE: POST (Batch/Lista) ---
        [HttpPost("batch")]
        public async Task<ActionResult> PostEmployeeList(List<Employee> employees)
        {
            try
            {
                // Delega la lógica de inserción masiva al servicio
                await _employeeService.AddEmployeeListAsync(employees);

                return Ok(employees.Count + " empleados agregados exitosamente.");
            }
            catch (ApplicationException ex)
            {
                // Capturar excepciones de validación de negocio (ej. "lote demasiado grande")
                return BadRequest(ex.Message);
            }
        }

        // --- DELETE: DELETE ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                // Delega la lógica de eliminación al servicio
                await _employeeService.DeleteEmployeeAsync(id);

                return NoContent(); // 204 No Content, estándar para DELETE exitoso
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}