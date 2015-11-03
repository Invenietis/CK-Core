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
using Cake.Common.Tools.NUnit;
using Cake.Common.Text;
using Cake.Common.Tools.NuGet.Push;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace CodeCake
{
    /// <summary>
    /// Sample build "script".
    /// It can be decorated with AddPath attributes that inject paths into the PATH environment variable. 
    /// </summary>
    [AddPath( "CodeCakeBuilder/Tools" )]
    public class Build : CodeCakeHost
    {
        public Build()
        {
            var nugetOutputDir = Cake.Directory( "CodeCakeBuilder/Release" );
            DNXSolution dnxSolution = null;
            IEnumerable<DNXProjectFile> projectsToPublish = null;
            SimpleRepositoryInfo gitInfo = null;
            string configuration = null;

            Setup( () =>
            {
                dnxSolution = Cake.GetDNXSolution( p => p.ProjectName != "CodeCakeBuilder" );
                if( !dnxSolution.IsValid ) throw new Exception( "Unable to initialize solution." );
                projectsToPublish = dnxSolution.Projects.Where( p => !p.ProjectName.EndsWith( "Tests" ) );
            } );

            Teardown( () =>
            {
                dnxSolution.RestoreProjectFiles();
            } );

            Task( "Check-Repository" )
                .Does( () =>
                {
                    gitInfo = dnxSolution.RepositoryInfo;
                    if( !gitInfo.IsValid ) throw new Exception( "Repository is not ready to be published." );
                    configuration = gitInfo.IsValidRelease && gitInfo.PreReleaseName.Length == 0 ? "Release" : "Debug";

                    Cake.Information( "Publishing {0} projects with version={1} and configuration={2}: {3}",
                        projectsToPublish.Count(),
                        gitInfo.SemVer,
                        configuration,
                        String.Join( ", ", projectsToPublish.Select( p => p.ProjectName ) ) );
                } );

            Task( "Set-ProjectVersion" )
                .Does( () =>
                {
                    if( dnxSolution.UpdateProjectFiles() > 0 )
                    {
                        Cake.DNURestore( c =>
                        {
                            c.Quiet = true;
                            c.ProjectPaths.UnionWith( dnxSolution.Projects.Select( p => p.ProjectFilePath ) );
                        } );
                    }
                } );

            Task( "Clean" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                {
                    Cake.CleanDirectories( "**/bin/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( "**/obj/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( nugetOutputDir );
                } );

            Task( "Build-And-Pack" )
                .IsDependentOn( "Clean" )
                .IsDependentOn( "Set-ProjectVersion" )
                .Does( () =>
                {
                    Cake.DNUBuild( c =>
                    {
                        c.GeneratePackage = true;
                        c.Configurations.Add( configuration );
                        c.ProjectPaths.UnionWith( dnxSolution.Projects.Select( p => p.ProjectDir ) );
                        //c.Quiet = true;
                    } );
                } );

            Task( "Unit-Testing" )
                .IsDependentOn( "Build-And-Pack" )
                .Does( () =>
                {
                    Cake.CreateDirectory( nugetOutputDir );
                } );

            Task( "Push-NuGet-Packages" )
                .IsDependentOn( "Unit-Testing" )
                .Does( () =>
                {
                    var nugetPackages = Cake.GetFiles( nugetOutputDir.Path + "/*.nupkg" ).Select( f => f.FullPath );
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
                        PushNuGetPackages( "NUGET_API_KEY", "https://www.nuget.org/api/v2/package", nugetPackages );
                    }
                    else
                    {
                        Debug.Assert( gitInfo.IsValidCIBuild );
                        PushNuGetPackages( "MYGET_EXPLORE_API_KEY", "https://www.myget.org/F/invenietis-explore/api/v2/package", nugetPackages );
                    }
                } );

            // The Default task for this script can be set here.
            Task( "Default" )
                .IsDependentOn( "Push-NuGet-Packages" );

        }
        private void PushNuGetPackages( string apiKeyName, string pushUrl, IEnumerable<string> nugetPackages )
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