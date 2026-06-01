using System;
using System.Threading.Tasks;
using MultiPlug.Base.Exchange;
using MultiPlug.Ext._4IR.SMTMachine.Models;

namespace MultiPlug.Ext._4IR.SMTMachine.Components.Machine
{
    public class MachineComponent : MachineProperties
    {
        internal event Action EventsUpdated;
        internal event Action SubscriptionsUpdated;

        private enum TransportState
        {
            Unknown,
            Busy,
            MachineReady,
            TransportingIn,
            Processing,
            WaitForMachineReady,
            TransportingOut,
            Reset
        }

        private enum CoverState
        {
            Unknown,
            Closed,
            Open
        }

        internal int TransportStateId
        {
            get
            {
                return (int)m_TransportState;
            }
        }

        internal string TransportStateDescription
        {
            get
            {
                return TransportStateToString(m_TransportState);
            }
        }

        internal int CoverStateId
        {
            get
            {
                return (int)m_CoverState;
            }
        }

        internal string CoverStateDescription
        {
            get
            {
                return CoverStateToString(m_CoverState);
            }
        }

        private TransportState m_TransportState = TransportState.Unknown;
        private CoverState m_CoverState = CoverState.Unknown;

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

            TransportStateEvent = new Event { Guid = Guid.NewGuid().ToString(), Id = "SMTMachine.TransportState", Description = "Transport State", Subjects = new string[] { "Id", "Description" }, Group = "Machine" };
            CoverStateEvent = new Event { Guid = Guid.NewGuid().ToString(), Id = "SMTMachine.CoverState", Description = "Cover State", Subjects = new string[] { "Id", "Description" }, Group = "Machine" };
        }

        private string TransportStateToString(TransportState theTransportState)
        {
            switch(theTransportState)
            {
                case TransportState.Unknown:
                    return "Unknown";
                case TransportState.MachineReady:
                    return "Machine Ready. Waiting";
                case TransportState.TransportingIn:
                    return "Transporting In";
                case TransportState.Processing:
                    return "Processing";
                case TransportState.WaitForMachineReady:
                    return "Processed. Waiting";
                case TransportState.TransportingOut:
                    return "Transporting Out";
                case TransportState.Reset:
                    return "Resetting";
            }

            return "Unknown";
        }

        private string CoverStateToString(CoverState theCoverState)
        {
            switch (theCoverState)
            {
                case CoverState.Unknown:
                    return "Unknown";
                case CoverState.Open:
                    return "Open";
                case CoverState.Closed:
                    return "Closed";
            }

            return "Unknown";
        }

        private void ChangeTransportState(TransportState theTransportState)
        {
            Task.Run(() =>
            {
                TransportStateEvent.Invoke(new Payload(TransportStateEvent.Id, new PayloadSubject[]
                {
                new PayloadSubject(TransportStateEvent.Subjects[0], ((int)theTransportState).ToString()),
                new PayloadSubject(TransportStateEvent.Subjects[1], TransportStateToString(theTransportState))
                }));
            });

            m_TransportState = theTransportState;
        }

        private void ChangeCoverState(CoverState theCoverState)
        {
            Task.Run(() =>
            {
                CoverStateEvent.Invoke(new Payload(CoverStateEvent.Id, new PayloadSubject[]
                {
                new PayloadSubject(CoverStateEvent.Subjects[0], ((int)theCoverState).ToString()),
                new PayloadSubject(CoverStateEvent.Subjects[1], CoverStateToString(theCoverState))
                }));
            });

            m_CoverState = theCoverState;
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
                if (m_TransportState == TransportState.Unknown)
                {
                    OnSMEMAUpstreamGoodBoardAvailable(SMEMAUpstreamGoodBoardAvailableSubscription.Cache());
                    OnSMEMAUpstreamBadBoardAvailable(SMEMAUpstreamBadBoardAvailableSubscription.Cache());
                    OnSMEMADownstreamMachineReady(SMEMADownstreamMachineReadySubscription.Cache());

                    ChangeTransportState(TransportState.MachineReady);
                    ChangeCoverState(CoverState.Closed);
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
        private void StartProcessing()
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
                    case TransportState.MachineReady:
                        if(m_UpstreamGoodBoardAvailable || m_UpstreamBadBoardAvailable)
                        {
                            isBadBoard = false;
                            if (m_UpstreamBadBoardAvailable)
                            {
                                isBadBoard = true;
                            }

                            StartTransportIn();
                            ChangeTransportState(TransportState.TransportingIn);

                        }
                        break;

                    case TransportState.TransportingIn:
                        if(m_TransportingIn == false)
                        {
                            MachineReady(false);
                            StartProcessing();
                            ChangeTransportState(TransportState.Processing);
                        }
                        break;

                    case TransportState.Processing:
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
                                ChangeTransportState(TransportState.TransportingOut);
                            }
                            else
                            {
                                ChangeTransportState(TransportState.WaitForMachineReady);
                            }

                        }
                        break;
                    case TransportState.WaitForMachineReady:
                        if (m_DownstreamMachineReady)
                        {
                            StartTransportOut();
                            ChangeTransportState(TransportState.TransportingOut);
                        }
                        break;
                    case TransportState.TransportingOut:
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
                            ChangeTransportState(TransportState.Reset);
                        }
                        break;
                    case TransportState.Reset:
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
                                ChangeTransportState(TransportState.TransportingIn);
                            }
                            else
                            {
                                ChangeTransportState(TransportState.MachineReady);
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

        internal void UpdateProperties(MachineProperties theUpdatedProperties)
        {
            bool UpdateEvents = false;
            bool UpdateSubscriptions = false;

            if (theUpdatedProperties.SMEMAUpstreamMachineReadyEvent != null)
            {
                if(Event.Merge(SMEMAUpstreamMachineReadyEvent, theUpdatedProperties.SMEMAUpstreamMachineReadyEvent, true))
                {
                    UpdateEvents = true;
                }
            }

            if (theUpdatedProperties.SMEMAUpstreamGoodBoardAvailableSubscription != null)
            {
                if(Subscription.Merge(SMEMAUpstreamGoodBoardAvailableSubscription, theUpdatedProperties.SMEMAUpstreamGoodBoardAvailableSubscription))
                {
                    UpdateSubscriptions = true;
                }
            }
            if (theUpdatedProperties.SMEMAUpstreamBadBoardAvailableSubscription != null)
            {
                if (Subscription.Merge(SMEMAUpstreamBadBoardAvailableSubscription, theUpdatedProperties.SMEMAUpstreamBadBoardAvailableSubscription))
                {
                    UpdateSubscriptions = true;
                }
            }
            if (theUpdatedProperties.SMEMADownstreamGoodBoardAvailableEvent != null)
            {
                if (Event.Merge(SMEMADownstreamGoodBoardAvailableEvent, theUpdatedProperties.SMEMADownstreamGoodBoardAvailableEvent, true))
                {
                    UpdateEvents = true;
                }
            }
            if (theUpdatedProperties.SMEMADownstreamBadBoardAvailableEvent != null)
            {
                if (Event.Merge(SMEMADownstreamBadBoardAvailableEvent, theUpdatedProperties.SMEMADownstreamBadBoardAvailableEvent, true))
                {
                    UpdateEvents = true;
                }
            }
            if (theUpdatedProperties.SMEMADownstreamMachineReadySubscription != null)
            {
                if (Subscription.Merge(SMEMADownstreamMachineReadySubscription, theUpdatedProperties.SMEMADownstreamMachineReadySubscription))
                {
                    UpdateSubscriptions = true;
                }
            }

            if(UpdateSubscriptions)
            {
                SubscriptionsUpdated?.Invoke();
            }

            if(UpdateEvents)
            {
                SubscriptionsUpdated?.Invoke();
            }

        }
    }
}
