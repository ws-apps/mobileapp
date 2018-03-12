#tool "nuget:?package=xunit.runner.console&version=2.2.0"
#tool "nuget:?package=NUnit.Runners&version=2.6.3"

public class TemporaryFileTransformation
{
    public string Path { get; set; }
    public string Original { get; set; }
    public string Temporary { get; set; }
}

var target = Argument("target", "Default");
var buildAll = Argument("buildall", Bitrise.IsRunningOnBitrise);

private Action Test(string[] projectPaths)
{
    var settings = new DotNetCoreTestSettings { NoBuild = true };

    return () => 
    {
        foreach (var projectPath in projectPaths)
        {
            DotNetCoreTest(projectPath, settings);
        }
    };
}

private Action UITest(string[] dllPaths)
{
    return () => 
    {
        foreach(var dllPath in dllPaths)
        {
            var args = $"tools/nunit.runners.2.6.3/NUnit.Runners/tools/nunit-console.exe {dllPath} -stoponerror";

            var result = StartProcess("mono", new ProcessSettings { Arguments = args });
            if (result == 0) continue;

            throw new Exception($"Failed while running UI tests at {dllPath}");
        }
    };
}

private Action BuildSolution(string configuration, string platform = "")
{
    const string togglSolution = "./Toggl.sln";
    var buildSettings = new MSBuildSettings 
    {
        Verbosity = Bitrise.IsRunningOnBitrise ? Verbosity.Verbose : Verbosity.Minimal,
        Configuration = configuration
    };

	return () => MSBuild(togglSolution, buildSettings);
}

private Action GenerateApk(string configuration)
{
    const string droidProject = "./Toggl.Giskard/Toggl.Giskard.csproj";
    var buildSettings = new MSBuildSettings 
    {
        Verbosity = Bitrise.IsRunningOnBitrise ? Verbosity.Verbose : Verbosity.Minimal,
        Configuration = configuration
    };

    buildSettings.WithTarget("SignAndroidPackage");

    return () => MSBuild(droidProject, buildSettings);
}

private string GetCommitHash()
{   
    IEnumerable<string> redirectedOutput;
    StartProcess("git", new ProcessSettings
    {
        Arguments = "rev-parse HEAD",
        RedirectStandardOutput = true
    }, out redirectedOutput);

    return redirectedOutput.Last();
}

private string GetCommitCount()
{   
    IEnumerable<string> redirectedOutput;
    StartProcess("git", new ProcessSettings
    {
        Arguments = "rev-list --count HEAD",
        RedirectStandardOutput = true
    }, out redirectedOutput);

    return redirectedOutput.Last();
}

private TemporaryFileTransformation GetAndroidProjectConfigurationTransformation()
{
    const string path = "Toggl.Giskard/Toggl.Giskard.csproj";
    var storePass = EnvironmentVariable("BITRISEIO_ANDROID_KEYSTORE_PASSWORD");
    var keyAlias = EnvironmentVariable("BITRISEIO_ANDROID_KEYSTORE_ALIAS");
    var keyPass = EnvironmentVariable("BITRISEIO_ANDROID_KEYSTORE_PRIVATE_KEY_PASSWORD");

    var filePath = GetFiles(path).Single();
    var file = TransformTextFile(filePath).ToString();

    return new TemporaryFileTransformation
    {
        Path = path,
        Original = file,
        Temporary = file.Replace("{KEYSTORE_PASSWORD}", storePass)
                        .Replace("{KEYSTORE_ALIAS}", keyAlias)
                        .Replace("{KEYSTORE_ALIAS_PASSWORD}", keyPass)
    };
}

private TemporaryFileTransformation GetIosAnalyticsServicesConfigurationTransformation()
{
    const string path = "Toggl.Daneel/GoogleService-Info.plist";
    var adUnitForBannerTest = EnvironmentVariable("TOGGL_AD_UNIT_ID_FOR_BANNER_TEST");
    var adUnitIdForInterstitialTest = EnvironmentVariable("TOGGL_AD_UNIT_ID_FOR_INTERSTITIAL_TEST");
    var clientId = EnvironmentVariable("TOGGL_CLIENT_ID");
    var reversedClientId = EnvironmentVariable("TOGGL_REVERSED_CLIENT_ID");
    var apiKey = EnvironmentVariable("TOGGL_API_KEY");
    var gcmSenderId = EnvironmentVariable("TOGGL_GCM_SENDER_ID");
    var projectId = EnvironmentVariable("TOGGL_PROJECT_ID");
    var storageBucket = EnvironmentVariable("TOGGL_STORAGE_BUCKET");
    var googleAppId = EnvironmentVariable("TOGGL_GOOGLE_APP_ID");

    var filePath = GetFiles(path).Single();
    var file = TransformTextFile(filePath).ToString();

    return new TemporaryFileTransformation
    { 
        Path = path, 
        Original = file,
        Temporary = file.Replace("{TOGGL_AD_UNIT_ID_FOR_BANNER_TEST}", adUnitForBannerTest)
                        .Replace("{TOGGL_AD_UNIT_ID_FOR_INTERSTITIAL_TEST}", adUnitIdForInterstitialTest)
                        .Replace("{TOGGL_CLIENT_ID}", clientId)
                        .Replace("{TOGGL_REVERSED_CLIENT_ID}", reversedClientId)
                        .Replace("{TOGGL_API_KEY}", apiKey)
                        .Replace("{TOGGL_GCM_SENDER_ID}", gcmSenderId)
                        .Replace("{TOGGL_PROJECT_ID}", projectId)
                        .Replace("{TOGGL_STORAGE_BUCKET}", storageBucket)
                        .Replace("{TOGGL_GOOGLE_APP_ID}", googleAppId)
    };
}

private TemporaryFileTransformation GetIosCrashConfigurationTransformation()
{
    const string path = "Toggl.Daneel/Startup/AppDelegate.cs";
    var appCenterId = EnvironmentVariable("TOGGL_APP_CENTER_ID_IOS");

    var filePath = GetFiles(path).Single();
    var file = TransformTextFile(filePath).ToString();

    return new TemporaryFileTransformation
    { 
        Path = path, 
        Original = file,
        Temporary = file.Replace("{TOGGL_APP_CENTER_ID_IOS}", appCenterId)
    };
}

private TemporaryFileTransformation GetAndroidGoogleServicesTransformation()
{
    const string path = "Toggl.Giskard/google-services.json";
    var apiKey = EnvironmentVariable("TOGGL_DROID_GOOGLE_SERVICES_API_KEY");
    var clientId = EnvironmentVariable("TOGGL_DROID_GOOGLE_SERVICES_CLIENT_ID");
    var mobileSdkAppId = EnvironmentVariable("TOGGL_DROID_GOOGLE_SERVICES_MOBILE_SDK_APP_ID");
    var projectNumber = EnvironmentVariable("TOGGL_DROID_GOOGLE_SERVICES_PROJECT_NUMBER");
    var projectId = EnvironmentVariable("TOGGL_DROID_GOOGLE_SERVICES_PROJECT_ID");

    var filePath = GetFiles(path).Single();
    var file = TransformTextFile(filePath).ToString();

    return new TemporaryFileTransformation
    { 
        Path = path, 
        Original = file,
        Temporary = file.Replace("{TOGGL_DROID_GOOGLE_SERVICES_API_KEY}", apiKey)
                        .Replace("{TOGGL_DROID_GOOGLE_SERVICES_CLIENT_ID}", clientId)
                        .Replace("{TOGGL_DROID_GOOGLE_SERVICES_MOBILE_SDK_APP_ID}", mobileSdkAppId)
                        .Replace("{TOGGL_DROID_GOOGLE_SERVICES_PROJECT_NUMBER}", projectNumber)
                        .Replace("{TOGGL_DROID_GOOGLE_SERVICES_PROJECT_ID}", projectId)
    };
}

private TemporaryFileTransformation GetAndroidGoogleLoginTransformation()
{
    const string path = "Toggl.Giskard/Services/GoogleService.cs";
    var clientId = EnvironmentVariable("TOGGL_DROID_GOOGLE_SERVICES_CLIENT_ID");

    var filePath = GetFiles(path).Single();
    var file = TransformTextFile(filePath).ToString();

    return new TemporaryFileTransformation
    { 
        Path = path, 
        Original = file,
        Temporary = file.Replace("{TOGGL_DROID_GOOGLE_SERVICES_CLIENT_ID}", clientId)
    };
}

private TemporaryFileTransformation GetDroidCrashConfigurationTransformation()
{
    const string path = "Toggl.Giskard/Startup/SplashScreen.cs";
    var appCenterId = EnvironmentVariable("TOGGL_APP_CENTER_ID_DROID");

    var filePath = GetFiles(path).Single();
    var file = TransformTextFile(filePath).ToString();

    return new TemporaryFileTransformation
    { 
        Path = path, 
        Original = file,
        Temporary = file.Replace("{TOGGL_APP_CENTER_ID_DROID}", appCenterId)
    };
}

private TemporaryFileTransformation GetIosInfoConfigurationTransformation()
{
    const string path = "Toggl.Daneel/Info.plist";

    var commitCount = GetCommitCount();
    var reversedClientId = EnvironmentVariable("TOGGL_REVERSED_CLIENT_ID");
    var filePath = GetFiles(path).Single();
    var file = TransformTextFile(filePath).ToString();

    return new TemporaryFileTransformation
    { 
        Path = path, 
        Original = file,
        Temporary = file.Replace("{TOGGL_REVERSED_CLIENT_ID}", reversedClientId)
                        .Replace("IOS_BUNDLE_VERSION", commitCount)
    };
}

private TemporaryFileTransformation GetIntegrationTestsConfigurationTransformation()
{   
    const string path = "Toggl.Ultrawave.Tests.Integration/Helper/Configuration.cs";
    var commitHash = GetCommitHash(); 
    var filePath = GetFiles(path).Single();
    var file = TransformTextFile(filePath).ToString();

    return new TemporaryFileTransformation
    { 
        Path = path, 
        Original = file,
        Temporary = file.Replace("{CAKE_COMMIT_HASH}", commitHash)
    };
}

var transformations = new List<TemporaryFileTransformation> 
{
    GetIosInfoConfigurationTransformation(),
    GetIosCrashConfigurationTransformation(),
    GetDroidCrashConfigurationTransformation(),
    GetIntegrationTestsConfigurationTransformation(),
    GetIosAnalyticsServicesConfigurationTransformation(),
    GetAndroidProjectConfigurationTransformation(),
    GetAndroidGoogleServicesTransformation(),
    GetAndroidGoogleLoginTransformation()
};

private HashSet<string> targetsThatSkipTearDown = new HashSet<string>
{
    "Build.Release.iOS.AdHoc",
    "Build.Release.iOS.AppStore",
    "Build.Release.Android.AdHoc"
};

private string[] GetUnitTestProjects() => new []
{
    "./Toggl.Multivac.Tests/Toggl.Multivac.Tests.csproj",
    "./Toggl.Ultrawave.Tests/Toggl.Ultrawave.Tests.csproj",
    "./Toggl.PrimeRadiant.Tests/Toggl.PrimeRadiant.Tests.csproj",
    "./Toggl.Foundation.Tests/Toggl.Foundation.Tests.csproj",
};

private string[] GetUITestFiles() => new []
{
    "./bin/Debug/Toggl.Giskard.Tests.UI.dll",
    "./bin/Debug/Toggl.Daneel.Tests.UI.dll"
};

private string[] GetIntegrationTestProjects()
    => new [] { "./Toggl.Ultrawave.Tests.Integration/Toggl.Ultrawave.Tests.Integration.csproj" };

Setup(context => transformations.ForEach(transformation => System.IO.File.WriteAllText(transformation.Path, transformation.Temporary)));
Teardown(context =>
{
    if (targetsThatSkipTearDown.Contains(target))
        return;

    transformations.ForEach(transformation => System.IO.File.WriteAllText(transformation.Path, transformation.Original));
});

//Build
Task("Clean")
    .Does(() => 
        {
            CleanDirectory("./bin");
            CleanDirectory("./Toggl.Daneel/obj");
            CleanDirectory("./Toggl.Daneel.Tests/obj");
            CleanDirectory("./Toggl.Daneel.Tests.UI/obj");
            CleanDirectory("./Toggl.Giskard/obj");
            CleanDirectory("./Toggl.Giskard.Tests/obj");
            CleanDirectory("./Toggl.Giskard.Tests.UI/obj");
            CleanDirectory("./Toggl.Foundation/obj");
            CleanDirectory("./Toggl.Foundation.MvvmCross/obj");
            CleanDirectory("./Toggl.Foundation.Tests/obj");
            CleanDirectory("./Toggl.Multivac/obj");
            CleanDirectory("./Toggl.Multivac.Tests/obj");
            CleanDirectory("./Toggl.PrimeRadiant/obj");
            CleanDirectory("./Toggl.PrimeRadiant.Realm/obj");
            CleanDirectory("./Toggl.PrimeRadiant.Tests/obj");
            CleanDirectory("./Toggl.Ultrawave/obj");
            CleanDirectory("./Toggl.Ultrawave.Tests/obj");
            CleanDirectory("./Toggl.Ultrawave.Tests.Integration/obj");
        });

Task("Nuget")
    .IsDependentOn("Clean")
    .Does(() => NuGetRestore("./Toggl.sln"));

Task("Build.Tests.All")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("Debug"))
    .Does(GenerateApk("Debug"));

Task("Build.Tests.Unit")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("UnitTests"));

Task("Build.Tests.Integration")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("ApiTests"));

Task("Build.Tests.UI")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("Debug"))
    .Does(GenerateApk("Debug"));

//iOS Builds
Task("Build.Release.iOS.AdHoc")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("Release.AdHoc"));

Task("Build.Release.iOS.AppStore")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("Release.AppStore", ""));

//Android Builds
Task("Build.Release.Android.AdHoc")
    .IsDependentOn("Nuget")
    .Does(BuildSolution("Release.AdHoc.Giskard", ""));

//Unit Tests
Task("Tests.Unit")
    .IsDependentOn(buildAll ? "Build.Tests.All" : "Build.Tests.Unit")
    .Does(Test(GetUnitTestProjects()));

//Integration Tests
Task("Tests.Integration")
    .IsDependentOn(buildAll ? "Build.Tests.All" : "Build.Tests.Integration")
    .Does(Test(GetIntegrationTestProjects()));

//UI Tests
Task("Tests.UI")
    .IsDependentOn("Build.Tests.UI")
    .Does(UITest(GetUITestFiles()));

// All Tests
Task("Tests")
    .IsDependentOn("Tests.Unit")
    .IsDependentOn("Tests.Integration")
    .IsDependentOn("Tests.UI");

//Default Operation
Task("Default")
    .IsDependentOn("Tests.Unit");

RunTarget(target);
