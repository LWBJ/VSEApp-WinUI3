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
    public class VSEExperienceDeleteFormViewModel: ObservableRecipient
    {
        public VSEExperienceDeleteFormViewModel(IAppState appState)
        {
            this.AppState = appState;

            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperienceDelete += VSEControl_VSEExperienceDelete;

            this.DeleteExperienceCommand = new AsyncRelayCommand(this.DeleteExperienceCommand_Execute, this.DeleteExperienceCommand_CanExecute);
        }

        public IAppState AppState;

        //--------------------------------------------------------------Notification------------------------------------------------
        private void VSEControl_VSEExperienceDelete(object sender, FormResponse_EventHandlerArg e)
        {
            string notification = e.StatusMessage;
            if (notification == "OK")
            {
                this.StatusMessage = "Experience Deleted";
            }
            else
            {
                this.StatusMessage = "An Error Occurred";
            }

            this.IsStatusMessageOpen = true;
            this.IsLoading = false;
        }

        //----------------------------------------------------------------Messages-----------------------------------------------------
        protected override void OnActivated()
        {
            Messenger.Register<VSEExperienceDeleteFormViewModel, OpenModal_VSEExperience, string>(this, "Delete", (r, m) => Receive(m));
        }
        private void Receive(OpenModal_VSEExperience message)
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

        private VSEExperience deletionTarget;
        private bool isLoading = false;
        private string statusMessage = "";
        private bool isStatusMessageOpen = false;
        private bool isValid = true;

        public VSEExperience DeletionTarget
        {
            get => deletionTarget;
            set
            {
                SetProperty(ref deletionTarget, value);
                OnPropertyChanged(nameof(IsValid));
                this.DeleteExperienceCommand.NotifyCanExecuteChanged();
            }
        }
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                SetProperty(ref isLoading, value);
                OnPropertyChanged(nameof(IsValid));
                this.DeleteExperienceCommand.NotifyCanExecuteChanged();
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
                this.DeleteExperienceCommand.NotifyCanExecuteChanged();
            }
        }

        //---------------------------------------------------------Actions & Commands-------------------------------------------------------
        public void Unloaded()
        {
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperienceDelete -= VSEControl_VSEExperienceDelete;

            this.IsActive = false;
            Messenger.Unregister<OpenModal_VSEExperience, string>(this, "Delete");
        }

        public AsyncRelayCommand DeleteExperienceCommand { get; }
        private async Task DeleteExperienceCommand_Execute()
        {
            this.IsLoading = true;
            this.IsValid = false;
            await this.AppState.OverallAppModel.LoginControl.VSEControl.DeleteVSEDataAndNotify(VSEType.Experience, this.DeletionTarget.Url);
        }
        private bool DeleteExperienceCommand_CanExecute()
        {
            return this.IsValid && !this.IsLoading && (this.DeletionTarget != null) && !string.IsNullOrEmpty(this.DeletionTarget.Url);
        }
    }
}
