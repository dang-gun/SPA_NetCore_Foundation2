using DGAuthServer;
using DGAuthServer.Models;
using DGAuthServer.ModelsDB;
using DGAuthServer_Sample.Global;
using DGAuthServer_Sample.Models;
using EfMultiMigrations.Models;
using Microsoft.EntityFrameworkCore;
using ModelsDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Data;

namespace DGAuthServer_Sample;


/// <summary>
/// 
/// </summary>
public class Startup
{
	/// <summary>
	/// 
	/// </summary>
	public IConfiguration Configuration { get; }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="configuration"></param>
	/// <param name="env"></param>
	public Startup(IConfiguration configuration, IHostEnvironment env)
	{
		//전달받은 'appsettings.json'백업
		Configuration = configuration;

		//DB 정보 읽기 ******************
		//DB정보 받으려는 타겟
		string sConnectStringSelect = "Test_sqlite";
		//string sConnectStringSelect = "Test_mssql";

		//사용하려는 DB타입
		GlobalStatic.DBType_String = Configuration[sConnectStringSelect + ":DBType"].ToLower();
		switch (GlobalStatic.DBType_String)
		{
			case "sqlite":
				GlobalStatic.DBType = DGAuthDbType.Sqlite;
				break;
			case "mssql":
				GlobalStatic.DBType = DGAuthDbType.Mssql;
				break;

			default://기본
				GlobalStatic.DBType = DGAuthDbType.Memory;
				break;
		}
		//DB 커낵션
		GlobalStatic.DBString = Configuration[sConnectStringSelect + ":ConnectionString"];


		//깃에 DB정보가 올라가지 않도록 'SettingInfo_gitignore.json'을 읽어서 사용합니다
		//실서비스에 사용할때는 이 블록을 지우고 사용해야 합니다.
		{
			//설정 파일 읽기
			string sJson = File.ReadAllText("SettingInfo_gitignore.json");
			SettingInfoModel? loadSetting = JsonConvert.DeserializeObject<SettingInfoModel>(sJson);

			switch (GlobalStatic.DBType_String)
			{
				case "sqlite":
					GlobalStatic.DBString = loadSetting!.ConnectionString_Sqlite;
					break;
				case "mssql":
					GlobalStatic.DBString = loadSetting!.ConnectionString_Mssql;
					break;
			}
		}



		//db 마이그레이션 적용
		switch (GlobalStatic.DBType)
		{
			case DGAuthDbType.Sqlite:
				using (ModelsDbContext_Sqlite db1 = new ModelsDbContext_Sqlite())
				{
					//db1.Database.EnsureCreated();
					db1.Database.Migrate();
				}
				break;
			case DGAuthDbType.Mssql:
				using (ModelsDbContext_Mssql db1 = new ModelsDbContext_Mssql())
				{
					//db1.Database.EnsureCreated();
					db1.Database.Migrate();
				}
				break;

			default://기본
				using (ModelsDbContext db1 = new ModelsDbContext())
				{
					//db1.Database.EnsureCreated();
					db1.Database.Migrate();
				}
				break;
		}
	}

	/// <summary>
	/// This method gets called by the runtime. Use this method to add services to the container.
	/// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
	/// </summary>
	/// <param name="services"></param>
	public void ConfigureServices(IServiceCollection services)
	{
		//API모델을 파스칼 케이스 유지하기
		services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.ContractResolver = new DefaultContractResolver(); });

		//DGAuthServer Setting 정보 전달
		//services.Configure<DgJwtAuthSettingModel>(Configuration.GetSection("JwtSecretSetting"));

		//사용할 DB 알림
		Action<DbContextOptionsBuilder>? dbContextOptionsBuilder = null;
		switch (GlobalStatic.DBType)
		{
			case DGAuthDbType.Sqlite:
				dbContextOptionsBuilder
					= (options => options.UseSqlite(GlobalStatic.DBString));
				break;
			case DGAuthDbType.Mssql:
				dbContextOptionsBuilder
					= (options => options.UseSqlServer(GlobalStatic.DBString));
				break;
		}

		//DGAuthServer Setting 정보 전달
		services.AddDgAuthServerBuilder(
			new DgAuthSettingModel()
			{
				DbType = GlobalStatic.DBType,

				Secret = Configuration["DgAuthServerSetting:Secret"],
				//개인 시크릿 허용
				SecretAlone = true,

				//테스트를 위해 60초로 설정
				AccessTokenLifetime = 60,

				//쿠키 의존 샘플이므로 쿠키사용 필수
				AccessTokenCookie = true,
				RefreshTokenCookie = true,

				//메모리 캐쉬 사용 허용
				MemoryCacheIs = true,
			}
			, dbContextOptionsBuilder);


		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();
	}

	/// <summary>
	/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	/// </summary>
	/// <param name="app"></param>
	/// <param name="env"></param>
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();

			//스웨거 사용
			//app.UseSwagger();
			//app.UseSwaggerUI();
		}

		//DGAuthServerService 빌더
		app.UseDgAuthServerAppBuilder();

		//스웨거 사용
		app.UseSwagger();
		app.UseSwaggerUI();

		//3.0 api 라우트
		app.UseRouting();

		//기본 페이지
		app.UseDefaultFiles();
		//wwwroot
		app.UseStaticFiles();


		//3.0 api 라우트 끝점
		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});
	}
}

