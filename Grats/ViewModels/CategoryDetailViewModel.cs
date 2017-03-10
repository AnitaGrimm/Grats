using Grats.Extensions;
using Grats.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Grats.ViewModels
{
    public class CategoryDetailViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public Category Category { get; set; }

        private bool isBirthday;

        public ObservableCollection<ContactViewModel> Contacts = new ObservableCollection<ContactViewModel>();
        public string Name
        {
            get { return Category.Name;  }
            set
            {
                this.Category.Name = value;
                this.OnPropertyChanged();
            }
        }
        public DateTimeOffset? Date { get; set; }
        public bool IsBirthday
        {
            get
            {
                return this.isBirthday;
            }
            set
            {
                this.isBirthday = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("IsGeneral");
            }
        }
        public bool IsGeneral => !IsBirthday;

        public string MessageText
        {
            get { return Category.Template; }
            set
            {
                this.Category.Template = value;
                this.OnPropertyChanged();
            }
        }

        public Color Color
        {
            get { return ColorExtensions.FromHex(Category.Color);  }
            set
            {
                if (this.Category.Color.ToString() != value.ToString())
                {
                    this.Category.Color = value.ToString();
                    this.OnPropertyChanged();
                }
            }
        }

        public object CalendarPicker { get; private set; }

        public CategoryDetailViewModel() { }

        public CategoryDetailViewModel(Category category)
        {
            this.Category = category;
            this.Color = ColorExtensions.FromHex(category.Color);
            var contactViewModels = from categoryContact in category.CategoryContacts
                                    select new ContactViewModel(categoryContact.Contact);
            foreach (var vm in contactViewModels)
                Contacts.Add(vm);
            if (category is GeneralCategory)
                Date = (category as GeneralCategory).Date;
            else if (category is BirthdayCategory)
                IsBirthday = true;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Save(GratsDBContext db)
        {
            foreach (var categoryContact in Category.CategoryContacts)
            {
                var vkID = categoryContact.Contact.VKID;
                var existingContact = db.Contacts
                    .FirstOrDefault(c => c.VKID == vkID);
                if (existingContact != null)
                    categoryContact.Contact = existingContact;
            }

            if (Category.ID == 0)
                Create(db);
            else
                Update(db);
        }

        private void Update(GratsDBContext db)
        {
            if (IsBirthday && Category is GeneralCategory)
            {
                var result = new BirthdayCategory(Category);
                db.GeneralCategories.Remove(Category as GeneralCategory);
                db.BirthdayCategories.Add(result);
                db.SaveChanges();
                (result as ITaskGenerator).Regenerate(db);
            }
            else if(IsGeneral && Category is BirthdayCategory)
            {
                var result = new GeneralCategory(Category, Date.Value.DateTime);
                db.BirthdayCategories.Remove(Category as BirthdayCategory);
                db.GeneralCategories.Add(result);
                db.SaveChanges();
                (result as ITaskGenerator).Regenerate(db);
            }
            else
            {
                db.SaveChanges();
                (Category as ITaskGenerator).Regenerate(db);
            }
        }

        private void Create(GratsDBContext db)
        {
            if (IsBirthday)
            {
                var result = new BirthdayCategory(Category);
                db.BirthdayCategories.Add(result);
                db.SaveChanges();
                (result as ITaskGenerator).Generate(db);
            }
            else
            {
                var result = new GeneralCategory(Category, Date.Value.DateTime);
                db.GeneralCategories.Add(result);
                db.SaveChanges();
                (result as ITaskGenerator).Generate(db);
            }
        }
    }
}
