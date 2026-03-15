using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
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
using Verbizky.DB;
using static System.Net.Mime.MediaTypeNames;

namespace Verbizky.Win
{
    /// <summary>
    /// Логика взаимодействия для login.xaml
    /// </summary>
    public partial class login : Window
    {
        int CapchaCheks = 3;

        int c1;
        int c2;

        public login()
        {
            InitializeComponent();
        }

        private void Messages(PackIconKind Icon, string Name, string Text)
        {
            Message.Visibility = Visibility.Visible;
            IconMSG.Kind = Icon;
            NameMSG.Content = Name;
            TextMSG.Text = Text;
        }

        private void Messages(PackIconKind Icon, string Name, string Text, string Type)
        {
            Message.Visibility = Visibility.Visible;

            switch (Type)
            {
                case "Ошибка":
                    BgMessage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7D2B25"));
                    IconMSG.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDAD6"));
                    NameMSG.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDAD6"));
                    break;

                case "Предупрежение":
                    BgMessage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7e5726"));
                    IconMSG.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fbffd6"));
                    NameMSG.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fbffd6"));
                    break;

                case "Инфорамция":
                    BgMessage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264c7e"));
                    IconMSG.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d6e7ff"));
                    NameMSG.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d6e7ff"));
                    break;
            }

            IconMSG.Kind = Icon;
            NameMSG.Content = Name;
            TextMSG.Text = Text;
        }

        private void HomeBT_Click(object sender, RoutedEventArgs e)
        {
            MainUser mainUser = new MainUser();
            mainUser.Show();
            Close();
        }

        private void NoAccBT_Click(object sender, RoutedEventArgs e)
        {
            Register register = new Register();
            register.Show();
            Close();
        }

        private void LogInBT_Click(object sender, RoutedEventArgs e)
        {
            using (VBZ_2Entities1 context = new VBZ_2Entities1())
            {
                

                if (string.IsNullOrEmpty(LoginTXT.Text) ||
                    string.IsNullOrEmpty(PasswrdTXT.Password))
                {
                    Messages(PackIconKind.Error,
                             "Ошибка",
                             "Заполните поля\n" +
                             $"Телефон / Почта: {LoginTXT.Text}\n" +
                             $"Пароль: {PasswrdTXT.Password}",
                             "Ошибка");
                }
                else
                {
                    var MailUser = context.User.FirstOrDefault(x => x.Mail == LoginTXT.Text);

                    if (MailUser == null || MailUser.Password != PasswrdTXT.Password)
                    {
                        Messages(PackIconKind.Error,
                                 "Ошибка",
                                 "Неверная почта или пароль",
                                 "Ошибка");
                    }
                    else
                    {
                        Captcha.Visibility = Visibility.Visible;
                        int Cd1;
                        int Cd2;

                        Random random = new Random();

                        Cd1 = random.Next(1000, 9999);
                        Cd2 = random.Next(1000, 9999);

                        c1 = Cd1;
                        c2 = Cd2;

                        CaptchaCode.Content = $"{Cd1} - {Cd2}";
                    }
                }
            }
        }

        private async void CapthaLoad()
        {
            if (CapchaCheks == 0)
            {
                LogInBT.IsEnabled = false;
                RegBT.IsEnabled = false;
                LoginTXT.IsEnabled = false;
                PasswrdTXT.IsEnabled = false;

                await Task.Delay(3000);

                LogInBT.IsEnabled = true;
                RegBT.IsEnabled = true;
                LoginTXT.IsEnabled = true;
                PasswrdTXT.IsEnabled = true;

                CapchaCheks = 3;
            }
        }

        private void OkBT_Click(object sender, RoutedEventArgs e)
        {
            Message.Visibility = Visibility.Collapsed;
        }

        private void CheckCaptcha_Click(object sender, RoutedEventArgs e)
        {
            using (VBZ_2Entities1 context = new VBZ_2Entities1())
            {
                var MailUser = context.User.FirstOrDefault(x => x.Mail == LoginTXT.Text);

                try
                {
                    int Codes1 = Convert.ToInt32(Code1.Text);
                    int Codes2 = Convert.ToInt32(Code2.Text);

                    if (Code1.Text == null || Code2.Text == null ||
                    Codes1 != c1 || Codes2 != c2)
                    {
                        Captcha.Visibility = Visibility.Collapsed;
                        if (CapchaCheks > 0)
                        {
                            CapchaCheks -= 1;
                        }

                        Messages(PackIconKind.Error,
                                 "Ошибка",
                                 $"Неверная капча, у вас осталось {CapchaCheks} попыток",
                                 "Ошибка");

                        CapthaLoad();
                    }
                    else
                    {
                        Captcha.Visibility = Visibility.Collapsed;
                        switch (MailUser.RoleName)
                        {
                            case "Участники":
                                MainUser mainUser = new MainUser();
                                mainUser.Show();
                                mainUser.LogInUserInMail(LoginTXT.Text);
                                Close();
                                break;

                            case "Организаторы":
                                Organizer organizer = new Organizer(MailUser.Id);
                                organizer.Show();
                                Close();
                                break;

                            case "Модераторы":
                                Moderator moderator = new Moderator(MailUser.Id);
                                moderator.Show();
                                Close();
                                break;

                            case "Жюри":
                                Jury jury = new Jury(MailUser.Id);
                                jury.Show();
                                Close();
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Messages(PackIconKind.Error,
                             "Ошибка",
                             ex.Message,
                             "Ошибка");
                }

            }
        }

        private void CloseCaptcha_Click(object sender, RoutedEventArgs e)
        {
            Captcha.Visibility = Visibility.Collapsed;
        }

        private void ExitBT_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
