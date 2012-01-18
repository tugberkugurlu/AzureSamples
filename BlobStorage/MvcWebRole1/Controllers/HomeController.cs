using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.ServiceRuntime;
using MvcWebRole1.Services;

namespace MvcWebRole1.Controllers {

    public class HomeController : Controller {

        public MyBlobStorageService 
            _myBlobStorageService = new MyBlobStorageService();

        public ActionResult Index() {

            // Retrieve a reference to a container 
            CloudBlobContainer blobContainer = 
                _myBlobStorageService.GetCloudBlobContainer();

            List<string> blobs = new List<string>();

            // Loop over blobs within the container and output the URI to each of them
            foreach (var blobItem in blobContainer.ListBlobs())
                blobs.Add(blobItem.Uri.ToString());

            return View(blobs);
        }

        [HttpPost]
        [ActionName("index")]
        public ActionResult Index_post(HttpPostedFileBase fileBase) {

            if (fileBase.ContentLength > 0) {

                // Retrieve a reference to a container 
                CloudBlobContainer blobContainer = 
                    _myBlobStorageService.GetCloudBlobContainer();

                CloudBlob blob = 
                    blobContainer.GetBlobReference(fileBase.FileName);
                
                // Create or overwrite the "myblob" blob with contents from a local file
                blob.UploadFromStream(fileBase.InputStream);

            }

            return RedirectToAction("index");
        }   
    }
}
