namespace JwtAuth;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public delegate bool GetDbUniqueTokenDelegate(string sNewRefreshToken);

public interface IJwtUtils
{
    public string GenerateJwtToken(long idUser);
    public int? ValidateJwtToken(string token);
    public RefreshTokenModels GenerateRefreshToken(string ipAddress, GetDbUniqueTokenDelegate getDbUniqueTokenDelegate);
}

public class JwtUtils : IJwtUtils
{
    
    /// <summary>
    /// 암호용 키
    /// </summary>
    private readonly string m_sSecret;

    public JwtUtils(string sSecret)
    {
        this.m_sSecret = sSecret;
    }

    /// <summary>
    /// 액세스 토큰 생성
    /// </summary>
    /// <param name="idUser"></param>
    /// <returns></returns>
    public string GenerateJwtToken(long idUser)
    {
        // generate token that is valid for 15 minutes
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(this.m_sSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", idUser.ToString()) }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public int? ValidateJwtToken(string token)
    {
        if (token == null)
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(this.m_sSecret);
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
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

            // return user id from JWT token if validation successful
            return userId;
        }
        catch
        {
            // return null if validation fails
            return null;
        }
    }

    

    /// <summary>
    /// 리플래시 토큰 생성
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    public RefreshTokenModels GenerateRefreshToken(
        string ipAddress
        , GetDbUniqueTokenDelegate getDbUniqueTokenDelegate)
    {
        var refreshToken = new RefreshTokenModels
        {
            Token = getUniqueToken(),
            // token is valid for 7 days
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        return refreshToken;

        string getUniqueToken()
        {
            // token is a cryptographically strong random sequence of values
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            // ensure token is unique by checking against db
            //var tokenIsUnique = !_context.Users.Any(u => u.RefreshTokens.Any(t => t.Token == token));



            if (false == getDbUniqueTokenDelegate(token))
            {//생성된 토큰이 이미 있다.

                //생성된 토큰이 없을때까지 반복
                return getUniqueToken();
            }
                
            
            return token;
        }
    }
}