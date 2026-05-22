# Session Log

## Session: Launchpad AWS Deploy Setup

**Branch:** `feature/launchpad-aws-deploy` (cut from `develop`)

### Goal
Deploy the Angular (Nx) Launchpad app to AWS S3 + CloudFront via GitHub Actions.

### Decisions Made
- **S3 buckets:** `forge-launchpad-accp` and `forge-launchpad-prod`
- **Region:** `us-east-1`
- **Custom domain:** None for now — default CloudFront domain
- **GitHub secrets:** Repo-level (no environment scoping)
- **accp API URL:** `https://trdy33o0b2.execute-api.us-east-1.amazonaws.com/api/v1`
- **prod API URL:** `PROD_API_BASE_URL_PLACEHOLDER` (to be updated when prod is live)

### Files Changed on Branch
| File | Change |
|------|--------|
| `.github/workflows/launchpad-deploy.yml` | Updated with S3 sync + CloudFront invalidation |
| `apps/launchpad/project.json` | Added `fileReplacements` for production + new `accp` build config |
| `apps/launchpad/src/environments/environment.accp.ts` | New file — accp API URL |
| `apps/launchpad/src/environments/environment.prod.ts` | Updated placeholder |
| `appsettings.json` | CORS updated with CloudFront domain placeholders |
| `LAUNCHPAD_AWS_INFRA_SETUP.md` | New one-time AWS setup guide |

### GitHub Secrets Still Needed
These must be added after AWS infra is created:
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `CF_DISTRIBUTION_ID_ACCP`
- `CF_DISTRIBUTION_ID_PROD`

### Next Step
Commit the changes and push the branch (`feature/launchpad-aws-deploy`).
