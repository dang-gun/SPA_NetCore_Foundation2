
using DGAuthServer.Models;
using DGAuthServer.ModelsDB;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace DGAuthServer;


/// <summary>
/// ��ū ó���� ���� ����
/// </summary>
public class DGAuthServerService
{
    /// <summary>
    /// ������ ��ū ����
    /// </summary>
    /// <param name="idUser">���� ������ ���� ���� ��ȣ</param>
    /// <param name="sClass">�� ��ū�� �з��ϱ� ���� �̸�</param>
    /// <param name="response">�߰� ó���� ���� ��������</param>
    /// <returns></returns>
    public string AccessTokenGenerate(
        long idUser
        , string sClass
        , HttpResponse? response)
    {

        //��ũ�� Ű �ӽ� ����
        string sSecret = string.Empty;
        
        if (true == DGAuthServerGlobal.Setting.SecretAlone)
        {//ȥ�ڻ���ϴ� ��ũ��

            DgAuthAccessToken? findAT = null;

            using (DgAuthDbContext db1 = new DgAuthDbContext())
            {
                findAT 
                    = db1.DGAuthServer_AccessToken
                        .Where(w => w.idUser == idUser
                                && w.Class == sClass)
                        .FirstOrDefault();

                if (null != findAT)
                {//����ϴ� ��ũ���� �ִ�.

                    //����
                    sSecret = findAT.Secret;
                }
                else
                {//����ϴ� ��ũ���� ����.
                    DgAuthAccessToken newAT = new DgAuthAccessToken();
                    //����� �Է�
                    newAT.idUser = idUser;
                    newAT.Class = sClass;
                    //���ο� ��ũ���� �����Ѵ�.
                    sSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                    newAT.Secret = sSecret;

                    //db�� ����
                    db1.Add(newAT);
                    db1.SaveChanges();
                }
            }//end using db1
        }
        else
        {
            //���� ��ũ���� ����.
            sSecret = DGAuthServerGlobal.Setting.Secret!;
        }

        //��ũ���� ����Ʈ �迭�� ��ȯ
        byte[] byteSecretKey = new byte[0];
        byteSecretKey = Encoding.ASCII.GetBytes(sSecret);


        //jwt ���� ��ū �ڵ鷯
        JwtSecurityTokenHandler tokenHandler 
            = new JwtSecurityTokenHandler();

        //������Ű ������ �ʿ��� ������ �Է��Ѵ�.
        SecurityTokenDescriptor tokenDescriptor 
            = new SecurityTokenDescriptor
            {
                //�߰��� ���� ����(Ŭ����)
                Subject = new ClaimsIdentity(
                            new[] { new Claim(DGAuthServerGlobal.Setting.UserIdName
                                            , idUser.ToString()) }),
                //��ȿ�Ⱓ
                Expires = DateTime.UtcNow.AddSeconds(DGAuthServerGlobal.Setting.AccessTokenLifetime),
                //��ȣȭ ���
                SigningCredentials 
                    = new SigningCredentials(new SymmetricSecurityKey(byteSecretKey)
                                                , SecurityAlgorithms.HmacSha256Signature)
            };

        //��ū ����
        SecurityToken token 
            = tokenHandler.CreateToken(tokenDescriptor);

        //����ȭ
        StringBuilder sbToken = new StringBuilder();
        if (true == DGAuthServerGlobal.Setting.SecretAlone)
        {//ȥ �ڻ���ϴ� ��ũ��

            //�Ǿտ� ���� ������ȣ�� �ٿ��ش�.
            sbToken.Append(idUser);
            //���� ��ȣ
            sbToken.Append(DGAuthServerGlobal.Setting.SecretAloneDelimeter);
        }

        //������� ��ū �߰�
        sbToken.Append(tokenHandler.WriteToken(token));



        if (true == DGAuthServerGlobal.Setting.AccessTokenCookie
            && null != response)
        {//��Ű ���

            //���÷��� ��ū�� �����Ѵ�.
            this.Cookie_AccessToken(sbToken.ToString(), response);
        }

        return sbToken.ToString();
    }

    /// <summary>
    /// ������ ��ū Ȯ��.
    /// <para>��Ű�� ������̸� sToken�� ���õǰ� ��Ű�� �о� ����Ѵ�.</para>
    /// </summary>
    /// <remarks>�̵������� ȣ���ؼ� ����Ѵ�.</remarks>
    /// <param name="sToken"></param>
    /// <param name="sClass"></param>
    /// <param name="request"></param>
    /// <returns>ã�Ƴ� idUser, ��ū ��ü�� ������ -1, ��ū�� ��ȿ���� ������ 0 </returns>
    public long AccessTokenValidate(
        string sToken
        , string sClass
        , HttpRequest? request)
    {
        string sTokenFinal = String.Empty;

        if (null != request
            && true == DGAuthServerGlobal.Setting.AccessTokenCookie)
        {//��Ű ���

            string? sTokenTemp
                = request.Cookies[DGAuthServerGlobal.Setting.AccessTokenCookieName];
            if (null != sTokenTemp)
            {//�˻��� ���� ������ ����
                sTokenFinal = sTokenTemp.ToString();
            }
        }
        else
        {
            sTokenFinal = sToken;
        }


        if (string.Empty == sTokenFinal)
        {//���޵� ��ū ���� ����.
            return -1;
        }


        //���� ���� ��ū���� ��ū ������ ����� ������
        string sTokenCut = string.Empty;
        //ã�Ƴ� ���� ��ȣ
        long idUser = 0;
        //��ũ�� Ű �ӽ� ����
        string sSecret = string.Empty;

        //����� ��ũ�� Ű ã��
        if (true == DGAuthServerGlobal.Setting.SecretAlone)
		{//ȥ�ڻ���ϴ� ��ũ��

            //ù��° �����ڸ� ã�´�.
            int nUser = sTokenFinal.IndexOf(DGAuthServerGlobal.Setting.SecretAloneDelimeter);

            if (0 > nUser)
            {//������ ��ū�� ���� ��ȣ�� ����.
                return 0;
            }


            //ã�� ������ ��ġ�� ���� ���̵� �ڸ���.
            string sCutUser = sTokenFinal.Substring(0, nUser);

			if (false == Int64.TryParse(sCutUser, out idUser))
			{//���ڷ� ��ȯ�� �� ����.
				return 0;
			}
			else if (0 >= idUser)
			{//���� ������ ����.
                
                //���� ������ �߸� �Ǿ���.
				return 0;
			}

            //�߸� �����Ϳ��� ��ū ������ ����
            //������ ������ġ = ã�� ������ ��ġ + ������ ũ��
            sTokenCut = sTokenFinal.Substring(
                            nUser + DGAuthServerGlobal.Setting.SecretAloneDelimeter.Length);

            //ã�� ��������ū ������
            DgAuthAccessToken? findAT = null;

            //����� ��ũ�� �˻�
            using (DgAuthDbContext db1 = new DgAuthDbContext())
            {
                findAT
                    = db1.DGAuthServer_AccessToken
                        .Where(w => w.idUser == idUser
                                && w.Class == sClass)
                        .FirstOrDefault();

            }//end using db1

            if (null == findAT
                || string.Empty == findAT.Secret)
            {//��ũ�� ������ ����.
                return 0;
            }

            //ã�� ��ũ�� ����
            sSecret = findAT.Secret;
        }
		else
		{
			sTokenCut = sTokenFinal;
            sSecret = DGAuthServerGlobal.Setting.Secret!;

        }



        //���� �м� ���� ****************
		JwtSecurityTokenHandler tokenHandler 
            = new JwtSecurityTokenHandler();
        //���� ��ũ���� ����Ʈ �迭�� ��ȯ
        byte[] byteKey 
            = Encoding.ASCII.GetBytes(sSecret);
        try
        {
            //��ū �ؼ��� �����Ѵ�.
            tokenHandler.ValidateToken(sTokenCut, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(byteKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;
            //ã�� ũ���ӿ��� ���� ������ȣ�� �����Ѵ�.
            long accountId 
                = Int64.Parse(jwtToken.Claims
                                .First(x => x.Type == DGAuthServerGlobal.Setting.UserIdName)
                                .Value);

            //ã�� ���̵�
            return accountId;
        }
        catch
        {//��ū�� �ؼ����� ���ߴ�.
            
            //��ū�� ����������� �ؼ����� ���ϸ� 0 ������ ��������
            //��Ȳ�� ���� ���÷��� ��ū���� ��ū�� �����ϵ��� �˷��� �Ѵ�.
            return 0;
        }
    }

    /// <summary>
    /// ������ ��ū�� �����Ŵ
    /// </summary>
    /// <remarks>
    /// ������ ��ū�� �����ų ����� ����<br />
    /// ���� ��ũ�� Ű�� ������϶��� ����ũ�� �����ϴ�.<br />
    /// ��Ű�� ������̶�� ��Ű�� �����ִ� ����� �Ѵ�.<br />
    /// bAllRevoke�� true��� ���� ��ũ�� Ű�� ��߱� �Ǹ鼭 
    /// ���� ��������ū�� ����� �� ���� �ȴ�.
    /// <para>
    /// ���� ����Ʈ(Ȥ�� ���α׷�)���� �ϳ��� ���������� �ΰ� ����Ұ�� 
    /// idUser�� ��ġ�� �ٸ� ����Ʈ������ ��������ū�� ����ǰ� �ȴ�.
    /// </para>
    /// </remarks>
    /// 
    /// <param name="bAllRevoke">Ŭ������ ������� ��ü ������ ��ū�� ����ũ�Ѵ�.<br />
    /// ���� ��ũ���� ������̶�� false�϶��� ���� ��ũ���� ������ �ʴ´�.
    /// </param>
    /// <param name="idUser"></param>
    /// <param name="sClass">�� ��ū�� �з��ϱ� ���� �̸�</param>
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
            {//��ü �˻�
                iqFindAT
                    = db1.DGAuthServer_AccessToken
                        .Where(w => w.idUser == idUser);
            }
            else
            {//��� �˻�
                iqFindAT
                    = db1.DGAuthServer_AccessToken
                        .Where(w => w.idUser == idUser
                                && w.Class == sClass);
            }

            //����� ������ �����.
            //���� ��ũ���� ������̶�� ��������ū�� ��û�ϸ鼭 �ٽ� �����ȴ�.
            //�׷��� ���⼭�� ����⸸ �ϸ�ȴ�.
            db1.DGAuthServer_AccessToken.RemoveRange(iqFindAT);

            db1.SaveChanges();
        }//end using db1


        if (true == DGAuthServerGlobal.Setting.AccessTokenCookie
            && null != response)
        {//��Ű ���

            //���÷��� ��ū�� �����Ѵ�.
            this.Cookie_AccessToken(string.Empty, response);
        }
    }


    /// <summary>
    /// ���÷��� ��ū ����.
    /// </summary>
    /// <remarks>�ߺ��˻� �� ��ȿ���˻縦 �ϰ��� ���ǿ� �´� ��ū�� ����(����)�Ѵ�.</remarks>
    /// <param name="typeUsage"></param>
    /// <param name="idUser">�� ��ū�� ������ ������ ������ȣ</param>
    /// <param name="sClass">�� ��ū�� �з��ϱ� ���� �̸�</param>
    /// <param name="response">�߰� ó���� ���� ��������</param>
    /// <returns></returns>
    public string RefreshTokenGenerate(
        RefreshTokenUsageType? typeUsage
        , long idUser
        , string sClass
        , HttpResponse? response)
    {
        string sReturn = string.Empty;

        //���� �ð�
        DateTime dtNow = DateTime.Now;

        RefreshTokenUsageType typeNewTokenFinal = RefreshTokenUsageType.None;
        if (null != typeUsage)
        {//���޵� �ɼ��� �ִ�.
            typeNewTokenFinal = (RefreshTokenUsageType)typeUsage;
        }
        else
        {
            //������ ������ �о� ����Ѵ�.
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
                        //���� ��ū�� ����
                        bNew = true;
                    }
                    break;

                case RefreshTokenUsageType.OneTimeOnlyDelay:
                case RefreshTokenUsageType.ReUse:
                case RefreshTokenUsageType.ReUseAddTime:
                    {
                        //���� ��ū�� �ִ��� ã�´�.
                        DgAuthRefreshToken? findRT
                            = db1.DGAuthServer_RefreshToken
                                .Where(w => w.idUser == idUser
                                        && w.Class == sClass)
                                .FirstOrDefault();

                        if (null != findRT)
                        {//���� ��ū�� �ִ�.

                            //��ȿ���� ���� ��ġ��
                            findRT.ActiveCheck();

                            if (true == findRT.ActiveIs)
                            {//��ū�� ��ȿ�ϴ�.

                                //����ϴ� ��ū ����
                                sReturn = findRT.RefreshToken;

                                if (RefreshTokenUsageType.ReUseAddTime == typeNewTokenFinal)
                                {
                                    //���� �ð��� �·��ش�.
                                    findRT.ExpiresTime
                                        = DateTime.UtcNow
                                            .AddSeconds(DGAuthServerGlobal.Setting
                                                            .RefreshTokenLifetime);
                                }
                                else if (RefreshTokenUsageType.OneTimeOnlyDelay == typeNewTokenFinal)
                                {

                                    //���� ��ū�� ������¥�� Ȯ���Ѵ�.
                                    if (findRT.GenerateTime.AddSeconds(DGAuthServerGlobal.Setting.OneTimeOnlyDelayTime)
                                         < dtNow)
                                    {//�����ð� + ������ �ð� ���� ���� �ð��� ���Ĵ�

                                        //���� ��ū�� ����
                                        bNew = true;
                                    }
                                    else
                                    {//�ƴϸ�
                                     
                                        //���� ��ū ���
                                        bNew = false;
                                    }   
                                }
                            }
                            else
                            {//��ū�� ���� �ƴ�.

                                //���� ��ū�� ����
                                bNew = true;
                            }   
                        }
                        else
                        {//���� ��ū�� ����.

                            //���� ��ū�� ����
                            bNew = true;
                        }
                    }
                    break;

                case RefreshTokenUsageType.None:
                default:
                    break;
            }

            if (true == bNew)
            {//��ū�� ���� �����ؾ� �Ѵ�.

                while (true)
                {
                    sReturn = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
                    if (false == this.RefreshTokenGenerate_OverflowCheck(sReturn))
                    {//�ߺ����� �ʾҴ�.
                        break;
                    }
                }


                //���̺� �����Ѵ�.
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
                //db�� �߰�
                db1.DGAuthServer_RefreshToken.Add(newRT);


                //���� ��ū ���� ó��
                //��� �˻�
                IQueryable<DgAuthRefreshToken> iqFindRT
                    = db1.DGAuthServer_RefreshToken
                        .Where(w => w.idUser == idUser
                                && w.Class == sClass
                                && true == w.ActiveIs);
                //linq�� �����͸� �����Ҷ��� ���� �ַ����� �ƴϴ�.
                //�ݺ������� ���������ϴ� ���� �ξ� ���ɿ� ������ �ȴ�.
                foreach (DgAuthRefreshToken itemURT in iqFindRT)
                {
                    //���� �ð��� ������
                    itemURT.RevokeTime = dtNow;
                    itemURT.ActiveCheck();
                }

            }//end if (true == bNew)

            db1.SaveChanges();
        }//end using db1

        if (true == DGAuthServerGlobal.Setting.RefreshTokenCookie
            && null != response)
        {//��Ű ���

            //���÷��� ��ū�� �����Ѵ�.
            this.Cookie_RefreshToken(sReturn, response);
        }
            
        return sReturn;
    }

    /// <summary>
    /// ���÷��� ��ū���� ���� ������ȣ�� ã�´�.
    /// <para>��Ű�� ������̸� sRefreshToken�� ���õǰ� ��Ű�� �о� ����Ѵ�.</para>
    /// </summary>
    /// <param name="sRefreshToken"></param>
    /// <param name="sClass">�� ��ū�� �з��ϱ� ���� �̸�</param>
    /// <param name="request"></param>
    /// <returns>��ū�� ��ȿ���� ������ 0</returns>
    public long RefreshTokenFindUser(
        string sRefreshToken
        , string sClass
        , HttpRequest? request)
    {
        long idUser = 0;
        string sTokenFinal = String.Empty;

        if (null != request
            && true == DGAuthServerGlobal.Setting.RefreshTokenCookie)
        {//��Ű ���

            string? sTokenTemp
                = request.Cookies[DGAuthServerGlobal.Setting.RefreshTokenCookieName];
            if (null != sTokenTemp)
            {//�˻��� ���� ������ ����
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
            {//ã��

                //��ū�� ��ȿ���� Ȯ��
                findRT.ActiveCheck();
                if (true == findRT.ActiveIs)
                {//��ȿ�ϴ�
                    idUser = findRT.idUser;
                }
                else
                {//�ƴϴ�.
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
    /// ����������ū�� ���� ó���Ѵ�.
    /// </summary>
    /// <param name="bAllRevoke">Ŭ������ ������� ��ü ���÷��� ��ū�� ����ũ�Ѵ�.</param>
    /// <param name="idUser"></param>
    /// <param name="sClass">�� ��ū�� �з��ϱ� ���� �̸�</param>
    /// <param name="response"></param>
    public void RefreshTokenRevoke(
        bool bAllRevoke
        , long idUser
        , string sClass
        , HttpResponse? response)
    {
        //���� �ð�
        DateTime dtNow = DateTime.Now;

        using (DgAuthDbContext db1 = new DgAuthDbContext())
        {

            IQueryable<DgAuthRefreshToken> iqFindRT;

            if (true == bAllRevoke)
            {//��ü ����ũ
                iqFindRT = db1.DGAuthServer_RefreshToken
                        .Where(w => w.idUser == idUser);
            }
            else
            {
                iqFindRT = db1.DGAuthServer_RefreshToken
                        .Where(w => w.idUser == idUser
                                && w.Class == sClass);
            }


            //linq�� �����͸� �����Ҷ��� ���� �ַ����� �ƴϴ�.
            //�ݺ������� ���������ϴ� ���� �ξ� ���ɿ� ������ �ȴ�.
            foreach (DgAuthRefreshToken itemURT in iqFindRT)
            {
                //���� �ð��� ������
                itemURT.RevokeTime = dtNow;
                itemURT.ActiveCheck();
            }

            db1.SaveChanges();
        }//end using db1


        if (true == DGAuthServerGlobal.Setting.RefreshTokenCookie
            && null != response)
        {//��Ű ���

            //���÷��� ��ū�� ������ �����Ѵ�.
            this.Cookie_RefreshToken(String.Empty, response);
        }
    }


    /// <summary>
    /// HttpContext.User�� Ŭ������ �˻��Ͽ� ���� ���������� �޴´�.
    /// </summary>
    /// <param name="claimsPrincipal"></param>
    /// <returns></returns>
    public long? ClaimDataGet(ClaimsPrincipal claimsPrincipal)
    {
        //�������� Ȯ��
        long nUser = 0;
        foreach (Claim item in claimsPrincipal.Claims.ToArray())
        {
            if (item.Type == DGAuthServerGlobal.Setting.UserIdName)
            {//���� ������ �ִ�.
                nUser = Convert.ToInt64(item.Value);
                break;
            }
        }

        return nUser;
    }

    /// <summary>
    /// ���޵� ��ū�� �ߺ��Ǿ��ִ��� Ȯ��
    /// </summary>
    /// <param name="sToken"></param>
    /// <returns>true:�ߺ�</returns>
    private bool RefreshTokenGenerate_OverflowCheck(string sToken)
    {
        bool bReturn = false;

        using (DgAuthDbContext db1 = new DgAuthDbContext())
        {
            //����ִ� ��ū�� ���� ��ū �ִ��� �˻�
            DgAuthRefreshToken? findRT
                = db1.DGAuthServer_RefreshToken
                    .Where(w => w.RefreshToken == sToken
                            && w.ActiveIs == true)
                    .FirstOrDefault();

            if (null != findRT)
            {//�˻��� ���� �ִ�.

                //��ȿ���˻縦 ���ְ�
                findRT.ActiveCheck();
                if (true == findRT.ActiveIs)
                {//������ ����ִ�.

                    //�ߺ�
                    bReturn = true;
                }
                else
                {
                    bReturn = false;
                }

                //��ȿ�� �˻� ����
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
    /// ��Ű�� ������ ��ū ������ ��û�Ѵ�.
    /// </summary>
    /// <param name="sToken"></param>
    /// <param name="response">�߰� ó���� ���� ��������</param>
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
    /// ��⿡ ���÷��� ��ū ������ ��û�Ѵ�.
    /// </summary>
    /// <param name="sToken"></param>
    /// <param name="response">�߰� ó���� ���� ��������</param>
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
    /// ������ �ִ� ���÷��� ��ū�� ���Ῡ�θ� Ȯ���ϰ�
    /// ����� ��ū�� DB���� �����.
    /// </summary>
    public void DbClear()
    {
        //���̺� ����
        using (DgAuthDbContext db1 = new DgAuthDbContext())
        {
            //�̹� ������� ���� ����Ʈ �˻�
            IQueryable<DgAuthRefreshToken> findList
                = db1.DGAuthServer_RefreshToken
                    .Where(w => w.ActiveIs == true);

            //��ū üũ
            foreach (DgAuthRefreshToken itemRT in findList)
            {
                itemRT.ActiveCheck();
            }

            //üũ�Ȱ� �����ϰ�
            db1.SaveChanges();


            //�̹� ����� ����Ʈ �˻�
            IQueryable<DgAuthRefreshToken> findEndList
                = db1.DGAuthServer_RefreshToken
                    .Where(w => w.ActiveIs == true);
            //�����
            db1.DGAuthServer_RefreshToken.RemoveRange(findList);
        }

        //������ �ð� ���
        DGAuthServerGlobal.DbClearTime = DateTime.Now;
        //���� �����ð� ���
        DGAuthServerGlobal.DbClearExpectedTime
            = DGAuthServerGlobal.DbClearTime
                .AddSeconds(DGAuthServerGlobal.Setting.DbClearTime);

    }
}