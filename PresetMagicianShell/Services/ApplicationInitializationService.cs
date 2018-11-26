// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationInitializationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PresetMagicianShell.Services
{
    using Catel;
    using Catel.IoC;
    using Catel.Logging;
    using Catel.MVVM;
    using Orchestra.Services;
    using System.Threading.Tasks;

    public class ApplicationInitializationService : ApplicationInitializationServiceBase
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly IServiceLocator _serviceLocator;
        private readonly ICommandManager _commandManager;
        private readonly ITypeFactory _typeFactory;

        #endregion Fields

        #region Constructors

        public ApplicationInitializationService(ITypeFactory typeFactory, IServiceLocator serviceLocator, ICommandManager commandManager)
        {
            Argument.IsNotNull(() => typeFactory);
            Argument.IsNotNull(() => serviceLocator);
            Argument.IsNotNull(() => commandManager);

            _typeFactory = typeFactory;
            _serviceLocator = serviceLocator;
            _commandManager = commandManager;
        }

        #endregion Constructors

        #region Methods

        public override async Task InitializeBeforeCreatingShellAsync()
        {
            // Non-async first
            RegisterTypes();
            InitializeCommands();
        }

        public override async Task InitializeAfterCreatingShellAsync()
        {
            await base.InitializeAfterCreatingShellAsync();
        }

        private void InitializeCommands()
        {
            Log.Info("Initializing commands");
        }

        private void RegisterTypes()
        {
            var serviceLocator = ServiceLocator.Default;
        }

        #endregion Methods
    }
}