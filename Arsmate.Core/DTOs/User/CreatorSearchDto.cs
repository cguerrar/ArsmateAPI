using System.ComponentModel.DataAnnotations;

namespace Arsmate.Core.DTOs.User
{
    /// <summary>
    /// DTO para búsqueda de creadores
    /// </summary>
    public class CreatorSearchDto
    {
        public string Query { get; set; }

        [Range(0, 999.99, ErrorMessage = "El precio mínimo debe estar entre 0 y 999.99")]
        public decimal? MinPrice { get; set; }

        [Range(0, 999.99, ErrorMessage = "El precio máximo debe estar entre 0 y 999.99")]
        public decimal? MaxPrice { get; set; }

        public bool? IsVerified { get; set; }
        public bool? HasFreeContent { get; set; }
        public string Category { get; set; }
        public string[] Tags { get; set; }

        [Range(1, 100, ErrorMessage = "La página debe estar entre 1 y 100")]
        public int Page { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "El tamaño de página debe estar entre 1 y 50")]
        public int PageSize { get; set; } = 20;

        public string SortBy { get; set; } = "popularity"; // popularity, newest, price_low, price_high
    }
}