using OAuth2Test1.Global;
using Newtonsoft.Json.Serialization;

namespace OAuth2Test1
{
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
            string sConnectStringSelect = "Test_sqlite";
            GlobalStatic.DBType = Configuration[sConnectStringSelect + ":DBType"].ToLower();
            GlobalStatic.DBString = Configuration[sConnectStringSelect + ":ConnectionString"];

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //API모델을 파스칼 케이스 유지하기
            services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.ContractResolver = new DefaultContractResolver(); });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();


            //외부 공급자 연결
            //https://docs.microsoft.com/ko-kr/dotnet/architecture/microservices/secure-net-microservices-web-applications/
            //
            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            //services.AddAuthentication()
            //    .AddMicrosoftAccount(microsoftOptions =>
            //    {
            //        microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ClientId"];
            //        microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
            //    })
            //    .AddGoogle(googleOptions => { ... })
            //    .AddTwitter(twitterOptions => { ... })
            //    .AddFacebook(facebookOptions => { ... });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                //스웨거 사용
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
