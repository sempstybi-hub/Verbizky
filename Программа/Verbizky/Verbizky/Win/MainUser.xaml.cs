using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Verbizky.DB;

namespace Verbizky.Win
{
    public partial class MainUser : Window
    {
        private bool UserAccOn = false;
        private int IdUsers;

        public class SecurityEventViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Event { get; set; }
            public DateTime Date { get; set; }
            public int Days { get; set; }
            public string Img { get; set; }
            public City City { get; set; }
            public bool IsSubscribed { get; set; }
            public BitmapImage ImageSource { get; set; }
        }

        public MainUser()
        {
            InitializeComponent();
            LoadCard();
        }

        private void MainUser_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateButtonsVisibility();
        }

        private async void LoadCard()
        {
            Lists.ItemsSource = null;
            Load.Visibility = Visibility.Visible;
            await Task.Delay(1000);
            Load.Visibility = Visibility.Collapsed;

            using (VBZ_2Entities1 context = new VBZ_2Entities1())
            {
                try
                {
                    var events = context.SecurityEvents.Include("City").ToList();
                    var subscribedEventIds = UserAccOn ?
                        new HashSet<int>(context.SetEvents.Where(x => x.UserId == IdUsers && x.Activity == 1).Select(x => x.SecEventsId).ToList())
                        : new HashSet<int>();

                    var eventViewModels = events.Select(x=> {
                        BitmapImage imgSource = null;
                        string img = x.Img;

                        if (string.IsNullOrEmpty(img))
                            img = "https://img.icons8.com/skeuomorphism/1024/image.png";

                        if (!string.IsNullOrEmpty(img))
                        {
                            try
                            {
                                if (img.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                                {
                                    imgSource = new BitmapImage(new Uri(img));
                                }
                                else
                                {
                                    string base64String = img.Contains(",") ? img.Substring(img.IndexOf(",") + 1) : img;
                                    if (base64String.Length % 4 == 0 &&
                                        System.Text.RegularExpressions.Regex.IsMatch(base64String.Trim(), @"^[a-zA-Z0-9\+/]*={0,2}$"))
                                    {
                                        byte[] bytes = Convert.FromBase64String(base64String);
                                        imgSource = new BitmapImage();

                                        using (var ms = new MemoryStream(bytes))
                                        {
                                            imgSource.BeginInit();
                                            imgSource.StreamSource = ms;
                                            imgSource.CacheOption = BitmapCacheOption.OnLoad;
                                            imgSource.EndInit();
                                        }
                                    }
                                    else
                                    {
                                        imgSource = new BitmapImage(new Uri("https://img.icons8.com/skeuomorphism/1024/image.png"));
                                    }
                                }
                            }
                            catch
                            {
                                imgSource = new BitmapImage(new Uri("https://img.icons8.com/skeuomorphism/1024/image.png"));
                            }
                        }
                        else
                        {
                            imgSource = new BitmapImage(new Uri("https://img.icons8.com/skeuomorphism/1024/image.png"));
                        }

                        return new SecurityEventViewModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Event = x.Event,
                            Date = x.Date,
                            Days = x.Days,
                            Img = x.Img,
                            City = x.City,
                            IsSubscribed = subscribedEventIds.Contains(x.Id),
                            ImageSource = imgSource
                        };
                    }).ToList();

                    Lists.ItemsSource = eventViewModels;
                    UpdateButtonsVisibility();
                    NameFilCB.ItemsSource = context.SecurityEvents.Select(x => x.Name).Distinct().ToList();
                }
                catch (Exception ex)
                {
                    Messages(PackIconKind.Error,
                            "Ошибка",
                            ex.Message +
                            "\n\"попробуйте перезапустить программу\"",
                            "Ошибка");
                }
            }
        }

        private void UpdateButtonsVisibility()
        {
            if (Lists.ItemsSource == null) return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in Lists.Items)
                {
                    var container = Lists.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                    if (container == null) continue;

                    var subscribeButton = FindVisualChild<Button>(container, "SubscribeBTNS");
                    var doneCardUser = FindVisualChild<StackPanel>(container, "DoneCardUser");

                    if (subscribeButton != null && doneCardUser != null)
                    {
                        var viewModel = item as SecurityEventViewModel;
                        if (viewModel != null)
                        {
                            subscribeButton.Visibility = (UserAccOn && viewModel.IsSubscribed)
                                ? Visibility.Collapsed
                                : Visibility.Visible;

                            doneCardUser.Visibility = viewModel.IsSubscribed
                                ? Visibility.Visible
                                : Visibility.Collapsed;
                        }
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild && typedChild.Name == childName)
                {
                    return typedChild;
                }

                var result = FindVisualChild<T>(child, childName);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private void UpdCardBT_Click(object sender, RoutedEventArgs e)
        {
            LoadCard();
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

                case "Информация":
                    BgMessage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264c7e"));
                    IconMSG.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d6e7ff"));
                    NameMSG.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d6e7ff"));
                    break;

                case "Успех":
                    BgMessage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#267e2b"));
                    IconMSG.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d6ffda"));
                    NameMSG.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d6ffda"));
                    break;
            }
            IconMSG.Kind = Icon;
            NameMSG.Content = Name;
            TextMSG.Text = Text;
        }

        private void OkBT_Click(object sender, RoutedEventArgs e)
        {
            Message.Visibility = Visibility.Collapsed;
        }

        private void SubscribeBTNS_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                int id = (int)button.Tag;
                using (VBZ_2Entities1 context = new VBZ_2Entities1())
                {
                    var selectedItemId = context.SecurityEvents.FirstOrDefault(x => x.Id == id);
                    if (!UserAccOn)
                    {
                        Messages(PackIconKind.Warning,
                                 "Предупрежение",
                                 "Записаться можно только зарегистрированным пользователям, если вы не зарегистрированы, то зарегистрируйтесь, или войдите в аккаунт",
                                 "Предупрежение");
                    }
                    else
                    {
                        var existingSubscription = context.SetEvents
                            .FirstOrDefault(x => x.UserId == IdUsers && x.SecEventsId == id);

                        if (existingSubscription == null)
                        {
                            SetEvents setEvents = new SetEvents();
                            setEvents.UserId = IdUsers;
                            setEvents.SecEventsId = id;
                            setEvents.Activity = 1;

                            context.SetEvents.Add(setEvents);
                            context.SaveChanges();

                            Messages(PackIconKind.Check,
                                     "Успех",
                                     "Вы успешно записались на мероприятие",
                                     "Успех");

                            LoadCard();
                        }
                        else
                        {
                            Messages(PackIconKind.Info,
                                     "Информация",
                                     "Вы уже подписаны на это мероприятие",
                                     "Информация");
                        }
                    }
                }
            }
        }

        public void LogInUserInMail(string Mail)
        {
            using (VBZ_2Entities1 context = new VBZ_2Entities1())
            {
                var user = context.User.FirstOrDefault(x => x.Mail == Mail);
                UserAccOn = true;
                TitleName.Content = $"Главная - {user.Surname} {user.Name}";
                IdUsers = user.Id;
                AccBT.Visibility = Visibility.Visible;
                LogInBT.Visibility = Visibility.Collapsed;
                LoadCard();
            }
        }

        private void LogInBT_Click(object sender, RoutedEventArgs e)
        {
            login login = new login();
            login.Show();
            Close();
        }

        private void SetNameFilCbBT_Click(object sender, RoutedEventArgs e)
        {
            using (VBZ_2Entities1 context = new VBZ_2Entities1())
            {
                var events = context.SecurityEvents.Where(x => x.Name == NameFilCB.Text).Include("City").ToList();
                foreach (var ev in events)
                {
                    if (string.IsNullOrEmpty(ev.Img))
                    {
                        ev.Img = "https://img.icons8.com/skeuomorphism/1024/image.png";
                    }
                }
                Lists.ItemsSource = events;
                UpdateButtonsVisibility();
            }
        }

        private void DefoltFilBT_Click(object sender, RoutedEventArgs e)
        {
            LoadCard();
        }

        private void AccBT_Click(object sender, RoutedEventArgs e)
        {
            Users userss = new Users(IdUsers);
            userss.Show();
            Close();
        }

        private void DateSetFilDpBT_Click(object sender, RoutedEventArgs e)
        {
            using (VBZ_2Entities1 context = new VBZ_2Entities1())
            {
                DateTime Dates = Convert.ToDateTime(DateFiltDP.Text);
                var events = context.SecurityEvents.Where(x => x.Date == Dates).Include("City").ToList();
                foreach (var ev in events)
                {
                    if (string.IsNullOrEmpty(ev.Img))
                    {
                        ev.Img = "https://img.icons8.com/skeuomorphism/1024/image.png";
                    }
                }
                Lists.ItemsSource = events;
                UpdateButtonsVisibility();
            }
        }

        private void DoneBTNS_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                int id = (int)button.Tag;
                using (VBZ_2Entities1 context = new VBZ_2Entities1())
                {
                    var subscription = context.SetEvents
                        .FirstOrDefault(x => x.UserId == IdUsers && x.SecEventsId == id);

                    if (subscription != null)
                    {
                        context.SetEvents.Remove(subscription);
                        context.SaveChanges();

                        Messages(PackIconKind.Check,
                                 "Успех",
                                 "Вы отписались от мероприятия",
                                 "Успех");
                        LoadCard();
                    }
                }
            }
        }

        private void ExitBT_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}