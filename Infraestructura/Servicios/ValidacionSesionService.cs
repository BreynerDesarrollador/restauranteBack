using Microsoft.AspNetCore.Http;
using RestauranteBack.WebApiRestaurante.ClasesGenerales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Infraestructura.Servicios
{
    public class ValidacionSesionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ValidacionSesionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<string> obtenerDatosusuarioJWT()
        {
            var userId = string.Empty;

            var usuarioAuth = _httpContextAccessor.HttpContext.User.Claims.Count();
            if (usuarioAuth > 0)
            {
                userId = _httpContextAccessor.HttpContext.User.Claims.First().Value;
            }
            else
                throw new ExcepcionPeticionApi("Usuario no autenticado", 400);


            return userId;

        }
    }
}
