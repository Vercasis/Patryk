using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace WEB_API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class calculateController : ControllerBase
    {
      
        [HttpGet]
        [Produces("application/json")]
        [Route("xy")]
        public dynamic xy(string formula, double from,double to, int n)
        {
            RPN rPN = new RPN();
            return rPN.Formula(formula,from,to,n);
        }

       
        [HttpGet]
        [Produces("application/json")]
        public dynamic Get2(string formula, double x)
        {
            RPN rPN = new RPN();
            return rPN.Formula(formula, x);
        }

       
    }
}