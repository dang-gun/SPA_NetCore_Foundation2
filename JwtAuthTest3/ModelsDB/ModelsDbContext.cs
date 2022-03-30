namespace ModelsDB;

using Microsoft.EntityFrameworkCore;
using WebApi.Entities;
using BCrypt.Net;
using JwtAuthTest3.Global;

/// <summary>
/// 데이터 베이스에 접근하기위한 컨택스트
/// </summary>
public class ModelsDbContext : DbContext
{
    /// <summary>
    /// 
    /// </summary>
#pragma warning disable CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.
    public ModelsDbContext()
#pragma warning restore CS8618 // 생성자를 종료할 때 null을 허용하지 않는 필드에 null이 아닌 값을 포함해야 합니다. null 허용으로 선언해 보세요.
    {
    }

    /// <summary>
    /// DB 구성 시작
    /// </summary>
    /// <remarks>모든 ORM라이브러리를 미리 로드할 필요가 없으니 
    /// 프로젝트에 맞게 수동으로 구성할수 있도록 이렇게 구성한다.</remarks>
    /// <param name="options"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        switch (GlobalStatic.DBType)
        {
            case "inmemory":
                //options.UseInMemoryDatabase("TestDb");
                break;
            case "sqlite":
                options.UseSqlite(GlobalStatic.DBString);
                break;
            case "mysql":
                //options.UseSqlite(GlobalStatic.DBString);
                break;

            case "mssql":
            default:
                //options.UseSqlServer(GlobalStatic.DBString);
                break;
        }
    }

    /// <summary>
    /// 테이블(모델)이 생성될때 기본값으로 들어가는 데이터들
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                idUser = 1,
                PasswordHash = BCrypt.HashPassword("1111"),
                SignName = "test01"
            }
            , new User
            {
                idUser = 2,
                PasswordHash = BCrypt.HashPassword("1111"),
                SignName = "test02"
            });

        modelBuilder.Entity<UserInfo>().HasData(
            new UserInfo
            {
                idUserInfo = 1,
                idUser = 1,
                ViewName = "Test 01",
                Created = DateTime.Now,
                UpdateLast = DateTime.Now,
            }
            , new UserInfo
            {
                idUserInfo = 2,
                idUser = 2,
                ViewName = "Test 02",
                Created = DateTime.Now,
                UpdateLast = DateTime.Now,
            });
    }


    /// <summary>
    /// 유저 테이블
    /// </summary>
    public DbSet<User> User { get; set; }
    /// <summary>
    /// 유저 상세 정보 고유키
    /// </summary>
    public DbSet<UserInfo> UserInfo { get; set; }
    /// <summary>
    /// 유저 리플레시 토큰
    /// </summary>
    public DbSet<UserRefreshToken> UserRefreshToken { get; set; }
}