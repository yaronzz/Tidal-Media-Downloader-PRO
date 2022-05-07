using Stylet;
using StyletIoC;
using TIDALDL_UI.Pages;

namespace TIDALDL_UI
{
    public class Bootstrapper : Bootstrapper<LoginViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
        }
    }
}
