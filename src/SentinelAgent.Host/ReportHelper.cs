using SentinelAgent.Domain.Aggregates;
using SentinelAgent.Domain.Domains;
using System.Text;

namespace SentinelAgent.Host;

internal static class ReportHelper
{
    public static void AddReportHeader(StringBuilder reportBuilder)
    {
        reportBuilder.Append("""
<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8">
<title>AI Incident Dashboard</title>

<style>
body {
    font-family: -apple-system, Segoe UI, Roboto;
    background: #f5f7fb;
    margin: 0;
    padding: 20px;
}

.container {
    max-width: 1100px;
    margin: auto;
}

h1 {
    text-align: center;
}

.toolbar {
    display: flex;
    justify-content: space-between;
    margin-bottom: 20px;
}

input {
    padding: 8px;
    width: 250px;
    border-radius: 6px;
    border: 1px solid #ccc;
    background-color: #faad14;
}

button {
    padding: 8px 12px;
    margin-right: 5px;
    border: none;
    border-radius: 6px;
    cursor: pointer;
}
.filter-group button {
    padding: 8px 16px;
    margin-right: 8px;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 600;
    transition: opacity 0.2s;
    color: white; /* Default for dark backgrounds */
}

.filter-group button:hover {
    opacity: 0.8;
}

/* Severity specific colors */
.btn-all { background-color: #64748b; }      /* Slate Gray */
.btn-critical { background-color: #be123c; } /* Deep Red (Matches your report border) */
.btn-high { background-color: #f97316; }     /* Orange */
.btn-medium { background-color: #eab308; color: #000 !important; } /* Yellow */
.btn-low { background-color: #22c55e; }      /* Green */

.card {
    background: white;
    border-radius: 10px;
    padding: 15px;
    margin-bottom: 15px;
    box-shadow: 0 4px 10px rgba(0,0,0,0.08);
}

.header {
    display: flex;
    justify-content: space-between;
    cursor: pointer;
}

.badge {
    padding: 4px 10px;
    border-radius: 6px;
    font-size: 12px;
    font-weight: bold;
}

.severity-Critical .badge { background: #ff4d4f; color: white; }
.severity-High .badge { background: #fa8c16; color: white; }
.severity-Medium .badge { background: #faad14; }
.severity-Low .badge { background: #52c41a; color: white; }

.details {
    display: none;
    margin-top: 10px;
}

.meta {
    font-size: 13px;
    color: #666;
    margin-top: 5px;
}
</style>

<script>
function filterSeverity(level) {
    document.querySelectorAll('.card').forEach(function(c) {
        if (level === 'all' || c.classList.contains('severity-' + level)) {
            c.style.display = 'block';
        } else {
            c.style.display = 'none';
        }
    });
}

function searchTickets() {
    var value = document.getElementById('search').value.toLowerCase();
    document.querySelectorAll('.card').forEach(function(c) {
        var text = c.innerText.toLowerCase();
        c.style.display = text.includes(value) ? 'block' : 'none';
    });
}

function toggleDetails(id) {
    var el = document.getElementById(id);
    el.style.display = el.style.display === 'block' ? 'none' : 'block';
}
</script>

</head>
<body>

<div class="container">
<h1>AI Incident Dashboard</h1>

<div class="toolbar">
   <div class="filter-group">
    <button class="btn-all" onclick="filterSeverity('all')">All</button>
    <button class="btn-critical" onclick="filterSeverity('Critical')">Critical</button>
    <button class="btn-high" onclick="filterSeverity('High')">High</button>
    <button class="btn-medium" onclick="filterSeverity('Medium')">Medium</button>
    <button class="btn-low" onclick="filterSeverity('Low')">Low</button>
</div>

    <input id="search" placeholder="Search..." onkeyup="searchTickets()" />
</div>
""");
    }


    public static void AddBody(
            StringBuilder reportBuilder,
            string location,
            RootCauseAnalysisResult rca,
            IncidentTicket ticket)
    {
        var id = Guid.NewGuid().ToString();

        reportBuilder.Append($"""
            <div class='card severity-{ticket.Severity}'>
            
    
    <div class='header' onclick="toggleDetails('{id}')">
        <h3>#{ticket.Id} — {ticket.Title}</h3>
        <span class='badge'>{ticket.Severity}</span>
    </div>

    <div class='meta'>
        Team: Dev-Team | Source: {location}
    </div>

    <div id='{id}' class='details'>

        <p><strong>Summary:</strong> {rca.Summary}</p>

        <p><strong>Suggested Fix:</strong> {rca.SuggestedFix ?? "Consult Documentation"}</p>

        <p><strong>Confidence:</strong> {(rca.ConfidenceScore * 100):F0}%</p>

        <h4>Possible Causes</h4>
        <ul>
            {string.Join("", rca.PossibleCauses.Select(c => $"<li>{c}</li>"))}
        </ul>

        <hr/>

        <p><strong>Technical Detail:</strong></p>
        <p>{rca.TechnicalDetail ?? "Consult Dev Team"}</p>

    </div>
</div>
""");
     
    }
}