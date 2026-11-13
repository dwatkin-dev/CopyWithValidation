namespace CopyWithValidation.Core;

public static class ByteSize
{
    public static string Format(long bytes)
    {
        string[] units = ["bytes", "KB", "MB", "GB", "TB"];
        double size = bytes;
        int index = 0;

        while (size >= 1024)
        {
            size /= 1024;
            index++;
        }

        return $"{size:0.#} {units[index]}";
    }
}
