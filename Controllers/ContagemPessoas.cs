
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Guigomesa.RinhaBackend.Controllers;

[ApiController]
[Route("contagem-pessoas")]
public class ContagemPessoas : ControllerBase
{
    public RinhaContext Context { get; set; }

    public ContagemPessoas(RinhaContext context)
    {
        Context = context;
    }

    [HttpGet("")]
    public async Task<ActionResult> Get()
    {
        var total = await Context.Pessoas.CountAsync();
        return Ok(total);
    }
}
