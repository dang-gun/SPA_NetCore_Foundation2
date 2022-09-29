using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGAuthServer.Models;

public enum DbType
{
	None = 0,

	/// <summary>
	/// MS Sql
	/// </summary>
	Mssql,

	/// <summary>
	/// Sqlite
	/// </summary>
	Sqlite,
}
