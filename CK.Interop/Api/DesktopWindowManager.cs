#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Interop\Api\DesktopWindowManager.cs) is part of CiviKey. 
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
using System.Runtime.InteropServices;

namespace CK.Interop
{
    public static class Dwm
    {
        public static DwmApi Functions = PInvoker.GetInvoker<DwmApi>();

        public enum ThumbnailFlags : uint
        {
            /// <summary>
            /// Indicates a value for rcDestination has been specified.
            /// </summary>
            DWM_TNP_RECTDESTINATION = 0x00000001, 
            /// <summary>
            /// Indicates a value for rcSource has been specified.
            /// </summary>
            DWM_TNP_RECTSOURCE = 0x00000002,
            /// <summary>
            /// Indicates a value for opacity has been specified.
            /// </summary>
            DWM_TNP_OPACITY = 0x00000004, 
            /// <summary>
            /// Indicates a value for fVisible has been specified.
            /// </summary>
            DWM_TNP_VISIBLE = 0x00000008,
            /// <summary>
            /// Indicates a value for fSourceClientAreaOnly has been specified.
            /// </summary>
            DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct THUMBNAIL_PROPERTIES
        {
            internal ThumbnailFlags dwFlags;
            internal Win.Rect rcDestination;
            internal Win.Rect rcSource;
            internal byte opacity;
            internal bool fVisible;
            internal bool fSourceClientAreaOnly;
        };

        public enum BlurBehindDwFlags : uint
        {
            /// <summary>
            /// <see cref="BlurBehind.Enable"/> has been specified.
            /// </summary>
            DWM_BB_ENABLE = 0x00000001,
            /// <summary>
            /// <see cref="BlurBehind.RgnBlur"/> has been specified.
            /// </summary>
            DWM_BB_BLURREGION = 0x00000002,
            /// <summary>
            /// <see cref="TransitionOnMaximized"/> has been specified.
            /// </summary>
            DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct BlurBehind
        {
            public BlurBehindDwFlags Flags;
            public bool Enable;
            public IntPtr RgnBlur;
            public bool TransitionOnMaximized;
        };

        /// <summary>
        /// Enable/disable non-client rendering based on window style.
        /// </summary>
        public const int DWMNCRP_USEWINDOWSTYLE = 0;
        
        /// <summary>
        /// Disabled non-client rendering; window style is ignored.
        /// </summary>
        public const int DWMNCRP_DISABLED = 1;
        
        /// <summary>
        /// Enabled non-client rendering; window style is ignored.
        /// </summary>
        public const int DWMNCRP_ENABLED = 2;

        /// <summary>
        /// Enable/disable non-client rendering. Use DWMNCRP_* values.
        /// </summary>
        public const int DWMWA_NCRENDERING_ENABLED = 1;
        
        /// <summary>
        /// Non-client rendering policy. 
        /// </summary>
        public const int DWMWA_NCRENDERING_POLICY = 2;
        
        /// <summary>
        /// Potentially enable/forcibly disable transitions 0 or 1. 
        /// </summary>
        public const int DWMWA_TRANSITIONS_FORCEDISABLED = 3; 

        [StructLayout( LayoutKind.Sequential )]
        public struct UNSIGNED_RATIO
        {
            public UInt32 uiNumerator;
            public UInt32 uiDenominator;
        };

        [StructLayout( LayoutKind.Sequential )]
        public struct PresentParameters
        {
            public int Size;
            public bool Queue;
            public UInt64 RefreshStart;
            public uint Buffer;
            public bool UseSourceRate;
            public UNSIGNED_RATIO Numerator;
        };

        [StructLayout( LayoutKind.Explicit )]
        public struct TimingInfo
        {
            [FieldOffset( 0 )]
            public UInt32 Size;
            
            /// <summary>
            /// Monitor refresh rate.
            /// </summary>
            [FieldOffset( 4 )]
            public UNSIGNED_RATIO rateRefresh;
            
            /// <summary>
            /// Composition rate.
            /// </summary>
            [FieldOffset( 12 )]
            public UNSIGNED_RATIO rateCompose;  
            
            /// <summary>
            /// QPC time at VBlank.
            /// </summary>
            [FieldOffset( 20 )]
            public UInt64 qpcVBlank;

            /// <summary>
            /// DWM refresh counter.
            /// </summary>
            [FieldOffset( 28 )]
            public UInt64 cRefresh;        
            
            /// <summary>
            /// QPC time at a compose time.
            /// </summary>
            [FieldOffset( 36 )]
            public UInt64 qpcCompose;

            /// <summary>
            /// Frame number that was composed at qpcCompose.
            /// </summary>
            [FieldOffset( 44 )]
            public UInt64 cFrame;

            /// <summary>
            /// Refresh count of the frame that was composed at qpcCompose.
            /// </summary>
            [FieldOffset( 52 )]
            public UInt64 cRefreshFrame;

            /// <summary>
            /// The target refresh count of the last frame confirmed completed by the GPU.
            /// </summary>
            [FieldOffset( 60 )]
            public UInt64 cRefreshConfirmed; 

            /// <summary>
            /// The number of outstanding flips.
            /// </summary>
            [FieldOffset( 68 )]
            public UInt32 cFlipsOutstanding;  // 
            
            // Feedback on previous performance only valid on 2nd and subsequent calls

            /// <summary>
            /// Last frame displayed.
            /// </summary>
            [FieldOffset( 72 )]
            public UInt64 cFrameCurrent;
            /// <summary>
            /// Number of frames available but not displayed, used or dropped.
            /// </summary>
            [FieldOffset( 80 )]
            public UInt64 cFramesAvailable;  
            /// <summary>
            /// Source frame number when the following statistics where last cleared.
            /// </summary>
            [FieldOffset( 88 )]
            public UInt64 cFrameCleared;
            /// <summary>
            /// Number of new frames received.
            /// </summary>
            [FieldOffset( 96 )]
            public UInt64 cFramesReceived;    
            /// <summary>
            /// Number of unique frames displayed.
            /// </summary>
            [FieldOffset( 104 )]
            public UInt64 cFramesDisplayed;   
            /// <summary>
            /// Number of rendered frames that were never displayed because composition occured too late
            /// </summary>
            [FieldOffset( 112 )]
            public UInt64 cFramesDropped; 
            /// <summary>
            /// Number of times an old frame was composed when a new frame should have been used but was not available.
            /// </summary>
            [FieldOffset( 120 )]          
            public UInt64 cFramesMissed;  
        };


        [NativeDll( DefaultDllNameGeneric = "DwmApi" )]
        public interface DwmApi
        {
            [DllImport( EntryPoint = "DwmExtendFrameIntoClientArea", PreserveSig = false )]
            void ExtendFrameIntoClientArea( IntPtr hwnd, ref Win.Margins margins );

            [DllImport( EntryPoint = "DwmEnableComposition" )]
            void EnableComposition( bool enabled );

            [DllImport( EntryPoint = "DwmIsCompositionEnabled", PreserveSig = false )]
            bool IsCompositionEnabled();

            [DllImport( EntryPoint = "DwmEnableBlurBehindWindow", PreserveSig = false )]
            void EnableBlurBehindWindow( IntPtr hwnd, ref BlurBehind bb );

            [DllImport( EntryPoint = "DwmGetCompositionTimingInfo", PreserveSig = false )]
            TimingInfo GetCompositionTimingInfo( IntPtr hwnd );

            [DllImport( EntryPoint = "DwmSetWindowAttribute", PreserveSig = false )]
            void SetWindowAttribute( IntPtr hwnd, uint dwAttributeToSet, IntPtr pvAttributeValue, uint cbAttribute );

            [DllImport( EntryPoint = "DwmGetWindowAttribute", PreserveSig = false )]
            void GetWindowAttribute( IntPtr hwnd, uint dwAttributeToGet, IntPtr pvAttributeValue, uint cbAttribute );

            [DllImport( EntryPoint = "DwmSetPresentParameters", PreserveSig = false )]
            void SetPresentParameters( IntPtr hwnd, ref PresentParameters presentParams );

        }

    }

}
