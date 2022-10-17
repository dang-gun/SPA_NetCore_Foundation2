using ApiResult;
using ModelsDB;

namespace SPA_NetCore_Foundation2_01.Models;

/// <summary>
/// 사인인이 성공하였을때 전달되는 정보(자바스크립트 전달용)
/// </summary>
	public class SignInfoResultModel : ApiResultBaseModel
	{
    /// <summary>
    /// 성공여부
    /// </summary>
    public bool Complete { get; set; } = false;
    
    /// <summary>
    /// 검색된 유저 정보
    /// </summary>
    public List<User>? UserInfo { get; set; }

}
