## Requirements

- [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download)
- [Pulumi 3.x](https://www.pulumi.com/docs/get-started/install/)
- [AWS CLI](https://aws.amazon.com/cli/)

> Make sure the default AWS CLI profile is configured or run `aws configure` and fill the inputs.

> Configure script variables in `./scripts/vars.config` (default values are already set and usable)

## Create the environment

Run `./scripts/deploy.sh` to:

- Build the lambda binaries in `./src` (only if missing)
- Login locally into pulumi. This will create $user/.pulumi folder where stacks states and history will be stored.
- Create & configure the DEV stack.
- Perform a "pulumi update" which will create all the resources in the AWS cloud.

## Consume & monitor application 

After creating or updating, you will be provided with the HTTP API URL in the console under the "Outputs" section. 

Available endpoints:
- GET /users | Returns mock data from a lambda
- POST /users | Creates a SQS message which triggers a lambda

Both lambdas have logs implemented which can be consumed from CloudWatch.

Logging can be manually enabled in the API Gateway for further debugging.

## Updates

After changing the code run `./scripts/deploy.sh` to execute a Pulumi update. If `./src` code has been modified, run `./scripts/build.sh` before the update.

> Note: The API Gateway has auto-deploy enabled.

## Delete all resources

In order to delete all AWS resources run `./scripts/destroy.sh`

## Known issues

- The post endpoint seems to have an issue and always returns 500 without inserting the message in the queue.
