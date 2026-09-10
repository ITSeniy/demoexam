using Microsoft.Data.SqlClient;
using ObuVApp.Models;

namespace ObuVApp.Database;

public static class UserRepository
{
    public static User? Login(string login, string password)
    {
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand(@"
            SELECT п.ID, п.РольID, р.Название, п.ФИО, п.Логин, п.Пароль
            FROM Пользователи п
            JOIN Роли р ON р.ID = п.РольID
            WHERE п.Логин = @login AND п.Пароль = @pwd", con);
        cmd.Parameters.AddWithValue("@login", login);
        cmd.Parameters.AddWithValue("@pwd", password);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new User
        {
            ID = r.GetInt32(0),
            РольID = r.GetInt32(1),
            Роль = r.GetString(2),
            ФИО = r.GetString(3),
            Логин = r.GetString(4),
            Пароль = r.GetString(5),
        };
    }
}
