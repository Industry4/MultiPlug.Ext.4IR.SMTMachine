using System;
using System.Threading.Tasks;
using MultiPlug.Base.Exchange;
using MultiPlug.Ext._4IR.SMTMachine.Models;

namespace MultiPlug.Ext._4IR.SMTMachine.Components.Machine
{
    public class MachineComponent : MachineProperties
    {
        internal enum ProcessState
        {
            Unknown,
            MachineReady,
            TransportingIn,
            Processing,
            WaitForMachineReady,
            TransportingOut,
            Reset
        }

        private ProcessState m_TransportState = ProcessState.Unknown;

        public MachineComponent()
        {
            SMEMAUpstreamMachineReadyEvent = new Event { Guid = Guid.NewGuid().ToString(), Id = "SMTMachine.MachineReady", Description = "Machine Ready", Subjects = new string[] { "state" }, Group = "Machine" };
            SMEMADownstreamGoodBoardAvailableEvent = new Event { Guid = Guid.NewGuid().ToString(), Id = "SMTMachine.GoodBoardAvailable", Description = "Good Board Available", Subjects = new string[] { "state" }, Group = "Machine" };
            SMEMADownstreamBadBoardAvailableEvent = new Event { Guid = Guid.NewGuid().ToString(), Id = "SMTMachine.BadBoardAvailable", Description = "Bad Board Available", Subjects = new string[] { "state" }, Group = "Machine" };

            SMEMAUpstreamGoodBoardAvailableSubscription = new Subscription(Guid.NewGuid().ToString(), "SMEMA-1-B-2[0]");
            SMEMAUpstreamGoodBoardAvailableSubscription.Event += OnSMEMAUpstreamGoodBoardAvailable;

            SMEMAUpstreamBadBoardAvailableSubscription = new Subscription(Guid.NewGuid().ToString(), "SMEMA-1-B-3[0]");
            SMEMAUpstreamBadBoardAvailableSubscription.Event += OnSMEMAUpstreamBadBoardAvailable;

            SMEMADownstreamMachineReadySubscription = new Subscription(Guid.NewGuid().ToString(), "SMEMA-1-A-1[0]");
            SMEMADownstreamMachineReadySubscription.Event += OnSMEMADownstreamMachineReady;
        }

        private void MachineReady(bool theState)
        {
            SMEMAUpstreamMachineReadyEvent.Invoke(new Payload(SMEMAUpstreamMachineReadyEvent.Id, new PayloadSubject[] { new PayloadSubject(SMEMAUpstreamMachineReadyEvent.Subjects[0], theState? "1" : "0") }));
        }

        private void GoodBoardAvailable(bool theState)
        {
            SMEMADownstreamGoodBoardAvailableEvent.Invoke(new Payload(SMEMADownstreamGoodBoardAvailableEvent.Id, new PayloadSubject[] { new PayloadSubject(SMEMADownstreamGoodBoardAvailableEvent.Subjects[0], theState ? "1" : "0") }));
        }

        private void BadBoardAvailable(bool theState)
        {
            SMEMADownstreamBadBoardAvailableEvent.Invoke(new Payload(SMEMADownstreamBadBoardAvailableEvent.Id, new PayloadSubject[] { new PayloadSubject(SMEMADownstreamBadBoardAvailableEvent.Subjects[0], theState ? "1" : "0") }));
        }


        internal void Start()
        {
            Task.Delay(5000).ContinueWith(t =>
            {
                if (m_TransportState == ProcessState.Unknown)
                {
                    OnSMEMAUpstreamGoodBoardAvailable(SMEMAUpstreamGoodBoardAvailableSubscription.Cache());
                    OnSMEMAUpstreamBadBoardAvailable(SMEMAUpstreamBadBoardAvailableSubscription.Cache());
                    OnSMEMADownstreamMachineReady(SMEMADownstreamMachineReadySubscription.Cache());

                    m_TransportState = ProcessState.MachineReady;
                    MachineReady(true);
                    StateMachine();
                }
            });
        }

        bool m_UpstreamGoodBoardAvailable;
        bool m_UpstreamBadBoardAvailable;
        bool m_DownstreamMachineReady;

        private static object m_Lock = new object();

        bool m_TransportingIn;
        private void StartTransportIn()
        {
            m_TransportingIn = true;
            Task.Delay(1000).ContinueWith(t =>
            {
                m_TransportingIn = false;
                StateMachine();
            });
        }

        bool m_Processing;
        private void StartProcesing()
        {
            m_Processing = true;
            Task.Delay(2000).ContinueWith(t =>
            {
                m_Processing = false;
                StateMachine();
            });
        }

        bool m_TransportingOut;
        private void StartTransportOut()
        {
            m_TransportingOut = true;
            Task.Delay(2000).ContinueWith(t =>
            {
                m_TransportingOut = false;
                StateMachine();
            });
        }

        bool m_Resetting;
        private void doReset()
        {
            m_Resetting = true;
            Task.Delay(1000).ContinueWith(t =>
            {
                m_Resetting = false;
                StateMachine();
            });
        }

        bool isBadBoard;

        private void StateMachine()
        {
            lock(m_Lock)
            {
                switch(m_TransportState)
                {
                    case ProcessState.MachineReady:
                        if(m_UpstreamGoodBoardAvailable || m_UpstreamBadBoardAvailable)
                        {
                            isBadBoard = false;
                            if (m_UpstreamBadBoardAvailable)
                            {
                                isBadBoard = true;
                            }

                            StartTransportIn();
                            m_TransportState = ProcessState.TransportingIn;

                        }
                        break;

                    case ProcessState.TransportingIn:
                        if(m_TransportingIn == false)
                        {
                            MachineReady(false);
                            StartProcesing();
                            m_TransportState = ProcessState.Processing;
                        }
                        break;

                    case ProcessState.Processing:
                        if(m_Processing == false)
                        {
                            if(isBadBoard)
                            {
                                BadBoardAvailable(true);
                            }
                            else
                            {
                                GoodBoardAvailable(true);
                            }

                            if(m_DownstreamMachineReady)
                            {
                                StartTransportOut();
                                m_TransportState = ProcessState.TransportingOut;
                            }
                            else
                            {
                                m_TransportState = ProcessState.WaitForMachineReady;
                            }

                        }
                        break;
                    case ProcessState.WaitForMachineReady:
                        if (m_DownstreamMachineReady)
                        {
                            StartTransportOut();
                            m_TransportState = ProcessState.TransportingOut;
                        }
                        break;
                    case ProcessState.TransportingOut:
                        if (m_TransportingOut == false)
                        {
                            if (isBadBoard)
                            {
                                BadBoardAvailable(false);
                            }
                            else
                            {
                                GoodBoardAvailable(false);
                            }

                            doReset();
                            m_TransportState = ProcessState.Reset;
                        }
                        break;
                    case ProcessState.Reset:
                        if(m_Resetting == false)
                        {
                            MachineReady(true);

                            if (m_UpstreamGoodBoardAvailable || m_UpstreamBadBoardAvailable)
                            {
                                isBadBoard = false;
                                if (m_UpstreamBadBoardAvailable)
                                {
                                    isBadBoard = true;
                                }

                                StartTransportIn();
                                m_TransportState = ProcessState.TransportingIn;
                            }
                            else
                            {
                                m_TransportState = ProcessState.MachineReady;
                            }
                        }
                        break;

                }
            }
        }

        private void OnSMEMAUpstreamGoodBoardAvailable(SubscriptionEvent theEvent)
        {
            foreach (var Subject in theEvent.PayloadSubjects)
            {
                if (Subject.Value.Equals("0", StringComparison.OrdinalIgnoreCase))
                {
                    m_UpstreamGoodBoardAvailable = false;
                    StateMachine();
                    return;
                }

                if (Subject.Value.Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    m_UpstreamGoodBoardAvailable = true;
                    StateMachine();
                    return;
                }
            }
        }

        private void OnSMEMAUpstreamBadBoardAvailable(SubscriptionEvent theEvent)
        {
            foreach (var Subject in theEvent.PayloadSubjects)
            {
                if (Subject.Value.Equals("0", StringComparison.OrdinalIgnoreCase))
                {
                    m_UpstreamBadBoardAvailable = false;
                    StateMachine();
                    return;
                }

                if (Subject.Value.Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    m_UpstreamBadBoardAvailable = true;
                    StateMachine();
                    return;
                }
            }
        }

        private void OnSMEMADownstreamMachineReady(SubscriptionEvent theEvent)
        {
            foreach (var Subject in theEvent.PayloadSubjects)
            {
                if (Subject.Value.Equals("0", StringComparison.OrdinalIgnoreCase))
                {
                    m_DownstreamMachineReady = false;
                    StateMachine();
                    return;
                }

                if (Subject.Value.Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    m_DownstreamMachineReady = true;
                    StateMachine();
                    return;
                }
            }
        }
    }
}
