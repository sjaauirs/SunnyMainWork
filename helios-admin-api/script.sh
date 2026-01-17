#!/bin/bash
export AWS_ACCESS_KEY_ID="$1"
export AWS_SECRET_ACCESS_KEY="$2"
push_nuget="$3"

export AWS_DEFAULT_REGION=us-east-2
export PATH="$PATH:/root/.dotnet/tools"

dotnet tool install -g AWS.CodeArtifact.NuGet.CredentialProvider --version 1.0.1
dotnet codeartifact-creds install
apt update ; apt install unzip -y ; apt install nuget -y
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
chmod +x ./aws/install
bash ./aws/install
aws --version
CODEARTIFACT_AUTH_TOKEN=`aws codeartifact get-authorization-token --domain heliosnugetrepo --domain-owner 375611915274 --query authorizationToken --output text`
aws codeartifact login --tool dotnet --repository helios-common-lib --domain heliosnugetrepo --domain-owner 375611915274
dotnet nuget update source heliosnugetrepo -s "https://heliosnugetrepo-375611915274.d.codeartifact.us-east-2.amazonaws.com/nuget/helios-common-lib/v3/index.json" --password ${CODEARTIFACT_AUTH_TOKEN} --username aws --store-password-in-clear-text

if [ "$push_nuget" = "true" ] ; then
	nuget_path="$4"
	dotnet nuget push ${nuget_path}/nuget/*.nupkg --skip-duplicate -s https://heliosnugetrepo-375611915274.d.codeartifact.us-east-2.amazonaws.com/nuget/helios-common-lib/v3/index.json
fi
