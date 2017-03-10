using Grats.Model;
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

namespace Grats
{
    /// <summary>
    /// Детальная страница шаблона сообщения
    /// </summary>
    public sealed partial class TemplatesDetailPage : Page
    {
        public Template MessageTemplate;

        public TemplatesDetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Template)
                MessageTemplate = e.Parameter as Template;
            else
                throw new NotSupportedException("Parameter not supported");
        }
    }
}
