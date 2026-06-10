using System;
using System.Threading.Tasks;
using MultiPlug.Base.Exchange;
using MultiPlug.Ext._4IR.SMTMachine.Models;
using MultiPlug.Ext._4IR.SMTMachine.Models.Exchange;

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
            Reset,
            Adjusting
        }

        private enum CoverState
        {
            Unknown,
            Closed,
            Opening,
            Open,
            Closing
        }

        private enum StartState
        {
            Stopped,
            Starting,
            Started
        }

        internal int TransportStateId
        {
            get
            {
                return (int)m_TransportState;
            }
        }

        internal string TransportStateDescription()
        {

                return TransportStateToString(m_TransportState);

        }

        internal int CoverStateId
        {
            get
            {
                return (int)m_CoverState;
            }
        }

        internal string CoverStateDescription()
        {
            return CoverStateToString(m_CoverState);
        }

        private TransportState m_TransportState = TransportState.Unknown;
        private CoverState m_CoverState = CoverState.Unknown;

        public float? Width { get; set; } = 0;

        public MachineComponent()
        {
            SMEMAUpstreamMachineReadyEvent = new ReadyEvent { Guid = Guid.NewGuid().ToString(), Id = "SMTMachine.MachineReady", Description = "Machine Ready", Subjects = new string[] { "state" }, Group = "Machine" };
            SMEMADownstreamGoodBoardAvailableEvent = new ReadyEvent { Guid = Guid.NewGuid().ToString(), Id = "SMTMachine.GoodBoardAvailable", Description = "Good Board Available", Subjects = new string[] { "state" }, Group = "Machine" };
            SMEMADownstreamBadBoardAvailableEvent = new ReadyEvent { Guid = Guid.NewGuid().ToString(), Id = "SMTMachine.BadBoardAvailable", Description = "Bad Board Available", Subjects = new string[] { "state" }, Group = "Machine" };

            SMEMAUpstreamGoodBoardAvailableSubscription = new ReadySubscription(Guid.NewGuid().ToString(), "SMEMA-1-B-2[0]");
            SMEMAUpstreamGoodBoardAvailableSubscription.Event += OnSMEMAUpstreamGoodBoardAvailable;

            SMEMAUpstreamBadBoardAvailableSubscription = new ReadySubscription(Guid.NewGuid().ToString(), "SMEMA-1-B-3[0]");
            SMEMAUpstreamBadBoardAvailableSubscription.Event += OnSMEMAUpstreamBadBoardAvailable;

            SMEMADownstreamMachineReadySubscription = new ReadySubscription(Guid.NewGuid().ToString(), "SMEMA-1-A-1[0]");
            SMEMADownstreamMachineReadySubscription.Event += OnSMEMADownstreamMachineReady;

            TransportStateEvent = new Event { Guid = Guid.NewGuid().ToString(), Id = "SMTMachine.TransportState", Description = "Transport State", Subjects = new string[] { "Id", "Description" }, Group = "Machine" };
            CoverStateEvent = new Event { Guid = Guid.NewGuid().ToString(), Id = "SMTMachine.CoverState", Description = "Cover State", Subjects = new string[] { "Id", "Description" }, Group = "Machine" };

            DurationTransportIn = 1000;
            DurationProcessing = 2000;
            DurationTransportOut = 2000;
            DurationReset = 1000;

            AutomaticAdjustmentsEvent = new AdjustmentsEvent{ Guid = Guid.NewGuid().ToString(), Id = "SMTMachine.AutomaticAdjustments", Description = "Automatic Adjustments", Subjects = new string[] { "result" }, Group = "Machine" };

            AutomaticAdjustmentsSubscription = new Subscription(Guid.NewGuid().ToString());
            AutomaticAdjustmentsSubscription.Event += OnAutomaticAdjustmentsSubscription;

            WidthStateEvent = new Event { Guid = Guid.NewGuid().ToString(), Id = "SMTMachine.WidthState", Description = "Width Value", Subjects = new string[] { "Width" }, Group = "Machine" };

            AutomaticAdjustmentsSpeed = 200;
            AutomaticAdjustmentsFeedback = true;
        }

        string NewWidthValue = string.Empty;

        private void OnAutomaticAdjustmentsSubscription(SubscriptionEvent theEvent)
        {
            foreach (var Subject in theEvent.PayloadSubjects)
            {
                NewWidthValue = Subject.Value;
            }

            if( NewWidthValue != string.Empty && m_TransportState == TransportState.MachineReady && m_CoverState == CoverState.Closed)
            {
                if(isAdjustmentRequired())
                {
                    MachineReady(false);
                    StartAdjusting();
                    ChangeTransportState(TransportState.Adjusting);
                }
            }
        }

        private bool m_Adjusting;
        private void StartAdjusting()
        {
            m_Adjusting = true;

            float NewWidth;

            float.TryParse(NewWidthValue, out NewWidth);

            var Difference = Width.Value - NewWidth;

            if (NewWidth > Width.Value)
            {
                Difference = NewWidth - Width.Value;
            }

            var Centimeters = Difference / 10; // How many CM is the diff change
            var WholeCentimeters = Math.Ceiling(Centimeters); // Round it up

            NewWidthValue = string.Empty;

            Task.Delay(AutomaticAdjustmentsSpeed.Value * Convert.ToInt32(WholeCentimeters)).ContinueWith(t =>
            {
                Width = NewWidth;

                WidthStateEvent.Invoke(new Payload(WidthStateEvent.Id, new PayloadSubject[]
                {
                    new PayloadSubject(WidthStateEvent.Subjects[0], Width.ToString()),
                }));

                if(AutomaticAdjustmentsFeedback.Value) // Feedback Option
                {
                    AutomaticAdjustmentsEvent.Invoke(new Payload(AutomaticAdjustmentsEvent.Id, new PayloadSubject[]
                    {
                    new PayloadSubject(AutomaticAdjustmentsEvent.Subjects[0], AutomaticAdjustmentsEvent.OkayValue),
                    }));
                }

                m_Adjusting = false;
                StateMachine();
            });
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
                case TransportState.Adjusting:
                    return "Adjusting Width";
            }

            return string.Empty;
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
                case CoverState.Closing:
                    return "Closing";
                case CoverState.Opening:
                    return "Opening";
            }

            return string.Empty;
        }

        private void ChangeTransportState(TransportState theTransportState)
        {
            m_TransportState = theTransportState;

            Task.Run(() =>
            {
                TransportStateEvent.Invoke(new Payload(TransportStateEvent.Id, new PayloadSubject[]
                {
                new PayloadSubject(TransportStateEvent.Subjects[0], ((int)m_TransportState).ToString()),
                new PayloadSubject(TransportStateEvent.Subjects[1], TransportStateToString(m_TransportState))
                }));
            });
        }

        private void ChangeCoverState(CoverState theCoverState)
        {
            m_CoverState = theCoverState;

            Task.Run(() =>
            {
                CoverStateEvent.Invoke(new Payload(CoverStateEvent.Id, new PayloadSubject[]
                {
                new PayloadSubject(CoverStateEvent.Subjects[0], ((int)m_CoverState).ToString()),
                new PayloadSubject(CoverStateEvent.Subjects[1], CoverStateToString(m_CoverState))
                }));
            });

            StateMachine();
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

        private StartState m_Started = StartState.Stopped;
        internal void Start()
        {
            if(m_Started == StartState.Stopped)
            {
                m_Started = StartState.Starting;

                if (m_TransportState == TransportState.Unknown)
                {
                    OnSMEMAUpstreamGoodBoardAvailable(SMEMAUpstreamGoodBoardAvailableSubscription.Cache());
                    OnSMEMAUpstreamBadBoardAvailable(SMEMAUpstreamBadBoardAvailableSubscription.Cache());
                    OnSMEMADownstreamMachineReady(SMEMADownstreamMachineReadySubscription.Cache());

                    ChangeTransportState(TransportState.MachineReady);
                    MachineReady(true);
                    m_Started = StartState.Started;
                    ChangeCoverState(CoverState.Closed);
                }
            }
        }

        bool m_UpstreamGoodBoardAvailable;
        bool m_UpstreamBadBoardAvailable;
        bool m_DownstreamMachineReady;

        private static object m_Lock = new object();

        private bool m_TransportingIn;
        private void StartTransportIn()
        {
            m_TransportingIn = true;
            Task.Delay(DurationTransportIn.Value).ContinueWith(t =>
            {
                m_TransportingIn = false;
                StateMachine();
            });
        }

        private bool m_Processing;
        private void StartProcessing()
        {
            m_Processing = true;
            Task.Delay(DurationProcessing.Value).ContinueWith(t =>
            {
                m_Processing = false;
                StateMachine();
            });
        }

        private bool m_TransportingOut;
        private void StartTransportOut()
        {
            m_TransportingOut = true;
            Task.Delay(DurationTransportOut.Value).ContinueWith(t =>
            {
                m_TransportingOut = false;
                StateMachine();
            });
        }

        private bool m_Resetting;
        private void doReset()
        {
            m_Resetting = true;
            Task.Delay(DurationReset.Value).ContinueWith(t =>
            {
                m_Resetting = false;
                StateMachine();
            });
        }

        private bool isBadBoard;

        private void StateMachine()
        {
            if(m_Started != StartState.Started)
            {
                return;
            }

            lock(m_Lock)
            {
                switch(m_TransportState)
                {
                    case TransportState.MachineReady:
                        switch (m_CoverState)
                        {
                            case CoverState.Closed:
                                if(isAdjustmentRequired())
                                {
                                    MachineReady(false);
                                    StartAdjusting();
                                    ChangeTransportState(TransportState.Adjusting);
                                }
                                else if (m_UpstreamGoodBoardAvailable || m_UpstreamBadBoardAvailable)
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
                            case CoverState.Opening:
                                MachineReady(false);
                                ChangeCoverState(CoverState.Open);
                                break;
                            case CoverState.Closing:
                                MachineReady(true);
                                ChangeCoverState(CoverState.Closed);
                                break;
                        }
                        break;

                    case TransportState.TransportingIn:
                        if(m_TransportingIn == false)
                        {
                            MachineReady(false);

                            switch (m_CoverState)
                            {
                                case CoverState.Closed:
                                    StartProcessing();
                                    ChangeTransportState(TransportState.Processing);
                                    break;
                                case CoverState.Opening:
                                    ChangeCoverState(CoverState.Open);
                                    break;
                                case CoverState.Closing:
                                    ChangeCoverState(CoverState.Closed);
                                    break;
                            }
                        }
                        break;

                    case TransportState.Processing:
                        if(m_Processing == false)
                        {
                            switch(m_CoverState)
                            {
                                case CoverState.Closed:
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
                                    break;

                                case CoverState.Opening:
                                    ChangeCoverState(CoverState.Open);
                                    break;
                                case CoverState.Closing:
                                    ChangeCoverState(CoverState.Closed);
                                    break;
                            }
                        }
                        break;
                    case TransportState.WaitForMachineReady:
                        switch (m_CoverState)
                        {
                            case CoverState.Closed:
                                if (m_DownstreamMachineReady)
                                {
                                    StartTransportOut();
                                    ChangeTransportState(TransportState.TransportingOut);
                                }
                                break;
                            case CoverState.Opening:
                                if (isBadBoard)
                                {
                                    BadBoardAvailable(false);
                                }
                                else
                                {
                                    GoodBoardAvailable(false);
                                }
                                ChangeCoverState(CoverState.Open);
                                break;
                            case CoverState.Closing:
                                if (isBadBoard)
                                {
                                    BadBoardAvailable(true);
                                }
                                else
                                {
                                    GoodBoardAvailable(true);
                                }
                                ChangeCoverState(CoverState.Closed);
                                break;
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
                        switch (m_CoverState)
                        {
                            case CoverState.Closed:
                                if (m_Resetting == false)
                                {
                                    if (isAdjustmentRequired())
                                    {
                                        StartAdjusting();
                                        ChangeTransportState(TransportState.Adjusting);
                                    }
                                    else
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
                                }
                                break;
                            case CoverState.Opening:
                                ChangeCoverState(CoverState.Open);
                                break;
                            case CoverState.Closing:
                                ChangeCoverState(CoverState.Closed);
                                break;
                        }
                        break;

                    case TransportState.Adjusting:
                        if(m_Adjusting == false)
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

        private bool isAdjustmentRequired()
        {
            if(NewWidthValue != string.Empty)
            {
                float N;
                if(float.TryParse(NewWidthValue, out N))
                {
                    if (N != Width)
                    {
                        return true;
                    }
                    else
                    {
                        if (AutomaticAdjustmentsFeedback.Value) // Feedback Option
                        {
                            AutomaticAdjustmentsEvent.Invoke(new Payload(AutomaticAdjustmentsEvent.Id, new PayloadSubject[]
                            {
                                new PayloadSubject(AutomaticAdjustmentsEvent.Subjects[0], AutomaticAdjustmentsEvent.OkayValue),
                            }));
                        }
                    }
                }
            }

            return false;
        }

        private void OnSMEMAUpstreamGoodBoardAvailable(SubscriptionEvent theEvent)
        {
            foreach (var Subject in theEvent.PayloadSubjects)
            {
                if (Subject.Value.Equals(SMEMAUpstreamGoodBoardAvailableSubscription.NotReadyValue, StringComparison.OrdinalIgnoreCase))
                {
                    m_UpstreamGoodBoardAvailable = false;
                    StateMachine();
                    return;
                }

                if (Subject.Value.Equals(SMEMAUpstreamGoodBoardAvailableSubscription.ReadyValue, StringComparison.OrdinalIgnoreCase))
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
                if (Subject.Value.Equals(SMEMAUpstreamBadBoardAvailableSubscription.NotReadyValue, StringComparison.OrdinalIgnoreCase))
                {
                    m_UpstreamBadBoardAvailable = false;
                    StateMachine();
                    return;
                }

                if (Subject.Value.Equals(SMEMAUpstreamBadBoardAvailableSubscription.ReadyValue, StringComparison.OrdinalIgnoreCase))
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
                if (Subject.Value.Equals(SMEMADownstreamMachineReadySubscription.NotReadyValue, StringComparison.OrdinalIgnoreCase))
                {
                    m_DownstreamMachineReady = false;
                    StateMachine();
                    return;
                }

                if (Subject.Value.Equals(SMEMADownstreamMachineReadySubscription.ReadyValue, StringComparison.OrdinalIgnoreCase))
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
                if(ReadyEvent.Merge(SMEMAUpstreamMachineReadyEvent, theUpdatedProperties.SMEMAUpstreamMachineReadyEvent, true))
                {
                    UpdateEvents = true;
                }
            }

            if (theUpdatedProperties.SMEMAUpstreamGoodBoardAvailableSubscription != null)
            {
                if(ReadySubscription.Merge(SMEMAUpstreamGoodBoardAvailableSubscription, theUpdatedProperties.SMEMAUpstreamGoodBoardAvailableSubscription))
                {
                    UpdateSubscriptions = true;
                }
            }
            if (theUpdatedProperties.SMEMAUpstreamBadBoardAvailableSubscription != null)
            {
                if (ReadySubscription.Merge(SMEMAUpstreamBadBoardAvailableSubscription, theUpdatedProperties.SMEMAUpstreamBadBoardAvailableSubscription))
                {
                    UpdateSubscriptions = true;
                }
            }
            if (theUpdatedProperties.SMEMADownstreamGoodBoardAvailableEvent != null)
            {
                if (ReadyEvent.Merge(SMEMADownstreamGoodBoardAvailableEvent, theUpdatedProperties.SMEMADownstreamGoodBoardAvailableEvent, true))
                {
                    UpdateEvents = true;
                }
            }
            if (theUpdatedProperties.SMEMADownstreamBadBoardAvailableEvent != null)
            {
                if (ReadyEvent.Merge(SMEMADownstreamBadBoardAvailableEvent, theUpdatedProperties.SMEMADownstreamBadBoardAvailableEvent, true))
                {
                    UpdateEvents = true;
                }
            }
            if (theUpdatedProperties.SMEMADownstreamMachineReadySubscription != null)
            {
                if (ReadySubscription.Merge(SMEMADownstreamMachineReadySubscription, theUpdatedProperties.SMEMADownstreamMachineReadySubscription))
                {
                    UpdateSubscriptions = true;
                }
            }

            if (theUpdatedProperties.AutomaticAdjustmentsEvent != null )
            {
                if(AdjustmentsEvent.Merge(AutomaticAdjustmentsEvent, theUpdatedProperties.AutomaticAdjustmentsEvent, true))
                {
                    UpdateEvents = true;
                }
            }

            if(theUpdatedProperties.AutomaticAdjustmentsSubscription != null)
            {
                if(Subscription.Merge(AutomaticAdjustmentsSubscription, theUpdatedProperties.AutomaticAdjustmentsSubscription))
                {
                    UpdateSubscriptions = true;
                }
            }

            if (theUpdatedProperties.DurationTransportIn != null)
            {
                DurationTransportIn = theUpdatedProperties.DurationTransportIn;
            }
            if (theUpdatedProperties.DurationProcessing != null)
            {
                DurationProcessing = theUpdatedProperties.DurationProcessing;
            }
            if (theUpdatedProperties.DurationTransportOut != null)
            {
                DurationTransportOut = theUpdatedProperties.DurationTransportOut;
            }
            if (theUpdatedProperties.DurationReset != null)
            {
                DurationReset = theUpdatedProperties.DurationReset;
            }
            if (theUpdatedProperties.AutomaticAdjustmentsSpeed != null)
            {
                AutomaticAdjustmentsSpeed = theUpdatedProperties.AutomaticAdjustmentsSpeed;
            }
            if(theUpdatedProperties.AutomaticAdjustmentsFeedback != null)
            {
                AutomaticAdjustmentsFeedback = theUpdatedProperties.AutomaticAdjustmentsFeedback;
            }

            if (UpdateSubscriptions)
            {
                SubscriptionsUpdated?.Invoke();
            }

            if(UpdateEvents)
            {
                EventsUpdated?.Invoke();
            }

        }

        internal void CoverToggle()
        {
            switch(m_CoverState)
            {
                case CoverState.Closed:
                    ChangeCoverState(CoverState.Opening);
                    break;
                case CoverState.Opening:
                    ChangeCoverState(CoverState.Closed);
                    break;
                case CoverState.Open:
                    ChangeCoverState(CoverState.Closing);
                    break;
                case CoverState.Closing:
                    ChangeCoverState(CoverState.Open);
                    break;
            }
        }

        internal bool AddBoard()
        {
            if(m_CoverState == CoverState.Open)
            {
                isBadBoard = false;
                m_TransportState = TransportState.Processing;
                return true;
            }
            else
            {
                return false;
            }

        }

        internal bool RemoveBoard()
        {
            if (m_CoverState == CoverState.Open)
            {
                BadBoardAvailable(false);
                GoodBoardAvailable(false);
                m_TransportState = TransportState.Reset;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
