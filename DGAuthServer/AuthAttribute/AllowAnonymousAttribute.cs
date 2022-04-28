
namespace DGAuthServer.AuthAttribute;

/// <summary>
/// 익명 허용 == 인증 스킵 속성
/// </summary>
/// <remarks>이 속성을 사용하면 AuthorizeAttribute는 스킵되게 된다<br />
/// 하지만 JwtMiddleware에서 토큰을 해석하므로 인증된 유저라면 Claim에 idUser가 들어간다. </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowAnonymousAttribute : Attribute
{ }