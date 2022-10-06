using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VSEAppV2.Models;
using VSEAppV2.Services;

namespace VSEAppV2.ViewModels
{
    public class LoginFormViewModel: ObservableRecipient
    {
        public LoginFormViewModel(IAppState appState)
        {
            this.AppState = appState;
            this.AppState.OverallAppModel.LoginControl.LoginFormResponse += LoginControl_LoginFormResponse;

            this.LoginCommand = new AsyncRelayCommand(this.Login, this.CanLogin);
        }

        private void LoginControl_LoginFormResponse(object sender, FormResponse_EventHandlerArg e)
        {
            string notification = e.StatusMessage;
            if (notification == "OK")
            {
                //Clear Form
                this.StatusMessage = "";
                this.Username = "";
                this.Password = "";

                this.errorDictionary.Clear();
                OnPropertyChanged(nameof(ErrorMessageString));
                this.IsValid = false;
            } 
            else if (notification.Contains("This field may not be blank", StringComparison.InvariantCultureIgnoreCase) || notification.Contains("No active account found with the given credentials", StringComparison.InvariantCultureIgnoreCase))
            {
                this.StatusMessage = "Username or password incorrect";
            } 
            else if (notification.Contains("incorrect", StringComparison.InvariantCultureIgnoreCase))
            {
                this.StatusMessage = "Username or password incorrect";
            }
            else
            {
                this.StatusMessage = "Server Error Occurred";
            }

            this.IsStatusMessageOpen = true;
            this.IsLoading = false;
        }

        public IAppState AppState { get; }

        //Notifications

        //----------------------------------------------Form Properties & Validation-----------------------------
        private bool isLoading = false;
        private string statusMessage = "";
        private bool isStatusMessageOpen = false;

        private string username = "";
        private string password = "";
        private bool isValid = false;
        private Dictionary<string, string> errorDictionary = new Dictionary<string, string>();

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                SetProperty(ref isLoading, value);
                OnPropertyChanged(nameof(IsValid));
                this.LoginCommand.NotifyCanExecuteChanged();
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
        public bool IsValid
        {
            get => isValid && !isLoading;
            set
            {
                SetProperty(ref isValid, value);
                this.LoginCommand.NotifyCanExecuteChanged();
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
            }
            else
            {
                errorDictionary.Remove("username");
            }

            if (this.Password.Length < 8)
            {
                errorDictionary["password1"] = "Password needs to be at least 8 characters";
            }
            else
            {
                errorDictionary.Remove("password1");
            }

            this.IsValid = this.errorDictionary.Values.Count == 0;
            OnPropertyChanged(nameof(ErrorMessageString));
        }
        private HttpContent generateHttpContent()
        {
            var values = new Dictionary<string, string>
            {
                { "username", this.Username },
                { "password", this.Password }
            };
            return new FormUrlEncodedContent(values);
        }

        //----------------------------------------------Commands-----------------------------
        public AsyncRelayCommand LoginCommand { get; }
        private async Task Login()
        {
            this.IsLoading = true;

            HttpContent loginInfo = this.generateHttpContent();
            await this.AppState.OverallAppModel.LoginControl.PostLoginRequestAndNotify(loginInfo);   
        }
        private bool CanLogin()
        {
            return (this.IsValid && !this.IsLoading);
        }

        public void Unloaded()
        {
            this.AppState.OverallAppModel.LoginControl.LoginFormResponse -= LoginControl_LoginFormResponse;
        }
    }
}
