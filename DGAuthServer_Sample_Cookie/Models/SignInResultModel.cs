using ApiResult;

namespace DGAuthServer_Sample.Models;

/// <summary>
/// 사인인이 성공하였을때 전달되는 정보(자바스크립트 전달용)
/// </summary>
public class SignInResultModel: ApiResultBaseModel
{

    /// <summary>
    /// 유저 아이디
    /// </summary>
    public long idUser { get; set; } = 0;

    /// <summary>
    /// 엑세스 토큰
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    /// <summary>
    /// 리플레시 토큰
    /// </summary>
		public string RefreshToken { get; set; } = string.Empty;
	}
