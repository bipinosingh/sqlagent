using OpenAI;
using OpenAI.Chat;
using System.Data;

namespace SqlAgent.Services
{
    public class AIService
    {

        private readonly OpenAI.Chat.ChatClient _chatClient;

        public AIService(IConfiguration config)
        {
            var apiKey = config["OpenAI:ApiKey"];
            _chatClient = new OpenAI.Chat.ChatClient(model: "gpt-4.1-mini", apiKey: apiKey);
        }

        public async Task<string> GenerateSqlAsync(string question)
        {
            //var prompt = $@"You are a SQL Server expert.

            //                Database schema:
            //                Customers(Id, Name, Country)
            //                Orders(Id, CustomerId, Amount, OrderDate)

            //                Convert the following natural language question into SQL query.

            //                Rules:
            //                - Only generate SELECT queries
            //                - Do NOT include explanations
            //                - Use proper JOINs where needed

            //                Question: {{question}}";


            var prompt = $@"You are a SQL Server expert.

                            Database schema:
                            Customers(Id, Name, Country)
                            Orders(Id, CustomerId, Amount, OrderDate)

                            Instructions:
                            - Only use tables that are explicitly required by the question
                            - DO NOT join tables unless the question clearly requires it
                            - If the question is only about Customers, use ONLY Customers table
                            - If the question is only about Orders, use ONLY Orders table
                            - Use JOIN only when necessary (e.g., customer + order data together)
                            - Return ONLY SQL query (no explanation)

                            Question: {question}";

            var response = await _chatClient.CompleteChatAsync(
            new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage("You are a helpful SQL generator."),
                ChatMessage.CreateUserMessage(prompt)
            });

            var raw = response.Value.Content[0].Text.Trim();

            if (raw.StartsWith("```"))
            {
                raw = raw.Replace("```sql", "")
                         .Replace("```", "")
                         .Trim();
            }

            return raw;

        }

        public async Task<string> ExplainSqlAsync(string sql)
        {
            var prompt = $@"
                            You are a SQL Server expert and teacher.

                            Explain the following SQL query in simple, clear terms.

                            Rules:
                            - Explain what the query does
                            - Mention tables used
                            - Mention filters, joins, aggregations if any
                            - Keep explanation concise and easy to understand
                            - Do NOT rewrite the SQL

                            SQL Query:
                            {sql}
                            ";

            var response = await _chatClient.CompleteChatAsync(
                new List<ChatMessage>
                {
            ChatMessage.CreateSystemMessage("You explain SQL queries clearly."),
            ChatMessage.CreateUserMessage(prompt)
                });

            var result = response.Value;

            if (result.Content.Count == 0)
                return "No explanation generated.";

            return result.Content[0].Text.Trim();
        }

        public async Task<string> ExplainDataAsync(string question, string sql, IEnumerable<dynamic> data)
        {
            var preview = string.Join("\n", data.Take(5).Select(d => string.Join(", ", ((IDictionary<string, object>)d).Select(x => $"{x.Key}: {x.Value}"))));

            var prompt = $@"
                            You are a business analyst.

                            User Question:
                            {question}

                            SQL Query:
                            {sql}

                            Data (sample):
                            {preview}

                            Explain the result in simple, clear business terms.
                            Keep it short and meaningful.
                            ";

            var response = await _chatClient.CompleteChatAsync(
                new List<ChatMessage>
                {
            ChatMessage.CreateSystemMessage("You explain data clearly."),
            ChatMessage.CreateUserMessage(prompt)
                });

            return response.Value.Content[0].Text.Trim();
        }

        public async Task<string> ExplainDataAsync_old(string question, string sql, IEnumerable<dynamic> data)
        {
            var dataPreview = string.Join("\n", data.Take(5));

            var prompt = $@"You are a business analyst.

                            User Question: {question}
                            SQL Query: {sql}

                            Data:
                            {dataPreview}

                            Explain the result in simple business terms.
                            ";

            var response = await _chatClient.CompleteChatAsync(
                new List<ChatMessage>
                {
            ChatMessage.CreateSystemMessage("You explain data clearly."),
            ChatMessage.CreateUserMessage(prompt)
                });

            return response.Value.Content[0].Text.Trim();
        }

    }
}
