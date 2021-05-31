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

using UnityEditor;

namespace Arcade.UnityEditor
{
    internal sealed class DatabaseEditorWindowContext<T> : SK.Utilities.StateMachine.Context<DatabaseEditorWindowState<T>>
        where T : DatabaseEntry
    {
        public readonly IDatabaseEditorWindow<T> DatabaseEditorWindow;

        public T[] Entries { get; private set; }
        public SerializedObject TempCfgObject { get; private set; }
        public SerializedProperty TempValueProperty { get; private set; }

        public DatabaseEditorWindowContext(IDatabaseEditorWindow<T> databaseEditorWindow)
        {
            DatabaseEditorWindow = databaseEditorWindow;
            TransitionTo<DatabaseEditorWindowNormalState<T>>();
        }

        public void RefreshDatabase()
        {
            ClearSerializedFields();
            if (DatabaseEditorWindow.Database.LoadAll())
                RefreshEntriesArray();
        }

        public void RefreshEntriesArray()
        {
            ClearSerializedFields();
            Entries = DatabaseEditorWindow.Database.Values;
        }

        public bool AddEntry(T entry) => !(DatabaseEditorWindow.Database.Add(entry) is null);

        public bool SaveEntry(T entry) => DatabaseEditorWindow.Database.Save(entry);

        public void DeleteEntry(T entry)
        {
            if (EditorUtility.DisplayDialog($"Delete {typeof(T).Name}", $"Delete '{entry}' configuration?", "Yes", "No"))
                if (DatabaseEditorWindow.Database.Delete(entry.Id))
                    RefreshEntriesArray();
        }

        public void SetSerializedFields(T entry)
        {
            ClearSerializedFields();

            TempCfgObject     = DatabaseEditorWindow.GetSerializedObject(entry);
            TempValueProperty = TempCfgObject.FindProperty("Value");
        }

        public void ClearSerializedFields()
        {
            TempValueProperty?.Dispose();
            TempValueProperty = null;

            TempCfgObject?.Dispose();
            TempCfgObject = null;
        }
    }
}
