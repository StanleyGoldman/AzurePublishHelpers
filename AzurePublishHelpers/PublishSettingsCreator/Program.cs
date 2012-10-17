using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AzurePublishHelpers.PublishSettingsCreator
{
    class Program
    {
        private const StoreLocation certificateStoreLocation = StoreLocation.CurrentUser;
        private const StoreName certificateStoreName = StoreName.My;
        private const string publishFileFormat = @"<?xml version=""1.0"" encoding=""utf-8""?>
<PublishData>
  <PublishProfile
    PublishMethod=""AzureServiceManagementAPI""
    Url=""https://management.core.windows.net/""
    ManagementCertificate=""{0}"">
    <Subscription
      Id=""{1}""
      Name=""{2}"" />
  </PublishProfile>
</PublishData>";

        static void Main(string[] args)
        {
            var subscriptionName = args[0];
            var subscriptionId = args[1];
            var certificateThumbprint = args[2];
            
            var certificateStore = new X509Store(certificateStoreName, certificateStoreLocation);
            certificateStore.Open(OpenFlags.ReadOnly);
            
            var certificates = certificateStore.Certificates;
            var matchingCertificates = certificates.Find(X509FindType.FindByThumbprint, certificateThumbprint, false);
            if (matchingCertificates.Count == 0)
            {
                Console.WriteLine("No matching certificate found. Please ensure that proper values are specified for Certificate Store Name, Location and Thumbprint");
            }
            else
            {
                var certificate = matchingCertificates[0];
                var certificateData = Convert.ToBase64String(certificate.Export(X509ContentType.Pkcs12, string.Empty));
                
                if (string.IsNullOrWhiteSpace(subscriptionName))
                {
                    subscriptionName = subscriptionId;
                }
                
                var publishSettingsFileData = string.Format(publishFileFormat, certificateData, subscriptionId, subscriptionName);
                var fileName = string.Format(@"{0}\{1}.publishsettings", Environment.CurrentDirectory, subscriptionId);
                
                File.WriteAllBytes(fileName, Encoding.UTF8.GetBytes(publishSettingsFileData));
                Console.WriteLine("Publish settings file written successfully at: " + fileName);
            }
            
            Console.WriteLine("Press any key to terminate the program.");
            Console.ReadLine();
        }
    }
}
