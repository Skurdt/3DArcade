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

using UnityEngine;

namespace Arcade
{
    public abstract class CylArcadeController : ArcadeController
    {
        public sealed override float AudioMinDistance { get; protected set; }
        public sealed override float AudioMaxDistance { get; protected set; }
        public sealed override AnimationCurve VolumeCurve { get; protected set; }
        public sealed override CameraSettings CameraSettings => _arcadeContext.ArcadeConfiguration.Value.CylArcadeProperties.CameraSettings;
        public sealed override RenderSettings RenderSettings => _arcadeContext.ArcadeConfiguration.Value.CylArcadeProperties.RenderSettings;
        public sealed override bool GameModelsSpawnAtPositionWithRotation => false;

        //protected abstract Transform TransformAnchor { get; }
        //protected abstract Vector3 TransformVector { get; }

        //protected sealed override PlayerControls PlayerControls => _main.PlayerCylControls;

        //protected CylArcadeProperties _cylArcadeProperties;

        //protected int _sprockets;
        //protected int _selectionIndex;
        //protected Vector3 _centerTargetPosition;

        //protected Transform _targetSelection;

        public CylArcadeController(ArcadeContext arcadeContext)
        : base(arcadeContext)
        {
            AudioMinDistance = 0f;
            AudioMaxDistance = 50f;

            VolumeCurve = new AnimationCurve(new Keyframe[]
            {
                 new Keyframe(0f, 1f),
                 new Keyframe(1f, 1f)
            });
        }

        protected sealed override void SetupPlayer()
        {
            if (_arcadeContext.GeneralConfiguration.Value.EnableVR)
                _arcadeContext.Player.TransitionTo<PlayerVirtualRealityCylState>();
            else
                _arcadeContext.Player.TransitionTo<PlayerStandardCylState>();

            //_main.PlayerCylControls.gameObject.SetActive(false);
            //_main.PlayerCylControls.gameObject.SetActive(true);
            //_main.PlayerCylControls.MouseLookEnabled = _cylArcadeProperties.MouseLook;
            //_main.PlayerCylControls.SetVerticalLookLimits(-40f, 40f);
            //_main.PlayerCylControls.SetHorizontalLookLimits(0f, 0f);

            //PlayerControls.transform.SetPositionAndRotation(CameraSettings.Position, Quaternion.Euler(0f, CameraSettings.Rotation.y, 0f));

            //PlayerControls.Camera.rect = CameraSettings.ViewportRect;

            //CinemachineNewVirtualCamera vCam = PlayerControls.VirtualCamera;
            //vCam.transform.eulerAngles    = new Vector3(0f, CameraSettings.Rotation.y, 0f);
            //vCam.m_Lens.Orthographic      = CameraSettings.Orthographic;
            //vCam.m_Lens.FieldOfView       = CameraSettings.FieldOfView;
            //vCam.m_Lens.OrthographicSize  = CameraSettings.AspectRatio;
            //vCam.m_Lens.NearClipPlane     = CameraSettings.NearClipPlane;
            //vCam.m_Lens.FarClipPlane      = CameraSettings.FarClipPlane;

            //CinemachineTransposer transposer = vCam.GetCinemachineComponent<CinemachineTransposer>();
            //transposer.m_FollowOffset.y      = CameraSettings.Height;
        }

        //protected abstract float GetSpacing(Transform previousModel, Transform currentModel);

        //protected abstract bool MoveForwardCondition();

        //protected abstract bool MoveBackwardCondition();

        //protected abstract void AdjustWheelForward(float dt);

        //protected abstract void AdjustWheelBackward(float dt);

        //protected abstract void AdjustModelPosition(Transform model, bool forward, float spacing);

        //protected override void PostLoadScene()
        //{
        //    _cylArcadeProperties  = _arcadeConfiguration.CylArcadeProperties ?? throw new System.NullReferenceException(nameof(_arcadeConfiguration.CylArcadeProperties));
        //    _centerTargetPosition = new Vector3(0f, 0f, _cylArcadeProperties.SelectedPositionZ);
        //}

        //protected sealed override void AddModelsToWorldAdditionalLoopStepsForGames(GameObject instantiatedModel)
        //{
        //    if (instantiatedModel.TryGetComponent(out Rigidbody rigidbody))
        //    {
        //        rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        //        rigidbody.interpolation          = RigidbodyInterpolation.None;
        //        rigidbody.useGravity             = false;
        //        rigidbody.isKinematic            = true;
        //    }

        //    instantiatedModel.SetActive(false);
        //}

        //protected sealed override void AddModelsToWorldAdditionalLoopStepsForProps(GameObject instantiatedModel)
        //{
        //    if (Application.isPlaying)
        //        VideoPlayerController.PlayVideo(instantiatedModel);
        //}

        //protected sealed override void LateSetupWorld()
        //{
        //    base.LateSetupWorld();

        //    _sprockets            = Mathf.Clamp(_cylArcadeProperties.Sprockets, 1, _allGames.Count);
        //    int selectedSprocket  = Mathf.Clamp(_cylArcadeProperties.SelectedSprocket - 1, 0, _sprockets);
        //    int halfSprockets     = _sprockets % 2 != 0 ? _sprockets / 2 : _sprockets / 2 - 1;
        //    _selectionIndex       = halfSprockets - selectedSprocket;

        //    if (_cylArcadeProperties.InverseList)
        //    {
        //        _allGames.Reverse();
        //        _allGames.RotateRight(_selectionIndex + 1);
        //    }
        //    else
        //        _allGames.RotateRight(_selectionIndex);

        //    SetupWheel();

        //    if (_selectionIndex >= 0 && _allGames.Count >= _selectionIndex)
        //        CurrentGame = _allGames[_selectionIndex].GetComponent<ModelConfigurationComponent>();
        //    else
        //        CurrentGame = null;
        //}

        //protected sealed override IEnumerator CoNavigateForward(float dt)
        //{
        //    _animating = true;

        //    _targetSelection = _allGames[_selectionIndex + 1];

        //    ParentGamesToAnchor();

        //    while (MoveForwardCondition())
        //    {
        //        AdjustWheelForward(dt);
        //        yield return null;
        //    }

        //    ResetGamesParent();

        //    _allGames.RotateLeft();

        //    UpdateWheel();

        //    _animating = false;
        //}

        //protected sealed override IEnumerator CoNavigateBackward(float dt)
        //{
        //    _animating = true;

        //    _targetSelection = _allGames[_selectionIndex - 1];

        //    ParentGamesToAnchor();

        //    while (MoveBackwardCondition())
        //    {
        //        AdjustWheelBackward(dt);
        //        yield return null;
        //    }

        //    ResetGamesParent();

        //    _allGames.RotateRight();

        //    UpdateWheel();

        //    _animating = false;
        //}

        //protected void SetupWheel()
        //{
        //    if (_allGames.Count < 1)
        //        return;

        //    Transform firstModel = _allGames[_selectionIndex];
        //    firstModel.gameObject.SetActive(true);
        //    firstModel.SetPositionAndRotation(_centerTargetPosition, Quaternion.Euler(_cylArcadeProperties.SprocketRotation));

        //    for (int i = _selectionIndex + 1; i < _sprockets; ++i)
        //    {
        //        Transform previousModel = _allGames[i - 1];

        //        Transform currentModel = _allGames[i];
        //        currentModel.gameObject.SetActive(true);
        //        currentModel.SetPositionAndRotation(previousModel.localPosition, previousModel.localRotation);
        //        float spacing = GetSpacing(previousModel, currentModel);
        //        AdjustModelPosition(currentModel, true, spacing);
        //    }

        //    for (int i = _selectionIndex - 1; i >= 0; --i)
        //    {
        //        Transform previousModel = _allGames[i + 1];

        //        Transform currentModel = _allGames[i];
        //        currentModel.gameObject.SetActive(true);
        //        currentModel.SetPositionAndRotation(previousModel.localPosition, previousModel.localRotation);
        //        float spacing = GetSpacing(previousModel, currentModel);
        //        AdjustModelPosition(currentModel, false, spacing);
        //    }

        //    foreach (Transform model in _allGames.Skip(_sprockets))
        //    {
        //        model.gameObject.SetActive(false);
        //        model.localPosition = Vector3.zero;
        //    }
        //}

        //protected void UpdateWheel()
        //{
        //    if (_allGames.Count < 1)
        //        return;

        //    Transform previousModel = _allGames[_sprockets - 2];
        //    Transform newModel      = _allGames[_sprockets - 1];
        //    newModel.gameObject.SetActive(true);
        //    newModel.SetPositionAndRotation(previousModel.localPosition, previousModel.localRotation);
        //    float spacing = GetSpacing(previousModel, newModel);
        //    AdjustModelPosition(newModel, true, spacing);

        //    previousModel = _allGames[1];
        //    newModel      = _allGames[0];
        //    newModel.gameObject.SetActive(true);
        //    newModel.SetPositionAndRotation(previousModel.localPosition, previousModel.localRotation);
        //    spacing = GetSpacing(previousModel, newModel);
        //    AdjustModelPosition(newModel, false, spacing);

        //    foreach (Transform model in _allGames.Skip(_sprockets))
        //    {
        //        model.gameObject.SetActive(false);
        //        model.localPosition = Vector3.zero;
        //    }

        //    CurrentGame = _allGames[_selectionIndex].GetComponent<ModelConfigurationComponent>();
        //}

        //protected float GetHorizontalSpacing(Transform previousModel, Transform currentModel) => previousModel.GetHalfWidth() + currentModel.GetHalfWidth() + _cylArcadeProperties.ModelSpacing;

        //protected float GetVerticalSpacing(Transform previousModel, Transform currentModel) => previousModel.GetHalfHeight() + currentModel.GetHalfHeight() + _cylArcadeProperties.ModelSpacing;

        //protected void ParentGamesToAnchor()
        //{
        //    foreach (Transform game in _allGames.Take(_sprockets))
        //        game.SetParent(TransformAnchor);
        //}

        //protected void ResetGamesParent()
        //{
        //    foreach (Transform game in _allGames.Take(_sprockets))
        //        game.SetParent(_normalHierarchy.GamesNode);
        //}
    }
}
