namespace ApiResult;

/// <summary>
/// API 결과 공통 베이스.
/// </summary>
public class ApiResultBaseModel
{
	/// <summary>
	/// 실패시 전달한 코드
	/// 0 : 성공.
	/// 다른 값은 모두 실패
	/// </summary>
	public string InfoCode { get; set; } = String.Empty;
	/// <summary>
	/// 전달할 메시지
	/// </summary>
	public string Message { get; set; } = String.Empty;

	/// <summary>
	/// 성공했을때 문자
	/// </summary>
	private string SuccessString = String.Empty;

	/// <summary>
	/// 기본 생성.
	/// InfoCode가 "0"로 초기화됨
	/// </summary>
	public ApiResultBaseModel()
	{
		this.SuccessString = ApiResultType.None.GetHashCode().ToString();

		this.Reset();
	}

	/// <summary>
	/// 인포코드와 메시지를 넣고 생성
	/// </summary>
	/// <param name="sInfoCode"></param>
	/// <param name="sMessage"></param>
	public ApiResultBaseModel(string sInfoCode, string sMessage)
	{
		this.SuccessString = ApiResultType.None.GetHashCode().ToString();

		this.InfoCode = sInfoCode;
		this.Message = sMessage;
	}

	/// <summary>
	/// 성공으로 초기화한다.
	/// </summary>
	public void Reset()
	{
		this.InfoCode = this.SuccessString;
		this.Message = string.Empty;
	}

	/// <summary>
	/// 타입 세팅
	/// </summary>
	/// <param name="typeApiResult"></param>
	public void TypeSet(ApiResultType typeApiResult)
	{
		this.InfoCode = typeApiResult.GetHashCode().ToString();
	}

	/// <summary>
	/// InfoCode값이 성공값인지 여부
	/// </summary>
	/// <returns></returns>
	public bool IsSuccess()
	{
		bool bReturn = false;

		if (this.InfoCode == this.SuccessString)
		{//성공문자열이다.
			bReturn = true;
		}

		return bReturn;

	}
}