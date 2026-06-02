
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
    }
}
