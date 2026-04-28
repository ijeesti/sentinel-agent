using SentinelAgent.Domain.Aggregates;
using SentinelAgent.Domain.Interfaces;

namespace SentinelAgent.Infrastructure.Reports;

public class HtmlReportGenerator : IReportGenerator
{
    public string GenerateIncidentReport(IncidentTicket ticket)
    {
        string severityColor = ticket.Severity.ToString() switch
        {
            "Critical" => "#be123c",
            "High" => "#e11d48",
            "Medium" => "#f59e0b",
            _ => "#2563eb"
        };

        // NOTICE: $$ before the triple quotes
        return $$"""
        <html>
        <head>
            <style>
                /* C# ignores these single braces now! */
                body { 
                    font-family: 'Segoe UI', system-ui, sans-serif; 
                    background: #f8fafc; 
                    padding: 40px; 
                    line-height: 1.5;
                }
                .card { 
                    max-width: 800px; 
                    margin: auto; 
                    background: white; 
                    padding: 30px; 
                    border-radius: 12px; 
                    border-top: 10px solid {{severityColor}};
                    box-shadow: 0 10px 15px -3px rgba(0,0,0,0.1);
                }
                .badge { 
                    display: inline-block; 
                    padding: 4px 12px; 
                    border-radius: 999px; 
                    font-size: 0.75rem; 
                    font-weight: 700; 
                    background: #f1f5f9; 
                }
                .content-box { 
                    background: #fdf2f2; 
                    padding: 15px; 
                    border-radius: 6px; 
                    white-space: pre-wrap; /* This preserves the AI's line breaks! */
                    font-family: 'Consolas', monospace;
                    font-size: 0.9rem;
                }
            </style>
        </head>
        <body>
            <div class="card">
                <span class="badge"><b>Severity:</b> {{ticket.Severity}} </span>
                <h1>Title:{{ticket.Title}}</h1>
                <h2>Description:{{ticket.Description}}</h2>
                <h3 style="color: #1e293b; border-bottom: 1px solid #e2e8f0;">🔍Analysis</h3>
                <p><strong>Summary:</strong>{{ticket.RootCause.Summary}}</p>

          <h3 style="color: #2563eb;">RootCause</h3>
                <div class="content-box" style="background: #eff6ff; color: #1e40af;">
                   <strong> <b>TechnicalDetail:</b> {{ticket.RootCause.TechnicalDetail}}</strong>
                   <strong> <b>SuggestedFix:</b> {{ticket.RootCause.SuggestedFix}}</strong>
                   <strong> <b>ConfidenceScore:</b> {{ticket.RootCause.ConfidenceScore}}</strong>
                </div>
                
                <h3 style="color: #e11d48;">🚨Steps to Reproduce</h3>
                <div class="content-box" style="background: #fff1f2; color: #9f1239;">
                    {{ticket.ReproductionSteps}}
                </div>

                <h3 style="color: #2563eb;">✅ Acceptance Criteria</h3>
                <div class="content-box" style="background: #eff6ff; color: #1e40af;">
                    {{ticket.AcceptanceCriteria}}
                </div>
            </div>
        </body>
        </html>
        """;
    }
}