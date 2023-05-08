using System;
using System.Collections.Generic;
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

namespace OOOSportWPF.Windows
{
    /// <summary>
    /// Логика взаимодействия для ClientProductsWindow.xaml
    /// </summary>
    public partial class ClientProductsWindow : Window
    {
        List<Product> products;
        StackPanel productPanel;
        string filterText = "";
        string filterDiscount = "";
        int filterPrice = 2;
        private User User;
        private List<Product> orderProducts = new List<Product>();

        public ClientProductsWindow()
        {
            InitializeComponent();
            loadDataSet();
            loadData();
        }
        public ClientProductsWindow(User user)
        {
            InitializeComponent();
            this.User = user;
            InitializeWindowData();
            loadDataSet();
            loadData();
        }

        private void InitializeWindowData()
        {
            if(User!=null)
                txtFIO.Text = $"{User.UserSurname} {User.UserName} {User.UserPatronymic}";
        }

        private void loadDataSet()
        {
            productPanel = productsPanel;
            using (var db = new EntityModel())
            {
                var productsAll = db.Product.OrderByDescending(v => v.ProductID).ToList();
                if (filterText.Replace(" ", "") != "")
                {
                    products = (from p in db.Product where p.ProductName.Contains(filterText) select p).ToList();
                }
                else
                {
                    products = productsAll;
                }
                if (filterDiscount == "0")
                {
                    products = (from p in products where p.ProductMaxDiscountAmount < 10 select p).ToList();
                }
                else if (filterDiscount == "10")
                {
                    products = (from p in products where p.ProductMaxDiscountAmount < 15 && p.ProductMaxDiscountAmount >= 10 select p).ToList();
                }
                else if (filterDiscount == "15")
                {
                    products = (from p in products where p.ProductMaxDiscountAmount >= 15 select p).ToList();
                }
                if (filterPrice == 0)
                {
                    products = products.OrderByDescending(p => p.ProductCost).ToList();
                }else if (filterPrice == 1)
                {
                    products = products.OrderBy(p => p.ProductCost).ToList();
                }
                else
                {
                    products = products.OrderByDescending(p => p.ProductID).ToList();
                }
                txtCount.Text = $"Количество товаров: {products.Count()} из {productsAll.Count()}.";
            }
        }

        private void loadData()
        {
            productsPanel.Children.Clear();
            using (var db = new EntityModel())
            {
                foreach (var product in products)
                {
                    var mainPanel = new Grid();
                    mainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
                    mainPanel.ColumnDefinitions.Add(new ColumnDefinition());
                    mainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });

                    var image = new Image();
                    BitmapImage bitmap;
                    var imagePath = product.ProductPhoto;
                    if (imagePath != null && !String.IsNullOrEmpty(imagePath))
                        bitmap = new BitmapImage(new Uri(System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Resources\\Products\\" + imagePath));
                    else
                        bitmap = new BitmapImage(new Uri(System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Resources\\" + "picture.png"));
                    image.Source = bitmap;

                    var middlePanel = new StackPanel() { Orientation = Orientation.Vertical, Margin = new Thickness(10, 10, 10, 10) };
                    var txtName = new TextBlock() { Text = "Название продукта: ", FontWeight = FontWeights.Bold };
                    var txtDesc = new TextBlock() { Text = "Описание: " };
                    var txtManufacturer = new TextBlock() { Text = "Производитель: " };
                    var txtPrice = new TextBlock() { Text = "Цена: " };
                    middlePanel.Children.Add(txtName);
                    middlePanel.Children.Add(txtDesc);
                    middlePanel.Children.Add(txtManufacturer);
                    middlePanel.Children.Add(txtPrice);

                    var endPanel = new Grid() { Margin = new Thickness(5, 5, 5, 5) };
                    endPanel.RowDefinitions.Add(new RowDefinition());

                    var txtDiscount = new TextBlock() { FontSize = 18, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };

                    Grid.SetRow(txtDiscount, 0);
                    endPanel.Children.Add(txtDiscount);

                    txtName.Text += product.ProductName;
                    txtDesc.Text += product.ProductDescription;
                    txtManufacturer.Text += db.ProductManufacturer.Find(product.ProductManufacturerID).ProductManufacturerName;
                    txtPrice.Text += product.ProductCost;

                    txtDiscount.Text += "Скидка до -" + product.ProductMaxDiscountAmount + "%";
                    if (product.ProductMaxDiscountAmount > 15)
                    {
                        txtDiscount.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7fff00"));
                    }

                    Grid.SetColumn(image, 0);
                    Grid.SetColumn(middlePanel, 1);
                    Grid.SetColumn(endPanel, 2);

                    mainPanel.Children.Add(image);
                    mainPanel.Children.Add(middlePanel);
                    mainPanel.Children.Add(endPanel);

                    ContextMenu contextMenu = new ContextMenu();
                    MenuItem addMenuItem = new MenuItem();
                    addMenuItem.Header = "Добавить к заказу";
                    addMenuItem.Tag = product;
                    contextMenu.Items.Add(addMenuItem);
                    addMenuItem.Click += BtnAdd_Click;
                    mainPanel.ContextMenu = contextMenu;

                    productPanel.Children.Add(mainPanel);
                }
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as MenuItem).Tag as Product;
            if (orderProducts.IndexOf(product) == -1)
            {
                orderProducts.Add(product);
                txtQuantity.Text = "Количество позиций в заказе: " + orderProducts.Count.ToString();
                btnOrder.Visibility = Visibility.Visible;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();

        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            filterText = txtSearch.Text;
            loadDataSet();
            loadData();
        }
        private void myComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            int selectedItem = comboBox.SelectedIndex;

                switch (selectedItem)
                {
                    case 0:
                        filterDiscount = "0";
                        break;
                    case 1:
                        filterDiscount = "10";
                        break;
                    case 2:
                        filterDiscount = "15";
                        break;
                    case 3:
                        filterDiscount = "";
                        break;
                    default:
                    filterDiscount = "";
                    break;
                }

                loadDataSet();
                loadData();
        }

        private void priceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            int selectedItem = comboBox.SelectedIndex;
            filterPrice = selectedItem;
            loadDataSet();
            loadData();

        }

        private void btnOrderProduct_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new MakeOrderWindow(orderProducts,User).ShowDialog();
            orderProducts.Clear();
            txtQuantity.Text = "";
            btnOrder.Visibility = Visibility.Collapsed;
            this.Show();
        }
    }
}
