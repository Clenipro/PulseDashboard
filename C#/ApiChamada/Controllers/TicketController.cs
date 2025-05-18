using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApiChamada.Data;
using ApiChamada.Models;

namespace ApiChamada.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly TicketDao _ticketDao;

        public TicketController(TicketDao ticketDao)
        {
            _ticketDao = ticketDao;
        }

        [HttpPost("criar-ticket")]
        public async Task<IActionResult> CriarTicket([FromBody] Ticket ticket)
        {
            var novoTicket = await _ticketDao.CriarTicket(ticket);
            return CreatedAtAction(nameof(CriarTicket), new { id = novoTicket.Id }, novoTicket);
        }
    }
}