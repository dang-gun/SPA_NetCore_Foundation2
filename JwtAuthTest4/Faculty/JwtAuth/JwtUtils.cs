namespace JwtAuth;

using JwtAuth.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModelsDB;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public interface IJwtUtils
{
    public string GenerateJwtToken(User account);
    public int? ValidateJwtToken(string token);
}

/// <summary>
/// https://jasonwatmore.com/post/2022/01/24/net-6-jwt-authentication-with-refresh-tokens-tutorial-with-example-api#running-angular
/// </summary>
public class JwtUtils : IJwtUtils
{
    /// <summary>
    /// ������ ���� ����
    /// </summary>
    private readonly JwtAuthSettingModel _JwtAuthSetting;

    public JwtUtils(IOptions<JwtAuthSettingModel> appSettings)
    {
        _JwtAuthSetting = appSettings.Value;

        if (_JwtAuthSetting.Secret == null 
            || _JwtAuthSetting.Secret == string.Empty)
        {//��ũ�� ���� ����.

            //���� �����Ѵ�.
            _JwtAuthSetting.Secret 
                = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }

    /// <summary>
    /// ������ ��ū ����
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    public string GenerateJwtToken(User account)
    {
        // generate token that is valid for 15 minutes
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_JwtAuthSetting.Secret!);
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
    /// ������ ��ū Ȯ��.
    /// </summary>
    /// <remarks>�̵������� ȣ���ؼ� ����Ѵ�.</remarks>
    /// <param name="token"></param>
    /// <returns>ã�Ƴ� idUser</returns>
    public int? ValidateJwtToken(string token)
    {
        if (token == null)
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_JwtAuthSetting.Secret!);
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
    /// ���÷��� ��ū ����.
    /// </summary>
    /// <remarks>�ߺ��˻�� ���� �����Ƿ� �ʿ��ϴٸ� ȣ�����ʿ��� �ߺ��˻縦 �ؾ� �Ѵ�.</remarks>
    /// <returns></returns>
    public UserRefreshToken GenerateRefreshToken()
    {
        var refreshToken = new UserRefreshToken
        {
            //�����ϰ� �� ����
            RefreshToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
            //������ �ð�(��)��ŭ �ð��� �����Ѵ�.
            ExpiresTime = DateTime.UtcNow.AddSeconds(this._JwtAuthSetting.RefreshTokenLifetime),
        };
            
        return refreshToken;
    }
}