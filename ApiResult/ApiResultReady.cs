using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiResult;

/// <summary>
/// api요청을 처리할때 요청결과처리를 공통화 하는 클래스.
/// ApiResultFailModel를 공통으로 리턴하기 위해 베이스를 가지고 있다.
/// 결과 출력용으로 데이터는 외부로 부터 받아야 한다.
/// 외부에서는 ToResult를 이용하여 API 전달용 개체를 받는다.
/// </summary>
public class ApiResultReady
{
    /// <summary>
    /// 컨트롤러베이스의 기능을 쓰기위한 개체
    /// </summary>
    private ControllerBase ThisCB { get; set; }

    /// <summary>
    /// 전달받은 결과 오브젝트
    /// </summary>
    public ApiResultBaseModel ResultObject { get; set; }

    /// <summary>
    /// 성공 여부.
    /// 알수 없는 에러가 났을때 처리하기위한 용도.
    /// 알수 있는 에러는 이값을을 true로 해둔다.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 실패시 전달한 코드
    /// 0 : 성공.
    /// 다른 값은 모두 실패
    /// </summary>
    public string InfoCode
    {
        get
        {
            return this.ResultObject.InfoCode;
        }
        set
        {
            this.ResultObject.InfoCode = value;
        }
    }
    /// <summary>
    /// 전달할 메시지
    /// </summary>
    public string Message
    {
        get
        {
            return this.ResultObject.Message;
        }
        set
        {
            this.ResultObject.Message = value;
        }
    }

    /// <summary>
    /// API의 처음부분에서 선언한다.
    /// 'ApiResultBaseModel'로 생성합니다.
    /// </summary>
    /// <param name="cbThis">컨트롤러 기능을 사용하기위한 인스턴스</param>
    public ApiResultReady(ControllerBase cbThis)
    {
        this.ThisCB = cbThis;
        this.Success = true;

        this.ResultObject = new ApiResultBaseModel();
    }

    /// <summary>
    /// API의 처음부분에서 선언한다.
    /// </summary>
    /// <param name="cbThis">컨트롤러 기능을 사용하기위한 인스턴스</param>
    /// <param name="arModel">리턴에 사용할 모델</param>
    public ApiResultReady(
        ControllerBase cbThis
        , ApiResultBaseModel arModel )
    {
        this.ThisCB = cbThis;
        this.Success = true;

        //전달받은 모델 저장
        this.ResultObject = arModel;
    }


    /// <summary>
    /// Api 결과 정보 저장
    /// </summary>
    /// <param name="sInfoCode"></param>
    /// <param name="sMessage"></param>
    public void ApiResultInfoSet(string sInfoCode, string sMessage)
    {
        this.InfoCode = sInfoCode;
        this.Message = sMessage;
    }
    /// <summary>
    /// Api 결과 정보 저장
    /// </summary>
    /// <param name="typeInfoCode"></param>
    /// <param name="sMessage"></param>
    public void ApiResultInfoSet(ApiResultType typeInfoCode, string sMessage)
    {
        this.ApiResultInfoSet(typeInfoCode.ToString(), sMessage);
    }

    /// <summary>
    /// InfoCode값이 성공값인지 여부
    /// </summary>
    /// <returns></returns>
    public bool IsSuccess()
    {
        return this.ResultObject.IsSuccess();
    }


    /// <summary>
    /// API끝에서 호출한다.
    /// ApiResult를 생성하여 리턴해 준다.
    /// </summary>
    /// <returns></returns>
    public ObjectResult ToResult()
    {
        return this.ToResult(this.ResultObject);
    }


    /// <summary>
    /// API끝에서 호출하여 'ObjectResult'를 생성하여 리턴해 준다.
    /// 만들어지는 결과의 ApiResultBaseModel데이터는 this 기준이다.
    /// </summary>
    /// <param name="objResultData">전달할 모델</param>
    /// <returns></returns>
    public ObjectResult ToResult(object objResultData)
    {
        ObjectResult? orReturn = null;

        if (null == objResultData)
        {//오브젝트가 없다.
            //없으면 가지고 있는 오브젝트를 자동으로 사용한다.
            objResultData = this.ResultObject;
        }

        if (this.Success == true)
        {//성공
            //성공은 전달받은 오브젝트를 준다,
            orReturn = this.ThisCB.StatusCode(200, objResultData);
        }
        else
        {//실패
		 
			ApiResultObjectModel afm 
                = new ApiResultObjectModel(
                    ((ApiResultBaseModel)objResultData).InfoCode
                    , ((ApiResultBaseModel)objResultData).Message);

            //여기에 들어왔다는건 예측 가능한 오류가 났다는 의미다.
            //예측가능한 오류는 200으로 바꿔준다.
            orReturn = this.ThisCB.StatusCode(StatusCodes.Status200OK, afm);
            //여기서 예측가능한 오류를 200으로 바꾸지 않으려면 이 코드를 사용한다.
            //orReturn = this.ThisCB.StatusCode(this.StatusCode, afm);
        }

        return orReturn;
    }
}
