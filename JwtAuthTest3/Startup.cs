using JwtAuthTest3.Global;
using Newtonsoft.Json.Serialization;
using WebApi.Authorization;
using WebApi.Helpers;
using WebApi.Services;

namespace JwtAuthTest3
{
    /// <summary>
    /// 새로 고침 토큰 API를 사용하여 ASP.NET Core JWT를 로컬에서 실행
    /// https://jasonwatmore.com/post/2022/01/24/net-6-jwt-authentication-with-refresh-tokens-tutorial-with-example-api#running-angular
    /// 
    /// https://jasonwatmore.com/post/2022/02/26/net-6-boilerplate-api-tutorial-with-email-sign-up-verification-authentication-forgot-password
    /// https://github.com/cornflourblue/dotnet-6-signup-verification-api
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

            //DB 커낵션 스트링 받아오기
            //string sConnectStringSelect = "DangGunDotNet_mssql";
            string sConnectStringSelect = "JwtAuthTest_sqlite";
            GlobalStatic.DBType = Configuration[sConnectStringSelect + ":DBType"].ToLower();
            GlobalStatic.DBString = Configuration[sConnectStringSelect + ":ConnectionString"];

        }


        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.<br /> 
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<DataContext>();
            //API모델을 파스칼 케이스 유지하기
            services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.ContractResolver = new DefaultContractResolver(); });

            

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddSwaggerGen();

            // configure DI for application services
            services.AddScoped<IJwtUtils, JwtUtils>();
            services.AddScoped<IAccountService, AccountService>();
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

                //스웨거 사용 - 개발버전에서만 사용할때는 이것만 사용한다.
                //app.UseSwagger();
                //app.UseSwaggerUI();
            }

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
}
