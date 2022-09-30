using System;
using System.Diagnostics;
using DGAuthServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace DGAuthServer.ModelsDB;

/// <summary>
/// 
/// </summary>
public class DgAuthDbContext : DbContext
{
#pragma warning disable CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.
    public DgAuthDbContext(DbContextOptions<DgAuthDbContext> options)
			: base(options)
    {
    }

    public DgAuthDbContext()
    {
    }
#pragma warning restore CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.

	protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
		if (null != DGAuthServerGlobal.ActDbContextOnConfiguring)
        {
            DGAuthServerGlobal.ActDbContextOnConfiguring(options);
        }
        else if(string.Empty != DGAuthServerGlobal.DbConnectString)
        {
			Console.WriteLine("OnConfiguring : " + DGAuthServerGlobal.DbConnectString);

			switch (DGAuthServerGlobal.Setting.DbType)
            {
				case DGAuthDbType.Sqlite:
					options.UseSqlite(DGAuthServerGlobal.DbConnectString);
					break;

				case DGAuthDbType.Mssql:
					options.UseSqlServer(DGAuthServerGlobal.DbConnectString);
					break;
			}
        }
    }


    /// <summary>
    /// 엑세스 토큰
    /// </summary>
    public DbSet<DgAuthAccessToken> DGAuthServer_AccessToken { get; set; }

    /// <summary>
    /// 리플레시 토큰
    /// </summary>
    public DbSet<DgAuthRefreshToken> DGAuthServer_RefreshToken { get; set; }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}



///// <summary>
///// https://stackoverflow.com/a/60602620/6725889
///// </summary>
//public class YourDbContextFactory : IDesignTimeDbContextFactory<DgAuthDbContext>
//{
//    public DgAuthDbContext CreateDbContext(string[] args)
//    {
//        DbContextOptionsBuilder<DgAuthDbContext> optionsBuilder
//            = new DbContextOptionsBuilder<DgAuthDbContext>();
//        //optionsBuilder.UseSqlite(DGAuthServerGlobal.DbConnectString);
//        Console.WriteLine("aaaaa - " + DGAuthServerGlobal.DbType);

//        if (string.Empty != DGAuthServerGlobal.DbConnectString)
//        {


//            switch (DGAuthServerGlobal.DbType)
//            {
//                case DbType.Mssql:
//                    optionsBuilder.UseSqlServer(DGAuthServerGlobal.DbConnectString);
//                    break;
//                case DbType.Sqlite:
//                    optionsBuilder.UseSqlite(DGAuthServerGlobal.DbConnectString);
//                    break;
//            }
//        }

//        return new DgAuthDbContext(optionsBuilder.Options);
//    }
//}
