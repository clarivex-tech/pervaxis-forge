# Identity Token Enrichment print

Forge embeds the templates in this folder into its existing Engine. Select AWS and
Identity Token Enrichment in Launchpad, supplying the region and an existing user
pool ID. The output is a ZIP containing a SAM/CloudFormation template with inline
Python, tests, example events, deployment defaults and operator instructions.

The handler has no database or SDK dependency. Trusted provisioning assigns
`custom:tenant_id`; the handler validates and canonicalizes it into access-token
`tenant_id`. Provisioning and tenant administration belong to the backend.

Deploy once per pool. CloudFormation creates the Lambda, logs, error alarm and
pool-scoped invocation permission. Attach the V2_0 trigger in the Cognito console
as a separate explicit step; no generated resource updates or replaces the pool.
Existing pre-token triggers require integration rather than blind replacement.

For generation and validation tests, run the Engine and API test projects.
The generated README explains running Python tests against the actual inline code.
Run `python validate-print.py` here for the dependency-free Python contract tests;
Forge PR and deployment CI also run these tests with Python 3.13.
