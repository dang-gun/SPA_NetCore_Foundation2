using DGAuthServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DGAuthServer;

public static class DGAuthServerGlobal
{
	/// <summary>
	/// 사용할 옵션
	/// </summary>
	public static DgAuthSettingModel Setting 
		= new DgAuthSettingModel();

	/// <summary>
	/// 사용할 서비스 개체
	/// </summary>
	/// <remarks>
	/// 서비스에 주입할것인가 말것인가에대한 고민을 많이하고 주입하지 않는것으로 결론을 내렸다.<br />
	/// 서비스를 주입하면 가져다 쓰는데 너무 많은 코드를 써야하는데
	/// 이 서비스는 인증관련이고 사실상 싱글톤으로 구현해도 스택틱과 동일하게 동작하게 된다.
	/// </remarks>
	public static DGAuthServerService Service
		= new DGAuthServerService();

	/// <summary>
	/// 마지막으로 디비를 정리한 시간
	/// </summary>
	public static DateTime DbClearTime = DateTime.Now;
	/// <summary>
	/// 다음 정리 예정시간
	/// </summary>
	public static DateTime DbClearExpectedTime = DateTime.Now;

	/// <summary>
	/// DB 컨택스트의 OnConfiguring이벤트에 사용될 액션
	/// </summary>
	public static Action<DbContextOptionsBuilder>? ActDbContextOnConfiguring = null;

	/// <summary>
	/// 메모리 캐쉬 사용시 개체
	/// </summary>
	public static IMemoryCache? MemoryCache;

}
