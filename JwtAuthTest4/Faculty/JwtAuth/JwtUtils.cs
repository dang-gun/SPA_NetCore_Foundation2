namespace WebApi.Authorization;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModelsDB;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApi.Helpers;

public interface IJwtUtils
{
    public string GenerateJwtToken(User account);
    public int? ValidateJwtToken(string token);
}

public class JwtUtils : IJwtUtils
{
    private readonly ModelsDbContext _context;
    private readonly AppSettings _appSettings;

    public JwtUtils(
        ModelsDbContext context,
        IOptions<AppSettings> appSettings)
    {
        _context = context;
        _appSettings = appSettings.Value;
    }

    /// <summary>
    /// 엑세스 토큰 생성
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    public string GenerateJwtToken(User account)
    {
        // generate token that is valid for 15 minutes
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("idUser", account.idUser.ToString()) }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// 엑세스 토큰 확인.
    /// </summary>
    /// <remarks>미들웨어에서도 호출해서 사용한다.</remarks>
    /// <param name="token"></param>
    /// <returns>찾아낸 idUser</returns>
    public int? ValidateJwtToken(string token)
    {
        if (token == null)
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "idUser").Value);

            // return account id from JWT token if validation successful
            return accountId;
        }
        catch
        {
            // return null if validation fails
            return null;
        }
    }

    /// <summary>
    /// 리플레시 토큰 생성
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    public UserRefreshToken GenerateRefreshToken(string ipAddress)
    {
        var refreshToken = new UserRefreshToken
        {
            // token is a cryptographically strong random sequence of values
            Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
            // token is valid for 7 days
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        // ensure token is unique by checking against db
        UserRefreshToken? findURT 
            = _context.UserRefreshToken
                .FirstOrDefault(w=> w.RefreshToken == refreshToken.Token);

        if (null != findURT)
        {//생성된 값이 중복되었다면 다시 생성한다.
            return GenerateRefreshToken(ipAddress);
        }
            
        return refreshToken;
    }
}