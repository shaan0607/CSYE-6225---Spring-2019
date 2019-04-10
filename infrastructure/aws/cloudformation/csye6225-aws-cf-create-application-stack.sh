echo "Please enter Network Stack Name:"
read networkStackName
if [ -z "$networkStackName" ]
then
	echo "StackName error exiting!"
	exit 1
fi
echo "$networkStackName"


#application
echo "Please enter Application Stack Name:"
read appStackName
if [ -z "$appStackName" ]
then
	echo "StackName error exiting!"
	exit 1
fi
echo "$appStackName"



echo "Please enter an active keyPair to associate with EC2:"
read keyName
if [ -z "$keyName" ]
then
	echo "StackName error exiting!"
	exit 1
fi
echo "$keyName"

#echo "Please enter the ImageID of centos AMI  created"

echo "Your latest AMI ID is:"
imageid=$(aws ec2 describe-images --filters "Name=name,Values=csye6225*" --query "sort_by(Images, &CreationDate)[-1].[ImageId]" --output "text")
if [ $? -eq 0 ]
then
        echo "$imageid"
else
        echo "AMI Id retrival Failed"
        exit 0
fi





vpcID=$(aws ec2 describe-vpcs --query 'Vpcs[].{VpcId:VpcId}' \
--filters "Name=tag:Name,Values=$networkStackName-csye6225-vpc" "Name=is-default, Values=false" --output text 2>&1)


if [ -z "$vpcID" ]
then
	echo "VPC ID error \n Exiting"
	exit 1
fi




subnet=$(aws ec2 describe-subnets --filters Name=vpc-id,Values=${vpcID})
subnetid1=$(echo -e "$subnet" | jq '.Subnets[0].SubnetId' | tr -d '"')
subnetid2=$(echo -e "$subnet" | jq '.Subnets[1].SubnetId' | tr -d '"')
subnetid3=$(echo -e "$subnet" | jq '.Subnets[2].SubnetId' | tr -d '"')

CERTIFICATE=$(aws acm list-certificates --query 'CertificateSummaryList[0].CertificateArn' --output text)
CERTIFICATENOWAF=$(aws acm list-certificates --query 'CertificateSummaryList[1].CertificateArn' --output text)
if [ -z "$subnetid1" ]
then
	echo "Subnet ID 1 error \n Exiting"
	exit 1
fi




if [ -z "$subnetid2" ]
then
	echo "Subnet ID 2 error \n Exiting"
	exit 1
fi


if [ -z "$subnetid3" ]
then
	echo "Subnet ID 3 error \n Exiting"
	exit 1
fi

export circleciuser=circleci
DOMAIN_NAME=$(aws route53 list-hosted-zones --query HostedZones[0].Name --output text)
Bucket="${DOMAIN_NAME}csye6225.com"



DOMAIN_NAME1=$(aws route53 list-hosted-zones --query HostedZones[0].Name --output text)

CD_DOMAIN="code-deploy."${DOMAIN_NAME1%?}
nWafDomain="nowaf."${DOMAIN_NAME}


# Create CloudFormation Stack
echo "Validating template"
TMP_code=`aws cloudformation validate-template --template-body file://./csye6225-cf-auto-scaling-application.json`
if [ -z "$TMP_code" ]
then
	echo "Template error exiting!"
	exit 1
fi
echo "Cloudformation template validation success"

echo "Now Creating CloudFormation Stack"


CRTSTACK_Code=`aws cloudformation create-stack --stack-name $appStackName --template-body file://./csye6225-cf-auto-scaling-application.json --capabilities CAPABILITY_NAMED_IAM --parameters   ParameterKey=KeyName,ParameterValue=$keyName ParameterKey=myVpc,ParameterValue=$vpcID ParameterKey=circleci,ParameterValue=$circleciuser ParameterKey=nWafDomain,ParameterValue=$nWafDomain ParameterKey=PublicSubnetKey1,ParameterValue=$subnetid1 ParameterKey=PublicSubnetKey2,ParameterValue=$subnetid2 ParameterKey=PublicSubnetKey3,ParameterValue=$subnetid3 ParameterKey=CERTIFICATENOWAF,ParameterValue=$CERTIFICATENOWAF  ParameterKey=ImageID,ParameterValue=$imageid ParameterKey=Bucket,ParameterValue=arn:aws:s3:::$Bucket ParameterKey=Bucket1,ParameterValue=arn:aws:s3:::$Bucket/* ParameterKey=CDARN,ParameterValue=arn:aws:s3:::$CD_DOMAIN ParameterKey=CDARN1,ParameterValue=arn:aws:s3:::$CD_DOMAIN/* ParameterKey=Bucket3,ParameterValue=$Bucket ParameterKey=S3Bucket,ParameterValue=$Bucket ParameterKey=CERTIFICATE,ParameterValue=$CERTIFICATE ParameterKey=domain,ParameterValue=$DOMAIN_NAME`

if [ -z "$CRTSTACK_Code" ]
then
	echo "Stack Creation error exiting!"
	exit 1
fi
aws cloudformation wait stack-create-complete --stack-name $appStackName
echo "Application Stack Created"
echo "Check AWS Cloudformation"
