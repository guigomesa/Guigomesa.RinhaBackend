using Models;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Dapper;
using System.Net;
using Services;
using Cache;

namespace Guigomesa.RinhaBackend.Controllers;

[ApiController]
[Route("pessoas")]
public class PessoasController : ControllerBase
{
    public RinhaContext Context { get; set; }
    public CacheMemoria Cache { get; set; }

    public PessoasController(RinhaContext context, CacheMemoria cache)
    {
        Context = context;
        Cache = cache;
    }
 
    [HttpGet("{id}", Name = nameof(GetById))]
    public async Task<ActionResult> GetById([FromRoute] Guid id)
    {
        if (id == Guid.Empty)
        {
            return NotFound("Id não encontrado");
        }

       return await BuscarPorGuid(id);
    }

    [HttpGet("", Name = nameof(GetByTermo))]
    public async Task<ActionResult> GetByTermo([FromQuery] string termo = "")
    {
        if (string.IsNullOrWhiteSpace(termo))
        {
            return BadRequest("O termo de busca não pode ser vazio");
        }
        return await Listar(termo);
    }

    private async Task<ActionResult> BuscarPorGuid(Guid id)
    {
        var pessoaCache = await Cache.Get<PessoaCache>($"pessoa_{id}");
        if (pessoaCache != null)
        {
            return Ok(pessoaCache);
        }

        var pessoa = await Context.Pessoas.FindAsync(id);
        if (pessoa == null)
        {
            return NotFound();
        }

        pessoaCache = PessoaCache.ToCache(pessoa);
        await Cache.Set($"pessoa_{id}", pessoaCache, TimeSpan.FromMinutes(1));

        return Ok(pessoaCache);
    }

    private async Task<ActionResult> Listar(string termo)
    {
        DbConnection connection = null;
        DbTransaction transaction = null;
        try
        {
            var cacheado = await Cache.Get<List<PessoaCache>>($"pessoas_termo_{termo}");
            if(cacheado!= null && cacheado.Any())
            {
                return Ok(cacheado);
            }

            connection = Context.Database.GetDbConnection();
            await connection.OpenAsync();
            transaction = await connection.BeginTransactionAsync();
            var commandTimeout = Context.Database.GetCommandTimeout();

            const string QUERY = @"
            select 
                p.""Id"",
                p.""Apelido"",
                p.""Nome"",
                p.""Nascimento"" as NascimentoQuery,
                p.""StacksDb""::text as ""StacksQuery""
             from public.""Pessoas"" p
            where p.""Nome"" ilike @termo
            or p.""Apelido"" ilike @termo
            or p.""StacksDb""::jsonb @> @jsonTermo::jsonb
            limit 50
        ";

            var command = new CommandDefinition(QUERY,
                new
                {
                    termo = $"%{termo}%",
                    jsonTermo = $@"[""{termo}""]"
                },
                transaction,
                commandTimeout: commandTimeout);

            var pessoas = await connection.QueryAsync<Pessoa>(command);

            if (pessoas == null || !pessoas.Any())
            {
                return Ok();
            }

            var pessoasCache = pessoas.Select(PessoaCache.ToCache).ToList();
            await Cache.Set($"pessoas_termo_{termo}", pessoasCache, TimeSpan.FromSeconds(2));

            return Ok(pessoas);
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.CommitAsync();
                transaction.Dispose();
            }
            if (connection != null)
            {

                await connection.CloseAsync();
                connection.Dispose();
            }
        }
    }

    [HttpPost("")]
    public async Task<IActionResult> Post([FromBody] Pessoa pessoa)
    {
        try
        {
            await Context.Pessoas.AddAsync(pessoa);
            await Context.SaveChangesAsync();

            Response.StatusCode = (int)HttpStatusCode.Created;
            Response.Headers.Add("Location", Url.Link(nameof(GetById), new { id = pessoa.Id }));

            await Cache.Set($"pessoa_{pessoa.Id}", PessoaCache.ToCache(pessoa), TimeSpan.FromMinutes(1));
            return StatusCode((int)HttpStatusCode.Created);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            if (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
            {
                return BadRequest("Já existe uma pessoa com este apelido");
            }
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

    }
}