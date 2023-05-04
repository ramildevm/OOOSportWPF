using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для MakeEditProductWindow.xaml
    /// </summary>
    public partial class MakeEditProductWindow : Window
    {
        Product Product;
        public MakeEditProductWindow(Product product)
        {
            InitializeComponent();
            Product = product;
            InitializeWindowData();
            loadData();
        }

        private void InitializeWindowData()
        {
            using (var db = new EntityModel())
            {
                var categoryList = db.ProductCategory.ToArray();
                categoryComboBox.ItemsSource = categoryList;
                categoryComboBox.DisplayMemberPath = "ProductCategoryName";

                var manufacturerList = db.ProductManufacturer.ToArray();
                manufacturerComboBox.ItemsSource = manufacturerList;
                manufacturerComboBox.DisplayMemberPath = "ProductManufacturerName";

                var supplierList = db.ProductSupplier.ToArray();
                supplierComboBox.ItemsSource = supplierList;
                supplierComboBox.DisplayMemberPath = "ProductSupplierName";

                var unitTypeList = db.UnitType.ToArray();
                unitComboBox.ItemsSource = unitTypeList;
                unitComboBox.DisplayMemberPath = "UnitTypeName";
            }
        }

        private void loadData()
        {
            if (Product == null)
            {
                btnRemoveProduct.Visibility = Visibility.Collapsed;
                Product = new Product();
                DataContext = Product;
            }
            else
            {
                var imagePath = Product.ProductPhoto;
                if (imagePath != null && !String.IsNullOrEmpty(imagePath))
                {
                    var bitmap = new BitmapImage(new Uri(System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Resources\\Products\\" + imagePath));
                    productImage.Source = bitmap.Clone();
                    bitmap = null;
                }
                DataContext = Product;
                artikulTextBox.IsEnabled = false;
                categoryComboBox.SelectedIndex = Product.ProductCategoryID-1;
                manufacturerComboBox.SelectedIndex = Product.ProductManufacturerID-1;
                supplierComboBox.SelectedIndex = Product.ProductSupplierID-1;
                unitComboBox.SelectedIndex = Product.UnitTypeID-1;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new EntityModel())
            {

                if (String.IsNullOrEmpty(artikulTextBox.Text) ||
                    String.IsNullOrEmpty(unitCostTextBox.Text) ||
                    String.IsNullOrEmpty(maxDiscountTextBox.Text) ||
                    String.IsNullOrEmpty(nameTextBox.Text) ||
                    String.IsNullOrEmpty(currentDiscountTextBox.Text) ||
                    String.IsNullOrEmpty(quantityTextBox.Text) ||
                    String.IsNullOrEmpty(descriptionTextBox.Text) ||
                    unitComboBox.SelectedIndex == -1 ||
                    manufacturerComboBox.SelectedIndex == -1 ||
                    supplierComboBox.SelectedIndex == -1 ||
                    categoryComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Не все поля заполнены!");
                    return;
                }
                try
                {
                    Convert.ToDecimal(unitCostTextBox.Text.Replace('.', ','));
                    Convert.ToByte(maxDiscountTextBox.Text);
                    Convert.ToByte(currentDiscountTextBox.Text);
                    Convert.ToInt32(quantityTextBox.Text);
                }
                catch (FormatException exp)
                {
                    MessageBox.Show("Поля имеют неверный формат!");
                    return;
                }
                if (Product.ProductID == 0)
                {
                    Product.ProductArticleNumber = artikulTextBox.Text;
                    Product.ProductName = nameTextBox.Text;
                    Product.UnitTypeID = unitComboBox.SelectedIndex + 1;
                    Product.ProductCost = Convert.ToInt32(unitCostTextBox.Text);
                    Product.ProductMaxDiscountAmount = Convert.ToByte(maxDiscountTextBox.Text);
                    Product.ProductManufacturerID = manufacturerComboBox.SelectedIndex + 1;
                    Product.ProductSupplierID = supplierComboBox.SelectedIndex + 1;
                    Product.ProductCategoryID = categoryComboBox.SelectedIndex + 1;
                    Product.ProductDiscountAmount = Convert.ToByte(currentDiscountTextBox.Text);
                    Product.ProductQuantityInStock = Convert.ToInt32(quantityTextBox.Text);
                    Product.ProductDescription = descriptionTextBox.Text;
                    db.Product.Add(Product);
                    db.SaveChanges();
                    MessageBox.Show("Товар добавлен");
                    this.Close();
                }
                else
                {
                    Product.ProductArticleNumber = artikulTextBox.Text;
                    Product.ProductName = nameTextBox.Text;
                    Product.UnitTypeID = unitComboBox.SelectedIndex + 1;
                    Product.ProductCost = Convert.ToDecimal(unitCostTextBox.Text.Replace('.',','));
                    Product.ProductMaxDiscountAmount = Convert.ToByte(maxDiscountTextBox.Text);
                    Product.ProductManufacturerID = manufacturerComboBox.SelectedIndex + 1;
                    Product.ProductSupplierID = supplierComboBox.SelectedIndex + 1;
                    Product.ProductCategoryID = categoryComboBox.SelectedIndex + 1;
                    Product.ProductDiscountAmount = Convert.ToByte(currentDiscountTextBox.Text);
                    Product.ProductQuantityInStock = Convert.ToInt32(quantityTextBox.Text);
                    Product.ProductDescription = descriptionTextBox.Text;
                    db.Entry(Product).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    MessageBox.Show("Товар обновлен");
                    this.Close();
                }
            }
        }

        private void btnRemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new EntityModel())
            {
                db.Entry(this.Product).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
            }
            this.Close();
        }


        private void ButtonLoadImg_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(artikulTextBox.Text))
            {
                MessageBox.Show("Введите артикул товара!");
                return;
            }
            OpenFileDialog op = new OpenFileDialog();
            string folderpath = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Resources\\Products\\";
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                        "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                        "Portable Network Graphic (*.png)|*.png";

            bool? myResult;
            myResult = op.ShowDialog();
            if (myResult != null && myResult == true)
            {
                byte[] imageBytes = File.ReadAllBytes(op.FileName);
                string fileGuid = artikulTextBox.Text;
                string fileExtension = System.IO.Path.GetExtension(op.FileName);
                string fileName = fileGuid + fileExtension;
                string newFilePath = System.IO.Path.Combine(folderpath, fileName);
                productImage.Source = null;
                using (var fileStream = new FileStream(newFilePath, FileMode.OpenOrCreate))
                {
                    fileStream.Write(imageBytes, 0, imageBytes.Length);
                }

                Uri fileUri = new Uri(newFilePath);
                BitmapImage image = new BitmapImage(fileUri);                

                Product.ProductPhoto = fileName;               
                productImage.Source = image;
            }
            MessageBox.Show("Изображение загружено!");            
        }
    }
}
