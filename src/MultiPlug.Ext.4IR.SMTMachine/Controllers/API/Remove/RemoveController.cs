using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext._4IR.SMTMachine.Controllers.API.Remove
{
    [Route("remove")]
    public class RemoveController : APIEndpoint
    {
        public Response Post()
        {
            if (Core.Instance.Machine.RemoveBoard())
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            else
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Forbidden
                };
            }
        }
    }
}