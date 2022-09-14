
using DGAuthServer.Models;
using DGAuthServer.ModelsDB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace DGAuthServer;


/// <summary>
/// 토큰 처리를 위한 서비스
/// </summary>
public class DGAuthServerService
{
    /// <summary>
    /// 엑세스 토큰 생성
    /// </summary>
    /// <param name="idUser">유저 구분을 위한 고유 번호</param>
    /// <param name="sClass">이 토큰을 분류하기 위한 이름</param>
    /// <param name="response">추가 처리를 위한 리스폰스</param>
    /// <returns></returns>
    public string AccessTokenGenerate(
        long idUser
        , string sClass
        , HttpResponse? response)
    {

        //시크릿 키 임시 저장
        string sSecret = string.Empty;
        
        if (true == DGAuthServerGlobal.Setting.SecretAlone)
        {//혼자사용하는 시크릿

            DgAuthAccessToken? findAT = null;

            using (DgAuthDbContext db1 = new DgAuthDbContext())
            {
                findAT 
                    = db1.DGAuthServer_AccessToken
                        .Where(w => w.idUser == idUser
                                && w.Class == sClass)
                        .FirstOrDefault();

                if (null != findAT)
                {//사용하는 시크릿이 있다.

                    //전달
                    sSecret = findAT.Secret;
                }
                else
                {//사용하는 시크릿이 없다.
                    DgAuthAccessToken newAT = new DgAuthAccessToken();
                    //사용자 입력
                    newAT.idUser = idUser;
                    newAT.Class = sClass;
                    //새로운 시크릿을 생성한다.
                    sSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                    newAT.Secret = sSecret;

                    //db에 적용
                    db1.Add(newAT);
                    db1.SaveChanges();

                    if (true == DGAuthServerGlobal.Setting.MemoryCacheIs)
                    {//메모리 캐쉬에 저장

                        this.MemoryCacheSaveSecret(idUser, sSecret);
                    }
                }
            }//end using db1
        }
        else
        {
            //공용 시크릿을 쓴다.
            sSecret = DGAuthServerGlobal.Setting.Secret!;
        }

        //시크릿을 바이트 배열로 변환
        byte[] byteSecretKey = new byte[0];
        byteSecretKey = Encoding.ASCII.GetBytes(sSecret);


        //jwt 보안 토큰 핸들러
        JwtSecurityTokenHandler tokenHandler 
            = new JwtSecurityTokenHandler();

        //엑세스키 생성에 필요한 정보를 입력한다.
        SecurityTokenDescriptor tokenDescriptor 
            = new SecurityTokenDescriptor
            {
                //추가로 담을 정보(클래임)
                Subject = new ClaimsIdentity(
                            new[] { new Claim(DGAuthServerGlobal.Setting.UserIdName
                                            , idUser.ToString()) }),
                //유효기간
                Expires = DateTime.UtcNow.AddSeconds(DGAuthServerGlobal.Setting.AccessTokenLifetime),
                //암호화 방식
                SigningCredentials 
                    = new SigningCredentials(new SymmetricSecurityKey(byteSecretKey)
                                                , SecurityAlgorithms.HmacSha256Signature)
            };

        //토큰 생성
        SecurityToken token 
            = tokenHandler.CreateToken(tokenDescriptor);

        //직렬화
        StringBuilder sbToken = new StringBuilder();
        if (true == DGAuthServerGlobal.Setting.SecretAlone)
        {//혼 자사용하는 시크릿

            //맨앞에 유저 고유번호를 붙여준다.
            sbToken.Append(idUser);
            //구분 기호
            sbToken.Append(DGAuthServerGlobal.Setting.SecretAloneDelimeter);
        }

        //만들어진 토큰 추가
        sbToken.Append(tokenHandler.WriteToken(token));



        if (true == DGAuthServerGlobal.Setting.AccessTokenCookie
            && null != response)
        {//쿠키 사용

            //리플레시 토큰은 저장한다.
            this.Cookie_AccessToken(sbToken.ToString(), response);
        }

        return sbToken.ToString();
    }

    /// <summary>
    /// 엑세스 토큰 확인.
    /// <para>쿠키가 사용중이면 sToken는 무시되고 쿠키를 읽어 사용한다.</para>
    /// </summary>
    /// <remarks>미들웨어에서도 호출해서 사용한다.</remarks>
    /// <param name="sToken"></param>
    /// <param name="sClass"></param>
    /// <param name="request"></param>
    /// <returns>찾아낸 idUser, 토큰 자체가 없으면 -1, 토큰이 유효하지 않으면 0 </returns>
    public long AccessTokenValidate(
        string sToken
        , string sClass
        , HttpRequest? request)
    {
        string sTokenFinal = String.Empty;

        if (null != request
            && true == DGAuthServerGlobal.Setting.AccessTokenCookie)
        {//쿠키 사용

            string? sTokenTemp
                = request.Cookies[DGAuthServerGlobal.Setting.AccessTokenCookieName];
            if (null != sTokenTemp)
            {//검색된 값이 있으면 전달
                sTokenFinal = sTokenTemp.ToString();
            }
        }
        else
        {
            sTokenFinal = sToken;
        }


        if (string.Empty == sTokenFinal)
        {//전달된 토큰 값이 없다.
            return -1;
        }


        //전달 받은 토큰에서 토큰 정보만 추출된 데이터
        string sTokenCut = string.Empty;
        //찾아낸 유저 번호
        long idUser = 0;
        //시크릿 키 임시 저장
        string sSecret = string.Empty;

        //사용할 시크릿 키 찾기
        if (true == DGAuthServerGlobal.Setting.SecretAlone)
		{//혼자사용하는 시크릿

            //첫번째 구분자를 찾는다.
            int nUser = sTokenFinal.IndexOf(DGAuthServerGlobal.Setting.SecretAloneDelimeter);

            if (0 > nUser)
            {//엑세스 토큰에 유저 번호가 없다.
                return 0;
            }


            //찾은 구분자 위치로 유저 아이디를 자른다.
            string sCutUser = sTokenFinal.Substring(0, nUser);

			if (false == Int64.TryParse(sCutUser, out idUser))
			{//숫자로 변환할 수 없다.
				return 0;
			}
			else if (0 >= idUser)
			{//유저 정보가 없다.
                
                //유저 정보가 잘못 되었다.
				return 0;
			}

            //잘린 데이터에서 토큰 정보만 저장
            //데이터 시작위치 = 찾은 구분자 위치 + 구분자 크기
            sTokenCut = sTokenFinal.Substring(
                            nUser + DGAuthServerGlobal.Setting.SecretAloneDelimeter.Length);

            

            if (true == DGAuthServerGlobal.Setting.MemoryCacheIs
                && true == this.MemoryCacheFindSecret(idUser, out sSecret))
            {//메모리 캐쉬를 사용중이고
                //메모리 캐쉬에서 값을 찾았다.

                //이미 sSecret에 데이터가 들어갔으니 처리하지 않는다.
            }
            else
            {
                //연결된 시크릿 검색
                using (DgAuthDbContext db1 = new DgAuthDbContext())
                {
                    DgAuthAccessToken? findAT
                        = db1.DGAuthServer_AccessToken
                            .Where(w => w.idUser == idUser
                                    && w.Class == sClass)
                            .FirstOrDefault();

                    if (null != findAT)
                    {//엑세스 토큰을 찾았다.

                        //찾은 시크릿 전달
                        sSecret = findAT.Secret;

                        if (true == DGAuthServerGlobal.Setting.MemoryCacheIs)
                        {
                            //메모리 캐쉬에 저장한다.
                            this.MemoryCacheSaveSecret(idUser, sSecret);
                        }
                    }
                }//end using db1
            }

            

            if (null == sSecret
                || string.Empty == sSecret)
            {//시크릿 정보가 없다.
                return 0;
            }
        }
		else
		{
			sTokenCut = sTokenFinal;
            sSecret = DGAuthServerGlobal.Setting.Secret!;
        }


        //정보 분석 시작 ****************
		JwtSecurityTokenHandler tokenHandler 
            = new JwtSecurityTokenHandler();
        //받은 시크릿을 바이트 배열로 변환
        byte[] byteKey 
            = Encoding.ASCII.GetBytes(sSecret);
        try
        {
            //토큰 해석을 시작한다.
            tokenHandler.ValidateToken(sTokenCut, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(byteKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;
            //찾은 크래임에서 유저 고유번호만 추출한다.
            long accountId 
                = Int64.Parse(jwtToken.Claims
                                .First(x => x.Type == DGAuthServerGlobal.Setting.UserIdName)
                                .Value);

            //찾은 아이디
            return accountId;
        }
        catch
        {//토큰을 해석하지 못했다.
            
            //토큰을 어떤사유에서든 해석하지 못하면 0 에러를 내보내어
            //상황에 따라 리플레시 토큰으로 토큰을 갱신하도록 알려야 한다.
            return 0;
        }
    }

    /// <summary>
    /// 엑세스 토큰을 만료시킴
    /// </summary>
    /// <remarks>
    /// 엑세스 토큰을 만료시킬 방법은 없다<br />
    /// 개인 시크릿 키를 사용중일때만 리보크가 가능하다.<br />
    /// 쿠키를 사용중이라면 쿠키를 지워주는 기능을 한다.<br />
    /// bAllRevoke가 true라면 개인 시크릿 키가 재발급 되면서 
    /// 기존 엑세스토큰은 사용할 수 없게 된다.
    /// <para>
    /// 여러 사이트(혹은 프로그램)에서 하나의 인증서버를 두고 사용할경우 
    /// idUser가 겹치면 다른 사이트에서도 엑세스토큰이 만료되게 된다.
    /// </para>
    /// </remarks>
    /// 
    /// <param name="bAllRevoke">클래스와 상관없이 전체 엑세스 토큰을 리보크한다.<br />
    /// 개인 시크릿을 사용중이라면 false일때는 개인 시크릿이 변하지 않는다.
    /// </param>
    /// <param name="idUser"></param>
    /// <param name="sClass">이 토큰을 분류하기 위한 이름</param>
    /// <param name="response"></param>
    /// <returns></returns>
    public void AccessTokenRevoke(
        bool bAllRevoke
        , long idUser
        , string sClass
        , HttpResponse? response)
    {
        using (DgAuthDbContext db1 = new DgAuthDbContext())
        {
            IQueryable<DgAuthAccessToken> iqFindAT;

            if (true == bAllRevoke)
            {//전체 검색
                iqFindAT
                    = db1.DGAuthServer_AccessToken
                        .Where(w => w.idUser == idUser);
            }
            else
            {//대상 검색
                iqFindAT
                    = db1.DGAuthServer_AccessToken
                        .Where(w => w.idUser == idUser
                                && w.Class == sClass);
            }

            //대상의 내용을 지운다.
            //개인 시크릿을 사용중이라면 엑세스토큰을 요청하면서 다시 생성된다.
            //그래서 여기서는 지우기만 하면된다.
            db1.DGAuthServer_AccessToken.RemoveRange(iqFindAT);

            db1.SaveChanges();
        }//end using db1


        if (true == DGAuthServerGlobal.Setting.AccessTokenCookie
            && null != response)
        {//쿠키 사용

            //리플레시 토큰은 저장한다.
            this.Cookie_AccessToken(string.Empty, response);
        }
    }


    /// <summary>
    /// 리플레시 토큰 생성.
    /// </summary>
    /// <remarks>중복검사 및 유효성검사를 하고나서 조건에 맞는 토큰을 전달(생성)한다.</remarks>
    /// <param name="typeUsage"></param>
    /// <param name="idUser">이 토큰을 소유할 유저의 고유번호</param>
    /// <param name="sClass">이 토큰을 분류하기 위한 이름</param>
    /// <param name="response">추가 처리를 위한 리스폰스</param>
    /// <returns></returns>
    public string RefreshTokenGenerate(
        RefreshTokenUsageType? typeUsage
        , long idUser
        , string sClass
        , HttpResponse? response)
    {
        string sReturn = string.Empty;

        //지금 시간
        DateTime dtNow = DateTime.Now;

        RefreshTokenUsageType typeNewTokenFinal = RefreshTokenUsageType.None;
        if (null != typeUsage)
        {//전달된 옵션이 있다.
            typeNewTokenFinal = (RefreshTokenUsageType)typeUsage;
        }
        else
        {
            //설정된 정보를 읽어 사용한다.
            typeNewTokenFinal 
                = DGAuthServerGlobal.Setting.RefreshTokenReUseType;
        }


        using (DgAuthDbContext db1 = new DgAuthDbContext())
        {
            bool bNew = false;


            switch (typeNewTokenFinal)
            {
                case RefreshTokenUsageType.OneTimeOnly:
                    {
                        //새로 토큰을 생성
                        bNew = true;
                    }
                    break;

                case RefreshTokenUsageType.OneTimeOnlyDelay:
                case RefreshTokenUsageType.ReUse:
                case RefreshTokenUsageType.ReUseAddTime:
                    {
                        //기존 토큰이 있는지 찾는다.
                        DgAuthRefreshToken? findRT
                            = db1.DGAuthServer_RefreshToken
                                .Where(w => w.idUser == idUser
                                        && w.Class == sClass)
                                .FirstOrDefault();

                        if (null != findRT)
                        {//기존 토큰이 있다.

                            //유효성을 새로 고치고
                            findRT.ActiveCheck();

                            if (true == findRT.ActiveIs)
                            {//토큰이 유효하다.

                                //사용하던 토큰 전달
                                sReturn = findRT.RefreshToken;

                                if (RefreshTokenUsageType.ReUseAddTime == typeNewTokenFinal)
                                {
                                    //만료 시간을 는려준다.
                                    findRT.ExpiresTime
                                        = DateTime.UtcNow
                                            .AddSeconds(DGAuthServerGlobal.Setting
                                                            .RefreshTokenLifetime);
                                }
                                else if (RefreshTokenUsageType.OneTimeOnlyDelay == typeNewTokenFinal)
                                {

                                    //기존 토큰의 생성날짜를 확인한다.
                                    if (findRT.GenerateTime.AddSeconds(DGAuthServerGlobal.Setting.OneTimeOnlyDelayTime)
                                         < dtNow)
                                    {//생성시간 + 딜레이 시간 보다 지금 시간이 이후다

                                        //새로 토큰을 생성
                                        bNew = true;
                                    }
                                    else
                                    {//아니면
                                     
                                        //기존 토큰 사용
                                        bNew = false;
                                    }   
                                }
                            }
                            else
                            {//토큰이 만료 됐다.

                                //새로 토큰을 생성
                                bNew = true;
                            }   
                        }
                        else
                        {//기존 토큰이 없다.

                            //새로 토큰을 생성
                            bNew = true;
                        }
                    }
                    break;

                case RefreshTokenUsageType.None:
                default:
                    break;
            }

            if (true == bNew)
            {//토큰을 새로 생성해야 한다.

                while (true)
                {
                    sReturn = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
                    if (false == this.RefreshTokenGenerate_OverflowCheck(sReturn))
                    {//중복되지 않았다.
                        break;
                    }
                }


                //테이블에 저장한다.
                DgAuthRefreshToken newRT = new DgAuthRefreshToken()
                {
                    idUser = idUser,
                    Class = sClass,
                    RefreshToken = sReturn,
                    GenerateTime = dtNow,
                    ExpiresTime
                        = DateTime.UtcNow
                            .AddSeconds(DGAuthServerGlobal.Setting
                                            .RefreshTokenLifetime),

                };
                newRT.ActiveCheck();
                //db에 추가
                db1.DGAuthServer_RefreshToken.Add(newRT);


                //기존 토큰 만료 처리
                //대상 검색
                IQueryable<DgAuthRefreshToken> iqFindRT
                    = db1.DGAuthServer_RefreshToken
                        .Where(w => w.idUser == idUser
                                && w.Class == sClass
                                && true == w.ActiveIs);
                //linq는 데이터를 수정할때는 좋은 솔류션이 아니다.
                //반복문으로 직접수정하는 것이 훨씬 성능에 도움이 된다.
                foreach (DgAuthRefreshToken itemURT in iqFindRT)
                {
                    //만료 시간을 기입함
                    itemURT.RevokeTime = dtNow;
                    itemURT.ActiveCheck();
                }

            }//end if (true == bNew)

            db1.SaveChanges();
        }//end using db1

        if (true == DGAuthServerGlobal.Setting.RefreshTokenCookie
            && null != response)
        {//쿠키 사용

            //리플레시 토큰은 저장한다.
            this.Cookie_RefreshToken(sReturn, response);
        }
            
        return sReturn;
    }

    /// <summary>
    /// 리플레시 토큰으로 유저 고유번호를 찾는다.
    /// <para>쿠키가 사용중이면 sRefreshToken는 무시되고 쿠키를 읽어 사용한다.</para>
    /// </summary>
    /// <param name="sRefreshToken"></param>
    /// <param name="sClass">이 토큰을 분류하기 위한 이름</param>
    /// <param name="request"></param>
    /// <returns>토큰이 유효하지 않으면 0</returns>
    public long RefreshTokenFindUser(
        string sRefreshToken
        , string sClass
        , HttpRequest? request)
    {
        long idUser = 0;
        string sTokenFinal = String.Empty;

        if (null != request
            && true == DGAuthServerGlobal.Setting.RefreshTokenCookie)
        {//쿠키 사용

            string? sTokenTemp
                = request.Cookies[DGAuthServerGlobal.Setting.RefreshTokenCookieName];
            if (null != sTokenTemp)
            {//검색된 값이 있으면 전달
                sTokenFinal = sTokenTemp.ToString();
            }
        }
        else
        {
            sTokenFinal = sRefreshToken;
        }


        using (DgAuthDbContext db1 = new DgAuthDbContext())
        {
            DgAuthRefreshToken? findRT
                = db1.DGAuthServer_RefreshToken
                    .Where(w => w.RefreshToken == sRefreshToken
                            && w.ActiveIs == true
                            && w.Class == sClass)
                    .FirstOrDefault();

            if (null != findRT)
            {//찾음

                //토큰이 유효한지 확인
                findRT.ActiveCheck();
                if (true == findRT.ActiveIs)
                {//유효하다
                    idUser = findRT.idUser;
                }
                else
                {//아니다.
                    idUser = 0;
                }
            }
            else
            {
                idUser = 0;
            }

            db1.SaveChanges();
        }//end using db1

        return idUser;
    }

    /// <summary>
    /// 리프레시토큰을 만료 처리한다.
    /// </summary>
    /// <param name="bAllRevoke">클래스와 상관없이 전체 리플레시 토큰을 리보크한다.</param>
    /// <param name="idUser"></param>
    /// <param name="sClass">이 토큰을 분류하기 위한 이름</param>
    /// <param name="response"></param>
    public void RefreshTokenRevoke(
        bool bAllRevoke
        , long idUser
        , string sClass
        , HttpResponse? response)
    {
        //지금 시간
        DateTime dtNow = DateTime.Now;

        using (DgAuthDbContext db1 = new DgAuthDbContext())
        {

            IQueryable<DgAuthRefreshToken> iqFindRT;

            if (true == bAllRevoke)
            {//전체 리보크
                iqFindRT = db1.DGAuthServer_RefreshToken
                        .Where(w => w.idUser == idUser);
            }
            else
            {
                iqFindRT = db1.DGAuthServer_RefreshToken
                        .Where(w => w.idUser == idUser
                                && w.Class == sClass);
            }


            //linq는 데이터를 수정할때는 좋은 솔류션이 아니다.
            //반복문으로 직접수정하는 것이 훨씬 성능에 도움이 된다.
            foreach (DgAuthRefreshToken itemURT in iqFindRT)
            {
                //만료 시간을 기입함
                itemURT.RevokeTime = dtNow;
                itemURT.ActiveCheck();
            }

            db1.SaveChanges();
        }//end using db1


        if (true == DGAuthServerGlobal.Setting.RefreshTokenCookie
            && null != response)
        {//쿠키 사용

            //리플레시 토큰을 빈값으로 저장한다.
            this.Cookie_RefreshToken(String.Empty, response);
        }
    }


    /// <summary>
    /// HttpContext.User의 클레임을 검색하여 유저 고유정보를 받는다.
    /// </summary>
    /// <param name="claimsPrincipal"></param>
    /// <returns></returns>
    public long? ClaimDataGet(ClaimsPrincipal claimsPrincipal)
    {
        //인증정보 확인
        long nUser = 0;
        foreach (Claim item in claimsPrincipal.Claims.ToArray())
        {
            if (item.Type == DGAuthServerGlobal.Setting.UserIdName)
            {//인증 정보가 있다.
                nUser = Convert.ToInt64(item.Value);
                break;
            }
        }

        return nUser;
    }

    /// <summary>
    /// 전달된 토큰이 중복되어있는지 확인
    /// </summary>
    /// <param name="sToken"></param>
    /// <returns>true:중복</returns>
    private bool RefreshTokenGenerate_OverflowCheck(string sToken)
    {
        bool bReturn = false;

        using (DgAuthDbContext db1 = new DgAuthDbContext())
        {
            //살아있는 토큰중 같은 토큰 있는지 검사
            DgAuthRefreshToken? findRT
                = db1.DGAuthServer_RefreshToken
                    .Where(w => w.RefreshToken == sToken
                            && w.ActiveIs == true)
                    .FirstOrDefault();

            if (null != findRT)
            {//검색된 값이 있다.

                //유효성검사를 해주고
                findRT.ActiveCheck();
                if (true == findRT.ActiveIs)
                {//아직도 살아있다.

                    //중복
                    bReturn = true;
                }
                else
                {
                    bReturn = false;
                }

                //유효성 검사 저장
                db1.SaveChanges();
            }
            else
            {
                bReturn = false;
            }
            
        }//end using db1

        return bReturn;
    }//end RefreshTokenGenerate()

    /// <summary>
    /// 쿠키에 엑세스 토큰 저장을 요청한다.
    /// </summary>
    /// <param name="sToken"></param>
    /// <param name="response">추가 처리를 위한 리스폰스</param>
    public void Cookie_AccessToken(string sToken, HttpResponse response)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow
                        .AddSeconds(DGAuthServerGlobal.Setting.AccessTokenLifetime)
        };
        response.Cookies.Append(
            DGAuthServerGlobal.Setting.AccessTokenCookieName
            , sToken
            , cookieOptions);
    }

    /// <summary>
    /// 쿠기에 리플레이 토큰 저장을 요청한다.
    /// </summary>
    /// <param name="sToken"></param>
    /// <param name="response">추가 처리를 위한 리스폰스</param>
    public void Cookie_RefreshToken(string sToken, HttpResponse response)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow
                        .AddSeconds(DGAuthServerGlobal.Setting.RefreshTokenLifetime)
        };
        response.Cookies.Append(
            DGAuthServerGlobal.Setting.RefreshTokenCookieName
            , sToken
            , cookieOptions);
    }

    /// <summary>
    /// 가지고 있는 리플레시 토큰의 만료여부를 확인하고
    /// 만료된 토큰을 DB에서 지운다.
    /// </summary>
    public void DbClear()
    {
        //테이블 생성
        using (DgAuthDbContext db1 = new DgAuthDbContext())
        {
            //이미 만료되지 않은 리스트 검색
            IQueryable<DgAuthRefreshToken> findList
                = db1.DGAuthServer_RefreshToken
                    .Where(w => w.ActiveIs == true);

            //토큰 체크
            foreach (DgAuthRefreshToken itemRT in findList)
            {
                itemRT.ActiveCheck();
            }

            //체크된걸 저장하고
            db1.SaveChanges();


            //이미 만료된 리스트 검색
            IQueryable<DgAuthRefreshToken> findEndList
                = db1.DGAuthServer_RefreshToken
                    .Where(w => w.ActiveIs == true);
            //지우기
            db1.DGAuthServer_RefreshToken.RemoveRange(findList);
        }

        //정리한 시간 기록
        DGAuthServerGlobal.DbClearTime = DateTime.Now;
        //다은 예정시간 계산
        DGAuthServerGlobal.DbClearExpectedTime
            = DGAuthServerGlobal.DbClearTime
                .AddSeconds(DGAuthServerGlobal.Setting.DbClearTime);

    }

    /// <summary>
    /// 메모리 캐쉬에 시크릿 키를 저장한다.
    /// </summary>
    /// <param name="idUser"></param>
    /// <param name="sSecret"></param>
    private void MemoryCacheSaveSecret(long idUser, string sSecret)
    {
        //메모리 캐쉬에 저장한다.
        ICacheEntry cacheEntry
            = DGAuthServerGlobal.MemoryCache!
                .CreateEntry(DGAuthServerGlobal.Setting.AccessTokenCookieName + idUser);
        cacheEntry.SetValue(sSecret);
    }

    /// <summary>
    /// 메모리 캐쉬에서 시크릿 키를 찾아 리턴한다.
    /// </summary>
    /// <param name="idUser"></param>
    /// <param name="sSecret"></param>
    /// <returns></returns>
    private bool MemoryCacheFindSecret(long idUser, out string sSecret)
    {
        bool bReturn = false;

        bReturn
            = DGAuthServerGlobal.MemoryCache!
                .TryGetValue(DGAuthServerGlobal.Setting.AccessTokenCookieName + idUser
                                , out sSecret);
        if (null == sSecret)
        {
            sSecret = string.Empty;
        }
        
        return bReturn;
    }
}