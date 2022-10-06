using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSEAppV2.Services;

namespace VSEAppV2.Models
{
    public class OverallAppModel: BaseAppModel
    {   
        public OverallAppModel()
        {
            this.LoginControl = new LoginControlModel(this);
        }
        public LoginControlModel LoginControl { get; }

        //Dumb properties
        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

    }
}
