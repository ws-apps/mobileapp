﻿using Realms;
using System.Linq;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;
using Toggl.Multivac.Models;

namespace Toggl.PrimeRadiant.Realm
{
    internal partial class RealmWorkspaceFeature : RealmObject, IDatabaseWorkspaceFeature
    {
        [Ignored]
        public WorkspaceFeatureId FeatureId
        {
            get => (WorkspaceFeatureId)FeatureIdInt;
            set => FeatureIdInt = (int)value;
        }

        public int FeatureIdInt { get; set; }
        
        public bool Enabled { get; set; }

        public RealmWorkspaceFeature(IWorkspaceFeature entity)
        {
            FeatureId = entity.FeatureId;
            Enabled = entity.Enabled;
        }

        public static RealmWorkspaceFeature FindOrCreate(IWorkspaceFeature entity, Realms.Realm realm)
            => find(entity, realm) ?? create(entity, realm);

        private static RealmWorkspaceFeature find(IWorkspaceFeature entity, Realms.Realm realm)
            => realm.All<RealmWorkspaceFeature>()
                .SingleOrDefault(x => x.FeatureIdInt == (int)entity.FeatureId && x.Enabled == entity.Enabled);

        private static RealmWorkspaceFeature create(IWorkspaceFeature entity, Realms.Realm realm)
            => realm.Add(new RealmWorkspaceFeature(entity));
    }
}
