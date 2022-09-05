using ModelsDB;

namespace AspNetCore6React.Global;

public static class GlobalStatic
{
	/// <summary>
	/// DB 타입
	/// </summary>
	/// <remarks>저장전에 소문자로 변환해야 한다.</remarks>
	public static string DBType = "";
	/// <summary>
	/// DB 컨낵션 스트링 저장
	/// </summary>
	public static string DBString = "";

	/// <summary>
	/// 사인인 가능한 유저 정보
	/// </summary>
	public static List<User> Users;

	static GlobalStatic()
	{
		Users = new List<User>();
		Users.Add(new User() { idUser = 1, SignName = "test01", PasswordHash = "1111" });
		Users.Add(new User() { idUser = 2, SignName = "test02", PasswordHash = "1111" });
		Users.Add(new User() { idUser = 3, SignName = "test03", PasswordHash = "1111" });
	}
}
