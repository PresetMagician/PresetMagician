using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using SharedModels;

namespace PresetMagician.Services
{
    public interface IDatabaseService
    {
        ApplicationDatabaseContext Context { get; }
        IPresetDataStorer GetPresetDataStorer();
    }

    public class DatabaseService: IDatabaseService
    {
        private readonly ApplicationDatabaseContext _dbContext;
        
        public DatabaseService()
        {
            _dbContext = ApplicationDatabaseContext.Create();
        }

        public ApplicationDatabaseContext Context
        {
            get { return _dbContext; }
        }

        public IPresetDataStorer GetPresetDataStorer()
        {
            return _dbContext;
        }
    }
}