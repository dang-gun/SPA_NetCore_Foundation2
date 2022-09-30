
using DGAuthServer.Models;
using DGAuthServer.ModelsDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace DGAuthServer;

/// <summary>
/// DG Auth Server Service를 위한 빌더 구현
/// </summary>
public static class DgJwtAuthUtilsBuilder
{
    /// <summary>
    /// 서비스 빌더
    /// </summary>
    /// <param name="services"></param>
    /// <param name="settingData"></param>
    /// <param name="actDbContextOnConfiguring">
    /// DB 컨택스트 생성시 사용될 'OnConfiguring'액션<br />
    /// 기존 DB에 'DGAuthServer_AccessToken'과 'DGAuthServer_RefreshToken' 테이블이 없으면
    /// 해당 테이블이 생성된다.<br />
    /// 필요에 따라 자신의 컨택스트(모델)에 해당 테이블을 선언하여 접근할 수 있다.
    /// </param>
    /// <returns></returns>
    public static IServiceCollection AddDgAuthServerBuilder(
        this IServiceCollection services
        , DgAuthSettingModel settingData
        , Action<DbContextOptionsBuilder>? actDbContextOnConfiguring)
    {

        //세팅 데이터 저장
        DGAuthServerGlobal.Setting.ToCopy(settingData);

        //옵션 전달
        services.Configure<DgAuthSettingModel>(options =>
        {
            options.ToCopy(DGAuthServerGlobal.Setting);
        });

        if (DGAuthDbType.Memory == DGAuthServerGlobal.Setting.DbType
            || null == actDbContextOnConfiguring)
        {//사용 db가 메모리거나 
            //연결된 DB액션이 없다.

			//자체적으로 사용할 데이터 베이스
			DGAuthServerGlobal.ActDbContextOnConfiguring
				= (options => options.UseInMemoryDatabase(databaseName: "DGAuthServer_DB"));	
        }
        else
        {
			DGAuthServerGlobal.ActDbContextOnConfiguring
				= actDbContextOnConfiguring;
		}


		//테이블 생성 및 마이그레이션 정보 적용
		switch (DGAuthServerGlobal.Setting.DbType)
        {
            case DGAuthDbType.Sqlite:
				using (DgAuthDbContext_Sqlite dbSqlite = new DgAuthDbContext_Sqlite())
				{
					dbSqlite.Database.Migrate();
				}
				break;
			case DGAuthDbType.Mssql:
				using (DgAuthDbContext_Mssql dbSqlite = new DgAuthDbContext_Mssql())
				{
					dbSqlite.Database.Migrate();
				}
				break;

			default:
				using (DgAuthDbContext db1 = new DgAuthDbContext())
				{
					//db1.Database.EnsureCreated();
					db1.Database.Migrate();
				}
				break;
        }


        //db 클리어 설정
        Timer timerDbClear
            = new Timer((aa) 
                =>
                {
                    DGAuthServerGlobal.Service.DbClear();
                });
        //타이머 시작
        timerDbClear.Change(0, DGAuthServerGlobal.Setting.DbClearTime);

        //다은 예정시간 계산
        DGAuthServerGlobal.DbClearExpectedTime
            = DGAuthServerGlobal.DbClearTime
                .AddSeconds(DGAuthServerGlobal.Setting.DbClearTime);


        //메모리 캐쉬 사용여부 처리 ************************
        if (true == DGAuthServerGlobal.Setting.MemoryCacheIs)
        {//사용
            DGAuthServerGlobal.MemoryCache 
                = new MemoryCache(new MemoryCacheOptions() {});
        }

        return services;
    }


    /// <summary>
    /// 어플리케이션(미들웨어) 빌더
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseDgAuthServerAppBuilder(
        this IApplicationBuilder app)
    {

        //JwtAuth 미들웨어 주입
        app.UseMiddleware<DgAuthMiddleware>();
        return app;
    }
}

