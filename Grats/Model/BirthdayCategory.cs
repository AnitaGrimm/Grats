using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grats.Model
{
    /// <summary>
    /// Поздравление с днем рождения
    /// </summary>
    public class BirthdayCategory : Category, ITaskGenerator
    { 
        public void Generate()
        {
            throw new NotImplementedException();
        }

        public void Regenerate()
        {
            throw new NotImplementedException();
        }
    }
}
