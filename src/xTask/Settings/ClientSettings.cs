﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Settings
{
    using System.Collections.Generic;
    using Services;
    using Systems.File;
    using Systems.Configuration;

    public class ClientSettings : IClientSettings
    {
        private Dictionary<SettingsLocation, IClientSettingsView> _settingsViews = new Dictionary<SettingsLocation, IClientSettingsView>();
        private List<SettingsLocation> _locationPriority = new List<SettingsLocation>();
        private IConfigurationManager _configurationManager;
        private IFileService _fileService;

        private ClientSettings(string settingsSection, IConfigurationManager configurationManager, IFileService fileService)
        {
            SettingsSection = settingsSection;
            _configurationManager = configurationManager;
            _fileService = fileService;
        }

        public string SettingsSection { get; private set; }

        public static ClientSettings Create(string settingsSection, IConfigurationManager configurationManager, IFileService fileService)
        {
            ClientSettings settings = new ClientSettings(settingsSection, configurationManager, fileService);
            settings.Initialize();
            return settings;
        }

        private void Initialize()
        {
            AddViews(SettingsLocation.ContainingExecutable, SettingsLocation.RunningExecutable, SettingsLocation.Roaming, SettingsLocation.Local);
        }

        private void AddViews(params SettingsLocation[] locations)
        {
            foreach (SettingsLocation location in locations)
            {
                IClientSettingsView view = ClientSettingsView.Create(SettingsSection, location, _configurationManager, _fileService);
                if (view != null)
                {
                    _settingsViews.Add(location, ClientSettingsView.Create(SettingsSection, location, _configurationManager, _fileService));
                    _locationPriority.Add(location);
                }
            }
        }

        public bool SaveSetting(SettingsLocation location, string name, string value)
        {
            if (!_settingsViews.ContainsKey(location))
            {
                return false;
            }

            return _settingsViews[location].SaveSetting(name, value);
        }

        public bool RemoveSetting(SettingsLocation location, string name)
        {
            if (!_settingsViews.ContainsKey(location))
            {
                return false;
            }

            return _settingsViews[location].RemoveSetting(name);
        }

        public string GetSetting(string name)
        {
            string value = null;
            foreach (SettingsLocation location in _locationPriority)
            {
                value = _settingsViews[location].GetSetting(name) ?? value;
            }
            return value;
        }

        public IEnumerable<ClientSetting> GetAllSettings()
        {
            List<ClientSetting> settings = new List<ClientSetting>();
            foreach (IClientSettingsView view in _settingsViews.Values)
            {
                settings.AddRange(view.GetAllSettings());
            }

            return settings;
        }

        public string GetConfigurationPath(SettingsLocation location)
        {
            return GetConfigurationPath(location);
        }
    }
}
