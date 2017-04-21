using System.ComponentModel.DataAnnotations;

namespace Zabbkit.Web.Models
{
    public class TokenRenewRequest
    {
        [Required]
        [RegularExpression(Const.Validation.MongoIdRegexp, ErrorMessage = Const.Validation.MongoIdErrorMessage)]
        public string Id { get; set; }
        public DeviceType Type { get; set; }
        [Required]
        [MinLength(Const.Validation.MinTokenLength, ErrorMessage = "Old token is invalid")]
        public string OldToken { get; set; }
        [Required]
        [MinLength(Const.Validation.MinTokenLength, ErrorMessage = "New token is invalid")]
        public string NewToken { get; set; }
    }
}