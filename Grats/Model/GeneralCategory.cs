using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grats.Model
{
    /// <summary>
    /// Поздравление в заданную дату
    /// </summary>
    public class GeneralCategory: Category, ITaskGenerator
    {
        public DateTime Date { get; set; }

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
