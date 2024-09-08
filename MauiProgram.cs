using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace Trackit
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit(); // Add this line
            return builder.Build();
        }
    }
}
