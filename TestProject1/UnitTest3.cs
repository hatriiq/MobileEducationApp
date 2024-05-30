using Xunit;
using System;
using C971;

namespace TestProject1
{
    public class UnitTest3
    {
        [Fact]
        public void CheckDates_ShouldReturnTrue_WhenEndDateIsGreaterThanOrEqualToStartDate()
        {
 
            DateTime startDate = new DateTime(2024, 5, 1);
            DateTime endDate = new DateTime(2024, 5, 2);

            bool result = MainPage.CheckDates(startDate, endDate);

            Assert.True(result);
        }

        [Fact]
        public void CheckDates_ShouldReturnFalse_WhenEndDateIsEarlierThanStartDate()
        {
            
            DateTime startDate = new DateTime(2024, 5, 2);
            DateTime endDate = new DateTime(2024, 5, 1);
     
            bool result = MainPage.CheckDates(startDate, endDate);

            Assert.False(result);
        }
    }
}



