using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Grats.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Создание цвета из HEX-нотации с альфа-каналом
        /// Использование:
        ///     var hex = "#00FF00FF;
        ///     var color = Color.FromHex(hex);
        /// </summary>
        /// <param name="hex">Цвет в шестнадцатиричной нотации</param>
        /// <returns>Цвет</returns>
        public static Color FromHex(String hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            return Color.FromArgb(a, r, g, b);
        }
    }
}
