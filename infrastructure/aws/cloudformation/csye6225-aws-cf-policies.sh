echo Enter Stack name
#read stack name
read sn
#create stack
{
  validresp=$(aws cloudformation validate-template --template-body file://csye6225-aws-cf-policies.json) &&
  echo "Template validated"
} || {
  echo "$validresp"
  echo "Invalid Template"
  exit 1
}
export circleciuser=circleci

DOMAIN_NAME=$(aws route53 list-hosted-zones --query HostedZones[0].Name --output text)
Bucket="${DOMAIN_NAME}csye6225.com"

DOMAIN_NAME1=$(aws route53 list-hosted-zones --query HostedZones[0].Name --output text)

CD_DOMAIN="code-deploy."${DOMAIN_NAME1%?}

createres=$(aws cloudformation create-stack  --stack-name $sn --capabilities CAPABILITY_NAMED_IAM --template-body file://csye6225-aws-cf-policies.json  --parameters ParameterKey=circleci,ParameterValue=$circleciuser  ParameterKey=Bucket,ParameterValue=arn:aws:s3:::$Bucket ParameterKey=Bucket1,ParameterValue=arn:aws:s3:::$Bucket/* ParameterKey=CDARN,ParameterValue=arn:aws:s3:::$CD_DOMAIN ParameterKey=CDARN1,ParameterValue=arn:aws:s3:::$CD_DOMAIN/* ParameterKey=CDAPPNAME,ParameterValue=csye6225-webapp)
echo Creating stack "$sn". Please wait...
resp=$(aws cloudformation wait stack-create-complete --stack-name $sn)
if [[ -z "$resp" ]]; then
  echo Stack "$sn" sucessfully created
else
  echo "$resp"
  exit 1
fi
