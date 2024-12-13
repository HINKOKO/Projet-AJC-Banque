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
        [HttpPut("UpdateSolde/{numeroCompte}")]
        public async Task<IActionResult> UpdateSolde(string numeroCompte, [FromBody] decimal montant)
        {
            if (string.IsNullOrEmpty(numeroCompte) || numeroCompte.Length != 11)
            {
                return BadRequest("Le numéro de compte doit comporter exactement 11 caractères.");
            }

            var compte = await _context.Comptes.FirstOrDefaultAsync(c => c.NumeroCompte == numeroCompte);
            if (compte == null)
            {
                return NotFound($"Aucun compte trouvé avec le numéro {numeroCompte}.");
            }

            compte.Solde += montant;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Erreur lors de la mise à jour du solde : {ex.Message}");
            }

            return Ok($"Le solde du compte {numeroCompte} a été mis à jour avec succès.");
        }

        // DELETE api/<ComptesController>/5
        /*[HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
