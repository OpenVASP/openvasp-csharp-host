pipeline {
  agent any
  stages {
    stage('Dotnet Restore') {
      steps {
        sh 'dotnet restore src/${ServiceName}/${ServiceName}.csproj'
      }
    }

    stage('Dotnet Build') {
      steps {
        sh 'dotnet build --configuration Release --no-restore src/${ServiceName}/${ServiceName}.csproj'
      }
    }

    stage('Dotnet Publish') {
      steps {
        sh 'dotnet publish src/${ServiceName}/${ServiceName}.csproj --configuration Release --output service --no-restore'
      }
    }

    stage('Docker Build') {
      steps {
        sh '''
        docker build --tag openvaspenterprise/${DockerName}:0.${BUILD_ID} service
        docker tag openvaspenterprise/${DockerName}:${BUILD_ID} openvaspenterprise/${DockerName}:latest
        docker login -u=$REGISTRY_AUTH_USR -p=$REGISTRY_AUTH_PSW
        docker push openvaspenterprise/${DockerName}:0.${BUILD_ID}
        docker push openvaspenterprise/${DockerName}:latest
'''
      }
    }

    stage('Kubernetes Deployment') {
      parallel {
        stage('Kubernetes Deployment') {
          steps {
            sh 'kubectl --kubeconfig=/kube/dev get nodes'
          }
        }

        stage('Ingress Check') {
          steps {
            sh 'echo "test"'
          }
        }

      }
    }

  }
  environment {
    RepoName = 'openvasp-csharp-host'
    ServiceName = 'OpenVASP.Host'
    DockerName = 'host'
    REGISTRY_AUTH = credentials('dockerhub')
  }
}