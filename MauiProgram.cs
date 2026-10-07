using Microsoft.Extensions.Logging;

namespace clock;

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
			});

		builder.Services.AddMauiBlazorWebView();
        builder.Services.AddScoped<Services.ClockService>();
        builder.Services.AddScoped<Services.AlarmService>();
        builder.Services.AddScoped<Services.TimerService>();
        builder.Services.AddScoped(provider => new Services.TimeStateStore(
            Path.Combine(FileSystem.Current.AppDataDirectory, "time-state.json"),
            provider.GetRequiredService<Services.AlarmService>(),
            provider.GetRequiredService<Services.TimerService>()));
        builder.Services.AddScoped<Services.StopwatchService>();
        builder.Services.AddScoped<Services.Audio.WavBase64Decoder>();
        builder.Services.AddScoped<Services.Audio.BundledAudioService>();
        builder.Services.AddScoped<Services.Audio.AlertAudioService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
