
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models {
    public class Stack {

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id {get;set;}
        public required Pessoa Pessoa {get;set;}

        [Required]
        public required string Nome{get;set;}
    }
}