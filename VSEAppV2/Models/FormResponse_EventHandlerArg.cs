using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSEAppV2.Models
{
    public class FormResponse_EventHandlerArg: EventArgs
    {
        public FormResponse_EventHandlerArg(string statusMessage)
        {
            this.StatusMessage = statusMessage;
        }
        
        public string StatusMessage;
    }
}
