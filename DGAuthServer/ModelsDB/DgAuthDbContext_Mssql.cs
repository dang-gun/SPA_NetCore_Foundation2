using System;
using System.Diagnostics;
using DGAuthServer.Models;
using EfMultiMigrations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DGAuthServer.ModelsDB;


public class DgAuthDbContext_Mssql : DgAuthDbContext
{
	public DgAuthDbContext_Mssql(DbContextOptions<DgAuthDbContext> options)
			: base(options)
	{
	}

	public DgAuthDbContext_Mssql()
	{
	}
}


public class DgAuthDbContext_MssqlFactory : IDesignTimeDbContextFactory<DgAuthDbContext_Mssql>
{
	public DgAuthDbContext_Mssql CreateDbContext(string[] args)
	{
		DbContextOptionsBuilder<DgAuthDbContext> optionsBuilder
			= new DbContextOptionsBuilder<DgAuthDbContext>();

		//설정 파일 읽기
		string sJson = File.ReadAllText("SettingInfo_gitignore.json");
		SettingInfoModel? loadSetting = JsonConvert.DeserializeObject<SettingInfoModel>(sJson);

		//Add-Migration InitialCreate -Context DgAuthDbContext_Mssql -OutputDir Migrations/Mssql
		//Update-Database -Context DgAuthDbContext_Mssql -Migration 0
		//"Server=[주소];DataBase=[데이터 베이스];UId=[아이디];pwd=[비밀번호]"
		DGAuthServerGlobal.Setting.DbType = DGAuthDbType.Mssql;
		DGAuthServerGlobal.DbConnectString = loadSetting!.ConnectionString_Mssql;

		return new DgAuthDbContext_Mssql(optionsBuilder.Options);
	}
}
