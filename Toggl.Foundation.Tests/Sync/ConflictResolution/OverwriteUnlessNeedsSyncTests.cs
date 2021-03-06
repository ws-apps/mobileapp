﻿using System;
using Xunit;
using FluentAssertions;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Tests.Sync.ConflictResolution
{
    public sealed class OverwriteUnlessNeedsSyncTests
    {
        [Fact, LogIfTooSlow]
        public void ThrowsWhenIncomingEntityIsNull()
        {
            var existingEntity = new TestModel();

            Action resolving = () => resolver.Resolve(null, null);
            Action resolvingWithExistingLocalEntity = () => resolver.Resolve(existingEntity, null);

            resolving.ShouldThrow<ArgumentNullException>();
            resolvingWithExistingLocalEntity.ShouldThrow<ArgumentNullException>();
        }

        [Fact, LogIfTooSlow]
        public void IgnoreWhenThereIsAnExistingEntityLocallyWhichNeedsSync()
        {
            var existingEntity = new TestModel();
            existingEntity.SyncStatus = SyncStatus.SyncNeeded;
            var incomingEntity = new TestModel();

            var mode = resolver.Resolve(existingEntity, incomingEntity);

            mode.Should().Be(ConflictResolutionMode.Ignore);
        }

        [Fact, LogIfTooSlow]
        public void UpdateWhenThereIsAnExistingEntityLocally()
        {
            var existingEntity = new TestModel();
            var incomingEntity = new TestModel();

            var mode = resolver.Resolve(existingEntity, incomingEntity);

            mode.Should().Be(ConflictResolutionMode.Update);
        }

        [Fact, LogIfTooSlow]
        public void CreateNewWhenThereIsNoExistingEntity()
        {
            var incomingEntity = new TestModel();

            var mode = resolver.Resolve(null, incomingEntity);

            mode.Should().Be(ConflictResolutionMode.Create);
        }

        private sealed class TestModel : IDatabaseSyncable
        {
            public SyncStatus SyncStatus { get; set; }

            public string LastSyncErrorMessage { get; }

            public bool IsDeleted { get; }
        }

        private OverwriteUnlessNeedsSync<TestModel> resolver { get; }
            = new OverwriteUnlessNeedsSync<TestModel>();
    }
}
