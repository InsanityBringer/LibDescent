using System.IO;

namespace LibDescent.Tests
{
    static class TestUtils
    {
        public static Stream GetResourceStream(string resourceName)
        {
            return typeof(TestUtils).Assembly.GetManifestResourceStream(typeof(TestUtils), resourceName);
        }

        public static byte[] GetArrayFromResourceStream(string resourceName)
        {
            var resourceStream = GetResourceStream(resourceName);
            var memoryStream = new MemoryStream();
            resourceStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
