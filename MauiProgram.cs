using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Microcharts.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Trackit
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseSkiaSharp();

            //Task.Run(async () => await App.Database.AddTestDataAsync()).Wait();

            return builder.Build();

        }
    }
}
