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

using SK.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcade
{
    public abstract class Database<T> : ScriptableObject, IEnumerable<T>
        where T : DatabaseEntry
    {
        [SerializeField] private VirtualFileSystem _virtualFileSystem;

        public string[] Names => _entries.Keys.ToArray();
        public T[] Values => _entries.Values.ToArray();

        protected abstract string DirectoryAlias { get; }

        [field: System.NonSerialized] protected string Directory { get; private set; }

        protected readonly SortedList<string, T> _entries = new SortedList<string, T>();

        public void Initialize()
        {
            if (!_virtualFileSystem.TryGetDirectory(DirectoryAlias, out string directory))
            {
                Debug.LogWarning($"[{GetType().Name}.Initialize] Directory not mapped in VirtualFileSystem, using default values");
                return;
            }

            Directory = directory;

            PostInitialize();
        }

        public bool Contains(string name) => _entries.ContainsKey(name);

        public T Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[{GetType().Name}.Get] Passed null for configuration ID");
                return null;
            }

            if (!_entries.ContainsKey(id))
            {
                Debug.LogWarning($"[{GetType().Name}.Get] Configuration not found: {id}");
                return null;
            }

            return _entries[id];
        }

        public bool TryGet(string id, out T outResult)
        {
            outResult = !string.IsNullOrEmpty(id) ? Get(id) : null;
            return !(outResult is null);
        }

        public T Add(T entry)
        {
            if (entry is null)
            {
                Debug.LogWarning($"[{GetType().Name}.Add] Passed null entry");
                return null;
            }

            if (Contains(entry.Id))
            {
                Debug.LogWarning($"[{GetType().Name}.Add] Entry already exists: {entry.Id}");
                return null;
            }

            _entries.Add(entry.Id, entry);

            PostAdd(entry);

            return entry;
        }

        public abstract bool Save(T item);

        public bool Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[{GetType().Name}.Delete] Passed null or empty entry ID");
                return false;
            }

            if (!Contains(id))
            {
                Debug.LogWarning($"[{GetType().Name}.Delete] Entry not found: {id}");
                return false;
            }

            if (!_entries.Remove(id))
            {
                Debug.LogWarning($"[{GetType().Name}.Delete] Dictionary error");
                return false;
            }

            PostDelete(id);

            return true;
        }

        public void DeleteAll()
        {
            DeleteAllFromDisk();
            _entries.Clear();
        }

        public bool LoadAll()
        {
            if (!System.IO.Directory.Exists(Directory))
            {
                Debug.LogWarning($"[{GetType().Name}.LoadAll] Directory doesn't exists: {Directory}");
                return false;
            }

            _entries.Clear();
            return LoadAllFromDisk();
        }

        public abstract bool SaveAll();

        protected abstract void PostInitialize();

        protected abstract void PostAdd(T entry);

        protected abstract void PostDelete(string id);

        protected abstract bool LoadAllFromDisk();

        protected abstract void DeleteAllFromDisk();

        protected static bool Serialize<U>(string filePath, U obj) where U : class => XMLUtils.Serialize(filePath, obj);

        protected static U Deserialize<U>(string filePath) where U : class => XMLUtils.Deserialize<U>(filePath);

        public T this[int index]
        {
            get => _entries.Values[index];
            private set => _entries.Values[index] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T entry in _entries.Values)
                yield return entry;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
