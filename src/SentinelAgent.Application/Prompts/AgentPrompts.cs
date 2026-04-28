namespace SentinelAgent.Application.Prompts;

public static class AgentPrompts
{
    /// <summary>
    /// Instructs the LLM to produce a structured bug ticket as JSON.
    /// Variables: {{rawInput}}, {{inputType}}, {{additionalContext}}
    /// </summary>
    public const string TicketGenerator = """
        You are a senior QA engineer. Analyze the failure and produce a bug ticket.

        Failure Input:
        {{$rawInput}}

        Additional Context:
        {{$additionalContext}}

        Return ONLY valid JSON in the following format:

        {
          "title": "string",
          "description": "string",
          "rootCause": {
            "summary": "string",
            "technicalDetail": "string",
            "confidenceScore": 0.85,
            "suggestedFix": "string"
          },
          "acceptanceCriteria": ["string"],
          "reproductionSteps": ["string"],
          "severity": "Critical"
        }

        IMPORTANT:
        - Output must be valid JSON
        - Do NOT return C# or any other format
        - Do NOT include explanations
        - Do NOT include markdown
        - JSON must start with { and end with }
        """;

    /// <summary>
    /// Instructs the LLM to perform root cause analysis and return structured JSON.
    /// Variables: {{rawInput}}, {{inputType}}, {{codeSnippet}}
    /// </summary>
    public const string RootCauseAnalyzer = """
        You are an expert software debugger and root cause analyst.

        ## Failure Input (type: {{$inputType}})
        {{$rawInput}}

        ## Relevant Code
        {{$codeSnippet}}

        ## Task
        Perform a thorough root cause analysis.

        Return ONLY valid JSON to map data :
        {
          "summary": "one-line root cause statement",
          "technicalDetail": "detailed explanation",
          "confidenceScore": 0.85,
          "suggestedFix": "code-level fix suggestion",
          "possibleCauses": [
            "Primary cause",
            "Alternative cause"
          ]
        }
        Confidence score: 0.0 = complete guess, 1.0 = certain.
        Base confidence on how much evidence is visible in the input.

        Return ONLY valid JSON.
        DO NOT include:
        - markdown
        - backticks
        - explanations
        - headings

        Output must start with '{' and end with '}'.
        """;
    /// <summary>
    /// Instructs the LLM to evaluate a test run against its ticket's acceptance criteria.
    /// Variables: {{ticketId}}, {{ticketDescription}}, {{acceptanceCriteria}}, {{testCode}}, {{testOutput}}
    /// </summary>
    public const string TicketAwareTestRunner = """
        You are a QA engineer evaluating whether a test implementation correctly validates a ticket.

        ## Ticket ID
        {{$ticketId}}

        ## Ticket Description
        {{$ticketDescription}}

        ## Acceptance Criteria
        {{$acceptanceCriteria}}

        ## Test Code
        ```csharp
        {{$testCode}}
        ```

        {{#if testOutput}}
        ## Test Output / Results
        ```
        {{$testOutput}}
        ```
        {{/if}}

        ## Task
        Evaluate:
        1. Does the test adequately cover each acceptance criterion?
        2. Are there gaps in test coverage?
        3. If test output is provided, does it indicate pass or failure?

        Respond ONLY with valid JSON:
        {
          "passed": true,
          "summary": "overall assessment",
          "criteriaResults": [
            "AC1: Covered — test verifies X",
            "AC2: NOT covered — no assertion for Y"
          ],
          "failureReason": null,
          "suggestedFix": null
        }
        """;
}

