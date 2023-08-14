
using Models;

namespace Cache;

public class PessoaCache
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Apelido { get; set; }
    public string Nome { get; set; }
    public DateOnly Nascimento { get; set; }
    public List<string> Stacks  { get; set;  } = new List<string>();

    public static PessoaCache ToCache(Pessoa model) => new PessoaCache
    {
        Id = new Guid(model.Id.ToString()),
        Apelido = model.Apelido.ToString(),
        Nome = model.Nome.ToString(),
        Nascimento = new DateOnly(model.Nascimento.Year, model.Nascimento.Month, model.Nascimento.Day),
        Stacks = model.Stacks.Select(x => x.ToString()).ToList()
    };

}