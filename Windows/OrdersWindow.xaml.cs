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
    /// Логика взаимодействия для OrdersWindow.xaml
    /// </summary>
    public partial class OrdersWindow : Window
    {
        private int _userId;
        private string _userFullName;
        private string _userRole;

        public OrdersWindow(int userId, string fullName, string role)
        {
            InitializeComponent();

            _userId = userId;
            _userFullName = fullName;
            _userRole = role;

            LblUser.Text = fullName;

            // Кнопки только для администратора
            if (_userRole != "Администратор")
            {
                BtnAdd.Visibility = Visibility.Collapsed;
                BtnDelete.Visibility = Visibility.Collapsed;
            }

            LoadOrders();
        }

        // Загружаем заказы из БД
        private void LoadOrders()
        {
            string query = @"
                SELECT
                    o.order_id,
                    o.order_number,
                    o.order_code,
                    os.status_name,
                    CONCAT(pp.city, ', ', pp.street, ', ', pp.house_number) as pickup_address,
                    o.order_date,
                    o.delivery_date
                FROM orders o
                LEFT JOIN order_statuses os  ON o.status_id       = os.status_id
                LEFT JOIN pickup_points pp   ON o.pickup_point_id = pp.point_id
                ORDER BY o.order_number";

            DataTable dt = DatabaseHelper.ExecuteQuery(query);

            var orders = new List<OrderRow>();

            foreach (DataRow row in dt.Rows)
            {
                orders.Add(new OrderRow
                {
                    OrderId = Convert.ToInt32(row["order_id"]),
                    OrderNumber = Convert.ToInt32(row["order_number"]),
                    OrderCode = row["order_code"].ToString(),
                    StatusName = row["status_name"].ToString(),
                    PickupAddress = row["pickup_address"].ToString(),
                    OrderDate = Convert.ToDateTime(row["order_date"])
                                       .ToString("dd.MM.yyyy"),
                    DeliveryDate = row["delivery_date"] == DBNull.Value
                                       ? ""
                                       : Convert.ToDateTime(row["delivery_date"])
                                           .ToString("dd.MM.yyyy")
                });
            }

            LvOrders.ItemsSource = orders;
        }

        // Двойной клик - редактировать заказ
        private void LvOrders_MouseDoubleClick(object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_userRole != "Администратор") return;

            var selected = LvOrders.SelectedItem as OrderRow;
            if (selected == null) return;

            var w = new OrderEditWindow(selected.OrderId);
            w.Closed += (s, args) => LoadOrders();
            w.ShowDialog();
        }

        // Добавить заказ
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // 0 = новый заказ
            var w = new OrderEditWindow(0);
            w.Closed += (s, args) => LoadOrders();
            w.ShowDialog();
        }

        // Удалить заказ
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = LvOrders.SelectedItem as OrderRow;

            if (selected == null)
            {
                MessageBox.Show("Выберите заказ для удаления!",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var answer = MessageBox.Show(
                $"Удалить заказ № {selected.OrderNumber}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes) return;

            // Сначала удаляем состав заказа
            DatabaseHelper.ExecuteNonQuery(
                $"DELETE FROM order_items WHERE order_id = {selected.OrderId}");

            // Потом удаляем заказ
            DatabaseHelper.ExecuteNonQuery(
                $"DELETE FROM orders WHERE order_id = {selected.OrderId}");

            MessageBox.Show("Заказ удалён!",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadOrders();
        }

        // Назад к товарам
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Класс строки заказа для ListView
    public class OrderRow
    {
        public int OrderId { get; set; }
        public int OrderNumber { get; set; }
        public string OrderCode { get; set; }
        public string StatusName { get; set; }
        public string PickupAddress { get; set; }
        public string OrderDate { get; set; }
        public string DeliveryDate { get; set; }
    }
}
