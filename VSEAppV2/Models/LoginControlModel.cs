using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VSEAppV2.Models
{
    public class LoginControlModel: BaseAppModel
    {
        public LoginControlModel(OverallAppModel overallApp)
        {
            this.OverallApp = overallApp;
            this.VSEControl = new VSEControlModel(overallApp, this);
        }
        public OverallAppModel OverallApp { get;}
        public VSEControlModel VSEControl { get; }

        //-------------------------------------------------Properties--------------------------------------------
        private VSEUser currentUser;
        public VSEUser CurrentUser
        {
            get => currentUser;
            set
            {
                var originalUser = currentUser;
                SetProperty(ref currentUser, value);
                if (originalUser == null && value != null ||  originalUser != null && value == null)
                {
                    OnPropertyChanged(nameof(IsLoggedIn));
                }
            }
        }
        public bool IsLoggedIn
        {
            get => currentUser != null;
        }

        //---------------------------------------------Methods----------------------------------------------
        public async Task FirstStartUp()
        {
            this.OverallApp.IsLoading = true;
            await this.SetCurrentUserBasedOnTokenOrSetNullAsync();
            OnPropertyChanged(nameof(IsLoggedIn));
            this.OverallApp.IsLoading = false;
        }
        /*public async Task ReadUserDataWrapper()
        {
            this.OverallApp.IsLoading = true;
            await this.SetCurrentUserBasedOnTokenOrSetNullAsync();
            this.OverallApp.IsLoading = false;
        }*/
        public async Task SetCurrentUserBasedOnTokenOrSetNullAsync()
        {
            this.OverallApp.IsLoading = true;

            string accessToken = await this.APIHelperService.GetAccessTokenOrEmptyStringAsync();

            if (!string.IsNullOrEmpty(accessToken))
            {
                var apiHelperResponse = await APIHelperService.GetUserDataOrNullAsync(accessToken);
                VSEUser user = (VSEUser)apiHelperResponse.ReturnedData;
                if (user != null)
                {
                    this.CurrentUser = user;
                    return;
                }
            }

            this.CurrentUser = null;
        }

        public async Task PostSignupRequestAndNotify(HttpContent signupFormContent)
        {
            this.OverallApp.IsLoading = true;

            string url = "https://lwbjvseapp.herokuapp.com/drf_api/signup/";
            string responseStatus = await this.APIHelperService.PostOrPutAndReturnStatusStringAsync(url, null, HttpMethod.Post, signupFormContent);

            //Notify & set loading
            SignupFormResponse?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
            this.OverallApp.IsLoading = false;
        }
        public async Task PostLoginRequestAndNotify(HttpContent loginFormContent)
        {
            this.OverallApp.IsLoading = true;

            var loginRequestApiHelperResponse = await this.APIHelperService.PostLoginRequestAndReturnTokenPairOrNullAsync(loginFormContent);
            TokenPair tokenPair = (TokenPair)loginRequestApiHelperResponse.ReturnedData;
            if (tokenPair == null)
            {
                LoginFormResponse?.Invoke(this, new FormResponse_EventHandlerArg(loginRequestApiHelperResponse.StatusString));
                this.OverallApp.IsLoading = false;
                return;
            }

            var GetUserDataAPIHelperResponse = await this.APIHelperService.GetUserDataOrNullAsync(tokenPair.AccessToken);
            VSEUser userData = (VSEUser)GetUserDataAPIHelperResponse.ReturnedData;
            if (userData == null)
            {
                LoginFormResponse?.Invoke(this, new FormResponse_EventHandlerArg(GetUserDataAPIHelperResponse.StatusString));
                this.OverallApp.IsLoading = false;
                return;
            }

            //Login is successful, execute login & notify
            LoginFormResponse?.Invoke(this, new FormResponse_EventHandlerArg(GetUserDataAPIHelperResponse.StatusString));
            this.Login(tokenPair, userData);
            this.OverallApp.IsLoading = false;
        }
        private void Login(TokenPair tokenPair, VSEUser newUser)
        {
            //Save login data
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["refreshToken"] = tokenPair.RefreshToken;

            //Set current user
            this.CurrentUser = newUser;
        }
        public void Logout()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["refreshToken"] = null;
            this.CurrentUser = null;

            //Clear VSEControl data
            this.VSEControl.VSEValueList.Clear();
            this.VSEControl.VSESkillList.Clear();
            this.VSEControl.VSEExperienceList.Clear();
        }

        public async Task PutUpdateUsernameAndNotify(HttpContent editUsernameFormContent)
        {
            this.OverallApp.IsLoading = true;

            string accessToken = await this.APIHelperService.GetAccessTokenOrEmptyStringAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                this.SessionExpired?.Invoke(this, EventArgs.Empty);
                this.EditUsername_FormResponse?.Invoke(this, new FormResponse_EventHandlerArg("Session Expired"));
                this.OverallApp.IsLoading = false;
                return;
            }

            string responseStatus = await this.APIHelperService.PostOrPutAndReturnStatusStringAsync(this.CurrentUser.Url, accessToken, HttpMethod.Put, editUsernameFormContent);
            if (responseStatus == "OK")
            {
                await this.SetCurrentUserBasedOnTokenOrSetNullAsync();
            }

            this.EditUsername_FormResponse?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
            this.OverallApp.IsLoading = false;
        }
        public async Task PutUpdatePasswordAndNotify(HttpContent loginFormContent, HttpContent newPasswordFormContent)
        {
            this.OverallApp.IsLoading = true;

            var loginRequestAPIHelperResponse = await this.APIHelperService.PostLoginRequestAndReturnTokenPairOrNullAsync(loginFormContent);
            TokenPair tokenPair = (TokenPair)loginRequestAPIHelperResponse.ReturnedData;
            if (tokenPair == null)
            {
                this.EditPassword_FormResponse?.Invoke(this, new FormResponse_EventHandlerArg("Password Incorrect"));
                this.OverallApp.IsLoading = false;
                return;
            }

            string responseStatus = await this.APIHelperService.PostOrPutAndReturnStatusStringAsync(this.CurrentUser.Url, tokenPair.AccessToken, HttpMethod.Put, newPasswordFormContent);
            if (responseStatus == "OK")
            {
                await this.SetCurrentUserBasedOnTokenOrSetNullAsync();
            }

            this.EditPassword_FormResponse?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
            this.OverallApp.IsLoading = false;
            return;
        }
        public async Task DeleteUserAndNotify(HttpContent loginFormContent)
        {
            this.OverallApp.IsLoading = true;

            var LoginRequestAPIHelperResponse = await this.APIHelperService.PostLoginRequestAndReturnTokenPairOrNullAsync(loginFormContent);
            TokenPair tokenPair = (TokenPair)LoginRequestAPIHelperResponse.ReturnedData;
            if (tokenPair == null)
            {
                this.DeleteUser_FormResponse?.Invoke(this, new FormResponse_EventHandlerArg("Password Incorrect"));
                this.OverallApp.IsLoading = false;
                return;
            }

            string responseStatus = await this.APIHelperService.DeleteAndReturnStatusStringAsync(this.CurrentUser.Url, tokenPair.AccessToken);
            if (responseStatus == "OK")
            {
                this.Logout();
            }

            this.DeleteUser_FormResponse?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
            this.OverallApp.IsLoading = false;
        }

        //-------------------------------------------Events------------------------------------------
        public event EventHandler SessionExpired;
        public void RaiseSessionExpired()
        {
            this.SessionExpired?.Invoke(this, EventArgs.Empty);
        }
        
        public event EventHandler<FormResponse_EventHandlerArg> SignupFormResponse;
        public event EventHandler<FormResponse_EventHandlerArg> LoginFormResponse;
        public event EventHandler<FormResponse_EventHandlerArg> EditUsername_FormResponse;
        public event EventHandler<FormResponse_EventHandlerArg> EditPassword_FormResponse;
        public event EventHandler<FormResponse_EventHandlerArg> DeleteUser_FormResponse;
    }
}
