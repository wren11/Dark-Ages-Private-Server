///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using System.IO;
using System.Xml.Serialization;

namespace Darkages.Compression
{
    public class CompressableObject
    {
        [XmlIgnore] public byte[] DeflatedData { get; set; }

        [XmlIgnore] public byte[] InflatedData { get; set; }

        [XmlIgnore] public string Filename { get; set; }

        public static T Load<T>(string filename, bool deflated = true)
            where T : CompressableObject, new()
        {
            var result = new T();

            if (deflated)
            {
                result.DeflatedData = File.ReadAllBytes(filename);
                result.Decompress();
            }
            else
            {
                result.InflatedData = File.ReadAllBytes(filename);
                result.Compress();
            }

            result.Filename = filename;

            using (var stream = new MemoryStream(result.InflatedData))
            {
                result.Load(stream);
            }

            return result;
        }

        public static void Save(string filename, CompressableObject obj)
        {
            using (var stream = new MemoryStream())
            {
                obj.Save(stream);
                obj.InflatedData = stream.ToArray();
            }

            obj.Compress();

            File.WriteAllBytes(filename + ".deflated", obj.DeflatedData);
            File.WriteAllBytes(filename + ".inflated", obj.InflatedData);
        }

        public void Compress()
        {
            DeflatedData = CompressionProvider.Deflate(InflatedData);
        }

        public void Decompress()
        {
            InflatedData = CompressionProvider.Inflate(DeflatedData);
        }

        public virtual void Load(MemoryStream stream)
        {
        }

        public virtual void Save(MemoryStream stream)
        {
        }
    }
}
