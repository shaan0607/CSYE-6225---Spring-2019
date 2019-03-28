echo "Please enter Serverless Stack Name:"
read appStackName
if [ -z "$appStackName" ]
then
	echo "StackName error exiting!"
	exit 1
fi
echo "$appStackName"
echo "Fetching domain name from Route 53"
DOMAINNAME=$(aws route53 list-hosted-zones --query HostedZones[0].Name --output text)
DOMAINNAME="${DOMAINNAME%?}"
echo "DOMAIN_NAME:- $DOMAINNAME"

LAMBDABUCKET="code-deploy."${DOMAINNAME}
echo "LAMBDA_BUCKET:- $LAMBDABUCKET"



AccountId=$(aws iam get-user|python -c "import json as j,sys;o=j.load(sys.stdin);print o['User']['Arn'].split(':')[4]")
echo "AccountId: $AccountId"

SNSTOPIC_ARN="arn:aws:sns:us-east-1:$AccountId:SNSTopicResetPassword"
echo "SNSTOPIC_ARN: $SNSTOPIC_ARN"

createres=$(aws cloudformation create-stack --stack-name $appStackName-serverless --capabilities "CAPABILITY_NAMED_IAM" --template-body file://csye6225-aws-cf-lambda.json --parameters ParameterKey=LAMBDABUCKET,ParameterValue=$LAMBDABUCKET ParameterKey=SNSTOPICARN,ParameterValue=$SNSTOPIC_ARN ParameterKey=DOMAINNAME,ParameterValue=$DOMAINNAME)
resp=$(aws cloudformation wait stack-create-complete --stack-name $appStackName-serverless)
if [[ -z "$resp" ]]; then
  echo Stack "$appStackName" sucessfully created
else
  echo "$resp"
  exit 1
fi
STACKDETAILS=$(aws cloudformation describe-stacks --stack-name $appStackName-serverless --query Stacks[0].StackId --output text)
