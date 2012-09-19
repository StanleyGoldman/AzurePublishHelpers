using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using AzurePublishHelpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePublishHelpersTest
{
    [TestClass]
    public class AzurePubXmlHelperTest
    {
        [TestMethod]
        public void GetPublishProfileTest()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "AzureSDK17Profile.pubxml");
            var publishProfile = new AzurePubXmlHelper().GetPublishProfile(filePath);

            publishProfile.Should().NotBeNull();
            publishProfile.AppendTimestampToDeploymentLabel.Should().Be(true);
            publishProfile.AzureDeleteDeploymentOnFailure.Should().Be(false);
            publishProfile.AzureDeploymentReplacementMethod.Should().Be("AutomaticUpgrade");
            publishProfile.AzureFallbackToDeleteAndRecreateIfUpgradeFails.Should().Be(false);
            publishProfile.ConnectionName.Should().Be("MySubscription");
            publishProfile.DeploymentLabel.Should().Be("MyDeploymentLabel");
            publishProfile.DeploymentSlot.Should().Be("Production");
            publishProfile.EnableIntelliTrace.Should().Be(false);
            publishProfile.EnableProfiling.Should().Be(false);
            publishProfile.EnableRemoteDesktop.Should().Be(true);
            publishProfile.EnableWebDeploy.Should().Be(true);
            publishProfile.HostedServiceLabel.Should().Be("MyServiceLabel");
            publishProfile.HostedServiceName.Should().Be("MyService");
            publishProfile.ServiceConfiguration.Should().Be("Cloud");
            publishProfile.StorageAccountLabel.Should().Be("MyStorageAccountLabel");
            publishProfile.StorageAccountName.Should().Be("MyStorageAccountName");
        }
    }
}
