using ApiAJCBanque.Datas;
using LibAJCBanque;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiAJCBanque.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly MyDbContext _context;
        public ClientsController(MyDbContext context)
        {
            _context = context;
        }
        
        // GET: api/<ClientsController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            return await _context.Clients.Include(c => c.Adresse).ToListAsync();
        }

        // GET: api/Clients/Particulier
        [HttpGet("Par")]
        public async Task<ActionResult<IEnumerable<ClientPar>>> GetParClients()
        {
            return await _context.Clients.OfType<ClientPar>().Include(c => c.Adresse).ToListAsync();
        }

        // GET: api/Clients/Professionnelemium
        [HttpGet("Pro")]
        public async Task<ActionResult<IEnumerable<ClientPro>>> GetPr0Clients()
        {
            return await _context.Clients.OfType<ClientPro>()
                .Include(c => c.Adresse)
                .Include(c2 =>c2.AdresseSiege)
                .ToListAsync();
        }

        // GET api/<ClientsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ClientsController>
        /*[HttpPost]
        public void Post([FromBody] string value)
        {
        }*/

        // PUT api/<ClientsController>/5
        /*[HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }*/

        // DELETE api/<ClientsController>/5
        /*[HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
