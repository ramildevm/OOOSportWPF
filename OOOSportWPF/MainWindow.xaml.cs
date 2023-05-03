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
        }


        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!isFirstTry)
            {
                CaptchaWindow captcha = new CaptchaWindow();
                if (captcha.ShowDialog() == true)
                {
                    isFirstTry = true;
                }
                else
                {
                    btnLogin.IsEnabled = false;
                    Thread.Sleep(10000);
                    btnLogin.IsEnabled = true;
                }
            }

            var login = txtLogin.Text;
            var password = txtPassword.Password;
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
                        new AdminProductsWindow().Show();
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
