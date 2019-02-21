# Delete CloudFormation Stack
echo $1
echo "Deleting CloudFormation Stack"
aws cloudformation delete-stack --stack-name $1

aws cloudformation wait stack-delete-complete --stack-name $1

echo "Cloudformation Stack deleted"
