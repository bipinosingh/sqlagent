using Microsoft.AspNetCore.Mvc;
using SqlAgent.Models;
using SqlAgent.Services;
using SqlAgent.DTOs;
using System.Diagnostics;

namespace SqlAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly SqlExecutorService _sqlExecutor;
    private readonly AIService _aiService;
    private readonly SqlValidator _validator;
    private readonly QueryResponse _queryResponse;
    private readonly ILogger<QueryController> _logger;

    public QueryController(SqlExecutorService sqlExecutor, AIService aiService, SqlValidator validator, QueryResponse queryResponse, ILogger<QueryController> logger)
    {
        _sqlExecutor = sqlExecutor;
        _aiService = aiService;
        _validator = validator;
        _queryResponse = queryResponse;
        _logger = logger;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] QueryRequest request)
    {
        var totalTimer = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Received question: {Question}", request.Question);

            var sqlTimer = Stopwatch.StartNew();
            var sql = await _aiService.GenerateSqlAsync(request.Question);
            sqlTimer.Stop();

            _logger.LogInformation("Generated SQL: {Sql}", sql);

            if (!_validator.IsSafe(sql))
            {
                _logger.LogWarning("Unsafe SQL detected: {Sql}", sql);
                return BadRequest(new QueryResponse
                {
                    Success = false,
                    Question = request.Question,
                    SqlQuery = sql,
                    Error = "Unsafe SQL detected"
                });
            }

            var dbTimer = Stopwatch.StartNew();
            var data = await _sqlExecutor.ExecuteQueryAsync(sql);
            dbTimer.Stop();

            var explainTimer = Stopwatch.StartNew();
            var explanation = await _aiService.ExplainDataAsync(request.Question, sql, data);
            explainTimer.Stop();

            

            totalTimer.Stop();

            _logger.LogInformation("Query executed successfully. Rows returned: {Count} | Performance Metrics | SQL Generation: {SqlMs} ms | DB Execution: {DbMs} ms | Explanation: {ExplainMs} ms | Total: {TotalMs} ms", data.Count(), sqlTimer.ElapsedMilliseconds,
                                    dbTimer.ElapsedMilliseconds,
                                    explainTimer.ElapsedMilliseconds,
                                    totalTimer.ElapsedMilliseconds);

            var response = new QueryResponse
            {
                Success = true,
                Question = request.Question,
                SqlQuery = sql,
                ResultCount = data.Count(),
                

                SqlGenerationTimeMs = sqlTimer.ElapsedMilliseconds,

                DatabaseExecutionTimeMs = dbTimer.ElapsedMilliseconds,

                ExplanationTimeMs = explainTimer.ElapsedMilliseconds,

                TotalExecutionTimeMs = totalTimer.ElapsedMilliseconds,

                Data = data,
                Explanation = explanation

            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing question");

            return StatusCode(500, new QueryResponse
            {
                Success = false,
                Error = ex.Message
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