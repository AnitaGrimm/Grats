using Grats.Extensions;
using Grats.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Grats.ViewModels
{
    public class CategoryMasterViewModel
    {
        public Category Category { get; set; }
        public SolidColorBrush Fill
        {
            get
            {
                return new SolidColorBrush(ColorExtensions.FromHex(Category.Color));
            }
        }
        public string Name
        {
            get
            {
                return Category.Name;
            }
        }

        public CategoryMasterViewModel() { }

        public CategoryMasterViewModel(Category category)
        {
            this.Category = category;
        }
        public bool IsValid()
        {
            return Name != "" && Category != null && Category.CategoryContacts != null && Category.CategoryContacts.Count!=0 && Category.Tasks != null && Category.Tasks.Count!=0 && Category.Name != "" && Category.Template!="";
        }
    }
}
