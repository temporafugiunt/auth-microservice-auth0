@Library('freebyTech')_

import com.freebyTech.BuildInfo

String versionPrefix = '1.0'
String repository = 'k8s-tests'    
String imageName = 'auth-microservice-auth0'
String dockerBuildArguments = ''
BuildInfo buildInfo

node 
{
  buildInfo = build(this, versionPrefix, repository, imageName, dockerBuildArguments, true, true)

  getApproval(imageName, 'None\ndev')

  if(!'None'.equalsIgnoreCase(env.NAMESPACE)) 
  {
    deploy(buildInfo, repository, imageName, true)
  }
}

