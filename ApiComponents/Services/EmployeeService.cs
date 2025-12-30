using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Persistence.Repositories; // Para inyectar el repositorio
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiComponents.Services
{
    public class EmployeeService : IEmployeeService
    {
        // 1. Inyectamos la Interfaz del Repositorio
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        // ----------------------------------------------------------------------
        // OPERACIÓN AVANZADA: PAGINACIÓN Y FILTRADO
        // ----------------------------------------------------------------------

        public async Task<PaginatedList<Employee>> GetPagedEmployeesAsync(EmployeeQueryParams queryParams)
        {
            // **Aquí iría Lógica de Negocio si la hubiera antes de consultar:**
            // Ejemplo: Validar si el usuario tiene permisos específicos para ciertas columnas.

            // 1. Delega la consulta al Repositorio.
            var pagedList = await _employeeRepository.GetPagedEmployeesAsync(queryParams);

            // 2. **Aquí iría el Mapeo a DTOs de Respuesta si usaras DTOs diferentes
            //    al modelo de dominio (Employee)**. Por simplicidad, devolvemos el modelo.

            return pagedList;
        }

        // ----------------------------------------------------------------------
        // OPERACIONES CRUD
        // ----------------------------------------------------------------------
        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _employeeRepository.GetAllAsync();
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            // 1. Lógica de Negocio (ej. Validación de ID)
            if (id <= 0)
            {
                throw new ArgumentException("El ID del empleado debe ser positivo.");
            }

            // 2. Llama al repositorio
            return await _employeeRepository.GetByIdAsync(id);
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            // 1. Lógica de Negocio (Validaciones complejas de Dominio)
            if (employee.birthDate > DateTime.Now.AddYears(-18))
            {
                throw new ApplicationException("El empleado debe ser mayor de 18 años.");
            }

            // 2. Llama al repositorio
            await _employeeRepository.AddAsync(employee);
        }

        public async Task UpdateEmployeeAsync(int id, Employee employee)
        {
            // 1. Lógica de Negocio (Validación de existencia y reglas de actualización)
            if (!await _employeeRepository.ExistsAsync(id))
            {
                throw new KeyNotFoundException($"Empleado con ID {id} no encontrado.");
            }

            // 2. Llama al repositorio
            await _employeeRepository.UpdateAsync(employee);
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            // 1. Lógica de Negocio (ej. No permitir eliminar si tiene tareas pendientes)
            // if (await _taskRepository.HasPendingTasks(id)) { throw ... }

            // 2. Llama al repositorio
            await _employeeRepository.DeleteAsync(id);
        }

        public async Task AddEmployeeListAsync(List<Employee> employees)
        {
            // 1. Validación de seguridad
            if (employees == null || !employees.Any()) return;

            if (employees.Count > 500)
            {
                throw new ApplicationException("El tamaño máximo total permitido es 500.");
            }

            // 2. Procesamiento por lotes
            var lotes = employees.Chunk(20);

            foreach (var lote in lotes)
            {
                // 3. Enviamos el lote al repositorio
                await _employeeRepository.AddEmployeeListAsync(lote.ToList());

                // 4. Pausa de cortesía (IMPORTANTE para servidores compartidos)
                // Esto evita que SQL Server interprete la ráfaga de peticiones como un ataque o saturación
                await Task.Delay(100);
            }
        }
    }
}