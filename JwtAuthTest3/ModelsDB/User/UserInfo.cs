using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ModelsDB
{
	/// <summary>
	/// 유저 상세 정보
	/// </summary>
	public class UserInfo
	{
		/// <summary>
		/// 유저 상세 정보 고유키
		/// </summary>
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int idUserInfo { get; set; }

		/// <summary>
		/// 연결된 유저의 고유키
		/// </summary>
		public int idUser { get; set; }

		/// <summary>
		/// 화면에 보여질 이름
		/// </summary>
		public string ViewName { get; set; } = string.Empty;


		/// <summary>
		/// 계정 생성 날짜
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// 계정 정보 마지막 업데이트 날짜
		/// </summary>
		public DateTime? UpdateLast { get; set; }

	}
}
