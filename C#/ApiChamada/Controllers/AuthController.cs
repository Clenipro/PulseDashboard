using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ApiChamada.Data;
using ApiChamada.Models;
using BCrypt.Net;
using System.Security.Cryptography;

namespace ApiChamada.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserDao _userDao;
        private readonly IConfiguration _configuration;

        public AuthController(UserDao userDao, IConfiguration configuration)
        {
            _userDao = userDao;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Credenciais inválidas" });
            }

            var user = await _userDao.AuthenticateUser(request.Username, request.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Credenciais inválidas" });
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays"));
            await _userDao.SaveRefreshToken(user.Id, refreshToken, expiresAt);

            var userResponse = new
            {
                id = user.Id,
                nome = user.Nome,
                username = user.Username,
                role = user.Role
            };

            return Ok(new { token, refreshToken, user = userResponse });
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateRequest request)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

                var user = await _userDao.GetUserById(userId);
                if (user == null)
                {
                    return Unauthorized(new { message = "Usuário não encontrado" });
                }

                var userResponse = new
                {
                    id = user.Id,
                    nome = user.Nome,
                    username = user.Username,
                    role = user.Role
                };

                return Ok(new { user = userResponse });
            }
            catch (Exception)
            {
                return Unauthorized(new { message = "Token inválido ou expirado" });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var refreshToken = await _userDao.GetRefreshToken(request.RefreshToken);
            if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Token de renovação inválido ou expirado" });
            }

            var user = await _userDao.GetUserById(refreshToken.UserId);
            if (user == null)
            {
                return Unauthorized(new { message = "Usuário não encontrado" });
            }

            var newToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            await _userDao.DeleteRefreshToken(request.RefreshToken);
            await _userDao.SaveRefreshToken(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays")));

            var userResponse = new
            {
                id = user.Id,
                nome = user.Nome,
                username = user.Username,
                role = user.Role
            };

            return Ok(new { token = newToken, refreshToken = newRefreshToken, user = userResponse });
        }

        [HttpPost("update-name")]
        public async Task<IActionResult> UpdateName([FromBody] UpdateNameRequest request)
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = GetUserIdFromToken(token);
                if (userId == null)
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                var user = await _userDao.GetUserById(userId.Value);
                if (user == null)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                await _userDao.UpdateUserName(userId.Value, request.Name);
                return Ok(new { message = "Nome atualizado com sucesso" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Falha ao atualizar o nome" });
            }
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = GetUserIdFromToken(token);
                if (userId == null)
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                var user = await _userDao.GetUserById(userId.Value);
                if (user == null)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                await _userDao.UpdateUserPassword(userId.Value, hashedPassword);
                return Ok(new { message = "Senha atualizada com sucesso" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Falha ao atualizar a senha" });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes"));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private int? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
            }
            catch
            {
                return null;
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ValidateRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class UpdateNameRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdatePasswordRequest
    {
        public string Password { get; set; } = string.Empty;
    }
}