using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RestauranteBack.Infraestructura.Servicios;
using RestauranteBack.Modelo.DTO;
using RestauranteBack.Modelo.Entidades;
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
        /*[HttpPost]
        public async Task<ActionResult<RestauranteDto>> CrearRestaurante(RestauranteDto restauranteDto)
        {
            var creado = await _restauranteService.CrearRestauranteAsync(restauranteDto);
            return CreatedAtAction(nameof(ObtenerRestaurantePorId), new { id = creado.Id }, creado);
        }*/

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
            try
            {
                var restaurante = await _restauranteService.ObtenerRestaurantePorIdAsync(id);
                if (restaurante == null)
                    return NotFound();
                return Ok(new RespuestaWebApi<object> { data = restaurante });
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
        [HttpPost("{restauranteId}/DarMeGusta")]
        public async Task<IActionResult> DarMegustaRestaurante(string restauranteId)
        {
            try
            {
                await _restauranteService.DarMeGustaRestaurante(restauranteId);
                return Ok(new RespuestaWebApi<object> { });
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
        [HttpDelete("{restauranteId}/QuitarMeGusta")]
        public async Task<IActionResult> QuitarMeGustaRestaurante(string restauranteId)
        {
            try
            {
                await _restauranteService.QuitarMeGustaRestaurante(restauranteId);
                return Ok(new RespuestaWebApi<object> { });
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
        [HttpPost("{restauranteId}/DarNoMeGusta")]
        public async Task<IActionResult> DarNoMeGustaRestaurante(string restauranteId)
        {
            try
            {
                await _restauranteService.DarNoMeGustaRestaurante(restauranteId);
                return Ok(new RespuestaWebApi<object> { });
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
        [HttpDelete("{restauranteId}/QuitarNoMeGusta")]
        public async Task<IActionResult> QuitarNoMeGustaRestaurante(string restauranteId)
        {
            try
            {
                await _restauranteService.QuitarNoMeGustaRestaurante(restauranteId);
                return Ok(new RespuestaWebApi<object> { });
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
        [HttpGet("ObtenerResenasRestaurante/{restauranteId}")]
        public async Task<ActionResult<RespuestaPaginada<ResenaDTO>>> ObtenerResenasRestaurante(
    string restauranteId,
    [FromQuery] BusquedaResenasParametros parametros)
        {
            try
            {
                var datosResenas = await _restauranteService.ObtenerResenasRestaurante(restauranteId, parametros);
                return Ok(new RespuestaWebApi<object> { data = datosResenas });
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
        [HttpGet("restaurante/{restauranteId}/estadisticas")]
        public async Task<ActionResult<EstadisticaRestauranteDTO>> ObtenerEstadisticas(string restauranteId)
        {
            try
            {
                var datosEstadistica = await _restauranteService.estadisticasRestuarante(restauranteId);
                return Ok(new RespuestaWebApi<object> { data = datosEstadistica });
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
        [HttpGet("restaurante/registrarVisita/{idRestaurante}")]
        public async Task<ActionResult<IEnumerable<Resena>>> registrarVisita(string idRestaurante)
        {
            try
            {
                await _restauranteService.registrarVisita(idRestaurante);
                return Ok(new RespuestaWebApi<object> { });
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
        [HttpPut("{id}/imagenes")]
        public async Task<IActionResult> ActualizarImagenes(string id, [FromBody] ActualizarImagenesRestauranteDto dto)
        {
            try
            {
                await _restauranteService.ActualizarImagenesAsync(id, dto);
                return Ok(new RespuestaWebApi<object> { mensaje = "Imagenes actualizadas con exito" });
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
    }
}
