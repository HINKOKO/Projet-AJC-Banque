using ApiAJCBanque.Datas;
using LibAJCBanque;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiAJCBanque.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComptesController : ControllerBase
    {
        private readonly MyDbContext _context;
        public ComptesController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/<ComptesController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Compte>>> GetComptes()
        {
            return await _context.Comptes
                .Include(c => c.Client)
                .ToListAsync();
        }

        // GET api/<ComptesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ComptesController>
        /*[HttpPost]
        public void Post([FromBody] string value)
        {
        }*/

        // PUT api/<ComptesController>/5
        /*[HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }*/

        // DELETE api/<ComptesController>/5
        /*[HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
