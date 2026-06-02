using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext._4IR.SMTMachine.Controllers.SharedRazor;
using MultiPlug.Ext._4IR.SMTMachine.Models;
using MultiPlug.Ext._4IR.SMTMachine.Models.Apps;
using MultiPlug.Ext._4IR.SMTMachine.Models.Exchange;

namespace MultiPlug.Ext._4IR.SMTMachine.Controllers.Settings.Home
{
    [Route("")]
    public class HomeController : SettingsApp
    {
        public HomeController()
        {
        }

        public Response Get()
        {
            return new Response
            {
                Model = new SettingsHomeModel
                {
                    MachineReadyEventId = Core.Instance.Machine.SMEMAUpstreamMachineReadyEvent.Id,
                    MachineReadyEventDescription = Core.Instance.Machine.SMEMAUpstreamMachineReadyEvent.Description,
                    MachineReadyEventSubject = Core.Instance.Machine.SMEMAUpstreamMachineReadyEvent.Subjects[0],
                    MachineReadyEventReadyValue = Core.Instance.Machine.SMEMAUpstreamMachineReadyEvent.ReadyValue,
                    MachineReadyEventNotReadyValue = Core.Instance.Machine.SMEMAUpstreamMachineReadyEvent.NotReadyValue,

                    GoodBoardSubscriptionId = Core.Instance.Machine.SMEMAUpstreamGoodBoardAvailableSubscription.Id,
                    GoodBoardSubscriptionAvailableValue = Core.Instance.Machine.SMEMAUpstreamGoodBoardAvailableSubscription.ReadyValue,
                    GoodBoardSubscriptionNotAvailableValue = Core.Instance.Machine.SMEMAUpstreamGoodBoardAvailableSubscription.NotReadyValue,
                    GoodBoardSubscriptionConnected = Core.Instance.Machine.SMEMAUpstreamGoodBoardAvailableSubscription.Connected,

                    BadBoardSubscriptionId = Core.Instance.Machine.SMEMAUpstreamBadBoardAvailableSubscription.Id,
                    BadBoardSubscriptionAvailableValue = Core.Instance.Machine.SMEMAUpstreamBadBoardAvailableSubscription.ReadyValue,
                    BadBoardSubscriptionNotAvailableValue = Core.Instance.Machine.SMEMAUpstreamBadBoardAvailableSubscription.NotReadyValue,
                    BadBoardSubscriptionConnected = Core.Instance.Machine.SMEMAUpstreamBadBoardAvailableSubscription.Connected,

                    GoodBoardAvailableEventId = Core.Instance.Machine.SMEMADownstreamGoodBoardAvailableEvent.Id,
                    GoodBoardAvailableEventDescription = Core.Instance.Machine.SMEMADownstreamGoodBoardAvailableEvent.Description,
                    GoodBoardAvailableEventSubject = Core.Instance.Machine.SMEMADownstreamGoodBoardAvailableEvent.Subjects[0],
                    GoodBoardAvailableEventAvailableValue = Core.Instance.Machine.SMEMADownstreamGoodBoardAvailableEvent.ReadyValue,
                    GoodBoardAvailableEventNotAvailableValue = Core.Instance.Machine.SMEMADownstreamGoodBoardAvailableEvent.NotReadyValue,

                    BadBoardAvailableEventId = Core.Instance.Machine.SMEMADownstreamBadBoardAvailableEvent.Id,
                    BadBoardAvailableEventDescription = Core.Instance.Machine.SMEMADownstreamBadBoardAvailableEvent.Description,
                    BadBoardAvailableEventSubject = Core.Instance.Machine.SMEMADownstreamBadBoardAvailableEvent.Subjects[0],
                    BadBoardAvailableEventAvailableValue = Core.Instance.Machine.SMEMADownstreamBadBoardAvailableEvent.ReadyValue,
                    BadBoardAvailableEventNotAvailableValue = Core.Instance.Machine.SMEMADownstreamBadBoardAvailableEvent.NotReadyValue,

                    MachineReadySubscriptionId = Core.Instance.Machine.SMEMADownstreamMachineReadySubscription.Id,
                    MachineReadySubscriptionAvailableValue = Core.Instance.Machine.SMEMADownstreamMachineReadySubscription.ReadyValue,
                    MachineReadySubscriptionNotAvailableValue = Core.Instance.Machine.SMEMADownstreamMachineReadySubscription.NotReadyValue,
                    MachineReadySubscriptionConnected = Core.Instance.Machine.SMEMADownstreamMachineReadySubscription.Connected,
                },
                Template = Templates.SettingsHome
            };
        }

        public Response Post(SettingsHomeModel theModel)
        {
            var NewProperties = new MachineProperties
            {
                SMEMAUpstreamMachineReadyEvent = new ReadyEvent
                {
                    Id = theModel.MachineReadyEventId,
                    Description = theModel.MachineReadyEventDescription,
                    Subjects = new string[] { theModel.MachineReadyEventSubject },
                    ReadyValue = theModel.MachineReadyEventReadyValue,
                    NotReadyValue = theModel.MachineReadyEventNotReadyValue
                },

                SMEMAUpstreamGoodBoardAvailableSubscription = new ReadySubscription
                {
                    Id = theModel.GoodBoardSubscriptionId,
                    ReadyValue = theModel.GoodBoardSubscriptionAvailableValue,
                    NotReadyValue = theModel.GoodBoardSubscriptionNotAvailableValue,
                },

                SMEMAUpstreamBadBoardAvailableSubscription = new ReadySubscription
                {
                    Id = theModel.BadBoardSubscriptionId,
                    ReadyValue = theModel.BadBoardSubscriptionAvailableValue,
                    NotReadyValue = theModel.BadBoardSubscriptionNotAvailableValue,
                },

                SMEMADownstreamGoodBoardAvailableEvent = new ReadyEvent
                {
                    Id = theModel.GoodBoardAvailableEventId,
                    Description = theModel.GoodBoardAvailableEventDescription,
                    Subjects = new string[] { theModel.GoodBoardAvailableEventSubject },
                    ReadyValue = theModel.GoodBoardAvailableEventAvailableValue,
                    NotReadyValue = theModel.GoodBoardAvailableEventNotAvailableValue
                },

                SMEMADownstreamBadBoardAvailableEvent = new ReadyEvent
                {
                    Id = theModel.BadBoardAvailableEventId,
                    Description = theModel.BadBoardAvailableEventDescription,
                    Subjects = new string[] { theModel.BadBoardAvailableEventSubject },
                    ReadyValue = theModel.BadBoardAvailableEventAvailableValue,
                    NotReadyValue = theModel.BadBoardAvailableEventNotAvailableValue
                },

                SMEMADownstreamMachineReadySubscription = new ReadySubscription
                {
                    Id = theModel.MachineReadySubscriptionId,
                    ReadyValue = theModel.MachineReadySubscriptionAvailableValue,
                    NotReadyValue = theModel.MachineReadySubscriptionNotAvailableValue,
                }
            };

            Core.Instance.Machine.UpdateProperties(NewProperties);

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Moved,
                Location = Context.Referrer
            };
        }
    }
}
