using System;
using Microsoft.Extensions.Configuration;

namespace ProAgil.Domain
{
    public abstract class ConfigSettings
    {
        public IConfigurationRoot Configuration { get; private set; } = null!;

        protected void LoadConfiguration(string file)
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(file, optional: true, reloadOnChange: false);
            string? netCoreEnv = EnvironmentVariableExtensions.GetNetCoreEnv();
            if (netCoreEnv != null && !string.IsNullOrWhiteSpace(netCoreEnv))
            {
                int startIndex = file.LastIndexOf(".json");
                string path = file.Insert(startIndex, "." + netCoreEnv);
                configurationBuilder = configurationBuilder.AddJsonFile(
                    path,
                    optional: true,
                    reloadOnChange: false
                );
            }

            Configuration = configurationBuilder.Build();
        }

        public string? GetValue(string key)
        {
            return Configuration[key];
        }
    }

    public sealed class ConfigAppSettings : ConfigSettings
    {
        private static ConfigAppSettings? instance = null;

        private static readonly object padlock = new object();

        public static ConfigAppSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new ConfigAppSettings();
                        }
                    }
                }

                return instance;
            }
        }

        public ConfigAppSettings()
        {
            LoadConfiguration("appsettings.json");
        }
    }

    public static class EnvironmentVariableExtensions
    {
        public static T? GetEnvironmentVariable<T>(
            string environmentKey,
            string configFileKey,
            IConfigurationRoot? configuration = null
        )
        {
            string? text = null;
            if (configuration == null)
            {
                configuration = ConfigAppSettings.Instance.Configuration;
            }

            if (!string.IsNullOrEmpty(environmentKey))
            {
                text = Environment.GetEnvironmentVariable(environmentKey);
            }

            if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(configFileKey))
            {
                text = configuration[configFileKey];
            }

            if (text == null)
            {
                return default;
            }

            return text.ConvertValue<T>();
        }

        public static T? GetEnvironmentByConfigFile<T>(
            string configFileKey,
            IConfigurationRoot? configuration = null
        )
        {
            string? text = null;
            if (configuration == null)
            {
                configuration = ConfigAppSettings.Instance.Configuration;
            }

            if (!string.IsNullOrEmpty(configFileKey))
            {
                text = configuration[configFileKey];
            }

            if (text == null)
            {
                return default;
            }

            return text.ConvertValue<T>();
        }

        public static T? GetEnvironmentVariable<T>(string environmentKey)
        {
            string? text = null;
            if (!string.IsNullOrEmpty(environmentKey))
            {
                text = Environment.GetEnvironmentVariable(environmentKey);
            }

            if (text == null)
            {
                return default;
            }

            return text.ConvertValue<T>();
        }

        public static bool IsProduction()
        {
            if (!string.Equals(GetNetCoreEnv(), "PRODUCTION", StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(GetNetCoreEnv(), "PROD", StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        public static string? GetNetCoreEnv()
        {
            string? environmentVariable = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            string? environmentVariable2 = Environment.GetEnvironmentVariable(
                "ASPNETCORE_ENVIRONMENT"
            );
            if (!string.IsNullOrEmpty(environmentVariable))
            {
                return environmentVariable;
            }

            return environmentVariable2;
        }
    }

    internal static class ConvertExtensions
    {
        public static T? ConvertValue<T>(this string value)
        {
            return value.ConvertValue<string, T>();
        }

        public static TDest? ConvertValue<TOrign, TDest>(this TOrign value)
        {
            try
            {
                return (TDest?)Convert.ChangeType(value, typeof(TDest));
            }
            catch
            {
                return default;
            }
        }
    }
}
