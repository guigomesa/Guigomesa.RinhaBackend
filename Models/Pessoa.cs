
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Pessoa
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id {get;set;}
        
        [Required(ErrorMessage = "O apelido é obrigatório")]
        [StringLength(32, ErrorMessage = "O apelido deve ter no máximo 32 caracteres")]
        public required string Apelido {get;set;}

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public required string Nome {get;set;}

        [Required(ErrorMessage = "A data de nascimento é obrigatória")]
        public DateOnly Nascimento{get;set;}
        
        public List<Stack> Stacks {get;set;}
    }
}