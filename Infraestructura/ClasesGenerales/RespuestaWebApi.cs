using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace RestauranteBack.WebApiRestaurante.ClasesGenerales
{
    public class RespuestaWebApi<T>
    {
        public bool exito { get; set; } = true;
        public string mensaje { get; set; } = string.Empty;

        public T data { get; set; }
    }
}
