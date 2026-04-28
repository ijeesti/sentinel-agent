# sentinel-agent
## AI-Powered Incident Triage System

An event-driven, AI-powered incident triage system that transforms raw operational events into structured, actionable incident reports and dashboards using a locally hosted LLM.

## 🚀 Overview

Modern distributed systems generate massive volumes of logs and alerts. Engineers spend significant time:

- Parsing noisy logs
- Correlating events
- Performing root cause analysis
- Creating incident tickets manually

This project automates that entire workflow using AI agents.

## 💡 What This System Does

Given raw operational events (logs, stack traces, alerts), the system:

1. Classifies incidents
2. Performs root cause analysis
3. Redacts sensitive data (PII, internal IPs)
4. Suggests remediation steps
5. Generates structured HTML incident tickets
6. Builds a searchable, filterable incident dashboard

All processing happens **locally** using a self-hosted LLM.

---
### 📊 Live Artifacts & Results

Experience the system's output through these generated samples:

* **[🏗️ View Project Architecture](https://github.com/ijeesti/sentinel-agent/blob/main/Results/sentinel-agent-architechutre.png)** *A high-level blueprint of the event-driven, private AI triage pipeline.*

* **[🖥️ View Interactive Dashboard](https://htmlpreview.github.io/?https://github.com/ijeesti/sentinel-agent/blob/main/Results/Dashboard/IncidentReport_20260427_194440.html)** *A single pane of glass for multi-event triage, featuring severity filtering and real-time search.*

* **[📄 View Sample Ticket](https://htmlpreview.github.io/?https://github.com/ijeesti/sentinel-agent/blob/main/Results/TicketSamples/TicketReport_20260428_121851.html)** *Deep-dive analysis of an SMTP failure with sanitized technical details and suggested remediation.*

---

## 🧠 Example
### Input (Raw Event)
```
//System.Net.Mail.SmtpException: Failure sending mail
// Stack Trace:
    // at System.Net.Mail.SmtpClient.Send(MailMessage message)
    // at MyCompany.Services.NotificationService.NotifyVendor() 
    // in C:\Services\NotificationService.cs:line 42
    // Source Context: CRITICAL: SmtpException at 10.0.0.42
    // Failed to notify user john.doe@external-vendor.com
    // Email relay failure containing PII and internal network topology. source IP 172.16.254.1
```
---

### Output (Generated Incident)

- **Severity:** Critical  
- **Title:** Email Relay Failure  
- **Description:** Failed to notify user [EMAIL_REDACTED] due to SmtpException at [IP_REDACTED]

#### Root Cause
- SmtpException during email processing  
- Internal network topology exposed via stack trace  

#### Suggested Fix
- Review SMTP configuration  
- Enforce secure communication protocols  

#### Security Handling
- Email redacted → `[EMAIL_REDACTED]`
- IP addresses redacted → `[IP_REDACTED]`

You can view a full, AI-generated HTML incident report here:  
[View Sample Ticket](https://github.com/ijeesti/sentinel-agent/blob/main/Results/TicketSamples/TicketReport_20260428_121851.html)

---

## 🏗️ Architecture
Event Sources
↓
Ingestion Layer (Kafka / RabbitMQ / Logs)
↓
AI Orchestration (Semantic Kernel)
↓
Processing Pipeline
- Classification
- Root Cause Analysis
- PII Redaction
- Suggested Fix Generation
↓
Outputs
- HTML Incident Ticket
- Incident Dashboard

---

## ⚙️ Tech Stack

- **Backend:** .NET 8 / C#
- **AI Orchestration:** Semantic Kernel
- **LLM Runtime:** Ollama (LLaMA local model)
- **Messaging:** Kafka / RabbitMQ (pluggable)
- **Output:** HTML Reports + Dashboard
- **Architecture Style:** Event-driven, modular, extensible

---

## 🔐 Key Capabilities

### ✅ AI-Driven Incident Analysis
Automated classification and root cause detection using AI agents.

### ✅ Privacy by Design
- No external API calls  
- Sensitive data redacted before output  
- Runs fully locally  

### ✅ Structured Ticket Generation
Produces consistent, high-quality incident reports with:
- Severity
- Root cause
- Suggested fix
- Repro steps
- Acceptance criteria

### ✅ Interactive Dashboard
- Filter by severity  
- Search incidents  
- Expandable details  
- Confidence scoring  
* **[Interactive Incident Dashboard](https://htmlpreview.github.io/?https://github.com/ijeesti/sentinel-agent/blob/main/Results/Dashboard/IncidentReport_20260427_194440.html)** *
---

## 📊 Output Artifacts

### 1. HTML Ticket
- Clean, structured, production-ready format  
- Designed for integration with systems like Jira  

### 2. Incident Dashboard
- Aggregated view of incidents  
- Real-time filtering and search  

---

## ▶️ How to Run

```bash
# 1. Clone repo
git clone https://github.com/your-username/ai-incident-triage

# 2. Start Ollama (local LLM)
ollama run llama3

# 3. Run application
dotnet run --project IncidentAgent.Host
```
### 🧪 Sample Use Cases

1. Email service failures (SMTP issues)  
2. Authentication spikes  
3. API failures  
4. Database deadlocks  
5. Observability pipeline alerts  

---

### 🔮 Roadmap

1. RAG-based enrichment (incident history + runbooks)  
2. Duplicate detection and clustering  
3. Cross-event correlation  
4. Auto-remediation workflows  
5. Slack / Teams integration  
6. Multi-tenant support  

---

### 🧭 Design Principles

1. Event-first architecture  
2. Domain-driven design  
3. AI as a co-pilot, not a black box  
4. Privacy and control over data  
5. Extensibility via plugins  
