using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSEAppV2.Models;

namespace VSEAppV2.Messages
{
    public class OpenModal_VSESkill: ValueChangedMessage<VSESkill>
    {
        public OpenModal_VSESkill(VSESkill value) : base(value) { }
    }
}
