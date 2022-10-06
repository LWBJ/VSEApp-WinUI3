using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSEAppV2.Models;
using VSEAppV2.Services;
using VSEAppV2.Views;

namespace VSEAppV2.ViewModels
{
    public class LoggedInViewModel: ObservableRecipient
    {
        public LoggedInViewModel(IAppState appState)
        {
            this.AppState = appState;
            this.AppState.OverallAppModel.PropertyChanged += OverallAppModel_PropertyChanged;
            this.AppState.OverallAppModel.LoginControl.PropertyChanged += LoginControl_PropertyChanged;
            this.AppState.OverallAppModel.LoginControl.SessionExpired += LoginControl_SessionExpired;

            this.RefreshCommand = new AsyncRelayCommand(RefreshData);
            this.LogoutCommand = new RelayCommand(Logout);

            //Startup code
            this.RefreshData();
        }
        public IAppState AppState { get; }
        
        //----------------------------------------------------Notifications-------------------------------------------------------
        private void LoginControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentUser")
            {
                OnPropertyChanged(nameof(CurrentUser));
            }
        }
        private void OverallAppModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsLoading")
            {
                OnPropertyChanged(nameof(IsLoading));
            }
        }
        private void LoginControl_SessionExpired(object sender, EventArgs e)
        {
            this.IsStatusMessageOpen = true;
        }

        //----------------------------------------------Exposed Properties-----------------------------
        public bool IsLoading
        {
            get => this.AppState.OverallAppModel.IsLoading;
            set
            {
                this.AppState.OverallAppModel.IsLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
        public VSEUser CurrentUser
        {
            get => this.AppState.OverallAppModel.LoginControl.CurrentUser;
            set
            {
                this.AppState.OverallAppModel.LoginControl.CurrentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        private bool isStatusMessageOpen;
        public bool IsStatusMessageOpen
        {
            get => isStatusMessageOpen;
            set => SetProperty(ref isStatusMessageOpen, value);
        }
        //----------------------------------------------Commands---------------------------------------
        public RelayCommand LogoutCommand { get; }
        private void Logout()
        {
            this.AppState.OverallAppModel.LoginControl.Logout();
        }

        public AsyncRelayCommand RefreshCommand { get; }
        private async Task RefreshData()
        {
            this.AppState.OverallAppModel.IsLoading = true;
            await this.AppState.OverallAppModel.LoginControl.VSEControl.SetVSEDataListAndNotify(VSEType.Value);
            await this.AppState.OverallAppModel.LoginControl.VSEControl.SetVSEDataListAndNotify(VSEType.Skill);
            await this.AppState.OverallAppModel.LoginControl.VSEControl.SetVSEDataListAndNotify(VSEType.Experience);
            this.AppState.OverallAppModel.IsLoading = false;
        }

        public void Unloaded()
        {
            this.AppState.OverallAppModel.PropertyChanged -= OverallAppModel_PropertyChanged;
            this.AppState.OverallAppModel.LoginControl.PropertyChanged -= LoginControl_PropertyChanged;
            this.AppState.OverallAppModel.LoginControl.SessionExpired -= LoginControl_SessionExpired;
        }
    }
}
