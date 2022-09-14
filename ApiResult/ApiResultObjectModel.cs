using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiResult;

/// <summary>
/// 스웨거에 노출시키지 않고 모델을 리턴할때 사용한다.
/// 테스트용으로 사용해도된다.(매번 새로운 모델을 만들기 힘들기 때문)
/// </summary>
public class ApiResultObjectModel : ApiResultBaseModel
{
	/// <summary>
	/// 전달할 오브젝트
	/// </summary>
	public object? ResultObject { get; set; } = null;


	/// <summary>
	/// 기본 생성
	/// </summary>
	public ApiResultObjectModel()
		: base()
	{

	}

	/// <summary>
	/// 리턴할 모델 지정하여 생성
	/// </summary>
	/// <param name="objResult"></param>
	public ApiResultObjectModel(object objResult)
		: base()
	{
		this.ResultObject = objResult;
	}

	/// <summary>
	/// 인포코드와 메시지를 넣고 생성
	/// </summary>
	/// <param name="sInfoCode"></param>
	/// <param name="sMessage"></param>
	public ApiResultObjectModel(string sInfoCode, string sMessage)
		: base(sInfoCode, sMessage)
	{
	}
}
