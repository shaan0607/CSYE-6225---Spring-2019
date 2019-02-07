STACK_NAME=$1
echo "Creating VPC"
VPCid=`aws ec2 create-vpc --cidr-block 10.0.0.0/16 --query 'Vpc.VpcId' --output text`
aws ec2 create-tags --resources $VPCid --tags Key=Name,Value=$STACK_NAME-csye6225-vpc
echo "Vpc created-> Vpc Id:  "$VPCid

echo "Creating Subnet1"
echo "Enter subnet1 Name"
read "subnet1_name1"
echo "Enter CIDR block details for subnet1"
read "subnet1_cidr1"
echo "Enter availability zone"
read "subnet1_az1"
subnet1_id=`aws ec2 create-subnet --vpc-id $VPCid --cidr-block $subnet1_cidr1 --availability-zone $subnet1_az1 --query 'Subnet.SubnetId' --output text`
aws ec2 create-tags --resources $subnet1_id --tags Key=Name,Value=$STACK_NAME-subnet1

echo "Creating Subnet2"
echo "Enter subnet2 Name"
read "subnet2_name2"
echo "Enter CIDR block details for subnet2"
read "subnet2_cidr2"
echo "Enter availability zone"
read "subnet2_az2"
subnet2_id=`aws ec2 create-subnet --vpc-id $VPCid --cidr-block $subnet2_cidr2 --availability-zone $subnet2_az2 --query 'Subnet.SubnetId' --output text`
aws ec2 create-tags --resources $subnet2_id --tags Key=Name,Value=$STACK_NAME-subnet2

echo "Creating Subnet3"
echo "Enter subnet3 Name"
read "subnet3_name3"
echo "Enter CIDR block details for subnet3"
read "subnet3_cidr3"
echo "Enter availability zone"
read "subnet3_az3"
subnet3_id=`aws ec2 create-subnet --vpc-id $VPCid --cidr-block $subnet3_cidr3 --availability-zone $subnet3_az3 --query 'Subnet.SubnetId' --output text`
aws ec2 create-tags --resources $subnet3_id --tags Key=Name,Value=$STACK_NAME-subnet3


echo "Creating InternetGateway"
InternetGatewayId=`aws ec2 create-internet-gateway --query 'InternetGateway.InternetGatewayId' --output text`
aws ec2 create-tags --resources $InternetGatewayId --tags Key=Name,Value=$STACK_NAME-csye6225-InternetGateway

echo "Attaching the internet gateway to vpc"
aws ec2 attach-internet-gateway --internet-gateway-id $InternetGatewayId --vpc-id $VPCid

echo "creating route table"
routeTableId=`aws ec2 create-route-table --vpc-id $VPCid --query 'RouteTable.RouteTableId' --output text`
aws ec2 create-tags --resources $routeTableId --tags Key=Name,Value=$STACK_NAME-csye6225-public-route-table

aws ec2 create-route --route-table-id $routeTableId --destination-cidr-block 0.0.0.0/0 --gateway-id $InternetGatewayId

aws ec2 associate-route-table --subnet-id $subnet1_id --route-table-id $routeTableId
echo "subnet is associated with route table"

aws ec2 associate-route-table --subnet-id $subnet2_id --route-table-id $routeTableId
echo "subnet is associated with route table"

aws ec2 associate-route-table --subnet-id $subnet3_id --route-table-id $routeTableId
echo "subnet is associated with route table"

groupid=$(aws ec2 describe-security-groups --filters Name=vpc-id,Values=$VPCid --query "SecurityGroups[*].{ID:GroupId}" --output text)
echo $groupid
echo "Removing default security rule"
aws ec2 revoke-security-group-ingress --group-id $groupid --protocol "-1" --port -1 --source-group $groupid
aws ec2 revoke-security-group-egress --group-id $groupid --protocol "-1" --port -1 --cidr 0.0.0.0/0
echo "Creatiing new rule for port 22, 80"
aws ec2 authorize-security-group-ingress --group-id $groupid --protocol tcp --port 22 --cidr 0.0.0.0/0
aws ec2 authorize-security-group-ingress --group-id $groupid --protocol tcp --port 80 --cidr 0.0.0.0/0

echo "Task completed successfully"

