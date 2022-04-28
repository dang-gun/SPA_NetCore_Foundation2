

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace DGAuthServer.AuthAttribute;

/// <summary>
/// 인증 필수 속성
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>
    /// 인증요청이 왔다.
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
        {//AllowAnonymous으로 설정되어 있다.

            //인증을 스킵한다.
            return;
        }
            

        //인증정보 확인
        long nUser = 0;
        Claim? findClaim 
            = context.HttpContext
                    .User
                    .Claims
                    .Where(w => w.Type == "idUser")
                    .FirstOrDefault();

        if (null != findClaim)
        {//인증정보가 있다.
            nUser = Int64.Parse(findClaim.Value);
        }


        if (0 >= nUser)
        {//인증정보가 없다.
            
            //401에러
            context.Result 
                = new JsonResult(new { message = "Unauthorized" }) 
                        { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}