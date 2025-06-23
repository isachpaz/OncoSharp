// // RTToolkitSharp
// // Copyright (c) 2014 - 2025 Medical Innovation and Technology P.C.
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/RTToolkitSharp for more information.

using NUnit.Framework;
using RTToolkitSharp.RTQuantities.Quantities.Volume;


namespace RTQuantitiesLibTests
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