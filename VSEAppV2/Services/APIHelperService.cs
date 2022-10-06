using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using VSEAppV2.Models;

namespace VSEAppV2.Services
{
    public class APIHelperService: IAPIHelperService
    {
        public HttpClient HttpClient => (App.Current as App).HttpClient;

        public async Task<string> GetAccessTokenOrEmptyStringAsync()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string refreshToken = (string)localSettings.Values["refreshToken"];

            if (refreshToken == null)
            {
                return "";
            }

            var formValues = new Dictionary<string, string>
                {
                    {"refresh", refreshToken},
                };
            var formEncodedValues = new FormUrlEncodedContent(formValues);
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://lwbjvseapp.herokuapp.com/drf_api/token_refresh/"),
                Content = formEncodedValues
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var response = await this.HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    var result = await JsonSerializer.DeserializeAsync<TokenPair>(responseStream);

                    return result.AccessToken;
                }

                return "";
            } catch
            {
                return null;
            }
        }

        public async Task<ReturnedDataAndResponseStatusObject> GetDataStreamOrNullAsync (string url, string accessToken) {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await this.HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    return new ReturnedDataAndResponseStatusObject(responseStream, "OK");
                }
                else
                {
                    return new ReturnedDataAndResponseStatusObject(null, await response.Content.ReadAsStringAsync());
                }
            } catch
            {
                return new ReturnedDataAndResponseStatusObject(null, "BIGOOPS");
            }
        }

        public async Task<string> PostOrPutAndReturnStatusStringAsync(string url, string accessToken, HttpMethod httpMethod, HttpContent theFormContent)
        {
            var request = new HttpRequestMessage()
            {
                Method = httpMethod,
                RequestUri = new Uri(url),
                Content = theFormContent
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (accessToken != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            try
            {
                var response = await this.HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    //return await response.Content.ReadAsStringAsync();
                    return "OK";
                }
                else
                {
                    return await response.Content.ReadAsStringAsync();
                }
            } catch
            {
                return "BIGOOPS";
            }
        }
        
        public async Task<string> DeleteAndReturnStatusStringAsync(string url, string accessToken)
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await this.HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    return "An error occurred";
                }
            } catch
            {
                return "An error occurred";
            }
        }

        public async Task<ReturnedDataAndResponseStatusObject> PostLoginRequestAndReturnTokenPairOrNullAsync(HttpContent loginFormContent)
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://lwbjvseapp.herokuapp.com/drf_api/token_pair/"),
                Content = loginFormContent
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var response = await this.HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    var results = await JsonSerializer.DeserializeAsync<TokenPair>(responseStream);

                    return new ReturnedDataAndResponseStatusObject(results, "OK");
                }
                else
                {
                    return new ReturnedDataAndResponseStatusObject(null, await response.Content.ReadAsStringAsync());
                }
            } catch
            {
                return new ReturnedDataAndResponseStatusObject(null, "BIGOOPS");
            }
        }

        public async Task<ReturnedDataAndResponseStatusObject> GetUserDataOrNullAsync(string accessToken)
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://lwbjvseapp.herokuapp.com/drf_api/user/"),
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await this.HttpClient.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var finalRequestUri = response.RequestMessage.RequestUri;
                    var finalRequest = new HttpRequestMessage()
                    {
                        Method = HttpMethod.Get,
                        RequestUri = finalRequestUri
                    };
                    finalRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    finalRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    try
                    {
                        var finalResponse = await this.HttpClient.SendAsync(finalRequest);
                        if (finalResponse.IsSuccessStatusCode)
                        {
                            var finalResponseStream = await finalResponse.Content.ReadAsStreamAsync();
                            var finalResult = await JsonSerializer.DeserializeAsync<VSEUser>(finalResponseStream);
                            return new ReturnedDataAndResponseStatusObject(finalResult, "OK");
                        }
                        else
                        {
                            return new ReturnedDataAndResponseStatusObject(null, await finalResponse.Content.ReadAsStringAsync());
                        }
                    } catch
                    {
                            return new ReturnedDataAndResponseStatusObject(null, "BIGOOPS");
                    }
                }
                else
                {
                    return new ReturnedDataAndResponseStatusObject(null, await response.Content.ReadAsStringAsync());
                }
            } catch
            {
                    return new ReturnedDataAndResponseStatusObject(null, "BIGOOPS");
            }
        }
    }

    public class ReturnedDataAndResponseStatusObject
    {
        public ReturnedDataAndResponseStatusObject(object returnedData, string statusString)
        {
            this.ReturnedData = returnedData;
            this.StatusString = statusString;
        }
        public object ReturnedData { get; set; }
        public string StatusString { get; set; }
    }
}
