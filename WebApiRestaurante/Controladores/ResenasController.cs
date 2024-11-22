using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using MongoDB.Bson;
using MongoDB.Driver;
using RestauranteBack.Infraestructura.Servicios;
using RestauranteBack.Modelo.DTO;
using RestauranteBack.Modelo.Entidades;
using RestauranteBack.Modelo.Interfaces;
using RestauranteBack.WebApiRestaurante.ClasesGenerales;
using System.Security.Claims;

namespace RestauranteBack.WebApiRestaurante.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResenasController : ControllerBase
    {
        private readonly IMongoCollection<Resena> _resenas;
        private readonly ResenaServicio _resenaServicio;
        public ResenasController(ResenaServicio resenaServicio)
        {
            _resenaServicio = resenaServicio;
        }
        [HttpPost("RegistrarResena")]
        public async Task<ActionResult<ResenaDTO>> CrearResena([FromBody] CrearResenaDTO dto)
        {
            try
            {
                var datosRsena = await _resenaServicio.CrearResena(dto);
                return Ok(new RespuestaWebApi<object> { data = datosRsena });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(500, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }

        [HttpPost("{restauranteId}/{resenaId}/DarMeGusta")]
        public async Task<IActionResult> DarMeGustaResena(string restauranteId, string resenaId)
        {
            try
            {
                await _resenaServicio.DarMeGustaResena(restauranteId, resenaId);
                return Ok(new RespuestaWebApi<object> { });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(500, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }
        [HttpDelete("{restauranteId}/{resenaId}/QuitarMeGusta")]
        public async Task<IActionResult> QuitarMeGusta(string restauranteId, string resenaId)
        {
            try
            {
                await _resenaServicio.QuitarMeGustaResena(restauranteId, resenaId);
                return Ok(new RespuestaWebApi<object> {  });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(500, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }

        [HttpPost("{restauranteId}/{resenaId}/DarNoMeGusta")]
        public async Task<IActionResult> DarNoMeGusta(string restauranteId, string resenaId)
        {
            try
            {
                await _resenaServicio.DarNoMeGusta(restauranteId, resenaId);
                return Ok(new RespuestaWebApi<object> { });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(500, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }

        [HttpDelete("{restauranteId}/{resenaId}/QuitarNoMeGusta")]
        public async Task<IActionResult> QuitarNoMeGusta(string restauranteId, string resenaId)
        {
            try
            {
                await _resenaServicio.QuitarNoMeGusta(restauranteId, resenaId);
                return Ok(new RespuestaWebApi<object> { });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(500, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }

        [HttpPost("{resenaId}/respuesta")]
        public async Task<IActionResult> AgregarRespuesta(string resenaId, Respuesta respuesta)
        {
            try
            {
                await _resenaServicio.agregarRespuestaResena(resenaId, respuesta);
                return Ok(new RespuestaWebApi<object> { });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(500, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }

        [HttpPost("{resenaId}/respuesta/{respuestaId}/DarMeGustaRespuesta")]
        public async Task<IActionResult> DarMeGustaRespuesta(string resenaId, string respuestaId)
        {
            try
            {
                await _resenaServicio.DarMeGustaRespuesta(resenaId, respuestaId);
                return Ok(new RespuestaWebApi<object> { });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(500, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }


        // Endpoint para obtener una reseña específica
        [HttpGet("restaurante/{restauranteId}/resena/{resenaId}")]
        public async Task<ActionResult<ResenaDTO>> ObtenerResena(string restauranteId, string resenaId)
        {
            try
            {
                var datosResena = await _resenaServicio.ObtenerResena(restauranteId, resenaId);
                return Ok(new RespuestaWebApi<object> { data = datosResena });
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
