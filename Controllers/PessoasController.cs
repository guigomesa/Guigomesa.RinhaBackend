using Models;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Data.Common;
using Dapper;

namespace Guigomesa.RinhaBackend.Controllers;


[ApiController]
[Route("[controller]")]
public class PessoasController : ControllerBase
{
    public RinhaContext Context { get; set; }

    public PessoasController(RinhaContext context)
    {
        Context = context;
    }

    [HttpGet("{id:guid}")]
    [HttpGet("")]
    public async Task<ActionResult> Get([FromQuery] string? termo = null, Guid? id = null)
    {
        try
        {
            if (id.HasValue && id.Value != Guid.Empty)
            {
                return await BuscarPorGuid(id.Value);
            }
            if (!string.IsNullOrEmpty(termo))
            {
                return await Listar(termo);
            }

            return StatusCode((int)HttpStatusCode.MethodNotAllowed);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

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
            transaction = await  connection.BeginTransactionAsync();
            var commandTimeout =  Context.Database.GetCommandTimeout();

            const string QUERY = @"
            select 
                p.""Id"",
p.""Apelido"",
p.""Nome"",
p.""Nascimento"",
p.""StacksDb"" as ""Stack""
             from public.""Pessoas"" p
            where p.""Nome"" ilike @termo
            or p.""Apelido"" ilike @termo
            or p.""StacksDb"" @> @jsonTermo
            limit 50
        ";
        //    or p.""StacksDb"" @> @jsonTermo

            var jsonTermo = $@"[""{termo}""]::json";
            var command = new CommandDefinition(QUERY,
                new { termo, jsonTermo },
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
    public async Task<ActionResult> Post([FromBody] Pessoa pessoa)
    {
        try
        {
            await Context.Pessoas.AddAsync(pessoa);
            await Context.SaveChangesAsync();
            return CreatedAtRoute("Get", new { id = pessoa.Id }, pessoa);
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