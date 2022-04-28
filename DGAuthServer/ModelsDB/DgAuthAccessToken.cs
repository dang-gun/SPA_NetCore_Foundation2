using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DGAuthServer.ModelsDB;

/// <summary>
/// 엑세스 토큰
/// </summary>
public class DgAuthAccessToken
{

	/// <summary>
	/// 엑세스 토큰 고유키
	/// </summary>
	/// <remarks>이 값을 가지고 외부의 유저와 매칭시킨다.</remarks>
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int idDgAuthAccessToken { get; set; }

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
	/// 이 유저가 사용중인 시크릿 코드
	/// <para>이 값이 바뀌면 기존 엑세스토큰은 만료된다.</para>
	/// </summary>
	public string Secret { get; set; } = string.Empty;
	
}
