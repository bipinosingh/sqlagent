using SqlAgent.Agent;
using SqlAgent.DTOs;
using System.Diagnostics;

namespace SqlAgent.Services;

public class AgentService
{
    private readonly AIService _aiService;
    private readonly SqlExecutorService _sqlExecutor;
    private readonly SqlValidator _validator;
    private readonly SchemaService _schemaService;
    private readonly ILogger<AgentService> _logger;
    private readonly IConfiguration _config;

    public AgentService(
        AIService aiService,
        SqlExecutorService sqlExecutor,
        SqlValidator validator,
        SchemaService schemaService,
        ILogger<AgentService> logger,
        IConfiguration config)
    {
        _aiService = aiService;
        _sqlExecutor = sqlExecutor;
        _validator = validator;
        _schemaService = schemaService;
        _logger = logger;
        _config = config;
    }

    public async Task<QueryResponse> ProcessAsync(string question)
    {
        var context = new AgentContext
        {
            Question = question
        };

        var totalTimer = Stopwatch.StartNew();
        var sqlTimer = Stopwatch.StartNew();

        try
        {
            //-----------------------------------------------------
            // STEP 1 : Validate Question
            //-----------------------------------------------------

            var columns = await _schemaService.GetColumnsAsync();

            var validation =
                await _aiService.ValidateQuestionAsync(
                    question,
                    columns);

            if (validation != "VALID")
            {
                return new QueryResponse
                {
                    Success = false,
                    Error = $"Column '{validation}' doesn't exist.",
                    Data = columns
                };
            }

            //-----------------------------------------------------
            // STEP 2 : Generate SQL
            //-----------------------------------------------------

            context.Sql =
                await _aiService.GenerateSqlAsync(question);

            sqlTimer.Stop();

            _logger.LogInformation(
                "Generated SQL:\n{Sql}",
                context.Sql);

            //-----------------------------------------------------
            // STEP 3 : Retry Loop
            //-----------------------------------------------------

            int maxAttempts = int.TryParse(_config["AgentSettings:MaxAttempts"], out var value)? value:3;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                context.Attempt = attempt;

                //-------------------------------------------------
                // Validate SQL
                //-------------------------------------------------

                if (!_validator.IsSafe(context.Sql))
                {
                    _logger.LogWarning(
                        "Unsafe SQL. Attempt {Attempt}",
                        attempt);

                    context.Sql =
                        await _aiService.CorrectSqlAsync(
                            question,
                            await _schemaService.GetSchemaAsync(),
                            context.Sql,
                            "Unsafe SQL detected");

                    continue;
                }

                try
                {
                    //-------------------------------------------------
                    // Execute SQL
                    //-------------------------------------------------

                    context.Data =
                        await _sqlExecutor.ExecuteQueryAsync(
                            context.Sql);

                    //-------------------------------------------------
                    // Empty Result Retry
                    //-------------------------------------------------

                    if (!context.Data.Any())
                    {
                        _logger.LogInformation(
                            "No rows returned. Correcting SQL.");

                        context.Sql =
                            await _aiService.CorrectSqlAsync(
                                question,
                                await _schemaService.GetSchemaAsync(),
                                context.Sql,
                                "SQL executed successfully but returned zero rows.");

                        continue;
                    }

                    //-------------------------------------------------
                    // Explain Data
                    //-------------------------------------------------

                    context.Explanation =
                        await _aiService.ExplainDataAsync(
                            question,
                            context.Sql,
                            context.Data);

                    context.Success = true;

                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Execution failed. Attempt {Attempt}",
                        attempt);

                    if (attempt == maxAttempts)
                    {
                        context.Error = ex.Message;
                        break;
                    }

                    context.Sql =
                        await _aiService.CorrectSqlAsync(
                            question,
                            await _schemaService.GetSchemaAsync(),
                            context.Sql,
                            ex.Message);

                    continue;
                }
            }

            totalTimer.Stop();

            return new QueryResponse
            {
                Success = context.Success,
                Question = question,
                SqlQuery = context.Sql,
                ResultCount = context.Data?.Count() ?? 0,
                Data = context.Data,
                Explanation = context.Explanation,
                Error = context.Error,
                SqlGenerationTimeMs = sqlTimer.ElapsedMilliseconds,
                TotalExecutionTimeMs = totalTimer.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent failed.");

            return new QueryResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<string> ExplainSqlAsync(string sql)
    {
        return await _aiService.ExplainSqlAsync(sql);
    }
}