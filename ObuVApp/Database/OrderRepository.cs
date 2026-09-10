using Microsoft.Data.SqlClient;
using ObuVApp.Models;

namespace ObuVApp.Database;

public static class OrderRepository
{
    public static List<Order> GetAll()
    {
        var orders = new List<Order>();
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand(@"
            SELECT з.НомерЗаказа, з.ДатаЗаказа, з.ДатаДоставки,
                   з.ПунктВыдачиID, п.Адрес, з.ФИОКлиента, з.КодДляПолучения, з.СтатусЗаказа
            FROM Заказы з
            JOIN ПунктыВыдачи п ON п.ID = з.ПунктВыдачиID
            ORDER BY з.НомерЗаказа", con);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            orders.Add(new Order
            {
                НомерЗаказа = r.GetInt32(0),
                ДатаЗаказа = r.GetDateTime(1),
                ДатаДоставки = r.GetDateTime(2),
                ПунктВыдачиID = r.GetInt32(3),
                АдресПунктаВыдачи = r.GetString(4),
                ФИОКлиента = r.GetString(5),
                КодДляПолучения = r.GetInt32(6),
                СтатусЗаказа = r.GetString(7).Trim(),
            });
        }
        return orders;
    }

    public static List<OrderItem> GetItems(int orderId)
    {
        var items = new List<OrderItem>();
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand(@"
            SELECT с.ID, с.НомерЗаказа, с.АртикулТовара, т.НаименованиеТовара, с.Количество
            FROM СоставЗаказа с
            JOIN Товары т ON т.Артикул = с.АртикулТовара
            WHERE с.НомерЗаказа = @id", con);
        cmd.Parameters.AddWithValue("@id", orderId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            items.Add(new OrderItem
            {
                ID = r.GetInt32(0),
                НомерЗаказа = r.GetInt32(1),
                АртикулТовара = r.GetString(2),
                НаименованиеТовара = r.GetString(3),
                Количество = r.GetInt32(4),
            });
        }
        return items;
    }

    public static int Insert(Order o)
    {
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand(@"
            INSERT INTO Заказы (ДатаЗаказа, ДатаДоставки, ПунктВыдачиID, ФИОКлиента, КодДляПолучения, СтатусЗаказа)
            VALUES (@d1, @d2, @pv, @fio, @code, @status);
            SELECT SCOPE_IDENTITY();", con);
        SetParams(cmd, o);
        var id = Convert.ToInt32(cmd.ExecuteScalar());

        InsertItems(con, id, o.Позиции);
        return id;
    }

    public static void Update(Order o)
    {
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand(@"
            UPDATE Заказы SET
                ДатаЗаказа = @d1, ДатаДоставки = @d2,
                ПунктВыдачиID = @pv, ФИОКлиента = @fio,
                КодДляПолучения = @code, СтатусЗаказа = @status
            WHERE НомерЗаказа = @num", con);
        SetParams(cmd, o);
        cmd.Parameters.AddWithValue("@num", o.НомерЗаказа);
        cmd.ExecuteNonQuery();

        using var del = new SqlCommand("DELETE FROM СоставЗаказа WHERE НомерЗаказа = @id", con);
        del.Parameters.AddWithValue("@id", o.НомерЗаказа);
        del.ExecuteNonQuery();

        InsertItems(con, o.НомерЗаказа, o.Позиции);
    }

    public static void Delete(int orderId)
    {
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand("DELETE FROM Заказы WHERE НомерЗаказа = @id", con);
        cmd.Parameters.AddWithValue("@id", orderId);
        cmd.ExecuteNonQuery();
    }

    public static List<PickupPoint> GetPickupPoints()
    {
        var list = new List<PickupPoint>();
        using var con = DbHelper.GetConnection();
        con.Open();
        using var cmd = new SqlCommand("SELECT ID, Адрес FROM ПунктыВыдачи ORDER BY ID", con);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(new PickupPoint { ID = r.GetInt32(0), Адрес = r.GetString(1) });
        return list;
    }

    private static void SetParams(SqlCommand cmd, Order o)
    {
        cmd.Parameters.AddWithValue("@d1", o.ДатаЗаказа);
        cmd.Parameters.AddWithValue("@d2", o.ДатаДоставки);
        cmd.Parameters.AddWithValue("@pv", o.ПунктВыдачиID);
        cmd.Parameters.AddWithValue("@fio", o.ФИОКлиента);
        cmd.Parameters.AddWithValue("@code", o.КодДляПолучения);
        cmd.Parameters.AddWithValue("@status", o.СтатусЗаказа);
    }

    private static void InsertItems(SqlConnection con, int orderId, List<OrderItem> items)
    {
        foreach (var item in items)
        {
            using var cmd = new SqlCommand(@"
                INSERT INTO СоставЗаказа (НомерЗаказа, АртикулТовара, Количество)
                VALUES (@ord, @art, @qty)", con);
            cmd.Parameters.AddWithValue("@ord", orderId);
            cmd.Parameters.AddWithValue("@art", item.АртикулТовара);
            cmd.Parameters.AddWithValue("@qty", item.Количество);
            cmd.ExecuteNonQuery();
        }
    }
}
