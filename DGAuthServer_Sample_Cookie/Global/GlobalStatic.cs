﻿using DGAuthServer.Models;
using System.Data;

namespace DGAuthServer_Sample.Global
{
	/// <summary>
	/// 이 프로젝트에서 사용할 전역 변수
	/// </summary>
	public static class GlobalStatic
	{
		/// <summary>
		/// DB 타입
		/// </summary>
		/// <remarks>저장전에 소문자로 변환해야 한다.</remarks>
		public static string DBType_String = "";
		/// <summary>
		/// DB 타입
		/// </summary>
		public static DGAuthDbType DBType = DGAuthDbType.Memory;
		/// <summary>
		/// DB 컨낵션 스트링 저장
		/// </summary>
		public static string DBString = "";

	}
}
