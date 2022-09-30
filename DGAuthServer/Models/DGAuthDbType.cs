using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGAuthServer.Models;

/// <summary>
/// 사용할 수 있는 DB의 타입
/// </summary>
public enum DGAuthDbType
{
	/// <summary>
	/// In Memory
	/// </summary>
	Memory = 0,

	/// <summary>
	/// Sqlite
	/// </summary>
	Sqlite,

	/// <summary>
	/// MS Sql
	/// </summary>
	Mssql,

	
}
