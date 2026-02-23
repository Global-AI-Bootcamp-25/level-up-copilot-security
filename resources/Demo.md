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
- Run prompts locally & cloud to generate project documentation

## Internal compliance
- Comply with internal documentation (copilot-instructions) at GitHub level

## Subagents
- Minimal scenario to teach subagents working (Diego will test it)

## Agentic workflows (experimental)
- Teach automatic documentation update workflow prepared in GitHub

## Conclusions
- Rapid pace of change

