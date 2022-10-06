using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSEAppV2.Services;
using VSEAppV2.Views;

namespace VSEAppV2.ViewModels
{
    public class MainWindowViewModel : ObservableRecipient
    {
        public MainWindowViewModel(IAppState appState, Frame frame)
        {
            this.AppState = appState;
            this.AppState.OverallAppModel.LoginControl.PropertyChanged += LoginControl_PropertyChanged;

            //Start up code
            this.mainFrame = frame;
            this.ToDoOnStartup();
        }
        public IAppState AppState { get; }
        private void LoginControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsLoggedIn")
            {
                if (this.AppState.OverallAppModel.LoginControl.IsLoggedIn && !string.IsNullOrEmpty(this.AppState.OverallAppModel.LoginControl.CurrentUser.Username))
                {
                    mainFrame.Navigate(typeof(LoggedInView));
                }
                else
                {
                    mainFrame.Navigate(typeof(LoggedOutPage));
                }
            }
        }

        //----------------------------------------------Exposed Properties-----------------------------
        private Frame mainFrame;

        //----------------------------------------------Actions-----------------------------------------
        private void ToDoOnStartup()
        {
            this.AppState.OverallAppModel.LoginControl.FirstStartUp();
        }
    }
}
