using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Common.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace Code.Cake
{
    static class CommandRunner
    {
        public static int RunCmd( this ICakeContext context, string commandLine )
        {
            ProcessStartInfo cmdStartInfo = new ProcessStartInfo();
            cmdStartInfo.FileName = @"cmd.exe";
            cmdStartInfo.RedirectStandardOutput = true;
            cmdStartInfo.RedirectStandardError = true;
            cmdStartInfo.RedirectStandardInput = true;
            cmdStartInfo.UseShellExecute = false;
            cmdStartInfo.CreateNoWindow = true;

            Process cmdProcess = new Process();
            cmdProcess.StartInfo = cmdStartInfo;
            cmdProcess.ErrorDataReceived += ( o, e ) => { if( !string.IsNullOrEmpty( e.Data ) ) context.Error( e.Data ); };
            cmdProcess.OutputDataReceived += ( o, e ) => { if( !string.IsNullOrEmpty( e.Data ) ) context.Information( e.Data ); };
            cmdProcess.EnableRaisingEvents = true;
            cmdProcess.Start();
            cmdProcess.BeginOutputReadLine();
            cmdProcess.BeginErrorReadLine();

            cmdProcess.StandardInput.WriteLine( commandLine + " & exit" );
            cmdProcess.WaitForExit();
            return cmdProcess.ExitCode;
        }

        /// <summary>
        /// Runs dnu restore.
        /// </summary>
        /// <param name="context">This cake context.</param>
        /// <param name="config">The configuration to use.</param>
        public static void DNURestore( this ICakeContext context, Action<DNURestoreSettings> config )
        {
            var c = new DNURestoreSettings();
            config( c );
            var b = new StringBuilder();
            b.Append( "dnu " );
            c.ToString( b );
            RunCmd( context, b.ToString() );
        }

        public static void DNUBuild( this ICakeContext context, Action<DNUBuildSettings> config )
        {
            var c = new DNUBuildSettings();
            config( c );
            var b = new StringBuilder();
            b.Append( "dnu " );
            c.ToString( b );
            RunCmd( context, b.ToString() );
        }

    }
}
