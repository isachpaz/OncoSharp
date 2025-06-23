// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using NUnit.Framework;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.Core.Tests
{
    [TestFixture]
    public class VolumeTests
    {
        [Test]
        public void Volume_Empty_Check_Equality_Test()
        {
            var vol1 = VolumeValue.Empty();
            var vol2 = VolumeValue.Empty();

            Assert.That(vol1, Is.EqualTo(vol2));
        }
    }
}