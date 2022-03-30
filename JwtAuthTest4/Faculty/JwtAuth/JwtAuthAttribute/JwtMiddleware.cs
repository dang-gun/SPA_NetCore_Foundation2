namespace WebApi.Authorization;

using Microsoft.Extensions.Options;
using ModelsDB;
using System.Security.Claims;
using WebApi.Helpers;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AppSettings _appSettings;

    public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
    {
        _next = next;
        _appSettings = appSettings.Value;
    }

    public async Task Invoke(
        HttpContext context
        , IJwtUtils jwtUtils)
    {
        //��ū ����
        string? token 
            = context.Request
                .Headers["Authorization"]
                .FirstOrDefault()?
                .Split(" ")
                .Last();

        if (null != token)
        {//��ū�� �ִ�.

            //��ū���� idUser ����
            int? idUser = jwtUtils.ValidateJwtToken(token);


            if (idUser != null && 0 < idUser)
            {//����� ���̵� �ִ�.

                //������ ��ū�� �����Ͱ� ������ Ŭ���ӵ����͸� �߰��� �ش�.
                var claims = new List<Claim>
                {
                    new Claim("idUser", idUser.ToString()!)
                };

                //HttpContext�� Ŭ���� ������ �־��ش�.
                ClaimsIdentity appIdentity = new ClaimsIdentity(claims);
                context.User.AddIdentity(appIdentity);
            }
        }

        

        await _next(context);
    }
}