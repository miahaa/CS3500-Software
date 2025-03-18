﻿using LoggerLibrary;
using Microsoft.Extensions.Logging;

namespace ChatServer
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
                    configure.AddProvider(new CustomFileLoggerProvider());
                    configure.SetMinimumLevel(LogLevel.Debug);

                })
                .AddTransient<MainPage>();

            return builder.Build();
        }
    }
}
