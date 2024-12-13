using ApiAJCBanque.Datas;
using LibAJCBanque;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiAJCBanque.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartesController : ControllerBase
    {
        private readonly MyDbContext _context;
        public CartesController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Carte>>> GetCartes()
        {
            return await _context.Cartes
                .Include(c => c.Compte)
                .ToListAsync();
        }

        // GET api/<CartesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CartesController>
        /*[HttpPost]
        public void Post([FromBody] string value)
        {
        }*/

        // PUT api/<CartesController>/5
        /*[HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }*/

        // DELETE api/<CartesController>/5
        /*[HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
