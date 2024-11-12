using RestauranteBack.Modelo.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Infraestructura.Servicios
{
    public interface IAuthService
    {
        Task<InicioSesionRespuestaDto> LoginAsync(InicioSesionDto loginDto);
        Task<UsuarioDTO> VerifyTokenAsync(string token);
    }
}
