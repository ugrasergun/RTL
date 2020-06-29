using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RTL.API.Models;
using RTL.API.Services;

namespace RTL.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowController : ControllerBase
    {
        private readonly ShowService _showService;

        public ShowController(ShowService showService)
        {
            _showService = showService;
        }

        [HttpGet(Name = "GetShow")]
        public ActionResult<Show> GetShow(int showId)
        {
            var show = _showService.GetShowByShowId(showId);

            if(show == null)
                return NotFound();

            return show;
        }

        [HttpPost]
        public void Create(Show show)
        {
            _showService.Create(show);
        }
    }
}