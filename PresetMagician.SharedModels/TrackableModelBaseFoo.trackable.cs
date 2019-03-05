using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Catel.Data;
using TrackableEntities;
using TrackableEntities.Client;

namespace SharedModels
{
  
        public abstract partial class TrackableModelBaseFoo : ITrackable, IIdentifiable
        {
            #region ITrackable / IIdentifiable

       
        
        /// <summary>
        /// Generate entity identifier used for correlation with MergeChanges (if not yet done)
        /// </summary>
        public virtual void SetEntityIdentifier()
        {
            if (EntityIdentifier == Guid.Empty)
                EntityIdentifier = Guid.NewGuid();
        }

        /// <summary>
        /// Copy entity identifier used for correlation with MergeChanges from another entity
        /// </summary>
        /// <param name="other">Other trackable object</param>
        public virtual void SetEntityIdentifier(IIdentifiable other)
        {
            if (other is EntityBase otherEntity)
                EntityIdentifier = otherEntity.EntityIdentifier;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same
        /// type. The comparison is based on EntityIdentifier.
        /// 
        /// If the local EntityIdentifier is empty, then return false.
        /// </summary>
        /// <param name="other">An object to compare with this object</param>
        /// <returns></returns>
        public virtual bool IsEquatable(IIdentifiable other)
        {
            if (EntityIdentifier == default)
                return false;

            if (!(other is EntityBase otherEntity))
                return false;

            return EntityIdentifier.Equals(otherEntity.EntityIdentifier);
        }

        bool IEquatable<IIdentifiable>.Equals(IIdentifiable other)
        {
            return IsEquatable(other);
        }

        /// <summary>
        /// Change-tracking state of an entity.
        /// </summary>
        [NotMapped]
        public virtual TrackingState TrackingState { get; set; }

        /// <summary>
        /// Properties on an entity that have been modified.
        /// </summary>
        [NotMapped]
        public virtual ICollection<string> ModifiedProperties { get; set; } = new HashSet<string>();

        /// <summary>
        /// Identifier used for correlation with MergeChanges.
        /// </summary>
        [NotMapped]
        public virtual Guid EntityIdentifier { get; set; }

        #endregion
    }
}