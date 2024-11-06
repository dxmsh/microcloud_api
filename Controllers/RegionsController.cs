// Controllers/RegionsController.cs
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
    public class RegionsController : ControllerBase
    {
        private readonly string _connectionString;

        public RegionsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureSqlDb");
        }

        [HttpGet]
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
    }
}
