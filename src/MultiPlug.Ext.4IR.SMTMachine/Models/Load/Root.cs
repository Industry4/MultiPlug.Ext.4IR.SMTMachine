using System.Runtime.Serialization;

namespace MultiPlug.Ext._4IR.SMTMachine.Models.Load
{
    public class Root
    {
        [DataMember]
        public MachineProperties Machine { get; set; }
    }
}
