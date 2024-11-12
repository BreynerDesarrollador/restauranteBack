using Microsoft.AspNetCore.Mvc;
using RestauranteBack.Infraestructura.Servicios;
using RestauranteBack.Modelo.DTO;
using RestauranteBack.WebApiRestaurante.ClasesGenerales;

namespace RestauranteBack.WebApiRestaurante.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaestrosController : ControllerBase
    {
        private readonly maestrosServicios _maestrosServicio;

        public MaestrosController(maestrosServicios maestrosService)
        {
            _maestrosServicio = maestrosService;
        }

        // GET: api/restaurante
        [HttpGet()]
        public async Task<ActionResult<List<MaestrosDTO>>> ObtenerMaestros()
        {
            try
            {
                var datosMaestros = await _maestrosServicio.ObtenerMaestrosAsync();
                return Ok(new RespuestaWebApi<object> { data = datosMaestros });
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
