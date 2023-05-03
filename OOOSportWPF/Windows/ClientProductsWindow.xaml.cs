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
        public ClientProductsWindow()
        {
            InitializeComponent();
            loadDataSet("");
            loadData();
        }
        public ClientProductsWindow(bool flag)
        {
            InitializeComponent();
            loadDataSet("");
            loadData();
        }

        private void loadDataSet(string v)
        {
            productPanel = productsPanel;
            using (var db = new EntityModel())
            {
                if(filterText.Replace(" ", "") != "")
                {
                    products = (from p in db.Product where p.ProductName.Contains(filterText) select p).ToList();
                }
                else
                {
                    products = db.Product.ToList();
                }
                if (v == "0")
                {
                    products = (from p in products where p.ProductMaxDiscountAmount < 10 select p).ToList();
                }
                else if (v == "10")
                {
                    products = (from p in products where p.ProductMaxDiscountAmount < 15 && p.ProductMaxDiscountAmount > 10 select p).ToList();
                }
                else if (v == "15")
                {
                    products = (from p in products where p.ProductMaxDiscountAmount > 15 select p).ToList();
                }
            }
        }

        private void loadData()
        {
            productsPanel.Children.Clear();
            using (var db = new EntityModel()) {
                foreach (var product in products)
                {
                    var mainPanel = new Grid();
                    mainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
                    mainPanel.ColumnDefinitions.Add(new ColumnDefinition());
                    mainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });

                    var image = new Image();

                    var middlePanel = new StackPanel() { Orientation = Orientation.Vertical, Margin = new Thickness(10,10,10,10)};
                    var txtName = new TextBlock() { Text = "Название продукта: ", FontWeight=FontWeights.Bold};
                    var txtDesc = new TextBlock() { Text = "Описание: " };
                    var txtManufacturer = new TextBlock() { Text = "Производитель: " };
                    var txtPrice = new TextBlock() { Text = "Цена: " };
                    middlePanel.Children.Add(txtName);
                    middlePanel.Children.Add(txtDesc);
                    middlePanel.Children.Add(txtManufacturer);
                    middlePanel.Children.Add(txtPrice);

                    var endPanel = new Grid() { Margin = new Thickness(5, 5, 5, 5) };
                    endPanel.RowDefinitions.Add(new RowDefinition());
                    endPanel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(70)});

                    var txtDiscount = new TextBlock() { FontSize = 20, HorizontalAlignment=HorizontalAlignment.Center, VerticalAlignment=VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                    var btnAdd = new Button() { Content = "Добавить", Tag = product };
                    btnAdd.Click += BtnEdit_Click;
                    Grid.SetRow(txtDiscount, 0);
                    Grid.SetRow(btnAdd, 1);
                    endPanel.Children.Add(txtDiscount);
                    endPanel.Children.Add(btnAdd);

                    txtName.Text += product.ProductName;
                    txtDesc.Text += product.ProductDescription;
                    txtManufacturer.Text += db.ProductManufacturer.Find(product.ProductManufacturerID).ProductManufacturerName;
                    txtPrice.Text += product.ProductCost;

                    txtDiscount.Text += product.ProductMaxDiscountAmount;
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

                    productPanel.Children.Add(mainPanel);
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            new MainWindow().ShowDialog();
            
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            filterText = txtSearch.Text;
            loadDataSet(filterDiscount);
            loadData();
        }
        private void myComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
            {
                string selectedValue = selectedItem.Content.ToString();
                switch (selectedValue)
                {
                    case "0-9,99%":
                        filterDiscount = "0";
                        break;
                    case "10-14,99%":
                        filterDiscount = "10";
                        break;
                    case "15%":
                        filterDiscount = "15";
                        break;
                    case "Все":
                        filterDiscount = "";
                        break;
                    default:
                        break;
                }

                loadDataSet(filterDiscount);
                loadData();
            }
        }
    }
}
