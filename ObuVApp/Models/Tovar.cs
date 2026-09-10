namespace ObuVApp.Models;

public class Tovar
{
    public string Артикул { get; set; } = "";
    public string НаименованиеТовара { get; set; } = "";
    public string ЕдиницаИзмерения { get; set; } = "шт.";
    public decimal Цена { get; set; }
    public string Поставщик { get; set; } = "";
    public string Производитель { get; set; } = "";
    public string КатегорияТовара { get; set; } = "";
    public int ДействующаяСкидка { get; set; }
    public int КолвоНаСкладе { get; set; }
    public string? ОписаниеТовара { get; set; }
    public string? Фото { get; set; }

    public decimal ЦенаСоСкидкой => Цена * (1 - ДействующаяСкидка / 100m);
    public bool БольшаяСкидка => ДействующаяСкидка > 15;
}
