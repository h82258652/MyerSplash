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
    public class TestSearch
    {
        [TestMethod]
        public async Task TestSearchCase1()
        {
            var result = await CloudService.SearchImages(1, 10, "cat", CTSFactory.MakeCTS(20000).Token);
            Assert.IsTrue(result.IsRequestSuccessful);
        }
    }
}
