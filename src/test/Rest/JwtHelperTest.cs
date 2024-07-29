using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using iLib.src.main.rest;
using iLib.src.main.Model;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public class JwtHelperTest
{
    private static readonly string validEmail = "user@example.com";
    private static readonly UserRole validRole = UserRole.ADMINISTRATOR;
    private static readonly Guid validUserId = Guid.NewGuid();

    private const string secretKey = "iLib SWAM project - Mihail Teodor Gurzu";
    private static readonly byte[] key = Encoding.ASCII.GetBytes(secretKey);

    [Fact]
    public void GenerateToken_ShouldReturnValidToken()
    {
        var token = JwtHelper.GenerateToken(validUserId, validEmail, validRole.ToString());
        var handler = new JwtSecurityTokenHandler();
        var decoded = handler.ReadJwtToken(token);

        Assert.Equal(validUserId.ToString(), decoded.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(validEmail, decoded.Claims.First(c => c.Type == "email").Value);
        Assert.Equal(validRole.ToString(), decoded.Claims.First(c => c.Type == "role").Value);
    }

    [Fact]
    public void TokenExpirationIsSetCorrectly()
    {
        var token = JwtHelper.GenerateToken(validUserId, validEmail, validRole.ToString());
        var handler = new JwtSecurityTokenHandler();
        var decoded = handler.ReadJwtToken(token);
        var expectedDuration = TimeSpan.FromHours(1).TotalMilliseconds;
        var actualDuration = (decoded.ValidTo - decoded.IssuedAt).TotalMilliseconds;

        Assert.Equal(expectedDuration, actualDuration, precision: 0);
    }

    [Fact]
    public void ValidateToken_ShouldReturnTrueForValidToken()
    {
        var token = JwtHelper.GenerateToken(validUserId, validEmail, validRole.ToString());

        var isValid = JwtHelper.ValidateToken(token);

        Assert.True(isValid);
    }

    [Fact]
    public void ValidateToken_WithInvalidIssuer_ShouldReturnFalse()
    {
        var token = GenerateTokenWithCustomIssuer("invalidIssuer");

        var isValid = JwtHelper.ValidateToken(token);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnFalse()
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var handler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                    new Claim(JwtRegisteredClaimNames.Sub, validUserId.ToString()),
                    new Claim("email", validEmail),
                    new Claim(ClaimTypes.Role, validRole.ToString())
                ]),
            NotBefore = oneHourAgo.AddMinutes(-2),
            Expires = oneHourAgo.AddMinutes(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(tokenDescriptor);
        var tokenString = handler.WriteToken(token);

        var isValid = JwtHelper.ValidateToken(tokenString);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnFalse()
    {
        var token = "invalid.token.here";

        var isValid = JwtHelper.ValidateToken(token);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateToken_WithEmptyToken_ShouldReturnFalse()
    {
        var token = "";

        var isValid = JwtHelper.ValidateToken(token);

        Assert.False(isValid);
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ShouldReturnCorrectUserId()
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, validUserId.ToString())]),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(tokenDescriptor);
        var tokenString = handler.WriteToken(token);

        var userId = JwtHelper.GetUserIdFromToken(tokenString);

        Assert.Equal(validUserId, userId);
    }

    [Fact]
    public void GetUserIdFromToken_WithMalformedToken_ShouldReturnNull()
    {
        var token = "not.valid.token";

        var userId = JwtHelper.GetUserIdFromToken(token);

        Assert.Null(userId);
    }

    [Fact]
    public void GetUserIdFromToken_WithNonNumericSubject_ShouldReturnNull()
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, "invalid user id")]),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(tokenDescriptor);
        var tokenString = handler.WriteToken(token);

        var userId = JwtHelper.GetUserIdFromToken(tokenString);

        Assert.Null(userId);
    }

    [Fact]
    public void GetEmailFromToken_WithValidToken_ShouldReturnCorrectEmail()
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("email", validEmail)]),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(tokenDescriptor);
        var tokenString = handler.WriteToken(token);

        var email = JwtHelper.GetEmailFromToken(tokenString);

        Assert.Equal(validEmail, email);
    }

    [Fact]
    public void GetEmailFromToken_WithoutEmailClaim_ShouldReturnNull()
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(tokenDescriptor);
        var tokenString = handler.WriteToken(token);

        var email = JwtHelper.GetEmailFromToken(tokenString);

        Assert.Null(email);
    }

    [Fact]
    public void GetEmailFromToken_WithMalformedToken_ShouldReturnNull()
    {
        var token = "not.valid.token";

        var email = JwtHelper.GetEmailFromToken(token);

        Assert.Null(email);
    }

    [Fact]
    public void GetUserRoleFromToken_WithValidRole_ShouldReturnCorrectRole()
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Role, UserRole.ADMINISTRATOR.ToString())]),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(tokenDescriptor);
        var tokenString = handler.WriteToken(token);

        var role = JwtHelper.GetUserRoleFromToken(tokenString);

        Assert.Equal(UserRole.ADMINISTRATOR.ToString(), role);
    }

    [Fact]
    public void GetUserRoleFromToken_WithoutRoleClaim_ShouldReturnNull()
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(tokenDescriptor);
        var tokenString = handler.WriteToken(token);

        var role = JwtHelper.GetUserRoleFromToken(tokenString);

        Assert.Null(role);
    }

    [Fact]
    public void GetUserRoleFromToken_WithMalformedToken_ShouldReturnNull()
    {
        var token = "not.valid.token";

        var role = JwtHelper.GetUserRoleFromToken(token);

        Assert.Null(role);
    }

    [Fact]
    public void GetUserRoleFromToken_WithInvalidRole_ShouldReturnNull()
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Role, "NOT_A_REAL_ROLE")]),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(tokenDescriptor);
        var tokenString = handler.WriteToken(token);

        var role = JwtHelper.GetUserRoleFromToken(tokenString);

        Assert.Null(role);
    }

    private static string GenerateTokenWithCustomIssuer(string issuer)
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                    new Claim(JwtRegisteredClaimNames.Sub, validUserId.ToString()),
                    new Claim("email", validEmail),
                    new Claim(ClaimTypes.Role, validRole.ToString())
                ]),
            Issuer = issuer,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }
}

