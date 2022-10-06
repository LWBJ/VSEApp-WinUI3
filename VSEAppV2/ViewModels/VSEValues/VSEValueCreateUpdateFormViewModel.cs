using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class VSEValueCreateUpdateFormViewModel: ObservableRecipient
    {
        public VSEValueCreateUpdateFormViewModel(IAppState appState)
        {
            this.AppState = appState;

            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperiencesSet += VSEControl_VSEExperiencesSet;
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEValueUpdate += VSEControl_VSEValueUpdate;
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEValueCreate += VSEControl_VSEValueCreate;

            this.CreateOrUpdateCommand = new AsyncRelayCommand(CreateOrUpdateCommand_Execute, CreateOrUpdateCommand_CanExecute);
            this.ClearFormCommand = new RelayCommand(ClearFormCommand_Execute);
        }
        public IAppState AppState;

        //---------------------------------------------------------Notifications----------------------------------------------
        private void VSEControl_VSEExperiencesSet(object sender, EventArgs e)
        {
            //Copy original list of URLs only
            var originalUrlResults = from exp in this.SelectedVSEExperiences
                                     select exp.Url;
            List<string> originallUrls = new List<string>(originalUrlResults);

            //Make changes
            OnPropertyChanged(nameof(DisplayedVSEExperiences));

            //Update selected items list on view model
            this.SelectedVSEExperiences.Clear();
            foreach (var displayItem in this.DisplayedVSEExperiences)
            {
                if (originallUrls.Contains(displayItem.Url))
                {
                    this.SelectedVSEExperiences.Add(displayItem);
                }
            }

            //Notify view
            this.InitialFormSet?.Invoke(this, EventArgs.Empty);
        }

        private void VSEControl_VSEValueCreateOrUpdate(FormResponse_EventHandlerArg e, string requestType)
        {
            string notification = e.StatusMessage;
            if (notification == "OK")
            {
                this.StatusMessage = "Value " + (requestType == "Create"? "Created" : "Updated");
            }
            else
            {
                this.StatusMessage = "An error occurred";
            }

            this.IsStatusMessageOpen = true;
            this.IsLoading = false;
        }

        private void VSEControl_VSEValueCreate(object sender, FormResponse_EventHandlerArg e)
        {
            this.VSEControl_VSEValueCreateOrUpdate(e, "Create");
        }
        private void VSEControl_VSEValueUpdate(object sender, FormResponse_EventHandlerArg e)
        {
            this.VSEControl_VSEValueCreateOrUpdate(e, "Update");
        }

        //---------------------------------------------------------Messaging--------------------------------------------------
        protected override void OnActivated()
        {
            Messenger.Register<VSEValueCreateUpdateFormViewModel, OpenModal_VSEValue, string>(this, "CreateUpdate", (r, m) => this.Receive(m));
        }

        private void Receive(OpenModal_VSEValue message)
        {
            //Reset to a fresh form
            OnPropertyChanged(nameof(DisplayedVSEExperiences));
            this.IsStatusMessageOpen = false;
            this.StatusMessage = "";
            this.ErrorMessage = "";
            this.IsValid = false;

            if (message.Value == null)
            {
                //It is a Create dialog, set up empty form values & create specific data
                this.DialogTitle = "Create Value";
                this.UpdateTarget = null;
                this.VSEValueName = "";
                this.SelectedVSEExperiences.Clear();
            }
            else
            {
                //It is an Update dialog, set up initial form from message.Value & update specific data
                this.DialogTitle = "Update Value";
                this.UpdateTarget = message.Value;
                this.VSEValueName = message.Value.Name;

                foreach (VSEExperience exp in this.DisplayedVSEExperiences)
                {
                    if (message.Value.ExperienceURLList.Contains(exp.Url))
                    {
                        this.SelectedVSEExperiences.Add(exp);
                    }
                }
            }

            //Open modal anyway
            if (this.CreateUpdateDialog != null)
            {
                this.CreateUpdateDialog.ShowAsync();
            }
            this.InitialFormSet?.Invoke(this, EventArgs.Empty);
        }

        //---------------------------------------------------------Exposed Properties--------------------------------------------------
        public ContentDialog CreateUpdateDialog { get; set; }

        private VSEValue updateTarget;
        public VSEValue UpdateTarget
        {
            get => updateTarget;
            set => SetProperty(ref updateTarget, value);
        }

        private string dialogTitle;
        public string DialogTitle
        {
            get => dialogTitle;
            set => SetProperty(ref dialogTitle, value);
        }

        //---------------------------------------------------------Form fields & validation-----------------------------------------------
        private bool isLoading = false;
        private string statusMessage = "";
        private bool isStausMessageOpen = false;
        private string errorMessage = "";
        private bool isValid = false;
        
        private string vseValueName = "";

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                SetProperty(ref isLoading, value);
                OnPropertyChanged(nameof(IsValid));
                this.CreateOrUpdateCommand.NotifyCanExecuteChanged();
            }
        }
        public string StatusMessage
        {
            get => statusMessage;
            set => SetProperty(ref statusMessage, value);
        }
        public bool IsStatusMessageOpen
        {
            get => isStausMessageOpen;
            set => SetProperty(ref isStausMessageOpen, value);
        }
        public string ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }
        public bool IsValid
        {
            get => isValid && !IsLoading;
            set 
            {
                SetProperty(ref isValid, value);
                this.CreateOrUpdateCommand.NotifyCanExecuteChanged();
            }
        }
        
        public string VSEValueName
        {
            get => vseValueName;
            set
            {
                SetProperty(ref vseValueName, value);
                this.CheckIsValid();
            }
        }
        public ObservableCollection<VSEExperience> DisplayedVSEExperiences
        {
            get => new ObservableCollection<VSEExperience>(this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperienceList);
        }
        public ObservableCollection<VSEExperience> SelectedVSEExperiences { get; } = new ObservableCollection<VSEExperience>();
        
        private void CheckIsValid()
        {
            if (this.VSEValueName.Length <= 0)
            {
                this.IsValid = false;
                this.ErrorMessage = "Value name cannot be blank";
            } else
            {
                this.IsValid = true;
                this.ErrorMessage = null;
            }
        }
        private HttpContent GenerateHttpContent()
        {
            VSEValue_CreateUpdate_FormValue newValue = new VSEValue_CreateUpdate_FormValue();
            newValue.name = this.VSEValueName;
            newValue.experiences = new List<string>();
            foreach (VSEExperience exp in SelectedVSEExperiences)
            {
                newValue.experiences.Add(exp.Url);
            }

            string jsonString = JsonSerializer.Serialize(newValue);
            return new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
        }
        //---------------------------------------------------------Actions & Commands-------------------------------------------------------
        public void Unloaded()
        {
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperiencesSet -= VSEControl_VSEExperiencesSet;
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEValueUpdate -= VSEControl_VSEValueUpdate;
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEValueCreate -= VSEControl_VSEValueCreate;

            this.IsActive = false;
            Messenger.Unregister<OpenModal_VSEValue, string>(this, "CreateUpdate");
        }

        public AsyncRelayCommand CreateOrUpdateCommand { get; }
        private async Task CreateOrUpdateCommand_Execute()
        {
            this.IsLoading = true;
            HttpContent formContent = this.GenerateHttpContent();

            if (this.UpdateTarget == null || string.IsNullOrEmpty(this.UpdateTarget.Url))
            {
                //Create
                await this.AppState.OverallAppModel.LoginControl.VSEControl.CreateVSEDataAndNotify(VSEType.Value, formContent);
            }
            else
            {
                //Update
                await this.AppState.OverallAppModel.LoginControl.VSEControl.UpdateVSEDataAndNotify(VSEType.Value, formContent, this.UpdateTarget.Url);
            }
        }
        private bool CreateOrUpdateCommand_CanExecute()
        {
            return this.IsValid && !this.IsLoading;
        }

        public RelayCommand ClearFormCommand { get; }
        private void ClearFormCommand_Execute()
        {
            this.VSEValueName = "";
            this.SelectedVSEExperiences.Clear();
            this.InitialFormSet?.Invoke(this, EventArgs.Empty);
        }
        //----------------------------------------------------------------Additional Events------------------------------------
        public event EventHandler InitialFormSet;
    }

    public class VSEValue_CreateUpdate_FormValue
    {
        public string name { get; set; }
        public List<string> experiences { get; set; }
    }
}
