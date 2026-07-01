using Microsoft.AspNetCore.Mvc;
using SqlAgent.DTOs;
using SqlAgent.Models;
using SqlAgent.Services;

namespace SqlAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly AgentService _agentService;
    private readonly ILogger<QueryController> _logger;

    public QueryController(
        AgentService agentService,
        ILogger<QueryController> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] QueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new QueryResponse
            {
                Success = false,
                Error = "Question is required."
            });
        }

        try
        {
            var response = await _agentService.ProcessAsync(request.Question);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");

            return StatusCode(500, new QueryResponse
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    [HttpPost("explain-sql")]
    public async Task<IActionResult> ExplainSql(
        [FromBody] string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return BadRequest("SQL is required.");

        var explanation =
            await _agentService.ExplainSqlAsync(sql);

        return Ok(new
        {
            sql,
            explanation
        });
    }
}