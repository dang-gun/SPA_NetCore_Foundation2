using SPA_NetCore_Foundation2_01.Models;
using Microsoft.AspNetCore.Mvc;
using ModelsDB;
using SPA_NetCore_Foundation2_01.Global;
using DGAuthServer.AuthAttribute;
using DGAuthServer;
using System.Net;
using ApiResult;

namespace SPA_NetCore_Foundation2_01.Controllers;


/// <summary>
/// 사인 관련(인,아웃,조인)
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class SignController : ControllerBase
{
	/// <summary>
	/// 가입
	/// </summary>
	/// <param name="sSignName"></param>
	/// <param name="sPassword"></param>
	/// <returns></returns>
	[HttpPost]
	public ActionResult<ApiResultObjectModel> Register(
		[FromForm] string sSignName
		, [FromForm] string sPassword)
	{
		//로그인 처리용 모델
		ApiResultObjectModel arReturn = new ApiResultObjectModel();

		if (string.Empty == sSignName)
		{
			arReturn.InfoCode = "-11";
			arReturn.Message = "사인 인 이름은 필수 입니다.";
		}
		else if (string.Empty == sPassword)
		{
			arReturn.InfoCode = "-12";
			arReturn.Message = "사인 인 비밀번호는 필수 입니다.";
		}

		if (true == arReturn.IsSuccess())
		{
			using (ModelsDbContext db1 = new ModelsDbContext())
			{
				//같은 이름이 있는지 찾는다.
				User? findUser
					= db1.User.Where(w => w.SignName == sSignName)
						.FirstOrDefault();

				if (null != findUser)
				{
					arReturn.InfoCode = "-21";
					arReturn.Message = "이미 있는 사인 인 이름입니다.";
				}


				if (true == arReturn.IsSuccess())
				{
					User newUser = new User();
					newUser.SignName = sSignName;
					newUser.PasswordHash = sPassword;
					db1.User.Add(newUser);
				}

				db1.SaveChanges();
			}//end using db1
		}

		return arReturn;
	}

	/// <summary>
	/// 사인 인
	/// </summary>
	/// <param name="sSignName"></param>
	/// <param name="sPassword"></param>
	/// <returns></returns>
	[HttpPut]
	[AllowAnonymous]
	public ActionResult<SignInResultModel> SignIn(
				[FromForm] string sSignName
				, [FromForm] string sPassword)
	{
		ApiResultReady arReturn = new ApiResultReady(this);
		SignInResultModel rmReturn = new SignInResultModel();
		arReturn.ResultObject = rmReturn;


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

				rmReturn.idUser = findUser.idUser;

				//리플레시 토큰 생성
				string st
					= DGAuthServerGlobal.Service
						.RefreshTokenGenerate(
							null
							, findUser.idUser
							, string.Empty
							, Response);
				//엑세스 토큰 생성
				string at
					= DGAuthServerGlobal.Service
						.AccessTokenGenerate(
							findUser.idUser
							, string.Empty
							, Response);

				//새로운 토큰 생성
				rmReturn.AccessToken = at;
				rmReturn.RefreshToken = st;
			}
		}//end using db1

		return arReturn.ToResult();
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
	public new ActionResult<ApiResultObjectModel> SignOut()
	{
		ApiResultReady arReturn = new ApiResultReady(this);



		//대상 유저를 검색하고
		long? idUser = DGAuthServerGlobal.Service.ClaimDataGet(HttpContext.User);
		if (null != idUser)
		{//대상이 있다.
			DGAuthServerGlobal.Service
				.RefreshTokenRevoke(
					false
					, (long)idUser
					, String.Empty
					, Response);

			DGAuthServerGlobal.Service
				.AccessTokenRevoke(
					false
					, (long)idUser
					, String.Empty
					, Response);
		}

		return arReturn.ToResult();
	}

	/// <summary>
	/// 지정한 유저의 정보를 준다.
	/// <para>AnonymousAndAuthorize테스트</para>
	/// </summary>
	/// <returns></returns>
	[HttpGet]
	[AnonymousAndAuthorize]
	//[ServiceFilter(typeof(AnonymousAndAuthorizeAttribute))]
	public ActionResult<SignInfoResultModel> SignInfo()
	{
		ApiResultReady arReturn = new ApiResultReady(this);
		SignInfoResultModel rmReturn = new SignInfoResultModel();
		arReturn.ResultObject = rmReturn;

		//전달할 개체를 만들고
		rmReturn.UserInfo = new List<User>();

		//유저 정보를 찾는다.
		long? idUser = DGAuthServerGlobal.Service.ClaimDataGet(HttpContext.User);

		using (ModelsDbContext db1 = new ModelsDbContext())
		{
			User? findUser
			= db1.User.Where(w =>
					w.idUser == idUser)
			.FirstOrDefault();

			if (null != findUser)
			{//유저 찾음

				//전체 리스트
				rmReturn.UserInfo
					= db1.User.ToList();
			}
			else
			{
				//한개만 전달 리스트
				rmReturn.UserInfo.Add(db1.User.FirstOrDefault()!);
			}

		}//end using db1


		return arReturn.ToResult();
	}

	/// <summary>
	/// 리플레시 토큰으로 새로운 액세스 토큰을 생성해준다.
	/// </summary>
	/// <returns></returns>
	[HttpPut]
	public ActionResult<SignInResultModel> RefreshToken()
	{
		ApiResultReady arReturn = new ApiResultReady(this);
		SignInResultModel rmReturn = new SignInResultModel();
		arReturn.ResultObject = rmReturn;

		DateTime dtNow = DateTime.Now;

		string sNewST = string.Empty;
		string sNewAT = string.Empty;

		//소유 유저 찾기
		long idUser = DGAuthServerGlobal.Service
						.RefreshTokenFindUser(string.Empty, string.Empty, Request);

		if (0 < idUser)
		{//대상 찾음

			//토큰 새로 받고
			sNewST
				= DGAuthServerGlobal.Service
					.RefreshTokenGenerate(
						null
						, idUser
						, string.Empty
						, Response);

			sNewAT = DGAuthServerGlobal.Service
						.AccessTokenGenerate(
							idUser
							, string.Empty
							, Response);

			rmReturn.AccessToken = sNewAT;
			rmReturn.RefreshToken = sNewST;
		}
		else
		{
			//없으면 권한 없음 에러를 낸다.
			arReturn.ApiResultInfoSet("-1", "권한이 없습니다.");
			Response.StatusCode = (int)HttpStatusCode.Unauthorized;
		}

		if (false == arReturn.IsSuccess())
		{
			//쿠키에서 토큰 제거
			DGAuthServerGlobal.Service.Cookie_AccessToken(String.Empty, Response);
			DGAuthServerGlobal.Service.Cookie_RefreshToken(String.Empty, Response);
		}

		return arReturn.ToResult();
	}


	/// <summary>
	/// 가지고 있는 리플레시 토큰을 만료 시킨다.
	/// </summary>
	/// <returns></returns>
	[HttpDelete]
	[Authorize]
	public ActionResult<ApiResultBaseModel> RefreshTokenRevoke()
	{
		ApiResultBaseModel arReturn = new ApiResultBaseModel();

		DateTime dtNow = DateTime.Now;

		long idUser = DGAuthServerGlobal.Service
							.RefreshTokenFindUser(String.Empty, String.Empty, Request);

		if (0 < idUser)
		{
			DGAuthServerGlobal.Service
				.RefreshTokenRevoke(
					false
					, idUser
					, String.Empty
					, Response);

			DGAuthServerGlobal.Service
				.AccessTokenRevoke(
					false
					, idUser
					, String.Empty
					, Response);
		}
		else
		{
			//없으면 권한 없음 에러를 낸다.
			arReturn.InfoCode = "-1";
			arReturn.Message = "권한이 없습니다.";
			Response.StatusCode = (int)HttpStatusCode.Unauthorized;
		}


		return arReturn;
	}

	/// <summary>
	/// 로그인된 계정이 가지고 있는 모든 리플레시 토큰을 만료 시킨다.
	/// </summary>
	/// <returns></returns>
	[Authorize]
	[HttpDelete]
	public ActionResult<ApiResultBaseModel> RefreshTokenRevokeAll()
	{
		//로그인 처리용 모델
		ApiResultBaseModel arReturn = new ApiResultBaseModel();
		
		DateTime dtNow = DateTime.Now;


		//대상 유저를 검색한다.
		long? idUser = DGAuthServerGlobal.Service.ClaimDataGet(HttpContext.User);

		if (null != idUser)
		{//비어있지 않다.

			DGAuthServerGlobal.Service
				.RefreshTokenRevoke(
					true
					, (long)idUser
					, String.Empty
					, Response);

			DGAuthServerGlobal.Service
				.AccessTokenRevoke(
					true
					, (long)idUser
					, String.Empty
					, Response);
		}
		else
		{
			//없으면 권한 없음 에러를 낸다.
			arReturn.InfoCode = "-1";
			arReturn.Message = "권한이 없습니다.";
			Response.StatusCode = (int)HttpStatusCode.Unauthorized;
		}


		if (false == arReturn.IsSuccess())
		{
			//쿠키에서 토큰 제거
			DGAuthServerGlobal.Service.Cookie_AccessToken(String.Empty, Response);
			DGAuthServerGlobal.Service.Cookie_RefreshToken(String.Empty, Response);
		}


		return arReturn;
	}


}
