using MongoDB.Driver;
using RestauranteBack.Modelo.DTO;
using RestauranteBack.Modelo.Interfaces;
using RestauranteBack.WebApiRestaurante.ClasesGenerales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Infraestructura.Servicios
{
    public class maestrosServicios
    {
        private readonly IMongoCollection<Maestros> _maestros;

        public maestrosServicios(IMongoDatabase database)
        {
            _maestros = database.GetCollection<Maestros>("maestros");
        }
        // Método para obtener todos los restaurantes
        public async Task<List<MaestrosDTO>> ObtenerMaestrosAsync()
        {
            try
            {
                var datosMaestros = await _maestros.Find(_ => true).ToListAsync();
                return datosMaestros?.ConvertAll(MapearEntidadDTO);
            }
            catch (ExcepcionPeticionApi ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }

        private MaestrosDTO MapearEntidadDTO(Maestros dto) => new MaestrosDTO
        {
            categorias = dto.categorias,
            ubicaciones = dto.ubicaciones,
        };
    }
}
