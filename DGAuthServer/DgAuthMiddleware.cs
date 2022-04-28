
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DGAuthServer;

/// <summary>
/// ���� �̵����
/// </summary>
/// <remarks>��</remarks>
public class DgAuthMiddleware
{
    private readonly RequestDelegate _next;

    public DgAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// ���� ���޿� �̵� ����
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
        //����� ��ū
        string sToken = string.Empty;
        //��ū�� �ִ��� ����
        bool bToken = false;

        if (string.Empty == DGAuthServerGlobal.Setting.AuthTokenStartName)
        {//���� ��ū ���� �ܾ ������� �ʴ´�.

            //��ū ����
            string? token2
                = context.Request
                    .Headers[DGAuthServerGlobal.Setting.AuthHeaderName]
                    .FirstOrDefault();

            if (null != token2
                && string.Empty != token2)
            {//��ū ������ �ִ�.
                bToken = true;
                sToken = token2!;
            }
        }
        else
        {//���� ��ū ���� �ܾ ����Ѵ�.

            //��ū�� ������ ���⼭ 2�� �̻� ���´�.
            string[]? arrToken
                = context.Request
                    .Headers[DGAuthServerGlobal.Setting.AuthHeaderName]
                    .FirstOrDefault()?
                    .Split(DGAuthServerGlobal.Setting.AuthTokenStartName_Complete);

            if (null != arrToken
                && 1 < arrToken.Length)
            {//��ū ������ �ִ�.
                bToken = true;
                sToken = arrToken.Last();
            }
        }


        //��ū ������ �������� �Ӽ��� �����ϱ����� �� ó���� �Ѵ�.
        long idUser = 0;
        if (true == bToken)
        {//��ū�� �ִ�.

            //��ū���� idUser ����
            idUser 
                = DGAuthServerGlobal.Service
                    .AccessTokenValidate(
                        sToken
                        , string.Empty
                        , context.Request);
        }
        else
        {//��ū ����
            idUser = -1;
        }

        //ó���� ��ū ������ �����Ѵ�.
        //������ ��ū�� �����Ͱ� ������ Ŭ���ӵ����͸� �߰��� �ش�.
        var claims
            = new List<Claim>
                {
                        new Claim(DGAuthServerGlobal.Setting.UserIdName
                                    , idUser.ToString())
                };

        //HttpContext�� Ŭ���� ������ �־��ش�.
        ClaimsIdentity appIdentity = new ClaimsIdentity(claims);
        context.User.AddIdentity(appIdentity);



        await _next(context);
    }
}