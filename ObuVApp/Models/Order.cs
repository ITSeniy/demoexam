namespace ObuVApp.Models;

public class Order
{
    public int НомерЗаказа { get; set; }
    public DateTime ДатаЗаказа { get; set; }
    public DateTime ДатаДоставки { get; set; }
    public int ПунктВыдачиID { get; set; }
    public string АдресПунктаВыдачи { get; set; } = "";
    public string ФИОКлиента { get; set; } = "";
    public int КодДляПолучения { get; set; }
    public string СтатусЗаказа { get; set; } = "Новый";
    public List<OrderItem> Позиции { get; set; } = [];
}

public class OrderItem
{
    public int ID { get; set; }
    public int НомерЗаказа { get; set; }
    public string АртикулТовара { get; set; } = "";
    public string НаименованиеТовара { get; set; } = "";
    public int Количество { get; set; }
}

public class PickupPoint
{
    public int ID { get; set; }
    public string Адрес { get; set; } = "";
    public override string ToString() => Адрес;
}
