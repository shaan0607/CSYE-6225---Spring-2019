#!/bin/bash -e

if [ -z "$1" ]
then
	echo "No command line argument provided for stack STACK_NAME"
	exit 1
else
	echo "Started with deletion of WAF stack in cloud formation"
fi

RC=$(aws cloudformation describe-stacks --stack-name $1-waf --query Stacks[0].StackId --output text)

if [ $? -eq 0 ]
then
	continue
else
	echo "Stack $1 doesn't exist..."
	exit 0
fi


RC=$(aws cloudformation delete-stack --stack-name $1-waf)
echo "Deletion in progress..."
RC=$(aws cloudformation wait stack-delete-complete --stack-name $1-waf)

if [ $? -eq 0 ]
then
  echo "WAF stack deletion complete successfully"
else
 	echo "Failed Stack deletion"
 	exit 1
fi
