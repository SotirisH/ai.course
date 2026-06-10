# Test Strategy for {Feature}

## 1. Purpose
{short description}

## 2. Risks
- {risk 1}
- {risk 2}
- {risk 3}

## 3. Test Layers
- **Unit Tests**
    - {unit test focus}
- **Integration Tests**
    - {integration test focus}
- **E2E Tests (Happy Path Only)**
    - {e2e focus}

## 4. Test Scenarios (Gherkin)

### Positive Scenarios
Scenario: {positive scenario name}
Given {initial state}
When {action}
Then {expected outcome}

### Negative Scenarios
Scenario: {negative scenario name}
Given {invalid or missing state}
When {action}
Then {error or validation outcome}

### Edge Cases
Scenario: {edge case name}
Given {edge condition}
When {action}
Then {expected outcome}

### Mapping Scenarios
Scenario: {mapping scenario name}
Given {input DTO}
When {mapping occurs}
Then {output DTO is correct}

### Database Scenarios
Scenario: {db scenario name}
Given {database state}
When {repository action}
Then {expected DB result}

## 5. Automation Approach
- xUnit + Shouldly
- Testcontainers (if DB involved)
- MockHttp (for external API calls)
- Playwright (for E2E happy path)
- In-memory mocks (for handlers)

## 6. Missing Information
- {question 1}
- {question 2}
- {question 3}
