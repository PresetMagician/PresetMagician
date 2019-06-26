using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Catel.Reflection;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Services;
using PresetMagician.Legacy.Models;
using PresetMagician.Legacy.Services.EventArgs;
using NewModels = PresetMagician.Core.Models;
using Type = PresetMagician.Legacy.Models.Type;

namespace PresetMagician.Legacy.Services
{
    public class Ef6MigrationService
    {
        private ApplicationDatabaseContext _dbContext;
        private readonly DataPersisterService _dataPersister;
        private readonly PresetDataPersisterService _presetDataPersister;
        public List<Plugin> OldPlugins;
        public List<Mode> OldModes;
        public List<Type> OldTypes;
        public List<Preset> OldPresets;
        private int CurrentPlugin;
        private int UpdateCounter;

        public event EventHandler<MigrationProgessEventArgs> MigrationProgressUpdated;

        public Ef6MigrationService(DataPersisterService dataPersister, PresetDataPersisterService presetDataPersister)
        {
            _dataPersister = dataPersister;
            _presetDataPersister = presetDataPersister;
        }

        public void LoadData()
        {
            _dbContext = new ApplicationDatabaseContext();
            _dbContext.Migrate();
            OldPlugins = _dbContext.Plugins.Include(plugin => plugin.AdditionalBankFiles)
                .Include(plugin => plugin.PluginLocation).ToList();
            OldModes = _dbContext.Modes.ToList();
            OldTypes = _dbContext.Types.ToList();
            OldPresets = _dbContext.Presets.Include(p => p.Modes)
                .Include(p => p.Types).ToList();
        }

        public void MigratePlugins()
        {
            foreach (var plugin in OldPlugins)
            {
                CurrentPlugin = OldPlugins.IndexOf(plugin) + 1;
                var newPlugin = MigratePlugin(plugin);
            }


            _dbContext.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.Move(ApplicationDatabaseContext.DefaultDatabasePath,
                ApplicationDatabaseContext.DefaultDatabasePath + ".old");
        }

        public NewModels.Plugin MigratePlugin(Plugin oldPlugin)
        {
            _presetDataPersister.OpenDatabase().Wait();
            var newPlugin = new NewModels.Plugin();

            if (oldPlugin.PluginLocation != null)
            {
                newPlugin.PluginLocation = new NewModels.PluginLocation();
                MigratePluginLocation(oldPlugin, oldPlugin.PluginLocation, newPlugin.PluginLocation);
            }

            foreach (var preset in oldPlugin.Presets)
            {
                var data = _dbContext.GetPresetData(preset);

                if (data == null)
                {
                    // The preset data is null for some reason, skip this one
                    continue;
                }

                UpdateCounter++;
                var currentPreset = oldPlugin.Presets.IndexOf(preset) + 1;

                if (UpdateCounter > 111)
                {
                    MigrationProgressUpdated?.Invoke(this,
                        new MigrationProgessEventArgs(
                            $"({CurrentPlugin}/{OldPlugins.Count}) {oldPlugin.PluginName} Preset {currentPreset} / {oldPlugin.Presets.Count}"));
                    UpdateCounter = 0;
                }

                var newPreset = new NewModels.Preset();
                newPreset.Plugin = newPlugin;
                newPlugin.Presets.Add(newPreset);

                MigratePreset(preset, newPreset);


                newPreset.Plugin = newPlugin;
                newPreset.OriginalMetadata.Plugin = newPlugin;
                _presetDataPersister.PersistPreset(newPreset.OriginalMetadata, data, true).Wait();
            }


            var propertiesToMigrate = new HashSet<string>
            {
                nameof(Plugin.PluginType),
                nameof(Plugin.PluginName),
                nameof(Plugin.LastKnownGoodDllPath),
                nameof(Plugin.PluginVendor),
                nameof(Plugin.VstPluginId),
                nameof(Plugin.IsEnabled),
                nameof(Plugin.IsReported),
                nameof(Plugin.IsSupported),
                nameof(Plugin.DontReport),
                nameof(Plugin.DefaultControllerAssignments),
            };

            foreach (var propertyName in propertiesToMigrate)
            {
                var oldValue = PropertyHelper.GetPropertyValue(oldPlugin, propertyName);
                PropertyHelper.SetPropertyValue(newPlugin, propertyName, oldValue);
            }

            MigrateModes(oldPlugin.DefaultModes, newPlugin.DefaultCharacteristics);
            MigrateTypes(oldPlugin.DefaultTypes, newPlugin.DefaultTypes);

            if (oldPlugin.PluginInfo != null)
            {
                newPlugin.PluginInfo = new NewModels.VstPluginInfoSurrogate();
            }

            MigratePluginInfo(oldPlugin.PluginInfo, newPlugin.PluginInfo);
            MigratePluginCapabilities(oldPlugin.PluginCapabilities, newPlugin.PluginCapabilities);
            MigrateBankFiles(oldPlugin.AdditionalBankFiles, newPlugin.AdditionalBankFiles);

            _dataPersister.SavePlugin(newPlugin);
            _dataPersister.SavePresetsForPlugin(newPlugin);
            _presetDataPersister.CloseDatabase().Wait();
            return newPlugin;
        }

        private void MigratePluginCapabilities(IList<PluginInfoItem> oldPluginCapabilities,
            List<NewModels.PluginInfoItem> newPluginCapabilities)
        {
            foreach (var cap in oldPluginCapabilities)
            {
                newPluginCapabilities.Add(new NewModels.PluginInfoItem
                    {Name = cap.Name, Value = cap.Value, Category = cap.Category});
            }
        }

        private void MigratePluginInfo(VstPluginInfoSurrogate oldPluginInfo,
            NewModels.VstPluginInfoSurrogate newPluginInfo)
        {
            if (oldPluginInfo == null)
            {
                return;
            }

            var propertiesToMigrate = new HashSet<string>
            {
                nameof(VstPluginInfoSurrogate.Flags),
                nameof(VstPluginInfoSurrogate.InitialDelay),
                nameof(VstPluginInfoSurrogate.ProgramCount),
                nameof(VstPluginInfoSurrogate.PluginVersion),
                nameof(VstPluginInfoSurrogate.ParameterCount),
                nameof(VstPluginInfoSurrogate.PluginID),
                nameof(VstPluginInfoSurrogate.AudioInputCount),
                nameof(VstPluginInfoSurrogate.AudioOutputCount),
            };

            foreach (var propertyName in propertiesToMigrate)
            {
                var oldValue = PropertyHelper.GetPropertyValue(oldPluginInfo, propertyName);
                PropertyHelper.SetPropertyValue(newPluginInfo, propertyName, oldValue);
            }
        }

        private void MigratePluginLocation(Plugin oldPlugin, PluginLocation oldLocation,
            NewModels.PluginLocation newLocation)
        {
            var propertiesToMigrate = new HashSet<string>
            {
                nameof(PluginLocation.DllHash),
                nameof(PluginLocation.DllPath),
                nameof(PluginLocation.PluginName),
                nameof(PluginLocation.PluginName),
                nameof(PluginLocation.PluginVendor),
                nameof(PluginLocation.PluginProduct),
                nameof(PluginLocation.VstPluginId),
                nameof(PluginLocation.VendorVersion),
                nameof(PluginLocation.LastModifiedDateTime),
            };

            foreach (var propertyName in propertiesToMigrate)
            {
                var oldValue = PropertyHelper.GetPropertyValue(oldLocation, propertyName);
                PropertyHelper.SetPropertyValue(newLocation, propertyName, oldValue);
            }

            newLocation.HasMetadata = oldPlugin.HasMetadata;
        }

        private void MigrateModes(ICollection<Mode> oldModes,
            ICollection<NewModels.Characteristic> newModes)
        {
            foreach (var mode in oldModes)
            {
                newModes.Add(new NewModels.Characteristic {CharacteristicName = mode.Name});
            }
        }

        private void MigrateBankFiles(ICollection<BankFile> oldBankFiles,
            EditableCollection<NewModels.BankFile> newBankFiles)
        {
            foreach (var oldBankFile in oldBankFiles)
            {
                newBankFiles.Add(new NewModels.BankFile
                {
                    Path = oldBankFile.Path, BankName = oldBankFile.BankName, ProgramRange = oldBankFile.ProgramRange
                });
            }
        }

        private void MigrateTypes(ICollection<Type> oldTypes,
            ICollection<NewModels.Type> newTypes)
        {
            foreach (var type in oldTypes)
            {
                newTypes.Add(new NewModels.Type {TypeName = type.Name, SubTypeName = type.SubTypeName});
            }
        }

        private void MigratePreset(Preset oldPreset, NewModels.Preset newPreset)
        {
            var propertiesToMigrate = new HashSet<string>
            {
                nameof(Preset.PresetId),
                nameof(Preset.PresetHash),
                nameof(Preset.LastExported),
                nameof(Preset.PresetCompressedSize),
                nameof(Preset.PresetSize),
            };

            var metadataPropertiesToMigrate = new HashSet<string>
            {
                nameof(Preset.Author),
                nameof(Preset.Comment),
                nameof(Preset.BankPath),
                nameof(Preset.SourceFile),
                nameof(Preset.PresetName)
            };

            foreach (var propertyName in propertiesToMigrate)
            {
                var oldValue = PropertyHelper.GetPropertyValue(oldPreset, propertyName);
                PropertyHelper.SetPropertyValue(newPreset, propertyName, oldValue);
            }

            foreach (var propertyName in metadataPropertiesToMigrate)
            {
                var oldValue = PropertyHelper.GetPropertyValue(oldPreset, propertyName);
                PropertyHelper.SetPropertyValue(newPreset.OriginalMetadata, propertyName, oldValue);
            }

            MigrateModes(oldPreset.Modes, newPreset.OriginalMetadata.Characteristics);
            MigrateTypes(oldPreset.Types, newPreset.OriginalMetadata.Types);

            newPreset.SetFromPresetParser(newPreset.OriginalMetadata);
        }
    }
}