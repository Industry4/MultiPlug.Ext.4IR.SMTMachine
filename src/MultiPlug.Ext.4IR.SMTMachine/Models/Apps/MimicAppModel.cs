
namespace MultiPlug.Ext._4IR.SMTMachine.Models.Apps
{
    public class MimicAppModel
    {
        public int TransportStateId { get; internal set; }
        public string TransportStateDescription { get; internal set; }
        public int CoverStateId { get; internal set; }
        public string CoverStateDescription { get; internal set; }
        public int DurationTransportIn { get; set; }
        public int DurationProcessing { get; set; }
        public int DurationTransportOut { get; set; }
        public int DurationReset { get; set; }
        public string WidthDescription { get; internal set; }
        public int AutomaticAdjustmentsSpeed { get; internal set; }
        public bool AutomaticAdjustmentsFeedback { get; internal set; }
    }
}
