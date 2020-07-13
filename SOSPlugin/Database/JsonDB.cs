using System;
using System.IO;
using Newtonsoft.Json;

namespace SOSPlugin.DB
{
    public class JsonFiler
    {
        public static void SaveDataJson<TypeValue>(string _path, TypeValue _data)
        {
            try
            {
                // Open File
                FileStream fs = new FileStream(_path, FileMode.Create);
                StreamWriter stream = new StreamWriter(fs);

                // Create or Save File
                string jsonData = JsonConvert.SerializeObject(_data, Formatting.Indented);
                stream.Write(jsonData);

                // Close File
                stream.Close();
                fs.Close();

                Console.WriteLine("Data has been successfully saved.");
            }
            catch
            {
                Console.WriteLine("Cannot save data. Something is Wrong.");
            }
        }

        public static TypeValue LoadDataJson<TypeValue>(string _path)
        {
            try
            {
                FileStream fs;
                StreamReader stream;

                // Open File
                if (File.Exists(_path))
                {
                    fs = new FileStream(_path, FileMode.Open);
                    stream = new StreamReader(fs);
                }
                else
                {
                    fs = new FileStream(_path, FileMode.Create);
                    stream = new StreamReader(fs);
                }

                // Load File
                TypeValue _data = JsonConvert.DeserializeObject<TypeValue>(stream.ReadToEnd());

                // Close File
                stream.Close();
                fs.Close();

                return _data;
            }
            catch
            {
                Console.WriteLine("Cannot load file.");
                return default;
            }
        }
    }
}