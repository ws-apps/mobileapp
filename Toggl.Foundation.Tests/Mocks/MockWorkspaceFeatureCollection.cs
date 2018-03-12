﻿using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Mocks
{
    public sealed class MockWorkspaceFeatureCollection : IDatabaseWorkspaceFeatureCollection
    {
        public IDatabaseWorkspace Workspace { get; set; }

        public IEnumerable<IDatabaseWorkspaceFeature> DatabaseFeatures { get; set; }

        public long WorkspaceId { get; set; }

        public IEnumerable<IWorkspaceFeature> Features { get; set; }

        public bool IsEnabled(WorkspaceFeatureId feature)
            => Features.Any(f => f.FeatureId == feature && f.Enabled);
    }
}
