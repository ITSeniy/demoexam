using ObuVApp.Database;
using ObuVApp.Models;
using System.Windows;

namespace ObuVApp.Windows;

public partial class TovarEditWindow : Window
{
    private readonly Tovar? _original;
    private readonly bool _isEdit;

    public TovarEditWindow(Tovar? tovar)
    {
        InitializeComponent();
        _original = tovar;
        _isEdit = tovar != null;
        TbTitle.Text = _isEdit ? "Редактирование товара" : "Добавление товара";

        var cats = TovarRepository.GetCategories();
        CbCategory.ItemsSource = cats;

        if (_isEdit)
        {
            TbArticul.Text = tovar!.Артикул;
            TbArticul.IsReadOnly = true;
            TbName.Text = tovar.НаименованиеТовара;
            CbCategory.Text = tovar.КатегорияТовара;
            TbPrice.Text = tovar.Цена.ToString("F2");
            TbDiscount.Text = tovar.ДействующаяСкидка.ToString();
            TbSupplier.Text = tovar.Поставщик;
            TbManufacturer.Text = tovar.Производитель;
            TbQty.Text = tovar.КолвоНаСкладе.ToString();
            TbUnit.Text = tovar.ЕдиницаИзмерения;
            TbDesc.Text = tovar.ОписаниеТовара ?? "";
            TbPhoto.Text = tovar.Фото ?? "";
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        TbError.Text = "";

        if (string.IsNullOrWhiteSpace(TbArticul.Text))
        { TbError.Text = "Укажите артикул."; return; }
        if (string.IsNullOrWhiteSpace(TbName.Text))
        { TbError.Text = "Укажите наименование."; return; }
        if (string.IsNullOrWhiteSpace(CbCategory.Text))
        { TbError.Text = "Укажите категорию."; return; }
        if (!decimal.TryParse(TbPrice.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var price) || price < 0)
        { TbError.Text = "Цена должна быть числом >= 0."; return; }
        if (!int.TryParse(TbDiscount.Text, out var disc) || disc < 0 || disc > 100)
        { TbError.Text = "Скидка должна быть числом от 0 до 100."; return; }
        if (!int.TryParse(TbQty.Text, out var qty) || qty < 0)
        { TbError.Text = "Кол-во должно быть числом >= 0."; return; }

        var tovar = new Tovar
        {
            Артикул = TbArticul.Text.Trim(),
            НаименованиеТовара = TbName.Text.Trim(),
            КатегорияТовара = CbCategory.Text.Trim(),
            Цена = price,
            ДействующаяСкидка = disc,
            Поставщик = TbSupplier.Text.Trim(),
            Производитель = TbManufacturer.Text.Trim(),
            КолвоНаСкладе = qty,
            ЕдиницаИзмерения = string.IsNullOrWhiteSpace(TbUnit.Text) ? "шт." : TbUnit.Text.Trim(),
            ОписаниеТовара = string.IsNullOrWhiteSpace(TbDesc.Text) ? null : TbDesc.Text.Trim(),
            Фото = string.IsNullOrWhiteSpace(TbPhoto.Text) ? null : TbPhoto.Text.Trim(),
        };

        try
        {
            if (_isEdit)
                TovarRepository.Update(tovar);
            else
                TovarRepository.Insert(tovar);

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
