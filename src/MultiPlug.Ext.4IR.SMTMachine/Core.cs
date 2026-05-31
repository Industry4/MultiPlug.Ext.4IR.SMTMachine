using MultiPlug.Base;
using MultiPlug.Base.Exchange;
using MultiPlug.Ext._4IR.SMTMachine.Components.Machine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
    }
}
