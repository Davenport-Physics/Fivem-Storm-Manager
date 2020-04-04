
using System.Configuration;
using System.Data.SQLite;

namespace FivemStormManager
{
    class DbAccess
    {
        private readonly string connection_string    = ConfigurationManager.AppSettings["DbAccessConnectionString"];
        private readonly SQLiteConnection connection = null;
        public DbAccess()
        {
            connection = new SQLiteConnection(this.connection_string);
            connection.Open();
            CreateLogTableIfNeeded();
        }

        public void Log(string message)
        {
            SQLiteCommand cmd = new SQLiteCommand(this.connection);
            cmd.CommandText   = @"INSERT INTO log (Log) VALUES (@Log)";
            cmd.Parameters.AddWithValue("@Log", message);
            cmd.ExecuteNonQuery();
        }

        private void CreateLogTableIfNeeded()
        {
            SQLiteCommand cmd = new SQLiteCommand(this.connection);
            cmd.CommandText   = @"CREATE TABLE IF NOT EXISTS log(LogId INTEGER PRIMARY KEY, TimeStamp DATETIME DEFAULT(datetime(CURRENT_TIMESTAMP, 'localtime')), Log TEXT NOT NULL);";
            cmd.ExecuteNonQuery();
        }

        ~DbAccess()
        {
            if (this.connection != null)
                connection.Close();
        }
    }
}
