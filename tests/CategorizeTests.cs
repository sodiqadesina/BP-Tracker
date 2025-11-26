
using BPA1.Models;
using Xunit;

namespace Tests
{
    public class CategorizeTests
    {
        [Theory]
        [InlineData(110,70,"Normal")]
        [InlineData(125,75,"Elevated")]
        [InlineData(132,79,"Stage 1")]
        [InlineData(141,90,"Stage 2")]
        [InlineData(181,110,"Hypertensive Crisis")]
        public void Categorize_Works(int s, int d, string expected)
        {
            Assert.Equal(expected, BpMeasurement.Categorize(s,d));
        }
    }
}
