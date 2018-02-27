using Cake.Common.Diagnostics;
using Cake.Common.Solution;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Pack;
using Cake.Core.IO;
using SimpleGitVersion;
using System.Collections.Generic;

namespace CodeCake
{
    public partial class Build
    {
        void StandardCreateNuGetPackages( DirectoryPath releasesDir, IEnumerable<SolutionProject> projectsToPublish, SimpleRepositoryInfo gitInfo, string configuration )
        {
            var settings = new DotNetCorePackSettings().AddVersionArguments( gitInfo, c =>
            {
                c.NoBuild = true;
                c.IncludeSymbols = true;
                c.Configuration = configuration;
                c.OutputDirectory = releasesDir;
            } );
            foreach( SolutionProject p in projectsToPublish )
            {
                Cake.Information( p.Path.GetDirectory().FullPath );
                Cake.DotNetCorePack( p.Path.GetDirectory().FullPath, settings );
            }
        }
    }
}
