using Microsoft.Extensions.Logging;
using KuranMealApp.Services;
using KuranMealApp.ViewModels;
using KuranMealApp.Views;

namespace KuranMealApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		KuranMealApp.Services.CrashLogger.Initialize();
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("OpenDyslexic-Regular.otf", "OpenDyslexic");
				fonts.AddFont("Amiri-Regular.ttf", "ArabicMushaf");
				fonts.AddFont("Inter-Regular.ttf", "InterRegular");
				fonts.AddFont("Lora-Regular.ttf", "LoraRegular");
				fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register Services
		builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
		builder.Services.AddSingleton<ISettingsService, SettingsService>();

		// Register ViewModels
		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddTransient<AyetlerViewModel>();
		builder.Services.AddTransient<AyarlarViewModel>();
		builder.Services.AddTransient<AramaViewModel>();
		builder.Services.AddTransient<FihristViewModel>();

		// Register Views
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddTransient<AyetlerPage>();
		builder.Services.AddTransient<AramaPage>();
		builder.Services.AddTransient<FihristPage>();
		builder.Services.AddTransient<AyarlarPage>();

		return builder.Build();
	}
}
