using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace AntDesign.Docs.Localization
{
    public class InAssemblyLanguageService : ILanguageService
    {
        private readonly Assembly _resourcesAssembly;
        private readonly string _resourceDirectionary;
        private Resources _resources;

        public InAssemblyLanguageService(Assembly assembly, CultureInfo culture, string resourceDirectionary = "Resources")
        {
            _resourcesAssembly = assembly;
            SetDefaultLanguage(culture);
            _resourceDirectionary = resourceDirectionary;
        }

        public InAssemblyLanguageService(Assembly assembly, string resourceDirectionary = "Resources")
        {
            _resourcesAssembly = assembly;
            SetDefaultLanguage(CultureInfo.CurrentCulture);
            _resourceDirectionary = resourceDirectionary;
        }

        public CultureInfo CurrentCulture { get; private set; }

        public event EventHandler<CultureInfo> LanguageChanged;

        public string this[string key] => _resources[key];

        private void SetDefaultLanguage(CultureInfo culture)
        {
            var availableResources = _resourcesAssembly
                .GetManifestResourceNames()
                .Select(x => Regex.Match(x, $@"^.*{_resourceDirectionary.Replace('/', '.').Replace('\\', '.')}\.(.+)\.json"))
                .Where(x => x.Success)
                .Select(x => (CultureName: x.Groups[1].Value, ResourceName: x.Value))
                .ToList();

            var (_, resourceName) = availableResources.FirstOrDefault(x => x.CultureName.Equals(culture.Name, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
            {
                (_, resourceName) = availableResources.FirstOrDefault(x => x.CultureName.Equals("en-US", StringComparison.OrdinalIgnoreCase));
                culture = CultureInfo.GetCultureInfo("en-US");
            }

            CultureInfo.CurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            CurrentCulture = culture;
            _resources = GetKeysFromCulture(culture.Name, resourceName);

            if (_resources == null)
                throw new FileNotFoundException($"There is no language files existing in the Resource folder within '{_resourcesAssembly.GetName().Name}' assembly");
        }

        public void SetLanguage(CultureInfo culture)
        {
            if (!culture.Equals(CultureInfo.CurrentCulture))
            {
                CultureInfo.CurrentCulture = culture;
            }

            if (CurrentCulture == null || !CurrentCulture.Equals(culture))
            {
                CurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                string fileName = $"{_resourcesAssembly.GetName().Name}.Resources.{culture.Name}.json";

                _resources = GetKeysFromCulture(culture.Name, fileName);

                if (_resources == null)
                    throw new FileNotFoundException($"There is no language files for '{culture.Name}' existing in the Resources folder within '{_resourcesAssembly.GetName().Name}' assembly");

                LanguageChanged?.Invoke(this, culture);
            }
        }

        private Resources GetKeysFromCulture(string culture, string fileName)
        {
            try
            {
                // Read the file
                using var fileStream = _resourcesAssembly.GetManifestResourceStream(fileName);
                if (fileStream == null) return null;
                using var streamReader = new StreamReader(fileStream);
                var content = streamReader.ReadToEnd();
                return new Resources(content);
            }
            catch (System.Exception e)
            {
                return null;
            }
        }
    }
}
