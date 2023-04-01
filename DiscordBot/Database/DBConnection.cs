using MySqlConnector;

namespace Data
{
    public class DBConnection
    {
        private DBConnection()
        {
        }

        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public MySqlConnection Connection { get; private set; }

        private static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        public MySqlConnection getConnection() 
        {
            string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}", Server, DatabaseName, UserName, Password);
            try
            {
                MySqlConnection connection = new MySqlConnection(connstring);
                connection.Open();
                return connection;
            } catch(Exception ex) 
            {
                Console.WriteLine("[DBError]: " + ex.Message);
                return null;
            }
        }
    }
}