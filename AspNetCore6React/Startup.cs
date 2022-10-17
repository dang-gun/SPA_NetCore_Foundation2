﻿using AspNetCore6React.Global;
using AspNetCore6React.Models;
using DGAuthServer;
using DGAuthServer.Models;
using DGAuthServer.ModelsDB;
using EfMultiMigrations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using ModelsDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AspNetCore6React;

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
	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;

		//DB 정보 읽기 ******************
		//DB정보 받으려는 타겟
		string sConnectStringSelect = "Test_sqlite";

		//사용하려는 DB타입
		switch (configuration[sConnectStringSelect + ":DBType"].ToLower())
		{
			case "sqlite":
				GlobalStatic.DBType = UseDbType.Sqlite;
				break;
			case "mssql":
				GlobalStatic.DBType = UseDbType.Mssql;
				break;

			default://기본
				GlobalStatic.DBType = UseDbType.Memory;
				break;
		}

		//DB 커낵션
		GlobalStatic.DBString = configuration[sConnectStringSelect + ":ConnectionString"];

		//깃에 DB정보가 올라가지 않도록 'SettingInfo_gitignore.json'을 읽어서 사용합니다
		//실서비스에 사용할때는 이 블록을 지우고 사용해야 합니다.
		{
			//설정 파일 읽기
			string sJson = File.ReadAllText("SettingInfo_gitignore.json");
			SettingInfoModel? loadSetting = JsonConvert.DeserializeObject<SettingInfoModel>(sJson);

			switch (GlobalStatic.DBType)
			{
				case UseDbType.Sqlite:
					GlobalStatic.DBString = loadSetting!.ConnectionString_Sqlite;
					break;
				case UseDbType.Mssql:
					GlobalStatic.DBString = loadSetting!.ConnectionString_Mssql;
					break;
			}
		}

		//db 마이그레이션 적용
		switch (GlobalStatic.DBType)
		{
			case UseDbType.Sqlite:
				using (ModelsDbContext_Sqlite db1 = new ModelsDbContext_Sqlite())
				{
					//db1.Database.EnsureCreated();
					db1.Database.Migrate();
				}
				break;
			case UseDbType.Mssql:
				using (ModelsDbContext_Mssql db1 = new ModelsDbContext_Mssql())
				{
					//db1.Database.EnsureCreated();
					db1.Database.Migrate();
				}
				break;

			default://기본
				using (DgAuthDbContext db1 = new DgAuthDbContext())
				{
					//db1.Database.EnsureCreated();
					db1.Database.Migrate();
				}
				break;
		}
	}

	/// <summary>
	/// This method gets called by the runtime. Use this method to add services to the container.
	/// </summary>
	/// <param name="services"></param>
	public void ConfigureServices(IServiceCollection services)
	{
		//API모델을 파스칼 케이스 유지하기
		services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.ContractResolver = new DefaultContractResolver(); });

		//DGAuthServer Setting 정보 전달
		//services.Configure<DgJwtAuthSettingModel>(Configuration.GetSection("JwtSecretSetting"));

		//사용할 DB 알림
		DGAuthDbType typeDgAuthServerDb = DGAuthDbType.Memory;
		switch (GlobalStatic.DBType)
		{
			case UseDbType.Sqlite:
				typeDgAuthServerDb = DGAuthDbType.Sqlite;
				break;
			case UseDbType.Mssql:
				typeDgAuthServerDb = DGAuthDbType.Mssql;
				break;
		}

		services.AddDgAuthServerBuilder(
			new DgAuthSettingModel()
			{
				Secret = this.Configuration["JwtSecretSetting:Secret"],
				//개인 시크릿 허용
				SecretAlone = false,

				//테스트를 위해 60초로 설정
				AccessTokenLifetime = 60,
				AccessTokenCookie = true,
				RefreshTokenCookie = true,

				DbType = typeDgAuthServerDb,

				//메모리 캐쉬 사용 허용
				MemoryCacheIs = true,
			}
			, (options => options.UseSqlite(GlobalStatic.DBString)));
		//, null);

		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();

	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="app"></param>
	/// <param name="env"></param>
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		
		// Configure the HTTP request pipeline.
		if (env.IsDevelopment())
		{//개발 버전에서만 스웨거 사용
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		//DGAuthServerService 빌더
		app.UseDgAuthServerAppBuilder();

		//3.0 api 라우트
		app.UseRouting();
		//https로 자동 리디렉션
		app.UseHttpsRedirection();

		//기본 페이지
		app.UseDefaultFiles();
		//wwwroot
		app.UseStaticFiles();


		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});
	}
}
