using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Org.BouncyCastle.Crypto.Generators;
using RestauranteBack.Modelo.DTO;
using RestauranteBack.Modelo.Interfaces;
using RestauranteBack.Modelo.Provider;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Infraestructura.Servicios
{
    public class AuthSesionServicio
    {
        private readonly IConfiguration _configuration;
        private readonly MongoDBProvider _mongoDBProvider;
        public AuthSesionServicio(MongoDBProvider database, IConfiguration configuration)
        {
            _mongoDBProvider = database;
            _configuration = configuration;
        }

        public async Task<InicioSesionRespuestaDto> InicioSesionAsync(InicioSesionDto loginDto)
        {
            var usuario = await _mongoDBProvider.GetUsuariosCollection().Find(u => u.usuario == loginDto.usuario).FirstOrDefaultAsync();
            if (usuario == null || !VerificarClave(loginDto.clave, usuario.clave))
            {
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            var token = GenerarJwtToken(usuario);
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

        public async Task<UsuarioDTO> VerificarTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

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

                var usuario = await _mongoDBProvider.GetUsuariosCollection().Find(u => u._id == userId).FirstOrDefaultAsync();
                if (usuario == null) throw new UnauthorizedAccessException();

                return new UsuarioDTO
                {
                    _id = usuario._id,
                    usuario = usuario.usuario,
                    rol = usuario.rol
                };
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }
        }

        private string GenerarJwtToken(Usuario user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("id", user._id),
                new Claim(ClaimTypes.Name, user.usuario),
                new Claim(ClaimTypes.Role, user.rol)
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

        private bool VerificarClave(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
