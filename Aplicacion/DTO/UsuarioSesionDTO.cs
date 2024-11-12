using RestauranteBack.Modelo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.DTO
{
    public class Usuario : BaseEntity
    {
        public string usuario { get; set; }
        public string clave { get; set; }
        public string rol { get; set; }
        public string nombre { get; set; }
        public string celular { get; set; }
        public string correo { get; set; }
    }

    public class InicioSesionDto
    {
        public string usuario { get; set; }
        public string clave { get; set; }
    }

    public class InicioSesionRespuestaDto
    {
        public string Token { get; set; }
        public UsuarioDTO usuario { get; set; }
    }

    public class UsuarioDTO : BaseEntity
    {
        public string usuario { get; set; }
        public string rol { get; set; }
        public string clave { get; set; }
        public string nombre { get; set; }
        public string celular { get; set; }
        public string correo { get; set; }
    }
}
