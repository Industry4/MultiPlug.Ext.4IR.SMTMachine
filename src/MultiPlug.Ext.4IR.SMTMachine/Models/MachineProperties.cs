using MultiPlug.Base;
using MultiPlug.Base.Exchange;
using System.Runtime.Serialization;

namespace MultiPlug.Ext._4IR.SMTMachine.Models
{
    public class MachineProperties : MultiPlugBase
    {
        [DataMember]
        public Event SMEMAUpstreamMachineReadyEvent { get; set; }
        [DataMember]
        public Subscription SMEMAUpstreamGoodBoardAvailableSubscription { get; set; }
        [DataMember]
        public Subscription SMEMAUpstreamBadBoardAvailableSubscription { get; set; }
        [DataMember]
        public Event SMEMADownstreamGoodBoardAvailableEvent { get; set; }
        [DataMember]
        public Event SMEMADownstreamBadBoardAvailableEvent { get; set; }
        [DataMember]
        public Subscription SMEMADownstreamMachineReadySubscription { get; set; }
    }
}
