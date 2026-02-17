---
name: company-guides
description: Company-wide engineering best practices for code style, linting, security, and privacy. Use when asked to check or apply company best practices, review for policy compliance, or align code with company guides.
---

# Company Guides

Provide strict, policy-style guidance for company-wide best practices. Apply the rules below to any requested changes, reviews, or recommendations.

## Code Style and Linting

- Enforce the project linting and formatting configuration as the source of truth. Do not introduce formatting that conflicts with the existing config.
- Keep diffs minimal and focused. Avoid opportunistic refactors unless explicitly requested.
- Prefer explicit, readable code over cleverness. If a construct is non-obvious, add a short comment only when it materially reduces cognitive load.
- Keep names consistent with surrounding code. Do not invent new naming patterns.
- Do not change public APIs without confirming intent.

### Frontend Specific
- No route should be harcoded in the components. All must be defined in a single file, and imported from there. This is to ensure that we can easily change routes without having to search for them in the codebase.
- Styles must be written in SCSS, and follow the BEM convention. This is to ensure that our styles are consistent and easy to maintain. 
- All components must receive props and emit events, and never manipulate the DOM directly. This is to ensure that our components are reusable and testable.
- Every component must have a minimal set of unit tests, covering at least the rendering and the main interactions. This is to ensure that our components are reliable and maintainable.

### Backend Specific
- All authentication systems must be secure by design and follow best practices.
- Use strong cryptography either for hashing, encryption and signing.

## Commit Messages
- Follow conventional commits format. Use the appropriate type (feat, fix, docs, style, refactor, perf, test, chore) and provide a clear description of the change.
- For breaking changes, include a clear description of the change and its impact, and use the "BREAKING CHANGE:" footer to indicate the breaking nature of the change.

## Documentation
- All projects must have a README that explains the purpose of the project and how to set it up. The README should be clear and concise, and should be updated as the project evolves.

## Use of AI Tooling
- When using AI Tools like GitHub Copilot for code generation, always review and ask to create at least a minimal copilot-instructions.md. This file shall at least include all information about the project, such as the tech stack, the architecture, and any other relevant information that can help the AI tool to generate better code. This is to ensure that the AI tool has enough context to generate code that is consistent with the project and its requirements.

## Security and Privacy

- Never log secrets, tokens, credentials, or personal data. Redact or omit sensitive values in examples.
- All endpoints must be authenticated and authorized according to the principle of least privilege. Do not expose unauthenticated endpoints.
- Avoid introducing new dependencies without clear benefit and a security rationale.
- Every time a new dependency is instroduced, check for known vulnerabilities and ensure it is actively maintained. Fetch github security advisories for the dependency and ensure there are no critical, nor high issues.
- Every component in the frontend must be secure by design

## Review Checklist

- Confirm changes match the user request and do not expand scope.
- Check for input validation and output encoding at trust boundaries.
- Verify logging avoids sensitive data.
- Ensure linting and formatting align with project rules.
- Flag potential breaking changes and request confirmation.
