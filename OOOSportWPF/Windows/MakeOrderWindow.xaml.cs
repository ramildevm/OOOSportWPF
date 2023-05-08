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
    /// Логика взаимодействия для MakeOrderWindow.xaml
    /// </summary>
    public partial class MakeOrderWindow : Window
    {
        private User user;
        private List<Product> orderProducts;
        private List<ProductObjectClass> productObjects;

        public MakeOrderWindow()
        {
            InitializeComponent();
            
        }

        public MakeOrderWindow(List<Product> orderProducts, User user)
        {
            InitializeComponent();
            this.user = user;
            this.orderProducts = orderProducts;
            loadProducts();
            setOrderData();
            refreshOrderData();
        }

        private void refreshOrderData()
        {
           
            using (var db = new EntityModel())
            {
                int price = 0, discount = 0, totalPrice = 0;
                bool isEnough = true;
                foreach(var p in productObjects)
                {
                    if (p.Product.ProductQuantityInStock < 3)
                        isEnough = false;
                    int productPrice = (int)p.Product.ProductCost * p.Quantity;
                    price += productPrice;
                    totalPrice += (productPrice * (100 - Convert.ToInt32(p.Product.ProductDiscountAmount))) / 100;
                }
                if(!isEnough)
                    txtDeliveryDate.Text = "Доставка до: " + DateTime.Now.AddDays(6).ToString("yyyy-MM-dd");
                txtPrice.Text = "Цена: " + price + " руб.";
                txtTotalPrice.Text = "Цена со скидкой: " + totalPrice + " руб.";
            }
        }

        private void setOrderData()
        {
            using (var db = new EntityModel())
            {
                var pickupList = db.PickupPoint.ToArray();
                comboBoxPickUp.ItemsSource = pickupList;
                comboBoxPickUp.DisplayMemberPath = "Address";
                var lastOrder = db.Order.ToList().LastOrDefault();
                txtId.Text = "Id заказа: " + (lastOrder == null ? "1" : (lastOrder.OrderID + 1).ToString());
                DateTime date = DateTime.Now;
                DateTime newDate = date.AddDays(3);
                txtCreateDate.Text = "Создан: " + newDate.ToString("yyyy-MM-dd");
                txtDeliveryDate.Text = "Доставка до: " + date.ToString("yyyy-MM-dd");
            }
        }

        private void loadProducts()
        {
            productObjects = new List<ProductObjectClass>();
            using (var db = new EntityModel())
            {
                foreach (var product in orderProducts)
                {
                    productObjects.Add(new ProductObjectClass(product,
                        db.ProductCategory.Where(v=>v.ProductCategoryID == product.ProductCategoryID).First(),
                        db.ProductManufacturer.Where(v=>v.ProductManufacturerID == product.ProductManufacturerID).First(),
                        db.ProductSupplier.Where(v=>v.ProductSupplierID == product.ProductSupplierID).First(),
                        db.UnitType.Where(v=>v.UnitTypeID == product.UnitTypeID).First()));
                    var imagePath = product.ProductPhoto;
                    if (imagePath != null && !String.IsNullOrEmpty(imagePath))
                        productObjects.Last().Product.ProductPhoto = (System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Resources\\Products\\" + imagePath);
                    else
                        productObjects.Last().Product.ProductPhoto = (System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Resources\\" + "picture.png");
                   
                }
            }
            productsGrid.ItemsSource = productObjects;

        }

        private class ProductObjectClass
        {

            public ProductObjectClass(Product product, ProductCategory category, ProductManufacturer manufacturer, ProductSupplier productSupplier, UnitType unitType)
            {
                Product = product;
                Category = category;
                Manufacturer = manufacturer;
                Supplier = productSupplier;
                UnitType = unitType;
                Quantity = 1;
            }

            public Product Product { get; }
            public ProductCategory Category { get; }
            public ProductManufacturer Manufacturer { get; }
            public ProductSupplier Supplier { get; }
            public UnitType UnitType { get; }
            public int Quantity { get; set; }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var row = textBox?.DataContext as ProductObjectClass;
            try
            {
                int quantity = Convert.ToInt32(textBox.Text);
                if (quantity == 0)
                {
                    var result = MessageBox.Show("Вы хотите убрать товар?", "Внимание", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        productObjects.Remove(row);
                        productsGrid.Items.Refresh();

                    }
                    else
                    {
                        textBox.Text = "1";
                    }
                }
                productObjects.FirstOrDefault(v => v.Product == row.Product).Quantity = quantity;
                refreshOrderData();
            }
            catch
            {
                textBox.Text = "";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var row = button?.DataContext as ProductObjectClass;
            productObjects.Remove(row); 
            productsGrid.Items.Refresh();
            refreshOrderData();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOrderProduct_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
