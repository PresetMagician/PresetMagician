using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.Threading;

namespace PresetMagician
{
    public abstract class ThreadedApplicationNotBusyCommandContainer : ApplicationNotBusyCommandContainer
    {
// ServiceLocator.ResolveType<IUIVisualizerService>();
        protected ThreadedApplicationNotBusyCommandContainer(string command, ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(command, commandManager, serviceLocator)
        {
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && !_globalFrontendService.ApplicationState.IsApplicationBusy;
        }


        protected async virtual Task ExecuteThreaded(object parameter)
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            try
            {
                await TaskHelper.Run(async () => await ExecuteThreaded(parameter));
            }
            catch (Exception e)
            {
                LogTo.Error(
                    $"Error executing command {CommandName} - Got exception {e.GetType().FullName} with message {e.Message}");
                LogTo.Debug(e.StackTrace);
            }

            Debug.WriteLine("completed!");
        }
    }
}