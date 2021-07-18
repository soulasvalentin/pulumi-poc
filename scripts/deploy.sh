cd "$(dirname "$0")"
source ./vars.config
cd ..

# check if lambda binaries exist
for d in src/*/; do
  DIR="$d"bin/release/netcoreapp3.1/publish
  if [ ! -d $DIR ]; then
    echo "Missing binaries. Building.."
    ./scripts/build.sh
    break
  fi
done

cd pulumi/test
dotnet test

cd ../infra
pulumi login $PULUMI_LOGIN
export PULUMI_CONFIG_PASSPHRASE=$PULUMI_CONFIG_PASSPHRASE

# check if stack exist
if [ ! -f Pulumi.$STACK_NAME.yaml ]; then
  echo Stack $STACK_NAME does not exist, creating..
  pulumi stack init $STACK_NAME
  pulumi config set aws:region $PULUMI_AWS_REGION
fi

pulumi stack select $STACK_NAME
if $ONLY_PREVIEW == true; then
  pulumi preview
else
  pulumi up -y
fi
