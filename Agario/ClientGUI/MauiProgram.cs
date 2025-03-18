/// <summary>
/// Author:    [Thu Ha]
/// Partner:   None
/// Date:      [04/20/2024]
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and [Thu Ha] - This work may not 
///            be copied for use in Academic Coursework.
///
/// I, [Thu Ha], certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source.  All 
/// references used in the completion of the assignments are cited 
/// in my README file.
///
/// File Contents (README file)
/// </summary>
using Microsoft.Extensions.Logging;
using Logger;
namespace ClientGUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .Services.AddLogging(configure =>
                {
                    configure.AddDebug();
                    configure.SetMinimumLevel(LogLevel.Debug);
                    configure.AddProvider(new CustomFileLoggerProvider());
                })
            .AddTransient<MainPage>();

            return builder.Build();
        }
    }
}