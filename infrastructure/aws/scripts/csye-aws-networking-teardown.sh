#!/bin/bash
echo "Enter VPCiD"
read VPCid
echo "Deleting all the subnets"
for i in `aws ec2 describe-subnets --filters Name=vpc-id,Values="${VPCid}" | grep subnet- | sed -E 's/^.*(subnet-[a-z0-9]+).*$/\1/'`; do aws ec2 delete-subnet --subnet-id=$i; done

echo "Detaching Internet Gateways"
for i in `aws ec2 describe-internet-gateways --filters Name=attachment.vpc-id,Values="${VPCid}" | grep igw- | sed -E 's/^.*(igw-[a-z0-9]+).*$/\1/'`; do aws ec2 detach-internet-gateway --internet-gateway-id=$i --vpc-id=$VPCid; done

echo "Deleting Internet Gateway"
aws ec2 delete-internet-gateway --internet-gateway-id=$i

echo "Deleting route"
aws ec2 delete-route --route-table-id=$i --destination-cidr-block 0.0.0.0/0

echo "Deleting routetable"
for i in `aws ec2 describe-route-tables --filters Name=vpc-id,Values="${VPCid}" | grep rtb- | sed -E 's/^.*(rtb-[a-z0-9]+).*$/\1/'`; do aws ec2 delete-route-table --route-table-id=$i; done

echo "Deleting vpc"
aws ec2 delete-vpc --vpc-id ${VPCid}

echo "Thankyou , Network Teardown is successful"


