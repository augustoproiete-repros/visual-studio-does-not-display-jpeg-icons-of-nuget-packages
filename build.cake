#tool "nuget:?package=NuGet.CommandLine&version=5.10.0"

#addin "nuget:?package=Cake.MinVer&version=1.0.1"
#addin "nuget:?package=Cake.Args&version=1.0.1"

var target       = ArgumentOrDefault<string>("target") ?? "pack";
var buildVersion = MinVer(s => s.WithTagPrefix("v").WithDefaultPreReleasePhase("preview"));

Task("clean")
    .Does(() =>
{
    CleanDirectory("./build/artifacts");
});

Task("pack")
    .IsDependentOn("clean")
    .Does(() =>
{
    NuGetPack("./src/PackageWithJpegIconUrl.nuspec", new NuGetPackSettings
    {
        Version = buildVersion.PackageVersion,
        OutputDirectory = "./build/artifacts",
    });

    NuGetPack("./src/PackageWithJpegIconEmbedded.nuspec", new NuGetPackSettings
    {
        Version = buildVersion.PackageVersion,
        OutputDirectory = "./build/artifacts",
    });

    NuGetPack("./src/PackageWithMSPaintJpegIconUrl.nuspec", new NuGetPackSettings
    {
        Version = buildVersion.PackageVersion,
        OutputDirectory = "./build/artifacts",
    });

    NuGetPack("./src/PackageWithMSPaintJpegIconEmbedded.nuspec", new NuGetPackSettings
    {
        Version = buildVersion.PackageVersion,
        OutputDirectory = "./build/artifacts",
    });

});

Task("push")
    .IsDependentOn("pack")
    .Does(context =>
{
    var url =  context.EnvironmentVariable("NUGET_URL");
    if (string.IsNullOrWhiteSpace(url))
    {
        context.Information("No NuGet URL specified. Skipping publishing of NuGet packages");
        return;
    }

    var apiKey =  context.EnvironmentVariable("NUGET_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        context.Information("No NuGet API key specified. Skipping publishing of NuGet packages");
        return;
    }

    var nugetPushSettings = new DotNetCoreNuGetPushSettings
    {
        Source = url,
        ApiKey = apiKey,
    };

    foreach (var nugetPackageFile in GetFiles("./build/artifacts/*.nupkg"))
    {
        DotNetCoreNuGetPush(nugetPackageFile.FullPath, nugetPushSettings);
    }
});

RunTarget(target);
