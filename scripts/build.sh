cd "$(dirname "$0")"
cd ../src
find . -maxdepth 1 -type d \( ! -name . \) -exec bash -c "cd '{}' && dotnet publish -c Release" \;