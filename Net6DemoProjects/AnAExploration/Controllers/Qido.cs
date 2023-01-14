using AnAExploration.dyanamic_policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AnAExploration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QidoController : ControllerBase
    {
        [HttpGet]
        // Policy = PERMISSION_2_Create_Update
        [PermissionAuthorize(PermissionOperator.Or, DicomPermission.Search,DicomPermission.All)]
        public IActionResult Get()
        {
            return Ok ("Qido");
        }

        
    }
    [ApiController]
    [Route("[controller]")]
    public class WadoController : ControllerBase
    {
        [HttpGet]
        // Policy = PERMISSION_2_Create_Update
        [PermissionAuthorize(PermissionOperator.Or, DicomPermission.Read, DicomPermission.All)]
        public IActionResult Get()
        {
            return Ok("Wado");
        }


    }
    [ApiController]
    [Route("[controller]")]
    public class StowController : ControllerBase
    {
        [HttpPost]
        // Policy = PERMISSION_2_Create_Update
       // [PermissionAuthorize(PermissionOperator.Or, DicomPermission.Create, DicomPermission.All)]
        public IActionResult Create([FromBody] Dictionary<string,string> kep)
        {
            return Ok("Stow");
        }


    }
}
