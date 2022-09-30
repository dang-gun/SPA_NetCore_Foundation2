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


//DB ���� �б� ******************
//DB���� �������� Ÿ��
string sConnectStringSelect = "Test_sqlite";

//����Ϸ��� DBŸ��
switch (configuration[sConnectStringSelect + ":DBType"].ToLower())
{
	case "sqlite":
		GlobalStatic.DBType = UseDbType.Sqlite;
		break;
	case "mssql":
		GlobalStatic.DBType = UseDbType.Mssql;
		break;

	default://�⺻
		GlobalStatic.DBType = UseDbType.Memory;
		break;
}

//DB Ŀ����
GlobalStatic.DBString = configuration[sConnectStringSelect + ":ConnectionString"];

//�꿡 DB������ �ö��� �ʵ��� 'SettingInfo_gitignore.json'�� �о ����մϴ�
//�Ǽ��񽺿� ����Ҷ��� �� ����� ����� ����ؾ� �մϴ�.
{
	//���� ���� �б�
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

//db ���̱׷��̼� ����
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

	default://�⺻
		using (DgAuthDbContext db1 = new DgAuthDbContext())
		{
			//db1.Database.EnsureCreated();
			db1.Database.Migrate();
		}
		break;
}

#endregion

#region ConfigureServices
//API���� �Ľ�Į ���̽� �����ϱ�
builder.Services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.ContractResolver = new DefaultContractResolver(); });

//DGAuthServer Setting ���� ����
//services.Configure<DgJwtAuthSettingModel>(Configuration.GetSection("JwtSecretSetting"));

//����� DB �˸�
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
		//���� ��ũ�� ���
		SecretAlone = false,

		//�׽�Ʈ�� ���� 60�ʷ� ����
		AccessTokenLifetime = 60,
		AccessTokenCookie = true,
		RefreshTokenCookie = true,

		DbType = typeDgAuthServerDb,

		//�޸� ĳ�� ��� ���
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
{//���� ���������� ������ ���
	app.UseSwagger();
	app.UseSwaggerUI();
}

//3.0 api ���Ʈ
app.UseRouting();
//https�� �ڵ� ���𷺼�
app.UseHttpsRedirection();

//�⺻ ������
app.UseDefaultFiles();
//wwwroot
app.UseStaticFiles();


app.MapControllers();

app.Run();
#endregion
