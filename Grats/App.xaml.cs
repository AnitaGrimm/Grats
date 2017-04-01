using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Microsoft.EntityFrameworkCore;
using Grats.Model;
using Windows.ApplicationModel.Resources;
using VkNet;
using VkNet.Enums.Filters;
using Windows.Storage;
using System.Threading.Tasks;
using VkNet.Exception;
using System.Diagnostics;
using VkNet.Utils.AntiCaptcha;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.UI.Xaml.Media.Animation;
using Windows.ApplicationModel.Background;

namespace Grats
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    sealed partial class App : Application
    {
        static string VK_API_LOGIN_KEY = "vk_api_login";
        static string VK_API_PASSWORD_KEY = "vk_api_password";
        static string VK_API_APP_ID_KEY = "vk_api_app_id";

        public VkApi VKAPI;
        public ulong VKAPIApplicationID;
        /// <summary>
        /// Инициализирует одноэлементный объект приложения.  Это первая выполняемая строка разрабатываемого
        /// кода; поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Как оказалось, EF DBContext не тред-сейфти
        /// </summary>
        public GratsDBContext dbContext
        {
            get
            {
                return new GratsDBContext();
            }
        }

        private void InitializeDB()
        {
            dbContext.Database.Migrate();
            CreateDefaultTemplates();
        }

        private void CreateDefaultTemplates()
        {
            var db = dbContext;
            if (db.Templates.Count() == 0)
            {
                // TODO: Перенести в ресурсы
                db.Add(new Template()
                {
                    Name = "День рождения",
                    Text = "Дорог^пол{м:ой, ж:ая, ой} ^имя ^фамилия, поздравляю тебя с днем рождения!",
                    IsEmbedded = true
                });
                db.SaveChanges();
            }
        }

        private void InitializeVKAPI()
        {
            VKAPIApplicationID = ulong.Parse(Resources[VK_API_APP_ID_KEY] as String);
            VKAPI = new VkApi();
            VKAPI.OnTokenExpires += VKAPI_OnTokenExpires;
        }

        private void VKAPI_OnTokenExpires(VkApi api)
        {
            this.SignOut();
        }

        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем.  Будут использоваться другие точки входа,
        /// например, если приложение запускается для открытия конкретного файла.
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            InitializeDB();
            InitializeVKAPI();

            Frame rootFrame = Window.Current.Content as Frame;

            // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
            // только обеспечьте активность окна
            if (rootFrame == null)
            {
                // Создание фрейма, который станет контекстом навигации, и переход к первой странице
                rootFrame = new Frame();

                // Настройка переходов
                rootFrame.ContentTransitions = new TransitionCollection();
                var navigationTheme = new NavigationThemeTransition();
                navigationTheme.DefaultNavigationTransitionInfo = new DrillInNavigationTransitionInfo();
                rootFrame.ContentTransitions.Add(navigationTheme);

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Загрузить состояние из ранее приостановленного приложения
                }

                // Размещение фрейма в текущем окне
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    if (TrySignIn())
                    {
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }
                    else
                        rootFrame.Navigate(typeof(LoginPage), e.Arguments);
                }
                // Обеспечение активности текущего окна
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Вызывается в случае сбоя навигации на определенную страницу
        /// </summary>
        /// <param name="sender">Фрейм, для которого произошел сбой навигации</param>
        /// <param name="e">Сведения о сбое навигации</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Вызывается при приостановке выполнения приложения.  Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }

        #region Авторизация

        // TODO: Добавить двухфакторную аутентификацию

        public bool TrySignIn()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var login = localSettings.Values[VK_API_LOGIN_KEY] as String;
            var password = localSettings.Values[VK_API_PASSWORD_KEY] as String;
            if (String.IsNullOrEmpty(login) || String.IsNullOrEmpty(password))
                return false;
            else
            {
                try
                {
                    SignIn(login, password);
                }
                catch (VkApiAuthorizationException e)
                {
                    Debug.Fail(e.Message);
                    return false;
                }
                catch (VkApiException e)
                {
                    Debug.Fail(e.Message);
                    return false;
                }
                return true;
            }
        }

        public void SignIn(String login, String password, long? captchaSid = null, string captchaValue = null)
        {
            VKAPI.Authorize(new ApiAuthParams()
            {
                ApplicationId = VKAPIApplicationID,
                Login = login,
                Password = password,
                Settings = Settings.All,
                CaptchaSid = captchaSid,
                CaptchaKey = captchaValue
            });
            RegisterTask();
            SaveCredentials(login, password);
        }

        public async Task SignInAsync(String login, String password, long? captchaSid = null, string captchaValue = null)
        {
            await VKAPI.AuthorizeAsync(new ApiAuthParams()
            {
                ApplicationId = VKAPIApplicationID,
                Login = login,
                Password = password,
                Settings = Settings.All,
                CaptchaSid = captchaSid,
                CaptchaKey = captchaValue
            });
            RegisterTask();
            SaveCredentials(login, password);
        }

        public void SignOut()
        {
            UnregisterTask("SendGrats");
            ClearCredentials();
            ShowLoginPage();
        }

        public void SaveCredentials(String login, String password)
        {
            // TODO: Сохранять пароль в секретных настройках
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[VK_API_LOGIN_KEY] = login;
            localSettings.Values[VK_API_PASSWORD_KEY] = password;
        }

        public void ClearCredentials()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values.Remove(VK_API_LOGIN_KEY);
            localSettings.Values.Remove(VK_API_PASSWORD_KEY);
        }

        #endregion

        #region Фоновые задачи

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
          MessageDispatcher.MessageDispatcher messageDispatcher = new MessageDispatcher.MessageDispatcher((App.Current as App).dbContext,null);
            try
            {
                messageDispatcher.Dispatch();
            }
            catch(VkApiException e)
            {
                //Обработка исключений VK
            }
        }
        private async void RegisterTask()
        {
            var taskRegistered = false;
            ApplicationTrigger trigger = null;
            var taskName = "SendGrats";
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    taskRegistered = true;
                    break;
                }
            }
            if (!taskRegistered)
            {

                var builder = new BackgroundTaskBuilder()
                {
                    Name = taskName,
                };
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                trigger = new ApplicationTrigger();
                var x= await BackgroundExecutionManager.RequestAccessAsync();
                builder.SetTrigger(trigger);
                builder.IsNetworkRequested = true;
                BackgroundTaskRegistration task = builder.Register();
                await trigger.RequestAsync();
                RegisterTaskByTime();
            }
        }

        private async void RegisterTaskByTime()
        {
            var taskRegistered = false;
            var x = await BackgroundExecutionManager.RequestAccessAsync();
            var trigger = new TimeTrigger(15, false);
            var taskName = "SendGratsByTime";
            foreach(var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name==taskName)
                {
                    taskRegistered = true;
                    break;
                }
            }
            if (!taskRegistered)
            {

                var builder = new BackgroundTaskBuilder();
                builder.Name = taskName;
                builder.TaskEntryPoint = typeof(TimerTask.BackgroundTaskbyTime).ToString();
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                
                builder.SetTrigger(new TimeTrigger(15,false));
                builder.IsNetworkRequested = true;
                BackgroundTaskRegistration task = builder.Register();
                task.Completed += Task_Completed;
                //await trigger.RequestAsync();
            }
        }

        private async void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
         foreach(var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == "SendGrats")
                {
                    BackgroundTaskRegistration taskReg = task.Value as BackgroundTaskRegistration;
                    ApplicationTrigger appTr = taskReg.Trigger as ApplicationTrigger;
                    await appTr.RequestAsync();
                    break; 
                }
            }
        }

        private void UnregisterTask(string taskName)
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    task.Value.Unregister(true);
                }
            }
        }
        #endregion

        #region Общая навигация

        public void ShowLoginPage()
        {
            (Window.Current.Content as Frame).Navigate(typeof(LoginPage), new DrillOutThemeAnimation());
        }

        #endregion
    }
}
