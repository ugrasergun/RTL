using NUnit.Framework;
using RTL.API.Models.Parsers;
using System.IO;
using System.Linq;

namespace RTL.Test
{
    [TestFixture]
    public class ParserTests
    {
        ShowParser parser = new ShowParser();

        [Test]
        public void Should_Parse_Correctly()
        {
            var jsonData = File.ReadAllText(@".\TestCases\Parser\Should_Parse_Correctly.json");
            var show = parser.ParseShowFromJSON(jsonData);

            Assert.IsNotNull(show);
            Assert.AreEqual(1, show.ShowId);
            Assert.AreEqual("Under the Dome", show.ShowName);
            Assert.AreEqual(15, show.Cast.Count);
            Assert.True(show.Cast.Any(c => c.ActorId == 1));
            Assert.AreEqual("Mackenzie Lintz", show.Cast[0].ActorName);
        }

        [Test]
        public void Should_Parse_Correctly_With_Null_Birthday()
        {
            var jsonData = File.ReadAllText(@".\TestCases\Parser\Should_Parse_Correctly_With_Null_Birthday.json");
            var show = parser.ParseShowFromJSON(jsonData);

            Assert.IsNotNull(show);
            Assert.AreEqual(1, show.ShowId);
            Assert.AreEqual("Under the Dome", show.ShowName);
            Assert.AreEqual(15, show.Cast.Count);
            Assert.True(show.Cast.Any(c => c.ActorId == 1));
            Assert.AreEqual("Mike Vogel", show.Cast.Where(c => c.ActorId == 1).FirstOrDefault().ActorName);
            Assert.IsNull(show.Cast.Where(c => c.ActorId == 1).FirstOrDefault().BirthDay);
        }

        [Test]
        public void Should_Parse_Correctly_With_No_Cast()
        {
            var jsonData = File.ReadAllText(@".\TestCases\Parser\Should_Parse_Correctly_With_No_Cast.json");
            var show = parser.ParseShowFromJSON(jsonData);

            Assert.IsNotNull(show);
            Assert.AreEqual(1, show.ShowId);
            Assert.AreEqual("Under the Dome", show.ShowName);
            Assert.IsNull(show.Cast);
        }
    }
}
