using Education.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IO = System.IO;

namespace Education.Windows
{
    /// <summary>
    /// Логика взаимодействия для ProductsWindow.xaml
    /// </summary>
    public partial class ProductsWindow : Window
    {
        // Текущий пользователь
        private int _userId;
        private string _userFullName;
        private string _userRole;

        // Все товары из БД
        private List<ProductRow> _allProducts = new List<ProductRow>();

        // Открытое окно редактирования (только одно!)
        private ProductEditWindow _editWindow = null;

        public ProductsWindow(int userId, string fullName, string role)
        {
            InitializeComponent(); // ← СНАЧАЛА инициализация

            _userId = userId;
            _userFullName = fullName;
            _userRole = role;

            // Показываем ФИО в правом верхнем углу
            LblUser.Text = fullName;

            // Настраиваем интерфейс под роль
            SetupByRole();

            // Загружаем поставщиков в фильтр
            LoadSuppliers();

            // Загружаем товары
            LoadProducts(); // ← Теперь LvProducts уже существует
        }

        // Скрываем лишние кнопки в зависимости от роли
        private void SetupByRole()
        {
            // По умолчанию скрываем всё
            TxtSearch.Visibility = Visibility.Collapsed;
            CmbSupplier.Visibility = Visibility.Collapsed;
            CmbSort.Visibility = Visibility.Collapsed;
            BtnOrders.Visibility = Visibility.Collapsed;
            BtnAdd.Visibility = Visibility.Collapsed;
            BtnDelete.Visibility = Visibility.Collapsed;

            if (_userRole == "Менеджер")
            {
                TxtSearch.Visibility = Visibility.Visible;
                CmbSupplier.Visibility = Visibility.Visible;
                CmbSort.Visibility = Visibility.Visible;
                BtnOrders.Visibility = Visibility.Visible;
            }
            else if (_userRole == "Администратор")
            {
                TxtSearch.Visibility = Visibility.Visible;
                CmbSupplier.Visibility = Visibility.Visible;
                CmbSort.Visibility = Visibility.Visible;
                BtnOrders.Visibility = Visibility.Visible;
                BtnAdd.Visibility = Visibility.Visible;
                BtnDelete.Visibility = Visibility.Visible;
            }
        }

        // Загружаем товары из БД
        private void LoadProducts()
        {
            string query = @"
                SELECT
                    p.product_id,
                    p.article,
                    p.product_name,
                    u.unit_name,
                    p.price,
                    s.supplier_name,
                    m.manufacturer_name,
                    c.category_name,
                    p.discount,
                    p.quantity_in_stock,
                    p.description,
                    p.image_path
                FROM products p
                LEFT JOIN units u        ON p.unit_id        = u.unit_id
                LEFT JOIN suppliers s    ON p.supplier_id    = s.supplier_id
                LEFT JOIN manufacturers m ON p.manufacturer_id = m.manufacturer_id
                LEFT JOIN categories c   ON p.category_id    = c.category_id
                ORDER BY p.product_id";

            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            _allProducts.Clear();

            foreach (DataRow row in dt.Rows)
            {
                int discount = Convert.ToInt32(row["discount"]);
                decimal price = Convert.ToDecimal(row["price"]);
                decimal final = price - (price * discount / 100m);
                int stock = Convert.ToInt32(row["quantity_in_stock"]);

                // Путь к изображению
                string imgName = row["image_path"].ToString();
                string imgPath = GetImagePath(imgName);

                var green = (System.Windows.Media.SolidColorBrush)(new System.Windows.Media.BrushConverter().ConvertFromString("#2E8B57"));

                System.Windows.Media.Brush bg = System.Windows.Media.Brushes.White;

                if (stock == 0)
                    bg = System.Windows.Media.Brushes.LightBlue;
                else if (discount > 15)
                    bg = green;

                _allProducts.Add(new ProductRow
                {
                    ProductId = Convert.ToInt32(row["product_id"]),
                    Article = row["article"].ToString(),
                    ProductName = row["product_name"].ToString(),
                    UnitName = unit_name(row),
                    Price = price,
                    FinalPrice = final,
                    Discount = discount,
                    QuantityInStock = stock,
                    SupplierName = row["supplier_name"].ToString(),
                    ManufacturerName = row["manufacturer_name"].ToString(),
                    CategoryName = row["category_name"].ToString(),
                    Description = row["description"].ToString(),
                    ImagePath = imgPath,

                    // Текст цены для отображения
                    PriceText = price.ToString("F2") + " ₽",
                    FinalPriceText = final.ToString("F2") + " ₽",

                    // Показывать перечёркнутую цену если скидка > 0
                    StrikeVisibility = discount > 0
                        ? Visibility.Visible
                        : Visibility.Collapsed,

                    RowBackground = bg,
                    DiscountText = "Действующая\nскидка: " + discount + "%"
                });

            }

            ApplyFilters();
        }

        // Костыль для unit_name (чтобы не падало если null)
        private string unit_name(DataRow row)
        {
            return row["unit_name"] == DBNull.Value
                ? ""
                : row["unit_name"].ToString();
        }

        // Получить полный путь к картинке
        private string GetImagePath(string imageName)
        {
            string folder = IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Images");

            if (!string.IsNullOrEmpty(imageName))
            {
                string full = IO.Path.Combine(folder, imageName);
                if (IO.File.Exists(full))
                    return full;
            }

            // Заглушка
            return IO.Path.Combine(folder, "picture.png");
        }

        // Загружаем поставщиков в ComboBox
        private void LoadSuppliers()
        {
            CmbSupplier.Items.Clear();
            CmbSupplier.Items.Add("Все поставщики");

            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT supplier_name FROM suppliers ORDER BY supplier_name");

            foreach (DataRow row in dt.Rows)
                CmbSupplier.Items.Add(row["supplier_name"].ToString());

            // ОТКЛЮЧАЕМ событие перед установкой значения
            CmbSupplier.SelectionChanged -= CmbSupplier_SelectionChanged;
            CmbSupplier.SelectedIndex = 0;
            CmbSupplier.SelectionChanged += CmbSupplier_SelectionChanged;
        }

        // Применить фильтр + поиск + сортировку
        private void ApplyFilters()
        {
            if (LvProducts == null) return;

            string search = (TxtSearch.Text ?? "").Trim().ToLower();

            // supplier может быть string, а может быть DataRowView
            string supplier = "Все поставщики";
            if (CmbSupplier.SelectedItem is string s)
                supplier = s;
            else if (CmbSupplier.SelectedItem is DataRowView drvSup)
                supplier = drvSup["supplier_name"].ToString();

            // sort тоже может быть ComboBoxItem или строка
            string sort = "Без сортировки";
            if (CmbSort.SelectedItem is ComboBoxItem cbi)
                sort = cbi.Content.ToString();
            else if (CmbSort.SelectedItem != null)
                sort = CmbSort.SelectedItem.ToString();

            var result = new List<ProductRow>();

            foreach (var p in _allProducts)
            {
                bool matchSearch =
                    string.IsNullOrEmpty(search) ||
                    (p.ProductName ?? "").ToLower().Contains(search) ||
                    (p.CategoryName ?? "").ToLower().Contains(search) ||
                    (p.Description ?? "").ToLower().Contains(search) ||
                    (p.ManufacturerName ?? "").ToLower().Contains(search) ||
                    (p.SupplierName ?? "").ToLower().Contains(search) ||
                    (p.Article ?? "").ToLower().Contains(search);

                bool matchSupplier =
                    supplier == "Все поставщики" ||
                    p.SupplierName == supplier;

                if (matchSearch && matchSupplier)
                    result.Add(p);
            }

            if (sort == "Кол-во: по возрастанию")
                result.Sort((a, b) => a.QuantityInStock.CompareTo(b.QuantityInStock));
            else if (sort == "Кол-во: по убыванию")
                result.Sort((a, b) => b.QuantityInStock.CompareTo(a.QuantityInStock));

            // ВАЖНО: сбросить ItemsSource, чтобы точно перерисовало
            LvProducts.ItemsSource = null;
            LvProducts.ItemsSource = result;

            // Отладка
            // MessageBox.Show($"После фильтров: {result.Count}", "DEBUG");
        }

        // Подсветка строк по условиям
        private void ColorRows()
        {
            foreach (var item in LvProducts.Items)
            {
                var row = item as ProductRow;
                var container = LvProducts.ItemContainerGenerator
                    .ContainerFromItem(item) as ListViewItem;

                if (container == null || row == null) continue;

                if (row.QuantityInStock == 0)
                    container.Background = Brushes.LightBlue;
                else if (row.Discount > 15)
                    container.Background = new SolidColorBrush(
                        Color.FromRgb(46, 139, 87));
                else
                    container.Background = Brushes.White;
            }
        }

        // Поиск в реальном времени
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        // Двойной клик - редактировать (только администратор)
        private void LvProducts_MouseDoubleClick(object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_userRole != "Администратор") return;

            var selected = LvProducts.SelectedItem as ProductRow;
            if (selected == null) return;

            // Не открывать второе окно!
            if (_editWindow != null && _editWindow.IsLoaded)
            {
                MessageBox.Show("Окно редактирования уже открыто!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                _editWindow.Focus();
                return;
            }

            _editWindow = new ProductEditWindow(selected.ProductId);
            _editWindow.Closed += (s, args) =>
            {
                _editWindow = null;
                LoadProducts();
            };
            _editWindow.Show();
        }

        // Добавить товар
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_editWindow != null && _editWindow.IsLoaded)
            {
                MessageBox.Show("Закройте текущее окно редактирования!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 0 = новый товар
            _editWindow = new ProductEditWindow(0);
            _editWindow.Closed += (s, args) =>
            {
                _editWindow = null;
                LoadProducts();
            };
            _editWindow.Show();
        }

        // Удалить товар
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = LvProducts.SelectedItem as ProductRow;

            if (selected == null)
            {
                MessageBox.Show("Выберите товар для удаления!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем есть ли товар в заказах
            DataTable check = DatabaseHelper.ExecuteQuery(
                $"SELECT COUNT(*) as cnt FROM order_items " +
                $"WHERE product_article = '{selected.Article}'");

            int cnt = Convert.ToInt32(check.Rows[0]["cnt"]);
            if (cnt > 0)
            {
                MessageBox.Show("Нельзя удалить товар, который есть в заказах!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Подтверждение
            var answer = MessageBox.Show(
                $"Удалить товар «{selected.ProductName}»?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes) return;

            DatabaseHelper.ExecuteNonQuery(
                $"DELETE FROM products WHERE product_id = {selected.ProductId}");

            MessageBox.Show("Товар удалён!", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);

            LoadProducts();
        }

        // Открыть заказы
        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            var w = new OrdersWindow(_userId, _userFullName, _userRole);
            w.Show();
        }

        // Выход
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }

    // Простой класс строки товара для ListView
    public class ProductRow
    {
        public int ProductId { get; set; }
        public string Article { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public int Discount { get; set; }
        public int QuantityInStock { get; set; }
        public string SupplierName { get; set; }
        public string ManufacturerName { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }

        // Для отображения цены
        public string PriceText { get; set; }
        public string FinalPriceText { get; set; }
        public Visibility StrikeVisibility { get; set; }
        public System.Windows.Media.Brush RowBackground { get; set; }
        public string DiscountText { get; set; }
    }
}
