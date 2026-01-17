# Helios ETL

Containerized ETL batch processing engine for Sunny Rewards/Benefits platform. Runs as AWS Batch jobs for data transformations, imports, integrations, and scheduled operations.

## Overview

- **.NET 8** console application
- **AWS Batch** (Fargate) runtime
- **Docker containers** deployed via ECR
- **Multi-environment** support (Development, QA, UAT, Integ, Production, SBX)

## Architecture

```
┌─────────────────────┐
│   S3 / EventBridge  │  Triggers
│   SQS / Manual      │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│   AWS Batch         │
│   Job Queue         │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│   ECR Image         │  helios-{env}-batch-job-api-ecr
│   helios-etl        │  ASPNETCORE_ENVIRONMENT={Env}
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│   Job Execution     │  48 ETL job types
│   (Fargate Task)    │  Loads appsettings.{Env}.json
└─────────────────────┘
           │
           ▼
┌─────────────────────┐
│   Target Resources  │
│   RDS, Redshift,    │
│   S3, APIs, DynamoDB│
└─────────────────────┘
```

## ETL Job Types (48 total)

### Member & Eligibility Operations
- `SUBSCRIBE_ONLY_MEMBER_IMPORT` - Subscribe-only member imports
- `SCAN_S3_MEMBER_IMPORT` - S3-triggered member file processing
- `MEMBER_IMPORT_EVENTING` - Member import event processing
- `TRANSFER_REDSHIFT_TO_POSTGRES` - Sync members from Redshift to Postgres
- `TRANSFER_REDSHIFT_TO_SFTP` - Export member data to SFTP

### FIS Card Operations
- `CREATE_CARD_30` - Card creation (type 30)
- `PROCESS_CARD_30_RESPONSE` - Process card 30 response files
- `PROCESS_CARD_60_RESPONSE` - Process card 60 response files
- `UPDATE_USER_INFO_FIS` - Update user information with FIS

### Financial Transactions
- `MONETARY_TXN` - Monetary transaction processing
- `NON_MONETARY_TXN` - Non-monetary transaction processing
- `EXTERNAL_TXN_SYNC` - External transaction synchronization
- `SCAN_S3_MONETARY_TXN` - S3 monetary transaction scanning
- `SCAN_S3_NON_MONETARY_TXN` - S3 non-monetary transaction scanning
- `VALUE_LOAD` - Value load operations
- `VALUE_LOAD_CONSUMER_LIST` - Bulk value loads

### Benefits Funding
- `EXECUTE_BENEFITS_FUNDING` - Execute benefits funding
- `EXECUTE_BENEFITS_FUNDING_CONSUMER_LIST` - Bulk benefits funding

### Health & Wellness
- `PROCESS_HEALTH_TASK` - Health task processing
- `SCAN_S3_HSA_SWEEPER` - HSA sweeper S3 scanning

### Cohort & Segmentation
- `EXECUTE_COHORTING` - Execute cohort operations
- `SCAN_S3_IMPORT_COHORT_CONSUMER` - Import cohort consumers from S3

### Sweepstakes
- `SWEEPSTAKES_WINNER_REPORT` - Generate winner reports
- `SWEEPSTAKES_ENTRIES_REPORT` - Generate entries reports

### Tasks & Workflows
- `PROCESS_RECURRING_CONSUMER_TASKS` - Recurring task processing
- `TASK_UPDATE` - Task updates
- `TASK_UPDATE_CUSTOM_FORMAT` - Custom format task updates
- `SCAN_S3_TASK_IMPORT` - S3 task import scanning
- `PROCESS_CONSUMER_TASKS_COMPLETION` - Task completion processing

### Data Management
- `ENCRYPT_AND_COPY` - File encryption and transfer
- `PROCESS_FILE_CRYPTO` - File cryptography operations
- `PROCESS_DEPOSIT_INSTRUCTIONS_FILE` - Deposit instruction processing
- `RESTORE_COSTCO_BACKUP` - Costco data restoration

### Integration & Sync
- `EXECUTE_TENANT_SYNC` - Tenant configuration sync
- `PROCESS_RETAIL_PRODUCT_SYNC` - Retail product synchronization
- `PROCESS_NOTIFICATION_RULES` - Notification rule processing

### Reporting
- `GENERATE_WALLET_BALANCES_REPORT` - Wallet balance reporting

### Import Operations
- `IMPORT_TRIVIA` - Trivia content import
- `IMPORT_QUESTIONNAIRE` - Questionnaire import

### Utilities
- `DELETE_CONSUMERS` - Consumer deletion
- `CREATE_DUPLICATE_CONSUMER` - Duplicate consumer creation
- `CLEAR_WALLET_ENTRIES` - Clear wallet entries

## Configuration

### Environment-Specific Settings

Each environment has its own `appsettings.{Environment}.json` file with environment-specific AWS resources, API endpoints, and configuration.

### Configuration Parameters

```json
{
  "Logging": {
    "LogGroup": "helios-{env}-etl"
  },
  "AWS": {
    "AWS_BUCKET_NAME": "helios-{env}-customer-1-s3",
    "AWS_TMP_BUCKET_NAME": "helios-{env}-tmp-s3",
    "AWS_SNS_TOPIC_NAME": "arn:aws:sns:...:helios-{env}-etl-alerts-sns",
    "AWS_BATCH_JOB_QUEUE_ARN": "arn:aws:batch:...:job-queue/helios-{env}-batch-job-queue"
  },
  "DatafeedApi": "https://datafeed-api.{domain}/api/v1/",
  "AdminAPI": "http://admin-api.{domain}/api/v1/"
}
```

## Deployment

### Prerequisites

**GitHub Secrets:**
- AWS access credentials for each environment
- CodeArtifact access for NuGet package retrieval

**AWS Resources:**
- ECR repository for container images
- AWS Batch job queue and compute environment
- DynamoDB table for job definitions
- CloudWatch log group

### Deployment

Deployment is managed through GitHub Actions workflows. Each environment has a dedicated workflow that:
- Builds the .NET application
- Creates a Docker container
- Pushes to environment-specific ECR repository
- Sets `ASPNETCORE_ENVIRONMENT` to load correct configuration

### Build Process

1. **CodeArtifact Authentication** - Fetches NuGet packages from `heliosnugetrepo`
2. **Dotnet Build** - Builds .NET 8 application
3. **Dotnet Publish** - Publishes release artifacts
4. **Docker Build** - Creates container with `ASPNETCORE_ENVIRONMENT={Env}`
5. **ECR Push** - Pushes image with `latest` tag

## Job Definition Management

### DynamoDB Table Structure

**Schema:**
- `jobDefinitionId` (PK) - UUID from `JobDefinition.cs` constants
- `jobDefinition` - AWS Batch job definition name (e.g., `helios-sbx-etl-monetary-txn-jobdef`)
- `jobDefinitionDescription` - Description (usually same as `jobDefinition`)
- `createdBy` - Creator (usually "System")
- `createdTs` - ISO 8601 timestamp

**Example:**
```json
{
  "jobDefinitionId": "jdi-33f9a17a571e463492d9a3e9cba81f5d",
  "jobDefinition": "helios-{env}-etl-monetary-txn-jobdef",
  "jobDefinitionDescription": "helios-{env}-etl-monetary-txn-jobdef",
  "createdBy": "System",
  "createdTs": "2025-11-13T05:01:57.233304"
}
```

### Adding New Job Definitions

1. Add constant to `src/Common/Constants/JobDefinition.cs`
2. Create corresponding AWS Batch job definition in Terraform
3. Add entry to DynamoDB `job_definition_{env}` table
4. Deploy updated ETL container

## Development

### Local Development

**Prerequisites:**
- .NET 8 SDK
- AWS credentials configured
- Access to CodeArtifact

**Build:**
```bash
dotnet restore src/App/SunnyRewards.Helios.ETL.App.csproj
dotnet build src/App/SunnyRewards.Helios.ETL.App.csproj
```

**Run Locally:**
```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project src/App/SunnyRewards.Helios.ETL.App.csproj
```

### Testing

```bash
# Run unit tests
dotnet test src/UnitTests/SunnyRewards.Helios.ETL.UnitTests.csproj
```

## Monitoring

### CloudWatch Logs

**Log Group:** `helios-{env}-etl`
**Log Streams:** One per Batch job execution

**Query Example:**
```
fields @timestamp, @message
| filter @message like /ERROR/
| sort @timestamp desc
| limit 100
```

### SNS Alerts

**Topic:** `helios-{env}-etl-alerts-sns`
**Subscriptions:** Email notifications for ETL failures

## Troubleshooting

### Common Issues

**1. Job Fails with "Environment not found"**
- Verify `ASPNETCORE_ENVIRONMENT` is set correctly in Batch job definition
- Ensure `appsettings.{Env}.json` exists in container

**2. CodeArtifact Authentication Failure**
- Check AWS credentials have `codeartifact:GetAuthorizationToken` permission
- Verify `script.sh` completes successfully during Docker build

**3. S3 Access Denied**
- Verify Batch job execution role has S3 permissions
- Check bucket names in `appsettings.{Env}.json` are correct

**4. API Calls Timeout**
- Verify network connectivity (VPC, security groups)
- Check API endpoints are correct for environment
- Verify DNS resolution for API endpoints

**5. Missing Job Definition in DynamoDB**
- Populate DynamoDB table with job definitions
- Verify `jobDefinitionId` matches constant in `JobDefinition.cs`

### Debug Mode

Enable verbose logging by updating `appsettings.{Env}.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

## Project Structure

```
helios-etl/
├── .github/workflows/          # GitHub Actions deployment workflows
├── db/scripts/                 # Database migration scripts
├── etl-scripts/                # Sample ETL scripts
├── sample_input/               # Sample input files for testing
├── src/
│   ├── App/                    # Main application entry point
│   │   ├── Program.cs
│   │   └── appsettings.*.json  # Environment-specific configs
│   ├── Common/                 # Shared constants and utilities
│   │   └── Constants/
│   │       └── JobDefinition.cs # Job definition ID constants
│   ├── Core/                   # Domain models and DTOs
│   ├── Infrastructure/         # Services, repositories, integrations
│   └── UnitTests/              # Unit tests
├── Dockerfile                  # Main Docker build file
├── script.sh                   # CodeArtifact authentication script
└── README.md                   # This file
```

## Dependencies

- **.NET 8.0** - Runtime framework
- **AWS SDK** - AWS service integrations
- **AutoMapper** - Object mapping
- **Dapper** - Micro ORM for database access
- **Npgsql** - PostgreSQL driver
- **CsvHelper** - CSV file processing
- **Helios Common Libraries** - Internal shared packages (from CodeArtifact)

## Related Infrastructure

- **Terraform Module:** `sunnybenefits-iac/helios-lambda-function/modules/batch`
- **Batch Job Definitions:** Defined in `lambdas-etl-batch.tf`
- **EKS Services:** API endpoints called by ETL jobs
- **S3 Buckets:** Source and destination for ETL operations
- **RDS/Redshift:** Database sources/targets

## Contributing

**Branch Strategy:**
- `develop` - Active development
- `release/{version}-{env}` - Environment-specific releases
- `release-{env}` - Ongoing release branches
- `release-hotfix` - Hotfix branches for production issues

**Deployment Flow:**
1. Develop on feature branches
2. Merge to `develop` via PR
3. Create release branch for target environment
4. Deploy via GitHub Actions workflow
5. Tag release with version number

## Support

- **Infrastructure Issues:** Check `sunnybenefits-iac/helios-infrastructure-operations-kb`
- **Application Logs:** CloudWatch Logs `helios-{env}-etl`
- **Batch Job Status:** AWS Batch Console → Job Queues → Job History
- **Job Definitions:** DynamoDB table `job_definition_{env}`

## License

Internal - Sunny Rewards/Benefits Platform
