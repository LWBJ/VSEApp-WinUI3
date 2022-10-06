using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VSEAppV2.Messages;
using VSEAppV2.Models;
using VSEAppV2.Services;

namespace VSEAppV2.ViewModels
{
    public class VSEExperiencesPageViewModel: ObservableRecipient
    {
        public VSEExperiencesPageViewModel(IAppState appState)
        {
            this.AppState = appState;
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperiencesSet += VSEControl_VSEExperiencesSet;

            this.OpenModalCommand = new RelayCommand<string>(this.OpenModal_Execute);
        }

        IAppState AppState { get; }

        //--------------------------------------------------------Notifications------------------------------------------------
        private void VSEControl_VSEExperiencesSet(object sender, EventArgs e)
        {
            //Get original selection
            VSEExperience originalSelection = this.CurrentSelection;

            //Update main list property
            OnPropertyChanged(nameof(VSEExperiences_Filtered));

            //Check if current user is not null & remains in filtered list, then set it back
            if (originalSelection != null)
            {
                foreach (var item in VSEExperiences_Filtered)
                {
                    if (item.Url == originalSelection.Url)
                    {
                        this.CurrentSelection = item;
                    }
                }
            }
        }

        //--------------------------------------------------Exposed Properties----------------------------------------------------
        public ObservableCollection<VSEExperience> VSEExperiences_Filtered
        {
            get
            {
                if (string.IsNullOrEmpty(this.Filter))
                {
                    return new ObservableCollection<VSEExperience>(this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperienceList);
                }
                else
                {
                    var filteredResults = from item in this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperienceList
                                          where item.Name.Contains(this.Filter, StringComparison.InvariantCultureIgnoreCase)
                                          select item;
                    return new ObservableCollection<VSEExperience>(filteredResults);
                }
            }
        }

        private VSEExperience currentSelection;
        public VSEExperience CurrentSelection
        {
            get => currentSelection;
            set
            {
                SetProperty(ref currentSelection, value);
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(ContentControlVisibility));
            }
        }

        private string filter = "";
        public string Filter
        {
            get => filter;
            set
            {
                VSEExperience originalSelection = this.CurrentSelection;

                SetProperty(ref filter, value);
                OnPropertyChanged(nameof(VSEExperiences_Filtered));

                if (originalSelection != null && this.VSEExperiences_Filtered.Contains(originalSelection))
                {
                    this.CurrentSelection = originalSelection;
                }
            }
        }

        public bool HasSelection
        {
            get => CurrentSelection != null;
        }
        public Visibility ContentControlVisibility
        {
            get => (CurrentSelection == null) ? Visibility.Collapsed : Visibility.Visible;
        }

        //---------------------------------------------------Commands & Actions---------------------------------------------------------------
        public void Unloaded()
        {
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperiencesSet -= VSEControl_VSEExperiencesSet;
        }

        public ICommand OpenModalCommand { get; }
        private void OpenModal_Execute(string parameter)
        {
            switch (parameter)
            {
                case "Create":
                    Messenger.Send(new OpenModal_VSEExperience(null), "CreateUpdate");
                    break;
                case "CreateMultiple":
                    Messenger.Send(new OpenModal_VSEExperience(null), parameter);
                    break;
                case "Update":
                    if (this.CurrentSelection != null)
                    {
                        Messenger.Send(new OpenModal_VSEExperience(this.CurrentSelection), "CreateUpdate");
                    }
                    break;
                case "Delete":
                    if (this.CurrentSelection != null)
                    {
                        Messenger.Send(new OpenModal_VSEExperience(this.CurrentSelection), parameter);
                    }
                    break;
            }
        }
    }
}
