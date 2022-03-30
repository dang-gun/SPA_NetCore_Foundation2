namespace JwtAuthTest3.Models.Signs
{
	/// <summary>
	/// 사인인 성공시 전달할 모델
	/// </summary>
	public class SignInResultModel
	{
        /// <summary>
        /// 유저의 고유 아이디
        /// </summary>
        public long idUser { get; set; }

        /// <summary>
        /// 화면에 보여질 이름
        /// </summary>
        public string ViewName { get; set; } = string.Empty;
    }
}
