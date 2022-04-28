using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DGAuthServer.ModelsDB;

/// <summary>
/// 리플레시 토큰
/// </summary>
public class DgAuthRefreshToken
{
	/// <summary>
	/// 리플레시 토큰 고유키
	/// </summary>
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int idDgAuthRefreshToken { get; set; }

	/// <summary>
	/// 외부에 연결할 유저의 고유키
	/// </summary>
	/// <remarks>이 값을 가지고 외부의 유저와 매칭시킨다.</remarks>
	public long idUser { get; set; }

	/// <summary>
	/// 이 토큰의 분류
	/// </summary>
	/// <remarks>
	/// 엑세스 토큰이 여러개 있는 경우(예> 다중플랫폼 허용, 여러 사이트에서 하나의 인증서버 사용)
	/// 각 토큰을 구분하기위한 구분용 문자열이다.<br />
	/// idUser와 합쳐 복합키처럼 사용해야 한다.
	/// </remarks>
	public string Class { get; set; } = string.Empty;

	/// <summary>
	/// 리플레시 토큰
	/// </summary>
	public string RefreshToken { get; set; } = string.Empty;

	/// <summary>
	/// 생성 시간
	/// </summary>
	public DateTime GenerateTime { get; set; }

	/// <summary>
	/// 만료 예정 시간
	/// </summary>
	public DateTime ExpiresTime { get; set; }

	/// <summary>
	/// 취소된 시간
	/// </summary>
	public DateTime? RevokeTime { get; set; }

	/// <summary>
	/// 생성당시 아이피
	/// </summary>
	public string? IpCreated { get; set; }

	#region 속성
	/// <summary>
	/// 만료 여부
	/// </summary>
	public bool ExpiredIs { get; set; }
	/// <summary>
	/// 취소 여부
	/// </summary>
	public bool RevokeIs { get; set; }
	/// <summary>
	/// 사용가능 여부
	/// </summary>
	public bool ActiveIs { get; set; }

	#endregion

	/// <summary>
	/// 이 토큰의 사용가능여부를 다시 확인한다.
	/// </summary>
	public DgAuthRefreshToken ActiveCheck()
	{
		this.ExpiredIs = DateTime.UtcNow >= this.ExpiresTime;
		this.RevokeIs = RevokeTime != null;
		this.ActiveIs = RevokeTime == null && !ExpiredIs;

		return this;
	}
}
