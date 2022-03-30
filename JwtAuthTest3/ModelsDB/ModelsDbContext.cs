namespace ModelsDB;

using Microsoft.EntityFrameworkCore;
using WebApi.Entities;
using BCrypt.Net;
using JwtAuthTest3.Global;

/// <summary>
/// ������ ���̽��� �����ϱ����� ���ý�Ʈ
/// </summary>
public class ModelsDbContext : DbContext
{
    /// <summary>
    /// 
    /// </summary>
#pragma warning disable CS8618 // �����ڸ� ������ �� null�� ������� �ʴ� �ʵ忡 null�� �ƴ� ���� �����ؾ� �մϴ�. null ������� ������ ������.
    public ModelsDbContext()
#pragma warning restore CS8618 // �����ڸ� ������ �� null�� ������� �ʴ� �ʵ忡 null�� �ƴ� ���� �����ؾ� �մϴ�. null ������� ������ ������.
    {
    }

    /// <summary>
    /// DB ���� ����
    /// </summary>
    /// <remarks>��� ORM���̺귯���� �̸� �ε��� �ʿ䰡 ������ 
    /// ������Ʈ�� �°� �������� �����Ҽ� �ֵ��� �̷��� �����Ѵ�.</remarks>
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
    /// ���̺�(��)�� �����ɶ� �⺻������ ���� �����͵�
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
    /// ���� ���̺�
    /// </summary>
    public DbSet<User> User { get; set; }
    /// <summary>
    /// ���� �� ���� ����Ű
    /// </summary>
    public DbSet<UserInfo> UserInfo { get; set; }
    /// <summary>
    /// ���� ���÷��� ��ū
    /// </summary>
    public DbSet<UserRefreshToken> UserRefreshToken { get; set; }
}