using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grats.Model
{
    /// <summary>
    /// Предустановленные и сохраненные шаблоны
    /// </summary>
    public class Template
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public bool IsEmbedded { get; set; }
    }
}
