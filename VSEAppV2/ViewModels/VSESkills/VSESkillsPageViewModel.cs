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
    public class VSESkillsPageViewModel: ObservableRecipient
    {
        public VSESkillsPageViewModel(IAppState appState)
        {
            this.AppState = appState;
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSESkillsSet += VSEControl_VSESkillsSet;

            this.OpenModalCommand = new RelayCommand<string>(this.OpenModal_Execute);
        }
        IAppState AppState { get; }

        //--------------------------------------------------------Notifications------------------------------------------------
        private void VSEControl_VSESkillsSet(object sender, EventArgs e)
        {
            //Get original selection
            VSESkill originalSelection = this.CurrentSelection;

            //Update main list property
            OnPropertyChanged(nameof(VSESkills_Filtered));

            //Check if current user is not null & remains in filtered list, then set it back
            if (originalSelection != null)
            {
                foreach (var item in VSESkills_Filtered)
                {
                    if (item.Url == originalSelection.Url)
                    {
                        this.CurrentSelection = item;
                    }
                }
            }
        }

        //--------------------------------------------------Exposed Properties----------------------------------------------------
        public ObservableCollection<VSESkill> VSESkills_Filtered
        {
            get
            {
                if (string.IsNullOrEmpty(this.Filter))
                {
                    return new ObservableCollection<VSESkill>(this.AppState.OverallAppModel.LoginControl.VSEControl.VSESkillList);
                }
                else
                {
                    var filteredResults = from item in this.AppState.OverallAppModel.LoginControl.VSEControl.VSESkillList
                                          where item.Name.Contains(this.Filter, StringComparison.InvariantCultureIgnoreCase)
                                          select item;
                    return new ObservableCollection<VSESkill>(filteredResults);
                }
            }
        }

        private VSESkill currentSelection;
        public VSESkill CurrentSelection
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
                VSESkill originalSelection = this.CurrentSelection;

                SetProperty(ref filter, value);
                OnPropertyChanged(nameof(VSESkills_Filtered));

                if (originalSelection != null && this.VSESkills_Filtered.Contains(originalSelection))
                {
                    this.CurrentSelection = originalSelection;
                }
            }
        }

        public bool HasSelection
        {
            get => currentSelection != null;
        }
        public Visibility ContentControlVisibility
        {
            get => (CurrentSelection == null) ? Visibility.Collapsed : Visibility.Visible;
        }

        //---------------------------------------------------Commands & Actions---------------------------------------------------------------
        public void Unloaded()
        {
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSESkillsSet -= VSEControl_VSESkillsSet;
        }

        public ICommand OpenModalCommand { get; }
        private void OpenModal_Execute(string parameter)
        {
            switch (parameter)
            {
                case "Create":
                    Messenger.Send(new OpenModal_VSESkill(null), "CreateUpdate");
                    break;
                case "CreateMultiple":
                    Messenger.Send(new OpenModal_VSESkill(null), parameter);
                    break;
                case "Update":
                    if (this.CurrentSelection != null)
                    {
                        Messenger.Send(new OpenModal_VSESkill(this.CurrentSelection), "CreateUpdate");
                    }
                    break;
                case "Delete":
                    if (this.CurrentSelection != null)
                    {
                        Messenger.Send(new OpenModal_VSESkill(this.CurrentSelection), parameter);
                    }
                    break;
            }
        }
    }
}
