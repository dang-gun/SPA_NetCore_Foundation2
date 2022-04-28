namespace DGAuthServer.Models;

/// <summary>
/// 리플레시 토큰을 어떤식으로 발급(재발급) 할지 타입
/// </summary>
public enum RefreshTokenUsageType
{
	/// <summary>
	/// 설정 없음
	/// </summary>
	None,

	/// <summary>
	/// 한번만 사용
	/// </summary>
	OneTimeOnly,
	/// <summary>
	/// 한번만 사용 - 딜레이
	/// </summary>
	/// <remarks>
	/// 빠르게 엑세스토큰을 갱신하는경우(예> 0.001초에 3번) OneTimeOnly옵션을 사용하면
	/// 각 요청별로 리플래시토큰이 새로 발급되서 인증이 꼬이는 문제가 발생한다.<br />
	/// 이 옵션을 사용하면 세팅에 저장된 시간동안은 마지막으로 생성된 리플레시 토큰을 전달하게 되어
	/// 빠른 갱신에 대응할 수 있게된다.
	/// </remarks>
	OneTimeOnlyDelay,

	/// <summary>
	/// 다시 사용 : 만료기간은 그대로 둠
	/// </summary>
	ReUse,
	/// <summary>
	/// 다시 사용 : 만료기간은 옵션에 맞게 늘림
	/// </summary>
	ReUseAddTime,

}
