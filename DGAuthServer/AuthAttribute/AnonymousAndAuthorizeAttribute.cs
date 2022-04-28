using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace DGAuthServer.AuthAttribute;

/// <summary>
/// �͸��� ����ϰ� ���������� �������� �������� ��ȿ�� �˻縦 �Ѵ�.
/// </summary>
/// <remarks>�� �Ӽ��� ����ϸ� ���������� �������� AllowAnonymousAttribute����
/// ���������� �������� AuthorizeAttributeó�� �۵��Ѵ�.<br />
/// ������ �� �Ӽ� ��ü�� ���÷��� ��ūó���� ���� �����Ƿ�
/// ���÷��� ��ū�� �ִµ� ��������ū�� ���� ��Ȳ�� ó������ ���Ѵ�.
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


        //1�̻��϶��� �������� ��ū�� ���� ���̴�.
        //�� �Ӽ��� -1(�͸�ó��)�϶��� �͸�ó���� ����Ѵ�.
        //�׷��� 0(��ū������ �߸���)�� ó���� �Ѵ�.
        if (0 == nUser)
        {//���������� �߸��Ǿ���.

            //���������� �ִµ� �߸��Ǿ���.
            //401����
            context.Result
                = new JsonResult(new { message = "Unauthorized" })
                { StatusCode = StatusCodes.Status401Unauthorized };
        }

    }
}