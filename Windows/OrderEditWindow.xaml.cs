using Education.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Education.Windows
{
    /// <summary>
    /// Логика взаимодействия для OrderEditWindow.xaml
    /// </summary>
    public partial class OrderEditWindow : Window
    {
        // 0 = новый заказ
        private int _orderId;

        public OrderEditWindow(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;

            LoadComboBoxes();

            if (_orderId == 0)
            {
                // Новый заказ - скрываем ID
                LblId.Visibility = Visibility.Collapsed;
                TxtId.Visibility = Visibility.Collapsed;

                // Дата заказа - сегодня
                DtOrderDate.SelectedDate = DateTime.Today;
            }
            else
            {
                // Редактирование - загружаем данные
                LblId.Visibility = Visibility.Visible;
                TxtId.Visibility = Visibility.Visible;
                LoadOrderData();
            }
        }

        // Загружаем выпадающие списки
        private void LoadComboBoxes()
        {
            // Статусы
            DataTable statuses = DatabaseHelper.ExecuteQuery(
                "SELECT status_id, status_name FROM order_statuses ORDER BY status_name");
            CmbStatus.DisplayMemberPath = "status_name";
            CmbStatus.SelectedValuePath = "status_id";
            CmbStatus.ItemsSource = statuses.DefaultView;

            // Пункты выдачи
            DataTable points = DatabaseHelper.ExecuteQuery(@"
                SELECT point_id,
                       CONCAT(city, ', ', street, ', ', house_number) as address
                FROM pickup_points
                ORDER BY city, street");
            CmbPickupPoint.DisplayMemberPath = "address";
            CmbPickupPoint.SelectedValuePath = "point_id";
            CmbPickupPoint.ItemsSource = points.DefaultView;
        }

        // Загружаем данные заказа
        private void LoadOrderData()
        {
            string query = $@"
                SELECT
                    o.order_id,
                    o.order_code,
                    o.status_id,
                    o.pickup_point_id,
                    o.order_date,
                    o.delivery_date
                FROM orders o
                WHERE o.order_id = {_orderId}";

            DataTable dt = DatabaseHelper.ExecuteQuery(query);

            if (dt.Rows.Count == 0) return;

            DataRow row = dt.Rows[0];

            TxtId.Text = row["order_id"].ToString();
            TxtCode.Text = row["order_code"].ToString();

            CmbStatus.SelectedValue = row["status_id"];
            CmbPickupPoint.SelectedValue = row["pickup_point_id"];

            DtOrderDate.SelectedDate = Convert.ToDateTime(row["order_date"]);

            if (row["delivery_date"] != DBNull.Value)
                DtDeliveryDate.SelectedDate = Convert.ToDateTime(row["delivery_date"]);
        }

        // Сохранить заказ
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(TxtCode.Text))
            {
                MessageBox.Show("Введите артикул заказа!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbStatus.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус заказа!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbPickupPoint.SelectedValue == null)
            {
                MessageBox.Show("Выберите пункт выдачи!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DtOrderDate.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату заказа!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string code = TxtCode.Text.Trim();
            int statusId = Convert.ToInt32(CmbStatus.SelectedValue);
            int pointId = Convert.ToInt32(CmbPickupPoint.SelectedValue);
            string orderDate = DtOrderDate.SelectedDate.Value.ToString("yyyy-MM-dd");
            string deliveryDate = DtDeliveryDate.SelectedDate.HasValue
                ? $"'{DtDeliveryDate.SelectedDate.Value:yyyy-MM-dd}'"
                : "NULL";

            if (_orderId == 0)
            {
                // Получаем следующий номер заказа
                DataTable dt = DatabaseHelper.ExecuteQuery(
                    "SELECT MAX(order_number) + 1 as next_num FROM orders");
                int nextNum = Convert.ToInt32(dt.Rows[0]["next_num"]);

                string query = $@"
                    INSERT INTO orders
                        (order_number, order_code, status_id,
                         pickup_point_id, order_date, delivery_date)
                    VALUES
                        ({nextNum}, '{code}', {statusId},
                         {pointId}, '{orderDate}', {deliveryDate})";

                DatabaseHelper.ExecuteNonQuery(query);

                MessageBox.Show("Заказ успешно добавлен!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                string query = $@"
                    UPDATE orders SET
                        order_code      = '{code}',
                        status_id       = {statusId},
                        pickup_point_id = {pointId},
                        order_date      = '{orderDate}',
                        delivery_date   = {deliveryDate}
                    WHERE order_id = {_orderId}";

                DatabaseHelper.ExecuteNonQuery(query);

                MessageBox.Show("Заказ успешно обновлён!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            this.Close();
        }

        // Отмена
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
