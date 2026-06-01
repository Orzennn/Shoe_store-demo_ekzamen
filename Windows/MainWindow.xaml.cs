using Education.Database;
using Education.Windows; // Если ProductList лежит в папке Windows
using Npgsql;
using System;
using System.Data;
using System.Windows;

namespace Education
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Password;

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните логин и пароль!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Ищем пользователя в базе
            string query = $@"
                SELECT u.user_id, u.full_name, r.role_name
                FROM users u
                JOIN roles r ON u.role_id = r.role_id
                WHERE u.login = '{login}' AND u.password = '{password}'";

            DataTable result = DatabaseHelper.ExecuteQuery(query);

            if (result.Rows.Count == 0)
            {
                MessageBox.Show("Неверный логин или пароль!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Читаем данные пользователя
            int userId = Convert.ToInt32(result.Rows[0]["user_id"]);
            string fullName = result.Rows[0]["full_name"].ToString();
            string role = result.Rows[0]["role_name"].ToString();

            // Открываем окно товаров с нужной ролью
            var window = new ProductsWindow(userId, fullName, role);
            window.Show();
            this.Close();
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            // Гость - открываем без авторизации
            var window = new ProductsWindow(0, "Гость", "Гость");
            window.Show();
            this.Close();
        }
    }
}