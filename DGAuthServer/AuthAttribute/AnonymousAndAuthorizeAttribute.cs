using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace DGAuthServer.AuthAttribute;

/// <summary>
/// 익명을 허용하고 인증정보가 있을때는 인증정보 유효성 검사를 한다.
/// </summary>
/// <remarks>이 속성을 사용하면 인증정보가 없을때는 AllowAnonymousAttribute으로
/// 인증정보가 있을때는 AuthorizeAttribute처럼 작동한다.<br />
/// 하지만 이 속성 자체는 리플레시 토큰처리를 하지 안으므로
/// 리플레시 토큰은 있는데 엑세스토큰을 없는 상황은 처리하지 못한다.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AnonymousAndAuthorizeAttribute : Attribute, IAuthorizationFilter
{
	public AnonymousAndAuthorizeAttribute()
	{

	}

    public void OnAuthorization(AuthorizationFilterContext context)
	{

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


        //1이상일때는 정상적인 토큰을 받은 것이다.
        //이 속성은 -1(익명처리)일때는 익명처리를 허용한다.
        //그래서 0(토큰정보가 잘못됨)만 처리를 한다.
        if (0 == nUser)
        {//인증정보가 잘못되었다.

            //인증정보가 있는데 잘못되었다.
            //401에러
            context.Result
                = new JsonResult(new { message = "Unauthorized" })
                { StatusCode = StatusCodes.Status401Unauthorized };
        }

    }
}