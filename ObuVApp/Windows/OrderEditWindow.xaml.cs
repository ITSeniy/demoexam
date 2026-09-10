using ObuVApp.Database;
using ObuVApp.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ObuVApp.Windows;

public partial class OrderEditWindow : Window
{
    private readonly Order? _original;
    private readonly bool _isEdit;
    private List<Tovar> _allTovar = [];
    private readonly ObservableCollection<OrderItem> _items = [];

    public OrderEditWindow(Order? order)
    {
        InitializeComponent();
        _original = order;
        _isEdit = order != null;
        TbTitle.Text = _isEdit ? "Редактирование заказа" : "Добавление заказа";

        LoadComboData();
        DgItems.ItemsSource = _items;

        if (_isEdit)
        {
            DpOrderDate.SelectedDate = order!.ДатаЗаказа;
            DpDeliveryDate.SelectedDate = order.ДатаДоставки;
            CbPickup.SelectedValue = order.ПунктВыдачиID;
            TbClient.Text = order.ФИОКлиента;
            TbCode.Text = order.КодДляПолучения.ToString();

            var statusItems = CbStatus.Items.Cast<ComboBoxItem>();
            var match = statusItems.FirstOrDefault(i => i.Content.ToString() == order.СтатусЗаказа);
            if (match != null) CbStatus.SelectedItem = match;
            else CbStatus.SelectedIndex = 0;

            var positions = OrderRepository.GetItems(order.НомерЗаказа);
            foreach (var p in positions) _items.Add(p);
        }
        else
        {
            DpOrderDate.SelectedDate = DateTime.Today;
            DpDeliveryDate.SelectedDate = DateTime.Today.AddDays(7);
            CbStatus.SelectedIndex = 0;
        }
    }

    private void LoadComboData()
    {
        var points = OrderRepository.GetPickupPoints();
        CbPickup.ItemsSource = points;
        CbPickup.SelectedValuePath = "ID";
        CbPickup.DisplayMemberPath = "Адрес";

        _allTovar = TovarRepository.GetAll();

        // Обновить источник для колонки товара
        if (DgItems.Columns[0] is DataGridComboBoxColumn col)
        {
            col.ItemsSource = _allTovar;
        }
    }

    private void BtnAddItem_Click(object sender, RoutedEventArgs e)
    {
        _items.Add(new OrderItem
        {
            АртикулТовара = _allTovar.FirstOrDefault()?.Артикул ?? "",
            НаименованиеТовара = _allTovar.FirstOrDefault()?.НаименованиеТовара ?? "",
            Количество = 1,
        });
    }

    private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (DgItems.SelectedItem is OrderItem item)
            _items.Remove(item);
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        TbError.Text = "";

        if (DpOrderDate.SelectedDate == null)
        { TbError.Text = "Укажите дату заказа."; return; }
        if (DpDeliveryDate.SelectedDate == null)
        { TbError.Text = "Укажите дату доставки."; return; }
        if (CbPickup.SelectedItem == null)
        { TbError.Text = "Выберите пункт выдачи."; return; }
        if (string.IsNullOrWhiteSpace(TbClient.Text))
        { TbError.Text = "Укажите ФИО клиента."; return; }
        if (!int.TryParse(TbCode.Text, out var code))
        { TbError.Text = "Код получения должен быть числом."; return; }
        if (_items.Count == 0)
        { TbError.Text = "Добавьте хотя бы один товар в заказ."; return; }
        if (_items.Any(i => string.IsNullOrEmpty(i.АртикулТовара) || i.Количество <= 0))
        { TbError.Text = "Проверьте позиции заказа: артикул и количество должны быть заполнены."; return; }

        var order = new Order
        {
            НомерЗаказа = _original?.НомерЗаказа ?? 0,
            ДатаЗаказа = DpOrderDate.SelectedDate!.Value,
            ДатаДоставки = DpDeliveryDate.SelectedDate!.Value,
            ПунктВыдачиID = (int)CbPickup.SelectedValue,
            ФИОКлиента = TbClient.Text.Trim(),
            КодДляПолучения = code,
            СтатусЗаказа = (CbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Новый",
            Позиции = [.. _items],
        };

        try
        {
            if (_isEdit)
                OrderRepository.Update(order);
            else
                OrderRepository.Insert(order);

            DialogResult = true;
        }
        catch (Exception ex)
        {
            TbError.Text = "Ошибка сохранения: " + ex.Message;
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
