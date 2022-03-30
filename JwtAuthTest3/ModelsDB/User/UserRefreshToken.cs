using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ModelsDB
{
	/// <summary>
	/// 유저 리플레시 토큰
	/// </summary>
	public class UserRefreshToken
	{
		/// <summary>
		/// 유저 리플레시 토큰 고유키
		/// </summary>
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int idUserRefreshToken { get; set; }

		/// <summary>
		/// 연결된 유저의 고유키
		/// </summary>
		public int idUser { get; set; }

		/// <summary>
		/// 리플레시 토큰
		/// </summary>
		public string RefreshToken { get; set; } = string.Empty;

		/// <summary>
		/// 이 토큰이 취소 됐는지 여부
		/// </summary>
		public bool RevokeIs { get; set; } = false;

		/// <summary>
		/// 만료 예정 시간
		/// </summary>
		public DateTime ExpiresTime { get; set; }

		/// <summary>
		/// 실제 취소된 시간
		/// </summary>
		public DateTime? RevokeTime { get; set; }
	}
}
