Please load the following files as your global context:
[index.md](.ai/index.md)

# Available Agents

The following agents are available for use in the Feature Workflow (`FeatureWorkflow.prompt.md`).
Each agent has a dedicated role, persona, and LLM model. They must be invoked via `run_subagent`.

| Agent Name   | File                                  | Model                      | Role                                                                 |
|--------------|---------------------------------------|----------------------------|----------------------------------------------------------------------|
| `planner`    | `.ai/agents/planner.agent.md`         | `deepseek/deepseek-v4-pro`   | Stage 1 — Feature Plan: analyzes work items, produces implementation plan |
| `C#Coder`    | `.ai/agents/C#Coder.agent.md`         | `deepseek/deepseek-v4-flash` | Stage 2 — Feature Implementation: implements the feature in C# code  |
| `TestPlanner`| `.ai/agents/testplanner.agent.md`     | `deepseek/deepseek-v4-pro`   | Stage 3 — Test Planning: produces test strategy, Gherkin scenarios, and test file map |
| `TestCoder`  | `.ai/agents/testcoder.agent.md`       | `deepseek/deepseek-v4-flash` | Stage 4 — Test Implementation: implements all test scenarios as C# test code |
