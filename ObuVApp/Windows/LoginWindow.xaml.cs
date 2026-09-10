using ObuVApp.Database;
using ObuVApp.Models;
using System.Windows;

namespace ObuVApp.Windows;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        TbError.Text = "";
        var login = TbLogin.Text.Trim();
        var pwd = PbPassword.Password.Trim();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pwd))
        {
            TbError.Text = "Введите логин и пароль.";
            return;
        }

        try
        {
            var user = UserRepository.Login(login, pwd);
            if (user == null)
            {
                TbError.Text = "Неверный логин или пароль.";
                return;
            }

            var mainWnd = new ProductsWindow(user);
            mainWnd.Show();
            Close();
        }
        catch (Exception ex)
        {
            TbError.Text = "Ошибка подключения к БД: " + ex.Message;
        }
    }

    private void BtnGuest_Click(object sender, RoutedEventArgs e)
    {
        var mainWnd = new ProductsWindow(null);
        mainWnd.Show();
        Close();
    }
}
