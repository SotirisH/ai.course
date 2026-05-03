# Overview
One Prompt to Rule Them All: How to Reuse the Same Markdown Instructions Across Copilot, Claude, Cursor & Codex
This is inspired by the idea of "[One Prompt to Rule Them All](https://medium.com/@genyklemberg/one-prompt-to-rule-them-all-how-to-reuse-the-same-markdown-instructions-across-copilot-claude-42693df4df00)" in the context of AI language models. 
The goal is to create a single set of Markdown instructions that can be reused across different AI platforms such as Copilot, Claude, Cursor, and Codex.
This approach allows for consistency and efficiency when working with multiple AI tools, enabling users to leverage the strengths of each platform without having to
rewrite instructions for each one. By using a unified Markdown format, users can easily adapt their prompts to fit the specific requirements of each AI model while maintaining a cohesive workflow.

# Folder Structure
The project is organized into the following folders:

- `readme.md` → explains how AI tools should reference these files
- `rules/` → modular development rules (without any AI specific headers)
- `prompts/` → reusable prompt templates (without any AI specific headers)
- `schema/` → schemas for agents, commands, skills (Claude)
- `optional examples/` → example prompts using the rules
