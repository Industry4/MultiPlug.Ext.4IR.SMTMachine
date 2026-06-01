using MultiPlug.Base.Attribute;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Http;
using MultiPlug.Ext._4IR.SMTMachine.Controllers.SharedRazor;
using MultiPlug.Ext._4IR.SMTMachine.Models.Apps;

namespace MultiPlug.Ext._4IR.SMTMachine.Controllers.Apps.Mimic
{
    [Route("")]
    public class HomeController : MimicApp
    {
        public HomeController()
        {
        }

        public Response Get()
        {
            return new Response
            {
                Model = new MimicAppModel
                {
                    TransportStateId = Core.Instance.Machine.TransportStateId,
                    TransportStateDescription = Core.Instance.Machine.TransportStateDescription,
                    CoverStateId = Core.Instance.Machine.CoverStateId,
                    CoverStateDescription = Core.Instance.Machine.CoverStateDescription,
                },
                Subscriptions = new Subscription[]
                {
                    new Subscription("TransportStateEventId", Core.Instance.Machine.TransportStateEvent.Id ),
                    new Subscription("CoverStateEventId", Core.Instance.Machine.CoverStateEvent.Id )
                },
                Template = Templates.AppsMimic
            };
        }

        public Response Post(MimicAppModel theModel)
        {

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Moved,
                Location = Context.Referrer
            };
        }
    }
}
