using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MyApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Requires authentication for all actions by default
    public class RegionsController : ControllerBase
    {
        private readonly string _connectionString;

        public RegionsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureSqlDb");
        }

        // GET: api/regions - Get all regions
        [HttpGet]
        [AllowAnonymous]  // Allows access without authentication
        public async Task<ActionResult<IEnumerable<Region>>> GetRegions()
        {
            var regions = new List<Region>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT id, region_name FROM regions";

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        regions.Add(new Region
                        {
                            Id = reader.GetInt32(0),
                            RegionName = reader.GetString(1)
                        });
                    }
                }
            }

            return Ok(regions);
        }

        // GET: api/regions/{id} - Get a specific region by ID
        [HttpGet("{id}")]
        [AllowAnonymous]  // Allows access without authentication
        public async Task<ActionResult<Region>> GetRegion(int id)
        {
            Region region = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT id, region_name FROM regions WHERE id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            region = new Region
                            {
                                Id = reader.GetInt32(0),
                                RegionName = reader.GetString(1)
                            };
                        }
                    }
                }
            }

            if (region == null)
            {
                return NotFound();
            }

            return Ok(region);
        }

        // POST: api/regions - Add a new region
        [HttpPost]
        public async Task<ActionResult<Region>> AddRegion(Region region)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "INSERT INTO regions (region_name) VALUES (@RegionName); SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RegionName", region.RegionName);

                    // Execute and get the newly inserted ID
                    var id = await command.ExecuteScalarAsync();
                    region.Id = Convert.ToInt32(id);
                }
            }

            return CreatedAtAction(nameof(GetRegion), new { id = region.Id }, region);
        }

        // PUT: api/regions/{id} - Update a specific region by ID
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegion(int id, Region region)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "UPDATE regions SET region_name = @RegionName WHERE id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RegionName", region.RegionName);
                    command.Parameters.AddWithValue("@Id", id);

                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound();
                    }
                }
            }

            return NoContent();
        }

        // DELETE: api/regions/{id} - Delete a specific region by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegion(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "DELETE FROM regions WHERE id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound();
                    }
                }
            }

            return NoContent();
        }
    }
}
