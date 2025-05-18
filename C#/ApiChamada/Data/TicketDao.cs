using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ApiChamada.Models;

namespace ApiChamada.Data
{
    public class TicketDao
    {
        private readonly TicketContext _context;

        public TicketDao(TicketContext context)
        {
            _context = context;
        }

        public async Task<Ticket> ProcessarTicketPorQRCode(string qrCodeData)
        {
            // Remove sufixo \r\n, se presente
            qrCodeData = qrCodeData.TrimEnd('\r', '\n');

            // Verifica se o ticket já existe com o token informado
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Token == qrCodeData);

            if (ticket != null)
            {
                // Marca o ticket como usado
                ticket.Usado = 1;
                ticket.AtualizadoEm = DateTime.Now;

                await _context.SaveChangesAsync();
                return ticket;
            }

            return null;
        }

        public async Task<Ticket> CriarTicket(Ticket ticket)
        {
            ticket.CriadoEm = DateTime.Now;
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }
    }

    // Contexto do Banco de Dados
    public class TicketContext : DbContext
    {
        public TicketContext(DbContextOptions<TicketContext> options)
            : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("tickets");
                entity.HasKey(e => e.Id);
            });
        }
    }
}