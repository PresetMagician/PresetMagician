using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Catel;
using Catel.Data;

namespace PresetMagician.Extensions
{
    /// <summary>
    /// IModel extensions.
    /// </summary>
    public static partial class ISuspendableModelExtensions
    {
        /// <summary>
        /// Clears the <see cref="ModelBase.IsDirty" /> on all childs.
        /// </summary>
        /// <param name="model">The model.</param>
        public static void ClearIsDirtyOnAllChildsSuspended(this IModel model)
        {
            Argument.IsNotNull("model", model);


            ClearIsDirtyOnAllChildsSuspended(model, new HashSet<IModel>());
        }

        /// <summary>
        /// Clears the <see cref="ModelBase.IsDirty"/> on all childs.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="handledReferences">The already handled references, required to prevent circular stackoverflows.</param>
        private static void ClearIsDirtyOnAllChildsSuspended(object obj, HashSet<IModel> handledReferences)
        {
            var objAsModelBase = obj as ModelBase;
            var objAsIEnumerable = obj as IEnumerable;
            if (objAsIEnumerable is string)
            {
                objAsIEnumerable = null;
            }

            if (objAsModelBase != null)
            {
                if (handledReferences.Contains(objAsModelBase))
                {
                    return;
                }
                
                
                using (objAsModelBase.SuspendChangeCallbacks()) 
                using (objAsModelBase.SuspendChangeNotifications())
                {
                    
                        ((IModel) objAsModelBase).SetValue(nameof(ModelBase.IsDirty), false);
                    

                    handledReferences.Add(objAsModelBase);

                    var catelTypeInfo = PropertyDataManager.Default.GetCatelTypeInfo(obj.GetType());
                    foreach (var property in catelTypeInfo.GetCatelProperties())
                    {
                        var value = ((IModel) objAsModelBase).GetValue(property.Value.Name);

                        ClearIsDirtyOnAllChildsSuspended(value, handledReferences);
                    }
                }
            }
            else if (objAsIEnumerable != null)
            {
                foreach (var childItem in objAsIEnumerable)
                {
                    
                        ClearIsDirtyOnAllChildsSuspended(childItem, handledReferences);
                    
                }
            }
        }
    }
}