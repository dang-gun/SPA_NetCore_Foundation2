namespace JwtAuth
{
	/// <summary>
	/// 토큰 처리시 정보가 담긴 모델
	/// </summary>
	public class TokenResultModel
	{
		/// <summary>
		/// 에러 여부
		/// </summary>
		public bool ErrorIs { get; set; } = false;
		/// <summary>
		/// 전달할 메시지
		/// </summary>
		public string Message { get; set; } = string.Empty;
	}
}
