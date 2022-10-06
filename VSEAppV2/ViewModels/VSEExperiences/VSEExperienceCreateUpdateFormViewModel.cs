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
    public class VSEExperienceCreateUpdateFormViewModel: ObservableRecipient
    {
        public VSEExperienceCreateUpdateFormViewModel(IAppState appState)
        {
            this.AppState = appState;

            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEValuesSet += VSEControl_VSEValuesSet;
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSESkillsSet += VSEControl_VSESkillsSet;
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperienceCreate += VSEControl_VSEExperienceCreate;
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperienceUpdate += VSEControl_VSEExperienceUpdate; 

            this.CreateOrUpdateCommand = new AsyncRelayCommand(CreateOrUpdateCommand_Execute, CreateOrUpdateCommand_CanExecute);
            this.ClearFormCommand = new RelayCommand(ClearFormCommand_Execute);
        }

        public IAppState AppState;

        //---------------------------------------------------------Notifications----------------------------------------------
        private void VSEControl_VSEExperienceUpdate(object sender, FormResponse_EventHandlerArg e)
        {
            this.VSEControl_VSEExperienceCreateOrUpdate(e, "Update");
        }
        private void VSEControl_VSEExperienceCreate(object sender, FormResponse_EventHandlerArg e)
        {
            this.VSEControl_VSEExperienceCreateOrUpdate(e, "Create");
        }
        private void VSEControl_VSEExperienceCreateOrUpdate(FormResponse_EventHandlerArg e, string requestType)
        {
            string notification = e.StatusMessage;
            if (notification == "OK")
            {
                this.StatusMessage = "Experience " + (requestType == "Create" ? "Created" : "Updated");
            }
            else
            {
                this.StatusMessage = "An error occurred";
            }

            this.IsStatusMessageOpen = true;
            this.IsLoading = false;
        }

        private void VSEControl_VSESkillsSet(object sender, EventArgs e)
        {
            //Copy original list of URLs only
            var originalUrlResults = from skill in this.SelectedVSESkills
                                    select skill.Url;
            List<string> originalUrls = new List<string>(originalUrlResults);

            //Make changes
            OnPropertyChanged(nameof(DisplayedVSESkills));

            //Update selected items list on view model
            this.SelectedVSESkills.Clear();
            foreach (var displayItem in this.DisplayedVSESkills)
            {
                if (originalUrls.Contains(displayItem.Url))
                {
                    this.SelectedVSESkills.Add(displayItem);
                }
            }

            //Notify view
            this.InitialFormSet?.Invoke(this, EventArgs.Empty);
        }
        private void VSEControl_VSEValuesSet(object sender, EventArgs e)
        {
            //Copy original list of URLs only
            var originalUrlResults = from item in this.SelectedVSEValues
                                           select item.Url;
            List<string> originalUrls = new List<string>(originalUrlResults);

            //Make changes
            OnPropertyChanged(nameof(DisplayedVSEValues));

            //Update selected items list on view model
            this.SelectedVSEValues.Clear();
            foreach (var displayItem in this.DisplayedVSEValues)
            {
                if (originalUrls.Contains(displayItem.Url))
                {
                    this.SelectedVSEValues.Add(displayItem);
                }
            }

            //Notify view
            this.InitialFormSet?.Invoke(this, EventArgs.Empty);
        }

        //---------------------------------------------------------Messaging--------------------------------------------------
        protected override void OnActivated()
        {
            Messenger.Register<VSEExperienceCreateUpdateFormViewModel, OpenModal_VSEExperience, string>(this, "CreateUpdate", (r, m) => this.Receive(m));
        }

        private void Receive(OpenModal_VSEExperience message)
        {
            //Reset to a fresh form
            OnPropertyChanged(nameof(DisplayedVSEValues));
            OnPropertyChanged(nameof(DisplayedVSESkills));
            this.IsStatusMessageOpen = false;
            this.StatusMessage = "";
            this.ErrorMessage = "";
            this.IsValid = false;

            if (message.Value == null)
            {
                //It is a Create dialog, set up empty form values & create specific data
                this.DialogTitle = "Create Experience";
                this.UpdateTarget = null;
                this.VSEExperienceName = "";

                this.SelectedVSEValues.Clear();
                this.SelectedVSESkills.Clear();
            }
            else
            {
                //It is an Update dialog, set up initial form from message.Value & update specific data
                this.DialogTitle = "Update Experience";
                this.UpdateTarget = message.Value;
                this.VSEExperienceName = message.Value.Name;

                foreach (VSEValue vseValue in this.DisplayedVSEValues)
                {
                    if (message.Value.ValueURLList.Contains(vseValue.Url))
                    {
                        this.SelectedVSEValues.Add(vseValue);
                    }
                }
                foreach (VSESkill vseSkill in this.DisplayedVSESkills)
                {
                    if (message.Value.SkillURLList.Contains(vseSkill.Url))
                    {
                        this.SelectedVSESkills.Add(vseSkill);
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

        private VSEExperience updateTarget;
        public VSEExperience UpdateTarget
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

        private string vseExperienceName = "";

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

        public string VSEExperienceName
        {
            get => vseExperienceName;
            set
            {
                SetProperty(ref vseExperienceName, value);
                this.CheckIsValid();
            }
        }
        public ObservableCollection<VSEValue> DisplayedVSEValues
        {
            get => new ObservableCollection<VSEValue>(this.AppState.OverallAppModel.LoginControl.VSEControl.VSEValueList);
        }
        public List<VSEValue> SelectedVSEValues { get; } = new List<VSEValue>();
        public ObservableCollection<VSESkill> DisplayedVSESkills
        {
            get => new ObservableCollection<VSESkill>(this.AppState.OverallAppModel.LoginControl.VSEControl.VSESkillList);
        }
        public List<VSESkill> SelectedVSESkills { get; } = new List<VSESkill>();

        private void CheckIsValid()
        {
            if (this.vseExperienceName.Length <= 0)
            {
                this.IsValid = false;
                this.ErrorMessage = "Experience name cannot be blank";
            }
            else
            {
                this.IsValid = true;
                this.ErrorMessage = null;
            }
        }
        private HttpContent GenerateHttpContent()
        {
            Experience_CreateUpdate_FormValue newValue = new Experience_CreateUpdate_FormValue();
            newValue.name = this.VSEExperienceName;
            newValue.value_set = new List<string>();
            newValue.skill_set = new List<string>();

            foreach (VSEValue vseValue in SelectedVSEValues)
            {
                newValue.value_set.Add(vseValue.Url);
            }
            foreach (VSESkill vseSkill in SelectedVSESkills)
            {
                newValue.skill_set.Add(vseSkill.Url);
            }

            string jsonString = JsonSerializer.Serialize(newValue);
            return new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
        }

        //---------------------------------------------------------Actions & Commands-------------------------------------------------------
        public void Unloaded()
        {
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEValuesSet -= VSEControl_VSEValuesSet;
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSESkillsSet -= VSEControl_VSESkillsSet;
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperienceCreate -= VSEControl_VSEExperienceCreate;
            this.AppState.OverallAppModel.LoginControl.VSEControl.VSEExperienceUpdate -= VSEControl_VSEExperienceUpdate;

            this.IsActive = false;
            Messenger.Unregister<OpenModal_VSEExperience, string>(this, "CreateUpdate");
        }

        public AsyncRelayCommand CreateOrUpdateCommand { get; }
        private async Task CreateOrUpdateCommand_Execute()
        {
            this.IsLoading = true;
            HttpContent formContent = this.GenerateHttpContent();

            if (this.UpdateTarget == null || string.IsNullOrEmpty(this.UpdateTarget.Url))
            {
                //Create
                await this.AppState.OverallAppModel.LoginControl.VSEControl.CreateVSEDataAndNotify(VSEType.Experience, formContent);
            }
            else
            {
                //Update
                await this.AppState.OverallAppModel.LoginControl.VSEControl.UpdateVSEDataAndNotify(VSEType.Experience, formContent, this.UpdateTarget.Url);
            }
        }
        private bool CreateOrUpdateCommand_CanExecute()
        {
            return this.IsValid && !this.IsLoading;
        }

        public RelayCommand ClearFormCommand { get; }
        private void ClearFormCommand_Execute()
        {
            this.VSEExperienceName = "";
            this.SelectedVSEValues.Clear();
            this.SelectedVSESkills.Clear();
            this.InitialFormSet?.Invoke(this, EventArgs.Empty);
        }

        //----------------------------------------------------------------Additional Events------------------------------------
        public event EventHandler InitialFormSet;
    }

    public class Experience_CreateUpdate_FormValue
    {
        public string name { get; set; }
        public List<string> value_set { get; set; }
        public List<string> skill_set { get; set; }
    }
}
