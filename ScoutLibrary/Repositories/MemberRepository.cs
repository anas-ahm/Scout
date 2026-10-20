using System;
using System.Collections.Generic;
using MySqlConnector;
using System.Text;

namespace ScoutLibrary.Repositories
{
    internal class MemberRepository
    {
        private MySqlConnection ConnectToDB()
        {
            DBWorker dbWorker = new DBWorker();

            MySqlConnection connection = dbWorker.Connect();

            return connection;
        }
    }
}
