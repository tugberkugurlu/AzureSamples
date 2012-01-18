using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace MvcWebRole1.Services {

    public class MyBlobStorageService {

        public CloudBlobContainer GetCloudBlobContainer() {

            // Retrieve storage account from connection-string
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString")
                );

            // Create the blob client 
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container 
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("albums");

            // Create the container if it doesn't already exist
            if (blobContainer.CreateIfNotExist()) {

                blobContainer.SetPermissions(
                   new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob }
                );
            }

            return blobContainer;
        }

    }
}