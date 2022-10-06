using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VSEAppV2.Services;

namespace VSEAppV2.ViewModels
{
    public class SignUpFormViewModel: ObservableRecipient
    {
        public SignUpFormViewModel(IAppState appState)
        {
            this.AppState = appState;
            this.AppState.OverallAppModel.LoginControl.SignupFormResponse += LoginControl_SignupFormResponse;

            this.SignupCommand = new AsyncRelayCommand(this.Signup, this.CanSignup);
        }
        
        public IAppState AppState { get; }
        
        //---------------------------------------------Notifications--------------------------------------------
        private void LoginControl_SignupFormResponse(object sender, Models.FormResponse_EventHandlerArg e)
        {
            string notification = e.StatusMessage;
            if (notification == "OK")
            {
                //Update status and clear form
                this.StatusMessage = "Account created, please login";
                this.Username = "";
                this.Password = "";
                this.Password2 = "";

                this.errorDictionary.Clear();
                OnPropertyChanged(nameof(ErrorMessageString));
                this.IsValid = false;
            }
            else if (notification.Contains("This field must be unique"))
            {
                this.StatusMessage = "Username already taken";
            }
            else if (notification.Contains("This password is too common"))
            {
                this.StatusMessage = "This password is too common";
            }
            else
            {
                this.StatusMessage = "An error occurred";
            }

            this.IsStatusMessageOpen = true;
            this.IsLoading = false;
        }

        //----------------------------------------------Form Properties & Validation-----------------------------
        private bool isLoading = false;
        private string statusMessage = "";
        private bool isStatusMessageOpen = false;

        private string username = "";
        private string password = "";
        private string password2 = "";
        private bool isValid = false;
        private Dictionary<string, string> errorDictionary = new Dictionary<string, string>();

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                SetProperty(ref isLoading, value);
                OnPropertyChanged(nameof(IsValid));
                this.SignupCommand.NotifyCanExecuteChanged();
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
        
        public string Username
        {
            get => username;
            set
            {
                SetProperty(ref username, value);
                this.CheckValidity();
            }
        }
        public string Password
        {
            get => password;
            set
            {
                SetProperty(ref password, value);
                this.CheckValidity();
            }
        }
        public string Password2
        {
            get => password2;
            set
            {
                SetProperty(ref password2, value);
                this.CheckValidity();
            }
        }
        public bool IsValid
        {
            get => isValid && !isLoading;
            set
            {
                SetProperty(ref isValid, value);
                this.SignupCommand.NotifyCanExecuteChanged();
            }
        }
        public string ErrorMessageString
        {
            get
            {
                string message = "";
                foreach (string error in errorDictionary.Values.ToList())
                {
                    message += (string.IsNullOrEmpty(error) ? "" : error + "\n");
                }
                return message;
            }
        }

        private void CheckValidity()
        {   
            if (this.Username.Length <= 0)
            {
                errorDictionary["username"] = "Username cannot be blank";
            } else
            {
                errorDictionary.Remove("username");
            }
            
            if (this.Password.Length < 8)
            {
                errorDictionary["password1"] = "Password needs to be at least 8 characters";
            } else
            {
                errorDictionary.Remove("password1");
            }

            if (this.Password != this.Password2)
            {
                errorDictionary["matchingPassword"] = "Passwords do not match";
            } else
            {
                errorDictionary.Remove("matchingPassword");
            }

            this.IsValid = this.errorDictionary.Values.Count == 0;
            OnPropertyChanged(nameof(ErrorMessageString));
        }
        private HttpContent generateHttpContent()
        {
            var values = new Dictionary<string, string>
            {
                { "username", this.Username },
                { "password", this.Password },
                { "password2", this.Password2 }
            };
            return new FormUrlEncodedContent(values);
        }

        //----------------------------------------------Command---------------------------------
        public AsyncRelayCommand SignupCommand { get; }
        private async Task Signup()
        {
            this.IsLoading = true;
            
            HttpContent formContent = this.generateHttpContent();
            await this.AppState.OverallAppModel.LoginControl.PostSignupRequestAndNotify(formContent);
        }
        private bool CanSignup()
        {
            return (this.IsValid && !this.IsLoading);
        }

        public void Unloaded()
        {
            this.AppState.OverallAppModel.LoginControl.SignupFormResponse -= LoginControl_SignupFormResponse;
        }
    }
}
