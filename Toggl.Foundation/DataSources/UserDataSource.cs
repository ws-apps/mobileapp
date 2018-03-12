using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class UserDataSource : IUserSource
    {
        private readonly ISingleObjectStorage<IDatabaseUser> storage;

        public UserDataSource(ISingleObjectStorage<IDatabaseUser> storage)
        {
            Ensure.Argument.IsNotNull(storage, nameof(storage));

            this.storage = storage;
        }

        public IObservable<IDatabaseUser> Current
            => storage.Single().Select(User.From);

        public IObservable<IDatabaseUser> UpdateWorkspace(long workspaceId)
            => storage
                .Single()
                .Select(user => user.With(workspaceId))
                .SelectMany(storage.Update);

        public IObservable<IDatabaseUser> Update(EditUserDTO dto)
            => storage
                .Single()
                .Select(user => updatedUser(user, dto))
                .SelectMany(storage.Update)
                .Select(User.From);

        public IDatabaseUser updatedUser(IDatabaseUser existing, EditUserDTO dto)
            => User.Builder
                   .FromExisting(existing)
                   .SetBeginningOfWeek(dto.BeginningOfWeek)
                   .Build();
    }
}
