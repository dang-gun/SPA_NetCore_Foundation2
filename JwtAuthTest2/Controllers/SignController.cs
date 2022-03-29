using JwtAuthTest2.Global;
using JwtAuthTest2.Models;
using Microsoft.AspNetCore.Mvc;
using ModelsDB;

namespace JwtAuthTest2.Controllers
{
	/// <summary>
	/// 사인 관련(인,아웃,조인)
	/// </summary>
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class SignController : ControllerBase
	{
		[HttpPut]
		public ActionResult<SignInModel> SignIn(
					[FromForm] string sSignName
					, [FromForm] string sPassword)
		{
			//로그인 처리용 모델
			SignInModel smResult = new SignInModel();

			User? findUser 
				= GlobalStatic.Users.Where(w=>
						w.SignName == sSignName
						&& w.Password == sPassword)
				.FirstOrDefault();

			if (null != findUser)
			{//유저 찾음
				smResult.Complete = true;
				smResult.Token
					= String.Format("{0}▩{1}"
						, sSignName
						, Guid.NewGuid().ToString());
			}


			return smResult;
		}



	}
}
