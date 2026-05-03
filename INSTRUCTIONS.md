# Module 1: Getting Started

Before you dive into the exercise, this guide walks you through the tool setup you'll need. It should take about 15 minutes.

## What you'll need

- [VS Code](https://code.visualstudio.com/) installed
- An [OpenRouter](https://openrouter.ai/) account with API credits
- The [Cline](https://marketplace.visualstudio.com/items?itemName=saoudrizwan.claude-dev) extension for VS Code

---

## Setup

### 1. Install Cline

In VS Code, open the Extensions panel (`Cmd+Shift+X` / `Ctrl+Shift+X`) and search for **Cline**. Install it, then reload VS Code.

Once installed, Cline will appear in the left sidebar.

---

### 2. Configure OpenRouter

Cline is model-agnostic — it routes requests through whichever AI provider you configure. We use [OpenRouter](https://openrouter.ai/) because it gives you access to many models through a single API key.

1. Sign up at [openrouter.ai](https://openrouter.ai/) and add some credits (a small amount goes a long way for this course)
2. Generate an API key from your [OpenRouter dashboard](https://openrouter.ai/keys)
3. In VS Code, open the Cline panel and click the **Settings** (gear) icon
4. Set the **API Provider** to `OpenRouter`
5. Paste your API key into the **OpenRouter API Key** field
6. Click **Save**

---

### 3. Plan mode vs. Act mode

Cline has two operating modes you'll use throughout this course:

| Mode | What it does |
| --- | --- |
| **Plan** | Thinks through the problem and proposes an approach — no file changes |
| **Act** | Executes: reads, writes, and edits files on your behalf |

The general pattern we follow is: **Plan first, then Act.** This keeps you in control — you review the plan before anything changes. You'll see this reflected throughout the exercises.

To switch modes, use the toggle at the bottom of the Cline panel.

---

### 4. Choosing a model

OpenRouter gives you access to many models. For most exercises in this course, we recommend starting with **Claude Sonnet 4** — it strikes a good balance between capability and cost.

To select a model:

1. Open the Cline settings panel
2. In the **Model** dropdown, search for `claude` and select `anthropic/claude-sonnet-4`

Feel free to experiment with other models as you progress — part of what this course teaches is developing a feel for when different models are more or less useful.

---

### 5. AGENTS.md

Most AI coding tools — Cline, Claude Code, Codex, and others — automatically load a file called `AGENTS.md` from the root of your project at the start of every new conversation. It's the standard way to give your assistant persistent context: coding preferences, project conventions, things it should always keep in mind.

Create one in your project root when you're ready to use it:

```sh
touch AGENTS.md
```

You'll build this out properly in Module 2. For now, just know it exists — and if you notice things you want your assistant to consistently do or avoid, jot them in `AGENTS.md` as you go.

---

## The Exercise

### 1. Create a working folder structure and initialise Git

```sh
mkdir -p ~/projects/ai-course/module1/{prompts,config-service}
cd ~/projects/ai-course/module1
touch JOURNAL.md
git init .
```

We will use `JOURNAL.md` to capture the details of our collaborations — an account of how we built the resulting assets. You can also use this as a learning journal.

### 2. Create a file that contains the initial specifications for your configuration service

> Because our prompt files will be applied in order, numerical prefixes can be helpful.

`prompts/1-web-api-specs.md` should include:

- Programming language (pick one you're already comfortable with — this is not the time to learn a new one)
- Web framework and key dependencies
- API endpoints and payload shapes
- **Database engine and driver** — use a real database, not in-memory or file-based storage; this API should be deployable
- Data access approach (ORM, direct SQL, etc.) and migration strategy
- Testing approach and tooling

Start with 5–10 key decisions — don't overthink it. The output you get will tell you what's missing. Add specifics and iterate.

> **Looking ahead:** In Module 3 you'll extend this service to support per-application feature flags. A separate `flags` table with a foreign key to `applications` is a clean extension point — worth keeping in mind when you design your schema.

### 3. Collaborate with your assistant to create a prompt

a. First, create a journal entry for this collaboration. We're asking our assistant to generate a prompt — one we'll use in the next step to produce an implementation plan.

Here's the start of our first journal entry:

- Prompt (what we're asking of our assistant): Read @/prompts/1-web-api-specs.md and follow the instructions at the top of the file.
- Tool (your AI assistant): Cline
- Mode (if applicable): Plan
- Context (clean, from previous, etc.): Clean
- Model (LLM model and version): Claude Sonnet 4
- Input (file added to the prompt): prompts/1-web-api-specs.md
- Output (file that contains the response): prompts/2-web-api-prompt.md
- Cost (total cost of the full run): \[enter after the run completes\]
- Reflections (narrative assessments of the response): \[enter after the run completes\]

b. Next, let's include our *prompt creation* instructions. At the top of your `prompts/1-web-api-specs.md`, include instructions for using the contents of this file to create a prompt. Here is an example, but try different variations to see what the assistant chooses to include or leave out:

> This document contains details necessary to create a prompt, which will later be used to create an implementation plan for a REST Web API. Please review the contents of this file and recommend a PROMPT that can be sent to an AI coding assistant for help with creating an implementation plan for this service.
>
> The prompt should:
>
> - ask the assistant to create a comprehensive plan that includes dependencies, file/folder structure, and architectural patterns.
> - recommend strict adherence to ALL of the details in this document.
> - strongly encourage the assistant to not add any additional dependencies without approval.
> - encourage the assistant to ask for more information if they need it.

c. Finally, issue the prompt from our journal entry (where we ask it to follow the instructions at the top of the file) to our assistant in our coding environment. Be sure your spec file is saved, you're in plan mode and have the correct model selected. You can either have your assistant write the prompt to `prompts/2-web-api-prompt.md` (you'll need to switch to act mode) or you can just copy/paste it and save the tokens.

d. Review the prompt, and add the cost and any reflections about the prompt your assistant created into the journal entry.

e. Save, stage, and commit all of your changes before moving on to the next step. If you would like to redo this step or iterate on it before moving on, that's cool, just commit FIRST.

### 4. Collaborate with your assistant to create a plan

a. Start with a new journal entry:

- Prompt: Read @/prompts/2-web-api-prompt.md and follow the instructions at the top of the file.
- Mode: Plan
- Context: Clean
- Input: prompts/2-web-api-specs.md
- Output: prompts/3-web-api-plan.md

b. Issue the prompt to your assistant and save the results into `prompts/3-web-api-plan.md`.

c. Review the plan, and record the cost and your reflections in the journal entry.

d. Save, stage, and commit all of your changes before moving on to the next step. If you would like to redo this step or iterate on it before moving on, that's cool, just commit FIRST.

### 5. Execute the implementation plan

a. You know the drill ... start with a journal entry:

- Prompt: Please create a Config API Service in the `config-service` folder, according to the Implementation Plan defined in @/prompts/3-web-api-plan.md
- Mode: Act
- Context: Clean
- Model: Claude Sonnet 4
- Input: prompts/3-web-api-plan.md
- Output: config-service/

b. Issue the prompt to your assistant. Make sure you are in a mode that lets them use filesystem tools (e.g. act) with *read* and *edit* access. While your assistant is scaffolding the project, monitor their progress. If they begin doing anything unexpected, you can stop them (ESC in Cline).

c. Review the scaffolded project. As you notice things, add them as reflections in the journal entry. There is an assumption you have noticed things you would like to be different if/when you run this implementation plan again.

d. For code-related improvements, capture them as persistent instructions your assistant will follow going forward. Add them to `AGENTS.md` in your project root — most AI coding tools load this automatically at the start of every conversation.

e. Save, stage, and commit all of your changes before moving on to the next step. If you would like to redo this step or iterate on it before moving on, that's cool, just commit FIRST. Be sure your `.gitignore` is in place and properly configured.

### 6. Rinse & Repeat

a. You can iterate from any of the earlier steps. You don't need to start all over with the specs. You can edit the prompt or the plan and generate assets in the subsequent steps.

If you would like to try a different set of specs, check out `examples/web-api-example-specs.md`.

b. Be sure to commit your experiments. It can be helpful to compare previous experiments. And with all of your experiments captured, you can have your assistant compare differences.

c. Once you're happy enough with the scaffolded project, you can transition to collaborating on the code directly.

### 7. Collaborative Code Improvements

a. You've decided your current scaffolded project is sufficient to iterate on. If the unit tests aren't already passing, this is a good place to start.

b. Identify the first improvement you would like to make. Try to avoid making the code changes directly. Use this time to practice collaborating with your assistant. Remember your assistant can assume a number of roles: brainstormer, mentor, QA expert, etc.

c. Don't forget to use your journal and commit frequently.

### 8. An Example

You should have a working Configuration API service that a client can successfully consume. If yours is in that state, wonderful! If you would like to compare your implementation with one prepared ahead of time, see `examples/config-service/svc` for a Python reference.

---

> **Why Cline**?Throughout this course we use [Cline](https://github.com/cline/cline) as our AI coding assistant. We chose it because it's open-source, model-agnostic, and works across multiple AI providers — so it doesn't lock you into a specific vendor. More importantly, the practices and perspectives in this course transfer to whatever tool your team uses: Cursor, Copilot, or anything else. Cline is the *vehicle*, not the destination.