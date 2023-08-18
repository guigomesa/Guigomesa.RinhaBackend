using Cache;
using Data;
using Models;

namespace Services;

public class Processamento {

    private RinhaContext _Context { get; }
    public Processamento(RinhaContext context) {
        _Context = context;
    }

    public Task SalvarPessoa(PessoaCache pessoaCache) {
        var pessoa = PessoaCache.ToPessoa(pessoaCache);
        _Context.Pessoas.Add(pessoa);
        return _Context.SaveChangesAsync();
    }
}