supernet=10.44.0.0
cider=16
subnetCider=27
username=admin
password=YOUR_PASSWORD
host=vmorionipam-df0f29270f.northeurope.cloudapp.azure.com

# Make sure Applicatiom was Published
# Build => Publish to Folder => "Choose Folder" => bin/Release/net6.0/publish

folder=bin/Release/net6.0/publish

# Execute 
../SubnetCalculator/$folder/SubnetCalculator $supernet $cider $subnetCider $username $password $host | grep rows | python -m json.tool