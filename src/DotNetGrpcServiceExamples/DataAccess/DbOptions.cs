namespace NewVoiceMedia.DotNetGrpcServiceExamples.DataAccess
{
    public class DbOptions
    {
        public string AURORA_MYSQL_READ_HOST { get; set; }
        public string AURORA_MYSQL_PORT { get; set; }
        public string AURORA_MYSQL_READ_USER { get; set; }
        public string AURORA_MYSQL_READ_PASSWORD { get; set; }
        public string AURORA_MYSQL_WRITE_HOST { get; set; }
        public string AURORA_MYSQL_WRITE_USER { get; set; }
        public string AURORA_MYSQL_WRITE_PASSWORD { get; set; }

        private int DB_CONNECTION_LIFETIME = 5;
        private string DB_CON_STRING_TEMPLATE = "Server={0};Uid={1};Port={2};password={3};Treat Tiny As Boolean=false;Allow User Variables=true;ConnectionLifeTime={4};SslMode=None;CharSet=utf8mb4;";

        public string ReadConnectionString => string.Format(
                DB_CON_STRING_TEMPLATE,
                AURORA_MYSQL_READ_HOST,
                AURORA_MYSQL_READ_USER,
                AURORA_MYSQL_PORT,
                AURORA_MYSQL_READ_PASSWORD,
                DB_CONNECTION_LIFETIME);

        public string WriteConnectionString => string.Format(
                DB_CON_STRING_TEMPLATE,
                AURORA_MYSQL_WRITE_HOST,
                AURORA_MYSQL_WRITE_USER,
                AURORA_MYSQL_PORT,
                AURORA_MYSQL_WRITE_PASSWORD,
                DB_CONNECTION_LIFETIME);
    }
}