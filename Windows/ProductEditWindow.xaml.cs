using Education.Database;
using Microsoft.Win32;
using System;
using System.Data;
using IO = System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Education.Windows
{
    /// <summary>
    /// Логика взаимодействия для ProductEditWindow.xaml
    /// </summary>
    public partial class ProductEditWindow : Window
    {
        // 0 = новый товар, иначе редактирование
        private int _productId;

        // Путь к старому фото (чтобы удалить при замене)
        private string _oldImagePath = "";

        // Путь к новому фото
        private string _newImagePath = "";

        public ProductEditWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;

            // Загружаем справочники
            LoadComboBoxes();

            if (_productId == 0)
            {
                // Новый товар - скрываем ID
                LblId.Visibility = Visibility.Collapsed;
                TxtId.Visibility = Visibility.Collapsed;

                // Ставим следующий ID автоматически
                DataTable dt = DatabaseHelper.ExecuteQuery(
                    "SELECT MAX(product_id) + 1 as next_id FROM products");
                string nextId = dt.Rows[0]["next_id"].ToString();
                TxtId.Text = nextId;

                // Показываем заглушку
                ShowPlaceholderImage();
            }
            else
            {
                // Редактирование - загружаем данные товара
                LblId.Visibility = Visibility.Visible;
                TxtId.Visibility = Visibility.Visible;
                LoadProductData();
            }
        }

        // Загружаем все выпадающие списки
        private void LoadComboBoxes()
        {
            // Категории
            DataTable categories = DatabaseHelper.ExecuteQuery(
                "SELECT category_id, category_name FROM categories ORDER BY category_name");
            CmbCategory.DisplayMemberPath = "category_name";
            CmbCategory.SelectedValuePath = "category_id";
            CmbCategory.ItemsSource = categories.DefaultView;

            // Производители
            DataTable manufacturers = DatabaseHelper.ExecuteQuery(
                "SELECT manufacturer_id, manufacturer_name FROM manufacturers ORDER BY manufacturer_name");
            CmbManufacturer.DisplayMemberPath = "manufacturer_name";
            CmbManufacturer.SelectedValuePath = "manufacturer_id";
            CmbManufacturer.ItemsSource = manufacturers.DefaultView;

            // Поставщики
            DataTable suppliers = DatabaseHelper.ExecuteQuery(
                "SELECT supplier_id, supplier_name FROM suppliers ORDER BY supplier_name");
            CmbSupplier.DisplayMemberPath = "supplier_name";
            CmbSupplier.SelectedValuePath = "supplier_id";
            CmbSupplier.ItemsSource = suppliers.DefaultView;

            // Единицы измерения
            DataTable units = DatabaseHelper.ExecuteQuery(
                "SELECT unit_id, unit_name FROM units ORDER BY unit_name");
            CmbUnit.DisplayMemberPath = "unit_name";
            CmbUnit.SelectedValuePath = "unit_id";
            CmbUnit.ItemsSource = units.DefaultView;
        }

        // Загружаем данные существующего товара
        private void LoadProductData()
        {
            string query = $@"
                SELECT
                    p.product_id,
                    p.article,
                    p.product_name,
                    p.category_id,
                    p.description,
                    p.manufacturer_id,
                    p.supplier_id,
                    p.price,
                    p.unit_id,
                    p.quantity_in_stock,
                    p.discount,
                    p.image_path
                FROM products p
                WHERE p.product_id = {_productId}";

            DataTable dt = DatabaseHelper.ExecuteQuery(query);

            if (dt.Rows.Count == 0) return;

            DataRow row = dt.Rows[0];

            // Заполняем поля
            TxtId.Text = row["product_id"].ToString();
            TxtName.Text = row["product_name"].ToString();
            TxtDescription.Text = row["description"].ToString();
            TxtPrice.Text = row["price"].ToString();
            TxtQuantity.Text = row["quantity_in_stock"].ToString();
            TxtDiscount.Text = row["discount"].ToString();

            // Выбираем значения в ComboBox
            CmbCategory.SelectedValue = row["category_id"];
            CmbManufacturer.SelectedValue = row["manufacturer_id"];
            CmbSupplier.SelectedValue = row["supplier_id"];
            CmbUnit.SelectedValue = row["unit_id"];

            // Загружаем фото
            string imageName = row["image_path"].ToString();
            _oldImagePath = GetImageFullPath(imageName);
            ShowImage(_oldImagePath);
        }

        // Выбор нового фото
        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (dialog.ShowDialog() != true) return;

            string selectedFile = dialog.FileName;

            // Папка для сохранения фото
            string imagesFolder = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

            if (!IO.Directory.Exists(imagesFolder))
                IO.Directory.CreateDirectory(imagesFolder);

            string newFileName = IO.Path.GetFileName(selectedFile);
            string destPath = IO.Path.Combine(imagesFolder, newFileName);

            IO.File.Copy(selectedFile, destPath, true);

            // Изменяем размер до 300x200
            ResizeImage(destPath);

            _newImagePath = destPath;

            // Показываем новое фото
            ShowImage(_newImagePath);
        }

        // Изменить размер изображения до 300x200
        private void ResizeImage(string imagePath)
        {
            try
            {
                BitmapImage original = new BitmapImage();
                original.BeginInit();
                original.UriSource = new Uri(imagePath);
                original.DecodePixelWidth = 300;
                original.DecodePixelHeight = 200;
                original.EndInit();

                // Сохраняем изменённое изображение
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(original));

                using (var stream = new IO.FileStream(imagePath, IO.FileMode.Create))
                {
                    encoder.Save(stream);
                }
            }
            catch
            {
                // Если не получилось изменить - оставляем как есть
            }
        }

        // Показать изображение
        private void ShowImage(string imagePath)
        {
            try
            {
                if (IO.File.Exists(imagePath))
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(imagePath);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    ImgProduct.Source = bmp;
                }
                else
                {
                    ShowPlaceholderImage();
                }
            }
            catch
            {
                ShowPlaceholderImage();
            }
        }

        // Показать заглушку
        private void ShowPlaceholderImage()
        {
            string placeholder = IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Images", "picture.png");

            if (IO.File.Exists(placeholder))
                ShowImage(placeholder);
        }

        // Получить полный путь к картинке
        private string GetImageFullPath(string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return "";

            return IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Images", imageName);
        }

        // Сохранить товар
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация полей
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Введите наименование товара!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию товара!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbManufacturer.SelectedValue == null)
            {
                MessageBox.Show("Выберите производителя!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbSupplier.SelectedValue == null)
            {
                MessageBox.Show("Выберите поставщика!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbUnit.SelectedValue == null)
            {
                MessageBox.Show("Выберите единицу измерения!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка цены
            if (!decimal.TryParse(TxtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену (число >= 0)!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка количества
            if (!int.TryParse(TxtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество (целое число >= 0)!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка скидки
            if (!int.TryParse(TxtDiscount.Text, out int discount) ||
                discount < 0 || discount > 100)
            {
                MessageBox.Show("Введите корректную скидку (от 0 до 100)!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем значения из ComboBox
            int categoryId = Convert.ToInt32(CmbCategory.SelectedValue);
            int manufacturerId = Convert.ToInt32(CmbManufacturer.SelectedValue);
            int supplierId = Convert.ToInt32(CmbSupplier.SelectedValue);
            int unitId = Convert.ToInt32(CmbUnit.SelectedValue);

            string name = TxtName.Text.Trim();
            string description = TxtDescription.Text.Trim();

            // Защита от апострофов в SQL
            name = name.Replace("'", "''");
            description = description.Replace("'", "''");

            // Путь к фото для сохранения в БД
            string imageNameForDb = "";
            if (!string.IsNullOrEmpty(_newImagePath))
                imageNameForDb = IO.Path.GetFileName(_newImagePath);
            else if (!string.IsNullOrEmpty(_oldImagePath))
                imageNameForDb = IO.Path.GetFileName(_oldImagePath);

            if (_productId == 0)
            {
                // Генерируем артикул из названия
                string article = GenerateArticle(TxtName.Text.Trim());

                string query = $@"
                                INSERT INTO products
                                    (article, product_name, category_id, description,
                                     manufacturer_id, supplier_id, price,
                                     unit_id, quantity_in_stock, discount, image_path)
                                VALUES
            ('{article}', '{name}', {categoryId}, '{description}',
             {manufacturerId}, {supplierId}, {price.ToString().Replace(',', '.')},
             {unitId}, {quantity}, {discount},
             '{imageNameForDb}')";

                DatabaseHelper.ExecuteNonQuery(query);

                MessageBox.Show($"Товар добавлен!\nАртикул: {article}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Редактирование существующего товара

                // Удаляем старое фото если загрузили новое
                if (!string.IsNullOrEmpty(_newImagePath) &&
                    !string.IsNullOrEmpty(_oldImagePath) &&
                    _newImagePath != _oldImagePath &&
                    IO.File.Exists(_oldImagePath))
                {
                    IO.File.Delete(_oldImagePath);
                }

                string query = $@"
                    UPDATE products SET
                        product_name      = '{name}',
                        category_id       = {categoryId},
                        description       = '{description}',
                        manufacturer_id   = {manufacturerId},
                        supplier_id       = {supplierId},
                        price             = {price.ToString().Replace(',', '.')},
                        unit_id           = {unitId},
                        quantity_in_stock = {quantity},
                        discount          = {discount},
                        image_path        = '{imageNameForDb}'
                    WHERE product_id = {_productId}";

                DatabaseHelper.ExecuteNonQuery(query);

                MessageBox.Show("Товар успешно обновлён!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            this.Close();
        }

        // Генерация артикула из названия
        private string GenerateArticle(string productName)
        {
            // Берём первую букву названия + случайные символы
            string prefix = "А";

            if (!string.IsNullOrEmpty(productName))
                prefix = productName.Substring(0, 1).ToUpper();

            // Случайные 5 символов (буквы + цифры)
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789";
            var rnd = new Random();
            string suffix = "";

            for (int i = 0; i < 5; i++)
                suffix += chars[rnd.Next(chars.Length)];

            string article = prefix + suffix;

            // Проверяем что такого артикула ещё нет
            DataTable check = DatabaseHelper.ExecuteQuery(
                $"SELECT COUNT(*) as cnt FROM products WHERE article = '{article}'");

            int count = Convert.ToInt32(check.Rows[0]["cnt"]);

            // Если есть совпадение — генерируем заново
            if (count > 0)
                return GenerateArticle(productName);

            return article;
        }

        // Отмена
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
