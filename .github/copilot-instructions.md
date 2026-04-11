---
applyTo: '**'
---

# Project Guidelines

## Project Context

- This repository is a workshop/demo project for security compliance with GitHub Copilot.
- Frontend and backend include intentionally insecure examples used for teaching.

## Architecture

- Frontend: Vue 3 + TypeScript app in [ClientApp](ClientApp) with source in [ClientApp/src](ClientApp/src).
- Backend: ASP.NET Core 3.1 API in [MyApp.API](MyApp.API).
- Demo endpoints and auth examples are in [MyApp.API/Controllers/InsecureController.cs](MyApp.API/Controllers/InsecureController.cs) and [MyApp.API/Authentication/BasicAuthenticationHandler.cs](MyApp.API/Authentication/BasicAuthenticationHandler.cs).

## Code Style

- Keep diffs small and task-focused. Avoid unrelated refactors.
- Follow existing naming and structure in surrounding code.
- Do not change public APIs unless explicitly requested.
- Frontend routes must be defined centrally in [ClientApp/src/router/index.ts](ClientApp/src/router/index.ts); do not hardcode routes in components.
- For frontend styling changes, use SCSS and BEM naming.
- In Vue components, prefer props/events and avoid direct DOM manipulation.

## Build And Run

- Frontend setup and run:
  - `cd ClientApp`
  - `npm install`
  - `npm run dev`
  - `npm run build`
  - `npm run preview`
- Frontend audit report:
  - `pwsh scripts/security-check.ps1`
  - Writes [docs/code-review/latest-clientapp-npm-audit.md](docs/code-review/latest-clientapp-npm-audit.md) and [docs/code-review/latest-clientapp-npm-audit.json](docs/code-review/latest-clientapp-npm-audit.json)
- Backend:
  - `dotnet build MyApp.sln`
  - `dotnet run --project MyApp.API`

## Conventions And Pitfalls

- Security vulnerabilities in demo files are often intentional; do not remediate unless explicitly requested.
- Treat [ClientApp/src/views/XssView.vue](ClientApp/src/views/XssView.vue) as a training artifact unless the task is specifically to fix it.
- Link to existing docs instead of duplicating large content:
  - Project overview: [README.md](README.md)
  - Workshop/demo flow: [resources/Demo.md](resources/Demo.md)
  - Frontend template notes: [ClientApp/README.md](ClientApp/README.md)
  - Security audit output: [docs/code-review/latest-clientapp-npm-audit.md](docs/code-review/latest-clientapp-npm-audit.md)
- When proposing commit messages, follow conventional commits.