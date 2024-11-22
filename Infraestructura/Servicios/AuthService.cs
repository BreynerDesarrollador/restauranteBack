using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using RestauranteBack.Modelo.DTO;
using RestauranteBack.Modelo.Interfaces;
using RestauranteBack.Modelo.Provider;
using RestauranteBack.WebApiRestaurante.ClasesGenerales;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Infraestructura.Servicios
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<Usuario> _usuarios;


        public AuthService(IMongoDatabase database, IConfiguration configuration)
        {
            _usuarios = database.GetCollection<Usuario>("usuarios");
            _configuration = configuration;
        }
        public async Task<UsuarioDTO> CreateUserAsync(UsuarioDTO usuarioDto)
        {
            // Validar si el usuario ya existe
            var existingUser = await _usuarios.Find(u => u.usuario == usuarioDto.usuario).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                throw new ExcepcionPeticionApi("El nombre de usuario ya está en uso.", 500);
            }
            var usuario = new Usuario
            {
                usuario = usuarioDto.usuario,
                clave = BCrypt.Net.BCrypt.HashPassword(usuarioDto.clave),
                rol = usuarioDto.rol,
                nombre = usuarioDto.nombre,
                celular = usuarioDto.celular,
                correo = usuarioDto.correo,
            };
            await _usuarios.InsertOneAsync(usuario);
            return usuarioDto;
        }

        public async Task<IEnumerable<UsuarioDTO>> GetAllUsersAsync()
        {
            var usuarios = await _usuarios.Find(_ => true).ToListAsync();
            return usuarios.Select(u => new UsuarioDTO
            {
                // Asigna las propiedades del modelo al DTO
            });
        }

        public async Task<UsuarioDTO> GetUserByIdAsync(string id)
        {
            var usuario = await _usuarios.Find(u => u._id == id).FirstOrDefaultAsync();
            if (usuario == null) return null;
            return new UsuarioDTO
            {
                // Asigna las propiedades del modelo al DTO
            };
        }

        public async Task<bool> UpdateUserAsync(string id, UsuarioDTO usuarioDto)
        {
            var updateResult = await _usuarios.ReplaceOneAsync(u => u._id == id, new Usuario
            {
                // Asigna las propiedades del DTO al modelo
            });
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var deleteResult = await _usuarios.DeleteOneAsync(u => u._id == id);
            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }
        public async Task<InicioSesionRespuestaDto> LoginAsync(InicioSesionDto loginDto)
        {
            try
            {
                var usuario = await _usuarios.Find(u => u.usuario == loginDto.usuario).FirstOrDefaultAsync();
                if (usuario == null || !VerifyPassword(loginDto.clave, usuario.clave))
                {
                    throw new ExcepcionPeticionApi("El usuario no existe o las credenciales son inválidas", 400);
                }

                var token = GenerateJwtToken(usuario);
                return new InicioSesionRespuestaDto
                {
                    Token = token,
                    usuario = new UsuarioDTO
                    {
                        _id = usuario._id,
                        usuario = usuario.usuario,
                        rol = usuario.rol
                    }
                };
            }
            catch (ExcepcionPeticionApi ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }

        public async Task<UsuarioDTO> VerifyTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "id").Value;

                var usuario = await _usuarios.Find(u => u._id == userId).FirstOrDefaultAsync();
                if (usuario == null) throw new ExcepcionPeticionApi("El usuario no se encuentra autenticado", 400);

                return new UsuarioDTO
                {
                    _id = usuario._id,
                    usuario = usuario.usuario,
                    rol = usuario.rol
                };
            }
            catch(ExcepcionPeticionApi ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }

        private string GenerateJwtToken(Usuario user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
            try
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim("id", user._id),
                new Claim("usuario", user.usuario),
                new Claim("rol", user.rol),
                new Claim("nombre", user?.nombre),
                new Claim("celular", user?.celular),
                new Claim("correo", user?.correo),
            }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    ),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"]
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, 500);
            }

        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
