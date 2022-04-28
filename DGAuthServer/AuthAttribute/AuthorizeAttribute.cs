

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace DGAuthServer.AuthAttribute;

/// <summary>
/// ���� �ʼ� �Ӽ�
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>
    /// ������û�� �Դ�.
    /// </summary>
    /// <param name="context"></param>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        //
        bool bAllowAnonymous 
            = context.ActionDescriptor
                    .EndpointMetadata
                    .OfType<AllowAnonymousAttribute>().Any();
        if (true == bAllowAnonymous)
        {//AllowAnonymous���� �����Ǿ� �ִ�.

            //������ ��ŵ�Ѵ�.
            return;
        }
            

        //�������� Ȯ��
        long nUser = 0;
        Claim? findClaim 
            = context.HttpContext
                    .User
                    .Claims
                    .Where(w => w.Type == "idUser")
                    .FirstOrDefault();

        if (null != findClaim)
        {//���������� �ִ�.
            nUser = Int64.Parse(findClaim.Value);
        }


        if (0 >= nUser)
        {//���������� ����.
            
            //401����
            context.Result 
                = new JsonResult(new { message = "Unauthorized" }) 
                        { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}