using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSEAppV2.ViewModels
{
    public class NotifyItemsControlOfNewSelectedItemsEvent<T>: EventArgs
    {
        public NotifyItemsControlOfNewSelectedItemsEvent(List<T> newSelectedItems)
        {
            this.NewSelectedItems = newSelectedItems;
        }
        public List<T> NewSelectedItems { get; set; }
    }
}
