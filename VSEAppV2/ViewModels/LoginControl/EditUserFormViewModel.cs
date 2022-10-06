using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
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
    public class EditUserFormViewModel: ObservableRecipient
    {
        public EditUserFormViewModel(IAppState appState)
        {
            this.AppState = appState;

            this.AppState.OverallAppModel.LoginControl.EditUsername_FormResponse += LoginControl_EditUsername_FormResponse;
            this.AppState.OverallAppModel.LoginControl.EditPassword_FormResponse += LoginControl_EditPassword_FormResponse;
            this.AppState.OverallAppModel.LoginControl.DeleteUser_FormResponse += LoginControl_DeleteUser_FormResponse;

            this.EditUsernameCommand = new AsyncRelayCommand(this.EditUsername, this.CanEditUsername);
            this.EditPasswordCommand = new AsyncRelayCommand(this.EditPassword, this.CanEditPassword);
            this.DeleteUserCommand = new AsyncRelayCommand(this.DeleteUser, this.CanDeleteUser);
        }

        public IAppState AppState { get; }
        //----------------------------------------------------Notifications-----------------------------------------------------
        private void LoginControl_DeleteUser_FormResponse(object sender, FormResponse_EventHandlerArg e)
        {
            string notification = e.StatusMessage;
            if (notification == "OK")
            {
                //User is deleted. Find someway to close modal.
                if (this.editUserModal != null)
                {
                    this.editUserModal.Hide();
                    this.editUserModal = null;
                }
            }
            else if (notification.Contains("Password Incorrect", StringComparison.InvariantCultureIgnoreCase))
            {
                this.DeleteUser_StatusMessage = "Password is incorrect";
            }
            else
            {
                this.DeleteUser_StatusMessage = "Server Error Occurred";
            }

            this.DeleteUser_StatusMessageIsOpen = true;
            this.IsLoading = false;
        }

        private void LoginControl_EditPassword_FormResponse(object sender, FormResponse_EventHandlerArg e)
        {
            string notification = e.StatusMessage;
            if (notification == "OK")
            {
                this.EditPassword_StatusMessage = "Password Changed";
            }
            else if (notification.Contains("Password Incorrect", StringComparison.InvariantCultureIgnoreCase))
            {
                this.EditPassword_StatusMessage = "Password is incorrect";
            }
            else if (notification.Contains("This password is too common", StringComparison.InvariantCultureIgnoreCase))
            {
                this.EditPassword_StatusMessage = "This password is too common";
            }
            else
            {
                this.EditPassword_StatusMessage = "Server Error Occurred";
            }

            this.EditPassword_StatusMessageIsOpen = true;
            this.IsLoading = false;
        }

        private async void LoginControl_EditUsername_FormResponse(object sender, FormResponse_EventHandlerArg e)
        {
            string notification = e.StatusMessage;
            if (notification == "OK")
            {
                this.EditUsername_StatusMessage = "Username changed";
                await this.AppState.OverallAppModel.LoginControl.SetCurrentUserBasedOnTokenOrSetNullAsync();
                this.originalUsername = this.AppState.OverallAppModel.LoginControl.CurrentUser.Username;
            }
            else if (notification.Contains("This field must be unique", StringComparison.InvariantCultureIgnoreCase))
            {
                this.EditUsername_StatusMessage = "Username already taken";
            }
            else if (notification.Contains("Session Expired", StringComparison.InvariantCultureIgnoreCase))
            {
                this.EditUsername_StatusMessage = "Session has expired. Please logout and login again";
            }
            else
            {
                this.EditUsername_StatusMessage = "A server error occurred";
            }

            this.EditUsername_StatusMessageIsOpen = true;
            this.IsLoading = false;
        }

        //---------------------------------------------Form properties, fields & validation-----------------------------------------
        private bool isLoading = false;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                SetProperty(ref isLoading, value);

                OnPropertyChanged(nameof(EditUsername_IsValid));
                OnPropertyChanged(nameof(EditPassword_IsValid));
                OnPropertyChanged(nameof(DeleteUser_IsValid));

                this.EditUsernameCommand.NotifyCanExecuteChanged();
                this.EditPasswordCommand.NotifyCanExecuteChanged();
                this.DeleteUserCommand.NotifyCanExecuteChanged();
            }
        }

        //Form fields & validation
        private string originalUsername = "";
        private string editUsername_Username = "";
        private bool editUsername_isValid = false;
        private Dictionary<string, string> editUsername_errorDictionary = new Dictionary<string, string>();
        private string editUsername_StatusMessage;
        private bool editUsername_StatusMessageIsOpen;

        private string editPassword_OldPassword = "";
        private string editPassword_NewPassword = "";
        private string editPassword_ConfirmPassword = "";
        private bool editPassword_isValid = false;
        private Dictionary<string, string> editPassword_errorDictionary = new Dictionary<string, string>();
        private string editPassword_StatusMessage;
        private bool editPassword_StatusMessageIsOpen;

        private string deleteUser_Password = "";
        private bool deleteUser_isValid = false;
        private Dictionary<string, string> deleteUser_errorDictionary = new Dictionary<string, string>();
        private string deleteUser_StatusMessage;
        private bool deleteUser_StatusMessageIsOpen;

        //---------------------------------public properties-------------------------------------

        public string EditUsername_Username
        {
            get => editUsername_Username;
            set
            {
                SetProperty(ref editUsername_Username, value);
                this.CheckEditUsernameValidity();
            }
        }
        public bool EditUsername_IsValid
        {
            get => editUsername_isValid && !IsLoading;
            set
            {
                SetProperty(ref editUsername_isValid, value);
                this.EditUsernameCommand.NotifyCanExecuteChanged();
            }
        }
        public string EditUsername_ErrorMessageString
        {
            get
            {
                string message = "";
                foreach (string error in editUsername_errorDictionary.Values.ToList())
                {
                    message += (string.IsNullOrEmpty(error) ? "" : error + "\n");
                }
                return message;
            }
        }
        private void CheckEditUsernameValidity()
        {
            if (this.EditUsername_Username.Length <= 0)
            {
                editUsername_errorDictionary["EditUsername"] = "New username cannot be blank";
            }
            else if (this.EditUsername_Username == this.originalUsername)
            {
                editUsername_errorDictionary["EditUsername"] = "Same username as before";
            }
            else
            {
                editUsername_errorDictionary.Remove("EditUsername");
            }

            this.EditUsername_IsValid = this.editUsername_errorDictionary.Values.Count == 0;
            OnPropertyChanged(nameof(EditUsername_ErrorMessageString));
        }
        private HttpContent EditUsername_GenerateHttpContent()
        {
            var values = new Dictionary<string, string>
            {
                { "username", this.EditUsername_Username },
            };
            return new FormUrlEncodedContent(values);
        }
        public string EditUsername_StatusMessage
        {
            get => editUsername_StatusMessage;
            set => SetProperty(ref editUsername_StatusMessage, value);
        }
        public bool EditUsername_StatusMessageIsOpen
        {
            get => editUsername_StatusMessageIsOpen;
            set => SetProperty(ref editUsername_StatusMessageIsOpen, value);
        }

        public string EditPassword_OldPassword
        {
            get => editPassword_OldPassword;
            set
            {
                SetProperty(ref editPassword_OldPassword, value);
                this.CheckEditPasswordValidity();
            }
        }
        public string EditPassword_NewPassword
        {
            get => editPassword_NewPassword;
            set
            {
                SetProperty(ref editPassword_NewPassword, value);
                this.CheckEditPasswordValidity();
            }
        }
        public string EditPassword_ConfirmPassword
        {
            get => editPassword_ConfirmPassword;
            set
            {
                SetProperty(ref editPassword_ConfirmPassword, value);
                this.CheckEditPasswordValidity();
            }
        }
        public bool EditPassword_IsValid
        {
            get => editPassword_isValid && !IsLoading;
            set
            {
                SetProperty(ref editPassword_isValid, value);
                this.EditPasswordCommand.NotifyCanExecuteChanged();
            }
        }
        public string EditPassword_ErrorMessageString
        {
            get
            {
                string message = "";
                foreach (string error in editPassword_errorDictionary.Values.ToList())
                {
                    message += (string.IsNullOrEmpty(error) ? "" : error + "\n");
                }
                return message;
            }
        }
        private void CheckEditPasswordValidity()
        {
            //-----------------------------------------Check old password length-----------------------------------------
            if (this.EditPassword_OldPassword.Length < 8)
            {
                editPassword_errorDictionary["EditPassword_OldPasswordLength"] = "Previous password is at least 8 characters";
            }
            else
            {
                editPassword_errorDictionary.Remove("EditPassword_OldPasswordLength");
            }
            //-----------------------------------------Check new password length-----------------------------------------
            if (this.EditPassword_NewPassword.Length < 8)
            {
                editPassword_errorDictionary["EditPassword_NewPasswordLength"] = "New password must be at least 8 characters";
            }
            else
            {
                editPassword_errorDictionary.Remove("EditPassword_NewPasswordLength");
            }
            //-----------------------------------------Check password match-----------------------------------------
            if (this.EditPassword_NewPassword != this.EditPassword_ConfirmPassword)
            {
                editPassword_errorDictionary["EditPassword_PasswordMatch"] = "Passwords do not match";
            }
            else
            {
                editPassword_errorDictionary.Remove("EditPassword_PasswordMatch");
            }

            this.EditPassword_IsValid = this.editPassword_errorDictionary.Values.Count == 0;
            OnPropertyChanged(nameof(EditPassword_ErrorMessageString));
        }
        private HttpContent EditPassword_GenerateHttpContent()
        {
            var values = new Dictionary<string, string>
            {
                { "password", this.EditPassword_NewPassword },
                { "password2", this.EditPassword_ConfirmPassword },
            };
            return new FormUrlEncodedContent(values);
        }
        public string EditPassword_StatusMessage
        {
            get => editPassword_StatusMessage;
            set => SetProperty(ref editPassword_StatusMessage, value);
        }
        public bool EditPassword_StatusMessageIsOpen
        {
            get => editPassword_StatusMessageIsOpen;
            set => SetProperty(ref editPassword_StatusMessageIsOpen, value);
        }

        public string DeleteUser_Password
        {
            get => deleteUser_Password;
            set
            {
                SetProperty(ref deleteUser_Password, value);
                this.CheckDeleteUserValidity();
            }
        }
        public bool DeleteUser_IsValid
        {
            get => deleteUser_isValid && !IsLoading;
            set
            {
                SetProperty(ref deleteUser_isValid, value);
                this.DeleteUserCommand.NotifyCanExecuteChanged();
            }
        }
        public string DeleteUser_ErrorMessageString
        {
            get
            {
                string message = "";
                foreach (string error in deleteUser_errorDictionary.Values.ToList())
                {
                    message += (string.IsNullOrEmpty(error) ? "" : error + "\n");
                }
                return message;
            }
        }
        private void CheckDeleteUserValidity()
        {
            if (this.DeleteUser_Password.Length < 8)
            {
                deleteUser_errorDictionary["DeleteUser"] = "Password is at least 8 characters";
            }
            else
            {
                deleteUser_errorDictionary.Remove("DeleteUser");
            }

            this.DeleteUser_IsValid = this.deleteUser_errorDictionary.Values.Count == 0;
            OnPropertyChanged(nameof(DeleteUser_ErrorMessageString));
        }
        public string DeleteUser_StatusMessage
        {
            get => deleteUser_StatusMessage;
            set => SetProperty(ref deleteUser_StatusMessage, value);
        }
        public bool DeleteUser_StatusMessageIsOpen
        {
            get => deleteUser_StatusMessageIsOpen;
            set => SetProperty(ref deleteUser_StatusMessageIsOpen, value);
        }

        private ContentDialog editUserModal;
        
        //------------------------------------------------------Actions & Commands---------------------------------------------------------
        public void SetInitialFormContent(ContentDialog contentDialog)
        {
            this.editUserModal = contentDialog;
            this.editUserModal.ShowAsync();
            
            if (this.AppState.OverallAppModel.LoginControl.CurrentUser != null && !string.IsNullOrEmpty(this.AppState.OverallAppModel.LoginControl.CurrentUser.Username))
            {
                this.originalUsername = this.AppState.OverallAppModel.LoginControl.CurrentUser.Username;
                this.EditUsername_Username = this.AppState.OverallAppModel.LoginControl.CurrentUser.Username;
            }
        }

        public AsyncRelayCommand EditUsernameCommand { get; }
        private async Task EditUsername()
        {
            this.IsLoading = true;
            
            HttpContent editUsernameFormContent = this.EditUsername_GenerateHttpContent();
            await this.AppState.OverallAppModel.LoginControl.PutUpdateUsernameAndNotify(editUsernameFormContent);
        }
        private bool CanEditUsername()
        {
            return (this.EditUsername_IsValid && !this.IsLoading);
        }

        public AsyncRelayCommand EditPasswordCommand { get; }
        private async Task EditPassword()
        {
            this.IsLoading = true;

            var loginFormValues = new Dictionary<string, string>
            {
                { "username", this.AppState.OverallAppModel.LoginControl.CurrentUser.Username },
                { "password", this.EditPassword_OldPassword },
            };
            HttpContent loginFormContent = new FormUrlEncodedContent(loginFormValues);
            HttpContent passwordFormContent = this.EditPassword_GenerateHttpContent();
            await this.AppState.OverallAppModel.LoginControl.PutUpdatePasswordAndNotify(loginFormContent, passwordFormContent);
        }
        private bool CanEditPassword()
        {
            return (this.EditPassword_IsValid && !this.IsLoading);
        }

        public AsyncRelayCommand DeleteUserCommand { get; }
        private async Task DeleteUser()
        {
            this.IsLoading = true;

            var loginFormValues = new Dictionary<string, string>
            {
                { "username", this.AppState.OverallAppModel.LoginControl.CurrentUser.Username },
                { "password", this.DeleteUser_Password },
            };
            HttpContent loginFormContent = new FormUrlEncodedContent(loginFormValues);

            await this.AppState.OverallAppModel.LoginControl.DeleteUserAndNotify(loginFormContent);
        }
        private bool CanDeleteUser()
        {
            return (this.DeleteUser_IsValid && !this.IsLoading);
        }
        
        public void Unloaded()
        {
            this.AppState.OverallAppModel.LoginControl.EditUsername_FormResponse -= LoginControl_EditUsername_FormResponse;
            this.AppState.OverallAppModel.LoginControl.EditPassword_FormResponse -= LoginControl_EditPassword_FormResponse;
            this.AppState.OverallAppModel.LoginControl.DeleteUser_FormResponse -= LoginControl_DeleteUser_FormResponse;
        }
    }
}
