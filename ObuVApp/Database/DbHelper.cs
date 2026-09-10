using Microsoft.Data.SqlClient;

namespace ObuVApp.Database;

public static class DbHelper
{
    public const string ConnectionString =
        "Server=localhost;Database=ObuVDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection GetConnection() => new(ConnectionString);
}
