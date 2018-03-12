using Toggl.Multivac;
using Toggl.PrimeRadiant;
using static Toggl.PrimeRadiant.ConflictResolutionMode;

namespace Toggl.Foundation.Sync.ConflictResolution
{
    public class OverwriteUnlessNeedsSync<T> : IConflictResolver<T>
        where T : class, IDatabaseSyncable
    {
        public ConflictResolutionMode Resolve(T localEntity, T serverEntity)
        {
            Ensure.Argument.IsNotNull(serverEntity, nameof(serverEntity));

            if (localEntity == null)
                return Create;

            if (localEntity.SyncStatus == SyncStatus.SyncNeeded)
                return Ignore;

            return Update;
        }
    }
}
