﻿using System;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Models
{
    internal partial class User
    {
        public User(IDatabaseUser user, long workspaceId)
            : this(user, SyncStatus.SyncNeeded, null)
        {
            DefaultWorkspaceId = workspaceId;
        }

        public sealed class Builder
        {
            public static Builder FromExisting(IDatabaseUser user)
             => new Builder(user);

            public string ApiToken { get; private set; }
            public long DefaultWorkspaceId { get; private set; }
            public Email Email { get; private set; }
            public string Fullname { get; private set; }
            public BeginningOfWeek BeginningOfWeek { get; private set; }
            public string Language { get; private set; }
            public string ImageUrl { get; private set; }
            public DateTimeOffset At { get; private set; }

            public Builder(IDatabaseUser user)
            {
                ApiToken = user.ApiToken;
                DefaultWorkspaceId = user.DefaultWorkspaceId;
                Email = user.Email;
                Fullname = user.Fullname;
                BeginningOfWeek = user.BeginningOfWeek;
                Language = user.Language;
                ImageUrl = user.ImageUrl;
                At = user.At;
            }

            public Builder SetBeginningOfWeek(BeginningOfWeek beginningOfWeek)
            {
                BeginningOfWeek = beginningOfWeek;
                return this;
            }

            public User Build()
            {
                ensureValidity();
                return new User(this);
            }

            private void ensureValidity()
            {
                if (Enum.IsDefined(typeof(BeginningOfWeek), BeginningOfWeek) == false)
                    throw new InvalidOperationException($"You need to set a valid value to the {nameof(BeginningOfWeek)} property before building user.");

                if (DefaultWorkspaceId == 0)
                    throw new InvalidOperationException($"{nameof(DefaultWorkspaceId)} must be specified before building user.");

                if (!Email.IsValid)
                    throw new InvalidOperationException($"{nameof(Email)} must be valid before building user.");

                if (string.IsNullOrEmpty(Fullname))
                    throw new InvalidOperationException($"{nameof(Fullname)} must be specified before building user.");
            }
        }

        private User(Builder builder)
        {
            ApiToken = builder.ApiToken;
            DefaultWorkspaceId = builder.DefaultWorkspaceId;
            Email = builder.Email;
            Fullname = builder.Fullname;
            BeginningOfWeek = builder.BeginningOfWeek;
            Language = builder.Language;
            ImageUrl = builder.ImageUrl;
            At = builder.At;
        }
    }

    internal static class UserExtensions
    {
        public static User With(this IDatabaseUser self, long workspaceId) => new User(self, workspaceId);
    }
}
