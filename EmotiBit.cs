namespace eDIPEmotiBit
{
    internal class EmotiBit
    {
        private const bool DEBUG = true;
        private const int LOW_BATTERY_LEVEL = 7;

        internal class TagList
        {
            internal readonly string Name;
            internal readonly string[] Tags;
            internal string? Path;

            internal TagList(string name, string[] tags)
            {
                Name = name;
                Tags = tags;
            }
        }

        // List of tags: https://github.com/EmotiBit/EmotiBit_XPlat_Utils/blob/8f22129740f9ea37c476b05e8007cea91755d6e2/src/EmotiBitPacket.cpp
        private readonly TagList[] mTagLists = [
            new(
                "Biometric",
                ["EA", "EL", "ER", "PI", "PR", "PG", "O2", "T0", "T1", "TH", "H0", "AX", "AY", "AZ", "GX", "GY", "GZ", "MX", "MY", "MZ", "HR", "BI", "SA", "SF", "SR"]
            ),
            new(
                "General",
                ["BV", "B%", "DC", "DO", "AK", "NK", "EM", "EI", "RD", "PN", "PO", "RS"]
            ),
            new(
                "Computer",
                ["GL", "GS", "GB", "GA", "TL", "TU", "TX", "LM", "RB", "RE", "UN", "MH", "HE"]
            ),
            new(
                "Unknown",
                []
            ),
        ];

        public event Action? OnBatteryLow;
        private int mBatteryLevel = 100;

        public void SetPath(string path)
        {
            var t = Debug.Timestamp();
            path = Path.Combine(
                Directory.GetCurrentDirectory(),
                path
            );

            Directory.CreateDirectory(path);

            foreach (var tagList in mTagLists)
            {
                tagList.Path = GetSafeFileName(Path.Combine(
                    path,
                    $"emotibit_{t}_{tagList.Name.ToLower()}_data.csv"
                ));

                if (!IsValidPath(tagList.Path))
                {
                    throw new Exception();
                }
            }
        }

        internal async Task OnData(string data)
        {
            if (DEBUG)
            {
                Debug.Log(data);
            }

            try
            {
                var fields = data.Split(',');
                if (data.Contains("B%"))
                {
                    if (fields.Length == 7)
                    {
                        if (int.TryParse(fields[6], out mBatteryLevel))
                        {
                            if (mBatteryLevel < LOW_BATTERY_LEVEL)
                            {
                                Debug.Log("Emotibit Battery Low!");
                                OnBatteryLow?.Invoke();
                            }
                        }
                    }
                }

                var writes = 0;
                foreach (var tagList in mTagLists)
                {
                    if (fields.Take(4).Any(_ => tagList.Tags.Contains(_)))
                    {
                        writes += 1;
                        await Write(tagList.Path!, data);
                    }
                }

                if (writes == 0)
                {
                    await Write(mTagLists.Last().Path!, data);
                }
                else if (writes > 1)
                {
                    Debug.Log($"Writes {writes} {data}");
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error: {ex.Message}");
                throw;
            }
        }

        private async Task Write(string path, string data)
        {
            try
            {
                await File.AppendAllLinesAsync(path, [$"{Debug.Timestamp()},{data}"]);
            }
            catch (IOException)
            {
                Debug.Log($"Cannot access {path}");
            }
        }

        private static string GetSafeFileName(string candidate)
        {
            var finalPath = candidate;

            var c = 1;
            while (File.Exists(finalPath))
            {
                var fileName = Path.GetFileNameWithoutExtension(finalPath);
                if (fileName.Contains('@'))
                {
                    // Remove index.
                    fileName = fileName.Split('@')[0];
                }

                fileName = $"{fileName}{"@" + c.ToString()}{Path.GetExtension(finalPath)}";
                finalPath = Path.Combine(Path.GetDirectoryName(finalPath)!, fileName);

                c += 1;
                if (c > 100) break;
            }

            return finalPath;
        }

        // Validates the given path to ensure it is a usable file path.
        public static bool IsValidPath(string path) =>
            !string.IsNullOrEmpty(path)
            && path.Length > 2
            && path.IndexOfAny(Path.GetInvalidPathChars()) < 0;
    }
}
