using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using iLib.src.main.Model;
using Microsoft.IdentityModel.Tokens;

namespace iLib.src.main.rest
{
    public class JwtHelper
    {
        private const string SecretKey = "iLib SWAM project - Mihail Teodor Gurzu";
        private static readonly byte[] Key = Encoding.ASCII.GetBytes(SecretKey);

        public static string GenerateToken(Guid id, string email, string userRole)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                    new Claim("email", email),
                    new Claim(ClaimTypes.Role, userRole)
                ]),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "iLib",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Key),
                    ValidateIssuer = true,
                    ValidIssuer = "iLib",
                    ValidateAudience = false
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Guid? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                if (tokenHandler.ReadToken(token) is not JwtSecurityToken jwtToken) return null;

                var id = jwtToken.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
                return Guid.Parse(id);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string? GetEmailFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                var emailClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "email");
                return emailClaim?.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }



        public static string? GetUserRoleFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                var roleClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "role");
                if (roleClaim == null)
                {
                    return null;
                }

                if (Enum.TryParse<UserRole>(roleClaim.Value, out var userRole))
                {
                    return userRole.ToString();
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
