using Microsoft.AspNetCore.Mvc;
using RestauranteBack.Infraestructura.Servicios;
using RestauranteBack.Modelo.DTO;
using RestauranteBack.Modelo.Interfaces;
using RestauranteBack.WebApiRestaurante.ClasesGenerales;

namespace RestauranteBack.WebApiRestaurante.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantesController : ControllerBase
    {
        private readonly RestauranteService _restauranteService;

        public RestaurantesController(RestauranteService restauranteService)
        {
            _restauranteService = restauranteService;
        }

        // POST: api/restaurante
        [HttpPost]
        public async Task<ActionResult<RestauranteDto>> CrearRestaurante(RestauranteDto restauranteDto)
        {
            var creado = await _restauranteService.CrearRestauranteAsync(restauranteDto);
            return CreatedAtAction(nameof(ObtenerRestaurantePorId), new { id = creado.Id }, creado);
        }

        // GET: api/restaurante
        [HttpGet()]
        public async Task<ActionResult<List<RestauranteDto>>> ObtenerRestaurantes(string? buscar, string? categoria, string? ubicacion)
        {
            try
            {
                var restaurantes = await _restauranteService.ObtenerRestaurantesAsync(buscar, categoria, ubicacion);
                return Ok(new RespuestaWebApi<object> { data = restaurantes });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(ex.CodigoError, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });

            }
        }

        // GET: api/restaurante/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RestauranteDto>> ObtenerRestaurantePorId(string id)
        {
            var restaurante = await _restauranteService.ObtenerRestaurantePorIdAsync(id);
            if (restaurante == null)
                return NotFound();

            return Ok(restaurante);
        }

        // PUT: api/restaurante/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarRestaurante(string id, RestauranteDto restauranteDto)
        {
            if (id != restauranteDto.Id)
                return BadRequest("El ID de la URL no coincide con el ID del objeto.");

            var actualizado = await _restauranteService.ActualizarRestauranteAsync(id, restauranteDto);
            if (!actualizado)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/restaurante/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarRestaurante(string id)
        {
            var eliminado = await _restauranteService.EliminarRestauranteAsync(id);
            if (!eliminado)
                return NotFound();

            return NoContent();
        }
    }
}
