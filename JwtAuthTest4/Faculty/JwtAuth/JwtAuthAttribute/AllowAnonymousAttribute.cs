namespace WebApi.Authorization;

/// <summary>
/// ���� ��ŵ �Ӽ�
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowAnonymousAttribute : Attribute
{ }