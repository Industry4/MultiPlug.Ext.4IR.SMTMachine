using System.Runtime.Serialization;
using MultiPlug.Base;
using MultiPlug.Ext._4IR.SMTMachine.Components.Machine;
using MultiPlug.Ext._4IR.SMTMachine.Models.Load;

namespace MultiPlug.Ext._4IR.SMTMachine
{
    public class Core : MultiPlugBase
    {
        private static Core m_Instance = null;
        public static Core Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new Core();
                }
                return m_Instance;
            }
        }

        private Core()
        {

        }

        [DataMember]
        public MachineComponent Machine = new MachineComponent();

        internal void Load(Root config)
        {
            if(config != null && config.Machine != null)
            {
                Machine.UpdateProperties(config.Machine);
            }
        }
    }
}
