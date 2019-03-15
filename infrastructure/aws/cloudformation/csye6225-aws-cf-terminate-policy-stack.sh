#!/bin/bash

echo "Please enter Policy Stack Name:"
read appStackName
if [ -z "$appStackName" ]
then
	echo "StackName error exiting!"
	exit 1
fi

RC=$(aws cloudformation describe-stacks --stack-name $appStackName --query Stacks[0].StackId --output text)

if [ $? -eq 0 ]
then
	echo "Stack $appStackName "
else
	echo "Stack $appStackName doesn't exist"
	exit 0
fi

# Domain name for ARN
echo "Fetching domain name from Route 53"
DOMAIN_NAME=$(aws route53 list-hosted-zones --query HostedZones[0].Name --output text)
echo "$DOMAIN_NAME"

# Emptying the code-deploy.$DOMAIN bucket
echo "Emptying the code deploy bucket"
RC=$(aws s3 rm s3://"code-deploy."${DOMAIN_NAME%?} --recursive)

if [ $? -eq 0 ]
then
  echo "Bucket successfully emptied"
else
 	echo "Emptying the bucket failed"
 	exit 1
fi

echo "Deleting stack: $RC"

aws cloudformation delete-stack --stack-name $appStackName

echo "Stack deletion in progress. Please wait"
RC=$(aws cloudformation wait stack-delete-complete --stack-name $appStackName)

if [ $? -eq 0 ]
then
  echo "Application stack deletion complete"
else
 	echo "Failed Stack deletion"
 	exit 1
fi
