var target = Argument("target", "Default");
var configuration = Argument("configuraion", "Release");

var solution = "./ORA.sln";
var trackerCsProject = "./ORA.Tracker/ORA.Tracker.csproj";
var buildDir = Directory("./build");

var buildSettings = new DotNetCoreBuildSettings()
{
  Framework = "netcoreapp3.1",
  Configuration = "Release",
  OutputDirectory = buildDir
};

Task("Default")
  .IsDependentOn("Build");

Task("Build")
  .IsDependentOn("Tracker");

Task("Tracker")
  .Does(() => {
    if (!DirectoryExists(buildDir))
      CreateDirectory(buildDir);

    DotNetCoreBuild(trackerCsProject, buildSettings);
  });

Task("Clean")
  .Does(() => {
    CleanDirectory(buildDir);
  });

RunTarget(target);
