using System.Collections.Generic;
using System.Linq;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Services
{
    public class TypesService
    {
         private readonly GlobalService _globalService;

        public TypesService(GlobalService globalService)
        {
            _globalService = globalService;
            TypeUsages = new WrappedEditableCollection<TypeUsage, Type>(_globalService.GlobalTypes);
        }

        public void UpdateTypesUsages()
        {
            var plugins = _globalService.Plugins;

            foreach (var c in TypeUsages)
            {
                c.Plugins.Clear();
                c.UsageCount = 0;
            }
            
            foreach (var plugin in plugins)
            {
                foreach (var preset in plugin.Presets)
                {
                    foreach (var type in preset.Metadata.Types)
                    {
                        var item = TypeUsages.GetFromOriginal(type);
                        item.UsageCount++;
                        item.Plugins.Add(plugin);
                    }
                }
            }
        }

        public List<Type> GetRedirectSources(Type type)
        {
            return (from t in _globalService.GlobalTypes
                where t.RedirectType == type
                orderby t.FullTypeName
                select t).ToList();
        }
        
        public bool IsRedirectTarget(Type type)
        {
            return (from t in _globalService.GlobalTypes
                where t.RedirectType == type
                select t).Any();
        }

        public List<Type> GetRedirectTargets(Type type)
        {
            return (from t in _globalService.GlobalTypes
                where !t.IsRedirect && !t.IsIgnored && t != type
                orderby t.FullTypeName
                select t).ToList();
        }

        public bool HasType(Type type )
        {
            return (from t in _globalService.GlobalTypes
                where t != type && t.TypeName== type.TypeName && t.SubTypeName == type.SubTypeName
                select t).Any();
        }

       

        public TypeUsage GetTypeUsageByType(Type type)
        {
            return (from c in TypeUsages where c.Type == type select c).SingleOrDefault();
        }

        public WrappedEditableCollection<TypeUsage,Type> TypeUsages { get; }
    }
}