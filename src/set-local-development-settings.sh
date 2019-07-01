. ./.secrets/set-external-resources-environment-variables.sh

export AUTH0__DOMAIN=$AUTH0_DOMAIN
export AUTH0__CLIENTID=$AUTH0_CLIENTID
export AUTH0__CLIENTSECRET=$AUTH0_CLIENTSECRET

export SANDBOXAPP__ENVIRONMENTNAME=localdev
export SANDBOXAPP__EXTERNALDNSNAME=http://localhost:5000

dotnet run