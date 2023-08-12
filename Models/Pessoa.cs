
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Collections.Generic;
using DTO;
using System.Text.Json.Serialization;

namespace Models
{
    public class Pessoa
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O apelido é obrigatório")]
        [StringLength(32, ErrorMessage = "O apelido deve ter no máximo 32 caracteres")]

        public required string Apelido { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public required string Nome { get; set; }

        [Required(ErrorMessage = "A data de nascimento é obrigatória")]
        public DateOnly Nascimento { get; set; }

        [JsonIgnore]
        public JsonDocument StacksDb { get; set; } = JsonDocument.Parse("[]");

        [NotMapped]
        public string Stacks
        {
            get => StacksDb?.Deserialize<List<String>>()?.Aggregate((x, y) => $"{x}, {y}") ?? String.Empty;
            set => StacksDb = JsonDocument.Parse(JsonSerializer.Serialize(value.Split(",").Select(x => x.Trim()).ToList()));
        }

        public void AddAtack(string stack)
        {
            var stacks = StacksDb.RootElement.EnumerateArray().Select(x => x.GetString()).ToList();
            stacks.Add(stack);
            StacksDb = JsonDocument.Parse(JsonSerializer.Serialize(stacks));
        }

        public List<String> GetStacks()
        {
            return
             StacksDb?.Deserialize<List<String>>()
             ?? new List<String>();
        }

        public Pessoa FromDTO(PessoaDTO dto)
        {
            return new Pessoa
            {
                Id = dto.Id,
                Apelido = dto.Apelido,
                Nome = dto.Nome,
                Nascimento = dto.Nascimento,
                StacksDb = JsonDocument.Parse(JsonSerializer.Serialize(dto.Stacks.Split(",").Select(x => x.Trim()).ToList()))
            };
        }

        public PessoaDTO ToDTO()
        {
            return new PessoaDTO
            {
                Id = Id,
                Apelido = Apelido,
                Nome = Nome,
                Nascimento = Nascimento,
                Stacks = Stacks
            };
        }

    }
}