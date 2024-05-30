using Xunit;
using Microsoft.Maui.Hosting;
using C971;

namespace TestProject1
{
    public class UnitTest2
    {
        [Fact]
        public void CreateMauiApp_ShouldBuildAppSuccessfully()
        {
            
            var app = MauiProgram.CreateMauiApp();

       
            Assert.NotNull(app);
            Assert.IsType<MauiApp>(app);
        }
    }
}

