using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VSEAppV2.Models
{
    public enum VSEType
    {
        Value,
        Skill,
        Experience
    }
    
    public class VSEControlModel: BaseAppModel
    {
        public VSEControlModel(OverallAppModel overallApp, LoginControlModel loginControl)
        {
            this.OverallApp = overallApp;
            this.LoginControl = loginControl;
        }
        public OverallAppModel OverallApp { get; }
        public LoginControlModel LoginControl { get; }

        //-------------------------------------------Properties---------------------------------------
        public List<VSEValue> VSEValueList { get; } = new List<VSEValue>();
        public List<VSESkill> VSESkillList { get; } = new List<VSESkill>();
        public List<VSEExperience> VSEExperienceList { get; } = new List<VSEExperience>();

        //------------------------------------------Methods------------------------------------------
        //Read data for v, S, E individually
        public async Task SetVSEDataListAndNotify(VSEType vseType)
        {
            this.OverallApp.IsLoading = true;

            string accessToken = await this.APIHelperService.GetAccessTokenOrEmptyStringAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                this.LoginControl.RaiseSessionExpired();
                return;
            }

            string url = "";
            switch (vseType)
            {
                case VSEType.Value:
                    url = "https://lwbjvseapp.herokuapp.com/drf_api/vse/Value/";
                    break;
                case VSEType.Skill:
                    url = "https://lwbjvseapp.herokuapp.com/drf_api/vse/Skill/";
                    break;
                case VSEType.Experience:
                    url = "https://lwbjvseapp.herokuapp.com/drf_api/vse/Experience/";
                    break;
            }

            var dataStreamAPIHelperResponse = await APIHelperService.GetDataStreamOrNullAsync(url, accessToken);
            Stream responseStream = (Stream)dataStreamAPIHelperResponse.ReturnedData;
            if (responseStream == null)
            {
                //Something has gone wrong. raise server error.
                this.ServerError?.Invoke(this, new FormResponse_EventHandlerArg(dataStreamAPIHelperResponse.StatusString));
                return;
            }

            switch (vseType)
            {
                case VSEType.Value:
                    List<VSEValue> vseValueResults = await JsonSerializer.DeserializeAsync<List<VSEValue>>(responseStream);
                    this.VSEValueList.Clear();
                    foreach (VSEValue vseValue in vseValueResults)
                    {
                        this.VSEValueList.Add(vseValue);
                    }
                    this.VSEValuesSet?.Invoke(this, EventArgs.Empty);
                    break;
                case VSEType.Skill:
                    List<VSESkill> vseSkillResults = await JsonSerializer.DeserializeAsync<List<VSESkill>>(responseStream);
                    this.VSESkillList.Clear();
                    foreach (VSESkill vseSkill in vseSkillResults)
                    {
                        this.VSESkillList.Add(vseSkill);
                    }
                    this.VSESkillsSet?.Invoke(this, EventArgs.Empty);
                    break;
                case VSEType.Experience:
                    List<VSEExperience> vseExperienceResults = await JsonSerializer.DeserializeAsync<List<VSEExperience>>(responseStream);
                    this.VSEExperienceList.Clear();
                    foreach (VSEExperience vseExperience in vseExperienceResults)
                    {
                        this.VSEExperienceList.Add(vseExperience);
                    }
                    this.VSEExperiencesSet?.Invoke(this, EventArgs.Empty);
                    break;
            }

            return;
        }
        
        //Generic create, update, delete methods, starting from user input
        public async Task CreateVSEDataAndNotify(VSEType vseType, HttpContent createFormContent)
        {
            this.OverallApp.IsLoading = true;

            string accessToken = await this.APIHelperService.GetAccessTokenOrEmptyStringAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                this.OverallApp.IsLoading = false;
                this.LoginControl.RaiseSessionExpired();
                return;
            }

            string url = "";
            switch (vseType)
            {
                case VSEType.Value:
                    url = "https://lwbjvseapp.herokuapp.com/drf_api/vse/Value/";
                    this.VSEValueList.Clear();
                    break;
                case VSEType.Skill:
                    url = "https://lwbjvseapp.herokuapp.com/drf_api/vse/Skill/";
                    this.VSESkillList.Clear();
                    break;
                case VSEType.Experience:
                    url = "https://lwbjvseapp.herokuapp.com/drf_api/vse/Experience/";
                    this.VSEExperienceList.Clear();
                    break;
            }

            string responseStatus = await this.APIHelperService.PostOrPutAndReturnStatusStringAsync(url, accessToken, HttpMethod.Post, createFormContent);
            if (responseStatus == "OK")
            {
                await this.SetVSEDataListAndNotify(VSEType.Value);
                await this.SetVSEDataListAndNotify(VSEType.Skill);
                await this.SetVSEDataListAndNotify(VSEType.Experience);
            }

            switch (vseType)
            {
                case VSEType.Value:
                    this.VSEValueCreate?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
                    break;
                case VSEType.Skill:
                    this.VSESkillCreate?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
                    break;
                case VSEType.Experience:
                    this.VSEExperienceCreate?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
                    break;
            }
            this.OverallApp.IsLoading = false;
        }
        public async Task UpdateVSEDataAndNotify(VSEType vseType, HttpContent updateFormContent, string url)
        {
            this.OverallApp.IsLoading = true;

            string accessToken = await this.APIHelperService.GetAccessTokenOrEmptyStringAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                this.OverallApp.IsLoading = false;
                this.LoginControl.RaiseSessionExpired();
                return;
            }

            string responseStatus = await this.APIHelperService.PostOrPutAndReturnStatusStringAsync(url, accessToken, HttpMethod.Put, updateFormContent);
            if (responseStatus == "OK")
            {
                await this.SetVSEDataListAndNotify(VSEType.Value);
                await this.SetVSEDataListAndNotify(VSEType.Skill);
                await this.SetVSEDataListAndNotify(VSEType.Experience);
            }

            switch (vseType)
            {
                case VSEType.Value:
                    this.VSEValueUpdate?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
                    break;
                case VSEType.Skill:
                    this.VSESkillUpdate?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
                    break;
                case VSEType.Experience:
                    this.VSEExperienceUpdate?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
                    break;
            }
            this.OverallApp.IsLoading = false;
        }
        public async Task DeleteVSEDataAndNotify(VSEType vseType, string url)
        {
            this.OverallApp.IsLoading = true;

            string accessToken = await this.APIHelperService.GetAccessTokenOrEmptyStringAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                this.OverallApp.IsLoading = false;
                this.LoginControl.RaiseSessionExpired();
                return;
            }

            string responseStatus = await this.APIHelperService.DeleteAndReturnStatusStringAsync(url, accessToken);
            if (responseStatus == "OK")
            {
                await this.SetVSEDataListAndNotify(VSEType.Value);
                await this.SetVSEDataListAndNotify(VSEType.Skill);
                await this.SetVSEDataListAndNotify(VSEType.Experience);
            }

            switch (vseType)
            {
                case VSEType.Value:
                    this.VSEValueDelete?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
                    break;
                case VSEType.Skill:
                    this.VSESkillDelete?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
                    break;
                case VSEType.Experience:
                    this.VSEExperienceDelete?.Invoke(this, new FormResponse_EventHandlerArg(responseStatus));
                    break;
            }
            this.OverallApp.IsLoading = false;
        }

        //----------------------------------------------------------Events----------------------------------------------------
        public event EventHandler<FormResponse_EventHandlerArg> ServerError;

        public event EventHandler VSEValuesSet;
        public event EventHandler<FormResponse_EventHandlerArg> VSEValueCreate;
        public event EventHandler<FormResponse_EventHandlerArg> VSEValueUpdate;
        public event EventHandler<FormResponse_EventHandlerArg> VSEValueDelete;

        public event EventHandler VSESkillsSet;
        public event EventHandler<FormResponse_EventHandlerArg> VSESkillCreate;
        public event EventHandler<FormResponse_EventHandlerArg> VSESkillUpdate;
        public event EventHandler<FormResponse_EventHandlerArg> VSESkillDelete;

        public event EventHandler VSEExperiencesSet;
        public event EventHandler<FormResponse_EventHandlerArg> VSEExperienceCreate;
        public event EventHandler<FormResponse_EventHandlerArg> VSEExperienceUpdate;
        public event EventHandler<FormResponse_EventHandlerArg> VSEExperienceDelete;
    }
}
