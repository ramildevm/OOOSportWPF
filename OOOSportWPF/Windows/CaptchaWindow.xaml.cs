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
    /// Логика взаимодействия для CaptchaWindow.xaml
    /// </summary>
    public partial class CaptchaWindow : Window
    {
        public CaptchaWindow()
        {
            InitializeComponent();
            makeCaptcha();
        }

        private void makeCaptcha()
        {
            var allowchar = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwyz1234567890";            

            Random rnd = new Random();
            var captcha = "";
            for (int i = 0; i < 4; i++)
            {
                captcha += allowchar[rnd.Next(0, allowchar.Length - 1)]; 
            }
            txtCaptcha.Content = captcha;

        }

        private void ButtonCheck_Click(object sender, RoutedEventArgs e)
        {
            if (txtCaptchaCheck.Text.ToString() == txtCaptcha.Content.ToString())
                this.DialogResult = true;
            else
                this.DialogResult = false;
        }
    }
}
