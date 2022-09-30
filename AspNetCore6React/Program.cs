using AspNetCore6React.Global;
using AspNetCore6React.Models;
using DGAuthServer;
using DGAuthServer.Models;
using DGAuthServer.ModelsDB;
using EfMultiMigrations.Models;
using Microsoft.EntityFrameworkCore;
using ModelsDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region Startup
//appsettings.json
IConfiguration configuration = builder.Configuration;


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

#endregion

#region ConfigureServices
//API모델을 파스칼 케이스 유지하기
builder.Services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.ContractResolver = new DefaultContractResolver(); });

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

builder.Services.AddDgAuthServerBuilder(
	new DgAuthSettingModel()
	{
		Secret = configuration["JwtSecretSetting:Secret"],
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion


#region Configure
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{//개발 버전에서만 스웨거 사용
	app.UseSwagger();
	app.UseSwaggerUI();
}

//3.0 api 라우트
app.UseRouting();
//https로 자동 리디렉션
app.UseHttpsRedirection();

//기본 페이지
app.UseDefaultFiles();
//wwwroot
app.UseStaticFiles();


app.MapControllers();

app.Run();
#endregion
