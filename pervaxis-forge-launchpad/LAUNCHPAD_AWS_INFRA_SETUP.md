# Launchpad AWS Infrastructure Setup

One-time setup guide for deploying the Launchpad Angular app to AWS S3 + CloudFront.

> **Run this once per environment.** After completing these steps, deployments are fully automated via GitHub Actions.

---

## Prerequisites

- AWS CLI installed and configured with sufficient permissions
- Access to the GitHub repository settings (to add secrets)

---

## 1. Create S3 Buckets

```bash
# ACCP
aws s3api create-bucket \
  --bucket forge-launchpad-accp \
  --region us-east-1

# PROD
aws s3api create-bucket \
  --bucket forge-launchpad-prod \
  --region us-east-1
```

Block all public access on both buckets (CloudFront will serve the content):

```bash
for BUCKET in forge-launchpad-accp forge-launchpad-prod; do
  aws s3api put-public-access-block \
    --bucket $BUCKET \
    --public-access-block-configuration \
      "BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true"
done
```

---

## 2. Create CloudFront Distributions

Create one distribution per environment. Key settings:

| Setting | Value |
|---|---|
| Origin domain | `<bucket-name>.s3.us-east-1.amazonaws.com` |
| Origin access | Origin Access Control (OAC) — **not** legacy OAI |
| Default root object | `index.html` |
| Viewer protocol policy | Redirect HTTP to HTTPS |
| Cache policy | `CachingDisabled` (or a short TTL — the deploy workflow invalidates on every push) |
| Custom error response | 403 → `/index.html` (status 200) — required for Angular client-side routing |

After creating each distribution, note the **Distribution ID** — you'll need it for the GitHub secrets.

### Grant CloudFront access to S3

After creating the OAC, attach a bucket policy to each bucket:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "AllowCloudFrontOAC",
      "Effect": "Allow",
      "Principal": {
        "Service": "cloudfront.amazonaws.com"
      },
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::forge-launchpad-accp/*",
      "Condition": {
        "StringEquals": {
          "AWS:SourceArn": "arn:aws:cloudfront::<ACCOUNT_ID>:distribution/<CF_DISTRIBUTION_ID_ACCP>"
        }
      }
    }
  ]
}
```

Repeat for the prod bucket, substituting the prod distribution ID.

---

## 3. Create a Deployment IAM User

Create a dedicated IAM user for GitHub Actions with the minimum required permissions:

```bash
aws iam create-user --user-name forge-launchpad-deployer
```

Attach this inline policy:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",
        "s3:DeleteObject",
        "s3:ListBucket"
      ],
      "Resource": [
        "arn:aws:s3:::forge-launchpad-accp",
        "arn:aws:s3:::forge-launchpad-accp/*",
        "arn:aws:s3:::forge-launchpad-prod",
        "arn:aws:s3:::forge-launchpad-prod/*"
      ]
    },
    {
      "Effect": "Allow",
      "Action": "cloudfront:CreateInvalidation",
      "Resource": [
        "arn:aws:cloudfront::<ACCOUNT_ID>:distribution/<CF_DISTRIBUTION_ID_ACCP>",
        "arn:aws:cloudfront::<ACCOUNT_ID>:distribution/<CF_DISTRIBUTION_ID_PROD>"
      ]
    }
  ]
}
```

Generate access keys:

```bash
aws iam create-access-key --user-name forge-launchpad-deployer
```

---

## 4. Add GitHub Secrets

Go to **GitHub → Repository → Settings → Secrets and variables → Actions** and add:

| Secret name | Value |
|---|---|
| `AWS_ACCESS_KEY_ID` | Access key ID from step 3 |
| `AWS_SECRET_ACCESS_KEY` | Secret access key from step 3 |
| `CF_DISTRIBUTION_ID_ACCP` | CloudFront distribution ID for ACCP |
| `CF_DISTRIBUTION_ID_PROD` | CloudFront distribution ID for PROD |

---

## 5. Update PROD API URL

Once the production API is live, replace the placeholder in `environment.prod.ts`:

```typescript
// apps/launchpad/src/environments/environment.prod.ts
apiBaseUrl: 'PROD_API_BASE_URL_PLACEHOLDER',  // ← replace this
```

---

## Deployment Flow (post-setup)

| Git push to | Builds with | Deploys to |
|---|---|---|
| `develop` | `accp` config | `forge-launchpad-accp` S3 → ACCP CloudFront |
| `main` | `production` config | `forge-launchpad-prod` S3 → PROD CloudFront |
