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

namespace Grats
{
    /// <summary>
    /// Страница авторизации
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Добавить плавности
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
            catch (CaptchaNeededException exception)
            {
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
            // TODO: Добавить плавности
            Frame.Navigate(typeof(MainPage));
        }
    }
}
