using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VSEAppV2.Models;

namespace VSEAppV2.Services
{
    public interface IAPIHelperService
    {
        public Task<string> GetAccessTokenOrEmptyStringAsync();

        public Task<ReturnedDataAndResponseStatusObject> GetDataStreamOrNullAsync(string url, string accessToken);
        public Task<string> PostOrPutAndReturnStatusStringAsync(string url, string accessToken, HttpMethod httpMethod, HttpContent theFormContent);
        public Task<string> DeleteAndReturnStatusStringAsync(string url, string accessToken);

        public Task<ReturnedDataAndResponseStatusObject> PostLoginRequestAndReturnTokenPairOrNullAsync(HttpContent loginFormContent);
        public Task<ReturnedDataAndResponseStatusObject> GetUserDataOrNullAsync(string accessToken);
    }
}
