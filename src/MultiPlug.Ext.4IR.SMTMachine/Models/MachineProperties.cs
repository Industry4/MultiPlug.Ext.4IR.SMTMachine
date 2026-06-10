using MultiPlug.Base;
using MultiPlug.Base.Exchange;
using MultiPlug.Ext._4IR.SMTMachine.Models.Exchange;
using System.Runtime.Serialization;

namespace MultiPlug.Ext._4IR.SMTMachine.Models
{
    public class MachineProperties : MultiPlugBase
    {
        [DataMember]
        public ReadyEvent SMEMAUpstreamMachineReadyEvent { get; set; }
        [DataMember]
        public ReadySubscription SMEMAUpstreamGoodBoardAvailableSubscription { get; set; }
        [DataMember]
        public ReadySubscription SMEMAUpstreamBadBoardAvailableSubscription { get; set; }
        [DataMember]
        public ReadyEvent SMEMADownstreamGoodBoardAvailableEvent { get; set; }
        [DataMember]
        public ReadyEvent SMEMADownstreamBadBoardAvailableEvent { get; set; }
        [DataMember]
        public ReadySubscription SMEMADownstreamMachineReadySubscription { get; set; }
        [DataMember]
        public Event TransportStateEvent { get; set; }
        [DataMember]
        public Event CoverStateEvent { get; set; }
        [DataMember]
        public int? DurationTransportIn { get; set; }
        [DataMember]
        public int? DurationProcessing { get; set; }
        [DataMember]
        public int? DurationTransportOut { get; set; }
        [DataMember]
        public int? DurationReset { get; set; }
        [DataMember]
        public Subscription AutomaticAdjustmentsSubscription { get; set; }
        [DataMember]
        public AdjustmentsEvent AutomaticAdjustmentsEvent { get; set; }
        [DataMember]
        public Event WidthStateEvent { get; set; }
        [DataMember]
        public int? AutomaticAdjustmentsSpeed { get; set; }
        [DataMember]
        public bool? AutomaticAdjustmentsFeedback { get; set; }
    }
}
