using ApiComponents.Models;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ApiComponents.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        // ----------------------------------------------------------------------
        // READ: GET /api/Country (Obtener todos)
        // ----------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            var countries = await _countryService.GetAllCountriesAsync();
            return Ok(countries);
        }

        // ----------------------------------------------------------------------
        // READ: GET /api/Country/{id} (Obtener por ID)
        // ----------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountry(int id)
        {
            try
            {
                var country = await _countryService.GetCountryByIdAsync(id);
                return Ok(country);
            }
            catch (KeyNotFoundException)
            {
                // El servicio lanzó esta excepción si el país no fue encontrado
                return NotFound(); // HTTP 404
            }
            catch (ArgumentException ex)
            {
                // El servicio lanzó esta excepción si el ID es inválido
                return BadRequest(ex.Message); // HTTP 400
            }
        }

        // ----------------------------------------------------------------------
        // CREATE: POST /api/Country (Crear nuevo)
        // ----------------------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(Country country)
        {
            try
            {
                // El servicio se encarga de las validaciones y el guardado
                await _countryService.AddCountryAsync(country);

                // Devuelve 201 CreatedAtAction, apuntando al nuevo recurso
                return CreatedAtAction(nameof(GetCountry), new { id = country.id }, country);
            }
            catch (ArgumentException ex)
            {
                // Captura validaciones de campo (ej. descripción vacía)
                return BadRequest(ex.Message); // HTTP 400
            }

            catch (InvalidOperationException ex)
            {
                // Captura la excepción de unicidad lanzada por el Servicio
                return Conflict(ex.Message); // HTTP 409 Conflict (El recurso ya existe)
            }
            
        }

        // ----------------------------------------------------------------------
        // UPDATE: PUT /api/Country/{id} (Actualizar existente)
        // ----------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, Country country)
        {
            if (id != country.id)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del cuerpo."); // HTTP 400
            }

            try
            {
                await _countryService.UpdateCountryAsync(id, country);
                return NoContent(); // HTTP 204 (Estándar para PUT exitoso sin contenido de retorno)
            }
            catch (KeyNotFoundException)
            {
                // El servicio lanzó esta excepción si el país no existe
                return NotFound(); // HTTP 404
            }
            catch (ArgumentException ex)
            {
                // Captura validaciones de campo
                return BadRequest(ex.Message); // HTTP 400
            }
        }

        // ----------------------------------------------------------------------
        // DELETE: DELETE /api/Country/{id} (Eliminar)
        // ----------------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            try
            {
                await _countryService.DeleteCountryAsync(id);
                return NoContent(); // HTTP 204 (Estándar para DELETE exitoso)
            }
            catch (KeyNotFoundException)
            {
                // El servicio lanzó esta excepción si el país no existe
                return NotFound(); // HTTP 404
            }
            // Opcionalmente, podrías capturar DbUpdateException si falla por FK (Foreign Key constraint)
            // y devolver un 409 Conflict.
        }
    }
}