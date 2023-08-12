
using System.ComponentModel.DataAnnotations;

namespace Models {
    public class Stack {

        public required Pessoa Pessoa {get;set;}

        [Required]
        public required string Nome{get;set;}
    }
}