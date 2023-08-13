
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using DTO;
using System.Text.Json.Serialization;

namespace Models
{
    public class Pessoa
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } = Guid.NewGuid();

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
        [JsonPropertyName("Stacks")]
        public List<string> Stacks
        {
            get => StacksDb?.Deserialize<List<string>>() ?? new List<string>();
            set
            {
                StacksDb = JsonDocument.Parse(JsonSerializer.Serialize(value));
            }
        }

        [NotMapped, JsonIgnore]
        public string StacksQuery
        {
            set
            {
                Stacks = JsonSerializer.Deserialize<List<string>>(value);
            }
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

    }
}