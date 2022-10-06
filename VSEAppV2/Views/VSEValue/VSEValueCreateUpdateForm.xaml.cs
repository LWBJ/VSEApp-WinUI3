using Microsoft.Toolkit.Mvvm.DependencyInjection;
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
using VSEAppV2.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VSEAppV2.Views
{
    public sealed partial class VSEValueCreateUpdateForm : UserControl
    {
        public VSEValueCreateUpdateForm()
        {
            this.InitializeComponent();
            this.DataContext = Ioc.Default.GetService<VSEValueCreateUpdateFormViewModel>();

            this.ViewModel.InitialFormSet += ViewModel_InitialFormSet;
        }
        public VSEValueCreateUpdateFormViewModel ViewModel => (VSEValueCreateUpdateFormViewModel)this.DataContext;

        private void createUpdateDialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CreateUpdateDialog = createUpdateDialog;
            this.ViewModel.IsActive = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Unloaded();
        }

        //Handling list view selected items binding
        private void ViewModel_InitialFormSet(object sender, EventArgs e)
        {
            for (int i= experienceList.SelectedItems.Count-1; i>=0; i--)
            {
                if (!this.ViewModel.SelectedVSEExperiences.Contains(experienceList.SelectedItems[i]))
                {
                    experienceList.SelectedItems.RemoveAt(i);
                }
            }

            foreach (VSEExperience exp in this.ViewModel.SelectedVSEExperiences)
            {
                if (!experienceList.SelectedItems.Contains(exp))
                {
                    experienceList.SelectedItems.Add(exp);
                }
            }
        }
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var addedItem in e.AddedItems)
            {
                if (!this.ViewModel.SelectedVSEExperiences.Contains(addedItem as VSEExperience))
                {
                    this.ViewModel.SelectedVSEExperiences.Add(addedItem as VSEExperience);
                }
            }

            foreach (var removedItem in e.RemovedItems)
            {
                if (this.ViewModel.SelectedVSEExperiences.Contains(removedItem as VSEExperience))
                {
                    this.ViewModel.SelectedVSEExperiences.Remove(removedItem as VSEExperience);
                }
            }
        }
    }
}
