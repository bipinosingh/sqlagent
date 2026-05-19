using Microsoft.AspNetCore.Mvc;
using SqlAgent.Models;
using SqlAgent.Services;

namespace SqlAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly SqlExecutorService _sqlExecutor;
    private readonly AIService _aiService;
    private readonly SqlValidator _validator;

    public QueryController(SqlExecutorService sqlExecutor, AIService aiService, SqlValidator validator)
    {
        _sqlExecutor = sqlExecutor;
        _aiService = aiService;
        _validator = validator;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] QueryRequest request)
    {
        string sql;
        int retries = 0;
        /*
        // Temporary hardcoded SQL
        var sql = "SELECT * FROM Customers";

        var data = await _sqlExecutor.ExecuteQueryAsync(sql);

        return Ok(new
        {
            sql,
            data
        });
        */

        try
        {
            sql = await _aiService.GenerateSqlAsync(request.Question);

            do
            {
                if (!_validator.IsSafe(sql))
                {
                    return BadRequest(new
                    {
                        error = "Unsafe SQL detected",
                        generatedSql = sql
                    });
                }
                else
                {
                    break;
                }
                retries++;
            } while (retries < 2);

            var data = await _sqlExecutor.ExecuteQueryAsync(sql);
            var explanation = await _aiService.ExplainDataAsync(request.Question, sql, data);

            if (!_validator.IsSafe(sql))
            {
                return BadRequest("Failed to generate safe SQL.");
            }
            else
            {
                return Ok(new
                {
                    success = true,
                    question = request.Question,
                    sqlQuery = sql,
                    resultCount = data.Count(),
                    data,
                    explanation
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message
            });
        }

    }




    [HttpPost("explain-sql")]
    public async Task<IActionResult> ExplainSql([FromBody] string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return BadRequest("SQL query is required.");
        }

        var explanation = await _aiService.ExplainSqlAsync(sql);

        return Ok(new
        {
            sql,
            explanation
        });
    }

}