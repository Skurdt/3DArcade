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

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Arcade
{
    [CreateAssetMenu(menuName = "3DArcade/ModelSpawner", fileName = "ModelSpawner")]
    public sealed class ModelSpawner : ModelSpawnerBase
    {
        protected override async UniTask<GameObject> SpawnAsync(AssetAddresses addressesToTry, Vector3 position, Quaternion orientation, Transform parent)
        {
            IList<IResourceLocation> resourceLocations;
            try
            {
                resourceLocations = await Addressables.LoadResourceLocationsAsync(addressesToTry, Addressables.MergeMode.UseFirst, typeof(GameObject));
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return null;
            }

            if (resourceLocations.Count == 0)
                return null;

            return await Addressables.InstantiateAsync(resourceLocations[0], position, orientation, parent);
        }
    }
}
