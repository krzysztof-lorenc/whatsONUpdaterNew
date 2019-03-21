#!groovy​
@Library('SoloPipeline')
import com.soloplan.*

pipeline {
  agent {
    label 'net-framework-sdk'
  } 

  stages {
    stage('Agent info') {
      steps {
        stepAgentInfo()
      }
    }

    stage('Build') {
      steps {
        stepMSBuild(project: 'src/Soloplan.WhatsON.sln')
      }
    }

    stage('Publish') {
      when {
        branch 'master'
      }
      
      steps {
        stepPublishArtifacts(bucket: "whatson")
      }
    }
  }

  post {
    success {
      stepArchiveArtifacts()
    }
  }
}