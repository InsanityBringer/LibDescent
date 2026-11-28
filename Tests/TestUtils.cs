using NUnit.Framework;
using System;
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

        public static string D1PigPath => TestContext.Parameters.Get("D1PigPath", Environment.GetEnvironmentVariable("LIBDESCENT_TESTS_D1_PIG"));
        public static string RequireD1Pig()
        {
            string path = D1PigPath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Assert.Ignore("Descent 1 PIG file not found. Please set the 'D1PigPath' test parameter or the " +
                    "'LIBDESCENT_TESTS_D1_PIG' environment variable to the path of a valid Descent 1 PIG file.");
            }
            return path;
        }

        public static string D1OutputPigPath => TestContext.Parameters.Get("D1OutputPigPath", Environment.GetEnvironmentVariable("LIBDESCENT_TESTS_D1_OUTPUT_PIG"));
        public static string RequireD1OutputPig()
        {
            string path = D1OutputPigPath;
            if (string.IsNullOrEmpty(path))
            {
                Assert.Ignore("Output PIG file not specified. Please set the 'D1OutputPigPath' test parameter or the " +
                    "'LIBDESCENT_TESTS_D1_OUTPUT_PIG' environment variable to the file path you want to write to.");
            }
            return path;
        }

        public static string HogPath => TestContext.Parameters.Get("HogPath", Environment.GetEnvironmentVariable("LIBDESCENT_TESTS_HOG"));
        public static string RequireHog()
        {
            string path = HogPath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Assert.Ignore("HOG file not found. Please set the 'HogPath' test parameter or the " +
                    "'LIBDESCENT_TESTS_HOG' environment variable to the path of a valid HOG file.");
            }
            return path;
        }
    }
}
