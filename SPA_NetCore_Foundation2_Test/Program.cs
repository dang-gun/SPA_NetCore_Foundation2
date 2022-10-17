using SPA_NetCore_Foundation2.Global;
using SPA_NetCore_Foundation2.Models;
using DGAuthServer;
using DGAuthServer.Models;
using DGAuthServer.ModelsDB;

namespace SPA_NetCore_Foundation2;

/// <summary>
/// 
/// </summary>
public class Program
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="args"></param>
	public static void Main(string[] args)
	{
		CreateHostBuilder(args).Build().Run();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="args"></param>
	/// <returns></returns>
	public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.UseStartup<Startup>();
			});
}
