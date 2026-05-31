using MultiPlug.Base.Exchange;
using MultiPlug.Extension.Core;

namespace MultiPlug.Ext._4IR.SMTMachine
{
    public class SMTMachine : MultiPlugExtension
    {
        public SMTMachine()
        {
        }

        public override Event[] Events
        {
            get
            {
                return new Event[]{ Core.Instance.Machine.SMEMAUpstreamMachineReadyEvent,
                    Core.Instance.Machine.SMEMADownstreamGoodBoardAvailableEvent,
                    Core.Instance.Machine.SMEMADownstreamBadBoardAvailableEvent };
            }
        }

        public override Subscription[] Subscriptions
        {
            get
            {
                return new Subscription[] { Core.Instance.Machine.SMEMAUpstreamGoodBoardAvailableSubscription,
                    Core.Instance.Machine.SMEMAUpstreamBadBoardAvailableSubscription,
                    Core.Instance.Machine.SMEMADownstreamMachineReadySubscription };
            }
        }

        public override void Start()
        {
            Core.Instance.Machine.Start();
        }
    }
}
