using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grats.ViewModels
{
    public class ReferenceViewModel
    {
        public string Title { get; private set; }
        public string Description { get; private set; }

        public ReferenceViewModel() { }

        public ReferenceViewModel(string title, string description)
        {
            this.Title = title;
            this.Description = description;
        }
    }
}
