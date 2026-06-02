using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiPlug.Ext._4IR.SMTMachine.Controllers.API.Add
{
    [Route("add")]
    public class AddController : APIEndpoint
    {
        public Response Post()
        {
            if(Core.Instance.Machine.AddBoard())
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