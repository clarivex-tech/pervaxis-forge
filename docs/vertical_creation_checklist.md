# Vertical Creation Checklist

Pre-flight checklist before enrolling a new vertical in Pervaxis Forge.

---

## 1. AWS IAM — Forge service identity

Forge calls `sts:AssumeRole` on the vertical's deployment role. The identity running Forge must have permission to do so.

**Dev:** `arn:aws:iam::198452822669:user/anand-clarivex-admin`  
**Prod:** Forge ECS task role (TBD — Phase 3)

### Attach this inline policy to the Forge identity (one-time, already required)

```json
{
  "Version": "2012-10-17",
  "Statement": [{
    "Effect": "Allow",
    "Action": "sts:AssumeRole",
    "Resource": "arn:aws:iam::*:role/*"
  }]
}
```

> Scope `Resource` to specific role ARNs in production.

---

## 2. AWS IAM — vertical deployment role (per vertical)

Each vertical requires a dedicated IAM role that Forge can assume to provision resources.

### Create the role

- **Role name convention:** `<vertical-slug>-forge-deployment` (e.g. `clarivolt-forge-deployment`)
- **Role type:** Another AWS account → or → Same account with trust policy below
- **Permissions:** Attach whatever policies the vertical's infrastructure provisioning needs (e.g. `PowerUserAccess` to start, scoped down later)

### Trust policy (allow Forge to assume the role)

```json
{
  "Version": "2012-10-17",
  "Statement": [{
    "Effect": "Allow",
    "Principal": {
      "AWS": "arn:aws:iam::198452822669:user/anand-clarivex-admin"
    },
    "Action": "sts:AssumeRole"
  }]
}
```

> In production replace `user/anand-clarivex-admin` with the Forge ECS task role ARN.

### Verify assume-role works before enrolling

```bash
aws sts assume-role \
  --role-arn arn:aws:iam::198452822669:role/<role-name> \
  --role-session-name forge-connectivity-test
```

Should return temporary credentials. If it errors, fix trust policy before enrolling.

---

## 3. GitHub — source control access

- Create or identify a GitHub Personal Access Token (PAT) or GitHub App token with access to the target org.
- Required scopes: `repo`, `admin:org` (read)
- The org must be accessible with the token: `curl -H "Authorization: Bearer <token>" https://api.github.com/orgs/<org>`

---

## 4. Enrollment request fields

| Field | Example | Notes |
|---|---|---|
| `slug` | `clarivolt` | Lowercase, kebab-case, unique, no trailing hyphen |
| `displayName` | `Clarivolt` | 1–255 chars |
| `ownerTeam` | `Platform Team` | 1–255 chars |
| `ownerEmail` | `team@clarivex.tech` | Valid email |
| `componentPrefix` | `CLV` | 2–5 letters, unique across verticals, used to prefix generated resource names |
| `cloudProvider.provider` | `AWS` | Must be `AWS` (Phase 0) |
| `cloudProvider.awsAccountId` | `198452822669` | 12-digit account ID |
| `cloudProvider.iamRoleArn` | `arn:aws:iam::198452822669:role/clarivolt-forge-deployment` | Must pass `sts:AssumeRole` check |
| `cloudProvider.defaultRegion` | `us-east-1` | Regex `^[a-z]{2}-[a-z]+-\d$` |
| `sourceControl.platform` | `GitHub` | Must be `GitHub` (Phase 0) |
| `sourceControl.gitHubOrg` | `clarivex-tech` | No whitespace |
| `sourceControl.accessToken` | `ghp_...` | PAT or GitHub App token |
| `sourceControl.defaultVisibility` | `Private` | `Private` or `Public` |
| `techDefaults.environments` | `["dev","staging","prod"]` | Non-empty, kebab-case, unique |
| `techDefaults.defaultEnvironment` | `dev` | Must be one of `environments` |

---

## 5. Post-enrollment

- [ ] Call `POST /api/v1/verticals/{slug}/validate` — confirms AWS role is assumable and GitHub org is reachable.
- [ ] Verify response shows `awsConnectivity.success: true` and `gitHubConnectivity.success: true`.
- [ ] If either fails, check IAM trust policy and GitHub token scopes, then re-validate.
