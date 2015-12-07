using System;

namespace AndBurn.DDSReader
{
    /// <summary>
    /// Thrown when there is an unknown compressor used in the DDS file.
    /// </summary>
    public class UnknownFileFormatException : Exception
    {
    }

    /// <summary>
    /// Thrown when an invalid file header has been encountered.
    /// </summary>
    public class InvalidFileHeaderException : Exception
    {
    }

    /// <summary>
    /// Thrown when the data does not contain a DDS image.
    /// </summary>
    public class NotADDSImageException : Exception
    {
    }
}