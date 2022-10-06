using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VSEAppV2.Messages;
using VSEAppV2.Models;
using VSEAppV2.Services;

namespace VSEAppV2.ViewModels
{
    public class VSEExperienceCreateMultipleFormViewModel: ObservableRecipient
    {
        public VSEExperienceCreateMultipleFormViewModel(IAppState appState)
        {
            this.AppState = appState;

            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperienceCreate += VSEControl_VSEExperienceCreate;

            this.CreateMultipleExperiencesCommand = new AsyncRelayCommand(CreateMultipleExperiencesCommand_Execute, CreateMultipleExperiencesCommand_CanExecute);
            this.ClearFormCommand = new RelayCommand(ClearFormCommand_Execute);
        }

        public IAppState AppState;

        //-----------------------------------------------------------Notifications---------------------------------------------------
        private void VSEControl_VSEExperienceCreate(object sender, FormResponse_EventHandlerArg e)
        {
            string notification = e.StatusMessage;
            if (notification == "OK")
            {
                this.StatusMessage = "Experiences Created";
            }
            else
            {
                this.StatusMessage = "An error occurred";
            }

            this.IsStatusMessageOpen = true;
            this.IsLoading = false;
        }

        //-----------------------------------------------------------Messages----------------------------------------------------------
        protected override void OnActivated()
        {
            Messenger.Register<VSEExperienceCreateMultipleFormViewModel, OpenModal_VSEExperience, string>(this, "CreateMultiple", (r, m) => Receive(m));
        }
        private void Receive(OpenModal_VSEExperience message)
        {
            //Render fresh form and open dialog
            this.StatusMessage = "";
            this.IsStatusMessageOpen = false;
            this.Field1 = "";
            this.Field2 = "";
            this.Field3 = "";
            this.Field4 = "";
            this.Field5 = "";
            this.Field6 = "";

            if (this.CreateMultipleDialog != null)
            {
                this.CreateMultipleDialog.ShowAsync();
            }
        }

        //---------------------------------------------------------Exposed form properties--------------------------------------------------
        public ContentDialog CreateMultipleDialog { get; set; }

        private bool isLoading = false;
        private string statusMessage = "";
        private bool isStatusMessageOpen = false;

        private string field1 = "";
        private string field2 = "";
        private string field3 = "";
        private string field4 = "";
        private string field5 = "";
        private string field6 = "";

        public string Field1
        {
            get => field1;
            set => SetProperty(ref field1, value);
        }
        public string Field2
        {
            get => field2;
            set => SetProperty(ref field2, value);
        }
        public string Field3
        {
            get => field3;
            set => SetProperty(ref field3, value);
        }
        public string Field4
        {
            get => field4;
            set => SetProperty(ref field4, value);
        }
        public string Field5
        {
            get => field5;
            set => SetProperty(ref field5, value);
        }
        public string Field6
        {
            get => field6;
            set => SetProperty(ref field6, value);
        }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                SetProperty(ref isLoading, value);
                this.CreateMultipleExperiencesCommand.NotifyCanExecuteChanged();
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

        private HttpContent GenerateHttpContent()
        {
            List<Experience_CreateUpdate_FormValue> list = new List<Experience_CreateUpdate_FormValue>();
            if (!string.IsNullOrEmpty(Field1))
            {
                list.Add(new Experience_CreateUpdate_FormValue
                {
                    name = this.Field1,
                    value_set = new List<string>(),
                    skill_set = new List<string>()
                });
            }
            if (!string.IsNullOrEmpty(Field2))
            {
                list.Add(new Experience_CreateUpdate_FormValue
                {
                    name = this.Field2,
                    value_set = new List<string>(),
                    skill_set = new List<string>()
                });
            }
            if (!string.IsNullOrEmpty(Field3))
            {
                list.Add(new Experience_CreateUpdate_FormValue
                {
                    name = this.Field3,
                    value_set = new List<string>(),
                    skill_set = new List<string>()
                });
            }
            if (!string.IsNullOrEmpty(Field4))
            {
                list.Add(new Experience_CreateUpdate_FormValue
                {
                    name = this.Field4,
                    value_set = new List<string>(),
                    skill_set = new List<string>()
                });
            }
            if (!string.IsNullOrEmpty(Field5))
            {
                list.Add(new Experience_CreateUpdate_FormValue
                {
                    name = this.Field5,
                    value_set = new List<string>(),
                    skill_set = new List<string>()
                });
            }
            if (!string.IsNullOrEmpty(Field6))
            {
                list.Add(new Experience_CreateUpdate_FormValue
                {
                    name = this.Field6,
                    value_set = new List<string>(),
                    skill_set = new List<string>()
                });
            }

            string jsonString = JsonSerializer.Serialize(list);
            return new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
        }

        //---------------------------------------------------------Actions & Commands-------------------------------------------------------
        public void Unloaded()
        {
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperienceCreate -= VSEControl_VSEExperienceCreate;

            this.IsActive = false;
            Messenger.Unregister<OpenModal_VSEExperience, string>(this, "CreateMultiple");
        }

        public AsyncRelayCommand CreateMultipleExperiencesCommand { get; }
        private async Task CreateMultipleExperiencesCommand_Execute()
        {
            this.IsLoading = true;
            HttpContent createMultipleFormContent = this.GenerateHttpContent();
            await this.AppState.OverallAppModel.LoginControl.VSEControl.CreateVSEDataAndNotify(VSEType.Experience, createMultipleFormContent);
        }
        private bool CreateMultipleExperiencesCommand_CanExecute()
        {
            return !this.IsLoading;
        }

        public RelayCommand ClearFormCommand { get; }
        private void ClearFormCommand_Execute()
        {
            this.Field1 = "";
            this.Field2 = "";
            this.Field3 = "";
            this.Field4 = "";
            this.Field5 = "";
            this.Field6 = "";
        }
    }
}
