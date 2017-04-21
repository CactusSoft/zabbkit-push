using System.ComponentModel.DataAnnotations;

namespace Zabbkit.Web.Models
{
    public class Message
    {
        [Required]
        [RegularExpression(Const.Validation.MongoIdRegexp, ErrorMessage = Const.Validation.MongoIdErrorMessage)]
        public string Id { get; set; }
        [MaxLength(200)]
        public string Text { get; set; }
        public long TriggerId { get; set; }
        public bool PlaySound { get; set; }
    }
}