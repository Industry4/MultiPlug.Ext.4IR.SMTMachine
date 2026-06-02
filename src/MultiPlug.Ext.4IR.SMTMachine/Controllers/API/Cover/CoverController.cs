using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext._4IR.SMTMachine.Controllers.API.Cover
{
    [Route("cover")]
    public class CoverController : APIEndpoint
    {
        public Response Post()
        {
            Core.Instance.Machine.CoverToggle();

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }
    }
}
