using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.DTO
{
    public class ActualizarImagenesRestauranteDto
    {
        public string imagenPrincipal { get; set; } // Base64 de la imagen principal
        public List<string>? imagenes { get; set; } // Lista de imágenes en base64
    }
}
