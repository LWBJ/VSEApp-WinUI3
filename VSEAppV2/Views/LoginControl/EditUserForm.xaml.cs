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
    public sealed partial class EditUserForm : UserControl
    {
        public EditUserForm()
        {
            this.InitializeComponent();
            //this.DataContext = new EditUserFormViewModel(Ioc.Default.GetService<IAppState>());
            this.DataContext = Ioc.Default.GetService<EditUserFormViewModel>();
        }

        public EditUserFormViewModel ViewModel => (EditUserFormViewModel)this.DataContext;

        private void OpenEditUserDialog_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.SetInitialFormContent(EditUserDialog);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Unloaded();
        }
    }
}
