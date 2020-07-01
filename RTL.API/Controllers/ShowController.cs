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
        private readonly IShowService _showService;

        public ShowController(IShowService showService)
        {
            _showService = showService;
        }

        [HttpGet(Name = "GetShows")]
        public ActionResult<List<Show>> GetShow(int pageNumber, int pageSize)
        {
            var shows = _showService.GetShowsByPage(pageNumber, pageSize);

            if(shows == null)
                return NotFound();

            return shows;
        }
    }
}