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
using VSEAppV2.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VSEAppV2.Views
{
    public sealed partial class VSEExperienceCreateUpdateForm : UserControl
    {
        public VSEExperienceCreateUpdateForm()
        {
            this.InitializeComponent();
            this.DataContext = Ioc.Default.GetService<VSEExperienceCreateUpdateFormViewModel>();

            this.ViewModel.InitialFormSet += ViewModel_InitialFormSet;
        }
        public VSEExperienceCreateUpdateFormViewModel ViewModel => (VSEExperienceCreateUpdateFormViewModel)this.DataContext;

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
            for (int i = valueList.SelectedItems.Count - 1; i >= 0; i--)
            {
                if (!this.ViewModel.SelectedVSEValues.Contains(valueList.SelectedItems[i]))
                {
                    valueList.SelectedItems.RemoveAt(i);
                }
            }
            foreach (var vseValue in this.ViewModel.SelectedVSEValues)
            {
                if (!valueList.SelectedItems.Contains(vseValue))
                {
                    valueList.SelectedItems.Add(vseValue);
                }
            }

            for (int i = skillList.SelectedItems.Count - 1; i >= 0; i--)
            {
                if (!this.ViewModel.SelectedVSESkills.Contains(skillList.SelectedItems[i]))
                {
                    skillList.SelectedItems.RemoveAt(i);
                }
            }
            foreach (var vseSkill in this.ViewModel.SelectedVSESkills)
            {
                if (!skillList.SelectedItems.Contains(vseSkill))
                {
                    skillList.SelectedItems.Add(vseSkill);
                }
            }
        }

        private void valueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var addedItem in e.AddedItems)
            {
                if (!this.ViewModel.SelectedVSEValues.Contains(addedItem as VSEValue))
                {
                    this.ViewModel.SelectedVSEValues.Add(addedItem as VSEValue);
                }
            }

            foreach (var removedItem in e.RemovedItems)
            {
                if (this.ViewModel.SelectedVSEValues.Contains(removedItem as VSEValue))
                {
                    this.ViewModel.SelectedVSEValues.Remove(removedItem as VSEValue);
                }
            }
        }

        private void skillList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var addedItem in e.AddedItems)
            {
                if (!this.ViewModel.SelectedVSESkills.Contains(addedItem as VSESkill))
                {
                    this.ViewModel.SelectedVSESkills.Add(addedItem as VSESkill);
                }
            }

            foreach (var removedItem in e.RemovedItems)
            {
                if (this.ViewModel.SelectedVSESkills.Contains(removedItem as VSESkill))
                {
                    this.ViewModel.SelectedVSESkills.Remove(removedItem as VSESkill);
                }
            }
        }
    }
}
