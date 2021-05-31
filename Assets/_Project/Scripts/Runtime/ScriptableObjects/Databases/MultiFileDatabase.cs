/* MIT License

 * Copyright (c) 2020 Skurdt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

using System.IO;
using UnityEngine;

namespace Arcade
{
    public abstract class MultiFileDatabase<T> : Database<T>
        where T : DatabaseEntry
    {
        public override bool Save(T item)
        {
            if (item is null || string.IsNullOrEmpty(item.Id))
            {
                Debug.LogWarning($"[{GetType().Name}] Failed to save configuration, data is null or invalid");
                return false;
            }

            string filePath = Path.Combine(Directory, $"{item.Id}.xml");
            return Serialize(filePath, item);
        }

        public sealed override bool SaveAll()
        {
            try
            {
                foreach (T entry in _entries.Values)
                    _ = Save(entry);

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        protected override void PostInitialize()
        {
        }

        protected sealed override void PostAdd(T item)
        {
            if (item is null || string.IsNullOrEmpty(item.Id))
            {
                Debug.LogWarning($"[{GetType().Name}] Entry is null or invalid");
                return;
            }

            try
            {
                string filePath = Path.Combine(Directory, $"{item.Id}.xml");
                _ = Serialize(filePath, item);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected sealed override void PostDelete(string id)
        {
            string filePath = Path.Combine(Directory, $"{id}.xml");
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        protected sealed override bool LoadAllFromDisk()
        {
            try
            {
                string[] filePaths = System.IO.Directory.GetFiles(Directory, "*.xml", SearchOption.TopDirectoryOnly);
                foreach (string filePath in filePaths)
                {
                    T entry = Deserialize<T>(filePath);
                    if (entry is null)
                        continue;
                    entry.Id = Path.GetFileNameWithoutExtension(filePath);
                    _entries.Add(entry.Id, entry);
                }

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        protected sealed override void DeleteAllFromDisk()
        {
            if (System.IO.Directory.Exists(Directory))
            {
                System.IO.Directory.Delete(Directory, true);
                _ = System.IO.Directory.CreateDirectory(Directory);
            }
        }
    }
}
