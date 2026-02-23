# Demo and prompts
This is the demo proposal for the talk "Level Up with GitHub Copilot"

1. Assess the audience level and explain the basics if necessary before going further
- Do you use AI for development in your daily work?
- How many of you use GH Copilot?
- Do you use any kind of customization? (Custom agents, skills, instructions, prompts...)
- Reference slide (add SKILLS)

##  Startup
- Generate and teach on the fly the .copilot-instructions
```
Chat settings icon -> Instructions & Rules -> Generate workspace instructions with agent
```

## SKILLs
-  STRIDE skill evaluate the codebase
``` text 
Could you please evaluate the #codebase from an STRIDE perspective?
```

## Custom agents
- Show the custom agent Plan
- [Awesome Copilot](https://github.com/github/awesome-copilot) (choose for example adr-generator and load it in the repo)
- Show the custom agents we already had in the repo
- Delegate a task to our agent "Security Reviewer" (local)
```
I would like to analyze the security posture of my project #codebase
```
- Show agents at organization level (.github-private/agents)

## Background agents
- Worktree and/or background agent to fix one of the issues that the "security reviewer" has found
```
I want you to fix the XSS I have in #file:XssView.vue 

Once you finished review from a security perspective that the vulnerability is mitigated. Do not modify any other file rather than the one I have referenced.
```

- Run prompts locally & cloud to generate project documentation
```

```

## Internal compliance
- Comply with internal documentation (copilot-instructions) at GitHub level

## Subagents
- Minimal scenario to teach subagents working
```
# Selecting no file nor custom agent
I want you to run an STRIDE analysis of the project, and once you have done that, gather only the critical issues, and in a subagent, analyze them with a fix proposal for each one of them. Try to be educational to understand the security issue.
```

- Advanced scenario
```
# Selecting no file nor custom agent
I want you to review my project and suggest an improvement plan. You need to analyze it from two different perspectives using subagents:

- Architecture
- Security

For each of these two, propose 1 key change, and present it to me in a categorized plan, one section each including: why this is the most relevant, and what we will accomplish with the suggested proposal.
```

## Agentic workflows (experimental)
- Teach automatic documentation update workflow prepared in GitHub

## Conclusions
- Rapid pace of change

