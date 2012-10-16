using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PublishSettingsCreator
{
    class Program
    {
        private static string subscriptionId = "ecd7cc1d-12ac-4cf6-a90b-0cf14db36020";
        private static string subscriptionName = "JustAProgrammer Azure";
        private static string certificateThumbprint = "2AC582102355D87142E399518AFBE75F4B7B3D74";
        private static StoreLocation certificateStoreLocation = StoreLocation.CurrentUser;
        private static StoreName certificateStoreName = StoreName.My;
        private static string publishFileFormat = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
            X509Store certificateStore = new X509Store(certificateStoreName, certificateStoreLocation);
            certificateStore.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certificates = certificateStore.Certificates;
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
                string publishSettingsFileData = string.Format(publishFileFormat, certificateData, subscriptionId, subscriptionName);
                string fileName = Path.GetTempPath() + subscriptionId + ".publishsettings";
                File.WriteAllBytes(fileName, Encoding.UTF8.GetBytes(publishSettingsFileData));
                Console.WriteLine("Publish settings file written successfully at: " + fileName);
            }
            Console.WriteLine("Press any key to terminate the program.");
            Console.ReadLine();
        }
    }

}
