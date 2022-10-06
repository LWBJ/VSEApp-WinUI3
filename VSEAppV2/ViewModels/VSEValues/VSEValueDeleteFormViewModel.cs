using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSEAppV2.Messages;
using VSEAppV2.Models;
using VSEAppV2.Services;

namespace VSEAppV2.ViewModels
{
    public class VSEValueDeleteFormViewModel: ObservableRecipient
    {
        public VSEValueDeleteFormViewModel(IAppState appState)
        {
            this.AppState = appState;

            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEValueDelete += VSEControl_VSEValueDelete;

            this.DeleteValueCommand = new AsyncRelayCommand(this.DeleteValueCommand_Execute, this.DeleteValueCommand_CanExecute);
        }
        public IAppState AppState;

        //--------------------------------------------------------------Notification------------------------------------------------
        private void VSEControl_VSEValueDelete(object sender, FormResponse_EventHandlerArg e)
        {
            string notification = e.StatusMessage;
            if (notification == "OK")
            {
                this.StatusMessage = "Value Deleted";
            } else
            {
                this.StatusMessage = "An Error Occurred";
            }

            this.IsStatusMessageOpen = true;
            this.IsLoading = false;
        }

        //----------------------------------------------------------------Messages-----------------------------------------------------
        protected override void OnActivated()
        {
            Messenger.Register<VSEValueDeleteFormViewModel, OpenModal_VSEValue, string>(this, "Delete", (r, m) => Receive(m));
        }
        private void Receive(OpenModal_VSEValue message)
        {
            //Render fresh form and open dialog
            this.IsValid = true;
            this.DeletionTarget = message.Value;
            this.StatusMessage = "";
            this.IsStatusMessageOpen = false;

            if (this.DeleteDialog != null)
            {
                this.DeleteDialog.ShowAsync();
            }
        }

        //---------------------------------------------------------Exposed form properties--------------------------------------------------
        public ContentDialog DeleteDialog { get; set; }
        
        private VSEValue deletionTarget;
        private bool isLoading = false;
        private string statusMessage = "";
        private bool isStatusMessageOpen = false;
        private bool isValid = true;

        public VSEValue DeletionTarget
        {
            get => deletionTarget;
            set
            {
                SetProperty(ref deletionTarget, value);
                OnPropertyChanged(nameof(IsValid));
                this.DeleteValueCommand.NotifyCanExecuteChanged();
            }
        }
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                SetProperty(ref isLoading, value);
                OnPropertyChanged(nameof(IsValid));
                this.DeleteValueCommand.NotifyCanExecuteChanged();
            }
        }
        public string StatusMessage
        {
            get => statusMessage;
            set => SetProperty(ref statusMessage, value);
        }
        public bool IsStatusMessageOpen
        {
            get => isStatusMessageOpen;
            set => SetProperty(ref isStatusMessageOpen, value);
        }
        public bool IsValid
        {
            get => isValid && !this.IsLoading && (this.DeletionTarget != null) && !string.IsNullOrEmpty(this.DeletionTarget.Url);
            set
            {
                SetProperty(ref isValid, value);
                this.DeleteValueCommand.NotifyCanExecuteChanged();
            }
        }
        //---------------------------------------------------------Actions & Commands-------------------------------------------------------
        public void Unloaded()
        {
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEValueDelete -= VSEControl_VSEValueDelete;

            this.IsActive = false;
            Messenger.Unregister<OpenModal_VSEValue, string>(this, "Delete");
        }

        public AsyncRelayCommand DeleteValueCommand { get; }
        private async Task DeleteValueCommand_Execute()
        {
            this.IsLoading = true;
            this.IsValid = false;
            await this.AppState.OverallAppModel.LoginControl.VSEControl.DeleteVSEDataAndNotify(VSEType.Value, this.DeletionTarget.Url);
        }
        private bool DeleteValueCommand_CanExecute()
        {
            return this.IsValid && !this.IsLoading && (this.DeletionTarget != null) && !string.IsNullOrEmpty(this.DeletionTarget.Url);
        }
    }
}
