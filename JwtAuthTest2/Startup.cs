﻿using Newtonsoft.Json.Serialization;

namespace JwtAuthTest2
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
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //API모델을 파스칼 케이스 유지하기
            //2.0
            //services.AddMvc().AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
            //3.0
            services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.ContractResolver = new DefaultContractResolver(); });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
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