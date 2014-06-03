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
