using MongoDB.Bson;
using MongoDB.Driver;
using RestauranteBack.Modelo.DTO;
using RestauranteBack.Modelo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Infraestructura.Servicios
{
    public class RestauranteService
    {
        private readonly IMongoCollection<Restaurante> _restaurantes;

        public RestauranteService(IMongoDatabase database)
        {
            _restaurantes = database.GetCollection<Restaurante>("restaurantes");
        }

        // Método para crear un nuevo restaurante
        public async Task<RestauranteDto> CrearRestauranteAsync(RestauranteDto restauranteDto)
        {
            var restaurante = MapearEntidad(restauranteDto);
            await _restaurantes.InsertOneAsync(restaurante);
            return MapToDto(restaurante);
        }

        // Método para obtener todos los restaurantes
        public async Task<List<RestauranteDto>> ObtenerRestaurantesAsync(string buscar, string categoria, string ubicacion)
        {
            var filter = Builders<Restaurante>.Filter.Empty;

            if (!string.IsNullOrEmpty(buscar))
            {
                var searchPattern = new BsonRegularExpression($".*{buscar}.*", "i");
                var nombreFilter = Builders<Restaurante>.Filter.Regex("nombre", searchPattern);
                var descripcionFilter = Builders<Restaurante>.Filter.Regex("descripcion", searchPattern);
                filter &= Builders<Restaurante>.Filter.Or(nombreFilter, descripcionFilter);
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                // Usar regex para hacer la búsqueda de categoría case-insensitive
                var categoriaPattern = new BsonRegularExpression($"^{categoria}$", "i");
                filter &= Builders<Restaurante>.Filter.Regex("categoria", categoriaPattern);
            }

            if (!string.IsNullOrEmpty(ubicacion))
            {
                // También hacer case-insensitive la búsqueda por ubicación
                var ubicacionPattern = new BsonRegularExpression($"^{ubicacion}$", "i");
                filter &= Builders<Restaurante>.Filter.Regex("ubicacion.ciudad", ubicacionPattern);
            }

            var restaurantes = await _restaurantes.Find(filter).ToListAsync();
            return restaurantes.ConvertAll(MapToDto);
        }

        // Método para obtener un restaurante por Id
        public async Task<RestauranteDto> ObtenerRestaurantePorIdAsync(string id)
        {
            var restaurante = await _restaurantes.Find(r => r.Id == id).FirstOrDefaultAsync();
            return restaurante != null ? MapToDto(restaurante) : null;
        }

        // Método para actualizar un restaurante
        public async Task<bool> ActualizarRestauranteAsync(string id, RestauranteDto restauranteDto)
        {
            var restaurante = MapearEntidad(restauranteDto);
            restaurante.Id = id;
            var resultado = await _restaurantes.ReplaceOneAsync(r => r.Id == id, restaurante);
            return resultado.ModifiedCount > 0;
        }

        // Método para eliminar un restaurante
        public async Task<bool> EliminarRestauranteAsync(string id)
        {
            var resultado = await _restaurantes.DeleteOneAsync(r => r.Id == id);
            return resultado.DeletedCount > 0;
        }

        // Método para mapear DTO a entidad
        private Restaurante MapearEntidad(RestauranteDto dto) => new Restaurante
        {
            Id = dto.Id,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Categoria = dto.Categoria,
            Horario = new Horario
            {
                Lunes = MapToDiaHorario(dto.Horario.Lunes),
                Martes = MapToDiaHorario(dto.Horario.Martes),
                Miercoles = MapToDiaHorario(dto.Horario.Miercoles),
                Jueves = MapToDiaHorario(dto.Horario.Jueves),
                Viernes = MapToDiaHorario(dto.Horario.Viernes),
                Sabado = MapToDiaHorario(dto.Horario.Sabado),
                Domingo = MapToDiaHorario(dto.Horario.Domingo)
            },
            Ubicacion = new Ubicacion
            {
                Direccion = dto.Ubicacion.Direccion,
                Ciudad = dto.Ubicacion.Ciudad,
                Pais = dto.Ubicacion.Pais
            },
            Menu = dto.Menu?.ConvertAll(menuItem => new MenuItem
            {
                Nombre = menuItem.Nombre,
                Descripcion = menuItem.Descripcion,
                Precio = menuItem.Precio,
                Disponible = menuItem.Disponible,
                Imagen = menuItem.Imagen
            }),
            Imagenes = dto.Imagenes,
            Caracteristicas = new Caracteristicas
            {
                TieneDelivery = dto.Caracteristicas.TieneDelivery,
                AceptaTarjeta = dto.Caracteristicas.AceptaTarjeta,
                TieneEstacionamiento = dto.Caracteristicas.TieneEstacionamiento,
                Wifi = dto.Caracteristicas.Wifi
            }
        };

        // Método para mapear entidad a DTO
        private RestauranteDto MapToDto(Restaurante entity) => new RestauranteDto
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            Categoria = entity.Categoria,
            Horario = new HorarioDto
            {
                Lunes = MapToDiaHorarioDto(entity.Horario.Lunes),
                Martes = MapToDiaHorarioDto(entity.Horario.Martes),
                Miercoles = MapToDiaHorarioDto(entity.Horario.Miercoles),
                Jueves = MapToDiaHorarioDto(entity.Horario.Jueves),
                Viernes = MapToDiaHorarioDto(entity.Horario.Viernes),
                Sabado = MapToDiaHorarioDto(entity.Horario.Sabado),
                Domingo = MapToDiaHorarioDto(entity.Horario.Domingo)
            },
            Ubicacion = new UbicacionDto
            {
                Direccion = entity.Ubicacion.Direccion,
                Ciudad = entity.Ubicacion.Ciudad,
                Pais = entity.Ubicacion.Pais
            },
            Menu = entity.Menu?.ConvertAll(menuItem => new MenuItemDto
            {
                Nombre = menuItem.Nombre,
                Descripcion = menuItem.Descripcion,
                Precio = menuItem.Precio,
                Disponible = menuItem.Disponible,
                Imagen = menuItem.Imagen
            }),
            Imagenes = entity.Imagenes,
            Caracteristicas = new CaracteristicasDto
            {
                TieneDelivery = entity.Caracteristicas.TieneDelivery,
                AceptaTarjeta = entity.Caracteristicas.AceptaTarjeta,
                TieneEstacionamiento = entity.Caracteristicas.TieneEstacionamiento,
                Wifi = entity.Caracteristicas.Wifi
            }
        };

        private DiaHorario MapToDiaHorario(DiaHorarioDto dto) => new DiaHorario { Apertura = dto.Apertura, Cierre = dto.Cierre };
        private DiaHorarioDto MapToDiaHorarioDto(DiaHorario diaHorario) => new DiaHorarioDto { Apertura = diaHorario.Apertura, Cierre = diaHorario.Cierre };
    }
}
