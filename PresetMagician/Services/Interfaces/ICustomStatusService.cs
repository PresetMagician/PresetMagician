using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluent;
using Orchestra.Services;

namespace PresetMagician.Services.Interfaces
{
    public interface ICustomStatusService: IStatusService
    {
        void Initialize(StatusBarItem statusBarItem);
    }
}
