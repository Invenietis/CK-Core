#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\GrandOutput.XAppDomain.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Remoting.Lifetime;
//using System.Text;
//using System.Threading.Tasks;

//namespace CK.Monitoring
//{
//    public sealed partial class GrandOutput
//    {

//        public class BridgeTarget : MarshalByRefObject, IGrandOutputBridgeTarget, ISponsor
//        {
//            readonly GrandOutput _grandOutput;

//            internal BridgeTarget( GrandOutput g )
//            {
//                _grandOutput = g;
//            }

//            public override object InitializeLifetimeService()
//            {
//                ILease lease = (ILease)base.InitializeLifetimeService();
//                if( lease.CurrentState == LeaseState.Initial )
//                {
//                    lease.Register( this );
//                }
//                return lease;
//            }

//            public TimeSpan Renewal( ILease lease )
//            {
//                return _grandOutput._channelHost.IsDisposed ? TimeSpan.Zero : lease.InitialLeaseTime;
//            }
//        }

//        public void SetDomainTarget( IGrandOutputBridgeTarget target )
//        {
//        }
//    }
//}
