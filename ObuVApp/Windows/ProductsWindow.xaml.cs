using ObuVApp.Database;
using ObuVApp.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ObuVApp.Windows;

public partial class ProductsWindow : Window
{
    private readonly User? _user;
    private List<Tovar> _allTovar = [];
    private readonly string _imgDir;

    public ProductsWindow(User? user)
    {
        InitializeComponent();
        _user = user;
        _imgDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

        Loaded += (_, _) => { SetupUI(); LoadData(); };
    }

    private void SetupUI()
    {
        if (_user == null)
        {
            TbUserInfo.Text = "Гость";
        }
        else
        {
            TbUserInfo.Text = $"{_user.ФИО} ({_user.Роль})";
            if (_user.IsManager || _user.IsAdmin)
            {
                FilterPanel.Visibility = Visibility.Visible;
                BtnOrders.Visibility = Visibility.Visible;
            }
            if (_user.IsAdmin)
            {
                AdminPanel.Visibility = Visibility.Visible;
            }
        }
    }

    private void LoadData()
    {
        try
        {
            _allTovar = TovarRepository.GetAll();

            // Загрузить категории в ComboBox
            if (FilterPanel.Visibility == Visibility.Visible)
            {
                var cats = new List<string> { "Все категории" };
                cats.AddRange(TovarRepository.GetCategories());
                CbCategory.ItemsSource = cats;
                CbCategory.SelectedIndex = 0;
            }

            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка загрузки товаров: " + ex.Message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        if (ProductsPanel == null) return;

        var filtered = _allTovar.AsEnumerable();

        if (FilterPanel?.Visibility == Visibility.Visible)
        {
            var search = TbSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(t =>
                    t.НаименованиеТовара.ToLower().Contains(search) ||
                    t.Артикул.ToLower().Contains(search));

            if (CbCategory.SelectedIndex > 0 && CbCategory.SelectedItem is string cat)
                filtered = filtered.Where(t => t.КатегорияТовара == cat);

            filtered = (CbSort.SelectedIndex) switch
            {
                1 => filtered.OrderBy(t => t.ЦенаСоСкидкой),
                2 => filtered.OrderByDescending(t => t.ЦенаСоСкидкой),
                3 => filtered.OrderBy(t => t.НаименованиеТовара),
                4 => filtered.OrderByDescending(t => t.НаименованиеТовара),
                5 => filtered.OrderBy(t => t.ДействующаяСкидка),
                6 => filtered.OrderByDescending(t => t.ДействующаяСкидка),
                _ => filtered
            };
        }

        var list = filtered.ToList();
        RenderProducts(list);

        if (AdminPanel?.Visibility == Visibility.Visible)
            TbCount.Text = $"Товаров: {list.Count}";
    }

    private void RenderProducts(List<Tovar> tovar)
    {
        if (ProductsPanel == null) return;
        ProductsPanel.Children.Clear();

        foreach (var t in tovar)
        {
            var bg = t.БольшаяСкидка
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E8B57"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7FFF00"));

            var card = new Border
            {
                Width = 200,
                Margin = new Thickness(8),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Background = bg,
                Cursor = Cursors.Hand,
                Tag = t,
            };

            var stack = new StackPanel { Margin = new Thickness(8) };

            // Фото
            var imgPath = t.Фото != null ? Path.Combine(_imgDir, t.Фото) : null;
            var img = new Image
            {
                Height = 140,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 0, 0, 6),
            };
            if (imgPath != null && File.Exists(imgPath))
                img.Source = new BitmapImage(new Uri(imgPath));
            stack.Children.Add(img);

            stack.Children.Add(new TextBlock
            {
                Text = t.НаименованиеТовара,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Times New Roman"),
                TextWrapping = TextWrapping.Wrap,
            });
            stack.Children.Add(new TextBlock
            {
                Text = $"Артикул: {t.Артикул}",
                FontFamily = new FontFamily("Times New Roman"),
                FontSize = 11,
                Foreground = Brushes.DimGray,
            });
            stack.Children.Add(new TextBlock
            {
                Text = $"Категория: {t.КатегорияТовара}",
                FontFamily = new FontFamily("Times New Roman"),
                FontSize = 11,
            });

            if (t.ДействующаяСкидка > 0)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = $"Цена: {t.Цена:N2} руб.",
                    FontFamily = new FontFamily("Times New Roman"),
                    FontSize = 11,
                    TextDecorations = TextDecorations.Strikethrough,
                    Foreground = Brushes.Gray,
                });
                stack.Children.Add(new TextBlock
                {
                    Text = $"Со скидкой {t.ДействующаяСкидка}%: {t.ЦенаСоСкидкой:N2} руб.",
                    FontFamily = new FontFamily("Times New Roman"),
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkGreen,
                });
            }
            else
            {
                stack.Children.Add(new TextBlock
                {
                    Text = $"Цена: {t.Цена:N2} руб.",
                    FontFamily = new FontFamily("Times New Roman"),
                    FontWeight = FontWeights.Bold,
                });
            }

            stack.Children.Add(new TextBlock
            {
                Text = $"На складе: {t.КолвоНаСкладе} {t.ЕдиницаИзмерения}",
                FontFamily = new FontFamily("Times New Roman"),
                FontSize = 11,
            });

            // Кнопки для администратора
            if (_user?.IsAdmin == true)
            {
                var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
                var btnEdit = new Button
                {
                    Content = "Изменить",
                    Tag = t,
                    Height = 26,
                    Padding = new Thickness(8, 0, 8, 0),
                    Margin = new Thickness(0, 0, 4, 0),
                    FontFamily = new FontFamily("Times New Roman"),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FA9A")),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                };
                btnEdit.Click += BtnEditTovar_Click;

                var btnDel = new Button
                {
                    Content = "Удалить",
                    Tag = t,
                    Height = 26,
                    Padding = new Thickness(8, 0, 8, 0),
                    FontFamily = new FontFamily("Times New Roman"),
                    Background = new SolidColorBrush(Colors.Salmon),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                };
                btnDel.Click += BtnDeleteTovar_Click;

                btnPanel.Children.Add(btnEdit);
                btnPanel.Children.Add(btnDel);
                stack.Children.Add(btnPanel);
            }

            card.Child = stack;
            ProductsPanel.Children.Add(card);
        }
    }

    private void Filter_Changed(object sender, RoutedEventArgs e) => ApplyFilter();

    private void BtnOrders_Click(object sender, RoutedEventArgs e)
    {
        new OrdersWindow(_user!).ShowDialog();
    }

    private void BtnAddTovar_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new TovarEditWindow(null);
        if (dlg.ShowDialog() == true)
            LoadData();
    }

    private void BtnEditTovar_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Tovar t)
        {
            var dlg = new TovarEditWindow(t);
            if (dlg.ShowDialog() == true)
                LoadData();
        }
    }

    private void BtnDeleteTovar_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Tovar t)
        {
            var res = MessageBox.Show($"Удалить товар «{t.НаименованиеТовара}» ({t.Артикул})?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    TovarRepository.Delete(t.Артикул);
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

    private void BtnExit_Click(object sender, RoutedEventArgs e)
    {
        var login = new LoginWindow();
        login.Show();
        Close();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (Application.Current.Windows.Count <= 1)
            Application.Current.Shutdown();
    }
}
