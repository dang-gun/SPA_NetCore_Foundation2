using JwtAuthTest3.Models.Signs;
using Microsoft.AspNetCore.Mvc;
using ModelsDB;
using BCrypt.Net;

namespace JwtAuthTest3.Controllers
{
	/// <summary>
	/// 사인 관련(인,아웃,조인)
	/// </summary>
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class SignController : Controller
	{
        /// <summary>
        /// 사인인 시도
        /// </summary>
        /// <param name="sSignName"></param>
        /// <param name="sPassword"></param>
        /// <returns></returns>
        [HttpPut]
        public ActionResult<SignInResultModel> SignIn(
            [FromForm] string sSignName
            , [FromForm] string sPassword)
        {
            //로그인 처리용 모델
            SignInResultModel armResult = new SignInResultModel();

            //API 호출 시간
            DateTime dtNow = DateTime.Now;

            //검색된 유저
            User? findUser = null;

            using (ModelsDbContext db1 = new ModelsDbContext())
            {
                //유저 검색
                findUser
                    = db1.User
                        .FirstOrDefault(m =>
                            m.SignName == sSignName);

                if (null != findUser)
                {//검색된 유저가 있다

                    if (true == BCrypt.Net.BCrypt.Verify(sPassword, findUser.PasswordHash))
                    {//비밀 번호 일치
                    }
                    else
                    {//비밀번호 틀림

                        //개체 초기화
                        findUser = null;
                    }
                }
            }//end using db1


            if (findUser != null)
            {
                //토큰 요청
                TokenResponse tr = GlobalStatic.TokenProc.RequestTokenAsync(sEmail, sPW).Result;

                if (true == tr.IsError)
                {//에러가 있다.
                    rrResult.InfoCode = "1";
                    rrResult.Message = "아이디나 비밀번호가 틀렸습니다.";
                }
                else
                {//에러가 없다.
                    using (SpaNetCoreFoundationContext db1 = new SpaNetCoreFoundationContext())
                    {
                        //기존 로그인한 유저 검색
                        UserSignIn[] arrSL
                            = db1.UserSignIn
                                .Where(m => m.idUser == findUser.idUser)
                                .ToArray();

                        //기존 로그인 토큰 제거
                        foreach (UserSignIn itemUSI in arrSL)
                        {
                            //리플레시 토큰 제거
                            if ((null != itemUSI.RefreshToken)
                                && (string.Empty != itemUSI.RefreshToken))
                            {
                                TokenRevocationResponse trr
                                    = GlobalStatic.TokenProc
                                        .RevocationTokenAsync(itemUSI.RefreshToken)
                                        .Result;
                            }
                        }//end foreach itemUSI

                        //기존 로그인한 유저 정보 제거
                        db1.UserSignIn.RemoveRange(arrSL);
                        //db 적용
                        db1.SaveChanges();


                        //로그인 되어있는 유저정보 저장
                        UserSignIn slItem = new UserSignIn();
                        slItem.idUser = findUser.idUser;
                        slItem.RefreshToken = tr.RefreshToken;
                        slItem.SignInDate = dtNow;
                        slItem.RefreshDate = dtNow;

                        //기존 로그인한 유저 정보 제거
                        db1.UserSignIn.Add(slItem);
                        //db 적용
                        db1.SaveChanges();

                        //로그인한 유저에게 전달할 정보
                        armResult.idUser = findUser.idUser;
                        armResult.Email = findUser.SignEmail;
                        armResult.ViewName = armResult.Email;

                        armResult.access_token = tr.AccessToken;
                        armResult.refresh_token = tr.RefreshToken;

                        //성공 로그
                        //사인인 성공 기록
                        GlobalSign.LogAdd_DB(
                            1
                            , ModelDB.UserSignLogType.SignIn
                            , findUser.idUser
                            , string.Format("SignIn 성공 - {0}", sEmail));
                    }//end using db1
                }
            }
            else
            {
                rrResult.InfoCode = "1";
                rrResult.Message = "아이디나 비밀번호가 틀렸습니다.";
            }

            return armResult;
        }
    }
}
