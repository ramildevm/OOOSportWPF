using OOOSportWPF.Windows;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OOOSportWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isFirstTry = true;
        public MainWindow()
        {
            InitializeComponent();
            txtLogin.Text = "loginDEpxl2018";
            txtPassword.Password = "P6h4Jq";
        }


        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!isFirstTry)
            {
                CaptchaWindow captcha = new CaptchaWindow();
                var result = captcha.ShowDialog();
                if (result == true)
                {
                    isFirstTry = true;
                }
                else 
                {
                    btnLogin.IsEnabled = false;
                    await Task.Delay(10000);
                    btnLogin.IsEnabled = true;
                    return;
                }
            }

            var login = txtLogin.Text;
            var password = txtPassword.Password;
            if(String.IsNullOrEmpty(login) || String.IsNullOrEmpty(password)) {
                MessageBox.Show("Не все поля заполнены!");
                isFirstTry = false;
                return;
            }
            using (var db = new EntityModel())
            {
                var user = db.User.Where(u => u.UserLogin == login && u.UserPassword == password).FirstOrDefault();
                if (user == null)
                {
                    MessageBox.Show("Логин или пароль введен неверно!");
                    isFirstTry = false;
                    return;
                }
                else
                {
                    var role = db.Role.Find(user.RoleID);
                    if (role.RoleName == db.Role.Find(1).RoleName)
                    {
                        new ClientProductsWindow(user).Show();
                        this.Close();
                    }
                    else if (role.RoleName == db.Role.Find(2).RoleName)
                    {
                        new AdminProductsWindow(user).Show();
                        this.Close();
                    }
                    else if (role.RoleName == db.Role.Find(3).RoleName)
                    {
                        new ManagerProductsWindow().Show();
                        this.Close();
                    }
                }
            }

        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            new ClientProductsWindow(null).Show();
            this.Close();
        }
    }
}
