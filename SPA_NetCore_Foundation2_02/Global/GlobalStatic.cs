using SPA_NetCore_Foundation2.Models;
using ModelsDB;

namespace SPA_NetCore_Foundation2.Global;

/// <summary>
/// 프로그램 전역 변수
/// </summary>
public static class GlobalStatic
{
	/// <summary>
	/// DB 타입
	/// </summary>
	public static UseDbType DBType = UseDbType.Memory;
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
