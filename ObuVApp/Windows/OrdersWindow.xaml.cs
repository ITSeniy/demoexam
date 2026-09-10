using ObuVApp.Database;
using ObuVApp.Models;
using System.Windows;
using System.Windows.Controls;

namespace ObuVApp.Windows;

public partial class OrdersWindow : Window
{
    private readonly User _user;
    public bool IsAdmin => _user.IsAdmin;

    public OrdersWindow(User user)
    {
        InitializeComponent();
        _user = user;
        DataContext = this;

        if (_user.IsAdmin)
            BtnAdd.Visibility = Visibility.Visible;

        LoadData();
    }

    private void LoadData()
    {
        try
        {
            var orders = OrderRepository.GetAll();
            DgOrders.ItemsSource = orders;
            TbCount.Text = $"Заказов: {orders.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка загрузки заказов: " + ex.Message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OrderEditWindow(null);
        if (dlg.ShowDialog() == true)
            LoadData();
    }

    private void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Order order)
        {
            var dlg = new OrderEditWindow(order);
            if (dlg.ShowDialog() == true)
                LoadData();
        }
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Order order)
        {
            var res = MessageBox.Show($"Удалить заказ №{order.НомерЗаказа}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    OrderRepository.Delete(order.НомерЗаказа);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления: " + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
