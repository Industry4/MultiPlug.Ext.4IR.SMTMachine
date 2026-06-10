using MultiPlug.Base.Exchange;
using MultiPlug.Ext._4IR.SMTMachine.Controllers.SharedRazor;
using MultiPlug.Ext._4IR.SMTMachine.Models.Load;
using MultiPlug.Ext._4IR.SMTMachine.Properties;
using MultiPlug.Extension.Core;
using MultiPlug.Extension.Core.Http;

namespace MultiPlug.Ext._4IR.SMTMachine
{
    public class SMTMachine : MultiPlugExtension
    {
        public SMTMachine()
        {
            Core.Instance.Machine.EventsUpdated += OnEventsUpdated;
            Core.Instance.Machine.SubscriptionsUpdated += OnSubscriptionsUpdated;
        }

        private void OnSubscriptionsUpdated()
        {
            MultiPlugActions.Extension.Updates.Subscriptions();
        }

        private void OnEventsUpdated()
        {
            MultiPlugActions.Extension.Updates.Events();
        }

        public override Event[] Events
        {
            get
            {
                return new Event[]{ 
                    Core.Instance.Machine.TransportStateEvent,
                    Core.Instance.Machine.CoverStateEvent,
                    Core.Instance.Machine.SMEMAUpstreamMachineReadyEvent,
                    Core.Instance.Machine.SMEMADownstreamGoodBoardAvailableEvent,
                    Core.Instance.Machine.SMEMADownstreamBadBoardAvailableEvent,
                    Core.Instance.Machine.AutomaticAdjustmentsEvent,
                    Core.Instance.Machine.WidthStateEvent};
            }
        }

        public override Subscription[] Subscriptions
        {
            get
            {
                return new Subscription[] { Core.Instance.Machine.SMEMAUpstreamGoodBoardAvailableSubscription,
                    Core.Instance.Machine.SMEMAUpstreamBadBoardAvailableSubscription,
                    Core.Instance.Machine.SMEMADownstreamMachineReadySubscription,
                    Core.Instance.Machine.AutomaticAdjustmentsSubscription};
            }
        }

        public override void Start()
        {
            Core.Instance.Machine.Start();
        }

        public override RazorTemplate[] RazorTemplates
        {
            get
            {
                return new RazorTemplate[]
                {
                    new RazorTemplate(Templates.AppsMimic, Resources.MimicApp_cshtml),
                    new RazorTemplate(Templates.SettingsHome, Resources.SettingsHome_cshtml),
                    new RazorTemplate(Templates.SettingsAbout, Resources.SettingsAbout_cshtml)
                };
            }
        }

        public void Load(Root config)
        {
            Core.Instance.Load(config);
        }

        public override object Save()
        {
            return Core.Instance;
        }
    }
}
