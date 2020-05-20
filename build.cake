var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solution = "./ORA.sln";
var trackerProject = "./ORA.Tracker/ORA.Tracker.csproj";
var buildDir = Directory("./build");

var buildSettings = new DotNetCoreBuildSettings()
{
  Framework = "netcoreapp3.1",
  Configuration = "Release",
  OutputDirectory = buildDir
};

// Tests
var unitTestProject = "./ORA.Tracker.Tests/Unit/ORA.Tracker.Tests.Unit.csproj";
var integrationTestProject = "./ORA.Tracker.Tests/Integration/ORA.Tracker.Tests.Integration.csproj";

var testSettings = new DotNetCoreTestSettings()
{
  Framework = "netcoreapp3.1",
  Configuration = "Release"
};

Task("Default")
  .IsDependentOn("Test")
  .IsDependentOn("Build");

Task("Build")
  .IsDependentOn("Tracker");

Task("Tracker")
  .Does(() => {
    if (!DirectoryExists(buildDir))
      CreateDirectory(buildDir);

    DotNetCoreBuild(trackerProject, buildSettings);
  });

Task("Test")
  .IsDependentOn("Format")
  .IsDependentOn("Unit")
  .IsDependentOn("Integration");

Task("Format")
  .Does(() => { });

Task("Unit")
  .Does(() => {
    DotNetCoreTest(unitTestProject, testSettings);
  });

Task("Integration")
  .Does(() => {
    DotNetCoreTest(integrationTestProject, testSettings);
  });

Task("Clean")
  .Does(() => {
    CleanDirectory(buildDir);
  });

RunTarget(target);
