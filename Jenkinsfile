pipeline {
  agent any
  stages {
    stage('Dotnet Build') {
      steps {
        sh 'dotnet build --configuration Release src/${ServiceName}/${ServiceName}.csproj'
      }
    }

    stage('Dotnet Publish') {
      steps {
        sh 'dotnet publish src/${ServiceName}/${ServiceName}.csproj --configuration Release --output ./docker/service --no-restore'
      }
    }

    stage('Docker Build') {
      steps {
        sh '''
        docker build --tag openvasporg/${DockerName}:0.${BUILD_ID} ./docker/service
        docker tag openvasporg/${DockerName}:0.${BUILD_ID} openvasporg/${DockerName}:latest
        docker login -u=$REGISTRY_AUTH_USR -p=$REGISTRY_AUTH_PSW
        docker push openvasporg/${DockerName}:0.${BUILD_ID}
        docker push openvasporg/${DockerName}:latest'''
      }
    }

    stage('Prepare Yamls') {
      parallel {
        stage('Namespace Check') {
          steps {
            sh 'kubectl --kubeconfig=/kube/dev get nodes'
          }
        }

        stage('Ingress Check') {
          steps {
            sh 'echo "test"'
          }
        }

        stage('Substitute Yamls') {
          steps {
            sh 'echo "Test"'
          }
        }

      }
    }

  }
  environment {
    RepoName = 'openvasp-csharp-host'
    ServiceName = 'OpenVASP.Host'
    DockerName = 'csharp-host'
    REGISTRY_AUTH = credentials('dockerhub')
  }
}