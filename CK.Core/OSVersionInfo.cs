#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\OSVersionInfo.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace CK.Core
{
    /// <summary>
    /// Provides detailed information about the host operating system.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class OSVersionInfo
    {
        /// <summary>
        /// This is equal to <c>Path.GetDirectoryName( System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName )</c>.
        /// </summary>
        public static readonly string RunningDirectory;
        
        /// <summary>
        /// True if we are running Mono.
        /// </summary>
        public static readonly bool IsMono;

        /// <summary>
        /// True if we are running on Unix.
        /// </summary>
        public static readonly bool IsUnix;

        /// <summary>
        /// Gets the <see cref="Environment.OSVersion"/> <see cref="OperatingSystem"/> object.
        /// It offers useful information such as <see cref="OperatingSystem.ServicePack">Service Pack</see> and 
        /// its overriden <see cref="OperatingSystem.ToString"/> provides a valuable description.
        /// </summary>
        public static readonly OperatingSystem OSVersion;

        /// <summary>
        /// True if Platform Invoke is supported.
        /// </summary>
        public static readonly bool PInvokeSupported;

        /// <summary>
        /// Captures simple Windows operating system versions.
        /// The idea is to rely on release time to ease simple version check (ie. OSLevel &gt;= SimpleOSLevel.WindowsVista).
        /// The <see cref="IsUnifiedArchitecture"/> bit applies to Windows NT 4.0 and above: it breaks the lines of old Windows 
        /// desktop editions (Windows 98, ME) and enables for instance Windows2000 to be considered as greater than WindowsME.
        /// </summary>
        /// <remarks>
        /// Of course, feature detection is always a best choice, but for simple tests this is enough and keeps things simple.
        /// </remarks>
        public enum SimpleOSLevel
        {
            /// <summary>
            /// Unknown Operating System.
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// Good old Windows 3.1 (just for fun).
            /// </summary>
            Windows31 = 199203,
            /// <summary>
            /// Windows CE (just for fun).
            /// </summary>
            WindowsCE = 199611,
            /// <summary>
            /// Windows 95.
            /// </summary>
            Windows95 = 199508,
            /// <summary>
            /// Importatnt release.
            /// </summary>
            Windows95R2 = 199608,
            /// <summary>
            /// Windows 95.
            /// </summary>
            Windows98 = 199805,
            /// <summary>
            /// Importatnt release.
            /// </summary>
            Windows98SecondEdition = 199904,
            /// <summary>
            /// Last version of the historical windows kernel.
            /// </summary>
            WindowsME = 200012,
            /// <summary>
            /// Based on VMS: the new preemptive multi-tasking kernel.
            /// </summary>
            WindowsNT351 = 199505,
            /// <summary>
            /// Tags Windows NT 4.0 and above.
            /// </summary>
            IsUnifiedArchitecture = 1<<20,
            /// <summary>
            /// First version of unified kernel between desktop and server systems.
            /// </summary>
            WindowsNT40 = IsUnifiedArchitecture | 199607,
            /// <summary>
            /// Server version.
            /// </summary>
            WindowsNT40Server = IsUnifiedArchitecture | 199608,
            /// <summary>
            /// Windows 2000.
            /// </summary>
            Windows2000 = IsUnifiedArchitecture | 200002,
            /// <summary>
            /// Major release (for us and .Net users).
            /// </summary>
            WindowsXP = IsUnifiedArchitecture | 200108,
            /// <summary>
            /// Based on XP kernel.
            /// </summary>
            WindowsServer2003 = IsUnifiedArchitecture | 200304,
            /// <summary>
            /// Commercial failure (but some interesting things inside).
            /// </summary>
            WindowsVista = IsUnifiedArchitecture | 200701,
            /// <summary>
            /// Server version based on Vista kernel.
            /// </summary>
            WindowsServer2008 = IsUnifiedArchitecture | 200802,
            /// <summary>
            /// Windows 7.
            /// </summary>
            Windows7 = IsUnifiedArchitecture | 200907,
            /// <summary>
            /// Server version based on Windows 7 kernel.
            /// </summary>
            WindowsServer2008R2 = IsUnifiedArchitecture | 200911,
            /// <summary>
            /// Current version.
            /// </summary>
            Windows8 = IsUnifiedArchitecture | 201208,
            /// <summary>
            /// Current server version.
            /// </summary>
            WindowsServer2012 = IsUnifiedArchitecture | 201211
        }

        static SimpleOSLevel _osLevel;

        /// <summary>
        /// Gets a simple, time-based, operating system level. <see cref="SimpleOSLevel"/>.
        /// Use <see cref="OSLevelDisplayName"/> for the associated display name.
        /// </summary>
        public static SimpleOSLevel OSLevel
        {
            get
            {
                if( _osLevel == SimpleOSLevel.Unknown )
                {
                    if( PInvokeSupported )
                    {
                        int majorVersion = OSVersion.Version.Major;
                        int minorVersion = OSVersion.Version.Minor;
                        OSVERSIONINFOEX? vEx = GetVersionExIfPossible();
                        switch( OSVersion.Platform )
                        {
                            case PlatformID.Win32S:
                                _osLevel = SimpleOSLevel.Windows31;
                                break;
                            case PlatformID.WinCE:
                                _osLevel = SimpleOSLevel.WindowsCE;
                                break;
                            case PlatformID.Win32Windows:
                                {
                                    if( majorVersion == 4 )
                                    {
                                        switch( minorVersion )
                                        {
                                            case 0:
                                                if( vEx.HasValue && (vEx.Value.szCSDVersion == "B" || vEx.Value.szCSDVersion == "C") )
                                                    _osLevel = SimpleOSLevel.Windows95R2;
                                                else
                                                    _osLevel = SimpleOSLevel.Windows95;
                                                break;
                                            case 10:
                                                if( vEx.HasValue && vEx.Value.szCSDVersion == "A" )
                                                    _osLevel = SimpleOSLevel.Windows98SecondEdition;
                                                else
                                                    _osLevel = SimpleOSLevel.Windows98;
                                                break;
                                            case 90:
                                                _osLevel = SimpleOSLevel.WindowsME;
                                                break;
                                        }
                                    }
                                    break;
                                }
                            case PlatformID.Win32NT:
                                {
                                    //byte productType = vEx.Value.wProductType;

                                    switch( majorVersion )
                                    {
                                        case 3:
                                            _osLevel = SimpleOSLevel.WindowsNT351;
                                            break;
                                        case 4:
                                            if( vEx.HasValue && vEx.Value.wProductType == 3 )
                                            {
                                                _osLevel = SimpleOSLevel.WindowsNT40Server;
                                            }
                                            else _osLevel = SimpleOSLevel.WindowsNT40;
                                            break;
                                        case 5:
                                            switch( minorVersion )
                                            {
                                                case 0:
                                                    _osLevel = SimpleOSLevel.Windows2000;
                                                    break;
                                                case 1:
                                                    _osLevel = SimpleOSLevel.WindowsXP;
                                                    break;
                                                case 2:
                                                    _osLevel = SimpleOSLevel.WindowsServer2003;
                                                    break;
                                            }
                                            break;
                                        case 6:
                                            switch( minorVersion )
                                            {
                                                case 0:
                                                    if( vEx.HasValue && vEx.Value.wProductType == 3 )
                                                    {
                                                        _osLevel = SimpleOSLevel.WindowsServer2008;
                                                    }
                                                    else _osLevel = SimpleOSLevel.WindowsVista;
                                                    break;

                                                case 1:
                                                    if( vEx.HasValue && vEx.Value.wProductType == 3 )
                                                    {
                                                        _osLevel = SimpleOSLevel.WindowsServer2008R2;
                                                    }
                                                    else _osLevel = SimpleOSLevel.Windows7;
                                                    break;
                                                case 2:
                                                    if( vEx.HasValue && vEx.Value.wProductType == 3 )
                                                    {
                                                        _osLevel = SimpleOSLevel.WindowsServer2012;
                                                    }
                                                    else _osLevel = SimpleOSLevel.Windows8;
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                }
                        }
                    }
                }
                return _osLevel;
            }
        }

        /// <summary>
        /// Gets a display name of <see cref="OSLevel"/>.
        /// </summary>
        public static string OSLevelDisplayName
        {
            get
            {
                switch( OSLevel )
                {
                    case SimpleOSLevel.Windows31: return "Windows 3.1";
                    case SimpleOSLevel.WindowsCE: return "Windows CE";
                    case SimpleOSLevel.Windows95: return "Windows 95";
                    case SimpleOSLevel.Windows95R2: return "Windows 95 OSR2";
                    case SimpleOSLevel.Windows98: return "Windows 98";
                    case SimpleOSLevel.Windows98SecondEdition: return "Windows 98 Second Edition";
                    case SimpleOSLevel.WindowsME: return "Windows ME";
                    case SimpleOSLevel.WindowsNT351: return "Windows NT 3.51";
                    case SimpleOSLevel.WindowsNT40: return "Windows NT 4.0";
                    case SimpleOSLevel.WindowsNT40Server: return "Windows NT 4.0 Server";
                    case SimpleOSLevel.Windows2000: return "Windows 2000";
                    case SimpleOSLevel.WindowsXP: return "Windows XP";
                    case SimpleOSLevel.WindowsServer2003: return "Windows Server 2003";
                    case SimpleOSLevel.WindowsVista: return "Windows Vista";
                    case SimpleOSLevel.WindowsServer2008: return "Windows Server 2008";
                    case SimpleOSLevel.Windows7: return "Windows 7";
                    case SimpleOSLevel.WindowsServer2008R2: return "Windows Server 2008 R2";
                    case SimpleOSLevel.Windows8: return "Windows 8";
                    case SimpleOSLevel.WindowsServer2012: return "Windows Server 2012";
                    default: return "(unknown)";
                }
            }
        }

        static OSVersionInfo()
        {
            IsMono = Type.GetType( "Mono.Runtime", false ) != null;
            OSVersion = Environment.OSVersion;
            PlatformID platformID = OSVersion.Platform;
            bool isWin32 = platformID == PlatformID.Win32NT || platformID == PlatformID.Win32Windows;
            
            if( Environment.Version.Major == 1 )
            {
                // Mono 1.0:  unix == 128 (No unix on MS.NET 1.x)
                IsUnix = (int)platformID == 128;
            }
            else IsUnix = platformID == PlatformID.Unix || platformID == PlatformID.MacOSX; 

            PInvokeSupported = isWin32 && !IsMono;

            RunningDirectory = System.IO.Path.GetDirectoryName( System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName );

        }

        /// <summary>
        /// Achitecture of the software or underlying operating system.
        /// </summary>
        public enum SoftwareArchitecture
        {
            /// <summary>
            /// Unkown.
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// 32 bits.
            /// </summary>
            Bit32 = 1,
            /// <summary>
            /// 64 bits.
            /// </summary>
            Bit64 = 2
        }

        /// <summary>
        /// Underlying achitecture of the hardware.
        /// </summary>
        public enum ProcessorArchitecture
        {
            /// <summary>
            /// Unknown
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// 32 bits
            /// </summary>
            Bit32 = 1,
            /// <summary>
            /// 64 bits
            /// </summary>
            Bit64 = 2,
            /// <summary>
            /// Itanium
            /// </summary>
            Itanium64 = 3
        }

        /// <summary>
        /// Determines if the current process runs in 32 or 64-bit.
        /// </summary>
        static public SoftwareArchitecture ProcessBits
        {
            get { return IntPtr.Size == 8 ? SoftwareArchitecture.Bit64 : SoftwareArchitecture.Bit32; }
        }

        /// <summary>
        /// Determines if the Operating System is 32 or 64 bits (regardless of the 
        /// current <see cref="ProcessBits"/>: we may be running a 32 bits process on a 64 bits OS).
        /// </summary>
        static public SoftwareArchitecture OSBits
        {
            get
            {
                SoftwareArchitecture osbits = SoftwareArchitecture.Unknown;
                switch( IntPtr.Size * 8 )
                {
                    case 64:
                        osbits = SoftwareArchitecture.Bit64;
                        break;

                    case 32:
                        if( Is32BitProcessOn64BitProcessor() )
                            osbits = SoftwareArchitecture.Bit64;
                        else
                            osbits = SoftwareArchitecture.Bit32;
                        break;
                }
                return osbits;
            }
        }

        /// <summary>
        /// Gets the <see cref="ProcessorArchitecture"/>.
        /// </summary>
        static public ProcessorArchitecture ProcessorBits
        {
            get
            {
                ProcessorArchitecture pbits = ProcessorArchitecture.Unknown;
                if( PInvokeSupported )
                {
                    try
                    {
                        SYSTEM_INFO l_System_Info = new SYSTEM_INFO();
                        GetNativeSystemInfo( ref l_System_Info );

                        switch( l_System_Info.uProcessorInfo.wProcessorArchitecture )
                        {
                            case 9: // PROCESSOR_ARCHITECTURE_AMD64
                                pbits = ProcessorArchitecture.Bit64;
                                break;
                            case 6: // PROCESSOR_ARCHITECTURE_IA64
                                pbits = ProcessorArchitecture.Itanium64;
                                break;
                            case 0: // PROCESSOR_ARCHITECTURE_INTEL
                                pbits = ProcessorArchitecture.Bit32;
                                break;
                            default: // PROCESSOR_ARCHITECTURE_UNKNOWN
                                pbits = ProcessorArchitecture.Unknown;
                                break;
                        }
                    }
                    catch {}
                }
                return pbits;
            }
        }

        #region OSVERSIONINFOEX
        [StructLayout( LayoutKind.Sequential )]
        private struct OSVERSIONINFOEX
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs( UnmanagedType.ByValTStr, SizeConst = 128 )]
            public string szCSDVersion;
            public short wServicePackMajor;
            public short wServicePackMinor;
            public short wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }

        private static OSVERSIONINFOEX? GetVersionExIfPossible()
        {
            if( PInvokeSupported )
            {
                OSVERSIONINFOEX o = new OSVERSIONINFOEX();
                o.dwOSVersionInfoSize = Marshal.SizeOf( typeof( OSVERSIONINFOEX ) );
                if( GetVersionEx( ref o ) ) return o;
            }
            return null;
        }

        [DllImport( "kernel32.dll" )]
        private static extern bool GetVersionEx( ref OSVERSIONINFOEX osVersionInfo );

        #endregion OSVERSIONINFOEX

        #region SYSTEM_INFO
        [StructLayout( LayoutKind.Sequential )]
        internal struct SYSTEM_INFO
        {
            internal PROCESSOR_INFO_UNION uProcessorInfo;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort dwProcessorLevel;
            public ushort dwProcessorRevision;
        }

        [DllImport( "kernel32.dll" )]
        internal static extern void GetNativeSystemInfo( [MarshalAs( UnmanagedType.Struct )] ref SYSTEM_INFO lpSystemInfo );
        #endregion SYSTEMINFO

        #region 64 bits detection
        [StructLayout( LayoutKind.Explicit )]
        internal struct PROCESSOR_INFO_UNION
        {
            [FieldOffset( 0 )]
            internal uint dwOemId;
            [FieldOffset( 0 )]
            internal ushort wProcessorArchitecture;
            [FieldOffset( 2 )]
            internal ushort wReserved;
        }

        [DllImport( "kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi )]
        internal extern static IntPtr LoadLibrary( string libraryName );

        [DllImport( "kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi )]
        internal extern static IntPtr GetProcAddress( IntPtr hwnd, string procedureName );
        
        delegate bool IsWow64ProcessDelegate( [In]IntPtr handle, [Out]out bool isWow64Process );

        private static IsWow64ProcessDelegate GetIsWow64ProcessDelegate()
        {
            // Do not free it: we always need it anyway.
            IntPtr handle = LoadLibrary( "kernel32" );
            if( handle != IntPtr.Zero )
            {
                // This entry point may not exist.
                IntPtr e = GetProcAddress( handle, "IsWow64Process" );
                if( e != IntPtr.Zero )
                {
                    return (IsWow64ProcessDelegate)Marshal.GetDelegateForFunctionPointer( e, typeof( IsWow64ProcessDelegate ) );
                }
            }

            return null;
        }

        private static bool Is32BitProcessOn64BitProcessor()
        {
            IsWow64ProcessDelegate f = GetIsWow64ProcessDelegate();
            if( f == null ) return false;
            bool isWow64;
            bool retVal = f.Invoke( Process.GetCurrentProcess().Handle, out isWow64 );
            return retVal && isWow64;
        }

        #endregion 64 BIT OS DETECTION

        /* Removed "Edition". Seems useless.
        static private string _osEdition;

        /// <summary>
        /// Gets the edition of the operating system running on this computer.
        /// </summary>
        static public string Edition
        {
            get
            {
                if( _osEdition == null )
                {
                    string edition = String.Empty;
                    
                    OperatingSystem osVersion = Environment.OSVersion;
                    OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
                    osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf( typeof( OSVERSIONINFOEX ) );
                    if( GetVersionEx( ref osVersionInfo ) )
                    {
                        int majorVersion = osVersion.Version.Major;
                        int minorVersion = osVersion.Version.Minor;
                        byte productType = osVersionInfo.wProductType;
                        short suiteMask = osVersionInfo.wSuiteMask;

                        #region VERSION 4
                        if( majorVersion == 4 )
                        {
                            if( productType == VER_NT_WORKSTATION )
                            {
                                // Windows NT 4.0 Workstation
                                edition = "Workstation";
                            }
                            else if( productType == VER_NT_SERVER )
                            {
                                if( (suiteMask & VER_SUITE_ENTERPRISE) != 0 )
                                {
                                    // Windows NT 4.0 Server Enterprise
                                    edition = "Enterprise Server";
                                }
                                else
                                {
                                    // Windows NT 4.0 Server
                                    edition = "Standard Server";
                                }
                            }
                        }
                        #endregion VERSION 4

                        #region VERSION 5
                        else if( majorVersion == 5 )
                        {
                            if( productType == VER_NT_WORKSTATION )
                            {
                                if( (suiteMask & VER_SUITE_PERSONAL) != 0 )
                                {
                                    edition = "Home";
                                }
                                else
                                {
                                    if( GetSystemMetrics( 86 ) == 0 ) // 86 == SM_TABLETPC
                                        edition = "Professional";
                                    else
                                        edition = "Tablet Edition";
                                }
                            }
                            else if( productType == VER_NT_SERVER )
                            {
                                if( minorVersion == 0 )
                                {
                                    if( (suiteMask & VER_SUITE_DATACENTER) != 0 )
                                    {
                                        // Windows 2000 Datacenter Server
                                        edition = "Datacenter Server";
                                    }
                                    else if( (suiteMask & VER_SUITE_ENTERPRISE) != 0 )
                                    {
                                        // Windows 2000 Advanced Server
                                        edition = "Advanced Server";
                                    }
                                    else
                                    {
                                        // Windows 2000 Server
                                        edition = "Server";
                                    }
                                }
                                else
                                {
                                    if( (suiteMask & VER_SUITE_DATACENTER) != 0 )
                                    {
                                        // Windows Server 2003 Datacenter Edition
                                        edition = "Datacenter";
                                    }
                                    else if( (suiteMask & VER_SUITE_ENTERPRISE) != 0 )
                                    {
                                        // Windows Server 2003 Enterprise Edition
                                        edition = "Enterprise";
                                    }
                                    else if( (suiteMask & VER_SUITE_BLADE) != 0 )
                                    {
                                        // Windows Server 2003 Web Edition
                                        edition = "Web Edition";
                                    }
                                    else
                                    {
                                        // Windows Server 2003 Standard Edition
                                        edition = "Standard";
                                    }
                                }
                            }
                        }
                        #endregion VERSION 5

                        #region VERSION 6
                        else if( majorVersion == 6 )
                        {
                            int ed;
                            if( GetProductInfo( majorVersion, minorVersion,
                                                osVersionInfo.wServicePackMajor, osVersionInfo.wServicePackMinor,
                                                out ed ) )
                            {
                                switch( (uint)ed )
                                {
                                    case PRODUCT_BUSINESS:
                                        edition = "Business";
                                        break;
                                    case PRODUCT_BUSINESS_N:
                                        edition = "Business N";
                                        break;
                                    case PRODUCT_CLUSTER_SERVER:
                                        edition = "HPC Edition";
                                        break;
                                    case PRODUCT_CLUSTER_SERVER_V:
                                        edition = "HPC Edition without Hyper-V";
                                        break;
                                    case PRODUCT_DATACENTER_SERVER:
                                        edition = "Datacenter Server";
                                        break;
                                    case PRODUCT_DATACENTER_SERVER_CORE:
                                        edition = "Datacenter Server (core installation)";
                                        break;
                                    case PRODUCT_DATACENTER_SERVER_V:
                                        edition = "Datacenter Server without Hyper-V";
                                        break;
                                    case PRODUCT_DATACENTER_SERVER_CORE_V:
                                        edition = "Datacenter Server without Hyper-V (core installation)";
                                        break;
                                    case PRODUCT_EMBEDDED:
                                        edition = "Embedded";
                                        break;
                                    case PRODUCT_ENTERPRISE:
                                        edition = "Enterprise";
                                        break;
                                    case PRODUCT_ENTERPRISE_N:
                                        edition = "Enterprise N";
                                        break;
                                    case PRODUCT_ENTERPRISE_E:
                                        edition = "Enterprise E";
                                        break;
                                    case PRODUCT_ENTERPRISE_SERVER:
                                        edition = "Enterprise Server";
                                        break;
                                    case PRODUCT_ENTERPRISE_SERVER_CORE:
                                        edition = "Enterprise Server (core installation)";
                                        break;
                                    case PRODUCT_ENTERPRISE_SERVER_CORE_V:
                                        edition = "Enterprise Server without Hyper-V (core installation)";
                                        break;
                                    case PRODUCT_ENTERPRISE_SERVER_IA64:
                                        edition = "Enterprise Server for Itanium-based Systems";
                                        break;
                                    case PRODUCT_ENTERPRISE_SERVER_V:
                                        edition = "Enterprise Server without Hyper-V";
                                        break;
                                    case PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT:
                                        edition = "Essential Business Server MGMT";
                                        break;
                                    case PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL:
                                        edition = "Essential Business Server ADDL";
                                        break;
                                    case PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC:
                                        edition = "Essential Business Server MGMTSVC";
                                        break;
                                    case PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC:
                                        edition = "Essential Business Server ADDLSVC";
                                        break;
                                    case PRODUCT_HOME_BASIC:
                                        edition = "Home Basic";
                                        break;
                                    case PRODUCT_HOME_BASIC_N:
                                        edition = "Home Basic N";
                                        break;
                                    case PRODUCT_HOME_BASIC_E:
                                        edition = "Home Basic E";
                                        break;
                                    case PRODUCT_HOME_PREMIUM:
                                        edition = "Home Premium";
                                        break;
                                    case PRODUCT_HOME_PREMIUM_N:
                                        edition = "Home Premium N";
                                        break;
                                    case PRODUCT_HOME_PREMIUM_E:
                                        edition = "Home Premium E";
                                        break;
                                    case PRODUCT_HOME_PREMIUM_SERVER:
                                        edition = "Home Premium Server";
                                        break;
                                    case PRODUCT_HYPERV:
                                        edition = "Microsoft Hyper-V Server";
                                        break;
                                    case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
                                        edition = "Windows Essential Business Management Server";
                                        break;
                                    case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
                                        edition = "Windows Essential Business Messaging Server";
                                        break;
                                    case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
                                        edition = "Windows Essential Business Security Server";
                                        break;
                                    case PRODUCT_PROFESSIONAL:
                                        edition = "Professional";
                                        break;
                                    case PRODUCT_PROFESSIONAL_N:
                                        edition = "Professional N";
                                        break;
                                    case PRODUCT_PROFESSIONAL_E:
                                        edition = "Professional E";
                                        break;
                                    case PRODUCT_SB_SOLUTION_SERVER:
                                        edition = "SB Solution Server";
                                        break;
                                    case PRODUCT_SB_SOLUTION_SERVER_EM:
                                        edition = "SB Solution Server EM";
                                        break;
                                    case PRODUCT_SERVER_FOR_SB_SOLUTIONS:
                                        edition = "Server for SB Solutions";
                                        break;
                                    case PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM:
                                        edition = "Server for SB Solutions EM";
                                        break;
                                    case PRODUCT_SERVER_FOR_SMALLBUSINESS:
                                        edition = "Windows Essential Server Solutions";
                                        break;
                                    case PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
                                        edition = "Windows Essential Server Solutions without Hyper-V";
                                        break;
                                    case PRODUCT_SERVER_FOUNDATION:
                                        edition = "Server Foundation";
                                        break;
                                    case PRODUCT_SMALLBUSINESS_SERVER:
                                        edition = "Windows Small Business Server";
                                        break;
                                    case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM:
                                        edition = "Windows Small Business Server Premium";
                                        break;
                                    case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE:
                                        edition = "Windows Small Business Server Premium (core installation)";
                                        break;
                                    case PRODUCT_SOLUTION_EMBEDDEDSERVER:
                                        edition = "Solution Embedded Server";
                                        break;
                                    case PRODUCT_SOLUTION_EMBEDDEDSERVER_CORE:
                                        edition = "Solution Embedded Server (core installation)";
                                        break;
                                    case PRODUCT_STANDARD_SERVER:
                                        edition = "Standard Server";
                                        break;
                                    case PRODUCT_STANDARD_SERVER_CORE:
                                        edition = "Standard Server (core installation)";
                                        break;
                                    case PRODUCT_STANDARD_SERVER_SOLUTIONS:
                                        edition = "Standard Server Solutions";
                                        break;
                                    case PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE:
                                        edition = "Standard Server Solutions (core installation)";
                                        break;
                                    case PRODUCT_STANDARD_SERVER_CORE_V:
                                        edition = "Standard Server without Hyper-V (core installation)";
                                        break;
                                    case PRODUCT_STANDARD_SERVER_V:
                                        edition = "Standard Server without Hyper-V";
                                        break;
                                    case PRODUCT_STARTER:
                                        edition = "Starter";
                                        break;
                                    case PRODUCT_STARTER_N:
                                        edition = "Starter N";
                                        break;
                                    case PRODUCT_STARTER_E:
                                        edition = "Starter E";
                                        break;
                                    case PRODUCT_STORAGE_ENTERPRISE_SERVER:
                                        edition = "Enterprise Storage Server";
                                        break;
                                    case PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE:
                                        edition = "Enterprise Storage Server (core installation)";
                                        break;
                                    case PRODUCT_STORAGE_EXPRESS_SERVER:
                                        edition = "Express Storage Server";
                                        break;
                                    case PRODUCT_STORAGE_EXPRESS_SERVER_CORE:
                                        edition = "Express Storage Server (core installation)";
                                        break;
                                    case PRODUCT_STORAGE_STANDARD_SERVER:
                                        edition = "Standard Storage Server";
                                        break;
                                    case PRODUCT_STORAGE_STANDARD_SERVER_CORE:
                                        edition = "Standard Storage Server (core installation)";
                                        break;
                                    case PRODUCT_STORAGE_WORKGROUP_SERVER:
                                        edition = "Workgroup Storage Server";
                                        break;
                                    case PRODUCT_STORAGE_WORKGROUP_SERVER_CORE:
                                        edition = "Workgroup Storage Server (core installation)";
                                        break;
                                    case PRODUCT_UNDEFINED:
                                        edition = "Unknown product";
                                        break;
                                    case PRODUCT_ULTIMATE:
                                        edition = "Ultimate";
                                        break;
                                    case PRODUCT_ULTIMATE_N:
                                        edition = "Ultimate N";
                                        break;
                                    case PRODUCT_ULTIMATE_E:
                                        edition = "Ultimate E";
                                        break;
                                    case PRODUCT_WEB_SERVER:
                                        edition = "Web Server";
                                        break;
                                    case PRODUCT_WEB_SERVER_CORE:
                                        edition = "Web Server (core installation)";
                                        break;
                                }
                            }
                        }
                        #endregion VERSION 6
                    }
                    _osEdition = edition;
                }
                return _osEdition;
            }
        }

        #region PRODUCT INFO
        [DllImport( "Kernel32.dll" )]
        internal static extern bool GetProductInfo(
            int osMajorVersion,
            int osMinorVersion,
            int spMajorVersion,
            int spMinorVersion,
            out int edition );
         
        [DllImport( "user32" )]
        internal static extern int GetSystemMetrics( int nIndex );
        
        [DllImport( "kernel32.dll" )]
        internal static extern void GetSystemInfo( [MarshalAs( UnmanagedType.Struct )] ref SYSTEM_INFO lpSystemInfo );
         * 
        #region PRODUCT (updated on april 2013)
        private const uint PRODUCT_BUSINESS = 0x00000006;
        private const uint PRODUCT_BUSINESS_N = 0x00000010;
        private const uint PRODUCT_CLUSTER_SERVER = 0x00000012;
        private const uint PRODUCT_CLUSTER_SERVER_V = 0x00000040;
        private const uint PRODUCT_CORE = 0x00000065;
        private const uint PRODUCT_CORE_N = 0x00000062;
        private const uint PRODUCT_CORE_COUNTRYSPECIFIC = 0x00000063;
        private const uint PRODUCT_CORE_SINGLELANGUAGE = 0x00000064;
        private const uint PRODUCT_DATACENTER_EVALUATION_SERVER = 0x00000050;
        private const uint PRODUCT_DATACENTER_SERVER = 0x00000008;
        private const uint PRODUCT_DATACENTER_SERVER_CORE = 0x0000000C;
        private const uint PRODUCT_DATACENTER_SERVER_CORE_V = 0x00000027;
        private const uint PRODUCT_DATACENTER_SERVER_V = 0x00000025;
        private const uint PRODUCT_ENTERPRISE = 0x00000004;
        private const uint PRODUCT_ENTERPRISE_E = 0x00000046;
        private const uint PRODUCT_ENTERPRISE_N_EVALUATION = 0x00000054;
        private const uint PRODUCT_ENTERPRISE_N = 0x0000001B;
        private const uint PRODUCT_ENTERPRISE_EVALUATION = 0x00000048;
        private const uint PRODUCT_ENTERPRISE_SERVER = 0x0000000A;
        private const uint PRODUCT_ENTERPRISE_SERVER_CORE = 0x0000000E;
        private const uint PRODUCT_ENTERPRISE_SERVER_CORE_V = 0x00000029;
        private const uint PRODUCT_ENTERPRISE_SERVER_IA64 = 0x0000000F;
        private const uint PRODUCT_ENTERPRISE_SERVER_V = 0x00000026;
        private const uint PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT = 0x0000003B;
        private const uint PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL = 0x0000003C;
        private const uint PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC = 0x0000003D;
        private const uint PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC = 0x0000003E;
        private const uint PRODUCT_HOME_BASIC = 0x00000002;
        private const uint PRODUCT_HOME_BASIC_E = 0x00000043;
        private const uint PRODUCT_HOME_BASIC_N = 0x00000005;
        private const uint PRODUCT_HOME_PREMIUM = 0x00000003;
        private const uint PRODUCT_HOME_PREMIUM_E = 0x00000044;
        private const uint PRODUCT_HOME_PREMIUM_N = 0x0000001A;
        private const uint PRODUCT_HOME_PREMIUM_SERVER = 0x00000022;
        private const uint PRODUCT_HOME_SERVER = 0x00000013;
        private const uint PRODUCT_HYPERV = 0x0000002A;
        private const uint PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 0x0000001E;
        private const uint PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 0x00000020;
        private const uint PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 0x0000001F;
        private const uint PRODUCT_MULTIPOINT_STANDARD_SERVER = 0x0000004C;
        private const uint PRODUCT_MULTIPOINT_PREMIUM_SERVER = 0x0000004D;
        private const uint PRODUCT_PROFESSIONAL = 0x00000030;
        private const uint PRODUCT_PROFESSIONAL_E = 0x00000045;
        private const uint PRODUCT_PROFESSIONAL_N = 0x00000031;
        private const uint PRODUCT_PROFESSIONAL_WMC = 0x00000067;
        private const uint PRODUCT_SB_SOLUTION_SERVER_EM = 0x00000036;
        private const uint PRODUCT_SERVER_FOR_SB_SOLUTIONS = 0x00000033;
        private const uint PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM = 0x00000037;
        private const uint PRODUCT_SERVER_FOR_SMALLBUSINESS = 0x00000018;
        private const uint PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 0x00000023;
        private const uint PRODUCT_SERVER_FOUNDATION = 0x00000021;
        private const uint PRODUCT_SB_SOLUTION_SERVER = 0x00000032;
        private const uint PRODUCT_SMALLBUSINESS_SERVER = 0x00000009;
        private const uint PRODUCT_SMALLBUSINESS_SERVER_PREMIUM = 0x00000019;
        private const uint PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE = 0x0000003F;
        private const uint PRODUCT_SOLUTION_EMBEDDEDSERVER = 0x00000038;
        private const uint PRODUCT_STANDARD_EVALUATION_SERVER = 0x0000004F;
        private const uint PRODUCT_STANDARD_SERVER = 0x00000007;
        private const uint PRODUCT_STANDARD_SERVER_CORE = 0x0000000D;
        private const uint PRODUCT_STANDARD_SERVER_V = 0x00000024;
        private const uint PRODUCT_STANDARD_SERVER_CORE_V = 0x00000028;
        private const uint PRODUCT_STANDARD_SERVER_SOLUTIONS = 0x00000034;
        private const uint PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE = 0x00000035;
        private const uint PRODUCT_STARTER = 0x0000000B;
        private const uint PRODUCT_STARTER_E = 0x00000042;
        private const uint PRODUCT_STARTER_N = 0x0000002F;
        private const uint PRODUCT_STORAGE_ENTERPRISE_SERVER = 0x00000017;
        private const uint PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE = 0x0000002E;
        private const uint PRODUCT_STORAGE_EXPRESS_SERVER = 0x00000014;
        private const uint PRODUCT_STORAGE_EXPRESS_SERVER_CORE = 0x0000002B;
        private const uint PRODUCT_STORAGE_STANDARD_EVALUATION_SERVER = 0x00000060;
        private const uint PRODUCT_STORAGE_STANDARD_SERVER = 0x00000015;
        private const uint PRODUCT_STORAGE_STANDARD_SERVER_CORE = 0x0000002C;
        private const uint PRODUCT_STORAGE_WORKGROUP_EVALUATION_SERVER = 0x0000005F;
        private const uint PRODUCT_STORAGE_WORKGROUP_SERVER = 0x00000016;
        private const uint PRODUCT_STORAGE_WORKGROUP_SERVER_CORE = 0x0000002D;
        private const uint PRODUCT_UNDEFINED = 0x00000000;
        private const uint PRODUCT_ULTIMATE = 0x00000001;
        private const uint PRODUCT_ULTIMATE_E = 0x00000047;
        private const uint PRODUCT_ULTIMATE_N = 0x0000001C;
        private const uint PRODUCT_WEB_SERVER = 0x00000011;
        private const uint PRODUCT_WEB_SERVER_CORE = 0x0000001D;

        // Old code (it seems to be).
        private const uint PRODUCT_SOLUTION_EMBEDDEDSERVER_CORE = 0x00000039;
        private const uint PRODUCT_EMBEDDED = 0x00000041;
        private const uint PRODUCT_UNLICENSED = 0xABCDABCD;
        #endregion PRODUCT

        #region VERSIONS
        private const int VER_NT_WORKSTATION = 1;
        private const int VER_NT_DOMAIN_CONTROLLER = 2;
        private const int VER_NT_SERVER = 3;
        private const int VER_SUITE_SMALLBUSINESS = 1;
        private const int VER_SUITE_ENTERPRISE = 2;
        private const int VER_SUITE_TERMINAL = 16;
        private const int VER_SUITE_DATACENTER = 128;
        private const int VER_SUITE_SINGLEUSERTS = 256;
        private const int VER_SUITE_PERSONAL = 512;
        private const int VER_SUITE_BLADE = 1024;
        #endregion VERSIONS
        
        */

    }
}
