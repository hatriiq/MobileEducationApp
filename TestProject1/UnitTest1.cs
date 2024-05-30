using Xunit;
using C971;

namespace TestProject1
{
    public class UnitTest1
    {
        [Theory]
        [InlineData("admin", "password", true)]
        [InlineData("admin", "wrongpassword", false)]
        [InlineData("wronguser", "password", false)]
        [InlineData("wronguser", "wrongpassword", false)]
        public void IsValidUser_ShouldReturnExpectedResult(string username, string password, bool expected)
        {
            
            var loginPage = new LoginPage();

            
            var result = loginPage.IsValidUser(username, password);

            
            Assert.Equal(expected, result);
        }
    }
}
