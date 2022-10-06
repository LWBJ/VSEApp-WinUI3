using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VSEAppV2.Models;

namespace VSEAppV2.Services
{
    public class AppState: ObservableRecipient, IAppState
    {
        public AppState(IAPIHelperService APIHelperService)
        {
            this.OverallAppModel = new OverallAppModel();
        }

        public OverallAppModel OverallAppModel { get; set; }
    }
}
