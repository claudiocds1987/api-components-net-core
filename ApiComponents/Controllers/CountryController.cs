using ApiComponents.Models;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            var countries = await _countryService.GetAllCountriesAsync();
            return Ok(countries);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountry(int id)
        {
            try
            {
                var country = await _countryService.GetCountryByIdAsync(id);
                return Ok(country);
            }
            // CAMBIO AQUÍ: Nombre completo para evitar ambigüedad con GreenDonut
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(Country country)
        {
            try
            {
                await _countryService.AddCountryAsync(country);
                return CreatedAtAction(nameof(GetCountry), new { id = country.id }, country);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, Country country)
        {
            if (id != country.id)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del cuerpo.");
            }

            try
            {
                await _countryService.UpdateCountryAsync(id, country);
                return NoContent();
            }
            // CAMBIO AQUÍ: Nombre completo
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            try
            {
                await _countryService.DeleteCountryAsync(id);
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