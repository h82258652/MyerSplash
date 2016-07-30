using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyerSplashShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject.APITest
{
    [TestClass]
    public class TestCategory
    {
        [TestMethod]
        public async Task TestGetCagetories()
        {
            var result = await CloudService.GetCategories(CTSFactory.MakeCTS().Token);
            Assert.IsTrue(result.IsRequestSuccessful);
        }
    }
}
