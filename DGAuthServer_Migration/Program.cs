using DGAuthServer;

namespace DGAuthServer_Migration
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello, World!");



			//Add-Migration InitialCreate -Context DGAuthServer.ModelsDB.DgAuthDbContext -OutputDir Migrations
			DGAuthServerGlobal.DbType = DGAuthServer.Models.DbType.Sqlite;
			DGAuthServerGlobal.DbConnectString
				= "Data Source=Test.db";


		}
	}
}