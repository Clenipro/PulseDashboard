using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiChamada.Models
{
    [Table("tickets")]
    public class Ticket
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("n_ticket")]
        public int NumeroTicket { get; set; }

        [Column("abrir_viatura_id")]
        public int AberturaViaturaId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("tipo")]
        public string Tipo { get; set; }

        [Column("pagamento")]
        public string Pagamento { get; set; }

        [Column("valor")]
        public int Valor { get; set; }

        [Column("usado")]
        public int? Usado { get; set; }

        [Column("n_vt_usado")]
        public int? NumeroViaturaUsado { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("segunda_via")]
        public int? SegundaVia { get; set; }

        [Column("online")]
        public int? Online { get; set; }

        [Column("chamou")]
        public int? Chamou { get; set; }

        [Column("create_at")]
        public DateTime CriadoEm { get; set; }

        [Column("update_at")]
        public DateTime? AtualizadoEm { get; set; }
    }
}