using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiResult;

/// <summary>
/// 자주쓰는 API 결과 타입
/// </summary>
public enum ApiResultType
{
	/// <summary>
	/// 성공
	/// </summary>
	None = 0,


	/// <summary>
	/// 권한 체크중 오류 발생
	/// </summary>
	PermissionCheckError = 1000,
	/// <summary>
	/// 로그인 필요
	/// </summary>
	LoginNecessaryError = 1100,

	/// <summary>
	/// 대상을 찾지 못했다.
	/// </summary>
	TargetFindError = 2000,
}