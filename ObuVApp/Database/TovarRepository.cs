using Microsoft.Data.SqlClient;
using ObuVApp.Models;

namespace ObuVApp.Database;

public static class TovarRepository
{
    public static List<Tovar> GetAll()
    {
        var list = new List<Tovar>();
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand("SELECT * FROM Товары ORDER BY НаименованиеТовара", con);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public static Tovar? GetByArticul(string articul)
    {
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand("SELECT * FROM Товары WHERE Артикул = @a", con);
        cmd.Parameters.AddWithValue("@a", articul);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Map(r) : null;
    }

    public static List<string> GetCategories()
    {
        var list = new List<string>();
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand("SELECT DISTINCT КатегорияТовара FROM Товары ORDER BY КатегорияТовара", con);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(r.GetString(0));
        return list;
    }

    public static void Insert(Tovar t)
    {
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand(@"
            INSERT INTO Товары (Артикул, НаименованиеТовара, ЕдиницаИзмерения, Цена, Поставщик, Производитель,
                КатегорияТовара, ДействующаяСкидка, КолвоНаСкладе, ОписаниеТовара, Фото)
            VALUES (@art, @name, @unit, @price, @sup, @mfr, @cat, @disc, @qty, @desc, @photo)", con);
        SetParams(cmd, t);
        cmd.ExecuteNonQuery();
    }

    public static void Update(Tovar t)
    {
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand(@"
            UPDATE Товары SET
                НаименованиеТовара = @name, ЕдиницаИзмерения = @unit, Цена = @price,
                Поставщик = @sup, Производитель = @mfr, КатегорияТовара = @cat,
                ДействующаяСкидка = @disc, КолвоНаСкладе = @qty,
                ОписаниеТовара = @desc, Фото = @photo
            WHERE Артикул = @art", con);
        SetParams(cmd, t);
        cmd.ExecuteNonQuery();
    }

    public static void Delete(string articul)
    {
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand("DELETE FROM Товары WHERE Артикул = @a", con);
        cmd.Parameters.AddWithValue("@a", articul);
        cmd.ExecuteNonQuery();
    }

    private static void SetParams(SqlCommand cmd, Tovar t)
    {
        cmd.Parameters.AddWithValue("@art", t.Артикул);
        cmd.Parameters.AddWithValue("@name", t.НаименованиеТовара);
        cmd.Parameters.AddWithValue("@unit", t.ЕдиницаИзмерения);
        cmd.Parameters.AddWithValue("@price", t.Цена);
        cmd.Parameters.AddWithValue("@sup", t.Поставщик);
        cmd.Parameters.AddWithValue("@mfr", t.Производитель);
        cmd.Parameters.AddWithValue("@cat", t.КатегорияТовара);
        cmd.Parameters.AddWithValue("@disc", t.ДействующаяСкидка);
        cmd.Parameters.AddWithValue("@qty", t.КолвоНаСкладе);
        cmd.Parameters.AddWithValue("@desc", (object?)t.ОписаниеТовара ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@photo", (object?)t.Фото ?? DBNull.Value);
    }

    private static Tovar Map(SqlDataReader r) => new()
    {
        Артикул = r.GetString(r.GetOrdinal("Артикул")),
        НаименованиеТовара = r.GetString(r.GetOrdinal("НаименованиеТовара")),
        ЕдиницаИзмерения = r.GetString(r.GetOrdinal("ЕдиницаИзмерения")),
        Цена = r.GetDecimal(r.GetOrdinal("Цена")),
        Поставщик = r.GetString(r.GetOrdinal("Поставщик")),
        Производитель = r.GetString(r.GetOrdinal("Производитель")),
        КатегорияТовара = r.GetString(r.GetOrdinal("КатегорияТовара")),
        ДействующаяСкидка = r.GetInt32(r.GetOrdinal("ДействующаяСкидка")),
        КолвоНаСкладе = r.GetInt32(r.GetOrdinal("КолвоНаСкладе")),
        ОписаниеТовара = r.IsDBNull(r.GetOrdinal("ОписаниеТовара")) ? null : r.GetString(r.GetOrdinal("ОписаниеТовара")),
        Фото = r.IsDBNull(r.GetOrdinal("Фото")) ? null : r.GetString(r.GetOrdinal("Фото")),
    };
}
