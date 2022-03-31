using JwtAuthTest4.Global;
using JwtAuthTest4.Models;
using Microsoft.AspNetCore.Mvc;
using ModelsDB;
using JwtAuth;

namespace JwtAuthTest4.Controllers
{
	/// <summary>
	/// 사인 관련(인,아웃,조인)
	/// </summary>
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class SignController : ControllerBase
	{
		public readonly string AccessTokenName = "AccessToken";
		public readonly string RefreshTokenName = "RefreshToken";


		/// <summary>
		/// 인증 유틸
		/// </summary>
		private readonly IJwtUtils _JwtUtils;

		public SignController(IJwtUtils jwtUtils)
		{
			this._JwtUtils = jwtUtils;
		}

		/// <summary>
		/// 사인 인
		/// </summary>
		/// <param name="sSignName"></param>
		/// <param name="sPassword"></param>
		/// <returns></returns>
		[HttpPut]
		[AllowAnonymous]
		public ActionResult<SignInModel> SignIn(
					[FromForm] string sSignName
					, [FromForm] string sPassword)
		{
			//로그인 처리용 모델
			SignInModel smResult = new SignInModel();

			DateTime dtNow = DateTime.Now;

			using (ModelsDbContext db1 = new ModelsDbContext())
			{
				User? findUser
				= db1.User.Where(w =>
						w.SignName == sSignName
						&& w.PasswordHash == sPassword)
				.FirstOrDefault();

				if (null != findUser)
				{//유저 찾음

					smResult.idUser = findUser.idUser;
					smResult.Complete = true;

					//엑세스 토큰 생성
					string at = this._JwtUtils.AccessTokenGenerate(findUser);
					string st = this._JwtUtils.RefreshTokenGenerate();

					while(true)
					{
						if (true == db1.UserRefreshToken.Any(a => a.RefreshToken == st))
						{//같은 토큰이 있다.
							st = this._JwtUtils.RefreshTokenGenerate();
						}
						else
						{
							//새로운 값이면 완료
							break;
						}
					}

					//기존 토큰 만료 처리
					IQueryable<UserRefreshToken> iqFindURT
						= db1.UserRefreshToken
							.Where(w => w.idUser == findUser.idUser
									&& true == w.ActiveIs);
					//linq는 데이터를 수정할때는 좋은 솔류션이 아니다.
					//반복문으로 직접수정하는 것이 훨씬 성능에 도움이 된다.
					foreach (UserRefreshToken itemURT in iqFindURT)
					{
						//만료 시간을 기입함
						itemURT.RevokeTime = dtNow;
						itemURT.ActiveCheck();
					}


					//새로운 토큰 생성
					smResult.AccessToken = at;
					smResult.RefreshToken = st;

					UserRefreshToken newURT = new UserRefreshToken()
					{
						idUser = findUser.idUser, 
						RefreshToken = st,
						ExpiresTime = DateTime.UtcNow.AddSeconds(1296000),
					};
					newURT.ActiveCheck();


					db1.UserRefreshToken.Add(newURT);
					db1.SaveChanges();

					//쿠키에 저장요청
					this.Cookie_AccessToken(at);
					this.Cookie_RefreshToken(st);
				}
			}//end using db1

			return smResult;
		}

		/// <summary>
		/// 사인 아웃
		/// </summary>
		/// <remarks>
		/// ControllerBase.SignOut가 있어서 new로 선언한다<br />
		/// ControllerBase.SignOut은 표준화된 외부 로그인방식 같은데....
		/// 어떻게 활용할지는 연구를 해봐야 할듯하다.
		/// </remarks>
		/// <returns></returns>
		[HttpPut]
		[Authorize]
		public new ActionResult<ApiResultModel> SignOut()
		{
			ApiResultModel arReturn = new ApiResultModel();
			arReturn.Complete = true;

			DateTime dtNow = DateTime.Now;

			//string? st = Request.Cookies[RefreshTokenName];

			//대상 유저를 검색하고
			long? idUser = this._JwtUtils.ClaimDataGet(HttpContext.User);
			if (null != idUser)
			{//대상이 있다.
				using (ModelsDbContext db1 = new ModelsDbContext())
				{
					//가지고 있는 기존 리플레시 토큰 만료 처리
					IQueryable<UserRefreshToken> iqFindURT
						= db1.UserRefreshToken
							.Where(w => w.idUser == idUser
									&& true == w.ActiveIs);
					//linq는 데이터를 수정할때는 좋은 솔류션이 아니다.
					//반복문으로 직접수정하는 것이 훨씬 성능에 도움이 된다.
					foreach (UserRefreshToken itemURT in iqFindURT)
					{
						//만료 시간을 기입함
						itemURT.RevokeTime = dtNow;
						itemURT.ActiveCheck();
					}
				}

				//쿠키에 저장요청
				//빈값을 저장해서 기존 토큰을 제거요청한다.
				this.Cookie_AccessToken("");
				this.Cookie_RefreshToken("");
			}

			return arReturn;
		}

		/// <summary>
		/// 지정한 유저의 정보를 준다.
		/// </summary>
		/// <param name="idUser"></param>
		/// <returns></returns>
		[HttpGet]
		//[Authorize]
		public ActionResult<SignInfoModel> SignInfo(long idUser)
		{
			//로그인 처리용 모델
			SignInfoModel smResult = new SignInfoModel();

			using (ModelsDbContext db1 = new ModelsDbContext())
			{
				User? findUser
				= db1.User.Where(w =>
						w.idUser == idUser)
				.FirstOrDefault();

				if (null != findUser)
				{//유저 찾음
					smResult.Complete = true;
					smResult.UserInfo = findUser;
				}

			}//end using db1
			


			return smResult;
		}




		/// <summary>
		/// 쿠키에 엑세스 토큰 저장을 요청한다.
		/// </summary>
		/// <param name="token"></param>
		private void Cookie_AccessToken(string token)
		{
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Expires = DateTime.UtcNow.AddDays(7)
			};
			Response.Cookies.Append(AccessTokenName, token, cookieOptions);
		}

		/// <summary>
		/// 쿠기에 리플레이 토큰 저장을 요청한다.
		/// </summary>
		/// <param name="token"></param>
		private void Cookie_RefreshToken(string token)
		{
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Expires = DateTime.UtcNow.AddDays(7)
			};
			Response.Cookies.Append(RefreshTokenName, token, cookieOptions);
		}
	}
}
