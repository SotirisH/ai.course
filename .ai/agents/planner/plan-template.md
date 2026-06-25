# Implementation Plan: {{FEATURE_NAME}}

## Metadata

- **Ticket**: {{TICKET_NUM}}
- **Feature Name**: {{FEATURE_NAME}}
- **Work Item Type**: {{WORK_ITEM_TYPE}}

---

## Story Summary

{{STORY_SUMMARY}}

---

## Acceptance Criteria (Given-When-Then)

{{#each ACCEPTANCE_CRITERIA}}
### {{AC_ID}} — {{AC_TITLE}}
- **Given** {{GIVEN}}
- **When** {{WHEN}}
- **Then** {{THEN}}
{{#if AND}} - **And** {{AND}}{{/if}}

{{/each}}

---

## Spec Consistency Check

| Check | Status | Detail |
|-------|--------|--------|
{{#each SPEC_CHECKS}}
| {{CHECK_DESCRIPTION}} | {{STATUS}} | {{DETAIL}} |
{{/each}}

**{{SPEC_ISSUES_SUMMARY}}**

---

## File Change List

### Domain Layer
| Action | File | Notes |
|--------|------|-------|
{{#each DOMAIN_FILES}}
| {{ACTION}} | {{FILE_PATH}} | {{NOTES}} |
{{/each}}

### Application Layer
| # | Action | File | Notes |
|---|--------|------|-------|
{{#each APPLICATION_FILES}}
| {{REF}} | {{ACTION}} | {{FILE_PATH}} | {{NOTES}} |
{{/each}}

### Infrastructure Layer
| # | Action | File | Notes |
|---|--------|------|-------|
{{#each INFRASTRUCTURE_FILES}}
| {{REF}} | {{ACTION}} | {{FILE_PATH}} | {{NOTES}} |
{{/each}}

### API / Presentation Layer
| # | Action | File | Notes |
|---|--------|------|-------|
{{#each API_FILES}}
| {{REF}} | {{ACTION}} | {{FILE_PATH}} | {{NOTES}} |
{{/each}}

---

## Implementation Details

{{#each IMPLEMENTATION_DETAILS}}
### {{SECTION_NUM}}. {{SECTION_TITLE}}

{{SECTION_CONTENT}}

{{/each}}

---

## Implementation Order

{{#each IMPLEMENTATION_STEPS}}
{{STEP_NUM}}. **{{STEP_TITLE}}** ({{STEP_REFS}}) — {{STEP_DESCRIPTION}}
{{/each}}

---

## Assumptions

| # | Assumption | Justification | User Decision |
|---|------------|---------------|---------------|
{{#each ASSUMPTIONS}}
| {{NUM}} | {{ASSUMPTION}} | {{JUSTIFICATION}} | {{USER_DECISION}} |
{{/each}}

---

## Questions for Clarification

| # | Question | Impact | User Decision |
|---|----------|--------|---------------|
{{#each QUESTIONS}}
| {{Q_NUM}} | {{QUESTION}} | {{IMPACT}} | {{USER_DECISION}} |
{{/each}}

---

## Risks

| Risk | Mitigation |
|------|------------|
{{#each RISKS}}
| {{RISK}} | {{MITIGATION}} |
{{/each}}
