using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Arsmate.Core.DTOs.Message
{
    /// <summary>
    /// DTO para enviar un mensaje
    /// </summary>
    public class SendMessageDto
    {
        [Required(ErrorMessage = "El destinatario es obligatorio")]
        public Guid RecipientId { get; set; }

        [StringLength(1000, ErrorMessage = "El mensaje no puede exceder 1000 caracteres")]
        public string Content { get; set; }

        [Range(0, 9999.99, ErrorMessage = "El precio debe estar entre 0 y 9999.99")]
        public decimal? Price { get; set; }

        public Guid? ReplyToMessageId { get; set; }

        public List<IFormFile> Attachments { get; set; }

        public bool IsTipMessage { get; set; }

        [Range(0, 9999.99, ErrorMessage = "La propina debe estar entre 0 y 9999.99")]
        public decimal? TipAmount { get; set; }
    }
}