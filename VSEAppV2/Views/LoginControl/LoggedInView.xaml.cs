using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using VSEAppV2.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using VSEAppV2.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VSEAppV2.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoggedInView : Page
    {
        public LoggedInView()
        {
            this.InitializeComponent();
            LoggedInFrame.Navigate(typeof(VSEValuesPage));

            //this.DataContext = new LoggedInViewModel(Ioc.Default.GetService<IAppState>());
            this.DataContext = Ioc.Default.GetService<LoggedInViewModel>();
        }

        public LoggedInViewModel ViewModel => (LoggedInViewModel)this.DataContext;

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            ContentControl navSelection = (sender as NavigationView).SelectedItem as ContentControl;
            switch (navSelection.Tag)
            {
                case "Values":
                    LoggedInFrame.Navigate(typeof(VSEValuesPage));
                    mainNavView.Header = "Values";
                    break;
                case "Skills":
                    LoggedInFrame.Navigate(typeof(VSESkillsPage));
                    mainNavView.Header = "Skills";
                    break;
                case "Experiences":
                    LoggedInFrame.Navigate(typeof(VSEExperiencesPage));
                    mainNavView.Header = "Experiences";
                    break;
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Unloaded();
        }
    }
}
