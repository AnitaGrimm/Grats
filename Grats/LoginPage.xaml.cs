using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VkNet;
using VkNet.Exception;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media.Animation;
using System.Net.Http;
using Windows.UI.ViewManagement;

namespace Grats
{
    /// <summary>
    /// Страница авторизации
    /// </summary>
    public sealed partial class LoginPage : Page, INotifyPropertyChanged
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private bool authenticating;

        public bool Authenticating
        {
            get { return authenticating; }
            set
            {
                this.authenticating = value;
                OnPropertyChanged();
                OnPropertyChanged("SignInFormVisibility");
                OnPropertyChanged("ProgressRingVisibility");
            }
        }

        public Visibility SignInFormVisibility => Authenticating ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ProgressRingVisibility => Authenticating ? Visibility.Visible : Visibility.Collapsed;

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Добавить плавности
            Authenticating = true;
            SignIn(LoginTextBox.Text, PasswordBox.Password);
        }

        private async void SignIn(String login, String password, long? captchaSid = null, string captchaKey = null)
        {

            var app = App.Current as App;
            try
            {
                await app.SignInAsync(LoginTextBox.Text, PasswordBox.Password, captchaSid, captchaKey);
            }
            catch (VkApiAuthorizationException exception)
            {
                await Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => { Authenticating = false; }
            );
                // TODO: Вывести куда-нибудь сообщения
                var alert = new ContentDialog()
                {
                    Title = "Не удается войти",
                    Content = "Проверьте правильность логина и пароля",
                    PrimaryButtonText = "OK"
                };
                Debug.WriteLine(exception.Message);
                await alert.ShowAsync();
                return;
            }
            catch (CaptchaNeededException exception)
            {
                await Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => { Authenticating = false; }
            );
                // TODO: Заменить диалог на что-нибудь более приятное
                CaptchaImage.Source = new BitmapImage(exception.Img);
                var result = await CaptchaDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                    SignIn(
                        LoginTextBox.Text,
                        PasswordBox.Password,
                        exception.Sid,
                        CaptchaTextBox.Text);
                return;
            }
            catch (VkApiException exception)
            {
                await Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => { Authenticating = false; }
            );
                // TODO: Вывести куда-нибудь сообщения
                var alert = new ContentDialog()
                {
                    Title = "Ошибка",
                    Content = exception.Message,
                    PrimaryButtonText = "OK"
                };
                Debug.WriteLine(exception.Message);
                await alert.ShowAsync();
                return;
            }
            catch (Exception exception)
            {
                await Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => { Authenticating = false; }
            );
                // TODO: Вывести куда-нибудь сообщения
                var alert = new ContentDialog()
                {
                    Title = "Ошибка",
                    Content = exception.Message,
                    PrimaryButtonText = "OK"
                };
                Debug.WriteLine(exception.Message);
                await alert.ShowAsync();
                return;
            }
            await Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => { Authenticating = false; }
            );
            // TODO: Добавить плавности
            Frame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ColorizeTitleBar();
        }

        private void ColorizeTitleBar()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = (this.Background as SolidColorBrush).Color;
        }
    }
}
