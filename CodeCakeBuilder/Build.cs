using Cake.Common;
using Cake.Common.Solution;
using Cake.Common.IO;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.NuGet;
using Cake.Core;
using Cake.Common.Diagnostics;
using SimpleGitVersion;
using Code.Cake;
using Cake.Common.Tools.NuGet.Pack;
using System;
using System.Linq;
using Cake.Common.Tools.SignTool;
using Cake.Core.Diagnostics;
using Cake.Common.Tools.NUnit;
using Cake.Common.Text;

namespace CodeCake
{
    /// <summary>
    /// Sample build "script".
    /// It can be decorated with AddPath attributes that inject paths into the PATH environment variable. 
    /// </summary>
    [AddPath( "%LOCALAPPDATA%/NuGet" )]
    [AddPath( "packages/**/tools*" )]
    public class Build : CodeCakeHost
    {
        public Build()
        {
            var securePath = Cake.Argument( "securePath", "../_Secure" );
            var secureDir = Cake.Directory( securePath );

            var nugetOutputDir = Cake.Directory( "CodeCakeBuilder/Release" );

            var projectsToPublish = Cake.ParseSolution( "CK-Core.sln" )
                                        .Projects
                                        .Where( p => p.Name != "CodeCakeBuilder" 
                                                     && p.Name != "CKMon2Htm.ConsoleDemo"
                                                     && !p.Path.Segments.Contains( "Tests" ) );
            SimpleRepositoryInfo gitInfo = null;
            string configuration = null;

            Task( "Check-Repository" )
                .Does( () =>
                {
                    configuration = "Debug";
                    Cake.MSBuild( projectsToPublish.Single( p => p.Name == "CKMon2Htm" ).Path, new MSBuildSettings()
                        .WithTarget( "Publish" )
                            .SetConfiguration( configuration )
                            //.WithProperty( "PublishUrl", @"CodeCakeBuilder\Release\CKMon2Htm\" + configuration )
                    );

                    var assembliesToSign = projectsToPublish
                               .Select( p => p.Path.GetDirectory() + "/bin/" + configuration + "/" + p.Name.Replace( ".Net40", "" ) )
                               .SelectMany( p => new[] { p + ".dll", p + ".exe" } )
                               .Where( p => Cake.FileExists( p ) );

                    Cake.Information( "Publishing {0} projects with version={1} and configuration={2}: {3}", 
                        projectsToPublish.Count(), 
                        "ppp", //gitInfo.SemVer, 
                        configuration, 
                        String.Join( ", ", projectsToPublish.Select( p => p.Name ) ) );

                    Cake.Information( "Publishing {0} projects with version={1} and configuration={2}: {3}", 
                        projectsToPublish.Count(), 
                        "pppp", //gitInfo.SemVer, 
                        configuration, 
                        String.Join( ", ", assembliesToSign ) );

                    gitInfo = Cake.GetSimpleRepositoryInfo();
                    if( !gitInfo.IsValid ) throw new Exception( "Repository is not ready to be published." );
                    configuration = gitInfo.IsValidRelease && gitInfo.PreReleaseName.Length == 0 ? "Release" : "Debug";

                    Cake.Information( "Publishing {0} projects with version={1} and configuration={2}: {3}", 
                        projectsToPublish.Count(), 
                        gitInfo.SemVer, 
                        configuration, 
                        String.Join( ", ", projectsToPublish.Select( p => p.Name ) ) );
                } );

            Task( "Clean" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                {
                    Cake.CleanDirectories( "**/bin/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( "**/obj/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( nugetOutputDir );
                } );

            Task( "Build" )
                .IsDependentOn( "Clean" )
                .Does( () =>
                {
                    using( var tempSln = Cake.CreateTemporarySolutionFile( "CK-Core.sln" ) )
                    {
                        tempSln.ExcludeProjectsFromBuild( "CodeCakeBuilder", "CKMon2Htm" );
                        Cake.MSBuild( tempSln.FullPath, new MSBuildSettings()
                                .SetConfiguration( configuration )
                                .SetVerbosity( Verbosity.Minimal )
                                .SetMaxCpuCount( 1 )
                                // Always generates Xml documentation. Relies on this definition in the csproj files:
                                //
                                // <PropertyGroup Condition=" $(GenerateDocumentation) != '' ">
                                //   <DocumentationFile>bin\$(Configuration)\$(AssemblyName).xml</DocumentationFile>
                                // </PropertyGroup>
                                //
                                .WithProperty( "GenerateDocumentation", "true" ) );
                        Cake.MSBuild( projectsToPublish.Single( p => p.Name == "CKMon2Htm" ).Path, new MSBuildSettings()
                            .WithTarget( "Publish" )
                                .SetConfiguration( configuration )
                                .WithProperty( "PublishUrl", @"CodeCakeBuilder\Release\CKMon2Htm\" + configuration )
                        );
                    }
                } );

            Task( "Unit-Testing" )
                .IsDependentOn( "Build" )
                .Does( () =>
                {
                    Cake.NUnit( "Tests/*.Tests/bin/" + configuration + "/*.Tests.dll", new NUnitSettings() {
                        Framework = "v4.5",
                        OutputFile = nugetOutputDir.Path + "/TestResult.txt",
                        StopOnError = true
                    } );
                    Cake.NUnit( "net40/Tests/*.Tests/bin/" + configuration + "/*.Tests.dll", new NUnitSettings() {
                        Framework = "v4.0",
                        OutputFile = nugetOutputDir.Path + "/TestResult.net40.txt",
                        StopOnError = true
                    } );
                } );


            Task( "Sign-Assemblies" )
                .IsDependentOn( "Build" )
                .WithCriteria( () => !gitInfo.IsValidCIBuild )
                .Does( () =>
                {
                    var assembliesToSign = projectsToPublish
                               .Select( p => p.Path.GetDirectory() + "/bin/" + configuration + "/" + p.Name.Replace( ".Net40", "" ) )
                               .SelectMany( p => new[] { p + ".dll", p + ".exe" } )
                               .Where( p => Cake.FileExists( p ) );

                    var signSettingsForRelease = new SignToolSignSettings()
                    {
                        TimeStampUri = new Uri( "http://timestamp.verisign.com/scripts/timstamp.dll" ),
                        CertPath = secureDir + Cake.File( "Invenietis-Authenticode.pfx" ),
                        Password = System.IO.File.ReadAllText( secureDir + Cake.File( "Invenietis-Authenticode.p.txt" ) )
                    };

                    Cake.Sign( assembliesToSign, signSettingsForRelease );
                } );

            Task( "Create-NuGet-Packages" )
                .IsDependentOn( "Sign-Assemblies" )
                .IsDependentOn( "Unit-Testing" )
                .Does( () =>
                {
                    Cake.CreateDirectory( nugetOutputDir );
                    var settings = new NuGetPackSettings()
                    {
                        Version = gitInfo.NuGetVersion,
                        BasePath = Cake.Environment.WorkingDirectory,
                        OutputDirectory = nugetOutputDir
                    };
                    Cake.CopyFiles( "CodeCakeBuilder/NuSpec/*.nuspec", nugetOutputDir );
                    foreach( var nuspec in Cake.GetFiles( nugetOutputDir.Path + "/*.nuspec" ) )
                    {
                        Cake.TransformTextFile( nuspec, "{{", "}}" ).WithToken( "configuration", configuration ).Save( nuspec );
                        Cake.NuGetPack( nuspec, settings );
                    }
                    Cake.DeleteFiles( nugetOutputDir.Path + "/*.nuspec" );
                } );

            // The Default task for this script can be set here.
            Task( "Default" ).IsDependentOn( "Create-NuGet-Packages" );
        }
    }
}
