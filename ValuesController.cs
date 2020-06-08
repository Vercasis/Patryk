using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace WEB_API.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
     
        [HttpGet]
        [Produces("application/json")]
        public string Get0()
        {        
            return "witam";
        }
       
    }
}