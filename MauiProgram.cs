using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using OxyPlot.Maui.Skia;
using SkiaSharp.Views.Maui.Handlers;
using SkiaSharp.Views.Maui.Controls;
using Trackit.Screens;
using Trackit.ViewModels;

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
                .UseOxyPlotSkia()
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddHandler(typeof(SKCanvasView), typeof(SKCanvasViewHandler));
                });

            builder.Services.AddTransient<Export>();
            builder.Services.AddTransient<ExportViewModel>();

            //Task.Run(async () => await App.Database.AddTestDataAsync()).Wait();

            return builder.Build();

        }
    }
}
