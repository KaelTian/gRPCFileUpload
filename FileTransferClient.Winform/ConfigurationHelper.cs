using FileTransferClient.Winform.Configs;
using Microsoft.Extensions.Configuration;

namespace FileTransferClient.Winform
{
    public static class ConfigurationHelper
    {
        private static IConfigurationRoot? _configuration;
        private static FileSystemWatcher? _watcher;
        private static AppConfig? _appConfig;

        public static event Action? OnConfigurationChanged;

        static ConfigurationHelper()
        {
            LoadConfiguration();

            // 开启监听配置文件变更
            WatchForChanges();
            //_watcher = new FileSystemWatcher(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            //_watcher.NotifyFilter = NotifyFilters.LastWrite;
            //_watcher.Changed += (sender, e) =>
            //{
            //    // Reload configuration when the file changes
            //    Thread.Sleep(100); // Wait for the file to be fully written
            //    LoadConfiguration();
            //    OnConfigurationChanged?.Invoke();
            //};
        }

        private static void WatchForChanges()
        {
            if (_configuration == null) return;
            var changeToken = _configuration.GetReloadToken();

            // 监听配置文件变化
            changeToken.RegisterChangeCallback(state =>
            {
                // Reload configuration when the file changes
                Thread.Sleep(100); // Wait for the file to be fully written
                LoadConfiguration();
                OnConfigurationChanged?.Invoke();

                // 继续监听下次配置文件变化
                WatchForChanges();
            }, null);
        }

        private static void LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
            _appConfig = new AppConfig();
            _configuration.Bind(_appConfig);
        }

        public static T GetSection<T>(string sectionName) where T : class, new()
        {
            var section = new T();
            _configuration?.GetSection(sectionName).Bind(section);
            return section;
        }

        public static AppConfig? GetConfig() => _appConfig;

        public static GrpcServiceSettings? GetGrpcServiceSettings(string serviceName)
        {
            if (_appConfig == null) return null;
            if (_appConfig.GrpcSettings.Services.TryGetValue(serviceName, out var settings))
            {
                return settings;
            }
            throw new KeyNotFoundException($"gRPC service configuration for '{serviceName}' not found.");
        }

        public static GrpcGlobalSettings? GetGrpcGlobalSettings() => _appConfig?.GrpcSettings?.GlobalSettings;

        public static ApplicationSettings? GetApplicationSettings() => _appConfig?.Application;
    }
}
