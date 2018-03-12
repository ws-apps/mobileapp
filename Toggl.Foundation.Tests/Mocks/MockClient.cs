﻿using System;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Mocks
{
    public sealed class MockClient : IDatabaseClient
    {
        public IDatabaseWorkspace Workspace { get; set; }

        public long WorkspaceId { get; set; }

        public string Name { get; set; }

        public DateTimeOffset At { get; set; }

        public DateTimeOffset? ServerDeletedAt { get; set; }

        public long Id { get; set; }

        public SyncStatus SyncStatus { get; set; }

        public string LastSyncErrorMessage { get; set; }

        public bool IsDeleted { get; set; }
    }
}
