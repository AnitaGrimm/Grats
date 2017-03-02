using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Grats.Extensions;
using Windows.UI;

namespace GratsTests
{
    public class ColorExtensionsTests
    {
        [Fact]
        public void CanCreateColorFromHex()
        {
            var color = ColorExtensions.FromHex("#FFFF0000");
            Assert.Equal(color, Colors.Red);
            color = ColorExtensions.FromHex("#FF008000");
            Assert.Equal(color, Colors.Green);
            color = ColorExtensions.FromHex("#FF0000FF");
            Assert.Equal(color, Colors.Blue);
        }

        [Fact]
        public void ThowsShitWhenWrongString()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { ColorExtensions.FromHex("123"); });
            Assert.Throws<FormatException>(() => { ColorExtensions.FromHex("#00GG00GG"); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { ColorExtensions.FromHex("#01424"); });
        }
    }
}
