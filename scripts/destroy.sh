cd "$(dirname "$0")"
source ./vars.config
cd ../pulumi/infra

export PULUMI_CONFIG_PASSPHRASE=$PULUMI_CONFIG_PASSPHRASE
pulumi stack select $STACK_NAME
pulumi destroy -y
pulumi stack rm $STACK_NAME -y