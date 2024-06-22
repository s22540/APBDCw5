using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using AnimalAPI.Models;
using Microsoft.Extensions.Configuration;

namespace AnimalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimalsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AnimalsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnimals([FromQuery] string orderBy = "name")
        {
            var validColumns = new List<string> { "name", "description", "category", "area" };
            if (!validColumns.Contains(orderBy.ToLower()))
            {
                return BadRequest("Invalid order by parameter.");
            }

            var query = $"SELECT * FROM Animals ORDER BY {orderBy}";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var command = new SqlCommand(query, connection);
                await connection.OpenAsync();
                var reader = await command.ExecuteReaderAsync();

                var animals = new List<Animal>();
                while (await reader.ReadAsync())
                {
                    animals.Add(new Animal
                    {
                        IdAnimal = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        Category = reader.GetString(3),
                        Area = reader.GetString(4)
                    });
                }

                return Ok(animals);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAnimal([FromBody] Animal animal)
        {
            var query = "INSERT INTO Animals (Name, Description, Category, Area) VALUES (@Name, @Description, @Category, @Area)";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", animal.Name);
                command.Parameters.AddWithValue("@Description", animal.Description);
                command.Parameters.AddWithValue("@Category", animal.Category);
                command.Parameters.AddWithValue("@Area", animal.Area);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                return CreatedAtAction(nameof(GetAnimals), new { id = animal.IdAnimal }, animal);
            }
        }

        [HttpPut("{idAnimal}")]
        public async Task<IActionResult> UpdateAnimal(int idAnimal, [FromBody] Animal animal)
        {
            if (idAnimal != animal.IdAnimal)
            {
                return BadRequest("Animal ID mismatch");
            }

            var query = "UPDATE Animals SET Name = @Name, Description = @Description, Category = @Category, Area = @Area WHERE IdAnimal = @IdAnimal";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", animal.Name);
                command.Parameters.AddWithValue("@Description", animal.Description);
                command.Parameters.AddWithValue("@Category", animal.Category);
                command.Parameters.AddWithValue("@Area", animal.Area);
                command.Parameters.AddWithValue("@IdAnimal", idAnimal);

                await connection.OpenAsync();
                var result = await command.ExecuteNonQueryAsync();

                if (result == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
        }

        [HttpDelete("{idAnimal}")]
        public async Task<IActionResult> DeleteAnimal(int idAnimal)
        {
            var query = "DELETE FROM Animals WHERE IdAnimal = @IdAnimal";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@IdAnimal", idAnimal);

                await connection.OpenAsync();
                var result = await command.ExecuteNonQueryAsync();

                if (result == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
        }
    }
}