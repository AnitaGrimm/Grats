using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grats.ViewModels
{
    public class FriendsGroupByKey: List<VKUserViewModel>
    {
        public char Key { get; set; }
    }
}
