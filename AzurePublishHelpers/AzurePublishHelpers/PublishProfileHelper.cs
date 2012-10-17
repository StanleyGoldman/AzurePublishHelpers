using System.IO;
using System.Xml.Linq;

namespace AzurePublishHelpers.PublishProfileReader
{
    public class PublishProfileHelper
    {
        public static PublishProfile GetPublishProfile(string xmlFile)
        {
            if (!File.Exists(xmlFile))
            {
                return null;
            }

            var doc = XDocument.Load(xmlFile);
            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            var props = doc.Element(ns + "Project").Element(ns + "PropertyGroup");

            var publishProfile = new PublishProfile();
            publishProfile.ConnectionName = props.Element(ns + "AzureCredentials").Value;
            publishProfile.HostedServiceName = props.Element(ns + "AzureHostedServiceName").Value;
            publishProfile.HostedServiceLabel = props.Element(ns + "AzureHostedServiceLabel").Value;
            publishProfile.DeploymentSlot = props.Element(ns + "AzureSlot").Value;
            publishProfile.EnableIntelliTrace = props.Element(ns + "AzureEnableIntelliTrace").Value == "True" ? true : false;
            publishProfile.EnableProfiling = props.Element(ns + "AzureEnableProfiling").Value == "True" ? true : false;
            publishProfile.EnableWebDeploy = props.Element(ns + "AzureEnableWebDeploy").Value == "True" ? true : false;
            publishProfile.StorageAccountName = props.Element(ns + "AzureStorageAccountName").Value;
            publishProfile.StorageAccountLabel = props.Element(ns + "AzureStorageAccountLabel").Value;
            publishProfile.DeploymentLabel = props.Element(ns + "AzureDeploymentLabel").Value;
            publishProfile.SolutionConfiguration = props.Element(ns + "AzureSolutionConfiguration").Value;
            publishProfile.ServiceConfiguration = props.Element(ns + "AzureServiceConfiguration").Value;
            publishProfile.AppendTimestampToDeploymentLabel = props.Element(ns + "AzureAppendTimestampToDeploymentLabel").Value == "True" ? true : false;
            publishProfile.AzureDeploymentReplacementMethod = props.Element(ns + "AzureDeploymentReplacementMethod").Value;
            publishProfile.AzureDeleteDeploymentOnFailure = props.Element(ns + "AzureDeleteDeploymentOnFailure").Value == "True" ? true : false;
            publishProfile.AzureFallbackToDeleteAndRecreateIfUpgradeFails = props.Element(ns + "AzureFallbackToDeleteAndRecreateIfUpgradeFails").Value == "True" ? true : false;
            publishProfile.EnableRemoteDesktop = props.Element(ns + "AzureEnableRemoteDesktop").Value == "True" ? true : false;

            return publishProfile;
        }
    }
}
