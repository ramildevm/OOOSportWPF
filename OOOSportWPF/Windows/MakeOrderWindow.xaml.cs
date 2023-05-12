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
using Word = Microsoft.Office.Interop.Word;

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
        DateTime createDate, deliveryDate;
        int getCode, price = 0, totalPrice =0;

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
            if (productObjects.Count == 0)
            {
                MessageBox.Show("Отмена заказа","Внимание",MessageBoxButton.OK);
                this.Close();
            }
            using (var db = new EntityModel())
            {
                bool isEnough = true;
                foreach(var p in productObjects)
                {
                    if (p.Product.ProductQuantityInStock < 3)
                        isEnough = false;
                    int productPrice = (int)p.Product.ProductCost * p.Quantity;
                    price += productPrice;
                    totalPrice += productPrice * (100 - Convert.ToInt32(p.Product.ProductDiscountAmount)) / 100;
                }
                if (!isEnough) {
                    deliveryDate = DateTime.Now.AddDays(6);
                    txtDeliveryDate.Text = "Доставка до: " + deliveryDate.ToString("yyyy/MM/dd");
                }
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
                createDate = DateTime.Now;
                deliveryDate = createDate.AddDays(3);
                txtCreateDate.Text = "Создан: " + createDate.ToString("yyyy/MM/dd");
                txtDeliveryDate.Text = "Доставка до: " + deliveryDate.ToString("yyyy/MM/dd");

                var rnd = new Random();
                getCode = rnd.Next(100, 999);
                while(db.Order.FirstOrDefault(o=>o.OrderStatusID!=2 && o.OrderGetCode == 0)!=null)
                    getCode = rnd.Next(100, 999);
                txtCode.Text = "Код для получения: " + getCode.ToString();
                
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
                        productObjects.FirstOrDefault(v => v.Product == row.Product).Quantity = quantity;
                    }
                }
                else
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
            if(comboBoxPickUp.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите пункт выдачи!");
                return;
            }
            var order = new Order()
            {
                OrderStatusID = 1,
                OrderCreateDate = createDate,
                OrderDeliveryDate = deliveryDate,
                PickupPointID = comboBoxPickUp.SelectedIndex + 1,
                OrderGetCode = getCode,
                UserID = null
            };
            if (user != null)
                order.UserID = user.UserID;
            using(var db = new EntityModel())
            {
                var result = db.Order.Add(order);
                foreach(var product in productObjects)
                {
                    db.OrderProduct.Add(new OrderProduct()
                    {
                        OrderID = result.OrderID,
                        ProductID = product.Product.ProductID,
                        Count = product.Quantity
                    });
                }
                db.SaveChanges();
                makePdf(result);
                this.Close();
            }
        }

        private void makePdf(Order result)
        {
            var app = new Word.Application();
            Word.Document doc = app.Documents.Add();

            // создание параграфа и добавление текста
            Word.Paragraph para = doc.Content.Paragraphs.Add();
            para.Range.Text = "ТАЛОН";
            para.Range.Font.Size = 14;
            para.Range.Font.Bold = 1;
            para.Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            para.Range.InsertParagraphAfter();

            // добавление даты и номера заказа
            para.Range.Text = $"Номер заказа: {result.OrderID}\nДата заказа: {result.OrderCreateDate.ToShortDateString()}";
            para.Range.Font.Size = 12;
            para.Range.Font.Bold = 0;
            para.Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            para.Range.InsertParagraphAfter();

            para.Range.Text = "Состав заказа:";
            para.Range.Font.Size = 12;
            para.Range.Font.Bold = 0;
            para.Range.InsertParagraphAfter();

            foreach (var product in productObjects)
            {
                para.Range.Text = $"  - {product.Product.ProductName}: {product.Quantity} шт. x {product.Product.ProductCost} руб. = {product.Product.ProductCost * product.Quantity} руб.";
                para.Range.Font.Size = 12;
                para.Range.Font.Bold = 0;
                para.Range.InsertParagraphAfter();
            }

            para.Range.Text = $"Сумма заказа: {totalPrice}\nСумма скидки: {totalPrice-price}";
            para.Range.Font.Size = 12;
            para.Range.Font.Bold = 0;
            para.Range.InsertParagraphAfter();

            // добавление пункта выдачи и кода получения
            para.Range.Text = $"Пункт выдачи: {comboBoxPickUp.SelectedValuePath}";
            para.Range.Font.Size = 12;
            para.Range.Font.Bold = 0; 
            para.Range.InsertParagraphAfter();

            Word.Range codeRange = doc.Content.Paragraphs.Add().Range;
            codeRange.Text = $"Код получения: {getCode}";
            codeRange.Font.Size = 14;
            codeRange.Font.Bold = 1;
            codeRange.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            codeRange.InsertParagraphAfter();


            // Сохраняем документ в формате PDF
            string filename = $"Талон заказа{result.OrderID.ToString()}.pdf";

            app.Visible = true;
            doc.SaveAs2(@"D:\"+filename, Word.WdExportFormat.wdExportFormatPDF);
            MessageBox.Show(@"Товар оформлен. Талон сохранен по пути: D:\" + filename, "Результат");
            // Открываем полученный файл
            System.Diagnostics.Process.Start(@"D:\" + filename);

            // Очищаем ресурсы
            doc.Close();
            app.Quit();
        }
    }
}
