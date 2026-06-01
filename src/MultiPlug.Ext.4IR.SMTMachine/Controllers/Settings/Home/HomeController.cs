using MultiPlug.Base.Attribute;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Http;
using MultiPlug.Ext._4IR.SMTMachine.Controllers.SharedRazor;
using MultiPlug.Ext._4IR.SMTMachine.Models;
using MultiPlug.Ext._4IR.SMTMachine.Models.Apps;
using System.Runtime.Serialization;

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
                    MachineReadyEventReadyValue = "1",
                    MachineReadyEventNotReadyValue = "0",

                    GoodBoardSubscriptionId = Core.Instance.Machine.SMEMAUpstreamGoodBoardAvailableSubscription.Id,
                    GoodBoardSubscriptionAvailableValue = "1",
                    GoodBoardSubscriptionNotAvailableValue = "0",
                    GoodBoardSubscriptionConnected = Core.Instance.Machine.SMEMAUpstreamGoodBoardAvailableSubscription.Connected,

                    BadBoardSubscriptionId = Core.Instance.Machine.SMEMAUpstreamBadBoardAvailableSubscription.Id,
                    BadBoardSubscriptionAvailableValue = "1",
                    BadBoardSubscriptionNotAvailableValue = "0",
                    BadBoardSubscriptionConnected = Core.Instance.Machine.SMEMAUpstreamBadBoardAvailableSubscription.Connected,

                    GoodBoardAvailableEventId = Core.Instance.Machine.SMEMADownstreamGoodBoardAvailableEvent.Id,
                    GoodBoardAvailableEventDescription = Core.Instance.Machine.SMEMADownstreamGoodBoardAvailableEvent.Description,
                    GoodBoardAvailableEventSubject = Core.Instance.Machine.SMEMADownstreamGoodBoardAvailableEvent.Subjects[0],
                    GoodBoardAvailableEventAvailableValue = "1",
                    GoodBoardAvailableEventNotAvailableValue = "0",

                    BadBoardAvailableEventId = Core.Instance.Machine.SMEMADownstreamBadBoardAvailableEvent.Id,
                    BadBoardAvailableEventDescription = Core.Instance.Machine.SMEMADownstreamBadBoardAvailableEvent.Description,
                    BadBoardAvailableEventSubject = Core.Instance.Machine.SMEMADownstreamBadBoardAvailableEvent.Subjects[0],
                    BadBoardAvailableEventAvailableValue = "1",
                    BadBoardAvailableEventNotAvailableValue = "0",

                    MachineReadySubscriptionId = Core.Instance.Machine.SMEMADownstreamMachineReadySubscription.Id,
                    MachineReadySubscriptionAvailableValue = "1",
                    MachineReadySubscriptionNotAvailableValue = "0",
                    MachineReadySubscriptionConnected = Core.Instance.Machine.SMEMADownstreamMachineReadySubscription.Connected,
                },
                Template = Templates.SettingsHome
            };
        }

        public Response Post(SettingsHomeModel theModel)
        {
            var NewProperties = new MachineProperties
            {
                SMEMAUpstreamMachineReadyEvent = new Event
                {
                    Id = theModel.MachineReadyEventId,
                    Description = theModel.MachineReadyEventDescription,
                    Subjects = new string[] { theModel.MachineReadyEventSubject }
                },

                SMEMAUpstreamGoodBoardAvailableSubscription = new Subscription
                {
                    Id = theModel.GoodBoardSubscriptionId
                },

                SMEMAUpstreamBadBoardAvailableSubscription = new Subscription
                {
                    Id = theModel.BadBoardSubscriptionId
                },

                SMEMADownstreamGoodBoardAvailableEvent = new Event
                {
                    Id = theModel.GoodBoardAvailableEventId,
                    Description = theModel.GoodBoardAvailableEventDescription,
                    Subjects = new string[] { theModel.GoodBoardAvailableEventSubject }
                },

                SMEMADownstreamBadBoardAvailableEvent = new Event
                {
                    Id = theModel.BadBoardAvailableEventId,
                    Description = theModel.BadBoardAvailableEventDescription,
                    Subjects = new string[] { theModel.BadBoardAvailableEventSubject }
                },

                SMEMADownstreamMachineReadySubscription = new Subscription
                {
                    Id = theModel.MachineReadySubscriptionId
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
