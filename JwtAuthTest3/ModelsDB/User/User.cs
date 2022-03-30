using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ModelsDB
{
	/// <summary>
	/// 유저 사인인 정보
	/// </summary>
	public class User
	{
		/// <summary>
		/// 유저 고유키
		/// </summary>
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int idUser { get; set; }
		
		/// <summary>
		/// 사인인에 사용되는 이름
		/// </summary>
		/// <remarks>프로젝트에따라 이것이 이름, 이메일 등의 다양한 값이 될 수 있으므로
		/// 네이밍을 이렇게 한다.</remarks>
		public string SignName { get; set; } = string.Empty;


		/// <summary>
		/// 단방향 암호화가된 비밀번호
		/// </summary>
		[JsonIgnore]
		public string PasswordHash { get; set; } = string.Empty;


	}
}
