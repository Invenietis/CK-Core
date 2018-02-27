using Cake.Common.Solution;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Build;
using SimpleGitVersion;

namespace CodeCake
{
    public partial class Build
    {
        void StandardSolutionBuild( string solutionFileName, SimpleRepositoryInfo gitInfo, string configuration )
        {
            using( var tempSln = Cake.CreateTemporarySolutionFile( solutionFileName ) )
            {
                tempSln.ExcludeProjectsFromBuild( "CodeCakeBuilder" );
                Cake.DotNetCoreBuild( tempSln.FullPath.FullPath,
                    new DotNetCoreBuildSettings().AddVersionArguments( gitInfo, s =>
                    {
                        s.Configuration = configuration;
                    } ) );
            }
        }

    }
}
