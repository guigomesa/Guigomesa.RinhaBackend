namespace DTO
{
    public class PessoaDTO
    {
        public Guid Id { get; set; }
        public string Apelido { get; set; }
        public string Nome { get; set; }
        public DateOnly Nascimento { get; set; }
        public string Stacks { get; set; }
    }

}