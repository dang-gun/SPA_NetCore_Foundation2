using System;
using System.Diagnostics;
using DGAuthServer.Models;
using EfMultiMigrations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DGAuthServer.ModelsDB;


public class DgAuthDbContext_Sqlite : DgAuthDbContext
{
	public DgAuthDbContext_Sqlite(DbContextOptions<DgAuthDbContext> options)
			: base(options)
	{
	}

	public DgAuthDbContext_Sqlite()
	{
	}
}


public class DgAuthDbContext_SqliteFactory : IDesignTimeDbContextFactory<DgAuthDbContext_Sqlite>
{
	public DgAuthDbContext_Sqlite CreateDbContext(string[] args)
	{
		DbContextOptionsBuilder<DgAuthDbContext> optionsBuilder
			= new DbContextOptionsBuilder<DgAuthDbContext>();

		//설정 파일 읽기
		string sJson = File.ReadAllText("SettingInfo_gitignore.json");
		SettingInfoModel? loadSetting = JsonConvert.DeserializeObject<SettingInfoModel>(sJson);

		//Add-Migration InitialCreate -Context DgAuthDbContext_Sqlite -OutputDir Migrations/Sqlite
		DGAuthServerGlobal.DbType = DbType.Sqlite;
		DGAuthServerGlobal.DbConnectString = loadSetting!.ConnectionString_Sqlite;

		return new DgAuthDbContext_Sqlite(optionsBuilder.Options);
	}
}
