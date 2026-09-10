namespace ObuVApp.Models;

public class User
{
    public int ID { get; set; }
    public int РольID { get; set; }
    public string Роль { get; set; } = "";
    public string ФИО { get; set; } = "";
    public string Логин { get; set; } = "";
    public string Пароль { get; set; } = "";

    public bool IsAdmin => Роль == "Администратор";
    public bool IsManager => Роль == "Менеджер";
    public bool IsClient => Роль == "Авторизированный клиент";
}
