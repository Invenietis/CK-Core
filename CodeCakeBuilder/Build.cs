using Cake.Common;
using Cake.Common.Solution;
using Cake.Common.IO;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.NuGet;
using Cake.Core;
using Cake.Common.Diagnostics;
using SimpleGitVersion;
using Code.Cake;
using Cake.Common.Build.AppVeyor;
using Cake.Common.Tools.NuGet.Pack;
using System;
using System.Linq;
using Cake.Common.Tools.SignTool;
using Cake.Core.Diagnostics;
using Cake.Common.Text;
using Cake.Common.Tools.NuGet.Push;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Cake.Common.Tools.NUnit;
using Cake.Core.IO;
using CK.Text;

namespace CodeCake
{
    /// <summary>
    /// Standard build "script".
    /// </summary>
    [AddPath( "CodeCakeBuilder/Tools" )]
    [AddPath( "packages/**/tools*" )]
    public class Build : CodeCakeHost
    {
        public Build()
        {
            const string solutionName = "CK-Core";
            const string solutionFileName = solutionName + ".sln";

            var releasesDir = Cake.Directory( "CodeCakeBuilder/Releases" );
            SimpleRepositoryInfo gitInfo = null;
            // Configuration is either "Debug" or "Release".
            string configuration = null;

            // We do not publish .Tests projects for this solution.
            var projectsToPublish = Cake.ParseSolution( solutionFileName )
                                        .Projects
                                        .Where( p => p.Name != "CodeCakeBuilder"
                                                     && !p.Path.Segments.Contains( "Tests" ) );

            Task( "Check-Repository" )
                .Does( () =>
                {
                    gitInfo = Cake.GetSimpleRepositoryInfo();

                    if( !gitInfo.IsValid )
                    {
                        if( Cake.IsInteractiveMode()
                            && Cake.ReadInteractiveOption( "Repository is not ready to be published. Proceed anyway?", 'Y', 'N' ) == 'Y' )
                        {
                            Cake.Warning( "GitInfo is not valid, but you choose to continue..." );
                        }
                        else throw new Exception( "Repository is not ready to be published." );
                    }
                    configuration = gitInfo.IsValidRelease && gitInfo.PreReleaseName.Length == 0 ? "Release" : "Debug";

                    Cake.Information( "Publishing {0} projects with version={1} and configuration={2}: {3}",
                        projectsToPublish.Count(),
                        gitInfo.SemVer,
                        configuration,
                        projectsToPublish.Select( p => p.Name ).Concatenate() );
                } );

            Task( "Restore-NuGet-Packages" )
                .Does( () =>
                {
                    Cake.NuGetRestore( solutionFileName );
                } );

            Task( "Clean" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                {
                    Cake.CleanDirectories( "**/bin/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( "**/obj/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( releasesDir );
                    Cake.DeleteFiles( "Tests/**/TestResult.xml" );
                } );

            Task( "Build" )
                .IsDependentOn( "Clean" )
                .IsDependentOn( "Restore-NuGet-Packages" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                {
                    using( var tempSln = Cake.CreateTemporarySolutionFile( solutionFileName ) )
                    {
                        tempSln.ExcludeProjectsFromBuild( "CodeCakeBuilder" );
                        Cake.MSBuild( tempSln.FullPath, settings =>
                        {
                            settings.Configuration = configuration;
                            settings.Verbosity = Verbosity.Minimal;
                            // Always generates Xml documentation. Relies on this definition in the csproj files:
                            //
                            // <PropertyGroup Condition=" $(GenerateDocumentation) != '' ">
                            //   <DocumentationFile>bin\$(Configuration)\$(AssemblyName).xml</DocumentationFile>
                            // </PropertyGroup>
                            //
                            settings.Properties.Add( "GenerateDocumentation", new[] { "true" } );
                        } );
                    }
                } );

            Task( "Unit-Testing" )
                .IsDependentOn( "Build" )
                .Does( () =>
                {
                    Cake.CreateDirectory( releasesDir );
                    var testDlls = Cake.ParseSolution( solutionFileName )
                     .Projects
                         .Where( p => p.Name.EndsWith( ".Tests" ) )
                         .Select( p => p.Path.GetDirectory().CombineWithFilePath( "bin/" + configuration + "/" + p.Name + ".dll" ) );
                    Cake.Information( "Testing: {0}", string.Join( ", ", testDlls.Select( p => p.GetFilename().ToString() ) ) );

                    Cake.NUnit( testDlls, new NUnitSettings()
                    {
                        Framework = "v4.5",
                        OutputFile = releasesDir.Path + "/TestResult.txt"
                    } );
                } );

            Task( "Create-NuGet-Packages" )
                .IsDependentOn( "Unit-Testing" )
                .WithCriteria( () => gitInfo.IsValid )
                .Does( () =>
                {
                    Cake.CreateDirectory( releasesDir );
                    var settings = new NuGetPackSettings()
                    {
                        Version = gitInfo.NuGetVersion,
                        BasePath = Cake.Environment.WorkingDirectory,
                        OutputDirectory = releasesDir
                    };
                    Cake.CopyFiles( "CodeCakeBuilder/NuSpec/*.nuspec", releasesDir );
                    foreach( var nuspec in Cake.GetFiles( releasesDir.Path + "/*.nuspec" ) )
                    {
                        TransformText( nuspec, configuration, gitInfo );
                        Cake.NuGetPack( nuspec, settings );
                    }
                    Cake.DeleteFiles( releasesDir.Path + "/*.nuspec" );
                } );


            Task( "Push-NuGet-Packages" )
                .IsDependentOn( "Create-NuGet-Packages" )
                .WithCriteria( () => gitInfo.IsValid )
                .Does( () =>
                {
                    IEnumerable<FilePath> nugetPackages = Cake.GetFiles( releasesDir.Path + "/*.nupkg" );
                    if( Cake.IsInteractiveMode() )
                    {
                        var localFeed = Cake.FindDirectoryAbove( "LocalFeed" );
                        if( localFeed != null )
                        {
                            Cake.Information( "LocalFeed directory found: {0}", localFeed );
                            if( Cake.ReadInteractiveOption( "Do you want to publish to LocalFeed?", 'Y', 'N' ) == 'Y' )
                            {
                                Cake.CopyFiles( nugetPackages, localFeed );
                            }
                        }
                    }
                    if( gitInfo.IsValidRelease )
                    {
                        if( gitInfo.PreReleaseName == "" 
                            || gitInfo.PreReleaseName == "prerelease" 
                            || gitInfo.PreReleaseName == "rc" )
                        {
                            PushNuGetPackages( "NUGET_API_KEY", "https://www.nuget.org/api/v2/package", nugetPackages );
                        }
                        else
                        {
                            // An alpha, beta, delta, epsilon, gamma, kappa goes to invenietis-preview.
                            PushNuGetPackages( "MYGET_PREVIEW_API_KEY", "https://www.myget.org/F/invenietis-preview/api/v2/package", nugetPackages );
                        }
                    }
                    else
                    {
                        Debug.Assert( gitInfo.IsValidCIBuild );
                        PushNuGetPackages( "MYGET_CI_API_KEY", "https://www.myget.org/F/invenietis-ci/api/v2/package", nugetPackages );
                    }
                } );

            Task( "Build-DocFX" )
                .IsDependentOn( "Check-Repository" )
                .WithCriteria( () => gitInfo.IsValid )
                .Does( () =>
                {
                    var settingsFile = Cake.File("DocFX/docfx.json");
                    var settingsFileCopy = Cake.File("DocFX/docfx.json.old");
                    try
                    {
                        // Create a copy restored in finally{}
                        Cake.Information( $"Backing up {settingsFile} to {settingsFileCopy}" );
                        Cake.CopyFile( settingsFile, settingsFileCopy );

                        // Edit the settings file
                        // TODO: A proper visitor & rewriter; target properties are:
                        //   o.build.globalMetadata._appVersion
                        //   o.build.globalMetadata._appShortVersion
                        Cake.Information( $"Writing {settingsFile}" );
                        Cake.Information( $"* _appVersion: {gitInfo.SemVer}" );
                        Cake.Information( $"* _appShortVersion: {gitInfo.NuGetVersion}" );
                        File.WriteAllText(
                            settingsFile,
                            File.ReadAllText( settingsFile )
                                .Replace( "%%VERSION%%", gitInfo.SemVer )
                                .Replace( "%%SHORTVERSION%%", gitInfo.NuGetVersion )
                            );

                        // Build DocFX project
                        var docFxProjectFile = Cake.File("DocFX/DocFX.csproj");

                        Cake.Information( $"Building {docFxProjectFile}" );

                        Cake.MSBuild( docFxProjectFile, settings =>
                        {
                            settings.Configuration = configuration;
                            settings.Verbosity = Verbosity.Minimal;
                        } );

                        // Pack output
                        var outDir = Cake.Directory("DocFX/_site");  // Configured in DocFX/docfx.json
                        var outZip = releasesDir.Path.CombineWithFilePath( $@"{solutionName}.{gitInfo.SemVer}.DocFX.zip" );

                        Cake.Information( $"Packing {outDir} to {outZip}" );
                        Cake.Zip( outDir, outZip );
                    }
                    finally
                    {
                        // Delete existing file and restore file copy
                        if( Cake.FileExists( settingsFileCopy ) )
                        {
                            Cake.Information( $"Restoring {settingsFile}" );
                            if( Cake.FileExists( settingsFile ) ) { Cake.DeleteFile( settingsFile ); }
                            Cake.MoveFile( settingsFileCopy, settingsFile );
                        }
                    }
                } );

            // The Default task for this script can be set here.
            Task( "Default" )
                .IsDependentOn( "Push-NuGet-Packages" );

        }

        private void TransformText( FilePath textFilePath, string configuration, SimpleRepositoryInfo gitInfo )
        {
            Cake.TransformTextFile( textFilePath, "{{", "}}" )
                    .WithToken( "configuration", configuration )
                    .WithToken( "NuGetVersion", gitInfo.NuGetVersion )
                    .WithToken( "CSemVer", gitInfo.SemVer )
                    .Save( textFilePath );
        }

        private void PushNuGetPackages( string apiKeyName, string pushUrl, IEnumerable<FilePath> nugetPackages )
        {
            // Resolves the API key.
            var apiKey = Cake.InteractiveEnvironmentVariable( apiKeyName );
            if( string.IsNullOrEmpty( apiKey ) )
            {
                Cake.Information( "Could not resolve {0}. Push to {1} is skipped.", apiKeyName, pushUrl );
            }
            else
            {
                var settings = new NuGetPushSettings
                {
                    Source = pushUrl,
                    ApiKey = apiKey
                };

                foreach( var nupkg in nugetPackages )
                {
                    Cake.NuGetPush( nupkg, settings );
                }
            }
        }
    }
}