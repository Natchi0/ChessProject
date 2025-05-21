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

		// POST: api/MatchRequests/Request/5
		[HttpPost("Request/{playerId}")]
		public async Task<IActionResult> RequestMatch(int playerId)
		{
			//chequear que el jugador exista
            var player = await _context.Players.FindAsync(playerId);
			if (player == null)
			{
				return NotFound("El jugador no existe");
			}

            //evitar que el jugador pida un match si ya tiene una en waiting
            var existingRequest = await _context.MatchRequests
                .Where(mr => mr.PlayerId == playerId && mr.Status == EMatchRequestStatus.Waiting)
                .FirstOrDefaultAsync();

            if(existingRequest != null)
            {
				//TODO: ver como manejar esto, si el jugador ya tiene una request en waiting
				//puedo devolver la request existente
				return Ok(existingRequest);
            }

            //verificar que el jugador no tenga mas de 5 juegos activos 
            var activeGames = await _context.Games
                .Where(g => (g.PlayerId1 == playerId || g.PlayerId2 == playerId) && g.State != EState.Finished)
                .ToArrayAsync();

            if (activeGames.Length >= 5)
            {
				return BadRequest("El jugador ya tiene 5 juegos activos");
			}

			//buscar una match request en waiting, la mas vieja - FIFO
			//TODO: ver como mejorar el matchmaking eligiendo segun nivel y todo eso
            var matchRequest = await _context.MatchRequests
                .Where(mr => mr.Status == EMatchRequestStatus.Waiting)
                .OrderBy(mr => mr.RequestedAt)
				.FirstOrDefaultAsync();

            if (matchRequest != null)
            {
				//hay una request en waiting, la emparejo con el jugador
                //TODO: ver lo del gameId, tengo que llamar GameServer y eso
				matchRequest.MatchedPlayerId = playerId;
				matchRequest.Status = EMatchRequestStatus.Accepted;
				matchRequest.GameId = null; //no tengo el gameId aun
                matchRequest.MatchedWith = player;

				_context.MatchRequests.Update(matchRequest);
				await _context.SaveChangesAsync();

                return Ok(matchRequest);
			}

			//si no se fue es porque no hay match requests en waiting, creo una
			matchRequest = new MatchRequest
            {
				PlayerId = playerId,
				RequestedAt = DateTime.UtcNow,
				Status = EMatchRequestStatus.Waiting,
			};

			_context.MatchRequests.Add(matchRequest);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetMatchRequest), new { id = matchRequest.Id }, matchRequest);
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
