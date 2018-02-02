using System;
using Toggl.Multivac;

namespace Toggl.PrimeRadiant.Settings
{
    public sealed class SettingsStorage : IAccessRestrictionStorage, IOnboardingStorage, IUserPreferences
    {
        private const string outdatedApiKey = "OutdatedApi";
        private const string outdatedClientKey = "OutdatedClient";
        private const string unauthorizedAccessKey = "UnauthorizedAccessForApiToken";

        private const string isNewUserKey = "IsNewUser";
        private const string lastAccessDateKey = "LastAccessDate";
        private const string completedOnboardingKey = "CompletedOnboarding";

        private const string preferManualMode = "PreferManualMode";

        private readonly Version version;
        private readonly IKeyValueStorage keyValueStorage;

        public SettingsStorage(Version version, IKeyValueStorage keyValueStorage)
        {
            Ensure.Argument.IsNotNull(keyValueStorage, nameof(keyValueStorage));

            this.version = version;
            this.keyValueStorage = keyValueStorage;
        }

        #region IAccessRestrictionStorage

        public void SetClientOutdated()
        {
            keyValueStorage.SetString(outdatedClientKey, version.ToString());
        }

        public void SetApiOutdated()
        {
            keyValueStorage.SetString(outdatedApiKey, version.ToString());
        }

        public void SetUnauthorizedAccess(string apiToken)
        {
            keyValueStorage.SetString(unauthorizedAccessKey, apiToken);
        }

        public bool IsClientOutdated()
            => isOutdated(outdatedClientKey);

        public bool IsApiOutdated()
            => isOutdated(outdatedApiKey);

        public bool IsUnauthorized(string apiToken)
            => apiToken == keyValueStorage.GetString(unauthorizedAccessKey);

        private bool isOutdated(string key)
        {
            var storedVersion = getStoredVersion(key);
            return storedVersion != null && version <= storedVersion;
        }

        private Version getStoredVersion(string key)
        {
            var stored = keyValueStorage.GetString(key);
            return stored == null ? null : Version.Parse(stored);
        }

        #endregion

        #region IOnboardingStorage

        public void SetLastOpened(DateTimeOffset date)
        {
            var dateString = date.ToString();
            keyValueStorage.SetString(lastAccessDateKey, dateString);
        }

        public void SetIsNewUser(bool isNewUser)
        {
            keyValueStorage.SetBool(isNewUserKey, isNewUser);
        }

        public void SetCompletedOnboarding()
        {
            keyValueStorage.SetBool(completedOnboardingKey, true);
        }

        public bool IsNewUser() => keyValueStorage.GetBool(isNewUserKey);

        public bool CompletedOnboarding() => keyValueStorage.GetBool(completedOnboardingKey);

        public string GetLastOpened() => keyValueStorage.GetString(lastAccessDateKey);

        #endregion

        #region IUserPreferences

        public bool IsManualModeEnabled() => keyValueStorage.GetBool(preferManualMode);

        public void EnableManualMode()
        {
            keyValueStorage.SetBool(preferManualMode, true);
        }

        public void EnableTimerMode()
        {
            keyValueStorage.SetBool(preferManualMode, false);
        }

        public void Reset()
        {
            EnableTimerMode();
        }

        #endregion
    }
}
