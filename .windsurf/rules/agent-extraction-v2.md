---
trigger: always_on
---

# Agent-Extraction-V2 — Project rules
Mode: Always On
Apply to: **/*.cs, **/*.md, **/*.csproj

- Always follow the pipeline architecture: PREPARATION → EXTRACTION → PARSING.
- Every dependency on an external executable service (message queues, databses engines, API REST, etc.) must to run in local containers; the only exception is OpenAI.
- .NET 9 technology, C# language; this a console solution with the following parameter: full file path. 
- The output is configurable by file.
- When you suggest changes: ask a confirmation (human-in-the-loop) before editing files.
- Priorize tests over sample files /samples and detailed logs by phase.
- All suggested third-party libraries, frameworks, tools, etc. must be open-source and free for non-commercial use.