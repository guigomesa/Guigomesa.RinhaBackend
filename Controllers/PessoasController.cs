using Models;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Dapper;
using System.Net;

namespace Guigomesa.RinhaBackend.Controllers;

[ApiController]
[Route("Pessoas")]
public class PessoasController : ControllerBase
{
    public RinhaContext Context { get; set; }

    public PessoasController(RinhaContext context)
    {
        Context = context;
    }

    [HttpGet("{id}", Name = nameof(GetById))]
    public async Task<ActionResult> GetById([FromRoute] Guid id)
    {
        return await BuscarPorGuid(id);
    }

    [HttpGet("", Name = nameof(GetByTermo))]
    public async Task<ActionResult> GetByTermo([FromQuery] string termo)
    {
        return await Listar(termo);
    }

    private async Task<ActionResult> BuscarPorGuid(Guid id)
    {
        var pessoa = await Context.Pessoas.FindAsync(id);
        if (pessoa == null)
        {
            return NotFound();
        }
        return Ok(pessoa);
    }

    private async Task<ActionResult> Listar(string termo)
    {
        DbConnection connection = null;
        DbTransaction transaction = null;
        try
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger("SQLLogger");

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
                return NotFound();
            }

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

            return StatusCode((int)HttpStatusCode.Created);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            if (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
            {
                return Conflict("Já existe uma pessoa com este apelido");
            }
            return Conflict(ex.InnerException?.Message ?? ex.Message);
        }

    }

    [HttpGet("contagem-pessoas")]
    public async Task<ActionResult> ContagemPessoas()
    {
        return Ok(await Context.Pessoas.CountAsync());
    }
}