using MultiPlug.Base.Http;
using MultiPlug.Extension.Core.Attribute;

namespace MultiPlug.Ext._4IR.SMTMachine.Controllers.Apps.Mimic
{
    [HttpEndpointType(HttpEndpointType.App)]
    [ViewAs(ViewAs.Partial)]
    [Name("SMT Machine Mimic")]
    public class MimicApp : Controller
    {
    }
}
