using ApiAJCBanque.Datas;
using ApiAJCBanque.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiAJCBanque.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperationsController : ControllerBase
    {
        private readonly MyDbContext _context;
        public OperationsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/<OperationsController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LibAJCBanque.Operation>>> GetOperations()
        {
            return await _context.Operations
                .Include(o => o.Compte)
                .Include(o => o.Carte)
                .ToListAsync();
        }

        // GET api/<OperationsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LibAJCBanque.Operation>> GetOperation(int id)
        {
            var operation = await _context.Operations
                .Include(o => o.Compte) // Include related entities
                .Include(o => o.Carte)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (operation == null)
            {
                return NotFound();
            }

            return operation;
        }

        // POST api/<OperationsController>
        [HttpPost]
        public async Task<ActionResult<LibAJCBanque.Operation>> PostOperation(OperationInputDto input)
        {
            // Retrieve the Carte based on NumeroCarte
            var carte = await _context.Cartes
                .Include(c => c.Compte) // Ensure the related Compte is loaded
                .FirstOrDefaultAsync(c => c.NumeroCarte == input.NumeroCarte);

            if (carte == null)
            {
                return BadRequest("Carte with the given numeroCarte does not exist.");
            }

            // Create a new Operation object
            var operation = new LibAJCBanque.Operation
            {
                //Id = input.Id,
                DateO = input.DateOperation,
                TypeOperation = input.TypeOp,
                Montant = input.Montant * input.Rate,
                Carte = carte,
                Compte = carte.Compte // Associate the related Compte
            };

            _context.Operations.Add(operation);
            await _context.SaveChangesAsync();

            //return CreatedAtAction(nameof(GetOperation), new { id = operation.Id }, operation);
            return Ok(operation);
        }

        // PUT api/<OperationsController>/5
        /*[HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }*/

        // DELETE api/<OperationsController>/5
        /*[HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
