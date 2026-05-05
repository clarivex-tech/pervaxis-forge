# Pervaxis Forge — Product Overview
**For Non-Technical Stakeholders**

**Version:** 1.1  
**Date:** May 5, 2026  
**Company:** Clarivex Technologies  
**Classification:** Internal

---

## What is Pervaxis Forge?

Pervaxis Forge is an internal platform tool that generates production-ready software project scaffolds in seconds. Think of it as a "project factory" — you describe what kind of service you need, and Forge instantly creates a complete, working codebase with all the required infrastructure, documentation, and tooling already configured.

Instead of developers spending hours or days setting up new projects manually, Forge reduces this to under 2 seconds with zero errors and 100% consistency across all projects.

---

## The Problem It Solves

### Before Forge:
When creating a new microservice or web application, teams face:

- **3-5 days of manual setup** — configuring build pipelines, security, testing, documentation
- **Inconsistent standards** — different teams set up projects differently, creating maintenance nightmares
- **Copy-paste errors** — manually copying configuration from old projects introduces bugs
- **Knowledge bottlenecks** — only senior developers know the "right way" to set up everything
- **Delayed delivery** — time spent on setup is time not spent building actual features
- **Re-entering context** — every new service requires re-entering the same cloud account, GitHub org, and environment details

### After Forge:
- **Under 2 seconds** to generate a complete project
- **100% consistency** — every project follows exact same structure and standards
- **Zero manual errors** — automated generation eliminates copy-paste mistakes
- **Knowledge democratization** — junior developers get expert-level project setups automatically
- **Immediate productivity** — teams start building features on day one
- **Vertical context preserved** — cloud, source control, and environment details set once per business domain

---

## How It Works (Simple Terms)

Forge works in two distinct phases: **Vertical Enrollment** (one-time setup per business domain) and **Service Generation** (everyday use).

---

### Phase A: Vertical Enrollment (One-Time Per Business Domain)

Before generating any services, a platform administrator enrolls the vertical — the business domain that owns those services (e.g., Clarivolt, ClariFrost).

#### Step A1: Vertical Identity
The admin names the vertical and assigns ownership:
- Vertical slug (e.g., `clarivolt`) — used as prefix in all generated services
- Display name, description, and owning team

#### Step A2: Cloud Provider
Select the cloud infrastructure provider:
- **AWS** — available and fully integrated
- Azure, GCP — visible but coming in future versions

Once selected, the admin provides AWS-specific details: Account ID, access role, and default region. No raw credentials — Forge uses AWS IAM role assumption.

#### Step A3: Source Control
Select the code repository platform:
- **GitHub** — default and fully integrated
- GitLab, Azure DevOps — visible but coming in future versions

The admin provides the GitHub organization name and an access token for Forge to create repositories automatically.

#### Step A4: Technical Defaults
Set default preferences for this vertical:
- Environment names (e.g., test, accp, prod)
- Default AWS region
- Default branch protection and repo visibility

#### Step A5: Review & Enroll
Confirm all details and enroll. The vertical is now registered and ready for service generation.

---

### Phase B: Service Generation (Everyday Use)

Once a vertical is enrolled, generating services is fast and context-free.

#### Step 1: Select Vertical
Choose an enrolled vertical (e.g., Clarivolt). All cloud, GitHub, and environment details are pre-loaded — no re-entry needed.

#### Step 2: Describe What's Needed
The admin fills out a simple form for each service:
- **Service name** (e.g., `intake-service`)
- **Type** — Backend service, web application, or both
- **Required capabilities** — File storage? Messaging? Database? AI assistance?
- **Number of services** — Can generate 5+ services at once

#### Step 3: Forge Generates Everything
In under 2 seconds, Forge creates:
- Complete source code structure
- All configuration files
- Security policies and compliance documentation
- Testing infrastructure
- Build and deployment pipelines
- Infrastructure-as-code templates
- Developer documentation

#### Step 4: Forge Sets Up Infrastructure
If requested, Forge automatically:
- Creates cloud resources (databases, storage, messaging queues) in the vertical's AWS account
- Configures security and access controls
- Sets up GitHub repositories with proper permissions in the vertical's GitHub org
- Stores connection credentials securely

#### Step 5: Team Starts Building
Developers receive a fully-configured project and immediately start writing business logic. No setup required.

---

## Business Value & ROI

### Time Savings
- **Per project**: 3-5 days → 2 seconds (99.9% reduction in setup time)
- **Per vertical** (10 services): 30-50 days → 20 seconds
- **Annual savings**: Assuming 20 new services/year = 60-100 developer-days saved

### Quality Improvements
- **Zero configuration defects** — eliminates entire class of setup errors
- **100% compliance** — every project follows security and coding standards automatically
- **Faster onboarding** — new developers work with familiar, consistent structure

### Strategic Benefits
- **Faster time-to-market** — new verticals launch weeks faster
- **Reduced technical debt** — consistent structure makes maintenance easier
- **Scalability** — can spin up new services instantly as business demands
- **Knowledge preservation** — best practices encoded in templates, not in people's heads

---

## Key Capabilities

### Vertical Management
Register and manage business domains (verticals) as first-class entities. Each vertical carries its own cloud account, GitHub organization, environment configuration, and naming context. Generate services within a vertical without re-entering infrastructure details.

### Multi-Service Generation
Generate 5, 10, or 20 services at once — all perfectly configured and ready to deploy — within a chosen vertical.

### Backend Services (BFF - Backend for Frontend)
Choose from 8 pre-integrated AWS capabilities:
- **File Storage** — Upload, download, manage files in cloud storage
- **Messaging** — Asynchronous communication between services
- **Caching** — High-speed data access
- **Search** — Full-text search across large datasets
- **Notifications** — Email and SMS delivery
- **Workflows** — Multi-step approval processes
- **AI Assistance** — AI-powered features via AWS Bedrock
- **Reporting** — Dashboard and analytics integration

### Web Applications (MFE - Microfrontends)
Choose from 14 pre-integrated Angular platform libraries:
- Authentication and authorization
- HTTP communication
- State management
- Error handling
- Internationalization (multiple languages)
- UI component libraries
- Mobile-responsive design

### Infrastructure Automation
- **One-click deployment** — Forge can create all cloud resources immediately
- **Template generation** — Also generates reusable infrastructure templates for future use
- **Multi-environment support** — Test, staging, and production configurations automatically handled

### GitHub Integration
- **Automatic repository creation** — One service = one repo, properly configured under the vertical's GitHub org
- **Security settings** — Branch protection, required reviews, and access controls pre-configured
- **CI/CD pipelines** — Automated testing and deployment workflows included

---

## Success Metrics

### Quantitative
- **Generation time**: < 2 seconds per service
- **Test coverage**: 90%+ on all generated code
- **First-time build success**: 100% (zero compilation errors)
- **Setup time reduction**: 99.9% (days → seconds)
- **Vertical enrollment time**: < 5 minutes

### Qualitative
- **Developer satisfaction**: Immediate productivity, zero frustration with setup
- **Consistency score**: 100% adherence to platform standards
- **Maintenance burden**: Reduced by 60%+ due to uniform structure

---

## Example Use Cases

### Enrolling a New Vertical (e.g., ClariFrost)
**Without Forge:**
- Manually document AWS account details for every engineer
- Re-enter GitHub org credentials for every new repo
- Inconsistent environment naming across services

**With Forge:**
- 5-minute vertical enrollment wizard
- Cloud + source control configured once, reused forever
- All 12 services generated in 30 seconds within the ClariFrost vertical

### Launching Services Within an Existing Vertical (e.g., Clarivolt)
**Without Forge:**
- Copy existing service, spend 2 days cleaning up and reconfiguring
- Risk of carrying over unnecessary dependencies
- Manual documentation updates

**With Forge:**
- Select Clarivolt vertical (pre-loaded cloud + GitHub context)
- Generate fresh service in 2 seconds
- Clean slate with only needed capabilities
- Documentation auto-generated and accurate

---

## Glossary

**BFF (Backend for Frontend)**  
A backend microservice that provides data and business logic to frontend applications.

**Canvas Libraries**  
Pre-built, production-ready Angular platform libraries (authentication, state management, UI components) for web applications.

**CI/CD (Continuous Integration/Continuous Deployment)**  
Automated pipelines that test, build, and deploy code changes.

**Genesis Modules**  
Pre-built, production-ready AWS integrations (file storage, messaging, caching, etc.) that can be added to any service.

**Infrastructure-as-Code (IaC)**  
Configuration files that define cloud resources (databases, storage, etc.) in a reproducible way. Forge generates both Terraform and AWS CDK formats.

**LocalStack**  
A tool that simulates AWS services on a developer's local machine for testing without cloud costs.

**MFE (Microfrontend)**  
A web application module that can be developed and deployed independently.

**Polyrepo**  
Architecture where each service lives in its own separate repository.

**Print / Scaffold**  
The complete project structure generated by Forge, ready for developers to add business logic.

**Vertical**  
A business domain or product line within Clarivex (e.g., Clarivolt, ClariFrost). Each vertical has its own cloud account, GitHub organization, and naming namespace. Verticals are enrolled once and then used as the context for all service generation within that domain.

**Vertical Enrollment**  
The one-time setup process for registering a vertical with Forge. Captures the vertical's identity, cloud provider credentials, source control configuration, and technical defaults.

---

## Frequently Asked Questions

### Does Forge replace developers?
No. Forge eliminates repetitive setup work so developers can focus on building valuable business features. It's a productivity multiplier, not a replacement.

### Can we customize generated projects?
Yes. Forge generates the foundation — developers add all business logic and customize as needed. The generated templates are starting points, not restrictions.

### What if requirements change after generation?
Forge generates infrastructure-as-code templates that can be modified and re-applied. Changes to infrastructure are version-controlled like code.

### Is Forge specific to Clarivex?
Currently yes — Forge embeds Pervaxis platform standards and Clarivex AWS integrations. However, the architecture is designed to be extensible for future multi-cloud or open-source scenarios.

### Who can use Forge?
Forge Launchpad is an admin-only tool. Platform and infrastructure administrators enroll verticals and generate projects for development teams.

### How many verticals can be enrolled?
Unlimited. Each vertical is fully isolated — separate cloud accounts, GitHub orgs, and naming namespaces.

---

## Next Steps

This document provides a high-level overview of Forge's capabilities and business value. For technical details, see:
- **FORGE_TECHNICAL_SPECIFICATION.md** — System architecture and implementation details
- **FORGE_BLUEPRINT_BFF.md** — Backend (Engine + API) development plan
- **FORGE_BLUEPRINT_UI.md** — Frontend (Launchpad + Angular Templates) development plan

---

**Pervaxis Forge — Accelerating Innovation Through Automation**  
*Clarivex Technologies © 2026*
