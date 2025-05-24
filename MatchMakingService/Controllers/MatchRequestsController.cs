using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DAL;
using DAL.Models;
using ChessUtilsLib;

namespace MatchMakingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MatchRequestsController(AppDbContext context)
        {
            _context = context;
        }

		//GET: api/MatchRequests/ClearAll
		[HttpGet("ClearAll")]
		public async Task<ActionResult> ClearAllMatchRequests()
		{
			var matchRequests = await _context.MatchRequests.ToListAsync();
			_context.MatchRequests.RemoveRange(matchRequests);

            var games = await _context.Games.ToListAsync();
			_context.Games.RemoveRange(games);

            var moves = await _context.Moves.ToListAsync();
			_context.Moves.RemoveRange(moves);

			await _context.SaveChangesAsync();
			return Ok("Todas las match, games y moves fueron borrados.");
		}

		// GET: api/MatchRequests
		[HttpGet]
        public async Task<ActionResult<IEnumerable<MatchRequest>>> GetMatchRequests()
        {
            return await _context.MatchRequests.ToListAsync();
        }

        // GET: api/MatchRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MatchRequest>> GetMatchRequest(int id)
        {
            var matchRequest = await _context.MatchRequests.FindAsync(id);

            if (matchRequest == null)
            {
                return NotFound();
            }

            return matchRequest;
        }

        // PUT: api/MatchRequests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMatchRequest(int id, MatchRequest matchRequest)
        {
            if (id != matchRequest.Id)
            {
                return BadRequest();
            }

            _context.Entry(matchRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MatchRequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/MatchRequests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MatchRequest>> PostMatchRequest(MatchRequest matchRequest)
        {
            _context.MatchRequests.Add(matchRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMatchRequest", new { id = matchRequest.Id }, matchRequest);
        }

        // DELETE: api/MatchRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMatchRequest(int id)
        {
            var matchRequest = await _context.MatchRequests.FindAsync(id);
            if (matchRequest == null)
            {
                return NotFound();
            }

            _context.MatchRequests.Remove(matchRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MatchRequestExists(int id)
        {
            return _context.MatchRequests.Any(e => e.Id == id);
        }
    }
}
