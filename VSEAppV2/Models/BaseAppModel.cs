using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSEAppV2.Services;

namespace VSEAppV2.Models
{
    public class BaseAppModel: ObservableObject
    {
        public APIHelperService APIHelperService { get; } = new APIHelperService();
        public void NotifySomething(string thing)
        {
            OnPropertyChanged(thing);
        }
    }
}
